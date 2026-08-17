using Microsoft.EntityFrameworkCore;
using PcServiceManager.Core.Entities;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<PcAsset> PcAssets => Set<PcAsset>();
    public DbSet<MaintenanceCategory> Categories => Set<MaintenanceCategory>();
    public DbSet<MaintenanceTask> Tasks => Set<MaintenanceTask>();
    public DbSet<MaintenanceTemplate> Templates => Set<MaintenanceTemplate>();
    public DbSet<MaintenanceTemplateItem> TemplateItems => Set<MaintenanceTemplateItem>();
    public DbSet<ServiceSession> ServiceSessions => Set<ServiceSession>();
    public DbSet<ServiceTaskResult> ServiceTaskResults => Set<ServiceTaskResult>();
    public DbSet<AppSettings> Settings => Set<AppSettings>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PcAsset configuration
        modelBuilder.Entity<PcAsset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DeviceType).HasConversion<string>();

            entity.HasMany(e => e.Tasks)
                  .WithOne(t => t.PcAsset)
                  .HasForeignKey(t => t.PcAssetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ServiceSessions)
                  .WithOne(s => s.PcAsset)
                  .HasForeignKey(s => s.PcAssetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // MaintenanceCategory configuration
        modelBuilder.Entity<MaintenanceCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // MaintenanceTask configuration
        modelBuilder.Entity<MaintenanceTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
            entity.Property(e => e.DeviceTypeFilter).HasConversion<string>();
            entity.Property(e => e.IntervalType).HasConversion<string>();
            entity.Property(e => e.QuickAction).HasConversion<string>();

            entity.HasOne(t => t.Category)
                  .WithMany(c => c.Tasks)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // MaintenanceTemplate configuration
        modelBuilder.Entity<MaintenanceTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasMany(t => t.Items)
                  .WithOne(i => i.MaintenanceTemplate)
                  .HasForeignKey(i => i.MaintenanceTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // MaintenanceTemplateItem configuration
        modelBuilder.Entity<MaintenanceTemplateItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskTitle).IsRequired().HasMaxLength(150);
            entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DefaultIntervalType).HasConversion<string>();
            entity.Property(e => e.QuickAction).HasConversion<string>();
        });

        // ServiceSession configuration
        modelBuilder.Entity<ServiceSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TemplateName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasMany(s => s.TaskResults)
                  .WithOne(r => r.ServiceSession)
                  .HasForeignKey(r => r.ServiceSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ServiceTaskResult configuration
        modelBuilder.Entity<ServiceTaskResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskTitle).IsRequired().HasMaxLength(150);
            entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>();
        });

        // AppSettings configuration
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Theme).HasConversion<string>();
        });
    }
}
