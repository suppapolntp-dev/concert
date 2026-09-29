# DOCUMENT TEXT MAP — TicketWorld Full Specification Plan Suite

<!-- FILE: Plan/00_ROADMAP_READING_ORDER.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\00_ROADMAP_READING_ORDER.md
Title: Project Roadmap & Reading Order Guide
Scope: Step-by-step reading order (01 to 14), 6 stages, RACI matrix and role-based reading guides.

<!-- FILE: Plan/01_concert_requirement_spec.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\01_concert_requirement_spec.md
Title: TicketWorld Concert Ticket Booking Platform — Requirement Specification
Scope: Core functional and non-functional requirements, business rules, booking lifecycle, waiting room rules, payment rules, user roles, data dictionary v2.1.

<!-- FILE: Plan/02_master_project_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\02_master_project_plan.md
Title: TicketWorld Master Project Plan
Scope: Project objectives, architecture overview, team organization, RACI matrix, risk management, milestone schedule, definition of done.

<!-- FILE: Plan/03_docker_infra_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\03_docker_infra_plan.md
Title: TicketWorld Infrastructure & Docker Compose Plan
Scope: Containerized services (PostgreSQL 16, Redis 7, MinIO, Seq, MailDev), port mappings, volume mounts, environment variables, health checks, backup strategy.

<!-- FILE: Plan/04_backend_guidelines.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\04_backend_guidelines.md
Title: TicketWorld Backend Architecture & Coding Guidelines
Scope: Clean Architecture (4 layers), .NET 8 / C# 12 standards, Result Pattern, MediatR CQRS, FluentValidation, exception handling, logging standards.

<!-- FILE: Plan/05_oop_design_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\05_oop_design_plan.md
Title: TicketWorld Domain Model & OOP Design Plan
Scope: 17 Domain Enums, 8 Value Objects, Domain Exceptions, Domain Events, Aggregate Roots, Entity interfaces, Repository contracts.

<!-- FILE: Plan/06_entity_db_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\06_entity_db_plan.md
Title: TicketWorld Entity & Database Schema Plan
Scope: 24 Entities, EF Core Fluent API mappings, PostgreSQL data types, primary keys (UUIDv7), foreign keys, indices, concurrency token (`xmin`), soft delete (`IsDeleted`).

<!-- FILE: Plan/07_api_endpoint_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\07_api_endpoint_plan.md
Title: TicketWorld REST API Specification
Scope: 78 REST endpoints, HTTP methods, route paths, auth policies, request/response DTOs, query parameters, rate limits, RFC 7807 error envelopes.

<!-- FILE: Plan/08_realtime_concurrency_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\08_realtime_concurrency_plan.md
Title: TicketWorld Real-Time & High Concurrency Architecture
Scope: Virtual Waiting Room (Redis Sorted Sets), token admission throttle, RedLock 3-second distributed seat locking, SignalR seat map broadcasting, fallback strategies.

<!-- FILE: Plan/09_payment_reconciliation_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\09_payment_reconciliation_plan.md
Title: TicketWorld Payment Flow & Reconciliation Plan
Scope: Payment gateway integration (Omise / Stripe), webhook handling, 3 race condition scenarios (clean win, race lost auto-refund, 90s grace window), Hangfire reconciliation jobs, financial audit trail.

<!-- FILE: Plan/10_uxui_design_system_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\10_uxui_design_system_plan.md
Title: TicketWorld UX/UI Design System & Wireframe Plan
Scope: Design tokens, color palette, typography scale, 4px grid, component library, responsive breakpoints, wireframes for customer, organizer, and admin portals.

<!-- FILE: Plan/11_frontend_architecture_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\11_frontend_architecture_plan.md
Title: TicketWorld Frontend Architecture Plan
Scope: Angular 18, standalone components, Signals reactive state, RxJS, multi-portal routing, SVG interactive seat map viewer, WebRTC camera QR scanner, error interceptor.

<!-- FILE: Plan/12_security_compliance_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\12_security_compliance_plan.md
Title: TicketWorld Security & Compliance Plan
Scope: OWASP API Security Top 10 mitigations, NIST SP 800-63B auth standards, PCI-DSS SAQ A tokenization compliance, Thailand PDPA (consent, crypto-shredding, anonymization), secret management.

<!-- FILE: Plan/13_testing_qa_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\13_testing_qa_plan.md
Title: TicketWorld Testing Strategy & QA Plan
Scope: Testing pyramid, unit tests (xUnit, FluentAssertions), integration tests (Testcontainers), k6 performance load testing (500 VUs, SLA thresholds), CI/CD automated test pipeline.

<!-- FILE: Plan/14_sprint_roadmap_plan.md -->
Path: c:\Users\wwwwww\Documents\project-test\concert\Plan\14_sprint_roadmap_plan.md
Title: TicketWorld Sprint Roadmap & Execution Plan
Scope: 6 implementation phases, 12 two-week sprints, milestone dependencies, critical path, risk mitigation milestones.
