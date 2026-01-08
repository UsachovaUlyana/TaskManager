using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Tags;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

/// <summary>
/// Controller for tag operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsController"/> class.
    /// </summary>
    /// <param name="tagService">The tag service.</param>
    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>
    /// Gets all tags with pagination.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of tags.</returns>
    /// <response code="200">Returns the paginated list of tags.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TagDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<TagDto>>>> GetTags(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _tagService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<TagDto>>.Ok(result));
    }

    /// <summary>
    /// Gets a tag by ID.
    /// </summary>
    /// <param name="id">The tag ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tag details.</returns>
    /// <response code="200">Returns the tag details.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the tag is not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TagDto>>> GetTag(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tag = await _tagService.GetByIdAsync(id, cancellationToken);
        if (tag == null)
        {
            throw new NotFoundException("Tag", id);
        }

        return Ok(ApiResponse<TagDto>.Ok(tag));
    }

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    /// <param name="request">The create tag request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tag.</returns>
    /// <response code="201">Returns the created tag.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="409">If a tag with the same name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TagDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<TagDto>>> CreateTag(
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        var tag = await _tagService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, ApiResponse<TagDto>.Ok(tag));
    }

    /// <summary>
    /// Updates a tag.
    /// </summary>
    /// <param name="id">The tag ID.</param>
    /// <param name="request">The update tag request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated tag.</returns>
    /// <response code="200">Returns the updated tag.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the tag is not found.</response>
    /// <response code="409">If a tag with the same name already exists.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<TagDto>>> UpdateTag(
        Guid id,
        [FromBody] UpdateTagRequest request,
        CancellationToken cancellationToken)
    {
        var tag = await _tagService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TagDto>.Ok(tag));
    }

    /// <summary>
    /// Deletes a tag.
    /// </summary>
    /// <param name="id">The tag ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    /// <response code="204">The tag was deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the tag is not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _tagService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
