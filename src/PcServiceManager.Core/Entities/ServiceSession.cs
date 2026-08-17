using System.ComponentModel.DataAnnotations;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.Core.Entities;

public class ServiceSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PcAssetId { get; set; }

    public PcAsset? PcAsset { get; set; }

    [Required]
    [MaxLength(100)]
    public string TemplateName { get; set; } = "General Service";

    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndTime { get; set; }

    public int DurationMinutes { get; set; }

    [MaxLength(100)]
    public string? PerformedBy { get; set; }

    [MaxLength(4000)]
    public string? OverallNote { get; set; }

    public int TotalTasks { get; set; }

    public int CompletedCount { get; set; }

    public int SkippedCount { get; set; }

    public int NeedsAttentionCount { get; set; }

    public int NotApplicableCount { get; set; }

    public ServiceSessionStatus Status { get; set; } = ServiceSessionStatus.InProgress;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ServiceTaskResult> TaskResults { get; set; } = new List<ServiceTaskResult>();
}
