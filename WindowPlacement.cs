namespace MMA;

internal static class WindowPlacement
{
    internal static Point GetStartupPosition(Rectangle workingArea, Size windowSize)
    {
        var maxX = Math.Max(0, workingArea.Width - windowSize.Width);
        var maxY = Math.Max(0, workingArea.Height - windowSize.Height);
        var x = (int)Math.Round(workingArea.Width * 0.875 - windowSize.Width / 2.0);
        var y = workingArea.Height / 5;

        return new Point(
            workingArea.X + Math.Clamp(x, 0, maxX),
            workingArea.Y + Math.Clamp(y, 0, maxY));
    }
}
