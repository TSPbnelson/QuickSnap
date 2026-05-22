using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace QuickSnap;

internal static class ClipboardIcon
{
    [DllImport("user32.dll")] private static extern bool DestroyIcon(IntPtr hIcon);

    // Draws a clipboard icon at 32×32 and returns a managed Icon.
    public static Icon Create()
    {
        using var bmp = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        var blue      = Color.FromArgb(0, 145, 200);       // #0091C8 - T3 brand blue
        var bodyFill  = Color.FromArgb(236, 246, 252);     // very light blue tint
        var border    = Color.FromArgb(40, 40, 40);

        // ── Clipboard body ────────────────────────────────────
        using (var fill = new SolidBrush(bodyFill))
            g.FillRectangle(fill, 4f, 9f, 24f, 20f);
        using (var pen = new Pen(border, 1.5f))
            g.DrawRectangle(pen, 4f, 9f, 24f, 20f);

        // ── Clip / clasp at top center ────────────────────────
        using (var fill = new SolidBrush(blue))
            g.FillRectangle(fill, 10f, 4f, 12f, 8f);
        using (var pen = new Pen(Color.FromArgb(0, 100, 150), 1f))
            g.DrawRectangle(pen, 10f, 4f, 12f, 8f);

        // ── Lines suggesting content ──────────────────────────
        using var linePen = new Pen(Color.FromArgb(155, 185, 210), 1.3f);
        g.DrawLine(linePen, 8f, 16f, 24f, 16f);
        g.DrawLine(linePen, 8f, 20f, 24f, 20f);
        g.DrawLine(linePen, 8f, 24f, 18f, 24f);

        // Convert bitmap → HICON → managed Icon (clone so we can destroy the handle)
        var hIcon = bmp.GetHicon();
        try   { return (Icon)Icon.FromHandle(hIcon).Clone(); }
        finally { DestroyIcon(hIcon); }
    }
}
