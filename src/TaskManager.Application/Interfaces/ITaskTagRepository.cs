using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Repository interface for TaskTag entities (Dapper-based for transactions).
/// </summary>
public interface ITaskTagRepository
{
    /// <summary>
    /// Assigns multiple tags to a task in a single transaction using Dapper.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="tagIds">The tag IDs to assign.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AssignTagsToTaskAsync(Guid taskId, IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all tags from a task using Dapper.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveAllTagsFromTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tags for a task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of tags assigned to the task.</returns>
    Task<IReadOnlyList<Tag>> GetTagsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
}
