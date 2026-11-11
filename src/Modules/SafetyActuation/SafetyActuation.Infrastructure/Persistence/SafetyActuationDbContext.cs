using Microsoft.EntityFrameworkCore;
using SafetyActuation.Domain.Entities;

namespace SafetyActuation.Infrastructure.Persistence;

public sealed class SafetyActuationDbContext : DbContext
{
    public const string MigrationsHistoryTable = "__ef_migrations_safety_actuation";

    public SafetyActuationDbContext(DbContextOptions<SafetyActuationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExposureState> Exposures => Set<ExposureState>();

    public DbSet<ActuatorState> Actuators => Set<ActuatorState>();

    public DbSet<EnvironmentalAlert> Alerts => Set<EnvironmentalAlert>();

    public DbSet<ActuatorAction> Actions => Set<ActuatorAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<ExposureState>(exposure =>
        {
            exposure.ToTable("exposure_states");
            exposure.HasKey(entry => entry.AreaId);
            exposure.Property(entry => entry.AreaId).HasColumnName("area_id");
            exposure.Property(entry => entry.Severity).HasColumnName("severity").HasMaxLength(16).IsRequired();
            exposure.Property(entry => entry.Highlighted).HasColumnName("highlighted");
        });

        modelBuilder.Entity<ActuatorState>(actuator =>
        {
            actuator.ToTable("actuator_states");
            actuator.HasKey(entry => entry.Id);
            actuator.Property(entry => entry.Id).HasColumnName("id");
            actuator.Property(entry => entry.AreaId).HasColumnName("area_id");
            actuator.Property(entry => entry.Type).HasColumnName("type").HasMaxLength(16).IsRequired();
            actuator.Property(entry => entry.RunState).HasColumnName("run_state").HasMaxLength(16).IsRequired();
            actuator.Property(entry => entry.Mode).HasColumnName("mode").HasMaxLength(16).IsRequired();
            actuator.Property(entry => entry.LastChangedAt).HasColumnName("last_changed_at");
            actuator.HasIndex(entry => new { entry.AreaId, entry.Type }).IsUnique();
        });

        modelBuilder.Entity<EnvironmentalAlert>(alert =>
        {
            alert.ToTable("environmental_alerts");
            alert.HasKey(entry => entry.Id);
            alert.Property(entry => entry.Id).HasColumnName("id");
            alert.Property(entry => entry.AreaId).HasColumnName("area_id");
            alert.Property(entry => entry.RaisedAt).HasColumnName("raised_at");
            alert.Property(entry => entry.WithdrawnAt).HasColumnName("withdrawn_at");
            alert.HasIndex(entry => new { entry.AreaId, entry.RaisedAt });
        });

        modelBuilder.Entity<ActuatorAction>(action =>
        {
            action.ToTable("actuator_actions");
            action.HasKey(entry => entry.Id);
            action.Property(entry => entry.Id).HasColumnName("id");
            action.Property(entry => entry.AreaId).HasColumnName("area_id");
            action.Property(entry => entry.Type).HasColumnName("type").HasMaxLength(16).IsRequired();
            action.Property(entry => entry.Succeeded).HasColumnName("succeeded");
            action.Property(entry => entry.RecordedAt).HasColumnName("recorded_at");
            action.HasIndex(entry => new { entry.AreaId, entry.RecordedAt });
        });
    }
}
