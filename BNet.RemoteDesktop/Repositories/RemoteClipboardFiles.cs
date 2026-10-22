using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace BNet.RemoteDesktop.Repositories
{
    // =========================================================================================
    //  RDP / AnyDesk style file clipboard.
    //
    //  The PC where you press Ctrl+C only announces a LIST of files (names + sizes).
    //  The other PC puts "virtual files" on its clipboard (FileGroupDescriptorW + FileContents).
    //  Nothing is copied until you paste: Explorer then reads each file through an IStream, and every
    //  Read() is answered by pulling the bytes from the other PC on demand.
    //  => no staging folder, paste starts immediately, data streams straight into the destination.
    // =========================================================================================

    /// <summary>One file or folder offered by the other PC.</summary>
    public sealed class RemoteFileEntry
    {
        public string RelativePath;
        public long Size;
        public bool IsDirectory;
        public long LastWriteFileTime;
    }

    internal static class ClipNative
    {
        [DllImport("ole32.dll")] public static extern int OleInitialize(IntPtr pvReserved);
        [DllImport("ole32.dll")] public static extern void OleUninitialize();
        [DllImport("ole32.dll")] public static extern int OleSetClipboard(ComTypes.IDataObject pDataObj);

        [DllImport("kernel32.dll")] private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
        [DllImport("kernel32.dll")] private static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")] private static extern bool GlobalUnlock(IntPtr hMem);

        private const uint GHND = 0x0042; // GMEM_MOVEABLE | GMEM_ZEROINIT

        public static IntPtr ToHGlobal(byte[] data)
        {
            IntPtr h = GlobalAlloc(GHND, (UIntPtr)(uint)Math.Max(1, data.Length));
            if (h == IntPtr.Zero) throw new OutOfMemoryException();
            IntPtr p = GlobalLock(h);
            Marshal.Copy(data, 0, p, data.Length);
            GlobalUnlock(h);
            return h;
        }
    }

    /// <summary>The IDataObject we hand to the Windows clipboard.</summary>
    internal sealed class VirtualFileDataObject : ComTypes.IDataObject
    {
        private const int S_OK = 0;
        private const int E_NOTIMPL = unchecked((int)0x80004001);
        private const int DV_E_LINDEX = unchecked((int)0x80040068);
        private const int DV_E_FORMATETC = unchecked((int)0x80040064);
        private const int DV_E_TYMED = unchecked((int)0x80040069);
        private const int DV_E_DVASPECT = unchecked((int)0x80040065);
        private const int DATA_S_SAMEFORMATETC = 0x00040130;
        private const int OLE_E_ADVISENOTSUPPORTED = unchecked((int)0x80040003);

        private const uint FD_ATTRIBUTES = 0x00000004;
        private const uint FD_WRITESTIME = 0x00000020;
        private const uint FD_FILESIZE = 0x00000040;
        private const uint FD_PROGRESSUI = 0x00004000;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        private static readonly short CfDescriptor = unchecked((short)DataFormats.GetFormat("FileGroupDescriptorW").Id);
        private static readonly short CfContents = unchecked((short)DataFormats.GetFormat("FileContents").Id);
        private static readonly short CfDropEffect = unchecked((short)DataFormats.GetFormat("Preferred DropEffect").Id);

        private readonly PeerHttpEngine _engine;
        private readonly int _announceId;
        private readonly List<RemoteFileEntry> _entries = new List<RemoteFileEntry>();
        private readonly List<int> _originalIndex = new List<int>(); // index in the sender's list
        private byte[] _descriptorBytes;

        public VirtualFileDataObject(PeerHttpEngine engine, int announceId, List<RemoteFileEntry> entries)
        {
            _engine = engine;
            _announceId = announceId;

            // Keep only names Explorer can represent and that cannot escape the paste target.
            // The sender identifies files by their ORIGINAL index, so we remember it for every kept entry.
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                string name = (e.RelativePath ?? "").Replace('/', '\\').Trim('\\');
                if (name.Length == 0 || name.Length >= 260) continue;
                if (Path.IsPathRooted(name) || name.Contains(":")) continue;
                bool bad = false;
                foreach (var part in name.Split('\\')) if (part == ".." || part == ".") { bad = true; break; }
                if (bad) continue;

                _entries.Add(new RemoteFileEntry
                {
                    RelativePath = name,
                    Size = e.Size,
                    IsDirectory = e.IsDirectory,
                    LastWriteFileTime = e.LastWriteFileTime
                });
                _originalIndex.Add(i);
            }
        }

        // ---------------- IDataObject ----------------

        public void GetData(ref ComTypes.FORMATETC format, out ComTypes.STGMEDIUM medium)
        {
            medium = new ComTypes.STGMEDIUM();

            if (format.cfFormat == CfDescriptor && (format.tymed & ComTypes.TYMED.TYMED_HGLOBAL) != 0)
            {
                medium.tymed = ComTypes.TYMED.TYMED_HGLOBAL;
                medium.unionmember = ClipNative.ToHGlobal(GetDescriptorBytes());
                medium.pUnkForRelease = null;
                return;
            }

            if (format.cfFormat == CfContents && (format.tymed & ComTypes.TYMED.TYMED_ISTREAM) != 0)
            {
                int idx = format.lindex;
                if (idx < 0 || idx >= _entries.Count || _entries[idx].IsDirectory)
                    throw new COMException("Bad file index", DV_E_LINDEX);

                var stream = new RemoteFileStream(_engine, _announceId, _originalIndex[idx], _entries[idx].Size);
                medium.tymed = ComTypes.TYMED.TYMED_ISTREAM;
                medium.unionmember = Marshal.GetComInterfaceForObject(stream, typeof(ComTypes.IStream));
                medium.pUnkForRelease = null;
                return;
            }

            if (format.cfFormat == CfDropEffect && (format.tymed & ComTypes.TYMED.TYMED_HGLOBAL) != 0)
            {
                medium.tymed = ComTypes.TYMED.TYMED_HGLOBAL;
                medium.unionmember = ClipNative.ToHGlobal(BitConverter.GetBytes(1)); // DROPEFFECT_COPY
                medium.pUnkForRelease = null;
                return;
            }

            throw new COMException("Format not supported", DV_E_FORMATETC);
        }

        public void GetDataHere(ref ComTypes.FORMATETC format, ref ComTypes.STGMEDIUM medium)
        {
            throw new COMException("Not implemented", E_NOTIMPL);
        }

        public int QueryGetData(ref ComTypes.FORMATETC format)
        {
            if (format.dwAspect != ComTypes.DVASPECT.DVASPECT_CONTENT) return DV_E_DVASPECT;

            if (format.cfFormat == CfDescriptor || format.cfFormat == CfDropEffect)
                return (format.tymed & ComTypes.TYMED.TYMED_HGLOBAL) != 0 ? S_OK : DV_E_TYMED;

            if (format.cfFormat == CfContents)
                return (format.tymed & ComTypes.TYMED.TYMED_ISTREAM) != 0 ? S_OK : DV_E_TYMED;

            return DV_E_FORMATETC;
        }

        public int GetCanonicalFormatEtc(ref ComTypes.FORMATETC formatIn, out ComTypes.FORMATETC formatOut)
        {
            formatOut = formatIn;
            formatOut.ptd = IntPtr.Zero;
            return DATA_S_SAMEFORMATETC;
        }

        public void SetData(ref ComTypes.FORMATETC formatIn, ref ComTypes.STGMEDIUM medium, bool release)
        {
            throw new COMException("Not implemented", E_NOTIMPL);
        }

        public ComTypes.IEnumFORMATETC EnumFormatEtc(ComTypes.DATADIR direction)
        {
            if (direction != ComTypes.DATADIR.DATADIR_GET) throw new COMException("Not implemented", E_NOTIMPL);

            return new FormatEtcEnum(new[]
            {
                new ComTypes.FORMATETC { cfFormat = CfDescriptor, dwAspect = ComTypes.DVASPECT.DVASPECT_CONTENT, lindex = -1, ptd = IntPtr.Zero, tymed = ComTypes.TYMED.TYMED_HGLOBAL },
                new ComTypes.FORMATETC { cfFormat = CfContents,   dwAspect = ComTypes.DVASPECT.DVASPECT_CONTENT, lindex = -1, ptd = IntPtr.Zero, tymed = ComTypes.TYMED.TYMED_ISTREAM },
                new ComTypes.FORMATETC { cfFormat = CfDropEffect, dwAspect = ComTypes.DVASPECT.DVASPECT_CONTENT, lindex = -1, ptd = IntPtr.Zero, tymed = ComTypes.TYMED.TYMED_HGLOBAL },
            });
        }

        public int DAdvise(ref ComTypes.FORMATETC pFormatetc, ComTypes.ADVF advf, ComTypes.IAdviseSink adviseSink, out int connection)
        {
            connection = 0;
            return OLE_E_ADVISENOTSUPPORTED;
        }

        public void DUnadvise(int connection)
        {
            throw new COMException("Advise not supported", OLE_E_ADVISENOTSUPPORTED);
        }

        public int EnumDAdvise(out ComTypes.IEnumSTATDATA enumAdvise)
        {
            enumAdvise = null;
            return OLE_E_ADVISENOTSUPPORTED;
        }

        // ---------------- FILEGROUPDESCRIPTORW ----------------

        private byte[] GetDescriptorBytes()
        {
            if (_descriptorBytes != null) return _descriptorBytes;

            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write((uint)_entries.Count);
                foreach (var e in _entries)
                {
                    uint flags = FD_ATTRIBUTES | FD_WRITESTIME | FD_PROGRESSUI | (e.IsDirectory ? 0u : FD_FILESIZE);
                    w.Write(flags);                    // dwFlags
                    w.Write(new byte[16]);             // clsid
                    w.Write(0); w.Write(0);            // sizel
                    w.Write(0); w.Write(0);            // pointl
                    w.Write(e.IsDirectory ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL);
                    w.Write(0L);                       // ftCreationTime
                    w.Write(0L);                       // ftLastAccessTime
                    w.Write(e.LastWriteFileTime);      // ftLastWriteTime
                    w.Write((uint)((ulong)e.Size >> 32));
                    w.Write((uint)((ulong)e.Size & 0xFFFFFFFF));

                    byte[] name = new byte[520];       // WCHAR cFileName[260]
                    byte[] raw = Encoding.Unicode.GetBytes(e.RelativePath);
                    Buffer.BlockCopy(raw, 0, name, 0, Math.Min(raw.Length, 518));
                    w.Write(name);
                }
                w.Flush();
                _descriptorBytes = ms.ToArray();
            }
            return _descriptorBytes;
        }

        private sealed class FormatEtcEnum : ComTypes.IEnumFORMATETC
        {
            private readonly ComTypes.FORMATETC[] _items;
            private int _index;

            public FormatEtcEnum(ComTypes.FORMATETC[] items) { _items = items; }

            public int Next(int celt, ComTypes.FORMATETC[] rgelt, int[] pceltFetched)
            {
                int n = 0;
                while (n < celt && _index < _items.Length) rgelt[n++] = _items[_index++];
                if (pceltFetched != null && pceltFetched.Length > 0) pceltFetched[0] = n;
                return n == celt ? 0 : 1; // S_OK : S_FALSE
            }

            public int Skip(int celt)
            {
                _index += celt;
                return _index <= _items.Length ? 0 : 1;
            }

            public int Reset() { _index = 0; return 0; }

            public void Clone(out ComTypes.IEnumFORMATETC newEnum)
            {
                newEnum = new FormatEtcEnum(_items) { _index = _index };
            }
        }
    }

    /// <summary>
    /// IStream that Explorer reads while pasting. Every cache miss is a request to the other PC.
    /// Reads 1 MB blocks and prefetches the next one, so throughput does not depend on Explorer's read size.
    /// </summary>
    internal sealed class RemoteFileStream : ComTypes.IStream
    {
        private const int BlockSize = 1024 * 1024;
        private const int E_NOTIMPL = unchecked((int)0x80004001);
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int STG_E_ACCESSDENIED = unchecked((int)0x80030005);

        private readonly PeerHttpEngine _engine;
        private readonly int _announceId;
        private readonly int _fileIndex;
        private readonly long _size;
        private long _pos;

        private byte[] _cur;
        private long _curIdx = -1;
        private Task<byte[]> _prefetch;
        private long _prefetchIdx = -1;

        public RemoteFileStream(PeerHttpEngine engine, int announceId, int fileIndex, long size)
        {
            _engine = engine;
            _announceId = announceId;
            _fileIndex = fileIndex;
            _size = size;
        }

        private byte[] Fetch(long blockIdx)
        {
            long offset = blockIdx * BlockSize;
            int len = (int)Math.Min(BlockSize, _size - offset);
            return _engine.RequestRange(_announceId, _fileIndex, offset, len);
        }

        private byte[] GetBlock(long idx)
        {
            if (_cur != null && _curIdx == idx) return _cur;

            byte[] data;
            if (_prefetch != null && _prefetchIdx == idx)
                data = _prefetch.GetAwaiter().GetResult();
            else
                data = Fetch(idx);

            _prefetch = null;
            _prefetchIdx = -1;
            _cur = data;
            _curIdx = idx;

            long next = idx + 1;
            if (next * BlockSize < _size)
            {
                _prefetchIdx = next;
                _prefetch = Task.Run(() => Fetch(next));
            }
            return data;
        }

        private int ReadInternal(byte[] buffer, int offset, int count)
        {
            int total = 0;
            while (total < count && _pos < _size)
            {
                long idx = _pos / BlockSize;
                byte[] block = GetBlock(idx);
                int inBlock = (int)(_pos - idx * BlockSize);
                int avail = block.Length - inBlock;
                if (avail <= 0) break; // remote file shrank
                int n = Math.Min(avail, count - total);
                Buffer.BlockCopy(block, inBlock, buffer, offset + total, n);
                total += n;
                _pos += n;
            }
            return total;
        }

        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            int total;
            try { total = ReadInternal(pv, 0, cb); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RemoteFileStream.Read failed: " + ex.Message);
                throw new COMException("Remote read failed", E_FAIL);
            }
            if (pcbRead != IntPtr.Zero) Marshal.WriteInt32(pcbRead, total);
        }

        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            long np;
            switch (dwOrigin)
            {
                case 0: np = dlibMove; break;            // STREAM_SEEK_SET
                case 1: np = _pos + dlibMove; break;     // STREAM_SEEK_CUR
                default: np = _size + dlibMove; break;   // STREAM_SEEK_END
            }
            if (np < 0) np = 0;
            _pos = np;
            if (plibNewPosition != IntPtr.Zero) Marshal.WriteInt64(plibNewPosition, _pos);
        }

        public void Stat(out ComTypes.STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new ComTypes.STATSTG { type = 2 /* STGTY_STREAM */, cbSize = _size, grfMode = 0 };
        }

        public void CopyTo(ComTypes.IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            byte[] buf = new byte[64 * 1024];
            long remaining = cb, totalRead = 0;
            try
            {
                while (remaining > 0 && _pos < _size)
                {
                    int got = ReadInternal(buf, 0, (int)Math.Min(buf.Length, remaining));
                    if (got <= 0) break;
                    pstm.Write(buf, got, IntPtr.Zero);
                    totalRead += got;
                    remaining -= got;
                }
            }
            catch (COMException) { throw; }
            catch (Exception) { throw new COMException("Remote read failed", E_FAIL); }

            if (pcbRead != IntPtr.Zero) Marshal.WriteInt64(pcbRead, totalRead);
            if (pcbWritten != IntPtr.Zero) Marshal.WriteInt64(pcbWritten, totalRead);
        }

        public void Clone(out ComTypes.IStream ppstm) { throw new COMException("Not implemented", E_NOTIMPL); }
        public void Commit(int grfCommitFlags) { }
        public void Revert() { }
        public void LockRegion(long libOffset, long cb, int dwLockType) { throw new COMException("Not implemented", E_NOTIMPL); }
        public void UnlockRegion(long libOffset, long cb, int dwLockType) { throw new COMException("Not implemented", E_NOTIMPL); }
        public void SetSize(long libNewSize) { throw new COMException("Read only", STG_E_ACCESSDENIED); }
        public void Write(byte[] pv, int cb, IntPtr pcbWritten) { throw new COMException("Read only", STG_E_ACCESSDENIED); }
    }
}