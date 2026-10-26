using CarePath.Data;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Infrastructure;

/// <summary>
/// Extension methods for managing EF Core database transactions.
/// .NET 10 / EF Core 10: BeginTransactionAsync and CommitAsync remain the recommended API.
/// </summary>
public static class DbContextTransactionExtensions
{
    /// <summary>
    /// Runs <paramref name="work"/> in a database transaction, joining the ambient one when
    /// a caller has already started it.
    /// </summary>
    public static async Task<T> InTransactionAsync<T>(
        this ApplicationDbContext db,
        Func<Task<T>> work,
        CancellationToken cancellationToken = default)
    {
        if (db.Database.CurrentTransaction != null)
        {
            return await work();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var result = await work();
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    /// <summary>Void overload of <see cref="InTransactionAsync{T}"/>.</summary>
    public static Task InTransactionAsync(
        this ApplicationDbContext db,
        Func<Task> work,
        CancellationToken cancellationToken = default) =>
        db.InTransactionAsync(async () =>
        {
            await work();
            return true;
        }, cancellationToken);
}
