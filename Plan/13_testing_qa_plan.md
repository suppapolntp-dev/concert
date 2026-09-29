# 🧪 Testing & QA Strategy Plan

> **Scope:** Unit, Integration, Architecture, End-to-End, Load & Security Testing  
> **Tools:** xUnit, FluentAssertions, NSubstitute, Testcontainers, NetArchTest, k6, Playwright  
> **Source Spec:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md) (Section 6.6)

---

## 1. Test Pyramid & Coverage Targets

```
                    ╱╲
                   ╱ E2E ╲               5%   (Playwright — Critical user flows)
                  ╱────────╲
                 ╱  Load    ╲           10%   (k6 — Concurrency stress tests)
                ╱────────────╲
               ╱ Integration  ╲         30%   (Testcontainers — API + DB + Redis)
              ╱────────────────╲
             ╱   Unit Tests     ╲       55%   (xUnit — Domain + Handlers)
            ╱────────────────────╲
```

| Layer | Tool | Target Coverage | Speed |
| :--- | :--- | :---: | :---: |
| **Unit Tests** | xUnit + FluentAssertions + NSubstitute | ≥ 80% (Domain + Application) | < 30s |
| **Integration Tests** | Testcontainers + WebApplicationFactory | ≥ 60% (API endpoints) | < 5 min |
| **Architecture Tests** | NetArchTest | 100% (Dependency rules) | < 5s |
| **Load Tests** | k6 | Key concurrency scenarios | 5-15 min |
| **E2E Tests** | Playwright | Top 5 critical paths | 3-5 min |
| **Security Scan** | CodeQL + Trivy + `npm audit` | Zero critical/high CVE | CI gate |

---

## 2. Unit Tests (`tests/Domain.UnitTests/` + `tests/Application.UnitTests/`)

### 2.1 What to Unit Test

| Target | Examples | Mocking Strategy |
| :--- | :--- | :--- |
| **Value Objects** | `Email`, `Money`, `DateRange`, `BookingReference` | No mocks needed (pure logic) |
| **Entity Domain Methods** | `Seat.Hold()`, `Seat.Release()`, `Booking.MarkConfirmed()` | No mocks needed |
| **Enum State Transitions** | Event: `Draft` → `PendingApproval` ✅, `Draft` → `OnSale` ❌ | No mocks needed |
| **Domain Services** | `PromoRedemptionService`, `TicketTransferService` | Mock dependencies via NSubstitute |
| **Command Handlers** | `CreateBookingHandler`, `HoldSeatHandler` | Mock `IRepository`, `IUnitOfWork` |
| **Query Handlers** | `GetEventBySlugHandler` | Mock `IRepository` |
| **Validators** | `CreateEventCommandValidator` | No mocks (FluentValidation) |

### 2.2 Sample Unit Tests

```csharp
// ── Value Object: Money ──────────────────
public class MoneyTests
{
    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var a = new Money(100m, Currency.THB);
        var b = new Money(50m, Currency.THB);

        var result = a.Add(b);

        result.Amount.Should().Be(150m);
        result.Currency.Should().Be(Currency.THB);
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsDomainException()
    {
        var thb = new Money(100m, Currency.THB);
        var usd = new Money(50m, Currency.USD);

        var act = () => thb.Add(usd);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot add*");
    }

    [Fact]
    public void Constructor_NegativeAmount_ThrowsDomainException()
    {
        var act = () => new Money(-1m, Currency.THB);

        act.Should().Throw<DomainException>();
    }
}

// ── Entity: Seat State Transitions ───────
public class SeatTests
{
    [Fact]
    public void Hold_WhenAvailable_SetsHeldStatus()
    {
        var seat = CreateAvailableSeat();
        var userId = Guid.NewGuid();

        seat.Hold(userId, TimeSpan.FromMinutes(10));

        seat.Status.Should().Be(SeatStatus.Held);
        seat.HeldByUserId.Should().Be(userId);
        seat.HeldUntil.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Hold_WhenAlreadyBooked_ThrowsDomainException()
    {
        var seat = CreateBookedSeat();

        var act = () => seat.Hold(Guid.NewGuid(), TimeSpan.FromMinutes(10));

        act.Should().Throw<DomainException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public void ConfirmBooking_WhenNotHeld_ThrowsDomainException()
    {
        var seat = CreateAvailableSeat();

        var act = () => seat.ConfirmBooking();

        act.Should().Throw<DomainException>()
            .WithMessage("*not held*");
    }
}

// ── Command Handler: HoldSeat ────────────
public class HoldSeatHandlerTests
{
    [Fact]
    public async Task Handle_ValidSeat_ReturnsHoldResult()
    {
        var seatRepo = Substitute.For<ISeatRepository>();
        var lockService = Substitute.For<IDistributedLockService>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var seat = CreateAvailableSeat();
        seatRepo.GetByIdAsync(seat.Id, Arg.Any<CancellationToken>()).Returns(seat);
        lockService.AcquireLockAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(new FakeAsyncDisposable());

        var handler = new HoldSeatHandler(seatRepo, lockService, unitOfWork);
        var command = new HoldSeatCommand(seat.Id, Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        seat.Status.Should().Be(SeatStatus.Held);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
```

---

## 3. Integration Tests (`tests/WebAPI.IntegrationTests/`)

### 3.1 Infrastructure: Testcontainers + WebApplicationFactory

```csharp
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("ticketworld_test")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace DB connection string
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseNpgsql(_postgres.GetConnectionString()));

            // Replace Redis connection
            services.RemoveAll<IConnectionMultiplexer>();
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(_redis.GetConnectionString()));
        });
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}
```

### 3.2 What to Integration Test

