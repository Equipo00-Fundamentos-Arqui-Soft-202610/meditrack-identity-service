using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd();

            entity.Property(u => u.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(u => u.Dni)
                .HasMaxLength(20);

            entity.Property(u => u.DateOfBirth);

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .IsRequired();
        });

        builder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_message");

            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id)
                .HasConversion(g => g.ToByteArray(), b => new Guid(b))
                .HasColumnType("binary(16)");

            entity.Property(m => m.EventType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(m => m.Payload)
                .HasColumnType("json")
                .IsRequired();

            entity.Property(m => m.OccurredAtUtc)
                .IsRequired();

            entity.Property(m => m.LastError)
                .HasMaxLength(500);

            entity.HasIndex(m => m.ProcessedAtUtc);
        });
    }
}
