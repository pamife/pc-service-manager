using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Models;
using PcServiceManager.Core.Services;
using PcServiceManager.Infrastructure.Data;
using PcServiceManager.Infrastructure.Services;
using Xunit;

namespace PcServiceManager.Tests;

public class ServiceWorkflowTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _dbContext;
    private readonly MaintenanceService _service;
    private readonly BackupExportService _backupService;

    public ServiceWorkflowTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _service = new MaintenanceService(_dbContext, new MockHardwareDiagnosticsService());
        _backupService = new BackupExportService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task ServiceWorkflow_ShouldHandleMultiplePcsAndSwitching()
    {
        await _service.InitializeDatabaseAsync();

        var pc1 = await _service.CreatePcAsync("LivingRoom-PC", DeviceType.Desktop);
        var pc2 = await _service.CreatePcAsync("Sister-Laptop", DeviceType.Laptop);

        var allPcs = await _service.GetAllPcsAsync();
        allPcs.Should().HaveCount(2);

        var active = await _service.GetActivePcAsync();
        active.Should().NotBeNull();
        active!.Id.Should().Be(pc1.Id);

        await _service.SetActivePcAsync(pc2.Id);
        var newActive = await _service.GetActivePcAsync();
        newActive!.Id.Should().Be(pc2.Id);
    }

    [Fact]
    public async Task ServiceWorkflow_LaptopShouldHaveLaptopSpecificTasks()
    {
        await _service.InitializeDatabaseAsync();

        var laptop = await _service.CreatePcAsync("Office-Laptop", DeviceType.Laptop);
        var tasks = await _service.GetTasksForPcAsync(laptop.Id);

        tasks.Should().Contain(t => t.Title == "Inspect Laptop Display Hinges & Bezel");
        tasks.Should().Contain(t => t.Title == "Laptop Battery Health Check");
    }

    [Fact]
    public async Task ServiceWorkflow_CancellingSession_ShouldSetCancelledStatus()
    {
        await _service.InitializeDatabaseAsync();
        var pc = await _service.CreatePcAsync("Test-PC", DeviceType.Desktop);

        var session = await _service.StartServiceSessionAsync(pc.Id, "Quick Check", "Paul");
        session.Status.Should().Be(ServiceSessionStatus.InProgress);

        await _service.CancelServiceSessionAsync(session.Id);

        var updated = await _dbContext.ServiceSessions.FindAsync(session.Id);
        updated!.Status.Should().Be(ServiceSessionStatus.Cancelled);
    }

    [Fact]
    public async Task ServiceWorkflow_ExportServiceReportText_ShouldGenerateFormattedReport()
    {
        await _service.InitializeDatabaseAsync();
        var pc = await _service.CreatePcAsync("Gaming-Rig", DeviceType.Desktop);
        var session = await _service.StartServiceSessionAsync(pc.Id, "Full Service", "Paul");

        var results = new List<ServiceTaskResult>
        {
            new()
            {
                TaskTitle = "Clean Dust Filters",
                CategoryName = "Physical Hardware & Dust",
                Status = ServiceTaskStatus.Completed,
                Notes = "Cleaned front mesh with compressed air"
            },
            new()
            {
                TaskTitle = "Check Operating Temperatures",
                CategoryName = "Physical Hardware & Dust",
                Status = ServiceTaskStatus.NeedsAttention,
                Notes = "GPU running hot at 88C under load"
            }
        };

        await _service.CompleteServiceSessionAsync(session.Id, "Thorough service completed", results);

        var report = await _backupService.ExportServiceReportTextAsync(session.Id);
        report.Should().NotBeNullOrWhiteSpace();
        report.Should().Contain("PC SERVICE LOGBOOK REPORT");
        report.Should().Contain("Gaming-Rig");
        report.Should().Contain("Clean Dust Filters");
        report.Should().Contain("GPU running hot at 88C");
        report.Should().Contain("SUMMARY METRICS:  1 Completed | 0 Skipped | 1 Needs Attention");
    }

    [Fact]
    public void FormattingHelpers_ShouldProduceHumanReadableOutputs()
    {
        MaintenanceScheduleCalculator.FormatInterval(IntervalType.Days, 14).Should().Be("Every 14 days");
        MaintenanceScheduleCalculator.FormatInterval(IntervalType.Weeks, 1).Should().Be("Every week");
        MaintenanceScheduleCalculator.FormatInterval(IntervalType.Months, 3).Should().Be("Every 3 months");
        MaintenanceScheduleCalculator.FormatInterval(IntervalType.Years, 1).Should().Be("Every year");
        MaintenanceScheduleCalculator.FormatInterval(IntervalType.OneTime, 1).Should().Be("One-time check");
        MaintenanceScheduleCalculator.FormatInterval(IntervalType.Disabled, 1).Should().Be("Disabled");

        MaintenanceScheduleCalculator.FormatDaysRemaining(0).Should().Be("Due today");
        MaintenanceScheduleCalculator.FormatDaysRemaining(1).Should().Be("Due tomorrow");
        MaintenanceScheduleCalculator.FormatDaysRemaining(5).Should().Be("In 5 days");
        MaintenanceScheduleCalculator.FormatDaysRemaining(-3).Should().Be("3 days overdue");
        MaintenanceScheduleCalculator.FormatDaysRemaining(null).Should().Be("Not scheduled");
    }
}
