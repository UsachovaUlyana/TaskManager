using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

/// <summary>
/// Controller for project operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IProjectRepository _projectRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectsController"/> class.
    /// </summary>
    /// <param name="projectService">The project service.</param>
    /// <param name="projectRepository">The project repository.</param>
    public ProjectsController(IProjectService projectService, IProjectRepository projectRepository)
    {
        _projectService = projectService;
        _projectRepository = projectRepository;
    }

    /// <summary>
    /// Gets projects with pagination.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of projects.</returns>
    /// <remarks>
    /// - Admin and API Key users can see all projects.
    /// - Regular users can only see projects they participate in.
    /// </remarks>
    /// <response code="200">Returns the paginated list of projects.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProjectDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectDto>>>> GetProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        PagedResult<ProjectDto> result;

        if (User.IsInRole("Admin") || User.IsInRole("ApiKey"))
        {
            result = await _projectService.GetAllAsync(page, pageSize, cancellationToken);
        }
        else
        {
            var userId = GetCurrentUserId();
            result = await _projectService.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        }

        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    /// <summary>
    /// Gets a project by ID.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The project details.</returns>
    /// <response code="200">Returns the project details.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user doesn't have access to this project.</response>
    /// <response code="404">If the project is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(
        Guid id,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);
        if (project == null)
        {
            throw new NotFoundException("Project", id);
        }

        // Check access for regular users
        if (!User.IsInRole("Admin") && !User.IsInRole("ApiKey"))
        {
            var userId = GetCurrentUserId();
            var isMember = await _projectRepository.IsUserMemberAsync(id, userId, cancellationToken);
            if (!isMember)
            {
                throw new ForbiddenException("You don't have access to this project.");
            }
        }

        return Ok(ApiResponse<ProjectDto>.Ok(project));
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="request">The create project request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created project.</returns>
    /// <response code="201">Returns the created project.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var project = await _projectService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, ApiResponse<ProjectDto>.Ok(project));
    }

    /// <summary>
    /// Updates a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="request">The update project request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated project.</returns>
    /// <response code="200">Returns the updated project.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user doesn't have permission to update this project.</response>
    /// <response code="404">If the project is not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(project));
    }

    /// <summary>
    /// Deletes a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The project was deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user doesn't have permission to delete this project.</response>
    /// <response code="404">If the project is not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _projectService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Adds a member to a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="request">The add member request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The member was added successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the project is not found.</response>
    /// <response code="409">If the user is already a member.</response>
    [HttpPost("{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMember(
        Guid id,
        [FromBody] AddMemberRequest request,
        CancellationToken cancellationToken)
    {
        await _projectService.AddMemberAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Removes a member from a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="userId">The user ID to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The member was removed successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the project is not found.</response>
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await _projectService.RemoveMemberAsync(id, userId, cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
