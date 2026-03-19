using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;

namespace MT5Clone.Charting.Renderers;

public class LineRenderer : IChartRenderer
{
    public ChartType ChartType => ChartType.Line;

    public void Render(IChartCanvas canvas, IReadOnlyList<Candle> candles, ChartViewport viewport)
    {
        if (candles.Count < 2) return;

        int start = Math.Max(0, viewport.FirstVisibleBar);
        int end = Math.Min(candles.Count - 1, viewport.LastVisibleBar);
        string lineColor = "#00BFFF";

        for (int i = start + 1; i <= end; i++)
        {
            double x1 = viewport.BarToX(i - 1);
            double y1 = viewport.PriceToY(candles[i - 1].Close);
            double x2 = viewport.BarToX(i);
            double y2 = viewport.PriceToY(candles[i].Close);

            canvas.DrawLine(x1, y1, x2, y2, lineColor, 2);
        }
    }
}

public class AreaRenderer : IChartRenderer
{
    public ChartType ChartType => ChartType.Area;

    public void Render(IChartCanvas canvas, IReadOnlyList<Candle> candles, ChartViewport viewport)
    {
        if (candles.Count < 2) return;

        int start = Math.Max(0, viewport.FirstVisibleBar);
        int end = Math.Min(candles.Count - 1, viewport.LastVisibleBar);
        string lineColor = "#00BFFF";
        string fillColor = "#1A00BFFF";

        // Draw filled area
        var points = new List<double>();
        for (int i = start; i <= end; i++)
        {
            points.Add(viewport.BarToX(i));
            points.Add(viewport.PriceToY(candles[i].Close));
        }
        // Add bottom points to close the polygon
        points.Add(viewport.BarToX(end));
        points.Add(viewport.ChartHeight);
        points.Add(viewport.BarToX(start));
        points.Add(viewport.ChartHeight);

        canvas.DrawPolygon(points.ToArray(), fillColor, lineColor);

        // Draw line on top
        for (int i = start + 1; i <= end; i++)
        {
            double x1 = viewport.BarToX(i - 1);
            double y1 = viewport.PriceToY(candles[i - 1].Close);
            double x2 = viewport.BarToX(i);
            double y2 = viewport.PriceToY(candles[i].Close);

            canvas.DrawLine(x1, y1, x2, y2, lineColor, 2);
        }
    }
}
