using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Projects;

/// <summary>
/// Request DTO for adding a member to a project.
/// </summary>
public class AddMemberRequest
{
    /// <summary>
    /// Gets or sets the user ID to add.
    /// </summary>
    [Required(ErrorMessage = "UserId is required")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the role for the user (Member or Owner).
    /// </summary>
    public string Role { get; set; } = "Member";
}
