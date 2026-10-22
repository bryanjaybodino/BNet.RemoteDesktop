using System;
using System.Collections.Concurrent;
using System.IO;

namespace BNet.RemoteDesktop.Repositories
{
    /// <summary>
    /// Tracks in-progress upload/download sessions on the HOST side.
    /// Thread-safe; all operations are lock-free via ConcurrentDictionary.
    /// </summary>
    public static class FileTransferStore
    {
        // ── Chunk size shared by both sides ────────────────────────────────────
        /// <summary>256 KB per chunk keeps individual HTTP bodies small.</summary>
        public const int ChunkSize = 256 * 1024;

        // ── Upload sessions ────────────────────────────────────────────────────

        private sealed class UploadSession
        {
            public string TempPath { get; set; }
            public string DestPath { get; set; }
            public int TotalChunks { get; set; }
            public int NextChunk { get; set; }
            public FileStream Stream { get; set; }
            public DateTime Created { get; set; } = DateTime.UtcNow;
        }

        private static readonly ConcurrentDictionary<string, UploadSession> _uploads
            = new ConcurrentDictionary<string, UploadSession>(StringComparer.Ordinal);

        /// <summary>
        /// Create an upload session. Returns the new transfer-id.
        /// Destination folder defaults to %USERPROFILE%\Downloads.
        /// </summary>
        public static string BeginUpload(string fileName, int totalChunks, string destFolder)
        {
            string safeFolder = string.IsNullOrWhiteSpace(destFolder)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
                : destFolder;

            Directory.CreateDirectory(safeFolder);

            string destPath = Path.Combine(safeFolder, fileName);   // exact path — viewer handles conflict
            string tempPath = destPath + ".rdtmp";

            var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write,
                                    FileShare.None, 65536, FileOptions.Asynchronous);

            string id = Guid.NewGuid().ToString("N");
            _uploads[id] = new UploadSession
            {
                TempPath = tempPath,
                DestPath = destPath,
                TotalChunks = totalChunks,
                NextChunk = 0,
                Stream = fs
            };
            return id;
        }

        /// <summary>
        /// Append a chunk.  Returns (ok, complete, savedPath, error).
        /// </summary>
        public static (bool ok, bool complete, string savedPath, string error)
            WriteChunk(string id, int chunkIndex, byte[] data)
        {
            if (!_uploads.TryGetValue(id, out var s))
                return (false, false, null, "Unknown transfer id");

            if (chunkIndex != s.NextChunk)
                return (false, false, null, $"Expected chunk {s.NextChunk}, got {chunkIndex}");

            try
            {
                s.Stream.Write(data, 0, data.Length);
                s.NextChunk++;
                bool done = (s.NextChunk >= s.TotalChunks);
                if (done)
                {
                    s.Stream.Flush();
                    s.Stream.Dispose();
                    File.Move(s.TempPath, s.DestPath);
                    _uploads.TryRemove(id, out _);
                    return (true, true, s.DestPath, null);
                }
                return (true, false, null, null);
            }
            catch (Exception ex)
            {
                AbortUpload(id);
                return (false, false, null, ex.Message);
            }
        }

        public static void AbortUpload(string id)
        {
            if (_uploads.TryRemove(id, out var s))
            {
                try { s.Stream?.Dispose(); } catch { }
                try { if (File.Exists(s.TempPath)) File.Delete(s.TempPath); } catch { }
            }
        }

        // ── Download sessions ──────────────────────────────────────────────────

        private sealed class DownloadSession
        {
            public string FilePath { get; set; }
            public long TotalSize { get; set; }
            public int TotalChunks { get; set; }
            public DateTime Created { get; set; } = DateTime.UtcNow;
        }

        private static readonly ConcurrentDictionary<string, DownloadSession> _downloads
            = new ConcurrentDictionary<string, DownloadSession>(StringComparer.Ordinal);

        /// <summary>
        /// Register a download session for an existing file on the host.
        /// Returns the transfer-id, total size, and total chunks.
        /// </summary>
        public static (string id, long size, int chunks, string error) BeginDownload(string filePath)
        {
            if (!File.Exists(filePath))
                return (null, 0, 0, "File not found: " + filePath);

            try
            {
                long size = new FileInfo(filePath).Length;
                int chunks = (int)Math.Ceiling((double)size / ChunkSize);
                if (chunks == 0) chunks = 1;

                string id = Guid.NewGuid().ToString("N");
                _downloads[id] = new DownloadSession
                {
                    FilePath = filePath,
                    TotalSize = size,
                    TotalChunks = chunks
                };
                return (id, size, chunks, null);
            }
            catch (Exception ex)
            {
                return (null, 0, 0, ex.Message);
            }
        }

