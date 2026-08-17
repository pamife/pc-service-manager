using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.Core.Services;

namespace PcServiceManager.UI.ViewModels.Items;

public partial class MaintenanceTaskItemViewModel : ObservableObject
{
    private readonly MaintenanceTask _task;
    private readonly ISystemActionService _systemActionService;

    public MaintenanceTask Task => _task;
    public Guid Id => _task.Id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _categoryName = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _detailedInstructions;

    [ObservableProperty]
    private string? _safetyWarning;

    [ObservableProperty]
    private bool _isAdvanced;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private IntervalType _intervalType;

    [ObservableProperty]
    private int _intervalValue;

    [ObservableProperty]
    private DateTime? _lastPerformedDate;

    [ObservableProperty]
    private DateTime? _nextDueDate;

    [ObservableProperty]
    private MaintenanceStatus _status;

    [ObservableProperty]
    private string _formattedInterval = string.Empty;

    [ObservableProperty]
    private string _formattedDaysRemaining = string.Empty;

    [ObservableProperty]
    private string _formattedLastPerformed = "Never";

    [ObservableProperty]
    private string _formattedNextDue = "Not set";

    [ObservableProperty]
    private bool _hasQuickAction;

    [ObservableProperty]
    private string _quickActionLabel = "Open Tool";

    public MaintenanceTaskItemViewModel(MaintenanceTask task, int dueSoonThreshold, ISystemActionService systemActionService)
    {
        _task = task;
        _systemActionService = systemActionService;
        RefreshFromModel(dueSoonThreshold);
    }

    public void RefreshFromModel(int dueSoonThreshold)
    {
        Title = _task.Title;
        CategoryName = _task.Category?.Name ?? "General";
        Description = _task.Description;
        DetailedInstructions = _task.DetailedInstructions;
        SafetyWarning = _task.SafetyWarning;
        IsAdvanced = _task.IsAdvanced;
        IsEnabled = _task.IsEnabled;
        IntervalType = _task.IntervalType;
        IntervalValue = _task.IntervalValue;
        LastPerformedDate = _task.LastPerformedDate;
        NextDueDate = _task.NextDueDate;

        var today = DateTime.UtcNow.Date;
        Status = MaintenanceScheduleCalculator.CalculateTaskStatus(
            NextDueDate,
            LastPerformedDate,
            IntervalType,
            IsEnabled,
            dueSoonThreshold,
            today);

        FormattedInterval = MaintenanceScheduleCalculator.FormatInterval(IntervalType, IntervalValue);

        var days = MaintenanceScheduleCalculator.GetDaysRemaining(NextDueDate, today);
        FormattedDaysRemaining = MaintenanceScheduleCalculator.FormatDaysRemaining(days);

        FormattedLastPerformed = LastPerformedDate.HasValue ? LastPerformedDate.Value.ToString("dd MMM yyyy") : "Never performed";
        FormattedNextDue = NextDueDate.HasValue ? NextDueDate.Value.ToString("dd MMM yyyy") : "No date set";

        HasQuickAction = _task.QuickAction != QuickActionType.None;
        QuickActionLabel = _task.QuickAction switch
        {
            QuickActionType.WindowsUpdate => "Open Windows Update",
            QuickActionType.WindowsSecurity => "Open Security Center",
            QuickActionType.StorageSense => "Open Storage Settings",
            QuickActionType.DeviceManager => "Open Device Manager",
            QuickActionType.TaskManager => "Open Task Manager",
            QuickActionType.ResourceMonitor => "Open Resource Monitor",
            QuickActionType.DiskManagement => "Open Disk Management",
            QuickActionType.EventViewer => "Open Event Viewer",
            QuickActionType.PowerOptions => "Battery / Power",
            QuickActionType.NetworkSettings => "Network Settings",
            QuickActionType.TempFilesClean => "Safe Temp Cleaner",
            QuickActionType.SystemIntegrityGuide => "Integrity Guide",
            _ => "Open Tool"
        };
    }

    [RelayCommand]
    private void ExecuteQuickAction()
    {
        if (HasQuickAction)
        {
            _systemActionService.ExecuteQuickAction(_task.QuickAction, _task.QuickActionPayload);
        }
    }
}
