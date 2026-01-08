using System.Security.Cryptography;
using System.Text;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.API.Middleware;

/// <summary>
/// Middleware for handling idempotency of POST requests.
/// </summary>
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;
    private const string IdempotencyKeyHeader = "X-Idempotency-Key";

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="idempotencyKeyRepository">The idempotency key repository.</param>
    public async Task InvokeAsync(HttpContext context, IIdempotencyKeyRepository idempotencyKeyRepository)
    {
        // Only handle POST requests
        if (context.Request.Method != HttpMethods.Post)
        {
            await _next(context);
            return;
        }

        // Check for idempotency key header
        if (!context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKeyValue))
        {
            await _next(context);
            return;
        }

        var key = idempotencyKeyValue.ToString();
        if (string.IsNullOrEmpty(key))
        {
            await _next(context);
            return;
        }

        // Check if this key already exists
        var existingKey = await idempotencyKeyRepository.GetByKeyAsync(key);
        if (existingKey != null)
        {
            _logger.LogInformation("Idempotent request detected. Key: {Key}, Path: {Path}", key, context.Request.Path);
            
            context.Response.StatusCode = existingKey.ResponseStatusCode;
            context.Response.ContentType = "application/json";
            
            if (!string.IsNullOrEmpty(existingKey.ResponseBody))
            {
                await context.Response.WriteAsync(existingKey.ResponseBody);
            }
            return;
        }

        // Enable buffering to read request body
        context.Request.EnableBuffering();
        var requestBody = await ReadRequestBodyAsync(context.Request);
        var requestBodyHash = HashString(requestBody);

        // Capture the original response body stream
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            // Read and save the response
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync();

            // Save idempotency key
            var idempotencyEntry = new IdempotencyKey
            {
                Id = Guid.NewGuid(),
                Key = key,
                RequestPath = context.Request.Path,
                RequestBodyHash = requestBodyHash,
                ResponseStatusCode = context.Response.StatusCode,
                ResponseBody = responseContent,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            await idempotencyKeyRepository.SaveAsync(idempotencyEntry);

            // Copy response to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static string HashString(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
