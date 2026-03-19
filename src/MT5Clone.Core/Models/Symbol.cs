using MT5Clone.Core.Enums;

namespace MT5Clone.Core.Models;

public class Symbol
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = string.Empty;
    public string QuoteCurrency { get; set; } = string.Empty;
    public string MarginCurrency { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public SymbolType SymbolType { get; set; }
    public SymbolTradeMode TradeMode { get; set; } = SymbolTradeMode.Full;
    public SymbolCalcMode CalcMode { get; set; } = SymbolCalcMode.Forex;

    public int Digits { get; set; } = 5;
    public double Point { get; set; } = 0.00001;
    public double TickSize { get; set; } = 0.00001;
    public double TickValue { get; set; } = 1.0;
    public double ContractSize { get; set; } = 100000;
    public double MinLot { get; set; } = 0.01;
    public double MaxLot { get; set; } = 100.0;
    public double LotStep { get; set; } = 0.01;
    public double SwapLong { get; set; }
    public double SwapShort { get; set; }
    public int SwapMode { get; set; }
    public int SwapRollover3Days { get; set; } = 3; // Wednesday

    public double MarginInitial { get; set; }
    public double MarginMaintenance { get; set; }
    public double MarginHedged { get; set; }
    public double MarginRate { get; set; } = 1.0;

    public double Bid { get; set; }
    public double Ask { get; set; }
    public double Last { get; set; }
    public double DayHigh { get; set; }
    public double DayLow { get; set; }
    public double DayOpen { get; set; }
    public double PreviousClose { get; set; }
    public long Volume { get; set; }

    public double Spread => (Ask - Bid) / Point;
    public double SpreadValue => Ask - Bid;

    public int StopsLevel { get; set; } = 0;
    public int FreezeLevel { get; set; } = 0;

    public DateTime TradeSessionStart { get; set; }
    public DateTime TradeSessionEnd { get; set; }

    public bool IsSelected { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsFavorite { get; set; }

    public string FormatPrice(double price)
    {
        return price.ToString($"F{Digits}");
    }
}
