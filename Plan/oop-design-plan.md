# 🧱 OOP Design Plan — Enums, Value Objects, DTOs & Class Architecture

> **Architecture:** Clean Architecture (.NET 8 / C#) — Domain-Driven Design (DDD) Tactical Patterns  
> **Scope:** Complete Type System for TicketWorld Platform covering Enums, Value Objects, DTOs, Domain Services, and Design Patterns  
> **Related:** [Plan/entity-plan.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/entity-plan.md) | [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md)

---

## 1. OOP Design Principles (กฎพื้นฐานที่ทีมต้องยึดถือ)

| # | Principle | Actionable Rule |
| :---: | :--- | :--- |
| 1 | **Encapsulation** | Property setter ต้องเป็น `private` หรือ `protected` เสมอ เปลี่ยนสถานะผ่าน Domain Method เท่านั้น |
| 2 | **Immutability First** | Value Objects ต้อง `readonly record struct` — สร้างแล้วห้ามแก้ |
| 3 | **Rich Domain Model** | Business logic อยู่ใน Entity/Value Object ไม่ใช่ใน Service class (No Anemic Model) |
| 4 | **Single Responsibility** | 1 class ทำ 1 หน้าที่ — Command Handler ไม่ validate, Validator ไม่ persist |
| 5 | **Dependency Inversion** | Domain/Application ขึ้นกับ Interface เท่านั้น ห้ามรู้จัก Concrete class ของ Infrastructure |
| 6 | **Guard Clauses** | ตรวจ invariant ที่ constructor/method ทันที ไม่รอให้ downstream จับ error |
| 7 | **Fail Fast** | throw domain exception ทันทีเมื่อ rule ถูกละเมิด ไม่ return null หรือ error code |

---

## 2. Enum Registry (`src/Domain/Enums/`)

### 2.1 Complete Enum List

```csharp
// ─────────────────────────────────────────────────────
// IDENTITY & ACCESS
// ─────────────────────────────────────────────────────

public enum UserRole
{
    Customer,        // Level 1 — ซื้อตั๋ว, ติดตามศิลปิน (รวมถึง Staff ที่ได้รับสิทธิ์ใน organizer_staff)
    Organizer,       // Level 2 — จัดงาน, สร้าง Event, เจ้าขององค์กร
    Admin            // Level 3 — ผู้ดูแลระบบส่วนกลาง
}
// หมายเหตุ: สิทธิ์พนักงานหน้างาน (Staff) ใช้ระบบ Event-Scoped Claims ผ่านตาราง organizer_staff (เช่น 'checkin.scan') ไม่ต้องเพิ่ม Global Role แยก

public enum AuthProvider
{
    Local,           // Email + Password
    Google           // OAuth 2.0 Google Sign-In
}

// ─────────────────────────────────────────────────────
// EVENT LIFECYCLE
// ─────────────────────────────────────────────────────

public enum EventStatus
{
    Draft,           // กำลังร่าง ยังไม่ส่งตรวจ
    PendingApproval, // ส่งให้ Admin ตรวจสอบแล้ว
    Published,       // Admin อนุมัติแล้ว รอเปิดขาย
    OnSale,          // เปิดขายอยู่
    SoldOut,         // ตั๋วหมด (กลับ OnSale ได้ถ้ามี Refund)
    Completed,       // งานจบแล้ว
    Cancelled        // ยกเลิก (Trigger Auto-Refund)
}

public enum EventCategory
{
    Concert,
    Festival,
    Sports,
    Theater,
    Conference,
    Exhibition,
    FanMeeting,
    Other
}

// ─────────────────────────────────────────────────────
// SEAT & INVENTORY
// ─────────────────────────────────────────────────────

public enum SeatStatus
{
    Available,       // ว่าง พร้อมจอง
    Held,            // ถูก Hold อยู่ (10 นาที)
    Booked,          // ชำระเงินแล้ว
    Disabled         // Organizer ปิดที่นั่งนี้
}

public enum ZoneType
{
    VIP,
    Premium,
    Standard,
    Economy,
    Standing,
    Wheelchair        // ที่นั่ง Accessible
}

// ─────────────────────────────────────────────────────
// BOOKING & PAYMENT
// ─────────────────────────────────────────────────────

public enum BookingStatus
{
    Pending,         // Hold ที่นั่งแล้ว รอชำระเงิน
    Confirmed,       // ชำระเงินสำเร็จ
    Cancelled,       // ยกเลิก (หมดเวลา / ผู้ใช้กดยกเลิก)
    Refunded,        // คืนเงินแล้ว
    Transferred      // โอนตั๋วให้ผู้อื่นแล้ว
}

public enum PaymentStatus
{
    Pending,         // รอ Gateway ตอบกลับ
    Processing,      // อยู่ระหว่างดำเนินการ
    Succeeded,       // ชำระสำเร็จ
    Failed,          // ถูกปฏิเสธ
    Cancelled,       // ผู้ใช้ยกเลิก
    Refunded         // คืนเงินแล้ว
}

public enum PaymentGateway
{
    Stripe,
    Omise,
    PayPal
}

public enum DiscountType
{
    Percentage,      // ลด X%
    FixedAmount      // ลดคงที่ X บาท
}

public enum RefundReason
{
    CustomerRequest,    // ลูกค้าขอคืน
    EventCancelled,     // งานถูกยกเลิก → auto-refund ทั้งหมด
    PaymentRaceLost,    // ชำระเงินสำเร็จ แต่ที่นั่งถูกคนอื่นชิง
    AdminDispute,       // Admin ตัดสิน
    DuplicatePayment    // ชำระซ้ำ
}

public enum RefundStatus
{
    Requested,
    Approved,
    Processing,
    Completed,
    Rejected
}

// ─────────────────────────────────────────────────────
// OPERATIONS
// ─────────────────────────────────────────────────────

public enum PayoutStatus
{
    Pending,         // รอ Admin อนุมัติ
    Approved,        // อนุมัติแล้ว รอโอน
    Processing,      // กำลังโอน
    Completed,       // โอนสำเร็จ
    Failed           // โอนไม่สำเร็จ
}

public enum WaitlistStatus
{
    Queued,          // อยู่ในคิว
    Notified,        // แจ้งแล้ว รอผู้ใช้ตัดสินใจ
    Converted,       // ผู้ใช้ซื้อตั๋วสำเร็จ
    Expired          // ไม่ตอบสนอง หมดเวลา (ถาวร)
}

public enum TransferStatus
{
    Pending,         // รอผู้รับยืนยัน
    Accepted,        // ผู้รับยืนยันแล้ว
    Rejected,        // ผู้รับปฏิเสธ
    Expired,         // หมดเวลา 24 ชม.
    Cancelled        // ผู้ส่งยกเลิก
}

public enum NotificationType
{
    BookingConfirmed,
    BookingCancelled,
    EventReminder,
    EventApproved,
    EventRejected,
    PayoutCompleted,
    WaitlistNotified,
    TicketTransferReceived,
    SecurityAlert
}
```

---

## 3. Value Objects (`src/Domain/ValueObjects/`)

Value Objects เป็นค่าที่ **ไม่มี Identity** เปรียบเทียบกันด้วย Value ไม่ใช่ Reference — ต้อง Immutable เสมอ

### 3.1 Complete Value Object Registry

```csharp
// ──────────────────────────────────────────
// Email: RFC 5322, normalized lowercase
// ──────────────────────────────────────────
public readonly record struct Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        value = value.Trim().ToLowerInvariant();

        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException($"Invalid email format: {value}");

        Value = value;
    }

    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;
}

// ──────────────────────────────────────────
// Money: Amount + Currency (ป้องกันบวกคนละสกุล)
// ──────────────────────────────────────────
public readonly record struct Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new DomainException("Money amount cannot be negative.");

        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot add {Currency} to {other.Currency}.");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot subtract different currencies.");

        if (Amount < other.Amount)
            throw new DomainException("Insufficient amount.");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money ApplyDiscount(decimal percentage)
    {
        if (percentage is < 0 or > 100)
            throw new DomainException("Discount percentage must be between 0 and 100.");

        return new Money(Amount * (1 - percentage / 100m), Currency);
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}

public enum Currency
{
    THB, USD, JPY, KRW, EUR, GBP, SGD
}

// ──────────────────────────────────────────
// Phone: E.164 format (+ country code)
// ──────────────────────────────────────────
public readonly record struct PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (!Regex.IsMatch(value, @"^\+[1-9]\d{6,14}$"))
            throw new DomainException($"Invalid E.164 phone number: {value}");

        Value = value;
    }

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}

// ──────────────────────────────────────────
// Slug: URL-safe lowercase identifier
// ──────────────────────────────────────────
public readonly record struct Slug
{
    public string Value { get; }

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Slug cannot be empty.");

        Value = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9-]", "-");
        Value = Regex.Replace(Value, @"-{2,}", "-").Trim('-');

        if (Value.Length > 300)
            throw new DomainException("Slug exceeds maximum length of 300.");
    }
}

// ──────────────────────────────────────────
// GeoCoordinate: (Latitude, Longitude)
// ──────────────────────────────────────────
public readonly record struct GeoCoordinate
{
    public double Latitude { get; }
    public double Longitude { get; }

    public GeoCoordinate(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new DomainException("Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180)
            throw new DomainException("Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }
}

// ──────────────────────────────────────────
// DateRange: Start ต้องมาก่อน End เสมอ
// ──────────────────────────────────────────
public readonly record struct DateRange
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public DateRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new DomainException("End date must be after start date.");

        Start = start;
        End = end;
    }

    public bool Contains(DateTime point) => point >= Start && point <= End;
    public bool Overlaps(DateRange other) => Start < other.End && End > other.Start;
    public TimeSpan Duration => End - Start;
}

// ──────────────────────────────────────────
// Percentage: 0-100 range
// ──────────────────────────────────────────
public readonly record struct Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value is < 0 or > 100)
            throw new DomainException("Percentage must be between 0 and 100.");

        Value = value;
    }

    public decimal ApplyTo(decimal amount) => amount * (Value / 100m);
}

// ──────────────────────────────────────────
// BookingReference: TW-YYYY-XXXXXX (auto-gen)
// ──────────────────────────────────────────
public readonly record struct BookingReference
{
    public string Value { get; }

    public BookingReference(string value)
    {
        if (!Regex.IsMatch(value, @"^TW-\d{4}-[A-Z0-9]{6}$"))
            throw new DomainException($"Invalid booking reference format: {value}");

        Value = value;
    }

    public static BookingReference Generate()
    {
        var year = DateTime.UtcNow.Year;
        var random = RandomNumberGenerator.GetBytes(4);
        var code = Convert.ToHexString(random)[..6].ToUpperInvariant();
        return new BookingReference($"TW-{year}-{code}");
    }
}
```

---

## 4. DTO / Request / Response Layer (`src/Application/`)

### 4.1 Naming Conventions
| Type | Pattern | Example | Location |
| :--- | :--- | :--- | :--- |
| **Command** | `VerbNounCommand` | `CreateEventCommand` | `Features/Events/Commands/` |
| **Query** | `GetNounQuery` | `GetEventBySlugQuery` | `Features/Events/Queries/` |
| **Response DTO** | `NounResponse` | `EventResponse` | `Features/Events/Queries/` |
| **List Item DTO** | `NounSummaryDto` | `EventSummaryDto` | `Features/Events/Queries/` |
| **Validator** | `VerbNounCommandValidator` | `CreateEventCommandValidator` | `Features/Events/Commands/` |

### 4.2 Sample CQRS Objects

```csharp
// ── COMMAND (Write) ──────────────────────────────────
public sealed record CreateEventCommand(
    string Title,
    string Description,
    Guid VenueId,
    DateTime StartAt,
    DateTime EndAt,
    string Currency,               // "THB", "USD"
    DateTime SaleStartAt,
    DateTime SaleEndAt,
    bool WaitingRoomEnabled,
    List<CreateZoneDto> Zones
) : IRequest<Guid>;

public sealed record CreateZoneDto(
    string Name,                    // "VIP", "Standard"
    string Color,                   // "#EC4899"
    decimal Price,
    int Capacity,
    bool IsNumberedSeat
);

// ── QUERY (Read) ─────────────────────────────────────
public sealed record GetEventBySlugQuery(
    string Slug
) : IRequest<EventDetailResponse>;

// ── RESPONSE DTO ─────────────────────────────────────
public sealed record EventDetailResponse(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    string Status,
    DateTime StartAt,
    DateTime EndAt,
    VenueResponse Venue,
    List<ZoneResponse> Zones,
    List<ArtistSummaryDto> Artists,
    int SoldSeats,
    int TotalSeats,
    bool WaitingRoomEnabled
);

public sealed record ZoneResponse(
    Guid Id,
    string Name,
    string Color,
    decimal Price,
    string Currency,
    int AvailableSeats,
    int Capacity,
    bool IsNumberedSeat
);

// ── LIST ITEM (lightweight) ──────────────────────────
public sealed record EventSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string CoverImageUrl,
    string Status,
    DateTime StartAt,
    string VenueName,
    string City,
    decimal StartingPrice,
    string Currency,
    int SoldPercentage                // Pre-calculated: (sold / total) * 100
);

// ── PAGINATED WRAPPER ────────────────────────────────
public sealed record PaginatedList<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

---

## 5. Domain Services (`src/Domain/Services/`)

Domain Services มีไว้สำหรับ Business Logic ที่ **ไม่เป็นของ Entity ใดตัวเดียว** หรือต้องใช้หลาย Aggregate ร่วมกัน

```csharp
// ── Promo Code Redemption (ข้าม Booking + PromoCode aggregate) ──
public sealed class PromoRedemptionService
{
    public Money CalculateDiscount(PromoCode promo, Money originalPrice)
    {
        if (!promo.IsActive || promo.IsExpired)
            throw new DomainException("PROMO_INVALID");

        if (promo.IsExhausted)
            throw new DomainException("PROMO_EXHAUSTED");

        return promo.DiscountType switch
        {
            DiscountType.Percentage  => originalPrice.ApplyDiscount(promo.DiscountValue),
            DiscountType.FixedAmount => originalPrice.Subtract(
                                            new Money(promo.DiscountValue, originalPrice.Currency)),
            _ => throw new DomainException("Unknown discount type.")
        };
    }
}

// ── Ticket Transfer Eligibility (ข้าม Booking + Event + User) ──
public sealed class TicketTransferService
{
    public void ValidateTransfer(BookingItem item, Booking booking, Event ev, Guid recipientId)
    {
        if (booking.Status != BookingStatus.Confirmed)
            throw new DomainException("BOOKING_NOT_CONFIRMED");

        if (ev.Status == EventStatus.Completed || ev.Status == EventStatus.Cancelled)
            throw new DomainException("EVENT_ENDED");

        if (ev.StartAt <= DateTime.UtcNow.AddHours(2))
            throw new DomainException("TRANSFER_TOO_CLOSE_TO_EVENT");

        if (booking.UserId == recipientId)
            throw new DomainException("CANNOT_TRANSFER_TO_SELF");
    }
}
```

---

## 6. Domain Exceptions (`src/Domain/Exceptions/`)

ข้อผิดพลาดจาก Domain ต้อง throw Exception เฉพาะเจาะจง เพื่อให้ Global Handler แปลงเป็น HTTP Status Code ถูกต้อง

```csharp
// ── Base ─────────────────────────────────
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}

