using System;

namespace BNet.RemoteDesktop.Models
{
    /// <summary>
    /// One saved remote-desk session (IP + port + friendly info).
    /// Persisted to %AppData%\RemoteDesk\sessions.json
    /// </summary>
    public class SessionEntry
    {
        public string HostIp { get; set; }
        public int Port { get; set; }
        public string MachineName { get; set; }   // filled after first successful connect
        public DateTime LastConnected { get; set; }

        /// <summary>Label shown in the saved-sessions list.</summary>
        public string DisplayLabel =>
            $"{HostIp}:{Port}" +
            (string.IsNullOrEmpty(MachineName) ? "" : $"  —  {MachineName}") +
            $"\n{LastConnected:yyyy-MM-dd  HH:mm}";
    }
}