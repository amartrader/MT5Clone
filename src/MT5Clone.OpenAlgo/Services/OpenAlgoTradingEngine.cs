using MT5Clone.Core.Enums;
using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.OpenAlgo.Models;

namespace MT5Clone.OpenAlgo.Services;

public class OpenAlgoTradingEngine : ITradingEngine
{
    private readonly OpenAlgoApiClient _client;
    private readonly OpenAlgoConfig _config;
    private readonly IMarketDataProvider _marketData;

    private readonly List<Order> _pendingOrders = new();
    private readonly List<Position> _openPositions = new();
    private readonly List<Deal> _deals = new();
    private readonly List<Order> _orderHistory = new();
    private Account _account;
    private long _nextTicket = 1;
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

    public OpenAlgoTradingEngine(OpenAlgoApiClient client, OpenAlgoConfig config, IMarketDataProvider marketData)
    {
        _client = client;
        _config = config;
        _marketData = marketData;
        _account = new Account
        {
            Login = 0,
            Name = "OpenAlgo Account",
            Server = config.Host,
            Currency = "INR",
            Balance = 0,
            Equity = 0,
            FreeMargin = 0,
            Leverage = 1
        };
    }

    public TradeResult SendOrder(TradeRequest request)
    {
        return SendOrderAsync(request).GetAwaiter().GetResult();
    }

    public async Task<TradeResult> SendOrderAsync(TradeRequest request)
    {
        var parts = request.Symbol.Split(':', 2);
        if (parts.Length != 2)
            return TradeResult.Failed("Invalid symbol format. Use EXCHANGE:SYMBOL (e.g., NSE:RELIANCE)");

        var exchange = parts[0];
        var symbol = parts[1];
        var action = request.OrderType == OrderType.Buy || request.OrderType == OrderType.BuyLimit ||
                     request.OrderType == OrderType.BuyStop ? "BUY" : "SELL";

        var priceType = request.OrderType switch
        {
            OrderType.Buy or OrderType.Sell => "MARKET",
            OrderType.BuyLimit or OrderType.SellLimit => "LIMIT",
            OrderType.BuyStop or OrderType.SellStop => "SL-M",
            OrderType.BuyStopLimit or OrderType.SellStopLimit => "SL",
            _ => "MARKET"
        };

        var product = "MIS"; // Default to intraday

        var result = await _client.PlaceOrderAsync(
            symbol, action, exchange,
            priceType, product,
            (int)request.Volume,
            request.Price,
            request.StopLimitPrice > 0 ? request.StopLimitPrice : request.Price);

        if (result.IsSuccess)
        {
            var ticket = _nextTicket++;
            var order = new Order
            {
                Ticket = ticket,
                Symbol = request.Symbol,
                Type = request.OrderType,
                State = OrderState.Placed,
                Volume = request.Volume,
                VolumeInitial = request.Volume,
                VolumeCurrent = request.Volume,
                Price = request.Price,
                StopLoss = request.StopLoss,
                TakeProfit = request.TakeProfit,
                TimeSetup = DateTime.UtcNow,
                Comment = request.Comment,
                ExternalId = result.OrderId ?? string.Empty
            };

            lock (_lock)
            {
                if (order.IsMarketOrder)
                {
                    order.State = OrderState.Filled;
                    order.TimeDone = DateTime.UtcNow;
                    _orderHistory.Add(order);

                    var position = new Position
                    {
                        Ticket = ticket,
                        Symbol = request.Symbol,
                        Type = action == "BUY" ? PositionType.Buy : PositionType.Sell,
                        Volume = request.Volume,
                        PriceOpen = request.Price > 0 ? request.Price : (_marketData.GetLastTick(request.Symbol)?.Last ?? 0),
                        PriceCurrent = _marketData.GetLastTick(request.Symbol)?.Last ?? request.Price,
                        StopLoss = request.StopLoss,
                        TakeProfit = request.TakeProfit,
                        Time = DateTime.UtcNow,
                        TimeUpdate = DateTime.UtcNow,
                        ExternalId = result.OrderId ?? string.Empty
                    };
                    _openPositions.Add(position);

                    var deal = new Deal
                    {
                        Ticket = _nextTicket++,
                        OrderTicket = ticket,
                        Symbol = request.Symbol,
                        Type = action == "BUY" ? DealType.Buy : DealType.Sell,
                        Entry = DealEntry.In,
                        Volume = request.Volume,
                        Price = position.PriceOpen,
                        Time = DateTime.UtcNow,
                        ExternalId = result.OrderId ?? string.Empty
                    };
                    _deals.Add(deal);

                    OrderFilled?.Invoke(this, new OrderEventArgs(order));
                    PositionOpened?.Invoke(this, new PositionEventArgs(position));
                    DealExecuted?.Invoke(this, new DealEventArgs(deal));
                }
                else
                {
                    _pendingOrders.Add(order);
                    OrderPlaced?.Invoke(this, new OrderEventArgs(order));
                }
            }

            return TradeResult.Succeeded(ticket, request.Price, request.Volume);
        }

        return TradeResult.Failed(result.Message ?? "Order placement failed");
    }

