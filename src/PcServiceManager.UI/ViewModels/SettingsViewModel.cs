using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;
using Wpf.Ui.Appearance;

namespace PcServiceManager.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IBackupExportService _backupExportService;

    public Action? OnDataChanged { get; set; }

    [ObservableProperty]
    private ObservableCollection<PcAsset> _pcList = new();

    [ObservableProperty]
    private PcAsset? _selectedPc;

    [ObservableProperty]
    private string _editPcName = string.Empty;

    [ObservableProperty]
    private DeviceType _editPcDeviceType = DeviceType.Desktop;

    [ObservableProperty]
    private string _editPcNotes = string.Empty;

    [ObservableProperty]
    private AppTheme _currentTheme = AppTheme.System;

    [ObservableProperty]
    private int _dueSoonDaysThreshold = 7;

    [ObservableProperty]
    private bool _notificationsEnabled = true;

    [ObservableProperty]
    private string _defaultTechnicianName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isAddingNewPc;

    [ObservableProperty]
    private string _newPcName = string.Empty;

    [ObservableProperty]
    private DeviceType _newPcDeviceType = DeviceType.Desktop;

    [ObservableProperty]
    private string _newPcNotes = string.Empty;

    public SettingsViewModel(
        IMaintenanceService maintenanceService,
        IBackupExportService backupExportService)
    {
        _maintenanceService = maintenanceService;
        _backupExportService = backupExportService;
    }

    public async Task LoadAsync()
    {
        var settings = await _maintenanceService.GetSettingsAsync();
        CurrentTheme = settings.Theme;
        DueSoonDaysThreshold = settings.DueSoonDaysThreshold;
        NotificationsEnabled = settings.NotificationsEnabled;
        DefaultTechnicianName = settings.DefaultTechnicianName ?? Environment.UserName;

        var pcs = await _maintenanceService.GetAllPcsAsync();
        PcList.Clear();
        foreach (var p in pcs) PcList.Add(p);

        var active = await _maintenanceService.GetActivePcAsync();
        SelectedPc = PcList.FirstOrDefault(p => p.Id == active?.Id) ?? PcList.FirstOrDefault();

        if (SelectedPc != null)
        {
            EditPcName = SelectedPc.Name;
            EditPcDeviceType = SelectedPc.DeviceType;
            EditPcNotes = SelectedPc.Notes ?? string.Empty;
        }

        IsAddingNewPc = false;
    }

    partial void OnSelectedPcChanged(PcAsset? value)
    {
        if (value != null)
        {
            EditPcName = value.Name;
            EditPcDeviceType = value.DeviceType;
            EditPcNotes = value.Notes ?? string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        var settings = await _maintenanceService.GetSettingsAsync();
        settings.Theme = CurrentTheme;
        settings.DueSoonDaysThreshold = DueSoonDaysThreshold;
        settings.NotificationsEnabled = NotificationsEnabled;
        settings.DefaultTechnicianName = DefaultTechnicianName;

        await _maintenanceService.UpdateSettingsAsync(settings);
        ApplyTheme(CurrentTheme);

        StatusMessage = "Settings saved successfully.";
        OnDataChanged?.Invoke();
    }

    [RelayCommand]
    private async Task SwitchActivePcAsync(PcAsset pc)
    {
        await _maintenanceService.SetActivePcAsync(pc.Id);
        SelectedPc = pc;
        StatusMessage = $"Active PC switched to '{pc.Name}'.";
        OnDataChanged?.Invoke();
    }

    [RelayCommand]
    private async Task SavePcDetailsAsync()
    {
        if (SelectedPc == null || string.IsNullOrWhiteSpace(EditPcName)) return;

        SelectedPc.Name = EditPcName.Trim();
        SelectedPc.DeviceType = EditPcDeviceType;
        SelectedPc.Notes = EditPcNotes.Trim();

        await _maintenanceService.UpdatePcAsync(SelectedPc);
        StatusMessage = $"PC '{SelectedPc.Name}' updated.";
        OnDataChanged?.Invoke();
    }

    [RelayCommand]
    private void ShowAddPcForm()
    {
        NewPcName = string.Empty;
        NewPcDeviceType = DeviceType.Desktop;
        NewPcNotes = string.Empty;
        IsAddingNewPc = true;
    }

    [RelayCommand]
    private void CancelAddPc()
    {
        IsAddingNewPc = false;
    }

    [RelayCommand]
    private async Task CreateNewPcAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPcName)) return;

        var pc = await _maintenanceService.CreatePcAsync(NewPcName, NewPcDeviceType, NewPcNotes, DefaultTechnicianName);
        IsAddingNewPc = false;
        await LoadAsync();
        StatusMessage = $"Created PC '{pc.Name}' with default maintenance schedule.";
        OnDataChanged?.Invoke();
    }

    [RelayCommand]
    private async Task DeleteSelectedPcAsync()
    {
        if (SelectedPc == null) return;

        if (PcList.Count <= 1)
        {
            StatusMessage = "Cannot delete the only configured PC.";
            return;
        }

        var result = MessageBox.Show(
            $"Are you sure you want to delete PC '{SelectedPc.Name}' and all its maintenance tasks and service history?",
            "Confirm PC Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            await _maintenanceService.DeletePcAsync(SelectedPc.Id);
            await LoadAsync();
            StatusMessage = "PC deleted.";
            OnDataChanged?.Invoke();
        }
    }

    [RelayCommand]
    private async Task ExportBackupJsonAsync()
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "JSON Backup (*.json)|*.json",
            FileName = $"PC_Service_Manager_Backup_{DateTime.Now:yyyyMMdd_HHmm}.json"
        };

        if (saveDialog.ShowDialog() == true)
        {
            var json = await _backupExportService.ExportFullBackupJsonAsync();
            await File.WriteAllTextAsync(saveDialog.FileName, json);
            StatusMessage = $"Full database backup exported to {Path.GetFileName(saveDialog.FileName)}";
        }
    }

    [RelayCommand]
    private async Task ImportBackupJsonAsync()
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "JSON Backup (*.json)|*.json"
        };

        if (openDialog.ShowDialog() == true)
        {
            var json = await File.ReadAllTextAsync(openDialog.FileName);
            var success = await _backupExportService.ImportFullBackupJsonAsync(json);

            if (success)
            {
                await LoadAsync();
                StatusMessage = "Backup successfully restored!";
                OnDataChanged?.Invoke();
            }
            else
            {
                StatusMessage = "Failed to import backup. Invalid JSON format.";
            }
        }
    }

    [RelayCommand]
    private async Task ResetAllDataAsync()
    {
        var result = MessageBox.Show(
            "WARNING: This will permanently delete all PCs, maintenance schedules, and service history. Are you sure you want to factory reset all data?",
            "Factory Reset Warning",
            MessageBoxButton.YesNo,
            MessageBoxImage.Stop);

        if (result == MessageBoxResult.Yes)
        {
            await _maintenanceService.ResetDatabaseAsync();
            await LoadAsync();
            StatusMessage = "Application database has been reset to defaults.";
            OnDataChanged?.Invoke();
        }
    }

    public static void ApplyTheme(AppTheme theme)
    {
        try
        {
            switch (theme)
            {
                case AppTheme.Dark:
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    break;
                case AppTheme.Light:
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    break;
                default:
                    ApplicationThemeManager.ApplySystemTheme();
                    break;
            }
        }
        catch
        {
            // Ignore theme application fallback
        }
    }
}
