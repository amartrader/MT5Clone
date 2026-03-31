using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using Xunit;

namespace MT5Clone.Tests.Core;

public class DealTests
{
    [Fact]
    public void NetProfit_IncludesAllComponents()
    {
        var deal = new Deal
        {
            Profit = 200.0,
            Swap = -5.0,
            Commission = -7.0,
            Fee = -1.0
        };

        Assert.Equal(187.0, deal.NetProfit);
    }

    [Fact]
    public void NetProfit_WithZeroFees()
    {
        var deal = new Deal { Profit = 100.0 };
        Assert.Equal(100.0, deal.NetProfit);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var deal = new Deal();
        Assert.Equal(string.Empty, deal.Symbol);
        Assert.Equal(string.Empty, deal.Comment);
        Assert.Equal(string.Empty, deal.ExternalId);
    }
}
