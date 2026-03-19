namespace MT5Clone.Core.Models;

public class Tick
{
    public string Symbol { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public double Bid { get; set; }
    public double Ask { get; set; }
    public double Last { get; set; }
    public double Volume { get; set; }
    public long TimeMilliseconds { get; set; }
    public TickFlags Flags { get; set; }

    public double Spread => Ask - Bid;
    public double Mid => (Bid + Ask) / 2.0;
}

[Flags]
public enum TickFlags
{
    None = 0,
    Bid = 1,
    Ask = 2,
    Last = 4,
    Volume = 8,
    Buy = 16,
    Sell = 32
}
