using System.Text.Json.Serialization;

namespace MT5Clone.OpenAlgo.Models;

public class BaseResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonIgnore]
    public bool IsSuccess => string.Equals(Status, "success", StringComparison.OrdinalIgnoreCase);
}

public class OrderResponse : BaseResponse
{
    [JsonPropertyName("orderid")]
    public string? OrderId { get; set; }
}

public class CancelAllOrderResponse : BaseResponse
{
    [JsonPropertyName("canceled_orders")]
    public List<string>? CanceledOrders { get; set; }

    [JsonPropertyName("failed_cancellations")]
    public List<string>? FailedCancellations { get; set; }
}

public class ClosePositionResponse : BaseResponse { }

public class QuotesResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public QuoteData? Data { get; set; }
}

public class QuoteData
{
    [JsonPropertyName("open")]
    public decimal Open { get; set; }

    [JsonPropertyName("high")]
    public decimal High { get; set; }

    [JsonPropertyName("low")]
    public decimal Low { get; set; }

    [JsonPropertyName("ltp")]
    public decimal Ltp { get; set; }

    [JsonPropertyName("ask")]
    public decimal Ask { get; set; }

    [JsonPropertyName("bid")]
    public decimal Bid { get; set; }

    [JsonPropertyName("prev_close")]
    public decimal PrevClose { get; set; }

    [JsonPropertyName("volume")]
    public long Volume { get; set; }

    [JsonPropertyName("oi")]
    public long Oi { get; set; }
}

public class DepthResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public DepthData? Data { get; set; }
}

public class DepthData
{
    [JsonPropertyName("open")]
    public decimal Open { get; set; }

    [JsonPropertyName("high")]
    public decimal High { get; set; }

    [JsonPropertyName("low")]
    public decimal Low { get; set; }

    [JsonPropertyName("ltp")]
    public decimal Ltp { get; set; }

    [JsonPropertyName("prev_close")]
    public decimal PrevClose { get; set; }

    [JsonPropertyName("volume")]
    public long Volume { get; set; }

    [JsonPropertyName("oi")]
    public long Oi { get; set; }

    [JsonPropertyName("totalbuyqty")]
    public long TotalBuyQty { get; set; }

    [JsonPropertyName("totalsellqty")]
    public long TotalSellQty { get; set; }

    [JsonPropertyName("asks")]
    public List<DepthLevel>? Asks { get; set; }

    [JsonPropertyName("bids")]
    public List<DepthLevel>? Bids { get; set; }
}

public class DepthLevel
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("orders")]
    public int Orders { get; set; }
}

public class HistoryResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public List<HistoryCandle>? Data { get; set; }
}

public class HistoryCandle
{
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("open")]
    public decimal Open { get; set; }

    [JsonPropertyName("high")]
    public decimal High { get; set; }

    [JsonPropertyName("low")]
    public decimal Low { get; set; }

    [JsonPropertyName("close")]
    public decimal Close { get; set; }

    [JsonPropertyName("volume")]
    public long Volume { get; set; }

    [JsonIgnore]
    public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).UtcDateTime;
}

public class SearchResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public List<SymbolData>? Data { get; set; }
}

public class SymbolData
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("brsymbol")]
    public string? BrSymbol { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("expiry")]
    public string? Expiry { get; set; }

    [JsonPropertyName("instrumenttype")]
    public string? InstrumentType { get; set; }

    [JsonPropertyName("lotsize")]
    public int LotSize { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("strike")]
    public decimal Strike { get; set; }

    [JsonPropertyName("tick_size")]
    public decimal TickSize { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }
}

public class FundsResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public FundsData? Data { get; set; }
}

public class FundsData
{
    [JsonPropertyName("availablecash")]
    public string? AvailableCash { get; set; }

    [JsonPropertyName("collateral")]
    public string? Collateral { get; set; }

    [JsonPropertyName("m2mrealized")]
    public string? M2MRealized { get; set; }

    [JsonPropertyName("m2munrealized")]
    public string? M2MUnrealized { get; set; }

    [JsonPropertyName("utiliseddebits")]
    public string? UtilisedDebits { get; set; }
}

public class OrderBookResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public OrderBookData? Data { get; set; }
}

public class OrderBookData
{
    [JsonPropertyName("orders")]
    public List<OrderBookEntry>? Orders { get; set; }

    [JsonPropertyName("statistics")]
    public OrderBookStatistics? Statistics { get; set; }
}

public class OrderBookEntry
{
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("orderid")]
    public string? OrderId { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("pricetype")]
    public string? PriceType { get; set; }

    [JsonPropertyName("order_status")]
    public string? OrderStatus { get; set; }

    [JsonPropertyName("trigger_price")]
    public decimal TriggerPrice { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}

public class OrderBookStatistics
{
    [JsonPropertyName("total_buy_orders")]
    public decimal TotalBuyOrders { get; set; }

    [JsonPropertyName("total_sell_orders")]
    public decimal TotalSellOrders { get; set; }

    [JsonPropertyName("total_completed_orders")]
    public decimal TotalCompletedOrders { get; set; }

    [JsonPropertyName("total_open_orders")]
    public decimal TotalOpenOrders { get; set; }

    [JsonPropertyName("total_rejected_orders")]
    public decimal TotalRejectedOrders { get; set; }
}

public class TradeBookResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public List<TradeBookEntry>? Data { get; set; }
}

public class TradeBookEntry
{
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("orderid")]
    public string? OrderId { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("average_price")]
    public decimal AveragePrice { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("trade_value")]
    public decimal TradeValue { get; set; }
}

public class PositionBookResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public List<PositionBookEntry>? Data { get; set; }
}

public class PositionBookEntry
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("average_price")]
    public string? AveragePrice { get; set; }

    [JsonPropertyName("ltp")]
    public string? Ltp { get; set; }

    [JsonPropertyName("pnl")]
    public string? Pnl { get; set; }
}

public class HoldingsResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public HoldingsData? Data { get; set; }
}

public class HoldingsData
{
    [JsonPropertyName("holdings")]
    public List<HoldingEntry>? Holdings { get; set; }

    [JsonPropertyName("statistics")]
    public HoldingsStatistics? Statistics { get; set; }
}

public class HoldingEntry
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("pnl")]
    public decimal Pnl { get; set; }

    [JsonPropertyName("pnlpercent")]
    public decimal PnlPercent { get; set; }
}

public class HoldingsStatistics
{
    [JsonPropertyName("totalholdingvalue")]
    public decimal TotalHoldingValue { get; set; }

    [JsonPropertyName("totalinvvalue")]
    public decimal TotalInvValue { get; set; }

    [JsonPropertyName("totalprofitandloss")]
    public decimal TotalProfitAndLoss { get; set; }

    [JsonPropertyName("totalpnlpercentage")]
    public decimal TotalPnlPercentage { get; set; }
}

public class OrderStatusResponse : BaseResponse
{
    [JsonPropertyName("data")]
    public OrderStatusData? Data { get; set; }
}

public class OrderStatusData
{
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("average_price")]
    public decimal AveragePrice { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("order_status")]
    public string? OrderStatus { get; set; }

    [JsonPropertyName("orderid")]
    public string? OrderId { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("pricetype")]
    public string? PriceType { get; set; }

    [JsonPropertyName("product")]
    public string? Product { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("trigger_price")]
    public decimal TriggerPrice { get; set; }
}
