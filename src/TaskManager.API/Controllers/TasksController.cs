using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

/// <summary>
/// Controller for task operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TasksController"/> class.
    /// </summary>
    /// <param name="taskService">The task service.</param>
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Gets tasks with filtering and pagination.
    /// </summary>
    /// <param name="filter">The filter parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of tasks.</returns>
    /// <remarks>
    /// - Admin users can see all tasks.
    /// - Regular users can only see their own tasks.
    /// - API Key users can see all tasks.
    /// </remarks>
    /// <response code="200">Returns the paginated list of tasks.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TaskDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<TaskDto>>>> GetTasks(
        [FromQuery] TaskFilterRequest filter,
        CancellationToken cancellationToken)
    {
        Guid? userId = null;
        
        // Regular users can only see their own tasks
        if (!User.IsInRole("Admin") && !User.IsInRole("ApiKey"))
        {
            userId = GetCurrentUserId();
        }

        var result = await _taskService.GetTasksAsync(filter, userId, cancellationToken);
        return Ok(ApiResponse<PagedResult<TaskDto>>.Ok(result));
    }

    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task details.</returns>
    /// <response code="200">Returns the task details.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user doesn't have access to this task.</response>
    /// <response code="404">If the task is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetTask(
        Guid id,
        CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        if (task == null)
        {
            throw new NotFoundException("Task", id);
        }

        // Check access for regular users
        if (!User.IsInRole("Admin") && !User.IsInRole("ApiKey"))
        {
            var userId = GetCurrentUserId();
            if (task.UserId != userId)
            {
                throw new ForbiddenException("You don't have access to this task.");
            }
        }

        return Ok(ApiResponse<TaskDto>.Ok(task));
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="request">The create task request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created task.</returns>
    /// <response code="201">Returns the created task.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, ApiResponse<TaskDto>.Ok(task));
    }

    /// <summary>
    /// Updates a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The update task request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated task.</returns>
    /// <remarks>
    /// - Admin users can update any task.
    /// - Regular users can only update their own tasks.
    /// - API Key users can update any task.
    /// </remarks>
    /// <response code="200">Returns the updated task.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user doesn't have access to this task.</response>
    /// <response code="404">If the task is not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTask(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        // Check access for regular users
        if (!User.IsInRole("Admin") && !User.IsInRole("ApiKey"))
        {
            var userId = GetCurrentUserId();
            var isOwner = await _taskService.IsOwnerAsync(id, userId, cancellationToken);
            if (!isOwner)
            {
                throw new ForbiddenException("You can only update your own tasks.");
            }
        }

        var task = await _taskService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TaskDto>.Ok(task));
    }

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    /// <remarks>
    /// - Admin users can delete any task.
    /// - Regular users can only delete their own tasks.
    /// - API Key users cannot delete tasks.
    /// </remarks>
    /// <response code="204">The task was deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user doesn't have permission to delete this task.</response>
    /// <response code="404">If the task is not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(
        Guid id,
        CancellationToken cancellationToken)
    {
        // API Key users cannot delete tasks
        if (User.IsInRole("ApiKey"))
        {
            throw new ForbiddenException("API Key users cannot delete tasks.");
        }

        // Check access for regular users
        if (!User.IsInRole("Admin"))
        {
            var userId = GetCurrentUserId();
            var isOwner = await _taskService.IsOwnerAsync(id, userId, cancellationToken);
            if (!isOwner)
            {
                throw new ForbiddenException("You can only delete your own tasks.");
            }
        }

        await _taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
