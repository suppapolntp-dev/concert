# 🏛️ TICKETWORLD — MASTER SPECIFICATION & PROJECT PLAN
> **Unified Architecture, Requirements & Team Assignment Blueprint**  
> **Version:** 2.1-Consolidated | **Tech Stack:** .NET 8 (Clean Architecture) + Angular 18 + PostgreSQL 16 + Redis + SignalR + Docker

---

## 📑 TABLE OF CONTENTS
1. [Executive Overview & Tech Stack](#1-executive-overview--tech-stack)
2. [Team Roles & Assignment Matrix (RACI)](#2-team-roles--assignment-matrix-raci)
3. [Security & Access Control (RBAC/ABAC)](#3-security--access-control-rbacabac)
4. [Database & Entity Architecture (24 Tables)](#4-database--entity-architecture-24-tables)
5. [High-Concurrency & Real-Time Engine](#5-high-concurrency--real-time-engine)
6. [Payment & Reconciliation Engine](#6-payment--reconciliation-engine)
7. [Frontend Architecture (3 Portals + Scanner)](#7-frontend-architecture-3-portals--scanner)
8. [Phase-by-Phase Roadmap & Sprints](#8-phase-by-phase-roadmap--sprints)
9. [DevOps, Docker & Database Seeding](#9-devops-docker--database-seeding)

---

## 1. Executive Overview & Tech Stack

TicketWorld is an enterprise-grade global concert ticketing platform engineered for high-concurrency ticket drops, real-time seat selection, fraud prevention, and strict financial reconciliation.

```
┌────────────────────────────────────────────────────────────────────────┐
│                        TICKETWORLD PLATFORM                            │
├───────────────────┬───────────────────────────────┬────────────────────┤
│ Customer Portal   │ Organizer & Staff Dashboard   │ Platform Admin UI  │
│ (Angular 18 SPA)  │ (Angular 18 + Mobile WebRTC)  │ (Angular 18 SPA)   │
├───────────────────┴───────────────────────────────┴────────────────────┤
│                       WebAPI Layer (.NET 8)                            │
│           (Controllers, Middlewares, SignalR Hubs, MediatR)            │
├───────────────────────────────────┬────────────────────────────────────┤
│ Application Layer (Use Cases)     │ Domain Layer (Business Invariants) │
├───────────────────────────────────┴────────────────────────────────────┤
│                    Infrastructure & Persistence                        │
│ (EF Core + Dapper + PostgreSQL 16 + Redis 7 + MinIO + Hangfire + Seq)   │
└────────────────────────────────────────────────────────────────────────┘
```

| Layer | Technology | Primary Role |
| :--- | :--- | :--- |
| **Backend Framework** | .NET 8 (C#) | Clean Architecture, MediatR CQRS, Minimal APIs / Controllers |
| **Frontend Framework** | Angular 18 (TypeScript) | Standalone Components, Reactive Signals, Tailwind CSS |
| **Primary Relational DB** | PostgreSQL 16 | Relational data integrity, JSONB layouts, `xmin` concurrency |
| **Cache & Distributed Lock**| Redis 7 | Virtual Waiting Room, Distributed Locking (RedLock), Session cache |
| **Real-time Engine** | ASP.NET Core SignalR | Live seat status broadcast (`/hubs/seats`) |
| **Job Scheduler** | Hangfire | Hold expiration, waitlist notification, payout releases |
| **Object Storage** | MinIO (S3-compatible) | Event banners, venue SVG layouts, avatars |
| **Log Management** | Serilog + Datalust Seq | Centralized structured log ingestion with Correlation IDs |

---

## 2. Team Roles & Assignment Matrix (RACI)

Use this matrix to assign deliverables across your engineering team:

| Functional Area | Backend Lead | Backend Dev | Frontend Lead | Frontend Dev | DevOps | QA Engineer |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| **Clean Architecture Foundation** | **A / R** | C | I | I | C | I |
| **Auth, JWT RS256, 2FA & RBAC** | **A** | **R** | C | **R** | I | C |
| **Database Entities & Migrations** | **A** | **R** | I | I | C | C |
| **SVG Seat Map Builder** | C | I | **A / R** | **R** | I | C |
| **Virtual Waiting Room (Redis)** | **A / R** | C | C | **R** | C | C |
| **Seat Hold Lock (RedLock 3s)** | **A / R** | C | I | I | I | **R** |
| **SignalR Seat Map Broadcast** | **A** | **R** | **A** | **R** | I | C |
| **Payment & Webhook Reconciliation** | **A / R** | C | I | **R** | I | **R** |
| **Mobile WebRTC QR Check-in** | C | **R** | C | **A / R** | I | C |
| **Docker, CI/CD, Secrets (.env)** | C | I | I | I | **A / R** | C |
| **Concurrency Load Testing (k6)** | C | I | I | I | C | **A / R** |

*Legend: **A** = Accountable (Owner), **R** = Responsible (Doer), **C** = Consulted, **I** = Informed.*

---

## 3. Security & Access Control (RBAC/ABAC)

### 3.1 Portal Roles
* **`ROLE_CUSTOMER` (Level 1):** Ticket purchase, waiting room, ticket wallet, transfers.
* **`ROLE_ORGANIZER` (Level 2):** Event creation, venue layout builder, sales analytics, staff invites.
* **`ROLE_ORG_STAFF` (Level 2b):** Scoped event access only (`checkin.scan`). Blocked from financial views.
* **`ROLE_ADMIN` (Level 3):** Event moderation, payout approval, system configuration, audit logs.

### 3.2 Token & Session Policies
* **Access Token:** Short-lived JWT (15 minutes), signed via **RS256**.
* **Refresh Token:** 7-day lifetime, stored in `httpOnly`, `Secure`, `SameSite=Strict` cookie.
* **Token Rotation & Reuse Detection:** Any use of an already rotated token immediately invalidates the entire user token family.
* **Sensitive Actions (Step-up Auth):** Payouts and admin role alterations require TOTP claim (`amr: mfa`) validated within the last 5 minutes.

---

## 4. Database & Entity Architecture (24 Tables)

All entities inherit standard Clean Architecture base contracts:
* `BaseEntity<Guid>`: Encapsulates `Id` and `DomainEvents`.
* `AggregateRoot<Guid>`: Transaction boundary marker.
* `IAuditableEntity`: `CreatedAt` and `UpdatedAt` populated via EF Core Interceptor.
* `ISoftDeletable`: `DeletedAt` with automatic Global Query Filter.

### Table Registry & Aggregate Groupings:
```
1. Identity & Access:
   ├── users                   (UUID PK, email UNIQUE, Argon2id password, 2FA, lockout)
   ├── refresh_tokens          (SHA-256 token_hash, rotation chain, device_info)
   ├── organizer_profiles      (Tax ID, AES-256 encrypted bank accounts, commission rate)
   └── organizer_staff         (Event-scoped permissions: checkin.scan)

2. Catalog & Venues:
   ├── artists                 (Slug, genre, atomic follower_count)
   ├── artist_followers        (Composite PK: user_id + artist_id)
   ├── venues                  (Geo-coordinates, capacity, city)
   └── seat_layouts            (JSONB layout data, SVG coordinates)

3. Events & Inventory:
   ├── events                  (Status state machine, sold_seats counter, start/end dates)
   ├── ticket_tiers            (VIP, Regular, Tier pricing, quota)
   └── seats                   (Section, row, number, status: Available/Held/Booked, xmin token)

4. Bookings & Financials:
   ├── bookings                (Booking reference, total amount, status state machine)
   ├── booking_items           (Individual seat item, HMAC signed QR payload)
   ├── payments                (PaymentIntent ID, Idempotency key, status, gateway ref)
   ├── promo_codes             (Discount type, max uses, atomic used_count update)
   ├── promo_usages            (Tracking promo redemptions per user)
   ├── payouts                 (Organizer fund transfers, snapshot bank details)
   └── refunds                 (Auto-refund on race condition, dispute refunds)

5. Operations & Operations:
   ├── reviews                 (Post-event customer feedback)
   ├── waitlists               (FCFS queue for sold-out events)
   ├── waiting_room_tokens     (Priority queue admission tokens)
   ├── ticket_transfers        (Customer-to-customer ticket reassignments)
   ├── audit_logs              (Who, When, What entity, Old JSON, New JSON)
   └── system_configs          (Key-Value platform feature flags)
```

---

## 5. High-Concurrency & Real-Time Engine

### 5.1 Virtual Waiting Room (Redis Sorted Sets)
* **Entry:** `ZADD waiting_room:queue:{eventId} {timestamp} {userId}`
* **Heartbeat:** Clients ping every 3s. Heartbeat loss > 9s results in queue pruning.
* **Admission Rate:** Background worker admits batches (e.g. 100 users / 5 seconds), issuing a signed 15-minute bypass token (`X-WaitingRoom-Token`).

### 5.2 Microsecond Distributed Lock (RedLock)
* Prevents double-holding on identical seats:
  `lock:seat:{seatId}` acquired in Redis with 3-second TTL.
* Database hold sets `status = 'Held'`, `held_until = NOW() + INTERVAL '10 MIN'`.

### 5.3 SignalR Real-Time Seat Map (`/hubs/seats`)
* Clients subscribe by event: `AddToGroupAsync(connectionId, eventId)`.
* Emits `SeatStatusChanged` (`Available` ➜ `Held` ➜ `Booked`) instantly to all viewers on the same map.

---

## 6. Payment & Reconciliation Engine

### 6.1 Critical Hold-Expiry vs Webhook Race Resolver
When a successful payment webhook arrives from the gateway, the seat state is evaluated under a `Serializable` transaction:

```
                  Webhook Arrives
                         │
        ┌────────────────┼────────────────┐
        ▼                ▼                ▼
[ Seat Still Held ] [ Available ]  [ Taken by Other ]
        │                │                │
        │         Paid within 90s?        │
        │          ┌─────┴─────┐          │
        │         YES          NO         │
        ▼          ▼           ▼          ▼
   [ Mark Booked & Issue QR ]   [ Auto-Refund Full Amount ]
```

1. **Clean Win:** Seat is still held by user ➜ Mark `Booked`, confirm booking, issue QR code.
2. **Race Lost:** Seat was already acquired by someone else ➜ Keep seat with new user, mark payment as `PaymentRaceLost`, execute **instant auto-refund**.
3. **Grace Window:** Seat reverted to available ➜ If paid within 90 seconds of hold expiration, re-acquire seat and confirm. If >90s, auto-refund.

---

## 7. Frontend Architecture (3 Portals + Scanner)

### 7.1 Multi-Portal Layout
* **Customer Portal (`/`):** Discovery, Virtual Waiting Room waiting screen, SVG Seat Map, Checkout, Ticket Wallet.
* **Organizer Portal (`/organizer`):** Sales Dashboard, SVG Seat Map Builder, Staff Manager.
* **Staff Mobile Scanner (`/organizer/checkin`):** WebRTC camera stream with `@zxing/browser`, sub-200ms scan latency, audio feedback (Chime / Buzzer).
* **Platform Admin (`/admin`):** Event moderation, Payout approval desk, Seq telemetry.

### 7.2 Interactive SVG Seat Map Component
* Renders coordinate-mapped vector seats.
* Signal-driven state: dynamically reacts to incoming SignalR messages without re-rendering the whole DOM.

---

## 8. Phase-by-Phase Roadmap & Sprints

```
Sprint 1-2   [Phase 1] Foundation, Clean Architecture, Auth, RBAC, Docker
Sprint 3-4   [Phase 2] Catalog, Venues, SVG Seat Map Builder, Event Lifecycle
Sprint 5-6   [Phase 3] Waiting Room, RedLock Seat Hold, Real-time SignalR
Sprint 7-8   [Phase 4] Stripe/Omise Checkout, Webhook Reconciliation, QR Ticket
Sprint 9-10  [Phase 5] Staff QR Scanner, Ticket Transfers, Waitlist Engine
Sprint 11-12 [Phase 6] Organizer Payouts, Auto-Refunds, PDPA Erasure, APM/Audit
```

---

## 9. DevOps, Docker & Database Seeding

### 9.1 Environment & Secret Isolation
* All sensitive credentials isolated into root `.env` (never committed to Git).
* Safe placeholders provided in [.env.example](file:///c:/Users/wwwwww/Documents/project-test/concert/.env.example).

### 9.2 Container Stack
* **PostgreSQL 16:** Relational data store with `./init-scripts` mount for initial seed.
* **Redis 7:** Cache, RedLock, and Waiting Room queues (password-protected).
* **MinIO:** S3 storage (Port 9000/9001).
* **Seq:** Structured logging sink (Port 5341/8081).
* **MailDev:** Mock email testing inbox (Port 1080/1025).

### 9.3 Database Seeding Standards
* **System Master Data:** Executed automatically in all environments via idempotent `ApplicationDbContextInitializer` or [init-scripts/01_seed_master_data.sql](file:///c:/Users/wwwwww/Documents/project-test/concert/init-scripts/01_seed_master_data.sql).
* **High-Volume Seats Bulk Import:** Streamed directly using PostgreSQL `COPY BINARY` (`NpgsqlBinaryImporter`) to populate 10,000+ seats without EF Core memory overhead.
