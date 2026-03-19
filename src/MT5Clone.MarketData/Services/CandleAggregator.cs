using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;

namespace MT5Clone.MarketData.Services;

public class CandleAggregator
{
    public bool UpdateCandle(List<Candle> candles, Tick tick, TimeFrame timeFrame)
    {
        DateTime candleTime = GetCandleTime(tick.Time, timeFrame);
        bool isNewCandle = false;

        if (candles.Count == 0 || candles.Last().Time != candleTime)
        {
            var newCandle = new Candle
            {
                Time = candleTime,
                Open = tick.Bid,
                High = tick.Bid,
                Low = tick.Bid,
                Close = tick.Bid,
                TickVolume = 1,
                RealVolume = (long)tick.Volume,
                Spread = (int)((tick.Ask - tick.Bid) / 0.00001),
                TimeFrame = timeFrame
            };
            candles.Add(newCandle);
            isNewCandle = true;

            // Keep max 10000 candles per timeframe
            if (candles.Count > 10000)
            {
                candles.RemoveRange(0, candles.Count - 10000);
            }
        }
        else
        {
            var current = candles.Last();
            current.High = Math.Max(current.High, tick.Bid);
            current.Low = Math.Min(current.Low, tick.Bid);
            current.Close = tick.Bid;
            current.TickVolume++;
            current.RealVolume += (long)tick.Volume;
        }

        return isNewCandle;
    }

    public static DateTime GetCandleTime(DateTime time, TimeFrame timeFrame)
    {
        return timeFrame switch
        {
            TimeFrame.M1 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, time.Kind),
            TimeFrame.M2 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 2 * 2, 0, time.Kind),
            TimeFrame.M3 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 3 * 3, 0, time.Kind),
            TimeFrame.M4 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 4 * 4, 0, time.Kind),
            TimeFrame.M5 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 5 * 5, 0, time.Kind),
            TimeFrame.M6 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 6 * 6, 0, time.Kind),
            TimeFrame.M10 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 10 * 10, 0, time.Kind),
            TimeFrame.M12 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 12 * 12, 0, time.Kind),
            TimeFrame.M15 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 15 * 15, 0, time.Kind),
            TimeFrame.M20 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 20 * 20, 0, time.Kind),
            TimeFrame.M30 => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute / 30 * 30, 0, time.Kind),
            TimeFrame.H1 => new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, time.Kind),
            TimeFrame.H2 => new DateTime(time.Year, time.Month, time.Day, time.Hour / 2 * 2, 0, 0, time.Kind),
            TimeFrame.H3 => new DateTime(time.Year, time.Month, time.Day, time.Hour / 3 * 3, 0, 0, time.Kind),
            TimeFrame.H4 => new DateTime(time.Year, time.Month, time.Day, time.Hour / 4 * 4, 0, 0, time.Kind),
            TimeFrame.H6 => new DateTime(time.Year, time.Month, time.Day, time.Hour / 6 * 6, 0, 0, time.Kind),
            TimeFrame.H8 => new DateTime(time.Year, time.Month, time.Day, time.Hour / 8 * 8, 0, 0, time.Kind),
            TimeFrame.H12 => new DateTime(time.Year, time.Month, time.Day, time.Hour / 12 * 12, 0, 0, time.Kind),
            TimeFrame.D1 => new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, time.Kind),
            TimeFrame.W1 => GetWeekStart(time),
            TimeFrame.MN1 => new DateTime(time.Year, time.Month, 1, 0, 0, 0, time.Kind),
            _ => time
        };
    }

    private static DateTime GetWeekStart(DateTime time)
    {
        int diff = (7 + (time.DayOfWeek - DayOfWeek.Monday)) % 7;
        return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, time.Kind).AddDays(-diff);
    }
}
