using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;

namespace PcServiceManager.UI.ViewModels.Items;

public partial class ServiceTaskChecklistItemViewModel : ObservableObject
{
    private readonly ISystemActionService _systemActionService;

    public Guid? MaintenanceTaskId { get; set; }

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
    private QuickActionType _quickAction;

    [ObservableProperty]
    private string? _quickActionPayload;

    [ObservableProperty]
    private bool _hasQuickAction;

    [ObservableProperty]
    private string _quickActionLabel = "Open Tool";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCompleted))]
    [NotifyPropertyChangedFor(nameof(IsSkipped))]
    [NotifyPropertyChangedFor(nameof(IsNeedsAttention))]
    [NotifyPropertyChangedFor(nameof(IsNotApplicable))]
    private ServiceTaskStatus _status = ServiceTaskStatus.Completed;

    [ObservableProperty]
    private string? _notes;

    public bool IsCompleted => Status == ServiceTaskStatus.Completed;
    public bool IsSkipped => Status == ServiceTaskStatus.Skipped;
    public bool IsNeedsAttention => Status == ServiceTaskStatus.NeedsAttention;
    public bool IsNotApplicable => Status == ServiceTaskStatus.NotApplicable;

    public ServiceTaskChecklistItemViewModel(ISystemActionService systemActionService)
    {
        _systemActionService = systemActionService;
    }

    public static ServiceTaskChecklistItemViewModel FromTask(MaintenanceTask task, ISystemActionService actionService)
    {
        var vm = new ServiceTaskChecklistItemViewModel(actionService)
        {
            MaintenanceTaskId = task.Id,
            Title = task.Title,
            CategoryName = task.Category?.Name ?? "General",
            Description = task.Description,
            DetailedInstructions = task.DetailedInstructions,
            SafetyWarning = task.SafetyWarning,
            IsAdvanced = task.IsAdvanced,
            QuickAction = task.QuickAction,
            QuickActionPayload = task.QuickActionPayload,
            Status = ServiceTaskStatus.Completed
        };

        vm.HasQuickAction = task.QuickAction != QuickActionType.None;
        vm.QuickActionLabel = task.QuickAction switch
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

        return vm;
    }

    public static ServiceTaskChecklistItemViewModel FromTemplateItem(MaintenanceTemplateItem item, ISystemActionService actionService)
    {
        var vm = new ServiceTaskChecklistItemViewModel(actionService)
        {
            MaintenanceTaskId = null,
            Title = item.TaskTitle,
            CategoryName = item.CategoryName,
            Description = item.Description,
            DetailedInstructions = item.DetailedInstructions,
            SafetyWarning = item.SafetyWarning,
            IsAdvanced = item.IsAdvanced,
            QuickAction = item.QuickAction,
            Status = ServiceTaskStatus.Completed
        };

        vm.HasQuickAction = item.QuickAction != QuickActionType.None;
        vm.QuickActionLabel = item.QuickAction switch
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

        return vm;
    }

    [RelayCommand]
    private void SetStatus(string statusName)
    {
        if (Enum.TryParse<ServiceTaskStatus>(statusName, true, out var parsed))
        {
            Status = parsed;
        }
    }

    [RelayCommand]
    private void ExecuteQuickAction()
    {
        if (HasQuickAction)
        {
            _systemActionService.ExecuteQuickAction(QuickAction, QuickActionPayload);
        }
    }
}
