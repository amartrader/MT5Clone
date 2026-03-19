using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;

namespace MT5Clone.Core.Interfaces;

public interface IStrategyTester
{
    event EventHandler<BacktestProgressEventArgs>? ProgressChanged;
    event EventHandler<BacktestCompletedEventArgs>? Completed;

    Task<BacktestResult> RunAsync(BacktestSettings settings, CancellationToken cancellationToken = default);
    void Cancel();
    bool IsRunning { get; }
}

public class BacktestSettings
{
    public string Symbol { get; set; } = string.Empty;
    public TimeFrame TimeFrame { get; set; } = TimeFrame.H1;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public double InitialDeposit { get; set; } = 10000;
    public string Currency { get; set; } = "USD";
    public int Leverage { get; set; } = 100;
    public BacktestModel Model { get; set; } = BacktestModel.EveryTick;
    public bool UseSpread { get; set; } = true;
    public int Spread { get; set; } = 10;
    public string StrategyName { get; set; } = string.Empty;
    public Dictionary<string, object> StrategyParameters { get; set; } = new();
}

public enum BacktestModel
{
    EveryTick,
    OneMinuteOHLC,
    OpenPriceOnly,
    MathCalculations
}

public class BacktestResult
{
    public double InitialDeposit { get; set; }
    public double FinalBalance { get; set; }
    public double NetProfit { get; set; }
    public double GrossProfit { get; set; }
    public double GrossLoss { get; set; }
    public double ProfitFactor { get; set; }
    public double ExpectedPayoff { get; set; }
    public double AbsoluteDrawdown { get; set; }
    public double MaximalDrawdown { get; set; }
    public double MaximalDrawdownPercent { get; set; }
    public double RelativeDrawdown { get; set; }
    public double RelativeDrawdownPercent { get; set; }
    public int TotalTrades { get; set; }
    public int ShortPositions { get; set; }
    public int ShortWins { get; set; }
    public int LongPositions { get; set; }
    public int LongWins { get; set; }
    public int ProfitTrades { get; set; }
    public int LossTrades { get; set; }
    public double LargestProfitTrade { get; set; }
    public double LargestLossTrade { get; set; }
    public double AverageProfitTrade { get; set; }
    public double AverageLossTrade { get; set; }
    public int MaxConsecutiveWins { get; set; }
    public int MaxConsecutiveLosses { get; set; }
    public double MaxConsecutiveProfit { get; set; }
    public double MaxConsecutiveLoss { get; set; }
    public double SharpeRatio { get; set; }
    public double RecoveryFactor { get; set; }
    public List<Deal> Deals { get; set; } = new();
    public List<EquityCurvePoint> EquityCurve { get; set; } = new();
    public List<BalanceCurvePoint> BalanceCurve { get; set; } = new();
    public TimeSpan Duration { get; set; }
}

public class EquityCurvePoint
{
    public DateTime Time { get; set; }
    public double Equity { get; set; }
}

public class BalanceCurvePoint
{
    public DateTime Time { get; set; }
    public double Balance { get; set; }
}

public class BacktestProgressEventArgs : EventArgs
{
    public double ProgressPercent { get; }
    public DateTime CurrentTime { get; }
    public BacktestProgressEventArgs(double progressPercent, DateTime currentTime)
    {
        ProgressPercent = progressPercent;
        CurrentTime = currentTime;
    }
}

public class BacktestCompletedEventArgs : EventArgs
{
    public BacktestResult Result { get; }
    public BacktestCompletedEventArgs(BacktestResult result) => Result = result;
}
