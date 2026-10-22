using BNet.RemoteDesktop.Models;
using BNet.RemoteDesktop.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BNet.RemoteDesktop
{
    // ── Conflict resolution choices ────────────────────────────────────────────
    /// <summary>
    /// The user's decision when a transfer target already exists.
    /// </summary>
    internal enum ConflictChoice
    {
        Overwrite,      // replace this one file
        OverwriteAll,   // replace this and all subsequent conflicts
        Skip,           // skip this file
        SkipAll,        // skip this and all subsequent conflicts
        Cancel          // abort the entire batch
    }

    public sealed partial class FileTransferForm : Form
    {
        // ── Engine reference ───────────────────────────────────────────────────
        private readonly PeerHttpEngine _engine;

        // ── State ──────────────────────────────────────────────────────────────
        private string _localPath;
        private string _remotePath;
        private bool _transferring;

        private readonly Stack<string> _localHistory = new Stack<string>();
        private readonly Stack<string> _remoteHistory = new Stack<string>();

        private string _remoteParentPath;

        // suppress per-file TransferComplete refresh during batch operations
        private bool _batchInProgress;

        // ── Image indices ──────────────────────────────────────────────────────
        private const int IMG_FOLDER = 0;
        private const int IMG_FILE = 1;
        private const int IMG_DRIVE = 2;

        // ── Construction ───────────────────────────────────────────────────────
        public FileTransferForm(PeerHttpEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException("engine");

            InitializeComponent();
            BuildImageList();
            WireInternalEvents();

            this.Text = "File Transfer  -  " + (_engine.PeerInfo?.MachineName ?? "Remote Host");
            _localPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            WireEngineEvents();
        }

        // ── Lambda events that cannot live in InitializeComponent ──────────────
        private void WireInternalEvents()
        {
            this._pathBarLocal.SizeChanged += (s, ev) =>
                this._txtLocalPath.Width = Math.Max(60, this._pathBarLocal.Width - 66);
            this._pathBarRemote.SizeChanged += (s, ev) =>
                this._txtRemotePath.Width = Math.Max(60, this._pathBarRemote.Width - 66);

            this._txtLocalPath.Enter += (s, ev) => ((TextBox)s).SelectAll();
            this._txtLocalPath.MouseUp += (s, ev) =>
            {
                if (((TextBox)s).SelectionLength == 0) ((TextBox)s).SelectAll();
            };

            this._txtRemotePath.Enter += (s, ev) => ((TextBox)s).SelectAll();
            this._txtRemotePath.MouseUp += (s, ev) =>
            {
                if (((TextBox)s).SelectionLength == 0) ((TextBox)s).SelectAll();
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _ = RefreshLocalAsync();
            _ = RefreshRemoteAsync();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnwireEngineEvents();
            base.OnFormClosed(e);
        }

        // ── Engine events ──────────────────────────────────────────────────────
        private void WireEngineEvents()
        {
            // _engine.TransferProgress += OnTransferProgress;
            // _engine.TransferComplete += OnTransferComplete;
            // _engine.TransferFailed += OnTransferFailed;
        }

        private void UnwireEngineEvents()
        {
            // _engine.TransferProgress -= OnTransferProgress;
            // _engine.TransferComplete -= OnTransferComplete;
            // _engine.TransferFailed -= OnTransferFailed;
        }

        private void OnTransferProgress(string id, long sent, long total)
        {
            if (InvokeRequired) { Invoke((Action)(() => OnTransferProgress(id, sent, total))); return; }
            if (total <= 0) return;
            int pct = (int)(sent * 100 / total);
            _progressBar.Value = Math.Min(100, pct);
            _lblProgress.Text = pct + "%  (" + FormatSize(sent) + " / " + FormatSize(total) + ")";
        }

        private void OnTransferComplete(string id, string path, bool isUpload)
        {
            if (InvokeRequired) { Invoke((Action)(() => OnTransferComplete(id, path, isUpload))); return; }

            _progressBar.Value = 100;
            _lblProgress.Text = "Done";
            SetStatus(isUpload ? "Upload complete -> " + path : "Download complete -> " + path);

            if (!_batchInProgress)
            {
                _transferring = false;
                SetTransferButtons(true);
                if (isUpload) _ = RefreshRemoteAsync();
                else _ = RefreshLocalAsync();
            }
        }

        private void OnTransferFailed(string id, string error, bool isUpload)
        {
            if (InvokeRequired) { Invoke((Action)(() => OnTransferFailed(id, error, isUpload))); return; }
            _transferring = false;
            _batchInProgress = false;
            SetTransferButtons(true);
            _progressBar.Value = 0;
            _lblProgress.Text = "";
            SetStatus((isUpload ? "Upload" : "Download") + " failed: " + error);
            MessageBox.Show(error, (isUpload ? "Upload" : "Download") + " Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ── Local file system ──────────────────────────────────────────────────
        private async Task RefreshLocalAsync()
        {
            _listLocal.Items.Clear();
            _txtLocalPath.Text = _localPath ?? "";

            try
            {
                if (string.IsNullOrWhiteSpace(_localPath))
                {
                    foreach (DriveInfo d in DriveInfo.GetDrives())
                    {
                        try
                        {
                            var item = new ListViewItem(d.Name, IMG_DRIVE);
                            item.SubItems.Add("");
                            item.SubItems.Add(FormatSize(d.TotalSize));
                            item.Tag = d.RootDirectory.FullName;
                            _listLocal.Items.Add(item);
                        }
                        catch { }
                    }
                    return;
                }

                var dir = new DirectoryInfo(_localPath);
                foreach (var sub in dir.GetDirectories().OrderBy(d => d.Name))
                {
                    try
                    {
                        var item = new ListViewItem(sub.Name, IMG_FOLDER);
                        item.SubItems.Add("<DIR>");
                        item.SubItems.Add(sub.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                        item.Tag = sub.FullName;
                        _listLocal.Items.Add(item);
                    }
                    catch { }
                }
                foreach (var file in dir.GetFiles().OrderBy(f => f.Name))
                {
                    try
                    {
                        var item = new ListViewItem(file.Name, IMG_FILE);
                        item.SubItems.Add(FormatSize(file.Length));
                        item.SubItems.Add(file.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                        item.Tag = file.FullName;
                        _listLocal.Items.Add(item);
                    }
                    catch { }
                }
            }
            catch (Exception ex) { SetStatus("Local browse error: " + ex.Message); }

            await Task.CompletedTask;
        }

        // ── Remote file system ─────────────────────────────────────────────────
        private async Task RefreshRemoteAsync()
        {
            SetStatus("Browsing host...");
            _listRemote.Items.Clear();

            var listing = await _engine.BrowseRemoteAsync(_remotePath ?? "");
            if (listing == null) { SetStatus("Cannot browse host file system."); return; }
            if (!string.IsNullOrEmpty(listing.Error)) { SetStatus("Host error: " + listing.Error); return; }

            _remotePath = listing.CurrentPath;
            _remoteParentPath = listing.ParentPath;
            _txtRemotePath.Text = _remotePath ?? "/";

            if (listing.Items != null)
            {
                foreach (var fi in listing.Items)
                {
                    int imgIdx = fi.IsDirectory ? IMG_FOLDER : IMG_FILE;
                    var item = new ListViewItem(fi.Name, imgIdx);
                    item.SubItems.Add(fi.IsDirectory ? "<DIR>" : FormatSize(fi.Size));
                    item.SubItems.Add(fi.Modified == DateTime.MinValue
                        ? "" : fi.Modified.ToString("yyyy-MM-dd HH:mm"));
                    item.Tag = fi;
                    _listRemote.Items.Add(item);
                }
            }
            SetStatus("Ready.");
        }

        // ── Path sanitisation ──────────────────────────────────────────────────
        private static string SanitizePath(string raw)
        {
            if (raw == null) return "";
            var sb = new System.Text.StringBuilder(raw.Length);
            foreach (char c in raw)
            {
                var cat = char.GetUnicodeCategory(c);
                if (cat == System.Globalization.UnicodeCategory.Control ||
                    cat == System.Globalization.UnicodeCategory.Format)
                    continue;
                sb.Append(c);
            }
            return sb.ToString().Trim();
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONFLICT RESOLUTION DIALOG
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Shows a custom conflict dialog with five buttons:
        ///   Overwrite | Overwrite All | Skip | Skip All | Cancel
        /// Must be called on the UI thread.
        /// </summary>
        private static ConflictChoice AskConflict(string fileName, bool isUpload)
        {
            string direction = isUpload ? "remote host" : "local folder";
            string message =
                $"\"{fileName}\" already exists in the {direction}.\n\n" +
                "What would you like to do?";

            // Dialog dimensions
            const int dlgWidth = 460;
            const int dlgHeight = 175;
            const int btnWidth = 130;
            const int btnHeight = 32;
            const int btnY = 100;
            const int gap = 10;
            // 3 buttons total: Overwrite | Overwrite All | Cancel Transfer
            // Total button block width = 3 * btnWidth + 2 * gap = 410
            const int totalBtns = 3 * btnWidth + 2 * gap;       // 410
            const int startX = (dlgWidth - totalBtns) / 2;   // 25

            using (var dlg = new Form())
            {
                dlg.Text = "File Already Exists";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ClientSize = new Size(dlgWidth, dlgHeight);
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Color.FromArgb(32, 32, 35);
                dlg.ForeColor = Color.FromArgb(220, 220, 220);
                dlg.Font = new Font("Segoe UI", 9f);

                var lbl = new Label
                {
                    Text = message,
                    Location = new Point(16, 16),
                    Size = new Size(dlgWidth - 32, 60),
                    ForeColor = Color.FromArgb(220, 220, 220),
                    Font = new Font("Segoe UI", 9f)
                };

                // Helper — creates a flat button at a given X on the button row
                Button MakeBtn(string text, int x, ConflictChoice choice, Color bg)
                {
                    var btn = new Button
                    {
                        Text = text,
                        Location = new Point(x, btnY),
                        Size = new Size(btnWidth, btnHeight),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = bg,
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand
                    };
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Click += (s, e) => { dlg.Tag = choice; dlg.DialogResult = DialogResult.OK; };
                    return btn;
                }

                int x1 = startX;
                int x2 = startX + btnWidth + gap;
                int x3 = startX + (btnWidth + gap) * 2;

                var btnOverwrite = MakeBtn("Overwrite", x1, ConflictChoice.Overwrite, Color.FromArgb(160, 90, 20));
                var btnOverwriteAll = MakeBtn("Overwrite All", x2, ConflictChoice.OverwriteAll, Color.FromArgb(180, 70, 20));
                var btnCancel = MakeBtn("Cancel Transfer", x3, ConflictChoice.Cancel, Color.FromArgb(160, 40, 40));

                dlg.Controls.AddRange(new System.Windows.Forms.Control[] { lbl, btnOverwrite, btnOverwriteAll, btnCancel });

                dlg.ShowDialog();

                return dlg.Tag is ConflictChoice c ? c : ConflictChoice.Cancel;
            }
        }

        // ── Upload (Viewer -> Host) ────────────────────────────────────────────
        private void OnUploadClick(object sender, EventArgs e)
        {
            if (_transferring) return;

            var selectedItems = _listLocal.SelectedItems.Cast<ListViewItem>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Select one or more files or folders in the Local (left) pane to upload.",
                    "Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var uploadPairs = new List<(string localFile, string remoteFolder)>();

            foreach (var lvItem in selectedItems)
            {
                if (lvItem.Tag is string path)
                {
                    if (File.Exists(path))
                        uploadPairs.Add((path, _remotePath));
                    else if (Directory.Exists(path))
                    {
                        string folderName = Path.GetFileName(
                            path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                        string baseRemote = CombineRemotePath(_remotePath, folderName);
                        CollectFolderFiles(path, baseRemote, uploadPairs);
                    }
                }
            }

            if (uploadPairs.Count == 0)
            {
                MessageBox.Show("No uploadable files found in the selection.",
                    "Upload", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            UploadPairs(uploadPairs);
        }

        private static void CollectFolderFiles(
            string localDir, string remoteBase,
            List<(string localFile, string remoteFolder)> result)
        {
            foreach (var file in Directory.GetFiles(localDir))
                result.Add((file, remoteBase));
            foreach (var sub in Directory.GetDirectories(localDir))
            {
                string subName = Path.GetFileName(sub);
                string remoteNext = CombineRemotePath(remoteBase, subName);
                CollectFolderFiles(sub, remoteNext, result);
            }
        }

        private static string CombineRemotePath(string basePath, string name)
        {
            if (string.IsNullOrWhiteSpace(basePath) || basePath == "/")
                return name;
            return basePath.TrimEnd('\\', '/') + '\\' + name;
        }

        /// <summary>
        /// Uploads a list of (localFile, remoteDestFolder) pairs.
        /// Before each file is sent, checks if the file already exists on the
        /// remote host and prompts the user when it does.
        /// </summary>
        private async void UploadPairs(List<(string localFile, string remoteFolder)> pairs)
        {
            _transferring = true;
            _batchInProgress = true;
            SetTransferButtons(false);

            // Persistent choice for "apply to all" scenarios.
            ConflictChoice? applyToAll = null;

            int skipped = 0;

            for (int i = 0; i < pairs.Count; i++)
            {
                var (localFile, remoteFolder) = pairs[i];
                string fileName = Path.GetFileName(localFile);

                // ── Check if the file already exists on the host ───────────────
                string remoteCandidatePath = CombineRemotePath(remoteFolder, fileName);
                bool remoteExists = await RemoteFileExistsAsync(remoteCandidatePath);

                if (remoteExists)
                {
                    ConflictChoice choice;

                    if (applyToAll.HasValue)
                    {
                        // Honour a previously chosen "all" decision.
                        choice = applyToAll.Value;
                    }
                    else
                    {
                        // Must ask on the UI thread.
                        choice = ConflictChoice.Cancel;
                        Invoke((Action)(() =>
                        {
                            choice = AskConflict(fileName, isUpload: true);
                        }));
                    }

                    switch (choice)
                    {
                        case ConflictChoice.OverwriteAll:
                            applyToAll = ConflictChoice.Overwrite;   // treat rest as Overwrite
                            // fall through to Overwrite
                            goto case ConflictChoice.Overwrite;

                        case ConflictChoice.Overwrite:
                            // Delete the remote file first so the host doesn't
                            // rename it to avoid collisions.
                            await _engine.DeleteRemoteFileAsync(remoteCandidatePath);
                            break;

                        case ConflictChoice.SkipAll:
                            applyToAll = ConflictChoice.Skip;        // treat rest as Skip
                            goto case ConflictChoice.Skip;

                        case ConflictChoice.Skip:
                            skipped++;
                            SetStatus($"Skipped {i + 1}/{pairs.Count}: {fileName}");
                            continue;   // move to next file

                        case ConflictChoice.Cancel:
                        default:
                            // Abort the whole batch.
                            _batchInProgress = false;
                            _transferring = false;
                            SetTransferButtons(true);
                            SetStatus("Upload cancelled by user.");
                            _ = RefreshRemoteAsync();
                            return;
                    }
                }

                SetStatus($"Uploading {i + 1}/{pairs.Count}: {fileName}…");
                _progressBar.Value = 0;
                _lblProgress.Text = $"File {i + 1} of {pairs.Count}";
                await _engine.UploadFileAsync(localFile, remoteFolder);
            }

            _batchInProgress = false;
            _transferring = false;
            SetTransferButtons(true);

            int done = pairs.Count - skipped;
            SetStatus(done == 1 && skipped == 0
                ? "Upload complete."
                : $"{done} file(s) uploaded, {skipped} skipped.");

            _ = RefreshRemoteAsync();
        }

        // ── Download (Host -> Viewer) ──────────────────────────────────────────
        private void OnDownloadClick(object sender, EventArgs e)
        {
            if (_transferring) return;

            var selected = _listRemote.SelectedItems.Cast<ListViewItem>().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show(
                    "Select one or more files or folders in the Remote (right) pane to download.",
                    "Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var filePaths = new List<string>();
            var folderItems = new List<FileItem>();

            foreach (var lvItem in selected)
            {
                if (lvItem.Tag is FileItem fi)
                {
                    if (fi.IsDirectory) folderItems.Add(fi);
                    else filePaths.Add(fi.FullPath);
                }
            }

            if (filePaths.Count == 0 && folderItems.Count == 0)
            {
                MessageBox.Show("Nothing selected to download.",
                    "Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DownloadMixed(filePaths, folderItems);
        }

        /// <summary>
        /// Downloads files and folder contents.
        /// Before each file is written, checks whether it already exists locally
        /// and prompts the user when it does.
        /// </summary>
        private async void DownloadMixed(List<string> files, List<FileItem> folders)
        {
            _transferring = true;
            _batchInProgress = true;
            SetTransferButtons(false);

            // Build a flat list of (remotePath, localDestFolder) for all files.
            var allPairs = new List<(string remotePath, string localDestFolder)>();

            foreach (string p in files)
                allPairs.Add((p, _localPath));

            foreach (var folder in folders)
            {
                SetStatus($"Collecting contents of folder: {folder.Name}…");
                await CollectRemoteFolderFiles(folder.FullPath, _localPath, folder.Name, allPairs);
            }

            ConflictChoice? applyToAll = null;
            int skipped = 0;

            for (int i = 0; i < allPairs.Count; i++)
            {
                var (remotePath, localDest) = allPairs[i];
                string fileName = Path.GetFileName(remotePath);
                string localTarget = Path.Combine(localDest, fileName);

                // ── Check if the file already exists locally ───────────────────
                if (File.Exists(localTarget))
                {
                    ConflictChoice choice;

                    if (applyToAll.HasValue)
                    {
                        choice = applyToAll.Value;
                    }
                    else
                    {
                        choice = ConflictChoice.Cancel;
                        Invoke((Action)(() =>
                        {
                            choice = AskConflict(fileName, isUpload: false);
                        }));
                    }

                    switch (choice)
                    {
                        case ConflictChoice.OverwriteAll:
                            applyToAll = ConflictChoice.Overwrite;
                            goto case ConflictChoice.Overwrite;

                        case ConflictChoice.Overwrite:
                            // Delete the local copy so DownloadFileAsync writes
                            // to the exact path without auto-renaming.
                            try { File.Delete(localTarget); } catch { }
                            break;

                        case ConflictChoice.SkipAll:
                            applyToAll = ConflictChoice.Skip;
                            goto case ConflictChoice.Skip;

                        case ConflictChoice.Skip:
                            skipped++;
                            SetStatus($"Skipped {i + 1}/{allPairs.Count}: {fileName}");
                            continue;

                        case ConflictChoice.Cancel:
                        default:
                            _batchInProgress = false;
                            _transferring = false;
                            SetTransferButtons(true);
                            SetStatus("Download cancelled by user.");
                            _ = RefreshLocalAsync();
                            return;
                    }
                }

                SetStatus($"Downloading {i + 1}/{allPairs.Count}: {fileName}…");
                _progressBar.Value = 0;
                _lblProgress.Text = $"File {i + 1} of {allPairs.Count}";
                Directory.CreateDirectory(localDest);
                await _engine.DownloadFileAsync(remotePath, localDest);
            }

            _batchInProgress = false;
            _transferring = false;
            SetTransferButtons(true);

            int done = allPairs.Count - skipped;
            SetStatus(done == 1 && skipped == 0
                ? "Download complete."
                : $"{done} file(s) downloaded, {skipped} skipped.");

            _ = RefreshLocalAsync();
        }

        // ── Remote folder recursion ────────────────────────────────────────────
        private async Task CollectRemoteFolderFiles(
            string remoteFolderPath,
            string localBase,
            string folderName,
            List<(string remotePath, string localDest)> result)
        {
            string localFolder = Path.Combine(localBase, folderName);

            var listing = await _engine.BrowseRemoteAsync(remoteFolderPath);
            if (listing == null || !string.IsNullOrEmpty(listing.Error) || listing.Items == null)
                return;

            foreach (var fi in listing.Items)
            {
                if (fi.IsDirectory)
                    await CollectRemoteFolderFiles(fi.FullPath, localFolder, fi.Name, result);
                else
                    result.Add((fi.FullPath, localFolder));
            }
        }

        // ── Remote existence helper ────────────────────────────────────────────
        /// <summary>
        /// Asks the host whether <paramref name="remotePath"/> exists as a file.
        /// Uses the existing /filebrowse endpoint to list the parent directory
        /// and checks if the file name appears in the result.
        /// Returns false on any error so transfers proceed normally.
        /// </summary>
        private async Task<bool> RemoteFileExistsAsync(string remotePath)
        {
            try
            {
                string parentDir = Path.GetDirectoryName(remotePath);
                string fileName = Path.GetFileName(remotePath);
                if (string.IsNullOrEmpty(fileName)) return false;

                var listing = await _engine.BrowseRemoteAsync(parentDir ?? "");
                if (listing == null || listing.Items == null) return false;

                return listing.Items.Any(fi =>
                    !fi.IsDirectory &&
                    string.Equals(fi.Name, fileName, StringComparison.OrdinalIgnoreCase));
            }
            catch { return false; }
        }

        // ── Navigation: Local ──────────────────────────────────────────────────
        private void OnLocalDoubleClick(object sender, EventArgs e)
        {
            if (_listLocal.SelectedItems.Count == 0) return;
            var item = _listLocal.SelectedItems[0];
            if (item.Tag is string path && Directory.Exists(path))
            {
                _localHistory.Push(_localPath);
                _localPath = path;
                _ = RefreshLocalAsync();
                UpdateLocalNavButtons();
            }
        }

        private void OnLocalUpClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_localPath)) return;
            _localHistory.Push(_localPath);
            var parent = Directory.GetParent(_localPath);
            _localPath = parent?.FullName ?? "";
            _ = RefreshLocalAsync();
            UpdateLocalNavButtons();
        }

        private void OnLocalBackClick(object sender, EventArgs e)
        {
            if (_localHistory.Count == 0) return;
            _localPath = _localHistory.Pop();
            _ = RefreshLocalAsync();
            UpdateLocalNavButtons();
        }

        private void OnLocalPathEnter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            string typed = SanitizePath(((TextBox)sender).Text);
            _txtLocalPath.Text = typed;

            if (Directory.Exists(typed))
            {
                _localHistory.Push(_localPath);
                _localPath = typed;
                _ = RefreshLocalAsync();
                UpdateLocalNavButtons();
            }
            else
            {
                MessageBox.Show("Path not found:\n" + typed,
                    "Navigate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtLocalPath.Text = _localPath ?? "";
            }
        }

        private void UpdateLocalNavButtons() =>
            _btnLocalBack.Enabled = _localHistory.Count > 0;

        // ── Navigation: Remote ─────────────────────────────────────────────────
        private void OnRemoteDoubleClick(object sender, EventArgs e)
        {
            if (_listRemote.SelectedItems.Count == 0) return;
            var item = _listRemote.SelectedItems[0];
            if (item.Tag is FileItem fi && fi.IsDirectory)
            {
                _remoteHistory.Push(_remotePath);
                _remotePath = fi.FullPath;
                _ = RefreshRemoteAsync();
                UpdateRemoteNavButtons();
            }
        }

        private void OnRemoteUpClick(object sender, EventArgs e)
        {
            _remoteHistory.Push(_remotePath);
            _remotePath = _remoteParentPath;
            _ = RefreshRemoteAsync();
            UpdateRemoteNavButtons();
        }

        private void OnRemoteBackClick(object sender, EventArgs e)
        {
            if (_remoteHistory.Count == 0) return;
            _remotePath = _remoteHistory.Pop();
            _ = RefreshRemoteAsync();
            UpdateRemoteNavButtons();
        }

        private void OnRemotePathEnter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            string typed = SanitizePath(((TextBox)sender).Text);
            _txtRemotePath.Text = typed;

            _remoteHistory.Push(_remotePath);
            _remotePath = string.IsNullOrWhiteSpace(typed) ? null : typed;
            _ = RefreshRemoteAsync();
            UpdateRemoteNavButtons();
        }

        private void UpdateRemoteNavButtons() =>
            _btnRemoteBack.Enabled = _remoteHistory.Count > 0;

        // ── Helpers ────────────────────────────────────────────────────────────
        private void SetStatus(string msg)
        {
            if (InvokeRequired) { Invoke((Action<string>)SetStatus, msg); return; }
            _lblStatus.Text = msg;
        }

        private void SetTransferButtons(bool enabled)
        {
            _btnUpload.Enabled = enabled;
            _btnDownload.Enabled = enabled;
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.#") + " KB";
            if (bytes < 1024L * 1024 * 1024) return (bytes / (1024.0 * 1024)).ToString("0.##") + " MB";
            return (bytes / (1024.0 * 1024 * 1024)).ToString("0.##") + " GB";
        }

        // ── Image list ─────────────────────────────────────────────────────────
        private void BuildImageList()
        {
            _imageList.Images.Add(DrawFolder());  // IMG_FOLDER = 0
            _imageList.Images.Add(DrawFile());    // IMG_FILE   = 1
            _imageList.Images.Add(DrawDrive());   // IMG_DRIVE  = 2
        }

        private static Bitmap DrawFolder()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (var b = new SolidBrush(Color.FromArgb(255, 200, 100)))
                    g.FillRectangle(b, 0, 4, 16, 11);
                using (var b = new SolidBrush(Color.FromArgb(255, 180, 60)))
                    g.FillRectangle(b, 0, 4, 7, 3);
                using (var b = new SolidBrush(Color.FromArgb(200, 140, 40)))
                {
                    g.FillRectangle(b, 0, 4, 16, 1);
                    g.FillRectangle(b, 0, 14, 16, 1);
                }
            }
            return bmp;
        }

        private static Bitmap DrawFile()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (var b = new SolidBrush(Color.FromArgb(220, 220, 230)))
                    g.FillRectangle(b, 2, 0, 11, 16);
                using (var b = new SolidBrush(Color.FromArgb(180, 180, 190)))
                {
                    g.FillRectangle(b, 13, 0, 1, 3);
                    g.FillRectangle(b, 10, 0, 3, 1);
                    g.FillRectangle(b, 10, 3, 3, 1);
                }
            }
            return bmp;
        }

        private static Bitmap DrawDrive()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (var b = new SolidBrush(Color.FromArgb(120, 160, 220)))
                    g.FillRectangle(b, 0, 4, 16, 10);
                using (var b = new SolidBrush(Color.FromArgb(80, 120, 180)))
                    g.FillRectangle(b, 0, 4, 16, 2);
                using (var b = new SolidBrush(Color.White))
                    g.FillEllipse(b, 2, 9, 3, 3);
            }
            return bmp;
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            _ = RefreshLocalAsync();
            _ = RefreshRemoteAsync();
        }
    }
}