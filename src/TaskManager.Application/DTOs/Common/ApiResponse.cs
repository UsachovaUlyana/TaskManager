namespace TaskManager.Application.DTOs.Common;

/// <summary>
/// Standard API response wrapper.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the error information if the request failed.
    /// </summary>
    public ApiError? Error { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <returns>A successful ApiResponse.</returns>
    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="error">The error information.</param>
    /// <returns>A failed ApiResponse.</returns>
    public static ApiResponse<T> Fail(ApiError error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error
        };
    }
}
