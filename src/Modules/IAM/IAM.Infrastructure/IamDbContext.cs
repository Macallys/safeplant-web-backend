using IAM.Domain;
using Microsoft.EntityFrameworkCore;

namespace IAM.Infrastructure;

public sealed class IamDbContext : DbContext
{
    public IamDbContext(DbContextOptions<IamDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> Accounts => Set<UserAccount>();

    public DbSet<Session> Sessions => Set<Session>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(account =>
        {
            account.ToTable("user_accounts");
            account.HasKey(entry => entry.Id);
            account.Property(entry => entry.Id).HasColumnName("id");
            account.Property(entry => entry.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
            account.HasIndex(entry => entry.Email).IsUnique();
            account.Property(entry => entry.PasswordHash).HasColumnName("password_hash").IsRequired();
            account.Property(entry => entry.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(32);
            account.Property(entry => entry.Enabled).HasColumnName("enabled");
        });

        modelBuilder.Entity<Session>(session =>
        {
            session.ToTable("sessions");
            session.HasKey(entry => entry.Id);
            session.Property(entry => entry.Id).HasColumnName("id");
            session.Property(entry => entry.AccountId).HasColumnName("account_id");
            session.Property(entry => entry.Channel).HasColumnName("channel").HasConversion<string>().HasMaxLength(16);
            session.Property(entry => entry.ExpiresAt).HasColumnName("expires_at");
        });
    }
}
