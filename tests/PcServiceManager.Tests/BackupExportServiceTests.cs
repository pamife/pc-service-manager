using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PcServiceManager.Core.Enums;
using PcServiceManager.Infrastructure.Data;
using PcServiceManager.Infrastructure.Services;
using Xunit;

namespace PcServiceManager.Tests;

public class BackupExportServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _dbContext;
    private readonly MaintenanceService _maintService;
    private readonly BackupExportService _backupService;

    public BackupExportServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _maintService = new MaintenanceService(_dbContext, new MockHardwareDiagnosticsService());
        _backupService = new BackupExportService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task ExportAndImport_ShouldRestoreDataSuccessfully()
    {
        await _maintService.InitializeDatabaseAsync();
        var pc = await _maintService.CreatePcAsync("BackupTest-PC", DeviceType.Desktop, "My backup rig");

        // Export JSON
        var json = await _backupService.ExportFullBackupJsonAsync();
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("BackupTest-PC");

        // Export CSV
        var csv = await _backupService.ExportServiceHistoryCsvAsync(pc.Id);
        csv.Should().NotBeNull();
        csv.Should().Contain("Date,PC Name,Template");

        // Test Import in fresh DB
        using var newConn = new SqliteConnection("DataSource=:memory:");
        newConn.Open();
        var newOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(newConn)
            .Options;

        using var newDb = new AppDbContext(newOptions);
        await newDb.Database.EnsureCreatedAsync();

        var newBackupService = new BackupExportService(newDb);
        var importSuccess = await newBackupService.ImportFullBackupJsonAsync(json);

        importSuccess.Should().BeTrue();

        var restoredPcs = await newDb.PcAssets.ToListAsync();
        restoredPcs.Should().HaveCount(1);
        restoredPcs[0].Name.Should().Be("BackupTest-PC");

        var restoredTasks = await newDb.Tasks.ToListAsync();
        restoredTasks.Should().NotBeEmpty();
    }
}
