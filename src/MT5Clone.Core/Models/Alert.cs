namespace MT5Clone.Core.Models;

public class Alert
{
    public long Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public AlertCondition Condition { get; set; }
    public double Value { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool IsTriggered { get; set; }
    public int MaxTriggers { get; set; } = 1;
    public int TriggerCount { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? TriggeredTime { get; set; }
    public DateTime? ExpirationTime { get; set; }
    public AlertAction Action { get; set; } = AlertAction.Sound;
    public string SoundFile { get; set; } = "alert.wav";
}

public enum AlertCondition
{
    BidGreaterThan,
    BidLessThan,
    AskGreaterThan,
    AskLessThan,
    LastGreaterThan,
    LastLessThan,
    TimeIs,
    VolumeGreaterThan
}

public enum AlertAction
{
    Sound,
    File,
    Email,
    Notification,
    SoundAndNotification
}
