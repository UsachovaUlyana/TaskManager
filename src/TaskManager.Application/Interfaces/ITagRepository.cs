using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Repository interface for Tag entities.
/// </summary>
public interface ITagRepository : IRepository<Tag>
{
    /// <summary>
    /// Gets a tag by name.
    /// </summary>
    /// <param name="name">The tag name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tag if found; otherwise, null.</returns>
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tags by their IDs.
    /// </summary>
    /// <param name="ids">The tag IDs.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of tags.</returns>
    Task<IReadOnlyList<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
