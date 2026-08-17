using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;

namespace PcServiceManager.UI.ViewModels;

public partial class FirstLaunchWizardViewModel : ObservableObject
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IHardwareDiagnosticsService _hardwareDiagnosticsService;

    public Action? OnWizardCompleted { get; set; }

    [ObservableProperty]
    private string _pcName = "Brother-PC";

    [ObservableProperty]
    private DeviceType _deviceType = DeviceType.Desktop;

    [ObservableProperty]
    private string _technicianName = Environment.UserName;

    [ObservableProperty]
    private string _notes = "Maintained with PC Service Manager";

    [ObservableProperty]
    private bool _isDetectingHardware = true;

    [ObservableProperty]
    private string _detectedHardwareSummary = "Detecting system hardware...";

    [ObservableProperty]
    private bool _isBusy;

    public FirstLaunchWizardViewModel(
        IMaintenanceService maintenanceService,
        IHardwareDiagnosticsService hardwareDiagnosticsService)
    {
        _maintenanceService = maintenanceService;
        _hardwareDiagnosticsService = hardwareDiagnosticsService;

        _ = DetectHardwareAsync();
    }

    private async Task DetectHardwareAsync()
    {
        try
        {
            var diag = await _hardwareDiagnosticsService.GetDiagnosticInfoAsync();
            if (!string.IsNullOrWhiteSpace(diag.MachineName) && diag.MachineName != "Not available")
            {
                PcName = diag.MachineName;
            }

            DetectedHardwareSummary = $"{diag.Manufacturer} {diag.Model} • {diag.CpuName} • {diag.TotalRam} RAM • {diag.OsVersion}";
        }
        catch
        {
            DetectedHardwareSummary = "Hardware detection completed.";
        }
        finally
        {
            IsDetectingHardware = false;
        }
    }

    [RelayCommand]
    private async Task CompleteSetupAsync()
    {
        if (string.IsNullOrWhiteSpace(PcName))
        {
            PcName = "My-PC";
        }

        IsBusy = true;

        await _maintenanceService.CreatePcAsync(
            PcName.Trim(),
            DeviceType,
            Notes,
            TechnicianName);

        IsBusy = false;
        OnWizardCompleted?.Invoke();
    }
}
