using PcServiceManager.Core.Models;

namespace PcServiceManager.Core.Interfaces;

public interface IBackupExportService
{
    Task<string> ExportFullBackupJsonAsync(Guid? pcAssetId = null, CancellationToken cancellationToken = default);
    Task<bool> ImportFullBackupJsonAsync(string jsonContent, CancellationToken cancellationToken = default);
    Task<string> ExportServiceHistoryCsvAsync(Guid pcAssetId, CancellationToken cancellationToken = default);
    Task<string> ExportServiceReportTextAsync(Guid serviceSessionId, CancellationToken cancellationToken = default);
}
