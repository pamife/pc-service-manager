using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.Core.Models;

namespace PcServiceManager.Infrastructure.Services;

public class HardwareDiagnosticsService : IHardwareDiagnosticsService
{
    public Task<PcDiagnosticInfo> GetDiagnosticInfoAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var info = new PcDiagnosticInfo();

            try
            {
                info.MachineName = Environment.MachineName;
                info.OsDescription = RuntimeInformation.OSDescription;
                info.OsArchitecture = RuntimeInformation.OSArchitecture.ToString();
                info.Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

                // Safely query WMI for Windows Version & Details
                TryQueryWmi("SELECT Caption, Version, InstallDate FROM Win32_OperatingSystem", obj =>
                {
                    info.OsVersion = obj["Caption"]?.ToString() ?? obj["Version"]?.ToString() ?? RuntimeInformation.OSDescription;
                    if (obj["InstallDate"] != null)
                    {
                        var dStr = obj["InstallDate"].ToString();
                        if (dStr != null && dStr.Length >= 8 &&
                            DateTime.TryParseExact(dStr.Substring(0, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var instDate))
                        {
                            info.InstallDate = instDate;
                        }
                    }
                });

                // Computer System (Manufacturer, Model, Total Physical Memory)
                TryQueryWmi("SELECT Manufacturer, Model, TotalPhysicalMemory, SystemType FROM Win32_ComputerSystem", obj =>
                {
                    info.Manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? "Not available";
                    info.Model = obj["Model"]?.ToString()?.Trim() ?? "Not available";
                    info.SystemType = obj["SystemType"]?.ToString() ?? "Not available";

                    if (obj["TotalPhysicalMemory"] != null &&
                        long.TryParse(obj["TotalPhysicalMemory"].ToString(), out var bytes))
                    {
                        info.TotalRam = $"{Math.Round((double)bytes / (1024 * 1024 * 1024), 1)} GB";
                    }
                });

                // Processor
                TryQueryWmi("SELECT Name, NumberOfLogicalProcessors FROM Win32_Processor", obj =>
                {
                    info.CpuName = obj["Name"]?.ToString()?.Trim() ?? "Not available";
                    if (obj["NumberOfLogicalProcessors"] != null &&
                        int.TryParse(obj["NumberOfLogicalProcessors"].ToString(), out var cores))
                    {
                        info.CpuLogicalCores = cores;
                    }
                });

                // Video Controller (GPU)
                TryQueryWmi("SELECT Name FROM Win32_VideoController", obj =>
                {
                    var name = obj["Name"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        info.GpuName = string.IsNullOrEmpty(info.GpuName) || info.GpuName == "Not available"
                            ? name
                            : $"{info.GpuName}, {name}";
                    }
                });

                // Baseboard (Motherboard)
                TryQueryWmi("SELECT Manufacturer, Product FROM Win32_BaseBoard", obj =>
                {
                    var mfg = obj["Manufacturer"]?.ToString()?.Trim();
                    var prod = obj["Product"]?.ToString()?.Trim();
                    info.Motherboard = $"{mfg} {prod}".Trim();
                });

                // BIOS
                TryQueryWmi("SELECT SMBIOSBIOSVersion, ReleaseDate FROM Win32_BIOS", obj =>
                {
                    var biosVer = obj["SMBIOSBIOSVersion"]?.ToString()?.Trim();
                    info.BiosVersion = biosVer ?? "Not available";
                });

                // Drives
                var driveList = new List<DriveInfoModel>();
                try
                {
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
                        {
                            driveList.Add(new DriveInfoModel
                            {
                                Name = drive.Name,
                                VolumeLabel = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? "Local Disk" : drive.VolumeLabel,
                                DriveFormat = drive.DriveFormat,
                                TotalSize = drive.TotalSize,
                                AvailableFreeSpace = drive.AvailableFreeSpace
                            });
                        }
                    }
                }
                catch
                {
                    // Fallback if drive querying is restricted
                }
                info.Drives = driveList;
            }
            catch
            {
                // Never crash
            }

            return info;
        }, cancellationToken);
    }

    private static void TryQueryWmi(string query, Action<ManagementObject> onObject)
    {
        try
        {
            if (!OperatingSystem.IsWindows()) return;

            using var searcher = new ManagementObjectSearcher(query);
            using var collection = searcher.Get();
            foreach (ManagementObject obj in collection)
            {
                onObject(obj);
                break; // First match is usually sufficient for single-system properties
            }
        }
        catch
        {
            // Silently ignore WMI failures and fallback to default
        }
    }
}
