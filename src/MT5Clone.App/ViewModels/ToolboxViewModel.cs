using System.Collections.ObjectModel;
using MT5Clone.App.Helpers;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;

namespace MT5Clone.App.ViewModels;

public class ToolboxViewModel : ViewModelBase
{
    private readonly MarketDataService _marketDataService;
    private bool _isVisible = true;
    private int _selectedTab;

    public ObservableCollection<EconomicEvent> EconomicEvents { get; } = new();
    public ObservableCollection<Alert> Alerts { get; } = new();
    public ObservableCollection<NewsItem> News { get; } = new();

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public int SelectedTab
    {
        get => _selectedTab;
        set => SetProperty(ref _selectedTab, value);
    }

    public ToolboxViewModel(MarketDataService marketDataService)
    {
        _marketDataService = marketDataService;
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        // Sample Economic Events
        EconomicEvents.Add(new EconomicEvent
        {
            Time = DateTime.UtcNow.AddHours(2),
            Country = "US",
            Currency = "USD",
            Name = "Non-Farm Payrolls",
            Impact = EconomicEventImpact.High,
            Forecast = "185K",
            Previous = "175K"
        });
        EconomicEvents.Add(new EconomicEvent
        {
            Time = DateTime.UtcNow.AddHours(4),
            Country = "EU",
            Currency = "EUR",
            Name = "ECB Interest Rate Decision",
            Impact = EconomicEventImpact.High,
            Forecast = "4.50%",
            Previous = "4.50%"
        });
        EconomicEvents.Add(new EconomicEvent
        {
            Time = DateTime.UtcNow.AddHours(6),
            Country = "GB",
            Currency = "GBP",
            Name = "Manufacturing PMI",
            Impact = EconomicEventImpact.Medium,
            Forecast = "46.5",
            Previous = "46.2"
        });
        EconomicEvents.Add(new EconomicEvent
        {
            Time = DateTime.UtcNow.AddHours(8),
            Country = "JP",
            Currency = "JPY",
            Name = "BOJ Policy Rate",
            Impact = EconomicEventImpact.High,
            Forecast = "-0.10%",
            Previous = "-0.10%"
        });
        EconomicEvents.Add(new EconomicEvent
        {
            Time = DateTime.UtcNow.AddHours(10),
            Country = "US",
            Currency = "USD",
            Name = "CPI m/m",
            Impact = EconomicEventImpact.High,
            Forecast = "0.3%",
            Previous = "0.4%"
        });

        // Sample News
        News.Add(new NewsItem
        {
            Time = DateTime.UtcNow.AddMinutes(-30),
            Title = "EUR/USD rises on weak US data",
            Category = "Forex",
            Source = "MT5Clone News"
        });
        News.Add(new NewsItem
        {
            Time = DateTime.UtcNow.AddHours(-1),
            Title = "Gold reaches new weekly high",
            Category = "Commodities",
            Source = "MT5Clone News"
        });
        News.Add(new NewsItem
        {
            Time = DateTime.UtcNow.AddHours(-2),
            Title = "Fed signals potential rate cut in 2024",
            Category = "Central Banks",
            Source = "MT5Clone News"
        });
        News.Add(new NewsItem
        {
            Time = DateTime.UtcNow.AddHours(-3),
            Title = "Bitcoin breaks above 42,000 resistance",
            Category = "Crypto",
            Source = "MT5Clone News"
        });
    }
}
