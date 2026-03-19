using MT5Clone.Core.Enums;

namespace MT5Clone.Core.Models;

public class Deal
{
    public long Ticket { get; set; }
    public long OrderTicket { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public DealType Type { get; set; }
    public DealEntry Entry { get; set; }
    public double Volume { get; set; }
    public double Price { get; set; }
    public DateTime Time { get; set; }

    public double Commission { get; set; }
    public double Swap { get; set; }
    public double Profit { get; set; }
    public double Fee { get; set; }

    public long Magic { get; set; }
    public long PositionId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;

    public double NetProfit => Profit + Swap + Commission + Fee;
}
