using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.Core.Models;
using PcServiceManager.Infrastructure.Data;

namespace PcServiceManager.Infrastructure.Services;

public class BackupExportService : IBackupExportService
{
    private readonly AppDbContext _dbContext;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public BackupExportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> ExportFullBackupJsonAsync(Guid? pcAssetId = null, CancellationToken cancellationToken = default)
    {
        var backup = new FullBackupDto
        {
            AppVersion = "1.0.0",
            ExportDate = DateTime.UtcNow,
            ExportedBy = Environment.UserName,
            Categories = await _dbContext.Categories.AsNoTracking().ToListAsync(cancellationToken),
            Templates = await _dbContext.Templates.AsNoTracking().Include(t => t.Items).ToListAsync(cancellationToken),
            Settings = await _dbContext.Settings.AsNoTracking().FirstOrDefaultAsync(cancellationToken)
        };

        if (pcAssetId.HasValue)
        {
            backup.PcAssets = await _dbContext.PcAssets
                .AsNoTracking()
                .Where(p => p.Id == pcAssetId.Value)
                .ToListAsync(cancellationToken);

            backup.Tasks = await _dbContext.Tasks
                .AsNoTracking()
                .Where(t => t.PcAssetId == pcAssetId.Value)
                .ToListAsync(cancellationToken);

            backup.ServiceSessions = await _dbContext.ServiceSessions
                .AsNoTracking()
                .Where(s => s.PcAssetId == pcAssetId.Value)
                .Include(s => s.TaskResults)
                .ToListAsync(cancellationToken);
        }
        else
        {
            backup.PcAssets = await _dbContext.PcAssets.AsNoTracking().ToListAsync(cancellationToken);
            backup.Tasks = await _dbContext.Tasks.AsNoTracking().ToListAsync(cancellationToken);
            backup.ServiceSessions = await _dbContext.ServiceSessions
                .AsNoTracking()
                .Include(s => s.TaskResults)
                .ToListAsync(cancellationToken);
        }

        return JsonSerializer.Serialize(backup, JsonOptions);
    }

