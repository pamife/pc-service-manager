using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Interfaces;

namespace PcServiceManager.UI.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IBackupExportService _backupExportService;
    private Guid _activePcId;

    [ObservableProperty]
    private ObservableCollection<ServiceSession> _serviceSessions = new();

    [ObservableProperty]
    private ServiceSession? _selectedSession;

    [ObservableProperty]
    private bool _isDetailViewOpen;

    [ObservableProperty]
    private string _pcName = string.Empty;

    [ObservableProperty]
    private string _notificationMessage = string.Empty;

    public HistoryViewModel(IMaintenanceService maintenanceService, IBackupExportService backupExportService)
    {
        _maintenanceService = maintenanceService;
        _backupExportService = backupExportService;
    }

    public async Task LoadAsync(Guid pcId)
    {
        _activePcId = pcId;
        var pc = await _maintenanceService.GetActivePcAsync();
        PcName = pc?.Name ?? "PC";

        var history = await _maintenanceService.GetServiceHistoryAsync(pcId);
        ServiceSessions.Clear();
        foreach (var s in history)
        {
            ServiceSessions.Add(s);
        }

        IsDetailViewOpen = false;
        SelectedSession = null;
    }

    [RelayCommand]
    private async Task OpenSessionDetailsAsync(ServiceSession session)
    {
        var fullSession = await _maintenanceService.GetServiceSessionDetailsAsync(session.Id);
        SelectedSession = fullSession ?? session;
        IsDetailViewOpen = true;
    }

    [RelayCommand]
    private void CloseDetails()
    {
        IsDetailViewOpen = false;
        SelectedSession = null;
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "CSV Spreadsheet (*.csv)|*.csv",
            FileName = $"{PcName}_ServiceHistory_{DateTime.Now:yyyyMMdd}.csv"
        };

        if (saveDialog.ShowDialog() == true)
        {
            var csv = await _backupExportService.ExportServiceHistoryCsvAsync(_activePcId);
            await File.WriteAllTextAsync(saveDialog.FileName, csv);
            NotificationMessage = $"Service history successfully exported to {Path.GetFileName(saveDialog.FileName)}";
        }
    }

    [RelayCommand]
    private async Task CopyReportTextAsync(ServiceSession session)
    {
        var report = await _backupExportService.ExportServiceReportTextAsync(session.Id);
        Clipboard.SetText(report);
        NotificationMessage = "Service certificate copied to clipboard!";
    }
}
