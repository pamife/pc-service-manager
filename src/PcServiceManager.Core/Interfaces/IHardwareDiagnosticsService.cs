using PcServiceManager.Core.Models;

namespace PcServiceManager.Core.Interfaces;

public interface IHardwareDiagnosticsService
{
    Task<PcDiagnosticInfo> GetDiagnosticInfoAsync(CancellationToken cancellationToken = default);
}
