using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.Trading.Services;
using MT5Clone.MarketData.Services;

namespace MT5Clone.Strategy.Backtesting;

public class BacktestEngine : IStrategyTester
{
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public event EventHandler<BacktestProgressEventArgs>? ProgressChanged;
    public event EventHandler<BacktestCompletedEventArgs>? Completed;

    public bool IsRunning => _isRunning;

    public async Task<BacktestResult> RunAsync(BacktestSettings settings, CancellationToken cancellationToken = default)
    {
        if (_isRunning) throw new InvalidOperationException("Backtest already running");

        _isRunning = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var startTime = DateTime.UtcNow;

        // Yield to allow UI to update before starting CPU-bound work
        await Task.Yield();

        try
        {
            var account = new Account
            {
                Balance = settings.InitialDeposit,
                Equity = settings.InitialDeposit,
                FreeMargin = settings.InitialDeposit,
                Currency = settings.Currency,
                Leverage = settings.Leverage,
                AccountType = AccountType.Demo,
                TradeMode = AccountTradeMode.Demo,
                TradeAllowed = true
            };

            var engine = new TradingEngine(account);
            var dataProvider = new SimulatedDataProvider();

            // Create symbol for backtesting
            var symbol = CreateBacktestSymbol(settings.Symbol, settings.Spread);
            engine.RegisterSymbol(symbol);
            dataProvider.InitializePrice(symbol);

            // Generate historical data
            var candles = dataProvider.GenerateHistoricalCandles(symbol, settings.TimeFrame,
                (int)((settings.DateTo - settings.DateFrom).TotalMinutes / (int)settings.TimeFrame));

            var result = new BacktestResult
            {
                InitialDeposit = settings.InitialDeposit
            };

            var equityCurve = new List<EquityCurvePoint>();
            var balanceCurve = new List<BalanceCurvePoint>();
            double maxEquity = settings.InitialDeposit;
            double maxDrawdown = 0;
            double maxDrawdownPercent = 0;

            // Process each candle
            for (int i = 0; i < candles.Count; i++)
            {
                _cts.Token.ThrowIfCancellationRequested();

                var candle = candles[i];
                var tick = new Tick
                {
                    Symbol = settings.Symbol,
                    Time = candle.Time,
                    Bid = candle.Close,
                    Ask = candle.Close + settings.Spread * symbol.Point,
                    Last = candle.Close,
                    Volume = candle.TickVolume
                };

                symbol.Bid = tick.Bid;
                symbol.Ask = tick.Ask;
                engine.ProcessTick(tick);

                // Simple example strategy: MA crossover
                if (i >= 26)
                {
                    double fastMA = 0, slowMA = 0;
                    for (int j = 0; j < 12; j++) fastMA += candles[i - j].Close;
                    fastMA /= 12;
                    for (int j = 0; j < 26; j++) slowMA += candles[i - j].Close;
                    slowMA /= 26;

                    double prevFastMA = 0, prevSlowMA = 0;
                    for (int j = 1; j < 13; j++) prevFastMA += candles[i - j].Close;
                    prevFastMA /= 12;
                    for (int j = 1; j < 27; j++) prevSlowMA += candles[i - j].Close;
                    prevSlowMA /= 26;

                    var positions = engine.GetOpenPositions();
                    if (fastMA > slowMA && prevFastMA <= prevSlowMA && positions.Count == 0)
                    {
                        engine.SendOrder(new TradeRequest
                        {
                            Action = TradeAction.Deal,
                            Symbol = settings.Symbol,
                            OrderType = OrderType.Buy,
                            Volume = 0.1
                        });
                    }
                    else if (fastMA < slowMA && prevFastMA >= prevSlowMA && positions.Count > 0)
                    {
                        foreach (var pos in positions.ToList())
                            engine.ClosePosition(pos.Ticket);
                    }
                }

                // Track equity curve
                var currentAccount = engine.GetAccount();
                equityCurve.Add(new EquityCurvePoint { Time = candle.Time, Equity = currentAccount.Equity });
                balanceCurve.Add(new BalanceCurvePoint { Time = candle.Time, Balance = currentAccount.Balance });

                if (currentAccount.Equity > maxEquity) maxEquity = currentAccount.Equity;
                double drawdown = maxEquity - currentAccount.Equity;
                if (drawdown > maxDrawdown) maxDrawdown = drawdown;
                double ddPercent = maxEquity > 0 ? (drawdown / maxEquity) * 100 : 0;
                if (ddPercent > maxDrawdownPercent) maxDrawdownPercent = ddPercent;

                // Report progress
                if (i % 100 == 0)
                {
                    double progress = (double)i / candles.Count * 100;
                    ProgressChanged?.Invoke(this, new BacktestProgressEventArgs(progress, candle.Time));
                }
            }

            // Close all remaining positions
            engine.CloseAllPositions();

            // Calculate results
            var finalAccount = engine.GetAccount();
            var deals = engine.GetDeals(settings.DateFrom, settings.DateTo);

            result.FinalBalance = finalAccount.Balance;
            result.NetProfit = finalAccount.Balance - settings.InitialDeposit;
            result.MaximalDrawdown = maxDrawdown;
            result.MaximalDrawdownPercent = maxDrawdownPercent;
            result.EquityCurve = equityCurve;
            result.BalanceCurve = balanceCurve;
            result.Deals = deals.ToList();
            result.TotalTrades = deals.Count(d => d.Entry == DealEntry.In);
            result.Duration = DateTime.UtcNow - startTime;

            CalculateDetailedStats(result, deals.ToList());

            ProgressChanged?.Invoke(this, new BacktestProgressEventArgs(100, settings.DateTo));
            Completed?.Invoke(this, new BacktestCompletedEventArgs(result));

            return result;
        }
        finally
        {
            _isRunning = false;
        }
    }

