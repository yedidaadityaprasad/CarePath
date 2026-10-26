using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

/// <summary>
/// Records and queries the application audit log.
/// .NET 10: All async methods accept CancellationToken for proper request cancellation support.
/// </summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public AuditService(ApplicationDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public void Record(string action, string entityName, object entityId, string? details = null)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("An audit action is required.", nameof(action));
        }

        var id = entityId?.ToString();
        if (string.IsNullOrWhiteSpace(id) || id == "0")
        {
            throw new InvalidOperationException(
                $"Cannot audit {action} on {entityName} before the entity has been saved.");
        }

        _db.AuditEntries.Add(new AuditEntry
        {
            UserName = _currentUser.UserName,
            Action = action,
            EntityName = entityName,
            EntityId = id,
            Timestamp = _clock.Now,
            Details = details is { Length: > 2000 } ? details[..2000] : details
        });
    }

    public Task<PagedResult<AuditEntry>> SearchAsync(
        AuditIndexViewModel filter,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (filter.From.HasValue && filter.To.HasValue && filter.To.Value.Date < filter.From.Value.Date)
        {
            throw new DomainValidationException(
                "The 'To' date must be on or after the 'From' date.", nameof(filter.To));
        }

        var query = _db.AuditEntries.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.EntityName))
        {
            query = query.Where(a => a.EntityName == filter.EntityName);
        }
        if (!string.IsNullOrWhiteSpace(filter.EntityId))
        {
            var entityId = filter.EntityId.Trim();
            query = query.Where(a => a.EntityId == entityId);
        }
        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            var user = filter.UserName.Trim();
            query = query.Where(a => a.UserName.Contains(user));
        }
        if (filter.From.HasValue)
        {
            var from = filter.From.Value.Date;
            query = query.Where(a => a.Timestamp >= from);
        }
        if (filter.To.HasValue)
        {
            var toExclusive = filter.To.Value.Date.AddDays(1);
            query = query.Where(a => a.Timestamp < toExclusive);
        }

        return query
            .OrderByDescending(a => a.Timestamp)
            .ThenByDescending(a => a.Id)
            .ToPagedResultAsync(filter.Page, pageSize, cancellationToken);
    }

    public async Task<AuditDetailsViewModel> GetDetailsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entry = await _db.AuditEntries.AsNoTracking()
            .GetRequiredAsync(id, "Audit entry", cancellationToken: cancellationToken);

        var related = await _db.AuditEntries.AsNoTracking()
            .Where(a => a.EntityName == entry.EntityName && a.EntityId == entry.EntityId && a.Id != entry.Id)
            .OrderByDescending(a => a.Timestamp)
            .Take(50)
            .ToListAsync(cancellationToken);

        return new AuditDetailsViewModel { Entry = entry, RelatedEntries = related };
    }

    public async Task<IReadOnlyList<string>> GetEntityNamesAsync(
        CancellationToken cancellationToken = default) =>
        await _db.AuditEntries.AsNoTracking()
            .Select(a => a.EntityName)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AuditEntry>> GetRecentForUserAsync(
        string userName,
        int take = 20,
        CancellationToken cancellationToken = default) =>
        await _db.AuditEntries.AsNoTracking()
            .Where(a => a.UserName == userName)
            .OrderByDescending(a => a.Timestamp)
            .Take(take)
            .ToListAsync(cancellationToken);
}
