using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.Infrastructure.Data;
using PcServiceManager.Infrastructure.Services;
using PcServiceManager.UI.ViewModels;
using PcServiceManager.UI.Views;
using Wpf.Ui.Appearance;

namespace PcServiceManager.UI;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PcServiceManager");
        Directory.CreateDirectory(appDataFolder);

        var dbPath = Path.Combine(appDataFolder, "pc_service_manager.db");

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // EF Core SQLite DbContext
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite($"Data Source={dbPath}");
                });

                // Domain & Infrastructure Services
                services.AddSingleton<IHardwareDiagnosticsService, HardwareDiagnosticsService>();
                services.AddSingleton<ISystemActionService, SystemActionService>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddScoped<IBackupExportService, BackupExportService>();
                services.AddScoped<IMaintenanceService, MaintenanceService>();

                // ViewModels
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<MaintenanceViewModel>();
                services.AddSingleton<ServiceModeViewModel>();
                services.AddSingleton<HistoryViewModel>();
                services.AddSingleton<PcInfoViewModel>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<FirstLaunchWizardViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
                services.AddTransient<DashboardView>();
                services.AddTransient<MaintenanceView>();
                services.AddTransient<ServiceModeView>();
                services.AddTransient<HistoryView>();
                services.AddTransient<PcInfoView>();
                services.AddTransient<SettingsView>();
                services.AddTransient<FirstLaunchWizardView>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Apply theme from settings or system
        using (var scope = _host.Services.CreateScope())
        {
            var maintService = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();
            await maintService.InitializeDatabaseAsync();
            var settings = await maintService.GetSettingsAsync();
            SettingsViewModel.ApplyTheme(settings.Theme);
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }
}
