using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;

namespace MT5Clone.MarketData.Services;

public class MarketDataService : IMarketDataProvider
{
    private readonly Dictionary<string, Symbol> _symbols = new();
    private readonly Dictionary<string, Tick> _lastTicks = new();
    private readonly Dictionary<string, Dictionary<TimeFrame, List<Candle>>> _candles = new();
    private readonly Dictionary<string, MarketDepth> _marketDepths = new();
    private readonly HashSet<string> _subscribedSymbols = new();
    private readonly HashSet<string> _subscribedDepth = new();
    private readonly CandleAggregator _candleAggregator;
    private readonly SimulatedDataProvider _dataProvider;
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public event EventHandler<TickEventArgs>? TickReceived;
    public event EventHandler<CandleEventArgs>? CandleUpdated;
    public event EventHandler<MarketDepthEventArgs>? MarketDepthUpdated;

    public bool IsRunning => _isRunning;

    public MarketDataService()
    {
        _candleAggregator = new CandleAggregator();
        _dataProvider = new SimulatedDataProvider();
        InitializeSymbols();
    }

    private void InitializeSymbols()
    {
        AddSymbol(CreateForexSymbol("EURUSD", "Euro vs US Dollar", "EUR", "USD", 5, 0.00001));
        AddSymbol(CreateForexSymbol("GBPUSD", "Great Britain Pound vs US Dollar", "GBP", "USD", 5, 0.00001));
        AddSymbol(CreateForexSymbol("USDJPY", "US Dollar vs Japanese Yen", "USD", "JPY", 3, 0.001));
        AddSymbol(CreateForexSymbol("USDCHF", "US Dollar vs Swiss Franc", "USD", "CHF", 5, 0.00001));
        AddSymbol(CreateForexSymbol("AUDUSD", "Australian Dollar vs US Dollar", "AUD", "USD", 5, 0.00001));
        AddSymbol(CreateForexSymbol("USDCAD", "US Dollar vs Canadian Dollar", "USD", "CAD", 5, 0.00001));
        AddSymbol(CreateForexSymbol("NZDUSD", "New Zealand Dollar vs US Dollar", "NZD", "USD", 5, 0.00001));
        AddSymbol(CreateForexSymbol("EURGBP", "Euro vs Great Britain Pound", "EUR", "GBP", 5, 0.00001));
        AddSymbol(CreateForexSymbol("EURJPY", "Euro vs Japanese Yen", "EUR", "JPY", 3, 0.001));
        AddSymbol(CreateForexSymbol("GBPJPY", "Great Britain Pound vs Japanese Yen", "GBP", "JPY", 3, 0.001));

        AddSymbol(CreateCFDSymbol("XAUUSD", "Gold vs US Dollar", 2, 0.01, 100, 1900.00));
        AddSymbol(CreateCFDSymbol("XAGUSD", "Silver vs US Dollar", 3, 0.001, 5000, 23.00));
        AddSymbol(CreateCFDSymbol("US30", "US Wall Street 30", 1, 0.1, 1, 34000.00));
        AddSymbol(CreateCFDSymbol("US500", "US 500 Index", 1, 0.1, 1, 4400.00));
        AddSymbol(CreateCFDSymbol("USTEC", "US Tech 100", 1, 0.1, 1, 15000.00));
        AddSymbol(CreateCFDSymbol("DE40", "Germany 40 Index", 1, 0.1, 1, 15800.00));
        AddSymbol(CreateCFDSymbol("UK100", "UK 100 Index", 1, 0.1, 1, 7500.00));
        AddSymbol(CreateCFDSymbol("JP225", "Japan 225 Index", 0, 1, 1, 33000.00));

        AddSymbol(CreateCryptoSymbol("BTCUSD", "Bitcoin vs US Dollar", 2, 0.01, 42000.00));
        AddSymbol(CreateCryptoSymbol("ETHUSD", "Ethereum vs US Dollar", 2, 0.01, 2200.00));

        foreach (var symbol in _symbols.Values)
        {
            _dataProvider.InitializePrice(symbol);
            _candles[symbol.Name] = new Dictionary<TimeFrame, List<Candle>>();
            foreach (TimeFrame tf in Enum.GetValues<TimeFrame>())
            {
                _candles[symbol.Name][tf] = _dataProvider.GenerateHistoricalCandles(symbol, tf, 1000);
            }
        }
    }

    private Symbol CreateForexSymbol(string name, string desc, string baseCurr, string quoteCurr, int digits, double point)
    {
        return new Symbol
        {
            Name = name,
            Description = desc,
            BaseCurrency = baseCurr,
            QuoteCurrency = quoteCurr,
            MarginCurrency = baseCurr,
            Path = $"Forex\\{name}",
            SymbolType = SymbolType.Forex,
            Digits = digits,
            Point = point,
            TickSize = point,
            TickValue = point == 0.001 ? 100.0 / point : 1.0,
            ContractSize = 100000,
            MinLot = 0.01,
            MaxLot = 100.0,
            LotStep = 0.01
        };
    }

