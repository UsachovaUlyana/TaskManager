using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Users;

/// <summary>
/// Request DTO for creating a user (admin only).
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role (User or Admin).
    /// </summary>
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = "User";
}
