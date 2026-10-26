using System.Diagnostics;
using CarePath.Models.ViewModels;
using CarePath.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarePath.Controllers;

/// <summary>
/// Dashboard and error-handling controller.
/// .NET 10: Action methods accept CancellationToken for proper request cancellation.
/// IExceptionHandlerPathFeature is still the recommended way to access exception details.
/// </summary>
public class HomeController : CarePathController
{
    private readonly IDashboardService _dashboard;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IDashboardService dashboard, ILogger<HomeController> logger)
    {
        _dashboard = dashboard;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await _dashboard.GetAsync(cancellationToken));

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        if (feature?.Error != null)
        {
            _logger.LogError(
                feature.Error,
                "Unhandled exception on {Path} (trace {TraceId})",
                feature.Path,
                requestId);
        }
        return View(new ErrorViewModel { RequestId = requestId });
    }

    [AllowAnonymous]
    [Route("Home/StatusCode")]
    public IActionResult StatusCodePage(int code)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = code,
            Title = code == 404 ? "Page not found" : $"Error {code}",
            Message = code == 404
                ? "The page you requested does not exist."
                : "The request could not be completed."
        };
        Response.StatusCode = code;
        return View("DomainError", model);
    }
}
