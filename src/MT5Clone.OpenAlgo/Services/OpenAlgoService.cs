using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;
using MT5Clone.OpenAlgo.Models;

namespace MT5Clone.OpenAlgo.Services;

/// <summary>
/// Main facade for OpenAlgo integration. Manages connection state and provides
/// unified access to market data and trading functionality.
/// </summary>
public class OpenAlgoService : IDisposable
{
    private OpenAlgoConfig _config;
    private OpenAlgoApiClient? _client;
    private OpenAlgoMarketDataProvider? _marketDataProvider;
    private OpenAlgoTradingEngine? _tradingEngine;
    private CancellationTokenSource? _refreshCts;
    private bool _isConnected;

    public event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;
    public event EventHandler<string>? LogMessage;

    public bool IsConnected => _isConnected;
    public OpenAlgoConfig Config => _config;
    public OpenAlgoApiClient? Client => _client;
    public OpenAlgoMarketDataProvider? MarketData => _marketDataProvider;
    public OpenAlgoTradingEngine? Trading => _tradingEngine;
    public IMarketDataProvider? MarketDataProvider => _marketDataProvider;
    public ITradingEngine? TradingEngine => _tradingEngine;

    public OpenAlgoService()
    {
        _config = new OpenAlgoConfig();
    }

    public OpenAlgoService(OpenAlgoConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public void UpdateConfig(string apiKey, string host, string strategy = "MT5Clone")
    {
        _config = new OpenAlgoConfig
        {
            ApiKey = apiKey,
            Host = host,
            Strategy = strategy
        };
    }

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        if (!_config.IsValid)
        {
            OnLog("Invalid configuration. Please set API Key and Host.");
            OnConnectionStatusChanged(false, "Invalid configuration");
            return false;
        }

        try
        {
            OnLog($"Connecting to OpenAlgo at {_config.Host}...");

            // Dispose existing connections
            await DisconnectAsync();

            _client = new OpenAlgoApiClient(_config);
            _marketDataProvider = new OpenAlgoMarketDataProvider(_client, _config);
            _tradingEngine = new OpenAlgoTradingEngine(_client, _config, _marketDataProvider);

            // Test connection
            var connected = await _client.TestConnectionAsync(ct);
            if (!connected)
            {
                OnLog("Connection test failed. Check API key and server URL.");
                OnConnectionStatusChanged(false, "Connection test failed");
                return false;
            }

            // Start market data polling
            await _marketDataProvider.StartAsync(ct);

            // Refresh account data
            await _tradingEngine.RefreshAccountDataAsync(ct);
            await _tradingEngine.RefreshOrderBookAsync(ct);

            // Start periodic refresh
            _refreshCts = new CancellationTokenSource();
            _ = Task.Run(() => PeriodicRefreshAsync(_refreshCts.Token));

            _isConnected = true;
            OnLog("Connected to OpenAlgo successfully.");
            OnConnectionStatusChanged(true, "Connected");
            return true;
        }
        catch (Exception ex)
        {
            OnLog($"Connection failed: {ex.Message}");
            OnConnectionStatusChanged(false, $"Error: {ex.Message}");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();
        _refreshCts = null;

        if (_marketDataProvider != null)
        {
            await _marketDataProvider.StopAsync();
            _marketDataProvider.Dispose();
            _marketDataProvider = null;
        }

        _client?.Dispose();
        _client = null;
        _tradingEngine = null;
        _isConnected = false;

        OnConnectionStatusChanged(false, "Disconnected");
        OnLog("Disconnected from OpenAlgo.");
    }

    private async Task PeriodicRefreshAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, ct); // Refresh every 5 seconds

                if (_tradingEngine != null)
                {
                    await _tradingEngine.RefreshAccountDataAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                OnLog($"Refresh error: {ex.Message}");
            }
        }
    }

    private void OnConnectionStatusChanged(bool connected, string message)
    {
        ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs(connected, message));
    }

    private void OnLog(string message)
    {
        LogMessage?.Invoke(this, message);
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }
}

public class ConnectionStatusEventArgs : EventArgs
{
    public bool IsConnected { get; }
    public string Message { get; }

    public ConnectionStatusEventArgs(bool isConnected, string message)
    {
        IsConnected = isConnected;
        Message = message;
    }
}
