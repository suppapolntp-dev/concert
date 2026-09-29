# Summary

This unit report delivers an exhaustive security, compliance, and infrastructure audit for **Track 3 (security_compliance_and_infrastructure)** across the TicketWorld platform specification suite, specifically benchmarking:
- `Plan/03_docker_infra_plan.md` (Container secrets, environment configuration, port security)
- `Plan/04_backend_guidelines.md` (Middleware pipeline, validation, exception handling, seeding)
- `Plan/07_api_endpoint_plan.md` (Auth policies, token endpoints, rate limits, error formats)
- `Plan/12_security_compliance_plan.md` (Thailand PDPA, PCI-DSS SAQ A, KMS Key Management, Incident Response)
- `Plan/13_testing_qa_plan.md` (Security and compliance automated test suites)

Cross-plan consistency checks were performed against `Plan/01_concert_requirement_spec.md`, `Plan/06_entity_db_plan.md`, `Plan/09_payment_reconciliation_plan.md`, and `Plan/11_frontend_architecture_plan.md`.

### Core Benchmark Outcomes:
1. **RFC 7807 (Problem Details) Compliance:**
   - **Severe Schema Split:** `Plan/01` defines a wrapped envelope (`{ success: false, error: { code, message, details } }`) mislabeled as RFC 7807, while `Plan/04` specifies flat top-level RFC 7807 properties (`type, title, status, detail, instance, traceId`), and `Plan/07` completely omits error formats and status code mappings.
   - **Media Type & Validation Gaps:** Missing `Content-Type: application/problem+json` header contract and missing `HttpValidationProblemDetails` schema for 400/422 validation errors.
   - **Information Disclosure Risk:** Missing concrete exception handling implementation in `Plan/04` to sanitize internal database errors (`NpgsqlException`, raw SQL, connection strings, stack traces) in non-development environments.
2. **OWASP API Security Top 10 (2023) & NIST SP 800-63B:**
   - **Algorithm Confusion & Symmetric Key Contradiction (API2:2023):** `Plan/03` defines a symmetric `JWT_SECRET`, directly contradicting the asymmetric RS256 + KMS + JWKS mandate in `Plan/01`, `Plan/04`, and `Plan/12`.
   - **Default Admin Credential in Source Code:** `Plan/04` hardcodes a plaintext password (`SecureAdminP@ss2026!`) in master database seed code.
   - **BOLA / IDOR (API1:2023):** Missing resource ownership authorization enforcement mechanisms for orders, tickets, QR codes, and organizer events in `Plan/04` and `Plan/07`.
   - **BOPLA / Mass Assignment (API3:2023):** Missing strict DTO property whitelisting on `PUT /auth/me` and `PUT /organizer/events/{id}`.
   - **NIST SP 800-63B Deficiencies:** Missing per-account brute-force lockout rules in `Plan/07` and `Plan/12` (despite being specified in `Plan/01`), missing MFA backup recovery codes, and undefined Step-Up Authentication flow (`amr: mfa`).
3. **Financial & Privacy Compliance (PCI-DSS SAQ A & Thailand PDPA):**
   - **PCI-DSS SAQ A Ingestion Violation:** `Plan/12` claims backend filters out `card.*` from raw payloads; handling card data on the backend breaches SAQ A scope entirely. Omise hosted fields are omitted. Prohibiting `last4` storage breaks legitimate receipt generation.
   - **PDPA Contract Discrepancy:** `Plan/12` promises `POST /auth/me/export` and `PUT /auth/me/consent`, but neither endpoint exists in `Plan/07`, and `Consent` entities are missing from `Plan/06`.
   - **Crypto-Shredding Protocol Missing:** Neither plan formally defines crypto-shredding (per-subject DEK destruction) to reconcile PDPA erasure with Thai accounting retention mandates (5–7 years).
4. **Infrastructure & Container Security (Docker Compose):**
   - **Unrestricted Port Bindings (`0.0.0.0`):** `Plan/03` maps PostgreSQL, Redis, MinIO, Seq, and MailDev to `0.0.0.0`, exposing internal data tiers to external network interfaces.
   - **Secret Leak via Redis CLI:** Redis healthcheck exposes the password in the container process table via `redis-cli -a`.
5. **QA & Automated Security Testing (Plan 13):**
   - **Zero Functional Security Tests:** `Plan/13` contains static vulnerability scans but lacks automated integration suites for BOLA, Broken Auth, Mass Assignment, Rate Limiting, RFC 7807 compliance, and PDPA anonymization.

---

# Potential Mistakes and Improvements

### 1. [CRITICAL] Error Envelope Specification Conflict & RFC 7807 Non-Compliance
- **File & Section:** `Plan/01_concert_requirement_spec.md` (§3.1, lines 657–669), `Plan/04_backend_guidelines.md` (§3.2, lines 91–100), `Plan/07_api_endpoint_plan.md` (§1.1)
- **Defect Description:**
  There is a direct architectural conflict between `Plan/01` and `Plan/04`:
  - `Plan/01` defines an error envelope wrapped under `{ "success": false, "error": { ... } }` and mislabels it as RFC 7807.
  - `Plan/04` defines standard top-level RFC 7807 members (`type`, `title`, `status`, `detail`, `instance`, `traceId`).
  - `Plan/07` completely omits error response envelope contracts and status code mappings.
  Under RFC 7807 (and RFC 9457), Problem Details members MUST be top-level JSON properties and returned with `Content-Type: application/problem+json`. Wrapping them under a proprietary `"error"` key breaks client libraries expecting RFC 7807.
- **Diff / Fix Recommendation:**
  Standardize on RFC 7807 top-level properties across all plans and document the `application/problem+json` header.

```diff
--- a/Plan/01_concert_requirement_spec.md
+++ b/Plan/01_concert_requirement_spec.md
@@ -657,13 +657,12 @@
 #### Error (4xx/5xx — RFC 7807 ProblemDetails):
 ```json
 {
-  "success": false,
-  "error": {
-    "code": "SEAT_ALREADY_HELD",
-    "message": "Selected seat(s) are currently held by another user.",
-    "details": [{ "field": "seatIds", "issue": "Seat VIP-A-15 is locked until 12:35:00Z" }],
-    "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
-  },
-  "timestamp": "2026-09-12T12:30:00Z"
+  "type": "https://ticketworld.io/errors/seat-already-held",
+  "title": "Seat Unavailable",
+  "status": 409,
+  "detail": "Selected seat(s) are currently held by another user.",
+  "instance": "/api/v1/seats/hold",
+  "code": "SEAT_ALREADY_HELD",
+  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00",
+  "timestamp": "2026-09-12T12:30:00Z"
 }
 ```
```

