using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a task item in the system.
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the task.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the task title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the task status.
    /// </summary>
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    /// <summary>
    /// Gets or sets the task priority.
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// Gets or sets the due date for the task.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created the task.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user who created the task.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ID of the project this task belongs to.
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project this task belongs to.
    /// </summary>
    public Project? Project { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the task was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the task was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of task-tag associations.
    /// </summary>
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
