using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Oscillators;

public class Stochastic : IndicatorBase
{
    public override string Name => "Stochastic Oscillator";
    public override string ShortName => $"Stoch({GetParameter("KPeriod", 5)},{GetParameter("DPeriod", 3)},{GetParameter("Slowing", 3)})";
    public override IndicatorType Type => IndicatorType.Stochastic;
    public override bool IsOverlay => false;
    public override int RequiredBars => GetParameter("KPeriod", 5) + GetParameter("DPeriod", 3) + GetParameter("Slowing", 3);

    public Stochastic(int kPeriod = 5, int dPeriod = 3, int slowing = 3)
    {
        Parameters["KPeriod"] = kPeriod;
        Parameters["DPeriod"] = dPeriod;
        Parameters["Slowing"] = slowing;
        AddBuffer("K", "%K", "#00BFFF");
        AddBuffer("D", "%D", "#FF0000");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int kPeriod = GetParameter("KPeriod", 5);
        int dPeriod = GetParameter("DPeriod", 3);
        int slowing = GetParameter("Slowing", 3);

        SetBufferSize(candles.Count);
        var kLine = Buffers[0].Data;
        var dLine = Buffers[1].Data;

        int startIdx = kPeriod + slowing - 2;
        if (candles.Count <= startIdx) return;

        // Calculate raw %K
        var rawK = new double[candles.Count];
        for (int i = kPeriod - 1; i < candles.Count; i++)
        {
            double highest = double.MinValue;
            double lowest = double.MaxValue;
            for (int j = 0; j < kPeriod; j++)
            {
                highest = Math.Max(highest, candles[i - j].High);
                lowest = Math.Min(lowest, candles[i - j].Low);
            }
            double range = highest - lowest;
            rawK[i] = range > 0 ? ((candles[i].Close - lowest) / range) * 100 : 50;
        }

        // Apply slowing (SMA of raw %K)
        for (int i = startIdx; i < candles.Count; i++)
        {
            double sum = 0;
            for (int j = 0; j < slowing; j++)
                sum += rawK[i - j];
            kLine[i] = sum / slowing;
        }

        // %D = SMA of %K
        for (int i = startIdx + dPeriod - 1; i < candles.Count; i++)
        {
            double sum = 0;
            for (int j = 0; j < dPeriod; j++)
                sum += kLine[i - j];
            dLine[i] = sum / dPeriod;
        }

        // Fill NaN
        for (int i = 0; i < startIdx; i++)
            kLine[i] = double.NaN;
        for (int i = 0; i < startIdx + dPeriod - 1; i++)
            dLine[i] = double.NaN;
    }
}
