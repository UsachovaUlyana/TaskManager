namespace TaskManager.Application.DTOs.Common;

/// <summary>
/// Standard API error response.
/// </summary>
public class ApiError
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional error details.
    /// </summary>
    public object? Details { get; set; }

    /// <summary>
    /// Gets or sets the trace ID for debugging.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Creates a new API error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="details">Additional error details.</param>
    /// <returns>A new ApiError instance.</returns>
    public static ApiError Create(string code, string message, object? details = null)
    {
        return new ApiError
        {
            Code = code,
            Message = message,
            Details = details
        };
    }
}
