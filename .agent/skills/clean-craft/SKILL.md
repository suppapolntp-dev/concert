---
name: clean-craft
description: >-
  Universal software craftsmanship, architectural boundaries, OOP/Clean Code principles,
  RESTful API envelopes, global CSS architecture, and complete UI state disciplines.
  Trigger on /craft, /clean-craft, /architecture, /api-design, /ui-states and proactively
  whenever designing system architectures, scaffolding features, reviewing code, or building UI.
---

# 🏛️ Universal Software Craftsmanship & System Architecture Skill

This skill governs standard engineering practices, architectural boundaries, OOP design
principles, RESTful contracts, global CSS topography, and robust UI/UX state engineering
across all application layers — regardless of the programming language or runtime.

**Recite once at the start of any session governed by this skill:**

> **The Five Guardrails:**
> 1. Layers are boundaries, not suggestions.
> 2. Controllers are thin — zero business logic.
> 3. Every response wears a standard envelope.
> 4. Every UI surface covers four states.
> 5. Contracts first, pixels last.

---

## 1. 📂 Core Architectural Topography (Layer Separation)

Maintain strict separation of concerns across physical or logical boundaries.
When scaffolding or reviewing any project, map every file to exactly one of these layers.
If a file serves two layers, it must be split.

```text
├── Presentation Layer (Web / Client / Interface)
│   ├── routes / pages / screens          # Entry points & view scaffolds
│   ├── components                        # UI building blocks
│   │   ├── primitives                    # Atomic design controls (Buttons, Inputs, Modals)
│   │   ├── common                        # Reusable compounds (DataTable, Skeletons, EmptyState)
│   │   └── domain-specific               # Feature-bound composite widgets
│   ├── state / context / stores          # Client-side presentation state
│   ├── network / api-clients             # Gateway clients & HTTP interceptors
│   └── contracts / types                 # Client-side view models & DTOs
│
└── Backend Layer (Service / Server / Core)
    ├── entrypoints / controllers         # Thin protocol adapters (HTTP, gRPC, CLI)
    ├── domain / entities                 # Core business identity, invariants & rules
    ├── application / services            # Business workflows, use-cases & orchestration
    ├── contracts / dtos                  # Request/Response data transfer structures
    ├── persistence / data-access         # Database context, mappings & migrations
    ├── middlewares / interceptors        # Cross-cutting concerns (Auth, Exceptions, Audit)
    └── common / infrastructure           # Shared primitives, utils & external clients
```

### Layer Violation Checklist

Before writing or approving any code, verify:

- [ ] No persistence or database imports inside a controller / entrypoint.
- [ ] No HTTP/request object referenced inside domain / entity code.
- [ ] No direct database queries outside the persistence layer.
- [ ] No business calculation inside a controller — delegate to service or domain.
- [ ] No UI rendering logic coupled directly to raw data-fetching without typed DTOs.

---

## 2. 🧩 OOP & Clean Code Principles

### 2.1 Single Responsibility Principle (SRP)

| Component | Allowed Responsibilities | Strictly Forbidden |
|---|---|---|
| **Entrypoint / Controller** | Deserialize input → validate schema → dispatch to service → serialize envelope | Business logic, raw queries, data calculations, direct I/O |
| **Application / Service** | Orchestrate domain + persistence + external gateways | Direct HTTP concerns, HTML/view rendering, raw queries |
| **Domain Model / Entity** | Encapsulate invariants, expose domain actions as methods | Infrastructure imports, serialization annotations, framework coupling |
| **Repository / Data-Access** | CRUD, query composition, migration management | Business rules, domain validation beyond schema constraints |

### 2.2 Dependency Inversion Principle (DIP) & Abstraction

- High-level policy must **never** depend on low-level implementation details. Both depend on abstractions (interfaces, protocols, abstract base contracts).
- Inject dependencies via constructors or factories. **No** hardcoded static calls or direct instantiations of I/O resources inside core logic.
- Differentiate component lifecycles clearly:

| Lifecycle | Examples | Rule |
|---|---|---|
| **Request-Scoped** | Unit of work, DB context, user session | Create per request, dispose at boundary |
| **Singleton** | Stateless utils, global cache, token verifier | One instance, thread-safe, no mutable shared state |
| **Background Worker** | Async consumers, event listeners, cron runners | Isolated scope, own error boundary, idempotent |

### 2.3 Composition over Inheritance

- Use inheritance **exclusively** for strictly immutable identity and auditing contracts (e.g., base entity holding ID, creation timestamps, tracking markers).
- Use **composition** for polymorphic behaviors, capabilities, and business strategies.
- If an inheritance tree exceeds depth 2, refactor to composition + interfaces.

---

## 3. 🌐 API Standards & Protocol Envelopes

### 3.1 Unified Response Envelope

**Every** network response must follow this predictable, generic envelope contract.
No raw arrays, no bare strings, no ad-hoc shapes.

