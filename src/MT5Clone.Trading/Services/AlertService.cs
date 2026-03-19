using MT5Clone.Core.Interfaces;
using MT5Clone.Core.Models;

namespace MT5Clone.Trading.Services;

public class AlertService : IAlertService
{
    private readonly List<Alert> _alerts = new();
    private long _nextId = 1;

    public event EventHandler<AlertTriggeredEventArgs>? AlertTriggered;

    public void AddAlert(Alert alert)
    {
        alert.Id = _nextId++;
        alert.CreatedTime = DateTime.UtcNow;
        _alerts.Add(alert);
    }

    public void RemoveAlert(long alertId)
    {
        _alerts.RemoveAll(a => a.Id == alertId);
    }

    public void ModifyAlert(Alert alert)
    {
        var existing = _alerts.FirstOrDefault(a => a.Id == alert.Id);
        if (existing != null)
        {
            int index = _alerts.IndexOf(existing);
            _alerts[index] = alert;
        }
    }

    public void EnableAlert(long alertId, bool enabled)
    {
        var alert = _alerts.FirstOrDefault(a => a.Id == alertId);
        if (alert != null)
        {
            alert.IsEnabled = enabled;
        }
    }

    public IReadOnlyList<Alert> GetAlerts() => _alerts.AsReadOnly();

    public void ProcessTick(Tick tick)
    {
        foreach (var alert in _alerts.Where(a => a.IsEnabled && !a.IsTriggered && a.Symbol == tick.Symbol))
        {
            if (alert.ExpirationTime.HasValue && DateTime.UtcNow > alert.ExpirationTime.Value)
            {
                alert.IsEnabled = false;
                continue;
            }

            bool triggered = alert.Condition switch
            {
                AlertCondition.BidGreaterThan => tick.Bid > alert.Value,
                AlertCondition.BidLessThan => tick.Bid < alert.Value,
                AlertCondition.AskGreaterThan => tick.Ask > alert.Value,
                AlertCondition.AskLessThan => tick.Ask < alert.Value,
                AlertCondition.LastGreaterThan => tick.Last > alert.Value,
                AlertCondition.LastLessThan => tick.Last < alert.Value,
                AlertCondition.VolumeGreaterThan => tick.Volume > alert.Value,
                _ => false
            };

            if (triggered)
            {
                alert.TriggerCount++;
                alert.TriggeredTime = DateTime.UtcNow;

                if (alert.TriggerCount >= alert.MaxTriggers)
                {
                    alert.IsTriggered = true;
                }

                AlertTriggered?.Invoke(this, new AlertTriggeredEventArgs(alert, tick));
            }
        }
    }
}
