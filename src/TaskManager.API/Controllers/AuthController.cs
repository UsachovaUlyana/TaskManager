using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication response with JWT token.</returns>
    /// <response code="200">Returns the JWT token and user information.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="409">If the username or email already exists.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response));
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication response with JWT token.</returns>
    /// <response code="200">Returns the JWT token and user information.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the credentials are invalid.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response));
    }

    /// <summary>
    /// Generates a new API key for the authenticated user.
    /// </summary>
    /// <param name="request">The API key creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated API key (shown only once).</returns>
    /// <response code="200">Returns the generated API key.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <remarks>
    /// **Important:** The API key is shown only once. Store it securely.
    /// 
    /// To use the API key, include it in the X-Api-Key header:
    /// ```
    /// X-Api-Key: tm_your_generated_key_here
    /// ```
    /// </remarks>
    [HttpPost("api-key")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ApiKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ApiKeyResponse>>> GenerateApiKey(
        [FromBody] CreateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.GenerateApiKeyAsync(request, cancellationToken);
        return Ok(ApiResponse<ApiKeyResponse>.Ok(response));
    }
}
