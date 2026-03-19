# MT5Clone - MetaTrader 5 Trading Terminal Clone

Complete MT5 Trading Terminal Clone built with C# .NET 8.0 and Avalonia UI. Runs on **Windows, Linux, and macOS**.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (download and install)

## How to Build & Run

### Windows (create EXE)

**Option 1 - Double-click the batch file:**
1. Double-click `build-windows.bat`
2. Your EXE will be at `publish\win-x64\MT5Clone.exe`

**Option 2 - Command line:**
```cmd
dotnet publish src\MT5Clone.App\MT5Clone.App.csproj -c Release -r win-x64 --self-contained -o publish\win-x64
```
Then run `publish\win-x64\MT5Clone.exe`

**Option 3 - Visual Studio:**
1. Open `MT5Clone.sln` in Visual Studio
2. Set **MT5Clone.App** as the startup project
3. Press F5 to run

### Linux
```bash
dotnet publish src/MT5Clone.App/MT5Clone.App.csproj -c Release -r linux-x64 --self-contained -o publish/linux-x64
./publish/linux-x64/MT5Clone
```

### macOS
```bash
dotnet publish src/MT5Clone.App/MT5Clone.App.csproj -c Release -r osx-x64 --self-contained -o publish/osx-x64
./publish/osx-x64/MT5Clone
```

## Architecture (7 Projects)

| Project | Description |
|---------|-------------|
| MT5Clone.Core | Models, enums, interfaces, events |
| MT5Clone.MarketData | Simulated price feeds, candle aggregation, tick data |
| MT5Clone.Trading | Order management, position tracking, margin calculations |
| MT5Clone.Indicators | 15+ technical indicators (MA, RSI, MACD, BB, Stochastic, CCI, ADX, Ichimoku, SAR, etc.) |
| MT5Clone.Charting | Candlestick/Bar/Line/Area renderers, 10 drawing tools, crosshair |
| MT5Clone.Strategy | Backtesting engine with comprehensive statistics |
| MT5Clone.App | Full Avalonia UI application with dark theme |

## UI Features

- Market Watch panel with real-time simulated data
- Chart view with OHLC display and indicator overlays
- Terminal with Trade/Exposure/History/Alerts/Journal/Mailbox/Market tabs
- Navigator tree view (Accounts, Indicators, Expert Advisors, Scripts)
- Order dialog with Buy/Sell/Close
- Strategy Tester with Results/Graph/Report/Journal
- Economic Calendar with impact filtering
- Complete menu bar and toolbar with timeframe selection (M1 to MN)
- Professional dark theme
