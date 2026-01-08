namespace TaskManager.Domain.Enums;

/// <summary>
/// Represents the status of a task.
/// </summary>
public enum TaskItemStatus
{
    /// <summary>
    /// Task is pending and not yet started.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Task is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task has been completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Task has been cancelled.
    /// </summary>
    Cancelled = 3
}