---

### 2. [CRITICAL] Missing Production Exception Sanitization & DB Error Leak in Middleware
- **File & Section:** `Plan/04_backend_guidelines.md` (§6.2, lines 282–286)
- **Defect Description:**
  `Plan/04` states "Redact internal exception traces from production responses", but provides no concrete ASP.NET Core 8 `IExceptionHandler` implementation. In high-concurrency systems, unhandled database exceptions (`NpgsqlException`, `PostgresException`) or ORM state errors expose internal PostgreSQL schema names, constraint names, hostnames, and stack traces to clients if not caught and transformed. This violates OWASP API8:2023 (Security Misconfiguration) and CWE-209.
- **Diff / Fix Recommendation:**
  Implement a production-hardened `GlobalExceptionHandler` that maps domain exceptions to appropriate 4xx status codes and redacts 500 server errors into a safe RFC 7807 envelope while logging the full exception with `TraceId` to Serilog.

```diff
--- a/Plan/04_backend_guidelines.md
+++ b/Plan/04_backend_guidelines.md
@@ -282,6 +282,53 @@
 ### 6.2 Global Exception Handler
 - Use ASP.NET Core 8 `IExceptionHandler`.
 - Return RFC 7807 `ProblemDetails` consistently.
-- Redact internal exception traces from production responses.
+
+```csharp
+public class GlobalExceptionHandler : IExceptionHandler
+{
+    private readonly ILogger<GlobalExceptionHandler> _logger;
+    private readonly IHostEnvironment _env;
+
+    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
+    {
+        _logger = logger;
+        _env = env;
+    }
+
+    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
+    {
+        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
+        _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);
+
+        var (status, type, title, detail) = exception switch
+        {
+            ValidationException valEx => (StatusCodes.Status422UnprocessableEntity, "https://ticketworld.io/errors/validation-error", "Validation Failed", "One or more validation errors occurred."),
+            SeatLockedException => (StatusCodes.Status409Conflict, "https://ticketworld.io/errors/seat-unavailable", "Seat Unavailable", exception.Message),
+            ResourceNotFoundException => (StatusCodes.Status404NotFound, "https://ticketworld.io/errors/not-found", "Resource Not Found", exception.Message),
+            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "https://ticketworld.io/errors/forbidden", "Forbidden", "Access is denied."),
+            _ => (StatusCodes.Status500InternalServerError, "https://ticketworld.io/errors/internal-server-error", "Internal Server Error", _env.IsDevelopment() ? exception.Message : "An unexpected internal server error occurred. Quote TraceId when contacting support.")
+        };
+
+        httpContext.Response.StatusCode = status;
+        httpContext.Response.ContentType = "application/problem+json";
+
+        var problem = new ProblemDetails
+        {
+            Type = type,
+            Title = title,
+            Status = status,
+            Detail = detail,
+            Instance = httpContext.Request.Path
+        };
+        problem.Extensions["traceId"] = traceId;
+        problem.Extensions["timestamp"] = DateTime.UtcNow;
+        if (exception is ValidationException vEx)
+        {
+            problem.Extensions["errors"] = vEx.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
+        }
+
+        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
+        return true;
+    }
+}
+```
```

---

### 3. [CRITICAL] Architectural Inconsistency: Symmetric `JWT_SECRET` vs Asymmetric RS256 with KMS/JWKS
- **File & Section:** `Plan/03_docker_infra_plan.md` (§1.1, line 32) vs `Plan/04_backend_guidelines.md` (§5.1, line 261), `Plan/12_security_compliance_plan.md` (§1.1, line 15), `Plan/01_concert_requirement_spec.md` (§1, line 117)
- **Defect Description:**
  `Plan/01` (line 117) explicitly commands: *"JWT Signing: RS256 (asymmetric) + เผย JWKS endpoint สำหรับ key rotation; ห้ามใช้ HS256 กัน algorithm confusion"*. `Plan/04` and `Plan/12` reiterate RS256 with KMS.
  However, `Plan/03_docker_infra_plan.md` defines `JWT_SECRET: Min64CharactersLongSecretKeyForJwtSigning2026TicketWorldPlatform`.
  This creates a severe design flaw: developers will configure ASP.NET Core JWT authentication with `SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))` instead of loading RSA credentials. This exposes the system to JWT Algorithm Confusion attacks (OWASP API2:2023 / CVE-2015-9235), where an attacker signs a token using the server's public key as an HMAC secret, and renders the required JWKS endpoint (`/.well-known/jwks.json`) defunct.
- **Diff / Fix Recommendation:**
  Deprecate `JWT_SECRET` in `Plan/03`. Specify RSA private/public key parameters or KMS Key ARN. Add the JWKS endpoint to `Plan/07_api_endpoint_plan.md`.

```diff
--- a/Plan/03_docker_infra_plan.md
+++ b/Plan/03_docker_infra_plan.md
@@ -32,3 +32,4 @@
-| `JWT_SECRET` | Secret key for signing access tokens | `Min64CharactersLongSecretKeyForJwtSigning2026TicketWorldPlatform` |
+| `JWT_PRIVATE_KEY_PATH` | Path to RSA private key PEM for token signing (RS256) | `/run/secrets/jwt_private_key.pem` |
+| `JWT_PUBLIC_KEY_PATH` | Path to RSA public key PEM for token verification & JWKS | `/run/secrets/jwt_public_key.pem` |
+| `JWT_KEY_ID` | Key ID (kid) for JWKS header matching & zero-downtime rotation | `tw-key-2026-v1` |
```

```diff
--- a/Plan/07_api_endpoint_plan.md
+++ b/Plan/07_api_endpoint_plan.md
@@ -69,2 +69,3 @@
+| 18 | `GET` | `/.well-known/jwks.json` | Public | — | เผยแพร่ Public Keys (JWKS) สำหรับตรวจสอบ RS256 JWT Signature |
```

---

