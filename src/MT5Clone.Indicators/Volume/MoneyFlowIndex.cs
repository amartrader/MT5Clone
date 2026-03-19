using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Volume;

public class MoneyFlowIndex : IndicatorBase
{
    public override string Name => "Money Flow Index";
    public override string ShortName => $"MFI({GetParameter("Period", 14)})";
    public override IndicatorType Type => IndicatorType.MoneyFlowIndex;
    public override bool IsOverlay => false;
    public override int RequiredBars => GetParameter("Period", 14) + 1;

    public MoneyFlowIndex(int period = 14)
    {
        Parameters["Period"] = period;
        AddBuffer("MFI", $"MFI({period})", "#FF8C00");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int period = GetParameter("Period", 14);
        SetBufferSize(candles.Count);
        var mfi = Buffers[0].Data;

        if (candles.Count < period + 1) return;

        for (int i = period; i < candles.Count; i++)
        {
            double positiveFlow = 0;
            double negativeFlow = 0;

            for (int j = 1; j <= period; j++)
            {
                double typicalPrice = candles[i - period + j].TypicalPrice;
                double prevTypicalPrice = candles[i - period + j - 1].TypicalPrice;
                double moneyFlow = typicalPrice * candles[i - period + j].TickVolume;

                if (typicalPrice > prevTypicalPrice)
                    positiveFlow += moneyFlow;
                else if (typicalPrice < prevTypicalPrice)
                    negativeFlow += moneyFlow;
            }

            mfi[i] = negativeFlow > 0
                ? 100 - (100 / (1 + positiveFlow / negativeFlow))
                : 100;
        }

        for (int i = 0; i < period; i++)
            mfi[i] = double.NaN;
    }
}
