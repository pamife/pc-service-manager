using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.Core.Data;

public static class DefaultDataSeed
{
    public static List<MaintenanceCategory> GetDefaultCategories()
    {
        return new List<MaintenanceCategory>
        {
            new() { Id = 1, Name = "Software & Updates", Icon = "Apps24", SortOrder = 1, Description = "Windows updates, drivers, software, and startup apps" },
            new() { Id = 2, Name = "Security & Integrity", Icon = "ShieldCheckmark24", SortOrder = 2, Description = "Antivirus, system integrity, backups, and restore points" },
            new() { Id = 3, Name = "Storage & Performance", Icon = "Storage24", SortOrder = 3, Description = "Disk health, temporary files cleanup, and storage capacity" },
            new() { Id = 4, Name = "Physical Hardware & Dust", Icon = "Wrench24", SortOrder = 4, Description = "Dust filters, fans, cables, noise, and temperatures" },
            new() { Id = 5, Name = "Peripherals & Workstation", Icon = "Desktop24", SortOrder = 5, Description = "Monitors, keyboard, mouse, USB ports, and exterior" }
        };
    }

    public static List<MaintenanceTask> CreateDefaultTasksForPc(Guid pcAssetId, DeviceType deviceType)
    {
        var tasks = new List<MaintenanceTask>();

        // 1. Software & Updates
        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 1,
            Title = "Check Windows Updates",
            Description = "Scan and install the latest Windows cumulative and feature updates.",
            DetailedInstructions = "1. Open Windows Update Settings.\n2. Click 'Check for updates'.\n3. Allow all pending quality and security updates to install.\n4. Restart the PC if requested.",
            IntervalType = IntervalType.Days,
            IntervalValue = 14,
            QuickAction = QuickActionType.WindowsUpdate,
            SortOrder = 1
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 1,
            Title = "Check Hardware Drivers",
            Description = "Verify GPU drivers (NVIDIA/AMD/Intel) and check Device Manager for unknown devices or warnings.",
            DetailedInstructions = "1. Open Device Manager.\n2. Check for any yellow exclamation mark warning symbols.\n3. Open your GPU control app (NVIDIA App/GeForce Experience, AMD Adrenalin, or Intel Arc Control) and install the latest graphics driver.",
            IntervalType = IntervalType.Months,
            IntervalValue = 1,
            QuickAction = QuickActionType.DeviceManager,
            SortOrder = 2
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 1,
            Title = "Review Startup Applications",
            Description = "Disable unnecessary apps that launch at Windows startup to improve boot speed and memory availability.",
            DetailedInstructions = "1. Open Task Manager.\n2. Navigate to the 'Startup apps' tab.\n3. Identify apps not needed immediately at startup and set them to 'Disabled'.\n4. Keep essential background helpers (audio, GPU, cloud sync).",
            IntervalType = IntervalType.Months,
            IntervalValue = 2,
            QuickAction = QuickActionType.TaskManager,
            SortOrder = 3
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 1,
            Title = "Update Web Browsers & Essential Software",
            Description = "Ensure web browsers (Chrome, Edge, Firefox, Brave) and critical productivity tools are up to date.",
            DetailedInstructions = "1. Open installed web browsers, navigate to Settings > About, and trigger the update check.\n2. Update common tools like archive managers (7-Zip), document viewers (Adobe/Foxit), and communication apps.",
            IntervalType = IntervalType.Months,
            IntervalValue = 1,
            QuickAction = QuickActionType.None,
            SortOrder = 4
        });

        // 2. Security & Integrity
        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 2,
            Title = "Windows Security & Antivirus Status",
            Description = "Ensure Microsoft Defender or installed security suite is active, definitions are current, and no threats are detected.",
            DetailedInstructions = "1. Open Windows Security.\n2. Check Virus & Threat Protection status.\n3. Verify Protection definitions are current.\n4. Run a Quick Scan if no scan was performed recently.",
            IntervalType = IntervalType.Days,
            IntervalValue = 14,
            QuickAction = QuickActionType.WindowsSecurity,
            SortOrder = 5
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 2,
            Title = "Verify Data Backup",
            Description = "Check that critical personal files, documents, and photos are backed up to external drive or cloud storage.",
            DetailedInstructions = "1. Verify external backup drive connection or cloud sync client (OneDrive, Google Drive, etc.).\n2. Check the date of the most recent successful backup run.\n3. Spot check a few restored files to confirm backups are not corrupted.",
            IntervalType = IntervalType.Months,
            IntervalValue = 1,
            QuickAction = QuickActionType.None,
            SortOrder = 6
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 2,
            Title = "Windows System Integrity Check (SFC / DISM)",
            Description = "Run system file checker to detect and repair corrupted Windows system files.",
            DetailedInstructions = "1. Open Windows Terminal / PowerShell as Administrator.\n2. Run: sfc /scannow\n3. If errors are found that cannot be repaired, run: DISM /Online /Cleanup-Image /RestoreHealth\n4. Confirm that Windows resource protection found no violations or successfully repaired them.",
            IntervalType = IntervalType.Months,
            IntervalValue = 3,
            QuickAction = QuickActionType.SystemIntegrityGuide,
            SortOrder = 7
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 2,
            Title = "Check System Restore Points",
            Description = "Ensure System Protection is enabled on the Windows OS drive and a recent restore point exists.",
            DetailedInstructions = "1. Open System Properties (System Protection tab).\n2. Verify Protection is 'On' for C: drive.\n3. Create a manual restore point before conducting major updates or driver changes.",
            IntervalType = IntervalType.Months,
            IntervalValue = 2,
            QuickAction = QuickActionType.None,
            SortOrder = 8
        });

        // 3. Storage & Performance
        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 3,
            Title = "Check Disk Space & Storage Sense",
            Description = "Verify all drives have at least 15-20% free space for healthy SSD wear-leveling and Windows paging.",
            DetailedInstructions = "1. Open Storage Settings.\n2. Review space usage on C: drive and secondary drives.\n3. Empty Recycle Bin and remove unnecessary large downloads if space is tight.",
            IntervalType = IntervalType.Months,
            IntervalValue = 1,
            QuickAction = QuickActionType.StorageSense,
            SortOrder = 9
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 3,
            Title = "Safe Temporary Files Cleanup",
            Description = "Review and safely clean temporary cache files, crash dumps, and Windows upgrade remnants.",
            DetailedInstructions = "1. Use the built-in Safe Temp Files scanner in PC Service Manager or Windows Storage Sense.\n2. Preview the items and confirm cleanup.\n3. Never force-delete files currently locked by active applications.",
            IntervalType = IntervalType.Months,
            IntervalValue = 2,
            QuickAction = QuickActionType.TempFilesClean,
            SortOrder = 10
        });

        // 4. Physical Hardware & Dust
        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 4,
            Title = "Clean Dust Filters",
            Description = "Remove and clean front, top, and power supply dust mesh filters to maintain proper airflow.",
            DetailedInstructions = "1. Shut down PC and unplug power cable.\n2. Remove magnetic or slide-out dust filters from the PC chassis.\n3. Clean using a soft brush, vacuum with brush attachment, or wash with warm water (dry completely before reattaching!).\n4. Reinstall filters securely.",
            IntervalType = IntervalType.Months,
            IntervalValue = 3,
            DeviceTypeFilter = DeviceType.Desktop,
            QuickAction = QuickActionType.None,
            SortOrder = 11
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 4,
            Title = "Inspect Fans & Case Interior for Dust",
            Description = "Inspect CPU cooler, GPU heatsink, and case fans for dust accumulation.",
            DetailedInstructions = "1. Power off and disconnect PC.\n2. Open side panel.\n3. Use compressed air in short bursts to clear dust from heatsinks and fan blades (hold fan blades gently to prevent overspinning).\n4. Wipe case surfaces with a dry microfiber cloth.",
            IntervalType = IntervalType.Months,
            IntervalValue = 6,
            QuickAction = QuickActionType.None,
            SortOrder = 12
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 4,
            Title = "Check Operating Temperatures & Fan Noise",
            Description = "Monitor CPU/GPU idle and load temperatures; listen for unusual fan bearing grinding or rattles.",
            DetailedInstructions = "1. Run a hardware monitor or check Task Manager Performance tab.\n2. Confirm idle CPU temp is under 50°C and load temp remains under 85-90°C.\n3. Listen for rattling, buzzing, or oscillating fan motor noise that indicates failing bearings.",
            IntervalType = IntervalType.Months,
            IntervalValue = 3,
            QuickAction = QuickActionType.ResourceMonitor,
            SortOrder = 13
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 4,
            Title = "Inspect Visible Cables & Internal Connectors",
            Description = "Check power cables, SATA/PCIe power connectors, and display cables for secure seating and damage.",
            DetailedInstructions = "1. Inspect external power and HDMI/DisplayPort cables for kinks or strain.\n2. Ensure internal 24-pin, EPS 8-pin, and 12VHPWR/PCIe cables are fully clicked into place.",
            IntervalType = IntervalType.Months,
            IntervalValue = 6,
            QuickAction = QuickActionType.None,
            SortOrder = 14
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 4,
            Title = "Thermal Paste Replacement & Deep Heatsink Service",
            Description = "Replace aged thermal interface material between CPU/GPU and heatsink.",
            DetailedInstructions = "1. Only perform if CPU/GPU temperatures are consistently throttling after dust removal.\n2. Disassemble cooler carefully.\n3. Clean old paste with 99% Isopropyl alcohol and lint-free wipes.\n4. Apply high quality thermal paste (pea-sized dot) and remount cooler with even cross-pattern torque.",
            SafetyWarning = "ADVANCED OPERATION: Disassembling coolers carries risk of damage to CPU pins, GPU dies, and PCB components. Never open the PC power supply unit (PSU). Do not overtighten screws.",
            IsAdvanced = true,
            IntervalType = IntervalType.Years,
            IntervalValue = 2,
            QuickAction = QuickActionType.None,
            SortOrder = 15
        });

        // 5. Peripherals & Workstation
        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 5,
            Title = "Clean PC Exterior & Chassis",
            Description = "Wipe down external chassis panels, front I/O bezel, and glass side panel.",
            DetailedInstructions = "1. Use a damp microfiber cloth with mild electronics cleaner.\n2. Clean dust off top vents and power button.\n3. Clean tempered glass panel with glass cleaner applied to cloth (never spray directly onto PC).",
            IntervalType = IntervalType.Months,
            IntervalValue = 3,
            QuickAction = QuickActionType.None,
            SortOrder = 16
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 5,
            Title = "Clean Monitor, Keyboard & Mouse",
            Description = "Clean display surface with screen-safe wipes; remove debris from keyboard switches and optical mouse sensor.",
            DetailedInstructions = "1. Turn off monitor. Wipe display gently with a clean microfiber cloth and screen cleaner.\n2. Invert keyboard and use compressed air to remove food crumbs/debris from between keycaps.\n3. Clean mouse glide feet and optical sensor lens with a dry cotton swab.",
            IntervalType = IntervalType.Months,
            IntervalValue = 1,
            QuickAction = QuickActionType.None,
            SortOrder = 17
        });

        tasks.Add(new MaintenanceTask
        {
            PcAssetId = pcAssetId,
            CategoryId = 5,
            Title = "Check External USB Ports & Connectors",
            Description = "Inspect front and rear USB-A and USB-C ports for lint, bent pins, or loose jacks.",
            DetailedInstructions = "1. Visually check ports with a flashlight for trapped pocket lint or debris.\n2. Carefully clean lint with a non-conductive wooden or plastic pick if necessary.",
            IntervalType = IntervalType.Months,
            IntervalValue = 6,
            QuickAction = QuickActionType.None,
            SortOrder = 18
        });

        // Laptop Specific Tasks
        if (deviceType == DeviceType.Laptop)
        {
            tasks.Add(new MaintenanceTask
            {
                PcAssetId = pcAssetId,
                CategoryId = 4,
                Title = "Inspect Laptop Display Hinges & Bezel",
                Description = "Check smooth hinge movement and ensure the display casing is not splitting or cracking.",
                DetailedInstructions = "1. Open and close the laptop lid gently from the center.\n2. Check for abnormal resistance, clicking, or separation around the hinge housing.",
                IntervalType = IntervalType.Months,
                IntervalValue = 3,
                DeviceTypeFilter = DeviceType.Laptop,
                QuickAction = QuickActionType.None,
                SortOrder = 19
            });

            tasks.Add(new MaintenanceTask
            {
                PcAssetId = pcAssetId,
                CategoryId = 2,
                Title = "Laptop Battery Health Check",
                Description = "Generate and inspect Windows Battery Report for full charge capacity degradation.",
                DetailedInstructions = "1. Open PowerShell and run: powercfg /batteryreport /output \"$HOME\\Desktop\\battery_report.html\"\n2. Open the report on Desktop.\n3. Compare 'Design Capacity' vs 'Full Charge Capacity' to evaluate battery wear.",
                IntervalType = IntervalType.Months,
                IntervalValue = 3,
                DeviceTypeFilter = DeviceType.Laptop,
                QuickAction = QuickActionType.PowerOptions,
                SortOrder = 20
            });
        }

        return tasks;
    }

    public static List<MaintenanceTemplate> GetDefaultTemplates()
    {
        var quickCheck = new MaintenanceTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Quick Check",
            Description = "Rapid essential health check: Windows Updates, Antivirus status, Disk space, and Temperatures.",
            IsBuiltIn = true,
            Items = new List<MaintenanceTemplateItem>
            {
                new() { TaskTitle = "Check Windows Updates", CategoryName = "Software & Updates", QuickAction = QuickActionType.WindowsUpdate, SortOrder = 1 },
                new() { TaskTitle = "Windows Security & Antivirus Status", CategoryName = "Security & Integrity", QuickAction = QuickActionType.WindowsSecurity, SortOrder = 2 },
                new() { TaskTitle = "Check Disk Space & Storage Sense", CategoryName = "Storage & Performance", QuickAction = QuickActionType.StorageSense, SortOrder = 3 },
                new() { TaskTitle = "Check Operating Temperatures & Fan Noise", CategoryName = "Physical Hardware & Dust", QuickAction = QuickActionType.ResourceMonitor, SortOrder = 4 }
            }
        };

        var regularMaintenance = new MaintenanceTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Regular Maintenance",
            Description = "Comprehensive routine maintenance covering software updates, driver checks, backup verification, temp cleanup, and dust filters.",
            IsBuiltIn = true,
            Items = new List<MaintenanceTemplateItem>
            {
                new() { TaskTitle = "Check Windows Updates", CategoryName = "Software & Updates", QuickAction = QuickActionType.WindowsUpdate, SortOrder = 1 },
                new() { TaskTitle = "Check Hardware Drivers", CategoryName = "Software & Updates", QuickAction = QuickActionType.DeviceManager, SortOrder = 2 },
                new() { TaskTitle = "Review Startup Applications", CategoryName = "Software & Updates", QuickAction = QuickActionType.TaskManager, SortOrder = 3 },
                new() { TaskTitle = "Windows Security & Antivirus Status", CategoryName = "Security & Integrity", QuickAction = QuickActionType.WindowsSecurity, SortOrder = 4 },
                new() { TaskTitle = "Verify Data Backup", CategoryName = "Security & Integrity", QuickAction = QuickActionType.None, SortOrder = 5 },
                new() { TaskTitle = "Check Disk Space & Storage Sense", CategoryName = "Storage & Performance", QuickAction = QuickActionType.StorageSense, SortOrder = 6 },
                new() { TaskTitle = "Safe Temporary Files Cleanup", CategoryName = "Storage & Performance", QuickAction = QuickActionType.TempFilesClean, SortOrder = 7 },
                new() { TaskTitle = "Clean Dust Filters", CategoryName = "Physical Hardware & Dust", QuickAction = QuickActionType.None, SortOrder = 8 },
                new() { TaskTitle = "Check Operating Temperatures & Fan Noise", CategoryName = "Physical Hardware & Dust", QuickAction = QuickActionType.ResourceMonitor, SortOrder = 9 },
                new() { TaskTitle = "Clean Monitor, Keyboard & Mouse", CategoryName = "Peripherals & Workstation", QuickAction = QuickActionType.None, SortOrder = 10 }
            }
        };

        var fullService = new MaintenanceTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Full Service",
            Description = "Complete digital service book inspection including all software, security, system file integrity, deep hardware dust cleaning, cables, and workstation peripherals.",
            IsBuiltIn = true,
            Items = new List<MaintenanceTemplateItem>
            {
                new() { TaskTitle = "Check Windows Updates", CategoryName = "Software & Updates", QuickAction = QuickActionType.WindowsUpdate, SortOrder = 1 },
                new() { TaskTitle = "Check Hardware Drivers", CategoryName = "Software & Updates", QuickAction = QuickActionType.DeviceManager, SortOrder = 2 },
                new() { TaskTitle = "Review Startup Applications", CategoryName = "Software & Updates", QuickAction = QuickActionType.TaskManager, SortOrder = 3 },
                new() { TaskTitle = "Update Web Browsers & Essential Software", CategoryName = "Software & Updates", QuickAction = QuickActionType.None, SortOrder = 4 },
                new() { TaskTitle = "Windows Security & Antivirus Status", CategoryName = "Security & Integrity", QuickAction = QuickActionType.WindowsSecurity, SortOrder = 5 },
                new() { TaskTitle = "Verify Data Backup", CategoryName = "Security & Integrity", QuickAction = QuickActionType.None, SortOrder = 6 },
                new() { TaskTitle = "Windows System Integrity Check (SFC / DISM)", CategoryName = "Security & Integrity", QuickAction = QuickActionType.SystemIntegrityGuide, SortOrder = 7 },
                new() { TaskTitle = "Check System Restore Points", CategoryName = "Security & Integrity", QuickAction = QuickActionType.None, SortOrder = 8 },
                new() { TaskTitle = "Check Disk Space & Storage Sense", CategoryName = "Storage & Performance", QuickAction = QuickActionType.StorageSense, SortOrder = 9 },
                new() { TaskTitle = "Safe Temporary Files Cleanup", CategoryName = "Storage & Performance", QuickAction = QuickActionType.TempFilesClean, SortOrder = 10 },
                new() { TaskTitle = "Clean Dust Filters", CategoryName = "Physical Hardware & Dust", QuickAction = QuickActionType.None, SortOrder = 11 },
                new() { TaskTitle = "Inspect Fans & Case Interior for Dust", CategoryName = "Physical Hardware & Dust", QuickAction = QuickActionType.None, SortOrder = 12 },
                new() { TaskTitle = "Check Operating Temperatures & Fan Noise", CategoryName = "Physical Hardware & Dust", QuickAction = QuickActionType.ResourceMonitor, SortOrder = 13 },
                new() { TaskTitle = "Inspect Visible Cables & Internal Connectors", CategoryName = "Physical Hardware & Dust", QuickAction = QuickActionType.None, SortOrder = 14 },
                new() { TaskTitle = "Clean PC Exterior & Chassis", CategoryName = "Peripherals & Workstation", QuickAction = QuickActionType.None, SortOrder = 15 },
                new() { TaskTitle = "Clean Monitor, Keyboard & Mouse", CategoryName = "Peripherals & Workstation", QuickAction = QuickActionType.None, SortOrder = 16 },
                new() { TaskTitle = "Check External USB Ports & Connectors", CategoryName = "Peripherals & Workstation", QuickAction = QuickActionType.None, SortOrder = 17 }
            }
        };

        return new List<MaintenanceTemplate> { quickCheck, regularMaintenance, fullService };
    }
}
