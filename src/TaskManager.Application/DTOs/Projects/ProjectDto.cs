namespace TaskManager.Application.DTOs.Projects;

/// <summary>
/// Response DTO for project data.
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the list of project members.
    /// </summary>
    public List<ProjectMemberDto> Members { get; set; } = new();
}

/// <summary>
/// DTO for project member data.
/// </summary>
public class ProjectMemberDto
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role in the project.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the user joined the project.
    /// </summary>
    public DateTime JoinedAt { get; set; }
}
