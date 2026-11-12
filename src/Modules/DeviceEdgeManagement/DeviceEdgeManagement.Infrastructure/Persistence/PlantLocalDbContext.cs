using Microsoft.EntityFrameworkCore;

namespace DeviceEdgeManagement.Infrastructure.Persistence;

internal sealed class PlantLocalDbContext : DbContext
{
    public PlantLocalDbContext(DbContextOptions<PlantLocalDbContext> options)
        : base(options)
    {
    }

    public DbSet<PlantAreaRow> Areas => Set<PlantAreaRow>();

    public DbSet<PlantThresholdRow> Thresholds => Set<PlantThresholdRow>();

    public DbSet<PlantDeviceRow> Devices => Set<PlantDeviceRow>();

    public DbSet<PlantCredentialRow> Credentials => Set<PlantCredentialRow>();

    public DbSet<LocalReadingRow> Readings => Set<LocalReadingRow>();

    public DbSet<LocalExposureRow> Exposures => Set<LocalExposureRow>();

    public DbSet<LocalAlertRow> Alerts => Set<LocalAlertRow>();

    public DbSet<LocalActuatorRow> Actuators => Set<LocalActuatorRow>();

    public DbSet<LocalActionRow> Actions => Set<LocalActionRow>();

    public DbSet<OutboundRow> Outbound => Set<OutboundRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlantAreaRow>(area =>
        {
            area.ToTable("plant_areas");
            area.HasKey(entry => entry.AreaId);
            area.Property(entry => entry.AreaId).HasColumnName("area_id");
            area.Property(entry => entry.Name).HasColumnName("name").IsRequired();
            area.Property(entry => entry.Location).HasColumnName("location").IsRequired();
            area.Property(entry => entry.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<PlantThresholdRow>(threshold =>
        {
            threshold.ToTable("plant_thresholds");
            threshold.HasKey(entry => entry.AreaId);
            threshold.Property(entry => entry.AreaId).HasColumnName("area_id");
            threshold.Property(entry => entry.Co2LimitPpm).HasColumnName("co2_limit_ppm");
            threshold.Property(entry => entry.NoiseLimitDb).HasColumnName("noise_limit_db");
            threshold.Property(entry => entry.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<PlantDeviceRow>(device =>
        {
            device.ToTable("plant_devices");
            device.HasKey(entry => entry.DeviceId);
            device.Property(entry => entry.DeviceId).HasColumnName("device_id");
            device.Property(entry => entry.AreaId).HasColumnName("area_id");
            device.Property(entry => entry.Kind).HasColumnName("kind").IsRequired();
        });

        modelBuilder.Entity<PlantCredentialRow>(credential =>
        {
            credential.ToTable("plant_credentials");
            credential.HasKey(entry => entry.DeviceId);
            credential.Property(entry => entry.DeviceId).HasColumnName("device_id");
            credential.Property(entry => entry.SecretHash).HasColumnName("secret_hash").IsRequired();
            credential.Property(entry => entry.ExpiresAt).HasColumnName("expires_at");
            credential.Property(entry => entry.RevokedAt).HasColumnName("revoked_at");
        });

        modelBuilder.Entity<LocalReadingRow>(reading =>
        {
            reading.ToTable("local_readings");
            reading.HasKey(entry => new { entry.AreaId, entry.Measure });
            reading.Property(entry => entry.AreaId).HasColumnName("area_id");
            reading.Property(entry => entry.Measure).HasColumnName("measure");
            reading.Property(entry => entry.DeviceId).HasColumnName("device_id").IsRequired();
            reading.Property(entry => entry.Ppm).HasColumnName("ppm");
            reading.Property(entry => entry.Decibels).HasColumnName("db");
            reading.Property(entry => entry.State).HasColumnName("state");
            reading.Property(entry => entry.RecordedAt).HasColumnName("recorded_at");
        });

        modelBuilder.Entity<LocalExposureRow>(exposure =>
        {
            exposure.ToTable("local_exposure");
            exposure.HasKey(entry => entry.AreaId);
            exposure.Property(entry => entry.AreaId).HasColumnName("area_id");
            exposure.Property(entry => entry.Severity).HasColumnName("severity").IsRequired();
            exposure.Property(entry => entry.Highlighted).HasColumnName("highlighted");
        });

        modelBuilder.Entity<LocalAlertRow>(alert =>
        {
            alert.ToTable("local_alerts");
            alert.HasKey(entry => entry.Id);
            alert.Property(entry => entry.Id).HasColumnName("id");
            alert.Property(entry => entry.AreaId).HasColumnName("area_id");
            alert.Property(entry => entry.RaisedAt).HasColumnName("raised_at");
            alert.Property(entry => entry.WithdrawnAt).HasColumnName("withdrawn_at");
        });

        modelBuilder.Entity<LocalActuatorRow>(actuator =>
        {
            actuator.ToTable("local_actuators");
            actuator.HasKey(entry => entry.Id);
            actuator.Property(entry => entry.Id).HasColumnName("id");
            actuator.Property(entry => entry.AreaId).HasColumnName("area_id");
            actuator.Property(entry => entry.Type).HasColumnName("type").IsRequired();
            actuator.Property(entry => entry.RunState).HasColumnName("run_state").IsRequired();
            actuator.Property(entry => entry.Mode).HasColumnName("mode").IsRequired();
            actuator.Property(entry => entry.LastChangedAt).HasColumnName("last_changed_at");
            actuator.HasIndex(entry => new { entry.AreaId, entry.Type }).IsUnique();
        });

        modelBuilder.Entity<LocalActionRow>(action =>
        {
            action.ToTable("local_actions");
            action.HasKey(entry => entry.Id);
            action.Property(entry => entry.Id).HasColumnName("id");
            action.Property(entry => entry.AreaId).HasColumnName("area_id");
            action.Property(entry => entry.Type).HasColumnName("type").IsRequired();
            action.Property(entry => entry.Succeeded).HasColumnName("succeeded");
            action.Property(entry => entry.RecordedAt).HasColumnName("recorded_at");
            action.Property(entry => entry.AttemptedRunState).HasColumnName("attempted_run_state").IsRequired();
            action.Property(entry => entry.Synced).HasColumnName("synced");
        });

        modelBuilder.Entity<OutboundRow>(outbound =>
        {
            outbound.ToTable("outbound_queue");
            outbound.HasKey(entry => entry.Id);
            outbound.Property(entry => entry.Id).HasColumnName("id").ValueGeneratedOnAdd();
            outbound.Property(entry => entry.Kind).HasColumnName("kind").IsRequired();
            outbound.Property(entry => entry.Body).HasColumnName("body").IsRequired();
            outbound.Property(entry => entry.AreaId).HasColumnName("area_id");
            outbound.Property(entry => entry.CreatedAt).HasColumnName("created_at");
        });
    }
}

internal sealed class PlantAreaRow
{
    public Guid AreaId { get; set; }

    public string Name { get; set; } = "";

    public string Location { get; set; } = "";

    public DateTimeOffset UpdatedAt { get; set; }
}

internal sealed class PlantThresholdRow
{
    public Guid AreaId { get; set; }

    public int Co2LimitPpm { get; set; }

    public int NoiseLimitDb { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

internal sealed class PlantDeviceRow
{
    public string DeviceId { get; set; } = "";

    public Guid AreaId { get; set; }

    public string Kind { get; set; } = "";
}

internal sealed class PlantCredentialRow
{
    public string DeviceId { get; set; } = "";

    public string SecretHash { get; set; } = "";

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}

internal sealed class LocalReadingRow
{
    public Guid AreaId { get; set; }

    public string Measure { get; set; } = "";

    public string DeviceId { get; set; } = "";

    public int? Ppm { get; set; }

    public int? Decibels { get; set; }

    public string? State { get; set; }

    public DateTimeOffset RecordedAt { get; set; }
}

internal sealed class LocalExposureRow
{
    public Guid AreaId { get; set; }

    public string Severity { get; set; } = "";

    public bool Highlighted { get; set; }
}

internal sealed class LocalAlertRow
{
    public Guid Id { get; set; }

    public Guid AreaId { get; set; }

    public DateTimeOffset RaisedAt { get; set; }

    public DateTimeOffset? WithdrawnAt { get; set; }
}

internal sealed class LocalActuatorRow
{
    public Guid Id { get; set; }

    public Guid AreaId { get; set; }

    public string Type { get; set; } = "";

    public string RunState { get; set; } = "";

    public string Mode { get; set; } = "";

    public DateTimeOffset LastChangedAt { get; set; }
}

internal sealed class LocalActionRow
{
    public Guid Id { get; set; }

    public Guid AreaId { get; set; }

    public string Type { get; set; } = "";

    public bool Succeeded { get; set; }

    public DateTimeOffset RecordedAt { get; set; }

    public string AttemptedRunState { get; set; } = "";

    public bool Synced { get; set; }
}

internal sealed class OutboundRow
{
    public long Id { get; set; }

    public string Kind { get; set; } = "";

    public string Body { get; set; } = "";

    public Guid? AreaId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
