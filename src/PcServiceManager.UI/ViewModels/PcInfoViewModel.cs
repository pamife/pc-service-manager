using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.Core.Models;

namespace PcServiceManager.UI.ViewModels;

public partial class PcInfoViewModel : ObservableObject
{
    private readonly IHardwareDiagnosticsService _hardwareDiagnosticsService;
    private readonly ISystemActionService _systemActionService;

    [ObservableProperty]
    private PcDiagnosticInfo _diagnosticInfo = new();

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private TempFileScanResult? _tempScanResult;

    [ObservableProperty]
    private bool _isScanningTemp;

    [ObservableProperty]
    private bool _isCleaningTemp;

    [ObservableProperty]
    private string _tempCleanStatusMessage = string.Empty;

    public PcInfoViewModel(
        IHardwareDiagnosticsService hardwareDiagnosticsService,
        ISystemActionService systemActionService)
    {
        _hardwareDiagnosticsService = hardwareDiagnosticsService;
        _systemActionService = systemActionService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        DiagnosticInfo = await _hardwareDiagnosticsService.GetDiagnosticInfoAsync();
        IsLoading = false;
    }

    [RelayCommand]
    private async Task RefreshDiagnosticsAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private void LaunchTool(string toolName)
    {
        if (Enum.TryParse<QuickActionType>(toolName, true, out var actionType))
        {
            _systemActionService.ExecuteQuickAction(actionType);
        }
    }

    [RelayCommand]
    private async Task ScanTempFilesAsync()
    {
        IsScanningTemp = true;
        TempCleanStatusMessage = "Scanning temporary directories...";
        TempScanResult = await _systemActionService.ScanTemporaryFilesAsync();
        IsScanningTemp = false;
        TempCleanStatusMessage = $"Found {TempScanResult.TotalFiles} items ({TempScanResult.FormattedTotalSize}) that can be safely cleaned.";
    }

    [RelayCommand]
    private async Task CleanTempFilesAsync()
    {
        if (TempScanResult == null || TempScanResult.TotalBytes == 0) return;

        IsCleaningTemp = true;
        TempCleanStatusMessage = "Safely cleaning selected temporary files...";

        var (deleted, freedBytes) = await _systemActionService.CleanTemporaryFilesAsync(TempScanResult);

        IsCleaningTemp = false;

        var freedMb = Math.Round((double)freedBytes / (1024 * 1024), 1);
        TempCleanStatusMessage = $"Cleaned {deleted} files, freed {freedMb} MB disk space.";

        // Re-scan
        TempScanResult = await _systemActionService.ScanTemporaryFilesAsync();
    }
}
