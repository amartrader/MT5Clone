using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Oscillators;

public class MACD : IndicatorBase
{
    public override string Name => "MACD";
    public override string ShortName => $"MACD({GetParameter("FastPeriod", 12)},{GetParameter("SlowPeriod", 26)},{GetParameter("SignalPeriod", 9)})";
    public override IndicatorType Type => IndicatorType.MACD;
    public override bool IsOverlay => false;
    public override int RequiredBars => GetParameter("SlowPeriod", 26) + GetParameter("SignalPeriod", 9);

    public MACD(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9, AppliedPrice appliedPrice = AppliedPrice.Close)
    {
        Parameters["FastPeriod"] = fastPeriod;
        Parameters["SlowPeriod"] = slowPeriod;
        Parameters["SignalPeriod"] = signalPeriod;
        Parameters["AppliedPrice"] = appliedPrice;
        AddBuffer("MACD", "MACD", "#00BFFF", IndicatorBufferStyle.Histogram);
        AddBuffer("Signal", "Signal", "#FF0000");
        AddBuffer("Histogram", "Histogram", "#808080", IndicatorBufferStyle.Histogram);
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int fastPeriod = GetParameter("FastPeriod", 12);
        int slowPeriod = GetParameter("SlowPeriod", 26);
        int signalPeriod = GetParameter("SignalPeriod", 9);
        var appliedPrice = GetParameter("AppliedPrice", AppliedPrice.Close);

        SetBufferSize(candles.Count);
        var macdLine = Buffers[0].Data;
        var signalLine = Buffers[1].Data;
        var histogram = Buffers[2].Data;

        if (candles.Count < slowPeriod) return;

        // Calculate prices
        var prices = candles.Select(c => GetAppliedPrice(c, appliedPrice)).ToList();

        // Calculate fast and slow EMA
        var fastEMA = CalculateEMA(prices, fastPeriod);
        var slowEMA = CalculateEMA(prices, slowPeriod);

        // MACD line
        for (int i = 0; i < candles.Count; i++)
        {
            if (double.IsNaN(fastEMA[i]) || double.IsNaN(slowEMA[i]))
            {
                macdLine[i] = double.NaN;
            }
            else
            {
                macdLine[i] = fastEMA[i] - slowEMA[i];
            }
        }

        // Signal line (EMA of MACD)
        var validMacd = macdLine.Where(v => !double.IsNaN(v)).ToList();
        if (validMacd.Count >= signalPeriod)
        {
            int startIndex = macdLine.FindIndex(v => !double.IsNaN(v));
            var signalEMA = CalculateEMA(validMacd, signalPeriod);

            int j = 0;
            for (int i = startIndex; i < candles.Count; i++)
            {
                if (!double.IsNaN(macdLine[i]) && j < signalEMA.Length)
                {
                    signalLine[i] = signalEMA[j];
                    histogram[i] = double.IsNaN(signalEMA[j]) ? double.NaN : macdLine[i] - signalEMA[j];
                    j++;
                }
                else
                {
                    signalLine[i] = double.NaN;
                    histogram[i] = double.NaN;
                }
            }
        }

        // Fill NaN
        for (int i = 0; i < slowPeriod - 1; i++)
        {
            macdLine[i] = signalLine[i] = histogram[i] = double.NaN;
        }
    }

    private double[] CalculateEMA(IReadOnlyList<double> data, int period)
    {
        var result = new double[data.Count];
        Array.Fill(result, double.NaN);

        if (data.Count < period) return result;

        double sum = 0;
        for (int i = 0; i < period; i++) sum += data[i];
        result[period - 1] = sum / period;

        double multiplier = 2.0 / (period + 1);
        for (int i = period; i < data.Count; i++)
        {
            result[i] = (data[i] - result[i - 1]) * multiplier + result[i - 1];
        }

        return result;
    }
}

internal static class ListExtensions
{
    public static int FindIndex(this List<double> list, Predicate<double> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i])) return i;
        }
        return -1;
    }
}
