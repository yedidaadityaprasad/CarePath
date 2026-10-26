using System.Diagnostics;
using CarePath.Exceptions;
using CarePath.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CarePath.Infrastructure;

/// <summary>
/// Turns domain exceptions that escape a controller action into a friendly error page.
/// .NET 10: Implements IExceptionFilter (synchronous); for async scenarios use IAsyncExceptionFilter.
/// </summary>
public class DomainExceptionFilter : IExceptionFilter
{
    private readonly ILogger<DomainExceptionFilter> _logger;
    private readonly IModelMetadataProvider _metadataProvider;

    public DomainExceptionFilter(ILogger<DomainExceptionFilter> logger, IModelMetadataProvider metadataProvider)
    {
        _logger = logger;
        _metadataProvider = metadataProvider;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not DomainException domainException)
        {
            return;
        }

        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        _logger.LogWarning(
            domainException,
            "Domain rule violation {ExceptionType} on {Method} {Path} by {User} (trace {TraceId}): {Message}",
            domainException.GetType().Name,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            context.HttpContext.User.Identity?.Name ?? "anonymous",
            traceId,
            domainException.Message);

        var model = new ErrorViewModel
        {
            RequestId = traceId,
            Title = domainException.Title,
            Message = domainException.Message,
            StatusCode = domainException.StatusCode
        };

        context.Result = new ViewResult
        {
            ViewName = "DomainError",
            StatusCode = domainException.StatusCode,
            ViewData = new ViewDataDictionary<ErrorViewModel>(_metadataProvider, context.ModelState) { Model = model }
        };
        context.ExceptionHandled = true;
    }
}
