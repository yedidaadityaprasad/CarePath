using CarePath.Exceptions;
using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Infrastructure;

/// <summary>
/// Extension methods for querying domain entities.
/// .NET 10 / EF Core 10: FirstOrDefaultAsync is still the recommended single-entity lookup.
/// </summary>
public static class EntityQueryExtensions
{
    /// <summary>
    /// Returns the entity with the given <paramref name="id"/>, or throws
    /// <see cref="EntityNotFoundException"/> if it does not exist.
    /// </summary>
    public static async Task<T> GetRequiredAsync<T>(
        this IQueryable<T> query,
        int id,
        string entityName,
        string? propertyName = null,
        CancellationToken cancellationToken = default)
        where T : Entity
    {
        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
               ?? throw new EntityNotFoundException(entityName, id, propertyName);
    }

    /// <summary>Returns a display label for a bed in the format WARD-BedNumber.</summary>
    public static string BedLabel(this Bed bed) => $"{bed.Ward?.Code ?? "?"}-{bed.BedNumber}";
}
