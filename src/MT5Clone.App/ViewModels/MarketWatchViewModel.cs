using System.Collections.ObjectModel;
using System.Windows.Input;
using MT5Clone.App.Helpers;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;
using MT5Clone.OpenAlgo.Services;

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
    private double _high;
    private double _low;
    private double _last;
    private long _volume;
    private string _exchange = string.Empty;

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
    public double High { get => _high; set => SetProperty(ref _high, value); }
    public double Low { get => _low; set => SetProperty(ref _low, value); }
    public double Last { get => _last; set => SetProperty(ref _last, value); }
    public long Volume { get => _volume; set => SetProperty(ref _volume, value); }
    public string Exchange { get => _exchange; set => SetProperty(ref _exchange, value); }

    // Computed display property
    public string Symbol => Name.Contains(':') ? Name.Split(':')[1] : Name;
}

public class MarketWatchViewModel : ViewModelBase
{
    private readonly MarketDataService _marketDataService;
    private readonly OpenAlgoService _openAlgoService;
    private bool _isVisible = true;
    private bool _isOpenAlgoMode;
    private MarketWatchSymbolItem? _selectedSymbol;
    private bool _showTickChart;
    private bool _showDetails;
    private string _searchFilter = string.Empty;
    private string _filterText = string.Empty;

    public ObservableCollection<MarketWatchSymbolItem> Symbols { get; } = new();
    public ObservableCollection<MarketWatchSymbolItem> FilteredSymbols { get; } = new();
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
                if (_isOpenAlgoMode && _openAlgoService.MarketData != null)
                {
                    _openAlgoService.MarketData.SubscribeSymbol(value.Name);
                    _openAlgoService.MarketData.SubscribeMarketDepth(value.Name);
                }
                else
                {
                    _marketDataService.SubscribeMarketDepth(value.Name);
                }
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

