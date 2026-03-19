using MT5Clone.Core.Models;

namespace MT5Clone.Core.Interfaces;

public interface IAlertService
{
    event EventHandler<AlertTriggeredEventArgs>? AlertTriggered;

    void AddAlert(Alert alert);
    void RemoveAlert(long alertId);
    void ModifyAlert(Alert alert);
    void EnableAlert(long alertId, bool enabled);
    IReadOnlyList<Alert> GetAlerts();
    void ProcessTick(Tick tick);
}

public class AlertTriggeredEventArgs : EventArgs
{
    public Alert Alert { get; }
    public Tick Tick { get; }
    public AlertTriggeredEventArgs(Alert alert, Tick tick)
    {
        Alert = alert;
        Tick = tick;
    }
}
