using MT5Clone.Core.Enums;
using MT5Clone.Core.Models;
using MT5Clone.Trading.Services;
using Xunit;

namespace MT5Clone.Tests.Trading;

public class TradingEngineTests
{
    private static Symbol CreateForexSymbol(string name = "EURUSD")
    {
        return new Symbol
        {
            Name = name,
            Digits = 5,
            Point = 0.00001,
            TickSize = 0.00001,
            ContractSize = 100000,
            MinLot = 0.01,
            MaxLot = 100.0,
            LotStep = 0.01,
            Bid = 1.08500,
            Ask = 1.08510,
            SymbolType = SymbolType.Forex,
            QuoteCurrency = "USD"
        };
    }

    private static TradingEngine CreateEngine(double balance = 10000.0)
    {
        var account = new Account
        {
            Balance = balance,
            Equity = balance,
            FreeMargin = balance,
            Currency = "USD",
            Leverage = 100,
            TradeAllowed = true
        };
        var engine = new TradingEngine(account);
        engine.RegisterSymbol(CreateForexSymbol());
        return engine;
    }

    [Fact]
    public void SendOrder_BuyMarket_CreatesPosition()
    {
        var engine = CreateEngine();
        var request = new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1
        };

        var result = engine.SendOrder(request);

        Assert.True(result.Success);
        Assert.Equal(1, engine.GetOpenPositions().Count);
        Assert.Equal("EURUSD", engine.GetOpenPositions()[0].Symbol);
        Assert.Equal(PositionType.Buy, engine.GetOpenPositions()[0].Type);
        Assert.Equal(0.1, engine.GetOpenPositions()[0].Volume);
    }

    [Fact]
    public void SendOrder_SellMarket_CreatesPosition()
    {
        var engine = CreateEngine();
        var request = new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Sell,
            Volume = 0.1
        };

        var result = engine.SendOrder(request);

        Assert.True(result.Success);
        Assert.Equal(1, engine.GetOpenPositions().Count);
        Assert.Equal(PositionType.Sell, engine.GetOpenPositions()[0].Type);
    }

    [Fact]
    public void SendOrder_InvalidSymbol_Fails()
    {
        var engine = CreateEngine();
        var request = new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "INVALID",
            OrderType = OrderType.Buy,
            Volume = 0.1
        };

        var result = engine.SendOrder(request);

        Assert.False(result.Success);
        Assert.Contains("not found", result.Comment);
    }

    [Fact]
    public void SendOrder_VolumeTooSmall_Fails()
    {
        var engine = CreateEngine();
        var request = new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.001 // Below MinLot of 0.01
        };

        var result = engine.SendOrder(request);

        Assert.False(result.Success);
        Assert.Contains("Invalid volume", result.Comment);
    }

    [Fact]
    public void SendOrder_VolumeTooLarge_Fails()
    {
        var engine = CreateEngine();
        var request = new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 200.0 // Above MaxLot of 100.0
        };

        var result = engine.SendOrder(request);

        Assert.False(result.Success);
        Assert.Contains("Invalid volume", result.Comment);
    }

    [Fact]
    public void SendOrder_InsufficientMargin_Fails()
    {
        var engine = CreateEngine(balance: 10.0);
        var request = new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 10.0
        };

        var result = engine.SendOrder(request);

        Assert.False(result.Success);
        Assert.Contains("Not enough money", result.Comment);
    }

    [Fact]
    public void SendOrder_TradingDisabled_Fails()
    {
        var account = new Account
        {
            Balance = 10000,
            Equity = 10000,
            FreeMargin = 10000,
            Leverage = 100,
            TradeAllowed = false
        };
        var engine = new TradingEngine(account);
        engine.RegisterSymbol(CreateForexSymbol());

        var result = engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1
        });

        Assert.False(result.Success);
        Assert.Contains("not allowed", result.Comment);
    }

    [Fact]
    public void SendOrder_PendingBuyLimit_PlacesOrder()
    {
        var engine = CreateEngine();
        var request = new TradeRequest
        {
            Action = TradeAction.Pending,
            Symbol = "EURUSD",
            OrderType = OrderType.BuyLimit,
            Volume = 0.1,
            Price = 1.08000
        };

        var result = engine.SendOrder(request);

        Assert.True(result.Success);
        Assert.Equal(1, engine.GetPendingOrders().Count);
        Assert.Equal(OrderType.BuyLimit, engine.GetPendingOrders()[0].Type);
        Assert.Equal(0, engine.GetOpenPositions().Count);
    }

    [Fact]
    public void SendOrder_PendingWithZeroPrice_Fails()
    {
        var engine = CreateEngine();
        var request = new TradeRequest
        {
            Action = TradeAction.Pending,
            Symbol = "EURUSD",
            OrderType = OrderType.BuyLimit,
            Volume = 0.1,
            Price = 0
        };

        var result = engine.SendOrder(request);

        Assert.False(result.Success);
        Assert.Contains("Invalid price", result.Comment);
    }

    [Fact]
    public void ClosePosition_ClosesAndRecordsProfit()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1
        });

        var position = engine.GetOpenPositions()[0];
        var result = engine.ClosePosition(position.Ticket);

        Assert.True(result.Success);
        Assert.Equal(0, engine.GetOpenPositions().Count);
    }

    [Fact]
    public void ClosePosition_InvalidTicket_Fails()
    {
        var engine = CreateEngine();
        var result = engine.ClosePosition(99999);

        Assert.False(result.Success);
        Assert.Contains("not found", result.Comment);
    }

    [Fact]
    public void ClosePosition_PartialClose_ReducesVolume()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 1.0
        });

        var position = engine.GetOpenPositions()[0];
        engine.ClosePosition(position.Ticket, 0.3);

        Assert.Equal(1, engine.GetOpenPositions().Count);
        Assert.Equal(0.7, engine.GetOpenPositions()[0].Volume, 2);
    }

    [Fact]
    public void ModifyPosition_UpdatesSLTP()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1
        });

        var position = engine.GetOpenPositions()[0];
        var result = engine.ModifyPosition(position.Ticket, 1.08000, 1.09000);

        Assert.True(result.Success);
        Assert.Equal(1.08000, engine.GetOpenPositions()[0].StopLoss);
        Assert.Equal(1.09000, engine.GetOpenPositions()[0].TakeProfit);
    }

    [Fact]
    public void CancelOrder_RemovesPendingOrder()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Pending,
            Symbol = "EURUSD",
            OrderType = OrderType.BuyLimit,
            Volume = 0.1,
            Price = 1.08000
        });

        var order = engine.GetPendingOrders()[0];
        var result = engine.CancelOrder(order.Ticket);

        Assert.True(result.Success);
        Assert.Equal(0, engine.GetPendingOrders().Count);
    }

    [Fact]
    public void CloseAllPositions_ClosesEverything()
    {
        var engine = CreateEngine();

        for (int i = 0; i < 3; i++)
        {
            engine.SendOrder(new TradeRequest
            {
                Action = TradeAction.Deal,
                Symbol = "EURUSD",
                OrderType = OrderType.Buy,
                Volume = 0.1
            });
        }

        Assert.Equal(3, engine.GetOpenPositions().Count);

        engine.CloseAllPositions();

        Assert.Equal(0, engine.GetOpenPositions().Count);
    }

    [Fact]
    public void ProcessTick_UpdatesPositionPrice()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1
        });

        var tick = new Tick
        {
            Symbol = "EURUSD",
            Bid = 1.09000,
            Ask = 1.09010,
            Time = DateTime.UtcNow
        };

        engine.ProcessTick(tick);

        var position = engine.GetOpenPositions()[0];
        Assert.Equal(1.09000, position.PriceCurrent);
    }

    [Fact]
    public void ProcessTick_TriggersStopLoss()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1,
            StopLoss = 1.08000
        });

        Assert.Equal(1, engine.GetOpenPositions().Count);

        var tick = new Tick
        {
            Symbol = "EURUSD",
            Bid = 1.07900,
            Ask = 1.07910,
            Time = DateTime.UtcNow
        };

        engine.ProcessTick(tick);

        Assert.Equal(0, engine.GetOpenPositions().Count);
    }

    [Fact]
    public void ProcessTick_TriggersTakeProfit()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1,
            TakeProfit = 1.09000
        });

        Assert.Equal(1, engine.GetOpenPositions().Count);

        var tick = new Tick
        {
            Symbol = "EURUSD",
            Bid = 1.09100,
            Ask = 1.09110,
            Time = DateTime.UtcNow
        };

        engine.ProcessTick(tick);

        Assert.Equal(0, engine.GetOpenPositions().Count);
    }

    [Fact]
    public void ProcessTick_FillsPendingBuyLimit()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Pending,
            Symbol = "EURUSD",
            OrderType = OrderType.BuyLimit,
            Volume = 0.1,
            Price = 1.08000
        });

        Assert.Equal(1, engine.GetPendingOrders().Count);
        Assert.Equal(0, engine.GetOpenPositions().Count);

        var tick = new Tick
        {
            Symbol = "EURUSD",
            Bid = 1.07990,
            Ask = 1.07999,
            Time = DateTime.UtcNow
        };

        engine.ProcessTick(tick);

        Assert.Equal(0, engine.GetPendingOrders().Count);
        Assert.Equal(1, engine.GetOpenPositions().Count);
    }

    [Fact]
    public void GetAccount_ReturnsAccountInfo()
    {
        var engine = CreateEngine(balance: 5000.0);
        var account = engine.GetAccount();

        Assert.Equal(5000.0, account.Balance);
    }

    [Fact]
    public void GetDeals_ReturnsDealsInDateRange()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1
        });

        var deals = engine.GetDeals(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
        Assert.True(deals.Count > 0);
    }

    [Fact]
    public void GetOrderHistory_ReturnsFilledOrders()
    {
        var engine = CreateEngine();
        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1
        });

        var history = engine.GetOrderHistory(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1));
        Assert.True(history.Count > 0);
    }

    [Fact]
    public void Events_FiredOnOrderExecution()
    {
        var engine = CreateEngine();
        bool positionOpened = false;
        bool orderFilled = false;
        bool dealExecuted = false;

        engine.PositionOpened += (s, e) => positionOpened = true;
        engine.OrderFilled += (s, e) => orderFilled = true;
        engine.DealExecuted += (s, e) => dealExecuted = true;

        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1
        });

        Assert.True(positionOpened);
        Assert.True(orderFilled);
        Assert.True(dealExecuted);
    }

    [Fact]
    public void Events_FiredOnPositionClose()
    {
        var engine = CreateEngine();
        bool positionClosed = false;
        engine.PositionClosed += (s, e) => positionClosed = true;

        engine.SendOrder(new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = "EURUSD",
            OrderType = OrderType.Buy,
            Volume = 0.1
        });

        var position = engine.GetOpenPositions()[0];
        engine.ClosePosition(position.Ticket);

        Assert.True(positionClosed);
    }
}
