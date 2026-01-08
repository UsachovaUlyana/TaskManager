using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Project entities.
/// </summary>
public class ProjectRepository : BaseRepository<Project>, IProjectRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ProjectRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<PagedResult<Project>> GetByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(p => p.UserProjects)
            .ThenInclude(up => up.User)
            .Where(p => p.UserProjects.Any(up => up.UserId == userId));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Project>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc/>
    public async Task<Project?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.UserProjects)
            .ThenInclude(up => up.User)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> IsUserMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.UserProjects
            .AnyAsync(up => up.ProjectId == projectId && up.UserId == userId, cancellationToken);
    }
}
