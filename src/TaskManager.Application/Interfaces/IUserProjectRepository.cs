using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Repository interface for UserProject entities.
/// </summary>
public interface IUserProjectRepository
{
    /// <summary>
    /// Adds a user to a project.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="role">The user's role in the project.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddUserToProjectAsync(Guid userId, Guid projectId, ProjectRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a user from a project.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveUserFromProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's role in a project.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user-project relationship if found; otherwise, null.</returns>
    Task<UserProject?> GetUserProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all members of a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of user-project relationships.</returns>
    Task<IReadOnlyList<UserProject>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default);
}
