using System.Diagnostics;
using System.IO;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.Core.Models;

namespace PcServiceManager.Infrastructure.Services;

public class SystemActionService : ISystemActionService
{
    public bool ExecuteQuickAction(QuickActionType actionType, string? payload = null)
    {
        try
        {
            switch (actionType)
            {
                case QuickActionType.WindowsUpdate:
                    Process.Start(new ProcessStartInfo("ms-settings:windowsupdate") { UseShellExecute = true });
                    return true;

                case QuickActionType.WindowsSecurity:
                    Process.Start(new ProcessStartInfo("windowsdefender:") { UseShellExecute = true });
                    return true;

                case QuickActionType.StorageSense:
                    Process.Start(new ProcessStartInfo("ms-settings:storagesense") { UseShellExecute = true });
                    return true;

                case QuickActionType.DeviceManager:
                    Process.Start(new ProcessStartInfo("devmgmt.msc") { UseShellExecute = true });
                    return true;

                case QuickActionType.TaskManager:
                    Process.Start(new ProcessStartInfo("taskmgr.exe") { UseShellExecute = true });
                    return true;

                case QuickActionType.ResourceMonitor:
                    Process.Start(new ProcessStartInfo("resmon.exe") { UseShellExecute = true });
                    return true;

                case QuickActionType.DiskManagement:
                    Process.Start(new ProcessStartInfo("diskmgmt.msc") { UseShellExecute = true });
                    return true;

                case QuickActionType.EventViewer:
                    Process.Start(new ProcessStartInfo("eventvwr.msc") { UseShellExecute = true });
                    return true;

                case QuickActionType.PowerOptions:
                    Process.Start(new ProcessStartInfo("ms-settings:powersleep") { UseShellExecute = true });
                    return true;

                case QuickActionType.NetworkSettings:
                    Process.Start(new ProcessStartInfo("ms-settings:network") { UseShellExecute = true });
                    return true;

                case QuickActionType.SystemIntegrityGuide:
                    // Open Windows Terminal or Command Prompt guide
                    Process.Start(new ProcessStartInfo("cmd.exe", "/c start cmd.exe") { UseShellExecute = true });
                    return true;

                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    public Task<TempFileScanResult> ScanTemporaryFilesAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var result = new TempFileScanResult();

            // 1. User Temp folder (%TEMP%)
            var userTemp = Path.GetTempPath();
            if (Directory.Exists(userTemp))
            {
                var (bytes, count) = GetFolderMetrics(userTemp);
                result.Locations.Add(new ScannedFolder
                {
                    Path = userTemp,
                    Description = "User Temporary Cache (%TEMP%)",
                    Bytes = bytes,
                    FileCount = count,
                    IsSelected = true
                });
            }

            // 2. Windows Temp folder (C:\Windows\Temp)
            var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var winTemp = Path.Combine(winDir, "Temp");
            if (Directory.Exists(winTemp))
            {
                var (bytes, count) = GetFolderMetrics(winTemp);
                result.Locations.Add(new ScannedFolder
                {
                    Path = winTemp,
                    Description = "System Temporary Directory (Windows\\Temp)",
                    Bytes = bytes,
                    FileCount = count,
                    IsSelected = true
                });
            }

            // 3. User Crash Dumps (%LOCALAPPDATA%\CrashDumps)
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var crashDumps = Path.Combine(localAppData, "CrashDumps");
            if (Directory.Exists(crashDumps))
            {
                var (bytes, count) = GetFolderMetrics(crashDumps);
                result.Locations.Add(new ScannedFolder
                {
                    Path = crashDumps,
                    Description = "Application Crash Dumps",
                    Bytes = bytes,
                    FileCount = count,
                    IsSelected = true
                });
            }

            // 4. Windows Software Distribution Downloads
            var softDistDownload = Path.Combine(winDir, "SoftwareDistribution", "Download");
            if (Directory.Exists(softDistDownload))
            {
                var (bytes, count) = GetFolderMetrics(softDistDownload);
                result.Locations.Add(new ScannedFolder
                {
                    Path = softDistDownload,
                    Description = "Windows Update Cache Files",
                    Bytes = bytes,
                    FileCount = count,
                    IsSelected = false // Unchecked by default for extra safety
                });
            }

            return result;
        }, cancellationToken);
    }

    public Task<(int deletedFiles, long freedBytes)> CleanTemporaryFilesAsync(TempFileScanResult scanResult, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            int deletedFiles = 0;
            long freedBytes = 0;

            foreach (var loc in scanResult.Locations.Where(l => l.IsSelected))
            {
                if (!Directory.Exists(loc.Path)) continue;

                try
                {
                    var dirInfo = new DirectoryInfo(loc.Path);
                    foreach (var file in dirInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        try
                        {
                            // Skip files modified within the last 2 hours (actively used)
                            if (DateTime.UtcNow - file.LastWriteTimeUtc < TimeSpan.FromHours(2))
                            {
                                continue;
                            }

                            var len = file.Length;
                            file.Delete();
                            deletedFiles++;
                            freedBytes += len;
                        }
                        catch
                        {
                            // File locked or permission denied - safe to ignore
                        }
                    }

                    // Attempt empty subdirectory cleanup safely
                    foreach (var subDir in dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        try
                        {
                            if (DateTime.UtcNow - subDir.LastWriteTimeUtc > TimeSpan.FromHours(2))
                            {
                                subDir.Delete(true);
                            }
                        }
                        catch
                        {
                            // In use - safe to skip
                        }
                    }
                }
                catch
                {
                    // Ignore folder access restrictions
                }
            }

            return (deletedFiles, freedBytes);
        }, cancellationToken);
    }

    private static (long bytes, int count) GetFolderMetrics(string path)
    {
        long totalBytes = 0;
        int fileCount = 0;

        try
        {
            var dir = new DirectoryInfo(path);
            foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    totalBytes += file.Length;
                    fileCount++;
                }
                catch
                {
                    // In-use or permissions
                }
            }
        }
        catch
        {
            // Directory traversal issue
        }

        return (totalBytes, fileCount);
    }
}
