using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Tasks;

/// <summary>
/// Request DTO for updating a task.
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// Gets or sets the task title.
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(300, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 300 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task description.
    /// </summary>
    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the task status (Pending, InProgress, Completed, Cancelled).
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Gets or sets the task priority (Low, Medium, High, Critical).
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Gets or sets the due date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the project ID.
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the list of tag IDs to assign.
    /// </summary>
    public List<Guid> TagIds { get; set; } = new();
}
