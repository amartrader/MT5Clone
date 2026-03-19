using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Oscillators;

public class WilliamsR : IndicatorBase
{
    public override string Name => "Williams' Percent Range";
    public override string ShortName => $"W%R({GetParameter("Period", 14)})";
    public override IndicatorType Type => IndicatorType.WilliamsPercentR;
    public override bool IsOverlay => false;
    public override int RequiredBars => GetParameter("Period", 14);

    public WilliamsR(int period = 14)
    {
        Parameters["Period"] = period;
        AddBuffer("WPR", $"W%R({period})", "#00FF00");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int period = GetParameter("Period", 14);
        SetBufferSize(candles.Count);
        var wpr = Buffers[0].Data;

        for (int i = 0; i < candles.Count; i++)
        {
            if (i < period - 1)
            {
                wpr[i] = double.NaN;
                continue;
            }

            double highest = double.MinValue;
            double lowest = double.MaxValue;
            for (int j = 0; j < period; j++)
            {
                highest = Math.Max(highest, candles[i - j].High);
                lowest = Math.Min(lowest, candles[i - j].Low);
            }

            double range = highest - lowest;
            wpr[i] = range > 0 ? -100 * (highest - candles[i].Close) / range : 0;
        }
    }
}
