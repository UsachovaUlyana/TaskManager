namespace TaskManager.Application.DTOs.Auth;

/// <summary>
/// Response DTO for API key generation.
/// </summary>
public class ApiKeyResponse
{
    /// <summary>
    /// Gets or sets the generated API key.
    /// This value is shown only once and cannot be retrieved again.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the API key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name/description of the API key.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date of the API key.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the API key. Null if never expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
