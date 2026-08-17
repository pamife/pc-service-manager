using System.ComponentModel.DataAnnotations;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.Core.Entities;

public class MaintenanceTemplateItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MaintenanceTemplateId { get; set; }

    public MaintenanceTemplate? MaintenanceTemplate { get; set; }

    [Required]
    [MaxLength(150)]
    public string TaskTitle { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(4000)]
    public string? DetailedInstructions { get; set; }

    [MaxLength(500)]
    public string? SafetyWarning { get; set; }

    public bool IsAdvanced { get; set; }

    public IntervalType DefaultIntervalType { get; set; } = IntervalType.Months;

    public int DefaultIntervalValue { get; set; } = 1;

    public QuickActionType QuickAction { get; set; } = QuickActionType.None;

    public int SortOrder { get; set; }
}
