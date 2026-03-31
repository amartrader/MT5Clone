using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;
using Xunit;

namespace MT5Clone.Tests.MarketData;

public class SimulatedDataProviderTests
{
    private static Symbol CreateSymbol()
    {
        return new Symbol
        {
            Name = "EURUSD",
            Digits = 5,
            Point = 0.00001,
            Bid = 1.08500,
            Ask = 1.08510
        };
    }

    [Fact]
    public void InitializePrice_SetsSymbolPrices()
    {
        var provider = new SimulatedDataProvider();
        var symbol = CreateSymbol();

        provider.InitializePrice(symbol);

        Assert.True(symbol.Bid > 0);
        Assert.True(symbol.Ask > symbol.Bid);
        Assert.True(symbol.DayOpen > 0);
        Assert.True(symbol.DayHigh >= symbol.DayOpen);
        Assert.True(symbol.DayLow <= symbol.DayOpen);
        Assert.True(symbol.PreviousClose > 0);
    }

    [Fact]
    public void GenerateTick_ReturnsValidTick()
    {
        var provider = new SimulatedDataProvider();
        var symbol = CreateSymbol();
        provider.InitializePrice(symbol);

        var tick = provider.GenerateTick(symbol);

        Assert.Equal("EURUSD", tick.Symbol);
        Assert.True(tick.Bid > 0);
        Assert.True(tick.Ask > tick.Bid);
        Assert.True(tick.Volume > 0);
    }

    [Fact]
    public void GenerateTick_ProducesVaryingPrices()
    {
        var provider = new SimulatedDataProvider();
        var symbol = CreateSymbol();
        provider.InitializePrice(symbol);

        var ticks = new List<Tick>();
        for (int i = 0; i < 100; i++)
        {
            ticks.Add(provider.GenerateTick(symbol));
        }

        var distinctBids = ticks.Select(t => t.Bid).Distinct().Count();
        Assert.True(distinctBids > 1, "Expected varying prices from simulated data");
    }

    [Fact]
    public void GenerateMarketDepth_ReturnsValidDepth()
    {
        var provider = new SimulatedDataProvider();
        var symbol = CreateSymbol();
        provider.InitializePrice(symbol);

        var depth = provider.GenerateMarketDepth(symbol);

        Assert.Equal("EURUSD", depth.Symbol);
        Assert.True(depth.Entries.Count > 0);
        Assert.True(depth.Bids.Count > 0);
        Assert.True(depth.Asks.Count > 0);
    }

    [Fact]
    public void GenerateHistoricalCandles_ReturnsRequestedCount()
    {
        var provider = new SimulatedDataProvider();
        var symbol = CreateSymbol();
        provider.InitializePrice(symbol);

        var candles = provider.GenerateHistoricalCandles(symbol, TimeFrame.H1, 200);

        Assert.Equal(200, candles.Count);
    }

    [Fact]
    public void GenerateHistoricalCandles_HaveValidOHLC()
    {
        var provider = new SimulatedDataProvider();
        var symbol = CreateSymbol();
        provider.InitializePrice(symbol);

        var candles = provider.GenerateHistoricalCandles(symbol, TimeFrame.H1, 100);

        foreach (var candle in candles)
        {
            Assert.True(candle.High >= candle.Low, $"High ({candle.High}) should be >= Low ({candle.Low})");
            Assert.True(candle.High >= candle.Open, $"High ({candle.High}) should be >= Open ({candle.Open})");
            Assert.True(candle.High >= candle.Close, $"High ({candle.High}) should be >= Close ({candle.Close})");
            Assert.True(candle.Low <= candle.Open, $"Low ({candle.Low}) should be <= Open ({candle.Open})");
            Assert.True(candle.Low <= candle.Close, $"Low ({candle.Low}) should be <= Close ({candle.Close})");
        }
    }

    [Fact]
    public void GenerateHistoricalCandles_AreChronological()
    {
        var provider = new SimulatedDataProvider();
        var symbol = CreateSymbol();
        provider.InitializePrice(symbol);

        var candles = provider.GenerateHistoricalCandles(symbol, TimeFrame.H1, 100);

        for (int i = 1; i < candles.Count; i++)
        {
            Assert.True(candles[i].Time > candles[i - 1].Time,
                $"Candle at index {i} ({candles[i].Time}) should be after candle at {i - 1} ({candles[i - 1].Time})");
        }
    }
}
