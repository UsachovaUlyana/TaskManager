using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Auth;

/// <summary>
/// Request DTO for creating an API key.
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>
    /// Gets or sets the name/description for the API key.
    /// </summary>
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the expiration days for the API key. If null, the key never expires.
    /// </summary>
    [Range(1, 365, ErrorMessage = "Expiration days must be between 1 and 365")]
    public int? ExpirationDays { get; set; }
}
