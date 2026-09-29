namespace TicketWorld.Application.Common.Interfaces;

/// <summary>
/// Distributed lock service (RedLock) for high-concurrency seat reservation and financial mutations.
/// </summary>
public interface IDistributedLockService
{
    Task<IDisposable?> AcquireLockAsync(
        string resourceKey,
        TimeSpan expiryTime,
        TimeSpan waitTime,
        TimeSpan retryTime,
        CancellationToken cancellationToken = default);

    Task<bool> ReleaseLockAsync(string resourceKey, CancellationToken cancellationToken = default);
}
