using System.Collections.ObjectModel;
using System.Windows.Input;
using MT5Clone.App.Helpers;
using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;

namespace MT5Clone.App.ViewModels;

public class ChartViewModel : ViewModelBase
{
    private readonly MarketDataService _marketDataService;
    private string _symbol = "EURUSD";
    private TimeFrame _timeFrame = TimeFrame.H1;
    private ChartType _chartType = ChartType.Candlestick;
    private bool _autoScroll = true;
    private bool _showGrid = true;
    private bool _showVolume = true;
    private bool _showCrosshair = true;
    private bool _showOHLC = true;
    private bool _chartShift = true;
    private double _zoomLevel = 1.0;
    private string _ohlcText = string.Empty;
    private string _timeFrameText = "H1";
    private int _digits = 5;

    public ObservableCollection<Candle> Candles { get; } = new();
    public ObservableCollection<IIndicator> Indicators { get; } = new();
    public ObservableCollection<IDrawingTool> DrawingTools { get; } = new();

    public string Symbol
    {
        get => _symbol;
        set => SetProperty(ref _symbol, value);
    }

    public TimeFrame TimeFrame
    {
        get => _timeFrame;
        set
        {
            SetProperty(ref _timeFrame, value);
            TimeFrameText = value.ToString();
            LoadCandles();
        }
    }

    public ChartType ChartType
    {
        get => _chartType;
        set => SetProperty(ref _chartType, value);
    }

    public bool AutoScroll
    {
        get => _autoScroll;
        set => SetProperty(ref _autoScroll, value);
    }

    public bool ShowGrid
    {
        get => _showGrid;
        set => SetProperty(ref _showGrid, value);
    }

    public bool ShowVolume
    {
        get => _showVolume;
        set => SetProperty(ref _showVolume, value);
    }

    public bool ShowCrosshair
    {
        get => _showCrosshair;
        set => SetProperty(ref _showCrosshair, value);
    }

    public bool ShowOHLC
    {
        get => _showOHLC;
        set => SetProperty(ref _showOHLC, value);
    }

    public bool ChartShift
    {
        get => _chartShift;
        set => SetProperty(ref _chartShift, value);
    }

    public double ZoomLevel
    {
        get => _zoomLevel;
        set => SetProperty(ref _zoomLevel, Math.Max(0.1, Math.Min(5.0, value)));
    }

    public string OHLCText
    {
        get => _ohlcText;
        set => SetProperty(ref _ohlcText, value);
    }

    public string TimeFrameText
    {
        get => _timeFrameText;
        set => SetProperty(ref _timeFrameText, value);
    }

    public int Digits
    {
        get => _digits;
        set => SetProperty(ref _digits, value);
    }

    public ICommand AddIndicatorCommand { get; }
    public ICommand RemoveIndicatorCommand { get; }
    public ICommand AddDrawingToolCommand { get; }
    public ICommand RemoveDrawingToolCommand { get; }
    public ICommand ToggleGridCommand { get; }
    public ICommand ToggleVolumeCommand { get; }
    public ICommand ToggleCrosshairCommand { get; }
    public ICommand ChartPropertiesCommand { get; }
    public ICommand RefreshCommand { get; }

    public ChartViewModel(MarketDataService marketDataService)
    {
        _marketDataService = marketDataService;

        AddIndicatorCommand = new RelayCommand<string>(AddIndicator);
        RemoveIndicatorCommand = new RelayCommand<IIndicator>(RemoveIndicator);
        AddDrawingToolCommand = new RelayCommand<string>(AddDrawingTool);
        RemoveDrawingToolCommand = new RelayCommand<IDrawingTool>(RemoveDrawingTool);
        ToggleGridCommand = new RelayCommand(() => ShowGrid = !ShowGrid);
        ToggleVolumeCommand = new RelayCommand(() => ShowVolume = !ShowVolume);
        ToggleCrosshairCommand = new RelayCommand(() => ShowCrosshair = !ShowCrosshair);
        ChartPropertiesCommand = new RelayCommand(ShowChartProperties);
        RefreshCommand = new RelayCommand(LoadCandles);

        _marketDataService.CandleUpdated += OnCandleUpdated;
    }

