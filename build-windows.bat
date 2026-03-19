@echo off
echo Building MT5Clone for Windows...
echo.
dotnet publish src\MT5Clone.App\MT5Clone.App.csproj -c Release -r win-x64 --self-contained -o publish\win-x64
echo.
echo ==========================================
echo Build complete!
echo Your executable is at: publish\win-x64\MT5Clone.exe
echo ==========================================
pause
