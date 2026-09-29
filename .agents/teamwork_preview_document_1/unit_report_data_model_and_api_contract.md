# Summary

This report delivers a rigorous architectural consistency audit for Track 1: **data_model_and_api_contract** across `Plan/00_ROADMAP_READING_ORDER.md`, `Plan/01_concert_requirement_spec.md` (Section 6 & 7 Data Dictionary v2.1), `Plan/02_master_project_plan.md`, `Plan/05_oop_design_plan.md`, `Plan/06_entity_db_plan.md`, and `Plan/07_api_endpoint_plan.md`.

### Audit Scope & Key Metrics
- **Entities & Tables Audited:** 24 relational tables and entity aggregates across Plan/01, Plan/02, Plan/05, and Plan/06.
- **Enums & Value Objects Audited:** 17 Domain Enums and 8 Domain Value Objects in Plan/05 against PostgreSQL types, check constraints, and DTO definitions.
- **Endpoints Audited:** All 78 REST endpoints in Plan/07 evaluated against the business rules, state machines, and workflows of Plan/01.
- **Total Discrepancies Cataloged:** 27 findings (9 [CRITICAL], 8 [HIGH], 6 [MEDIUM], 4 [LOW]).

### Executive Assessment
The architectural specification demonstrates strong domain depth, but exhibits severe cross-plan discrepancies that will break compilation, database migrations, and runtime API operations if not reconciled:
1. **Critical API Gap (21 Missing Endpoints):** `Plan/07_api_endpoint_plan.md` omits 21 vital endpoints required by `Plan/01`, including Organizer Promo Code CRUD, Customer & Organizer Refund lifecycle workflows, Admin Organizer approvals, PDPA Right to Erasure/Export, and Omise Webhook ingestion.
2. **DTO Schema Void:** `Plan/07_api_endpoint_plan.md` provides table route lists but zero request/response DTO schemas for ~70 endpoints, forcing developers to guess payload contracts.
3. **Database Schema & Entity Desynchronization:** `Plan/06_entity_db_plan.md` only registers 22 entities (omitting `Review`, `Notification`, `WebhookEvent`, and `EventArtist`), while `Plan/02` invents a phantom table `promo_usages`. Furthermore, `SeatLayout` and `TicketTier` clash with Plan/01's `venues.seat_map_template` and `zones`.
4. **Data Type & Constraint Runtime Crashes:**
   - `DiscountType.FixedAmount` (11 characters) crashes PostgreSQL column `promo_codes.discount_type VARCHAR(10)`.
   - `Percentage` Value Object (0–100 scale) violates database constraint `CHECK (0..1)` on `commission_rate`.
   - Global Soft-Delete query filters configured in EF Core for `Event` and `Venue` crash with SQL 42703 because neither table possesses a `deleted_at` column in `Plan/01`.
   - `SeatStatus` enum serializes `Held` whereas Plan/01 SQL triggers and state machines require string value `Hold`.
   - `PaymentStatus` lacks `PartiallyRefunded` and uses `Succeeded` instead of Plan/01's `Completed`.
5. **High-Concurrency Bottleneck:** `WaitingRoomToken` is modeled as a PostgreSQL entity in Plan/06 and Plan/02, contradicting the high-throughput Redis in-memory queue design of Plan/01.

---

# Potential Mistakes and Improvements

## [CRITICAL]

### 1. 21 Missing REST API Endpoints in Plan/07 Required by Plan/01 Business Rules
- **File & Section:** `Plan/07_api_endpoint_plan.md` (Sections 4, 11, 12, 13) vs `Plan/01_concert_requirement_spec.md` (Section 1.2, 3.2, 7.2, 7.3).
- **Defect:** `Plan/07` documents exactly 78 endpoints, but omits 21 critical endpoints explicitly mandated by `Plan/01` business rules. Without these endpoints, core business functions cannot execute:
  1. `POST /api/v1/auth/me/export` (PDPA Right to Portability, Plan/01 line 1191).
  2. `POST /api/v1/my-tickets/{id}/refund-request` (Customer Refund Submission, Plan/01 line 724).
  3. `POST /api/v1/my-tickets/{id}/transfer/reject` (Transfer Rejection, Plan/05 line 174).
  4. `POST /api/v1/my-tickets/{id}/transfer/cancel` (Transfer Cancellation by sender before accept, Plan/05 line 176).
  5. `POST /api/v1/organizer/events/{id}/cancel` (Organizer Event Cancellation triggering auto-refunds, Plan/01 line 803).
  6. `GET /api/v1/organizer/events/{id}/waitlist` (Organizer Event Waitlist inspection, Plan/01 line 809).
  7. `POST /api/v1/organizer/promo-codes` (Create promo code, Plan/01 line 810).
  8. `GET /api/v1/organizer/promo-codes` (List organizer promo codes, Plan/01 line 811).
  9. `PUT /api/v1/organizer/promo-codes/{id}` (Update promo code, Plan/01 line 812).
  10. `DELETE /api/v1/organizer/promo-codes/{id}` (Delete promo code, Plan/01 line 813).
  11. `GET /api/v1/organizer/refunds` (List pending refunds for organizer events, Plan/01 line 816).
  12. `PUT /api/v1/organizer/refunds/{id}/approve` (Approve/reject refund request, Plan/01 line 817).
  13. `PUT /api/v1/organizer/staff/{id}` (Update staff permissions and event scope, Plan/01 line 820).
  14. `PUT /api/v1/organizer/settings` (Update organizer company profile and encrypted bank details, Plan/01 line 822).
  15. `GET /api/v1/admin/organizers` (List pending/active organizer profiles, Plan/01 line 836).
  16. `POST /api/v1/admin/organizers/{id}/approve` (Approve organizer profile to enable ticket sales, Plan/01 line 837).
  17. `GET /api/v1/admin/analytics` (Platform GMV, revenue trend, payment method breakdown, Plan/01 line 839).
  18. `GET /api/v1/admin/config` (Fetch all platform feature flags and configs, Plan/01 line 841).
  19. `GET /api/v1/admin/refunds` (Admin dispute refund escalation list, Plan/01 line 845).
  20. `POST /api/v1/admin/refunds/{id}/decide` (Admin override/decision on refund disputes, Plan/01 line 846).
  21. `POST /api/v1/admin/users/{id}/anonymize` (Admin PDPA crypto-shredding/anonymization, Plan/01 line 847, 1199).
  22. `POST /api/v1/webhooks/omise` (Omise payment webhook ingestion, Plan/01 line 856).
