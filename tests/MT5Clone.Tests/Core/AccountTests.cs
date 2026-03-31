using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using Xunit;

namespace MT5Clone.Tests.Core;

public class AccountTests
{
    private static Account CreateTestAccount()
    {
        return new Account
        {
            Login = 12345,
            Name = "Test Account",
            Balance = 10000.0,
            Equity = 10000.0,
            FreeMargin = 10000.0,
            Currency = "USD",
            Leverage = 100,
            AccountType = AccountType.Demo,
            TradeAllowed = true
        };
    }

    [Fact]
    public void UpdateEquity_WithProfit_IncreasesEquity()
    {
        var account = CreateTestAccount();
        account.UpdateEquity(500.0);

        Assert.Equal(500.0, account.Profit);
        Assert.Equal(10500.0, account.Equity);
        Assert.Equal(10500.0, account.FreeMargin);
    }

    [Fact]
    public void UpdateEquity_WithLoss_DecreasesEquity()
    {
        var account = CreateTestAccount();
        account.UpdateEquity(-300.0);

        Assert.Equal(-300.0, account.Profit);
        Assert.Equal(9700.0, account.Equity);
        Assert.Equal(9700.0, account.FreeMargin);
    }

    [Fact]
    public void UpdateEquity_WithMargin_CalculatesFreeMargin()
    {
        var account = CreateTestAccount();
        account.Margin = 1000.0;
        account.UpdateEquity(200.0);

        Assert.Equal(10200.0, account.Equity);
        Assert.Equal(9200.0, account.FreeMargin);
    }

    [Fact]
    public void UpdateEquity_CalculatesMarginLevel()
    {
        var account = CreateTestAccount();
        account.Margin = 2000.0;
        account.UpdateEquity(0.0);

        // MarginLevel = (Equity / Margin) * 100 = (10000 / 2000) * 100 = 500
        Assert.Equal(500.0, account.MarginLevel);
    }

    [Fact]
    public void UpdateEquity_WithZeroMargin_MarginLevelIsZero()
    {
        var account = CreateTestAccount();
        account.UpdateEquity(0.0);

        Assert.Equal(0.0, account.MarginLevel);
    }

    [Fact]
    public void UpdateEquity_WithCredit_IncludesCredit()
    {
        var account = CreateTestAccount();
        account.Credit = 500.0;
        account.UpdateEquity(100.0);

        // Equity = Balance + Credit + PnL = 10000 + 500 + 100 = 10600
        Assert.Equal(10600.0, account.Equity);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var account = new Account();
        Assert.Equal("USD", account.Currency);
        Assert.Equal(2, account.CurrencyDigits);
        Assert.Equal(100, account.Leverage);
        Assert.Equal(100.0, account.MarginCallLevel);
        Assert.Equal(50.0, account.StopOutLevel);
        Assert.Equal(200, account.MaxOrders);
        Assert.True(account.TradeAllowed);
        Assert.True(account.ExpertAllowed);
        Assert.Equal(AccountMarginMode.RetailHedging, account.MarginMode);
        Assert.Equal(AccountStopOutMode.Percent, account.StopOutMode);
    }
}
