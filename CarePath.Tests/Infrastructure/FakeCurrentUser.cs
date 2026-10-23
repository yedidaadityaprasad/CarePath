using CarePath.Services;

namespace CarePath.Tests.Infrastructure;

public sealed class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(string userName)
    {
        UserName = userName;
    }

    public string UserName { get; set; }
}
