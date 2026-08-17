using System.ComponentModel.DataAnnotations;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.Core.Entities;

public class AppSettings
{
    public int Id { get; set; } = 1;

    public Guid? ActivePcId { get; set; }

    public AppTheme Theme { get; set; } = AppTheme.System;

    public int DueSoonDaysThreshold { get; set; } = 7;

    public bool NotificationsEnabled { get; set; } = true;

    [MaxLength(50)]
    public string NotificationFrequency { get; set; } = "OnStartup"; // Daily, Weekly, OnStartup

    public DateTime? LastNotificationCheck { get; set; }

    [MaxLength(100)]
    public string? DefaultTechnicianName { get; set; }

    public DateTime? LastBackupDate { get; set; }
}
