using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Repository interface for ApiKey entities.
/// </summary>
public interface IApiKeyRepository : IRepository<ApiKey>
{
    /// <summary>
    /// Validates an API key.
    /// </summary>
    /// <param name="keyHash">The hashed API key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The API key if valid; otherwise, null.</returns>
    Task<ApiKey?> ValidateKeyAsync(string keyHash, CancellationToken cancellationToken = default);
}
