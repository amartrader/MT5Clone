using MT5Clone.Core.Enums;

namespace MT5Clone.Core.Models;

public class Candle
{
    public DateTime Time { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public long TickVolume { get; set; }
    public long RealVolume { get; set; }
    public int Spread { get; set; }
    public TimeFrame TimeFrame { get; set; }

    public bool IsBullish => Close >= Open;
    public bool IsBearish => Close < Open;
    public double Body => Math.Abs(Close - Open);
    public double UpperShadow => High - Math.Max(Open, Close);
    public double LowerShadow => Math.Min(Open, Close) - Low;
    public double Range => High - Low;
    public double MedianPrice => (High + Low) / 2.0;
    public double TypicalPrice => (High + Low + Close) / 3.0;
    public double WeightedClose => (High + Low + Close + Close) / 4.0;

    public Candle Clone()
    {
        return new Candle
        {
            Time = Time,
            Open = Open,
            High = High,
            Low = Low,
            Close = Close,
            TickVolume = TickVolume,
            RealVolume = RealVolume,
            Spread = Spread,
            TimeFrame = TimeFrame
        };
    }
}
