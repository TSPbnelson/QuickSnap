using System.Drawing.Imaging;

namespace QuickSnap;

public class TrayApplicationContext : ApplicationContext
{
    private NotifyIcon _trayIcon = null!;
    private HotkeyManager _hotkey = null!;
    private AppSettings _settings;
    private readonly Icon _trayIconImage = ClipboardIcon.Create();

    public TrayApplicationContext()
    {
        _settings = AppSettings.Load();
        InitTray();
        InitHotkey();
    }

    // ── Tray ──────────────────────────────────────────────────

    private void InitTray()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Capture Region\tCtrl+PrtSc", null, (_, _) => StartRegionCapture());
        menu.Items.Add("Capture Full Screen", null, (_, _) => CaptureFullScreen());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Settings…", null, (_, _) => OpenSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());

        _trayIcon = new NotifyIcon
        {
            Text = "QuickSnap",
            Icon = _trayIconImage,
            Visible = true,
            ContextMenuStrip = menu
        };

        _trayIcon.DoubleClick += (_, _) => StartRegionCapture();
    }

    private void InitHotkey()
    {
        _hotkey = new HotkeyManager();
        _hotkey.HotkeyPressed += (_, _) => StartRegionCapture();

        if (!_hotkey.Register(_settings.HotkeyModifier, _settings.HotkeyKey))
        {
            Notify("Hotkey conflict",
                $"{_settings.HotkeyModifier}+{_settings.HotkeyKey} is already in use by another app. Change it in Settings.");
        }
    }

    // ── Capture ───────────────────────────────────────────────

    private void StartRegionCapture()
    {
        var activeScreen = Screen.FromPoint(Cursor.Position);
        using var overlay = new OverlayForm(activeScreen);
        if (overlay.ShowDialog() == DialogResult.OK && overlay.SelectedBitmap != null)
            Save(overlay.SelectedBitmap);
    }

    private void CaptureFullScreen()
    {
        var bounds = SystemInformation.VirtualScreen;
        var bmp = new Bitmap(bounds.Width, bounds.Height);
        using var g = Graphics.FromImage(bmp);
        g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
        Save(bmp);
        bmp.Dispose();
    }

    // ── Save ──────────────────────────────────────────────────

    private void Save(Bitmap bmp)
    {
        var prefix = string.IsNullOrWhiteSpace(_settings.UsernamePrefix)
            ? ""
            : _settings.UsernamePrefix.Trim() + "_";
        var ext = _settings.ImageFormat.ToLower() == "jpg" ? "jpg" : "png";
        var filename = $"{prefix}{DateTime.Now:yyyyMMdd_HHmmss}.{ext}";

        bool saved = false;

        if (!string.IsNullOrWhiteSpace(_settings.SavePath))
            saved |= WriteToDisk(bmp, Path.Combine(_settings.SavePath, filename), ext);

        if (!string.IsNullOrWhiteSpace(_settings.LocalBackupPath))
            WriteToDisk(bmp, Path.Combine(_settings.LocalBackupPath, filename), ext);

        if (_settings.CopyToClipboard)
            Clipboard.SetImage(bmp);

        if (_settings.ShowNotification && saved)
            Notify("Screenshot saved", filename);
    }

    private bool WriteToDisk(Bitmap bmp, string fullPath, string ext)
    {
        try
        {
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (ext == "jpg")
            {
                var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                var ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)_settings.JpgQuality);
                bmp.Save(fullPath, codec, ep);
            }
            else
            {
                bmp.Save(fullPath, ImageFormat.Png);
            }
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not save to:\n{fullPath}\n\n{ex.Message}",
                "QuickSnap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }

    // ── Helpers ───────────────────────────────────────────────

    private void Notify(string title, string message) =>
        _trayIcon.ShowBalloonTip(4000, title, message, ToolTipIcon.Info);

    private void OpenSettings()
    {
        using var form = new SettingsForm(_settings);
        if (form.ShowDialog() != DialogResult.OK) return;

        _settings = form.Settings;
        _settings.Save();

        _hotkey.Unregister();
        if (!_hotkey.Register(_settings.HotkeyModifier, _settings.HotkeyKey))
            Notify("Hotkey conflict",
                $"{_settings.HotkeyModifier}+{_settings.HotkeyKey} is already in use. Choose a different key.");
    }

    private void ExitApp()
    {
        _trayIcon.Visible = false;
        _hotkey.Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon?.Dispose();
            _hotkey?.Dispose();
            _trayIconImage?.Dispose();
        }
        base.Dispose(disposing);
    }
}
