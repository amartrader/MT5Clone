using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;

namespace MT5Clone.Charting.Drawing;

public abstract class DrawingToolBase : IDrawingTool
{
    private static long _idCounter = 1;

    public long Id { get; } = _idCounter++;
    public abstract string Name { get; }
    public abstract DrawingToolType ToolType { get; }
    public string Color { get; set; } = "#FFFFFF";
    public int Width { get; set; } = 1;
    public bool IsSelected { get; set; }
    public bool IsVisible { get; set; } = true;
    public List<DrawingPoint> Points { get; } = new();
    public Dictionary<string, object> Properties { get; } = new();

    public bool IsComplete => Points.Count >= RequiredPoints;
    public abstract int RequiredPoints { get; }

    public virtual void AddPoint(DrawingPoint point)
    {
        if (Points.Count < RequiredPoints)
            Points.Add(point);
    }

    public virtual void MovePoint(int index, DrawingPoint point)
    {
        if (index >= 0 && index < Points.Count)
            Points[index] = point;
    }

    public abstract void Render(IChartCanvas canvas, ChartViewport viewport);
    public abstract bool HitTest(double x, double y, ChartViewport viewport);

    protected double DistanceToLineSegment(double px, double py, double x1, double y1, double x2, double y2)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        double lengthSquared = dx * dx + dy * dy;

        if (lengthSquared == 0)
            return Math.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1));

        double t = Math.Max(0, Math.Min(1, ((px - x1) * dx + (py - y1) * dy) / lengthSquared));
        double projX = x1 + t * dx;
        double projY = y1 + t * dy;

        return Math.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));
    }
}
