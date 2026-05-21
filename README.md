# QuickSnap

Lightweight screenshot utility for Windows. Press `Ctrl+PrintScreen`, drag to select a region, and it auto-saves to a configured UNC share or local folder with a `prefix_YYYYMMDD_HHMMSS.png` filename — no clipboard paste required.

## Download

Grab the latest `QuickSnap.exe` and `settings.json` from [Releases](../../releases).

## Setup

1. Place `QuickSnap.exe` and `settings.json` in the same folder.
2. Edit `settings.json` with your share path and preferences (or use the Settings UI after launch).
3. Run `QuickSnap.exe` — it lives in the system tray.
4. Optional: enable **Run on Startup** in Settings so it starts with Windows.

## settings.json

| Key | Description | Default |
|-----|-------------|---------|
| `SavePath` | Primary destination (UNC or local path) | `\\192.168.1.1\ITSupport\Imports` |
| `LocalBackupPath` | Optional second save location | *(empty)* |
| `UsernamePrefix` | Prepended to filename, e.g. `brad` → `brad_20260521_143022.png` | *(empty)* |
| `CopyToClipboard` | Also copy to clipboard after capture | `true` |
| `ShowNotification` | Tray balloon on successful save | `true` |
| `ImageFormat` | `"png"` or `"jpg"` | `"png"` |
| `JpgQuality` | JPEG quality 50–100 (only used when format is jpg) | `90` |
| `HotkeyModifier` | `Ctrl`, `Alt`, `Shift`, or `Win` | `"Ctrl"` |
| `HotkeyKey` | `PrintScreen`, `F1`–`F12` | `"PrintScreen"` |
| `RunOnStartup` | Add to Windows startup | `false` |

## Tray menu

- **Capture Region** — draw a selection box (same as hotkey)
- **Capture Full Screen** — saves entire virtual desktop
- **Settings…** — opens the settings window
- **Exit**

## Hotkey conflict

If `Ctrl+PrintScreen` is already claimed by another app, change `HotkeyModifier`/`HotkeyKey` in `settings.json` or via Settings → Capture Hotkey.
