using System.ComponentModel.DataAnnotations;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.Core.Entities;

public class MaintenanceTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PcAssetId { get; set; }

    public PcAsset? PcAsset { get; set; }

    public int CategoryId { get; set; }

    public MaintenanceCategory? Category { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(4000)]
    public string? DetailedInstructions { get; set; }

    [MaxLength(500)]
    public string? SafetyWarning { get; set; }

    public bool IsAdvanced { get; set; }

    public DeviceType DeviceTypeFilter { get; set; } = DeviceType.Other; // Other means Applicable to all

    public IntervalType IntervalType { get; set; } = IntervalType.Months;

    public int IntervalValue { get; set; } = 1;

    public bool IsEnabled { get; set; } = true;

    public DateTime? LastPerformedDate { get; set; }

    public DateTime? NextDueDate { get; set; }

    public QuickActionType QuickAction { get; set; } = QuickActionType.None;

    [MaxLength(500)]
    public string? QuickActionPayload { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
