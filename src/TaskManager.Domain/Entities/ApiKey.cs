namespace TaskManager.Domain.Entities;

/// <summary>
/// Represents an API key for service-to-service authentication.
/// </summary>
public class ApiKey
{
    /// <summary>
    /// Gets or sets the unique identifier for the API key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the hashed API key.
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name/description of the API key.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the API key is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time when the API key was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the API key expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
