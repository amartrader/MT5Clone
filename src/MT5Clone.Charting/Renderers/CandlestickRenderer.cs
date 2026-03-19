using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;

namespace MT5Clone.Charting.Renderers;

public class CandlestickRenderer : IChartRenderer
{
    public ChartType ChartType => ChartType.Candlestick;

    public void Render(IChartCanvas canvas, IReadOnlyList<Candle> candles, ChartViewport viewport)
    {
        if (candles.Count == 0) return;

        int start = Math.Max(0, viewport.FirstVisibleBar);
        int end = Math.Min(candles.Count - 1, viewport.LastVisibleBar);

        for (int i = start; i <= end; i++)
        {
            var candle = candles[i];
            double x = viewport.BarToX(i);
            double bodyWidth = viewport.BarWidth * 0.8;

            double yOpen = viewport.PriceToY(candle.Open);
            double yClose = viewport.PriceToY(candle.Close);
            double yHigh = viewport.PriceToY(candle.High);
            double yLow = viewport.PriceToY(candle.Low);

            bool isBullish = candle.IsBullish;
            string bodyColor = isBullish ? "#00FF00" : "#FF0000";
            string wickColor = isBullish ? "#00FF00" : "#FF0000";

            // Draw upper wick
            canvas.DrawLine(x, yHigh, x, Math.Min(yOpen, yClose), wickColor);

            // Draw lower wick
            canvas.DrawLine(x, Math.Max(yOpen, yClose), x, yLow, wickColor);

            // Draw body
            double bodyTop = Math.Min(yOpen, yClose);
            double bodyHeight = Math.Abs(yClose - yOpen);
            if (bodyHeight < 1) bodyHeight = 1;

            string fillColor = isBullish ? "transparent" : bodyColor;
            canvas.DrawRectangle(x - bodyWidth / 2, bodyTop, bodyWidth, bodyHeight, fillColor, bodyColor);
        }
    }
}
