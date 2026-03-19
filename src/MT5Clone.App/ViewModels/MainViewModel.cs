using System.Collections.ObjectModel;
using System.Windows.Input;
using MT5Clone.App.Helpers;
using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;
using MT5Clone.Trading.Services;

namespace MT5Clone.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly MarketDataService _marketDataService;
    private readonly TradingEngine _tradingEngine;
    private readonly AlertService _alertService;

    private string _title = "MT5Clone Terminal";
    private string _statusText = "Disconnected";
    private string _connectionStatus = "Disconnected";
    private bool _isConnected;

    public MarketWatchViewModel MarketWatch { get; }
    public ChartViewModel Chart { get; }
    public TerminalViewModel Terminal { get; }
    public NavigatorViewModel Navigator { get; }
    public ToolboxViewModel Toolbox { get; }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    // Commands
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand NewOrderCommand { get; }
    public ICommand NewChartCommand { get; }
    public ICommand ToggleMarketWatchCommand { get; }
    public ICommand ToggleNavigatorCommand { get; }
    public ICommand ToggleTerminalCommand { get; }
    public ICommand ToggleToolboxCommand { get; }
    public ICommand ExitCommand { get; }

    // Timeframe commands
    public ICommand TimeFrameM1Command { get; }
    public ICommand TimeFrameM5Command { get; }
    public ICommand TimeFrameM15Command { get; }
    public ICommand TimeFrameM30Command { get; }
    public ICommand TimeFrameH1Command { get; }
    public ICommand TimeFrameH4Command { get; }
    public ICommand TimeFrameD1Command { get; }
    public ICommand TimeFrameW1Command { get; }
    public ICommand TimeFrameMN1Command { get; }

    // Chart type commands
    public ICommand ChartTypeCandlesCommand { get; }
    public ICommand ChartTypeBarsCommand { get; }
    public ICommand ChartTypeLineCommand { get; }

    // Zoom commands
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand AutoScrollCommand { get; }

    public MainViewModel()
    {
        _marketDataService = new MarketDataService();
        _tradingEngine = new TradingEngine();
        _alertService = new AlertService();

        // Register symbols with trading engine
        foreach (var symbol in _marketDataService.GetSymbols())
        {
            _tradingEngine.RegisterSymbol(symbol);
        }

        // Wire up tick processing
        _marketDataService.TickReceived += (s, e) =>
        {
            _tradingEngine.ProcessTick(e.Tick);
            _alertService.ProcessTick(e.Tick);
        };

        // Create sub view models
        MarketWatch = new MarketWatchViewModel(_marketDataService);
        Chart = new ChartViewModel(_marketDataService);
        Terminal = new TerminalViewModel(_tradingEngine, _marketDataService);
        Navigator = new NavigatorViewModel();
        Toolbox = new ToolboxViewModel(_marketDataService);

        // Commands
        ConnectCommand = new RelayCommand(Connect);
        DisconnectCommand = new RelayCommand(Disconnect);
        NewOrderCommand = new RelayCommand(OpenNewOrder);
        NewChartCommand = new RelayCommand(OpenNewChart);
        ToggleMarketWatchCommand = new RelayCommand(() => MarketWatch.IsVisible = !MarketWatch.IsVisible);
        ToggleNavigatorCommand = new RelayCommand(() => Navigator.IsVisible = !Navigator.IsVisible);
        ToggleTerminalCommand = new RelayCommand(() => Terminal.IsVisible = !Terminal.IsVisible);
        ToggleToolboxCommand = new RelayCommand(() => Toolbox.IsVisible = !Toolbox.IsVisible);
        ExitCommand = new RelayCommand(() => Environment.Exit(0));

        // Timeframe commands
        TimeFrameM1Command = new RelayCommand(() => Chart.SetTimeFrame(TimeFrame.M1));
        TimeFrameM5Command = new RelayCommand(() => Chart.SetTimeFrame(TimeFrame.M5));
        TimeFrameM15Command = new RelayCommand(() => Chart.SetTimeFrame(TimeFrame.M15));
        TimeFrameM30Command = new RelayCommand(() => Chart.SetTimeFrame(TimeFrame.M30));
        TimeFrameH1Command = new RelayCommand(() => Chart.SetTimeFrame(TimeFrame.H1));
        TimeFrameH4Command = new RelayCommand(() => Chart.SetTimeFrame(TimeFrame.H4));
        TimeFrameD1Command = new RelayCommand(() => Chart.SetTimeFrame(TimeFrame.D1));
        TimeFrameW1Command = new RelayCommand(() => Chart.SetTimeFrame(TimeFrame.W1));
        TimeFrameMN1Command = new RelayCommand(() => Chart.SetTimeFrame(TimeFrame.MN1));

        // Chart type commands
        ChartTypeCandlesCommand = new RelayCommand(() => Chart.SetChartType(ChartType.Candlestick));
        ChartTypeBarsCommand = new RelayCommand(() => Chart.SetChartType(ChartType.Bar));
        ChartTypeLineCommand = new RelayCommand(() => Chart.SetChartType(ChartType.Line));

        // Zoom commands
        ZoomInCommand = new RelayCommand(() => Chart.ZoomIn());
        ZoomOutCommand = new RelayCommand(() => Chart.ZoomOut());
        AutoScrollCommand = new RelayCommand(() => Chart.ToggleAutoScroll());

        // Set initial chart
        Chart.SetSymbol("EURUSD");
    }

    private async void Connect()
    {
        try
        {
            StatusText = "Connecting...";
            ConnectionStatus = "Connecting";
            await _marketDataService.StartAsync();
            IsConnected = true;
            ConnectionStatus = "Connected";
            StatusText = "Connected to MT5Clone-Demo";
            Title = "MT5Clone Terminal - 12345678 - MT5Clone-Demo";
        }
        catch (Exception ex)
        {
            StatusText = $"Connection failed: {ex.Message}";
            ConnectionStatus = "Failed";
        }
    }

    private async void Disconnect()
    {
        await _marketDataService.StopAsync();
        IsConnected = false;
        ConnectionStatus = "Disconnected";
        StatusText = "Disconnected";
        Title = "MT5Clone Terminal";
    }

    private void OpenNewOrder()
    {
        Terminal.ShowOrderDialog(MarketWatch.SelectedSymbol?.Name ?? "EURUSD");
    }

    private void OpenNewChart()
    {
        var symbol = MarketWatch.SelectedSymbol?.Name ?? "EURUSD";
        Chart.SetSymbol(symbol);
    }
}