    public async Task<bool> ImportFullBackupJsonAsync(string jsonContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jsonContent)) return false;

        try
        {
            var backup = JsonSerializer.Deserialize<FullBackupDto>(jsonContent, JsonOptions);
            if (backup == null || (!backup.PcAssets.Any() && !backup.Tasks.Any() && !backup.ServiceSessions.Any()))
            {
                return false;
            }

            // Using transaction for safe atomic restore
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Import / Upsert Categories
            if (backup.Categories != null && backup.Categories.Any())
            {
                foreach (var cat in backup.Categories)
                {
                    var existing = await _dbContext.Categories.FindAsync(new object[] { cat.Id }, cancellationToken);
                    if (existing == null)
                    {
                        await _dbContext.Categories.AddAsync(cat, cancellationToken);
                    }
                    else
                    {
                        existing.Name = cat.Name;
                        existing.Icon = cat.Icon;
                        existing.SortOrder = cat.SortOrder;
                        existing.Description = cat.Description;
                    }
                }
            }

            // Import / Upsert PcAssets
            if (backup.PcAssets != null && backup.PcAssets.Any())
            {
                foreach (var pc in backup.PcAssets)
                {
                    var existing = await _dbContext.PcAssets.FindAsync(new object[] { pc.Id }, cancellationToken);
                    if (existing == null)
                    {
                        // Detach navigation properties to avoid EF duplicate tracking issues during batch import
                        pc.Tasks = new List<MaintenanceTask>();
                        pc.ServiceSessions = new List<ServiceSession>();
                        await _dbContext.PcAssets.AddAsync(pc, cancellationToken);
                    }
                    else
                    {
                        existing.Name = pc.Name;
                        existing.Manufacturer = pc.Manufacturer;
                        existing.Model = pc.Model;
                        existing.SerialNumber = pc.SerialNumber;
                        existing.DeviceType = pc.DeviceType;
                        existing.OperatingSystem = pc.OperatingSystem;
                        existing.CpuName = pc.CpuName;
                        existing.RamTotal = pc.RamTotal;
                        existing.GpuName = pc.GpuName;
                        existing.StorageSummary = pc.StorageSummary;
                        existing.Notes = pc.Notes;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Import / Upsert Tasks
            if (backup.Tasks != null && backup.Tasks.Any())
            {
                foreach (var task in backup.Tasks)
                {
                    var existing = await _dbContext.Tasks.FindAsync(new object[] { task.Id }, cancellationToken);
                    if (existing == null)
                    {
                        task.PcAsset = null;
                        task.Category = null;
                        await _dbContext.Tasks.AddAsync(task, cancellationToken);
                    }
                    else
                    {
                        existing.Title = task.Title;
                        existing.Description = task.Description;
                        existing.DetailedInstructions = task.DetailedInstructions;
                        existing.SafetyWarning = task.SafetyWarning;
                        existing.IsAdvanced = task.IsAdvanced;
                        existing.IntervalType = task.IntervalType;
                        existing.IntervalValue = task.IntervalValue;
                        existing.IsEnabled = task.IsEnabled;
                        existing.LastPerformedDate = task.LastPerformedDate;
                        existing.NextDueDate = task.NextDueDate;
                        existing.QuickAction = task.QuickAction;
                    }
                }
            }

            // Import / Upsert Service Sessions & Results
            if (backup.ServiceSessions != null && backup.ServiceSessions.Any())
            {
                foreach (var session in backup.ServiceSessions)
                {
                    var existing = await _dbContext.ServiceSessions
                        .Include(s => s.TaskResults)
                        .FirstOrDefaultAsync(s => s.Id == session.Id, cancellationToken);

                    if (existing == null)
                    {
                        session.PcAsset = null;
                        var results = session.TaskResults?.ToList() ?? new List<ServiceTaskResult>();
                        session.TaskResults = new List<ServiceTaskResult>();
                        await _dbContext.ServiceSessions.AddAsync(session, cancellationToken);

                        foreach (var res in results)
                        {
                            res.ServiceSession = null;
                            res.ServiceSessionId = session.Id;
                            await _dbContext.ServiceTaskResults.AddAsync(res, cancellationToken);
                        }
                    }
                }
            }

            // Import / Upsert Templates
            if (backup.Templates != null && backup.Templates.Any())
            {
                foreach (var template in backup.Templates)
                {
                    var existing = await _dbContext.Templates
                        .Include(t => t.Items)
                        .FirstOrDefaultAsync(t => t.Id == template.Id, cancellationToken);

                    if (existing == null)
                    {
                        var items = template.Items?.ToList() ?? new List<MaintenanceTemplateItem>();
                        template.Items = new List<MaintenanceTemplateItem>();
                        await _dbContext.Templates.AddAsync(template, cancellationToken);

                        foreach (var item in items)
                        {
                            item.MaintenanceTemplate = null;
                            item.MaintenanceTemplateId = template.Id;
                            await _dbContext.TemplateItems.AddAsync(item, cancellationToken);
                        }
                    }
                }
            }

            // Settings
            if (backup.Settings != null)
            {
                var existingSettings = await _dbContext.Settings.FirstOrDefaultAsync(cancellationToken);
                if (existingSettings == null)
                {
                    await _dbContext.Settings.AddAsync(backup.Settings, cancellationToken);
                }
                else
                {
                    existingSettings.Theme = backup.Settings.Theme;
                    existingSettings.DueSoonDaysThreshold = backup.Settings.DueSoonDaysThreshold;
                    existingSettings.NotificationsEnabled = backup.Settings.NotificationsEnabled;
                    existingSettings.NotificationFrequency = backup.Settings.NotificationFrequency;
                    existingSettings.DefaultTechnicianName = backup.Settings.DefaultTechnicianName;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> ExportServiceHistoryCsvAsync(Guid pcAssetId, CancellationToken cancellationToken = default)
    {
        var sessions = await _dbContext.ServiceSessions
            .AsNoTracking()
            .Include(s => s.PcAsset)
            .Where(s => s.PcAssetId == pcAssetId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Date,PC Name,Template,Duration (min),Performed By,Status,Completed,Skipped,Needs Attention,Notes");

        foreach (var s in sessions)
        {
            var dateStr = s.StartTime.ToString("yyyy-MM-dd HH:mm");
            var pcName = EscapeCsv(s.PcAsset?.Name ?? "PC");
            var template = EscapeCsv(s.TemplateName);
            var duration = s.DurationMinutes.ToString();
            var technician = EscapeCsv(s.PerformedBy ?? "N/A");
            var status = s.Status.ToString();
            var completed = s.CompletedCount;
            var skipped = s.SkippedCount;
            var needsAttention = s.NeedsAttentionCount;
            var note = EscapeCsv(s.OverallNote ?? string.Empty);

            sb.AppendLine($"{dateStr},{pcName},{template},{duration},{technician},{status},{completed},{skipped},{needsAttention},{note}");
        }

        return sb.ToString();
    }

    public async Task<string> ExportServiceReportTextAsync(Guid serviceSessionId, CancellationToken cancellationToken = default)
    {
        var session = await _dbContext.ServiceSessions
            .AsNoTracking()
            .Include(s => s.PcAsset)
            .Include(s => s.TaskResults)
            .FirstOrDefaultAsync(s => s.Id == serviceSessionId, cancellationToken);

        if (session == null) return "Service session record not found.";

        var sb = new StringBuilder();
        sb.AppendLine("================================================================================");
        sb.AppendLine("                           PC SERVICE LOGBOOK REPORT                            ");
        sb.AppendLine("================================================================================");
        sb.AppendLine();
        sb.AppendLine($"PC Name:            {session.PcAsset?.Name ?? "PC"}");
        sb.AppendLine($"Device Model:       {session.PcAsset?.Manufacturer} {session.PcAsset?.Model}");
        sb.AppendLine($"Operating System:   {session.PcAsset?.OperatingSystem}");
        sb.AppendLine($"Service Date:       {session.StartTime:dddd, MMMM dd, yyyy HH:mm}");
        sb.AppendLine($"Duration:           {session.DurationMinutes} minutes");
        sb.AppendLine($"Service Type:       {session.TemplateName}");
        sb.AppendLine($"Performed By:       {session.PerformedBy ?? "Technician"}");
        sb.AppendLine($"Status:             {session.Status}");
        sb.AppendLine();
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine($"SUMMARY METRICS:  {session.CompletedCount} Completed | {session.SkippedCount} Skipped | {session.NeedsAttentionCount} Needs Attention");
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine();
        sb.AppendLine("MAINTENANCE CHECKLIST & OUTCOMES:");
        sb.AppendLine();

        int index = 1;
        foreach (var task in session.TaskResults.OrderBy(r => r.CategoryName).ThenBy(r => r.TaskTitle))
        {
            var statusSymbol = task.Status switch
            {
                Core.Enums.ServiceTaskStatus.Completed => "[✓ COMPLETED]",
                Core.Enums.ServiceTaskStatus.Skipped => "[⚠ SKIPPED  ]",
                Core.Enums.ServiceTaskStatus.NeedsAttention => "[✕ ATTENTION]",
                Core.Enums.ServiceTaskStatus.NotApplicable => "[— N/A      ]",
                _ => "[           ]"
            };

            sb.AppendLine($"  {index:D2}. {statusSymbol}  [{task.CategoryName}] {task.TaskTitle}");
            if (!string.IsNullOrWhiteSpace(task.Notes))
            {
                sb.AppendLine($"      Observation / Notes: {task.Notes}");
            }
            index++;
        }

        sb.AppendLine();
        sb.AppendLine("--------------------------------------------------------------------------------");
        sb.AppendLine("GENERAL TECHNICIAN NOTES:");
        sb.AppendLine(string.IsNullOrWhiteSpace(session.OverallNote) ? "No general notes recorded." : session.OverallNote);
        sb.AppendLine("================================================================================");

        return sb.ToString();
    }

    private static string EscapeCsv(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
