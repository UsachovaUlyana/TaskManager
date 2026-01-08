using System.Net;
using System.Text.Json;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.Exceptions;

namespace TaskManager.API.Middleware;

/// <summary>
/// Middleware for centralized exception handling.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, error) = exception switch
        {
            NotFoundException ex => (HttpStatusCode.NotFound, ApiError.Create(ex.Code, ex.Message)),
            ConflictException ex => (HttpStatusCode.Conflict, ApiError.Create(ex.Code, ex.Message)),
            ValidationException ex => (HttpStatusCode.BadRequest, ApiError.Create(ex.Code, ex.Message, ex.Errors)),
            UnauthorizedException ex => (HttpStatusCode.Unauthorized, ApiError.Create(ex.Code, ex.Message)),
            ForbiddenException ex => (HttpStatusCode.Forbidden, ApiError.Create(ex.Code, ex.Message)),
            AppException ex => (HttpStatusCode.BadRequest, ApiError.Create(ex.Code, ex.Message)),
            _ => (HttpStatusCode.InternalServerError, ApiError.Create("INTERNAL_ERROR", "An unexpected error occurred."))
        };

        error.TraceId = traceId;

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning("Application exception occurred. Code: {Code}, Message: {Message}, TraceId: {TraceId}",
                error.Code, error.Message, traceId);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var response = ApiResponse<object>.Fail(error);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
