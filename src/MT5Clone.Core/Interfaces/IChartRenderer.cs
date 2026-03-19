using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;

namespace MT5Clone.Core.Interfaces;

public interface IChartRenderer
{
    ChartType ChartType { get; }
    void Render(IChartCanvas canvas, IReadOnlyList<Candle> candles, ChartViewport viewport);
}

public interface IChartCanvas
{
    double Width { get; }
    double Height { get; }

    void DrawLine(double x1, double y1, double x2, double y2, string color, double thickness = 1, double[]? dashPattern = null);
    void DrawRectangle(double x, double y, double width, double height, string fillColor, string? strokeColor = null, double strokeThickness = 1);
    void DrawEllipse(double x, double y, double width, double height, string fillColor, string? strokeColor = null, double strokeThickness = 1);
    void DrawText(string text, double x, double y, string color, double fontSize = 12, string fontFamily = "Segoe UI", TextAlignment alignment = TextAlignment.Left);
    void DrawPolygon(double[] points, string fillColor, string? strokeColor = null, double strokeThickness = 1);
    void SetClip(double x, double y, double width, double height);
    void ClearClip();
    void Clear(string backgroundColor);
}

public enum TextAlignment
{
    Left,
    Center,
    Right
}

public class ChartViewport
{
    public int FirstVisibleBar { get; set; }
    public int LastVisibleBar { get; set; }
    public int BarCount { get; set; }
    public double BarWidth { get; set; } = 8;
    public double BarSpacing { get; set; } = 2;
    public double PriceMin { get; set; }
    public double PriceMax { get; set; }
    public double ChartWidth { get; set; }
    public double ChartHeight { get; set; }
    public double PriceAreaWidth { get; set; } = 80;
    public double TimeAreaHeight { get; set; } = 30;
    public double ScrollPosition { get; set; }
    public double ZoomLevel { get; set; } = 1.0;

    public double PriceToY(double price)
    {
        if (PriceMax <= PriceMin) return 0;
        double range = PriceMax - PriceMin;
        return ChartHeight - ((price - PriceMin) / range * ChartHeight);
    }

    public double YToPrice(double y)
    {
        if (ChartHeight <= 0) return 0;
        double range = PriceMax - PriceMin;
        return PriceMax - (y / ChartHeight * range);
    }

    public double BarToX(int barIndex)
    {
        int relativeIndex = barIndex - FirstVisibleBar;
        return relativeIndex * (BarWidth + BarSpacing) + BarWidth / 2;
    }

    public int XToBar(double x)
    {
        return (int)(x / (BarWidth + BarSpacing)) + FirstVisibleBar;
    }

    public int VisibleBarCount => LastVisibleBar - FirstVisibleBar + 1;

    public void AutoScale(IReadOnlyList<Candle> candles)
    {
        if (candles.Count == 0) return;

        int start = Math.Max(0, FirstVisibleBar);
        int end = Math.Min(candles.Count - 1, LastVisibleBar);

        double min = double.MaxValue;
        double max = double.MinValue;

        for (int i = start; i <= end; i++)
        {
            if (candles[i].Low < min) min = candles[i].Low;
            if (candles[i].High > max) max = candles[i].High;
        }

        double padding = (max - min) * 0.05;
        PriceMin = min - padding;
        PriceMax = max + padding;
    }
}
