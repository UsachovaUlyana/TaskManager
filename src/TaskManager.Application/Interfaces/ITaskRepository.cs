using TaskManager.Application.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Repository interface for TaskItem entities.
/// </summary>
public interface ITaskRepository : IRepository<TaskItem>
{
    /// <summary>
    /// Gets tasks by user ID with pagination and filtering.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="priority">Optional priority filter.</param>
    /// <param name="projectId">Optional project ID filter.</param>
    /// <param name="dueDateFrom">Optional due date from filter.</param>
    /// <param name="dueDateTo">Optional due date to filter.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated result of tasks.</returns>
    Task<PagedResult<TaskItem>> GetByUserIdAsync(
        Guid userId,
        TaskItemStatus? status,
        TaskPriority? priority,
        Guid? projectId,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tasks with pagination and filtering.
    /// </summary>
    /// <param name="status">Optional status filter.</param>
    /// <param name="priority">Optional priority filter.</param>
    /// <param name="projectId">Optional project ID filter.</param>
    /// <param name="dueDateFrom">Optional due date from filter.</param>
    /// <param name="dueDateTo">Optional due date to filter.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated result of tasks.</returns>
    Task<PagedResult<TaskItem>> GetAllFilteredAsync(
        TaskItemStatus? status,
        TaskPriority? priority,
        Guid? projectId,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by ID with related data (tags, project).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task if found; otherwise, null.</returns>
    Task<TaskItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
