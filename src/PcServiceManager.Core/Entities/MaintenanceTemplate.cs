using System.ComponentModel.DataAnnotations;

namespace PcServiceManager.Core.Entities;

public class MaintenanceTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsBuiltIn { get; set; }

    public ICollection<MaintenanceTemplateItem> Items { get; set; } = new List<MaintenanceTemplateItem>();
}
