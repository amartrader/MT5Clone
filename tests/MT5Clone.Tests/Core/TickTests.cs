using MT5Clone.Core.Models;
using Xunit;

namespace MT5Clone.Tests.Core;

public class TickTests
{
    [Fact]
    public void Spread_CalculatedFromBidAsk()
    {
        var tick = new Tick { Bid = 1.08500, Ask = 1.08510 };
        Assert.Equal(0.00010, tick.Spread, 5);
    }

    [Fact]
    public void Mid_IsAverageOfBidAsk()
    {
        var tick = new Tick { Bid = 1.08500, Ask = 1.08510 };
        Assert.Equal(1.08505, tick.Mid, 5);
    }

    [Fact]
    public void TickFlags_CanBeCombined()
    {
        var tick = new Tick { Flags = TickFlags.Bid | TickFlags.Ask };
        Assert.True(tick.Flags.HasFlag(TickFlags.Bid));
        Assert.True(tick.Flags.HasFlag(TickFlags.Ask));
        Assert.False(tick.Flags.HasFlag(TickFlags.Last));
    }

    [Fact]
    public void DefaultSymbol_IsEmpty()
    {
        var tick = new Tick();
        Assert.Equal(string.Empty, tick.Symbol);
    }
}
