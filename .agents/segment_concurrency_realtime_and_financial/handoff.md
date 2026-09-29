# Handoff Report — Track 2: Concurrency, Real-Time & Financial Integrity Review

## 1. Observation
- Target Files Audited:
  - `Plan/01_concert_requirement_spec.md`: Lines 700-1050 (Virtual Waiting Room endpoints, Seat/Booking state machines, rate limits, Hold-Expiry vs Webhook arrival rules, SignalR specifications).
  - `Plan/03_docker_infra_plan.md`: Lines 114-131 (Redis single-node container configuration with `--appendonly yes`, lacking `maxmemory` and `noeviction` boundaries).
  - `Plan/06_entity_db_plan.md`: Lines 99-155 (`Seat.cs` entity methods `Hold`, `ConfirmBooking`, `Release`, missing `PaymentIntentId` reset in `Hold`), Lines 161-190 (`SeatConfiguration.cs` mapping `builder.UseXminAsConcurrencyToken()` vs unmapped `Version` property, missing partial index on `held_until`), and Table of Entities missing `WebhookEvent`.
  - `Plan/08_realtime_concurrency_plan.md`: Lines 33-51 (Redis Sorted Set queueing using bare timestamp `ZADD`, missing admission transition state, 15m vs 5m token mismatch), Lines 65-95 (`HoldSeatAsync` with hardcoded 3-second RedLock lease, lack of multi-seat canonical ordering, plain `FirstOrDefaultAsync` without `FOR UPDATE NOWAIT`), Lines 115-135 (Hangfire expiry query filtering out `Pending` payments indefinitely).
  - `Plan/09_payment_reconciliation_plan.md`: Lines 68-107 (`ReconcileSuccessfulPaymentAsync` calling `s.ConfirmBooking()` on `Available` seats in Scenario 3, missing partial multi-seat conflict release, executing external HTTP call `_refundService.ExecuteAutoRefundAsync` inside a Serializable database transaction, timestamp arithmetic inverted for grace period).
- Output Deliverable:
  - Created `c:\Users\wwwwww\Documents\project-test\concert\.agents\teamwork_preview_document_1\unit_report_concurrency_realtime_and_financial.md` with 20 categorized findings ([CRITICAL], [HIGH], [MEDIUM], [LOW]) and complete code/diff recommendations.

## 2. Logic Chain
1. **Scenario 3 Domain Invariant Breakdown:** In `Plan/06` line 140, `Seat.ConfirmBooking()` throws `DomainException` unless `Status == SeatStatus.Held`. In `Plan/09` line 91, Scenario 3 is entered when `seatsStillAvailable == true` (`Status == SeatStatus.Available`). Calling `s.ConfirmBooking()` unconditionally throws `DomainException`, crashing the webhook handler and leaving payments in limbo without tickets or auto-refunds.
2. **Infinite Lockout:** In `Plan/08` line 127, the SQL query explicitly excludes `p.status IN ('Pending', 'Processing')`. Because it excludes them, Hangfire never updates `s.held_until` and never frees the seat if the user abandons checkout, causing an infinite seat lock leak.
3. **Queue Invariant Breakdown:** Storing scores as bare millisecond timestamps in Redis Sorted Sets causes score ties under 50,000 requests. Redis resolves ties lexicographically on member strings (`userId` UUID), giving preferential treatment to users with lower hex characters. Monotonic sequence tie-breaking (`INCR`) is mathematically necessary for FIFO queueing.
4. **Lock Expiry vs DB Latency:** A 3-second RedLock without auto-renewal expires under database connection pool saturation. Because `Plan/08` does not use `FOR UPDATE NOWAIT`, a second buyer reads `Available` before the first transaction commits, leading to concurrency collisions and phantom state divergence.
5. **Serializable Deadlocks & Replay Vulnerability:** Invoking external payment gateway refund endpoints inside a PostgreSQL Serializable transaction blocks DB connections for hundreds of milliseconds. If the transaction aborts after Stripe refunds, retried webhooks trigger duplicate refunds. Decoupling via the Outbox Pattern eliminates this financial leak.

## 3. Caveats
- The audit evaluated the architectural and technical plans as specified in `Plan/`. No backend C# runtime code is currently running in this repository; the recommendations provide drop-in implementations for Phase 2/3 development.
- Performance characteristics (e.g. 50 users/second admission vs 500 VUs) are based on the load testing targets defined in `Plan/13_testing_qa_plan.md`.

## 4. Conclusion
The high-concurrency and financial integrity plans contain critical implementation flaws that would lead to production failure under concert on-sale conditions (crashed webhooks, orphaned seat holds, queue bias, and infinite seat locking). All flaws have been identified, categorized, and resolved with concrete diffs and C#/SQL/Lua scripts in the deliverable report.

## 5. Verification Method
1. Inspect deliverable report:
   `view_file` at `c:\Users\wwwwww\Documents\project-test\concert\.agents\teamwork_preview_document_1\unit_report_concurrency_realtime_and_financial.md`.
2. Verify all 20 findings across the 3 required top-level headers:
   `# Summary`, `# Potential Mistakes and Improvements`, `# Minor Corrections and Typos`.
3. Verify domain invariant logic by comparing `Seat.cs` (`Plan/06` line 140) with `ReconcileSuccessfulPaymentAsync` (`Plan/09` line 93).
