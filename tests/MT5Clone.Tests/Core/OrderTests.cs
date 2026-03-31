using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using Xunit;

namespace MT5Clone.Tests.Core;

public class OrderTests
{
    [Theory]
    [InlineData(OrderType.Buy, true)]
    [InlineData(OrderType.Sell, true)]
    [InlineData(OrderType.BuyLimit, false)]
    [InlineData(OrderType.SellLimit, false)]
    [InlineData(OrderType.BuyStop, false)]
    [InlineData(OrderType.SellStop, false)]
    public void IsMarketOrder_CorrectForType(OrderType type, bool expected)
    {
        var order = new Order { Type = type };
        Assert.Equal(expected, order.IsMarketOrder);
    }

    [Theory]
    [InlineData(OrderType.BuyLimit, true)]
    [InlineData(OrderType.SellLimit, true)]
    [InlineData(OrderType.BuyStop, true)]
    [InlineData(OrderType.SellStop, true)]
    [InlineData(OrderType.Buy, false)]
    [InlineData(OrderType.Sell, false)]
    public void IsPendingOrder_CorrectForType(OrderType type, bool expected)
    {
        var order = new Order { Type = type };
        Assert.Equal(expected, order.IsPendingOrder);
    }

    [Theory]
    [InlineData(OrderType.Buy, true)]
    [InlineData(OrderType.BuyLimit, true)]
    [InlineData(OrderType.BuyStop, true)]
    [InlineData(OrderType.BuyStopLimit, true)]
    [InlineData(OrderType.Sell, false)]
    [InlineData(OrderType.SellLimit, false)]
    public void IsBuyOrder_CorrectForType(OrderType type, bool expected)
    {
        var order = new Order { Type = type };
        Assert.Equal(expected, order.IsBuyOrder);
    }

    [Theory]
    [InlineData(OrderType.Sell, true)]
    [InlineData(OrderType.SellLimit, true)]
    [InlineData(OrderType.SellStop, true)]
    [InlineData(OrderType.SellStopLimit, true)]
    [InlineData(OrderType.Buy, false)]
    [InlineData(OrderType.BuyLimit, false)]
    public void IsSellOrder_CorrectForType(OrderType type, bool expected)
    {
        var order = new Order { Type = type };
        Assert.Equal(expected, order.IsSellOrder);
    }
}
