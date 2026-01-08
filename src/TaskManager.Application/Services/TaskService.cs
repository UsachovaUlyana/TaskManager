using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Tags;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

/// <summary>
/// Task service implementation.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskTagRepository _taskTagRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<TaskService> _logger;
    private const string CacheKeyPrefix = "tasks:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskService"/> class.
    /// </summary>
    public TaskService(
        ITaskRepository taskRepository,
        ITaskTagRepository taskTagRepository,
        ICacheService cacheService,
        ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _taskTagRepository = taskTagRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<TaskDto>> GetTasksAsync(TaskFilterRequest filter, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}list:{userId}:{filter.Status}:{filter.Priority}:{filter.ProjectId}:{filter.Page}:{filter.PageSize}";
        
        var cached = await _cacheService.GetAsync<PagedResult<TaskDto>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Returning cached tasks for key {CacheKey}", cacheKey);
            return cached;
        }

        TaskItemStatus? status = null;
        TaskPriority? priority = null;

        if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<TaskItemStatus>(filter.Status, true, out var s))
        {
            status = s;
        }

        if (!string.IsNullOrEmpty(filter.Priority) && Enum.TryParse<TaskPriority>(filter.Priority, true, out var p))
        {
            priority = p;
        }

        PagedResult<TaskItem> result;

        if (userId.HasValue)
        {
            result = await _taskRepository.GetByUserIdAsync(
                userId.Value, status, priority, filter.ProjectId,
                filter.DueDateFrom, filter.DueDateTo, filter.Page, filter.PageSize, cancellationToken);
        }
        else
        {
            result = await _taskRepository.GetAllFilteredAsync(
                status, priority, filter.ProjectId,
                filter.DueDateFrom, filter.DueDateTo, filter.Page, filter.PageSize, cancellationToken);
        }

        var response = new PagedResult<TaskDto>
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
    public async Task<TaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";
        
        var cached = await _cacheService.GetAsync<TaskDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var task = await _taskRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (task == null)
        {
            return null;
        }

        var dto = MapToDto(task);
        await _cacheService.SetAsync(cacheKey, dto, CacheExpiration, cancellationToken);

        return dto;
    }

    /// <inheritdoc/>
    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<TaskItemStatus>(request.Status, true, out var status))
        {
            status = TaskItemStatus.Pending;
        }

        if (!Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
        {
            priority = TaskPriority.Medium;
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = status,
            Priority = priority,
            DueDate = request.DueDate,
            UserId = userId,
            ProjectId = request.ProjectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task, cancellationToken);

        if (request.TagIds.Count > 0)
        {
            await _taskTagRepository.AssignTagsToTaskAsync(task.Id, request.TagIds, cancellationToken);
        }

        // Invalidate list caches
        await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}list:*", cancellationToken);

        var createdTask = await _taskRepository.GetByIdWithDetailsAsync(task.Id, cancellationToken);
        return MapToDto(createdTask!);
    }

    /// <inheritdoc/>
    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Task", id);

        if (!Enum.TryParse<TaskItemStatus>(request.Status, true, out var status))
        {
            status = task.Status;
        }

        if (!Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
        {
            priority = task.Priority;
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = status;
        task.Priority = priority;
        task.DueDate = request.DueDate;
        task.ProjectId = request.ProjectId;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task, cancellationToken);

        // Update tags
        await _taskTagRepository.AssignTagsToTaskAsync(task.Id, request.TagIds, cancellationToken);

        // Invalidate caches
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}list:*", cancellationToken);

        var updatedTask = await _taskRepository.GetByIdWithDetailsAsync(task.Id, cancellationToken);
        return MapToDto(updatedTask!);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Task", id);

        await _taskRepository.DeleteAsync(task, cancellationToken);

        // Invalidate caches
        await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}", cancellationToken);
        await _cacheService.RemoveByPatternAsync($"{CacheKeyPrefix}list:*", cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> IsOwnerAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        return task?.UserId == userId;
    }

    private static TaskDto MapToDto(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            DueDate = task.DueDate,
            UserId = task.UserId,
            Username = task.User?.Username,
            ProjectId = task.ProjectId,
            ProjectName = task.Project?.Name,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            Tags = task.TaskTags.Select(tt => new TagDto
            {
                Id = tt.Tag.Id,
                Name = tt.Tag.Name,
                Color = tt.Tag.Color,
                CreatedAt = tt.Tag.CreatedAt
            }).ToList()
        };
    }
}
