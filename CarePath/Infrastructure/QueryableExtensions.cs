using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Infrastructure;

/// <summary>
/// Extension methods for <see cref="IQueryable{T}"/> to support server-side pagination.
/// .NET 10 / EF Core 10: CountAsync and ToListAsync are still the recommended approach.
/// </summary>
public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, page, pageSize, total);
    }
}
