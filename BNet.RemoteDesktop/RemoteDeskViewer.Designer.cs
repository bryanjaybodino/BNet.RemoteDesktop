using System.Windows.Forms;

namespace BNet.RemoteDesktop
{
    partial class RemoteDeskViewer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoteDeskViewer));
            this._toolbar = new System.Windows.Forms.Panel();
            this._cboScreen = new System.Windows.Forms.ComboBox();
            this._btnDisconnect = new System.Windows.Forms.Button();
            this._btnSendKeys = new System.Windows.Forms.Button();
            this._btnFiles = new System.Windows.Forms.Button();
            this._lblInfo = new System.Windows.Forms.Label();
            this._screen = new System.Windows.Forms.PictureBox();
            this._menuSendKeys = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._screen)).BeginInit();
            this.SuspendLayout();
            // 
            // _toolbar
            // 
            this._toolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this._toolbar.Controls.Add(this._cboScreen);
            this._toolbar.Controls.Add(this._btnDisconnect);
            this._toolbar.Controls.Add(this._btnSendKeys);
            this._toolbar.Controls.Add(this._btnFiles);
            this._toolbar.Controls.Add(this._lblInfo);
            this._toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this._toolbar.Location = new System.Drawing.Point(0, 0);
            this._toolbar.Name = "_toolbar";
            this._toolbar.Size = new System.Drawing.Size(1100, 50);
            this._toolbar.TabIndex = 0;
            // 
            // _cboScreen
            // 
            this._cboScreen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this._cboScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._cboScreen.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._cboScreen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this._cboScreen.Location = new System.Drawing.Point(334, 11);
            this._cboScreen.Name = "_cboScreen";
            this._cboScreen.Size = new System.Drawing.Size(180, 27);
            this._cboScreen.TabIndex = 0;
            this._cboScreen.TabStop = false;
            this._cboScreen.SelectedIndexChanged += new System.EventHandler(this.OnScreenSelected);
            // 
            // _btnDisconnect
            // 
            this._btnDisconnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this._btnDisconnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnDisconnect.FlatAppearance.BorderSize = 0;
            this._btnDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDisconnect.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._btnDisconnect.ForeColor = System.Drawing.Color.White;
            this._btnDisconnect.Location = new System.Drawing.Point(12, 11);
            this._btnDisconnect.Name = "_btnDisconnect";
            this._btnDisconnect.Size = new System.Drawing.Size(100, 28);
            this._btnDisconnect.TabIndex = 1;
            this._btnDisconnect.TabStop = false;
            this._btnDisconnect.Text = "Disconnect";
            this._btnDisconnect.UseVisualStyleBackColor = false;
            this._btnDisconnect.Click += new System.EventHandler(this.OnDisconnect);
            // 
            // _btnSendKeys
            // 
            this._btnSendKeys.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            this._btnSendKeys.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnSendKeys.FlatAppearance.BorderSize = 0;
            this._btnSendKeys.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSendKeys.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._btnSendKeys.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this._btnSendKeys.Location = new System.Drawing.Point(120, 11);
            this._btnSendKeys.Name = "_btnSendKeys";
            this._btnSendKeys.Size = new System.Drawing.Size(108, 28);
            this._btnSendKeys.TabIndex = 2;
            this._btnSendKeys.TabStop = false;
            this._btnSendKeys.Text = "Send Keys  ▾";
            this._btnSendKeys.UseVisualStyleBackColor = false;
            // 
            // _btnFiles
            // 
            this._btnFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(100)))), ((int)(((byte)(160)))));
            this._btnFiles.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnFiles.FlatAppearance.BorderSize = 0;
            this._btnFiles.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnFiles.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._btnFiles.ForeColor = System.Drawing.Color.White;
            this._btnFiles.Location = new System.Drawing.Point(236, 11);
            this._btnFiles.Name = "_btnFiles";
            this._btnFiles.Size = new System.Drawing.Size(90, 28);
            this._btnFiles.TabIndex = 3;
            this._btnFiles.TabStop = false;
            this._btnFiles.Text = "📁 Files";
            this._btnFiles.UseVisualStyleBackColor = false;
            // 
            // _lblInfo
            // 
            this._lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._lblInfo.BackColor = System.Drawing.Color.Transparent;
            this._lblInfo.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(140)))), ((int)(((byte)(140)))));
            this._lblInfo.Location = new System.Drawing.Point(640, 16);
            this._lblInfo.Name = "_lblInfo";
            this._lblInfo.Size = new System.Drawing.Size(452, 18);
            this._lblInfo.TabIndex = 4;
            this._lblInfo.Text = "Waiting for first frame…";
            this._lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _screen
            // 
            this._screen.BackColor = System.Drawing.Color.Black;
            this._screen.Cursor = System.Windows.Forms.Cursors.Default;
            this._screen.Dock = System.Windows.Forms.DockStyle.Fill;
            this._screen.Location = new System.Drawing.Point(0, 50);
            this._screen.Name = "_screen";
            this._screen.Size = new System.Drawing.Size(1100, 570);
            this._screen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._screen.TabIndex = 1;
            this._screen.TabStop = false;
            this._screen.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this._screen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this._screen.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
            this._screen.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.OnMouseWheel);
            // 
            // _menuSendKeys
            // 
            this._menuSendKeys.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this._menuSendKeys.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._menuSendKeys.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this._menuSendKeys.ImageScalingSize = new System.Drawing.Size(20, 20);
            this._menuSendKeys.Name = "_menuSendKeys";
            this._menuSendKeys.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this._menuSendKeys.ShowImageMargin = false;
            this._menuSendKeys.Size = new System.Drawing.Size(36, 4);
            // 
            // RemoteDeskViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1100, 620);
            this.Controls.Add(this._screen);
            this.Controls.Add(this._toolbar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(700, 420);
            this.Name = "RemoteDeskViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RemoteDesk  —  Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            this._toolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._screen)).EndInit();
            this.ResumeLayout(false);

        }

        // ── Low-level key event factories ──────────────────────────────────────

        private static BNet.RemoteDesktop.Models.RemoteInputEvent KD(ushort vk, bool ext = false)
            => new BNet.RemoteDesktop.Models.RemoteInputEvent
            { Type = "keydown", VirtualKey = vk, Extended = ext };

        private static BNet.RemoteDesktop.Models.RemoteInputEvent KU(ushort vk, bool ext = false)
            => new BNet.RemoteDesktop.Models.RemoteInputEvent
            { Type = "keyup", VirtualKey = vk, Extended = ext };

        #endregion

        private System.Windows.Forms.Panel _toolbar;
        private System.Windows.Forms.Button _btnDisconnect;
        private System.Windows.Forms.Button _btnSendKeys;
        private System.Windows.Forms.Button _btnFiles;           // ← NEW
        private System.Windows.Forms.Label _lblInfo;
        private System.Windows.Forms.PictureBox _screen;
        private System.Windows.Forms.ComboBox _cboScreen;
        private ContextMenuStrip _menuSendKeys;
    }
}