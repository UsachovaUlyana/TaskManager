using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Tests.Repositories;

/// <summary>
/// Unit tests for TaskRepository.
/// </summary>
public class TaskRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskRepository _repository;
    private readonly User _testUser;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new TaskRepository(_context);

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
    public async Task AddAsync_ShouldAddTask()
    {
        // Arrange
        var task = CreateTestTask();

        // Act
        var result = await _repository.AddAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask_WhenTaskExists()
    {
        // Arrange
        var task = CreateTestTask();
        await _repository.AddAsync(task);

        // Act
        var result = await _repository.GetByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ShouldReturnTaskWithRelatedData()
    {
        // Arrange
        var task = CreateTestTask();
        await _repository.AddAsync(task);

        // Act
        var result = await _repository.GetByIdWithDetailsAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserTasks()
    {
        // Arrange
        var task1 = CreateTestTask("Task 1");
        var task2 = CreateTestTask("Task 2");
        await _repository.AddAsync(task1);
        await _repository.AddAsync(task2);

        // Act
        var result = await _repository.GetByUserIdAsync(
            _testUser.Id, null, null, null, null, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldFilterByStatus()
    {
        // Arrange
        var task1 = CreateTestTask("Task 1");
        task1.Status = TaskItemStatus.Pending;
        var task2 = CreateTestTask("Task 2");
        task2.Status = TaskItemStatus.Completed;
        await _repository.AddAsync(task1);
        await _repository.AddAsync(task2);

        // Act
        var result = await _repository.GetByUserIdAsync(
            _testUser.Id, TaskItemStatus.Pending, null, null, null, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(TaskItemStatus.Pending);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldFilterByPriority()
    {
        // Arrange
        var task1 = CreateTestTask("Task 1");
        task1.Priority = TaskPriority.High;
        var task2 = CreateTestTask("Task 2");
        task2.Priority = TaskPriority.Low;
        await _repository.AddAsync(task1);
        await _repository.AddAsync(task2);

        // Act
        var result = await _repository.GetByUserIdAsync(
            _testUser.Id, null, TaskPriority.High, null, null, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Priority.Should().Be(TaskPriority.High);
    }

    [Fact]
    public async Task GetAllFilteredAsync_ShouldReturnAllTasks()
    {
        // Arrange
        var task1 = CreateTestTask("Task 1");
        var task2 = CreateTestTask("Task 2");
        await _repository.AddAsync(task1);
        await _repository.AddAsync(task2);

        // Act
        var result = await _repository.GetAllFilteredAsync(
            null, null, null, null, null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllFilteredAsync_ShouldFilterByDueDate()
    {
        // Arrange
        var task1 = CreateTestTask("Task 1");
        task1.DueDate = DateTime.UtcNow.AddDays(1);
        var task2 = CreateTestTask("Task 2");
        task2.DueDate = DateTime.UtcNow.AddDays(10);
        await _repository.AddAsync(task1);
        await _repository.AddAsync(task2);

        // Act
        var result = await _repository.GetAllFilteredAsync(
            null, null, null, DateTime.UtcNow, DateTime.UtcNow.AddDays(5), 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTask()
    {
        // Arrange
        var task = CreateTestTask();
        await _repository.AddAsync(task);
        task.Title = "Updated Title";
        task.Status = TaskItemStatus.Completed;

        // Act
        await _repository.UpdateAsync(task);

        // Assert
        var updatedTask = await _repository.GetByIdAsync(task.Id);
        updatedTask.Should().NotBeNull();
        updatedTask!.Title.Should().Be("Updated Title");
        updatedTask.Status.Should().Be(TaskItemStatus.Completed);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteTask()
    {
        // Arrange
        var task = CreateTestTask();
        await _repository.AddAsync(task);

        // Act
        await _repository.DeleteAsync(task);

        // Assert
        var deletedTask = await _repository.GetByIdAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            await _repository.AddAsync(CreateTestTask($"Task {i}"));
        }

        // Act
        var result = await _repository.GetPagedAsync(null, 2, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();
    }

    private TaskItem CreateTestTask(string title = "Test Task")
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Test Description",
            Status = TaskItemStatus.Pending,
            Priority = TaskPriority.Medium,
            UserId = _testUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
