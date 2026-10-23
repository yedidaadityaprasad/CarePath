using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarePath.Models.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class UserSummary
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
}

public class UserIndexViewModel
{
    public IReadOnlyList<UserSummary> Users { get; set; } = Array.Empty<UserSummary>();
}

public class UserCreateViewModel
{
    [Required, EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Full name")]
    public string? FullName { get; set; }

    [Required]
    public string? Role { get; set; }

    [Required, StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string? ConfirmPassword { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Roles { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Full name")]
    public string? FullName { get; set; }

    [Required]
    public string? Role { get; set; }

    [Display(Name = "Account active")]
    public bool IsActive { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> Roles { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class UserDetailsViewModel
{
    public UserSummary User { get; set; } = null!;
    public IReadOnlyList<CarePath.Models.Entities.AuditEntry> RecentActivity { get; set; } = Array.Empty<CarePath.Models.Entities.AuditEntry>();
}