### 4. [CRITICAL] Hardcoded Admin Seed Password in Source Code
- **File & Section:** `Plan/04_backend_guidelines.md` (§4.5, lines 185–193)
- **Defect Description:**
  In `ApplicationDbContextInitializer.SeedAsync()`, line 188 hardcodes:
  `PasswordHash = _passwordHasher.Hash("SecureAdminP@ss2026!")`.
  The guidelines label this section as *"Master / System Data (All Environments including Prod)"*.
  Embedding default administrative passwords in code violates OWASP API2:2023, CWE-259, and PCI-DSS Requirement 2.1 (*"Never use vendor-supplied or default passwords"*). If this initializer runs in production or staging, the administrator account is pre-compromised.
- **Diff / Fix Recommendation:**
  Refactor the seeder to read `ADMIN_INITIAL_PASSWORD` from environment variables / secret store, mandate a forced password change upon first login (`MustChangePassword = true`), and strictly forbid default passwords in non-development environments.

```diff
--- a/Plan/04_backend_guidelines.md
+++ b/Plan/04_backend_guidelines.md
@@ -183,7 +183,12 @@
         if (!await _context.Users.AnyAsync(u => u.Role == "Admin", cancellationToken))
         {
+            var initialAdminPassword = Environment.GetEnvironmentVariable("ADMIN_INITIAL_PASSWORD")
+                ?? throw new InvalidOperationException("ADMIN_INITIAL_PASSWORD must be configured before bootstrapping admin account.");
+
             _context.Users.Add(new User
             {
                 Email = "admin@ticketworld.io",
-                PasswordHash = _passwordHasher.Hash("SecureAdminP@ss2026!"),
+                PasswordHash = _passwordHasher.Hash(initialAdminPassword),
                 Role = "Admin",
                 IsVerified = true,
+                MustChangePassword = true
             });
             await _context.SaveChangesAsync(cancellationToken);
         }
```

---

### 5. [CRITICAL] Insecure Host Port Bindings (`0.0.0.0`) Exposing Data Tier to Network
- **File & Section:** `Plan/03_docker_infra_plan.md` (§3, lines 102, 121, 142–143, 163–164, 176–177)
- **Defect Description:**
  Docker Compose maps auxiliary service ports using generic strings (`"${POSTGRES_PORT:-5432}:5432"`, `"${REDIS_PORT:-6379}:6379"`, etc.). By default, Docker binds published ports to `0.0.0.0`, exposing PostgreSQL, Redis, MinIO S3/Console, Seq Ingestion/UI, and MailDev to all external network interfaces on the host machine.
  Because `Plan/03` explicitly targets *"Local Development & Staging Multi-Container Environment"*, deploying this file on a staging server or public cloud VM allows external actors to directly attack PostgreSQL (port 5432) and Redis (port 6379), bypassing reverse proxies and host UFW firewalls.
- **Diff / Fix Recommendation:**
  Explicitly bind all exposed container ports to `127.0.0.1`. In staging multi-container configurations, eliminate host port mappings entirely for internal services and route communication over `ticketworld-network`.

```diff
--- a/Plan/03_docker_infra_plan.md
+++ b/Plan/03_docker_infra_plan.md
@@ -101,3 +101,3 @@
     ports:
-      - "${POSTGRES_PORT:-5432}:5432"
+      - "127.0.0.1:${POSTGRES_PORT:-5432}:5432"
@@ -120,3 +120,3 @@
     ports:
-      - "${REDIS_PORT:-6379}:6379"
+      - "127.0.0.1:${REDIS_PORT:-6379}:6379"
@@ -141,4 +141,4 @@
     ports:
-      - "${MINIO_API_PORT:-9000}:9000"
-      - "${MINIO_CONSOLE_PORT:-9001}:9001"
+      - "127.0.0.1:${MINIO_API_PORT:-9000}:9000"
+      - "127.0.0.1:${MINIO_CONSOLE_PORT:-9001}:9001"
@@ -162,4 +162,4 @@
     ports:
-      - "${SEQ_INGESTION_PORT:-5341}:5341"
-      - "${SEQ_UI_PORT:-8081}:80"
+      - "127.0.0.1:${SEQ_INGESTION_PORT:-5341}:5341"
+      - "127.0.0.1:${SEQ_UI_PORT:-8081}:80"
@@ -175,4 +175,4 @@
     ports:
-      - "${MAILDEV_WEB_PORT:-1080}:1080"
-      - "${MAILDEV_SMTP_PORT:-1025}:1025"
+      - "127.0.0.1:${MAILDEV_WEB_PORT:-1080}:1080"
+      - "127.0.0.1:${MAILDEV_SMTP_PORT:-1025}:1025"
```

---

### 6. [CRITICAL] PCI-DSS SAQ A Scope Invalidation via Ingestion of Raw Card Data
- **File & Section:** `Plan/12_security_compliance_plan.md` (§3, line 84), `Plan/01_concert_requirement_spec.md` (§7.4, line 1211)
- **Defect Description:**
  `Plan/12` states:
  `Payload Sanitization: payments.raw_response ผ่าน whitelist filter — ตัด card.*, last4, network ก่อนบันทึก`.
  This statement exposes a critical compliance misconception:
  1. Under PCI-DSS v4.0 SAQ A, the merchant’s backend servers MUST NEVER ingest, process, or transmit raw cardholder data (`card.*`). If card data touches the backend application layer before being "sanitized", the system breaches SAQ A criteria and is automatically forced into full **SAQ D** scope (requiring extensive hardware audits, quarterly ASV network scans, and full CDE isolation).
  2. Stripping `last4` and `network` (card brand) is counter-productive and misinformed: PCI-DSS explicitly permits storing masked PAN (last 4 digits) for customer receipts and dispute reconciliation. What is strictly forbidden from storage is SAD (CVV/CVC, CAV2, CID, full magnetic stripe data).
  3. `Plan/12` omits Omise hosted tokenization (Omise.js / Card Tokens), which is the primary payment gateway for Thailand (PromptPay + Credit Card).
- **Diff / Fix Recommendation:**
  Enforce strict SAQ A tokenization where only PSP tokens (`tok_...` / `pm_...`) reach the backend. Formally whitelist `last4`, `brand`, and `exp_month`/`exp_year` while banning CVV from all layers.

