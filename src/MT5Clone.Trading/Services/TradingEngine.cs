using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;

namespace MT5Clone.Trading.Services;

public class TradingEngine : ITradingEngine
{
    private readonly Account _account;
    private readonly List<Order> _pendingOrders = new();
    private readonly List<Position> _openPositions = new();
    private readonly List<Deal> _deals = new();
    private readonly List<Order> _orderHistory = new();
    private readonly Dictionary<string, Symbol> _symbols = new();
    private long _nextTicket = 100000;
    private readonly object _lock = new();

    public event EventHandler<OrderEventArgs>? OrderPlaced;
    public event EventHandler<OrderEventArgs>? OrderModified;
    public event EventHandler<OrderEventArgs>? OrderCanceled;
    public event EventHandler<OrderEventArgs>? OrderFilled;
    public event EventHandler<PositionEventArgs>? PositionOpened;
    public event EventHandler<PositionEventArgs>? PositionModified;
    public event EventHandler<PositionEventArgs>? PositionClosed;
    public event EventHandler<DealEventArgs>? DealExecuted;
    public event EventHandler<AccountEventArgs>? AccountUpdated;

    public TradingEngine(Account? account = null)
    {
        _account = account ?? new Account
        {
            Login = 12345678,
            Name = "Demo Account",
            Server = "MT5Clone-Demo",
            Company = "MT5Clone",
            Currency = "USD",
            Balance = 10000.00,
            Equity = 10000.00,
            FreeMargin = 10000.00,
            Leverage = 100,
            AccountType = AccountType.Demo,
            TradeMode = AccountTradeMode.Demo,
            MarginMode = AccountMarginMode.RetailHedging,
            TradeAllowed = true,
            ExpertAllowed = true
        };
    }

    public void RegisterSymbol(Symbol symbol)
    {
        _symbols[symbol.Name] = symbol;
    }

    public TradeResult SendOrder(TradeRequest request)
    {
        lock (_lock)
        {
            if (!_account.TradeAllowed)
                return TradeResult.Failed("Trading is not allowed");

            if (!_symbols.TryGetValue(request.Symbol, out var symbol))
                return TradeResult.Failed($"Symbol {request.Symbol} not found");

            if (request.Volume < symbol.MinLot || request.Volume > symbol.MaxLot)
                return TradeResult.Failed($"Invalid volume: {request.Volume}");

            if (request.Volume % symbol.LotStep != 0)
            {
                request.Volume = Math.Round(request.Volume / symbol.LotStep) * symbol.LotStep;
            }

            switch (request.Action)
            {
                case TradeAction.Deal:
                    return ExecuteMarketOrder(request, symbol);
                case TradeAction.Pending:
                    return PlacePendingOrder(request, symbol);
                default:
                    return TradeResult.Failed("Invalid trade action");
            }
        }
    }

