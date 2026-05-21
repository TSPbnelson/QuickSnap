namespace QuickSnap;

static class Program
{
    private static Mutex? _mutex;

    [STAThread]
    static void Main()
    {
        _mutex = new Mutex(true, "QuickSnap_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "QuickSnap is already running.\nCheck the system tray.",
                "QuickSnap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
}
