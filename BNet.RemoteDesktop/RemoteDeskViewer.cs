using BNet.RemoteDesktop.Models;
using BNet.RemoteDesktop.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.RemoteDesktop
{
    public partial class RemoteDeskViewer : Form
    {
        private readonly PeerHttpEngine _engine;

        private Image _currentImage;
        private volatile bool _disposed = false;

        private int _frameCount;
        private DateTime _fpsTimer = DateTime.UtcNow;
        private int _fps;

        private LowLevelKeyboardHook _hook;
        // Keys currently held down on the host (sent by us, not yet released).
        private readonly HashSet<ushort> _heldKeys = new HashSet<ushort>();
        private bool _screenSelectorReady = false;
        private int _selectedScreenIdx = 0;
        private FileTransferForm _fileTransferForm;

        public RemoteDeskViewer(PeerHttpEngine engine)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            _engine = engine;

            InitializeComponent();
            BuildFunctionKeyMenu();

            _toolbar.Visible = false;
            _screen.Visible = false;

            this.BackColor = Color.White;
            this.Paint += (s, e) => e.Graphics.Clear(Color.White);

            // Drag and Drop support for multi-files and folders
            this.AllowDrop = true;
            _screen.AllowDrop = true;
            _screen.DragEnter += (s, e) => {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            };
            _screen.DragDrop += (s, e) => {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                {
                    var sc = new System.Collections.Specialized.StringCollection();
                    foreach (string f in files) sc.Add(f);

                    // The files are offered to the remote PC's clipboard; press Ctrl+V there to paste (streamed on demand).
                    bool offered = _engine.OfferFilesToPeer(sc);
                    SetStatus(offered ? "Files ready on the remote clipboard - press Ctrl+V on the remote PC." : "Not connected.");
                }
            };

            // NOTE: the clipboard watcher (files, folders and text, both directions) now lives in
            // PeerHttpEngine, so it works on the host PC too. Nothing to do here.

            this.Shown += async (s, e) =>
            {
                _toolbar.Visible = true;
                _screen.Visible = true;

                _btnSendKeys.Click += (ss, ee) =>
                    _menuSendKeys.Show(_btnSendKeys, new Point(0, _btnSendKeys.Height));
                _btnFiles.Click += OnFilesClick;

                _screen.GetType()
                       .GetMethod("SetStyle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                       ?.Invoke(_screen, new object[] { ControlStyles.Selectable, true });

                string peerName = _engine.PeerInfo?.MachineName ?? "Remote";
                this.Text = "RemoteDesk  —  Controlling: " + peerName;

                _engine.FrameReceived += OnFrameReceived;
                _engine.PeerDisconnected += OnPeerDisconnected;
                _engine.StartViewing();

                _screen.Focus();

                await PopulateScreenSelectorAsync();

                _hook = new LowLevelKeyboardHook();
                _hook.ShouldCapture = () => !_disposed
                                            && Form.ActiveForm == this
                                            && !_cboScreen.Focused
                                            && !_cboScreen.DroppedDown;
                _hook.KeyIntercepted += OnLowLevelKey;
                _hook.Install();

                // Never leave a key stuck down on the host when we lose focus.
                this.Deactivate += (ss, ee) => ReleaseAllKeys();
                _screen.LostFocus += (ss, ee) => ReleaseAllKeys();

                this.Refresh();
            };
        }

        private async System.Threading.Tasks.Task PopulateScreenSelectorAsync()
        {
            List<string> labels = await _engine.GetRemoteScreenLabelsAsync();

            if (labels == null || labels.Count == 0)
                labels = new List<string> { "Screen 1 (Primary)" };

            if (_cboScreen.InvokeRequired)
                _cboScreen.Invoke(new Action(() => FillScreenCombo(labels)));
            else
                FillScreenCombo(labels);
        }

        private void FillScreenCombo(List<string> labels)
        {
            _screenSelectorReady = false;
            _cboScreen.Items.Clear();
            foreach (string l in labels)
                _cboScreen.Items.Add(l);
            if (_cboScreen.Items.Count > 0)
                _cboScreen.SelectedIndex = 0;
            _screenSelectorReady = true;
        }

        private async void OnScreenSelected(object sender, EventArgs e)
        {
            if (!_screenSelectorReady) return;
            int idx = _cboScreen.SelectedIndex;
            if (idx < 0) return;
            _selectedScreenIdx = idx;
            await _engine.SelectScreenAsync(idx);
        }

        private void OnFilesClick(object sender, EventArgs e)
        {
            if (_fileTransferForm == null || _fileTransferForm.IsDisposed)
            {
                _fileTransferForm = new FileTransferForm(_engine);
                _fileTransferForm.FormClosed += (s, _) => _fileTransferForm = null;
                _fileTransferForm.Show(this);
            }
            else
            {
                _fileTransferForm.BringToFront();
            }
        }

        private void BuildFunctionKeyMenu()
        {
            AddMenuHeader("── Window Management ──");
            AddMenuItem("Alt + Tab              (switch window)", AltKey(0x09));
            AddMenuItem("Alt + Shift + Tab      (switch window back)", AltShiftKey(0x09));
            AddMenuItem("Alt + Esc              (cycle windows)", AltKey(0x1B));
            AddMenuItem("Alt + F4               (close window)", AltKey(0x73));
            AddMenuSep();

            AddMenuHeader("── Task Manager / System ──");
            AddMenuItem("Ctrl + Shift + Esc     (Task Manager)", CtrlShiftKey(0x1B));
            AddMenuItem("Ctrl + Esc             (Start menu)", CtrlKey(0x1B));
            AddMenuSep();

            AddMenuHeader("── Win Key Shortcuts ──");
            AddMenuItem("Win                    (Start / toggle Start)", WinKey(0x00));
            AddMenuItem("Win + D                (show Desktop)", WinLetterKey(0x44));
            AddMenuItem("Win + E                (File Explorer)", WinLetterKey(0x45));
            AddMenuItem("Win + R                (Run)", WinLetterKey(0x52));
            AddMenuItem("Win + M                (minimize all)", WinLetterKey(0x4D));
            AddMenuItem("Win + Tab              (Task View)", WinLetterKey(0x09));
            AddMenuSep();
        }

        /// <summary>
        /// Called by the low-level hook for EVERY key while the viewer is focused
        /// (arrows, Alt+Tab, Win+*, F-keys, numpad...). Modifiers are forwarded as
        /// their own events, so the host sees the exact same key sequence.
        /// Returning true swallows the key locally.
        /// </summary>
        private bool OnLowLevelKey(ushort vk, bool extended, bool down,
                                   bool ctrl, bool alt, bool shift)
        {
            if (_disposed) return false;

            if (down)
            {
                _heldKeys.Add(vk);
            }
            else
            {
                // Key-up for something we never sent down (e.g. the Alt of an
                // Alt+Tab that brought us back here) → let Windows have it.
                if (!_heldKeys.Remove(vk)) return false;
            }

            Send(new RemoteInputEvent
            {
                Type = down ? "keydown" : "keyup",
                VirtualKey = vk,
                Extended = extended,
                Ctrl = ctrl,
                Alt = alt,
                Shift = shift
            });
            return true;
        }

        private void ReleaseAllKeys()
        {
            if (_heldKeys.Count == 0) return;
            var keys = new List<ushort>(_heldKeys);
            _heldKeys.Clear();
            foreach (ushort vk in keys)
                Send(new RemoteInputEvent { Type = "keyup", VirtualKey = vk });
        }

        /// <summary>
        /// Fallback used only if the low-level hook could not be installed:
        /// claim navigation keys that WinForms would otherwise eat for focus
        /// traversal (arrows, Tab, Esc...), so they still reach the host.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((_hook == null || !_hook.IsInstalled) && _screen != null && _screen.Focused)
            {
                Keys code = keyData & Keys.KeyCode;
                switch (code)
                {
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Tab:
                    case Keys.Escape:
                    case Keys.Home:
                    case Keys.End:
                    case Keys.PageUp:
                    case Keys.PageDown:
                    case Keys.Insert:
                    case Keys.Delete:
                    case Keys.Enter:
                    case Keys.Back:
                    case Keys.Space:
                        var ke = new KeyEventArgs(keyData);
                        Send(MakeKeyEvent("keydown", ke));
                        Send(MakeKeyEvent("keyup", ke));
                        return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OnFrameReceived(byte[] jpeg)
        {
            if (_disposed) return;
            try
            {
                Image newImg;
                using (var ms = new MemoryStream(jpeg, 0, jpeg.Length, writable: false, publiclyVisible: true))
                    newImg = Image.FromStream(ms);

                _frameCount++;
                var now = DateTime.UtcNow;
                if ((now - _fpsTimer).TotalSeconds >= 1.0)
                {
                    _fps = _frameCount;
                    _frameCount = 0;
                    _fpsTimer = now;
                }

                if (_screen.InvokeRequired)
                    _screen.BeginInvoke(new Action<Image>(SwapImage), newImg);
                else
                    SwapImage(newImg);
            }
            catch { }
        }

        private void SwapImage(Image newImg)
        {
            if (_disposed) { newImg?.Dispose(); return; }

            Image old = _currentImage;
            _currentImage = newImg;
            _screen.Image = newImg;
            old?.Dispose();

            _lblInfo.Text = newImg.Width + " x " + newImg.Height
                + "   |   " + _fps + " fps"
                + "   |   " + _engine.PeerInfo?.MachineName;
        }

        private Point MapToHost(Point p)
        {
            Image img = _currentImage;
            if (img == null) return p;

            double sx = (double)_screen.Width / img.Width;
            double sy = (double)_screen.Height / img.Height;
            double s = Math.Min(sx, sy);

            int dispW = (int)(img.Width * s);
            int dispH = (int)(img.Height * s);
            int ox = (_screen.Width - dispW) / 2;
            int oy = (_screen.Height - dispH) / 2;

            double fx = (p.X - ox) / s;
            double fy = (p.Y - oy) / s;

            fx = Math.Max(0, Math.Min(img.Width - 1, fx));
            fy = Math.Max(0, Math.Min(img.Height - 1, fy));

            return new Point((int)fx, (int)fy);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var p = MapToHost(e.Location);
            Image img = _currentImage;
            Send(new RemoteInputEvent
            {
                Type = "mousemove",
                X = p.X,
                Y = p.Y,
                ScreenIndex = _selectedScreenIdx,
                FrameW = img?.Width ?? 0,
                FrameH = img?.Height ?? 0,
            });
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            _screen.Focus();
            _screen.Capture = true;
            var p = MapToHost(e.Location);
            Image img = _currentImage;
            Send(new RemoteInputEvent
            {
                Type = "mousedown",
                X = p.X,
                Y = p.Y,
                Button = BtnIdx(e.Button),
                ScreenIndex = _selectedScreenIdx,
                FrameW = img?.Width ?? 0,
                FrameH = img?.Height ?? 0,
            });
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _screen.Capture = false;
            var p = MapToHost(e.Location);
            Image img = _currentImage;
            Send(new RemoteInputEvent
            {
                Type = "mouseup",
                X = p.X,
                Y = p.Y,
                Button = BtnIdx(e.Button),
                ScreenIndex = _selectedScreenIdx,
                FrameW = img?.Width ?? 0,
                FrameH = img?.Height ?? 0,
            });
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            Send(new RemoteInputEvent { Type = "wheel", Delta = e.Delta });
        }

        private static int BtnIdx(MouseButtons b)
        {
            if (b == MouseButtons.Right) return 2;
            if (b == MouseButtons.Middle) return 1;
            return 0;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
            if (_hook != null && _hook.IsInstalled) return; // hook already forwarded it
            Send(MakeKeyEvent("keydown", e));
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
            if (_hook != null && _hook.IsInstalled) return;
            Send(MakeKeyEvent("keyup", e));
        }

        private void SetStatus(string msg)
        {
            if (_lblInfo.InvokeRequired)
                _lblInfo.Invoke(new Action<string>(SetStatus), msg);
            else
                _lblInfo.Text = msg;
        }

        private void OnSendKeysMenuClick(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item?.Tag is RemoteInputEvent[] combo)
                foreach (var ev in combo)
                    Send(ev);
            _screen.Focus();
        }

        private static RemoteInputEvent MakeKeyEvent(string type, KeyEventArgs e)
            => new RemoteInputEvent
            {
                Type = type,
                VirtualKey = (int)e.KeyCode,
                Ctrl = e.Control,
                Alt = e.Alt,
                Shift = e.Shift,
                Extended = IsExtended(e.KeyCode)
            };

        private static bool IsExtended(Keys k)
            => k == Keys.PageUp || k == Keys.PageDown ||
               k == Keys.End || k == Keys.Home ||
               k == Keys.Left || k == Keys.Up ||
               k == Keys.Right || k == Keys.Down ||
               k == Keys.Insert || k == Keys.Delete ||
               k == Keys.RControlKey || k == Keys.RMenu ||
               k == Keys.LWin || k == Keys.RWin || k == Keys.Apps ||
               k == Keys.NumLock || k == Keys.Divide || k == Keys.PrintScreen;

        private async void OnDisconnect(object sender, EventArgs e)
        {
            await _engine.DisconnectAsync();
            this.Close();
        }

        private void OnPeerDisconnected()
        {
            if (InvokeRequired) { Invoke((Action)OnPeerDisconnected); return; }
            MessageBox.Show("The host has disconnected.", "RemoteDesk",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            _disposed = true;

            try { _fileTransferForm?.Close(); } catch { }

            ReleaseAllKeys();
            _hook?.Uninstall();
            _hook = null;

            _engine.FrameReceived -= OnFrameReceived;
            _engine.PeerDisconnected -= OnPeerDisconnected;
            _engine.StopViewing();

            _screen.Image = null;
            _currentImage?.Dispose();
            _currentImage = null;
        }

        private void Send(RemoteInputEvent evt) => _engine.SendInput(evt);

        private void AddMenuHeader(string text) => _menuSendKeys.Items.Add(new ToolStripMenuItem(text) { Enabled = false });
        private void AddMenuSep() => _menuSendKeys.Items.Add(new ToolStripSeparator());
        private void AddMenuItem(string label, RemoteInputEvent[] combo)
        {
            var item = new ToolStripMenuItem(label) { Tag = combo };
            item.Click += OnSendKeysMenuClick;
            _menuSendKeys.Items.Add(item);
        }

        private static RemoteInputEvent[] BareKey(ushort vk) => new[]
        {
            new RemoteInputEvent { Type = "keydown", VirtualKey = vk },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = vk }
        };

        private static RemoteInputEvent[] CtrlKey(ushort vk) => new[]
        {
            new RemoteInputEvent { Type = "keydown", VirtualKey = 0x11 },
            new RemoteInputEvent { Type = "keydown", VirtualKey = vk, Ctrl = true },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = vk, Ctrl = true },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = 0x11 }
        };

        private static RemoteInputEvent[] AltKey(ushort vk) => new[]
        {
            new RemoteInputEvent { Type = "keydown", VirtualKey = 0x12 },
            new RemoteInputEvent { Type = "keydown", VirtualKey = vk, Alt = true },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = vk, Alt = true },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = 0x12 }
        };

        private static RemoteInputEvent[] AltShiftKey(ushort vk) => new[]
        {
            new RemoteInputEvent { Type = "keydown", VirtualKey = 0x12 },
            new RemoteInputEvent { Type = "keydown", VirtualKey = 0x10 },
            new RemoteInputEvent { Type = "keydown", VirtualKey = vk, Alt = true, Shift = true },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = vk, Alt = true, Shift = true },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = 0x10 },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = 0x12 }
        };

        private static RemoteInputEvent[] CtrlShiftKey(ushort vk) => new[]
        {
            new RemoteInputEvent { Type = "keydown", VirtualKey = 0x11 },
            new RemoteInputEvent { Type = "keydown", VirtualKey = 0x10 },
            new RemoteInputEvent { Type = "keydown", VirtualKey = vk, Ctrl = true, Shift = true },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = vk, Ctrl = true, Shift = true },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = 0x10 },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = 0x11 }
        };

        private static RemoteInputEvent[] WinKey(ushort vk) => new[]
        {
            new RemoteInputEvent { Type = "keydown", VirtualKey = 0x5B },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = 0x5B }
        };

        private static RemoteInputEvent[] WinKey2(ushort vk, bool ext = false) => new[]
        {
            new RemoteInputEvent { Type = "keydown", VirtualKey = 0x5B },
            new RemoteInputEvent { Type = "keydown", VirtualKey = vk, Extended = ext },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = vk, Extended = ext },
            new RemoteInputEvent { Type = "keyup",   VirtualKey = 0x5B }
        };

        private static RemoteInputEvent[] WinLetterKey(ushort vk) => WinKey2(vk);
    }
}