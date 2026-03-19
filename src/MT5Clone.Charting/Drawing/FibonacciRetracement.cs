using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;

namespace MT5Clone.Charting.Drawing;

public class FibonacciRetracement : DrawingToolBase
{
    public override string Name => "Fibonacci Retracement";
    public override DrawingToolType ToolType => DrawingToolType.FibonacciRetracement;
    public override int RequiredPoints => 2;

    private static readonly double[] FibLevels = { 0.0, 0.236, 0.382, 0.5, 0.618, 0.786, 1.0, 1.272, 1.618 };
    private static readonly string[] FibColors = { "#808080", "#FF0000", "#00FF00", "#00BFFF", "#FFFF00", "#FF00FF", "#808080", "#FF6600", "#FF6600" };

    public override void Render(IChartCanvas canvas, ChartViewport viewport)
    {
        if (Points.Count < 2) return;

        double price1 = Points[0].Price;
        double price2 = Points[1].Price;
        double priceRange = price2 - price1;

        double x1 = viewport.BarToX(Points[0].BarIndex);
        double x2 = viewport.BarToX(Points[1].BarIndex);
        double left = Math.Min(x1, x2);
        double right = Math.Max(x1, x2);
        double drawWidth = viewport.ChartWidth - viewport.PriceAreaWidth;

        for (int i = 0; i < FibLevels.Length; i++)
        {
            double price = price1 + priceRange * FibLevels[i];
            double y = viewport.PriceToY(price);
            string color = i < FibColors.Length ? FibColors[i] : "#808080";

            canvas.DrawLine(0, y, drawWidth, y, color, 1, new[] { 2.0, 2.0 });

            string label = $"{FibLevels[i] * 100:F1}% ({price:F5})";
            canvas.DrawText(label, left + 5, y - 14, color, 9);
        }

        // Draw the main trendline
        double mainY1 = viewport.PriceToY(price1);
        double mainY2 = viewport.PriceToY(price2);
        canvas.DrawLine(x1, mainY1, x2, mainY2, Color, Width);

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

        double price1 = Points[0].Price;
        double price2 = Points[1].Price;
        double priceRange = price2 - price1;

        for (int i = 0; i < FibLevels.Length; i++)
        {
            double price = price1 + priceRange * FibLevels[i];
            double lineY = viewport.PriceToY(price);
            if (Math.Abs(y - lineY) < 5) return true;
        }

        return false;
    }
}