- **Recommendation & Fix:** Update `Plan/07_api_endpoint_plan.md` to add these endpoints into their respective modules, bringing the total endpoint count from 78 to 99 endpoints, and update the summary table accordingly.

```markdown
<!-- Diff for Plan/07_api_endpoint_plan.md -->
### Add to Module 4: Seats & Booking — Customer
| 11 | `POST` | `/my-tickets/{id}/refund-request` | Customer (Owner) | ✅ Key | ยื่นคำขอคืนเงินตาม Refund Policy ของ Event |
| 12 | `POST` | `/my-tickets/{id}/transfer/reject` | Customer (Recipient) | ✅ | ปฏิเสธการรับตั๋วที่โอนมา |
| 13 | `POST` | `/my-tickets/{id}/transfer/cancel` | Customer (Sender) | ✅ | ขอยกเลิกการโอนก่อนผู้รับกดยืนยัน |

### Add to Module 11: Organizer Dashboard
| 16 | `POST` | `/organizer/events/{id}/cancel` | Organizer (Owner) | ✅ Key | ยกเลิกอีเวนต์ (Trigger Auto-Refund) |
| 17 | `GET` | `/organizer/events/{id}/waitlist` | Organizer (Owner) | — | ดูรายชื่อและสถานะ Waitlist ของงาน |
| 18 | `POST` | `/organizer/promo-codes` | Organizer | ✅ Key | สร้างโค้ดส่วนลดใหม่ |
| 19 | `GET` | `/organizer/promo-codes` | Organizer | — | รายการโค้ดส่วนลดทั้งหมดของตนเอง |
| 20 | `PUT` | `/organizer/promo-codes/{id}` | Organizer (Owner) | — | แก้ไขข้อมูลโค้ดส่วนลด |
| 21 | `DELETE` | `/organizer/promo-codes/{id}` | Organizer (Owner) | — | ลบโค้ดส่วนลด |
| 22 | `GET` | `/organizer/refunds` | Organizer | — | คำขอคืนเงินของ Event ตนเองที่รออนุมัติ |
| 23 | `PUT` | `/organizer/refunds/{id}/approve` | Organizer (Owner) | ✅ Key | อนุมัติหรือปฏิเสธคำขอคืนเงิน |
| 24 | `PUT` | `/organizer/staff/{id}` | Organizer | — | แก้ไขสิทธิ์และ Event Scope ของ Staff |
| 25 | `PUT` | `/organizer/settings` | Organizer | — | แก้ไขโปรไฟล์ผู้จัดและข้อมูลบัญชีธนาคาร (Encrypted) |

### Add to Module 12: Platform Admin
| 13 | `GET` | `/admin/organizers` | Admin | — | รายการผู้จัดงานทั้งหมดและคิวรอตรวจสอบ |
| 14 | `POST` | `/admin/organizers/{id}/approve` | Admin | ✅ Key | อนุมัติโปรไฟล์ผู้จัดงานให้เริ่มขายได้ |
| 15 | `GET` | `/admin/analytics` | Admin | — | GMV รวม, แนวโน้มรายได้, สัดส่วนช่องทางชำระเงิน |
| 16 | `GET` | `/admin/config` | Admin | — | ดึงรายการ Platform Configs และ Feature Flags ทั้งหมด |
| 17 | `GET` | `/admin/refunds` | Admin | — | รายการข้อพิพาทคำขอคืนเงินทั้งหมดในระบบ |
| 18 | `POST` | `/admin/refunds/{id}/decide` | Admin (Step-up) | ✅ Key | วินิจฉัยข้อพิพาทการคืนเงินนอกนโยบายปกติ |
| 19 | `POST` | `/admin/users/{id}/anonymize` | Admin (Step-up) | ✅ Key | ดำเนินการลบ/นิรนามข้อมูลส่วนบุคคลตาม PDPA |

### Add to Module 13: Webhook Receiver
| 2 | `POST` | `/webhooks/omise` | Signature Verify | ✅ | รับ Webhook การชำระเงินจาก Omise Gateway |
```

---

### 2. Complete Absence of Request and Response DTO Schemas in Plan/07
- **File & Section:** `Plan/07_api_endpoint_plan.md` (Entire file) vs `Plan/05_oop_design_plan.md` (Section 4).
- **Defect:** `Plan/07` provides summary endpoint catalog tables with HTTP methods and paths, but specifies **zero payload schemas** for request bodies, query string parameters, or response data models (except citing a broken reference to `Plan/concert.md`). `Plan/05` defines only 4 CQRS records (`CreateEventCommand`, `CreateTicketTierDto`, `GetEventBySlugQuery`, `EventDetailResponse`), leaving the remaining ~70 endpoints without formal DTO contracts. Frontend and backend developers cannot construct type-safe API clients or MediatR handlers without guessing contract property names and types.
- **Recommendation & Fix:** Introduce a dedicated Section 14 in `Plan/07_api_endpoint_plan.md` or a companion schema catalog specifying C# sealed records and TypeScript interface models for all payload contracts. At minimum, publish core command and response contracts:
  - `HoldSeatsRequest` (`List<Guid> SeatIds`, `Guid EventId`)
  - `CheckoutBookingRequest` (`string PaymentMethod`, `string ReturnUrl`, `string? PromoCode`)
  - `CreateOrganizerStaffRequest` (`string Email`, `List<string> Permissions`, `Guid? EventId`)
  - `SubmitRefundDecisionRequest` (`bool Approved`, `string? RejectionReason`)