// ── Specific Exceptions ─────────────────
public sealed class NotFoundException : DomainException            // → 404
{
    public NotFoundException(string entity, object id)
        : base("NOT_FOUND", $"{entity} with id '{id}' was not found.") { }
}

public sealed class ConflictException : DomainException            // → 409
{
    public ConflictException(string message)
        : base("CONFLICT", message) { }
}

public sealed class ForbiddenException : DomainException           // → 403
{
    public ForbiddenException(string message = "Access denied.")
        : base("FORBIDDEN", message) { }
}

public sealed class BusinessRuleException : DomainException        // → 422
{
    public BusinessRuleException(string errorCode, string message)
        : base(errorCode, message) { }
}

// ── ตัวอย่างจาก Business Rules ──────────
public sealed class SeatUnavailableException : ConflictException
{
    public SeatUnavailableException(string seatNumber)
        : base($"Seat {seatNumber} is not available.") { }
}

public sealed class PromoExhaustedException : BusinessRuleException
{
    public PromoExhaustedException()
        : base("PROMO_EXHAUSTED", "Promo code has reached maximum usage.") { }
}

public sealed class HoldExpiredException : BusinessRuleException
{
    public HoldExpiredException()
        : base("HOLD_EXPIRED", "Your seat hold has expired. Please select seats again.") { }
}

