using System;

namespace BNet.RemoteDesktop.Models
{
    /// <summary>
    /// Carries file-transfer metadata and (optionally) chunk data over HTTP.
    ///
    /// Upload flow  (Viewer → Host):
    ///   POST /filetransfer/upload/begin   → { FileName, TotalSize, TotalChunks }
    ///   POST /filetransfer/upload/chunk   → { TransferId, ChunkIndex, TotalChunks, Data (base64) }
    ///   POST /filetransfer/upload/cancel  → { TransferId }
    ///
    /// Download flow (Viewer requests a file from Host):
    ///   POST /filetransfer/download/begin → { RemotePath }
    ///   GET  /filetransfer/download/chunk → ?id=&chunk=  returns FileTransferChunk JSON
    ///   POST /filetransfer/download/cancel→ { TransferId }
    ///
    /// Browse flow   (Viewer browses Host file system):
    ///   GET  /filebrowse?path=<encoded>
    ///   Returns FileListingResponse JSON.
    /// </summary>

    // ── Upload init ────────────────────────────────────────────────────────────
    public class FileUploadBeginRequest
    {
        public string FileName { get; set; }
        public long TotalSize { get; set; }
        public int TotalChunks { get; set; }
        /// <summary>Destination directory on the host (empty = Downloads).</summary>
        public string DestFolder { get; set; }
    }

    public class FileUploadBeginResponse
    {
        public string TransferId { get; set; }   // guid
        public bool Accepted { get; set; }
        public string Error { get; set; }
    }

    // ── Upload chunk ───────────────────────────────────────────────────────────
    public class FileUploadChunkRequest
    {
        public string TransferId { get; set; }
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        /// <summary>Raw bytes of this chunk, base64-encoded.</summary>
        public string Data { get; set; }
    }

    public class FileUploadChunkResponse
    {
        public bool Ok { get; set; }
        public bool Complete { get; set; }   // true on the last chunk
        public string SavedPath { get; set; }   // filled when Complete
        public string Error { get; set; }
    }

    // ── Download init ──────────────────────────────────────────────────────────
    public class FileDownloadBeginRequest
    {
        public string RemotePath { get; set; }
    }

    public class FileDownloadBeginResponse
    {
        public string TransferId { get; set; }
        public string FileName { get; set; }
        public long TotalSize { get; set; }
        public int TotalChunks { get; set; }
        public bool Accepted { get; set; }
        public string Error { get; set; }
    }

    // ── Download chunk ─────────────────────────────────────────────────────────
    public class FileDownloadChunkResponse
    {
        public string TransferId { get; set; }
        public int ChunkIndex { get; set; }
        public string Data { get; set; }   // base64
        public bool IsLast { get; set; }
        public string Error { get; set; }
    }

    // ── Cancel ────────────────────────────────────────────────────────────────
    public class FileTransferCancelRequest
    {
        public string TransferId { get; set; }
    }

    // ── File-system browsing ───────────────────────────────────────────────────
    public class FileListingRequest
    {
        public string Path { get; set; }
    }

    public class FileListingResponse
    {
        public string CurrentPath { get; set; }
        public string ParentPath { get; set; }
        public FileItem[] Items { get; set; }
        public string Error { get; set; }
    }

    public class FileItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public string Extension { get; set; }
    }
}