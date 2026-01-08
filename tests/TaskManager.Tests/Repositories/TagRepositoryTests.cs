using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Tests.Repositories;

/// <summary>
/// Unit tests for TagRepository.
/// </summary>
public class TagRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TagRepository _repository;

    public TagRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new TagRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task AddAsync_ShouldAddTag()
    {
        // Arrange
        var tag = CreateTestTag();

        // Act
        var result = await _repository.AddAsync(tag);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(tag.Id);
        result.Name.Should().Be(tag.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTag_WhenTagExists()
    {
        // Arrange
        var tag = CreateTestTag();
        await _repository.AddAsync(tag);

        // Act
        var result = await _repository.GetByIdAsync(tag.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(tag.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTagDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnTag_WhenNameExists()
    {
        // Arrange
        var tag = CreateTestTag();
        await _repository.AddAsync(tag);

        // Act
        var result = await _repository.GetByNameAsync(tag.Name);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(tag.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull_WhenNameDoesNotExist()
    {
        // Act
        var result = await _repository.GetByNameAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnMatchingTags()
    {
        // Arrange
        var tag1 = CreateTestTag("Tag 1", "#FF0000");
        var tag2 = CreateTestTag("Tag 2", "#00FF00");
        var tag3 = CreateTestTag("Tag 3", "#0000FF");
        await _repository.AddAsync(tag1);
        await _repository.AddAsync(tag2);
        await _repository.AddAsync(tag3);

        // Act
        var result = await _repository.GetByIdsAsync(new[] { tag1.Id, tag3.Id });

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Id == tag1.Id);
        result.Should().Contain(t => t.Id == tag3.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTags()
    {
        // Arrange
        var tag1 = CreateTestTag("Tag 1", "#FF0000");
        var tag2 = CreateTestTag("Tag 2", "#00FF00");
        await _repository.AddAsync(tag1);
        await _repository.AddAsync(tag2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTag()
    {
        // Arrange
        var tag = CreateTestTag();
        await _repository.AddAsync(tag);
        tag.Name = "Updated Name";
        tag.Color = "#FFFFFF";

        // Act
        await _repository.UpdateAsync(tag);

        // Assert
        var updatedTag = await _repository.GetByIdAsync(tag.Id);
        updatedTag.Should().NotBeNull();
        updatedTag!.Name.Should().Be("Updated Name");
        updatedTag.Color.Should().Be("#FFFFFF");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTag()
    {
        // Arrange
        var tag = CreateTestTag();
        await _repository.AddAsync(tag);

        // Act
        await _repository.DeleteAsync(tag);

        // Assert
        var deletedTag = await _repository.GetByIdAsync(tag.Id);
        deletedTag.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            await _repository.AddAsync(CreateTestTag($"Tag {i}", $"#FF{i:D4}"));
        }

        // Act
        var result = await _repository.GetPagedAsync(null, 2, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenTagExists()
    {
        // Arrange
        var tag = CreateTestTag();
        await _repository.AddAsync(tag);

        // Act
        var result = await _repository.ExistsAsync(t => t.Name == tag.Name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenTagDoesNotExist()
    {
        // Act
        var result = await _repository.ExistsAsync(t => t.Name == "nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    private static Tag CreateTestTag(string name = "Test Tag", string color = "#FF5733")
    {
        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = color,
            CreatedAt = DateTime.UtcNow
        };
    }
}
