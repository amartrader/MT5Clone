namespace MT5Clone.Core.Enums;

public enum DealType
{
    Buy,
    Sell,
    Balance,
    Credit,
    Charge,
    Correction,
    Bonus,
    Commission,
    CommissionDaily,
    CommissionMonthly,
    CommissionAgentDaily,
    CommissionAgentMonthly,
    Interest,
    BuyCanceled,
    SellCanceled,
    Dividend,
    DividendFranked,
    Tax
}

public enum DealEntry
{
    In,
    Out,
    InOut,
    OutBy
}
