using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

/// <summary>
/// Authentication service implementation.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="apiKeyRepository">The API key repository.</param>
    /// <param name="configuration">The configuration.</param>
    public AuthService(
        IUserRepository userRepository,
        IApiKeyRepository apiKeyRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _apiKeyRepository = apiKeyRepository;
        _configuration = configuration;
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if username exists
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
        {
            throw new ConflictException("Username is already taken.");
        }

        // Check if email exists
        existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new ConflictException("Email is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var token = GenerateJwtToken(user.Id, user.Username, user.Role.ToString());
        var expiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60") * 60;

        return new AuthResponse
        {
            Token = token,
            ExpiresIn = expiresIn,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }

    /// <inheritdoc/>
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
                   ?? await _userRepository.GetByEmailAsync(request.Username, cancellationToken);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        var token = GenerateJwtToken(user.Id, user.Username, user.Role.ToString());
        var expiresIn = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60") * 60;

        return new AuthResponse
        {
            Token = token,
            ExpiresIn = expiresIn,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }

    /// <inheritdoc/>
    public string GenerateJwtToken(Guid userId, string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <inheritdoc/>
    public bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    /// <inheritdoc/>
    public async Task<ApiKeyResponse> GenerateApiKeyAsync(CreateApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        // Generate a random API key with prefix
        var rawKey = $"tm_{GenerateRandomKey(32)}";
        
        // Hash the key for storage
        var keyHash = HashApiKey(rawKey);
        
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = keyHash,
            Name = string.IsNullOrWhiteSpace(request.Name) ? "API Key" : request.Name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpirationDays.HasValue 
                ? DateTime.UtcNow.AddDays(request.ExpirationDays.Value) 
                : null
        };

        await _apiKeyRepository.AddAsync(apiKey, cancellationToken);

        return new ApiKeyResponse
        {
            ApiKey = rawKey, // Return the raw key only once
            Id = apiKey.Id,
            Name = apiKey.Name,
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt
        };
    }

    private static string GenerateRandomKey(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var data = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(data);
        }
        var result = new StringBuilder(length);
        foreach (var b in data)
        {
            result.Append(chars[b % chars.Length]);
        }
        return result.ToString();
    }

    private static string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashedBytes);
    }
}
