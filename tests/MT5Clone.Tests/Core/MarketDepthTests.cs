using MT5Clone.Core.Models;
using Xunit;

namespace MT5Clone.Tests.Core;

public class MarketDepthTests
{
    [Fact]
    public void Bids_ReturnsBuyEntriesDescending()
    {
        var depth = new MarketDepth { Symbol = "EURUSD" };
        depth.Entries.Add(new MarketDepthEntry { Type = MarketDepthType.Buy, Price = 1.0850, Volume = 10 });
        depth.Entries.Add(new MarketDepthEntry { Type = MarketDepthType.Buy, Price = 1.0860, Volume = 20 });
        depth.Entries.Add(new MarketDepthEntry { Type = MarketDepthType.Sell, Price = 1.0870, Volume = 15 });

        var bids = depth.Bids;
        Assert.Equal(2, bids.Count);
        Assert.True(bids[0].Price > bids[1].Price);
    }

    [Fact]
    public void Asks_ReturnsSellEntriesAscending()
    {
        var depth = new MarketDepth { Symbol = "EURUSD" };
        depth.Entries.Add(new MarketDepthEntry { Type = MarketDepthType.Sell, Price = 1.0880, Volume = 10 });
        depth.Entries.Add(new MarketDepthEntry { Type = MarketDepthType.Sell, Price = 1.0870, Volume = 20 });
        depth.Entries.Add(new MarketDepthEntry { Type = MarketDepthType.Buy, Price = 1.0860, Volume = 15 });

        var asks = depth.Asks;
        Assert.Equal(2, asks.Count);
        Assert.True(asks[0].Price < asks[1].Price);
    }

    [Fact]
    public void EmptyDepth_ReturnsEmptyLists()
    {
        var depth = new MarketDepth();
        Assert.Empty(depth.Bids);
        Assert.Empty(depth.Asks);
    }
}
