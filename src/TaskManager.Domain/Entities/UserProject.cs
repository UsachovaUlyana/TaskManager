using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a many-to-many relationship between users and projects.
/// </summary>
public class UserProject
{
    /// <summary>
    /// Gets or sets the ID of the user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ID of the project.
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project.
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user's role within the project.
    /// </summary>
    public ProjectRole Role { get; set; } = ProjectRole.Member;

    /// <summary>
    /// Gets or sets the date and time when the user joined the project.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
