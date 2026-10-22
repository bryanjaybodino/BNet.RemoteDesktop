using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using BNet.RemoteDesktop.Models;
using BNet.RemoteDesktop.Controllers;

namespace BNet.RemoteDesktop.Repositories
{
    public class PeerHttpEngine : IDisposable
    {
        public readonly int ListenerPort;
        public PeerInfo MyInfo { get; private set; }
        public PeerInfo PeerInfo { get; private set; }

        private TcpListener _tcpListener;
        private TcpClient _client;
        private NetworkStream _netStream;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private volatile int _selectedScreenIndex = 0;
        private bool _hosting;
        private bool _viewing;
        private long _jpegQuality = 75L;
        private double _imageScale = 1.00;

        private const int ActiveDelayMs = 15;

        // ---- File streaming tuning ----
        private const int FileChunkSize = 80 * 1024;   // bytes per network chunk

        private readonly SynchronizationContext _ui;

        // ---- Clipboard sync (files + text, both directions) ----
        private const int MaxClipboardTextBytes = 16 * 1024 * 1024;
        private volatile bool _stopping;
        private volatile bool _clipBaseline;        // on connect: remember current clipboard, don't send it
        private volatile bool _forceClipboardSend;  // "pull" / "push now": send even if unchanged
        private string _lastFileSig = "";
        private string _lastTextSig = "";

        // ---- Virtual file clipboard (RDP-style: announce list, stream bytes on paste) ----
        private const int FileChunksPerYield = 8;       // 8 x 80 KB bursts, then yield so video/input can interleave
        private const int StreamReadTimeoutSec = 60;

        private sealed class OfferEntry
        {
            public string Rel;
            public string Abs;
            public long Size;
            public bool IsDir;
            public long WriteTime;
        }

        private sealed class PendingRead
        {
            public long StartOffset;
            public byte[] Data;
            public int Received;
            public volatile bool Error;
            public readonly ManualResetEventSlim Done = new ManualResetEventSlim(false);
        }

        private readonly object _offerLock = new object();
        private readonly Dictionary<int, List<OfferEntry>> _offers = new Dictionary<int, List<OfferEntry>>();
        private readonly Queue<int> _offerOrder = new Queue<int>();
        private int _announceCounter;
        private readonly ConcurrentDictionary<int, PendingRead> _pending = new ConcurrentDictionary<int, PendingRead>();
        private int _reqCounter;

        // dedicated STA thread + message loop that owns the virtual clipboard object
        private readonly object _clipStaLock = new object();
        private System.Windows.Forms.Control _clipInvoker;
        private ComTypes.IDataObject _virtualClipObject;

        /// <summary>When true, anything copied on this PC (files, folders, text) is sent to the other PC automatically.</summary>
        public bool AutoSyncClipboard { get; set; } = true;

        public event Action<byte[]> FrameReceived;
        public event Action<PeerInfo> PeerConnected;
        public event Action PeerDisconnected;

        public PeerHttpEngine(int port)
        {
            ListenerPort = port;
            _ui = SynchronizationContext.Current;
            var clipThread = new Thread(ClipboardWatchLoop) { IsBackground = true, Name = "ClipboardWatch" };
            clipThread.SetApartmentState(ApartmentState.STA); // Clipboard API needs STA
            clipThread.Start();
            MyInfo = new PeerInfo
            {
                MachineName = Environment.MachineName,
                AccountName = Environment.UserName,
                OSVersion = Environment.OSVersion.VersionString,
                Architecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit",
                ListenerPort = port
            };
        }

