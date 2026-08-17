using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.UI.ViewModels.Items;

namespace PcServiceManager.UI.ViewModels;

public partial class MaintenanceViewModel : ObservableObject
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ISystemActionService _systemActionService;
    private Guid _activePcId;
    private List<MaintenanceTaskItemViewModel> _allTaskVms = new();

    [ObservableProperty]
    private ObservableCollection<MaintenanceTaskItemViewModel> _filteredTasks = new();

    [ObservableProperty]
    private ObservableCollection<MaintenanceCategory> _categories = new();

    [ObservableProperty]
    private MaintenanceCategory? _selectedCategory;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isAddingTask;

    // New Task Form Properties
    [ObservableProperty]
    private string _newTaskTitle = string.Empty;

    [ObservableProperty]
    private string _newTaskDescription = string.Empty;

    [ObservableProperty]
    private string _newTaskDetailedInstructions = string.Empty;

    [ObservableProperty]
    private string _newTaskSafetyWarning = string.Empty;

    [ObservableProperty]
    private bool _newTaskIsAdvanced;

    [ObservableProperty]
    private MaintenanceCategory? _newTaskCategory;

    [ObservableProperty]
    private IntervalType _newTaskIntervalType = IntervalType.Months;

    [ObservableProperty]
    private int _newTaskIntervalValue = 1;

    public MaintenanceViewModel(IMaintenanceService maintenanceService, ISystemActionService systemActionService)
    {
        _maintenanceService = maintenanceService;
        _systemActionService = systemActionService;
    }

    public async Task LoadAsync(Guid pcId)
    {
        _activePcId = pcId;

        var cats = await _maintenanceService.GetCategoriesAsync();
        Categories.Clear();
        Categories.Add(new MaintenanceCategory { Id = 0, Name = "All Categories", Icon = "Grid24" });
        foreach (var c in cats) Categories.Add(c);
        SelectedCategory = Categories.FirstOrDefault();

        await ReloadTasksAsync();
    }

    private async Task ReloadTasksAsync()
    {
        var settings = await _maintenanceService.GetSettingsAsync();
        var tasks = await _maintenanceService.GetTasksForPcAsync(_activePcId);

        _allTaskVms = tasks
            .Select(t => new MaintenanceTaskItemViewModel(t, settings.DueSoonDaysThreshold, _systemActionService))
            .ToList();

        ApplyFilter();
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    partial void OnSelectedCategoryChanged(MaintenanceCategory? value) => ApplyFilter();

    private void ApplyFilter()
    {
        var query = _allTaskVms.AsEnumerable();

        if (SelectedCategory != null && SelectedCategory.Id != 0)
        {
            query = query.Where(t => t.Task.CategoryId == SelectedCategory.Id);
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.Trim().ToLowerInvariant();
            query = query.Where(t => t.Title.ToLowerInvariant().Contains(q) ||
                                     (t.Description != null && t.Description.ToLowerInvariant().Contains(q)));
        }

        FilteredTasks.Clear();
        foreach (var item in query.OrderBy(t => t.Status == MaintenanceStatus.Overdue ? 0 : t.Status == MaintenanceStatus.DueSoon ? 1 : 2)
                                  .ThenBy(t => t.NextDueDate))
        {
            FilteredTasks.Add(item);
        }
    }

    [RelayCommand]
    private void ShowAddTaskForm()
    {
        NewTaskTitle = string.Empty;
        NewTaskDescription = string.Empty;
        NewTaskDetailedInstructions = string.Empty;
        NewTaskSafetyWarning = string.Empty;
        NewTaskIsAdvanced = false;
        NewTaskCategory = Categories.FirstOrDefault(c => c.Id != 0);
        NewTaskIntervalType = IntervalType.Months;
        NewTaskIntervalValue = 1;
        IsAddingTask = true;
    }

    [RelayCommand]
    private void CancelAddTask()
    {
        IsAddingTask = false;
    }

    [RelayCommand]
    private async Task SaveNewTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle) || NewTaskCategory == null || NewTaskCategory.Id == 0)
        {
            return;
        }

        var task = new MaintenanceTask
        {
            PcAssetId = _activePcId,
            CategoryId = NewTaskCategory.Id,
            Title = NewTaskTitle.Trim(),
            Description = NewTaskDescription.Trim(),
            DetailedInstructions = NewTaskDetailedInstructions.Trim(),
            SafetyWarning = string.IsNullOrWhiteSpace(NewTaskSafetyWarning) ? null : NewTaskSafetyWarning.Trim(),
            IsAdvanced = NewTaskIsAdvanced,
            IntervalType = NewTaskIntervalType,
            IntervalValue = Math.Max(1, NewTaskIntervalValue),
            IsEnabled = true,
            QuickAction = QuickActionType.None
        };

        await _maintenanceService.AddTaskAsync(task);
        IsAddingTask = false;
        await ReloadTasksAsync();
    }

    [RelayCommand]
    private async Task ToggleTaskEnabledAsync(MaintenanceTaskItemViewModel item)
    {
        item.Task.IsEnabled = !item.Task.IsEnabled;
        await _maintenanceService.UpdateTaskAsync(item.Task);
        var settings = await _maintenanceService.GetSettingsAsync();
        item.RefreshFromModel(settings.DueSoonDaysThreshold);
        ApplyFilter();
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(MaintenanceTaskItemViewModel item)
    {
        await _maintenanceService.DeleteTaskAsync(item.Id);
        await ReloadTasksAsync();
    }
}
