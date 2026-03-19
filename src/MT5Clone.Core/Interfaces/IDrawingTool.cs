using MT5Clone.Core.Enums;

namespace MT5Clone.Core.Interfaces;

public interface IDrawingTool
{
    long Id { get; }
    string Name { get; }
    DrawingToolType ToolType { get; }
    string Color { get; set; }
    int Width { get; set; }
    bool IsSelected { get; set; }
    bool IsVisible { get; set; }
    List<DrawingPoint> Points { get; }
    Dictionary<string, object> Properties { get; }

    bool IsComplete { get; }
    int RequiredPoints { get; }
    void AddPoint(DrawingPoint point);
    void MovePoint(int index, DrawingPoint point);
    void Render(IChartCanvas canvas, ChartViewport viewport);
    bool HitTest(double x, double y, ChartViewport viewport);
}

public class DrawingPoint
{
    public DateTime Time { get; set; }
    public double Price { get; set; }
    public int BarIndex { get; set; }

    public DrawingPoint() { }

    public DrawingPoint(DateTime time, double price, int barIndex = 0)
    {
        Time = time;
        Price = price;
        BarIndex = barIndex;
    }
}
