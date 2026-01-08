using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Tags;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Interface for tag service.
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Gets all tags with pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of tags.</returns>
    Task<PagedResult<TagDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tag by ID.
    /// </summary>
    /// <param name="id">The tag ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tag DTO if found; otherwise, null.</returns>
    Task<TagDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    /// <param name="request">The create tag request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tag DTO.</returns>
    Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a tag.
    /// </summary>
    /// <param name="id">The tag ID.</param>
    /// <param name="request">The update tag request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated tag DTO.</returns>
    Task<TagDto> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a tag.
    /// </summary>
    /// <param name="id">The tag ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