    public string FilterText
    {
        get => _filterText;
        set
        {
            SetProperty(ref _filterText, value);
            FilterSymbols();
        }
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
    public ICommand SearchOpenAlgoCommand { get; }

    public MarketWatchViewModel(MarketDataService marketDataService, OpenAlgoService openAlgoService)
    {
        _marketDataService = marketDataService;
        _openAlgoService = openAlgoService;

        ShowSymbolsCommand = new RelayCommand(ShowSymbols);
        HideSymbolCommand = new RelayCommand(HideSelectedSymbol);
        ShowAllCommand = new RelayCommand(ShowAllSymbols);
        SymbolSpecificationCommand = new RelayCommand(ShowSpecification);
        ToggleTickChartCommand = new RelayCommand(() => ShowTickChart = !ShowTickChart);
        ToggleDetailsCommand = new RelayCommand(() => ShowDetails = !ShowDetails);
        SearchOpenAlgoCommand = new RelayCommand(SearchOpenAlgoSymbols);

        InitializeSymbols();

        _marketDataService.TickReceived += OnTickReceived;
        _marketDataService.MarketDepthUpdated += OnMarketDepthUpdated;
    }

    public void SwitchToOpenAlgo()
    {
        _isOpenAlgoMode = true;
        Symbols.Clear();
        FilteredSymbols.Clear();

        if (_openAlgoService.MarketData != null)
        {
            // Load OpenAlgo symbols
            foreach (var symbol in _openAlgoService.MarketData.GetSymbols())
            {
                var item = new MarketWatchSymbolItem
                {
                    Name = symbol.Name,
                    Bid = symbol.Bid,
                    Ask = symbol.Ask,
                    Digits = symbol.Digits,
                    High = symbol.DayHigh,
                    Low = symbol.DayLow,
                    Last = symbol.Last,
                    Volume = symbol.Volume,
                    Exchange = symbol.Path?.Split('\\')[0] ?? "",
                    BidFormatted = symbol.FormatPrice(symbol.Bid),
                    AskFormatted = symbol.FormatPrice(symbol.Ask),
                    Spread = symbol.Point > 0 ? (symbol.Ask - symbol.Bid) / symbol.Point : 0
                };
                Symbols.Add(item);
                FilteredSymbols.Add(item);

                // Subscribe to quotes
                _openAlgoService.MarketData.SubscribeSymbol(symbol.Name);
            }

            // Wire up OpenAlgo tick events
            _openAlgoService.MarketData.TickReceived += OnOpenAlgoTickReceived;
            _openAlgoService.MarketData.MarketDepthUpdated += OnOpenAlgoMarketDepthUpdated;
        }
    }

    public void SwitchToSimulated()
    {
        _isOpenAlgoMode = false;

        if (_openAlgoService.MarketData != null)
        {
            _openAlgoService.MarketData.TickReceived -= OnOpenAlgoTickReceived;
            _openAlgoService.MarketData.MarketDepthUpdated -= OnOpenAlgoMarketDepthUpdated;
        }

        Symbols.Clear();
        FilteredSymbols.Clear();
        InitializeSymbols();
    }

    private void InitializeSymbols()
    {
        foreach (var symbol in _marketDataService.GetSymbols())
        {
            var item = new MarketWatchSymbolItem
            {
                Name = symbol.Name,
                Bid = symbol.Bid,
                Ask = symbol.Ask,
                Digits = symbol.Digits,
                BidFormatted = symbol.FormatPrice(symbol.Bid),
                AskFormatted = symbol.FormatPrice(symbol.Ask),
                Spread = symbol.Spread
            };
            Symbols.Add(item);
            FilteredSymbols.Add(item);
        }
    }

    private void OnTickReceived(object? sender, TickEventArgs e)
    {
        if (_isOpenAlgoMode) return;

        var item = Symbols.FirstOrDefault(s => s.Name == e.Tick.Symbol);
        if (item == null) return;

        var symbol = _marketDataService.GetSymbol(e.Tick.Symbol);
        if (symbol == null) return;

        UpdateSymbolItem(item, e.Tick, symbol);
    }

    private void OnOpenAlgoTickReceived(object? sender, TickEventArgs e)
    {
        var item = Symbols.FirstOrDefault(s => s.Name == e.Tick.Symbol);
        if (item == null) return;

        var symbol = _openAlgoService.MarketData?.GetSymbol(e.Tick.Symbol);
        if (symbol == null) return;

        UpdateSymbolItem(item, e.Tick, symbol);
        item.Last = e.Tick.Last;
        item.High = symbol.DayHigh;
        item.Low = symbol.DayLow;
        item.Volume = symbol.Volume;
    }

    private static void UpdateSymbolItem(MarketWatchSymbolItem item, Tick tick, Symbol symbol)
    {
        item.Bid = tick.Bid;
        item.Ask = tick.Ask;
        item.BidFormatted = symbol.FormatPrice(tick.Bid);
        item.AskFormatted = symbol.FormatPrice(tick.Ask);
        item.Spread = symbol.Point > 0 ? (tick.Ask - tick.Bid) / symbol.Point : 0;

        if (symbol.PreviousClose > 0)
        {
            item.Change = tick.Bid - symbol.PreviousClose;
            item.ChangePercent = (item.Change / symbol.PreviousClose) * 100;
        }
    }

    private void OnMarketDepthUpdated(object? sender, MarketDepthEventArgs e)
    {
        if (_isOpenAlgoMode) return;
        if (SelectedSymbol?.Name != e.MarketDepth.Symbol) return;
        UpdateDepth(e.MarketDepth.Symbol);
    }

    private void OnOpenAlgoMarketDepthUpdated(object? sender, MarketDepthEventArgs e)
    {
        if (SelectedSymbol?.Name != e.MarketDepth.Symbol) return;
        UpdateDepthFromMarketDepth(e.MarketDepth);
    }

    private void UpdateDepth(string symbolName)
    {
        IMarketDataProvider provider = _isOpenAlgoMode && _openAlgoService.MarketData != null
            ? _openAlgoService.MarketData
            : _marketDataService;

        var depth = provider.GetMarketDepth(symbolName);
        if (depth == null) return;

        UpdateDepthFromMarketDepth(depth);
    }

    private void UpdateDepthFromMarketDepth(MarketDepth depth)
    {
        DepthEntries.Clear();
        foreach (var entry in depth.Entries.OrderByDescending(e => e.Price))
        {
            DepthEntries.Add(entry);
        }
    }

    private void FilterSymbols()
    {
        var filter = (FilterText ?? string.Empty).Trim();
        FilteredSymbols.Clear();

        foreach (var sym in Symbols)
        {
            if (string.IsNullOrEmpty(filter) ||
                sym.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                sym.Symbol.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                FilteredSymbols.Add(sym);
            }
        }
    }

    private async void SearchOpenAlgoSymbols()
    {
        if (!_isOpenAlgoMode || _openAlgoService.MarketData == null) return;
        if (string.IsNullOrWhiteSpace(FilterText)) return;

        var results = await _openAlgoService.MarketData.SearchAndAddSymbolsAsync(FilterText);
        foreach (var symbol in results)
        {
            if (Symbols.Any(s => s.Name == symbol.Name)) continue;

            var item = new MarketWatchSymbolItem
            {
                Name = symbol.Name,
                Bid = symbol.Bid,
                Ask = symbol.Ask,
                Digits = symbol.Digits,
                Last = symbol.Last,
                High = symbol.DayHigh,
                Low = symbol.DayLow,
                Volume = symbol.Volume,
                Exchange = symbol.Path?.Split('\\')[0] ?? "",
                BidFormatted = symbol.FormatPrice(symbol.Bid),
                AskFormatted = symbol.FormatPrice(symbol.Ask),
                Spread = symbol.Point > 0 ? (symbol.Ask - symbol.Bid) / symbol.Point : 0
            };
            Symbols.Add(item);
            FilteredSymbols.Add(item);

            _openAlgoService.MarketData.SubscribeSymbol(symbol.Name);
        }
    }

    private void ShowSymbols() { }
    private void HideSelectedSymbol() { }
    private void ShowAllSymbols()
    {
        Symbols.Clear();
        FilteredSymbols.Clear();
        if (_isOpenAlgoMode)
            SwitchToOpenAlgo();
        else
            InitializeSymbols();
    }
    private void ShowSpecification() { }
}
