namespace MT5Clone.Core.Models;

public class MarketDepthEntry
{
    public MarketDepthType Type { get; set; }
    public double Price { get; set; }
    public double Volume { get; set; }
}

public enum MarketDepthType
{
    Buy,
    Sell,
    BuyMarket,
    SellMarket
}

public class MarketDepth
{
    public string Symbol { get; set; } = string.Empty;
    public List<MarketDepthEntry> Entries { get; set; } = new();
    public DateTime Time { get; set; }

    public List<MarketDepthEntry> Bids => Entries.Where(e => e.Type == MarketDepthType.Buy).OrderByDescending(e => e.Price).ToList();
    public List<MarketDepthEntry> Asks => Entries.Where(e => e.Type == MarketDepthType.Sell).OrderBy(e => e.Price).ToList();
}
