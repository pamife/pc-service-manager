using PcServiceManager.Core.Entities;

namespace PcServiceManager.Core.Models;

public class FullBackupDto
{
    public string AppVersion { get; set; } = "1.0.0";
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;
    public string ExportedBy { get; set; } = Environment.UserName;
    public List<PcAsset> PcAssets { get; set; } = new();
    public List<MaintenanceCategory> Categories { get; set; } = new();
    public List<MaintenanceTask> Tasks { get; set; } = new();
    public List<MaintenanceTemplate> Templates { get; set; } = new();
    public List<MaintenanceTemplateItem> TemplateItems { get; set; } = new();
    public List<ServiceSession> ServiceSessions { get; set; } = new();
    public List<ServiceTaskResult> ServiceTaskResults { get; set; } = new();
    public AppSettings? Settings { get; set; }
}