    private void CalculateDetailedStats(BacktestResult result, List<Deal> deals)
    {
        var closingDeals = deals.Where(d => d.Entry == DealEntry.Out).ToList();

        result.ProfitTrades = closingDeals.Count(d => d.Profit > 0);
        result.LossTrades = closingDeals.Count(d => d.Profit < 0);
        result.GrossProfit = closingDeals.Where(d => d.Profit > 0).Sum(d => d.Profit);
        result.GrossLoss = Math.Abs(closingDeals.Where(d => d.Profit < 0).Sum(d => d.Profit));
        result.ProfitFactor = result.GrossLoss > 0 ? result.GrossProfit / result.GrossLoss : 0;
        result.ExpectedPayoff = closingDeals.Count > 0 ? result.NetProfit / closingDeals.Count : 0;

        if (closingDeals.Count > 0)
        {
            result.LargestProfitTrade = closingDeals.Max(d => d.Profit);
            result.LargestLossTrade = closingDeals.Min(d => d.Profit);
            result.AverageProfitTrade = result.ProfitTrades > 0 ? result.GrossProfit / result.ProfitTrades : 0;
            result.AverageLossTrade = result.LossTrades > 0 ? -result.GrossLoss / result.LossTrades : 0;
        }

        result.LongPositions = deals.Count(d => d.Type == DealType.Buy && d.Entry == DealEntry.In);
        result.ShortPositions = deals.Count(d => d.Type == DealType.Sell && d.Entry == DealEntry.In);

        // Consecutive wins/losses
        int consecutiveWins = 0, maxConsWins = 0;
        int consecutiveLosses = 0, maxConsLosses = 0;
        double consProfit = 0, maxConsProfit = 0;
        double consLoss = 0, maxConsLoss = 0;

        foreach (var deal in closingDeals)
        {
            if (deal.Profit > 0)
            {
                consecutiveWins++;
                consProfit += deal.Profit;
                consecutiveLosses = 0;
                if (consLoss < maxConsLoss) maxConsLoss = consLoss;
                consLoss = 0;
            }
            else
            {
                consecutiveLosses++;
                consLoss += deal.Profit;
                consecutiveWins = 0;
                if (consProfit > maxConsProfit) maxConsProfit = consProfit;
                consProfit = 0;
            }

            maxConsWins = Math.Max(maxConsWins, consecutiveWins);
            maxConsLosses = Math.Max(maxConsLosses, consecutiveLosses);
        }

        result.MaxConsecutiveWins = maxConsWins;
        result.MaxConsecutiveLosses = maxConsLosses;
        result.MaxConsecutiveProfit = Math.Max(maxConsProfit, consProfit);
        result.MaxConsecutiveLoss = Math.Min(maxConsLoss, consLoss);

        // Sharpe Ratio
        if (closingDeals.Count > 1)
        {
            double avgReturn = closingDeals.Average(d => d.Profit);
            double stdDev = Math.Sqrt(closingDeals.Sum(d => Math.Pow(d.Profit - avgReturn, 2)) / (closingDeals.Count - 1));
            result.SharpeRatio = stdDev > 0 ? avgReturn / stdDev * Math.Sqrt(252) : 0;
        }

        result.RecoveryFactor = result.MaximalDrawdown > 0 ? result.NetProfit / result.MaximalDrawdown : 0;
        result.AbsoluteDrawdown = result.InitialDeposit - result.BalanceCurve.Min(b => b.Balance);
    }

    private Symbol CreateBacktestSymbol(string name, int spread)
    {
        return new Symbol
        {
            Name = name,
            Description = name,
            Digits = 5,
            Point = 0.00001,
            TickSize = 0.00001,
            TickValue = 1.0,
            ContractSize = 100000,
            MinLot = 0.01,
            MaxLot = 100.0,
            LotStep = 0.01,
            SymbolType = SymbolType.Forex,
            TradeMode = SymbolTradeMode.Full
        };
    }

    public void Cancel()
    {
        _cts?.Cancel();
    }
}
