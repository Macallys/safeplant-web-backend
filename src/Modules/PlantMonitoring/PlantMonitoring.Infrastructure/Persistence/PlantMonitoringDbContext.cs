using PlantMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace PlantMonitoring.Infrastructure.Persistence;

public sealed class PlantMonitoringDbContext : DbContext
{
    public const string MigrationsHistoryTable = "__ef_migrations_plant_monitoring";

    public PlantMonitoringDbContext(DbContextOptions<PlantMonitoringDbContext> options)
        : base(options)
    {
    }

    public DbSet<IndustrialArea> Areas => Set<IndustrialArea>();

    public DbSet<AreaThresholds> Thresholds => Set<AreaThresholds>();

    public DbSet<AreaDevice> Devices => Set<AreaDevice>();

    public DbSet<CarbonDioxideReading> CarbonDioxideReadings => Set<CarbonDioxideReading>();

    public DbSet<NoiseReading> NoiseReadings => Set<NoiseReading>();

    public DbSet<PresenceChange> PresenceChanges => Set<PresenceChange>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<IndustrialArea>(area =>
        {
            area.ToTable("industrial_areas");
            area.HasKey(entry => entry.Id);
            area.Property(entry => entry.Id).HasColumnName("id");
            area.Property(entry => entry.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            area.Property(entry => entry.Location).HasColumnName("location").HasMaxLength(200).IsRequired();
            area.Property(entry => entry.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<AreaThresholds>(threshold =>
        {
            threshold.ToTable("area_thresholds");
            threshold.HasKey(entry => entry.AreaId);
            threshold.Property(entry => entry.AreaId).HasColumnName("area_id");
            threshold.Property(entry => entry.Co2LimitPpm).HasColumnName("co2_limit_ppm");
            threshold.Property(entry => entry.NoiseLimitDb).HasColumnName("noise_limit_db");
            threshold.Property(entry => entry.UpdatedAt).HasColumnName("updated_at");
            threshold.HasOne<IndustrialArea>()
                .WithOne()
                .HasForeignKey<AreaThresholds>(entry => entry.AreaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AreaDevice>(device =>
        {
            device.ToTable("area_devices");
            device.HasKey(entry => entry.Id);
            device.Property(entry => entry.Id).HasColumnName("id");
            device.Property(entry => entry.AreaId).HasColumnName("area_id");
            device.Property(entry => entry.DeviceId).HasColumnName("device_id").HasMaxLength(64).IsRequired();
            device.HasIndex(entry => entry.DeviceId).IsUnique();
            device.Property(entry => entry.Kind).HasColumnName("kind").HasMaxLength(16).IsRequired();
            device.HasOne<IndustrialArea>()
                .WithMany()
                .HasForeignKey(entry => entry.AreaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CarbonDioxideReading>(reading =>
        {
            reading.ToTable("carbon_dioxide_readings");
            reading.HasKey(entry => entry.Id);
            reading.Property(entry => entry.Id).HasColumnName("id");
            reading.Property(entry => entry.DeviceId).HasColumnName("device_id").HasMaxLength(64).IsRequired();
            reading.Property(entry => entry.AreaId).HasColumnName("area_id");
            reading.Property(entry => entry.Ppm).HasColumnName("ppm");
            reading.Property(entry => entry.RecordedAt).HasColumnName("recorded_at");
            reading.HasIndex(entry => new { entry.AreaId, entry.RecordedAt });
            reading.HasOne<IndustrialArea>()
                .WithMany()
                .HasForeignKey(entry => entry.AreaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NoiseReading>(reading =>
        {
            reading.ToTable("noise_readings");
            reading.HasKey(entry => entry.Id);
            reading.Property(entry => entry.Id).HasColumnName("id");
            reading.Property(entry => entry.DeviceId).HasColumnName("device_id").HasMaxLength(64).IsRequired();
            reading.Property(entry => entry.AreaId).HasColumnName("area_id");
            reading.Property(entry => entry.Db).HasColumnName("db");
            reading.Property(entry => entry.RecordedAt).HasColumnName("recorded_at");
            reading.HasIndex(entry => new { entry.AreaId, entry.RecordedAt });
            reading.HasOne<IndustrialArea>()
                .WithMany()
                .HasForeignKey(entry => entry.AreaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PresenceChange>(reading =>
        {
            reading.ToTable("presence_changes");
            reading.HasKey(entry => entry.Id);
            reading.Property(entry => entry.Id).HasColumnName("id");
            reading.Property(entry => entry.DeviceId).HasColumnName("device_id").HasMaxLength(64).IsRequired();
            reading.Property(entry => entry.AreaId).HasColumnName("area_id");
            reading.Property(entry => entry.State).HasColumnName("state").HasMaxLength(16).IsRequired();
            reading.Property(entry => entry.RecordedAt).HasColumnName("recorded_at");
            reading.HasIndex(entry => new { entry.AreaId, entry.RecordedAt });
            reading.HasOne<IndustrialArea>()
                .WithMany()
                .HasForeignKey(entry => entry.AreaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
