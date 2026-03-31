using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Oscillators;
using Xunit;

namespace MT5Clone.Tests.Indicators;

public class RSITests
{
    private static List<Candle> CreateTrendingCandles(int count, double startPrice, double step)
    {
        var candles = new List<Candle>();
        for (int i = 0; i < count; i++)
        {
            double close = startPrice + step * i;
            candles.Add(new Candle
            {
                Time = DateTime.UtcNow.AddHours(-count + i),
                Open = close - step * 0.5,
                High = close + Math.Abs(step) * 0.3,
                Low = close - Math.Abs(step) * 0.3,
                Close = close,
                TickVolume = 100,
                TimeFrame = TimeFrame.H1
            });
        }
        return candles;
    }

    [Fact]
    public void Calculate_WithSufficientData_ProducesBufferValues()
    {
        var rsi = new RSI();
        var candles = CreateTrendingCandles(30, 1.08000, 0.00010);

        rsi.Calculate(candles);

        Assert.True(rsi.Buffers.Count > 0);
        Assert.Equal(candles.Count, rsi.Buffers[0].Data.Count);
    }

    [Fact]
    public void Calculate_WithEmptyData_DoesNotThrow()
    {
        var rsi = new RSI();
        rsi.Calculate(new List<Candle>());
    }

    [Fact]
    public void Name_ContainsStrengthIndex()
    {
        var rsi = new RSI();
        Assert.Contains("Strength Index", rsi.Name);
    }

    [Fact]
    public void IsOverlay_IsFalse()
    {
        var rsi = new RSI();
        Assert.False(rsi.IsOverlay);
    }

    [Fact]
    public void RSI_InStrongUptrend_IsHigh()
    {
        var rsi = new RSI();
        var candles = CreateTrendingCandles(30, 1.08000, 0.00100);

        rsi.Calculate(candles);

        var lastValue = rsi.Buffers[0].Data.Last();
        if (!double.IsNaN(lastValue))
        {
            // Strong uptrend should produce high RSI
            Assert.True(lastValue > 50, $"RSI in strong uptrend should be > 50, got {lastValue}");
        }
    }
}