        public void Start()
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, ListenerPort);
                _tcpListener.Start();
                Task.Run(AcceptTcpLoop);
            }
            catch (Exception ex)
            {
                MessageBox.Show("TCP Listener start error: " + ex.Message);
            }
        }

        public void Stop()
        {
            _stopping = true;
            try { _clipInvoker?.BeginInvoke(new Action(Application.ExitThread)); } catch { }
            _cts.Cancel();
            StopHosting();
            StopViewing();
            try { _tcpListener?.Stop(); } catch { }
            try { _client?.Close(); } catch { }
            _cts.Dispose();
        }

        public void Dispose() => Stop();

        private async Task AcceptTcpLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    if (_client != null)
                    {
                        tcpClient.Close();
                        continue;
                    }

                    _client = tcpClient;
                    _client.NoDelay = true;
                    _netStream = _client.GetStream();

                    Task.Run(() => HandleHostHandshakeAndReadLoop(_netStream));
                }
                catch { break; }
            }
        }

        private async Task HandleHostHandshakeAndReadLoop(NetworkStream stream)
        {
            try
            {
                var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
                var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

                string peerInfoJson = reader.ReadString();
                PeerInfo = Json.Deserialize<PeerInfo>(peerInfoJson);

                string myInfoJson = Json.Serialize(MyInfo);
                writer.Write(myInfoJson);
                writer.Flush();

                _clipBaseline = true;
                PeerConnected?.Invoke(PeerInfo);

                await StartPacketReadLoop(reader, stream);
            }
            catch
            {
                // Disconnected
            }
            finally
            {
                TriggerDisconnect();
            }
        }

        public async Task<bool> ConnectAsync(string peerIp, int peerPort)
        {
            try
            {
                _client = new TcpClient();
                _client.NoDelay = true;
                await _client.ConnectAsync(peerIp, peerPort);
                _netStream = _client.GetStream();

                var reader = new BinaryReader(_netStream, Encoding.UTF8, leaveOpen: true);
                var writer = new BinaryWriter(_netStream, Encoding.UTF8, leaveOpen: true);

                string myInfoJson = Json.Serialize(MyInfo);
                writer.Write(myInfoJson);
                writer.Flush();

                string peerInfoJson = reader.ReadString();
                PeerInfo = Json.Deserialize<PeerInfo>(peerInfoJson);

                _clipBaseline = true;
                PeerConnected?.Invoke(PeerInfo);

                Task.Run(() => StartPacketReadLoop(reader, _netStream));

                return true;
            }
            catch
            {
                TriggerDisconnect();
                return false;
            }
        }

        private async Task StartPacketReadLoop(BinaryReader reader, NetworkStream stream)
        {
            try
            {
                while (!_cts.IsCancellationRequested && _client != null && _client.Connected)
                {
                    byte packetType = reader.ReadByte();
                    if (packetType == 1) // Frame Packet
                    {
                        int len = reader.ReadInt32();
                        byte[] frameData = reader.ReadBytes(len);
                        FrameReceived?.Invoke(frameData);
                    }
                    else if (packetType == 2) // Input Packet
                    {
                        int len = reader.ReadInt32();
                        byte[] jsonBytes = reader.ReadBytes(len);
                        string json = Encoding.UTF8.GetString(jsonBytes);
                        var ev = Json.Deserialize<RemoteInputEvent>(json);
                        if (ev != null)
                        {
                            RemapToPhysical(ev);
                            InputInjector.Dispatch(ev);
                        }
                    }
                    else if (packetType == 6)
                    {
                        // Clipboard text from the other PC
                        int len = reader.ReadInt32();
                        if (len < 0 || len > MaxClipboardTextBytes) throw new InvalidDataException("Bad clipboard text length");
                        byte[] textBytes = reader.ReadBytes(len);
                        ApplyRemoteText(Encoding.UTF8.GetString(textBytes));
                    }
                    else if (packetType == 8)
                    {
                        // The other PC copied files: it sends only the list (names/sizes), no data yet
                        int len = reader.ReadInt32();
                        if (len < 0 || len > 256 * 1024 * 1024) throw new InvalidDataException("Bad file list length");
                        byte[] payload = reader.ReadBytes(len);
                        HandleFileListAnnounce(payload);
                    }
                    else if (packetType == 9)
                    {
                        // The other PC is pasting and wants bytes of one of our announced files
                        int announceId = reader.ReadInt32();
                        int fileIndex = reader.ReadInt32();
                        long offset = reader.ReadInt64();
                        int length = reader.ReadInt32();
                        int reqId = reader.ReadInt32();
                        _ = Task.Run(() => ServeReadAsync(stream, announceId, fileIndex, offset, length, reqId));
                    }
                    else if (packetType == 10)
                    {
                        int reqId = reader.ReadInt32();
                        int status = reader.ReadInt32();
                        long offset = reader.ReadInt64();
                        int len = reader.ReadInt32();
                        bool last = reader.ReadByte() != 0;
                        if (len < 0 || len > 16 * 1024 * 1024) throw new InvalidDataException("Bad read response length");
                        byte[] data = len > 0 ? reader.ReadBytes(len) : new byte[0];
                        HandleReadResponse(reqId, status, offset, data, last);
                    }
                    else if (packetType == 7)
                    {
                        // The other PC asked us to send our current clipboard ("pull")
                        _forceClipboardSend = true;
                    }
                }
            }
            catch
            {
                // Stream closed or error
            }
            finally
            {
                TriggerDisconnect();
            }
        }

        private void TriggerDisconnect()
        {
            if (_client == null) return;
            try { _client?.Close(); } catch { }
            _client = null;
            _netStream = null;
            foreach (var pr in _pending.Values) { pr.Error = true; pr.Done.Set(); }
            PeerDisconnected?.Invoke();
        }

        public async Task DisconnectAsync()
        {
            StopHosting();
            StopViewing();
            TriggerDisconnect();
            await Task.CompletedTask;
        }

        public void StartHosting()
        {
            if (_hosting) return;
            _hosting = true;
            Task.Run(() => HostCaptureLoop(_cts.Token));
        }

        public void StopHosting() => _hosting = false;

        private async Task HostCaptureLoop(CancellationToken ct)
        {
            var compressor = new ImageCompressor();
            while (!ct.IsCancellationRequested && _hosting)
            {
                List<Bitmap> screens = null;
                try
                {
                    if (_netStream == null || _client == null || !_client.Connected)
                    {
                        await Task.Delay(100, ct);
                        continue;
                    }

                    screens = await ScreenCaptured.TakeScreenshot(false, _selectedScreenIndex).ConfigureAwait(false);
                    if (screens == null || screens.Count == 0 || screens[0] == null)
                    {
                        await Task.Delay(30, ct);
                        continue;
                    }

                    Bitmap bmp = screens[0];
                    byte[] raw;
                    using (var ms = await compressor.CompressImage(bmp, _jpegQuality, _imageScale))
                        raw = ms.ToArray();

                    lock (_netStream)
                    {
                        var writer = new BinaryWriter(_netStream, Encoding.UTF8, leaveOpen: true);
                        writer.Write((byte)1);
                        writer.Write(raw.Length);
                        writer.Write(raw);
                        writer.Flush();
                    }

                    await Task.Delay(ActiveDelayMs, ct);
                }
                catch (OperationCanceledException) { break; }
                catch { await Task.Delay(50, ct); }
                finally
                {
                    if (screens != null)
                        foreach (var b in screens)
                            try { b?.Dispose(); } catch { }
                }
            }
        }

        public void SendInput(RemoteInputEvent e)
        {
            if (e == null || _netStream == null || _client == null || !_client.Connected) return;
            try
            {
                string json = Json.Serialize(e);
                byte[] data = Encoding.UTF8.GetBytes(json);

                lock (_netStream)
                {
                    var writer = new BinaryWriter(_netStream, Encoding.UTF8, leaveOpen: true);
                    writer.Write((byte)2);
                    writer.Write(data.Length);
                    writer.Write(data);
                    writer.Flush();
                }
            }
            catch { }
        }

        public void StartViewing() => _viewing = true;
        public void StopViewing() => _viewing = false;

        private static void RemapToPhysical(RemoteInputEvent ev)
        {
            if (ev.Type == "keydown" || ev.Type == "keyup" || ev.Type == "wheel") return;
            if (ev.FrameW <= 0 || ev.FrameH <= 0) return;

            var screens = Screen.AllScreens;
            int idx = Math.Max(0, Math.Min(screens.Length - 1, ev.ScreenIndex));
            Screen screen = screens[idx];

            int physW, physH, physX, physY;
            try
            {
                decimal sf = ScreenScale.scale(screen);
                physW = (int)(screen.Bounds.Width * sf / 100m);
                physH = (int)(screen.Bounds.Height * sf / 100m);
                physX = (int)(screen.Bounds.X * sf / 100m);
                physY = (int)(screen.Bounds.Y * sf / 100m);
            }
            catch
            {
                physW = screen.Bounds.Width;
                physH = screen.Bounds.Height;
                physX = screen.Bounds.X;
                physY = screen.Bounds.Y;
            }

            ev.X = physX + (int)Math.Round((double)ev.X / ev.FrameW * physW);
            ev.Y = physY + (int)Math.Round((double)ev.Y / ev.FrameH * physH);
        }

        /// <summary>
        /// Offer files/folders (for example dropped on the viewer window) to the other PC.
        /// They appear on that PC's clipboard as virtual files, ready to paste with Ctrl+V. Nothing is copied until it pastes.
        /// </summary>
        public bool OfferFilesToPeer(System.Collections.Specialized.StringCollection paths)
        {
            if (paths == null || paths.Count == 0 || _netStream == null || _client == null || !_client.Connected)
                return false;

            var list = new string[paths.Count];
            paths.CopyTo(list, 0);
            AnnounceFiles(list);
            return true;
        }

        public async Task<bool> DeleteRemoteFileAsync(string remotePath) => await Task.FromResult(false);
        public async Task<FileListingResponse> BrowseRemoteAsync(string remotePath) => await Task.FromResult(new FileListingResponse());
        public async Task<string> UploadFileAsync(string localPath, string remoteDestFolder = null) => await Task.FromResult("");
        public async Task UploadFolderAsync(string localFolderPath, string remoteDestFolder = null) => await Task.CompletedTask;
        public async Task<string> DownloadFileAsync(string remoteFilePath, string localDestFolder = null) => await Task.FromResult("");
        public async Task<List<string>> GetRemoteScreenLabelsAsync() => await Task.FromResult(new List<string> { "Screen 1" });
        public async Task SelectScreenAsync(int screenIndex) => _selectedScreenIndex = screenIndex;

        /// <summary>Send this PC's current clipboard (files or text) to the other PC right now.</summary>
        public Task<bool> PushClipboardToHostAsync()
        {
            if (_netStream == null || _client == null || !_client.Connected) return Task.FromResult(false);
            _forceClipboardSend = true; // picked up by the clipboard thread within ~300 ms
            return Task.FromResult(true);
        }

        /// <summary>Ask the other PC to send its current clipboard (files or text) to this PC.</summary>
        public Task PullClipboardFromHostAsync()
        {
            var stream = _netStream;
            if (stream == null || _client == null || !_client.Connected) return Task.CompletedTask;
            try
            {
                lock (stream)
                {
                    var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
                    w.Write((byte)7);
                    w.Flush();
                }
            }
            catch { }
            return Task.CompletedTask;
        }

        // =====================================================================
        //  CLIPBOARD SYNC  (runs on BOTH PCs, so copy on either side reaches the other)
        // =====================================================================

        private void ClipboardWatchLoop()
        {
            while (!_stopping)
            {
                try
                {
                    Thread.Sleep(300);

                    var client = _client;
                    if (client == null || !client.Connected || _netStream == null) continue;

                    bool force = _forceClipboardSend;
                    _forceClipboardSend = false;
                    if (!force && !_clipBaseline && !AutoSyncClipboard) continue;

                    string[] files; string text;
                    ReadLocalClipboard(out files, out text);
                    string fileSig = files != null ? string.Join("|", files) : "";

                    // right after connecting: remember what is already on the clipboard, don't send it
                    if (_clipBaseline && !force)
                    {
                        _clipBaseline = false;
                        _lastFileSig = fileSig;
                        _lastTextSig = text ?? "";
                        continue;
                    }
                    _clipBaseline = false;

                    if (files != null)
                    {
                        if (!force && fileSig == _lastFileSig) continue;
                        _lastFileSig = fileSig;
                        _lastTextSig = "";
                        AnnounceFiles(files); // list only; bytes are streamed when the other PC pastes
                    }
                    else if (!string.IsNullOrEmpty(text))
                    {
                        if (!force && text == _lastTextSig) continue;
                        _lastTextSig = text;
                        _lastFileSig = "";
                        SendClipboardText(text);
                    }
                    else
                    {
                        // clipboard holds something else (e.g. virtual files from the other PC): forget old signatures
                        // so copying the same files/text again later is detected as a new copy
                        _lastFileSig = "";
                        _lastTextSig = "";
                    }
                }
                catch
                {
                    // clipboard busy / locked by another app: just try again on the next tick
                }
            }
        }

        private static void ReadLocalClipboard(out string[] files, out string text)
        {
            files = null;
            text = null;
            IDataObject data = Clipboard.GetDataObject();
            if (data == null) return;

            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                files = data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length == 0) files = null;
            }
            if (files == null && data.GetDataPresent(DataFormats.UnicodeText))
                text = data.GetData(DataFormats.UnicodeText) as string;
        }

        private void SendClipboardText(string text)
        {
            var stream = _netStream;
            if (stream == null || _client == null || !_client.Connected) return;

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            if (bytes.Length > MaxClipboardTextBytes) return;

            try
            {
                lock (stream)
                {
                    var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
                    w.Write((byte)6);
                    w.Write(bytes.Length);
                    w.Write(bytes);
                    w.Flush();
                }
            }
            catch { }
        }

        private void ApplyRemoteText(string text)
        {
            // remember it first so our own watcher does not send it straight back
            _lastTextSig = text;
            _lastFileSig = "";
            RunOnUi(() =>
            {
                try { Clipboard.SetDataObject(new DataObject(DataFormats.UnicodeText, text), true, 10, 50); }
                catch { }
            });
        }

        // =====================================================================
        //  VIRTUAL FILE CLIPBOARD  (copy here -> paste there, streamed on demand, nothing staged on disk)
        // =====================================================================

        // ---------- sender side: the files were copied on THIS PC ----------

        private void AnnounceFiles(string[] paths)
        {
            var stream = _netStream;
            if (stream == null || paths == null || paths.Length == 0) return;

            Task.Run(() =>
            {
                try
                {
                    var entries = new List<OfferEntry>();
                    foreach (string path in paths)
                    {
                        if (File.Exists(path))
                        {
                            var fi = new FileInfo(path);
                            entries.Add(new OfferEntry { Rel = fi.Name, Abs = path, Size = fi.Length, WriteTime = fi.LastWriteTimeUtc.ToFileTimeUtc() });
                        }
                        else if (Directory.Exists(path))
                        {
                            var di = new DirectoryInfo(path);
                            CollectOffer(di, di.Name, entries);
                        }
                    }
                    if (entries.Count == 0) return;

                    int id = Interlocked.Increment(ref _announceCounter);
                    lock (_offerLock)
                    {
                        _offers[id] = entries;
                        _offerOrder.Enqueue(id);
                        while (_offerOrder.Count > 4) _offers.Remove(_offerOrder.Dequeue()); // keep the last few copies
                    }

                    byte[] payload;
                    using (var ms = new MemoryStream())
                    using (var w = new BinaryWriter(ms, Encoding.UTF8))
                    {
                        w.Write(id);
                        w.Write(entries.Count);
                        foreach (var e in entries)
                        {
                            w.Write(e.Rel);
                            w.Write(e.Size);
                            w.Write(e.IsDir);
                            w.Write(e.WriteTime);
                        }
                        w.Flush();
                        payload = ms.ToArray();
                    }

                    lock (stream)
                    {
                        var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
                        w.Write((byte)8);
                        w.Write(payload.Length);
                        w.Write(payload);
                        w.Flush();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("AnnounceFiles error: " + ex.Message);
                }
            });
        }

        private static void CollectOffer(DirectoryInfo dir, string rel, List<OfferEntry> results)
        {
            try
            {
                results.Add(new OfferEntry { Rel = rel, Abs = dir.FullName, IsDir = true, WriteTime = dir.LastWriteTimeUtc.ToFileTimeUtc() });
                foreach (var f in dir.GetFiles())
                    results.Add(new OfferEntry { Rel = Path.Combine(rel, f.Name), Abs = f.FullName, Size = f.Length, WriteTime = f.LastWriteTimeUtc.ToFileTimeUtc() });
                foreach (var d in dir.GetDirectories())
                    CollectOffer(d, Path.Combine(rel, d.Name), results);
            }
            catch { }
        }

        // The other PC pastes: serve the requested byte range of an announced file.
        // Files are addressed ONLY by (announceId, index) from our own list, never by a path from the peer.
        private async Task ServeReadAsync(NetworkStream stream, int announceId, int index, long offset, int length, int reqId)
        {
            OfferEntry entry = null;
            lock (_offerLock)
            {
                List<OfferEntry> list;
                if (_offers.TryGetValue(announceId, out list) && index >= 0 && index < list.Count) entry = list[index];
            }

            if (entry == null || entry.IsDir || offset < 0 || length < 0)
            {
                try { WriteReadResponse(stream, reqId, 1, offset, null, 0, true); } catch { }
                return;
            }

            try
            {
                using (var fs = new FileStream(entry.Abs, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 65536, false))
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    byte[] buf = new byte[FileChunkSize];
                    long pos = offset;
                    int remaining = length;
                    int burst = 0;

                    while (true)
                    {
                        int toRead = Math.Min(buf.Length, remaining);
                        int n = toRead > 0 ? fs.Read(buf, 0, toRead) : 0;
                        remaining -= n;
                        bool last = n < toRead || remaining <= 0;

                        WriteReadResponse(stream, reqId, 0, pos, buf, n, last);
                        pos += n;
                        if (last) break;

                        if (++burst % FileChunksPerYield == 0)
                            await Task.Delay(1).ConfigureAwait(false); // let video frames / input in
                    }
                }
            }
            catch
            {
                try { WriteReadResponse(stream, reqId, 1, offset, null, 0, true); } catch { }
            }
        }

        private static void WriteReadResponse(NetworkStream stream, int reqId, int status, long offset, byte[] buf, int count, bool last)
        {
            lock (stream)
            {
                var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
                w.Write((byte)10);
                w.Write(reqId);
                w.Write(status);
                w.Write(offset);
                w.Write(count);
                w.Write((byte)(last ? 1 : 0));
                if (count > 0) w.Write(buf, 0, count);
                w.Flush();
            }
        }

        // ---------- receiver side: the files were copied on the OTHER PC ----------

        private void HandleFileListAnnounce(byte[] payload)
        {
            int id;
            var list = new List<RemoteFileEntry>();
            using (var br = new BinaryReader(new MemoryStream(payload), Encoding.UTF8))
            {
                id = br.ReadInt32();
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var e = new RemoteFileEntry();
                    e.RelativePath = br.ReadString();
                    e.Size = br.ReadInt64();
                    e.IsDirectory = br.ReadBoolean();
                    e.LastWriteFileTime = br.ReadInt64();
                    list.Add(e);
                }
            }

            var obj = new VirtualFileDataObject(this, id, list);
            RunOnClipboardSta(() =>
            {
                _virtualClipObject = obj; // keep it alive while it is on the clipboard
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    if (ClipNative.OleSetClipboard(obj) == 0) return;
                    Thread.Sleep(50); // clipboard busy
                }
            });
        }

        /// <summary>Called by RemoteFileStream while the user pastes: fetch a byte range from the other PC (blocking).</summary>
        internal byte[] RequestRange(int announceId, int fileIndex, long offset, int length)
        {
            var stream = _netStream;
            if (stream == null || _client == null || !_client.Connected)
                throw new IOException("Not connected");

            var pr = new PendingRead { StartOffset = offset, Data = new byte[length] };
            int id = Interlocked.Increment(ref _reqCounter);
            _pending[id] = pr;
            try
            {
                lock (stream)
                {
                    var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
                    w.Write((byte)9);
                    w.Write(announceId);
                    w.Write(fileIndex);
                    w.Write(offset);
                    w.Write(length);
                    w.Write(id);
                    w.Flush();
                }

                if (!pr.Done.Wait(TimeSpan.FromSeconds(StreamReadTimeoutSec))) throw new TimeoutException("Remote read timed out");
                if (pr.Error) throw new IOException("Remote read failed");

                if (pr.Received == length) return pr.Data;
                var part = new byte[pr.Received];
                Buffer.BlockCopy(pr.Data, 0, part, 0, pr.Received);
                return part;
            }
            finally
            {
                PendingRead removed;
                _pending.TryRemove(id, out removed);
            }
        }

        private void HandleReadResponse(int reqId, int status, long offset, byte[] data, bool last)
        {
            PendingRead pr;
            if (!_pending.TryGetValue(reqId, out pr)) return;

            if (status != 0) { pr.Error = true; pr.Done.Set(); return; }

            long rel = offset - pr.StartOffset;
            if (rel >= 0 && rel + data.Length <= pr.Data.Length)
            {
                Buffer.BlockCopy(data, 0, pr.Data, (int)rel, data.Length);
                pr.Received += data.Length;
            }
            if (last) pr.Done.Set();
        }

        // A dedicated STA thread with its own message loop owns the virtual clipboard object.
        // (Explorer calls into it during a paste; a long paste must never freeze the viewer window.)
        private void RunOnClipboardSta(Action action)
        {
            lock (_clipStaLock)
            {
                if (_clipInvoker == null)
                {
                    var ready = new ManualResetEventSlim(false);
                    var t = new Thread(() =>
                    {
                        ClipNative.OleInitialize(IntPtr.Zero);
                        _clipInvoker = new System.Windows.Forms.Control();
                        var handle = _clipInvoker.Handle; // force handle creation on this thread
                        ready.Set();
                        Application.Run();
                        ClipNative.OleUninitialize();
                    });
                    t.Name = "VirtualClipboardSta";
                    t.IsBackground = true;
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    ready.Wait();
                }
            }
            try { _clipInvoker.BeginInvoke(action); } catch { }
        }

        // Clipboard writes should happen on an STA thread: UI thread if we have one, else a short-lived STA thread.
        private void RunOnUi(Action action)
        {
            if (_ui != null) { _ui.Post(_ => action(), null); return; }

            try
            {
                if (Application.OpenForms.Count > 0)
                {
                    Application.OpenForms[0].BeginInvoke(action);
                    return;
                }
            }
            catch { }

            var t = new Thread(() => action());
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }
    }
}