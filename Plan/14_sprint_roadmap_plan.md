# 🗺️ System Implementation Roadmap & Sprint Plan

> **Scope:** Full Lifecycle Development Plan for TicketWorld Multi-Portal Platform  
> **Architecture Reference:** Clean Architecture (.NET 8) + Angular 18 + PostgreSQL 16 + Redis + SignalR  
> **Source Spec:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md)

---

## 1. Phase Breakdown Overview

| Phase | Focus Area | Deliverables | Target Duration |
| :---: | :--- | :--- | :---: |
| **Phase 1** | Foundation & Identity | Clean Architecture skeleton, JWT RS256, 2FA, RBAC/ABAC, Docker environment | Sprint 1-2 (4 weeks) |
| **Phase 2** | Catalog & Venue Layouts | Artists, Venues, SVG Seat Map Builder, Events, Ticket Tiers | Sprint 3-4 (4 weeks) |
| **Phase 3** | High-Concurrency & Real-time | Virtual Waiting Room, RedLock Seat Hold, SignalR Seat Map Hub | Sprint 5-6 (4 weeks) |
| **Phase 4** | Checkout & Reconciliation | Payment Gateway, Webhook Idempotency, Race Condition Resolver, QR Tickets | Sprint 7-8 (4 weeks) |
| **Phase 5** | Operations & Event-Day | Staff Check-in Scanner, Ticket Transfers, Waitlist Conversion Engine | Sprint 9-10 (4 weeks) |
| **Phase 6** | Enterprise & Compliance | Organizer Payouts, Auto-Refunds, PDPA Data Erasure, Audit Trail, DR | Sprint 11-12 (4 weeks) |

---

## 2. Detailed Phase Specifications

### Phase 1: Foundation & Identity Core
* **Backend:**
  * Clean Architecture solution structure (`Domain`, `Application`, `Infrastructure`, `WebAPI`).
  * Authentication: JWT RS256 signing, Refresh Token Rotation & Reuse Detection.
  * Security: TOTP Two-Factor Authentication, Account Lockout, Argon2id password hashing.
  * Authorization: Claims-based Policies (`Customer`, `Organizer`, `OrgStaff`, `Admin`) and resource ownership handlers.
* **Database & Infra:**
  * EF Core Migrations for `users`, `refresh_tokens`, `organizer_profiles`, `organizer_staff`.
  * Docker Compose stack verification (PostgreSQL, Redis, Seq, MinIO, MailDev).
* **Frontend:**
  * Angular 18 base project, Tailwind CSS design system, JWT HTTP Interceptor, Route Guards.

### Phase 2: Catalog & Seat Map Builder
* **Backend:**
  * CRUD APIs for `artists`, `artist_followers` (atomic counter trigger), `venues`.
  * Interactive Seat Map Layout schema: `seat_layouts` storing SVG coordinates and section configurations.
  * Event lifecycle engine: Draft ➜ Pending Approval ➜ Published ➜ On Sale ➜ Closed ➜ Cancelled.
* **Frontend:**
  * Organizer Portal: Event creation wizard, SVG Seat Map Builder (drag & drop seat/section arranger).
  * Customer Portal: Event discovery catalog, search, artist follow toggle.

### Phase 3: High-Concurrency & Real-Time Ticketing
* **Backend:**
  * **Virtual Waiting Room:** Redis Sorted Sets (`ZADD queue:{eventId}`) for fair queuing under traffic spikes.
  * **Distributed Locking:** RedLock algorithm (3-second TTL) preventing microsecond double-holds.
  * **Real-time SignalR Hub:** `/hubs/seats` broadcasting real-time seat transitions (`Available` ➜ `Held` ➜ `Booked`).
  * **Hold Expiry Background Worker:** Hangfire recurring job evaluating expired holds every 10 seconds.
* **Frontend:**
  * Real-time Interactive SVG Seat Map viewer with live color updates and 10-minute hold countdown timer.
  * Waiting room waiting screen with live queue position indicator.

### Phase 4: Financial Transactions & Reconciliation
* **Backend:**
  * Booking lifecycle state machine (`Pending` ➜ `Confirmed` / `Cancelled`).
  * Stripe/Omise Payment Gateway integration with `X-Idempotency-Key` protection.
  * **Critical Reconciliation Engine:** Resolving hold-expiry vs payment-webhook race conditions (90s grace window vs instant auto-refund).
  * Cryptographic QR Code Ticket Generation (HMAC-SHA256 signed payload).
* **Frontend:**
  * Checkout flow, payment intent integration, digital ticket QR display, email confirmation dispatch.

### Phase 5: Event-Day Operations & Ticketing Logistics
* **Backend:**
  * Staff Sub-role system: Limited event-scoped permissions (`checkin.scan`).
  * Ticket Check-in API with double-scan prevention and audit logging.
  * Ticket Transfer state machine: Safe ownership reassignment with invalidation of old QR codes.
  * Waitlist automated FCFS conversion engine triggered when booked tickets are cancelled.
* **Frontend:**
  * Organizer Staff Mobile Web App: Camera-based WebRTC QR code ticket scanner with sound feedback.
  * Customer Ticket Management: Download PDF/Apple Wallet pass, transfer ticket to friend.

### Phase 6: Enterprise Compliance & Governance
* **Backend:**
  * Organizer Payouts: Automated commission fee calculation and payout release workflows.
  * Refund Engine: Full/partial refund processing integrated with payment gateways.
  * PDPA & Privacy: Soft-delete anonymization, data erasure workflows.
  * Audit Logging: Interceptor capturing all mutations into `audit_logs` (Who, When, Old Values, New Values).
* **Frontend:**
  * Platform Admin Portal: Global revenue analytics, payout approval desk, system health telemetry.