```csharp
// Example standard DTO definitions to add to Plan/07:
namespace Application.Features.Seats.Commands;

public sealed record HoldSeatsRequest(
    Guid EventId,
    List<Guid> SeatIds
);

public sealed record HoldSeatsResponse(
    Guid BookingId,
    DateTime HeldUntil,
    List<Guid> HeldSeatIds,
    decimal TotalHoldAmount,
    string Currency
);

public sealed record CheckoutBookingRequest(
    string PaymentGateway,       // "Stripe" | "Omise"
    string PaymentMethod,        // "CreditCard" | "PromptPay"
    string? PromoCode,
    string? PaymentMethodToken
);

public sealed record CheckoutBookingResponse(
    Guid PaymentId,
    string ClientSecret,
    string PaymentStatus,
    string? RedirectUrl
);
```

---

### 3. Entity Class Registry Count and Omissions in Plan/06
- **File & Section:** `Plan/06_entity_db_plan.md` (Section 2) vs `Plan/01_concert_requirement_spec.md` (Section 2) vs `Plan/02_master_project_plan.md` (Section 4).
- **Defect:** Both `Plan/00` (line 67) and `Plan/02` (line 92) mandate a 24-entity architecture. However:
  1. `Plan/06` Section 2 tabulates only **22 entities**. It completely omits `Review` (table `reviews`, Plan/01 2.17) and `Notification` (table `notifications`, Plan/01 2.16).
  2. `Plan/06` also omits `WebhookEvent` (table `webhook_events`, Plan/01 2.21) and `EventArtist` (junction entity, Plan/01 2.8).
  3. `Plan/02` lists a phantom table `promo_usages` (line 124) that does not exist in `Plan/01`, `Plan/05`, or `Plan/06`.
  4. Pluralization mismatches: `waitlist` (Plan/01) vs `waitlists` (Plan/02, 06); `platform_configs` (Plan/01) vs `system_configs` (Plan/02, 06).
- **Recommendation & Fix:** Standardize the entity registry in `Plan/06` to include all required domain entities, remove `promo_usages` from `Plan/02`, and reconcile table names to match PostgreSQL snake_case conventions.

```markdown
<!-- Diff for Plan/06_entity_db_plan.md Section 2.5 -->
| Entity | Base / Interfaces | Table | Key Fields & Value Objects |
| :--- | :--- | :--- | :--- |
| `Review` | `AggregateRoot<Guid>`, `IAuditableEntity` | `reviews` | `UserId`, `EventId`, `Rating` (1-5), `Comment`, `IsHidden` |
| `Notification` | `BaseEntity<Guid>`, `IAuditableEntity` | `notifications` | `UserId`, `Type` (Enum), `Title`, `Message`, `Link`, `Metadata` (JSONB), `IsRead` |
| `WebhookEvent` | `BaseEntity<Guid>` | `webhook_events` | `Gateway`, `EventId` (Unique), `EventType`, `Payload` (JSONB), `SignatureValid`, `ReceivedAt`, `ProcessedAt` |
| `EventArtist` | Junction Entity | `event_artists` | Composite PK: `(EventId, ArtistId)`, `IsHeadliner`, `DisplayOrder` |
```

---

### 4. Database String Truncation Crash in Promo Code Discount Type
- **File & Section:** `Plan/05_oop_design_plan.md` (Section 2.1 line 125) vs `Plan/01_concert_requirement_spec.md` (Section 2.14 line 471).
- **Defect:** In `Plan/01`, column `promo_codes.discount_type` is defined as `VARCHAR(10)` with valid database values `'percent'` or `'fixed'`. In `Plan/05`, the C# enum is defined as:
  ```csharp
  public enum DiscountType { Percentage, FixedAmount }
  ```
  When EF Core maps this enum via `.HasConversion<string>()`, the value `DiscountType.FixedAmount` produces the string `"FixedAmount"`, which contains **11 characters**. Inserting `"FixedAmount"` into a `VARCHAR(10)` column in PostgreSQL triggers an immediate runtime exception: `22001: value too long for type character varying(10)`.
- **Recommendation & Fix:** Update `Plan/01` table definition to `VARCHAR(20)` and align the enum string conversion or use custom value converters in EF Core.

```markdown
<!-- Diff for Plan/01_concert_requirement_spec.md Section 2.14 -->
- | `discount_type` | `VARCHAR(10)` | - | NO | - | ประเภทส่วนลด (`percent` หรือ `fixed`) | `'percent'` |
+ | `discount_type` | `VARCHAR(20)` | - | NO | - | ประเภทส่วนลด (`Percentage` หรือ `FixedAmount`) | `'Percentage'` |
```

---

### 5. Check Constraint Violation: Percentage Value Object vs Database Commission Rate
- **File & Section:** `Plan/05_oop_design_plan.md` (Section 3.1 line 361) vs `Plan/01_concert_requirement_spec.md` (Section 2.3 line 191).
- **Defect:** In `Plan/05`, the `Percentage` Value Object enforces a range of 0 to 100:
  ```csharp
  public readonly record struct Percentage {
      public decimal Value { get; }
      public Percentage(decimal value) {
          if (value is < 0 or > 100) throw new DomainException(...);
          Value = value;
      }
  }
  ```
  However, in `Plan/01`, table `organizer_profiles.commission_rate` is defined as `DECIMAL(5,4) CHECK (0..1)` with default `0.0500` (meaning 5% is represented as `0.0500`). If domain logic assigns a 5% commission using `new Percentage(5m)`, persisting `5m` to `commission_rate` immediately violates PostgreSQL constraint `CHECK (commission_rate >= 0 AND commission_rate <= 1)` and exceeds precision `DECIMAL(5,4)`.
- **Recommendation & Fix:** Explicitly document the ratio representation in the domain model. Either maintain `Rate` (0.0 to 1.0) for financial formulas or configure an EF Core Value Converter that divides by 100 on persist and multiplies by 100 on read.

