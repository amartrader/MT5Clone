using MT5Clone.Core.Enums;

namespace MT5Clone.Core.Models;

public class Order
{
    public long Ticket { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public OrderType Type { get; set; }
    public OrderState State { get; set; }
    public OrderFillingType FillingType { get; set; }
    public OrderTimeInForce TimeInForce { get; set; }

    public double Volume { get; set; }
    public double VolumeInitial { get; set; }
    public double VolumeCurrent { get; set; }
    public double Price { get; set; }
    public double PriceTrigger { get; set; }
    public double PriceCurrent { get; set; }
    public double StopLoss { get; set; }
    public double TakeProfit { get; set; }

    public DateTime TimeSetup { get; set; }
    public DateTime TimeExpiration { get; set; }
    public DateTime TimeDone { get; set; }

    public long Magic { get; set; }
    public long PositionId { get; set; }
    public long PositionById { get; set; }

    public string Comment { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;

    public bool IsMarketOrder => Type == OrderType.Buy || Type == OrderType.Sell;
    public bool IsPendingOrder => !IsMarketOrder;
    public bool IsBuyOrder => Type == OrderType.Buy || Type == OrderType.BuyLimit ||
                               Type == OrderType.BuyStop || Type == OrderType.BuyStopLimit;
    public bool IsSellOrder => Type == OrderType.Sell || Type == OrderType.SellLimit ||
                                Type == OrderType.SellStop || Type == OrderType.SellStopLimit;
}
