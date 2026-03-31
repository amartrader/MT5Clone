using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using Xunit;

namespace MT5Clone.Tests.Core;

public class CandleTests
{
    [Fact]
    public void IsBullish_WhenCloseGreaterThanOpen_ReturnsTrue()
    {
        var candle = new Candle { Open = 1.0, Close = 1.5, High = 2.0, Low = 0.5 };
        Assert.True(candle.IsBullish);
        Assert.False(candle.IsBearish);
    }

    [Fact]
    public void IsBearish_WhenCloseLessThanOpen_ReturnsTrue()
    {
        var candle = new Candle { Open = 1.5, Close = 1.0, High = 2.0, Low = 0.5 };
        Assert.True(candle.IsBearish);
        Assert.False(candle.IsBullish);
    }

    [Fact]
    public void IsBullish_WhenCloseEqualsOpen_ReturnsTrue()
    {
        var candle = new Candle { Open = 1.0, Close = 1.0, High = 2.0, Low = 0.5 };
        Assert.True(candle.IsBullish);
    }

    [Fact]
    public void Body_ReturnsAbsoluteDifference()
    {
        var bullish = new Candle { Open = 1.0, Close = 1.5 };
        var bearish = new Candle { Open = 1.5, Close = 1.0 };
        Assert.Equal(0.5, bullish.Body);
        Assert.Equal(0.5, bearish.Body);
    }

    [Fact]
    public void Shadows_CalculatedCorrectly()
    {
        var candle = new Candle { Open = 1.2, Close = 1.4, High = 1.6, Low = 1.0 };
        Assert.Equal(0.2, candle.UpperShadow, 10);
        Assert.Equal(0.2, candle.LowerShadow, 10);
    }

    [Fact]
    public void Range_ReturnsHighMinusLow()
    {
        var candle = new Candle { High = 2.0, Low = 0.5 };
        Assert.Equal(1.5, candle.Range);
    }

    [Fact]
    public void MedianPrice_IsAvgOfHighLow()
    {
        var candle = new Candle { High = 2.0, Low = 1.0 };
        Assert.Equal(1.5, candle.MedianPrice);
    }

    [Fact]
    public void TypicalPrice_IsAvgOfHighLowClose()
    {
        var candle = new Candle { High = 3.0, Low = 1.0, Close = 2.0 };
        Assert.Equal(2.0, candle.TypicalPrice);
    }

    [Fact]
    public void WeightedClose_CalculatedCorrectly()
    {
        var candle = new Candle { High = 4.0, Low = 2.0, Close = 3.0 };
        // (4 + 2 + 3 + 3) / 4 = 3.0
        Assert.Equal(3.0, candle.WeightedClose);
    }

    [Fact]
    public void Clone_CreatesExactCopy()
    {
        var original = new Candle
        {
            Time = new DateTime(2024, 1, 1),
            Open = 1.0, High = 2.0, Low = 0.5, Close = 1.5,
            TickVolume = 100, RealVolume = 200, Spread = 5,
            TimeFrame = TimeFrame.H1
        };

        var clone = original.Clone();

        Assert.Equal(original.Time, clone.Time);
        Assert.Equal(original.Open, clone.Open);
        Assert.Equal(original.High, clone.High);
        Assert.Equal(original.Low, clone.Low);
        Assert.Equal(original.Close, clone.Close);
        Assert.Equal(original.TickVolume, clone.TickVolume);
        Assert.Equal(original.RealVolume, clone.RealVolume);
        Assert.Equal(original.Spread, clone.Spread);
        Assert.Equal(original.TimeFrame, clone.TimeFrame);
    }

    [Fact]
    public void Clone_IsNotSameReference()
    {
        var original = new Candle { Open = 1.0, Close = 1.5 };
        var clone = original.Clone();
        Assert.NotSame(original, clone);
    }
}
