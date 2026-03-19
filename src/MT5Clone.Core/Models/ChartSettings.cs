using MT5Clone.Core.Enums;

namespace MT5Clone.Core.Models;

public class ChartSettings
{
    public string Symbol { get; set; } = string.Empty;
    public TimeFrame TimeFrame { get; set; } = TimeFrame.H1;
    public ChartType ChartType { get; set; } = ChartType.Candlestick;
    public bool ShowGrid { get; set; } = true;
    public bool ShowVolume { get; set; } = true;
    public bool ShowAskLine { get; set; } = false;
    public bool ShowBidLine { get; set; } = true;
    public bool ShowLastLine { get; set; } = false;
    public bool ShowPeriodSeparators { get; set; } = true;
    public bool ShowTradeHistory { get; set; } = false;
    public bool ShowOrderLevels { get; set; } = true;
    public bool ShowCrosshair { get; set; } = true;
    public bool AutoScroll { get; set; } = true;
    public bool ChartShift { get; set; } = true;
    public double ChartShiftPercent { get; set; } = 0.2;
    public bool ShowOHLC { get; set; } = true;
    public bool ShowDataWindow { get; set; } = false;
    public bool ShowNavigator { get; set; } = true;
    public ChartColorScheme ColorScheme { get; set; } = ChartColorScheme.GreenOnBlack;
    public ChartColors Colors { get; set; } = new();
    public int MagnifierMode { get; set; } = 0;
}

public class ChartColors
{
    public string Background { get; set; } = "#000000";
    public string Foreground { get; set; } = "#C0C0C0";
    public string Grid { get; set; } = "#323232";
    public string BullCandle { get; set; } = "#00FF00";
    public string BearCandle { get; set; } = "#FF0000";
    public string BullOutline { get; set; } = "#00FF00";
    public string BearOutline { get; set; } = "#FF0000";
    public string LineChart { get; set; } = "#00FF00";
    public string VolumeUp { get; set; } = "#00FF00";
    public string VolumeDown { get; set; } = "#FF0000";
    public string BidLine { get; set; } = "#808080";
    public string AskLine { get; set; } = "#FF0000";
    public string StopLossLine { get; set; } = "#FF0000";
    public string TakeProfitLine { get; set; } = "#00FF00";
    public string Crosshair { get; set; } = "#808080";
    public string PeriodSeparator { get; set; } = "#404040";
    public string SelectionRect { get; set; } = "#3366FF";
}
