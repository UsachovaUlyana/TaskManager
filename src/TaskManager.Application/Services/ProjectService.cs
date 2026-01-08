using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

/// <summary>
/// Project service implementation.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserProjectRepository _userProjectRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProjectService> _logger;
    private const string CacheKeyPrefix = "projects:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectService"/> class.
    /// </summary>
    public ProjectService(
        IProjectRepository projectRepository,
        IUserProjectRepository userProjectRepository,
        ICacheService cacheService,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _userProjectRepository = userProjectRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<ProjectDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}all:{page}:{pageSize}";
        
        var cached = await _cacheService.GetAsync<PagedResult<ProjectDto>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var result = await _projectRepository.GetPagedAsync(null, page, pageSize, cancellationToken);

        var projects = new List<ProjectDto>();
        foreach (var project in result.Items)
        {
            var projectWithMembers = await _projectRepository.GetByIdWithMembersAsync(project.Id, cancellationToken);
            if (projectWithMembers != null)
            {
                projects.Add(MapToDto(projectWithMembers));
            }
        }

        var response = new PagedResult<ProjectDto>
        {
            Items = projects,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        await _cacheService.SetAsync(cacheKey, response, CacheExpiration, cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<ProjectDto>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}user:{userId}:{page}:{pageSize}";
        
        var cached = await _cacheService.GetAsync<PagedResult<ProjectDto>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var result = await _projectRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);

        var response = new PagedResult<ProjectDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        await _cacheService.SetAsync(cacheKey, response, CacheExpiration, cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public async Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";
        
        var cached = await _cacheService.GetAsync<ProjectDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var project = await _projectRepository.GetByIdWithMembersAsync(id, cancellationToken);
        if (project == null)
        {
            return null;
        }

        var dto = MapToDto(project);
        await _cacheService.SetAsync(cacheKey, dto, CacheExpiration, cancellationToken);

        return dto;
    }

    /// <inheritdoc/>
    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project, cancellationToken);

        // Add creator as owner
        await _userProjectRepository.AddUserToProjectAsync(userId, project.Id, ProjectRole.Owner, cancellationToken);

        // Invalidate caches
        await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}*", cancellationToken);

        var createdProject = await _projectRepository.GetByIdWithMembersAsync(project.Id, cancellationToken);
        return MapToDto(createdProject!);
    }

    /// <inheritdoc/>
    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Project", id);

        project.Name = request.Name;
        project.Description = request.Description;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, cancellationToken);

        // Invalidate caches
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}all:*", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}user:*", cancellationToken);

        var updatedProject = await _projectRepository.GetByIdWithMembersAsync(project.Id, cancellationToken);
        return MapToDto(updatedProject!);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Project", id);

        await _projectRepository.DeleteAsync(project, cancellationToken);

        // Invalidate caches
        await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}*", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddMemberAsync(Guid projectId, AddMemberRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project", projectId);

        var isMember = await _projectRepository.IsUserMemberAsync(projectId, request.UserId, cancellationToken);
        if (isMember)
        {
            throw new ConflictException("User is already a member of this project.");
        }

        if (!Enum.TryParse<ProjectRole>(request.Role, true, out var role))
        {
            role = ProjectRole.Member;
        }

        await _userProjectRepository.AddUserToProjectAsync(request.UserId, projectId, role, cancellationToken);

        // Invalidate caches
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}{projectId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}user:*", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project", projectId);

        await _userProjectRepository.RemoveUserFromProjectAsync(userId, projectId, cancellationToken);

        // Invalidate caches
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}{projectId}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}user:*", cancellationToken);
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Members = project.UserProjects.Select(up => new ProjectMemberDto
            {
                UserId = up.UserId,
                Username = up.User?.Username ?? string.Empty,
                Role = up.Role.ToString(),
                JoinedAt = up.JoinedAt
            }).ToList()
        };
    }
}