```csharp
// Concrete fix in Infrastructure/Persistence/Configurations/OrganizerProfileConfiguration.cs:
builder.Property(o => o.CommissionRate)
    .HasColumnType("decimal(5,4)")
    .HasConversion(
        percentage => percentage.Value / 100m,       // Domain (5%) -> DB (0.0500)
        decimalValue => new Percentage(decimalValue * 100m) // DB (0.0500) -> Domain (5%)
    )
    .IsRequired();
```

---

### 6. Global Soft-Delete Query Filter Runtime Crash on Events and Venues
- **File & Section:** `Plan/06_entity_db_plan.md` (Section 2.2 line 61, Section 2.3 line 67, Section 4.2 lines 200–214) vs `Plan/01_concert_requirement_spec.md` (Section 2.6 line 253, Section 2.7 line 273).
- **Defect:** In `Plan/06`, `Event` and `Venue` are declared to implement `ISoftDeletable`:
  ```csharp
  public interface ISoftDeletable {
      DateTime? DeletedAt { get; set; }
      bool IsDeleted => DeletedAt.HasValue;
  }
  ```
  `Plan/06` Section 4.2 applies a Global Query Filter to all entities implementing `ISoftDeletable`:
  ```csharp
  modelBuilder.Entity(entityType.ClrType).HasQueryFilter(p => p.DeletedAt == null);
  ```
  However, examining the database schema in `Plan/01`, **neither `venues` (Table 2.6) nor `events` (Table 2.7) contains a `deleted_at` column**. Only `users` (Table 2.1) and `organizer_profiles` (Table 2.3) have `deleted_at`. Any query against `events` or `venues` via EF Core will crash with PostgreSQL error `42703: column p.DeletedAt does not exist`.
- **Recommendation & Fix:** Add `deleted_at TIMESTAMPTZ NULL Index` to both `venues` and `events` tables in `Plan/01`, or remove `ISoftDeletable` from their class definitions in `Plan/06`. Given PDPA and event archival requirements, adding `deleted_at` to the database schema is the architecturally sound fix.

```markdown
<!-- Diff for Plan/01_concert_requirement_spec.md Section 2.6 (venues) -->
+ | `deleted_at` | `TIMESTAMPTZ` | Index | YES | NULL | Soft-delete timestamp (PDPA / Archival) | NULL |

<!-- Diff for Plan/01_concert_requirement_spec.md Section 2.7 (events) -->
+ | `deleted_at` | `TIMESTAMPTZ` | Index | YES | NULL | Soft-delete timestamp | NULL |
```

---

### 7. High-Concurrency Database Bottleneck: WaitingRoomToken Modeled as PostgreSQL Table
- **File & Section:** `Plan/06_entity_db_plan.md` (Section 2.5 line 86) and `Plan/02_master_project_plan.md` (Section 4 line 131) vs `Plan/01_concert_requirement_spec.md` (Section 3.2 line 737, Section 5.1).
- **Defect:** `Plan/02` and `Plan/06` list `waiting_room_tokens` as a persistent PostgreSQL entity (`WaitingRoomToken : BaseEntity<Guid>`). `Plan/01` explicitly dictates that the Virtual Waiting Room operates purely in **Redis** via Sorted Sets (`ZADD/ZRANK`) to protect the relational database from the "thundering herd" during ticket drops (100k+ users). Persisting high-frequency, ephemeral tokens (which expire in 5–15 minutes) into a relational PostgreSQL table will cause severe disk I/O contention, table bloat, and connection pool exhaustion on the primary database during on-sale spikes.
- **Recommendation & Fix:** Remove `waiting_room_tokens` from the PostgreSQL entity schema and EF Core `DbContext`. Designate `WaitingRoomToken` as a Redis-backed caching structure managed via `IDistributedCache` or `StackExchange.Redis`.

```markdown
<!-- Diff for Plan/06_entity_db_plan.md Section 2.5 -->
- | `WaitingRoomToken` | `BaseEntity<Guid>` | `waiting_room_tokens` | `EventId`, `TokenHash`, `PriorityScore`, `IssuedAt`, `ExpiresAt` |
+ <!-- WaitingRoomToken is an in-memory Redis Sorted Set + String token (TTL 15m), NOT a PostgreSQL relational table -->
```

---

### 8. Missing EF Core Entity Configurations (IEntityTypeConfiguration<T>)
- **File & Section:** `Plan/06_entity_db_plan.md` (Section 4).
- **Defect:** `Plan/06` provides only a single configuration implementation (`SeatConfiguration.cs`), leaving the remaining 23 entities completely unconfigured. Critical database mapping specifications are absent:
  - Table name mappings (EF Core defaults to plural class names like `Users` instead of `users`).
  - Column length constraints (unconfigured strings default to `text` instead of `varchar(n)`).
  - Value conversions for 17 enums (defaults to integers instead of string enums).
  - Index definitions, compound unique constraints, and foreign key cascade rules.
- **Recommendation & Fix:** Mandate that every entity possesses a dedicated configuration class implementing `IEntityTypeConfiguration<T>` in `src/Infrastructure/Persistence/Configurations/` and register them via `ApplyConfigurationsFromAssembly`.

