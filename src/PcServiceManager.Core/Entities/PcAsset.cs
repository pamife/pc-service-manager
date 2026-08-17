using System.ComponentModel.DataAnnotations;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.Core.Entities;

public class PcAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    public DeviceType DeviceType { get; set; } = DeviceType.Desktop;

    [MaxLength(150)]
    public string? OperatingSystem { get; set; }

    public DateTime? InstallDate { get; set; }

    [MaxLength(150)]
    public string? CpuName { get; set; }

    [MaxLength(50)]
    public string? RamTotal { get; set; }

    [MaxLength(150)]
    public string? GpuName { get; set; }

    [MaxLength(200)]
    public string? StorageSummary { get; set; }

    [MaxLength(100)]
    public string? Hostname { get; set; }

    [MaxLength(150)]
    public string? Motherboard { get; set; }

    [MaxLength(100)]
    public string? BiosVersion { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public bool IsDefault { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MaintenanceTask> Tasks { get; set; } = new List<MaintenanceTask>();

    public ICollection<ServiceSession> ServiceSessions { get; set; } = new List<ServiceSession>();
}