public sealed class TransferTooCloseException : BusinessRuleException
{
    public TransferTooCloseException()
        : base("TRANSFER_TOO_CLOSE", "Cannot transfer tickets within 2 hours of event start.") { }
}
```

---

## 7. Domain Events (`src/Domain/Events/`)

Domain Events เป็นการสื่อสาร Side Effects ระหว่าง Aggregate โดยไม่ให้ Aggregate เรียกกันตรงๆ

```csharp
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

// ── Event Published ─────────────────────
// Handlers: NotifyFollowersHandler, IndexEventSearchHandler
public sealed record EventPublishedEvent(
    Guid EventId,
    string Title,
    List<Guid> ArtistIds
) : DomainEventBase;

// ── Seat Held ───────────────────────────
// Handlers: BroadcastSeatStatusHandler (SignalR), ScheduleHoldExpiryHandler (Hangfire)
public sealed record SeatHeldEvent(
    Guid SeatId,
    Guid EventId,
    Guid UserId,
    DateTime HeldUntil
) : DomainEventBase;

// ── Booking Confirmed ───────────────────
// Handlers: IssueQRTicketsHandler, SendConfirmationEmailHandler, UpdateSoldCountHandler
public sealed record BookingConfirmedEvent(
    Guid BookingId,
    Guid UserId,
    Guid EventId,
    List<Guid> SeatIds,
    decimal TotalAmount,
    string Currency
) : DomainEventBase;

