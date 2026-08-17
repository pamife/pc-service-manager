using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Models;

namespace PcServiceManager.Core.Interfaces;

public interface ISystemActionService
{
    bool ExecuteQuickAction(QuickActionType actionType, string? payload = null);
    Task<TempFileScanResult> ScanTemporaryFilesAsync(CancellationToken cancellationToken = default);
    Task<(int deletedFiles, long freedBytes)> CleanTemporaryFilesAsync(TempFileScanResult scanResult, CancellationToken cancellationToken = default);
}
