using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;

namespace MT5Clone.Charting.Drawing;

public class RectangleDrawing : DrawingToolBase
{
    public override string Name => "Rectangle";
    public override DrawingToolType ToolType => DrawingToolType.Rectangle;
    public override int RequiredPoints => 2;
    public string FillColor { get; set; } = "#1AFFFFFF";

    public override void Render(IChartCanvas canvas, ChartViewport viewport)
    {
        if (Points.Count < 2) return;

        double x1 = viewport.BarToX(Points[0].BarIndex);
        double y1 = viewport.PriceToY(Points[0].Price);
        double x2 = viewport.BarToX(Points[1].BarIndex);
        double y2 = viewport.PriceToY(Points[1].Price);

        double left = Math.Min(x1, x2);
        double top = Math.Min(y1, y2);
        double width = Math.Abs(x2 - x1);
        double height = Math.Abs(y2 - y1);

        canvas.DrawRectangle(left, top, width, height, FillColor, Color, Width);

        if (IsSelected)
        {
            foreach (var point in Points)
            {
                double px = viewport.BarToX(point.BarIndex);
                double py = viewport.PriceToY(point.Price);
                canvas.DrawRectangle(px - 3, py - 3, 6, 6, Color);
            }
        }
    }

    public override bool HitTest(double x, double y, ChartViewport viewport)
    {
        if (Points.Count < 2) return false;

        double x1 = viewport.BarToX(Points[0].BarIndex);
        double y1 = viewport.PriceToY(Points[0].Price);
        double x2 = viewport.BarToX(Points[1].BarIndex);
        double y2 = viewport.PriceToY(Points[1].Price);

        double left = Math.Min(x1, x2);
        double top = Math.Min(y1, y2);
        double right = Math.Max(x1, x2);
        double bottom = Math.Max(y1, y2);

        // Check if near any edge
        bool nearLeft = Math.Abs(x - left) < 5 && y >= top && y <= bottom;
        bool nearRight = Math.Abs(x - right) < 5 && y >= top && y <= bottom;
        bool nearTop = Math.Abs(y - top) < 5 && x >= left && x <= right;
        bool nearBottom = Math.Abs(y - bottom) < 5 && x >= left && x <= right;

        return nearLeft || nearRight || nearTop || nearBottom;
    }
}

public class EllipseDrawing : DrawingToolBase
{
    public override string Name => "Ellipse";
    public override DrawingToolType ToolType => DrawingToolType.Ellipse;
    public override int RequiredPoints => 2;
    public string FillColor { get; set; } = "#1AFFFFFF";

    public override void Render(IChartCanvas canvas, ChartViewport viewport)
    {
        if (Points.Count < 2) return;

        double x1 = viewport.BarToX(Points[0].BarIndex);
        double y1 = viewport.PriceToY(Points[0].Price);
        double x2 = viewport.BarToX(Points[1].BarIndex);
        double y2 = viewport.PriceToY(Points[1].Price);

        double cx = (x1 + x2) / 2;
        double cy = (y1 + y2) / 2;
        double width = Math.Abs(x2 - x1);
        double height = Math.Abs(y2 - y1);

        canvas.DrawEllipse(cx - width / 2, cy - height / 2, width, height, FillColor, Color, Width);
    }

    public override bool HitTest(double x, double y, ChartViewport viewport)
    {
        if (Points.Count < 2) return false;

        double x1 = viewport.BarToX(Points[0].BarIndex);
        double y1 = viewport.PriceToY(Points[0].Price);
        double x2 = viewport.BarToX(Points[1].BarIndex);
        double y2 = viewport.PriceToY(Points[1].Price);

        double cx = (x1 + x2) / 2;
        double cy = (y1 + y2) / 2;
        double rx = Math.Abs(x2 - x1) / 2;
        double ry = Math.Abs(y2 - y1) / 2;

        if (rx == 0 || ry == 0) return false;

        double value = ((x - cx) * (x - cx)) / (rx * rx) + ((y - cy) * (y - cy)) / (ry * ry);
        return Math.Abs(value - 1.0) < 0.2;
    }
}

public class TextDrawing : DrawingToolBase
{
    public override string Name => "Text";
    public override DrawingToolType ToolType => DrawingToolType.Text;
    public override int RequiredPoints => 1;
    public string Text { get; set; } = "Text";
    public double FontSize { get; set; } = 12;

    public override void Render(IChartCanvas canvas, ChartViewport viewport)
    {
        if (Points.Count < 1) return;

        double x = viewport.BarToX(Points[0].BarIndex);
        double y = viewport.PriceToY(Points[0].Price);

        canvas.DrawText(Text, x, y, Color, FontSize);
    }

    public override bool HitTest(double x, double y, ChartViewport viewport)
    {
        if (Points.Count < 1) return false;

        double tx = viewport.BarToX(Points[0].BarIndex);
        double ty = viewport.PriceToY(Points[0].Price);

        return Math.Abs(x - tx) < Text.Length * 7 && Math.Abs(y - ty) < FontSize;
    }
}