// ── Booking Cancelled ───────────────────
// Handlers: ReleaseSeatHandler, TriggerRefundHandler, NotifyWaitlistHandler
public sealed record BookingCancelledEvent(
    Guid BookingId,
    Guid EventId,
    List<Guid> SeatIds,
    RefundReason Reason
) : DomainEventBase;

// ── Artist Followed ─────────────────────
// Handlers: IncrementFollowerCountHandler (Atomic SQL)
public sealed record ArtistFollowedEvent(
    Guid UserId,
    Guid ArtistId
) : DomainEventBase;
```

---

## 8. Repository & Service Interfaces (`src/Domain/Repositories/`)

Domain Layer กำหนดแค่ Interface — Infrastructure เป็นคนทำ Concrete Implementation

```csharp
// ── Generic Repository ───────────────────
public interface IRepository<T, TId> where T : AggregateRoot<TId>
{
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}

// ── Specific Repositories ────────────────
public interface IEventRepository : IRepository<Event, Guid>
{
    Task<Event?> GetBySlugAsync(Slug slug, CancellationToken ct = default);
    Task<PaginatedList<Event>> SearchAsync(EventSearchCriteria criteria, CancellationToken ct = default);
}

public interface ISeatRepository : IRepository<Seat, Guid>
{
    Task<List<Seat>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);
    Task<List<Seat>> GetAvailableByIdsForUpdateAsync(List<Guid> seatIds, CancellationToken ct = default);
}

