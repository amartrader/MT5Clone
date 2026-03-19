using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Trend;

public class ParabolicSAR : IndicatorBase
{
    public override string Name => "Parabolic SAR";
    public override string ShortName => $"SAR({GetParameter("Step", 0.02)},{GetParameter("Maximum", 0.2)})";
    public override IndicatorType Type => IndicatorType.ParabolicSAR;
    public override bool IsOverlay => true;
    public override int RequiredBars => 2;

    public ParabolicSAR(double step = 0.02, double maximum = 0.2)
    {
        Parameters["Step"] = step;
        Parameters["Maximum"] = maximum;
        AddBuffer("SAR", "SAR", "#00FFFF", IndicatorBufferStyle.Dots, 2);
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        double step = GetParameter("Step", 0.02);
        double maximum = GetParameter("Maximum", 0.2);

        SetBufferSize(candles.Count);
        var sar = Buffers[0].Data;

        if (candles.Count < 2) return;

        bool isLong = candles[1].Close > candles[0].Close;
        double af = step;
        double ep = isLong ? candles[0].High : candles[0].Low;
        double sarValue = isLong ? candles[0].Low : candles[0].High;

        sar[0] = sarValue;

        for (int i = 1; i < candles.Count; i++)
        {
            double prevSar = sarValue;

            if (isLong)
            {
                sarValue = prevSar + af * (ep - prevSar);
                sarValue = Math.Min(sarValue, candles[i - 1].Low);
                if (i >= 2) sarValue = Math.Min(sarValue, candles[i - 2].Low);

                if (candles[i].Low < sarValue)
                {
                    isLong = false;
                    sarValue = ep;
                    ep = candles[i].Low;
                    af = step;
                }
                else
                {
                    if (candles[i].High > ep)
                    {
                        ep = candles[i].High;
                        af = Math.Min(af + step, maximum);
                    }
                }
            }
            else
            {
                sarValue = prevSar + af * (ep - prevSar);
                sarValue = Math.Max(sarValue, candles[i - 1].High);
                if (i >= 2) sarValue = Math.Max(sarValue, candles[i - 2].High);

                if (candles[i].High > sarValue)
                {
                    isLong = true;
                    sarValue = ep;
                    ep = candles[i].High;
                    af = step;
                }
                else
                {
                    if (candles[i].Low < ep)
                    {
                        ep = candles[i].Low;
                        af = Math.Min(af + step, maximum);
                    }
                }
            }

            sar[i] = sarValue;
        }
    }
}
