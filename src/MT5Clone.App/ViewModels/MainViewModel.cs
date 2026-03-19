using System.Collections.ObjectModel;
using System.Windows.Input;
using MT5Clone.App.Helpers;
using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;
using MT5Clone.Trading.Services;
using MT5Clone.OpenAlgo.Models;
using MT5Clone.OpenAlgo.Services;

namespace MT5Clone.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly MarketDataService _marketDataService;
    private readonly TradingEngine _tradingEngine;
    private readonly AlertService _alertService;
    private readonly OpenAlgoService _openAlgoService;

    private string _title = "MT5Clone Terminal";
    private string _statusText = "Disconnected";
    private string _connectionStatus = "Disconnected";
    private bool _isConnected;
    private bool _isOpenAlgoMode;
    private bool _isOpenAlgoConnected;
    private string _openAlgoHost = "http://127.0.0.1:5000";
    private string _openAlgoApiKey = string.Empty;
    private string _openAlgoStrategy = "MT5Clone";
    private bool _showConnectionDialog;

    public MarketWatchViewModel MarketWatch { get; }
    public ChartViewModel Chart { get; }
    public TerminalViewModel Terminal { get; }
    public NavigatorViewModel Navigator { get; }
    public ToolboxViewModel Toolbox { get; }
    public OpenAlgoService OpenAlgo => _openAlgoService;

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

    public bool IsOpenAlgoMode
    {
        get => _isOpenAlgoMode;
        set => SetProperty(ref _isOpenAlgoMode, value);
    }

    public bool IsOpenAlgoConnected
    {
        get => _isOpenAlgoConnected;
        set => SetProperty(ref _isOpenAlgoConnected, value);
    }

    public string OpenAlgoHost
    {
        get => _openAlgoHost;
        set => SetProperty(ref _openAlgoHost, value);
    }

    public string OpenAlgoApiKey
    {
        get => _openAlgoApiKey;
        set => SetProperty(ref _openAlgoApiKey, value);
    }

    public string OpenAlgoStrategy
    {
        get => _openAlgoStrategy;
        set => SetProperty(ref _openAlgoStrategy, value);
    }

    public bool ShowConnectionDialog
    {
        get => _showConnectionDialog;
        set => SetProperty(ref _showConnectionDialog, value);
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

    // OpenAlgo commands
    public ICommand ConnectOpenAlgoCommand { get; }
    public ICommand DisconnectOpenAlgoCommand { get; }
    public ICommand ShowConnectionDialogCommand { get; }
    public ICommand HideConnectionDialogCommand { get; }

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
        _openAlgoService = new OpenAlgoService();

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

        // Wire up OpenAlgo events
        _openAlgoService.ConnectionStatusChanged += OnOpenAlgoConnectionStatusChanged;
        _openAlgoService.LogMessage += OnOpenAlgoLogMessage;

        // Create sub view models
        MarketWatch = new MarketWatchViewModel(_marketDataService, _openAlgoService);
        Chart = new ChartViewModel(_marketDataService, _openAlgoService);
        Terminal = new TerminalViewModel(_tradingEngine, _marketDataService, _openAlgoService);
        Navigator = new NavigatorViewModel(_openAlgoService);
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

        // OpenAlgo commands
        ConnectOpenAlgoCommand = new RelayCommand(ConnectOpenAlgo);
        DisconnectOpenAlgoCommand = new RelayCommand(DisconnectOpenAlgo);
        ShowConnectionDialogCommand = new RelayCommand(() => ShowConnectionDialog = true);
        HideConnectionDialogCommand = new RelayCommand(() => ShowConnectionDialog = false);

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
        if (IsOpenAlgoMode)
        {
            DisconnectOpenAlgo();
            return;
        }
        await _marketDataService.StopAsync();
        IsConnected = false;
        ConnectionStatus = "Disconnected";
        StatusText = "Disconnected";
        Title = "MT5Clone Terminal";
    }

    private async void ConnectOpenAlgo()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(OpenAlgoApiKey))
            {
                StatusText = "Please enter an OpenAlgo API Key";
                return;
            }

            StatusText = "Connecting to OpenAlgo...";
            ConnectionStatus = "Connecting";
            ShowConnectionDialog = false;

            _openAlgoService.UpdateConfig(OpenAlgoApiKey, OpenAlgoHost, OpenAlgoStrategy);

            var success = await _openAlgoService.ConnectAsync();
            if (success)
            {
                IsOpenAlgoMode = true;
                IsOpenAlgoConnected = true;
                IsConnected = true;
                ConnectionStatus = "OpenAlgo Connected";
                StatusText = $"Connected to OpenAlgo ({OpenAlgoHost})";
                Title = $"MT5Clone Terminal - OpenAlgo - {OpenAlgoHost}";

                // Update sub view models to use OpenAlgo data
                MarketWatch.SwitchToOpenAlgo();
                Chart.SwitchToOpenAlgo();
                Terminal.SwitchToOpenAlgo();
                Navigator.UpdateForOpenAlgo();
            }
            else
            {
                StatusText = "OpenAlgo connection failed. Check API key and server.";
                ConnectionStatus = "Failed";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"OpenAlgo connection failed: {ex.Message}";
            ConnectionStatus = "Failed";
        }
    }

    private async void DisconnectOpenAlgo()
    {
        await _openAlgoService.DisconnectAsync();
        IsOpenAlgoMode = false;
        IsOpenAlgoConnected = false;
        IsConnected = false;
        ConnectionStatus = "Disconnected";
        StatusText = "Disconnected from OpenAlgo";
        Title = "MT5Clone Terminal";

        MarketWatch.SwitchToSimulated();
        Chart.SwitchToSimulated();
        Terminal.SwitchToSimulated();
    }

    private void OnOpenAlgoConnectionStatusChanged(object? sender, ConnectionStatusEventArgs e)
    {
        IsOpenAlgoConnected = e.IsConnected;
        ConnectionStatus = e.IsConnected ? "OpenAlgo Connected" : e.Message;
    }

    private void OnOpenAlgoLogMessage(object? sender, string message)
    {
        Terminal.AddJournalEntry(message);
    }

    private void OpenNewOrder()
    {
        var symbol = MarketWatch.SelectedSymbol?.Name ?? (IsOpenAlgoMode ? "NSE:RELIANCE" : "EURUSD");
        Terminal.ShowOrderDialog(symbol);
    }

    private void OpenNewChart()
    {
        var symbol = MarketWatch.SelectedSymbol?.Name ?? (IsOpenAlgoMode ? "NSE:RELIANCE" : "EURUSD");
        Chart.SetSymbol(symbol);
    }
}
