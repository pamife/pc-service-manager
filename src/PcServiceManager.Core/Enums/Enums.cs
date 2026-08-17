namespace PcServiceManager.Core.Enums;

public enum DeviceType
{
    Desktop,
    Laptop,
    Server,
    AllInOne,
    Other
}

public enum IntervalType
{
    Days,
    Weeks,
    Months,
    Years,
    OneTime,
    Custom,
    Disabled
}

public enum MaintenanceStatus
{
    Good,
    DueSoon,
    Overdue,
    Disabled
}

public enum OverallHealthStatus
{
    Good,
    DueSoon,
    Overdue,
    Unknown
}

public enum ServiceTaskStatus
{
    Completed,
    Skipped,
    NeedsAttention,
    NotApplicable
}

public enum ServiceSessionStatus
{
    InProgress,
    Completed,
    Cancelled
}

public enum QuickActionType
{
    None,
    WindowsUpdate,
    WindowsSecurity,
    StorageSense,
    DeviceManager,
    TaskManager,
    ResourceMonitor,
    DiskManagement,
    TempFilesClean,
    SystemIntegrityGuide,
    EventViewer,
    PowerOptions,
    NetworkSettings
}

public enum AppTheme
{
    System,
    Dark,
    Light
}
