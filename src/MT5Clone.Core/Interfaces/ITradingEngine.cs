using MT5Clone.Core.Models;

namespace MT5Clone.Core.Interfaces;

public interface ITradingEngine
{
    event EventHandler<OrderEventArgs>? OrderPlaced;
    event EventHandler<OrderEventArgs>? OrderModified;
    event EventHandler<OrderEventArgs>? OrderCanceled;
    event EventHandler<OrderEventArgs>? OrderFilled;
    event EventHandler<PositionEventArgs>? PositionOpened;
    event EventHandler<PositionEventArgs>? PositionModified;
    event EventHandler<PositionEventArgs>? PositionClosed;
    event EventHandler<DealEventArgs>? DealExecuted;
    event EventHandler<AccountEventArgs>? AccountUpdated;

    TradeResult SendOrder(TradeRequest request);
    TradeResult ModifyOrder(TradeRequest request);
    TradeResult CancelOrder(long orderTicket);
    TradeResult ModifyPosition(long positionTicket, double stopLoss, double takeProfit);
    TradeResult ClosePosition(long positionTicket, double volume = 0);
    TradeResult ClosePositionBy(long positionTicket, long oppositePositionTicket);
    TradeResult CloseAllPositions(string? symbol = null);

    IReadOnlyList<Order> GetPendingOrders();
    IReadOnlyList<Position> GetOpenPositions();
    IReadOnlyList<Deal> GetDeals(DateTime from, DateTime to);
    IReadOnlyList<Order> GetOrderHistory(DateTime from, DateTime to);
    Account GetAccount();

    void ProcessTick(Tick tick);
}

public class OrderEventArgs : EventArgs
{
    public Order Order { get; }
    public OrderEventArgs(Order order) => Order = order;
}

public class PositionEventArgs : EventArgs
{
    public Position Position { get; }
    public PositionEventArgs(Position position) => Position = position;
}

public class DealEventArgs : EventArgs
{
    public Deal Deal { get; }
    public DealEventArgs(Deal deal) => Deal = deal;
}

public class AccountEventArgs : EventArgs
{
    public Account Account { get; }
    public AccountEventArgs(Account account) => Account = account;
}
