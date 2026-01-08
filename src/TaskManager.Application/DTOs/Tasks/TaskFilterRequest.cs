namespace TaskManager.Application.DTOs.Tasks;

/// <summary>
/// Request DTO for filtering tasks.
/// </summary>
public class TaskFilterRequest
{
    /// <summary>
    /// Gets or sets the status filter (Pending, InProgress, Completed, Cancelled).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the priority filter (Low, Medium, High, Critical).
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Gets or sets the project ID filter.
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the minimum due date filter.
    /// </summary>
    public DateTime? DueDateFrom { get; set; }

    /// <summary>
    /// Gets or sets the maximum due date filter.
    /// </summary>
    public DateTime? DueDateTo { get; set; }

    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = 10;
}
