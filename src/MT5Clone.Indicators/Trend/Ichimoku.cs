using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Trend;

public class Ichimoku : IndicatorBase
{
    public override string Name => "Ichimoku Kinko Hyo";
    public override string ShortName => $"Ichimoku({GetParameter("Tenkan", 9)},{GetParameter("Kijun", 26)},{GetParameter("Senkou", 52)})";
    public override IndicatorType Type => IndicatorType.Ichimoku;
    public override bool IsOverlay => true;
    public override int RequiredBars => GetParameter("Senkou", 52);

    public Ichimoku(int tenkanPeriod = 9, int kijunPeriod = 26, int senkouPeriod = 52)
    {
        Parameters["Tenkan"] = tenkanPeriod;
        Parameters["Kijun"] = kijunPeriod;
        Parameters["Senkou"] = senkouPeriod;
        AddBuffer("Tenkan", "Tenkan-sen", "#FF0000");
        AddBuffer("Kijun", "Kijun-sen", "#0000FF");
        AddBuffer("SenkouA", "Senkou Span A", "#00FF00");
        AddBuffer("SenkouB", "Senkou Span B", "#FF00FF");
        AddBuffer("Chikou", "Chikou Span", "#808080");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        int tenkanPeriod = GetParameter("Tenkan", 9);
        int kijunPeriod = GetParameter("Kijun", 26);
        int senkouPeriod = GetParameter("Senkou", 52);

        int totalSize = candles.Count + kijunPeriod;
        SetBufferSize(totalSize);

        var tenkan = Buffers[0].Data;
        var kijun = Buffers[1].Data;
        var senkouA = Buffers[2].Data;
        var senkouB = Buffers[3].Data;
        var chikou = Buffers[4].Data;

        for (int i = 0; i < candles.Count; i++)
        {
            // Tenkan-sen
            if (i >= tenkanPeriod - 1)
            {
                double high = double.MinValue, low = double.MaxValue;
                for (int j = 0; j < tenkanPeriod; j++)
                {
                    high = Math.Max(high, candles[i - j].High);
                    low = Math.Min(low, candles[i - j].Low);
                }
                tenkan[i] = (high + low) / 2;
            }
            else tenkan[i] = double.NaN;

            // Kijun-sen
            if (i >= kijunPeriod - 1)
            {
                double high = double.MinValue, low = double.MaxValue;
                for (int j = 0; j < kijunPeriod; j++)
                {
                    high = Math.Max(high, candles[i - j].High);
                    low = Math.Min(low, candles[i - j].Low);
                }
                kijun[i] = (high + low) / 2;
            }
            else kijun[i] = double.NaN;

            // Senkou Span A (shifted forward by kijunPeriod)
            if (!double.IsNaN(tenkan[i]) && !double.IsNaN(kijun[i]))
            {
                int shiftedIndex = i + kijunPeriod;
                if (shiftedIndex < totalSize)
                    senkouA[shiftedIndex] = (tenkan[i] + kijun[i]) / 2;
            }

            // Senkou Span B (shifted forward by kijunPeriod)
            if (i >= senkouPeriod - 1)
            {
                double high = double.MinValue, low = double.MaxValue;
                for (int j = 0; j < senkouPeriod; j++)
                {
                    high = Math.Max(high, candles[i - j].High);
                    low = Math.Min(low, candles[i - j].Low);
                }
                int shiftedIndex = i + kijunPeriod;
                if (shiftedIndex < totalSize)
                    senkouB[shiftedIndex] = (high + low) / 2;
            }

            // Chikou Span (shifted back by kijunPeriod)
            int chikouIndex = i - kijunPeriod;
            if (chikouIndex >= 0)
                chikou[chikouIndex] = candles[i].Close;
        }
    }
}
