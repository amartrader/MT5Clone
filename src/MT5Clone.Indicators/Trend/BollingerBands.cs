using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Trend;

public class BollingerBands : IndicatorBase
{
    public override string Name => "Bollinger Bands";
    public override string ShortName => $"BB({GetParameter("Period", 20)},{GetParameter("Deviation", 2.0)})";
    public override IndicatorType Type => IndicatorType.BollingerBands;
    public override bool IsOverlay => true;
    public override int RequiredBars => GetParameter("Period", 20);

    public BollingerBands(int period = 20, double deviation = 2.0, AppliedPrice appliedPrice = AppliedPrice.Close)
    {
        Parameters["Period"] = period;
        Parameters["Deviation"] = deviation;
        Parameters["AppliedPrice"] = appliedPrice;
        AddBuffer("Upper", "Upper Band", "#FF6666");
        AddBuffer("Middle", "Middle Band", "#FFFF00");
        AddBuffer("Lower", "Lower Band", "#FF6666");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int period = GetParameter("Period", 20);
        double deviation = GetParameter("Deviation", 2.0);
        var appliedPrice = GetParameter("AppliedPrice", AppliedPrice.Close);

        SetBufferSize(candles.Count);
        var upper = Buffers[0].Data;
        var middle = Buffers[1].Data;
        var lower = Buffers[2].Data;

        for (int i = 0; i < candles.Count; i++)
        {
            if (i < period - 1)
            {
                upper[i] = middle[i] = lower[i] = double.NaN;
                continue;
            }

            double sum = 0;
            for (int j = 0; j < period; j++)
                sum += GetAppliedPrice(candles[i - j], appliedPrice);
            double sma = sum / period;

            double sumSquares = 0;
            for (int j = 0; j < period; j++)
            {
                double diff = GetAppliedPrice(candles[i - j], appliedPrice) - sma;
                sumSquares += diff * diff;
            }
            double stdDev = Math.Sqrt(sumSquares / period);

            middle[i] = sma;
            upper[i] = sma + deviation * stdDev;
            lower[i] = sma - deviation * stdDev;
        }
    }
}
