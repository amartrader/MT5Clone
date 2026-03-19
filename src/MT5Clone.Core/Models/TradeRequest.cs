using MT5Clone.Core.Enums;

namespace MT5Clone.Core.Models;

public class TradeRequest
{
    public TradeAction Action { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public double Volume { get; set; }
    public double Price { get; set; }
    public double StopLoss { get; set; }
    public double TakeProfit { get; set; }
    public double StopLimitPrice { get; set; }
    public OrderType OrderType { get; set; }
    public OrderFillingType FillingType { get; set; }
    public OrderTimeInForce TimeInForce { get; set; }
    public DateTime Expiration { get; set; }
    public long Deviation { get; set; }
    public long Magic { get; set; }
    public long OrderTicket { get; set; }
    public long PositionTicket { get; set; }
    public long PositionByTicket { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public enum TradeAction
{
    Deal,
    Pending,
    Modify,
    Remove,
    CloseBy,
    ClosePartial
}
