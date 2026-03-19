using System.Collections.ObjectModel;
using System.Windows.Input;
using MT5Clone.App.Helpers;
using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Strategy.Backtesting;

namespace MT5Clone.App.ViewModels;

public class StrategyTesterViewModel : ViewModelBase
{
    private readonly BacktestEngine _backtestEngine;
    private string _symbol = "EURUSD";
    private TimeFrame _timeFrame = TimeFrame.H1;
    private DateTime _dateFrom;
    private DateTime _dateTo;
    private double _initialDeposit = 10000;
    private int _leverage = 100;
    private int _spread = 10;
    private double _progress;
    private string _progressText = "Ready";
    private bool _isRunning;

    // Results
    private double _netProfit;
    private double _grossProfit;
    private double _grossLoss;
    private double _profitFactor;
    private int _totalTrades;
    private int _profitTrades;
    private int _lossTrades;
    private double _maxDrawdown;
    private double _maxDrawdownPercent;
    private double _sharpeRatio;

    public string Symbol { get => _symbol; set => SetProperty(ref _symbol, value); }
    public TimeFrame TimeFrameValue { get => _timeFrame; set => SetProperty(ref _timeFrame, value); }
    public DateTime DateFrom { get => _dateFrom; set => SetProperty(ref _dateFrom, value); }
    public DateTime DateTo { get => _dateTo; set => SetProperty(ref _dateTo, value); }
    public double InitialDeposit { get => _initialDeposit; set => SetProperty(ref _initialDeposit, value); }
    public int Leverage { get => _leverage; set => SetProperty(ref _leverage, value); }
    public int Spread { get => _spread; set => SetProperty(ref _spread, value); }
    public double Progress { get => _progress; set => SetProperty(ref _progress, value); }
    public string ProgressText { get => _progressText; set => SetProperty(ref _progressText, value); }
    public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }

    public double NetProfit { get => _netProfit; set => SetProperty(ref _netProfit, value); }
    public double GrossProfit { get => _grossProfit; set => SetProperty(ref _grossProfit, value); }
    public double GrossLoss { get => _grossLoss; set => SetProperty(ref _grossLoss, value); }
    public double ProfitFactor { get => _profitFactor; set => SetProperty(ref _profitFactor, value); }
    public int TotalTrades { get => _totalTrades; set => SetProperty(ref _totalTrades, value); }
    public int ProfitTrades { get => _profitTrades; set => SetProperty(ref _profitTrades, value); }
    public int LossTrades { get => _lossTrades; set => SetProperty(ref _lossTrades, value); }
    public double MaxDrawdown { get => _maxDrawdown; set => SetProperty(ref _maxDrawdown, value); }
    public double MaxDrawdownPercent { get => _maxDrawdownPercent; set => SetProperty(ref _maxDrawdownPercent, value); }
    public double SharpeRatio { get => _sharpeRatio; set => SetProperty(ref _sharpeRatio, value); }

    public ObservableCollection<string> AvailableStrategies { get; } = new()
    {
        "MA Crossover",
        "RSI Strategy",
        "MACD Strategy",
        "Bollinger Bands Strategy"
    };

    public ICommand StartTestCommand { get; }
    public ICommand StopTestCommand { get; }

    public StrategyTesterViewModel()
    {
        _backtestEngine = new BacktestEngine();
        DateFrom = DateTime.UtcNow.AddMonths(-6);
        DateTo = DateTime.UtcNow;

        _backtestEngine.ProgressChanged += (s, e) =>
        {
            Progress = e.ProgressPercent;
            ProgressText = $"Testing: {e.ProgressPercent:F1}% - {e.CurrentTime:yyyy.MM.dd}";
        };

        _backtestEngine.Completed += (s, e) =>
        {
            var result = e.Result;
            NetProfit = result.NetProfit;
            GrossProfit = result.GrossProfit;
            GrossLoss = result.GrossLoss;
            ProfitFactor = result.ProfitFactor;
            TotalTrades = result.TotalTrades;
            ProfitTrades = result.ProfitTrades;
            LossTrades = result.LossTrades;
            MaxDrawdown = result.MaximalDrawdown;
            MaxDrawdownPercent = result.MaximalDrawdownPercent;
            SharpeRatio = result.SharpeRatio;
            IsRunning = false;
            ProgressText = $"Completed in {result.Duration.TotalSeconds:F1}s";
        };

        StartTestCommand = new RelayCommand(StartTest, () => !IsRunning);
        StopTestCommand = new RelayCommand(StopTest, () => IsRunning);
    }

    private async void StartTest()
    {
        IsRunning = true;
        ProgressText = "Starting backtest...";

        var settings = new BacktestSettings
        {
            Symbol = Symbol,
            TimeFrame = TimeFrameValue,
            DateFrom = DateFrom,
            DateTo = DateTo,
            InitialDeposit = InitialDeposit,
            Leverage = Leverage,
            Spread = Spread
        };

        try
        {
            await _backtestEngine.RunAsync(settings);
        }
        catch (OperationCanceledException)
        {
            ProgressText = "Test cancelled";
            IsRunning = false;
        }
        catch (Exception ex)
        {
            ProgressText = $"Error: {ex.Message}";
            IsRunning = false;
        }
    }

    private void StopTest()
    {
        _backtestEngine.Cancel();
    }
}
