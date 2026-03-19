using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Trend;

public class ADX : IndicatorBase
{
    public override string Name => "Average Directional Index";
    public override string ShortName => $"ADX({GetParameter("Period", 14)})";
    public override IndicatorType Type => IndicatorType.ADX;
    public override bool IsOverlay => false;
    public override int RequiredBars => GetParameter("Period", 14) * 2;

    public ADX(int period = 14)
    {
        Parameters["Period"] = period;
        AddBuffer("ADX", "ADX", "#FFFF00");
        AddBuffer("+DI", "+DI", "#00FF00");
        AddBuffer("-DI", "-DI", "#FF0000");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int period = GetParameter("Period", 14);
        SetBufferSize(candles.Count);

        var adx = Buffers[0].Data;
        var pdi = Buffers[1].Data;
        var mdi = Buffers[2].Data;

        if (candles.Count < period + 1) return;

        var tr = new double[candles.Count];
        var plusDM = new double[candles.Count];
        var minusDM = new double[candles.Count];

        for (int i = 1; i < candles.Count; i++)
        {
            double highDiff = candles[i].High - candles[i - 1].High;
            double lowDiff = candles[i - 1].Low - candles[i].Low;

            plusDM[i] = highDiff > lowDiff && highDiff > 0 ? highDiff : 0;
            minusDM[i] = lowDiff > highDiff && lowDiff > 0 ? lowDiff : 0;

            tr[i] = Math.Max(candles[i].High - candles[i].Low,
                Math.Max(Math.Abs(candles[i].High - candles[i - 1].Close),
                         Math.Abs(candles[i].Low - candles[i - 1].Close)));
        }

        // Smoothed TR, +DM, -DM
        var smoothTR = new double[candles.Count];
        var smoothPlusDM = new double[candles.Count];
        var smoothMinusDM = new double[candles.Count];

        double sumTR = 0, sumPlusDM = 0, sumMinusDM = 0;
        for (int i = 1; i <= period; i++)
        {
            sumTR += tr[i];
            sumPlusDM += plusDM[i];
            sumMinusDM += minusDM[i];
        }

        smoothTR[period] = sumTR;
        smoothPlusDM[period] = sumPlusDM;
        smoothMinusDM[period] = sumMinusDM;

        for (int i = period + 1; i < candles.Count; i++)
        {
            smoothTR[i] = smoothTR[i - 1] - smoothTR[i - 1] / period + tr[i];
            smoothPlusDM[i] = smoothPlusDM[i - 1] - smoothPlusDM[i - 1] / period + plusDM[i];
            smoothMinusDM[i] = smoothMinusDM[i - 1] - smoothMinusDM[i - 1] / period + minusDM[i];
        }

        // +DI and -DI
        var dx = new double[candles.Count];
        for (int i = period; i < candles.Count; i++)
        {
            pdi[i] = smoothTR[i] != 0 ? 100 * smoothPlusDM[i] / smoothTR[i] : 0;
            mdi[i] = smoothTR[i] != 0 ? 100 * smoothMinusDM[i] / smoothTR[i] : 0;

            double diSum = pdi[i] + mdi[i];
            dx[i] = diSum != 0 ? 100 * Math.Abs(pdi[i] - mdi[i]) / diSum : 0;
        }

        // ADX
        if (candles.Count >= period * 2)
        {
            double sumDX = 0;
            for (int i = period; i < period * 2; i++)
                sumDX += dx[i];
            adx[period * 2 - 1] = sumDX / period;

            for (int i = period * 2; i < candles.Count; i++)
                adx[i] = (adx[i - 1] * (period - 1) + dx[i]) / period;
        }

        // Fill NaN for initial values
        for (int i = 0; i < period; i++)
        {
            adx[i] = pdi[i] = mdi[i] = double.NaN;
        }
        for (int i = period; i < Math.Min(period * 2 - 1, candles.Count); i++)
        {
            adx[i] = double.NaN;
        }
    }
}
