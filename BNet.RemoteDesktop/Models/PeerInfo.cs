using System;

namespace BNet.RemoteDesktop.Models
{
    /// <summary>
    /// Sent during the /handshake POST so both sides learn about each other.
    /// </summary>
    public class PeerInfo
    {
        public string MachineName { get; set; }
        public string AccountName { get; set; }
        public string OSVersion { get; set; }
        public string Architecture { get; set; }
        /// <summary>The port number THIS peer's HTTP listener is bound to.</summary>
        public int ListenerPort { get; set; }
    }
}