```csharp
// Example configuration for BookingConfiguration.cs:
namespace Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.BookingReference)
            .HasConversion(v => v.Value, s => new BookingReference(s))
            .HasMaxLength(25)
            .IsRequired();
            
        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(b => b.TotalAmount).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(b => b.Currency).HasMaxLength(3).IsRequired();
        
        builder.HasIndex(b => b.BookingReference).IsUnique();
        builder.HasIndex(b => new { b.UserId, b.Status });
        builder.HasIndex(b => new { b.EventId, b.Status });
        
        builder.HasOne<User>().WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Event>().WithMany().HasForeignKey(b => b.EventId).OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

### 9. Enum Deserialization and State Machine Failure: SeatStatus, PaymentStatus, WaitlistStatus
- **File & Section:** `Plan/05_oop_design_plan.md` (Section 2.1) vs `Plan/01_concert_requirement_spec.md` (Section 2.10, 2.13, 2.18, 5.2).
- **Defect:**
  1. `SeatStatus`: Plan/05 line 77 defines `Available, Held, Booked, Disabled`. Plan/01 line 350, line 978 (State Machine), and line 995 (SQL trigger) explicitly use `'Hold'` (e.g. `UPDATE seats SET status='Hold'`). Serializing `SeatStatus.Held` writes `'Held'`, which will fail SQL queries looking for `status = 'Hold'`.
  2. `PaymentStatus`: Plan/05 line 108 defines `Pending, Processing, Succeeded, Failed, Cancelled, Refunded`. Plan/01 Table 2.13 line 431 specifies `Pending, Completed, Failed, Refunded, PartiallyRefunded`. Plan/05 lacks `PartiallyRefunded` (critical for v2.1 refund engine) and uses `Succeeded` instead of `Completed`. Stripe/Omise webhook handlers checking for `Completed` will throw deserialization errors.
  3. `WaitlistStatus`: Plan/05 line 162 defines `Queued, Notified, Converted, Expired`. Plan/01 line 567 specifies default `'Waiting'`.
- **Recommendation & Fix:** Harmonize the enums in `Plan/05` with `Plan/01` database defaults and state machine transitions.

```csharp
// Diff for Plan/05_oop_design_plan.md Section 2.1:
public enum SeatStatus
{
    Available,
    Hold,       // Renamed from Held to match Plan/01 SQL & State Machine
    Booked,
    Disabled
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,          // Renamed from Succeeded
    Failed,
    Cancelled,
    Refunded,
    PartiallyRefunded   // Added to support partial refunds
}

