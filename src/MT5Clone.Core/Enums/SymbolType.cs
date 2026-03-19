namespace MT5Clone.Core.Enums;

public enum SymbolType
{
    Forex,
    CFD,
    Futures,
    Stock,
    Bond,
    Index,
    Commodity,
    Crypto,
    Option
}

public enum SymbolTradeMode
{
    Disabled,
    LongOnly,
    ShortOnly,
    CloseOnly,
    Full
}

public enum SymbolCalcMode
{
    Forex,
    ForexNoLeverage,
    Futures,
    CFD,
    CFDIndex,
    CFDLeverage,
    Exchange,
    ExchangeMargin
}
