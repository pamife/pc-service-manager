using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.Core.Services;

namespace PcServiceManager.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IMaintenanceService _maintenanceService;
    private Guid _activePcId;

    public Action? RequestNavigateToServiceMode { get; set; }
    public Action? RequestNavigateToMaintenance { get; set; }
    public Action? RequestNavigateToHistory { get; set; }
    public Action? RequestNavigateToPcInfo { get; set; }
    public Action? RequestNavigateToSettings { get; set; }

    [ObservableProperty]
    private string _pcName = "Loading...";

    [ObservableProperty]
    private string _deviceSummary = string.Empty;

    [ObservableProperty]
    private OverallHealthStatus _overallStatus = OverallHealthStatus.Good;

    [ObservableProperty]
    private string _overallStatusText = "GOOD";

    [ObservableProperty]
    private int _overdueCount;

    [ObservableProperty]
    private int _dueSoonCount;

    [ObservableProperty]
    private int _healthyCount;

    [ObservableProperty]
    private string _nextMaintenanceTitle = "None Scheduled";

    [ObservableProperty]
    private string _nextMaintenanceSubtitle = "No upcoming tasks";

    [ObservableProperty]
    private string _lastServiceDate = "No service records yet";

    [ObservableProperty]
    private string _lastServiceDetails = "Run your first service session";

    [ObservableProperty]
    private ObservableCollection<ServiceSession> _recentSessions = new();

    public DashboardViewModel(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    public async Task LoadAsync(Guid pcId)
    {
        _activePcId = pcId;
        var pc = await _maintenanceService.GetActivePcAsync();
        if (pc == null) return;

        PcName = pc.Name;
        DeviceSummary = $"{pc.Manufacturer} {pc.Model} • {pc.OperatingSystem}".Trim(' ', '•');

        var settings = await _maintenanceService.GetSettingsAsync();
        var tasks = await _maintenanceService.GetTasksForPcAsync(pcId);
        var today = DateTime.UtcNow.Date;

        var taskStatuses = new List<MaintenanceStatus>();
        int overdue = 0;
        int dueSoon = 0;
        int healthy = 0;

        MaintenanceTask? nextTask = null;
        int? smallestDays = null;

        foreach (var task in tasks.Where(t => t.IsEnabled && t.IntervalType != IntervalType.Disabled))
        {
            var status = MaintenanceScheduleCalculator.CalculateTaskStatus(
                task.NextDueDate,
                task.LastPerformedDate,
                task.IntervalType,
                task.IsEnabled,
                settings.DueSoonDaysThreshold,
                today);

            taskStatuses.Add(status);

            if (status == MaintenanceStatus.Overdue) overdue++;
            else if (status == MaintenanceStatus.DueSoon) dueSoon++;
            else if (status == MaintenanceStatus.Good) healthy++;

            if (task.NextDueDate.HasValue)
            {
                var days = MaintenanceScheduleCalculator.GetDaysRemaining(task.NextDueDate, today);
                if (days.HasValue)
                {
                    if (!smallestDays.HasValue || days.Value < smallestDays.Value)
                    {
                        smallestDays = days;
                        nextTask = task;
                    }
                }
            }
        }

        OverdueCount = overdue;
        DueSoonCount = dueSoon;
        HealthyCount = healthy;

        OverallStatus = MaintenanceScheduleCalculator.CalculateOverallHealth(taskStatuses);
        OverallStatusText = OverallStatus switch
        {
            OverallHealthStatus.Good => "GOOD",
            OverallHealthStatus.DueSoon => "DUE SOON",
            OverallHealthStatus.Overdue => "OVERDUE",
            _ => "UNKNOWN"
        };

        if (nextTask != null)
        {
            NextMaintenanceTitle = nextTask.Title;
            NextMaintenanceSubtitle = MaintenanceScheduleCalculator.FormatDaysRemaining(smallestDays);
        }
        else
        {
            NextMaintenanceTitle = "All Up to Date";
            NextMaintenanceSubtitle = "No upcoming maintenance tasks";
        }

        // Recent history
        var history = await _maintenanceService.GetServiceHistoryAsync(pcId);
        RecentSessions.Clear();
        foreach (var s in history.Take(5))
        {
            RecentSessions.Add(s);
        }

        var lastSession = history.FirstOrDefault();
        if (lastSession != null)
        {
            LastServiceDate = lastSession.StartTime.ToString("dd MMMM yyyy");
            LastServiceDetails = $"{lastSession.TemplateName} • {lastSession.DurationMinutes} min • {lastSession.CompletedCount} tasks completed";
        }
        else
        {
            LastServiceDate = "No service records yet";
            LastServiceDetails = "Start a guided service session anytime";
        }
    }

    [RelayCommand]
    private void StartService() => RequestNavigateToServiceMode?.Invoke();

    [RelayCommand]
    private void NavigateMaintenance() => RequestNavigateToMaintenance?.Invoke();

    [RelayCommand]
    private void NavigateHistory() => RequestNavigateToHistory?.Invoke();

    [RelayCommand]
    private void NavigatePcInfo() => RequestNavigateToPcInfo?.Invoke();

    [RelayCommand]
    private void NavigateSettings() => RequestNavigateToSettings?.Invoke();
}
