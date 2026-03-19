using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MT5Clone.OpenAlgo.Models;

namespace MT5Clone.OpenAlgo.Services;

public class OpenAlgoApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly OpenAlgoConfig _config;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public OpenAlgoApiClient(OpenAlgoConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public bool IsConfigured => _config.IsValid;

    private Dictionary<string, object?> CreatePayload()
    {
        return new Dictionary<string, object?> { ["apikey"] = _config.ApiKey };
    }

    private async Task<T> PostAsync<T>(string endpoint, Dictionary<string, object?> payload, CancellationToken ct = default) where T : BaseResponse, new()
    {
        var url = _config.BaseUrl + endpoint;
        try
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return new T { Status = "error", Message = $"HTTP {(int)response.StatusCode}: {responseBody}" };
            }

            var result = JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
            return result ?? new T { Status = "error", Message = "Empty response from server" };
        }
        catch (TaskCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            return new T { Status = "error", Message = "Request timed out" };
        }
        catch (HttpRequestException ex)
        {
            return new T { Status = "error", Message = $"Connection error: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new T { Status = "error", Message = $"Unexpected error: {ex.Message}" };
        }
    }

    // ===== Order Management =====

    public async Task<OrderResponse> PlaceOrderAsync(
        string symbol, string action, string exchange,
        string priceType = "MARKET", string product = "MIS",
        int quantity = 1, double price = 0, double triggerPrice = 0,
        int disclosedQuantity = 0, CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["strategy"] = _config.Strategy;
        payload["symbol"] = symbol;
        payload["action"] = action.ToUpper();
        payload["exchange"] = exchange;
        payload["pricetype"] = priceType;
        payload["product"] = product;
        payload["quantity"] = quantity;
        if (price > 0) payload["price"] = price;
        if (triggerPrice > 0) payload["trigger_price"] = triggerPrice;
        if (disclosedQuantity > 0) payload["disclosed_quantity"] = disclosedQuantity;

        return await PostAsync<OrderResponse>("placeorder", payload, ct);
    }

    public async Task<OrderResponse> PlaceSmartOrderAsync(
        string symbol, string action, string exchange,
        int quantity, int positionSize,
        string priceType = "MARKET", string product = "MIS",
        double price = 0, double triggerPrice = 0,
        CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["strategy"] = _config.Strategy;
        payload["symbol"] = symbol;
        payload["action"] = action.ToUpper();
        payload["exchange"] = exchange;
        payload["pricetype"] = priceType;
        payload["product"] = product;
        payload["quantity"] = quantity;
        payload["position_size"] = positionSize;
        if (price > 0) payload["price"] = price;
        if (triggerPrice > 0) payload["trigger_price"] = triggerPrice;

        return await PostAsync<OrderResponse>("placesmartorder", payload, ct);
    }

    public async Task<OrderResponse> ModifyOrderAsync(
        string orderId, string symbol, string action, string exchange,
        string product, int quantity, double price,
        string priceType = "LIMIT", double triggerPrice = 0,
        int disclosedQuantity = 0, CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["strategy"] = _config.Strategy;
        payload["orderid"] = orderId;
        payload["symbol"] = symbol;
        payload["action"] = action.ToUpper();
        payload["exchange"] = exchange;
        payload["pricetype"] = priceType;
        payload["product"] = product;
        payload["quantity"] = quantity;
        payload["price"] = price;
        payload["trigger_price"] = triggerPrice;
        payload["disclosed_quantity"] = disclosedQuantity;

        return await PostAsync<OrderResponse>("modifyorder", payload, ct);
    }

    public async Task<OrderResponse> CancelOrderAsync(string orderId, CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["strategy"] = _config.Strategy;
        payload["orderid"] = orderId;

        return await PostAsync<OrderResponse>("cancelorder", payload, ct);
    }

    public async Task<CancelAllOrderResponse> CancelAllOrdersAsync(CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["strategy"] = _config.Strategy;

        return await PostAsync<CancelAllOrderResponse>("cancelallorder", payload, ct);
    }

    public async Task<ClosePositionResponse> CloseAllPositionsAsync(CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["strategy"] = _config.Strategy;

        return await PostAsync<ClosePositionResponse>("closeposition", payload, ct);
    }

    public async Task<OrderResponse> BasketOrderAsync(
        List<Dictionary<string, object?>> orders, CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["strategy"] = _config.Strategy;
        payload["orders"] = orders;

        return await PostAsync<OrderResponse>("basketorder", payload, ct);
    }

    public async Task<OrderResponse> SplitOrderAsync(
        string symbol, string action, string exchange,
        int quantity, int splitSize,
        string priceType = "MARKET", string product = "MIS",
        double price = 0, double triggerPrice = 0,
        CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["strategy"] = _config.Strategy;
        payload["symbol"] = symbol;
        payload["action"] = action.ToUpper();
        payload["exchange"] = exchange;
        payload["quantity"] = quantity;
        payload["splitsize"] = splitSize;
        payload["pricetype"] = priceType;
        payload["product"] = product;
        if (price > 0) payload["price"] = price;
        if (triggerPrice > 0) payload["trigger_price"] = triggerPrice;

        return await PostAsync<OrderResponse>("splitorder", payload, ct);
    }

    public async Task<OrderStatusResponse> GetOrderStatusAsync(string orderId, CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["strategy"] = _config.Strategy;
        payload["orderid"] = orderId;

        return await PostAsync<OrderStatusResponse>("orderstatus", payload, ct);
    }

    // ===== Market Data =====

    public async Task<QuotesResponse> GetQuotesAsync(string symbol, string exchange, CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["symbol"] = symbol;
        payload["exchange"] = exchange;

        return await PostAsync<QuotesResponse>("quotes", payload, ct);
    }

    public async Task<DepthResponse> GetDepthAsync(string symbol, string exchange, CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["symbol"] = symbol;
        payload["exchange"] = exchange;

        return await PostAsync<DepthResponse>("depth", payload, ct);
    }

    public async Task<HistoryResponse> GetHistoryAsync(
        string symbol, string exchange, string interval,
        string startDate, string endDate, CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["symbol"] = symbol;
        payload["exchange"] = exchange;
        payload["interval"] = interval;
        payload["start_date"] = startDate;
        payload["end_date"] = endDate;

        return await PostAsync<HistoryResponse>("history", payload, ct);
    }

    public async Task<SearchResponse> SearchSymbolsAsync(string query, string? exchange = null, CancellationToken ct = default)
    {
        var payload = CreatePayload();
        payload["query"] = query;
        if (!string.IsNullOrEmpty(exchange))
            payload["exchange"] = exchange;

        return await PostAsync<SearchResponse>("search", payload, ct);
    }

    // ===== Account / Portfolio =====

    public async Task<FundsResponse> GetFundsAsync(CancellationToken ct = default)
    {
        var payload = CreatePayload();
        return await PostAsync<FundsResponse>("funds", payload, ct);
    }

    public async Task<OrderBookResponse> GetOrderBookAsync(CancellationToken ct = default)
    {
        var payload = CreatePayload();
        return await PostAsync<OrderBookResponse>("orderbook", payload, ct);
    }

    public async Task<TradeBookResponse> GetTradeBookAsync(CancellationToken ct = default)
    {
        var payload = CreatePayload();
        return await PostAsync<TradeBookResponse>("tradebook", payload, ct);
    }

    public async Task<PositionBookResponse> GetPositionBookAsync(CancellationToken ct = default)
    {
        var payload = CreatePayload();
        return await PostAsync<PositionBookResponse>("positionbook", payload, ct);
    }

    public async Task<HoldingsResponse> GetHoldingsAsync(CancellationToken ct = default)
    {
        var payload = CreatePayload();
        return await PostAsync<HoldingsResponse>("holdings", payload, ct);
    }

    // ===== Connection Test =====

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await GetFundsAsync(ct);
            return result.IsSuccess;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