    private Symbol CreateCFDSymbol(string name, string desc, int digits, double point, double contractSize, double basePrice)
    {
        return new Symbol
        {
            Name = name,
            Description = desc,
            BaseCurrency = "USD",
            QuoteCurrency = "USD",
            MarginCurrency = "USD",
            Path = $"CFDs\\{name}",
            SymbolType = SymbolType.CFD,
            Digits = digits,
            Point = point,
            TickSize = point,
            TickValue = 1.0,
            ContractSize = contractSize,
            MinLot = 0.01,
            MaxLot = 100.0,
            LotStep = 0.01
        };
    }

    private Symbol CreateCryptoSymbol(string name, string desc, int digits, double point, double basePrice)
    {
        return new Symbol
        {
            Name = name,
            Description = desc,
            BaseCurrency = name.Replace("USD", ""),
            QuoteCurrency = "USD",
            MarginCurrency = "USD",
            Path = $"Crypto\\{name}",
            SymbolType = SymbolType.Crypto,
            Digits = digits,
            Point = point,
            TickSize = point,
            TickValue = 1.0,
            ContractSize = 1,
            MinLot = 0.01,
            MaxLot = 100.0,
            LotStep = 0.01
        };
    }

    private void AddSymbol(Symbol symbol)
    {
        _symbols[symbol.Name] = symbol;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning) return;
        _isRunning = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        foreach (var symbol in _symbols.Values)
        {
            SubscribeSymbol(symbol.Name);
        }

        await Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                foreach (var symbolName in _subscribedSymbols.ToArray())
                {
                    if (_symbols.TryGetValue(symbolName, out var symbol))
                    {
                        var tick = _dataProvider.GenerateTick(symbol);
                        ProcessTick(tick, symbol);
                    }
                }

                foreach (var symbolName in _subscribedDepth.ToArray())
                {
                    if (_symbols.TryGetValue(symbolName, out var symbol))
                    {
                        var depth = _dataProvider.GenerateMarketDepth(symbol);
                        _marketDepths[symbolName] = depth;
                        MarketDepthUpdated?.Invoke(this, new MarketDepthEventArgs(depth));
                    }
                }

                await Task.Delay(100, _cts.Token);
            }
        }, _cts.Token);
    }

    private void ProcessTick(Tick tick, Symbol symbol)
    {
        _lastTicks[tick.Symbol] = tick;
        symbol.Bid = tick.Bid;
        symbol.Ask = tick.Ask;
        symbol.Last = tick.Last;
        symbol.Volume++;

        if (symbol.DayHigh < tick.Ask) symbol.DayHigh = tick.Ask;
        if (symbol.DayLow == 0 || symbol.DayLow > tick.Bid) symbol.DayLow = tick.Bid;
        if (symbol.DayOpen == 0) symbol.DayOpen = tick.Bid;

        TickReceived?.Invoke(this, new TickEventArgs(tick));

        foreach (TimeFrame tf in Enum.GetValues<TimeFrame>())
        {
            if (_candles.TryGetValue(tick.Symbol, out var tfCandles) && tfCandles.TryGetValue(tf, out var candles))
            {
                bool isNew = _candleAggregator.UpdateCandle(candles, tick, tf);
                CandleUpdated?.Invoke(this, new CandleEventArgs(tick.Symbol, tf, candles.Last(), isNew));
            }
        }
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        _isRunning = false;
        await Task.CompletedTask;
    }

    public IReadOnlyList<Symbol> GetSymbols() => _symbols.Values.ToList();
    public Symbol? GetSymbol(string symbolName) => _symbols.GetValueOrDefault(symbolName);
    public Tick? GetLastTick(string symbolName) => _lastTicks.GetValueOrDefault(symbolName);

    public List<Candle> GetCandles(string symbolName, TimeFrame timeFrame, int count)
    {
        if (_candles.TryGetValue(symbolName, out var tfCandles) && tfCandles.TryGetValue(timeFrame, out var candles))
        {
            return candles.TakeLast(count).ToList();
        }
        return new List<Candle>();
    }

    public List<Candle> GetCandles(string symbolName, TimeFrame timeFrame, DateTime from, DateTime to)
    {
        if (_candles.TryGetValue(symbolName, out var tfCandles) && tfCandles.TryGetValue(timeFrame, out var candles))
        {
            return candles.Where(c => c.Time >= from && c.Time <= to).ToList();
        }
        return new List<Candle>();
    }

    public MarketDepth? GetMarketDepth(string symbolName) => _marketDepths.GetValueOrDefault(symbolName);

    public void SubscribeSymbol(string symbolName) => _subscribedSymbols.Add(symbolName);
    public void UnsubscribeSymbol(string symbolName) => _subscribedSymbols.Remove(symbolName);
    public void SubscribeMarketDepth(string symbolName) => _subscribedDepth.Add(symbolName);
    public void UnsubscribeMarketDepth(string symbolName) => _subscribedDepth.Remove(symbolName);
}
