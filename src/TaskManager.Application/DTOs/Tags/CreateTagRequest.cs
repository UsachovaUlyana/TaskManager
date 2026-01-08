using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Tags;

/// <summary>
/// Request DTO for creating a tag.
/// </summary>
public class CreateTagRequest
{
    /// <summary>
    /// Gets or sets the tag name.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tag color in hex format (e.g., #FF5733).
    /// </summary>
    [Required(ErrorMessage = "Color is required")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color (e.g., #FF5733)")]
    public string Color { get; set; } = "#000000";
}
