using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Interfaces;

namespace PcServiceManager.UI.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentViewTitle = "Dashboard";

    [ObservableProperty]
    private PcAsset? _activePc;

    [ObservableProperty]
    private List<PcAsset> _allPcs = new();

    [ObservableProperty]
    private bool _isFirstLaunch;

    public DashboardViewModel DashboardVm { get; }
    public MaintenanceViewModel MaintenanceVm { get; }
    public ServiceModeViewModel ServiceModeVm { get; }
    public HistoryViewModel HistoryVm { get; }
    public PcInfoViewModel PcInfoVm { get; }
    public SettingsViewModel SettingsVm { get; }
    public FirstLaunchWizardViewModel WizardVm { get; }

    public MainWindowViewModel(
        IMaintenanceService maintenanceService,
        INotificationService notificationService,
        DashboardViewModel dashboardVm,
        MaintenanceViewModel maintenanceVm,
        ServiceModeViewModel serviceModeVm,
        HistoryViewModel historyVm,
        PcInfoViewModel pcInfoVm,
        SettingsViewModel settingsVm,
        FirstLaunchWizardViewModel wizardVm)
    {
        _maintenanceService = maintenanceService;
        _notificationService = notificationService;
        DashboardVm = dashboardVm;
        MaintenanceVm = maintenanceVm;
        ServiceModeVm = serviceModeVm;
        HistoryVm = historyVm;
        PcInfoVm = pcInfoVm;
        SettingsVm = settingsVm;
        WizardVm = wizardVm;

        // Wire navigation callbacks
        DashboardVm.RequestNavigateToServiceMode = () => NavigateTo("ServiceMode");
        DashboardVm.RequestNavigateToMaintenance = () => NavigateTo("Maintenance");
        DashboardVm.RequestNavigateToHistory = () => NavigateTo("History");
        DashboardVm.RequestNavigateToPcInfo = () => NavigateTo("PcInfo");
        DashboardVm.RequestNavigateToSettings = () => NavigateTo("Settings");

        ServiceModeVm.RequestNavigateToHistory = () => NavigateTo("History");
        ServiceModeVm.RequestNavigateToDashboard = () => NavigateTo("Dashboard");

        WizardVm.OnWizardCompleted = async () =>
        {
            IsFirstLaunch = false;
            await RefreshActivePcAsync();
            NavigateTo("Dashboard");
        };

        SettingsVm.OnDataChanged = async () =>
        {
            await RefreshActivePcAsync();
        };
    }

    public async Task InitializeAsync()
    {
        await _maintenanceService.InitializeDatabaseAsync();

        var pcs = await _maintenanceService.GetAllPcsAsync();
        if (!pcs.Any())
        {
            IsFirstLaunch = true;
            CurrentView = WizardVm;
            CurrentViewTitle = "First Launch Setup";
            return;
        }

        await RefreshActivePcAsync();
        NavigateTo("Dashboard");

        // Check startup notification
        var settings = await _maintenanceService.GetSettingsAsync();
        if (settings.NotificationsEnabled && ActivePc != null)
        {
            var tasks = await _maintenanceService.GetTasksForPcAsync(ActivePc.Id);
            var today = DateTime.UtcNow.Date;
            var overdue = tasks.Count(t => t.IsEnabled && t.NextDueDate.HasValue && t.NextDueDate.Value.Date < today);
            var dueSoon = tasks.Count(t => t.IsEnabled && t.NextDueDate.HasValue && t.NextDueDate.Value.Date >= today && t.NextDueDate.Value.Date <= today.AddDays(settings.DueSoonDaysThreshold));

            if (overdue > 0 || dueSoon > 0)
            {
                _notificationService.ShowOverdueNotification(ActivePc.Name, overdue, dueSoon);
            }
        }
    }

    public async Task RefreshActivePcAsync()
    {
        ActivePc = await _maintenanceService.GetActivePcAsync();
        AllPcs = await _maintenanceService.GetAllPcsAsync();

        if (ActivePc != null)
        {
            await DashboardVm.LoadAsync(ActivePc.Id);
            await MaintenanceVm.LoadAsync(ActivePc.Id);
            await ServiceModeVm.LoadAsync(ActivePc.Id);
            await HistoryVm.LoadAsync(ActivePc.Id);
            await PcInfoVm.LoadAsync();
            await SettingsVm.LoadAsync();
        }
    }

    [RelayCommand]
    public void NavigateTo(string viewName)
    {
        CurrentView = viewName switch
        {
            "Dashboard" => DashboardVm,
            "Maintenance" => MaintenanceVm,
            "ServiceMode" => ServiceModeVm,
            "History" => HistoryVm,
            "PcInfo" => PcInfoVm,
            "Settings" => SettingsVm,
            _ => DashboardVm
        };

        CurrentViewTitle = viewName switch
        {
            "Dashboard" => "Dashboard",
            "Maintenance" => "Maintenance Tasks",
            "ServiceMode" => "Service Mode",
            "History" => "Service History",
            "PcInfo" => "PC Information & Tools",
            "Settings" => "Settings & Backup",
            _ => "PC Service Manager"
        };

        // Reload data if needed
        if (ActivePc != null)
        {
            switch (viewName)
            {
                case "Dashboard":
                    _ = DashboardVm.LoadAsync(ActivePc.Id);
                    break;
                case "Maintenance":
                    _ = MaintenanceVm.LoadAsync(ActivePc.Id);
                    break;
                case "ServiceMode":
                    _ = ServiceModeVm.LoadAsync(ActivePc.Id);
                    break;
                case "History":
                    _ = HistoryVm.LoadAsync(ActivePc.Id);
                    break;
                case "PcInfo":
                    _ = PcInfoVm.LoadAsync();
                    break;
                case "Settings":
                    _ = SettingsVm.LoadAsync();
                    break;
            }
        }
    }
}
