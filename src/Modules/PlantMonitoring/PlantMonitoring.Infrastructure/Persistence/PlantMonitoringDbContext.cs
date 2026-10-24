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
    }
}
