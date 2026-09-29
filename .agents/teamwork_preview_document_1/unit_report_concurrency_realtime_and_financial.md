# Summary

This unit report delivers an exhaustive mathematical and technical audit of Track 2: **High-Concurrency Workflows, Real-Time Synchronization, and Financial Integrity** across the TicketWorld specification suite (`Plan/01_concert_requirement_spec.md`, `Plan/03_docker_infra_plan.md`, `Plan/06_entity_db_plan.md`, `Plan/08_realtime_concurrency_plan.md`, and `Plan/09_payment_reconciliation_plan.md`).

The audit scrutinizes the complete end-to-end lifecycle of high-demand concert ticket on-sales: from ingress traffic buffering via Redis Virtual Waiting Room, microsecond distributed seat locking (RedLock) and database persistence (PostgreSQL transactions, `xmin` optimistic concurrency, and row-level locking), to asynchronous payment webhook reconciliation across Stripe/Omise, financial idempotency, and background expiry workers.

### Key Audit Findings Overview:
1. **Critical Financial & Domain Invariants Broken in Payment Reconciliation:** 
   - In `Plan/09_payment_reconciliation_plan.md`, Scenario 3 (Grace window reclamation) calls `seat.ConfirmBooking()` on seats with status `Available`. However, the domain entity `Seat.cs` in `Plan/06_entity_db_plan.md` explicitly throws a `DomainException("Cannot book a seat that is not held.")` when `ConfirmBooking()` is called on an `Available` seat. This causes an unhandled exception, rolls back the transaction, and leaves the paying customer charged with neither tickets nor a refund.
   - Partial seat matching in multi-seat bookings drops unfulfilled bookings into auto-refund while stranding held seats in an orphaned `Held` state indefinitely.
   - Outbound HTTP calls to payment gateways (`_refundService.ExecuteAutoRefundAsync`) are executed inside an active `Serializable` database transaction, exposing the system to transaction lock exhaustion, serialization deadlocks (`40001`), and duplicate refund issuance upon transaction rollback.
2. **Infinite Seat Lockout in Background Expiry Worker:**
   - In `Plan/08_realtime_concurrency_plan.md`, the Hangfire seat expiry query excludes seats where `payment.status IN ('Pending', 'Processing')` without enforcing a bounded cutoff. If a customer initiates checkout and abandons the browser, the seat is permanently excluded from expiry reclamation and remains locked forever.
3. **Queue Invariants & Microsecond Concurrency Flaws:**
   - In the Virtual Waiting Room (`Plan/08`), storing queue position via bare Unix millisecond timestamps in Redis Sorted Sets causes score collisions under 50,000-user surges, leading to lexicographical UUID sorting bias instead of FIFO fairness.
   - The 3-second RedLock lease TTL in `Plan/08` lacks an auto-renewal loop and is decoupled from database row locks. Under connection pool saturation or disk I/O latency exceeding 3 seconds, the lock expires before the reservation commits, creating a race condition where a second buyer reads `Available` status and acquires the same seat.
   - Running RedLock against a single-container Redis deployment (`Plan/03`) invalidates the mathematical fault-tolerance claims of the RedLock consensus algorithm.
4. **Missing Database Persistence & Schema Contracts:**
   - The `webhook_events` table mandated by `Plan/01` and `Plan/09` for idempotent webhook ingestion and replay protection is completely missing from `Plan/06_entity_db_plan.md`.
   - `Seat.cs` fails to clear `PaymentIntentId` on re-holding, leading to cross-user payment contamination.

---

# Potential Mistakes and Improvements

## 1. Virtual Waiting Room & Queue Integrity

### [CRITICAL] Redis Sorted Set Score Collision and Lexicographical Queue Jump
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §1.1 & §1.2; `Plan/01_concert_requirement_spec.md` §3 (Module: Virtual Waiting Room)
- **Specific Flaw:**
  Plan 08 §1.2 specifies queue insertion as:
  ```redis
  ZADD waiting_room:queue:{eventId} {currentTimestamp} {userId}
  ```
  When 50,000 concurrent users hit the waiting room at `sale_start`, thousands of requests arrive within the exact same millisecond. In Redis Sorted Sets, when elements have identical scores, Redis breaks ties by comparing member values lexicographically (`userId` string). Because `userId` is a UUIDv7 or UUIDv4, users whose UUIDs start with `0` or `1` are systematically queued ahead of users whose UUIDs start with `e` or `f`, violating FIFO fairness.
  Furthermore, `Plan/01` line 737 suggests using `score = enqueue time + jitter`. Adding random jitter causes earlier arrivals to be placed behind later arrivals, causing non-deterministic queue position jumps.
  Additionally, calling plain `ZADD` without the `NX` flag allows retrying clients to overwrite their existing score with `{currentTimestamp}`, inadvertently punishing them by sending them to the very end of the queue.
- **Diff / Code Fix Recommendation:**
  Use an atomic Redis Lua script that uses a high-resolution sub-millisecond sequence generator (`INCR`) to create a 64-bit monotonic score: `score = (timestamp * 1000000) + (counter % 1000000)`, and enforce `NX` (do not update score if member already exists):

```lua
-- File: src/Infrastructure/Redis/Scripts/EnqueueWaitingRoom.lua
-- KEYS[1]: waiting_room:queue:{eventId}
-- KEYS[2]: waiting_room:counter:{eventId}
-- ARGV[1]: current_unix_millis
-- ARGV[2]: user_id

-- 1. Check if user already queued
local existingScore = redis.call('ZSCORE', KEYS[1], ARGV[2])
if existingScore then
    local rank = redis.call('ZRANK', KEYS[1], ARGV[2])
    return { 0, rank } -- Already queued, return current rank
end

-- 2. Increment tie-breaker sequence counter
local seq = redis.call('INCR', KEYS[2])
if seq == 1 then
    redis.call('EXPIRE', KEYS[2], 86400) -- 24h TTL
end

-- 3. Composite score: (millis * 1,000,000) + (seq % 1,000,000)
local compositeScore = (tonumber(ARGV[1]) * 1000000) + (seq % 1000000)

redis.call('ZADD', KEYS[1], 'NX', compositeScore, ARGV[2])
local rank = redis.call('ZRANK', KEYS[1], ARGV[2])
return { 1, rank }
```

