# 🏛️ Domain Entities & EF Core Mapping Plan

> **Target:** Clean Architecture Domain Layer (`src/Domain/Entities`) & Persistence Layer (`src/Infrastructure/Persistence/Configurations`)  
> **Source Specification:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md) (Database Data Dictionary v2.1)

---

## 1. Base Entity Abstractions (`src/Domain/Common`)

All database entities inherit standard contract abstractions to enforce consistency, auditing, and soft-delete behaviors.

```csharp
// Base entity with generic identifier
public abstract class BaseEntity<TId>
{
    public TId Id { get; protected set; } = default!;
    
    // Domain Events for decoupling business side-effects
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Marker interface for Aggregate Roots
public abstract class AggregateRoot<TId> : BaseEntity<TId> { }

// Automatic audit timestamps
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}

// Soft-delete marker (PDPA compliance)
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
    bool IsDeleted => DeletedAt.HasValue;
}
```

---

## 2. Entity Class Registry & Aggregates

### 2.1 Identity & User Access Aggregate
| Entity | Base / Interfaces | Table | Key Fields & Value Objects |
| :--- | :--- | :--- | :--- |
| `User` | `AggregateRoot<Guid>`, `IAuditableEntity`, `ISoftDeletable` | `users` | `Email`, `PasswordHash`, `FullName`, `Role` (Enum), `TwoFactorEnabled`, `IsBanned` |
| `RefreshToken` | `BaseEntity<Guid>` | `refresh_tokens` | `UserId`, `TokenHash`, `ExpiresAt`, `IsRevoked`, `ReplacedByToken` |
| `OrganizerProfile` | `AggregateRoot<Guid>`, `IAuditableEntity`, `ISoftDeletable` | `organizer_profiles` | `UserId`, `CompanyName`, `TaxId`, `BankAccountNoEnc` (AES Encrypted), `CommissionRate` |
| `OrganizerStaff` | `BaseEntity<Guid>`, `IAuditableEntity` | `organizer_staff` | `OrganizerId`, `UserId`, `EventId` (Nullable), `Permissions` (`List<string>`), `IsActive` |

### 2.2 Artist & Venue Catalog Aggregate
| Entity | Base / Interfaces | Table | Key Fields & Value Objects |
| :--- | :--- | :--- | :--- |
| `Artist` | `AggregateRoot<Guid>`, `IAuditableEntity` | `artists` | `Name`, `Slug`, `Genre`, `SocialLinks` (JSONB), `FollowerCount` (Atomic) |
| `ArtistFollower` | Junction Entity | `artist_followers` | Composite PK: `(UserId, ArtistId)`, `FollowedAt` |
| `Venue` | `AggregateRoot<Guid>`, `IAuditableEntity`, `ISoftDeletable` | `venues` | `Name`, `Address`, `City`, `Coordinates` (Point), `TotalCapacity` |
| `SeatLayout` | `BaseEntity<Guid>`, `IAuditableEntity` | `seat_layouts` | `VenueId`, `Name`, `LayoutJson` (JSONB/SVG coordinate map) |

### 2.3 Event & Ticketing Aggregate
| Entity | Base / Interfaces | Table | Key Fields & Value Objects |
| :--- | :--- | :--- | :--- |
| `Event` | `AggregateRoot<Guid>`, `IAuditableEntity`, `ISoftDeletable` | `events` | `OrganizerId`, `VenueId`, `Title`, `Slug`, `Status` (Enum), `StartAt`, `SoldSeats`, `TotalSeats` |
| `TicketTier` | `BaseEntity<Guid>` | `ticket_tiers` | `EventId`, `Name` (e.g. VIP, Regular), `Price`, `TotalQuota`, `AvailableQuota` |
| `Seat` | `BaseEntity<Guid>` | `seats` | `EventId`, `TierId`, `SeatNumber`, `Status` (Available, Held, Booked), `HeldBy`, `HeldUntil`, `Version` (`xmin` RowVersion) |

### 2.4 Booking & Financial Aggregate
| Entity | Base / Interfaces | Table | Key Fields & Value Objects |
| :--- | :--- | :--- | :--- |
| `Booking` | `AggregateRoot<Guid>`, `IAuditableEntity` | `bookings` | `UserId`, `EventId`, `BookingReference`, `TotalAmount`, `Status` (Pending, Confirmed, Cancelled) |
| `BookingItem` | `BaseEntity<Guid>` | `booking_items` | `BookingId`, `SeatId`, `TierId`, `UnitPrice`, `QrCodePayload` (Encrypted / Signed) |
| `Payment` | `BaseEntity<Guid>`, `IAuditableEntity` | `payments` | `BookingId`, `PaymentIntentId`, `Gateway`, `Amount`, `Currency`, `Status`, `IdempotencyKey` |
| `PromoCode` | `AggregateRoot<Guid>` | `promo_codes` | `Code`, `DiscountType`, `DiscountValue`, `MaxUses`, `UsedCount`, `ValidUntil` |
| `Payout` | `BaseEntity<Guid>`, `IAuditableEntity` | `payouts` | `OrganizerId`, `Amount`, `Status`, `BankAccountSnapshotEnc`, `ApprovedBy` |
| `Refund` | `BaseEntity<Guid>`, `IAuditableEntity` | `refunds` | `BookingId`, `Amount`, `Reason`, `Status`, `TransactionReference` |

