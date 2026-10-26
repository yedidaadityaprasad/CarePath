namespace CarePath.Services;

/// <summary>
/// Provides the identity of the currently authenticated user.
/// </summary>
public interface ICurrentUser
{
    string UserName { get; }
}

/// <summary>
/// .NET 10: Uses <see cref="IHttpContextAccessor"/> to resolve the current user from the
/// ambient HTTP context. Registered as Scoped in Program.cs.
/// </summary>
public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string UserName => _accessor.HttpContext?.User.Identity?.Name ?? "system";
}
