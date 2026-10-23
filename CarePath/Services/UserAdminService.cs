using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly IClock _clock;

    public UserAdminService(UserManager<ApplicationUser> users, ApplicationDbContext db, IAuditService audit, IClock clock)
    {
        _users = users;
        _db = db;
        _audit = audit;
        _clock = clock;
    }

    public async Task<IReadOnlyList<UserSummary>> ListAsync()
    {
        var users = await _users.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync();
        var roleLookup = await (from ur in _db.UserRoles
                                join r in _db.Roles on ur.RoleId equals r.Id
                                select new { ur.UserId, r.Name }).ToListAsync();
        return users.Select(u => ToSummary(u, roleLookup.Where(r => r.UserId == u.Id).Select(r => r.Name!).ToList())).ToList();
    }

    public async Task<UserDetailsViewModel> GetDetailsAsync(string id)
    {
        var user = await FindAsync(id);
        var roles = await _users.GetRolesAsync(user);
        return new UserDetailsViewModel
        {
            User = ToSummary(user, roles.ToList()),
            RecentActivity = await _audit.GetRecentForUserAsync(user.UserName!, 25)
        };
    }

    public async Task<UserEditViewModel> GetForEditAsync(string id)
    {
        var user = await FindAsync(id);
        var roles = await _users.GetRolesAsync(user);
        return new UserEditViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = roles.FirstOrDefault(),
            IsActive = user.IsActive
        };
    }

    public async Task<string> CreateAsync(UserCreateViewModel model)
    {
        var role = ValidateRole(model.Role);
        var email = (model.Email ?? string.Empty).Trim();
        if (await _users.FindByEmailAsync(email) != null)
        {
            throw new DuplicateValueException("User", nameof(model.Email), "email", email);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = (model.FullName ?? string.Empty).Trim(),
            IsActive = true,
            CreatedAt = _clock.Now
        };
        var created = await _users.CreateAsync(user, model.Password ?? string.Empty);
        ThrowIfFailed(created, nameof(model.Password));
        ThrowIfFailed(await _users.AddToRoleAsync(user, role), nameof(model.Role));

        _audit.Record(AuditActions.UserCreated, nameof(ApplicationUser), user.Id, $"{email} as {role}");
        await _db.SaveChangesAsync();
        return user.Id;
    }

    public async Task UpdateAsync(UserEditViewModel model, string actingUserName)
    {
        var role = ValidateRole(model.Role);
        var user = await FindAsync(model.Id);
        var currentRoles = await _users.GetRolesAsync(user);
        var isSelf = string.Equals(user.UserName, actingUserName, StringComparison.OrdinalIgnoreCase);

        if (isSelf && (!model.IsActive || (currentRoles.Contains(Roles.Admin) && role != Roles.Admin)))
        {
            throw new DomainValidationException("You cannot deactivate your own account or remove your own Admin role.", nameof(model.IsActive));
        }

        if (currentRoles.Contains(Roles.Admin) && (role != Roles.Admin || !model.IsActive))
        {
            var activeAdmins = (await _users.GetUsersInRoleAsync(Roles.Admin)).Count(u => u.IsActive);
            if (activeAdmins <= 1)
            {
                throw new DomainValidationException("At least one active Admin account must remain.", nameof(model.Role));
            }
        }

        user.FullName = (model.FullName ?? string.Empty).Trim();
        user.IsActive = model.IsActive;
        user.LockoutEnabled = true;
        user.LockoutEnd = model.IsActive ? null : DateTimeOffset.MaxValue;
        ThrowIfFailed(await _users.UpdateAsync(user), nameof(model.FullName));

        if (!currentRoles.SequenceEqual(new[] { role }))
        {
            ThrowIfFailed(await _users.RemoveFromRolesAsync(user, currentRoles), nameof(model.Role));
            ThrowIfFailed(await _users.AddToRoleAsync(user, role), nameof(model.Role));
        }

        _audit.Record(AuditActions.UserUpdated, nameof(ApplicationUser), user.Id, $"role {role}, active {model.IsActive}");
        await _db.SaveChangesAsync();
    }

    private async Task<ApplicationUser> FindAsync(string id) =>
        await _users.FindByIdAsync(id) ?? throw new EntityNotFoundException("User", id);

    private static string ValidateRole(string? role)
    {
        var match = Roles.All.FirstOrDefault(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
        return match ?? throw new DomainValidationException($"'{role}' is not a valid role.", "Role");
    }

    private static void ThrowIfFailed(IdentityResult result, string propertyName)
    {
        if (!result.Succeeded)
        {
            throw new DomainValidationException(string.Join(" ", result.Errors.Select(e => e.Description)), propertyName);
        }
    }

    private static UserSummary ToSummary(ApplicationUser user, IReadOnlyList<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        FullName = user.FullName,
        IsActive = user.IsActive,
        Roles = roles,
        CreatedAt = user.CreatedAt
    };
}
