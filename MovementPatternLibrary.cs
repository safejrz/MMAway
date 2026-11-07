namespace MMA;

public enum MovementPattern
{
    Square,
    Circle,
    Triangle,
    VerticalHourglass,
    HorizontalHourglass,
    SpiralOut,
    SpiralIn,
    Oval,
    IceCreamCone,
    FigureEight
}

public static class MovementPatternLibrary
{
    public const int DefaultSegments = 240;

    public static IReadOnlyList<PointF> CreateSquare(int segments = DefaultSegments) =>
        SamplePolyline([new(-0.8f, -0.8f), new(0.8f, -0.8f), new(0.8f, 0.8f), new(-0.8f, 0.8f)],
            segments, close: true);

    public static IReadOnlyList<PointF> CreateCircle(int segments = DefaultSegments) =>
        SampleParametric(segments, t => new PointF((float)Math.Cos(t), (float)Math.Sin(t)), 2 * Math.PI);

    public static IReadOnlyList<PointF> CreateTriangle(int segments = DefaultSegments) =>
        SamplePolyline([new(0, -0.9f), new(0.82f, 0.75f), new(-0.82f, 0.75f)], segments, close: true);

    public static IReadOnlyList<PointF> CreateVerticalHourglass(int segments = DefaultSegments) =>
        SamplePolyline(
        [
            new(-0.82f, -0.85f), new(0.82f, -0.85f), new(-0.72f, 0), new(0.82f, 0.85f),
            new(-0.82f, 0.85f), new(0.72f, 0), new(-0.82f, -0.85f)
        ], segments);

    public static IReadOnlyList<PointF> CreateHorizontalHourglass(int segments = DefaultSegments) =>
        SamplePolyline(
            [
                new(-0.9f, -0.45f), new(0, -0.16f), new(0.9f, -0.45f),
                new(0.9f, 0.45f), new(0, 0.16f), new(-0.9f, 0.45f)
            ],
            segments, close: true);

    public static IReadOnlyList<PointF> CreateSpiralOut(int segments = DefaultSegments) =>
        SampleByIndex(segments, i =>
        {
            var progress = (double)i / (segments - 1);
            var angle = progress * 4 * Math.PI;
            var radius = 0.08 + 0.88 * progress;
            return new PointF((float)(radius * Math.Cos(angle)), (float)(radius * Math.Sin(angle)));
        });

    public static IReadOnlyList<PointF> CreateSpiralIn(int segments = DefaultSegments) =>
        SampleByIndex(segments, i =>
        {
            var progress = (double)i / (segments - 1);
            var angle = progress * 4 * Math.PI;
            var radius = 0.96 - 0.88 * progress;
            return new PointF((float)(radius * Math.Cos(angle)), (float)(radius * Math.Sin(angle)));
        });

    public static IReadOnlyList<PointF> CreateOval(int segments = DefaultSegments) =>
        SampleParametric(segments, t => new PointF((float)Math.Cos(t), (float)Math.Sin(t)), 2 * Math.PI);

    public static IReadOnlyList<PointF> CreateIceCreamCone(int segments = DefaultSegments)
    {
        if (segments < 8)
            throw new ArgumentOutOfRangeException(nameof(segments), "An ice-cream-cone path requires at least eight points.");

        var lineSteps = segments / 4;
        var arcSteps = segments - 2 * lineSteps;
        var path = new List<PointF>(segments);
        for (var i = 0; i < lineSteps; i++)
        {
            var t = (float)i / lineSteps;
            path.Add(new PointF(-0.5f * t, 1.0f - 1.2f * t));
        }
        for (var i = 0; i < arcSteps; i++)
        {
            var angle = Math.PI + Math.PI * i / (arcSteps - 1);
            path.Add(new PointF((float)(0.5 * Math.Cos(angle)), (float)(-0.2 + 0.5 * Math.Sin(angle))));
        }
        for (var i = 0; i < lineSteps; i++)
        {
            var t = (float)i / (lineSteps - 1);
            path.Add(new PointF(0.5f - 0.5f * t, -0.2f + 1.2f * t));
        }
        path[^1] = path[0];
        return path;
    }

    public static IReadOnlyList<PointF> CreateFigureEight(int segments = DefaultSegments) =>
        SampleParametric(segments, t => new PointF((float)Math.Sin(t), (float)(Math.Sin(2 * t) * 0.5)), 2 * Math.PI);

    private static IReadOnlyList<PointF> SampleParametric(int segments, Func<double, PointF> pointAt, double end)
    {
        ValidateSegments(segments);
        return SampleByIndex(segments, i => pointAt(end * i / (segments - 1)));
    }

    private static IReadOnlyList<PointF> SampleByIndex(int segments, Func<int, PointF> pointAt)
    {
        ValidateSegments(segments);
        return Enumerable.Range(0, segments).Select(pointAt).ToArray();
    }

    private static IReadOnlyList<PointF> SamplePolyline(IReadOnlyList<PointF> vertices, int count, bool close = false)
    {
        ValidateSegments(count);
        var edges = close ? vertices.Count : vertices.Count - 1;
        var result = new PointF[count];
        for (var i = 0; i < count; i++)
        {
            var progress = (float)i / (count - 1) * edges;
            var edge = Math.Min((int)progress, edges - 1);
            var start = vertices[edge];
            var end = vertices[(edge + 1) % vertices.Count];
            var t = progress - edge;
            result[i] = new PointF(start.X + (end.X - start.X) * t, start.Y + (end.Y - start.Y) * t);
        }
        return result;
    }

    private static void ValidateSegments(int segments)
    {
        if (segments < 2)
            throw new ArgumentOutOfRangeException(nameof(segments), "A movement path requires at least two points.");
    }
}
