using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Users;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Interface for user service.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets all users with pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of users.</returns>
    Task<PagedResult<UserDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user DTO if found; otherwise, null.</returns>
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user (admin only).
    /// </summary>
    /// <param name="request">The create user request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user DTO.</returns>
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
