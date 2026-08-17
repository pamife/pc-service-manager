using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.UI.ViewModels.Items;

namespace PcServiceManager.UI.ViewModels;

public enum ServiceSessionStep
{
    TemplateSelection,
    ActiveChecklist,
    SummaryCompletion
}

public partial class ServiceModeViewModel : ObservableObject
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ISystemActionService _systemActionService;
    private readonly INotificationService _notificationService;
    private readonly DispatcherTimer _sessionTimer;
    private Guid _activePcId;
    private DateTime _sessionStartTime;
    private ServiceSession? _currentSession;

    public Action? RequestNavigateToDashboard { get; set; }
    public Action? RequestNavigateToHistory { get; set; }

    [ObservableProperty]
    private ServiceSessionStep _currentStep = ServiceSessionStep.TemplateSelection;

    [ObservableProperty]
    private string _pcName = string.Empty;

    [ObservableProperty]
    private string _technicianName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MaintenanceTemplate> _availableTemplates = new();

    [ObservableProperty]
    private MaintenanceTemplate? _selectedTemplate;

    [ObservableProperty]
    private ObservableCollection<ServiceTaskChecklistItemViewModel> _checklistItems = new();

    [ObservableProperty]
    private string _formattedElapsedTime = "00:00";

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private int _skippedCount;

    [ObservableProperty]
    private int _needsAttentionCount;

    [ObservableProperty]
    private int _notApplicableCount;

    [ObservableProperty]
    private int _totalTasksCount;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _overallServiceNote = string.Empty;

    [ObservableProperty]
    private int _finalDurationMinutes;

    public ServiceModeViewModel(
        IMaintenanceService maintenanceService,
        ISystemActionService systemActionService,
        INotificationService notificationService)
    {
        _maintenanceService = maintenanceService;
        _systemActionService = systemActionService;
        _notificationService = notificationService;

        _sessionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _sessionTimer.Tick += OnSessionTimerTick;
    }

    public async Task LoadAsync(Guid pcId)
    {
        _activePcId = pcId;
        var pc = await _maintenanceService.GetActivePcAsync();
        PcName = pc?.Name ?? "PC";

        var settings = await _maintenanceService.GetSettingsAsync();
        if (string.IsNullOrWhiteSpace(TechnicianName))
        {
            TechnicianName = settings.DefaultTechnicianName ?? Environment.UserName;
        }

        var templates = await _maintenanceService.GetTemplatesAsync();
        AvailableTemplates.Clear();
        foreach (var t in templates) AvailableTemplates.Add(t);
        SelectedTemplate = AvailableTemplates.FirstOrDefault();

        // If not in active session, reset to step 1
        if (CurrentStep != ServiceSessionStep.ActiveChecklist)
        {
            CurrentStep = ServiceSessionStep.TemplateSelection;
        }
    }

    private void OnSessionTimerTick(object? sender, EventArgs e)
    {
        var elapsed = DateTime.UtcNow - _sessionStartTime;
        FormattedElapsedTime = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
    }

    [RelayCommand]
    private async Task StartServiceSessionAsync()
    {
        var pc = await _maintenanceService.GetActivePcAsync();
        if (pc == null) return;

        var templateName = SelectedTemplate?.Name ?? "Full Service";
        _currentSession = await _maintenanceService.StartServiceSessionAsync(_activePcId, templateName, TechnicianName);

        ChecklistItems.Clear();

        if (SelectedTemplate != null && SelectedTemplate.Items.Any())
        {
            // Load items from template matched with actual PC tasks where available
            var pcTasks = await _maintenanceService.GetTasksForPcAsync(_activePcId);

            foreach (var templateItem in SelectedTemplate.Items.OrderBy(i => i.SortOrder))
            {
                var matchedTask = pcTasks.FirstOrDefault(t => t.Title.Equals(templateItem.TaskTitle, StringComparison.OrdinalIgnoreCase));
                if (matchedTask != null)
                {
                    ChecklistItems.Add(ServiceTaskChecklistItemViewModel.FromTask(matchedTask, _systemActionService));
                }
                else
                {
                    ChecklistItems.Add(ServiceTaskChecklistItemViewModel.FromTemplateItem(templateItem, _systemActionService));
                }
            }
        }
        else
        {
            // Load all enabled tasks for PC
            var pcTasks = await _maintenanceService.GetTasksForPcAsync(_activePcId);
            foreach (var task in pcTasks.Where(t => t.IsEnabled))
            {
                ChecklistItems.Add(ServiceTaskChecklistItemViewModel.FromTask(task, _systemActionService));
            }
        }

        TotalTasksCount = ChecklistItems.Count;
        _sessionStartTime = DateTime.UtcNow;
        _sessionTimer.Start();
        CurrentStep = ServiceSessionStep.ActiveChecklist;
        UpdateProgress();
    }

    [RelayCommand]
    private void MarkAllCompleted()
    {
        foreach (var item in ChecklistItems)
        {
            item.Status = ServiceTaskStatus.Completed;
        }
        UpdateProgress();
    }

    [RelayCommand]
    private void UpdateProgress()
    {
        CompletedCount = ChecklistItems.Count(i => i.Status == ServiceTaskStatus.Completed);
        SkippedCount = ChecklistItems.Count(i => i.Status == ServiceTaskStatus.Skipped);
        NeedsAttentionCount = ChecklistItems.Count(i => i.Status == ServiceTaskStatus.NeedsAttention);
        NotApplicableCount = ChecklistItems.Count(i => i.Status == ServiceTaskStatus.NotApplicable);

        var total = ChecklistItems.Count;
        ProgressPercent = total > 0 ? (double)(CompletedCount + SkippedCount + NeedsAttentionCount + NotApplicableCount) / total * 100.0 : 0;
    }

    [RelayCommand]
    private void ReviewAndComplete()
    {
        _sessionTimer.Stop();
        var elapsed = DateTime.UtcNow - _sessionStartTime;
        FinalDurationMinutes = Math.Max(1, (int)elapsed.TotalMinutes);

        UpdateProgress();

        // Default suggestion for overall note
        if (string.IsNullOrWhiteSpace(OverallServiceNote))
        {
            OverallServiceNote = $"Completed maintenance service for {PcName}. {CompletedCount} tasks verified and completed.";
        }

        CurrentStep = ServiceSessionStep.SummaryCompletion;
    }

    [RelayCommand]
    private async Task FinalizeServiceAsync()
    {
        if (_currentSession == null) return;

        var results = ChecklistItems.Select(item => new ServiceTaskResult
        {
            MaintenanceTaskId = item.MaintenanceTaskId,
            TaskTitle = item.Title,
            CategoryName = item.CategoryName,
            Status = item.Status,
            Notes = item.Notes
        }).ToList();

        await _maintenanceService.CompleteServiceSessionAsync(_currentSession.Id, OverallServiceNote, results);

        _notificationService.ShowServiceCompletedNotification(PcName, _currentSession.TemplateName, CompletedCount);

        // Reset state
        CurrentStep = ServiceSessionStep.TemplateSelection;
        OverallServiceNote = string.Empty;
        ChecklistItems.Clear();

        RequestNavigateToDashboard?.Invoke();
    }

    [RelayCommand]
    private async Task CancelSessionAsync()
    {
        _sessionTimer.Stop();
        if (_currentSession != null)
        {
            await _maintenanceService.CancelServiceSessionAsync(_currentSession.Id);
        }

        CurrentStep = ServiceSessionStep.TemplateSelection;
        ChecklistItems.Clear();
    }
}
