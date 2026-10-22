using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BNet.RemoteDesktop
{
    /// <summary>
    /// WH_KEYBOARD_LL hook. While the viewer is focused, EVERY key (arrows, Alt+Tab,
    /// Win+*, Ctrl+Shift+Esc, F-keys, numpad, etc.) is handed to KeyIntercepted and,
    /// if a subscriber returns true, swallowed locally so only the host receives it.
    ///
    /// Must be installed from the UI thread (needs a message loop).
    /// </summary>
    public sealed class LowLevelKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const uint LLKHF_EXTENDED = 0x01;
        private const uint LLKHF_INJECTED = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
                                                       IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private IntPtr _hookHandle = IntPtr.Zero;
        private LowLevelKeyboardProc _proc; // keep-alive so the delegate is not GC'd

        // Modifier state. A low-level hook reports the SIDED codes (0xA0-0xA5),
        // never the generic VK_SHIFT/VK_CONTROL/VK_MENU.
        private bool _lCtrl, _rCtrl, _lAlt, _rAlt, _lShift, _rShift;

        private bool _installed;
        private bool _disposed;

        public bool IsInstalled => _installed;

        /// <summary>
        /// Return true while the viewer wants to capture the keyboard
        /// (viewer form active, screen area focused). Called on the UI thread.
        /// Key-UP events are always offered to subscribers so held keys can be released.
        /// </summary>
        public Func<bool> ShouldCapture { get; set; }

        /// <summary>
        /// (vk, extended, down, ctrl, alt, shift) → return true to suppress locally.
        /// </summary>
        public event Func<ushort, bool, bool, bool, bool, bool, bool> KeyIntercepted;

        public void Install()
        {
            if (_installed) return;
            _proc = HookCallback;
            using (var proc = Process.GetCurrentProcess())
            using (var mod = proc.MainModule)
                _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
                                               GetModuleHandle(mod.ModuleName), 0);
            _installed = _hookHandle != IntPtr.Zero;
        }

        public void Uninstall()
        {
            if (!_installed) return;
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
            _installed = false;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                try
                {
                    var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                    // Ignore keys we (or any program) injected - avoids feedback loops
                    // when this PC is also acting as a host.
                    if ((kb.flags & LLKHF_INJECTED) == 0)
                    {
                        int msg = wParam.ToInt32();
                        bool isDown = (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN);
                        bool isUp = (msg == WM_KEYUP || msg == WM_SYSKEYUP);

                        if (isDown || isUp)
                        {
                            bool extended = (kb.flags & LLKHF_EXTENDED) != 0;
                            ushort vk = (ushort)kb.vkCode;

                            switch (vk)
                            {
                                case 0xA2: _lCtrl = isDown; break;
                                case 0xA3: _rCtrl = isDown; break;
                                case 0xA4: _lAlt = isDown; break;
                                case 0xA5: _rAlt = isDown; break;
                                case 0xA0: _lShift = isDown; break;
                                case 0xA1: _rShift = isDown; break;
                            }

                            var handler = KeyIntercepted;
                            if (handler != null && (isUp || (ShouldCapture?.Invoke() ?? false)))
                            {
                                bool ctrl = _lCtrl || _rCtrl;
                                bool alt = _lAlt || _rAlt;
                                bool shift = _lShift || _rShift;

                                bool suppress = false;
                                foreach (Func<ushort, bool, bool, bool, bool, bool, bool> d
                                         in handler.GetInvocationList())
                                    suppress |= d(vk, extended, isDown, ctrl, alt, shift);

                                if (suppress)
                                    return (IntPtr)1; // swallow
                            }
                        }
                    }
                }
                catch { /* never let an exception escape a hook proc */ }
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Uninstall();
            _disposed = true;
        }
    }
}