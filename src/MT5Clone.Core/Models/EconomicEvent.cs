namespace MT5Clone.Core.Models;

public class EconomicEvent
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EconomicEventImpact Impact { get; set; }
    public string Actual { get; set; } = string.Empty;
    public string Forecast { get; set; } = string.Empty;
    public string Previous { get; set; } = string.Empty;
    public string Revised { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

public enum EconomicEventImpact
{
    None,
    Low,
    Medium,
    High
}
