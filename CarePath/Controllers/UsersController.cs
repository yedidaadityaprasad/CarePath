using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

[Authorize(Roles = Roles.Admin)]
public class UsersController : CarePathController
{
    private readonly IUserAdminService _users;
    private readonly ILookupService _lookups;

    public UsersController(IUserAdminService users, ILookupService lookups)
    {
        _users = users;
        _lookups = lookups;
    }

    public async Task<IActionResult> Index() => View(new UserIndexViewModel { Users = await _users.ListAsync() });

    public async Task<IActionResult> Details(string id) => View(await _users.GetDetailsAsync(id));

    [HttpGet]
    public IActionResult Create() => View(new UserCreateViewModel { Roles = _lookups.Roles() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await _users.CreateAsync(model);
                Success($"User {model.Email} created.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex)
            {
                AddDomainError(ex, model);
            }
        }
        model.Roles = _lookups.Roles();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var model = await _users.GetForEditAsync(id);
        model.Roles = _lookups.Roles();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserEditViewModel model)
    {
        model.Id = id;
        if (ModelState.IsValid)
        {
            try
            {
                await _users.UpdateAsync(model, User.Identity?.Name ?? string.Empty);
                Success("User updated.");
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DomainException ex) when (ex is not EntityNotFoundException)
            {
                AddDomainError(ex, model);
            }
        }
        model.Roles = _lookups.Roles();
        return View(model);
    }
}
