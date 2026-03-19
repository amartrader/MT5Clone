using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.OpenAlgo.Models;

namespace MT5Clone.OpenAlgo.Services;

public class OpenAlgoMarketDataProvider : IMarketDataProvider, IDisposable
{
    private readonly OpenAlgoApiClient _client;
    private readonly OpenAlgoConfig _config;
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    private readonly Dictionary<string, Symbol> _symbols = new();
    private readonly Dictionary<string, Tick> _lastTicks = new();
    private readonly Dictionary<string, Dictionary<TimeFrame, List<Candle>>> _candles = new();
    private readonly Dictionary<string, MarketDepth> _depths = new();
    private readonly HashSet<string> _subscribedSymbols = new();
    private readonly HashSet<string> _subscribedDepthSymbols = new();
    private readonly object _lock = new();

    // Default Indian exchange symbols for initial population
    private static readonly (string Symbol, string Exchange, string Description)[] DefaultSymbols =
    {
        ("NIFTY 50", "NSE_INDEX", "Nifty 50 Index"),
        ("NIFTY BANK", "NSE_INDEX", "Bank Nifty Index"),
        ("RELIANCE", "NSE", "Reliance Industries"),
        ("TCS", "NSE", "Tata Consultancy Services"),
        ("HDFCBANK", "NSE", "HDFC Bank"),
        ("INFY", "NSE", "Infosys"),
        ("ICICIBANK", "NSE", "ICICI Bank"),
        ("HINDUNILVR", "NSE", "Hindustan Unilever"),
        ("SBIN", "NSE", "State Bank of India"),
        ("BHARTIARTL", "NSE", "Bharti Airtel"),
        ("ITC", "NSE", "ITC Limited"),
        ("KOTAKBANK", "NSE", "Kotak Mahindra Bank"),
        ("LT", "NSE", "Larsen & Toubro"),
        ("AXISBANK", "NSE", "Axis Bank"),
        ("WIPRO", "NSE", "Wipro"),
        ("BAJFINANCE", "NSE", "Bajaj Finance"),
        ("MARUTI", "NSE", "Maruti Suzuki"),
        ("TATAMOTORS", "NSE", "Tata Motors"),
        ("SUNPHARMA", "NSE", "Sun Pharma"),
        ("TATASTEEL", "NSE", "Tata Steel"),
        ("GOLDM", "MCX", "Gold Mini"),
        ("CRUDEOILM", "MCX", "Crude Oil Mini"),
        ("SILVERM", "MCX", "Silver Mini"),
        ("NATURALGAS", "MCX", "Natural Gas"),
        ("USDINR", "CDS", "USD/INR Currency"),
    };

    public event EventHandler<TickEventArgs>? TickReceived;
#pragma warning disable CS0067
    public event EventHandler<CandleEventArgs>? CandleUpdated;
#pragma warning restore CS0067
    public event EventHandler<MarketDepthEventArgs>? MarketDepthUpdated;

    public bool IsRunning => _isRunning;

    public OpenAlgoMarketDataProvider(OpenAlgoApiClient client, OpenAlgoConfig config)
    {
        _client = client;
        _config = config;
        InitializeDefaultSymbols();
    }

    private void InitializeDefaultSymbols()
    {
        foreach (var (name, exchange, description) in DefaultSymbols)
        {
            var key = $"{exchange}:{name}";
            var symbolType = exchange switch
            {
                "NSE" or "BSE" => SymbolType.Stock,
                "NSE_INDEX" or "BSE_INDEX" => SymbolType.Index,
                "NFO" or "BFO" => SymbolType.Futures,
                "MCX" => SymbolType.Commodity,
                "CDS" or "BCD" => SymbolType.Forex,
                _ => SymbolType.Stock
            };

            var calcMode = exchange switch
            {
                "MCX" => SymbolCalcMode.Futures,
                "CDS" or "BCD" => SymbolCalcMode.Forex,
                "NFO" or "BFO" => SymbolCalcMode.Futures,
                _ => SymbolCalcMode.Exchange
            };

            _symbols[key] = new Symbol
            {
                Name = key,
                Description = description,
                BaseCurrency = "INR",
                QuoteCurrency = "INR",
                MarginCurrency = "INR",
                Path = $"{exchange}\\{name}",
                SymbolType = symbolType,
                CalcMode = calcMode,
                Digits = symbolType == SymbolType.Forex ? 4 : 2,
                Point = symbolType == SymbolType.Forex ? 0.0025 : 0.05,
                TickSize = symbolType == SymbolType.Forex ? 0.0025 : 0.05,
                TickValue = 1.0,
                ContractSize = 1,
                MinLot = 1,
                MaxLot = 10000,
                LotStep = 1,
            };
        }
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning) return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isRunning = true;

        // Start polling for quotes in a background task
        _ = Task.Run(() => PollQuotesAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _isRunning = false;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        return Task.CompletedTask;
    }

