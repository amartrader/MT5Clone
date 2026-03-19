namespace MT5Clone.Core.Enums;

public enum IndicatorType
{
    // Trend
    MovingAverage,
    BollingerBands,
    Ichimoku,
    ParabolicSAR,
    ADX,
    Envelopes,
    StandardDeviation,

    // Oscillators
    RSI,
    MACD,
    Stochastic,
    CCI,
    WilliamsPercentR,
    Momentum,
    DeMarker,
    ForceIndex,
    RateOfChange,
    RelativeVigorIndex,

    // Volume
    OBV,
    MoneyFlowIndex,
    AccumulationDistribution,
    Volumes,

    // Bill Williams
    AlligatorIndicator,
    AwesomeOscillator,
    Fractals,
    GatorOscillator,
    MarketFacilitationIndex,
    AcceleratorOscillator,

    // Custom
    Custom
}

public enum MovingAverageMethod
{
    Simple,
    Exponential,
    Smoothed,
    LinearWeighted
}

public enum AppliedPrice
{
    Close,
    Open,
    High,
    Low,
    Median,
    Typical,
    Weighted
}
