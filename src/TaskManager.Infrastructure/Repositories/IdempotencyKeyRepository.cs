using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for IdempotencyKey entities.
/// </summary>
public class IdempotencyKeyRepository : IIdempotencyKeyRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyKeyRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public IdempotencyKeyRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IdempotencyKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.IdempotencyKeys
            .FirstOrDefaultAsync(k => k.Key == key && k.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default)
    {
        await _context.IdempotencyKeys.AddAsync(idempotencyKey, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        var expiredKeys = await _context.IdempotencyKeys
            .Where(k => k.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _context.IdempotencyKeys.RemoveRange(expiredKeys);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
