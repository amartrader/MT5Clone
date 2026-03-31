using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;
using Xunit;

namespace MT5Clone.Tests.MarketData;

public class MarketDataServiceTests
{
    [Fact]
    public void GetSymbols_ReturnsAllSymbols()
    {
        var service = new MarketDataService();
        var symbols = service.GetSymbols();

        Assert.True(symbols.Count >= 20);
    }

    [Fact]
    public void GetSymbol_KnownSymbol_ReturnsSymbol()
    {
        var service = new MarketDataService();
        var symbol = service.GetSymbol("EURUSD");

        Assert.NotNull(symbol);
        Assert.Equal("EURUSD", symbol.Name);
        Assert.Equal(5, symbol.Digits);
        Assert.Equal(SymbolType.Forex, symbol.SymbolType);
    }

    [Fact]
    public void GetSymbol_UnknownSymbol_ReturnsNull()
    {
        var service = new MarketDataService();
        var symbol = service.GetSymbol("NONEXISTENT");

        Assert.Null(symbol);
    }

    [Fact]
    public void GetCandles_ReturnsHistoricalData()
    {
        var service = new MarketDataService();
        var candles = service.GetCandles("EURUSD", TimeFrame.H1, 100);

        Assert.Equal(100, candles.Count);
        foreach (var candle in candles)
        {
            Assert.True(candle.High >= candle.Low);
            Assert.True(candle.High >= candle.Open);
            Assert.True(candle.High >= candle.Close);
            Assert.True(candle.Low <= candle.Open);
            Assert.True(candle.Low <= candle.Close);
        }
    }

    [Fact]
    public void GetCandles_WithDateRange_FiltersCorrectly()
    {
        var service = new MarketDataService();
        var now = DateTime.UtcNow;
        var candles = service.GetCandles("EURUSD", TimeFrame.H1, now.AddDays(-5), now);

        // Should return some candles within the range
        foreach (var candle in candles)
        {
            Assert.True(candle.Time >= now.AddDays(-5));
            Assert.True(candle.Time <= now);
        }
    }

    [Fact]
    public void GetCandles_DifferentTimeframes_HaveDifferentCounts()
    {
        var service = new MarketDataService();
        var m1Candles = service.GetCandles("EURUSD", TimeFrame.M1, 500);
        var h1Candles = service.GetCandles("EURUSD", TimeFrame.H1, 500);

        Assert.Equal(500, m1Candles.Count);
        Assert.Equal(500, h1Candles.Count);
    }

    [Theory]
    [InlineData("EURUSD", SymbolType.Forex)]
    [InlineData("XAUUSD", SymbolType.CFD)]
    [InlineData("BTCUSD", SymbolType.Crypto)]
    public void GetSymbol_HasCorrectType(string name, SymbolType expectedType)
    {
        var service = new MarketDataService();
        var symbol = service.GetSymbol(name);

        Assert.NotNull(symbol);
        Assert.Equal(expectedType, symbol.SymbolType);
    }

    [Fact]
    public void SubscribeSymbol_DoesNotThrow()
    {
        var service = new MarketDataService();
        service.SubscribeSymbol("EURUSD");
        service.UnsubscribeSymbol("EURUSD");
    }

    [Fact]
    public void SubscribeMarketDepth_DoesNotThrow()
    {
        var service = new MarketDataService();
        service.SubscribeMarketDepth("EURUSD");
        service.UnsubscribeMarketDepth("EURUSD");
    }

    [Fact]
    public void IsRunning_InitiallyFalse()
    {
        var service = new MarketDataService();
        Assert.False(service.IsRunning);
    }

    [Fact]
    public void ForexSymbols_HaveCorrectProperties()
    {
        var service = new MarketDataService();
        var eurusd = service.GetSymbol("EURUSD");

        Assert.NotNull(eurusd);
        Assert.Equal(100000, eurusd.ContractSize);
        Assert.Equal(0.01, eurusd.MinLot);
        Assert.Equal(100.0, eurusd.MaxLot);
        Assert.Equal(0.01, eurusd.LotStep);
        Assert.Equal("EUR", eurusd.BaseCurrency);
        Assert.Equal("USD", eurusd.QuoteCurrency);
    }

    [Fact]
    public void JPYPairs_HaveThreeDigits()
    {
        var service = new MarketDataService();
        var usdjpy = service.GetSymbol("USDJPY");

        Assert.NotNull(usdjpy);
        Assert.Equal(3, usdjpy.Digits);
        Assert.Equal(0.001, usdjpy.Point);
    }
}
