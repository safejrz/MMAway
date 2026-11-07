using System.Drawing.Drawing2D;

namespace MMA;

internal sealed class SpinnerControl : Control
{
    private int angle;

    public SpinnerControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(204, 204, 204);
        TabStop = false;
    }

    public void Advance()
    {
        angle = (angle + 6) % 360;
        Invalidate();
    }

    public void Reset()
    {
        angle = 0;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var diameter = Math.Min(ClientSize.Width, ClientSize.Height);
        var radius = diameter * 0.34f;
        var dotSize = Math.Max(1f, diameter * 0.11f);
        var centerX = ClientSize.Width / 2f;
        var centerY = ClientSize.Height / 2f;
        for (var i = 0; i < 12; i++)
        {
            var radians = (angle + i * 30) * Math.PI / 180;
            var x = centerX + radius * (float)Math.Cos(radians) - dotSize / 2;
            var y = centerY + radius * (float)Math.Sin(radians) - dotSize / 2;
            using var brush = new SolidBrush(Color.FromArgb(55 + i * 16, ForeColor));
            e.Graphics.FillEllipse(brush, x, y, dotSize, dotSize);
        }
    }
}
