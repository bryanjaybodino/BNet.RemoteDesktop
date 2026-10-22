namespace BNet.RemoteDesktop.Models
{
    /// <summary>
    /// Carries clipboard text content between peers over HTTP.
    /// Direction: Viewer → Host  (POST /clipboard)
    ///            Host  → Viewer (GET  /clipboard)
    /// </summary>
    public class ClipboardPayload
    {
        /// <summary>"text"</summary>
        public string Kind { get; set; }

        /// <summary>Plain-text content.</summary>
        public string Text { get; set; }
    }
}