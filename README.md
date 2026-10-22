# RemoteDesk

A simple remote desktop app for Windows, similar to AnyDesk and TeamViewer, but built for **local network (LAN) use only**.

No account, no cloud, no server. One PC hosts, another PC connects directly to it.

## Features

- View and control another PC's screen (multi-monitor supported)
- Full keyboard and mouse control, including arrow keys and shortcuts like `Alt+Tab`, `Alt+F4` and `Win` key combinations
- Shared clipboard for text, files and folders (copy on one PC, paste on the other)
- Drag and drop files onto the remote screen
- Browse the remote PC's files and transfer them
- Remembers your recent connections
- No extra DLLs or config files needed

## Requirements

- Windows 10 or 11 on both PCs
- .NET Framework 4.7 or later
- Both PCs on the same local network

## How to use

**On the PC you want to control (Host)**
1. Open the app and pick a port (default `7891`).
2. Click **Start Host**.
3. Note the IP address shown.

**On your PC (Viewer)**
1. Open the app and enter the host's IP address and port.
2. Connect, then click inside the remote screen to start controlling it.

While the remote screen is focused, all keys (including `Alt+Tab`) go to the remote PC. To close the viewer, use **Disconnect** or the window's close button.

## Limitations

- `Ctrl+Alt+Del` and `Win+L` can't be sent (blocked by Windows).
- Admin-level windows on the host can't be controlled unless the app also runs as administrator.
- Windows only.

## Security note

RemoteDesk is made for **trusted local networks only**. It has no password and no encryption, so anyone on the network who knows the host's IP and port can connect. Do not expose it to the internet.
