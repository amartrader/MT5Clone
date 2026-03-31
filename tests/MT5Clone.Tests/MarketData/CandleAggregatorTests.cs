using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;
using Xunit;

namespace MT5Clone.Tests.MarketData;

public class CandleAggregatorTests
{
    private readonly CandleAggregator _aggregator = new();

    [Fact]
    public void UpdateCandle_FirstTick_CreatesNewCandle()
    {
        var candles = new List<Candle>();
        var tick = new Tick
        {
            Symbol = "EURUSD",
            Bid = 1.08500,
            Ask = 1.08510,
            Time = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Volume = 5
        };

        bool isNew = _aggregator.UpdateCandle(candles, tick, TimeFrame.M1);

        Assert.True(isNew);
        Assert.Single(candles);
        Assert.Equal(1.08500, candles[0].Open);
        Assert.Equal(1.08500, candles[0].High);
        Assert.Equal(1.08500, candles[0].Low);
        Assert.Equal(1.08500, candles[0].Close);
        Assert.Equal(1, candles[0].TickVolume);
    }

    [Fact]
    public void UpdateCandle_SameMinute_UpdatesExistingCandle()
    {
        var candles = new List<Candle>();
        var time = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        _aggregator.UpdateCandle(candles, new Tick { Bid = 1.08500, Time = time, Volume = 1 }, TimeFrame.M1);
        bool isNew = _aggregator.UpdateCandle(candles, new Tick { Bid = 1.08600, Time = time.AddSeconds(30), Volume = 2 }, TimeFrame.M1);

        Assert.False(isNew);
        Assert.Single(candles);
        Assert.Equal(1.08500, candles[0].Open);
        Assert.Equal(1.08600, candles[0].High);
        Assert.Equal(1.08500, candles[0].Low);
        Assert.Equal(1.08600, candles[0].Close);
        Assert.Equal(2, candles[0].TickVolume);
    }

    [Fact]
    public void UpdateCandle_NewMinute_CreatesNewCandle()
    {
        var candles = new List<Candle>();
        var time = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        _aggregator.UpdateCandle(candles, new Tick { Bid = 1.08500, Time = time, Volume = 1 }, TimeFrame.M1);
        bool isNew = _aggregator.UpdateCandle(candles, new Tick { Bid = 1.08600, Time = time.AddMinutes(1), Volume = 2 }, TimeFrame.M1);

        Assert.True(isNew);
        Assert.Equal(2, candles.Count);
    }

    [Fact]
    public void UpdateCandle_TracksHighLow()
    {
        var candles = new List<Candle>();
        var time = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        _aggregator.UpdateCandle(candles, new Tick { Bid = 1.08500, Time = time }, TimeFrame.M1);
        _aggregator.UpdateCandle(candles, new Tick { Bid = 1.08700, Time = time.AddSeconds(10) }, TimeFrame.M1);
        _aggregator.UpdateCandle(candles, new Tick { Bid = 1.08300, Time = time.AddSeconds(20) }, TimeFrame.M1);
        _aggregator.UpdateCandle(candles, new Tick { Bid = 1.08400, Time = time.AddSeconds(30) }, TimeFrame.M1);

        Assert.Equal(1.08500, candles[0].Open);
        Assert.Equal(1.08700, candles[0].High);
        Assert.Equal(1.08300, candles[0].Low);
        Assert.Equal(1.08400, candles[0].Close);
        Assert.Equal(4, candles[0].TickVolume);
    }

    [Fact]
    public void UpdateCandle_LimitsTo10000Candles()
    {
        var candles = new List<Candle>();
        var baseTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Add 10001 candles
        for (int i = 0; i < 10001; i++)
        {
            var tick = new Tick
            {
                Bid = 1.08500 + i * 0.00001,
                Time = baseTime.AddMinutes(i)
            };
            _aggregator.UpdateCandle(candles, tick, TimeFrame.M1);
        }

        Assert.Equal(10000, candles.Count);
    }

    [Theory]
    [InlineData(10, 0, TimeFrame.M1, 10, 0)]
    [InlineData(10, 7, TimeFrame.M5, 10, 5)]
    [InlineData(10, 23, TimeFrame.M15, 10, 15)]
    [InlineData(10, 45, TimeFrame.M30, 10, 30)]
    [InlineData(13, 30, TimeFrame.H1, 13, 0)]
    [InlineData(15, 30, TimeFrame.H4, 12, 0)]
    public void GetCandleTime_AlignsToPeriod(int hour, int minute, TimeFrame tf, int expectedHour, int expectedMinute)
    {
        var time = new DateTime(2024, 1, 15, hour, minute, 30, DateTimeKind.Utc);
        var candleTime = CandleAggregator.GetCandleTime(time, tf);

        Assert.Equal(expectedHour, candleTime.Hour);
        Assert.Equal(expectedMinute, candleTime.Minute);
        Assert.Equal(0, candleTime.Second);
    }

    [Fact]
    public void GetCandleTime_D1_AlignsMidnight()
    {
        var time = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc);
        var candleTime = CandleAggregator.GetCandleTime(time, TimeFrame.D1);

        Assert.Equal(0, candleTime.Hour);
        Assert.Equal(0, candleTime.Minute);
        Assert.Equal(15, candleTime.Day);
    }

    [Fact]
    public void GetCandleTime_MN1_AlignsToFirstOfMonth()
    {
        var time = new DateTime(2024, 3, 15, 14, 30, 0, DateTimeKind.Utc);
        var candleTime = CandleAggregator.GetCandleTime(time, TimeFrame.MN1);

        Assert.Equal(1, candleTime.Day);
        Assert.Equal(3, candleTime.Month);
    }

    [Fact]
    public void GetCandleTime_W1_AlignsToMonday()
    {
        // Jan 17, 2024 is a Wednesday
        var time = new DateTime(2024, 1, 17, 14, 30, 0, DateTimeKind.Utc);
        var candleTime = CandleAggregator.GetCandleTime(time, TimeFrame.W1);

        Assert.Equal(DayOfWeek.Monday, candleTime.DayOfWeek);
        Assert.Equal(15, candleTime.Day); // Monday the 15th
    }
}
