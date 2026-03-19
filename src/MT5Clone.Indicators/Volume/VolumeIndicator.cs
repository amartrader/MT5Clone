using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Volume;

public class VolumeIndicator : IndicatorBase
{
    public override string Name => "Volumes";
    public override string ShortName => "Volumes";
    public override IndicatorType Type => IndicatorType.Volumes;
    public override bool IsOverlay => false;

    public VolumeIndicator()
    {
        AddBuffer("Volume", "Volume", "#00FF00", IndicatorBufferStyle.Histogram);
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        SetBufferSize(candles.Count);
        var volume = Buffers[0].Data;

        for (int i = 0; i < candles.Count; i++)
        {
            volume[i] = candles[i].TickVolume;
        }
    }
}

public class AccumulationDistribution : IndicatorBase
{
    public override string Name => "Accumulation/Distribution";
    public override string ShortName => "A/D";
    public override IndicatorType Type => IndicatorType.AccumulationDistribution;
    public override bool IsOverlay => false;

    public AccumulationDistribution()
    {
        AddBuffer("AD", "A/D", "#00BFFF");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        SetBufferSize(candles.Count);
        var ad = Buffers[0].Data;

        if (candles.Count == 0) return;

        double cumAD = 0;
        for (int i = 0; i < candles.Count; i++)
        {
            double range = candles[i].High - candles[i].Low;
            double clv = range > 0
                ? ((candles[i].Close - candles[i].Low) - (candles[i].High - candles[i].Close)) / range
                : 0;
            cumAD += clv * candles[i].TickVolume;
            ad[i] = cumAD;
        }
    }
}
