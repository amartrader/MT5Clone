using System.Collections.ObjectModel;
using System.Windows.Input;
using MT5Clone.App.Helpers;
using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.MarketData.Services;
using MT5Clone.Trading.Services;
using MT5Clone.OpenAlgo.Services;

namespace MT5Clone.App.ViewModels;

public class TradeLogEntry : ViewModelBase
{
    public DateTime Time { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = "Info";
    public string Source { get; set; } = "System";
}

public class JournalEntry : ViewModelBase
{
    public DateTime Time { get; set; }
    public string Source { get; set; } = "System";
    public string Message { get; set; } = string.Empty;
}

public class AlertEntry : ViewModelBase
{
    public DateTime Time { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

public class ExposureEntry : ViewModelBase
{
    public string Asset { get; set; } = string.Empty;
    public double Volume { get; set; }
    public double Rate { get; set; }
    public double ValueUSD { get; set; }
    public double Percentage { get; set; }
}

public class HistoryEntry : ViewModelBase
{
    public DateTime Time { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Volume { get; set; }
    public double Price { get; set; }
    public double Profit { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class OpenPositionEntry : ViewModelBase
{
    private double _currentPrice;
    private double _profit;

    public string Symbol { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Volume { get; set; }
    public double OpenPrice { get; set; }
    public double CurrentPrice { get => _currentPrice; set => SetProperty(ref _currentPrice, value); }
    public double StopLoss { get; set; }
    public double TakeProfit { get; set; }
    public double Profit { get => _profit; set => SetProperty(ref _profit, value); }
    public double Swap { get; set; }
    public long Ticket { get; set; }
}

public class TerminalViewModel : ViewModelBase
{
    private readonly TradingEngine _tradingEngine;
    private readonly MarketDataService _marketDataService;
    private readonly OpenAlgoService _openAlgoService;
    private bool _isOpenAlgoMode;
    private bool _isVisible = true;
    private int _selectedTab;
    private bool _isOrderDialogOpen;
    private string _orderSymbol = "EURUSD";
    private OrderType _orderType = OrderType.Buy;
    private double _orderVolume = 0.1;
    private double _orderPrice;
    private double _orderStopLoss;
    private double _orderTakeProfit;
    private string _orderComment = string.Empty;

    public ObservableCollection<OpenPositionEntry> OpenPositions { get; } = new();
    public ObservableCollection<Order> PendingOrders { get; } = new();
    public ObservableCollection<HistoryEntry> TradeHistory { get; } = new();
    public ObservableCollection<Order> OrderHistory { get; } = new();
    public ObservableCollection<TradeLogEntry> TradeLog { get; } = new();
    public ObservableCollection<JournalEntry> JournalEntries { get; } = new();
    public ObservableCollection<TradeLogEntry> ExpertLog { get; } = new();
    public ObservableCollection<AlertEntry> Alerts { get; } = new();
    public ObservableCollection<ExposureEntry> ExposureData { get; } = new();
    public ObservableCollection<Core.Events.MailMessage> Mailbox { get; } = new();

    // Account summary
    private double _balance;
    private double _equity;
    private double _margin;
    private double _freeMargin;
    private double _marginLevel;
    private double _profit;

    public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
    public int SelectedTab { get => _selectedTab; set => SetProperty(ref _selectedTab, value); }
    public bool IsOrderDialogOpen { get => _isOrderDialogOpen; set => SetProperty(ref _isOrderDialogOpen, value); }
    public string OrderSymbol { get => _orderSymbol; set => SetProperty(ref _orderSymbol, value); }
    public OrderType OrderTypeValue { get => _orderType; set => SetProperty(ref _orderType, value); }
    public double OrderVolume { get => _orderVolume; set => SetProperty(ref _orderVolume, value); }
    public double OrderPrice { get => _orderPrice; set => SetProperty(ref _orderPrice, value); }
    public double OrderStopLoss { get => _orderStopLoss; set => SetProperty(ref _orderStopLoss, value); }
    public double OrderTakeProfit { get => _orderTakeProfit; set => SetProperty(ref _orderTakeProfit, value); }
    public string OrderComment { get => _orderComment; set => SetProperty(ref _orderComment, value); }

    public double Balance { get => _balance; set => SetProperty(ref _balance, value); }
    public double Equity { get => _equity; set => SetProperty(ref _equity, value); }
    public double Margin { get => _margin; set => SetProperty(ref _margin, value); }
    public double FreeMargin { get => _freeMargin; set => SetProperty(ref _freeMargin, value); }
    public double MarginLevel { get => _marginLevel; set => SetProperty(ref _marginLevel, value); }
    public double Profit { get => _profit; set => SetProperty(ref _profit, value); }

    public ICommand PlaceBuyCommand { get; }
    public ICommand PlaceSellCommand { get; }
    public ICommand PlaceBuyLimitCommand { get; }
    public ICommand PlaceSellLimitCommand { get; }
    public ICommand PlaceBuyStopCommand { get; }
    public ICommand PlaceSellStopCommand { get; }
    public ICommand ClosePositionCommand { get; }
    public ICommand CloseAllPositionsCommand { get; }
    public ICommand ModifyPositionCommand { get; }
    public ICommand CancelOrderCommand { get; }
    public ICommand CancelAllOrdersCommand { get; }
    public ICommand OpenOrderDialogCommand { get; }
    public ICommand CloseOrderDialogCommand { get; }
    public ICommand SubmitOrderCommand { get; }

    public TerminalViewModel(TradingEngine tradingEngine, MarketDataService marketDataService, OpenAlgoService openAlgoService)
    {
        _tradingEngine = tradingEngine;
        _marketDataService = marketDataService;
        _openAlgoService = openAlgoService;

        PlaceBuyCommand = new RelayCommand(() => ExecuteMarketOrder(OrderType.Buy));
        PlaceSellCommand = new RelayCommand(() => ExecuteMarketOrder(OrderType.Sell));
        PlaceBuyLimitCommand = new RelayCommand(() => ExecutePendingOrder(OrderType.BuyLimit));
        PlaceSellLimitCommand = new RelayCommand(() => ExecutePendingOrder(OrderType.SellLimit));
        PlaceBuyStopCommand = new RelayCommand(() => ExecutePendingOrder(OrderType.BuyStop));
        PlaceSellStopCommand = new RelayCommand(() => ExecutePendingOrder(OrderType.SellStop));
        ClosePositionCommand = new RelayCommand<Position>(ClosePosition);
        CloseAllPositionsCommand = new RelayCommand(CloseAllPositions);
        ModifyPositionCommand = new RelayCommand<Position>(ModifyPosition);
        CancelOrderCommand = new RelayCommand<Order>(CancelOrder);
        CancelAllOrdersCommand = new RelayCommand(CancelAllOrders);
        OpenOrderDialogCommand = new RelayCommand(() => IsOrderDialogOpen = true);
        CloseOrderDialogCommand = new RelayCommand(() => IsOrderDialogOpen = false);
        SubmitOrderCommand = new RelayCommand(SubmitOrder);

        // Subscribe to trading events
        _tradingEngine.PositionOpened += (s, e) => RefreshPositions();
        _tradingEngine.PositionClosed += (s, e) => RefreshPositions();
        _tradingEngine.PositionModified += (s, e) => RefreshPositions();
        _tradingEngine.OrderPlaced += (s, e) => RefreshOrders();
        _tradingEngine.OrderCanceled += (s, e) => RefreshOrders();
        _tradingEngine.OrderFilled += (s, e) => { RefreshOrders(); RefreshHistory(); };
        _tradingEngine.DealExecuted += (s, e) => RefreshHistory();
        _tradingEngine.AccountUpdated += (s, e) => UpdateAccountInfo(e.Account);

        UpdateAccountInfo(_tradingEngine.GetAccount());

        AddJournalEntry("Terminal initialized");
    }

    public void SwitchToOpenAlgo()
    {
        _isOpenAlgoMode = true;
        AddJournalEntry("Switched to OpenAlgo live trading mode");

        // Wire up OpenAlgo trading events
        if (_openAlgoService.Trading != null)
        {
            _openAlgoService.Trading.PositionOpened += (s, e) => RefreshPositions();
            _openAlgoService.Trading.PositionClosed += (s, e) => RefreshPositions();
            _openAlgoService.Trading.PositionModified += (s, e) => RefreshPositions();
            _openAlgoService.Trading.OrderPlaced += (s, e) => RefreshOrders();
            _openAlgoService.Trading.OrderCanceled += (s, e) => RefreshOrders();
            _openAlgoService.Trading.OrderFilled += (s, e) => { RefreshOrders(); RefreshHistory(); };
            _openAlgoService.Trading.DealExecuted += (s, e) => RefreshHistory();
            _openAlgoService.Trading.AccountUpdated += (s, e) => UpdateAccountInfo(e.Account);

            UpdateAccountInfo(_openAlgoService.Trading.GetAccount());
        }

        RefreshPositions();
        RefreshOrders();
    }

    public void SwitchToSimulated()
    {
        _isOpenAlgoMode = false;
        AddJournalEntry("Switched to simulated trading mode");

        RefreshPositions();
        RefreshOrders();
        UpdateAccountInfo(_tradingEngine.GetAccount());
    }

    public void AddJournalEntry(string message)
    {
        JournalEntries.Insert(0, new JournalEntry
        {
            Time = DateTime.UtcNow,
            Source = _isOpenAlgoMode ? "OpenAlgo" : "System",
            Message = message
        });
    }

    private ITradingEngine ActiveEngine =>
        _isOpenAlgoMode && _openAlgoService.Trading != null
            ? _openAlgoService.Trading
            : _tradingEngine;

    public void ShowOrderDialog(string symbol)
    {
        OrderSymbol = symbol;

        if (_isOpenAlgoMode && _openAlgoService.MarketData != null)
        {
            var symbolInfo = _openAlgoService.MarketData.GetSymbol(symbol);
            if (symbolInfo != null)
            {
                OrderPrice = symbolInfo.Bid;
            }
        }
        else
        {
            var symbolInfo = _marketDataService.GetSymbol(symbol);
            if (symbolInfo != null)
            {
                OrderPrice = symbolInfo.Bid;
            }
        }
        IsOrderDialogOpen = true;
    }

    private void ExecuteMarketOrder(OrderType orderType)
    {
        var request = new TradeRequest
        {
            Action = TradeAction.Deal,
            Symbol = OrderSymbol,
            OrderType = orderType,
            Volume = OrderVolume,
            StopLoss = OrderStopLoss,
            TakeProfit = OrderTakeProfit,
            Comment = OrderComment
        };

        var result = ActiveEngine.SendOrder(request);
        LogTrade(result, orderType.ToString());
        AddJournalEntry($"Market order {orderType}: {OrderSymbol} x{OrderVolume} - {(result.Success ? "Success" : "Failed: " + result.Comment)}");
    }

    private void ExecutePendingOrder(OrderType orderType)
    {
        var request = new TradeRequest
        {
            Action = TradeAction.Pending,
            Symbol = OrderSymbol,
            OrderType = orderType,
            Volume = OrderVolume,
            Price = OrderPrice,
            StopLoss = OrderStopLoss,
            TakeProfit = OrderTakeProfit,
            Comment = OrderComment
        };

        var result = ActiveEngine.SendOrder(request);
        LogTrade(result, orderType.ToString());
        AddJournalEntry($"Pending order {orderType}: {OrderSymbol} @ {OrderPrice} x{OrderVolume} - {(result.Success ? "Success" : "Failed: " + result.Comment)}");
    }

    private void SubmitOrder()
    {
        if (OrderTypeValue == OrderType.Buy || OrderTypeValue == OrderType.Sell)
            ExecuteMarketOrder(OrderTypeValue);
        else
            ExecutePendingOrder(OrderTypeValue);

        IsOrderDialogOpen = false;
    }

    private void ClosePosition(Position? position)
    {
        if (position == null) return;
        var result = ActiveEngine.ClosePosition(position.Ticket);
        LogTrade(result, "Close Position");
        AddJournalEntry($"Close position {position.Symbol} - {(result.Success ? "Success" : "Failed: " + result.Comment)}");
    }

    private void CloseAllPositions()
    {
        ActiveEngine.CloseAllPositions();
        RefreshPositions();
        AddJournalEntry("Close all positions requested");
    }

    private void ModifyPosition(Position? position)
    {
        if (position == null) return;
        ActiveEngine.ModifyPosition(position.Ticket, OrderStopLoss, OrderTakeProfit);
        AddJournalEntry($"Modify position {position.Symbol}: SL={OrderStopLoss}, TP={OrderTakeProfit}");
    }

    private void CancelOrder(Order? order)
    {
        if (order == null) return;
        ActiveEngine.CancelOrder(order.Ticket);
        AddJournalEntry($"Cancel order #{order.Ticket} {order.Symbol}");
    }

    private void CancelAllOrders()
    {
        foreach (var order in ActiveEngine.GetPendingOrders().ToList())
        {
            ActiveEngine.CancelOrder(order.Ticket);
        }
        RefreshOrders();
        AddJournalEntry("Cancel all orders requested");
    }

    private void RefreshPositions()
    {
        OpenPositions.Clear();
        foreach (var pos in ActiveEngine.GetOpenPositions())
        {
            OpenPositions.Add(new OpenPositionEntry
            {
                Symbol = pos.Symbol,
                Type = pos.Type.ToString(),
                Volume = pos.Volume,
                OpenPrice = pos.PriceOpen,
                CurrentPrice = pos.PriceCurrent,
                StopLoss = pos.StopLoss,
                TakeProfit = pos.TakeProfit,
                Profit = pos.Profit,
                Swap = pos.Swap,
                Ticket = pos.Ticket
            });
        }
    }

    private void RefreshOrders()
    {
        PendingOrders.Clear();
        foreach (var order in ActiveEngine.GetPendingOrders())
            PendingOrders.Add(order);
    }

    private void RefreshHistory()
    {
        TradeHistory.Clear();
        var deals = ActiveEngine.GetDeals(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);
        foreach (var deal in deals)
        {
            TradeHistory.Add(new HistoryEntry
            {
                Time = deal.Time,
                Symbol = deal.Symbol,
                Type = deal.Type.ToString(),
                Volume = deal.Volume,
                Price = deal.Price,
                Profit = deal.Profit,
                Comment = deal.Comment
            });
        }

        OrderHistory.Clear();
        var orders = ActiveEngine.GetOrderHistory(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);
        foreach (var order in orders)
            OrderHistory.Add(order);
    }

    private void UpdateAccountInfo(Account account)
    {
        Balance = account.Balance;
        Equity = account.Equity;
        Margin = account.Margin;
        FreeMargin = account.FreeMargin;
        MarginLevel = account.MarginLevel;
        Profit = account.Profit;
    }

    private void LogTrade(TradeResult result, string action)
    {
        TradeLog.Insert(0, new TradeLogEntry
        {
            Time = DateTime.UtcNow,
            Message = $"{action}: {(result.Success ? "Success" : "Failed")} - {result.Comment} (Order #{result.OrderTicket}, Price: {result.Price}, Volume: {result.Volume})",
            Level = result.Success ? "Trade" : "Error"
        });
    }
}