        /// <summary>
        /// Read one chunk from the registered download session.
        /// Returns the base64-encoded chunk data.
        /// </summary>
        public static (string data, bool isLast, string error) ReadChunk(string id, int chunkIndex)
        {
            if (!_downloads.TryGetValue(id, out var s))
                return (null, false, "Unknown download id");

            try
            {
                long offset = (long)chunkIndex * ChunkSize;
                using (var fs = new FileStream(s.FilePath, FileMode.Open,
                                               FileAccess.Read, FileShare.Read,
                                               65536, FileOptions.Asynchronous))
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    long remaining = fs.Length - offset;
                    int readLen = (int)Math.Min(ChunkSize, remaining);
                    byte[] buf = new byte[readLen];
                    int got = fs.Read(buf, 0, readLen);
                    if (got < readLen)
                        Array.Resize(ref buf, got);

                    bool isLast = (chunkIndex + 1 >= s.TotalChunks);
                    if (isLast) _downloads.TryRemove(id, out _);

                    return (Convert.ToBase64String(buf), isLast, null);
                }
            }
            catch (Exception ex)
            {
                _downloads.TryRemove(id, out _);
                return (null, false, ex.Message);
            }
        }

        public static void AbortDownload(string id) => _downloads.TryRemove(id, out _);

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a path that does not collide with existing files.
        /// E.g. "report.pdf" → "report (2).pdf" if needed.
        /// </summary>
        private static string UniqueFilePath(string folder, string fileName)
        {
            string candidate = Path.Combine(folder, fileName);
            if (!File.Exists(candidate)) return candidate;

            string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            int n = 2;
            do
            {
                candidate = Path.Combine(folder, $"{nameNoExt} ({n}){ext}");
                n++;
            }
            while (File.Exists(candidate));

            return candidate;
        }

        // ── Host file-system browsing ──────────────────────────────────────────

        /// <summary>
        /// Returns a listing for the given directory path.
        /// Empty/null path → drives (on Windows) or home directory.
        /// </summary>
        public static Models.FileListingResponse Browse(string path)
        {
            var resp = new Models.FileListingResponse();
            try
            {
                // Root-level: list logical drives
                if (string.IsNullOrWhiteSpace(path) || path == "/")
                {
                    var drives = DriveInfo.GetDrives();
                    resp.CurrentPath = "/";
                    resp.ParentPath = null;
                    var items = new System.Collections.Generic.List<Models.FileItem>();
                    foreach (var d in drives)
                    {
                        try
                        {
                            items.Add(new Models.FileItem
                            {
                                Name = d.Name,
                                FullPath = d.RootDirectory.FullName,
                                IsDirectory = true,
                                Size = 0,
                                Modified = DateTime.MinValue
                            });
                        }
                        catch { /* inaccessible drive */ }
                    }
                    resp.Items = items.ToArray();
                    return resp;
                }

                var dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    resp.Error = "Directory not found: " + path;
                    return resp;
                }

                resp.CurrentPath = dir.FullName;
                resp.ParentPath = dir.Parent?.FullName;

                var list = new System.Collections.Generic.List<Models.FileItem>();

                foreach (var sub in dir.GetDirectories())
                {
                    try
                    {
                        list.Add(new Models.FileItem
                        {
                            Name = sub.Name,
                            FullPath = sub.FullName,
                            IsDirectory = true,
                            Size = 0,
                            Modified = sub.LastWriteTime,
                            Extension = ""
                        });
                    }
                    catch { }
                }

                foreach (var file in dir.GetFiles())
                {
                    try
                    {
                        list.Add(new Models.FileItem
                        {
                            Name = file.Name,
                            FullPath = file.FullName,
                            IsDirectory = false,
                            Size = file.Length,
                            Modified = file.LastWriteTime,
                            Extension = file.Extension.ToLowerInvariant()
                        });
                    }
                    catch { }
                }

                resp.Items = list.ToArray();
            }
            catch (Exception ex)
            {
                resp.Error = ex.Message;
            }
            return resp;
        }
    }
}