#### Success Contract
```json
{
  "success": true,
  "message": "Resource fetched successfully",
  "data": { "id": "res_123", "name": "Item Name" },
  "errors": null
}
```

#### Paginated Collection Contract
```json
{
  "success": true,
  "message": "Query completed",
  "data": [ { "id": "res_1" }, { "id": "res_2" } ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalRecords": 105,
    "totalPages": 6
  },
  "errors": null
}
```

#### Failure Contract
```json
{
  "success": false,
  "message": "Human-readable failure summary",
  "data": null,
  "errors": [
    {
      "field": "email",
      "code": "INVALID_FORMAT",
      "detail": "Email address format is invalid"
    }
  ]
}
```

### 3.2 Semantic Resource Routing & Verbs

| Intent | Verb | URI Pattern | Ideal Status Code |
|---|---|---|---|
| Query collection | `GET` | `/api/v1/{resources}?page=1` | `200 OK` |
| Retrieve by identifier | `GET` | `/api/v1/{resources}/{id}` | `200 OK` or `404 Not Found` |
| Create new resource | `POST` | `/api/v1/{resources}` | `201 Created` with resource |
| Full / Partial update | `PUT` / `PATCH` | `/api/v1/{resources}/{id}` | `200 OK` |
| Deactivate / Remove | `DELETE` | `/api/v1/{resources}/{id}` | `200 OK` or `204 No Content` |

---

## 4. 🎨 Global CSS & Design System Architecture

Whether using Vanilla CSS, Tailwind, SCSS, or CSS Modules, maintain strict specificity tiers:

```text
├── styles / css / theme
│   ├── 00-tokens/               # 1. Design Tokens & Primitive Variables (colors, spacing, radius)
│   ├── 01-reset/                # 2. Reset / Normalize (box-sizing, fluid media, margin resets)
│   ├── 02-base/                 # 3. Base Element Typography (html, body, h1-h6, p, a - no classes)
│   ├── 03-layout/               # 4. App Shell & Macro Grid (container, header layout, drawer)
│   ├── 04-animations/           # 5. Shared Motion (@keyframes shimmer, spin, fade)
│   └── 05-utilities/            # 6. Global Utilities (.sr-only, .text-truncate)
└── components/                  # ❌ Component-specific styles belong strictly in components!
```

### CSS Guardrails
- **Semantic over Primitive:** In components, consume semantic variables (`var(--bg-surface)`), never raw colors (`#0f172a`).
- **No Component Classes in Global CSS:** Keep component styling encapsulated to avoid specificity wars.
- **Zero Magic Numbers:** Use token scales for margins, paddings, and font sizes.

---

## 5. 🖥️ UI/UX & Component Engineering Disciplines

### 5.1 UI State Completeness (The 4 Critical States)

Every dynamic component, screen, or data-driven widget **must** explicitly handle all four states:

| # | State | Requirement | Anti-Pattern |
|---|---|---|---|
| 1 | **Loading** | Render layout skeletons matching final content shape | Blocking full-screen spinner |
| 2 | **Success / Active** | Display resolved content with smooth micro-transitions | Hard layout shifts or jumpy renders |
| 3 | **Empty** | Clear visual feedback + explicit CTA (create / clear filters) | Blank white space or dead-end "No data" |
| 4 | **Error** | Descriptive explanation + safe recovery action (Retry button) | Silent failure or raw stack traces |

### 5.2 Form & Interaction Discipline

- **Instant Validation:** Surface field errors inline on blur or submit.
- **Prevent Double-Submission:** Disable submit triggers and display inline spinner during flight.
- **Accessible & Consistent Spacing:** Use semantic design tokens. Full keyboard navigation (`Tab`, `Enter`, `Space`).

---

## 6. 🔀 Vertical Slice Feature Workflow (Step-by-Step SOP)

When implementing any feature end-to-end, execute in this precise order:

1. **Contract & Schema First:** Define persistent schema and immutable DTO request-response contracts.
2. **Persistence Mapping:** Register repository bindings with clean migrations.
3. **Domain & Business Logic:** Implement domain invariants, interface contracts, and application service orchestration.
4. **Endpoint Exposure:** Bind thin entrypoints/controllers strictly wrapped in the Unified Response Envelope.
5. **Client Contracts & Services:** Declare client-side types mirroring DTOs with API error unwrapping.
6. **UI & View Scaffolding:** Build modular components covering all four UI states (Loading, Success, Empty, Error).

---

## 7. 🔍 Self-Audit Protocol

Before submitting or reporting any feature as complete, verify:

1. **Layer check:** Is every file cleanly assigned to one layer without cross-boundary contamination?
2. **Thin controller check:** Are all controllers free of business logic and raw queries?
3. **Envelope check:** Do all API responses strictly follow the envelope format?
4. **Four-state check:** Does the UI handle Loading, Success, Empty, and Error states?
5. **Token check:** Are all styles referencing design tokens instead of magic values?
6. **DIP check:** Does core logic depend purely on abstractions rather than concrete I/O implementations?