public interface IBookingRepository : IRepository<Booking, Guid>
{
    Task<int> CountActiveByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken ct = default);
    Task<Booking?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default);
}

// ── Unit of Work (Transaction Boundary) ──
public interface IUnitOfWork
{
    IEventRepository Events { get; }
    ISeatRepository Seats { get; }
    IBookingRepository Bookings { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}

// ── Application Service Interfaces ───────
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    UserRole Role { get; }
    Guid? OrganizerId { get; }
    bool HasPermission(string claim);
}

public interface IDateTimeService
{
    DateTime UtcNow { get; }
}

public interface IPasswordHasher
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string hashedPassword);
}

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateAccessToken(string token);
}

public interface IPaymentGateway
{
    Task<PaymentIntentResult> CreateIntentAsync(Money amount, string idempotencyKey, CancellationToken ct);
    Task<RefundResult> RefundAsync(string paymentIntentId, Money amount, CancellationToken ct);
}

public interface IDistributedLockService
{
    Task<IAsyncDisposable> AcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct);
}

public interface IFileStorageService
{
    Task<string> GeneratePresignedUploadUrlAsync(string bucket, string objectKey, TimeSpan expiry);
    Task DeleteAsync(string bucket, string objectKey, CancellationToken ct);
}
```

---

## 9. Class Hierarchy Summary Diagram

```
Domain/
├── Common/
│   ├── BaseEntity<TId>              (abstract class — Id, DomainEvents)
│   ├── AggregateRoot<TId>           (abstract class — extends BaseEntity)
│   ├── IAuditableEntity             (interface — CreatedAt, UpdatedAt)
│   ├── ISoftDeletable               (interface — DeletedAt)
│   └── IDomainEvent                 (interface — OccurredAt)
│
├── Enums/
│   ├── UserRole                     Customer | Organizer | Admin (Staff via organizer_staff)
│   ├── AuthProvider                 Local | Google
│   ├── EventStatus                  Draft → PendingApproval → Published → OnSale → ...
│   ├── EventCategory                Concert | Festival | Sports | ...
│   ├── SeatStatus                   Available | Held | Booked | Disabled
│   ├── ZoneType                     VIP | Premium | Standard | Standing | Wheelchair
│   ├── BookingStatus                Pending | Confirmed | Cancelled | Refunded | Transferred
│   ├── PaymentStatus                Pending | Processing | Succeeded | Failed | ...
│   ├── PaymentGateway               Stripe | Omise | PayPal
│   ├── DiscountType                 Percentage | FixedAmount
│   ├── RefundReason                 CustomerRequest | EventCancelled | PaymentRaceLost | ...
│   ├── RefundStatus                 Requested → Approved → Processing → Completed
│   ├── PayoutStatus                 Pending → Approved → Processing → Completed
│   ├── WaitlistStatus               Queued → Notified → Converted | Expired
│   ├── TransferStatus               Pending → Accepted | Rejected | Expired
│   └── NotificationType             BookingConfirmed | EventReminder | SecurityAlert | ...
│
├── ValueObjects/
│   ├── Email                        (readonly record struct — RFC 5322 + lowercase)
│   ├── Money                        (readonly record struct — Amount + Currency + math ops)
│   ├── PhoneNumber                  (readonly record struct — E.164 format)
│   ├── Slug                         (readonly record struct — URL-safe)
│   ├── GeoCoordinate                (readonly record struct — lat/lng bounds check)
│   ├── DateRange                    (readonly record struct — Start < End invariant)
│   ├── Percentage                   (readonly record struct — 0-100 range)
│   └── BookingReference             (readonly record struct — TW-YYYY-XXXXXX generator)
│
├── Entities/                        → See entity-plan.md for full mapping
│   ├── User (AggregateRoot)
│   ├── Event (AggregateRoot)
│   ├── Seat (BaseEntity)
│   ├── Booking (AggregateRoot)
│   └── ... (24 entities total)
│
├── Events/
│   ├── EventPublishedEvent
│   ├── SeatHeldEvent
│   ├── BookingConfirmedEvent
│   ├── BookingCancelledEvent
│   └── ArtistFollowedEvent
│
├── Exceptions/
│   ├── DomainException              (abstract base — ErrorCode + Message)
│   ├── NotFoundException             → 404
│   ├── ConflictException             → 409
│   ├── ForbiddenException            → 403
│   ├── BusinessRuleException         → 422
│   ├── SeatUnavailableException
│   ├── PromoExhaustedException
│   ├── HoldExpiredException
│   └── TransferTooCloseException
│
├── Services/
│   ├── PromoRedemptionService
│   └── TicketTransferService
│
└── Repositories/
    ├── IRepository<T, TId>
    ├── IEventRepository
    ├── ISeatRepository
    ├── IBookingRepository
    └── IUnitOfWork
```
