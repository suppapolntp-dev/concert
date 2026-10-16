# ⚡ High-Concurrency & Real-Time Architecture Plan

> **Scope:** Virtual Waiting Room, Distributed Seat Locking, Real-Time SignalR Broadcast, and Expiry Workers  
> **Key Technologies:** Redis 7 (Sorted Sets, Pub/Sub, Distributed Lock) + ASP.NET Core SignalR + Hangfire  
> **Source Spec:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md) (Sections 1.3, 5.1, 5.2)

---

## 1. Virtual Waiting Room (Traffic Spikes & Fairness)

To protect the database and downstream services during high-demand concert on-sales, incoming traffic is intercepted by a Redis-backed Virtual Waiting Room.

```
       [ Surge Traffic: 50,000 users ]
                      │
                      ▼
        ┌───────────────────────────┐
        │  Virtual Waiting Room     │
        │  (Redis Sorted Sets ZADD) │
        └─────────────┬─────────────┘
                      │
          Polling: Heartbeat / Position
                      │
    ┌─────────────────▼─────────────────┐
    │  Admission Token Issued (JWT)     │
    │  (Rate: 100 users / 5 seconds)    │
    └─────────────────┬─────────────────┘
                      │
                      ▼
         [ Active Booking Engine ]
```

### 1.1 Redis Data Structures
* **Queue Storage:** `waiting_room:queue:{eventId}` (Sorted Set: Score = Timestamp).
* **Active Passes:** `waiting_room:active:{eventId}` (Hash Set: `token` ➜ `expiry_timestamp`).
* **Heartbeat Tracker:** `waiting_room:heartbeat:{eventId}` (Sorted Set: Score = Last Seen Timestamp).

### 1.2 Admission Flow
1. **Enter Queue (`POST /api/v1/waitingroom/{eventId}/enter`):**
   * Assigns user to Redis Sorted Set: `ZADD waiting_room:queue:{eventId} {currentTimestamp} {userId}`.
   * Returns estimated queue position via `ZRANK`.
2. **Heartbeat Polling (`GET /api/v1/waitingroom/{eventId}/position`):**
   * Client polls every 3 seconds.
   * Updates heartbeat score. Clients missing 3 consecutive heartbeats (9s) are pruned from the queue.
3. **Queue Gatekeeper Worker (Hangfire):**
   * Releases batches (e.g. 50 users/second) based on platform server load capacity.
   * Generates a signed cryptographic bypass token valid for 15 minutes.
4. **Bypass Middleware:**
   * Mutating seat endpoints (`/seats/hold`) require header `X-WaitingRoom-Token`.
   * Platform Admins bypass the waiting room automatically.

---

## 2. Distributed Seat Locking (RedLock)

To prevent race conditions where multiple requests attempt to hold the same seat at the exact same millisecond:

```
Request A ───┐
             ├──> [ Redis RedLock: "lock:seat:{seatId}" (TTL: 3s) ] ──> Acquired: Proceed DB Hold
Request B ───┘                                                      ──> Failed: Return 409 Conflict
```

### 2.1 Locking Pattern
```csharp
public async Task<HoldSeatResult> HoldSeatAsync(Guid seatId, Guid userId, CancellationToken ct)
{
    var lockKey = $"lock:seat:{seatId}";
    await using var redLock = await _redisLockFactory.CreateLockAsync(lockKey, TimeSpan.FromSeconds(3));
    
    if (!redLock.IsAcquired)
    {
        throw new ConcurrencyConflictException("Seat is currently being acquired by another user.");
    }

    // Begin short-lived database transaction
    await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);
    
    var seat = await _dbContext.Seats.FirstOrDefaultAsync(s => s.Id == seatId, ct);
    if (seat == null || seat.Status != SeatStatus.Available)
    {
        throw new SeatUnavailableException("Seat is not available.");
    }

    seat.Hold(userId, TimeSpan.FromMinutes(10));
    await _dbContext.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);

    // Broadcast update via SignalR
    await _hubContext.Clients.Group(seat.EventId.ToString())
        .SendAsync("SeatStatusChanged", new { SeatId = seat.Id, Status = "Held" }, ct);

    return new HoldSeatResult(seat.Id, seat.HeldUntil!.Value);
}
```

---

## 3. Real-Time SignalR Seat Map Hub (`/hubs/seats`)

### 3.1 Partitioning & Connection Management
* Clients join a specific Event group upon opening the seat map:
  `await Groups.AddToGroupAsync(Context.ConnectionId, eventId.ToString());`
* Leaving the seat map removes the client from the group automatically on disconnect.

### 3.2 Real-time Message Contracts
| Event Name | Direction | Payload | Trigger |
| :--- | :---: | :--- | :--- |
| `JoinEvent` | Client ➜ Hub | `{ eventId: Guid }` | User opens seat map |
| `SeatStatusChanged` | Hub ➜ Client | `{ seatId: Guid, status: "Available" \| "Held" \| "Booked" }` | Successful hold, release, or checkout |
| `BatchSeatUpdate` | Hub ➜ Client | `List<{ seatId, status }>` | Bulk hold expiration |

---

## 4. Background Seat Expiry Worker (Hangfire)

A recurring job runs every 10 seconds to reclaim seats whose 10-minute hold window expired.

### 4.1 Soft-Extension Grace Handling
```sql
-- Find expired holds that DO NOT have an active checkout in progress
SELECT s.id, s.event_id 
FROM seats s
LEFT JOIN payments p ON p.payment_intent_id = s.payment_intent_id
WHERE s.status = 'Held' 
  AND s.held_until < NOW()
  AND (s.payment_intent_id IS NULL OR p.status NOT IN ('Pending', 'Processing'));
```

* If `payment_intent_id` is set and status is `Pending`:
  * Grant a **60-second soft-extension** before forcefully freeing the seat to prevent payment-gateway race conditions.
* On release:
  * Update seat status to `Available`.
  * Send `BatchSeatUpdate` to SignalR event group.
  * Trigger Waitlist Notification engine if customers are waiting.