```diff
--- a/Plan/12_security_compliance_plan.md
+++ b/Plan/12_security_compliance_plan.md
@@ -81,5 +81,6 @@
 | ข้อกำหนด | การปฏิบัติ |
 | :--- | :--- |
-| **SAQ A เท่านั้น** | ไม่มี Card Data แตะ Server — ใช้ Stripe Elements / Hosted Fields บน Frontend |
+| **SAQ A เท่านั้น** | ไม่มี Card Data แตะ Server ทุกกรณี — ใช้ Stripe Elements & Omise.js Hosted Fields บน Frontend โดยตรง Backend รับเฉพาะ Payment Method Token (`pm_*` / `tok_*`) |
 | **3DS / SCA** | เปิดทุก Transaction — บันทึกผลใน `payments.three_ds_status` |
-| **Payload Sanitization** | `payments.raw_response` ผ่าน whitelist filter — ตัด `card.*`, last4, network ก่อนบันทึก |
+| **Zero SAD & Sanitization** | ห้ามรับหรือบันทึก Sensitive Authentication Data (CVV/CVC) ทุกกรณี; `payments.raw_response` อนุญาตเก็บเฉพาะ non-sensitive metadata (`last4`, `brand`, `exp_month`, `gateway_tx_id`) เพื่อการออกใบเสร็จตาม PCI-DSS Req 3.4.1 |
+| **PSP Integration** | รองรับทั้ง Stripe (Global) และ Omise (Thailand Credit Card & PromptPay) ภายใต้เกณฑ์ SAQ A Hosted Tokenization |
```

---

### 7. [HIGH] OWASP API1:2023 BOLA / IDOR on Orders, Tickets, and Organizer Resources
- **File & Section:** `Plan/07_api_endpoint_plan.md` (§4, §11), `Plan/04_backend_guidelines.md` (§5.2)
- **Defect Description:**
  In `Plan/07`, sensitive resource endpoints are tagged with access requirements like `Customer (Owner)` or `Organizer (Owner)` (e.g. `GET /bookings/{id}`, `GET /my-tickets/{id}/qr`, `POST /my-tickets/{id}/transfer`, `PUT /organizer/events/{id}`, `GET /organizer/events/{id}/orders`).
  However, `Plan/04` does not specify a concrete runtime authorization mechanism. Standard CQRS command/query handlers frequently query `_dbContext.Bookings.FindAsync(request.Id)` without asserting that `booking.UserId == _currentUser.UserId`. This leaves endpoints vulnerable to Broken Object Level Authorization (BOLA / IDOR), allowing authenticated customers to access or transfer other customers' tickets by guessing or enumerating IDs.
- **Diff / Fix Recommendation:**
  Define an ASP.NET Core Resource-Based Authorization Requirement (`IAuthorizationService`) and mandate repository query filtering by current tenant/owner.

```diff
--- a/Plan/04_backend_guidelines.md
+++ b/Plan/04_backend_guidelines.md
@@ -267,3 +267,17 @@
 - Role-based authorization via ASP.NET Core Policies (`[Authorize(Policy = "OrganizerOnly")]`).
 - Resource-based authorization to enforce record ownership (`organizer_id == CurrentUser.OrganizerId`).
+
+#### Resource Ownership Enforcement Pattern (BOLA Defense)
+Every handler manipulating user-owned aggregates must enforce ownership either via EF Core query filtering or `IAuthorizationService`:
+```csharp
+public async Task<BookingDto> Handle(GetBookingByIdQuery request, CancellationToken ct)
+{
+    var booking = await _dbContext.Bookings
+        .Include(b => b.Items)
+        .FirstOrDefaultAsync(b => b.Id == request.BookingId && (b.UserId == _currentUser.UserId || _currentUser.IsAdmin), ct);
+
+    if (booking == null)
+        throw new ResourceNotFoundException("Booking", request.BookingId); // Return 404 to avoid ID enumeration
+    return _mapper.Map<BookingDto>(booking);
+}
+```
```

---

### 8. [HIGH] OWASP API3:2023 BOPLA / Mass Assignment on User & Event DTOs
- **File & Section:** `Plan/07_api_endpoint_plan.md` (§2 Endpoint 11 `PUT /auth/me`, §11 Endpoint 4 `PUT /organizer/events/{id}`)
- **Defect Description:**
  Neither `Plan/04` nor `Plan/07` defines strict request DTO property whitelisting. In endpoints such as `PUT /auth/me` and `PUT /organizer/events/{id}`, binding JSON payloads directly to domain entities or using unconstrained AutoMapper/Mapster profiles allows attackers to overwrite protected properties:
  - On `PUT /auth/me`: Injecting `"role": "Admin"`, `"is_verified": true`, `"is_banned": false`, or `"two_factor_enabled": false`.
  - On `PUT /organizer/events/{id}`: Overwriting `"status": "Published"` or `"status": "OnSale"`, bypassing the admin moderation and approval workflow.
- **Diff / Fix Recommendation:**
  Document strict request DTO contracts in `Plan/07` that forbid binding system state properties.

```diff
--- a/Plan/07_api_endpoint_plan.md
+++ b/Plan/07_api_endpoint_plan.md
@@ -53,3 +53,9 @@
 | 11 | `PUT` | `/auth/me` | Authenticated | — | แก้ไข Profile (ชื่อ, โทร, ภาษา, สกุลเงิน) |
+
+> **Security Note (BOPLA / Mass Assignment Defense):**
+> Request DTO `UpdateUserProfileRequest` strictly allows ONLY `FullName`, `PhoneNumber`, `PreferredLanguage`, and `PreferredCurrency`.
+> Mapping directly to `User.Role`, `User.IsVerified`, `User.IsBanned`, `User.TwoFactorEnabled` is strictly rejected by FluentValidation rule:
+> `.Must(dto => dto.Role == null).WithMessage("Mutating security claims via profile endpoint is forbidden.")`.
```

---

### 9. [HIGH] Thailand PDPA Cross-Plan Contract Discrepancy (Missing Endpoints & Entities)
- **File & Section:** `Plan/12_security_compliance_plan.md` (§2.2, lines 46, 49), `Plan/07_api_endpoint_plan.md` (§2, lines 38–54), `Plan/06_entity_db_plan.md` (§2.5)
- **Defect Description:**
  `Plan/12` explicitly defines:
  - **Right to Portability**: `POST /api/v1/auth/me/export` (Generates JSON export, sends download link valid for 24h).
  - **Withdraw Consent**: `PUT /api/v1/auth/me/consent` (Revokes marketing consent).
  However, in `Plan/07_api_endpoint_plan.md`, these endpoints are completely missing from the API catalog (the total remains 78 instead of 80).
  Furthermore, `Plan/06_entity_db_plan.md` has no `UserConsent` entity or schema to record consent history (timestamp, version, opt-in/opt-out status), violating Thailand PDPA Section 19 (which requires data controllers to maintain verifiable evidence of consent and withdrawal).
