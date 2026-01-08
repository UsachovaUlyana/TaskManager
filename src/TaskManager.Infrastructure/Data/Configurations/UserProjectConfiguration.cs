using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the UserProject entity (many-to-many).
/// </summary>
public class UserProjectConfiguration : IEntityTypeConfiguration<UserProject>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserProject> builder)
    {
        builder.ToTable("user_projects");

        builder.HasKey(up => new { up.UserId, up.ProjectId });

        builder.Property(up => up.UserId)
            .HasColumnName("user_id");

        builder.Property(up => up.ProjectId)
            .HasColumnName("project_id");

        builder.Property(up => up.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.Property(up => up.JoinedAt)
            .HasColumnName("joined_at")
            .IsRequired();

        builder.HasIndex(up => up.UserId);
        builder.HasIndex(up => up.ProjectId);
    }
}
