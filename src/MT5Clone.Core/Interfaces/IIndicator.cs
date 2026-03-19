using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;

namespace MT5Clone.Core.Interfaces;

public interface IIndicator
{
    string Name { get; }
    string ShortName { get; }
    IndicatorType Type { get; }
    bool IsOverlay { get; }
    int RequiredBars { get; }
    Dictionary<string, object> Parameters { get; }
    List<IndicatorBuffer> Buffers { get; }

    void Calculate(IReadOnlyList<Candle> candles);
    void SetParameter(string name, object value);
    IIndicator Clone();
}

public class IndicatorBuffer
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public IndicatorBufferStyle Style { get; set; } = IndicatorBufferStyle.Line;
    public string Color { get; set; } = "#FFFFFF";
    public int Width { get; set; } = 1;
    public List<double> Data { get; set; } = new();
    public bool IsVisible { get; set; } = true;
}

public enum IndicatorBufferStyle
{
    Line,
    Histogram,
    Arrow,
    Dots,
    Section,
    Area,
    None
}
