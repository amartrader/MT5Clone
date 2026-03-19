using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;

namespace MT5Clone.Charting.Drawing;

public class TrendLine : DrawingToolBase
{
    public override string Name => "Trend Line";
    public override DrawingToolType ToolType => DrawingToolType.TrendLine;
    public override int RequiredPoints => 2;
    public bool ExtendLeft { get; set; }
    public bool ExtendRight { get; set; }

    public override void Render(IChartCanvas canvas, ChartViewport viewport)
    {
        if (Points.Count < 2) return;

        double x1 = viewport.BarToX(Points[0].BarIndex);
        double y1 = viewport.PriceToY(Points[0].Price);
        double x2 = viewport.BarToX(Points[1].BarIndex);
        double y2 = viewport.PriceToY(Points[1].Price);

        if (ExtendRight || ExtendLeft)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            if (dx != 0)
            {
                double slope = dy / dx;
                if (ExtendRight)
                {
                    x2 = viewport.ChartWidth;
                    y2 = y1 + slope * (x2 - x1);
                }
                if (ExtendLeft)
                {
                    double origX1 = x1;
                    x1 = 0;
                    y1 = y1 - slope * (origX1 - x1);
                }
            }
        }

        canvas.DrawLine(x1, y1, x2, y2, Color, Width);

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

        return DistanceToLineSegment(x, y, x1, y1, x2, y2) < 5;
    }
}
