namespace CarePath.Services;

public interface ICurrentUser
{
    string UserName { get; }
}

public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string UserName => _accessor.HttpContext?.User.Identity?.Name ?? "system";
}
