using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for TaskItem entities.
/// </summary>
public class TaskRepository : BaseRepository<TaskItem>, ITaskRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TaskRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<PagedResult<TaskItem>> GetByUserIdAsync(
        Guid userId,
        TaskItemStatus? status,
        TaskPriority? priority,
        Guid? projectId,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.Project)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        query = ApplyFilters(query, status, priority, projectId, dueDateFrom, dueDateTo);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc/>
    public async Task<PagedResult<TaskItem>> GetAllFilteredAsync(
        TaskItemStatus? status,
        TaskPriority? priority,
        Guid? projectId,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.Project)
            .Include(t => t.User)
            .AsQueryable();

        query = ApplyFilters(query, status, priority, projectId, dueDateFrom, dueDateTo);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc/>
    public async Task<TaskItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.Project)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    private static IQueryable<TaskItem> ApplyFilters(
        IQueryable<TaskItem> query,
        TaskItemStatus? status,
        TaskPriority? priority,
        Guid? projectId,
        DateTime? dueDateFrom,
        DateTime? dueDateTo)
    {
        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (dueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= dueDateFrom.Value);
        }

        if (dueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= dueDateTo.Value);
        }

        return query;
    }
}
