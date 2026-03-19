using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;

namespace MT5Clone.Charting.Renderers;

public class VolumeRenderer
{
    public void Render(IChartCanvas canvas, IReadOnlyList<Candle> candles, ChartViewport viewport, double volumeAreaHeight)
    {
        if (candles.Count == 0) return;

        int start = Math.Max(0, viewport.FirstVisibleBar);
        int end = Math.Min(candles.Count - 1, viewport.LastVisibleBar);

        // Find max volume in visible range
        long maxVolume = 0;
        for (int i = start; i <= end; i++)
        {
            if (candles[i].TickVolume > maxVolume)
                maxVolume = candles[i].TickVolume;
        }

        if (maxVolume == 0) return;

        double volumeTop = viewport.ChartHeight - volumeAreaHeight;

        for (int i = start; i <= end; i++)
        {
            var candle = candles[i];
            double x = viewport.BarToX(i);
            double barHeight = (candle.TickVolume / (double)maxVolume) * volumeAreaHeight;
            double barWidth = viewport.BarWidth * 0.6;

            string color = candle.IsBullish ? "#4000FF00" : "#40FF0000";

            canvas.DrawRectangle(
                x - barWidth / 2,
                volumeTop + volumeAreaHeight - barHeight,
                barWidth,
                barHeight,
                color);
        }
    }
}
