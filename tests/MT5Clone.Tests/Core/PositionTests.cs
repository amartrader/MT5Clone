using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using Xunit;

namespace MT5Clone.Tests.Core;

public class PositionTests
{
    [Fact]
    public void NetProfit_IncludesSwapAndCommission()
    {
        var position = new Position
        {
            Profit = 100.0,
            Swap = -2.5,
            Commission = -7.0
        };

        Assert.Equal(90.5, position.NetProfit);
    }

    [Fact]
    public void NetProfit_WithZeroSwapAndCommission()
    {
        var position = new Position { Profit = 50.0 };
        Assert.Equal(50.0, position.NetProfit);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var position = new Position();
        Assert.Equal(string.Empty, position.Symbol);
        Assert.Equal(string.Empty, position.Comment);
        Assert.Equal(string.Empty, position.ExternalId);
        Assert.Equal(0, position.Ticket);
    }
}
