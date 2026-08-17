using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.Core.Interfaces;

public interface IMaintenanceService
{
    Task<PcAsset?> GetActivePcAsync(CancellationToken cancellationToken = default);
    Task<List<PcAsset>> GetAllPcsAsync(CancellationToken cancellationToken = default);
    Task<PcAsset> CreatePcAsync(string name, DeviceType deviceType, string? notes = null, string? defaultTechnician = null, CancellationToken cancellationToken = default);
    Task UpdatePcAsync(PcAsset pc, CancellationToken cancellationToken = default);
    Task DeletePcAsync(Guid pcId, CancellationToken cancellationToken = default);
    Task SetActivePcAsync(Guid pcId, CancellationToken cancellationToken = default);

    Task<List<MaintenanceCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<MaintenanceTask>> GetTasksForPcAsync(Guid pcId, CancellationToken cancellationToken = default);
    Task<MaintenanceTask> AddTaskAsync(MaintenanceTask task, CancellationToken cancellationToken = default);
    Task UpdateTaskAsync(MaintenanceTask task, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<List<MaintenanceTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<MaintenanceTemplate> AddTemplateAsync(MaintenanceTemplate template, CancellationToken cancellationToken = default);
    Task DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);

    Task<ServiceSession> StartServiceSessionAsync(Guid pcId, string templateName, string? technician, CancellationToken cancellationToken = default);
    Task<ServiceSession> CompleteServiceSessionAsync(Guid sessionId, string? overallNote, List<ServiceTaskResult> results, CancellationToken cancellationToken = default);
    Task CancelServiceSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<List<ServiceSession>> GetServiceHistoryAsync(Guid pcId, CancellationToken cancellationToken = default);
    Task<ServiceSession?> GetServiceSessionDetailsAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<AppSettings> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task UpdateSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);

    Task InitializeDatabaseAsync(CancellationToken cancellationToken = default);
    Task ResetDatabaseAsync(CancellationToken cancellationToken = default);
}