- **Diff / Fix Recommendation:**
  Add the two missing PDPA endpoints to `Plan/07` and define the `UserConsent` entity in `Plan/06`.

```diff
--- a/Plan/07_api_endpoint_plan.md
+++ b/Plan/07_api_endpoint_plan.md
@@ -53,3 +53,5 @@
 | 11 | `PUT` | `/auth/me` | Authenticated | — | แก้ไข Profile (ชื่อ, โทร, ภาษา, สกุลเงิน) |
 | 12 | `DELETE` | `/auth/me` | Authenticated | — | ขอลบบัญชี (PDPA Anonymize) |
+| 13 | `POST` | `/auth/me/export` | Authenticated | ✅ | ขอ Export ข้อมูลส่วนบุคคล (PDPA Data Portability) → ส่งลิงก์ทางอีเมล |
+| 14 | `PUT` | `/auth/me/consent` | Authenticated | — | ปรับปรุง / ถอนความยินยอม PDPA (Marketing Consent Opt-in/Opt-out) |
@@ -221,4 +223,4 @@
 | Module | Count |
 | :--- | :---: |
-| Auth & Identity | 17 |
+| Auth & Identity | 19 |
...
-| **Total** | **78** |
+| **Total** | **80** |
```

```diff
--- a/Plan/06_entity_db_plan.md
+++ b/Plan/06_entity_db_plan.md
@@ -87,2 +87,3 @@
 | `AuditLog` | `BaseEntity<Guid>` | `audit_logs` | `ActorId`, `Action`, `EntityName`, `EntityId`, `OldValuesJson`, `NewValuesJson`, `IpAddress` |
+| `UserConsent` | `BaseEntity<Guid>`, `IAuditableEntity` | `user_consents` | `UserId`, `ConsentType` (`Marketing`, `ThirdPartyAnalytics`), `IsGranted`, `ConsentVersion`, `IpAddress`, `GrantedAt`, `RevokedAt` |
```

---

### 10. [HIGH] NIST SP 800-63B Discrepancies: Missing Account Lockout & Password Rules
- **File & Section:** `Plan/07_api_endpoint_plan.md` (§1.2, line 30), `Plan/12_security_compliance_plan.md`, `Plan/06_entity_db_plan.md` (§2.1)
- **Defect Description:**
  1. `Plan/01` (line 120) mandates: *"Account Lockout: 5 failed attempts per 15 min -> 15 min lock (exponential backoff) + email alert; per-account not per-IP"*. It also specifies `failed_login_count` and `locked_until` in table `users` (lines 147–148).
  2. However, `Plan/07` only specifies rate limiting per IP (`10 req / 5 min Per IP`), which fails to defend against distributed credential stuffing attacks using diverse IP pools.
  3. `Plan/12` completely omits NIST SP 800-63B password rules (minimum 8 characters, maximum >= 64 characters, check against compromised password dictionaries / HaveIBeenPwned API, no arbitrary composition rules, no periodic mandatory rotation).
  4. `Plan/06_entity_db_plan.md` omitted `failed_login_count` and `locked_until` from the `User` entity, making it impossible to enforce persistent account lockout.
- **Diff / Fix Recommendation:**
  Add lockout fields to `User` in `Plan/06`, define per-account throttling in `Plan/07`, and formalize NIST SP 800-63B requirements in `Plan/12`.

```diff
--- a/Plan/06_entity_db_plan.md
+++ b/Plan/06_entity_db_plan.md
@@ -51,2 +51,2 @@
-| `User` | `AggregateRoot<Guid>`, `IAuditableEntity`, `ISoftDeletable` | `users` | `Email`, `PasswordHash`, `FullName`, `Role` (Enum), `TwoFactorEnabled`, `IsBanned` |
+| `User` | `AggregateRoot<Guid>`, `IAuditableEntity`, `ISoftDeletable` | `users` | `Email`, `PasswordHash`, `FullName`, `Role` (Enum), `TwoFactorEnabled`, `IsBanned`, `FailedLoginCount` (INT), `LockedUntil` (TIMESTAMPTZ) |
```

```diff
--- a/Plan/07_api_endpoint_plan.md
+++ b/Plan/07_api_endpoint_plan.md
@@ -30,2 +30,3 @@
 | Auth Login (`POST /auth/login`) | 10 req | 5 min | Per IP |
+| Auth Login (Account Lockout) | 5 failed attempts | 15 min | Per Email / Account (NIST SP 800-63B Lockout: 15 min lock + email alert) |
```

---

### 11. [HIGH] Missing MFA Recovery Codes & Undefined Step-Up Auth Flow (NIST AAL2)
- **File & Section:** `Plan/07_api_endpoint_plan.md` (§2 2FA Sub-module, §12 Platform Admin), `Plan/12_security_compliance_plan.md`
- **Defect Description:**
  1. `Plan/07` defines TOTP endpoints (`/auth/2fa/setup`, `/auth/2fa/verify`, `/auth/2fa/disable`), but omits one-time backup recovery codes. Under NIST SP 800-63B §5.1.3, authenticator loss without recovery codes creates permanent account lockout.
  2. In `Plan/07` Module 12, critical admin endpoints (`/admin/users/{id}/ban`, `/admin/users/{id}/role`, `/admin/payouts/{id}/process`, `/admin/config/{key}`) are marked `Admin (Step-up)`. However, there is no technical definition of how step-up is executed or validated. `Plan/01` (line 119) indicates an `amr: mfa` JWT claim valid for <= 5 minutes, but `Plan/07` lacks an endpoint to obtain this claim or pass the step-up challenge.
- **Diff / Fix Recommendation:**
  Add a dedicated recovery code endpoint and formalize the Step-Up challenge/exchange endpoint.

```diff
--- a/Plan/07_api_endpoint_plan.md
+++ b/Plan/07_api_endpoint_plan.md
@@ -62,3 +62,5 @@
 | 15 | `POST` | `/auth/2fa/disable` | Authenticated + Password | ปิด 2FA |
+| 16 | `POST` | `/auth/2fa/recovery-codes` | Authenticated + Password | สร้างชุด Recovery Codes 10 รหัส (แฮชเก็บใน DB) สำหรับกู้คืนบัญชี |
+| 17 | `POST` | `/auth/step-up` | Authenticated + OTP | ทำ Step-up Auth → ออก Step-up Token / Claim `amr: mfa` อายุ 5 นาที |
```

---

