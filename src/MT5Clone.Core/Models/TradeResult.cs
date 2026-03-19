namespace MT5Clone.Core.Models;

public class TradeResult
{
    public bool Success { get; set; }
    public int ReturnCode { get; set; }
    public string Comment { get; set; } = string.Empty;
    public long OrderTicket { get; set; }
    public long DealTicket { get; set; }
    public double Volume { get; set; }
    public double Price { get; set; }
    public double Bid { get; set; }
    public double Ask { get; set; }

    public static TradeResult Succeeded(long orderTicket, double price, double volume)
    {
        return new TradeResult
        {
            Success = true,
            ReturnCode = 10009,
            Comment = "Request completed",
            OrderTicket = orderTicket,
            Price = price,
            Volume = volume
        };
    }

    public static TradeResult Failed(string reason, int code = 10006)
    {
        return new TradeResult
        {
            Success = false,
            ReturnCode = code,
            Comment = reason
        };
    }
}