public enum WaitlistStatus
{
    Waiting,    // Renamed from Queued to match DB column default
    Notified,
    Converted,
    Expired
}
```

---

## [HIGH]

### 10. Concurrency Token Conflict: Plan/01 version INT vs Plan/06 PostgreSQL xmin
- **File & Section:** `Plan/06_entity_db_plan.md` (Section 3 line 113, Section 4.1 line 183) vs `Plan/01_concert_requirement_spec.md` (Section 2.10 line 356, Section 5.2 line 996).
- **Defect:** In `Plan/01`, `seats.version` is defined as a concrete table column: `version INT NOT NULL DEFAULT 1`, incremented manually in SQL: `UPDATE seats SET ..., version = version + 1`. In `Plan/06`, `SeatConfiguration` specifies: `builder.UseXminAsConcurrencyToken()`, which maps concurrency to PostgreSQL's internal 32-bit system column `xmin`.
  - Having both creates architectural schizophrenia: if EF Core uses `xmin`, the `version` column is redundant dead storage.
  - If high-concurrency raw SQL procedures (or Dapper) update `version = version + 1` while EF Core queries check `xmin`, concurrency validations will conflict.
- **Recommendation & Fix:** Unify the concurrency strategy. Adopt `xmin` as the single authoritative concurrency mechanism across all documents, and eliminate the manual integer `version` column from `seats` in `Plan/01` (or document `version` as an application audit revision rather than the concurrency token).

---

### 11. Data Portability & PDPA Erasure Endpoints Missing from API Contract
- **File & Section:** `Plan/07_api_endpoint_plan.md` vs `Plan/01_concert_requirement_spec.md` (Section 7.2 line 1191, Section 7.3 line 1199).
- **Defect:** Thailand PDPA compliance mandates both the **Right to Data Portability** and the **Right to Erasure**:
  - Plan/01 explicitly requires `POST /api/v1/auth/me/export` (generate JSON export of bookings/tickets within 24h).
  - Plan/01 explicitly requires `POST /api/v1/admin/users/{id}/anonymize` (Admin step-up action to crypto-shred PII and anonymize records).
  Neither endpoint is present in `Plan/07_api_endpoint_plan.md`. This represents a statutory regulatory compliance failure.
- **Recommendation & Fix:** Add both endpoints with required headers, RBAC policies, and rate limits to `Plan/07`.

---

### 12. Missing Foreign Key Relationship Configurations and Cascading Delete Rules in Plan/06
- **File & Section:** `Plan/06_entity_db_plan.md` (Section 4).
- **Defect:** In EF Core, required foreign keys without explicit configuration default to `CascadeDelete`. If an `Event` or `OrganizerProfile` is deleted (or soft-deleted without proper navigation filters), EF Core may emit `CASCADE DELETE` commands that purge associated `bookings`, `booking_items`, `payments`, `payouts`, and `refunds`. This directly violates financial accounting standards (7-year ledger retention) and Thailand PDPA Section 7.2.
- **Recommendation & Fix:** Explicitly declare `DeleteBehavior.Restrict` on all foreign key relationships targeting financial ledgers (`bookings`, `payments`, `payouts`, `refunds`, `audit_logs`).

```csharp
// Add to EF Core configurations:
builder.HasOne<Booking>()
    .WithMany()
    .HasForeignKey(p => p.BookingId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

### 13. Ticket Transfer Domain Discrepancy: In-Place Update vs Separate Aggregate
- **File & Section:** `Plan/01_concert_requirement_spec.md` (Section 2.12 lines 404–405) vs `Plan/05_oop_design_plan.md` (Section 2.1 line 170, Section 5) vs `Plan/06_entity_db_plan.md` (Section 2.5 line 84).
- **Defect:**
  - In `Plan/01`, ticket transfer is modeled as an in-place column update on `booking_items`: `transferred_to_user_id` and `transferred_at`. There is no `ticket_transfers` table in Plan/01's Data Dictionary.
  - In `Plan/02`, `Plan/05`, and `Plan/06`, ticket transfer is modeled as an independent entity/table `TicketTransfer` (`ticket_transfers`) with state transitions: `Pending -> Accepted | Rejected | Expired | Cancelled`.
  - Modeling transfers solely on `booking_items` loses multi-transfer audit trails (when ticket A is transferred from User 1 -> User 2 -> User 3) and cannot support the 24-hour expiration window before acceptance.
- **Recommendation & Fix:** Officially add table `ticket_transfers` to `Plan/01_concert_requirement_spec.md` Data Dictionary Section 2 to align with `Plan/02`, `Plan/05`, and `Plan/06`.

```markdown
<!-- Add Table 2.22 to Plan/01_concert_requirement_spec.md -->
### 2.22 Table: `ticket_transfers`
| Column Name | Data Type | Constraint | Nullable | Default | Description |
| :--- | :--- | :--- | :---: | :--- | :--- |
| `id` | `UUID` | PK | NO | `gen_random_uuid()` | รหัสการโอน |
| `booking_item_id` | `UUID` | FK (`booking_items.id`), Index | NO | - | ตั๋วใบที่ต้องการโอน |
| `from_user_id` | `UUID` | FK (`users.id`), Index | NO | - | ผู้โอน |
| `to_user_email` | `VARCHAR(255)` | - | NO | - | อีเมลผู้รับ |
| `to_user_id` | `UUID` | FK (`users.id`), Index | YES | NULL | บัญชีผู้รับเมื่อกดยอมรับ |
| `status` | `VARCHAR(20)` | Index | NO | `'Pending'` | `Pending`, `Accepted`, `Rejected`, `Expired`, `Cancelled` |
| `expires_at` | `TIMESTAMPTZ` | Index | NO | - | หมดเวลาภายใน 24 ชม. |
| `created_at` | `TIMESTAMPTZ` | - | NO | `NOW()` | เวลาที่เริ่มทำรายการ |
```

---

### 14. Terminology Discrepancy: zones vs ticket_tiers and seat_layouts
- **File & Section:** `Plan/01_concert_requirement_spec.md` (Section 2.6, 2.9) vs `Plan/05_oop_design_plan.md` (Section 2.1 line 85, Section 4 line 459) vs `Plan/06_entity_db_plan.md` (Section 2.2 line 62, Section 2.3 line 68).
- **Defect:**
  - `Plan/01` names Table 2.9 `zones` (with fields `name`, `color`, `price`, `currency`, `capacity`, `is_numbered_seat`).
  - `Plan/02`, `Plan/05`, and `Plan/06` rename this table/entity to `TicketTier` / `ticket_tiers` (with fields `Price`, `TotalQuota`, `AvailableQuota`).
  - In `Plan/01`, venue layout is embedded as `venues.seat_map_template JSONB`.
  - In `Plan/02` and `Plan/06`, venue layout is extracted into a standalone entity `SeatLayout` (table `seat_layouts`).
  - Having `zones` in Plan/01 and `ticket_tiers` in Plan/05/06 creates immense confusion in code generator tools and EF Core migrations.
- **Recommendation & Fix:** Harmonize the naming:
  - If a physical seat map can have multiple seat tiers within a zone (or zones define tiers), document the relationship explicitly: keep `SeatLayout` as the physical coordinate template, and standardize on `TicketTier` (`ticket_tiers`) or alias `Zone` to `TicketTier` uniformly across all plans.

---

### 15. Primary Key Generation: UUIDv4 (Guid.NewGuid()) vs Required UUIDv7
- **File & Section:** `Plan/06_entity_db_plan.md` (Section 3 line 119) vs `ANALYSIS_PARTITION.md` (line 13) vs `Plan/01_concert_requirement_spec.md` (Table 2.1 line 133).
- **Defect:** `ANALYSIS_PARTITION.md` specifies time-ordered sequential `UUIDv7` for primary keys to prevent B-tree page fragmentation during high-velocity insertions (ticket drop spikes). However, `Plan/06` implements:
  ```csharp
  Id = Guid.NewGuid();
  ```
  In .NET 8, `Guid.NewGuid()` produces a random **UUIDv4**. In PostgreSQL, random UUID inserts into high-traffic tables (`seats`, `bookings`, `booking_items`, `audit_logs`) cause heavy B-tree index fragmentation, cache misses, and WAL write amplification. (Native UUIDv7 is only available starting in .NET 9).
- **Recommendation & Fix:** Configure an explicit sequential UUIDv7 generator package in .NET 8 (such as `UUIDNext` or `Medo.Uuid7`) and register an EF Core ValueGenerator.

```csharp
// Solution in Domain/Common/Uuid7.cs:
using UUIDNext;

public static class Uuid7
{
    public static Guid NewGuid() => Uuid.NewDatabaseFriendly(Database.PostgreSql);
}

// In BaseEntity constructor:
Id = Uuid7.NewGuid();
```

---

### 16. AuditLog Primary Key Type Mismatch: BIGSERIAL vs Guid
- **File & Section:** `Plan/01_concert_requirement_spec.md` (Section 2.19 line 585) vs `Plan/06_entity_db_plan.md` (Section 2.5 line 87).
- **Defect:** In `Plan/01`, `audit_logs.id` is explicitly typed as `BIGSERIAL` (64-bit integer, monotonic counter) to guarantee strict temporal ordering and facilitate monthly partition pruning. In `Plan/06`, `AuditLog` is declared as:
  ```csharp
  public class AuditLog : BaseEntity<Guid>
  ```
  Using a `Guid` PK for `audit_logs` breaks chronological B-tree clustering unless UUIDv7 is strictly enforced, and directly contradicts `Plan/01`'s PostgreSQL schema specification.
- **Recommendation & Fix:** Define `AuditLog` as `BaseEntity<long>` in `Plan/06` to match `BIGSERIAL` in `Plan/01`.

```csharp
// Diff for Plan/06_entity_db_plan.md Section 2.5:
- | `AuditLog` | `BaseEntity<Guid>` | `audit_logs` | ...
+ | `AuditLog` | `BaseEntity<long>` | `audit_logs` | `Id` (BIGSERIAL PK), `ActorId`, ...
```

---

### 17. Auditable Interceptor Incompleteness & Incompatible Entity Timestamps
- **File & Section:** `Plan/06_entity_db_plan.md` (Section 1 line 30, Section 4.3 lines 220–248).
- **Defect:**
  1. `AuditableEntityInterceptor` only overrides `SavingChangesAsync`. Any invocation of synchronous `context.SaveChanges()` will bypass the interceptor, leaving `CreatedAt` and `UpdatedAt` unpopulated.
  2. `IAuditableEntity` requires both `CreatedAt` and `UpdatedAt`. However, over 50% of the tables in `Plan/01` (`venues`, `artists`, `zones`, `booking_items`, `payments`, `payouts`, `notifications`, `reviews`, `waitlist`, `audit_logs`) do NOT possess an `updated_at` column in the database schema. Applying `IAuditableEntity` indiscriminately forces EF Core to look for a non-existent `updated_at` column.
- **Recommendation & Fix:** Separate the auditing abstractions into `ICreationAuditable` (`CreatedAt`) and `IUpdateAuditable` (`UpdatedAt`), and override both `SavingChanges` and `SavingChangesAsync`.

```csharp
public interface ICreationAuditable { DateTime CreatedAt { get; set; } }
public interface IUpdateAuditable : ICreationAuditable { DateTime UpdatedAt { get; set; } }

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        UpdateTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private static void UpdateTimestamps(DbContext? context)
    {
        if (context == null) return;
        var now = DateTime.UtcNow;
        foreach (var entry in context.ChangeTracker.Entries<ICreationAuditable>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = now;
            if (entry is { State: EntityState.Modified, Entity: IUpdateAuditable updatable })
                updatable.UpdatedAt = now;
        }
    }
}
```

---

## [MEDIUM]

### 18. Value Object Incompatibilities: Slug Length and GeoCoordinate Type
- **File & Section:** `Plan/05_oop_design_plan.md` (Section 3.1 lines 311, 321) vs `Plan/01_concert_requirement_spec.md` (Table 2.6 line 262, Table 2.7 line 279) vs `Plan/06_entity_db_plan.md` (Section 2.2 line 61).
- **Defect:**
  1. `Slug` Value Object throws if `Value.Length > 300`. However, `Plan/01` specifies `events.slug VARCHAR(500)`. An event with a title producing a 350-character slug will be rejected by the domain VO despite fitting within the database column.
  2. `GeoCoordinate` uses `double Latitude` and `double Longitude`. `Plan/01` uses `DECIMAL(10,7)`. `Plan/06` lists `Coordinates (Point)` (PostGIS geometry type). If EF Core uses NetTopologySuite `Point`, mapping `GeoCoordinate` requires an explicit value converter.
- **Recommendation & Fix:** Increase `Slug` max length to 500 characters, and specify an EF Core ValueConverter between `GeoCoordinate` and NetTopologySuite `Point`.

```csharp
// In Domain/ValueObjects/Slug.cs:
if (Value.Length > 500)
    throw new DomainException("Slug exceeds maximum length of 500.");
```

---

### 19. Route Prefix and Angular SPA Route Collisions in Plan/07
- **File & Section:** `Plan/07_api_endpoint_plan.md` (Sections 2–13) vs `Plan/01_concert_requirement_spec.md` (Section 4.1).
- **Defect:** `Plan/07` states in Section 1 that the Base URL is `https://api.ticketworld.io/api/v1`. However, every route table in Sections 2 through 12 lists routes without the `/api/v1` prefix (e.g. `/events`, `/organizer/events`, `/admin/users`).
  In Angular 18 (`Plan/01` Section 4.1), the SPA frontend router uses identical route paths (`/events`, `/organizer/events`, `/admin/users`).
  If a reverse proxy or developer relies on the table text without the `/api/v1` prefix, API endpoints and client-side SPA routes collide.
- **Recommendation & Fix:** Prepend `/api/v1` consistently to all endpoint paths throughout `Plan/07`.

---

### 20. Ambiguous Route Parameter in DELETE /seats/hold/{bookingId}
- **File & Section:** `Plan/07_api_endpoint_plan.md` (Section 4 line 89) vs `Plan/06_entity_db_plan.md` (Section 3 line 128).
- **Defect:** In `Plan/07`, the seat release endpoint is defined as `DELETE /seats/hold/{bookingId}`. However, when seats are held via RedLock and PostgreSQL `SELECT FOR UPDATE NOWAIT`, the user is holding inventory prior to checkout. At this step, the hold may only have a `HoldToken` or `SeatIds`, not a materialized `bookingId`. If the booking is not yet created, passing `{bookingId}` is impossible.
- **Recommendation & Fix:** Update the route to `DELETE /seats/hold` taking `List<Guid> SeatIds` in the request body, or `DELETE /seats/hold/{holdToken}`.

---

### 21. Naming Convention Inconsistencies Across DTOs and Database Columns
- **File & Section:** `Plan/05_oop_design_plan.md` vs `Plan/06_entity_db_plan.md` vs `Plan/07_api_endpoint_plan.md`.
- **Defect:**
  - C# properties are PascalCase (`SeatNumber`, `TotalAmount`).
  - Database columns are snake_case (`seat_number`, `total_amount`).
  - JSON API payloads in Plan/01 are camelCase (`seatIds`, `traceId`), but waiting room tokens in Plan/07 use snake_case (`queue_token`, `seatmap_pass`).
  - Plan/06 does not configure EF Core snake_case naming conventions, meaning EF Core migrations will produce quoted PascalCase tables and columns by default in PostgreSQL.
- **Recommendation & Fix:** Explicitly add `UseSnakeCaseNamingConvention()` in `Program.cs` / `DbContextOptionsBuilder` and configure `System.Text.Json` naming policy to `JsonNamingPolicy.CamelCase`.

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());
```

---

### 22. NotificationType Enum Incompleteness in Plan/05
- **File & Section:** `Plan/05_oop_design_plan.md` (Section 2.1 line 179) vs `Plan/01_concert_requirement_spec.md` (Section 2.16 line 526).
- **Defect:** `Plan/05` defines 9 notification types: `BookingConfirmed`, `BookingCancelled`, `EventReminder`, `EventApproved`, `EventRejected`, `PayoutCompleted`, `WaitlistNotified`, `TicketTransferReceived`, `SecurityAlert`.
  `Plan/01` defines 10 notification types, including: `RefundProcessed`, `ArtistNewEvent`, `WaitlistAvailable`, `NewEventSubmitted`.
  Plan/05 misses `RefundProcessed`, `ArtistNewEvent`, and `NewEventSubmitted`, while naming waitlist notifications `WaitlistNotified` instead of `WaitlistAvailable`.
- **Recommendation & Fix:** Align `NotificationType` enum in `Plan/05` with `Plan/01`.

```csharp
public enum NotificationType
{
    BookingConfirmed,
    BookingCancelled,
    RefundProcessed,
    EventReminder,
    ArtistNewEvent,
    WaitlistAvailable,
    EventApproved,
    EventRejected,
    PayoutCompleted,
    NewEventSubmitted,
    TicketTransferReceived,
    SecurityAlert
}
```

---

### 23. Missing DTO Definitions in Plan/05
- **File & Section:** `Plan/05_oop_design_plan.md` (Sections 4, 8).
- **Defect:** In `Plan/05`, multiple signatures reference types that are never declared anywhere in the document:
  - Line 451: `VenueResponse Venue` (never defined).
  - Line 453: `List<ArtistSummaryDto> Artists` (never defined).
  - Line 697: `EventSearchCriteria criteria` (never defined).
  - Line 755: `PaymentIntentResult` (never defined).
  - Line 756: `RefundResult` (never defined).
- **Recommendation & Fix:** Declare all referenced records and DTOs in `Plan/05` Section 4.

```csharp
// Add to Plan/05 Section 4:
public sealed record VenueResponse(
    Guid Id,
    string Name,
    string Address,
    string City,
    string Country,
    int TotalCapacity
);

