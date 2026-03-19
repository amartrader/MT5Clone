using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;

namespace MT5Clone.Core.Interfaces;

public interface IMarketDataProvider
{
    event EventHandler<TickEventArgs>? TickReceived;
    event EventHandler<CandleEventArgs>? CandleUpdated;
    event EventHandler<MarketDepthEventArgs>? MarketDepthUpdated;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
    bool IsRunning { get; }

    IReadOnlyList<Symbol> GetSymbols();
    Symbol? GetSymbol(string symbolName);
    Tick? GetLastTick(string symbolName);
    List<Candle> GetCandles(string symbolName, TimeFrame timeFrame, int count);
    List<Candle> GetCandles(string symbolName, TimeFrame timeFrame, DateTime from, DateTime to);
    MarketDepth? GetMarketDepth(string symbolName);

    void SubscribeSymbol(string symbolName);
    void UnsubscribeSymbol(string symbolName);
    void SubscribeMarketDepth(string symbolName);
    void UnsubscribeMarketDepth(string symbolName);
}

public class TickEventArgs : EventArgs
{
    public Tick Tick { get; }
    public TickEventArgs(Tick tick) => Tick = tick;
}

public class CandleEventArgs : EventArgs
{
    public string Symbol { get; }
    public TimeFrame TimeFrame { get; }
    public Candle Candle { get; }
    public bool IsNewCandle { get; }

    public CandleEventArgs(string symbol, TimeFrame timeFrame, Candle candle, bool isNewCandle)
    {
        Symbol = symbol;
        TimeFrame = timeFrame;
        Candle = candle;
        IsNewCandle = isNewCandle;
    }
}

public class MarketDepthEventArgs : EventArgs
{
    public MarketDepth MarketDepth { get; }
    public MarketDepthEventArgs(MarketDepth marketDepth) => MarketDepth = marketDepth;
}
