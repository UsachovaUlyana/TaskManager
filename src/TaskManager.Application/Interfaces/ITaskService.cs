using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Interface for task service.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Gets tasks with filtering and pagination.
    /// </summary>
    /// <param name="filter">The filter request.</param>
    /// <param name="userId">Optional user ID for filtering by owner.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of tasks.</returns>
    Task<PagedResult<TaskDto>> GetTasksAsync(TaskFilterRequest filter, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task DTO if found; otherwise, null.</returns>
    Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="request">The create task request.</param>
    /// <param name="userId">The ID of the user creating the task.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created task DTO.</returns>
    Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The update task request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated task DTO.</returns>
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user owns a task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user owns the task; otherwise, false.</returns>
    Task<bool> IsOwnerAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
}
