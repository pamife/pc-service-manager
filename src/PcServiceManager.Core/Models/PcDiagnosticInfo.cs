namespace PcServiceManager.Core.Models;

public class DriveInfoModel
{
    public string Name { get; set; } = string.Empty;
    public string VolumeLabel { get; set; } = string.Empty;
    public string DriveFormat { get; set; } = string.Empty;
    public long TotalSize { get; set; }
    public long AvailableFreeSpace { get; set; }
    public double TotalSizeGb => Math.Round((double)TotalSize / (1024 * 1024 * 1024), 1);
    public double FreeSpaceGb => Math.Round((double)AvailableFreeSpace / (1024 * 1024 * 1024), 1);
    public double UsedSpaceGb => Math.Round(TotalSizeGb - FreeSpaceGb, 1);
    public double UsedPercent => TotalSize > 0 ? Math.Round(((double)(TotalSize - AvailableFreeSpace) / TotalSize) * 100, 1) : 0;
    public bool IsLowSpace => UsedPercent >= 90.0;
}

public class PcDiagnosticInfo
{
    public string MachineName { get; set; } = "Not available";
    public string OsDescription { get; set; } = "Not available";
    public string OsVersion { get; set; } = "Not available";
    public string OsArchitecture { get; set; } = "Not available";
    public DateTime? InstallDate { get; set; }
    public TimeSpan Uptime { get; set; }
    public string FormattedUptime => $"{(int)Uptime.TotalDays}d {Uptime.Hours}h {Uptime.Minutes}m";
    public string Manufacturer { get; set; } = "Not available";
    public string Model { get; set; } = "Not available";
    public string SystemType { get; set; } = "Not available";
    public string CpuName { get; set; } = "Not available";
    public int CpuLogicalCores { get; set; }
    public string TotalRam { get; set; } = "Not available";
    public string AvailableRam { get; set; } = "Not available";
    public string GpuName { get; set; } = "Not available";
    public string Motherboard { get; set; } = "Not available";
    public string BiosVersion { get; set; } = "Not available";
    public List<DriveInfoModel> Drives { get; set; } = new();
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
}
