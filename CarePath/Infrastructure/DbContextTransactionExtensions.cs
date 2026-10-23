using CarePath.Data;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Infrastructure;

public static class DbContextTransactionExtensions
{
    /// <summary>
    /// Runs <paramref name="work"/> in a database transaction, joining the ambient one when a caller already started it.
    /// </summary>
    public static async Task<T> InTransactionAsync<T>(this ApplicationDbContext db, Func<Task<T>> work)
    {
        if (db.Database.CurrentTransaction != null)
        {
            return await work();
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        var result = await work();
        await transaction.CommitAsync();
        return result;
    }

    public static Task InTransactionAsync(this ApplicationDbContext db, Func<Task> work) =>
        db.InTransactionAsync(async () =>
        {
            await work();
            return true;
        });
}
