using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using Xunit;

namespace MT5Clone.Tests.Core;

public class SymbolTests
{
    private static Symbol CreateForexSymbol(string name = "EURUSD")
    {
        return new Symbol
        {
            Name = name,
            Description = "Euro vs US Dollar",
            BaseCurrency = "EUR",
            QuoteCurrency = "USD",
            SymbolType = SymbolType.Forex,
            Digits = 5,
            Point = 0.00001,
            TickSize = 0.00001,
            ContractSize = 100000,
            MinLot = 0.01,
            MaxLot = 100.0,
            LotStep = 0.01,
            Bid = 1.08500,
            Ask = 1.08510
        };
    }

    [Fact]
    public void Spread_CalculatedFromBidAsk()
    {
        var symbol = CreateForexSymbol();
        // Spread = (Ask - Bid) / Point = 0.0001 / 0.00001 = 10
        Assert.Equal(10.0, symbol.Spread, 1);
    }

    [Fact]
    public void SpreadValue_ReturnsDifference()
    {
        var symbol = CreateForexSymbol();
        Assert.Equal(0.00010, symbol.SpreadValue, 5);
    }

    [Fact]
    public void FormatPrice_RespectsDigits()
    {
        var symbol = CreateForexSymbol();
        Assert.Equal("1.08500", symbol.FormatPrice(1.085));
    }

    [Fact]
    public void FormatPrice_WithThreeDigits()
    {
        var symbol = new Symbol { Digits = 3 };
        Assert.Equal("149.500", symbol.FormatPrice(149.5));
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var symbol = new Symbol();
        Assert.Equal(5, symbol.Digits);
        Assert.Equal(0.00001, symbol.Point);
        Assert.Equal(100000, symbol.ContractSize);
        Assert.True(symbol.IsVisible);
        Assert.False(symbol.IsSelected);
        Assert.False(symbol.IsFavorite);
        Assert.Equal(SymbolTradeMode.Full, symbol.TradeMode);
    }

    [Fact]
    public void SwapRollover3Days_DefaultsToWednesday()
    {
        var symbol = new Symbol();
        Assert.Equal(3, symbol.SwapRollover3Days);
    }
}
