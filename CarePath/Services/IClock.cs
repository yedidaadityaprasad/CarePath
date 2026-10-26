namespace CarePath.Services;

/// <summary>
/// Abstraction over the system clock. Kept for backward compatibility and testability.
/// In .NET 10, prefer injecting <see cref="System.TimeProvider"/> directly for new code.
/// </summary>
public interface IClock
{
    DateTime Now { get; }
}

/// <summary>
/// Production implementation backed by <see cref="TimeProvider.System"/>.
/// .NET 10: delegates to TimeProvider so the abstraction stays testable via FakeTimeProvider.
/// </summary>
public class SystemClock : IClock
{
    private readonly TimeProvider _timeProvider;

    public SystemClock(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DateTime Now => _timeProvider.GetLocalNow().DateTime;
}
