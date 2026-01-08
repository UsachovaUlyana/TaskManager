using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Tests.Repositories;

/// <summary>
/// Unit tests for ProjectRepository.
/// </summary>
public class ProjectRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProjectRepository _repository;
    private readonly User _testUser;

    public ProjectRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new ProjectRepository(_context);

        // Create test user
        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task AddAsync_ShouldAddProject()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = await _repository.AddAsync(project);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(project.Id);
        result.Name.Should().Be(project.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProject_WhenProjectExists()
    {
        // Arrange
        var project = CreateTestProject();
        await _repository.AddAsync(project);

        // Act
        var result = await _repository.GetByIdAsync(project.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(project.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProjectDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithMembersAsync_ShouldReturnProjectWithMembers()
    {
        // Arrange
        var project = CreateTestProject();
        await _repository.AddAsync(project);

        var userProject = new UserProject
        {
            UserId = _testUser.Id,
            ProjectId = project.Id,
            Role = ProjectRole.Owner,
            JoinedAt = DateTime.UtcNow
        };
        _context.UserProjects.Add(userProject);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithMembersAsync(project.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserProjects.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserProjects()
    {
        // Arrange
        var project1 = CreateTestProject("Project 1");
        var project2 = CreateTestProject("Project 2");
        await _repository.AddAsync(project1);
        await _repository.AddAsync(project2);

        _context.UserProjects.Add(new UserProject
        {
            UserId = _testUser.Id,
            ProjectId = project1.Id,
            Role = ProjectRole.Owner,
            JoinedAt = DateTime.UtcNow
        });
        _context.UserProjects.Add(new UserProject
        {
            UserId = _testUser.Id,
            ProjectId = project2.Id,
            Role = ProjectRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(_testUser.Id, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task IsUserMemberAsync_ShouldReturnTrue_WhenUserIsMember()
    {
        // Arrange
        var project = CreateTestProject();
        await _repository.AddAsync(project);

        _context.UserProjects.Add(new UserProject
        {
            UserId = _testUser.Id,
            ProjectId = project.Id,
            Role = ProjectRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.IsUserMemberAsync(project.Id, _testUser.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserMemberAsync_ShouldReturnFalse_WhenUserIsNotMember()
    {
        // Arrange
        var project = CreateTestProject();
        await _repository.AddAsync(project);

        // Act
        var result = await _repository.IsUserMemberAsync(project.Id, _testUser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProjects()
    {
        // Arrange
        var project1 = CreateTestProject("Project 1");
        var project2 = CreateTestProject("Project 2");
        await _repository.AddAsync(project1);
        await _repository.AddAsync(project2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProject()
    {
        // Arrange
        var project = CreateTestProject();
        await _repository.AddAsync(project);
        project.Name = "Updated Name";
        project.Description = "Updated Description";

        // Act
        await _repository.UpdateAsync(project);

        // Assert
        var updatedProject = await _repository.GetByIdAsync(project.Id);
        updatedProject.Should().NotBeNull();
        updatedProject!.Name.Should().Be("Updated Name");
        updatedProject.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProject()
    {
        // Arrange
        var project = CreateTestProject();
        await _repository.AddAsync(project);

        // Act
        await _repository.DeleteAsync(project);

        // Assert
        var deletedProject = await _repository.GetByIdAsync(project.Id);
        deletedProject.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            await _repository.AddAsync(CreateTestProject($"Project {i}"));
        }

        // Act
        var result = await _repository.GetPagedAsync(null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    private static Project CreateTestProject(string name = "Test Project")
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
