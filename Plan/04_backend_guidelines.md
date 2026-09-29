# 🛠️ Backend Development Guidelines
> **Stack:** .NET 8 (C#) | Clean Architecture | PostgreSQL 16 | Redis | MediatR (CQRS) | EF Core & Dapper

---

## 1. Project Architecture (Clean Architecture + CQRS)

```
src/
├── Domain/                 # Enterprise Business Rules (Zero external dependencies)
│   ├── Entities/           # Domain models with encapsulated business logic
│   ├── ValueObjects/       # Immutable value types (e.g., Money, EmailAddress)
│   ├── Enums/              # Business status & type enums
│   ├── Events/             # Domain events (e.g., SeatHeldDomainEvent)
│   ├── Exceptions/         # Domain-specific business exceptions
│   └── Repositories/       # Repository interfaces
│
├── Application/            # Application Business Rules (Use Cases)
│   ├── Common/             # Interfaces, Behaviors, DTOs, Mappings
│   │   ├── Behaviors/      # MediatR Pipeline (Validation, Logging, Performance)
│   │   └── Interfaces/     # Service contracts (ICurrentUserService, IDateTime)
│   └── Features/           # Vertical slices organized by aggregate / domain
│       └── [FeatureName]/  # e.g., Bookings, Events, Seats
│           ├── Commands/   # Write operations (Command, Handler, Validator)
│           └── Queries/    # Read operations (Query, Handler, DTO)
│
├── Infrastructure/         # External Concerns & Implementations
│   ├── Persistence/        # EF Core DbContext, Configurations, Migrations
│   ├── Repositories/       # Repository & UnitOfWork implementations
│   ├── Caching/            # Redis cache service & distributed locks
│   ├── Identity/           # JWT token generator, password hashing (Argon2id)
│   └── Services/           # Third-party integrations (Payment, Storage, Email)
│
└── WebAPI/                 # Presentation Layer & Entry Point
    ├── Controllers/        # Thin API controllers or Minimal API endpoints
    ├── Middlewares/        # Exception handler, CorrelationId, Rate limiting
    ├── Extensions/         # Service collection configuration & DI setup
    └── Program.cs          # Application bootstrapper
```

### Dependency Rules
- Inward direction only: `WebAPI` ➜ `Infrastructure` ➜ `Application` ➜ `Domain`.
- `Domain` has zero references to other projects, third-party libraries, or ORMs.
- `Application` references only `Domain`.

---

## 2. Coding Standards & Conventions

### 2.1 Naming Conventions
- **Classes / Records / Enums:** `PascalCase` (`BookingService`, `SeatStatus`)
- **Interfaces:** `IPascalCase` (`IBookingRepository`, `IEventService`)
- **Async Methods:** `PascalCase` + `Async` suffix (`GetByIdAsync`, `CreateBookingAsync`)
- **Variables / Parameters:** `camelCase` (`bookingId`, `cancellationToken`)
- **Private Readonly Fields:** `_camelCase` (`_dbContext`, `_logger`)
- **MediatR Requests:** `VerbSubjectCommand` / `GetSubjectQuery` (`CreateBookingCommand`, `GetEventByIdQuery`)

### 2.2 Async & Cancellation
- Always propagate `CancellationToken cancellationToken = default` down to every I/O call.
- Never use `.Result` or `.Wait()`. Always use `await`.
- Free UI/Context threads with `.ConfigureAwait(false)` in non-WebAPI assemblies.

### 2.3 Null Safety
- Enable `<Nullable>enable</Nullable>`.
- Return empty collections instead of `null` (`Array.Empty<T>()`, `List<T>`).
- Validate incoming arguments using Guard Clauses or Value Objects.

---

## 3. API Design Standards (RESTful)

### 3.1 Routing Conventions
- Use plural nouns: `/api/v1/events`, `/api/v1/bookings`.
- Nested resources for parent-child relations: `/api/v1/events/{eventId}/seats`.
- State changes / actions via verbs if outside CRUD: `POST /api/v1/bookings/{id}/cancel`.

### 3.2 Standard JSON Responses
- **Success:**
```json
{
  "success": true,
  "data": { ... },
  "metadata": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100
  }
}
```

- **Error (RFC 7807 ProblemDetails):**
```json
{
  "type": "https://ticketworld.io/errors/conflict",
  "title": "Seat Unavailable",
  "status": 409,
  "detail": "Seat A-12 is currently held by another user.",
  "instance": "/api/v1/seats/hold",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

### 3.3 HTTP Status Codes
- `200 OK`: Successful read or update.
- `201 Created`: Successful creation (include `Location` header).
- `204 No Content`: Successful deletion or action without return body.
- `400 Bad Request`: Malformed syntax or bad parameters.
- `401 Unauthorized`: Missing or invalid JWT.
- `403 Forbidden`: Authenticated, but lacking permission or resource ownership.
- `404 Not Found`: Resource does not exist.
- `409 Conflict`: Concurrency conflict or duplicate record.
- `422 Unprocessable Entity`: Business logic / domain rule validation failure.
- `429 Too Many Requests`: Rate limit exceeded.
- `500 Internal Server Error`: Unhandled server exception.

### 3.4 Idempotency
- Mutating endpoints that process payments or transactions (`POST /bookings`, `POST /checkout`) must accept `X-Idempotency-Key`.
- Store key in Redis with 24-hour TTL:
  - Cache initial request state (`In-Progress`).
  - Cache final response payload and status code to return identical responses on retry.

---

## 4. Database & Persistence (PostgreSQL 16)

### 4.1 Hybrid Data Access
- **EF Core:** For Domain write models, state transitions, and change tracking.
- **Dapper:** For complex queries, dashboards, and high-performance read-only projections.

### 4.2 Atomic Counters (Prevent Race Conditions)
- Never perform Read-Modify-Write in application memory for shared counters:
```csharp
// ❌ WRONG: Prone to race condition
var promo = await _context.PromoCodes.FindAsync(id);
if (promo.UsedCount < promo.MaxUses) {
    promo.UsedCount++;
    await _context.SaveChangesAsync();
}

// ✅ RIGHT: Single atomic database update
var updatedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
    UPDATE promo_codes
    SET used_count = used_count + 1
    WHERE id = {id} AND (max_uses IS NULL OR used_count < max_uses) AND is_active = TRUE;");
if (updatedRows == 0) throw new PromoExhaustedException();
```

### 4.3 Concurrency Control
- **PostgreSQL `xmin` / RowVersion:** Use optimistic locking for standard entities.
- **Redis Distributed Lock (RedLock):** Use short-lived locks (e.g., 3s TTL) for high-contention booking holds:
```csharp
await using var redLock = await _lockProvider.CreateLockAsync($"lock:seat:{seatId}", TimeSpan.FromSeconds(3));
if (!redLock.IsAcquired) throw new SeatLockedException();
```

### 4.4 Migrations
- Use EF Core Code-First Migrations.
- Apply Expand & Contract pattern for breaking schema alterations.
- Never block long transactions in production migrations.

### 4.5 Database Seeding Strategy (PostgreSQL & EF Core)

#### 1. Seed Data Classification
* **Master / System Data (All Environments including Prod):**
  * Roles, Permission Claims, System Admin account, System Configurations.
  * Must be strictly idempotent (safe to execute repeatedly).
* **Development / Test Fixtures (Dev & Staging Only):**
  * Sample Artists, Venues, Events, Mock Bookings, and Seat layouts.
  * Executed only when `builder.Environment.IsDevelopment()` or via dedicated CLI flag (`--seed-dev`).

#### 2. Seeding Pattern (`IDataSeeder` via Code)
* **Do NOT use EF Core `HasData()`** for transactional or large-volume entities (avoids rigid migration bloat and fixed ID collision).
* Implement an explicit `IDataSeeder` / `DbContextInitializer` invoked after `MigrateAsync()`:
```csharp
public class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Master Data: Ensure Roles & Default Admin exist
        if (!await _context.Users.AnyAsync(u => u.Role == "Admin", cancellationToken))
        {
            _context.Users.Add(new User
            {
                Email = "admin@ticketworld.io",
                PasswordHash = _passwordHasher.Hash("SecureAdminP@ss2026!"),
                Role = "Admin",
                IsVerified = true
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

#### 3. High-Volume Bulk Seeding (PostgreSQL Optimization)
* For large datasets (e.g., 5,000 - 50,000 venue seats per event), avoid single `INSERT` statements or EF Core change tracker:
  * **Option A (Docker local init):** Place `.sql` dump scripts into `./docker-entrypoint-initdb.d/` mapped to PostgreSQL container for initial instant boot.
  * **Option B (In-App Bulk Copy):** Use `NpgsqlBinaryImporter` (`COPY FROM STDIN BINARY`) to bulk stream seats directly into PostgreSQL with minimal memory overhead:
```csharp
await using var writer = await connection.BeginBinaryImportAsync(
    "COPY seats (id, event_id, section, row, seat_number, price, status) FROM STDIN (FORMAT BINARY)");
foreach (var seat in generatedSeats) {
    await writer.StartRowAsync();
    await writer.WriteAsync(seat.Id);
    await writer.WriteAsync(seat.EventId);
    await writer.WriteAsync(seat.Section);
    await writer.WriteAsync(seat.Row);
    await writer.WriteAsync(seat.SeatNumber);
    await writer.WriteAsync(seat.Price);
    await writer.WriteAsync(seat.Status);
}
await writer.CompleteAsync();
```

### 4.6 Domain Entities & EF Core Mapping Standards
> Detailed entity schema & code samples: [Plan/entity-plan.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/entity-plan.md)

#### 1. Entity Base Architecture
All entities in `src/Domain/Entities` must inherit standard abstractions:
* **`BaseEntity<TId>`:** Encapsulates `Id` and `List<IDomainEvent>` for event-driven side effects.
* **`AggregateRoot<TId>`:** Defines transaction and boundary consistency for aggregates (e.g., `Event`, `Booking`).
* **`IAuditableEntity`:** Mandates `CreatedAt` and `UpdatedAt` (automatically populated by EF Core `SaveChangesInterceptor`).
* **`ISoftDeletable`:** Mandates `DateTime? DeletedAt` with automatic global query filter `DeletedAt == null`.

#### 2. Domain Encapsulation Rules
* **No Anemic Models:** Setters must be `private` or `protected`. State changes occur strictly through explicit domain methods.
* **Constructor Rule:** Expose a parameterized constructor enforcing required properties, plus a `private` parameterless constructor for EF Core reflection.
```csharp
public class Seat : BaseEntity<Guid>
{
    public Guid EventId { get; private set; }
    public SeatStatus Status { get; private set; }
    public uint Version { get; private set; } // PostgreSQL xmin concurrency token

    private Seat() { } // EF Core only

    public void Hold(Guid userId, TimeSpan duration) { ... }
}
```

#### 3. Fluent Configuration Standards (`IEntityTypeConfiguration<T>`)
* Keep `DbContext` clean by separating mappings into `src/Infrastructure/Persistence/Configurations/`.
* Always use `builder.ToTable("lowercase_plural")`.
* Enforce PostgreSQL optimistic locking on high-concurrency entities:
```csharp
builder.UseXminAsConcurrencyToken();
```
* Store Enums as strings for database readability:
```csharp
builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
```

---

## 5. Security & Authentication

### 5.1 Token Management
- **Access Token:** Short-lived JWT (15 minutes), RS256 algorithm.
- **Refresh Token:** Stored in `httpOnly`, `Secure`, `SameSite=Strict` cookie.
- **Rotation & Reuse Detection:** Every refresh invalidates previous token; revoked token reuse triggers complete family revocation.

### 5.2 Authorization
- Role-based authorization via ASP.NET Core Policies (`[Authorize(Policy = "OrganizerOnly")]`).
- Resource-based authorization to enforce record ownership (`organizer_id == CurrentUser.OrganizerId`).

### 5.3 Data Protection
- Hash passwords using **Argon2id** or **BCrypt** (cost factor $\ge 12$).
- Encrypt sensitive data (Bank accounts, National ID) at application layer using **AES-256-GCM** with KMS keys.
- Mask sensitive fields in logs and API outputs.

---

## 6. Error Handling & Validation

### 6.1 Validation Pipeline
- Run FluentValidation automatically inside MediatR `ValidationBehavior`.
- Keep API controllers free from imperative `if (!ModelState.IsValid)` checks.

### 6.2 Global Exception Handler
- Use ASP.NET Core 8 `IExceptionHandler`.
- Return RFC 7807 `ProblemDetails` consistently.
- Redact internal exception traces from production responses.

---

## 7. Logging & Observability

### 7.1 Structured Logging (Serilog)
- Use message templates with named properties:
```csharp
_logger.LogInformation("Order {BookingId} created for customer {UserId}", bookingId, userId);
```
- Propagate `X-Correlation-Id` across all requests, logs, and background jobs.

### 7.2 Health Checks
- `/health/live`: Process liveness check.
- `/health/ready`: Readiness check (DB connectivity, Redis ping, external dependencies).

---

## 8. Testing Strategy

- **Unit Tests:** xUnit + FluentAssertions + NSubstitute for Domain models, rules, and Command/Query handlers.
- **Integration Tests:** Testcontainers (PostgreSQL, Redis) + `WebApplicationFactory<Program>` to test end-to-end API flows.
- **Architecture Tests:** NetArchTest to enforce dependency boundaries between layers.
