using System.ComponentModel.DataAnnotations;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.Core.Entities;

public class ServiceTaskResult
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ServiceSessionId { get; set; }

    public ServiceSession? ServiceSession { get; set; }

    public Guid? MaintenanceTaskId { get; set; }

    [Required]
    [MaxLength(150)]
    public string TaskTitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    public ServiceTaskStatus Status { get; set; } = ServiceTaskStatus.Completed;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
