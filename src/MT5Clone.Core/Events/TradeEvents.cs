using MT5Clone.Core.Models;

namespace MT5Clone.Core.Events;

public class TradeEventAggregator
{
    public event EventHandler<TradeLogEventArgs>? TradeLogAdded;
    public event EventHandler<JournalEventArgs>? JournalEntryAdded;
    public event EventHandler<MailEventArgs>? MailReceived;

    public void LogTrade(string message, TradeLogLevel level = TradeLogLevel.Info)
    {
        TradeLogAdded?.Invoke(this, new TradeLogEventArgs(message, level));
    }

    public void LogJournal(string message, string source = "")
    {
        JournalEntryAdded?.Invoke(this, new JournalEventArgs(message, source));
    }

    public void AddMail(MailMessage mail)
    {
        MailReceived?.Invoke(this, new MailEventArgs(mail));
    }
}

public class TradeLogEventArgs : EventArgs
{
    public DateTime Time { get; }
    public string Message { get; }
    public TradeLogLevel Level { get; }

    public TradeLogEventArgs(string message, TradeLogLevel level)
    {
        Time = DateTime.UtcNow;
        Message = message;
        Level = level;
    }
}

public enum TradeLogLevel
{
    Info,
    Warning,
    Error,
    Trade
}

public class JournalEventArgs : EventArgs
{
    public DateTime Time { get; }
    public string Message { get; }
    public string Source { get; }

    public JournalEventArgs(string message, string source)
    {
        Time = DateTime.UtcNow;
        Message = message;
        Source = source;
    }
}

public class MailMessage
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}

public class MailEventArgs : EventArgs
{
    public MailMessage Mail { get; }
    public MailEventArgs(MailMessage mail) => Mail = mail;
}
