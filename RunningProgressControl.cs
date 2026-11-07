using System.Drawing.Drawing2D;

namespace MMA;

internal sealed class RunningProgressControl : Control
{
    private double phase;

    public RunningProgressControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.White;
        TabStop = false;
        AccessibleRole = AccessibleRole.ProgressBar;
        AccessibleName = "Running";
    }

    public void Advance()
    {
        phase = (phase + 0.008) % 1;
        Invalidate();
    }

    public void Reset()
    {
        phase = 0;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (ClientSize.Width < 2 || ClientSize.Height < 2) return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var trackHeight = Math.Min(ClientSize.Height, Math.Max(2, (int)Math.Round(10 * DeviceDpi / 96.0)));
        var track = new Rectangle(0, (ClientSize.Height - trackHeight) / 2, ClientSize.Width, trackHeight);
        var diameter = Math.Min(track.Height, track.Width);
        using var shape = new GraphicsPath();
        shape.AddArc(track.Left, track.Top, diameter, diameter, 90, 180);
        shape.AddArc(track.Right - diameter, track.Top, diameter, diameter, 270, 180);
        shape.CloseFigure();
        using var background = new SolidBrush(Color.FromArgb(60, 60, 60));
        using var foreground = new SolidBrush(Color.FromArgb(0, 122, 204));
        e.Graphics.FillPath(background, shape);
        var state = e.Graphics.Save();
        e.Graphics.SetClip(shape);
        var segmentWidth = Math.Max(1, track.Width / 3);
        var segmentX = (float)(track.Left - segmentWidth + phase * (track.Width + segmentWidth));
        e.Graphics.FillRectangle(foreground, segmentX, track.Top, segmentWidth, track.Height);
        e.Graphics.Restore(state);
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
    }
}
