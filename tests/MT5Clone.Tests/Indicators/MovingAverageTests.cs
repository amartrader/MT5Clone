using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Trend;
using Xunit;

namespace MT5Clone.Tests.Indicators;

public class MovingAverageTests
{
    private static List<Candle> CreateCandles(params double[] closes)
    {
        return closes.Select((c, i) => new Candle
        {
            Time = DateTime.UtcNow.AddHours(-closes.Length + i),
            Open = c - 0.001,
            High = c + 0.002,
            Low = c - 0.002,
            Close = c,
            TickVolume = 100,
            TimeFrame = TimeFrame.H1
        }).ToList();
    }

    [Fact]
    public void Calculate_WithSufficientData_ProducesBufferValues()
    {
        var ma = new MovingAverage(period: 5);
        var candles = CreateCandles(1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9,
            2.0, 2.1, 2.2, 2.3, 2.4);

        ma.Calculate(candles);

        Assert.True(ma.Buffers.Count > 0);
        Assert.Equal(candles.Count, ma.Buffers[0].Data.Count);
        // First few values should be NaN (less than period)
        Assert.True(double.IsNaN(ma.Buffers[0].Data[0]));
        // Later values should be valid
        Assert.False(double.IsNaN(ma.Buffers[0].Data[candles.Count - 1]));
    }

    [Fact]
    public void Calculate_SMA_CorrectValues()
    {
        var ma = new MovingAverage(period: 3);
        var candles = CreateCandles(1.0, 2.0, 3.0, 4.0, 5.0);

        ma.Calculate(candles);

        // SMA(3) at index 2 = (1+2+3)/3 = 2.0
        Assert.Equal(2.0, ma.Buffers[0].Data[2], 5);
        // SMA(3) at index 3 = (2+3+4)/3 = 3.0
        Assert.Equal(3.0, ma.Buffers[0].Data[3], 5);
        // SMA(3) at index 4 = (3+4+5)/3 = 4.0
        Assert.Equal(4.0, ma.Buffers[0].Data[4], 5);
    }

    [Fact]
    public void Calculate_WithEmptyData_DoesNotThrow()
    {
        var ma = new MovingAverage();
        var candles = new List<Candle>();
        ma.Calculate(candles);
    }

    [Fact]
    public void Name_IsMovingAverage()
    {
        var ma = new MovingAverage();
        Assert.Equal("Moving Average", ma.Name);
    }

    [Fact]
    public void IsOverlay_IsTrue()
    {
        var ma = new MovingAverage();
        Assert.True(ma.IsOverlay);
    }

    [Fact]
    public void Type_IsMovingAverage()
    {
        var ma = new MovingAverage();
        Assert.Equal(IndicatorType.MovingAverage, ma.Type);
    }

    [Fact]
    public void SetParameter_ChangesPeriod()
    {
        var ma = new MovingAverage(period: 14);
        ma.SetParameter("Period", 20);
        Assert.Equal(20, ma.Parameters["Period"]);
    }

    [Fact]
    public void Clone_CreatesNewInstance()
    {
        var ma = new MovingAverage(period: 14);
        var clone = ma.Clone();
        Assert.NotSame(ma, clone);
        Assert.Equal(ma.Name, clone.Name);
    }
}
