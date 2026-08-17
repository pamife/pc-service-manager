using Microsoft.EntityFrameworkCore;
using PcServiceManager.Core.Data;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.Core.Services;
using PcServiceManager.Infrastructure.Data;

namespace PcServiceManager.Infrastructure.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly AppDbContext _dbContext;
    private readonly IHardwareDiagnosticsService _hardwareDiagnostics;

    public MaintenanceService(AppDbContext dbContext, IHardwareDiagnosticsService hardwareDiagnostics)
    {
        _dbContext = dbContext;
        _hardwareDiagnostics = hardwareDiagnostics;
    }

    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

        // Seed Categories if empty
        if (!await _dbContext.Categories.AnyAsync(cancellationToken))
        {
            var categories = DefaultDataSeed.GetDefaultCategories();
            await _dbContext.Categories.AddRangeAsync(categories, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Seed Templates if empty
        if (!await _dbContext.Templates.AnyAsync(cancellationToken))
        {
            var templates = DefaultDataSeed.GetDefaultTemplates();
            await _dbContext.Templates.AddRangeAsync(templates, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Seed Settings if empty
        if (!await _dbContext.Settings.AnyAsync(cancellationToken))
        {
            var settings = new AppSettings
            {
                Id = 1,
                Theme = AppTheme.System,
                DueSoonDaysThreshold = 7,
                NotificationsEnabled = true,
                NotificationFrequency = "OnStartup",
                DefaultTechnicianName = Environment.UserName
            };
            await _dbContext.Settings.AddAsync(settings, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<PcAsset?> GetActivePcAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        if (settings.ActivePcId.HasValue)
        {
            var pc = await _dbContext.PcAssets
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Category)
                .Include(p => p.ServiceSessions)
                .FirstOrDefaultAsync(p => p.Id == settings.ActivePcId.Value, cancellationToken);

            if (pc != null) return pc;
        }

        // Fallback to default or first PC
        var defaultPc = await _dbContext.PcAssets
            .Include(p => p.Tasks)
                .ThenInclude(t => t.Category)
            .Include(p => p.ServiceSessions)
            .FirstOrDefaultAsync(p => p.IsDefault, cancellationToken)
            ?? await _dbContext.PcAssets
            .Include(p => p.Tasks)
                .ThenInclude(t => t.Category)
            .Include(p => p.ServiceSessions)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultPc != null && settings.ActivePcId != defaultPc.Id)
        {
            settings.ActivePcId = defaultPc.Id;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return defaultPc;
    }

    public async Task<List<PcAsset>> GetAllPcsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PcAssets
            .AsNoTracking()
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PcAsset> CreatePcAsync(string name, DeviceType deviceType, string? notes = null, string? defaultTechnician = null, CancellationToken cancellationToken = default)
    {
        // Query hardware specs safely
        var diag = await _hardwareDiagnostics.GetDiagnosticInfoAsync(cancellationToken);

        var isFirstPc = !await _dbContext.PcAssets.AnyAsync(cancellationToken);

        var pc = new PcAsset
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(name) ? diag.MachineName : name.Trim(),
            DeviceType = deviceType,
            Manufacturer = diag.Manufacturer,
            Model = diag.Model,
            OperatingSystem = diag.OsVersion,
            InstallDate = diag.InstallDate,
            CpuName = diag.CpuName,
            RamTotal = diag.TotalRam,
            GpuName = diag.GpuName,
            Motherboard = diag.Motherboard,
            BiosVersion = diag.BiosVersion,
            Hostname = diag.MachineName,
            StorageSummary = diag.Drives.Any() ? string.Join(", ", diag.Drives.Select(d => $"{d.Name} ({d.FreeSpaceGb} GB free / {d.TotalSizeGb} GB)")) : "Local Disks",
            Notes = notes,
            IsDefault = isFirstPc,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _dbContext.PcAssets.AddAsync(pc, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Pre-populate default maintenance tasks
        var defaultTasks = DefaultDataSeed.CreateDefaultTasksForPc(pc.Id, deviceType);
        var today = DateTime.UtcNow.Date;

        foreach (var task in defaultTasks)
        {
            task.NextDueDate = MaintenanceScheduleCalculator.CalculateNextDueDate(today, task.IntervalType, task.IntervalValue);
        }

        await _dbContext.Tasks.AddRangeAsync(defaultTasks, cancellationToken);

        // Update settings active PC
        var settings = await GetSettingsAsync(cancellationToken);
        if (isFirstPc || !settings.ActivePcId.HasValue)
        {
            settings.ActivePcId = pc.Id;
        }

        if (!string.IsNullOrWhiteSpace(defaultTechnician))
        {
            settings.DefaultTechnicianName = defaultTechnician.Trim();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return pc;
    }

    public async Task UpdatePcAsync(PcAsset pc, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.PcAssets.FindAsync(new object[] { pc.Id }, cancellationToken);
        if (existing != null)
        {
            existing.Name = pc.Name;
            existing.Manufacturer = pc.Manufacturer;
            existing.Model = pc.Model;
            existing.SerialNumber = pc.SerialNumber;
            existing.DeviceType = pc.DeviceType;
            existing.OperatingSystem = pc.OperatingSystem;
            existing.InstallDate = pc.InstallDate;
            existing.CpuName = pc.CpuName;
            existing.RamTotal = pc.RamTotal;
            existing.GpuName = pc.GpuName;
            existing.StorageSummary = pc.StorageSummary;
            existing.Notes = pc.Notes;
            existing.IsDefault = pc.IsDefault;
            existing.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeletePcAsync(Guid pcId, CancellationToken cancellationToken = default)
    {
        var pc = await _dbContext.PcAssets.FindAsync(new object[] { pcId }, cancellationToken);
        if (pc != null)
        {
            _dbContext.PcAssets.Remove(pc);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var settings = await GetSettingsAsync(cancellationToken);
            if (settings.ActivePcId == pcId)
            {
                var nextPc = await _dbContext.PcAssets.FirstOrDefaultAsync(cancellationToken);
                settings.ActivePcId = nextPc?.Id;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task SetActivePcAsync(Guid pcId, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        settings.ActivePcId = pcId;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<MaintenanceCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MaintenanceTask>> GetTasksForPcAsync(Guid pcId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .Include(t => t.Category)
            .Where(t => t.PcAssetId == pcId)
            .OrderBy(t => t.CategoryId)
            .ThenBy(t => t.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceTask> AddTaskAsync(MaintenanceTask task, CancellationToken cancellationToken = default)
    {
        if (!task.NextDueDate.HasValue && task.IntervalType != IntervalType.Disabled)
        {
            var baseDate = task.LastPerformedDate ?? DateTime.UtcNow;
            task.NextDueDate = MaintenanceScheduleCalculator.CalculateNextDueDate(baseDate, task.IntervalType, task.IntervalValue);
        }

        await _dbContext.Tasks.AddAsync(task, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task UpdateTaskAsync(MaintenanceTask task, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Tasks.FindAsync(new object[] { task.Id }, cancellationToken);
        if (existing != null)
        {
            existing.Title = task.Title;
            existing.CategoryId = task.CategoryId;
            existing.Description = task.Description;
            existing.DetailedInstructions = task.DetailedInstructions;
            existing.SafetyWarning = task.SafetyWarning;
            existing.IsAdvanced = task.IsAdvanced;
            existing.DeviceTypeFilter = task.DeviceTypeFilter;
            existing.IntervalType = task.IntervalType;
            existing.IntervalValue = task.IntervalValue;
            existing.IsEnabled = task.IsEnabled;
            existing.LastPerformedDate = task.LastPerformedDate;
            existing.NextDueDate = task.NextDueDate;
            existing.QuickAction = task.QuickAction;
            existing.QuickActionPayload = task.QuickActionPayload;
            existing.SortOrder = task.SortOrder;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks.FindAsync(new object[] { taskId }, cancellationToken);
        if (task != null)
        {
            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<MaintenanceTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Templates
            .Include(t => t.Items)
            .AsNoTracking()
            .OrderByDescending(t => t.IsBuiltIn)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceTemplate> AddTemplateAsync(MaintenanceTemplate template, CancellationToken cancellationToken = default)
    {
        await _dbContext.Templates.AddAsync(template, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await _dbContext.Templates
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template != null && !template.IsBuiltIn)
        {
            _dbContext.Templates.Remove(template);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ServiceSession> StartServiceSessionAsync(Guid pcId, string templateName, string? technician, CancellationToken cancellationToken = default)
    {
        var session = new ServiceSession
        {
            Id = Guid.NewGuid(),
            PcAssetId = pcId,
            TemplateName = templateName,
            StartTime = DateTime.UtcNow,
            PerformedBy = technician,
            Status = ServiceSessionStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.ServiceSessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<ServiceSession> CompleteServiceSessionAsync(Guid sessionId, string? overallNote, List<ServiceTaskResult> results, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.ServiceSessions
            .Include(s => s.TaskResults)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session == null) throw new InvalidOperationException($"Service session {sessionId} not found.");

        session.EndTime = DateTime.UtcNow;
        var duration = (int)(session.EndTime.Value - session.StartTime).TotalMinutes;
        session.DurationMinutes = Math.Max(1, duration);
        session.OverallNote = overallNote;
        session.Status = ServiceSessionStatus.Completed;

        session.TotalTasks = results.Count;
        session.CompletedCount = results.Count(r => r.Status == ServiceTaskStatus.Completed);
        session.SkippedCount = results.Count(r => r.Status == ServiceTaskStatus.Skipped);
        session.NeedsAttentionCount = results.Count(r => r.Status == ServiceTaskStatus.NeedsAttention);
        session.NotApplicableCount = results.Count(r => r.Status == ServiceTaskStatus.NotApplicable);

        // Add task results
        foreach (var res in results)
        {
            res.ServiceSessionId = session.Id;
            res.RecordedAt = DateTime.UtcNow;
            await _dbContext.ServiceTaskResults.AddAsync(res, cancellationToken);

            // Update underlying MaintenanceTask NextDueDate and LastPerformedDate if Completed
            if (res.MaintenanceTaskId.HasValue && res.Status == ServiceTaskStatus.Completed)
            {
                var task = await _dbContext.Tasks.FindAsync(new object[] { res.MaintenanceTaskId.Value }, cancellationToken);
                if (task != null)
                {
                    task.LastPerformedDate = session.StartTime;
                    task.NextDueDate = MaintenanceScheduleCalculator.CalculateNextDueDate(session.StartTime, task.IntervalType, task.IntervalValue);
                }
            }
        }

        // Update PC asset timestamp
        var pc = await _dbContext.PcAssets.FindAsync(new object[] { session.PcAssetId }, cancellationToken);
        if (pc != null)
        {
            pc.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task CancelServiceSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.ServiceSessions.FindAsync(new object[] { sessionId }, cancellationToken);
        if (session != null)
        {
            session.Status = ServiceSessionStatus.Cancelled;
            session.EndTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<ServiceSession>> GetServiceHistoryAsync(Guid pcId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceSessions
            .AsNoTracking()
            .Include(s => s.TaskResults)
            .Where(s => s.PcAssetId == pcId && s.Status == ServiceSessionStatus.Completed)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceSession?> GetServiceSessionDetailsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceSessions
            .AsNoTracking()
            .Include(s => s.PcAsset)
            .Include(s => s.TaskResults)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<AppSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _dbContext.Settings.FirstOrDefaultAsync(cancellationToken);
        if (settings == null)
        {
            settings = new AppSettings
            {
                Id = 1,
                Theme = AppTheme.System,
                DueSoonDaysThreshold = 7,
                NotificationsEnabled = true,
                NotificationFrequency = "OnStartup",
                DefaultTechnicianName = Environment.UserName
            };
            await _dbContext.Settings.AddAsync(settings, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        return settings;
    }

    public async Task UpdateSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Settings.FirstOrDefaultAsync(cancellationToken);
        if (existing != null)
        {
            existing.ActivePcId = settings.ActivePcId;
            existing.Theme = settings.Theme;
            existing.DueSoonDaysThreshold = settings.DueSoonDaysThreshold;
            existing.NotificationsEnabled = settings.NotificationsEnabled;
            existing.NotificationFrequency = settings.NotificationFrequency;
            existing.DefaultTechnicianName = settings.DefaultTechnicianName;
            existing.LastNotificationCheck = settings.LastNotificationCheck;
            existing.LastBackupDate = settings.LastBackupDate;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await InitializeDatabaseAsync(cancellationToken);
    }
}
