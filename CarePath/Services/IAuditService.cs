using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

/// <summary>
/// Contract for recording and querying the application audit log.
/// .NET 10: CancellationToken added to all async methods for proper request cancellation.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Adds an audit row to the current unit of work.
    /// The row is persisted by the caller's SaveChangesAsync call.
    /// </summary>
    void Record(string action, string entityName, object entityId, string? details = null);

    Task<PagedResult<AuditEntry>> SearchAsync(
        AuditIndexViewModel filter,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<AuditDetailsViewModel> GetDetailsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetEntityNamesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEntry>> GetRecentForUserAsync(
        string userName,
        int take = 20,
        CancellationToken cancellationToken = default);
}
