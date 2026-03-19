using System.Collections.ObjectModel;
using System.Windows.Input;
using MT5Clone.App.Helpers;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;

namespace MT5Clone.App.ViewModels;

public class MarketWatchSymbolItem : ViewModelBase
{
    private string _name = string.Empty;
    private double _bid;
    private double _ask;
    private double _spread;
    private double _change;
    private double _changePercent;
    private string _bidFormatted = string.Empty;
    private string _askFormatted = string.Empty;
    private int _digits;
    private bool _isBidUp;
    private bool _isAskUp;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public double Bid { get => _bid; set { var old = _bid; SetProperty(ref _bid, value); IsBidUp = value >= old; } }
    public double Ask { get => _ask; set { var old = _ask; SetProperty(ref _ask, value); IsAskUp = value >= old; } }
    public double Spread { get => _spread; set => SetProperty(ref _spread, value); }
    public double Change { get => _change; set => SetProperty(ref _change, value); }
    public double ChangePercent { get => _changePercent; set => SetProperty(ref _changePercent, value); }
    public string BidFormatted { get => _bidFormatted; set => SetProperty(ref _bidFormatted, value); }
    public string AskFormatted { get => _askFormatted; set => SetProperty(ref _askFormatted, value); }
    public int Digits { get => _digits; set => SetProperty(ref _digits, value); }
    public bool IsBidUp { get => _isBidUp; set => SetProperty(ref _isBidUp, value); }
    public bool IsAskUp { get => _isAskUp; set => SetProperty(ref _isAskUp, value); }
}

public class MarketWatchViewModel : ViewModelBase
{
    private readonly MarketDataService _marketDataService;
    private bool _isVisible = true;
    private MarketWatchSymbolItem? _selectedSymbol;
    private bool _showTickChart;
    private bool _showDetails;
    private string _searchFilter = string.Empty;

    public ObservableCollection<MarketWatchSymbolItem> Symbols { get; } = new();
    public ObservableCollection<MarketDepthEntry> DepthEntries { get; } = new();

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public MarketWatchSymbolItem? SelectedSymbol
    {
        get => _selectedSymbol;
        set
        {
            SetProperty(ref _selectedSymbol, value);
            if (value != null)
            {
                _marketDataService.SubscribeMarketDepth(value.Name);
                UpdateDepth(value.Name);
            }
        }
    }

    public bool ShowTickChart
    {
        get => _showTickChart;
        set => SetProperty(ref _showTickChart, value);
    }

    public bool ShowDetails
    {
        get => _showDetails;
        set => SetProperty(ref _showDetails, value);
    }

    public string SearchFilter
    {
        get => _searchFilter;
        set
        {
            SetProperty(ref _searchFilter, value);
            FilterSymbols();
        }
    }

    public ICommand ShowSymbolsCommand { get; }
    public ICommand HideSymbolCommand { get; }
    public ICommand ShowAllCommand { get; }
    public ICommand SymbolSpecificationCommand { get; }
    public ICommand ToggleTickChartCommand { get; }
    public ICommand ToggleDetailsCommand { get; }

    public MarketWatchViewModel(MarketDataService marketDataService)
    {
        _marketDataService = marketDataService;

        ShowSymbolsCommand = new RelayCommand(ShowSymbols);
        HideSymbolCommand = new RelayCommand(HideSelectedSymbol);
        ShowAllCommand = new RelayCommand(ShowAllSymbols);
        SymbolSpecificationCommand = new RelayCommand(ShowSpecification);
        ToggleTickChartCommand = new RelayCommand(() => ShowTickChart = !ShowTickChart);
        ToggleDetailsCommand = new RelayCommand(() => ShowDetails = !ShowDetails);

        InitializeSymbols();

        _marketDataService.TickReceived += OnTickReceived;
        _marketDataService.MarketDepthUpdated += OnMarketDepthUpdated;
    }

    private void InitializeSymbols()
    {
        foreach (var symbol in _marketDataService.GetSymbols())
        {
            Symbols.Add(new MarketWatchSymbolItem
            {
                Name = symbol.Name,
                Bid = symbol.Bid,
                Ask = symbol.Ask,
                Digits = symbol.Digits,
                BidFormatted = symbol.FormatPrice(symbol.Bid),
                AskFormatted = symbol.FormatPrice(symbol.Ask),
                Spread = symbol.Spread
            });
        }
    }

    private void OnTickReceived(object? sender, TickEventArgs e)
    {
        var item = Symbols.FirstOrDefault(s => s.Name == e.Tick.Symbol);
        if (item == null) return;

        var symbol = _marketDataService.GetSymbol(e.Tick.Symbol);
        if (symbol == null) return;

        item.Bid = e.Tick.Bid;
        item.Ask = e.Tick.Ask;
        item.BidFormatted = symbol.FormatPrice(e.Tick.Bid);
        item.AskFormatted = symbol.FormatPrice(e.Tick.Ask);
        item.Spread = (e.Tick.Ask - e.Tick.Bid) / symbol.Point;

        if (symbol.PreviousClose > 0)
        {
            item.Change = e.Tick.Bid - symbol.PreviousClose;
            item.ChangePercent = (item.Change / symbol.PreviousClose) * 100;
        }
    }

    private void OnMarketDepthUpdated(object? sender, MarketDepthEventArgs e)
    {
        if (SelectedSymbol?.Name != e.MarketDepth.Symbol) return;
        UpdateDepth(e.MarketDepth.Symbol);
    }

    private void UpdateDepth(string symbolName)
    {
        var depth = _marketDataService.GetMarketDepth(symbolName);
        if (depth == null) return;

        DepthEntries.Clear();
        foreach (var entry in depth.Entries.OrderByDescending(e => e.Price))
        {
            DepthEntries.Add(entry);
        }
    }

    private void FilterSymbols()
    {
        // Re-filter based on search
    }

    private void ShowSymbols() { }
    private void HideSelectedSymbol() { }
    private void ShowAllSymbols()
    {
        Symbols.Clear();
        InitializeSymbols();
    }
    private void ShowSpecification() { }
}