    public TradeResult ModifyOrder(TradeRequest request)
    {
        lock (_lock)
        {
            var order = _pendingOrders.FirstOrDefault(o => o.Ticket == request.OrderTicket);
            if (order == null)
                return TradeResult.Failed("Order not found");

            var parts = order.Symbol.Split(':', 2);
            if (parts.Length != 2) return TradeResult.Failed("Invalid symbol format");

            if (string.IsNullOrEmpty(order.ExternalId))
                return TradeResult.Failed("No external order ID available");

            var action = order.IsBuyOrder ? "BUY" : "SELL";
            var priceType = order.Type switch
            {
                OrderType.BuyLimit or OrderType.SellLimit => "LIMIT",
                OrderType.BuyStop or OrderType.SellStop => "SL-M",
                OrderType.BuyStopLimit or OrderType.SellStopLimit => "SL",
                _ => "LIMIT"
            };

            var result = _client.ModifyOrderAsync(
                order.ExternalId, parts[1], action, parts[0],
                "MIS", (int)request.Volume, request.Price,
                priceType, request.StopLimitPrice).GetAwaiter().GetResult();

            if (result.IsSuccess)
            {
                order.Price = request.Price;
                order.Volume = request.Volume;
                order.VolumeCurrent = request.Volume;
                order.StopLoss = request.StopLoss;
                order.TakeProfit = request.TakeProfit;
                order.State = OrderState.Placed;

                OrderModified?.Invoke(this, new OrderEventArgs(order));
                return TradeResult.Succeeded(order.Ticket, request.Price, request.Volume);
            }

            return TradeResult.Failed(result.Message ?? "Order modification failed");
        }
    }

    public TradeResult CancelOrder(long orderTicket)
    {
        lock (_lock)
        {
            var order = _pendingOrders.FirstOrDefault(o => o.Ticket == orderTicket);
            if (order == null)
                return TradeResult.Failed("Order not found");

            if (!string.IsNullOrEmpty(order.ExternalId))
            {
                var result = _client.CancelOrderAsync(order.ExternalId).GetAwaiter().GetResult();
                if (!result.IsSuccess)
                    return TradeResult.Failed(result.Message ?? "Cancel failed");
            }

            order.State = OrderState.Canceled;
            order.TimeDone = DateTime.UtcNow;
            _pendingOrders.Remove(order);
            _orderHistory.Add(order);

            OrderCanceled?.Invoke(this, new OrderEventArgs(order));
            return TradeResult.Succeeded(order.Ticket, order.Price, order.Volume);
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
            return TradeResult.Succeeded(positionTicket, position.PriceOpen, position.Volume);
        }
    }

