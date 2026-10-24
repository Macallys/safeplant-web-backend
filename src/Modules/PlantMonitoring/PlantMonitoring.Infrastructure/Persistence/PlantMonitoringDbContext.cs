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
    }
}
