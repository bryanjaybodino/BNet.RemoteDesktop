using BNet.RemoteDesktop.Controllers;
using BNet.RemoteDesktop.Models;
using BNet.RemoteDesktop.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.RemoteDesktop
{
    public partial class RemoteDeskHome : Form
    {
        private PeerHttpEngine _engine;
        private const int DEFAULT_PORT = 7891;

        // Saved-session panel controls (built in code, not designer)
        private Panel _panelSessions;
        private Label _lblSessionsTitle;
        private ListBox _lstSessions;
        private Button _btnDeleteSession;
        private List<SessionEntry> _sessions = new List<SessionEntry>();


        private static readonly Font _sessFntBold = new Font("Segoe UI", 9F, FontStyle.Bold);
        private static readonly Font _sessFntSmall = new Font("Segoe UI", 7.5F);
        private static readonly SolidBrush _sessBrushSel = new SolidBrush(Color.White);
        private static readonly SolidBrush _sessBrushMain = new SolidBrush(Color.FromArgb(210, 210, 210));
        private static readonly SolidBrush _sessBrushSub = new SolidBrush(Color.FromArgb(110, 110, 110));
        private static readonly SolidBrush _sessBrushBgSel = new SolidBrush(Color.FromArgb(55, 55, 80));
        private static readonly SolidBrush _sessBrushBgNorm = new SolidBrush(Color.FromArgb(28, 28, 28));
        private static readonly Pen _sessPenSep = new Pen(Color.FromArgb(45, 45, 45));

        public RemoteDeskHome()
        {
            InitializeComponent();
            BuildSessionsPanel();
            PopulateMyIps();
            LoadSessions();

            this.Load += OnFormLoad;
        }

        // ── Auto-start ────────────────────────────────────────────────────────

        private void OnFormLoad(object sender, EventArgs e)
        {
            _txtHostPort.Text = DEFAULT_PORT.ToString();
            OnStartHost(this, EventArgs.Empty);
        }

        // ── Saved-sessions panel ──────────────────────────────────────────────

        private void BuildSessionsPanel()
        {
            this.ClientSize = new System.Drawing.Size(1100, 540);

            _panelSessions = new Panel
            {
                BackColor = Color.FromArgb(38, 38, 38),
                Location = new Point(848, 16),
                Size = new Size(234, 448),
                Name = "_panelSessions"
            };

            _lblSessionsTitle = new Label
            {
                Text = "Recent Sessions",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(240, 240, 240),
                Location = new Point(12, 20),
                AutoSize = true
            };

            var lblHint = new Label
            {
                Text = "Click to connect",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(110, 110, 110),
                Location = new Point(12, 46),
                AutoSize = true
            };

            var sep = new Panel
            {
                BackColor = Color.FromArgb(55, 55, 55),
                Location = new Point(12, 68),
                Size = new Size(210, 1)
            };

            _lstSessions = new ListBox
            {
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(210, 210, 210),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 8.5F),
                DrawMode = DrawMode.OwnerDrawVariable,
                Location = new Point(12, 78),
                Size = new Size(210, 316),
                Name = "_lstSessions",
                SelectionMode = SelectionMode.One
            };
            _lstSessions.DrawItem += OnSessionDrawItem;
            _lstSessions.MeasureItem += OnSessionMeasureItem;
            _lstSessions.DoubleClick += OnSessionDoubleClick;
            _lstSessions.MouseClick += OnSessionSingleClick;

            _btnDeleteSession = new Button
            {
                Text = "✕  Remove",
                BackColor = Color.FromArgb(55, 55, 58),
                ForeColor = Color.FromArgb(200, 80, 80),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(12, 404),
                Size = new Size(210, 30),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnDeleteSession.FlatAppearance.BorderSize = 0;
            _btnDeleteSession.Click += OnDeleteSession;

            _panelSessions.Controls.Add(_lblSessionsTitle);
            _panelSessions.Controls.Add(lblHint);
            _panelSessions.Controls.Add(sep);
            _panelSessions.Controls.Add(_lstSessions);
            _panelSessions.Controls.Add(_btnDeleteSession);

            _panelBody.Controls.Add(_panelSessions);
        }

        // ── Session list drawing ──────────────────────────────────────────────

        private void OnSessionMeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 44;
        }

        private void OnSessionDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _sessions.Count) return;
            var s = _sessions[e.Index];

            bool selected = (e.State & DrawItemState.Selected) != 0;


            e.Graphics.FillRectangle(
                selected ? _sessBrushBgSel : _sessBrushBgNorm,
                e.Bounds);

            string line1 = $"{s.HostIp}:{s.Port}";
            string line2 = string.IsNullOrEmpty(s.MachineName)
                ? s.LastConnected.ToString("yyyy-MM-dd  HH:mm")
                : $"{s.MachineName}   {s.LastConnected:yyyy-MM-dd  HH:mm}";

            SolidBrush brushMain = selected ? _sessBrushSel : _sessBrushMain;

            e.Graphics.DrawString(line1, _sessFntBold, brushMain,
                new RectangleF(e.Bounds.X + 8, e.Bounds.Y + 4, e.Bounds.Width - 8, 20));
            e.Graphics.DrawString(line2, _sessFntSmall, _sessBrushSub,
                new RectangleF(e.Bounds.X + 8, e.Bounds.Y + 24, e.Bounds.Width - 8, 18));

            e.Graphics.DrawLine(_sessPenSep,
                e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private void OnSessionSingleClick(object sender, MouseEventArgs e)
        {
            if (_lstSessions.SelectedIndex < 0) return;
            _btnDeleteSession.Enabled = true;

            var s = _sessions[_lstSessions.SelectedIndex];
            _txtTargetIp.Text = s.HostIp;
            _txtTargetPort.Text = s.Port.ToString();
        }

        private void OnSessionDoubleClick(object sender, EventArgs e)
        {
            if (_lstSessions.SelectedIndex < 0) return;
            var s = _sessions[_lstSessions.SelectedIndex];
            _txtTargetIp.Text = s.HostIp;
            _txtTargetPort.Text = s.Port.ToString();
            OnConnectAndView(this, EventArgs.Empty);
        }

        private void OnDeleteSession(object sender, EventArgs e)
        {
            if (_lstSessions.SelectedIndex < 0) return;
            var s = _sessions[_lstSessions.SelectedIndex];
            SessionStore.Delete(s.HostIp, s.Port);
            LoadSessions();
            _btnDeleteSession.Enabled = false;
        }

        private void LoadSessions()
        {
            _sessions = SessionStore.Load();
            _lstSessions.Items.Clear();
            foreach (var s in _sessions)
                _lstSessions.Items.Add(s);

            if (_sessions.Count == 0)
                _lstSessions.Items.Add("");
        }

        // ── Populate IP list ──────────────────────────────────────────────────

        private void PopulateMyIps()
        {
            _lstMyIps.Items.Clear();
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                        _lstMyIps.Items.Add(ua.Address + "   (" + ni.Name + ")");
                    }
                }
            }
            catch { }

            if (_lstMyIps.Items.Count == 0)
                _lstMyIps.Items.Add("No network adapters found");
        }

        // ── HOST side ─────────────────────────────────────────────────────────

        private void OnStartHost(object sender, EventArgs e)
        {
            int port;
            if (!int.TryParse(_txtHostPort.Text.Trim(), out port) || port < 1024 || port > 65535)
            {
                MessageBox.Show("Enter a valid port number (1024–65535).", "Invalid Port",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_engine != null) { _engine.Stop(); _engine = null; }

            try
            {
                _engine = new PeerHttpEngine(port);
                _engine.PeerConnected += OnHostGotViewer;
                _engine.PeerDisconnected += OnHostViewerLeft;
                _engine.Start();
                PortManager.OpenFirewallPort(port, port.ToString());
                _btnStartHost.Enabled = false;
                _btnStopHost.Enabled = true;
                _txtHostPort.Enabled = false;

                SetHostStatus("Hosting on port " + port + ". Waiting for a viewer…",
                    Color.FromArgb(102, 187, 106));
                _lblFooter.Text = "Tip: Tell the viewer one of the IPs above and port " + port + ".";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start listener:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnStopHost(object sender, EventArgs e)
        {

            int port;
            int.TryParse(_txtHostPort.Text.Trim(), out port);



            _engine?.Stop();
            _engine = null;

            _btnStartHost.Enabled = true;
            _btnStopHost.Enabled = false;
            _txtHostPort.Enabled = true;
            PortManager.CloseFirewallPort(port.ToString());
            SetHostStatus("Hosting stopped.", Color.FromArgb(110, 110, 110));
        }

        private void OnHostGotViewer(PeerInfo viewer)
        {
            if (InvokeRequired) { Invoke((Action<PeerInfo>)OnHostGotViewer, viewer); return; }
            _engine.StartHosting();
            SetHostStatus("Viewer connected: " + viewer.MachineName
                + " (" + viewer.AccountName + ")  —  streaming…",
                Color.FromArgb(102, 187, 106));
            this.Hide();
        }

        private void OnHostViewerLeft()
        {
            if (InvokeRequired) { Invoke((Action)OnHostViewerLeft); return; }
            _engine.StopHosting();
            SetHostStatus("Viewer disconnected. Waiting for next connection…",
                Color.FromArgb(255, 167, 38));
            this.Show();
            this.BringToFront();
        }

        // ── VIEWER side ───────────────────────────────────────────────────────

        private async void OnConnectAndView(object sender, EventArgs e)
        {
            string hostIp = _txtTargetIp.Text.Trim();
            if (string.IsNullOrEmpty(hostIp))
            {
                MessageBox.Show("Enter the host's IP address.", "Missing IP",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int port;
            if (!int.TryParse(_txtTargetPort.Text.Trim(), out port) || port < 1024 || port > 65535)
            {
                MessageBox.Show("Enter a valid port number.", "Invalid Port",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _btnConnect.Enabled = false;
            SetConnStatus("Connecting to " + hostIp + ":" + port + " …",
                Color.FromArgb(255, 167, 38));

            int viewerListenPort = port + 1;
            try
            {
                if (_engine != null) { _engine.Stop(); _engine = null; }

                _engine = new PeerHttpEngine(viewerListenPort);
                _engine.Start();

                bool ok = await _engine.ConnectAsync(hostIp, port);
                if (!ok)
                {
                    _engine.Stop(); _engine = null;
                    SetConnStatus("Connection failed. Is the host running and the IP correct?",
                        Color.FromArgb(239, 83, 80));
                    _btnConnect.Enabled = true;
                    return;
                }

                string machineName = _engine.PeerInfo?.MachineName ?? "";
                SessionStore.SaveOrUpdate(hostIp, port, machineName);
                LoadSessions();

                OpenViewerWindow();
            }
            catch (Exception ex)
            {
                SetConnStatus("Error: " + ex.Message, Color.FromArgb(239, 83, 80));
                _btnConnect.Enabled = true;
            }
        }

        private void OpenViewerWindow()
        {
            if (InvokeRequired) { Invoke((Action)OpenViewerWindow); return; }

            SetConnStatus("Connected to " + _engine.PeerInfo.MachineName + ". Viewer opened.",
                Color.FromArgb(102, 187, 106));

            this.Hide();

            var viewer = new RemoteDeskViewer(_engine);
            viewer.FormClosed += (s, a) =>
            {
                _engine?.Stop();
                _engine = null;

                if (!IsDisposed)
                {
                    _btnConnect.Enabled = true;
                    SetConnStatus("Disconnected.", Color.FromArgb(110, 110, 110));
                    this.Show();
                    this.BringToFront();
                }
            };
            viewer.Show();
        }

        // ── Status helpers ────────────────────────────────────────────────────

        private void SetHostStatus(string msg, Color color)
        {
            if (InvokeRequired) { Invoke((Action<string, Color>)SetHostStatus, msg, color); return; }
            _lblHostStatus.Text = msg;
            _lblHostStatus.ForeColor = color;
        }

        private void SetConnStatus(string msg, Color color)
        {
            if (InvokeRequired) { Invoke((Action<string, Color>)SetConnStatus, msg, color); return; }
            _lblConnStatus.Text = msg;
            _lblConnStatus.ForeColor = color;
        }

        // ── Cleanup ───────────────────────────────────────────────────────────

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            _engine?.Stop();
        }
    }
}