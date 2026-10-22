namespace BNet.RemoteDesktop
{
    partial class FileTransferForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileTransferForm));
            this._imageList = new System.Windows.Forms.ImageList(this.components);
            this._toolbar = new System.Windows.Forms.Panel();
            this._btnUpload = new System.Windows.Forms.Button();
            this._btnDownload = new System.Windows.Forms.Button();
            this._btnRefreshLocal = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this._progressPanel = new System.Windows.Forms.Panel();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._lblProgress = new System.Windows.Forms.Label();
            this._content = new System.Windows.Forms.TableLayoutPanel();
            this._grpLocal = new System.Windows.Forms.GroupBox();
            this._innerLocal = new System.Windows.Forms.Panel();
            this._listLocal = new System.Windows.Forms.ListView();
            this._colLocalName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colLocalSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colLocalDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._pathBarLocal = new System.Windows.Forms.Panel();
            this._btnLocalBack = new System.Windows.Forms.Button();
            this._btnLocalUp = new System.Windows.Forms.Button();
            this._txtLocalPath = new System.Windows.Forms.TextBox();
            this._grpRemote = new System.Windows.Forms.GroupBox();
            this._innerRemote = new System.Windows.Forms.Panel();
            this._listRemote = new System.Windows.Forms.ListView();
            this._colRemoteName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colRemoteSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._colRemoteDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._pathBarRemote = new System.Windows.Forms.Panel();
            this._btnRemoteBack = new System.Windows.Forms.Button();
            this._btnRemoteUp = new System.Windows.Forms.Button();
            this._txtRemotePath = new System.Windows.Forms.TextBox();
            this._toolbar.SuspendLayout();
            this._progressPanel.SuspendLayout();
            this._content.SuspendLayout();
            this._grpLocal.SuspendLayout();
            this._innerLocal.SuspendLayout();
            this._pathBarLocal.SuspendLayout();
            this._grpRemote.SuspendLayout();
            this._innerRemote.SuspendLayout();
            this._pathBarRemote.SuspendLayout();
            this.SuspendLayout();
            // 
            // _imageList
            // 
            this._imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this._imageList.ImageSize = new System.Drawing.Size(16, 16);
            this._imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _toolbar
            // 
            this._toolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(42)))));
            this._toolbar.Controls.Add(this._btnUpload);
            this._toolbar.Controls.Add(this._btnDownload);
            this._toolbar.Controls.Add(this._btnRefreshLocal);
            this._toolbar.Controls.Add(this._lblStatus);
            this._toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this._toolbar.Location = new System.Drawing.Point(0, 0);
            this._toolbar.Name = "_toolbar";
            this._toolbar.Size = new System.Drawing.Size(982, 48);
            this._toolbar.TabIndex = 2;
            // 
            // _btnUpload
            // 
            this._btnUpload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(140)))), ((int)(((byte)(60)))));
            this._btnUpload.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnUpload.FlatAppearance.BorderSize = 0;
            this._btnUpload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnUpload.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._btnUpload.ForeColor = System.Drawing.Color.White;
            this._btnUpload.Location = new System.Drawing.Point(8, 9);
            this._btnUpload.Name = "_btnUpload";
            this._btnUpload.Size = new System.Drawing.Size(104, 30);
            this._btnUpload.TabIndex = 0;
            this._btnUpload.Text = "Upload";
            this._btnUpload.UseVisualStyleBackColor = false;
            this._btnUpload.Click += new System.EventHandler(this.OnUploadClick);
            // 
            // _btnDownload
            // 
            this._btnDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(100)))), ((int)(((byte)(180)))));
            this._btnDownload.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnDownload.FlatAppearance.BorderSize = 0;
            this._btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDownload.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._btnDownload.ForeColor = System.Drawing.Color.White;
            this._btnDownload.Location = new System.Drawing.Point(120, 9);
            this._btnDownload.Name = "_btnDownload";
            this._btnDownload.Size = new System.Drawing.Size(104, 30);
            this._btnDownload.TabIndex = 1;
            this._btnDownload.Text = "Download";
            this._btnDownload.UseVisualStyleBackColor = false;
            this._btnDownload.Click += new System.EventHandler(this.OnDownloadClick);
            // 
            // _btnRefreshLocal
            // 
            this._btnRefreshLocal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(65)))));
            this._btnRefreshLocal.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnRefreshLocal.FlatAppearance.BorderSize = 0;
            this._btnRefreshLocal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnRefreshLocal.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._btnRefreshLocal.ForeColor = System.Drawing.Color.White;
            this._btnRefreshLocal.Location = new System.Drawing.Point(232, 9);
            this._btnRefreshLocal.Name = "_btnRefreshLocal";
            this._btnRefreshLocal.Size = new System.Drawing.Size(100, 30);
            this._btnRefreshLocal.TabIndex = 2;
            this._btnRefreshLocal.Text = "Refresh Local";
            this._btnRefreshLocal.UseVisualStyleBackColor = false;
            this._btnRefreshLocal.Click += new System.EventHandler(this.OnRefresh);
            // 
            // _lblStatus
            // 
            this._lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._lblStatus.BackColor = System.Drawing.Color.Transparent;
            this._lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this._lblStatus.Location = new System.Drawing.Point(1382, 9);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(350, 30);
            this._lblStatus.TabIndex = 3;
            this._lblStatus.Text = "Ready";
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _progressPanel
            // 
            this._progressPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(25)))));
            this._progressPanel.Controls.Add(this._progressBar);
            this._progressPanel.Controls.Add(this._lblProgress);
            this._progressPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._progressPanel.Location = new System.Drawing.Point(0, 563);
            this._progressPanel.Name = "_progressPanel";
            this._progressPanel.Size = new System.Drawing.Size(982, 30);
            this._progressPanel.TabIndex = 1;
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(8, 6);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(680, 18);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 0;
            this._progressBar.TabStop = false;
            // 
            // _lblProgress
            // 
            this._lblProgress.BackColor = System.Drawing.Color.Transparent;
            this._lblProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this._lblProgress.Location = new System.Drawing.Point(700, 6);
            this._lblProgress.Name = "_lblProgress";
            this._lblProgress.Size = new System.Drawing.Size(250, 18);
            this._lblProgress.TabIndex = 1;
            this._lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _content
            // 
            this._content.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
            this._content.ColumnCount = 2;
            this._content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._content.Controls.Add(this._grpLocal, 0, 0);
            this._content.Controls.Add(this._grpRemote, 1, 0);
            this._content.Dock = System.Windows.Forms.DockStyle.Fill;
            this._content.Location = new System.Drawing.Point(0, 48);
            this._content.Name = "_content";
            this._content.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this._content.RowCount = 1;
            this._content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._content.Size = new System.Drawing.Size(982, 515);
            this._content.TabIndex = 0;
            // 
            // _grpLocal
            // 
            this._grpLocal.Controls.Add(this._innerLocal);
            this._grpLocal.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grpLocal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(155)))));
            this._grpLocal.Location = new System.Drawing.Point(9, 7);
            this._grpLocal.Name = "_grpLocal";
            this._grpLocal.Padding = new System.Windows.Forms.Padding(4);
            this._grpLocal.Size = new System.Drawing.Size(479, 501);
            this._grpLocal.TabIndex = 0;
            this._grpLocal.TabStop = false;
            this._grpLocal.Text = "Local (Viewer)";
            // 
            // _innerLocal
            // 
            this._innerLocal.Controls.Add(this._listLocal);
            this._innerLocal.Controls.Add(this._pathBarLocal);
            this._innerLocal.Dock = System.Windows.Forms.DockStyle.Fill;
            this._innerLocal.Location = new System.Drawing.Point(4, 24);
            this._innerLocal.Name = "_innerLocal";
            this._innerLocal.Size = new System.Drawing.Size(471, 473);
            this._innerLocal.TabIndex = 0;
            // 
            // _listLocal
            // 
            this._listLocal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(40)))));
            this._listLocal.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colLocalName,
            this._colLocalSize,
            this._colLocalDate});
            this._listLocal.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listLocal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this._listLocal.FullRowSelect = true;
            this._listLocal.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._listLocal.HideSelection = false;
            this._listLocal.Location = new System.Drawing.Point(0, 28);
            this._listLocal.Name = "_listLocal";
            this._listLocal.Size = new System.Drawing.Size(471, 445);
            this._listLocal.SmallImageList = this._imageList;
            this._listLocal.TabIndex = 1;
            this._listLocal.UseCompatibleStateImageBehavior = false;
            this._listLocal.View = System.Windows.Forms.View.Details;
            this._listLocal.DoubleClick += new System.EventHandler(this.OnLocalDoubleClick);
            // 
            // _colLocalName
            // 
            this._colLocalName.Text = "Name";
            this._colLocalName.Width = 220;
            // 
            // _colLocalSize
            // 
            this._colLocalSize.Text = "Size";
            this._colLocalSize.Width = 80;
            // 
            // _colLocalDate
            // 
            this._colLocalDate.Text = "Modified";
            this._colLocalDate.Width = 130;
            // 
            // _pathBarLocal
            // 
            this._pathBarLocal.Controls.Add(this._btnLocalBack);
            this._pathBarLocal.Controls.Add(this._btnLocalUp);
            this._pathBarLocal.Controls.Add(this._txtLocalPath);
            this._pathBarLocal.Dock = System.Windows.Forms.DockStyle.Top;
            this._pathBarLocal.Location = new System.Drawing.Point(0, 0);
            this._pathBarLocal.Name = "_pathBarLocal";
            this._pathBarLocal.Size = new System.Drawing.Size(471, 28);
            this._pathBarLocal.TabIndex = 2;
            // 
            // _btnLocalBack
            // 
            this._btnLocalBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            this._btnLocalBack.Enabled = false;
            this._btnLocalBack.FlatAppearance.BorderSize = 0;
            this._btnLocalBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnLocalBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this._btnLocalBack.Location = new System.Drawing.Point(0, 2);
            this._btnLocalBack.Name = "_btnLocalBack";
            this._btnLocalBack.Size = new System.Drawing.Size(28, 24);
            this._btnLocalBack.TabIndex = 0;
            this._btnLocalBack.TabStop = false;
            this._btnLocalBack.Text = "<";
            this._btnLocalBack.UseVisualStyleBackColor = false;
            this._btnLocalBack.Click += new System.EventHandler(this.OnLocalBackClick);
            // 
            // _btnLocalUp
            // 
            this._btnLocalUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            this._btnLocalUp.FlatAppearance.BorderSize = 0;
            this._btnLocalUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnLocalUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this._btnLocalUp.Location = new System.Drawing.Point(30, 2);
            this._btnLocalUp.Name = "_btnLocalUp";
            this._btnLocalUp.Size = new System.Drawing.Size(28, 24);
            this._btnLocalUp.TabIndex = 1;
            this._btnLocalUp.TabStop = false;
            this._btnLocalUp.Text = "^";
            this._btnLocalUp.UseVisualStyleBackColor = false;
            this._btnLocalUp.Click += new System.EventHandler(this.OnLocalUpClick);
            // 
            // _txtLocalPath
            // 
            this._txtLocalPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtLocalPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(50)))));
            this._txtLocalPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtLocalPath.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this._txtLocalPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this._txtLocalPath.Location = new System.Drawing.Point(62, 4);
            this._txtLocalPath.Name = "_txtLocalPath";
            this._txtLocalPath.Size = new System.Drawing.Size(571, 24);
            this._txtLocalPath.TabIndex = 0;
            this._txtLocalPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnLocalPathEnter);
            // 
            // _grpRemote
            // 
            this._grpRemote.Controls.Add(this._innerRemote);
            this._grpRemote.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grpRemote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(155)))));
            this._grpRemote.Location = new System.Drawing.Point(494, 7);
            this._grpRemote.Name = "_grpRemote";
            this._grpRemote.Padding = new System.Windows.Forms.Padding(4);
            this._grpRemote.Size = new System.Drawing.Size(479, 501);
            this._grpRemote.TabIndex = 1;
            this._grpRemote.TabStop = false;
            this._grpRemote.Text = "Remote (Host)";
            // 
            // _innerRemote
            // 
            this._innerRemote.Controls.Add(this._listRemote);
            this._innerRemote.Controls.Add(this._pathBarRemote);
            this._innerRemote.Dock = System.Windows.Forms.DockStyle.Fill;
            this._innerRemote.Location = new System.Drawing.Point(4, 24);
            this._innerRemote.Name = "_innerRemote";
            this._innerRemote.Size = new System.Drawing.Size(471, 473);
            this._innerRemote.TabIndex = 0;
            // 
            // _listRemote
            // 
            this._listRemote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(40)))));
            this._listRemote.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colRemoteName,
            this._colRemoteSize,
            this._colRemoteDate});
            this._listRemote.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listRemote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this._listRemote.FullRowSelect = true;
            this._listRemote.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._listRemote.HideSelection = false;
            this._listRemote.Location = new System.Drawing.Point(0, 28);
            this._listRemote.Name = "_listRemote";
            this._listRemote.Size = new System.Drawing.Size(471, 445);
            this._listRemote.SmallImageList = this._imageList;
            this._listRemote.TabIndex = 1;
            this._listRemote.UseCompatibleStateImageBehavior = false;
            this._listRemote.View = System.Windows.Forms.View.Details;
            this._listRemote.DoubleClick += new System.EventHandler(this.OnRemoteDoubleClick);
            // 
            // _colRemoteName
            // 
            this._colRemoteName.Text = "Name";
            this._colRemoteName.Width = 220;
            // 
            // _colRemoteSize
            // 
            this._colRemoteSize.Text = "Size";
            this._colRemoteSize.Width = 80;
            // 
            // _colRemoteDate
            // 
            this._colRemoteDate.Text = "Modified";
            this._colRemoteDate.Width = 130;
            // 
            // _pathBarRemote
            // 
            this._pathBarRemote.Controls.Add(this._btnRemoteBack);
            this._pathBarRemote.Controls.Add(this._btnRemoteUp);
            this._pathBarRemote.Controls.Add(this._txtRemotePath);
            this._pathBarRemote.Dock = System.Windows.Forms.DockStyle.Top;
            this._pathBarRemote.Location = new System.Drawing.Point(0, 0);
            this._pathBarRemote.Name = "_pathBarRemote";
            this._pathBarRemote.Size = new System.Drawing.Size(471, 28);
            this._pathBarRemote.TabIndex = 2;
            // 
            // _btnRemoteBack
            // 
            this._btnRemoteBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            this._btnRemoteBack.Enabled = false;
            this._btnRemoteBack.FlatAppearance.BorderSize = 0;
            this._btnRemoteBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnRemoteBack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this._btnRemoteBack.Location = new System.Drawing.Point(0, 2);
            this._btnRemoteBack.Name = "_btnRemoteBack";
            this._btnRemoteBack.Size = new System.Drawing.Size(28, 24);
            this._btnRemoteBack.TabIndex = 0;
            this._btnRemoteBack.TabStop = false;
            this._btnRemoteBack.Text = "<";
            this._btnRemoteBack.UseVisualStyleBackColor = false;
            this._btnRemoteBack.Click += new System.EventHandler(this.OnRemoteBackClick);
            // 
            // _btnRemoteUp
            // 
            this._btnRemoteUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(58)))));
            this._btnRemoteUp.FlatAppearance.BorderSize = 0;
            this._btnRemoteUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnRemoteUp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this._btnRemoteUp.Location = new System.Drawing.Point(30, 2);
            this._btnRemoteUp.Name = "_btnRemoteUp";
            this._btnRemoteUp.Size = new System.Drawing.Size(28, 24);
            this._btnRemoteUp.TabIndex = 1;
            this._btnRemoteUp.TabStop = false;
            this._btnRemoteUp.Text = "^";
            this._btnRemoteUp.UseVisualStyleBackColor = false;
            this._btnRemoteUp.Click += new System.EventHandler(this.OnRemoteUpClick);
            // 
            // _txtRemotePath
            // 
            this._txtRemotePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtRemotePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(50)))));
            this._txtRemotePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._txtRemotePath.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this._txtRemotePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this._txtRemotePath.Location = new System.Drawing.Point(62, 4);
            this._txtRemotePath.Name = "_txtRemotePath";
            this._txtRemotePath.Size = new System.Drawing.Size(571, 24);
            this._txtRemotePath.TabIndex = 0;
            this._txtRemotePath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnRemotePathEnter);
            // 
            // FileTransferForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(982, 593);
            this.Controls.Add(this._content);
            this.Controls.Add(this._progressPanel);
            this.Controls.Add(this._toolbar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(720, 500);
            this.Name = "FileTransferForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File Transfer";
            this._toolbar.ResumeLayout(false);
            this._progressPanel.ResumeLayout(false);
            this._content.ResumeLayout(false);
            this._grpLocal.ResumeLayout(false);
            this._innerLocal.ResumeLayout(false);
            this._pathBarLocal.ResumeLayout(false);
            this._pathBarLocal.PerformLayout();
            this._grpRemote.ResumeLayout(false);
            this._innerRemote.ResumeLayout(false);
            this._pathBarRemote.ResumeLayout(false);
            this._pathBarRemote.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        // ── Designer fields ───────────────────────────────────────────────────
        // Images
        private System.Windows.Forms.ImageList _imageList;

        // Top-level containers
        private System.Windows.Forms.Panel _toolbar;
        private System.Windows.Forms.Panel _progressPanel;
        private System.Windows.Forms.TableLayoutPanel _content;

        // Toolbar
        private System.Windows.Forms.Button _btnUpload;
        private System.Windows.Forms.Button _btnDownload;
        private System.Windows.Forms.Button _btnRefreshLocal;
        private System.Windows.Forms.Label _lblStatus;

        // Progress
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label _lblProgress;

        // Local pane
        private System.Windows.Forms.GroupBox _grpLocal;
        private System.Windows.Forms.Panel _innerLocal;
        private System.Windows.Forms.Panel _pathBarLocal;
        private System.Windows.Forms.Button _btnLocalBack;
        private System.Windows.Forms.Button _btnLocalUp;
        private System.Windows.Forms.TextBox _txtLocalPath;
        private System.Windows.Forms.ListView _listLocal;
        private System.Windows.Forms.ColumnHeader _colLocalName;
        private System.Windows.Forms.ColumnHeader _colLocalSize;
        private System.Windows.Forms.ColumnHeader _colLocalDate;

        // Remote pane
        private System.Windows.Forms.GroupBox _grpRemote;
        private System.Windows.Forms.Panel _innerRemote;
        private System.Windows.Forms.Panel _pathBarRemote;
        private System.Windows.Forms.Button _btnRemoteBack;
        private System.Windows.Forms.Button _btnRemoteUp;
        private System.Windows.Forms.TextBox _txtRemotePath;
        private System.Windows.Forms.ListView _listRemote;
        private System.Windows.Forms.ColumnHeader _colRemoteName;
        private System.Windows.Forms.ColumnHeader _colRemoteSize;
        private System.Windows.Forms.ColumnHeader _colRemoteDate;
    }
}