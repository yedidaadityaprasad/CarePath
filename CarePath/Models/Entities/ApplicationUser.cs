using Microsoft.AspNetCore.Identity;

namespace CarePath.Models.Entities;

/// <summary>
/// Application user extending ASP.NET Core Identity's IdentityUser.
/// .NET 10: IdentityUser is unchanged; custom properties are preserved.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Display name shown in the navigation bar and audit log.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// When false the user cannot sign in. Enforced in AccountController and UserAdminService.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>UTC timestamp when the account was created.</summary>
    public DateTime CreatedAt { get; set; }
}
