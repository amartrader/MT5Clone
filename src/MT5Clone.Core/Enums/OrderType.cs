namespace MT5Clone.Core.Enums;

public enum OrderType
{
    Buy,
    Sell,
    BuyLimit,
    SellLimit,
    BuyStop,
    SellStop,
    BuyStopLimit,
    SellStopLimit,
    CloseBy
}

public enum OrderState
{
    Started,
    Placed,
    Canceled,
    Partial,
    Filled,
    Rejected,
    Expired,
    RequestAdd,
    RequestModify,
    RequestCancel
}

public enum OrderFillingType
{
    FillOrKill,
    ImmediateOrCancel,
    Return
}

public enum OrderTimeInForce
{
    GoodTillCancelled,
    Day,
    Specified,
    SpecifiedDay
}