### 2.5 Compliance, Waiting Room & System Operations
| Entity | Base / Interfaces | Table | Key Fields & Value Objects |
| :--- | :--- | :--- | :--- |
| `TicketTransfer` | `BaseEntity<Guid>`, `IAuditableEntity` | `ticket_transfers` | `BookingItemId`, `FromUserId`, `ToUserId`, `TransferStatus` |
| `Waitlist` | `BaseEntity<Guid>` | `waitlists` | `EventId`, `UserId`, `QueueNumber`, `Status` (Queued, Notified, Expired) |
| `WaitingRoomToken` | `BaseEntity<Guid>` | `waiting_room_tokens` | `EventId`, `TokenHash`, `PriorityScore`, `IssuedAt`, `ExpiresAt` |
| `AuditLog` | `BaseEntity<Guid>` | `audit_logs` | `ActorId`, `Action`, `EntityName`, `EntityId`, `OldValuesJson`, `NewValuesJson`, `IpAddress` |
| `SystemConfig` | `BaseEntity<string>` | `system_configs` | `Key` (PK), `ValueJson`, `Description`, `UpdatedAt` |

---

## 3. Sample Entity Implementation (`Seat.cs`)

High-contention entity demonstrating encapsulation, invariant protection, and optimistic concurrency.

```csharp
namespace Domain.Entities;

public class Seat : BaseEntity<Guid>
{
    public Guid EventId { get; private set; }
    public Guid TierId { get; private set; }
    public string Section { get; private set; } = null!;
    public string Row { get; private set; } = null!;
    public string SeatNumber { get; private set; } = null!;
    public SeatStatus Status { get; private set; } = SeatStatus.Available;
    
    public Guid? HeldByUserId { get; private set; }
    public DateTime? HeldUntil { get; private set; }
    public string? PaymentIntentId { get; private set; }
    
    // Concurrency token for PostgreSQL xmin
    public uint Version { get; private set; }

    private Seat() { } // Required by EF Core

    public Seat(Guid eventId, Guid tierId, string section, string row, string seatNumber)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        TierId = tierId;
        Section = section;
        Row = row;
        SeatNumber = seatNumber;
        Status = SeatStatus.Available;
    }

    public void Hold(Guid userId, TimeSpan duration)
    {
        if (Status != SeatStatus.Available && !(Status == SeatStatus.Held && HeldUntil < DateTime.UtcNow))
            throw new DomainException("Seat is not available for holding.");

        Status = SeatStatus.Held;
        HeldByUserId = userId;
        HeldUntil = DateTime.UtcNow.Add(duration);
    }

    public void ConfirmBooking()
    {
        if (Status != SeatStatus.Held)
            throw new DomainException("Cannot book a seat that is not held.");

        Status = SeatStatus.Booked;
        HeldUntil = null;
    }

    public void Release()
    {
        Status = SeatStatus.Available;
        HeldByUserId = null;
        HeldUntil = null;
        PaymentIntentId = null;
    }
}
```

---

## 4. EF Core Configuration & Mapping Rules (`IEntityTypeConfiguration<T>`)

### 4.1 Fluent API Entity Configuration (`SeatConfiguration.cs`)
```csharp
namespace Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("seats");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Section).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Row).HasMaxLength(20).IsRequired();
        builder.Property(s => s.SeatNumber).HasMaxLength(20).IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // PostgreSQL xmin Optimistic Concurrency token
        builder.UseXminAsConcurrencyToken();

        // Compound unique index (One seat per event row/number)
        builder.HasIndex(s => new { s.EventId, s.Section, s.Row, s.SeatNumber }).IsUnique();
        builder.HasIndex(s => new { s.EventId, s.Status });
    }
}
```

### 4.2 Global Query Filters for Soft-Delete
Configured globally in `ApplicationDbContext.OnModelCreating`:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    // Automatically apply soft-delete filter to all entities implementing ISoftDeletable
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "p");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.DeletedAt));
            var nullConstant = Expression.Constant(null, typeof(DateTime?));
            var comparison = Expression.Equal(property, nullConstant);
            var lambda = Expression.Lambda(comparison, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
```

### 4.3 Automatic Audit Fields Interceptor (`SaveEntitiesInterceptor`)
Intercepts `SaveChangesAsync` to populate timestamps automatically:
```csharp
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```
