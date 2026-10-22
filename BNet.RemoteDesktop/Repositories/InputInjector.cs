using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BNet.RemoteDesktop.Models;

namespace BNet.RemoteDesktop.Repositories
{
    /// <summary>
    /// Translates RemoteInputEvent objects into real Win32 SendInput calls
    /// on the machine being controlled (the Host side).
    /// Mouse coordinates are already physical virtual-desktop pixels
    /// (remapped by PeerHttpEngine.RemapToPhysical).
    /// Keyboard events are injected EXACTLY as received: the viewer sends modifier
    /// keys (Ctrl/Alt/Shift/Win) as their own keydown/keyup events, so the host
    /// must not add or remove modifiers itself.
    /// </summary>
    public static class InputInjector
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx, dy;
            public uint mouseData, dwFlags, time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk, wScan;
            public uint dwFlags, time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT { public uint uMsg; public ushort wParamL, wParamH; }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT { public uint type; public INPUTUNION u; }

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;

        private const int ABSOLUTE_MAX = 65535;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint n, INPUT[] inputs, int size);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int idx);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        private const uint MAPVK_VK_TO_VSC = 0;

        // Keys that Windows treats as "extended" (arrows, nav cluster, right-side
        // modifiers, Win/Apps, numpad / and NumLock, PrintScreen). Without the flag
        // an arrow key is delivered as a NUMPAD arrow, which types digits when
        // NumLock is on.
        private static readonly HashSet<int> ExtendedVks = new HashSet<int>
        {
            0x21, 0x22, 0x23, 0x24,        // PgUp PgDn End Home
            0x25, 0x26, 0x27, 0x28,        // Left Up Right Down
            0x2C, 0x2D, 0x2E,              // PrintScreen Insert Delete
            0x5B, 0x5C, 0x5D,              // LWin RWin Apps
            0x6F, 0x90,                    // Numpad Divide, NumLock
            0xA3, 0xA5                     // RCtrl RAlt
        };

        public static void Dispatch(RemoteInputEvent e)
        {
            if (e == null) return;
            switch (e.Type?.ToLower())
            {
                case "mousemove": DoMouseMove(e.X, e.Y); break;
                case "mousedown": DoMouseButton(e.X, e.Y, e.Button, true); break;
                case "mouseup": DoMouseButton(e.X, e.Y, e.Button, false); break;
                case "wheel": DoWheel(e.Delta); break;
                case "keydown": DoKey(e, true); break;
                case "keyup": DoKey(e, false); break;
            }
        }

        // ── Mouse ──────────────────────────────────────────────────────────────

        private static void DoMouseMove(int x, int y)
        {
            ToAbsolute(x, y, out int absX, out int absY);
            Send(MouseInput(absX, absY,
                MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK));
        }

        private static void DoMouseButton(int x, int y, int button, bool down)
        {
            DoMouseMove(x, y);

            uint flag;
            switch (button)
            {
                case 2: flag = down ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP; break;
                case 1: flag = down ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP; break;
                default: flag = down ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP; break;
            }

            ToAbsolute(x, y, out int absX, out int absY);
            Send(MouseInput(absX, absY,
                flag | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK));
        }

        private static void DoWheel(int delta)
        {
            int winDelta = (int)(Math.Sign(delta) * 120
                         * Math.Max(1, Math.Abs(delta) / 100));

            var inp = new INPUT
            {
                type = 0,
                u = new INPUTUNION
                {
                    mi = new MOUSEINPUT
                    {
                        mouseData = (uint)winDelta,
                        dwFlags = MOUSEEVENTF_WHEEL
                    }
                }
            };
            SendInput(1, new[] { inp }, Marshal.SizeOf(typeof(INPUT)));
        }

        // ── Keyboard ───────────────────────────────────────────────────────────

        private static void DoKey(RemoteInputEvent e, bool down)
        {
            if (e.VirtualKey <= 0 || e.VirtualKey > 0xFE) return;

            bool extended = e.Extended || ExtendedVks.Contains(e.VirtualKey);
            Send(KeyInput((ushort)e.VirtualKey, !down, extended));
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void ToAbsolute(int x, int y, out int absX, out int absY)
        {
            int vx = GetSystemMetrics(SM_XVIRTUALSCREEN);
            int vy = GetSystemMetrics(SM_YVIRTUALSCREEN);
            int vw = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            int vh = GetSystemMetrics(SM_CYVIRTUALSCREEN);

            absX = (int)((double)(x - vx) / vw * ABSOLUTE_MAX);
            absY = (int)((double)(y - vy) / vh * ABSOLUTE_MAX);
        }

        private static INPUT MouseInput(int absX, int absY, uint flags) => new INPUT
        {
            type = 0,
            u = new INPUTUNION
            {
                mi = new MOUSEINPUT { dx = absX, dy = absY, dwFlags = flags }
            }
        };

        private static INPUT KeyInput(ushort vk, bool keyUp, bool extended) => new INPUT
        {
            type = 1,
            u = new INPUTUNION
            {
                ki = new KEYBDINPUT
                {
                    wVk = vk,
                    wScan = (ushort)MapVirtualKey(vk, MAPVK_VK_TO_VSC),
                    dwFlags = (keyUp ? KEYEVENTF_KEYUP : 0u)
                            | (extended ? KEYEVENTF_EXTENDEDKEY : 0u)
                }
            }
        };

        private static void Send(INPUT inp) =>
            SendInput(1, new[] { inp }, Marshal.SizeOf(typeof(INPUT)));
    }
}