    private TradeResult ExecuteMarketOrder(TradeRequest request, Symbol symbol)
    {
        double executePrice;
        PositionType posType;

        if (request.OrderType == OrderType.Buy)
        {
            executePrice = symbol.Ask;
            posType = PositionType.Buy;
        }
        else if (request.OrderType == OrderType.Sell)
        {
            executePrice = symbol.Bid;
            posType = PositionType.Sell;
        }
        else
        {
            return TradeResult.Failed("Invalid order type for market execution");
        }

        // Check margin
        double requiredMargin = CalculateMargin(symbol, request.Volume, executePrice);
        if (requiredMargin > _account.FreeMargin)
            return TradeResult.Failed("Not enough money");

        long ticket = _nextTicket++;
        var now = DateTime.UtcNow;

        // Create order
        var order = new Order
        {
            Ticket = ticket,
            Symbol = request.Symbol,
            Type = request.OrderType,
            State = OrderState.Filled,
            FillingType = request.FillingType,
            Volume = request.Volume,
            VolumeInitial = request.Volume,
            VolumeCurrent = 0,
            Price = executePrice,
            PriceCurrent = executePrice,
            StopLoss = request.StopLoss,
            TakeProfit = request.TakeProfit,
            TimeSetup = now,
            TimeDone = now,
            Magic = request.Magic,
            Comment = request.Comment
        };
        _orderHistory.Add(order);
        OrderFilled?.Invoke(this, new OrderEventArgs(order));

        // Create deal
        var deal = new Deal
        {
            Ticket = _nextTicket++,
            OrderTicket = ticket,
            Symbol = request.Symbol,
            Type = request.OrderType == OrderType.Buy ? DealType.Buy : DealType.Sell,
            Entry = DealEntry.In,
            Volume = request.Volume,
            Price = executePrice,
            Time = now,
            Commission = -request.Volume * 3.5,
            PositionId = ticket,
            Magic = request.Magic,
            Comment = request.Comment
        };
        _deals.Add(deal);
        DealExecuted?.Invoke(this, new DealEventArgs(deal));

        // Create position
        var position = new Position
        {
            Ticket = ticket,
            Symbol = request.Symbol,
            Type = posType,
            Volume = request.Volume,
            PriceOpen = executePrice,
            PriceCurrent = executePrice,
            StopLoss = request.StopLoss,
            TakeProfit = request.TakeProfit,
            Time = now,
            TimeUpdate = now,
            Commission = deal.Commission,
            Identifier = ticket,
            Magic = request.Magic,
            Comment = request.Comment
        };
        _openPositions.Add(position);
        PositionOpened?.Invoke(this, new PositionEventArgs(position));

        // Update account
        _account.Margin += requiredMargin;
        _account.Commission += deal.Commission;
        UpdateAccountEquity();

        return TradeResult.Succeeded(ticket, executePrice, request.Volume);
    }

    private TradeResult PlacePendingOrder(TradeRequest request, Symbol symbol)
    {
        if (request.Price <= 0)
            return TradeResult.Failed("Invalid price");

        long ticket = _nextTicket++;

        var order = new Order
        {
            Ticket = ticket,
            Symbol = request.Symbol,
            Type = request.OrderType,
            State = OrderState.Placed,
            FillingType = request.FillingType,
            TimeInForce = request.TimeInForce,
            Volume = request.Volume,
            VolumeInitial = request.Volume,
            VolumeCurrent = request.Volume,
            Price = request.Price,
            PriceTrigger = request.StopLimitPrice,
            StopLoss = request.StopLoss,
            TakeProfit = request.TakeProfit,
            TimeSetup = DateTime.UtcNow,
            TimeExpiration = request.Expiration,
            Magic = request.Magic,
            Comment = request.Comment
        };
        _pendingOrders.Add(order);
        OrderPlaced?.Invoke(this, new OrderEventArgs(order));

        return TradeResult.Succeeded(ticket, request.Price, request.Volume);
    }

    public TradeResult ModifyOrder(TradeRequest request)
    {
        lock (_lock)
        {
            var order = _pendingOrders.FirstOrDefault(o => o.Ticket == request.OrderTicket);
            if (order == null)
                return TradeResult.Failed("Order not found");

            if (request.Price > 0) order.Price = request.Price;
            if (request.StopLoss >= 0) order.StopLoss = request.StopLoss;
            if (request.TakeProfit >= 0) order.TakeProfit = request.TakeProfit;
            if (request.Expiration != default) order.TimeExpiration = request.Expiration;

            OrderModified?.Invoke(this, new OrderEventArgs(order));
            return TradeResult.Succeeded(order.Ticket, order.Price, order.VolumeCurrent);
        }
    }

    public TradeResult CancelOrder(long orderTicket)
    {
        lock (_lock)
        {
            var order = _pendingOrders.FirstOrDefault(o => o.Ticket == orderTicket);
            if (order == null)
                return TradeResult.Failed("Order not found");

            order.State = OrderState.Canceled;
            order.TimeDone = DateTime.UtcNow;
            _pendingOrders.Remove(order);
            _orderHistory.Add(order);
            OrderCanceled?.Invoke(this, new OrderEventArgs(order));

            return TradeResult.Succeeded(order.Ticket, order.Price, 0);
        }
    }

