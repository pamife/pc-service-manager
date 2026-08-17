using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Interfaces;
using PcServiceManager.Core.Models;
using PcServiceManager.Infrastructure.Data;
using PcServiceManager.Infrastructure.Services;
using Xunit;

namespace PcServiceManager.Tests;

public class MockHardwareDiagnosticsService : IHardwareDiagnosticsService
{
    public Task<PcDiagnosticInfo> GetDiagnosticInfoAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PcDiagnosticInfo
        {
            MachineName = "Test-PC",
            OsVersion = "Windows 11 Pro 23H2",
            CpuName = "Intel Core i7-14700K",
            TotalRam = "32 GB",
            Manufacturer = "ASUS",
            Model = "Custom PC",
            Drives = new List<DriveInfoModel>
            {
                new() { Name = "C:\\", TotalSize = 1000000000000, AvailableFreeSpace = 600000000000 }
            }
        });
    }
}

public class MaintenanceServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _dbContext;
    private readonly MaintenanceService _service;

    public MaintenanceServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _service = new MaintenanceService(_dbContext, new MockHardwareDiagnosticsService());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task InitializeDatabaseAsync_ShouldSeedCategoriesAndTemplates()
    {
        await _service.InitializeDatabaseAsync();

        var categories = await _service.GetCategoriesAsync();
        var templates = await _service.GetTemplatesAsync();
        var settings = await _service.GetSettingsAsync();

        categories.Should().NotBeEmpty();
        categories.Should().HaveCount(5);
        templates.Should().HaveCount(3);
        settings.Should().NotBeNull();
        settings.DueSoonDaysThreshold.Should().Be(7);
    }

    [Fact]
    public async Task CreatePcAsync_ShouldCreatePcAndSeedDefaultTasks()
    {
        await _service.InitializeDatabaseAsync();

        var pc = await _service.CreatePcAsync("Brother-PC", DeviceType.Desktop, "Living room gaming rig", "Technician Paul");

        pc.Should().NotBeNull();
        pc.Name.Should().Be("Brother-PC");
        pc.DeviceType.Should().Be(DeviceType.Desktop);

        var tasks = await _service.GetTasksForPcAsync(pc.Id);
        tasks.Should().NotBeEmpty();
        tasks.Should().Contain(t => t.Title == "Check Windows Updates");
        tasks.Should().Contain(t => t.Title == "Clean Dust Filters");
    }

    [Fact]
    public async Task CompleteServiceSessionAsync_ShouldUpdateTaskDatesAndRecordHistory()
    {
        await _service.InitializeDatabaseAsync();
        var pc = await _service.CreatePcAsync("Office-PC", DeviceType.Desktop);
        var tasks = await _service.GetTasksForPcAsync(pc.Id);
        var taskToComplete = tasks.First(t => t.Title == "Check Windows Updates");

        var session = await _service.StartServiceSessionAsync(pc.Id, "Quick Check", "Paul");
        session.Should().NotBeNull();

        var results = new List<ServiceTaskResult>
        {
            new()
            {
                MaintenanceTaskId = taskToComplete.Id,
                TaskTitle = taskToComplete.Title,
                CategoryName = "Software & Updates",
                Status = ServiceTaskStatus.Completed,
                Notes = "All updates installed cleanly"
            },
            new()
            {
                TaskTitle = "Check Temperatures",
                CategoryName = "Physical Hardware & Dust",
                Status = ServiceTaskStatus.Skipped,
                Notes = "No load test needed"
            }
        };

        var completed = await _service.CompleteServiceSessionAsync(session.Id, "General inspection done", results);

        completed.Status.Should().Be(ServiceSessionStatus.Completed);
        completed.CompletedCount.Should().Be(1);
        completed.SkippedCount.Should().Be(1);
        completed.DurationMinutes.Should().BeGreaterThanOrEqualTo(1);

        // Verify task dates were updated
        var updatedTask = await _dbContext.Tasks.FindAsync(taskToComplete.Id);
        updatedTask.Should().NotBeNull();
        updatedTask!.LastPerformedDate.Should().NotBeNull();
        updatedTask.NextDueDate.Should().NotBeNull();
        updatedTask.NextDueDate.Should().BeAfter(DateTime.UtcNow.Date);

        // Verify service history query
        var history = await _service.GetServiceHistoryAsync(pc.Id);
        history.Should().HaveCount(1);
        history[0].TaskResults.Should().HaveCount(2);
    }
}
