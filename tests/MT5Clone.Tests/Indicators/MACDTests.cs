using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Oscillators;
using Xunit;

namespace MT5Clone.Tests.Indicators;

public class MACDTests
{
    private static List<Candle> CreateCandles(int count)
    {
        var random = new Random(42);
        var candles = new List<Candle>();
        double price = 1.08500;

        for (int i = 0; i < count; i++)
        {
            double change = (random.NextDouble() - 0.5) * 0.001;
            price += change;
            candles.Add(new Candle
            {
                Time = DateTime.UtcNow.AddHours(-count + i),
                Open = price - change * 0.5,
                High = price + 0.001,
                Low = price - 0.001,
                Close = price,
                TickVolume = 100,
                TimeFrame = TimeFrame.H1
            });
        }
        return candles;
    }

    [Fact]
    public void Calculate_WithSufficientData_ProducesBufferValues()
    {
        var macd = new MACD();
        var candles = CreateCandles(50);

        macd.Calculate(candles);

        Assert.True(macd.Buffers.Count > 0);
        Assert.Equal(candles.Count, macd.Buffers[0].Data.Count);
    }

    [Fact]
    public void Calculate_WithEmptyData_DoesNotThrow()
    {
        var macd = new MACD();
        macd.Calculate(new List<Candle>());
    }

    [Fact]
    public void Name_ContainsMACD()
    {
        var macd = new MACD();
        Assert.Contains("MACD", macd.Name);
    }

    [Fact]
    public void IsOverlay_IsFalse()
    {
        var macd = new MACD();
        Assert.False(macd.IsOverlay);
    }

    [Fact]
    public void HasMultipleBuffers()
    {
        var macd = new MACD();
        var candles = CreateCandles(50);
        macd.Calculate(candles);

        // MACD typically has MACD line, signal line, and histogram
        Assert.True(macd.Buffers.Count >= 2);
    }
}
