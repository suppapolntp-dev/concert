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
| `EventArtist` | Junction Entity | `event_artists` | Composite PK: `(EventId, ArtistId)`, `IsHeadliner`, `DisplayOrder` |

### 2.3 Event & Inventory Aggregate
| Entity | Base / Interfaces | Table | Key Fields & Value Objects |
| :--- | :--- | :--- | :--- |
| `Event` | `AggregateRoot<Guid>`, `IAuditableEntity`, `ISoftDeletable` | `events` | `OrganizerId`, `VenueId`, `Title`, `Slug`, `Status` (Enum), `EventStart`, `SaleStart`, `SoldSeats`, `TotalSeats` |
| `Zone` | `BaseEntity<Guid>`, `IAuditableEntity` | `zones` | `EventId`, `Name` (e.g. VIP, Cat 1), `Color` (Hex), `Price`, `Capacity`, `IsNumberedSeat`, `SortOrder` |
| `Seat` | `BaseEntity<Guid>`, `IAuditableEntity` | `seats` | `ZoneId`, `RowLabel`, `SeatNumber`, `DisplayLabel`, `Status` (Available, Held, Booked, Disabled), `HeldBy`, `HeldUntil`, `PosX`, `PosY`, `Version` (`xmin` RowVersion) |

### 2.4 Booking & Financial Aggregate
| Entity | Base / Interfaces | Table | Key Fields & Value Objects |
| :--- | :--- | :--- | :--- |
| `Booking` | `AggregateRoot<Guid>`, `IAuditableEntity` | `bookings` | `UserId`, `EventId`, `BookingNumber`, `TotalAmount`, `DiscountAmount`, `NetAmount`, `Status` (Pending, Confirmed, Cancelled) |
| `BookingItem` | `BaseEntity<Guid>` | `booking_items` | `BookingId`, `SeatId`, `ZoneName` (Snapshot), `SeatLabel` (Snapshot), `Price`, `TicketCode` (Unique), `QrCodePayload` (Encrypted / Signed) |
| `Payment` | `BaseEntity<Guid>`, `IAuditableEntity` | `payments` | `BookingId`, `PaymentIntentId`, `Gateway`, `Amount`, `Currency`, `Status`, `IdempotencyKey` |
| `PromoCode` | `AggregateRoot<Guid>` | `promo_codes` | `Code`, `DiscountType`, `DiscountValue`, `MaxUses`, `UsedCount`, `ValidUntil` |
| `PromoUsage` | `BaseEntity<Guid>`, `IAuditableEntity` | `promo_usages` | `PromoCodeId`, `UserId`, `BookingId`, `DiscountApplied`, `UsedAt` |
| `Payout` | `BaseEntity<Guid>`, `IAuditableEntity` | `payouts` | `OrganizerId`, `Amount`, `Status`, `BankAccountSnapshotEnc`, `ApprovedBy` |
| `Refund` | `BaseEntity<Guid>`, `IAuditableEntity` | `refunds` | `BookingId`, `Amount`, `Reason`, `Status`, `TransactionReference` |

### 2.5 Operations, Compliance & Engagement
| Entity | Base / Interfaces | Table | Key Fields & Value Objects |
| :--- | :--- | :--- | :--- |
| `TicketTransfer` | `BaseEntity<Guid>`, `IAuditableEntity` | `ticket_transfers` | `BookingItemId`, `FromUserId`, `ToUserId`, `TransferStatus` |
| `Waitlist` | `BaseEntity<Guid>` | `waitlist` | `EventId`, `UserId`, `ZoneId` (Nullable), `Quantity`, `Status` (Waiting, Notified, Converted, Expired), `Priority` |
| `WaitingRoomToken` | `BaseEntity<Guid>` | `waiting_room_tokens` | `EventId`, `TokenHash`, `PriorityScore`, `IssuedAt`, `ExpiresAt` |
| `Notification` | `BaseEntity<Guid>`, `IAuditableEntity` | `notifications` | `UserId`, `Type` (Enum), `Title`, `Message`, `Link`, `MetadataJson`, `IsRead` |
| `Review` | `BaseEntity<Guid>`, `IAuditableEntity` | `reviews` | `UserId`, `EventId`, `Rating` (1-5), `Comment`, `IsHidden` (Unique: UserId + EventId) |
| `AuditLog` | `BaseEntity<long>` | `audit_logs` | `UserId`, `Action`, `EntityType`, `EntityId`, `OldValuesJson`, `NewValuesJson`, `IpAddress` |
| `PlatformConfig` | `BaseEntity<string>` | `platform_configs` | `Key` (PK), `ValueJson`, `Description`, `UpdatedBy`, `UpdatedAt` |

---

## 3. Sample Entity Implementation (`Seat.cs`)

High-contention entity demonstrating encapsulation, invariant protection, and optimistic concurrency.

```csharp
namespace Domain.Entities;

public class Seat : BaseEntity<Guid>
{
    public Guid ZoneId { get; private set; }
    public string? RowLabel { get; private set; }
    public string? SeatNumber { get; private set; }
    public string DisplayLabel { get; private set; } = null!;
    public SeatStatus Status { get; private set; } = SeatStatus.Available;
    
    public Guid? HeldBy { get; private set; }
    public DateTime? HeldUntil { get; private set; }
    public string? PaymentIntentId { get; private set; }
    
    public int PosX { get; private set; }
    public int PosY { get; private set; }

    // Concurrency token for PostgreSQL xmin
    public uint Version { get; private set; }

    private Seat() { } // Required by EF Core

    public Seat(Guid zoneId, string? rowLabel, string? seatNumber, string displayLabel, int posX = 0, int posY = 0)
    {
        Id = Guid.NewGuid();
        ZoneId = zoneId;
        RowLabel = rowLabel;
        SeatNumber = seatNumber;
        DisplayLabel = displayLabel;
        PosX = posX;
        PosY = posY;
        Status = SeatStatus.Available;
    }

    public void Hold(Guid userId, TimeSpan duration)
    {
        if (Status != SeatStatus.Available && !(Status == SeatStatus.Held && HeldUntil < DateTime.UtcNow))
            throw new DomainException("Seat is not available for holding.");

        Status = SeatStatus.Held;
        HeldBy = userId;
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
        HeldBy = null;
        HeldUntil = null;
        PaymentIntentId = null;
    }

    public void Disable()
    {
        if (Status == SeatStatus.Booked)
            throw new DomainException("Cannot disable an already booked seat.");
        Status = SeatStatus.Disabled;
    }

    public void Enable()
    {
        if (Status == SeatStatus.Disabled)
            Status = SeatStatus.Available;
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

        builder.Property(s => s.RowLabel).HasMaxLength(10);
        builder.Property(s => s.SeatNumber).HasMaxLength(10);
        builder.Property(s => s.DisplayLabel).HasMaxLength(20).IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // PostgreSQL xmin Optimistic Concurrency token
        builder.UseXminAsConcurrencyToken();

        // Compound unique index (One seat per zone row/number)
        builder.HasIndex(s => new { s.ZoneId, s.RowLabel, s.SeatNumber }).IsUnique();
        builder.HasIndex(s => new { s.ZoneId, s.Status });
        builder.HasIndex(s => s.HeldUntil);
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
