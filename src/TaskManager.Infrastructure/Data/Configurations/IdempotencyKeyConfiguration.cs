using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the IdempotencyKey entity.
/// </summary>
public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("idempotency_keys");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property(i => i.Key)
            .HasColumnName("key")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(i => i.RequestPath)
            .HasColumnName("request_path")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.RequestBodyHash)
            .HasColumnName("request_body_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(i => i.ResponseStatusCode)
            .HasColumnName("response_status_code")
            .IsRequired();

        builder.Property(i => i.ResponseBody)
            .HasColumnName("response_body");

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.HasIndex(i => i.Key)
            .IsUnique();

        builder.HasIndex(i => i.ExpiresAt);
    }
}
