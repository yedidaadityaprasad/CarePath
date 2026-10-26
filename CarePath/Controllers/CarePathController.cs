using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

/// <summary>
/// Base controller providing shared helpers for flash messages, domain error mapping,
/// and status-change action patterns.
/// .NET 10: Inherits from <see cref="Controller"/> (Microsoft.AspNetCore.Mvc) — unchanged.
/// </summary>
public abstract class CarePathController : Controller
{
    /// <summary>Stores a success flash message in TempData for display after redirect.</summary>
    protected void Success(string message) => TempData.Success(message);

    /// <summary>Stores an error flash message in TempData for display after redirect.</summary>
    protected void Failure(string message) => TempData.Error(message);

    /// <summary>
    /// Maps a domain rule violation onto the form's model state so the view can redisplay it.
    /// </summary>
    protected void AddDomainError(DomainException exception, object model)
    {
        var key = exception.PropertyName != null
                  && model.GetType().GetProperty(exception.PropertyName) != null
            ? exception.PropertyName
            : string.Empty;
        ModelState.AddModelError(key, exception.Message);
    }

    /// <summary>
    /// Runs a status-change command and reports the outcome through TempData before redirecting.
    /// </summary>
    protected async Task<IActionResult> StatusActionAsync(
        Func<Task> action,
        string successMessage,
        Func<IActionResult> redirect)
    {
        try
        {
            await action();
            Success(successMessage);
        }
        catch (DomainException ex) when (ex is not EntityNotFoundException)
        {
            Failure(ex.Message);
        }
        return redirect();
    }

    /// <summary>
    /// Same as <see cref="StatusActionAsync"/> but requires a validated reason first.
    /// </summary>
    protected Task<IActionResult> ReasonActionAsync(
        ReasonActionViewModel input,
        Func<string, Task> action,
        string successMessage,
        Func<IActionResult> redirect)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(input.Reason))
        {
            var message = ModelState.Values
                              .SelectMany(v => v.Errors)
                              .Select(e => e.ErrorMessage)
                              .FirstOrDefault()
                          ?? "A reason is required.";
            Failure(message);
            return Task.FromResult(redirect());
        }
        return StatusActionAsync(() => action(input.Reason.Trim()), successMessage, redirect);
    }
}
