using TaskManager.Application.Common;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Repository interface for Project entities.
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// Gets projects by user ID (projects the user participates in).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated result of projects.</returns>
    Task<PagedResult<Project>> GetByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project by ID with related data (members).
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project if found; otherwise, null.</returns>
    Task<Project?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is a member of a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user is a member; otherwise, false.</returns>
    Task<bool> IsUserMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}
