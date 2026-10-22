namespace BNet.RemoteDesktop
{
    partial class RemoteDeskHome
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoteDeskHome));
            this._panelTopBar = new System.Windows.Forms.Panel();
            this._lblAppName = new System.Windows.Forms.Label();
            this._lblAppIcon = new System.Windows.Forms.Label();
            this._panelBody = new System.Windows.Forms.Panel();
            this._panelHostCard = new System.Windows.Forms.Panel();
            this._lblHostTitle = new System.Windows.Forms.Label();
            this._lblHostSubtitle = new System.Windows.Forms.Label();
            this._sepHost1 = new System.Windows.Forms.Panel();
            this._lblYourAddress = new System.Windows.Forms.Label();
            this._lblAddressHint = new System.Windows.Forms.Label();
            this._lstMyIps = new System.Windows.Forms.ListBox();
            this._sepHost2 = new System.Windows.Forms.Panel();
            this._lblHostPort = new System.Windows.Forms.Label();
            this._txtHostPort = new System.Windows.Forms.TextBox();
            this._btnStartHost = new System.Windows.Forms.Button();
            this._btnStopHost = new System.Windows.Forms.Button();
            this._lblHostStatus = new System.Windows.Forms.Label();
            this._panelViewerCard = new System.Windows.Forms.Panel();
            this._lblViewerTitle = new System.Windows.Forms.Label();
            this._lblViewerSubtitle = new System.Windows.Forms.Label();
            this._sepViewer1 = new System.Windows.Forms.Panel();
            this._lblRemoteAddress = new System.Windows.Forms.Label();
            this._txtTargetIp = new System.Windows.Forms.TextBox();
            this._lblTargetPort = new System.Windows.Forms.Label();
            this._txtTargetPort = new System.Windows.Forms.TextBox();
            this._btnConnect = new System.Windows.Forms.Button();
            this._lblConnStatus = new System.Windows.Forms.Label();
            this._panelFooter = new System.Windows.Forms.Panel();
            this._lblFooter = new System.Windows.Forms.Label();
            this._panelTopBar.SuspendLayout();
            this._panelBody.SuspendLayout();
            this._panelHostCard.SuspendLayout();
            this._panelViewerCard.SuspendLayout();
            this._panelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // _panelTopBar
            // 
            this._panelTopBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this._panelTopBar.Controls.Add(this._lblAppName);
            this._panelTopBar.Controls.Add(this._lblAppIcon);
            this._panelTopBar.Dock = System.Windows.Forms.DockStyle.Top;
            this._panelTopBar.Location = new System.Drawing.Point(0, 0);
            this._panelTopBar.Name = "_panelTopBar";
            this._panelTopBar.Size = new System.Drawing.Size(860, 52);
            this._panelTopBar.TabIndex = 0;
            // 
            // _lblAppName
            // 
            this._lblAppName.AutoSize = true;
            this._lblAppName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this._lblAppName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._lblAppName.Location = new System.Drawing.Point(49, 10);
            this._lblAppName.Name = "_lblAppName";
            this._lblAppName.Size = new System.Drawing.Size(236, 28);
            this._lblAppName.TabIndex = 1;
            this._lblAppName.Text = "BNet RemoteDesk";
            // 
            // _lblAppIcon
            // 
            this._lblAppIcon.AutoSize = true;
            this._lblAppIcon.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this._lblAppIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(57)))), ((int)(((byte)(53)))));
            this._lblAppIcon.Location = new System.Drawing.Point(14, 0);
            this._lblAppIcon.Name = "_lblAppIcon";
            this._lblAppIcon.Size = new System.Drawing.Size(48, 46);
            this._lblAppIcon.TabIndex = 0;
            this._lblAppIcon.Text = "⬡";
            // 
            // _panelBody
            // 
            this._panelBody.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this._panelBody.Controls.Add(this._panelHostCard);
            this._panelBody.Controls.Add(this._panelViewerCard);
            this._panelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this._panelBody.Location = new System.Drawing.Point(0, 52);
            this._panelBody.Name = "_panelBody";
            this._panelBody.Size = new System.Drawing.Size(860, 456);
            this._panelBody.TabIndex = 1;
            // 
            // _panelHostCard
            // 
            this._panelHostCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this._panelHostCard.Controls.Add(this._lblHostTitle);
            this._panelHostCard.Controls.Add(this._lblHostSubtitle);
            this._panelHostCard.Controls.Add(this._sepHost1);
            this._panelHostCard.Controls.Add(this._lblYourAddress);
            this._panelHostCard.Controls.Add(this._lblAddressHint);
            this._panelHostCard.Controls.Add(this._lstMyIps);
            this._panelHostCard.Controls.Add(this._sepHost2);
            this._panelHostCard.Controls.Add(this._lblHostPort);
            this._panelHostCard.Controls.Add(this._txtHostPort);
            this._panelHostCard.Controls.Add(this._btnStartHost);
            this._panelHostCard.Controls.Add(this._btnStopHost);
            this._panelHostCard.Controls.Add(this._lblHostStatus);
            this._panelHostCard.Location = new System.Drawing.Point(20, 16);
            this._panelHostCard.Name = "_panelHostCard";
            this._panelHostCard.Size = new System.Drawing.Size(394, 448);
            this._panelHostCard.TabIndex = 0;
            // 
            // _lblHostTitle
            // 
            this._lblHostTitle.AutoSize = true;
            this._lblHostTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._lblHostTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._lblHostTitle.Location = new System.Drawing.Point(22, 20);
            this._lblHostTitle.Name = "_lblHostTitle";
            this._lblHostTitle.Size = new System.Drawing.Size(95, 25);
            this._lblHostTitle.TabIndex = 0;
            this._lblHostTitle.Text = "This Desk";
            // 
            // _lblHostSubtitle
            // 
            this._lblHostSubtitle.AutoSize = true;
            this._lblHostSubtitle.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblHostSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblHostSubtitle.Location = new System.Drawing.Point(22, 46);
            this._lblHostSubtitle.Name = "_lblHostSubtitle";
            this._lblHostSubtitle.Size = new System.Drawing.Size(256, 19);
            this._lblHostSubtitle.TabIndex = 1;
            this._lblHostSubtitle.Text = "Share this machine with a remote viewer";
            // 
            // _sepHost1
            // 
            this._sepHost1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this._sepHost1.Location = new System.Drawing.Point(22, 74);
            this._sepHost1.Name = "_sepHost1";
            this._sepHost1.Size = new System.Drawing.Size(350, 1);
            this._sepHost1.TabIndex = 2;
            // 
            // _lblYourAddress
            // 
            this._lblYourAddress.AutoSize = true;
            this._lblYourAddress.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this._lblYourAddress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblYourAddress.Location = new System.Drawing.Point(22, 86);
            this._lblYourAddress.Name = "_lblYourAddress";
            this._lblYourAddress.Size = new System.Drawing.Size(149, 17);
            this._lblYourAddress.TabIndex = 3;
            this._lblYourAddress.Text = "YOUR LAN ADDRESSES";
            // 
            // _lblAddressHint
            // 
            this._lblAddressHint.AutoSize = true;
            this._lblAddressHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblAddressHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblAddressHint.Location = new System.Drawing.Point(22, 102);
            this._lblAddressHint.Name = "_lblAddressHint";
            this._lblAddressHint.Size = new System.Drawing.Size(220, 19);
            this._lblAddressHint.TabIndex = 4;
            this._lblAddressHint.Text = "Share one of these with the viewer";
            // 
            // _lstMyIps
            // 
            this._lstMyIps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this._lstMyIps.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._lstMyIps.Font = new System.Drawing.Font("Consolas", 9.5F);
            this._lstMyIps.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(57)))), ((int)(((byte)(53)))));
            this._lstMyIps.ItemHeight = 19;
            this._lstMyIps.Location = new System.Drawing.Point(22, 120);
            this._lstMyIps.Name = "_lstMyIps";
            this._lstMyIps.Size = new System.Drawing.Size(350, 76);
            this._lstMyIps.TabIndex = 5;
            // 
            // _sepHost2
            // 
            this._sepHost2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this._sepHost2.Location = new System.Drawing.Point(22, 222);
            this._sepHost2.Name = "_sepHost2";
            this._sepHost2.Size = new System.Drawing.Size(350, 1);
            this._sepHost2.TabIndex = 6;
            // 
            // _lblHostPort
            // 
            this._lblHostPort.AutoSize = true;
            this._lblHostPort.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this._lblHostPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblHostPort.Location = new System.Drawing.Point(22, 234);
            this._lblHostPort.Name = "_lblHostPort";
            this._lblHostPort.Size = new System.Drawing.Size(89, 17);
            this._lblHostPort.TabIndex = 7;
            this._lblHostPort.Text = "LISTEN PORT";
            // 
            // _txtHostPort
            // 
            this._txtHostPort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this._txtHostPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtHostPort.Font = new System.Drawing.Font("Consolas", 9.5F);
            this._txtHostPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._txtHostPort.Location = new System.Drawing.Point(22, 252);
            this._txtHostPort.Name = "_txtHostPort";
            this._txtHostPort.Size = new System.Drawing.Size(100, 26);
            this._txtHostPort.TabIndex = 8;
            this._txtHostPort.Text = "7891";
            // 
            // _btnStartHost
            // 
            this._btnStartHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(57)))), ((int)(((byte)(53)))));
            this._btnStartHost.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnStartHost.FlatAppearance.BorderSize = 0;
            this._btnStartHost.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnStartHost.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnStartHost.ForeColor = System.Drawing.Color.White;
            this._btnStartHost.Location = new System.Drawing.Point(22, 294);
            this._btnStartHost.Name = "_btnStartHost";
            this._btnStartHost.Size = new System.Drawing.Size(168, 36);
            this._btnStartHost.TabIndex = 9;
            this._btnStartHost.Text = "▶  Start Hosting";
            this._btnStartHost.UseVisualStyleBackColor = false;
            this._btnStartHost.Click += new System.EventHandler(this.OnStartHost);
            // 
            // _btnStopHost
            // 
            this._btnStopHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this._btnStopHost.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnStopHost.Enabled = false;
            this._btnStopHost.FlatAppearance.BorderSize = 0;
            this._btnStopHost.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnStopHost.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnStopHost.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this._btnStopHost.Location = new System.Drawing.Point(204, 294);
            this._btnStopHost.Name = "_btnStopHost";
            this._btnStopHost.Size = new System.Drawing.Size(168, 36);
            this._btnStopHost.TabIndex = 10;
            this._btnStopHost.Text = "■  Stop Hosting";
            this._btnStopHost.UseVisualStyleBackColor = false;
            this._btnStopHost.Click += new System.EventHandler(this.OnStopHost);
            // 
            // _lblHostStatus
            // 
            this._lblHostStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblHostStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblHostStatus.Location = new System.Drawing.Point(22, 344);
            this._lblHostStatus.Name = "_lblHostStatus";
            this._lblHostStatus.Size = new System.Drawing.Size(350, 44);
            this._lblHostStatus.TabIndex = 11;
            this._lblHostStatus.Text = "Not hosting.";
            // 
            // _panelViewerCard
            // 
            this._panelViewerCard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this._panelViewerCard.Controls.Add(this._lblViewerTitle);
            this._panelViewerCard.Controls.Add(this._lblViewerSubtitle);
            this._panelViewerCard.Controls.Add(this._sepViewer1);
            this._panelViewerCard.Controls.Add(this._lblRemoteAddress);
            this._panelViewerCard.Controls.Add(this._txtTargetIp);
            this._panelViewerCard.Controls.Add(this._lblTargetPort);
            this._panelViewerCard.Controls.Add(this._txtTargetPort);
            this._panelViewerCard.Controls.Add(this._btnConnect);
            this._panelViewerCard.Controls.Add(this._lblConnStatus);
            this._panelViewerCard.Location = new System.Drawing.Point(446, 16);
            this._panelViewerCard.Name = "_panelViewerCard";
            this._panelViewerCard.Size = new System.Drawing.Size(394, 448);
            this._panelViewerCard.TabIndex = 1;
            // 
            // _lblViewerTitle
            // 
            this._lblViewerTitle.AutoSize = true;
            this._lblViewerTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._lblViewerTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._lblViewerTitle.Location = new System.Drawing.Point(22, 20);
            this._lblViewerTitle.Name = "_lblViewerTitle";
            this._lblViewerTitle.Size = new System.Drawing.Size(128, 25);
            this._lblViewerTitle.TabIndex = 0;
            this._lblViewerTitle.Text = "Remote Desk";
            // 
            // _lblViewerSubtitle
            // 
            this._lblViewerSubtitle.AutoSize = true;
            this._lblViewerSubtitle.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblViewerSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblViewerSubtitle.Location = new System.Drawing.Point(22, 46);
            this._lblViewerSubtitle.Name = "_lblViewerSubtitle";
            this._lblViewerSubtitle.Size = new System.Drawing.Size(220, 19);
            this._lblViewerSubtitle.TabIndex = 1;
            this._lblViewerSubtitle.Text = "Control another machine remotely";
            // 
            // _sepViewer1
            // 
            this._sepViewer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this._sepViewer1.Location = new System.Drawing.Point(22, 74);
            this._sepViewer1.Name = "_sepViewer1";
            this._sepViewer1.Size = new System.Drawing.Size(350, 1);
            this._sepViewer1.TabIndex = 2;
            // 
            // _lblRemoteAddress
            // 
            this._lblRemoteAddress.AutoSize = true;
            this._lblRemoteAddress.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this._lblRemoteAddress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblRemoteAddress.Location = new System.Drawing.Point(22, 86);
            this._lblRemoteAddress.Name = "_lblRemoteAddress";
            this._lblRemoteAddress.Size = new System.Drawing.Size(121, 17);
            this._lblRemoteAddress.TabIndex = 3;
            this._lblRemoteAddress.Text = "REMOTE ADDRESS";
            // 
            // _txtTargetIp
            // 
            this._txtTargetIp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this._txtTargetIp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtTargetIp.Font = new System.Drawing.Font("Consolas", 13F);
            this._txtTargetIp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._txtTargetIp.Location = new System.Drawing.Point(22, 104);
            this._txtTargetIp.Name = "_txtTargetIp";
            this._txtTargetIp.Size = new System.Drawing.Size(234, 33);
            this._txtTargetIp.TabIndex = 4;
            // 
            // _lblTargetPort
            // 
            this._lblTargetPort.AutoSize = true;
            this._lblTargetPort.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this._lblTargetPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblTargetPort.Location = new System.Drawing.Point(272, 86);
            this._lblTargetPort.Name = "_lblTargetPort";
            this._lblTargetPort.Size = new System.Drawing.Size(42, 17);
            this._lblTargetPort.TabIndex = 5;
            this._lblTargetPort.Text = "PORT";
            // 
            // _txtTargetPort
            // 
            this._txtTargetPort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this._txtTargetPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtTargetPort.Font = new System.Drawing.Font("Consolas", 9.5F);
            this._txtTargetPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._txtTargetPort.Location = new System.Drawing.Point(272, 104);
            this._txtTargetPort.Name = "_txtTargetPort";
            this._txtTargetPort.Size = new System.Drawing.Size(100, 26);
            this._txtTargetPort.TabIndex = 6;
            this._txtTargetPort.Text = "7891";
            // 
            // _btnConnect
            // 
            this._btnConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this._btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnConnect.FlatAppearance.BorderSize = 0;
            this._btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnConnect.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._btnConnect.ForeColor = System.Drawing.Color.White;
            this._btnConnect.Location = new System.Drawing.Point(22, 160);
            this._btnConnect.Name = "_btnConnect";
            this._btnConnect.Size = new System.Drawing.Size(350, 40);
            this._btnConnect.TabIndex = 7;
            this._btnConnect.Text = "Connect  &&  View  →";
            this._btnConnect.UseVisualStyleBackColor = false;
            this._btnConnect.Click += new System.EventHandler(this.OnConnectAndView);
            // 
            // _lblConnStatus
            // 
            this._lblConnStatus.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblConnStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblConnStatus.Location = new System.Drawing.Point(22, 214);
            this._lblConnStatus.Name = "_lblConnStatus";
            this._lblConnStatus.Size = new System.Drawing.Size(350, 44);
            this._lblConnStatus.TabIndex = 8;
            this._lblConnStatus.Text = "Not connected.";
            // 
            // _panelFooter
            // 
            this._panelFooter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this._panelFooter.Controls.Add(this._lblFooter);
            this._panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._panelFooter.Location = new System.Drawing.Point(0, 508);
            this._panelFooter.Name = "_panelFooter";
            this._panelFooter.Size = new System.Drawing.Size(860, 32);
            this._panelFooter.TabIndex = 2;
            // 
            // _lblFooter
            // 
            this._lblFooter.Font = new System.Drawing.Font("Segoe UI", 8F);
            this._lblFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this._lblFooter.Location = new System.Drawing.Point(20, 8);
            this._lblFooter.Name = "_lblFooter";
            this._lblFooter.Size = new System.Drawing.Size(820, 18);
            this._lblFooter.TabIndex = 0;
            // 
            // RemoteDeskHome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.ClientSize = new System.Drawing.Size(860, 540);
            this.Controls.Add(this._panelBody);
            this.Controls.Add(this._panelFooter);
            this.Controls.Add(this._panelTopBar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "RemoteDeskHome";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BNet RemoteDesk";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this._panelTopBar.ResumeLayout(false);
            this._panelTopBar.PerformLayout();
            this._panelBody.ResumeLayout(false);
            this._panelHostCard.ResumeLayout(false);
            this._panelHostCard.PerformLayout();
            this._panelViewerCard.ResumeLayout(false);
            this._panelViewerCard.PerformLayout();
            this._panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _panelTopBar;
        private System.Windows.Forms.Label _lblAppIcon;
        private System.Windows.Forms.Label _lblAppName;
        private System.Windows.Forms.Panel _panelBody;
        private System.Windows.Forms.Panel _panelHostCard;
        private System.Windows.Forms.Label _lblHostTitle;
        private System.Windows.Forms.Label _lblHostSubtitle;
        private System.Windows.Forms.Panel _sepHost1;
        private System.Windows.Forms.Label _lblYourAddress;
        private System.Windows.Forms.Label _lblAddressHint;
        private System.Windows.Forms.ListBox _lstMyIps;
        private System.Windows.Forms.Panel _sepHost2;
        private System.Windows.Forms.Label _lblHostPort;
        private System.Windows.Forms.TextBox _txtHostPort;
        private System.Windows.Forms.Button _btnStartHost;
        private System.Windows.Forms.Button _btnStopHost;
        private System.Windows.Forms.Label _lblHostStatus;
        private System.Windows.Forms.Panel _panelViewerCard;
        private System.Windows.Forms.Label _lblViewerTitle;
        private System.Windows.Forms.Label _lblViewerSubtitle;
        private System.Windows.Forms.Panel _sepViewer1;
        private System.Windows.Forms.Label _lblRemoteAddress;
        private System.Windows.Forms.TextBox _txtTargetIp;
        private System.Windows.Forms.Label _lblTargetPort;
        private System.Windows.Forms.TextBox _txtTargetPort;
        private System.Windows.Forms.Button _btnConnect;
        private System.Windows.Forms.Label _lblConnStatus;
        private System.Windows.Forms.Panel _panelFooter;
        private System.Windows.Forms.Label _lblFooter;
    }
}