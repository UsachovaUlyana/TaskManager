using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Projects;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Interface for project service.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Gets all projects with pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of projects.</returns>
    Task<PagedResult<ProjectDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets projects by user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of projects.</returns>
    Task<PagedResult<ProjectDto>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project by ID.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project DTO if found; otherwise, null.</returns>
    Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="request">The create project request.</param>
    /// <param name="userId">The ID of the user creating the project.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created project DTO.</returns>
    Task<ProjectDto> CreateAsync(CreateProjectRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="request">The update project request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated project DTO.</returns>
    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a member to a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="request">The add member request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddMemberAsync(Guid projectId, AddMemberRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a member from a project.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="userId">The user ID to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}