### 12. [HIGH] Secret Leakage in Container Process Table via Redis CLI
- **File & Section:** `Plan/03_docker_infra_plan.md` (§3, line 127)
- **Defect Description:**
  The Redis container healthcheck is declared as:
  `test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]`.
  Passing passwords as command-line arguments exposes the plaintext secret in `/proc/[pid]/cmdline` and `ps aux` to any user or script on the container host. Redis issues an explicit security warning on stderr when `-a` is used in CLI arguments.
- **Diff / Fix Recommendation:**
  Utilize the `REDISCLI_AUTH` environment variable inside a shell command test.

```diff
--- a/Plan/03_docker_infra_plan.md
+++ b/Plan/03_docker_infra_plan.md
@@ -126,3 +126,3 @@
     healthcheck:
-      test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]
+      test: ["CMD-SHELL", "REDISCLI_AUTH=\"$REDIS_PASSWORD\" redis-cli ping | grep -q PONG"]
       interval: 10s
```

---

### 13. [HIGH] Complete Absence of Automated Security & Compliance Test Suites
- **File & Section:** `Plan/13_testing_qa_plan.md` (§1, §3, §7)
- **Defect Description:**
  `Plan/13` contains extensive load tests (k6), unit tests, and architecture tests, but has **zero** dedicated functional security test suites. The CI pipeline only runs static scanners (`dotnet list package --vulnerable`, `npm audit`, `trivy`).
  Critical vulnerability regressions—specifically BOLA/IDOR on bookings, Refresh Token family revocation on reuse, BOPLA / Mass Assignment on profiles, RFC 7807 problem details conformance, and PDPA anonymization integrity—are completely untested in the automated pipeline.
- **Diff / Fix Recommendation:**
  Introduce Section 3.3 in `Plan/13_testing_qa_plan.md` detailing automated security integration tests (`tests/WebAPI.SecurityTests/`).

```diff
--- a/Plan/13_testing_qa_plan.md
+++ b/Plan/13_testing_qa_plan.md
@@ -214,3 +214,40 @@
+
+### 3.3 Security & Compliance Test Suite (`tests/WebAPI.SecurityTests/`)
+
+Dedicated integration tests asserting OWASP API Top 10 and statutory compliance:
+
+| Test Category | Test Name | Expected Behavior |
+| :--- | :--- | :--- |
+| **BOLA / IDOR** | `GetBooking_OtherUserToken_Returns404NotFound` | User B requesting User A's `/api/v1/bookings/{id}` receives 404 to avoid ID enumeration. |
+| **BOLA / IDOR** | `DownloadTicketQr_NonOwner_Returns403Forbidden` | Unauthorized caller requesting `/my-tickets/{id}/qr` receives 403 Forbidden. |
+| **Broken Auth** | `RefreshToken_ReuseDetected_RevokesEntireTokenFamily` | Attempting to reuse a previously rotated refresh token revokes all user sessions and logs `SECURITY.TokenReuse`. |
+| **BOPLA** | `UpdateProfile_InjectAdminRole_RoleRemainsUnchanged` | Supplying `{ "role": "Admin" }` to `PUT /api/v1/auth/me` is rejected with 422 or ignored. |
+| **BFLA** | `AdminEndpoint_CustomerToken_Returns403Forbidden` | Calling `/api/v1/admin/users` with Customer JWT yields 403 Forbidden. |
+| **RFC 7807** | `UnhandledException_NonDevMode_DoesNotLeakStackTraceOrDbQuery` | Internal database failure returns `application/problem+json` with generic 500 error and no SQL fragments. |
+| **PDPA** | `DeleteAccount_SanitizesPii_KeepsFinancialLedgerIntegrity` | `DELETE /api/v1/auth/me` overwrites email with `anon+uuid`, sets phone to NULL, and leaves booking FK intact. |
+| **PCI-DSS** | `CheckoutPayload_CardDetailsSentDirectlyToApi_Returns400` | Any direct submission of PAN/CVV to WebAPI endpoints is blocked by middleware filter. |
+
+```csharp
+public class BolaSecurityTests : IClassFixture<IntegrationTestWebAppFactory>
+{
+    private readonly HttpClient _client;
+    public BolaSecurityTests(IntegrationTestWebAppFactory factory) => _client = factory.CreateClient();
+
+    [Fact]
+    public async Task GetBooking_BelongingToAnotherUser_ReturnsNotFound()
+    {
+        var userAToken = await TestAuthHelper.GetJwtForUserAsync(_client, "userA@ticketworld.io");
+        var userBBooking = await TestDataHelper.CreateBookingForUserAsync("userB@ticketworld.io");
+
+        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userAToken);
+        var response = await _client.GetAsync($"/api/v1/bookings/{userBBooking.Id}");
+
+        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
+        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
+        problem!.Type.Should().Be("https://ticketworld.io/errors/not-found");
+    }
+}
+```
```

---

### 14. [HIGH] Lack of Crypto-Shredding Lifecycle for Encrypted Financial Records
- **File & Section:** `Plan/12_security_compliance_plan.md` (§2.3), `Plan/01_concert_requirement_spec.md` (§7.3)
- **Defect Description:**
  The Thailand Personal Data Protection Act (PDPA B.E. 2562) grants data subjects the right to erasure. However, the Thai Accounting Act B.E. 2543 (Section 14) and Revenue Code require maintaining accounting records and receipts for 5 to 7 years.
  While `Plan/12` specifies static anonymization for user records (`anon+{uuid}@deleted.invalid`), it does not specify how to handle encrypted sensitive data (e.g. Organizer tax IDs and bank accounts encrypted with AES-256-GCM via KMS).
  If static keys are used, encrypted data remains recoverable from backups. Under a true **Crypto-Shredding** architecture, individual PII aggregates are encrypted using per-subject Data Encryption Keys (DEKs) wrapped by a KMS Key Encryption Key (KEK). Upon an approved PDPA erasure request, destroying the subject's DEK renders all historical encrypted data permanently unrecoverable across all database snapshots and backups without modifying immutable accounting records.
- **Diff / Fix Recommendation:**
  Formalize the Crypto-Shredding protocol in Section 2.3 of `Plan/12_security_compliance_plan.md`.

```diff
--- a/Plan/12_security_compliance_plan.md
+++ b/Plan/12_security_compliance_plan.md
@@ -74,2 +74,8 @@
 ```
