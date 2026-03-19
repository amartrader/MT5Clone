using System.Collections.ObjectModel;
using MT5Clone.App.Helpers;
using MT5Clone.OpenAlgo.Services;

namespace MT5Clone.App.ViewModels;

public class NavigatorItem : ViewModelBase
{
    private string _name = string.Empty;
    private string _icon = string.Empty;
    private string _iconType = string.Empty;
    private bool _isExpanded;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Icon { get => _icon; set => SetProperty(ref _icon, value); }
    public string IconType { get => _iconType; set => SetProperty(ref _iconType, value); }
    public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
    public ObservableCollection<NavigatorItem> Children { get; } = new();
}

public class NavigatorViewModel : ViewModelBase
{
    private readonly OpenAlgoService _openAlgoService;
    private bool _isVisible = true;
    private NavigatorItem? _selectedItem;

    public ObservableCollection<NavigatorItem> Items { get; } = new();
    public ObservableCollection<NavigatorItem> RootNodes { get; } = new();

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public NavigatorItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public NavigatorViewModel(OpenAlgoService openAlgoService)
    {
        _openAlgoService = openAlgoService;
        InitializeNavigator();
    }

    public void UpdateForOpenAlgo()
    {
        // Update accounts section to reflect OpenAlgo connection
        var accounts = Items.FirstOrDefault(i => i.Name == "Accounts");
        if (accounts != null)
        {
            accounts.Children.Clear();
            accounts.Children.Add(new NavigatorItem
            {
                Name = $"OpenAlgo - {_openAlgoService.Config.Host}",
                Icon = "\ud83d\udfe2",
                IconType = "Server"
            });

            // Add supported exchanges
            var exchanges = new NavigatorItem { Name = "Exchanges", Icon = "\ud83c\udfe6", IconType = "Folder", IsExpanded = true };
            exchanges.Children.Add(new NavigatorItem { Name = "NSE - National Stock Exchange", Icon = "\ud83d\udcca", IconType = "Exchange" });
            exchanges.Children.Add(new NavigatorItem { Name = "BSE - Bombay Stock Exchange", Icon = "\ud83d\udcca", IconType = "Exchange" });
            exchanges.Children.Add(new NavigatorItem { Name = "NFO - NSE Futures & Options", Icon = "\ud83d\udcca", IconType = "Exchange" });
            exchanges.Children.Add(new NavigatorItem { Name = "BFO - BSE Futures & Options", Icon = "\ud83d\udcca", IconType = "Exchange" });
            exchanges.Children.Add(new NavigatorItem { Name = "MCX - Multi Commodity Exchange", Icon = "\ud83d\udcca", IconType = "Exchange" });
            exchanges.Children.Add(new NavigatorItem { Name = "CDS - Currency Derivatives", Icon = "\ud83d\udcca", IconType = "Exchange" });
            accounts.Children.Add(exchanges);

            // Add supported brokers info
            var brokers = new NavigatorItem { Name = "Supported Brokers", Icon = "\ud83c\udfe6", IconType = "Folder" };
            brokers.Children.Add(new NavigatorItem { Name = "Zerodha", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "Upstox", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "Fyers", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "AngelOne", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "Shoonya (Finvasia)", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "Dhan", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "IIFL", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "Kotak Neo", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "5paisa", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "Flattrade", IconType = "Broker" });
            brokers.Children.Add(new NavigatorItem { Name = "+ 20 more...", IconType = "Info" });
            accounts.Children.Add(brokers);
        }
    }

    private void InitializeNavigator()
    {
        // Accounts
        var accounts = new NavigatorItem { Name = "Accounts", Icon = "\ud83d\udc64", IconType = "Account", IsExpanded = true };
        accounts.Children.Add(new NavigatorItem { Name = "12345678 - MT5Clone-Demo", Icon = "\ud83d\udcbb", IconType = "Server" });
        Items.Add(accounts);
        RootNodes.Add(accounts);

        // Indicators
        var indicators = new NavigatorItem { Name = "Indicators", Icon = "\ud83d\udcc8", IconType = "Indicator", IsExpanded = true };

        var trend = new NavigatorItem { Name = "Trend", Icon = "\ud83d\udcc1", IconType = "Folder" };
        trend.Children.Add(new NavigatorItem { Name = "Moving Average", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Bollinger Bands", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Ichimoku Kinko Hyo", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Parabolic SAR", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Average Directional Index", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Envelopes", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Standard Deviation", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        indicators.Children.Add(trend);

        var oscillators = new NavigatorItem { Name = "Oscillators", Icon = "\ud83d\udcc1", IconType = "Folder" };
        oscillators.Children.Add(new NavigatorItem { Name = "RSI", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "MACD", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Stochastic Oscillator", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "CCI", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Williams' Percent Range", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Momentum", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "DeMarker", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Force Index", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Relative Vigor Index", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        indicators.Children.Add(oscillators);

        var volume = new NavigatorItem { Name = "Volumes", Icon = "\ud83d\udcc1", IconType = "Folder" };
        volume.Children.Add(new NavigatorItem { Name = "On Balance Volume", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        volume.Children.Add(new NavigatorItem { Name = "Money Flow Index", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        volume.Children.Add(new NavigatorItem { Name = "Accumulation/Distribution", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        volume.Children.Add(new NavigatorItem { Name = "Volumes", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        indicators.Children.Add(volume);

        var billWilliams = new NavigatorItem { Name = "Bill Williams", Icon = "\ud83d\udcc1", IconType = "Folder" };
        billWilliams.Children.Add(new NavigatorItem { Name = "Alligator", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Awesome Oscillator", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Fractals", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Gator Oscillator", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Market Facilitation Index", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Accelerator Oscillator", Icon = "\ud83d\udcc9", IconType = "Indicator" });
        indicators.Children.Add(billWilliams);

        Items.Add(indicators);
        RootNodes.Add(indicators);

        // Expert Advisors
        var experts = new NavigatorItem { Name = "Expert Advisors", Icon = "\ud83e\udd16", IconType = "Expert", IsExpanded = false };
        experts.Children.Add(new NavigatorItem { Name = "ExpertMACD", Icon = "\u2699\ufe0f", IconType = "Expert" });
        experts.Children.Add(new NavigatorItem { Name = "ExpertMA Crossover", Icon = "\u2699\ufe0f", IconType = "Expert" });
        Items.Add(experts);
        RootNodes.Add(experts);

        // Scripts
        var scripts = new NavigatorItem { Name = "Scripts", Icon = "\ud83d\udcdc", IconType = "Script", IsExpanded = false };
        scripts.Children.Add(new NavigatorItem { Name = "CloseAll", Icon = "\ud83d\udcdd", IconType = "Script" });
        scripts.Children.Add(new NavigatorItem { Name = "PendingGrid", Icon = "\ud83d\udcdd", IconType = "Script" });
        Items.Add(scripts);
        RootNodes.Add(scripts);
    }
}
