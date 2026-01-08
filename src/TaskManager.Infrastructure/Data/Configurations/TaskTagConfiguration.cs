using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the TaskTag entity (many-to-many).
/// </summary>
public class TaskTagConfiguration : IEntityTypeConfiguration<TaskTag>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TaskTag> builder)
    {
        builder.ToTable("task_tags");

        builder.HasKey(tt => new { tt.TaskId, tt.TagId });

        builder.Property(tt => tt.TaskId)
            .HasColumnName("task_id");

        builder.Property(tt => tt.TagId)
            .HasColumnName("tag_id");

        builder.HasIndex(tt => tt.TaskId);
        builder.HasIndex(tt => tt.TagId);
    }
}
