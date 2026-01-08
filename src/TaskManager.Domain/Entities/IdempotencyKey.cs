namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents an idempotency key for ensuring POST request idempotency.
/// </summary>
public class IdempotencyKey
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the idempotency key value.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request path.
    /// </summary>
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hash of the request body.
    /// </summary>
    public string RequestBodyHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response status code.
    /// </summary>
    public int ResponseStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the response body.
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the key was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the key expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
