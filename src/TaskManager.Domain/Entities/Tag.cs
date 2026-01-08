namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents a tag that can be assigned to tasks.
/// </summary>
public class Tag
{
    /// <summary>
    /// Gets or sets the unique identifier for the tag.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tag name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tag color in hex format (e.g., #FF5733).
    /// </summary>
    public string Color { get; set; } = "#000000";

    /// <summary>
    /// Gets or sets the date and time when the tag was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of task-tag associations.
    /// </summary>
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
