using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Tags;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

/// <summary>
/// Tag service implementation.
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagService"/> class.
    /// </summary>
    /// <param name="tagRepository">The tag repository.</param>
    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<TagDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _tagRepository.GetPagedAsync(null, page, pageSize, cancellationToken);

        return new PagedResult<TagDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    /// <inheritdoc/>
    public async Task<TagDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
        return tag == null ? null : MapToDto(tag);
    }

    /// <inheritdoc/>
    public async Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken = default)
    {
        var existingTag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingTag != null)
        {
            throw new ConflictException($"Tag with name '{request.Name}' already exists.");
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Color = request.Color,
            CreatedAt = DateTime.UtcNow
        };

        await _tagRepository.AddAsync(tag, cancellationToken);

        return MapToDto(tag);
    }

    /// <inheritdoc/>
    public async Task<TagDto> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tag", id);

        // Check if name is already taken by another tag
        var existingTag = await _tagRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingTag != null && existingTag.Id != id)
        {
            throw new ConflictException($"Tag with name '{request.Name}' already exists.");
        }

        tag.Name = request.Name;
        tag.Color = request.Color;

        await _tagRepository.UpdateAsync(tag, cancellationToken);

        return MapToDto(tag);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tag", id);

        await _tagRepository.DeleteAsync(tag, cancellationToken);
    }

    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            CreatedAt = tag.CreatedAt
        };
    }
}