    public TradeResult ModifyPosition(long positionTicket, double stopLoss, double takeProfit)
    {
        lock (_lock)
        {
            var position = _openPositions.FirstOrDefault(p => p.Ticket == positionTicket);
            if (position == null)
                return TradeResult.Failed("Position not found");

            position.StopLoss = stopLoss;
            position.TakeProfit = takeProfit;
            position.TimeUpdate = DateTime.UtcNow;
            PositionModified?.Invoke(this, new PositionEventArgs(position));

            return TradeResult.Succeeded(position.Ticket, position.PriceCurrent, position.Volume);
        }
    }

    public TradeResult ClosePosition(long positionTicket, double volume = 0)
    {
        lock (_lock)
        {
            var position = _openPositions.FirstOrDefault(p => p.Ticket == positionTicket);
            if (position == null)
                return TradeResult.Failed("Position not found");

            if (!_symbols.TryGetValue(position.Symbol, out var symbol))
                return TradeResult.Failed("Symbol not found");

            double closeVolume = volume > 0 ? Math.Min(volume, position.Volume) : position.Volume;
            double closePrice = position.Type == PositionType.Buy ? symbol.Bid : symbol.Ask;

            // Calculate profit
            double profit = CalculatePositionProfit(position, closePrice, symbol, closeVolume);

            // Create close deal
            var deal = new Deal
            {
                Ticket = _nextTicket++,
                OrderTicket = _nextTicket++,
                Symbol = position.Symbol,
                Type = position.Type == PositionType.Buy ? DealType.Sell : DealType.Buy,
                Entry = DealEntry.Out,
                Volume = closeVolume,
                Price = closePrice,
                Time = DateTime.UtcNow,
                Profit = profit,
                Commission = -closeVolume * 3.5,
                Swap = position.Swap * (closeVolume / position.Volume),
                PositionId = position.Identifier,
                Magic = position.Magic,
                Comment = position.Comment
            };
            _deals.Add(deal);
            DealExecuted?.Invoke(this, new DealEventArgs(deal));

            if (closeVolume >= position.Volume)
            {
                // Full close
                _openPositions.Remove(position);
                _account.Balance += profit + deal.Commission + deal.Swap;
                _account.Margin -= CalculateMargin(symbol, closeVolume, position.PriceOpen);
            }
            else
            {
                // Partial close
                position.Volume -= closeVolume;
                position.TimeUpdate = DateTime.UtcNow;
                _account.Balance += profit + deal.Commission + deal.Swap;
                _account.Margin -= CalculateMargin(symbol, closeVolume, position.PriceOpen);
            }

            UpdateAccountEquity();
            PositionClosed?.Invoke(this, new PositionEventArgs(position));

            return TradeResult.Succeeded(deal.Ticket, closePrice, closeVolume);
        }
    }

    public TradeResult ClosePositionBy(long positionTicket, long oppositePositionTicket)
    {
        lock (_lock)
        {
            var position = _openPositions.FirstOrDefault(p => p.Ticket == positionTicket);
            var opposite = _openPositions.FirstOrDefault(p => p.Ticket == oppositePositionTicket);

            if (position == null || opposite == null)
                return TradeResult.Failed("Position not found");

            if (position.Symbol != opposite.Symbol || position.Type == opposite.Type)
                return TradeResult.Failed("Invalid close by operation");

            double closeVolume = Math.Min(position.Volume, opposite.Volume);

            // Close both positions
            var result1 = ClosePosition(positionTicket, closeVolume);
            if (!result1.Success) return result1;

            var result2 = ClosePosition(oppositePositionTicket, closeVolume);
            return result2;
        }
    }

    public TradeResult CloseAllPositions(string? symbol = null)
    {
        lock (_lock)
        {
            var positionsToClose = symbol != null
                ? _openPositions.Where(p => p.Symbol == symbol).ToList()
                : _openPositions.ToList();

            foreach (var position in positionsToClose)
            {
                ClosePosition(position.Ticket);
            }

            return TradeResult.Succeeded(0, 0, 0);
        }
    }

