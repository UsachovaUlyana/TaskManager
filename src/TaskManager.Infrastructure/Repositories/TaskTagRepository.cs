using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for TaskTag entities using Dapper for transactions.
/// </summary>
public class TaskTagRepository : ITaskTagRepository
{
    private readonly AppDbContext _context;
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskTagRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TaskTagRepository(AppDbContext context)
    {
        _context = context;
        _connectionString = context.Database.GetConnectionString() 
            ?? throw new InvalidOperationException("Connection string is not configured");
    }

    /// <inheritdoc/>
    public async Task AssignTagsToTaskAsync(Guid taskId, IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            // First, remove existing tags
            const string deleteSql = "DELETE FROM task_tags WHERE task_id = @TaskId";
            await connection.ExecuteAsync(deleteSql, new { TaskId = taskId }, transaction);

            // Then, insert new tags
            const string insertSql = "INSERT INTO task_tags (task_id, tag_id) VALUES (@TaskId, @TagId)";
            
            foreach (var tagId in tagIds)
            {
                await connection.ExecuteAsync(insertSql, new { TaskId = taskId, TagId = tagId }, transaction);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAllTagsFromTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "DELETE FROM task_tags WHERE task_id = @TaskId";
        await connection.ExecuteAsync(sql, new { TaskId = taskId });
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Tag>> GetTagsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            SELECT t.id, t.name, t.color, t.created_at as CreatedAt
            FROM tags t
            INNER JOIN task_tags tt ON t.id = tt.tag_id
            WHERE tt.task_id = @TaskId";

        var tags = await connection.QueryAsync<Tag>(sql, new { TaskId = taskId });
        return tags.ToList();
    }
}
