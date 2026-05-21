namespace QuickSnap;

public class SettingsForm : Form
{
    private TextBox _savePathBox = null!;
    private TextBox _localBackupBox = null!;
    private TextBox _prefixBox = null!;
    private ComboBox _formatCombo = null!;
    private NumericUpDown _jpgQualityBox = null!;
    private ComboBox _modifierCombo = null!;
    private ComboBox _keyCombo = null!;
    private CheckBox _clipboardCheck = null!;
    private CheckBox _notificationCheck = null!;
    private CheckBox _startupCheck = null!;

    public AppSettings Settings { get; private set; }

    public SettingsForm(AppSettings current)
    {
        Settings = current;
        Text = "QuickSnap — Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(500, 430);
        Font = new Font("Segoe UI", 9f);

        BuildUI();
        LoadValues();
    }

    private void BuildUI()
    {
        int y = 16;
        const int labelW = 150;
        const int inputX = 166;
        const int inputW = 310;
        const int rowH = 32;

        void Label(string text) =>
            Controls.Add(new Label
            {
                Text = text, AutoSize = false,
                Location = new Point(16, y + 3),
                Size = new Size(labelW, 22),
                TextAlign = ContentAlignment.MiddleRight
            });

        // ── Destinations ──────────────────────────────────────
        AddGroupHeader("Destinations", ref y);

        Label("Primary Save Path:");
        _savePathBox = new TextBox { Location = new Point(inputX, y), Width = inputW - 32 };
        var browseBtn = new Button { Text = "…", Location = new Point(inputX + inputW - 28, y - 1), Width = 28, Height = 23 };
        browseBtn.Click += BrowseSavePath;
        Controls.Add(_savePathBox); Controls.Add(browseBtn);
        y += rowH;

        Label("Local Backup Path:");
        _localBackupBox = new TextBox { Location = new Point(inputX, y), Width = inputW - 32 };
        var browseBtn2 = new Button { Text = "…", Location = new Point(inputX + inputW - 28, y - 1), Width = 28, Height = 23 };
        browseBtn2.Click += BrowseLocalPath;
        Controls.Add(_localBackupBox); Controls.Add(browseBtn2);
        y += rowH;

        // ── File ──────────────────────────────────────────────
        y += 4;
        AddGroupHeader("File", ref y);

        Label("Filename Prefix:");
        _prefixBox = new TextBox { Location = new Point(inputX, y), Width = 120 };
        Controls.Add(new Label { Text = "(e.g. brad)", Location = new Point(inputX + 126, y + 3), AutoSize = true, ForeColor = Color.Gray });
        Controls.Add(_prefixBox);
        y += rowH;

        Label("Image Format:");
        _formatCombo = new ComboBox { Location = new Point(inputX, y), Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
        _formatCombo.Items.AddRange(["png", "jpg"]);
        _formatCombo.SelectedIndexChanged += (_, _) => _jpgQualityBox.Enabled = _formatCombo.Text == "jpg";
        Controls.Add(_formatCombo);

        Controls.Add(new Label { Text = "JPG Quality:", Location = new Point(inputX + 90, y + 3), AutoSize = true });
        _jpgQualityBox = new NumericUpDown { Location = new Point(inputX + 170, y), Width = 60, Minimum = 50, Maximum = 100 };
        Controls.Add(_jpgQualityBox);
        y += rowH;

        // ── Hotkey ────────────────────────────────────────────
        y += 4;
        AddGroupHeader("Hotkey", ref y);

        Label("Capture Hotkey:");
        _modifierCombo = new ComboBox { Location = new Point(inputX, y), Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
        _modifierCombo.Items.AddRange(HotkeyManager.SupportedModifiers);
        Controls.Add(_modifierCombo);
        Controls.Add(new Label { Text = "+", Location = new Point(inputX + 86, y + 3), AutoSize = true });
        _keyCombo = new ComboBox { Location = new Point(inputX + 102, y), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        _keyCombo.Items.AddRange(HotkeyManager.SupportedKeys);
        Controls.Add(_keyCombo);
        y += rowH;

        // ── Behavior ──────────────────────────────────────────
        y += 4;
        AddGroupHeader("Behavior", ref y);

        _clipboardCheck = new CheckBox { Text = "Copy to clipboard after capture", Location = new Point(inputX, y), AutoSize = true };
        Controls.Add(_clipboardCheck); y += rowH;

        _notificationCheck = new CheckBox { Text = "Show tray notification after save", Location = new Point(inputX, y), AutoSize = true };
        Controls.Add(_notificationCheck); y += rowH;

        _startupCheck = new CheckBox { Text = "Run QuickSnap when Windows starts", Location = new Point(inputX, y), AutoSize = true };
        Controls.Add(_startupCheck); y += rowH;

        // ── Buttons ───────────────────────────────────────────
        y += 8;
        var btnSave = new Button { Text = "Save", Location = new Point(ClientSize.Width - 180, y), Width = 80, Height = 28, DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Cancel", Location = new Point(ClientSize.Width - 90, y), Width = 80, Height = 28, DialogResult = DialogResult.Cancel };
        btnSave.Click += (_, _) => ApplyValues();
        Controls.Add(btnSave); Controls.Add(btnCancel);
        AcceptButton = btnSave; CancelButton = btnCancel;
        ClientSize = new Size(500, y + 50);
    }

    private void AddGroupHeader(string title, ref int y)
    {
        Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Location = new Point(16, y),
            AutoSize = true,
            ForeColor = Color.FromArgb(0, 91, 200)
        });
        y += 22;
    }

    private void LoadValues()
    {
        _savePathBox.Text = Settings.SavePath;
        _localBackupBox.Text = Settings.LocalBackupPath;
        _prefixBox.Text = Settings.UsernamePrefix;
        _formatCombo.SelectedItem = Settings.ImageFormat;
        _jpgQualityBox.Value = Settings.JpgQuality;
        _jpgQualityBox.Enabled = Settings.ImageFormat == "jpg";
        _modifierCombo.SelectedItem = Settings.HotkeyModifier;
        _keyCombo.SelectedItem = Settings.HotkeyKey;
        _clipboardCheck.Checked = Settings.CopyToClipboard;
        _notificationCheck.Checked = Settings.ShowNotification;
        _startupCheck.Checked = Settings.RunOnStartup;
    }

    private void ApplyValues()
    {
        Settings = new AppSettings
        {
            SavePath = _savePathBox.Text.Trim(),
            LocalBackupPath = _localBackupBox.Text.Trim(),
            UsernamePrefix = _prefixBox.Text.Trim(),
            ImageFormat = _formatCombo.SelectedItem?.ToString() ?? "png",
            JpgQuality = (int)_jpgQualityBox.Value,
            HotkeyModifier = _modifierCombo.SelectedItem?.ToString() ?? "Ctrl",
            HotkeyKey = _keyCombo.SelectedItem?.ToString() ?? "PrintScreen",
            CopyToClipboard = _clipboardCheck.Checked,
            ShowNotification = _notificationCheck.Checked,
            RunOnStartup = _startupCheck.Checked,
        };
    }

    private void BrowseSavePath(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog { Description = "Select primary save folder", UseDescriptionForTitle = true };
        if (dlg.ShowDialog() == DialogResult.OK)
            _savePathBox.Text = dlg.SelectedPath;
    }

    private void BrowseLocalPath(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog { Description = "Select local backup folder", UseDescriptionForTitle = true };
        if (dlg.ShowDialog() == DialogResult.OK)
            _localBackupBox.Text = dlg.SelectedPath;
    }
}
