using MT5Clone.Core.Models;
using Xunit;

namespace MT5Clone.Tests.Core;

public class TradeResultTests
{
    [Fact]
    public void Succeeded_CreatesSuccessResult()
    {
        var result = TradeResult.Succeeded(12345, 1.08500, 0.1);

        Assert.True(result.Success);
        Assert.Equal(10009, result.ReturnCode);
        Assert.Equal("Request completed", result.Comment);
        Assert.Equal(12345, result.OrderTicket);
        Assert.Equal(1.08500, result.Price);
        Assert.Equal(0.1, result.Volume);
    }

    [Fact]
    public void Failed_CreatesFailureResult()
    {
        var result = TradeResult.Failed("Not enough money");

        Assert.False(result.Success);
        Assert.Equal(10006, result.ReturnCode);
        Assert.Equal("Not enough money", result.Comment);
    }

    [Fact]
    public void Failed_WithCustomCode()
    {
        var result = TradeResult.Failed("Invalid volume", 10014);

        Assert.False(result.Success);
        Assert.Equal(10014, result.ReturnCode);
    }
}