+
+### 2.4 Crypto-Shredding Protocol for Financial & Sensitive Records
+To reconcile PDPA Right to Erasure with Thai Accounting Act retention mandates (5–7 years):
+1. **Per-Subject Key Hierarchy:** Organizer bank accounts and taxpayer PII are encrypted using envelope encryption where the Data Encryption Key (DEK) is indexed per `user_id` in AWS KMS / Vault.
+2. **Key Destruction on Erasure:** Upon an erasure order or account termination, the KMS DEK associated with `user_id` is permanently deleted (`ScheduleKeyDeletion(0)`).
+3. **Irrevocable Invalidation:** Without the DEK, historical ciphertext stored in `organizer_profiles.bank_account_no_enc` and `payouts.bank_account_snapshot_enc` becomes computationally impossible to decrypt across all production tables and immutable backup archives, achieving statutory erasure while preserving database relational integrity.
```

---

### 15. [MEDIUM] Environment Variable Default Mismatch in Docker Compose
- **File & Section:** `Plan/03_docker_infra_plan.md` (§1.1 vs §3 lines 98–100, 108)
- **Defect Description:**
  In Section 1.1 Table, `POSTGRES_USER` default is documented as `ticketworld_admin`. In Section 3 Docker Compose, line 99 uses fallback `${POSTGRES_USER:-postgres}`.
  If `.env` is unpopulated, PostgreSQL initializes with superuser `postgres`. If the backend application launches expecting `ticketworld_admin`, database connections fail.
  Additionally, line 100 defines `POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}` with no variable validation guard, allowing Compose to start an unauthenticated or crashing database container.
- **Diff / Fix Recommendation:**
  Align the fallback user and use Docker Compose variable assertions (`${VAR:?error}`).

```diff
--- a/Plan/03_docker_infra_plan.md
+++ b/Plan/03_docker_infra_plan.md
@@ -98,3 +98,3 @@
       POSTGRES_DB: ${POSTGRES_DB:-ticketworld_db}
-      POSTGRES_USER: ${POSTGRES_USER:-postgres}
-      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
+      POSTGRES_USER: ${POSTGRES_USER:-ticketworld_admin}
+      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:?Error: POSTGRES_PASSWORD must be set in .env}
@@ -108,3 +108,3 @@
     healthcheck:
-      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-postgres} -d ${POSTGRES_DB:-ticketworld_db}"]
+      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-ticketworld_admin} -d ${POSTGRES_DB:-ticketworld_db}"]
```

---

### 16. [MEDIUM] Insecure Example Secrets in Documentation Table
- **File & Section:** `Plan/03_docker_infra_plan.md` (§1.1, lines 32–34)
- **Defect Description:**
  Section 1.1 displays production-looking keys (e.g. `kX82Jv4Q1L7M9zW2Xp3r8t6y1A2b3c4d5e6f7g8h9i0=` for AES encryption). Junior engineers or automated scripts frequently copy documentation tables into `.env.example` or staging configs, leading to hardcoded cryptographic keys in pre-production environments.
- **Diff / Fix Recommendation:**
  Replace with sanitized placeholders and specify a script command to generate keys.

```diff
--- a/Plan/03_docker_infra_plan.md
+++ b/Plan/03_docker_infra_plan.md
@@ -33,2 +33,2 @@
-| `AES_ENCRYPTION_KEY` | 32-byte Base64 key for AES-256-GCM field encryption | `kX82Jv4Q1L7M9zW2Xp3r8t6y1A2b3c4d5e6f7g8h9i0=` |
-| `PAYMENT_WEBHOOK_SECRET` | Webhook verification signing secret | `whsec_test_secret_key_ticketworld` |
+| `AES_ENCRYPTION_KEY` | 32-byte Base64 key for AES-256-GCM field encryption | `GENERATE_VIA: openssl rand -base64 32` |
+| `PAYMENT_WEBHOOK_SECRET` | Webhook verification signing secret | `GENERATE_VIA: openssl rand -hex 32` |
```

---

### 17. [MEDIUM] Unrestricted Resource Consumption (OWASP API4:2023) on Pagination & Presigned Uploads
- **File & Section:** `Plan/07_api_endpoint_plan.md` (§1.2, §3, §10)
- **Defect Description:**
  Endpoints `GET /events`, `GET /notifications`, and `GET /organizer/events/{id}/orders` accept pagination parameters but do not declare an enforceable maximum page size cap (`maxPageSize`). Submitting `pageSize=1000000` induces massive database query latency and server memory exhaustion.
  Furthermore, `POST /files/presigned-url` does not specify byte limits or MIME-type constraints in the S3 presigned policy, enabling users to upload multi-gigabyte files or malicious executables into MinIO storage.
- **Diff / Fix Recommendation:**
  Enforce a hard limit of `pageSize <= 100` across all query endpoints and define upload size policies (5MB max for images, 20MB for SVG layouts).

```diff
--- a/Plan/07_api_endpoint_plan.md
+++ b/Plan/07_api_endpoint_plan.md
@@ -35,2 +35,3 @@
 | OTP Verify (`POST /auth/2fa/verify`) | 5 req | 5 min | Per User |
+| Pagination Global Rule | Max `pageSize`: 100 items | Per Request | Automatically clamped via `Math.Min(pageSize, 100)` |
@@ -159,3 +160,3 @@
-| 1 | `POST` | `/files/presigned-url` | Organizer / Admin | ขอ Presigned URL สำหรับ Upload ไป MinIO/S3 |
+| 1 | `POST` | `/files/presigned-url` | Organizer / Admin | ขอ Presigned URL สำหรับ Upload ไป MinIO/S3 (จำกัด Max 5MB รูปภาพ, Max 20MB SVG ผังที่นั่ง, Whitelist MIME: `image/jpeg`, `image/png`, `image/webp`, `image/svg+xml`) |
```

---

### 18. [MEDIUM] Missing Omise Webhook Receiver Endpoint in API Catalog
- **File & Section:** `Plan/07_api_endpoint_plan.md` (§13, line 214) vs `Plan/01_concert_requirement_spec.md` (§7.1), `Plan/09_payment_reconciliation_plan.md` (§2)
- **Defect Description:**
  `Plan/07` Module 13 lists only `POST /webhooks/stripe`. However, `Plan/01`, `Plan/09`, and `Plan/12` explicitly state that TicketWorld uses **Omise** for Thailand payments (PromptPay QR and local credit cards). Omise sends event webhooks with a distinct signature verification header (`X-Omise-Signature`). Without a dedicated Omise webhook endpoint, PromptPay payments and local Thai card transactions cannot be reconciled automatically.
- **Diff / Fix Recommendation:**
  Add `POST /webhooks/omise` to Module 13 in `Plan/07`.

```diff
--- a/Plan/07_api_endpoint_plan.md
+++ b/Plan/07_api_endpoint_plan.md
@@ -214,2 +214,3 @@
 | 1 | `POST` | `/webhooks/stripe` | Signature Verify | รับ Payment Webhook (Idempotent dedupe by `event_id`) |
