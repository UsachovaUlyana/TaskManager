namespace TaskManager.Application.DTOs.Tags;

/// <summary>
/// Response DTO for tag data.
/// </summary>
public class TagDto
{
    /// <summary>
    /// Gets or sets the tag ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tag name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tag color in hex format.
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
