using CarePath.Models.Entities;
using CarePath.Models.ViewModels;

namespace CarePath.Services;

public interface IAuditService
{
    /// <summary>Adds an audit row to the current unit of work; it is persisted by the caller's SaveChanges.</summary>
    void Record(string action, string entityName, object entityId, string? details = null);

    Task<PagedResult<AuditEntry>> SearchAsync(AuditIndexViewModel filter, int pageSize = 50);

    Task<AuditDetailsViewModel> GetDetailsAsync(int id);

    Task<IReadOnlyList<string>> GetEntityNamesAsync();

    Task<IReadOnlyList<AuditEntry>> GetRecentForUserAsync(string userName, int take = 20);
}
