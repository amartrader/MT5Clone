using MT5Clone.Core.Interfaces;

namespace MT5Clone.Charting.Renderers;

public class IndicatorRenderer
{
    public void Render(IChartCanvas canvas, IIndicator indicator, ChartViewport viewport)
    {
        foreach (var buffer in indicator.Buffers.Where(b => b.IsVisible))
        {
            switch (buffer.Style)
            {
                case IndicatorBufferStyle.Line:
                    RenderLine(canvas, buffer, viewport);
                    break;
                case IndicatorBufferStyle.Histogram:
                    RenderHistogram(canvas, buffer, viewport);
                    break;
                case IndicatorBufferStyle.Dots:
                    RenderDots(canvas, buffer, viewport);
                    break;
                case IndicatorBufferStyle.Arrow:
                    RenderArrow(canvas, buffer, viewport);
                    break;
                case IndicatorBufferStyle.Area:
                    RenderArea(canvas, buffer, viewport);
                    break;
            }
        }
    }

    private void RenderLine(IChartCanvas canvas, IndicatorBuffer buffer, ChartViewport viewport)
    {
        int start = Math.Max(0, viewport.FirstVisibleBar);
        int end = Math.Min(buffer.Data.Count - 1, viewport.LastVisibleBar);

        for (int i = start + 1; i <= end; i++)
        {
            if (double.IsNaN(buffer.Data[i]) || double.IsNaN(buffer.Data[i - 1]))
                continue;

            double x1 = viewport.BarToX(i - 1);
            double y1 = viewport.PriceToY(buffer.Data[i - 1]);
            double x2 = viewport.BarToX(i);
            double y2 = viewport.PriceToY(buffer.Data[i]);

            canvas.DrawLine(x1, y1, x2, y2, buffer.Color, buffer.Width);
        }
    }

    private void RenderHistogram(IChartCanvas canvas, IndicatorBuffer buffer, ChartViewport viewport)
    {
        int start = Math.Max(0, viewport.FirstVisibleBar);
        int end = Math.Min(buffer.Data.Count - 1, viewport.LastVisibleBar);
        double zeroY = viewport.PriceToY(0);

        for (int i = start; i <= end; i++)
        {
            if (double.IsNaN(buffer.Data[i])) continue;

            double x = viewport.BarToX(i);
            double y = viewport.PriceToY(buffer.Data[i]);
            double barWidth = viewport.BarWidth * 0.6;

            string color = buffer.Data[i] >= 0 ? buffer.Color : "#FF0000";

            double top = Math.Min(y, zeroY);
            double height = Math.Abs(y - zeroY);

            canvas.DrawRectangle(x - barWidth / 2, top, barWidth, Math.Max(height, 1), color);
        }
    }

    private void RenderDots(IChartCanvas canvas, IndicatorBuffer buffer, ChartViewport viewport)
    {
        int start = Math.Max(0, viewport.FirstVisibleBar);
        int end = Math.Min(buffer.Data.Count - 1, viewport.LastVisibleBar);

        for (int i = start; i <= end; i++)
        {
            if (double.IsNaN(buffer.Data[i])) continue;

            double x = viewport.BarToX(i);
            double y = viewport.PriceToY(buffer.Data[i]);
            double dotSize = buffer.Width * 2 + 2;

            canvas.DrawEllipse(x - dotSize / 2, y - dotSize / 2, dotSize, dotSize, buffer.Color);
        }
    }

    private void RenderArrow(IChartCanvas canvas, IndicatorBuffer buffer, ChartViewport viewport)
    {
        int start = Math.Max(0, viewport.FirstVisibleBar);
        int end = Math.Min(buffer.Data.Count - 1, viewport.LastVisibleBar);

        for (int i = start; i <= end; i++)
        {
            if (double.IsNaN(buffer.Data[i])) continue;

            double x = viewport.BarToX(i);
            double y = viewport.PriceToY(buffer.Data[i]);

            canvas.DrawText("▲", x - 4, y - 8, buffer.Color, 10);
        }
    }

    private void RenderArea(IChartCanvas canvas, IndicatorBuffer buffer, ChartViewport viewport)
    {
        RenderLine(canvas, buffer, viewport);
    }
}
