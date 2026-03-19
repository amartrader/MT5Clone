using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;

namespace MT5Clone.Indicators.Base;

public abstract class IndicatorBase : IIndicator
{
    public abstract string Name { get; }
    public abstract string ShortName { get; }
    public abstract IndicatorType Type { get; }
    public virtual bool IsOverlay => false;
    public virtual int RequiredBars => 1;
    public Dictionary<string, object> Parameters { get; } = new();
    public List<IndicatorBuffer> Buffers { get; } = new();

    protected void AddBuffer(string name, string label, string color = "#FFFFFF",
        IndicatorBufferStyle style = IndicatorBufferStyle.Line, int width = 1)
    {
        Buffers.Add(new IndicatorBuffer
        {
            Name = name,
            Label = label,
            Color = color,
            Style = style,
            Width = width
        });
    }

    protected void SetBufferSize(int size)
    {
        foreach (var buffer in Buffers)
        {
            while (buffer.Data.Count < size)
                buffer.Data.Add(double.NaN);
            if (buffer.Data.Count > size)
                buffer.Data.RemoveRange(size, buffer.Data.Count - size);
        }
    }

    protected double GetAppliedPrice(Candle candle, AppliedPrice appliedPrice)
    {
        return appliedPrice switch
        {
            AppliedPrice.Close => candle.Close,
            AppliedPrice.Open => candle.Open,
            AppliedPrice.High => candle.High,
            AppliedPrice.Low => candle.Low,
            AppliedPrice.Median => candle.MedianPrice,
            AppliedPrice.Typical => candle.TypicalPrice,
            AppliedPrice.Weighted => candle.WeightedClose,
            _ => candle.Close
        };
    }

    protected T GetParameter<T>(string name, T defaultValue)
    {
        if (Parameters.TryGetValue(name, out var value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    public virtual void SetParameter(string name, object value)
    {
        Parameters[name] = value;
    }

    public abstract void Calculate(IReadOnlyList<Candle> candles);

    public virtual IIndicator Clone()
    {
        var clone = (IndicatorBase)MemberwiseClone();
        return clone;
    }
}
