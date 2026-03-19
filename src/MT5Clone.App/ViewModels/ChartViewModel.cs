using System.Collections.ObjectModel;
using System.Windows.Input;
using MT5Clone.App.Helpers;
using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;
using MT5Clone.OpenAlgo.Services;

namespace MT5Clone.App.ViewModels;

public class ChartViewModel : ViewModelBase
{
    private readonly MarketDataService _marketDataService;
    private readonly OpenAlgoService _openAlgoService;
    private bool _isOpenAlgoMode;
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
    private double _currentOpen;
    private double _currentHigh;
    private double _currentLow;
    private double _currentClose;
    private string _selectedSymbol = "EURUSD";
    private string _selectedTimeFrame = "H1";
    private string _chartTypeDisplay = "Candlestick";
    private string _crosshairInfo = string.Empty;
    private bool _hasIndicators;

    public ObservableCollection<Candle> Candles { get; } = new();
    public ObservableCollection<IIndicator> Indicators { get; } = new();
    public ObservableCollection<IDrawingTool> DrawingTools { get; } = new();
    public ObservableCollection<IndicatorDisplayItem> ActiveIndicators { get; } = new();

    public string Symbol
    {
        get => _symbol;
        set => SetProperty(ref _symbol, value);
    }

    public string SelectedSymbol
    {
        get => _selectedSymbol;
        set => SetProperty(ref _selectedSymbol, value);
    }

    public string SelectedTimeFrame
    {
        get => _selectedTimeFrame;
        set => SetProperty(ref _selectedTimeFrame, value);
    }

    public string ChartTypeDisplay
    {
        get => _chartTypeDisplay;
        set => SetProperty(ref _chartTypeDisplay, value);
    }

    public double CurrentOpen
    {
        get => _currentOpen;
        set => SetProperty(ref _currentOpen, value);
    }

    public double CurrentHigh
    {
        get => _currentHigh;
        set => SetProperty(ref _currentHigh, value);
    }

    public double CurrentLow
    {
        get => _currentLow;
        set => SetProperty(ref _currentLow, value);
    }

    public double CurrentClose
    {
        get => _currentClose;
        set => SetProperty(ref _currentClose, value);
    }

    public string CrosshairInfo
    {
        get => _crosshairInfo;
        set => SetProperty(ref _crosshairInfo, value);
    }

    public bool HasIndicators
    {
        get => _hasIndicators;
        set => SetProperty(ref _hasIndicators, value);
    }

    public TimeFrame TimeFrame
    {
        get => _timeFrame;
        set
        {
            SetProperty(ref _timeFrame, value);
            TimeFrameText = value.ToString();
            SelectedTimeFrame = value.ToString();
            LoadCandles();
        }
    }

    public ChartType ChartType
    {
        get => _chartType;
        set
        {
            SetProperty(ref _chartType, value);
            ChartTypeDisplay = value.ToString();
        }
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

    public ChartViewModel(MarketDataService marketDataService, OpenAlgoService openAlgoService)
    {
        _marketDataService = marketDataService;
        _openAlgoService = openAlgoService;

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

    public void SwitchToOpenAlgo()
    {
        _isOpenAlgoMode = true;
        // Load first available OpenAlgo symbol
        if (_openAlgoService.MarketData != null)
        {
            var symbols = _openAlgoService.MarketData.GetSymbols();
            if (symbols.Count > 0)
            {
                SetSymbol(symbols[0].Name);
            }
        }
    }

    public void SwitchToSimulated()
    {
        _isOpenAlgoMode = false;
        SetSymbol("EURUSD");
    }

    public void SetSymbol(string symbol)
    {
        Symbol = symbol;
        SelectedSymbol = symbol;

        if (_isOpenAlgoMode && _openAlgoService.MarketData != null)
        {
            var symbolInfo = _openAlgoService.MarketData.GetSymbol(symbol);
            if (symbolInfo != null)
            {
                Digits = symbolInfo.Digits;
            }
        }
        else
        {
            var symbolInfo = _marketDataService.GetSymbol(symbol);
            if (symbolInfo != null)
            {
                Digits = symbolInfo.Digits;
            }
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

    private async void LoadCandles()
    {
        List<Candle> candles;

        if (_isOpenAlgoMode && _openAlgoService.MarketData != null)
        {
            // Load real historical data from OpenAlgo
            var endDate = DateTime.UtcNow;
            var startDate = TimeFrame switch
            {
                TimeFrame.M1 or TimeFrame.M2 or TimeFrame.M3 => endDate.AddDays(-2),
                TimeFrame.M5 or TimeFrame.M6 => endDate.AddDays(-5),
                TimeFrame.M10 or TimeFrame.M12 or TimeFrame.M15 => endDate.AddDays(-10),
                TimeFrame.M20 or TimeFrame.M30 => endDate.AddDays(-30),
                TimeFrame.H1 or TimeFrame.H2 or TimeFrame.H3 => endDate.AddDays(-60),
                TimeFrame.H4 or TimeFrame.H6 or TimeFrame.H8 or TimeFrame.H12 => endDate.AddDays(-180),
                TimeFrame.D1 => endDate.AddYears(-2),
                TimeFrame.W1 => endDate.AddYears(-5),
                TimeFrame.MN1 => endDate.AddYears(-10),
                _ => endDate.AddDays(-30)
            };

            candles = await _openAlgoService.MarketData.LoadHistoryAsync(Symbol, TimeFrame, startDate, endDate);
        }
        else
        {
            candles = _marketDataService.GetCandles(Symbol, TimeFrame, 500);
        }

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
        if (_isOpenAlgoMode) return;
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
        CurrentOpen = last.Open;
        CurrentHigh = last.High;
        CurrentLow = last.Low;
        CurrentClose = last.Close;
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
            List<Candle> candles;
            if (_isOpenAlgoMode && _openAlgoService.MarketData != null)
            {
                candles = _openAlgoService.MarketData.GetCandles(Symbol, TimeFrame, 500);
            }
            else
            {
                candles = _marketDataService.GetCandles(Symbol, TimeFrame, 500);
            }
            indicator.Calculate(candles);
            Indicators.Add(indicator);
            ActiveIndicators.Add(new IndicatorDisplayItem { Name = indicatorName, Indicator = indicator });
            HasIndicators = ActiveIndicators.Count > 0;
        }
    }

    private void RemoveIndicator(IIndicator? indicator)
    {
        if (indicator != null)
        {
            Indicators.Remove(indicator);
            var displayItem = ActiveIndicators.FirstOrDefault(i => i.Indicator == indicator);
            if (displayItem != null) ActiveIndicators.Remove(displayItem);
            HasIndicators = ActiveIndicators.Count > 0;
        }
    }

    private void AddDrawingTool(string? toolName) { }
    private void RemoveDrawingTool(IDrawingTool? tool) { }
    private void ShowChartProperties() { }
}

public class IndicatorDisplayItem : ViewModelBase
{
    public string Name { get; set; } = string.Empty;
    public IIndicator? Indicator { get; set; }
}
