using System.ComponentModel.DataAnnotations;

namespace PcServiceManager.Core.Entities;

public class MaintenanceCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Icon { get; set; } = "Wrench24";

    public int SortOrder { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<MaintenanceTask> Tasks { get; set; } = new List<MaintenanceTask>();
}