public sealed record ArtistSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string? AvatarUrl,
    bool IsHeadliner
);

public sealed record PaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret,
    string Status
);

public sealed record RefundResult(
    string RefundId,
    decimal Amount,
    string Status
);
```

---

## [LOW]

### 24. Broken / Stale File Links in Plan Documentation
- **File & Section:** `Plan/05_oop_design_plan.md` (line 5), `Plan/06_entity_db_plan.md` (line 4), `Plan/07_api_endpoint_plan.md` (line 5).
- **Defect:** Documents cross-reference non-existent files:
  - `Plan/entity-plan.md` (actual file is `Plan/06_entity_db_plan.md`).
  - `Plan/concert.md` (actual file is `Plan/01_concert_requirement_spec.md`).
- **Recommendation & Fix:** Replace all stale markdown links with the actual numbered file paths.

---

### 25. PaymentGateway Enum Discrepancy: PayPal vs Mock
- **File & Section:** `Plan/05_oop_design_plan.md` (Section 2.1 line 118) vs `Plan/01_concert_requirement_spec.md` (Table 2.13 line 427).
- **Defect:** `Plan/05` lists `PaymentGateway` as `Stripe, Omise, PayPal`. `Plan/01` lists `Stripe, Omise, Mock`.
- **Recommendation & Fix:** Standardize `PaymentGateway` to `Stripe, Omise, PayPal, Mock` so unit/integration testing with mock gateways compiles cleanly without breaking production gateway typing.

---

### 26. BookingReference Format Discrepancy
- **File & Section:** `Plan/05_oop_design_plan.md` (Section 3.1 line 385) vs `Plan/01_concert_requirement_spec.md` (Table 2.11 line 368).
- **Defect:** In `Plan/05`, `BookingReference` enforces `^TW-\d{4}-[A-Z0-9]{6}$` (e.g. `TW-2026-A1B2C3`). In `Plan/01`, sample booking number is `'TW-20260912-8924A'` (date-based format).
- **Recommendation & Fix:** Update regex to allow date-based segments: `^TW-\d{4}(\d{4})?-[A-Z0-9]{5,6}$`.

---

### 27. Non-Nullable VenueId in CreateEventCommand Prevents Online Events
- **File & Section:** `Plan/05_oop_design_plan.md` (Section 4 line 421, line 451) vs `Plan/01_concert_requirement_spec.md` (Table 2.7 line 277).
- **Defect:** In `Plan/01`, `events.venue_id` is explicitly nullable (`Nullable: YES, NULL = Online Event`). In `Plan/05`, `CreateEventCommand` defines `Guid VenueId` (non-nullable) and `EventDetailResponse` defines `VenueResponse Venue` (non-nullable). Online events without a physical venue will fail command validation and serialization.
- **Recommendation & Fix:** Change property to `Guid? VenueId` and `VenueResponse? Venue`.

---

# Minor Corrections and Typos

1. **`Plan/00_ROADMAP_READING_ORDER.md`**:
   - Heading says "Stage 3 ... 24 Entities & EF Core Mapping", but `Plan/06` only tabulates 22 entities in Section 2. Update summary count to reflect the complete set of 24 entities once `Review` and `Notification` are added.
2. **`Plan/01_concert_requirement_spec.md` Table 2.10 (seats)**:
   - Line 350: Change `Default 'Available'` description sample `'Hold'` to `'Hold'` and ensure C# enum comment explicitly mentions mapping.
3. **`Plan/02_master_project_plan.md` Table Registry**:
   - Line 124: Delete `promo_usages` entry or clarify that per-user redemption tracking is evaluated via `bookings` query with `promo_code_id`.
   - Line 130: Rename `waitlists` to `waitlist` to match PostgreSQL table name.
   - Line 134: Rename `system_configs` to `platform_configs` to match PostgreSQL table name.
4. **`Plan/07_api_endpoint_plan.md` Summary Table**:
   - Update summary count from 78 to 99 endpoints after adding the 21 missing endpoints.
