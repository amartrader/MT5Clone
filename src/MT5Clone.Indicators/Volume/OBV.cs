using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Indicators.Base;

namespace MT5Clone.Indicators.Volume;

public class OBV : IndicatorBase
{
    public override string Name => "On Balance Volume";
    public override string ShortName => "OBV";
    public override IndicatorType Type => IndicatorType.OBV;
    public override bool IsOverlay => false;

    public OBV()
    {
        AddBuffer("OBV", "OBV", "#00FFFF");
    }

    public override void Calculate(IReadOnlyList<Candle> candles)
    {
        SetBufferSize(candles.Count);
        var obv = Buffers[0].Data;

        if (candles.Count == 0) return;

        obv[0] = candles[0].TickVolume;

        for (int i = 1; i < candles.Count; i++)
        {
            if (candles[i].Close > candles[i - 1].Close)
                obv[i] = obv[i - 1] + candles[i].TickVolume;
            else if (candles[i].Close < candles[i - 1].Close)
                obv[i] = obv[i - 1] - candles[i].TickVolume;
            else
                obv[i] = obv[i - 1];
        }
    }
}
