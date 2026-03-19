using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Oscillators;

public class CCI : IndicatorBase
{
    public override string Name => "Commodity Channel Index";
    public override string ShortName => $"CCI({GetParameter("Period", 14)})";
    public override IndicatorType Type => IndicatorType.CCI;
    public override bool IsOverlay => false;
    public override int RequiredBars => GetParameter("Period", 14);

    public CCI(int period = 14)
    {
        Parameters["Period"] = period;
        AddBuffer("CCI", $"CCI({period})", "#FF00FF");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int period = GetParameter("Period", 14);
        SetBufferSize(candles.Count);
        var cci = Buffers[0].Data;

        for (int i = 0; i < candles.Count; i++)
        {
            if (i < period - 1)
            {
                cci[i] = double.NaN;
                continue;
            }

            // Calculate typical prices and SMA
            double smaSum = 0;
            for (int j = 0; j < period; j++)
                smaSum += candles[i - j].TypicalPrice;
            double sma = smaSum / period;

            // Calculate mean deviation
            double mdSum = 0;
            for (int j = 0; j < period; j++)
                mdSum += Math.Abs(candles[i - j].TypicalPrice - sma);
            double meanDeviation = mdSum / period;

            cci[i] = meanDeviation != 0 ? (candles[i].TypicalPrice - sma) / (0.015 * meanDeviation) : 0;
        }
    }
}
