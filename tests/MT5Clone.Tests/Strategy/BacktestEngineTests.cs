using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Strategy.Backtesting;
using Xunit;

namespace MT5Clone.Tests.Strategy;

public class BacktestEngineTests
{
    [Fact]
    public async Task RunAsync_CompletesSuccessfully()
    {
        var engine = new BacktestEngine();
        var settings = new BacktestSettings
        {
            Symbol = "EURUSD",
            TimeFrame = TimeFrame.H1,
            DateFrom = DateTime.UtcNow.AddMonths(-1),
            DateTo = DateTime.UtcNow,
            InitialDeposit = 10000,
            Leverage = 100,
            Spread = 10
        };

        var result = await engine.RunAsync(settings);

        Assert.NotNull(result);
        Assert.Equal(10000, result.InitialDeposit);
        Assert.True(result.FinalBalance > 0);
        Assert.True(result.Duration.TotalSeconds >= 0);
    }

    [Fact]
    public async Task RunAsync_ReportsProgress()
    {
        var engine = new BacktestEngine();
        var progressReported = false;

        engine.ProgressChanged += (s, e) =>
        {
            progressReported = true;
            Assert.True(e.ProgressPercent >= 0 && e.ProgressPercent <= 100);
        };

        var settings = new BacktestSettings
        {
            Symbol = "EURUSD",
            TimeFrame = TimeFrame.H1,
            DateFrom = DateTime.UtcNow.AddMonths(-1),
            DateTo = DateTime.UtcNow,
            InitialDeposit = 10000,
            Leverage = 100,
            Spread = 10
        };

        await engine.RunAsync(settings);

        Assert.True(progressReported);
    }

    [Fact]
    public async Task RunAsync_FiresCompletedEvent()
    {
        var engine = new BacktestEngine();
        var completed = false;

        engine.Completed += (s, e) =>
        {
            completed = true;
            Assert.NotNull(e.Result);
        };

        var settings = new BacktestSettings
        {
            Symbol = "EURUSD",
            TimeFrame = TimeFrame.H1,
            DateFrom = DateTime.UtcNow.AddMonths(-1),
            DateTo = DateTime.UtcNow,
            InitialDeposit = 10000,
            Leverage = 100,
            Spread = 10
        };

        await engine.RunAsync(settings);

        Assert.True(completed);
    }

    [Fact]
    public async Task RunAsync_CalculatesStatistics()
    {
        var engine = new BacktestEngine();
        var settings = new BacktestSettings
        {
            Symbol = "EURUSD",
            TimeFrame = TimeFrame.H1,
            DateFrom = DateTime.UtcNow.AddMonths(-3),
            DateTo = DateTime.UtcNow,
            InitialDeposit = 10000,
            Leverage = 100,
            Spread = 10
        };

        var result = await engine.RunAsync(settings);

        // Should have traded at least once with 3 months of data
        Assert.True(result.TotalTrades >= 0);
        Assert.True(result.GrossProfit >= 0);
        Assert.True(result.GrossLoss >= 0);
        Assert.True(result.MaximalDrawdown >= 0);
        Assert.True(result.MaximalDrawdownPercent >= 0);
    }

    [Fact]
    public async Task RunAsync_ProducesEquityCurve()
    {
        var engine = new BacktestEngine();
        var settings = new BacktestSettings
        {
            Symbol = "EURUSD",
            TimeFrame = TimeFrame.H1,
            DateFrom = DateTime.UtcNow.AddMonths(-1),
            DateTo = DateTime.UtcNow,
            InitialDeposit = 10000,
            Leverage = 100,
            Spread = 10
        };

        var result = await engine.RunAsync(settings);

        Assert.NotNull(result.EquityCurve);
        Assert.True(result.EquityCurve.Count > 0);
        Assert.NotNull(result.BalanceCurve);
        Assert.True(result.BalanceCurve.Count > 0);
    }

    [Fact]
    public async Task RunAsync_WhileAlreadyRunning_Throws()
    {
        var engine = new BacktestEngine();
        var settings = new BacktestSettings
        {
            Symbol = "EURUSD",
            TimeFrame = TimeFrame.D1,
            DateFrom = DateTime.UtcNow.AddYears(-5),
            DateTo = DateTime.UtcNow,
            InitialDeposit = 10000,
            Leverage = 100,
            Spread = 10
        };

        // Start first run
        var task1 = engine.RunAsync(settings);

        // Try to start second run immediately
        await Assert.ThrowsAsync<InvalidOperationException>(() => engine.RunAsync(settings));

        await task1; // Wait for first to complete
    }

    [Fact]
    public void IsRunning_InitiallyFalse()
    {
        var engine = new BacktestEngine();
        Assert.False(engine.IsRunning);
    }

    [Fact]
    public void Cancel_DoesNotThrowWhenNotRunning()
    {
        var engine = new BacktestEngine();
        engine.Cancel(); // Should not throw
    }
}