    public TradeResult ClosePosition(long positionTicket, double volume = 0)
    {
        lock (_lock)
        {
            var position = _openPositions.FirstOrDefault(p => p.Ticket == positionTicket);
            if (position == null)
                return TradeResult.Failed("Position not found");

            var parts = position.Symbol.Split(':', 2);
            if (parts.Length != 2) return TradeResult.Failed("Invalid symbol format");

            var closeAction = position.Type == PositionType.Buy ? "SELL" : "BUY";
            var closeVolume = volume > 0 ? volume : position.Volume;

            var result = _client.PlaceOrderAsync(
                parts[1], closeAction, parts[0],
                "MARKET", "MIS", (int)closeVolume).GetAwaiter().GetResult();

            if (result.IsSuccess)
            {
                var closePrice = _marketData.GetLastTick(position.Symbol)?.Last ?? position.PriceCurrent;

                if (volume > 0 && volume < position.Volume)
                {
                    position.Volume -= volume;
                    position.TimeUpdate = DateTime.UtcNow;
                    PositionModified?.Invoke(this, new PositionEventArgs(position));
                }
                else
                {
                    _openPositions.Remove(position);
                    PositionClosed?.Invoke(this, new PositionEventArgs(position));
                }

                var deal = new Deal
                {
                    Ticket = _nextTicket++,
                    OrderTicket = position.Ticket,
                    Symbol = position.Symbol,
                    Type = closeAction == "BUY" ? DealType.Buy : DealType.Sell,
                    Entry = DealEntry.Out,
                    Volume = closeVolume,
                    Price = closePrice,
                    Profit = position.Profit,
                    Time = DateTime.UtcNow,
                    ExternalId = result.OrderId ?? string.Empty
                };
                _deals.Add(deal);
                DealExecuted?.Invoke(this, new DealEventArgs(deal));

                return TradeResult.Succeeded(position.Ticket, closePrice, closeVolume);
            }

            return TradeResult.Failed(result.Message ?? "Close position failed");
        }
    }

    public TradeResult ClosePositionBy(long positionTicket, long oppositePositionTicket)
    {
        return TradeResult.Failed("Close by opposite position not supported via OpenAlgo");
    }

    public TradeResult CloseAllPositions(string? symbol = null)
    {
        var result = _client.CloseAllPositionsAsync().GetAwaiter().GetResult();

        if (result.IsSuccess)
        {
            lock (_lock)
            {
                var positionsToClose = symbol != null
                    ? _openPositions.Where(p => p.Symbol == symbol).ToList()
                    : _openPositions.ToList();

                foreach (var pos in positionsToClose)
                {
                    _openPositions.Remove(pos);
                    PositionClosed?.Invoke(this, new PositionEventArgs(pos));
                }
            }
            return TradeResult.Succeeded(0, 0, 0);
        }

        return TradeResult.Failed(result.Message ?? "Close all positions failed");
    }

    public IReadOnlyList<Order> GetPendingOrders()
    {
        lock (_lock) { return _pendingOrders.ToList().AsReadOnly(); }
    }

    public IReadOnlyList<Position> GetOpenPositions()
    {
        lock (_lock) { return _openPositions.ToList().AsReadOnly(); }
    }

    public IReadOnlyList<Deal> GetDeals(DateTime from, DateTime to)
    {
        lock (_lock) { return _deals.Where(d => d.Time >= from && d.Time <= to).ToList().AsReadOnly(); }
    }

    public IReadOnlyList<Order> GetOrderHistory(DateTime from, DateTime to)
    {
        lock (_lock) { return _orderHistory.Where(o => o.TimeSetup >= from && o.TimeSetup <= to).ToList().AsReadOnly(); }
    }

    public Account GetAccount() => _account;

    public void ProcessTick(Tick tick)
    {
        lock (_lock)
        {
            foreach (var position in _openPositions)
            {
                if (position.Symbol == tick.Symbol)
                {
                    position.PriceCurrent = tick.Last;
                }
            }
        }
    }

