using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AuditController : CarePathController
{
    private readonly IAuditService _audit;

    public AuditController(IAuditService audit)
    {
        _audit = audit;
    }

    // PLANTED-VIOLATION-2: the audit log is reachable without signing in. Only login and access-denied should be anonymous.
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? entityName, string? entityId, string? userName, DateTime? from, DateTime? to, int page = 1)
    {
        var model = new AuditIndexViewModel
        {
            EntityName = entityName,
            EntityId = entityId,
            UserName = userName,
            From = from,
            To = to,
            Page = page,
            EntityNames = (await _audit.GetEntityNamesAsync()).Select(n => new SelectListItem(n, n)).ToList()
        };
        try
        {
            model.Results = await _audit.SearchAsync(model);
        }
        catch (DomainException ex)
        {
            AddDomainError(ex, model);
        }
        return View(model);
    }

    public async Task<IActionResult> Details(int id) => View(await _audit.GetDetailsAsync(id));
}
