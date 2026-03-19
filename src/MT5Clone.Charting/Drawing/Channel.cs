using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;

namespace MT5Clone.Charting.Drawing;

public class Channel : DrawingToolBase
{
    public override string Name => "Equidistant Channel";
    public override DrawingToolType ToolType => DrawingToolType.Channel;
    public override int RequiredPoints => 3;

    public override void Render(IChartCanvas canvas, ChartViewport viewport)
    {
        if (Points.Count < 3) return;

        double x1 = viewport.BarToX(Points[0].BarIndex);
        double y1 = viewport.PriceToY(Points[0].Price);
        double x2 = viewport.BarToX(Points[1].BarIndex);
        double y2 = viewport.PriceToY(Points[1].Price);
        double x3 = viewport.BarToX(Points[2].BarIndex);
        double y3 = viewport.PriceToY(Points[2].Price);

        // Main line
        canvas.DrawLine(x1, y1, x2, y2, Color, Width);

        // Parallel line through point 3
        double dx = x2 - x1;
        double dy = y2 - y1;
        double offsetX = x3 - x1;
        double offsetY = y3 - y1;

        canvas.DrawLine(x1 + offsetX, y1 + offsetY, x2 + offsetX, y2 + offsetY, Color, Width);

        // Middle line (dashed)
        canvas.DrawLine(
            (x1 + x1 + offsetX) / 2, (y1 + y1 + offsetY) / 2,
            (x2 + x2 + offsetX) / 2, (y2 + y2 + offsetY) / 2,
            Color, 1, new[] { 4.0, 4.0 });

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

        if (DistanceToLineSegment(x, y, x1, y1, x2, y2) < 5)
            return true;

        if (Points.Count >= 3)
        {
            double offsetX = viewport.BarToX(Points[2].BarIndex) - x1;
            double offsetY = viewport.PriceToY(Points[2].Price) - y1;
            if (DistanceToLineSegment(x, y, x1 + offsetX, y1 + offsetY, x2 + offsetX, y2 + offsetY) < 5)
                return true;
        }

        return false;
    }
}

public class AndrewsPitchfork : DrawingToolBase
{
    public override string Name => "Andrew's Pitchfork";
    public override DrawingToolType ToolType => DrawingToolType.AndrewsPitchfork;
    public override int RequiredPoints => 3;

    public override void Render(IChartCanvas canvas, ChartViewport viewport)
    {
        if (Points.Count < 3) return;

        double x1 = viewport.BarToX(Points[0].BarIndex);
        double y1 = viewport.PriceToY(Points[0].Price);
        double x2 = viewport.BarToX(Points[1].BarIndex);
        double y2 = viewport.PriceToY(Points[1].Price);
        double x3 = viewport.BarToX(Points[2].BarIndex);
        double y3 = viewport.PriceToY(Points[2].Price);

        // Midpoint of points 2 and 3
        double midX = (x2 + x3) / 2;
        double midY = (y2 + y3) / 2;

        // Median line from point 1 through midpoint
        double medianDx = midX - x1;
        double medianDy = midY - y1;
        double extendX = x1 + medianDx * 3;
        double extendY = y1 + medianDy * 3;

        canvas.DrawLine(x1, y1, extendX, extendY, Color, Width);

        // Upper line from point 2
        canvas.DrawLine(x2, y2, x2 + medianDx * 2, y2 + medianDy * 2, Color, Width);

        // Lower line from point 3
        canvas.DrawLine(x3, y3, x3 + medianDx * 2, y3 + medianDy * 2, Color, Width);

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
        if (Points.Count < 3) return false;

        double x1 = viewport.BarToX(Points[0].BarIndex);
        double y1 = viewport.PriceToY(Points[0].Price);
        double x2 = viewport.BarToX(Points[1].BarIndex);
        double y2 = viewport.PriceToY(Points[1].Price);
        double x3 = viewport.BarToX(Points[2].BarIndex);
        double y3 = viewport.PriceToY(Points[2].Price);

        double midX = (x2 + x3) / 2;
        double midY = (y2 + y3) / 2;

        return DistanceToLineSegment(x, y, x1, y1, midX, midY) < 5 ||
               DistanceToLineSegment(x, y, x2, y2, x2 + (midX - x1), y2 + (midY - y1)) < 5 ||
               DistanceToLineSegment(x, y, x3, y3, x3 + (midX - x1), y3 + (midY - y1)) < 5;
    }
}
