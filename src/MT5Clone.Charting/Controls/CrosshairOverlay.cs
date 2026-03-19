using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;

namespace MT5Clone.Charting.Controls;

public class CrosshairOverlay
{
    public bool IsVisible { get; set; }
    public double MouseX { get; set; }
    public double MouseY { get; set; }

    public void Render(IChartCanvas canvas, ChartViewport viewport, IReadOnlyList<Candle> candles, int digits = 5)
    {
        if (!IsVisible) return;

        string crosshairColor = "#808080";
        double chartRight = viewport.ChartWidth - viewport.PriceAreaWidth;
        double chartBottom = viewport.ChartHeight - viewport.TimeAreaHeight;

        // Horizontal line
        if (MouseY >= 0 && MouseY <= chartBottom)
        {
            canvas.DrawLine(0, MouseY, chartRight, MouseY, crosshairColor, 1, new[] { 2.0, 2.0 });

            // Price label
            double price = viewport.YToPrice(MouseY);
            string priceText = price.ToString($"F{digits}");
            double labelWidth = priceText.Length * 7 + 4;
            canvas.DrawRectangle(chartRight, MouseY - 8, labelWidth, 16, "#333333");
            canvas.DrawText(priceText, chartRight + 2, MouseY - 6, "#FFFFFF", 10);
        }

        // Vertical line
        if (MouseX >= 0 && MouseX <= chartRight)
        {
            canvas.DrawLine(MouseX, 0, MouseX, chartBottom, crosshairColor, 1, new[] { 2.0, 2.0 });

            // Time label
            int barIndex = viewport.XToBar(MouseX);
            if (barIndex >= 0 && barIndex < candles.Count)
            {
                string timeText = candles[barIndex].Time.ToString("yyyy.MM.dd HH:mm");
                double labelWidth = timeText.Length * 7 + 4;
                canvas.DrawRectangle(MouseX - labelWidth / 2, chartBottom, labelWidth, 18, "#333333");
                canvas.DrawText(timeText, MouseX - labelWidth / 2 + 2, chartBottom + 2, "#FFFFFF", 10);
            }
        }
    }

    public string GetDataWindowText(ChartViewport viewport, IReadOnlyList<Candle> candles, int digits = 5)
    {
        int barIndex = viewport.XToBar(MouseX);
        if (barIndex < 0 || barIndex >= candles.Count) return string.Empty;

        var candle = candles[barIndex];
        return $"Date: {candle.Time:yyyy.MM.dd}\n" +
               $"Time: {candle.Time:HH:mm}\n" +
               $"Open: {candle.Open.ToString($"F{digits}")}\n" +
               $"High: {candle.High.ToString($"F{digits}")}\n" +
               $"Low: {candle.Low.ToString($"F{digits}")}\n" +
               $"Close: {candle.Close.ToString($"F{digits}")}\n" +
               $"Volume: {candle.TickVolume}";
    }
}
