using System.Runtime.InteropServices;

namespace QuickSnap;

public class HotkeyManager : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 9001;

    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public static readonly string[] SupportedModifiers = ["Ctrl", "Alt", "Shift", "Win"];
    public static readonly string[] SupportedKeys =
        ["PrintScreen", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"];

    private static readonly Dictionary<string, uint> ModifierMap = new()
    {
        ["Alt"]   = 0x0001,
        ["Ctrl"]  = 0x0002,
        ["Shift"] = 0x0004,
        ["Win"]   = 0x0008,
    };

    private static readonly Dictionary<string, uint> KeyMap = new()
    {
        ["PrintScreen"] = 0x2C,
        ["F1"]  = 0x70, ["F2"]  = 0x71, ["F3"]  = 0x72, ["F4"]  = 0x73,
        ["F5"]  = 0x74, ["F6"]  = 0x75, ["F7"]  = 0x76, ["F8"]  = 0x77,
        ["F9"]  = 0x78, ["F10"] = 0x79, ["F11"] = 0x7A, ["F12"] = 0x7B,
    };

    public event EventHandler? HotkeyPressed;
    private bool _registered;

    public HotkeyManager() => CreateHandle(new CreateParams());

    public bool Register(string modifier, string key)
    {
        Unregister();
        if (!ModifierMap.TryGetValue(modifier, out uint mod) || !KeyMap.TryGetValue(key, out uint vk))
            return false;
        _registered = RegisterHotKey(Handle, HOTKEY_ID, mod, vk);
        return _registered;
    }

    public void Unregister()
    {
        if (_registered)
        {
            UnregisterHotKey(Handle, HOTKEY_ID);
            _registered = false;
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        Unregister();
        DestroyHandle();
        GC.SuppressFinalize(this);
    }
}
