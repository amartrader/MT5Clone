using MT5Clone.Core.Enums;

namespace MT5Clone.Core.Models;

public class Account
{
    public long Login { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public int CurrencyDigits { get; set; } = 2;
    public AccountType AccountType { get; set; }
    public AccountTradeMode TradeMode { get; set; }
    public AccountMarginMode MarginMode { get; set; } = AccountMarginMode.RetailHedging;
    public AccountStopOutMode StopOutMode { get; set; } = AccountStopOutMode.Percent;
    public int Leverage { get; set; } = 100;

    public double Balance { get; set; }
    public double Credit { get; set; }
    public double Equity { get; set; }
    public double Margin { get; set; }
    public double FreeMargin { get; set; }
    public double MarginLevel { get; set; }

    public double Profit { get; set; }
    public double Commission { get; set; }
    public double Swap { get; set; }

    public double MarginCallLevel { get; set; } = 100.0;
    public double StopOutLevel { get; set; } = 50.0;

    public int MaxOrders { get; set; } = 200;
    public bool TradeAllowed { get; set; } = true;
    public bool ExpertAllowed { get; set; } = true;

    public void UpdateEquity(double unrealizedPnL)
    {
        Profit = unrealizedPnL;
        Equity = Balance + Credit + unrealizedPnL;
        FreeMargin = Equity - Margin;
        MarginLevel = Margin > 0 ? (Equity / Margin) * 100.0 : 0;
    }
}
