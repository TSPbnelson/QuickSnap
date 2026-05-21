using System.Drawing.Imaging;

namespace QuickSnap;

public class OverlayForm : Form
{
    private Bitmap? _screenCapture;
    private Point _startPoint;
    private Rectangle _selection;
    private bool _isSelecting;

    private readonly Pen _borderPen = new(Color.White, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
    private readonly SolidBrush _dimBrush = new(Color.FromArgb(120, 0, 0, 0));
    private readonly Font _labelFont = new("Segoe UI", 9f);

    public Bitmap? SelectedBitmap { get; private set; }

    public OverlayForm()
    {
        var screen = SystemInformation.VirtualScreen;
        FormBorderStyle = FormBorderStyle.None;
        Bounds = screen;
        TopMost = true;
        ShowInTaskbar = false;
        Cursor = Cursors.Cross;
        BackColor = Color.Black;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        KeyPreview = true;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        CaptureAllScreens();
        Invalidate();
    }

    private void CaptureAllScreens()
    {
        var bounds = SystemInformation.VirtualScreen;
        _screenCapture = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(_screenCapture);
        g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_screenCapture == null) return;

        var g = e.Graphics;

        // Draw captured screen
        g.DrawImage(_screenCapture, 0, 0);

        // Dim everything
        g.FillRectangle(_dimBrush, ClientRectangle);

        if (!_isSelecting || _selection.Width == 0 || _selection.Height == 0)
            return;

        var sel = Normalize(_selection);

        // Reveal selected region (undimmed)
        g.DrawImage(_screenCapture, sel, sel, GraphicsUnit.Pixel);

        // Selection border
        g.DrawRectangle(_borderPen, sel);

        // Dimension label
        var label = $" {sel.Width} × {sel.Height} ";
        var size = g.MeasureString(label, _labelFont);
        float lx = sel.Right - size.Width;
        float ly = sel.Bottom + 4;
        if (ly + size.Height > ClientRectangle.Bottom) ly = sel.Top - size.Height - 4;
        if (lx < sel.Left) lx = sel.Left;

        g.FillRectangle(Brushes.Black, lx, ly, size.Width, size.Height);
        g.DrawString(label, _labelFont, Brushes.White, lx, ly);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        _startPoint = e.Location;
        _selection = new Rectangle(e.Location, Size.Empty);
        _isSelecting = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isSelecting) return;
        _selection = new Rectangle(_startPoint, new Size(e.X - _startPoint.X, e.Y - _startPoint.Y));
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (!_isSelecting || e.Button != MouseButtons.Left) return;
        _isSelecting = false;

        var sel = Normalize(_selection);
        if (sel.Width > 4 && sel.Height > 4)
        {
            SelectedBitmap = new Bitmap(sel.Width, sel.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(SelectedBitmap);
            g.DrawImage(_screenCapture!, 0, 0, sel, GraphicsUnit.Pixel);
            DialogResult = DialogResult.OK;
        }
        else
        {
            DialogResult = DialogResult.Cancel;
        }

        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private static Rectangle Normalize(Rectangle r) =>
        new(Math.Min(r.X, r.X + r.Width), Math.Min(r.Y, r.Y + r.Height),
            Math.Abs(r.Width), Math.Abs(r.Height));

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _screenCapture?.Dispose();
            _borderPen.Dispose();
            _dimBrush.Dispose();
            _labelFont.Dispose();
        }
        base.Dispose(disposing);
    }
}
