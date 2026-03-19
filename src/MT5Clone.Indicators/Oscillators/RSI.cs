using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Oscillators;

public class RSI : IndicatorBase
{
    public override string Name => "Relative Strength Index";
    public override string ShortName => $"RSI({GetParameter("Period", 14)})";
    public override IndicatorType Type => IndicatorType.RSI;
    public override bool IsOverlay => false;
    public override int RequiredBars => GetParameter("Period", 14) + 1;

    public RSI(int period = 14, AppliedPrice appliedPrice = AppliedPrice.Close)
    {
        Parameters["Period"] = period;
        Parameters["AppliedPrice"] = appliedPrice;
        AddBuffer("RSI", $"RSI({period})", "#00FFFF");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int period = GetParameter("Period", 14);
        var appliedPrice = GetParameter("AppliedPrice", AppliedPrice.Close);

        SetBufferSize(candles.Count);
        var rsi = Buffers[0].Data;

        if (candles.Count < period + 1) return;

        double gainSum = 0, lossSum = 0;
        for (int i = 1; i <= period; i++)
        {
            double change = GetAppliedPrice(candles[i], appliedPrice) - GetAppliedPrice(candles[i - 1], appliedPrice);
            if (change > 0) gainSum += change;
            else lossSum += Math.Abs(change);
        }

        double avgGain = gainSum / period;
        double avgLoss = lossSum / period;

        rsi[period] = avgLoss == 0 ? 100 : 100 - (100 / (1 + avgGain / avgLoss));

        for (int i = period + 1; i < candles.Count; i++)
        {
            double change = GetAppliedPrice(candles[i], appliedPrice) - GetAppliedPrice(candles[i - 1], appliedPrice);
            double gain = change > 0 ? change : 0;
            double loss = change < 0 ? Math.Abs(change) : 0;

            avgGain = (avgGain * (period - 1) + gain) / period;
            avgLoss = (avgLoss * (period - 1) + loss) / period;

            rsi[i] = avgLoss == 0 ? 100 : 100 - (100 / (1 + avgGain / avgLoss));
        }

        for (int i = 0; i < period; i++)
            rsi[i] = double.NaN;
    }
}