---

### [CRITICAL] Queue Dequeue Disconnect & Orphaned Polling State
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §1.2 (#3 Gatekeeper Worker); `Plan/07_api_endpoint_plan.md` §5
- **Specific Flaw:**
  Plan 08 specifies that the Gatekeeper Worker releases batches (e.g., 50 users/second) from `waiting_room:queue:{eventId}` and generates a bypass token. However, if the worker removes users from the Sorted Set via `ZPOPMIN`, the user's subsequent polling call to `GET /api/v1/queue/position` executes `ZRANK`, which immediately returns `nil` (null).
  The specification provides no transition state or storage mechanism for admitted users awaiting token retrieval. The client cannot distinguish between:
  1. "You were admitted to enter the seat map",
  2. "You were evicted for missing heartbeats", and
  3. "You were never in the queue".
  Moreover, Plan 08 mentions that Hangfire executes the Gatekeeper Worker. Hangfire's default polling loop is 15 seconds. A recurring job in Hangfire cannot guarantee 50 users/second smooth admission; it would release 750 users every 15 seconds, creating periodic thundering herds against PostgreSQL.
- **Diff / Code Fix Recommendation:**
  1. Replace Hangfire with an ASP.NET Core `IHostedService` (`BackgroundService`) running a dedicated token-bucket rate limiter ticking at 1-second intervals.
  2. Maintain a secondary Redis Sorted Set `waiting_room:admitted:{eventId}` with an expiration score (`score = admittedTimestamp + 60s`).
  3. Admitted users are atomically moved from `queue` to `admitted`. When `GET /queue/position` runs, it checks `waiting_room:admitted:{eventId}`: if present, it returns status `ADMITTED` and issues the `seatmap_pass`.

```csharp
// File: src/Infrastructure/WaitingRoom/WaitingRoomGatekeeperService.cs
public class WaitingRoomGatekeeperService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<WaitingRoomGatekeeperService> _logger;
    private const int ADMISSION_RATE_PER_SECOND = 50;

    public WaitingRoomGatekeeperService(IConnectionMultiplexer redis, ILogger<WaitingRoomGatekeeperService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        var db = _redis.GetDatabase();

        var admitScript = LuaScript.Prepare(@"
            -- KEYS[1]: queue, KEYS[2]: admitted
            -- ARGV[1]: batch_size, ARGV[2]: expiry_timestamp
            local admitted = redis.call('ZPOPMIN', KEYS[1], ARGV[1])
            for i = 1, #admitted, 2 do
                local userId = admitted[i]
                redis.call('ZADD', KEYS[2], ARGV[2], userId)
            end
            return #admitted / 2
        ");

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var activeEvents = await db.SetMembersAsync("waiting_room:active_events");
            foreach (var eventId in activeEvents)
            {
                var queueKey = $"waiting_room:queue:{eventId}";
                var admittedKey = $"waiting_room:admitted:{eventId}";
                var expiry = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds();

                await db.ScriptEvaluateAsync(admitScript, new {
                    batch_size = ADMISSION_RATE_PER_SECOND,
                    expiry_timestamp = expiry
                });
            }
        }
    }
}
```

---

### [HIGH] Waiting Room Token Security: Missing Claims and Replay Vulnerability
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §1.2 (#4); `Plan/01_concert_requirement_spec.md` §1.3 & §3
- **Specific Flaw:**
  Plan 08 specifies that mutating seat endpoints (`/seats/hold`) require header `X-WaitingRoom-Token`. However, neither Plan 08 nor Plan 01 specifies the cryptographic payload structure. If the token is simply signed without binding to the user's identity and specific event:
  1. **Event Hopping:** A user can queue for a low-demand event (e.g., local indie show with 0 wait time), obtain a valid pass, and use that token to hold seats at a sold-out stadium concert (`Taylor Swift`).
  2. **Token Hijacking & Scalper Trading:** Scalper bots can farm tokens using disposable IP addresses and inject the JWTs into buyer sessions.
  3. **Multi-Use Replay:** Without recording token redemption (`jti`), a user can share the token with multiple browser tabs to hold multiple seat batches simultaneously.
- **Diff / Code Fix Recommendation:**
  Define the strict JWT schema for `X-WaitingRoom-Token` and enforce verification in `WaitingRoomAuthorizationMiddleware`:

```csharp
// File: src/Infrastructure/Security/WaitingRoomTokenService.cs
public class WaitingRoomTokenClaims
{
    public const string EventId = "ev_id";
    public const string UserId = "sub";
    public const string Jti = "jti";
    public const string IssuedAt = "iat";
    public const string ExpiresAt = "exp";
}

// File: src/WebApi/Middleware/WaitingRoomValidationMiddleware.cs
public async Task InvokeAsync(HttpContext context, IConnectionMultiplexer redis, ICurrentUserService currentUser)
{
    var endpoint = context.GetEndpoint();
    var requiresQueue = endpoint?.Metadata.GetMetadata<RequireWaitingRoomPassAttribute>();

    if (requiresQueue != null)
    {
        if (!context.Request.Headers.TryGetValue("X-WaitingRoom-Token", out var tokenStr))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "QUEUE_REQUIRED",
                Detail = "Valid waiting room pass is required to hold seats."
            });
            return;
        }

        var principal = _tokenService.ValidateWaitingRoomToken(tokenStr!);
        var tokenUserId = Guid.Parse(principal.FindFirst(WaitingRoomTokenClaims.UserId)!.Value);
        var tokenEventId = Guid.Parse(principal.FindFirst(WaitingRoomTokenClaims.EventId)!.Value);
        var jti = principal.FindFirst(WaitingRoomTokenClaims.Jti)!.Value;

        var routeEventId = Guid.Parse(context.Request.RouteValues["eventId"]?.ToString() 
            ?? context.Request.Query["eventId"].ToString());

        if (tokenUserId != currentUser.UserId || tokenEventId != routeEventId)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }
    }
    await _next(context);
}
```

---

### [HIGH] Heartbeat Rate Mismatch and Starvation Pruning Race Condition
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §1.2 (#2); `Plan/01_concert_requirement_spec.md` §1.3 (Rate Limit Table) & §3
- **Specific Flaw:**
  Plan 08 §1.2 #2 states: *"Client polls every 3 seconds. Updates heartbeat score. Clients missing 3 consecutive heartbeats (9s) are pruned from the queue."*
  However:
  1. In `Plan/01_concert_requirement_spec.md` §3 (Module: Virtual Waiting Room, line 734), it states: *"ดูลำดับคิว + ค่า wait_seconds ประมาณการ (heartbeat 30s)"*.
  2. In `Plan/07_api_endpoint_plan.md` §1.2, the rate limit is set to `30 req / 1 min per User` (1 request per 2 seconds).
  3. A 9-second timeout threshold is extremely dangerous on mobile networks. In real-world cellular environments (LTE/5G handovers, tunnels, or radio power saving states), round-trip packet delay easily spikes above 9 seconds. Legitimate users waiting in queue for 45 minutes will suffer sudden evictions.
  4. Performing heartbeat cleanup via non-atomic operations (`ZRANGEBYSCORE` followed by `ZREM`) opens a race condition: a client sends a legitimate heartbeat between the read and delete operations, resulting in the active user being purged.
- **Diff / Code Fix Recommendation:**
  Standardize the polling interval to 5 seconds, set the eviction threshold to 30 seconds (6 missed intervals), and execute cleanup via an atomic Lua script:

```lua
-- File: src/Infrastructure/Redis/Scripts/PruneQueueHeartbeats.lua
-- KEYS[1]: waiting_room:heartbeat:{eventId}
-- KEYS[2]: waiting_room:queue:{eventId}
-- ARGV[1]: cutoff_timestamp (now - 30 seconds)

local expiredUsers = redis.call('ZRANGEBYSCORE', KEYS[1], '-inf', ARGV[1])
if #expiredUsers > 0 then
    redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', ARGV[1])
    for _, userId in ipairs(expiredUsers) do
        redis.call('ZREM', KEYS[2], userId)
    end
end
return #expiredUsers
```

---

## 2. Microsecond Seat Locking: RedLock vs Database Transactions

### [CRITICAL] 3-Second RedLock Expiry vs Database Transaction Race Window
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §2.1; `Plan/06_entity_db_plan.md` §4.1
- **Specific Flaw:**
  In Plan 08 §2.1, the RedLock lock acquisition uses a fixed 3-second TTL:
  ```csharp
  await using var redLock = await _redisLockFactory.CreateLockAsync(lockKey, TimeSpan.FromSeconds(3));
  ```
  During on-sale traffic spikes, thread pool starvation in .NET, connection pooling latency in PgBouncer/PostgreSQL, or WAL write checkpoints can cause the execution between line 69 and line 87 to exceed 3.0 seconds.
  When 3.001 seconds elapse:
  1. The Redis key `lock:seat:{seatId}` automatically expires.
  2. Request B for the exact same seat acquires the lock immediately.
  3. Request B executes line 79: `_dbContext.Seats.FirstOrDefaultAsync(s => s.Id == seatId, ct)`. Because Request A has not yet committed (`await tx.CommitAsync`), PostgreSQL default isolation level (`READ COMMITTED`) serves the unmodified row where `Status == Available`.
  4. Both Request A and Request B now believe they have legitimately acquired the seat.
  5. While EF Core's `xmin` token might throw `DbUpdateConcurrencyException` when Request B tries to commit, Request A may have already sent a SignalR broadcast or returned a successful 200 OK to User A, or vice-versa, resulting in user-facing race conditions and unhandled 500 errors.
- **Diff / Code Fix Recommendation:**
  1. Implement RedLock automatic lease extension (heartbeat/renewal timer) that keeps extending the Redis lock every 1 second while the C# database transaction is alive.
  2. Combine RedLock with PostgreSQL row-level pessimistic locking (`SELECT ... FOR UPDATE NOWAIT`) inside the transaction to ensure that the database rejects concurrent attempts at the storage engine level:

```csharp
// File: src/Infrastructure/Services/SeatReservationService.cs
public async Task<HoldSeatResult> HoldSeatAsync(Guid seatId, Guid userId, CancellationToken ct)
{
    var lockKey = $"lock:seat:{seatId}";
    
    // Acquire RedLock with 5s lease and auto-extension
    var expiry = TimeSpan.FromSeconds(5);
    var waitTime = TimeSpan.FromMilliseconds(500);
    var retryTime = TimeSpan.FromMilliseconds(50);

    await using var redLock = await _redisLockFactory.CreateLockAsync(lockKey, expiry, waitTime, retryTime, ct);
    if (!redLock.IsAcquired)
    {
        throw new ConcurrencyConflictException("Seat is currently being acquired by another user.");
    }

    await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
    
    // Pessimistic lock NOWAIT prevents relying purely on Redis TTL
    var seat = await _dbContext.Seats
        .FromSqlInterpolated($"SELECT * FROM seats WHERE id = {seatId} FOR UPDATE NOWAIT")
        .FirstOrDefaultAsync(ct);

    if (seat == null || seat.Status != SeatStatus.Available)
    {
        throw new SeatUnavailableException("Seat is not available.");
    }

    seat.Hold(userId, TimeSpan.FromMinutes(10));
    await _dbContext.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);

    // Publish to Outbox or broadcast with version
    await _hubContext.Clients.Group(seat.EventId.ToString())
        .SendAsync("SeatStatusChanged", new SeatStatusChangedDto(seat.Id, "Held", seat.Version), ct);

    return new HoldSeatResult(seat.Id, seat.HeldUntil!.Value);
}
```

---

### [CRITICAL] Architectural Inconsistency: Pessimistic Row Locking vs Optimistic `xmin` vs RedLock
- **File & Section:** `Plan/01_concert_requirement_spec.md` §5.2; `Plan/06_entity_db_plan.md` §4.1; `Plan/08_realtime_concurrency_plan.md` §2.1
- **Specific Flaw:**
  The specifications are divided across three conflicting concurrency strategies:
  1. `Plan/01_concert_requirement_spec.md` §5.2 mandates raw SQL pessimistic row locking:
     `SELECT id, status, version FROM seats WHERE id = ANY(@seatIds) AND status = 'Available' FOR UPDATE NOWAIT;`
  2. `Plan/06_entity_db_plan.md` §4.1 configures optimistic concurrency via PostgreSQL `xmin`:
     `builder.UseXminAsConcurrencyToken();`
  3. `Plan/08_realtime_concurrency_plan.md` §2.1 uses a plain LINQ query `FirstOrDefaultAsync` without `FOR UPDATE`, relying entirely on Redis RedLock.
  
  If the platform relies purely on `xmin`, under high contention hundreds of concurrent requests bypass Redis and slam PostgreSQL with update retries, resulting in severe CPU spikes and serialization aborts. If it relies purely on Redis RedLock, any Redis network partition or restart causes double booking because the database layer does not enforce row locks.
- **Diff / Code Fix Recommendation:**
  Harmonize into a formal **Two-Tier Concurrency Architecture**:
  - **Tier 1 (Application Edge / Redis):** Redis Distributed Lock rejects 99.9% of concurrent attempts in memory in sub-milliseconds with `409 Conflict`.
  - **Tier 2 (Storage Invariant / PostgreSQL):** EF Core executes `FOR UPDATE NOWAIT`. If Tier 1 experiences a network partition, PostgreSQL catches the race with error code `55P03` (`lock_not_available`) and aborts immediately without deadlocks. `xmin` remains as the final safety barrier for entity modifications.

---

### [CRITICAL] Multi-Seat Distributed Deadlock Vulnerability
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §2.1; `Plan/01_concert_requirement_spec.md` §1.2 & §5.2
- **Specific Flaw:**
  Plan 08 §2.1 defines `HoldSeatAsync(Guid seatId, ...)` handling only a single seat. However, concertgoers purchase 2 to 4 adjacent seats in a single checkout.
  If User 1 attempts to hold `[Seat A, Seat B]` and User 2 attempts to hold `[Seat B, Seat A]`:
  1. User 1 acquires Redis lock for `Seat A`.
  2. User 2 acquires Redis lock for `Seat B`.
  3. User 1 attempts to lock `Seat B` and blocks or fails.
  4. User 2 attempts to lock `Seat A` and blocks or fails.
  This results in a classic distributed deadlock or partial allocation (e.g., User 1 holds Seat A, but Seat B fails, leaving an orphaned hold).
- **Diff / Code Fix Recommendation:**
  Define a bulk seat holding command that enforces **Canonical Lexicographical Key Ordering** before lock acquisition, and acquires multi-key locks atomically via Lua:

```csharp
// File: src/Infrastructure/Services/SeatReservationService.cs
public async Task<HoldSeatsResult> HoldSeatsAsync(List<Guid> seatIds, Guid userId, CancellationToken ct)
{
    // 1. Enforce strict canonical ordering to eliminate circular wait deadlocks
    var orderedSeatIds = seatIds.Distinct().OrderBy(id => id).ToList();
    var lockKeys = orderedSeatIds.Select(id => $"lock:seat:{id}").ToArray();

    // 2. Multi-lock acquisition via RedLock or atomic Redis Lua script
    await using var multiLock = await _redisLockFactory.CreateMultiLockAsync(lockKeys, TimeSpan.FromSeconds(5), ct);
    if (!multiLock.IsAcquired)
    {
        throw new ConcurrencyConflictException("One or more selected seats are currently being acquired by another user.");
    }

    await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

    var seats = await _dbContext.Seats
        .FromSqlInterpolated($"SELECT * FROM seats WHERE id = ANY({orderedSeatIds.ToArray()}) FOR UPDATE NOWAIT")
        .ToListAsync(ct);

    if (seats.Count != orderedSeatIds.Count || seats.Any(s => s.Status != SeatStatus.Available))
    {
        throw new SeatUnavailableException("One or more seats are no longer available.");
    }

    foreach (var seat in seats)
    {
        seat.Hold(userId, TimeSpan.FromMinutes(10));
    }

    await _dbContext.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);

    return new HoldSeatsResult(seats.Select(s => s.Id).ToList(), seats.First().HeldUntil!.Value);
}
```

---

### [HIGH] Architectural Contradiction: RedLock on Single Redis Instance
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §2; `Plan/03_docker_infra_plan.md` §2 & §3; `Plan/01_concert_requirement_spec.md` §6.7
- **Specific Flaw:**
  Plan 08 explicitly prescribes the RedLock distributed locking algorithm via `_redisLockFactory.CreateLockAsync`.
  By mathematical definition (Salvatore Sanfilippo, RedLock algorithm), RedLock requires at least $N \ge 3$ independent Redis masters (typically 5) to achieve consensus quorum ($N/2 + 1$).
  However:
  - `Plan/03_docker_infra_plan.md` defines only a **single Redis container** (`ticketworld-redis`).
  - `Plan/01_concert_requirement_spec.md` §6.7 specifies "Redis: Sentinel mode (3 nodes)".
  
  Running RedLock against a single Redis instance incurs multi-node algorithmic overhead with zero fault tolerance. Moreover, running RedLock on Redis Sentinel with asynchronous master-replica replication is mathematically unsound: if the master crashes before replicating the lock to the replica, failover promotes the replica, and a second client acquires the identical lock, violating mutual exclusion.
- **Diff / Code Fix Recommendation:**
  In `Plan/08` and `Plan/03`:
  1. Clearly state that for local development and single-node setups, a standard atomic Redis Mutex (`SET key value NX PX ttl` with atomic Lua release) is used.
  2. For high-availability production environments requiring RedLock, deploy 3 independent Redis master instances, or document that Sentinel replication lag is mitigated by database Tier 2 locks (`FOR UPDATE NOWAIT`).

---

### [HIGH] Non-Transactional SignalR Broadcast & Missing Version Ordering
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §2.1 lines 90-91 & §3.2; `Plan/01_concert_requirement_spec.md` §3 (Module: SignalR Hub)
- **Specific Flaw:**
  In Plan 08 lines 90-91:
  ```csharp
  await tx.CommitAsync(ct);
  await _hubContext.Clients.Group(seat.EventId.ToString())
      .SendAsync("SeatStatusChanged", new { SeatId = seat.Id, Status = "Held" }, ct);
  ```
  1. If the API process terminates (crash, container restart, OOM) immediately after `tx.CommitAsync`, the seat status is committed in PostgreSQL, but the SignalR message is lost. All connected clients continue viewing the seat as Available.
  2. In Plan 01 line 874, the spec mandates: *"event ที่ broadcast มีแค่ `seatId, newStatus, version`"*. But in Plan 08 line 91, `version` is completely omitted. Without a monotonic version or sequence number, WebSocket frames delivered out of order due to network jitter cause clients to overwrite newer states with older ones (e.g., displaying a "Held" seat as "Available" after a rapid hold-release-hold sequence).
- **Diff / Code Fix Recommendation:**
  Include entity `Version` in all SignalR broadcast payloads, and dispatch the event via the Outbox Pattern or EF Core `SavedChangesInterceptor`:

```csharp
// File: src/Application/Common/Models/SeatStatusChangedDto.cs
public record SeatStatusChangedDto(Guid SeatId, string Status, uint Version, DateTime Timestamp);

// File: src/Infrastructure/Hubs/SeatMapHub.cs
public async Task BroadcastSeatStatusAsync(Guid eventId, Guid seatId, SeatStatus status, uint version)
{
    await _hubContext.Clients.Group(eventId.ToString()).SendAsync("SeatStatusChanged", 
        new SeatStatusChangedDto(seatId, status.ToString(), version, DateTime.UtcNow));
}
```

---

## 3. Payment Webhook Races & Financial Integrity

### [CRITICAL] Fatal Domain Exception in Scenario 3 Grace Window Reclamation
- **File & Section:** `Plan/09_payment_reconciliation_plan.md` §3.1 lines 90-95; `Plan/06_entity_db_plan.md` §3 lines 138-145
- **Specific Flaw:**
  In Plan 09 §3.1, Scenario 3 handles payments where the hold expired but the seat was not taken by anyone else (`seatsStillAvailable == true`):
  ```csharp
  else if (seatsStillAvailable && (paidAt - booking.HeldUntil!.Value).TotalSeconds <= 90)
  {
      // Scenario 3: Grace window reclamation
      seats.ForEach(s => s.ConfirmBooking());
      booking.MarkConfirmed();
      await IssueTicketQRsAsync(booking, ct);
  }
  ```
  However, in `Plan/06_entity_db_plan.md` §3 (lines 138-142), `Seat.ConfirmBooking()` is defined as:
  ```csharp
  public void ConfirmBooking()
  {
      if (Status != SeatStatus.Held)
          throw new DomainException("Cannot book a seat that is not held.");

      Status = SeatStatus.Booked;
      HeldUntil = null;
  }
  ```
  In Scenario 3, `seatsStillAvailable` evaluates to `true` **specifically because `s.Status == SeatStatus.Available`**!
  Invoking `s.ConfirmBooking()` on an `Available` seat **unconditionally throws `DomainException`**!
  The transaction is aborted and rolled back. The customer's card has been charged by Stripe, but the webhook handler crashes with a 500 Internal Server Error. The customer receives no tickets, the booking remains stuck in `Pending`, and the auto-refund is never triggered.
- **Diff / Code Fix Recommendation:**
  Add a dedicated domain method `ReclaimAndConfirmFromGracePeriod()` to `Seat.cs` that allows transitioning from `Available` or expired `Held` directly to `Booked`:

```csharp
// File: src/Domain/Entities/Seat.cs
public void ReclaimAndConfirmFromGracePeriod(Guid userId, string paymentIntentId)
{
    if (Status != SeatStatus.Available && !(Status == SeatStatus.Held && HeldByUserId == userId))
    {
        throw new DomainException("Seat is not eligible for grace-period reclamation.");
    }

    Status = SeatStatus.Booked;
    HeldByUserId = userId;
    PaymentIntentId = paymentIntentId;
    HeldUntil = null;
}
```

And update `ReconcileSuccessfulPaymentAsync` in `Plan/09_payment_reconciliation_plan.md`:
```csharp
// File: src/Infrastructure/Services/PaymentReconciliationService.cs
else if (seatsStillAvailable && IsWithinGracePeriod(paidAt, booking.HeldUntil!.Value, 90))
{
    // Scenario 3: Grace window reclamation
    seats.ForEach(s => s.ReclaimAndConfirmFromGracePeriod(booking.UserId, paymentIntentId));
    booking.MarkConfirmed();
    await IssueTicketQRsAsync(booking, ct);
}
```

---

### [CRITICAL] Partial Seat Conflict and Orphaned Seat Holds in Multi-Seat Reconciliation
- **File & Section:** `Plan/09_payment_reconciliation_plan.md` §3.1 lines 80-102
- **Specific Flaw:**
  Plan 09 checks seat status using all-or-nothing boolean conditions:
  ```csharp
  bool allSeatsRetained = seats.All(s => s.Status == SeatStatus.Held && s.HeldByUserId == booking.UserId);
  bool seatsStillAvailable = seats.All(s => s.Status == SeatStatus.Available);
  ```
  Consider a multi-seat order with 2 seats (Seat 1 and Seat 2):
  - **Case A (Partial Steal):** Seat 1 is still `Held` by `booking.UserId`, but Seat 2 expired and was claimed by User B (`Status == Held`, `HeldByUserId != booking.UserId`).
    Both `allSeatsRetained` and `seatsStillAvailable` evaluate to `false`. The code drops into the `else` branch (Scenario 2): it cancels the booking and refunds the payment.
    **CRITICAL LEAK:** Seat 1 is **NEVER released**! It remains in `Held` status until the background Hangfire worker runs minutes later.
  - **Case B (Mixed Retention & Grace):** Seat 1 is still `Held` by `booking.UserId`, but Seat 2 expired and reverted to `Available` within the 90s grace window.
    Both `allSeatsRetained` and `seatsStillAvailable` evaluate to `false`. Even though ALL seats are legally available to this user, the system rejects the booking and initiates an unnecessary auto-refund!
- **Diff / Code Fix Recommendation:**
  Evaluate each seat individually using a unified predicate: a seat is reclaimable if it is still held by the user OR is currently available within the grace window. If ANY seat is taken by a third party, explicitly release all held seats belonging to this booking before triggering the auto-refund:

```csharp
// File: src/Infrastructure/Services/PaymentReconciliationService.cs
bool isGraceValid = (paidAt - booking.HeldUntil!.Value).TotalSeconds <= 90;

bool allSeatsClaimable = seats.All(s => 
    (s.Status == SeatStatus.Held && s.HeldByUserId == booking.UserId) ||
    (s.Status == SeatStatus.Available && isGraceValid));

if (allSeatsClaimable)
{
    foreach (var seat in seats)
    {
        if (seat.Status == SeatStatus.Held)
            seat.ConfirmBooking();
        else
            seat.ReclaimAndConfirmFromGracePeriod(booking.UserId, paymentIntentId);
    }
    booking.MarkConfirmed();
    await _outboxService.QueueEventAsync(new BookingConfirmedEvent(booking.Id), ct);
}
else
{
    // Scenario 2: One or more seats lost to another buyer
    // CRITICAL: Explicitly release any seats that were still held by this booking!
    var heldByUs = seats.Where(s => s.Status == SeatStatus.Held && s.HeldByUserId == booking.UserId);
    foreach (var seat in heldByUs)
    {
        seat.Release();
    }

    booking.MarkCancelled();
    await _outboxService.QueueCommandAsync(new ProcessAutoRefundCommand(
        booking.Id, 
        RefundReason.PaymentRaceLost, 
        paymentIntentId), ct);
}
```

---

### [CRITICAL] External HTTP Refund Call Inside Serializable Database Transaction
- **File & Section:** `Plan/09_payment_reconciliation_plan.md` §3.1 lines 71, 101, 104-106
- **Specific Flaw:**
  Plan 09 §3.1 opens a database transaction at `IsolationLevel.Serializable`:
  ```csharp
  await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
  ...
  await _refundService.ExecuteAutoRefundAsync(booking.Id, RefundReason.PaymentRaceLost, ct); // Outbound HTTP Call to Stripe!
  ...
  await _dbContext.SaveChangesAsync(ct);
  await tx.CommitAsync(ct);
  ```
  1. **Lock Contention:** Making an external HTTP request to Stripe/Omise takes between 300ms and 2,000ms. Holding a PostgreSQL Serializable transaction open across network I/O causes massive serialization deadlocks (`40001 serialization_failure`) and exhausts the database connection pool.
  2. **Double Refund Vulnerability:** If `_refundService.ExecuteAutoRefundAsync` succeeds at Stripe, but `_dbContext.SaveChangesAsync` or `tx.CommitAsync` subsequently fails (due to a serialization anomaly or transient DB error), the database transaction rolls back. In the database, `booking.Status` remains `Pending`. When Stripe retries the webhook notification, the method executes again, sees the un-cancelled booking, and invokes `ExecuteAutoRefundAsync` a second time, resulting in duplicate refunds.
- **Diff / Code Fix Recommendation:**
  Never invoke external payment gateway APIs inside database transactions. Record the refund intent in the database, commit the transaction, and dispatch the refund via an asynchronous background job or the Outbox Pattern with an explicit Stripe Idempotency Key (`idem_refund_{bookingId}`):

```csharp
// Inside ReconcileSuccessfulPaymentAsync:
booking.MarkCancelled();

var refund = new Refund(
    booking.Id, 
    booking.TotalAmount, 
    RefundReason.PaymentRaceLost, 
    idempotencyKey: $"refund_{paymentIntentId}");

_dbContext.Refunds.Add(refund);

await _dbContext.SaveChangesAsync(ct);
await tx.CommitAsync(ct);

// Enqueue external refund API call outside the DB transaction
_backgroundJobClient.Enqueue<IRefundExecutionJob>(job => 
    job.ExecuteStripeRefundAsync(refund.Id, CancellationToken.None));
```

---

### [CRITICAL] Infinite Seat Lockout Bug in Hangfire Expiry Worker Query
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §4.1 lines 120-132
- **Specific Flaw:**
  Plan 08 §4.1 specifies the Hangfire background expiry job query:
  ```sql
  SELECT s.id, s.event_id 
  FROM seats s
  LEFT JOIN payments p ON p.payment_intent_id = s.payment_intent_id
  WHERE s.status = 'Held' 
    AND s.held_until < NOW()
    AND (s.payment_intent_id IS NULL OR p.status NOT IN ('Pending', 'Processing'));
  ```
  The document states: *"If `payment_intent_id` is set and status is `Pending`: Grant a 60-second soft-extension before forcefully freeing the seat"*.
  However, notice the SQL `WHERE` clause:
  `(s.payment_intent_id IS NULL OR p.status NOT IN ('Pending', 'Processing'))`
  This clause completely **excludes** any seat whose associated payment is `Pending`.
  Because the query excludes them, the worker **never** fetches them, and **never** updates `s.held_until`.
  If a customer opens the payment modal (generating a `payment_intent_id`) and then abandons the transaction by closing their browser, `p.status` remains `'Pending'` forever.
  As a result, the seat is permanently excluded from the query, is never released, and **remains locked indefinitely**!
- **Diff / Code Fix Recommendation:**
  Incorporate the 60-second soft-extension cutoff directly into the SQL query:

```sql
-- Corrected Hangfire Seat Expiry Query
SELECT s.id, s.event_id, s.payment_intent_id
FROM seats s
LEFT JOIN payments p ON p.payment_intent_id = s.payment_intent_id
WHERE s.status = 'Held'
  AND (
    -- Case 1: No payment intent initiated, expired normally past 10 minutes
    (s.payment_intent_id IS NULL AND s.held_until < NOW())
    OR
    -- Case 2: Payment intent failed or cancelled, expired past normal window
    (p.status IN ('Failed', 'Cancelled') AND s.held_until < NOW())
    OR
    -- Case 3: Payment intent pending/processing, but exceeded 60s soft-extension window
    (p.status IN ('Pending', 'Processing') AND s.held_until + INTERVAL '60 seconds' < NOW())
  );
```

---

### [CRITICAL] Missing `webhook_events` Table in EF Core Entity Plan
- **File & Section:** `Plan/06_entity_db_plan.md` §2 & §4; `Plan/09_payment_reconciliation_plan.md` §2.1; `Plan/01_concert_requirement_spec.md` §3 (Module: Webhooks)
- **Specific Flaw:**
  Plan 01 (§3, line 855) and Plan 09 (§2.1 #2) require storing all inbound webhooks in `webhook_events` to ensure idempotency and prevent replay attacks.
  However, in `Plan/06_entity_db_plan.md` (the authoritative schema plan), the `WebhookEvent` entity is **completely omitted** from the Entity Registry (Section 2), Base Aggregates, and Fluent API configurations (Section 4).
  Without a dedicated database table with a compound unique index on `(gateway, event_id)`, concurrent webhook deliveries from Stripe will bypass the in-memory checks and execute parallel reconciliation workflows.
- **Diff / Code Fix Recommendation:**
  Add `WebhookEvent` to `Plan/06_entity_db_plan.md`:

```csharp
// File: src/Domain/Entities/WebhookEvent.cs
public class WebhookEvent : BaseEntity<Guid>, IAuditableEntity
{
    public string Gateway { get; private set; } = null!;
    public string EventId { get; private set; } = null!;
    public string EventType { get; private set; } = null!;
    public string PayloadJson { get; private set; } = null!;
    public DateTime? ProcessedAt { get; private set; }
    public bool IsProcessed => ProcessedAt.HasValue;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    private WebhookEvent() { }

    public WebhookEvent(string gateway, string eventId, string eventType, string payloadJson)
    {
        Id = Guid.NewGuid();
        Gateway = gateway;
        EventId = eventId;
        EventType = eventType;
        PayloadJson = payloadJson;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkProcessed() => ProcessedAt = DateTime.UtcNow;
}

// File: src/Infrastructure/Persistence/Configurations/WebhookEventConfiguration.cs
public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("webhook_events");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Gateway).HasMaxLength(50).IsRequired();
        builder.Property(w => w.EventId).HasMaxLength(100).IsRequired();
        builder.Property(w => w.EventType).HasMaxLength(100).IsRequired();
        builder.Property(w => w.PayloadJson).HasColumnType("jsonb").IsRequired();

        // Enforce deduplication at database level
        builder.HasIndex(w => new { w.Gateway, w.EventId }).IsUnique();
        builder.HasIndex(w => w.ProcessedAt).HasFilter("processed_at IS NULL");
    }
}
```

---

### [HIGH] Cross-User Payment Contamination via Dirty `PaymentIntentId`
- **File & Section:** `Plan/06_entity_db_plan.md` §3 (lines 128-136 & 147-154)
- **Specific Flaw:**
  In `Seat.cs`, `PaymentIntentId` is set when a user initiates checkout.
  However, look at the `Hold` method:
  ```csharp
  public void Hold(Guid userId, TimeSpan duration)
  {
      if (Status != SeatStatus.Available && !(Status == SeatStatus.Held && HeldUntil < DateTime.UtcNow))
          throw new DomainException("Seat is not available for holding.");

      Status = SeatStatus.Held;
      HeldByUserId = userId;
      HeldUntil = DateTime.UtcNow.Add(duration);
  }
  ```
  Notice that `PaymentIntentId` is **never reset** when a new hold is established!
  If User A holds a seat and initiates checkout (`PaymentIntentId = "pi_userA"`), but then lets the hold expire:
  When User B subsequently acquires the seat via `seat.Hold(userB, ...)`, the seat **still contains User A's `PaymentIntentId`**!
  When the Hangfire worker inspects the seat, it checks User A's payment intent instead of User B's, corrupting the soft-extension grace handling and linking User B's reservation to User A's payment.
- **Diff / Code Fix Recommendation:**
  In `Seat.Hold`, explicitly clear `PaymentIntentId`:

```csharp
// File: src/Domain/Entities/Seat.cs
public void Hold(Guid userId, TimeSpan duration)
{
    if (Status != SeatStatus.Available && !(Status == SeatStatus.Held && HeldUntil < DateTime.UtcNow))
        throw new DomainException("Seat is not available for holding.");

    Status = SeatStatus.Held;
    HeldByUserId = userId;
    HeldUntil = DateTime.UtcNow.Add(duration);
    PaymentIntentId = null; // Fix: Prevent cross-user payment intent leakage
}
```

---

### [HIGH] Timestamp Semantic Inversion in Grace Window Evaluation
- **File & Section:** `Plan/09_payment_reconciliation_plan.md` §3.1 line 90
- **Specific Flaw:**
  Plan 09 line 90 checks:
  ```csharp
  else if (seatsStillAvailable && (paidAt - booking.HeldUntil!.Value).TotalSeconds <= 90)
  ```
  Mathematically:
  1. If `paidAt` occurred *before* `HeldUntil` (e.g., paid at 10:09, held until 10:10), `(paidAt - booking.HeldUntil.Value).TotalSeconds` is a negative number (e.g. `-60`).
  2. `-60 <= 90` evaluates to `true`.
  3. However, if `paidAt` is an arbitrary invalid timestamp or a payment from days ago, `(paidAt - booking.HeldUntil.Value).TotalSeconds` remains negative and passes the `<= 90` condition!
  4. More critically, if `paidAt` and `HeldUntil` have mismatched `DateTimeKind` (e.g. UTC vs Local Time / Thailand UTC+7), the difference is `+25,200` seconds, causing valid payments to be rejected and refunded.
- **Diff / Code Fix Recommendation:**
  Implement a bounded, UTC-safe helper method:

```csharp
private static bool IsWithinGracePeriod(DateTime paidAtUtc, DateTime heldUntilUtc, int graceSeconds)
{
    var delta = paidAtUtc - heldUntilUtc;
    // Payment must have occurred after hold expiration, but within the grace window
    return delta.TotalSeconds >= 0 && delta.TotalSeconds <= graceSeconds;
}
```

---

### [MEDIUM] Ambiguous Concurrency Token: EF Core `xmin` vs Entity `Version` Property
- **File & Section:** `Plan/06_entity_db_plan.md` §2.3 (line 69), §3 (line 113), §4.1 (line 183)
- **Specific Flaw:**
  In `Seat.cs` line 113:
  `public uint Version { get; private set; }`
  In `SeatConfiguration.cs` line 183:
  `builder.UseXminAsConcurrencyToken();`
  In Npgsql EF Core, `UseXminAsConcurrencyToken()` configures PostgreSQL's internal system column `xmin` as a shadow property. It does not automatically bind to an application property named `Version`.
  As written, EF Core will generate a separate physical column `version integer` in the PostgreSQL `seats` table while also tracking `xmin` in the background. The `Version` property will remain at its default value (`0`), confusing developers and breaking queries that rely on `s.Version` for SignalR ordering.
- **Diff / Code Fix Recommendation:**
  Explicitly map `Version` to `xmin` in `SeatConfiguration`:

```csharp
// File: src/Infrastructure/Persistence/Configurations/SeatConfiguration.cs
builder.Property(s => s.Version)
    .HasColumnName("xmin")
    .HasColumnType("xid")
    .ValueGeneratedOnAddOrUpdate()
    .IsConcurrencyToken();
```

---

### [MEDIUM] Redis Container Memory Limit & Unsafe Eviction Policy
- **File & Section:** `Plan/03_docker_infra_plan.md` §3 (Service 2: `redis`)
- **Specific Flaw:**
  The Redis service command in `docker-compose.yml` is specified as:
  ```yaml
  command: >
    sh -c 'redis-server --requirepass "${REDIS_PASSWORD}" --appendonly yes'
  ```
  Neither `maxmemory` nor `maxmemory-policy` is configured.
  Under heavy on-sale load (50,000 waiting room entries, session data, lock keys, SignalR backplane buffers):
  1. Redis will consume host memory uncontrollably until the operating system's OOM killer terminates the Redis process.
  2. If Redis defaults to an LRU or volatile eviction policy, Redis will silently evict active RedLock keys or waiting room queue elements to free memory, causing silent double-booking race conditions.
- **Diff / Code Fix Recommendation:**
  Update `docker-compose.yml` to specify memory boundaries and enforce `noeviction`:

```yaml
  # 2. Redis Cache & Locking
  redis:
    image: redis:7-alpine
    container_name: ticketworld-redis
    restart: unless-stopped
    command: >
      sh -c 'redis-server --requirepass "${REDIS_PASSWORD}" --appendonly yes --maxmemory 1gb --maxmemory-policy noeviction'
    ports:
      - "${REDIS_PORT:-6379}:6379"
    volumes:
      - redis_data:/data
    networks:
      - ticketworld-network
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5
```

---

### [MEDIUM] Missing High-Performance Partial Index for Seat Expiry
- **File & Section:** `Plan/06_entity_db_plan.md` §4.1; `Plan/08_realtime_concurrency_plan.md` §4
- **Specific Flaw:**
  The Hangfire seat expiry job executes every 10 seconds with the predicate:
  `WHERE status = 'Held' AND held_until < NOW()`
  `SeatConfiguration` defines only:
  `builder.HasIndex(s => new { s.EventId, s.Status });`
  For an arena or stadium venue with 20,000 to 80,000 seats, executing this query every 10 seconds forces PostgreSQL to perform bitmap index scans across all held seats or sequential scans, creating unnecessary I/O overhead on primary database instances.
- **Diff / Code Fix Recommendation:**
  Add a PostgreSQL partial index in `SeatConfiguration.cs`:

```csharp
// File: src/Infrastructure/Persistence/Configurations/SeatConfiguration.cs
builder.HasIndex(s => s.HeldUntil)
    .HasDatabaseName("ix_seats_held_until_held_status")
    .HasFilter("status = 'Held'");
```

---

# Minor Corrections and Typos

### [LOW] Inconsistent Endpoint Route: `/api/v1/waitingroom` vs `/api/v1/queue`
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §1.2 vs `Plan/01_concert_requirement_spec.md` §3 vs `Plan/07_api_endpoint_plan.md` §5
- **Specific Flaw:**
  Plan 08 §1.2 refers to endpoints as:
  - `POST /api/v1/waitingroom/{eventId}/enter`
  - `GET /api/v1/waitingroom/{eventId}/position`
  
  However, `Plan/01_concert_requirement_spec.md` §3 (Module: Virtual Waiting Room) and `Plan/07_api_endpoint_plan.md` §5 formally define them as:
  - `POST /api/v1/queue/enter`
  - `GET /api/v1/queue/position`
  - `POST /api/v1/queue/claim-pass`
- **Correction:**
  Update `Plan/08_realtime_concurrency_plan.md` §1.2 to standardize on `/api/v1/queue/enter` and `/api/v1/queue/position`.

---

### [LOW] Discrepancy in Bypass Token Validity (15 Minutes vs 5 Minutes)
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §1.2 (#3) vs `Plan/01_concert_requirement_spec.md` §3 vs `Plan/07_api_endpoint_plan.md` §5
- **Specific Flaw:**
  Plan 08 §1.2 (#3) states: *"Generates a signed cryptographic bypass token valid for 15 minutes."*
  However, Plan 01 §3 (line 735) and Plan 07 §5 (line 107) state: *"แลก seatmap_pass (JWT อายุ 5 นาที, ผูก event)"*.
  A 15-minute token allows users who have exited checkout to retain bypass access and repeatedly hold seats during peak demand.
- **Correction:**
  Standardize token TTL across all documents to **5 minutes** (`TimeSpan.FromMinutes(5)`).

---

### [LOW] RedLock Release Script Incomplete Specification
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §2
- **Specific Flaw:**
  Plan 08 mentions the RedLock acquisition pattern in C#, but fails to specify the Lua script used for atomic lease release and extension. In distributed locking, releasing a lock using a plain `DEL` command creates a race condition where a process that took longer than the TTL deletes another client's newly acquired lock.
- **Correction:**
  Formally document the standard Redis atomic release Lua script in Plan 08 §2:
  ```lua
  if redis.call("get", KEYS[1]) == ARGV[1] then
      return redis.call("del", KEYS[1])
  else
      return 0
  end
  ```

---

### [LOW] SignalR Broadcast Message Contract Missing Version
- **File & Section:** `Plan/08_realtime_concurrency_plan.md` §3.2
- **Specific Flaw:**
  In the Real-Time Message Contracts table (§3.2), `SeatStatusChanged` payload is listed as:
  `{ seatId: Guid, status: "Available" | "Held" | "Booked" }`
  `version` is omitted from the table, contradicting Plan 01 line 868 (`SeatStatusChanged(seatId, newStatus, version)`).
- **Correction:**
  Update Table 3.2 payload to:
  `{ seatId: Guid, status: "Available" | "Held" | "Booked", version: number }`.
