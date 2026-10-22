namespace BNet.RemoteDesktop.Models
{
    /// <summary>
    /// A single input event sent from the Viewer to the Host over HTTP.
    /// Type values: mousemove | mousedown | mouseup | wheel | keydown | keyup
    /// </summary>
    public class RemoteInputEvent
    {
        public string Type { get; set; }

        // ── Mouse ──────────────────────────────────────────────────────────────
        /// <summary>X in JPEG frame pixels (remapped to physical on host).</summary>
        public int X { get; set; }
        /// <summary>Y in JPEG frame pixels (remapped to physical on host).</summary>
        public int Y { get; set; }
        /// <summary>0 = Left  1 = Middle  2 = Right</summary>
        public int Button { get; set; }
        /// <summary>Wheel scroll delta (positive = down).</summary>
        public int Delta { get; set; }

        // ── Screen / frame context ─────────────────────────────────────────────
        /// <summary>Which screen index on the host this event targets (0 = primary).</summary>
        public int ScreenIndex { get; set; }
        /// <summary>Width of the JPEG frame the viewer received.</summary>
        public int FrameW { get; set; }
        /// <summary>Height of the JPEG frame the viewer received.</summary>
        public int FrameH { get; set; }

        // ── Keyboard ───────────────────────────────────────────────────────────
        /// <summary>Win32 Virtual Key code.</summary>
        public int VirtualKey { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        /// <summary>True for extended keys (arrows, ins, del, etc).</summary>
        public bool Extended { get; set; }
    }
}