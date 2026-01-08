namespace TaskManager.Application.Exceptions;

/// <summary>
/// Base application exception.
/// </summary>
public class AppException : Exception
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public AppException(string code, string message) : base(message)
    {
        Code = code;
    }
}

/// <summary>
/// Exception thrown when a resource is not found.
/// </summary>
public class NotFoundException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="resource">The resource type.</param>
    /// <param name="id">The resource identifier.</param>
    public NotFoundException(string resource, object id) 
        : base("NOT_FOUND", $"{resource} with ID '{id}' was not found.")
    {
    }
}

/// <summary>
/// Exception thrown when there is a conflict with existing data.
/// </summary>
public class ConflictException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConflictException(string message) 
        : base("CONFLICT", message)
    {
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : AppException
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public ValidationException(IDictionary<string, string[]> errors) 
        : base("VALIDATION_ERROR", "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValidationException(string message) 
        : base("VALIDATION_ERROR", message)
    {
        Errors = new Dictionary<string, string[]>();
    }
}

/// <summary>
/// Exception thrown when authentication fails.
/// </summary>
public class UnauthorizedException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public UnauthorizedException(string message = "Invalid credentials.") 
        : base("UNAUTHORIZED", message)
    {
    }
}

/// <summary>
/// Exception thrown when access is forbidden.
/// </summary>
public class ForbiddenException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ForbiddenException(string message = "You do not have permission to perform this action.") 
        : base("FORBIDDEN", message)
    {
    }
}
