using CarePath.Exceptions;
using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Infrastructure;

public static class EntityQueryExtensions
{
    public static async Task<T> GetRequiredAsync<T>(this IQueryable<T> query, int id, string entityName, string? propertyName = null)
        where T : Entity
    {
        return await query.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new EntityNotFoundException(entityName, id, propertyName);
    }

    public static string BedLabel(this Bed bed) => $"{bed.Ward?.Code ?? "?"}-{bed.BedNumber}";
}
