using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ApiKey entities.
/// </summary>
public class ApiKeyRepository : BaseRepository<ApiKey>, IApiKeyRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ApiKeyRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<ApiKey?> ValidateKeyAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                k => k.KeyHash == keyHash 
                     && k.IsActive 
                     && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow),
                cancellationToken);
    }
}
