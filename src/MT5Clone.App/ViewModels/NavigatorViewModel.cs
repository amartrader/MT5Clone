using System.Collections.ObjectModel;
using MT5Clone.App.Helpers;

namespace MT5Clone.App.ViewModels;

public class NavigatorItem : ViewModelBase
{
    private string _name = string.Empty;
    private string _iconType = string.Empty;
    private bool _isExpanded;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string IconType { get => _iconType; set => SetProperty(ref _iconType, value); }
    public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
    public ObservableCollection<NavigatorItem> Children { get; } = new();
}

public class NavigatorViewModel : ViewModelBase
{
    private bool _isVisible = true;
    private NavigatorItem? _selectedItem;

    public ObservableCollection<NavigatorItem> Items { get; } = new();

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

    public NavigatorViewModel()
    {
        InitializeNavigator();
    }

    private void InitializeNavigator()
    {
        // Accounts
        var accounts = new NavigatorItem { Name = "Accounts", IconType = "Account", IsExpanded = true };
        accounts.Children.Add(new NavigatorItem { Name = "12345678 - MT5Clone-Demo", IconType = "Server" });
        Items.Add(accounts);

        // Indicators
        var indicators = new NavigatorItem { Name = "Indicators", IconType = "Indicator", IsExpanded = true };

        var trend = new NavigatorItem { Name = "Trend", IconType = "Folder" };
        trend.Children.Add(new NavigatorItem { Name = "Moving Average", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Bollinger Bands", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Ichimoku Kinko Hyo", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Parabolic SAR", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Average Directional Index", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Envelopes", IconType = "Indicator" });
        trend.Children.Add(new NavigatorItem { Name = "Standard Deviation", IconType = "Indicator" });
        indicators.Children.Add(trend);

        var oscillators = new NavigatorItem { Name = "Oscillators", IconType = "Folder" };
        oscillators.Children.Add(new NavigatorItem { Name = "RSI", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "MACD", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Stochastic Oscillator", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "CCI", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Williams' Percent Range", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Momentum", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "DeMarker", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Force Index", IconType = "Indicator" });
        oscillators.Children.Add(new NavigatorItem { Name = "Relative Vigor Index", IconType = "Indicator" });
        indicators.Children.Add(oscillators);

        var volume = new NavigatorItem { Name = "Volumes", IconType = "Folder" };
        volume.Children.Add(new NavigatorItem { Name = "On Balance Volume", IconType = "Indicator" });
        volume.Children.Add(new NavigatorItem { Name = "Money Flow Index", IconType = "Indicator" });
        volume.Children.Add(new NavigatorItem { Name = "Accumulation/Distribution", IconType = "Indicator" });
        volume.Children.Add(new NavigatorItem { Name = "Volumes", IconType = "Indicator" });
        indicators.Children.Add(volume);

        var billWilliams = new NavigatorItem { Name = "Bill Williams", IconType = "Folder" };
        billWilliams.Children.Add(new NavigatorItem { Name = "Alligator", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Awesome Oscillator", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Fractals", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Gator Oscillator", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Market Facilitation Index", IconType = "Indicator" });
        billWilliams.Children.Add(new NavigatorItem { Name = "Accelerator Oscillator", IconType = "Indicator" });
        indicators.Children.Add(billWilliams);

        Items.Add(indicators);

        // Expert Advisors
        var experts = new NavigatorItem { Name = "Expert Advisors", IconType = "Expert", IsExpanded = false };
        experts.Children.Add(new NavigatorItem { Name = "ExpertMACD", IconType = "Expert" });
        experts.Children.Add(new NavigatorItem { Name = "ExpertMA Crossover", IconType = "Expert" });
        Items.Add(experts);

        // Scripts
        var scripts = new NavigatorItem { Name = "Scripts", IconType = "Script", IsExpanded = false };
        scripts.Children.Add(new NavigatorItem { Name = "CloseAll", IconType = "Script" });
        scripts.Children.Add(new NavigatorItem { Name = "PendingGrid", IconType = "Script" });
        Items.Add(scripts);
    }
}
