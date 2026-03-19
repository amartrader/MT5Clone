using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Trend;

public class MovingAverage : IndicatorBase
{
    public override string Name => "Moving Average";
    public override string ShortName => $"MA({GetParameter("Period", 14)})";
    public override IndicatorType Type => IndicatorType.MovingAverage;
    public override bool IsOverlay => true;
    public override int RequiredBars => GetParameter("Period", 14);

    public MovingAverage(int period = 14, MovingAverageMethod method = MovingAverageMethod.Simple,
        AppliedPrice appliedPrice = AppliedPrice.Close, string color = "#FFFF00")
    {
        Parameters["Period"] = period;
        Parameters["Method"] = method;
        Parameters["AppliedPrice"] = appliedPrice;
        AddBuffer("MA", $"MA({period})", color);
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int period = GetParameter("Period", 14);
        var method = GetParameter("Method", MovingAverageMethod.Simple);
        var appliedPrice = GetParameter("AppliedPrice", AppliedPrice.Close);

        SetBufferSize(candles.Count);
        var buffer = Buffers[0].Data;

        switch (method)
        {
            case MovingAverageMethod.Simple:
                CalculateSMA(candles, buffer, period, appliedPrice);
                break;
            case MovingAverageMethod.Exponential:
                CalculateEMA(candles, buffer, period, appliedPrice);
                break;
            case MovingAverageMethod.Smoothed:
                CalculateSmoothed(candles, buffer, period, appliedPrice);
                break;
            case MovingAverageMethod.LinearWeighted:
                CalculateLWMA(candles, buffer, period, appliedPrice);
                break;
        }
    }

    private void CalculateSMA(IReadOnlyList<Candle> candles, List<double> buffer, int period, AppliedPrice appliedPrice)
    {
        for (int i = 0; i < candles.Count; i++)
        {
            if (i < period - 1)
            {
                buffer[i] = double.NaN;
                continue;
            }

            double sum = 0;
            for (int j = 0; j < period; j++)
                sum += GetAppliedPrice(candles[i - j], appliedPrice);

            buffer[i] = sum / period;
        }
    }

    private void CalculateEMA(IReadOnlyList<Candle> candles, List<double> buffer, int period, AppliedPrice appliedPrice)
    {
        double multiplier = 2.0 / (period + 1);

        // First EMA value is SMA
        double sum = 0;
        for (int i = 0; i < period && i < candles.Count; i++)
        {
            sum += GetAppliedPrice(candles[i], appliedPrice);
            buffer[i] = double.NaN;
        }

        if (candles.Count >= period)
        {
            buffer[period - 1] = sum / period;

            for (int i = period; i < candles.Count; i++)
            {
                double price = GetAppliedPrice(candles[i], appliedPrice);
                buffer[i] = (price - buffer[i - 1]) * multiplier + buffer[i - 1];
            }
        }
    }

    private void CalculateSmoothed(IReadOnlyList<Candle> candles, List<double> buffer, int period, AppliedPrice appliedPrice)
    {
        double sum = 0;
        for (int i = 0; i < period && i < candles.Count; i++)
        {
            sum += GetAppliedPrice(candles[i], appliedPrice);
            buffer[i] = double.NaN;
        }

        if (candles.Count >= period)
        {
            buffer[period - 1] = sum / period;

            for (int i = period; i < candles.Count; i++)
            {
                double price = GetAppliedPrice(candles[i], appliedPrice);
                buffer[i] = (buffer[i - 1] * (period - 1) + price) / period;
            }
        }
    }

    private void CalculateLWMA(IReadOnlyList<Candle> candles, List<double> buffer, int period, AppliedPrice appliedPrice)
    {
        int weightSum = period * (period + 1) / 2;

        for (int i = 0; i < candles.Count; i++)
        {
            if (i < period - 1)
            {
                buffer[i] = double.NaN;
                continue;
            }

            double sum = 0;
            for (int j = 0; j < period; j++)
            {
                sum += GetAppliedPrice(candles[i - j], appliedPrice) * (period - j);
            }

            buffer[i] = sum / weightSum;
        }
    }

    public static double[] CalculateMAValues(IReadOnlyList<double> prices, int period, MovingAverageMethod method)
    {
        var result = new double[prices.Count];
        Array.Fill(result, double.NaN);

        if (prices.Count < period) return result;

        switch (method)
        {
            case MovingAverageMethod.Simple:
                for (int i = period - 1; i < prices.Count; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < period; j++) sum += prices[i - j];
                    result[i] = sum / period;
                }
                break;

            case MovingAverageMethod.Exponential:
                double mult = 2.0 / (period + 1);
                double smaSum = 0;
                for (int i = 0; i < period; i++) smaSum += prices[i];
                result[period - 1] = smaSum / period;
                for (int i = period; i < prices.Count; i++)
                    result[i] = (prices[i] - result[i - 1]) * mult + result[i - 1];
                break;
        }

        return result;
    }
}
