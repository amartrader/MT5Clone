using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;

namespace MT5Clone.MarketData.Services;

public class SimulatedDataProvider
{
    private readonly Random _random = new(42);
    private readonly Dictionary<string, double> _currentPrices = new();
    private readonly Dictionary<string, double> _volatilities = new();

    private static readonly Dictionary<string, (double price, double volatility, double spread)> DefaultPrices = new()
    {
        ["EURUSD"] = (1.08500, 0.00020, 0.00010),
        ["GBPUSD"] = (1.26500, 0.00025, 0.00012),
        ["USDJPY"] = (149.500, 0.020, 0.010),
        ["USDCHF"] = (0.87500, 0.00020, 0.00012),
        ["AUDUSD"] = (0.65500, 0.00015, 0.00010),
        ["USDCAD"] = (1.35500, 0.00020, 0.00012),
        ["NZDUSD"] = (0.61000, 0.00015, 0.00010),
        ["EURGBP"] = (0.85800, 0.00015, 0.00012),
        ["EURJPY"] = (162.200, 0.025, 0.015),
        ["GBPJPY"] = (189.100, 0.030, 0.020),
        ["XAUUSD"] = (1950.00, 0.50, 0.30),
        ["XAGUSD"] = (23.50, 0.020, 0.015),
        ["US30"] = (34500.0, 10.0, 2.0),
        ["US500"] = (4450.0, 2.0, 0.5),
        ["USTEC"] = (15200.0, 5.0, 1.0),
        ["DE40"] = (15800.0, 5.0, 1.5),
        ["UK100"] = (7550.0, 3.0, 1.0),
        ["JP225"] = (33200.0, 20.0, 5.0),
        ["BTCUSD"] = (42000.00, 50.0, 20.0),
        ["ETHUSD"] = (2250.00, 5.0, 2.0)
    };

    public void InitializePrice(Symbol symbol)
    {
        if (DefaultPrices.TryGetValue(symbol.Name, out var defaults))
        {
            _currentPrices[symbol.Name] = defaults.price;
            _volatilities[symbol.Name] = defaults.volatility;
            symbol.Bid = defaults.price;
            symbol.Ask = defaults.price + defaults.spread;
            symbol.DayOpen = defaults.price;
            symbol.DayHigh = defaults.price + defaults.volatility * 5;
            symbol.DayLow = defaults.price - defaults.volatility * 5;
            symbol.PreviousClose = defaults.price - defaults.volatility * 2;
        }
    }

    public Tick GenerateTick(Symbol symbol)
    {
        if (!_currentPrices.ContainsKey(symbol.Name))
        {
            _currentPrices[symbol.Name] = symbol.Bid;
            _volatilities[symbol.Name] = symbol.Point * 10;
        }

        double currentPrice = _currentPrices[symbol.Name];
        double volatility = _volatilities[symbol.Name];

        // Random walk with mean reversion
        double change = ((_random.NextDouble() * 2 - 1) * volatility) +
                        ((_random.NextDouble() * 2 - 1) * volatility * 0.3);

        currentPrice += change;
        _currentPrices[symbol.Name] = currentPrice;

        double spreadMultiplier = DefaultPrices.TryGetValue(symbol.Name, out var def) ? def.spread : symbol.Point * 10;
        double spread = spreadMultiplier * (0.8 + _random.NextDouble() * 0.4);

        return new Tick
        {
            Symbol = symbol.Name,
            Time = DateTime.UtcNow,
            Bid = Math.Round(currentPrice, symbol.Digits),
            Ask = Math.Round(currentPrice + spread, symbol.Digits),
            Last = Math.Round(currentPrice, symbol.Digits),
            Volume = _random.Next(1, 50),
            TimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Flags = TickFlags.Bid | TickFlags.Ask
        };
    }

    public MarketDepth GenerateMarketDepth(Symbol symbol)
    {
        var depth = new MarketDepth
        {
            Symbol = symbol.Name,
            Time = DateTime.UtcNow
        };

        double mid = _currentPrices.GetValueOrDefault(symbol.Name, symbol.Bid);
        double spread = DefaultPrices.TryGetValue(symbol.Name, out var def) ? def.spread : symbol.Point * 10;

        for (int i = 0; i < 10; i++)
        {
            depth.Entries.Add(new MarketDepthEntry
            {
                Type = MarketDepthType.Buy,
                Price = Math.Round(mid - spread * (i + 1) * 0.5, symbol.Digits),
                Volume = _random.Next(1, 100)
            });

            depth.Entries.Add(new MarketDepthEntry
            {
                Type = MarketDepthType.Sell,
                Price = Math.Round(mid + spread * (i + 1) * 0.5, symbol.Digits),
                Volume = _random.Next(1, 100)
            });
        }

        return depth;
    }

    public List<Candle> GenerateHistoricalCandles(Symbol symbol, TimeFrame timeFrame, int count)
    {
        var candles = new List<Candle>();
        double price = DefaultPrices.TryGetValue(symbol.Name, out var def) ? def.price : symbol.Bid;
        double volatility = def.volatility > 0 ? def.volatility : symbol.Point * 10;

        DateTime startTime = DateTime.UtcNow;
        int minutesPerCandle = (int)timeFrame;
        startTime = startTime.AddMinutes(-minutesPerCandle * count);

        for (int i = 0; i < count; i++)
        {
            DateTime candleTime = CandleAggregator.GetCandleTime(startTime.AddMinutes(minutesPerCandle * i), timeFrame);

            double open = price;
            double change1 = (_random.NextDouble() * 2 - 1) * volatility * Math.Sqrt(minutesPerCandle);
            double change2 = (_random.NextDouble() * 2 - 1) * volatility * Math.Sqrt(minutesPerCandle);
            double close = open + change1;
            double high = Math.Max(open, close) + Math.Abs(change2) * 0.5;
            double low = Math.Min(open, close) - Math.Abs(change2) * 0.5;

            candles.Add(new Candle
            {
                Time = candleTime,
                Open = Math.Round(open, symbol.Digits),
                High = Math.Round(high, symbol.Digits),
                Low = Math.Round(low, symbol.Digits),
                Close = Math.Round(close, symbol.Digits),
                TickVolume = _random.Next(100, 5000),
                RealVolume = _random.Next(1000, 50000),
                Spread = (int)(def.spread / symbol.Point),
                TimeFrame = timeFrame
            });

            price = close;
        }

        return candles;
    }
}
