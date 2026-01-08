namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a many-to-many relationship between tasks and tags.
/// </summary>
public class TaskTag
{
    /// <summary>
    /// Gets or sets the ID of the task.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the task.
    /// </summary>
    public TaskItem Task { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ID of the tag.
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    /// Gets or sets the tag.
    /// </summary>
    public Tag Tag { get; set; } = null!;
}
