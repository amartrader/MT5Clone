using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Oscillators;

public class MomentumIndicator : IndicatorBase
{
    public override string Name => "Momentum";
    public override string ShortName => $"Mom({GetParameter("Period", 14)})";
    public override IndicatorType Type => IndicatorType.Momentum;
    public override bool IsOverlay => false;
    public override int RequiredBars => GetParameter("Period", 14) + 1;

    public MomentumIndicator(int period = 14, AppliedPrice appliedPrice = AppliedPrice.Close)
    {
        Parameters["Period"] = period;
        Parameters["AppliedPrice"] = appliedPrice;
        AddBuffer("Momentum", $"Momentum({period})", "#00BFFF");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int period = GetParameter("Period", 14);
        var appliedPrice = GetParameter("AppliedPrice", AppliedPrice.Close);

        SetBufferSize(candles.Count);
        var momentum = Buffers[0].Data;

        for (int i = 0; i < candles.Count; i++)
        {
            if (i < period)
            {
                momentum[i] = double.NaN;
                continue;
            }

            double currentPrice = GetAppliedPrice(candles[i], appliedPrice);
            double previousPrice = GetAppliedPrice(candles[i - period], appliedPrice);

            momentum[i] = previousPrice != 0 ? (currentPrice / previousPrice) * 100 : 100;
        }
    }
}