    private async Task PollQuotesAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _isRunning)
        {
            try
            {
                string[] symbolsToUpdate;
                lock (_lock)
                {
                    symbolsToUpdate = _subscribedSymbols.ToArray();
                }

                foreach (var symbolKey in symbolsToUpdate)
                {
                    if (ct.IsCancellationRequested) break;

                    var parts = symbolKey.Split(':', 2);
                    if (parts.Length != 2) continue;
                    var exchange = parts[0];
                    var symbol = parts[1];

                    try
                    {
                        var quotes = await _client.GetQuotesAsync(symbol, exchange, ct);
                        if (quotes.IsSuccess && quotes.Data != null)
                        {
                            var tick = new Tick
                            {
                                Symbol = symbolKey,
                                Time = DateTime.UtcNow,
                                Bid = (double)quotes.Data.Bid,
                                Ask = (double)quotes.Data.Ask,
                                Last = (double)quotes.Data.Ltp,
                                Volume = quotes.Data.Volume,
                                TimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                Flags = TickFlags.Bid | TickFlags.Ask | TickFlags.Last | TickFlags.Volume
                            };

                            lock (_lock)
                            {
                                _lastTicks[symbolKey] = tick;

                                if (_symbols.TryGetValue(symbolKey, out var sym))
                                {
                                    sym.Bid = tick.Bid;
                                    sym.Ask = tick.Ask;
                                    sym.Last = tick.Last;
                                    sym.DayHigh = (double)quotes.Data.High;
                                    sym.DayLow = (double)quotes.Data.Low;
                                    sym.DayOpen = (double)quotes.Data.Open;
                                    sym.PreviousClose = (double)quotes.Data.PrevClose;
                                    sym.Volume = quotes.Data.Volume;
                                }
                            }

                            TickReceived?.Invoke(this, new TickEventArgs(tick));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        // Continue polling other symbols even if one fails
                    }
                }

                // Also poll market depth for subscribed symbols
                string[] depthSymbols;
                lock (_lock)
                {
                    depthSymbols = _subscribedDepthSymbols.ToArray();
                }

                foreach (var symbolKey in depthSymbols)
                {
                    if (ct.IsCancellationRequested) break;

                    var parts = symbolKey.Split(':', 2);
                    if (parts.Length != 2) continue;
                    var exchange = parts[0];
                    var symbol = parts[1];

                    try
                    {
                        var depth = await _client.GetDepthAsync(symbol, exchange, ct);
                        if (depth.IsSuccess && depth.Data != null)
                        {
                            var marketDepth = ConvertToMarketDepth(symbolKey, depth.Data);
                            lock (_lock)
                            {
                                _depths[symbolKey] = marketDepth;
                            }
                            MarketDepthUpdated?.Invoke(this, new MarketDepthEventArgs(marketDepth));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        // Continue polling
                    }
                }

                await Task.Delay(1000, ct); // Poll every 1 second
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                await Task.Delay(5000, ct); // Wait longer on error
            }
        }
    }

    private static MarketDepth ConvertToMarketDepth(string symbolKey, DepthData data)
    {
        var depth = new MarketDepth { Symbol = symbolKey };

        if (data.Bids != null)
        {
            foreach (var bid in data.Bids)
            {
                depth.Bids.Add(new MarketDepthEntry
                {
                    Price = (double)bid.Price,
                    Volume = bid.Quantity
                });
            }
        }

        if (data.Asks != null)
        {
            foreach (var ask in data.Asks)
            {
                depth.Asks.Add(new MarketDepthEntry
                {
                    Price = (double)ask.Price,
                    Volume = ask.Quantity
                });
            }
        }

        return depth;
    }

    public IReadOnlyList<Symbol> GetSymbols()
    {
        lock (_lock) { return _symbols.Values.ToList().AsReadOnly(); }
    }

    public Symbol? GetSymbol(string symbolName)
    {
        lock (_lock) { return _symbols.TryGetValue(symbolName, out var sym) ? sym : null; }
    }

    public Tick? GetLastTick(string symbolName)
    {
        lock (_lock) { return _lastTicks.TryGetValue(symbolName, out var tick) ? tick : null; }
    }

    public List<Candle> GetCandles(string symbolName, TimeFrame timeFrame, int count)
    {
        lock (_lock)
        {
            if (_candles.TryGetValue(symbolName, out var tfCandles) &&
                tfCandles.TryGetValue(timeFrame, out var candles))
            {
                return candles.TakeLast(count).ToList();
            }
        }
        return new List<Candle>();
    }

    public List<Candle> GetCandles(string symbolName, TimeFrame timeFrame, DateTime from, DateTime to)
    {
        lock (_lock)
        {
            if (_candles.TryGetValue(symbolName, out var tfCandles) &&
                tfCandles.TryGetValue(timeFrame, out var candles))
            {
                return candles.Where(c => c.Time >= from && c.Time <= to).ToList();
            }
        }
        return new List<Candle>();
    }

    public MarketDepth? GetMarketDepth(string symbolName)
    {
        lock (_lock) { return _depths.TryGetValue(symbolName, out var d) ? d : null; }
    }

    public void SubscribeSymbol(string symbolName)
    {
        lock (_lock) { _subscribedSymbols.Add(symbolName); }
    }

    public void UnsubscribeSymbol(string symbolName)
    {
        lock (_lock) { _subscribedSymbols.Remove(symbolName); }
    }

    public void SubscribeMarketDepth(string symbolName)
    {
        lock (_lock) { _subscribedDepthSymbols.Add(symbolName); }
    }

    public void UnsubscribeMarketDepth(string symbolName)
    {
        lock (_lock) { _subscribedDepthSymbols.Remove(symbolName); }
    }

    /// <summary>
    /// Load historical candles from OpenAlgo API for charting.
    /// </summary>
    public async Task<List<Candle>> LoadHistoryAsync(
        string symbolName, TimeFrame timeFrame,
        DateTime startDate, DateTime endDate,
        CancellationToken ct = default)
    {
        var parts = symbolName.Split(':', 2);
        if (parts.Length != 2) return new List<Candle>();
        var exchange = parts[0];
        var symbol = parts[1];

        var interval = TimeFrameToInterval(timeFrame);
        var result = await _client.GetHistoryAsync(
            symbol, exchange, interval,
            startDate.ToString("yyyy-MM-dd"),
            endDate.ToString("yyyy-MM-dd"), ct);

        if (!result.IsSuccess || result.Data == null)
            return new List<Candle>();

        var candles = result.Data.Select(h => new Candle
        {
            Time = h.DateTime,
            Open = (double)h.Open,
            High = (double)h.High,
            Low = (double)h.Low,
            Close = (double)h.Close,
            TickVolume = h.Volume,
            RealVolume = h.Volume,
            TimeFrame = timeFrame
        }).OrderBy(c => c.Time).ToList();

        lock (_lock)
        {
            if (!_candles.ContainsKey(symbolName))
                _candles[symbolName] = new Dictionary<TimeFrame, List<Candle>>();
            _candles[symbolName][timeFrame] = candles;
        }

        return candles;
    }

    /// <summary>
    /// Search for symbols via OpenAlgo API and add them to the available symbols list.
    /// </summary>
    public async Task<List<Symbol>> SearchAndAddSymbolsAsync(string query, string? exchange = null, CancellationToken ct = default)
    {
        var result = await _client.SearchSymbolsAsync(query, exchange, ct);
        if (!result.IsSuccess || result.Data == null)
            return new List<Symbol>();

        var addedSymbols = new List<Symbol>();
        lock (_lock)
        {
            foreach (var sd in result.Data)
            {
                if (string.IsNullOrEmpty(sd.Symbol) || string.IsNullOrEmpty(sd.Exchange))
                    continue;

                var key = $"{sd.Exchange}:{sd.Symbol}";
                if (!_symbols.ContainsKey(key))
                {
                    var symbolType = sd.Exchange switch
                    {
                        "NSE" or "BSE" => SymbolType.Stock,
                        "NSE_INDEX" or "BSE_INDEX" => SymbolType.Index,
                        "NFO" or "BFO" => SymbolType.Futures,
                        "MCX" => SymbolType.Commodity,
                        "CDS" or "BCD" => SymbolType.Forex,
                        _ => SymbolType.Stock
                    };

                    var sym = new Symbol
                    {
                        Name = key,
                        Description = sd.Name ?? sd.Symbol,
                        BaseCurrency = "INR",
                        QuoteCurrency = "INR",
                        MarginCurrency = "INR",
                        Path = $"{sd.Exchange}\\{sd.Symbol}",
                        SymbolType = symbolType,
                        Digits = symbolType == SymbolType.Forex ? 4 : 2,
                        Point = symbolType == SymbolType.Forex ? 0.0025 : 0.05,
                        TickSize = sd.TickSize > 0 ? (double)sd.TickSize : 0.05,
                        TickValue = 1.0,
                        ContractSize = sd.LotSize > 0 ? sd.LotSize : 1,
                        MinLot = 1,
                        MaxLot = 10000,
                        LotStep = 1,
                    };
                    _symbols[key] = sym;
                    addedSymbols.Add(sym);
                }
                else
                {
                    addedSymbols.Add(_symbols[key]);
                }
            }
        }

        return addedSymbols;
    }

    private static string TimeFrameToInterval(TimeFrame tf) => tf switch
    {
        TimeFrame.M1 => "1m",
        TimeFrame.M2 => "2m",
        TimeFrame.M3 => "3m",
        TimeFrame.M5 => "5m",
        TimeFrame.M10 => "10m",
        TimeFrame.M15 => "15m",
        TimeFrame.M20 => "20m",
        TimeFrame.M30 => "30m",
        TimeFrame.H1 => "1h",
        TimeFrame.H2 => "2h",
        TimeFrame.H3 => "3h",
        TimeFrame.H4 => "4h",
        TimeFrame.D1 => "D",
        TimeFrame.W1 => "W",
        TimeFrame.MN1 => "M",
        _ => "D"
    };

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }
}
