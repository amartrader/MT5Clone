using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;

namespace MT5Clone.Charting.Renderers;

public class BarRenderer : IChartRenderer
{
    public ChartType ChartType => ChartType.Bar;

    public void Render(IChartCanvas canvas, IReadOnlyList<Candle> candles, ChartViewport viewport)
    {
        if (candles.Count == 0) return;

        int start = Math.Max(0, viewport.FirstVisibleBar);
        int end = Math.Min(candles.Count - 1, viewport.LastVisibleBar);
        double tickWidth = viewport.BarWidth * 0.4;

        for (int i = start; i <= end; i++)
        {
            var candle = candles[i];
            double x = viewport.BarToX(i);

            double yOpen = viewport.PriceToY(candle.Open);
            double yClose = viewport.PriceToY(candle.Close);
            double yHigh = viewport.PriceToY(candle.High);
            double yLow = viewport.PriceToY(candle.Low);

            string color = candle.IsBullish ? "#00FF00" : "#FF0000";

            // Vertical line (high to low)
            canvas.DrawLine(x, yHigh, x, yLow, color);

            // Open tick (left)
            canvas.DrawLine(x - tickWidth, yOpen, x, yOpen, color);

            // Close tick (right)
            canvas.DrawLine(x, yClose, x + tickWidth, yClose, color);
        }
    }
}