    public void ProcessTick(Tick tick)
    {
        lock (_lock)
        {
            if (_symbols.TryGetValue(tick.Symbol, out var symbol))
            {
                symbol.Bid = tick.Bid;
                symbol.Ask = tick.Ask;
            }

            // Update open positions (snapshot to avoid collection modification during iteration)
            foreach (var position in _openPositions.Where(p => p.Symbol == tick.Symbol).ToList())
            {
                position.PriceCurrent = position.Type == PositionType.Buy ? tick.Bid : tick.Ask;
                if (symbol != null)
                {
                    position.Profit = CalculatePositionProfit(position, position.PriceCurrent, symbol, position.Volume);
                }

                // Check stop loss
                if (position.StopLoss > 0)
                {
                    bool slHit = position.Type == PositionType.Buy
                        ? tick.Bid <= position.StopLoss
                        : tick.Ask >= position.StopLoss;

                    if (slHit) ClosePosition(position.Ticket);
                }

                // Check take profit
                if (position.TakeProfit > 0)
                {
                    bool tpHit = position.Type == PositionType.Buy
                        ? tick.Bid >= position.TakeProfit
                        : tick.Ask <= position.TakeProfit;

                    if (tpHit) ClosePosition(position.Ticket);
                }
            }

            // Check pending orders
            var pendingToProcess = _pendingOrders.Where(o => o.Symbol == tick.Symbol).ToList();
            foreach (var order in pendingToProcess)
            {
                bool shouldExecute = order.Type switch
                {
                    OrderType.BuyLimit => tick.Ask <= order.Price,
                    OrderType.SellLimit => tick.Bid >= order.Price,
                    OrderType.BuyStop => tick.Ask >= order.Price,
                    OrderType.SellStop => tick.Bid <= order.Price,
                    _ => false
                };

                if (shouldExecute)
                {
                    _pendingOrders.Remove(order);
                    var request = new TradeRequest
                    {
                        Action = TradeAction.Deal,
                        Symbol = order.Symbol,
                        Volume = order.VolumeCurrent,
                        OrderType = order.IsBuyOrder ? OrderType.Buy : OrderType.Sell,
                        StopLoss = order.StopLoss,
                        TakeProfit = order.TakeProfit,
                        Magic = order.Magic,
                        Comment = order.Comment
                    };
                    ExecuteMarketOrder(request, symbol!);
                    order.State = OrderState.Filled;
                    order.TimeDone = DateTime.UtcNow;
                    _orderHistory.Add(order);
                }

                // Check expiration
                if (order.TimeExpiration != default && DateTime.UtcNow >= order.TimeExpiration)
                {
                    order.State = OrderState.Expired;
                    order.TimeDone = DateTime.UtcNow;
                    _pendingOrders.Remove(order);
                    _orderHistory.Add(order);
                }
            }

            UpdateAccountEquity();
            AccountUpdated?.Invoke(this, new AccountEventArgs(_account));
        }
    }

    private double CalculatePositionProfit(Position position, double currentPrice, Symbol symbol, double volume)
    {
        double direction = position.Type == PositionType.Buy ? 1.0 : -1.0;
        double priceDiff = (currentPrice - position.PriceOpen) * direction;
        double profit = priceDiff * volume * symbol.ContractSize;

        if (symbol.SymbolType == SymbolType.Forex && symbol.QuoteCurrency != "USD")
        {
            // Simplified conversion - in real MT5 this would use cross rates
            profit *= 1.0;
        }

        return Math.Round(profit, 2);
    }

    private double CalculateMargin(Symbol symbol, double volume, double price)
    {
        double margin = (volume * symbol.ContractSize * price) / _account.Leverage;
        return Math.Round(margin, 2);
    }

    private void UpdateAccountEquity()
    {
        double unrealizedPnL = _openPositions.Sum(p => p.Profit + p.Swap + p.Commission);
        _account.UpdateEquity(unrealizedPnL);
    }

    public IReadOnlyList<Order> GetPendingOrders() => _pendingOrders.AsReadOnly();
    public IReadOnlyList<Position> GetOpenPositions() => _openPositions.AsReadOnly();

    public IReadOnlyList<Deal> GetDeals(DateTime from, DateTime to)
        => _deals.Where(d => d.Time >= from && d.Time <= to).ToList();

    public IReadOnlyList<Order> GetOrderHistory(DateTime from, DateTime to)
        => _orderHistory.Where(o => o.TimeSetup >= from && o.TimeSetup <= to).ToList();

    public Account GetAccount() => _account;
}
