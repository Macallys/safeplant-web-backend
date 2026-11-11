using DeviceEdgeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceEdgeManagement.Infrastructure.Persistence;

public sealed class DeviceEdgeDbContext : DbContext
{
    public const string MigrationsHistoryTable = "__ef_migrations_device_edge";

    public DeviceEdgeDbContext(DbContextOptions<DeviceEdgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<DeviceCredential> Credentials => Set<DeviceCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<DeviceCredential>(credential =>
        {
            credential.ToTable("device_credentials");
            credential.HasKey(entry => entry.DeviceId);
            credential.Property(entry => entry.DeviceId).HasColumnName("device_id").HasMaxLength(64);
            credential.Property(entry => entry.SecretHash).HasColumnName("secret_hash").IsRequired();
            credential.Property(entry => entry.IssuedAt).HasColumnName("issued_at");
            credential.Property(entry => entry.ExpiresAt).HasColumnName("expires_at");
            credential.Property(entry => entry.RevokedAt).HasColumnName("revoked_at");
        });
    }
}
