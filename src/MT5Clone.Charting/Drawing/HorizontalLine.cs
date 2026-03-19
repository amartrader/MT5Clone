using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;

namespace MT5Clone.Charting.Drawing;

public class HorizontalLine : DrawingToolBase
{
    public override string Name => "Horizontal Line";
    public override DrawingToolType ToolType => DrawingToolType.HorizontalLine;
    public override int RequiredPoints => 1;
    public bool ShowPrice { get; set; } = true;

    public override void Render(IChartCanvas canvas, ChartViewport viewport)
    {
        if (Points.Count < 1) return;

        double y = viewport.PriceToY(Points[0].Price);
        canvas.DrawLine(0, y, viewport.ChartWidth - viewport.PriceAreaWidth, y, Color, Width, new[] { 4.0, 2.0 });

        if (ShowPrice)
        {
            canvas.DrawText(Points[0].Price.ToString("F5"),
                viewport.ChartWidth - viewport.PriceAreaWidth + 2, y - 8, Color, 9);
        }

        if (IsSelected)
        {
            canvas.DrawRectangle(viewport.ChartWidth / 2 - 3, y - 3, 6, 6, Color);
        }
    }

    public override bool HitTest(double x, double y, ChartViewport viewport)
    {
        if (Points.Count < 1) return false;
        double lineY = viewport.PriceToY(Points[0].Price);
        return Math.Abs(y - lineY) < 5;
    }
}

public class VerticalLine : DrawingToolBase
{
    public override string Name => "Vertical Line";
    public override DrawingToolType ToolType => DrawingToolType.VerticalLine;
    public override int RequiredPoints => 1;

    public override void Render(IChartCanvas canvas, ChartViewport viewport)
    {
        if (Points.Count < 1) return;

        double x = viewport.BarToX(Points[0].BarIndex);
        canvas.DrawLine(x, 0, x, viewport.ChartHeight - viewport.TimeAreaHeight, Color, Width, new[] { 4.0, 2.0 });

        if (IsSelected)
        {
            canvas.DrawRectangle(x - 3, viewport.ChartHeight / 2 - 3, 6, 6, Color);
        }
    }

    public override bool HitTest(double x, double y, ChartViewport viewport)
    {
        if (Points.Count < 1) return false;
        double lineX = viewport.BarToX(Points[0].BarIndex);
        return Math.Abs(x - lineX) < 5;
    }
}
