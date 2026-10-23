using System.ComponentModel.DataAnnotations;

namespace CarePath.Models.ViewModels;

public interface IPagedResult
{
    int Page { get; }
    int TotalPages { get; }
    int TotalCount { get; }
    bool HasPrevious { get; }
    bool HasNext { get; }
}

public class PagedResult<T> : IPagedResult
{
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Empty(int pageSize = 25) => new(Array.Empty<T>(), 1, pageSize, 0);
}

/// <summary>Posted by status-change buttons that must record why the change was made.</summary>
public class ReasonActionViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A reason is required.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "The reason must be between 3 and 500 characters.")]
    public string? Reason { get; set; }
}

/// <summary>Used by the shared status-action partial to render a POST form for a status change.</summary>
public class StatusActionFormModel
{
    public string Action { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public int Id { get; set; }
    public string ButtonText { get; set; } = string.Empty;
    public string ButtonClass { get; set; } = "btn-outline-secondary";
    public bool RequireReason { get; set; }
    public string? ConfirmMessage { get; set; }
    public ReasonActionViewModel Input { get; set; } = new();
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string Title { get; set; } = "Something went wrong";
    public string Message { get; set; } = "An unexpected error occurred while processing your request.";
    public int StatusCode { get; set; } = 500;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
