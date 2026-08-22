@echo off
cd /d "%~dp0"
set "PATH=%USERPROFILE%\.dotnet;%PATH%"
set "DOTNET_ROOT=%USERPROFILE%\.dotnet"
dotnet run --project "src\PcServiceManager.UI\PcServiceManager.UI.csproj"
if %ERRORLEVEL% neq 0 (
    echo.
    echo Application exited with error code %ERRORLEVEL%.
    pause
)
