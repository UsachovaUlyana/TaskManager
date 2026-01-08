using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Tag entities.
/// </summary>
public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TagRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }
}
