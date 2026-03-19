using MT5Clone.Core.Interfaces;

namespace MT5Clone.Charting.Renderers;

public class GridRenderer
{
    public void RenderGrid(IChartCanvas canvas, ChartViewport viewport, string gridColor, int digits = 5)
    {
        double priceRange = viewport.PriceMax - viewport.PriceMin;
        if (priceRange <= 0) return;

        // Calculate price grid step
        double step = CalculateGridStep(priceRange, 8);

        // Price grid lines
        double firstPrice = Math.Ceiling(viewport.PriceMin / step) * step;
        for (double price = firstPrice; price <= viewport.PriceMax; price += step)
        {
            double y = viewport.PriceToY(price);
            canvas.DrawLine(0, y, viewport.ChartWidth - viewport.PriceAreaWidth, y, gridColor, 1, new[] { 2.0, 4.0 });

            // Price label
            canvas.DrawText(price.ToString($"F{digits}"),
                viewport.ChartWidth - viewport.PriceAreaWidth + 5, y - 6,
                "#C0C0C0", 10);
        }

        // Time grid lines
        double timeStep = CalculateTimeGridStep(viewport);
        if (timeStep > 0)
        {
            for (int bar = viewport.FirstVisibleBar; bar <= viewport.LastVisibleBar; bar += (int)Math.Max(1, timeStep))
            {
                double x = viewport.BarToX(bar);
                if (x > 0 && x < viewport.ChartWidth - viewport.PriceAreaWidth)
                {
                    canvas.DrawLine(x, 0, x, viewport.ChartHeight - viewport.TimeAreaHeight, gridColor, 1, new[] { 2.0, 4.0 });
                }
            }
        }
    }

    public void RenderPriceScale(IChartCanvas canvas, ChartViewport viewport, string bgColor, string fgColor, int digits = 5)
    {
        double scaleX = viewport.ChartWidth - viewport.PriceAreaWidth;
        canvas.DrawRectangle(scaleX, 0, viewport.PriceAreaWidth, viewport.ChartHeight, bgColor);

        double priceRange = viewport.PriceMax - viewport.PriceMin;
        if (priceRange <= 0) return;

        double step = CalculateGridStep(priceRange, 8);
        double firstPrice = Math.Ceiling(viewport.PriceMin / step) * step;

        for (double price = firstPrice; price <= viewport.PriceMax; price += step)
        {
            double y = viewport.PriceToY(price);
            canvas.DrawText(price.ToString($"F{digits}"), scaleX + 5, y - 6, fgColor, 10);
        }
    }

    public void RenderTimeScale(IChartCanvas canvas, ChartViewport viewport, string bgColor, string fgColor,
        IReadOnlyList<Core.Models.Candle> candles)
    {
        double scaleY = viewport.ChartHeight - viewport.TimeAreaHeight;
        canvas.DrawRectangle(0, scaleY, viewport.ChartWidth, viewport.TimeAreaHeight, bgColor);

        double timeStep = CalculateTimeGridStep(viewport);
        if (timeStep <= 0) return;

        for (int bar = viewport.FirstVisibleBar; bar <= viewport.LastVisibleBar && bar < candles.Count; bar += (int)Math.Max(1, timeStep))
        {
            double x = viewport.BarToX(bar);
            if (x > 0 && x < viewport.ChartWidth - viewport.PriceAreaWidth)
            {
                string timeLabel = FormatTimeLabel(candles[bar].Time, candles[bar].TimeFrame);
                canvas.DrawText(timeLabel, x - 20, scaleY + 5, fgColor, 9);
            }
        }
    }

    private double CalculateGridStep(double range, int targetLines)
    {
        double rawStep = range / targetLines;
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
        double normalized = rawStep / magnitude;

        double step;
        if (normalized <= 1) step = magnitude;
        else if (normalized <= 2) step = 2 * magnitude;
        else if (normalized <= 5) step = 5 * magnitude;
        else step = 10 * magnitude;

        return step;
    }

    private double CalculateTimeGridStep(ChartViewport viewport)
    {
        int visibleBars = viewport.VisibleBarCount;
        if (visibleBars <= 0) return 0;

        int targetLabels = (int)(viewport.ChartWidth / 100);
        return Math.Max(1, visibleBars / Math.Max(1, targetLabels));
    }

    private string FormatTimeLabel(DateTime time, Core.Enums.TimeFrame timeFrame)
    {
        return timeFrame switch
        {
            Core.Enums.TimeFrame.MN1 => time.ToString("MMM yyyy"),
            Core.Enums.TimeFrame.W1 or Core.Enums.TimeFrame.D1 => time.ToString("dd MMM"),
            _ => time.ToString("HH:mm")
        };
    }
}
