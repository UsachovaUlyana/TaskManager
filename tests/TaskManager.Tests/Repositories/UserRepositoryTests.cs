using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;

namespace TaskManager.Tests.Repositories;

/// <summary>
/// Unit tests for UserRepository.
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = CreateTestUser();
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenUsernameExists()
    {
        // Arrange
        var user = CreateTestUser();
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByUsernameAsync(user.Username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenUsernameDoesNotExist()
    {
        // Act
        var result = await _repository.GetByUsernameAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        var user = CreateTestUser();
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByEmailAsync(user.Email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@test.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = CreateTestUser("user1", "user1@test.com");
        var user2 = CreateTestUser("user2", "user2@test.com");
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            await _repository.AddAsync(CreateTestUser($"user{i}", $"user{i}@test.com"));
        }

        // Act
        var result = await _repository.GetPagedAsync(null, 1, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        // Arrange
        var user = CreateTestUser();
        await _repository.AddAsync(user);
        user.Username = "updated_username";

        // Act
        await _repository.UpdateAsync(user);

        // Assert
        var updatedUser = await _repository.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Username.Should().Be("updated_username");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser()
    {
        // Arrange
        var user = CreateTestUser();
        await _repository.AddAsync(user);

        // Act
        await _repository.DeleteAsync(user);

        // Assert
        var deletedUser = await _repository.GetByIdAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var user = CreateTestUser();
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.ExistsAsync(u => u.Username == user.Username);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Act
        var result = await _repository.ExistsAsync(u => u.Username == "nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    private static User CreateTestUser(string username = "testuser", string email = "test@test.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = "hashedpassword",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
