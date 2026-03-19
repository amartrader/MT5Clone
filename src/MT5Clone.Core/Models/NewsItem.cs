namespace MT5Clone.Core.Models;

public class NewsItem
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public List<string> RelatedSymbols { get; set; } = new();
    public bool IsRead { get; set; }
}
