namespace MT5Clone.OpenAlgo.Models;

public class OpenAlgoConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Host { get; set; } = "http://127.0.0.1:5000";
    public string ApiVersion { get; set; } = "v1";
    public string Strategy { get; set; } = "MT5Clone";
    public double TimeoutSeconds { get; set; } = 120.0;
    public int WebSocketPort { get; set; } = 8765;

    public string BaseUrl => $"{Host.TrimEnd('/')}/api/{ApiVersion}/";

    public bool IsValid => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(Host);
}
