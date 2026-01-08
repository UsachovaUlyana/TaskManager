using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Projects;

/// <summary>
/// Request DTO for creating a project.
/// </summary>
public class CreateProjectRequest
{
    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }
}
