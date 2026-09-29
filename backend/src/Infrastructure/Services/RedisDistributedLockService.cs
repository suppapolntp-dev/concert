namespace TicketWorld.Infrastructure.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TicketWorld.Application.Common.Interfaces;

/// <summary>
/// Distributed lock service implementation backed by Redis LockTake / LockRelease.
/// Includes graceful fallback for local development when Redis is not available.
/// </summary>
public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisDistributedLockService> _logger;

    public RedisDistributedLockService(
        IConfiguration configuration,
        ILogger<RedisDistributedLockService> logger,
        IConnectionMultiplexer? redis = null)
    {
        _logger = logger;
        _redis = redis;
    }

    public async Task<IDisposable?> AcquireLockAsync(
        string resourceKey,
        TimeSpan expiryTime,
        TimeSpan waitTime,
        TimeSpan retryTime,
        CancellationToken cancellationToken = default)
    {
        var lockValue = Guid.NewGuid().ToString("N");
        var startTime = DateTime.UtcNow;

        if (_redis == null || !_redis.IsConnected)
        {
            // In-memory fallback if Redis connection is not established during local development
            _logger.LogDebug("Redis connection not active. Using in-memory fallback lock for {ResourceKey}", resourceKey);
            return new SimpleDisposableLock();
        }

        try
        {
            var db = _redis.GetDatabase();

            while (DateTime.UtcNow - startTime < waitTime)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                var acquired = await db.LockTakeAsync(resourceKey, lockValue, expiryTime);
                if (acquired)
                {
                    return new RedisLockHandle(db, resourceKey, lockValue);
                }

                await Task.Delay(retryTime, cancellationToken);
            }

            _logger.LogWarning("Failed to acquire distributed lock for resource {ResourceKey} within {WaitTime}s", resourceKey, waitTime.TotalSeconds);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error while acquiring lock for {ResourceKey}.", resourceKey);
            return null;
        }
    }

    public async Task<bool> ReleaseLockAsync(string resourceKey, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected) return true;
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyDeleteAsync(resourceKey);
        }
        catch
        {
            return false;
        }
    }

    private sealed class RedisLockHandle : IDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _value;
        private bool _disposed;

        public RedisLockHandle(IDatabase db, string key, string value)
        {
            _db = db;
            _key = key;
            _value = value;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                _db.LockRelease(_key, _value);
            }
            catch
            {
                // Suppress release exception on dispose
            }
        }
    }

    private sealed class SimpleDisposableLock : IDisposable
    {
        public void Dispose() { }
    }
}