| Scenario | Validates |
| :--- | :--- |
| `POST /auth/register` → `POST /auth/login` | Full auth flow, password hashing, JWT issuance |
| `POST /seats/hold` → `POST /bookings/{id}/checkout` | Hold → Payment → Confirm lifecycle |
| `POST /seats/hold` (concurrent x 2 same seat) | Only 1 succeeds, other gets 409 Conflict |
| `POST /bookings/{id}/cancel` | Seat released back to Available |
| `GET /events?q=coldplay&city=Bangkok` | Search, filter, pagination correctness |
| `POST /organizer/events` → `POST /*/submit` → Admin approve | Event state machine Draft → Published |
| `POST /promo-codes/validate` (used_count at max) | Returns 422 PROMO_EXHAUSTED |
| EF Core Migrations | Database schema applies cleanly on empty DB |

---

## 4. Architecture Tests (`tests/Architecture.Tests/`)

```csharp
public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(BaseEntity<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IUnitOfWork).Assembly;
    private static readonly Assembly InfraAssembly = typeof(ApplicationDbContext).Assembly;
    private static readonly Assembly WebApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_Should_Not_Reference_Any_Other_Layer()
    {
        Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Application", "Infrastructure", "WebAPI")
            .GetResult()
            .IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure_Or_WebAPI()
    {
        Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Infrastructure", "WebAPI")
            .GetResult()
            .IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Controllers_Should_Not_Access_DbContext_Directly()
    {
        Types.InAssembly(WebApiAssembly)
            .That().HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn("Infrastructure.Persistence")
            .GetResult()
            .IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_Should_Be_Sealed()
    {
        Types.InAssembly(ApplicationAssembly)
            .That().HaveNameEndingWith("Handler")
            .Should().BeSealed()
            .GetResult()
            .IsSuccessful.Should().BeTrue();
    }
}
```

---

## 5. Load & Concurrency Tests (k6)

### 5.1 Scenario: Hot On-Sale Seat Hold Race

```javascript
// k6/seat-hold-stress.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    seat_rush: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '10s', target: 500 },   // Ramp to 500 users
        { duration: '30s', target: 500 },   // Sustain 500 concurrent
        { duration: '10s', target: 0 },     // Ramp down
      ],
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<500'],        // 95th percentile < 500ms
    http_req_failed: ['rate<0.30'],          // Allow up to 30% 409 Conflict (expected)
    'checks': ['rate>0.60'],                 // 60%+ should succeed (not all — seats are limited)
  },
};

export default function () {
  const seatId = `seat-${Math.floor(Math.random() * 100)}`;

  const res = http.post(
    `${__ENV.API_URL}/api/v1/seats/hold`,
    JSON.stringify({ seatIds: [seatId] }),
    {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${__ENV.TOKEN}`,
        'X-WaitingRoom-Token': __ENV.ROOM_TOKEN,
      },
    }
  );

  check(res, {
    'hold succeeded (200)': (r) => r.status === 200,
    'conflict expected (409)': (r) => r.status === 409,
    'not rate limited (429)': (r) => r.status !== 429,
  });

  sleep(0.1);
}
```

### 5.2 Key Load Test Scenarios

| Scenario | VUs | Duration | Success Criteria |
| :--- | :---: | :--- | :--- |
| **Seat Hold Race** | 500 concurrent | 30s sustained | p95 < 500ms, no data corruption |
| **Waiting Room Queue** | 5,000 enter | 60s ramp | Queue position correct, no duplicates |
| **Checkout Payment** | 100 concurrent | 30s | No double-charge, idempotency works |
| **Event Search** | 1,000 concurrent | 60s | p95 < 200ms (cached), p99 < 500ms |
| **SignalR Connections** | 2,000 concurrent | 120s | All receive seat updates within 1s |

---

## 6. E2E Tests (Playwright)

### 6.1 Critical Path Test Cases

| # | Flow | Steps |
| :---: | :--- | :--- |
| 1 | **Customer Buy Ticket** | Register → Login → Search Event → Select Seats → Checkout → View QR |
| 2 | **Organizer Publish Event** | Login → Create Event → Add Seat Map → Submit → Admin Approve |
| 3 | **Staff Check-in** | Login as Staff → Open Scanner → Scan valid QR → See green confirmation |
| 4 | **Ticket Transfer** | Login → My Tickets → Transfer → Recipient Login → Accept |
| 5 | **Admin Payout** | Login Admin → View Pending Payouts → Approve → Verify status |

---

## 7. CI/CD Pipeline Integration

```yaml
# .github/workflows/ci.yml
jobs:
  unit-tests:
    steps:
      - dotnet test tests/Domain.UnitTests
      - dotnet test tests/Application.UnitTests

  architecture-tests:
    steps:
      - dotnet test tests/Architecture.Tests

  integration-tests:
    services: [docker]    # Required for Testcontainers
    steps:
      - dotnet test tests/WebAPI.IntegrationTests

  security-scan:
    steps:
      - dotnet list package --vulnerable --include-transitive
      - npm audit --production
      - trivy fs --scanners vuln,secret .

  load-tests:
    needs: [integration-tests]
    if: github.ref == 'refs/heads/main'     # Only on main branch
    steps:
      - k6 run k6/seat-hold-stress.js

  frontend-tests:
    steps:
      - npm run lint
      - npm run test -- --watch=false
      - npx playwright test                  # E2E (only on main)
```

### CI Gate Rules
- ❌ **Block merge** ถ้า Unit/Integration/Architecture test fail
- ❌ **Block merge** ถ้า Security scan พบ Critical/High CVE
- ⚠️ **Warn** ถ้า Load test p95 เกิน threshold (ไม่ block แต่แจ้ง)
- ✅ **E2E** รันเฉพาะ main branch merge (ไม่ block PR)
