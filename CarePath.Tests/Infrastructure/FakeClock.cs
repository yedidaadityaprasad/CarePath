using CarePath.Services;

namespace CarePath.Tests.Infrastructure;

public sealed class FakeClock : IClock
{
    public FakeClock(DateTime now)
    {
        Now = now;
    }

    public DateTime Now { get; set; }

    public void Advance(TimeSpan by) => Now = Now.Add(by);
}