    /// <summary>
    /// Refresh account data from OpenAlgo (funds, positions, orders).
    /// </summary>
    public async Task RefreshAccountDataAsync(CancellationToken ct = default)
    {
        // Fetch funds
        var funds = await _client.GetFundsAsync(ct);
        if (funds.IsSuccess && funds.Data != null)
        {
            if (decimal.TryParse(funds.Data.AvailableCash, out var cash))
                _account.Balance = (double)cash;
            _account.Equity = _account.Balance;
            _account.FreeMargin = _account.Balance;
            if (decimal.TryParse(funds.Data.UtilisedDebits, out var used))
                _account.Margin = (double)used;
            _account.FreeMargin = _account.Balance - _account.Margin;
            _account.Equity = _account.Balance + _account.Profit;

            AccountUpdated?.Invoke(this, new AccountEventArgs(_account));
        }

        // Fetch positions
        var positions = await _client.GetPositionBookAsync(ct);
        if (positions.IsSuccess && positions.Data != null)
        {
            lock (_lock)
            {
                _openPositions.Clear();
                foreach (var pb in positions.Data)
                {
                    if (string.IsNullOrEmpty(pb.Quantity)) continue;
                    if (!int.TryParse(pb.Quantity, out var qty) || qty == 0) continue;

                    _openPositions.Add(new Position
                    {
                        Ticket = _nextTicket++,
                        Symbol = $"{pb.Exchange}:{pb.Symbol}",
                        Type = qty > 0 ? PositionType.Buy : PositionType.Sell,
                        Volume = Math.Abs(qty),
                        PriceOpen = decimal.TryParse(pb.AveragePrice, out var ap) ? (double)ap : 0,
                        PriceCurrent = decimal.TryParse(pb.Ltp, out var ltp) ? (double)ltp : 0,
                        Profit = decimal.TryParse(pb.Pnl, out var pnl) ? (double)pnl : 0,
                        Time = DateTime.UtcNow,
                        TimeUpdate = DateTime.UtcNow,
                        ExternalId = $"{pb.Exchange}:{pb.Symbol}"
                    });
                }
            }
        }

        // Update total profit
        lock (_lock)
        {
            _account.Profit = _openPositions.Sum(p => p.Profit);
            _account.Equity = _account.Balance + _account.Profit;
        }
    }

    /// <summary>
    /// Refresh order book from OpenAlgo.
    /// </summary>
    public async Task RefreshOrderBookAsync(CancellationToken ct = default)
    {
        var orderBook = await _client.GetOrderBookAsync(ct);
        if (orderBook.IsSuccess && orderBook.Data?.Orders != null)
        {
            lock (_lock)
            {
                _pendingOrders.Clear();
                _orderHistory.Clear();

                foreach (var ob in orderBook.Data.Orders)
                {
                    var order = new Order
                    {
                        Ticket = _nextTicket++,
                        Symbol = $"{ob.Exchange}:{ob.Symbol}",
                        Type = MapPriceTypeToOrderType(ob.PriceType, ob.Action),
                        Volume = int.TryParse(ob.Quantity, out var q) ? q : 0,
                        VolumeInitial = int.TryParse(ob.Quantity, out var qi) ? qi : 0,
                        VolumeCurrent = int.TryParse(ob.Quantity, out var qc) ? qc : 0,
                        Price = (double)ob.Price,
                        PriceTrigger = (double)ob.TriggerPrice,
                        ExternalId = ob.OrderId ?? string.Empty,
                        Comment = ob.Product ?? string.Empty
                    };

                    var status = ob.OrderStatus?.ToUpper();
                    if (status == "OPEN" || status == "PENDING" || status == "TRIGGER PENDING")
                    {
                        order.State = OrderState.Placed;
                        _pendingOrders.Add(order);
                    }
                    else
                    {
                        order.State = status switch
                        {
                            "COMPLETE" or "COMPLETED" => OrderState.Filled,
                            "CANCELLED" or "CANCELED" => OrderState.Canceled,
                            "REJECTED" => OrderState.Rejected,
                            _ => OrderState.Filled
                        };
                        _orderHistory.Add(order);
                    }
                }
            }
        }
    }

    private static OrderType MapPriceTypeToOrderType(string? priceType, string? action)
    {
        var isBuy = string.Equals(action, "BUY", StringComparison.OrdinalIgnoreCase);
        return priceType?.ToUpper() switch
        {
            "MARKET" => isBuy ? OrderType.Buy : OrderType.Sell,
            "LIMIT" => isBuy ? OrderType.BuyLimit : OrderType.SellLimit,
            "SL-M" => isBuy ? OrderType.BuyStop : OrderType.SellStop,
            "SL" => isBuy ? OrderType.BuyStopLimit : OrderType.SellStopLimit,
            _ => isBuy ? OrderType.Buy : OrderType.Sell
        };
    }
}