+| 2 | `POST` | `/webhooks/omise` | Signature Verify | รับ Omise Webhook สำหรับ PromptPay / Credit Card (ตรวจสอบ HMAC Signature) |
```

---

### 19. [MEDIUM] PCI-DSS v4.0 Requirement 6.4.3: Missing Script Integrity & CSP Controls on Payment Pages
- **File & Section:** `Plan/12_security_compliance_plan.md` (§3), `Plan/04_backend_guidelines.md` (§3)
- **Defect Description:**
  PCI-DSS v4.0 introduces mandatory Requirement 6.4.3 for all SAQ A merchants (effective March 2025): merchants must implement technical controls to manage all scripts executed in the consumer's browser on payment checkout pages to prevent Magecart and client-side digital skimming attacks.
  Neither `Plan/04` nor `Plan/12` specifies Content Security Policy (CSP) headers or Subresource Integrity (SRI) checks.
- **Diff / Fix Recommendation:**
  Add CSP directives to backend security headers middleware restricting external scripts to `js.stripe.com` and `cdn.omise.co`.

```diff
--- a/Plan/12_security_compliance_plan.md
+++ b/Plan/12_security_compliance_plan.md
@@ -87,2 +87,3 @@
 | **Annual Pentest** | ทดสอบ penetration ประจำปี + ระบุ scope เครื่องที่แตะ token |
+| **Script Integrity (Req 6.4.3)** | Enforce strict Content-Security-Policy (CSP) บนหน้า Checkout: `script-src 'self' https://js.stripe.com https://cdn.omise.co` และ `frame-src https://js.stripe.com https://cdn.omise.co` พร้อมเปิดใช้งาน SRI (Subresource Integrity) เพื่อป้องกัน Magecart Skimming |
```

---

# Minor Corrections and Typos

### 1. [LOW] Broken Markdown Links to Deprecated Plan Filenames
- **File & Section:** `Plan/04_backend_guidelines.md` (line 219), `Plan/07_api_endpoint_plan.md` (line 5), `Plan/12_security_compliance_plan.md` (line 4), `Plan/13_testing_qa_plan.md` (line 5)
- **Defect Description:**
  Several plans reference outdated or non-existent file paths:
  - `Plan/04` line 219 references `[Plan/entity-plan.md](...)`, which should be `Plan/06_entity_db_plan.md`.
  - `Plan/07` line 5, `Plan/12` line 4, and `Plan/13` line 5 reference `[Plan/concert.md](...)`, which should be `Plan/01_concert_requirement_spec.md`.
- **Diff / Fix Recommendation:**
```diff
--- a/Plan/04_backend_guidelines.md
+++ b/Plan/04_backend_guidelines.md
@@ -219,1 +219,1 @@
-> Detailed entity schema & code samples: [Plan/entity-plan.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/entity-plan.md)
+> Detailed entity schema & code samples: [Plan/06_entity_db_plan.md](Plan/06_entity_db_plan.md)
```
```diff
--- a/Plan/07_api_endpoint_plan.md
+++ b/Plan/07_api_endpoint_plan.md
@@ -5,1 +5,1 @@
-> **Source Spec:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md) (Section 3)
+> **Source Spec:** [Plan/01_concert_requirement_spec.md](Plan/01_concert_requirement_spec.md) (Section 3)
```
```diff
--- a/Plan/12_security_compliance_plan.md
+++ b/Plan/12_security_compliance_plan.md
@@ -4,1 +4,1 @@
-> **Source Spec:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md) (Section 7)
+> **Source Spec:** [Plan/01_concert_requirement_spec.md](Plan/01_concert_requirement_spec.md) (Section 7)
```
```diff
--- a/Plan/13_testing_qa_plan.md
+++ b/Plan/13_testing_qa_plan.md
@@ -5,1 +5,1 @@
-> **Source Spec:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md) (Section 6.6)
+> **Source Spec:** [Plan/01_concert_requirement_spec.md](Plan/01_concert_requirement_spec.md) (Section 6.6)
```

---

### 2. [LOW] Inconsistent Variable Naming in k6 Stress Script
- **File & Section:** `Plan/13_testing_qa_plan.md` (§5.1, lines 309–310)
- **Defect Description:**
  In the k6 seat-hold stress testing script, line 309 references `'X-WaitingRoom-Token': __ENV.ROOM_TOKEN`, whereas line 303 uses `${__ENV.API_URL}` and line 308 uses `${__ENV.TOKEN}`. For consistency in k6 scripts, environment variable prefixes should follow identical casing conventions.
- **Diff / Fix Recommendation:**
```diff
--- a/Plan/13_testing_qa_plan.md
+++ b/Plan/13_testing_qa_plan.md
@@ -308,3 +308,3 @@
         'Authorization': `Bearer ${__ENV.TOKEN}`,
-        'X-WaitingRoom-Token': __ENV.ROOM_TOKEN,
+        'X-WaitingRoom-Token': __ENV.WAITING_ROOM_TOKEN,
       },
```

---

### 3. [LOW] Missing Missing CSRF Header Description in 04 Guidelines
- **File & Section:** `Plan/04_backend_guidelines.md` (§1, line 36)
- **Defect Description:**
  Line 36 lists `Middlewares/` as containing `Exception handler, CorrelationId, Rate limiting`, but omits `CsrfAntiforgeryMiddleware`. Since refresh tokens are stored in `httpOnly` cookies, double-submit cookie CSRF protection (`X-CSRF-Token`) is required for mutating requests as declared in `Plan/01` line 115 and `Plan/07` line 21.
- **Diff / Fix Recommendation:**
```diff
--- a/Plan/04_backend_guidelines.md
+++ b/Plan/04_backend_guidelines.md
@@ -36,1 +36,1 @@
-    ├── Middlewares/        # Exception handler, CorrelationId, Rate limiting
+    ├── Middlewares/        # Exception handler, CorrelationId, Rate limiting, CsrfAntiforgery
```
