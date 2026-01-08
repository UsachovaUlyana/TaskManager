using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserProject entities.
/// </summary>
public class UserProjectRepository : IUserProjectRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProjectRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task AddUserToProjectAsync(Guid userId, Guid projectId, ProjectRole role, CancellationToken cancellationToken = default)
    {
        var userProject = new UserProject
        {
            UserId = userId,
            ProjectId = projectId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        await _context.UserProjects.AddAsync(userProject, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveUserFromProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var userProject = await _context.UserProjects
            .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId, cancellationToken);

        if (userProject != null)
        {
            _context.UserProjects.Remove(userProject);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<UserProject?> GetUserProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.UserProjects
            .Include(up => up.User)
            .Include(up => up.Project)
            .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserProject>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.UserProjects
            .Include(up => up.User)
            .Where(up => up.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }
}