    public void SetSymbol(string symbol)
    {
        Symbol = symbol;
        var symbolInfo = _marketDataService.GetSymbol(symbol);
        if (symbolInfo != null)
        {
            Digits = symbolInfo.Digits;
        }
        LoadCandles();
    }

    public void SetTimeFrame(TimeFrame timeFrame)
    {
        TimeFrame = timeFrame;
    }

    public void SetChartType(ChartType chartType)
    {
        ChartType = chartType;
    }

    public void ZoomIn()
    {
        ZoomLevel *= 1.2;
    }

    public void ZoomOut()
    {
        ZoomLevel /= 1.2;
    }

    public void ToggleAutoScroll()
    {
        AutoScroll = !AutoScroll;
    }

    private void LoadCandles()
    {
        var candles = _marketDataService.GetCandles(Symbol, TimeFrame, 500);
        Candles.Clear();
        foreach (var candle in candles)
        {
            Candles.Add(candle);
        }

        // Recalculate indicators
        foreach (var indicator in Indicators)
        {
            indicator.Calculate(candles);
        }

        UpdateOHLCText();
    }

    private void OnCandleUpdated(object? sender, CandleEventArgs e)
    {
        if (e.Symbol != Symbol || e.TimeFrame != TimeFrame) return;

        if (e.IsNewCandle)
        {
            Candles.Add(e.Candle);
        }
        else if (Candles.Count > 0)
        {
            Candles[Candles.Count - 1] = e.Candle;
        }

        UpdateOHLCText();
    }

    private void UpdateOHLCText()
    {
        if (Candles.Count == 0) return;

        var last = Candles[Candles.Count - 1];
        OHLCText = $"{Symbol} {TimeFrameText}  O:{last.Open.ToString($"F{Digits}")}  H:{last.High.ToString($"F{Digits}")}  L:{last.Low.ToString($"F{Digits}")}  C:{last.Close.ToString($"F{Digits}")}  V:{last.TickVolume}";
    }

    private void AddIndicator(string? indicatorName)
    {
        if (string.IsNullOrEmpty(indicatorName)) return;

        IIndicator? indicator = indicatorName switch
        {
            "MA" => new Indicators.Trend.MovingAverage(),
            "BB" => new Indicators.Trend.BollingerBands(),
            "RSI" => new Indicators.Oscillators.RSI(),
            "MACD" => new Indicators.Oscillators.MACD(),
            "Stochastic" => new Indicators.Oscillators.Stochastic(),
            "CCI" => new Indicators.Oscillators.CCI(),
            "ADX" => new Indicators.Trend.ADX(),
            "SAR" => new Indicators.Trend.ParabolicSAR(),
            "Ichimoku" => new Indicators.Trend.Ichimoku(),
            "WPR" => new Indicators.Oscillators.WilliamsR(),
            "Momentum" => new Indicators.Oscillators.MomentumIndicator(),
            "OBV" => new Indicators.Volume.OBV(),
            "MFI" => new Indicators.Volume.MoneyFlowIndex(),
            "Volumes" => new Indicators.Volume.VolumeIndicator(),
            _ => null
        };

        if (indicator != null)
        {
            var candles = _marketDataService.GetCandles(Symbol, TimeFrame, 500);
            indicator.Calculate(candles);
            Indicators.Add(indicator);
        }
    }

    private void RemoveIndicator(IIndicator? indicator)
    {
        if (indicator != null)
            Indicators.Remove(indicator);
    }

    private void AddDrawingTool(string? toolName) { }
    private void RemoveDrawingTool(IDrawingTool? tool) { }
    private void ShowChartProperties() { }
}
