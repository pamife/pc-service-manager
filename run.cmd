@echo off
set "PATH=%USERPROFILE%\.dotnet;%PATH%"
set "DOTNET_ROOT=%USERPROFILE%\.dotnet"
dotnet run --project src\PcServiceManager.UI
