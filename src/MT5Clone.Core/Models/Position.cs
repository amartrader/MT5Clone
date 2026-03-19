using MT5Clone.Core.Enums;

namespace MT5Clone.Core.Models;

public class Position
{
    public long Ticket { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public PositionType Type { get; set; }
    public double Volume { get; set; }
    public double PriceOpen { get; set; }
    public double PriceCurrent { get; set; }
    public double StopLoss { get; set; }
    public double TakeProfit { get; set; }
    public DateTime Time { get; set; }
    public DateTime TimeUpdate { get; set; }

    public double Swap { get; set; }
    public double Commission { get; set; }
    public double Profit { get; set; }

    public long Magic { get; set; }
    public long Identifier { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;

    public double NetProfit => Profit + Swap + Commission;

    public double CalculateProfit(double currentPrice, double contractSize, double tickValue, double tickSize)
    {
        double direction = Type == PositionType.Buy ? 1.0 : -1.0;
        double priceDiff = (currentPrice - PriceOpen) * direction;
        return (priceDiff / tickSize) * tickValue * Volume * contractSize / 100000.0;
    }
}
