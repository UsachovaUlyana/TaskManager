using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

/// <summary>
/// Repository interface for IdempotencyKey entities.
/// </summary>
public interface IIdempotencyKeyRepository
{
    /// <summary>
    /// Gets an idempotency key by its value.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The idempotency key if found; otherwise, null.</returns>
    Task<IdempotencyKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an idempotency key with the response.
    /// </summary>
    /// <param name="idempotencyKey">The idempotency key to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired idempotency keys.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
}
