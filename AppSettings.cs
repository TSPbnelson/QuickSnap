using System.Text.Json;
using Microsoft.Win32;

namespace QuickSnap;

public class AppSettings
{
    public string SavePath { get; set; } = @"\\192.168.1.1\ITSupport\Imports";
    public string LocalBackupPath { get; set; } = "";
    public string UsernamePrefix { get; set; } = "";
    public bool CopyToClipboard { get; set; } = true;
    public bool ShowNotification { get; set; } = true;
    public string ImageFormat { get; set; } = "png";
    public int JpgQuality { get; set; } = 90;
    public string HotkeyModifier { get; set; } = "Ctrl";
    public string HotkeyKey { get; set; } = "PrintScreen";
    public bool RunOnStartup { get; set; } = false;

    private static readonly string SettingsPath = Path.Combine(
        AppContext.BaseDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
        }
        catch { }

        var defaults = new AppSettings();
        defaults.Save();
        return defaults;
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save settings:\n{ex.Message}",
                "QuickSnap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        ApplyStartupRegistry();
    }

    private void ApplyStartupRegistry()
    {
        const string regKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        const string appName = "QuickSnap";

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(regKey, writable: true);
            if (key == null) return;

            if (RunOnStartup)
                key.SetValue(appName, $"\"{AppContext.BaseDirectory}QuickSnap.exe\"");
            else
                key.DeleteValue(appName, throwOnMissingValue: false);
        }
        catch { }
    }
}
