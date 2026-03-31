using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Trend;
using Xunit;

namespace MT5Clone.Tests.Indicators;

public class BollingerBandsTests
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
        var bb = new BollingerBands();
        var candles = CreateCandles(50);

        bb.Calculate(candles);

        Assert.True(bb.Buffers.Count > 0);
        Assert.Equal(candles.Count, bb.Buffers[0].Data.Count);
    }

    [Fact]
    public void Calculate_UpperBandAboveMiddle()
    {
        var bb = new BollingerBands();
        var candles = CreateCandles(50);
        bb.Calculate(candles);

        // Bollinger Bands have 3 buffers: Upper=0, Middle=1, Lower=2
        if (bb.Buffers.Count >= 3)
        {
            var lastIdx = candles.Count - 1;
            var upper = bb.Buffers[0].Data[lastIdx];
            var middle = bb.Buffers[1].Data[lastIdx];
            var lower = bb.Buffers[2].Data[lastIdx];

            if (!double.IsNaN(middle) && !double.IsNaN(upper) && !double.IsNaN(lower))
            {
                Assert.True(upper >= middle, "Upper band should be >= middle band");
                Assert.True(lower <= middle, "Lower band should be <= middle band");
            }
        }
    }

    [Fact]
    public void Calculate_WithEmptyData_DoesNotThrow()
    {
        var bb = new BollingerBands();
        bb.Calculate(new List<Candle>());
    }

    [Fact]
    public void Name_ContainsBollinger()
    {
        var bb = new BollingerBands();
        Assert.Contains("Bollinger", bb.Name);
    }

    [Fact]
    public void IsOverlay_IsTrue()
    {
        var bb = new BollingerBands();
        Assert.True(bb.IsOverlay);
    }
}
