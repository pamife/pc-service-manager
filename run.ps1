Set-Location $PSScriptRoot
$env:PATH = "$HOME\.dotnet;$env:PATH"
$env:DOTNET_ROOT = "$HOME\.dotnet"
dotnet run --project "src/PcServiceManager.UI/PcServiceManager.UI.csproj"
