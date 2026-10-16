---
name: enterprise-software-craftsmanship-standards
description: >-
  Use this skill when designing system architectures, organizing directory structures,
  implementing Object-Oriented principles, defining API contracts, or building complete
  UI components across any programming language, framework, or technology stack.
  Trigger on /craft, /architecture, /api-design, /ui-states and proactively whenever the
  user asks to scaffold a feature, design a module, create an API endpoint, build a UI
  component, or review code for architectural compliance.
---

# 🏛️ Universal Software Craftsmanship & System Architecture Skill

This skill governs standard engineering practices, architectural boundaries, OOP design
principles, RESTful contracts, and robust UI/UX state engineering across all application
layers — regardless of the programming language or runtime.

**Recite once at the start of any session governed by this skill:**

> **The Five Guardrails:**
> 1. Layers are boundaries, not suggestions.
> 2. Controllers are thin — zero business logic.
> 3. Every response wears an envelope.
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
    ├── middlewares / interceptors         # Cross-cutting concerns (Auth, Exceptions, Audit)
    └── common / infrastructure           # Shared primitives, utils & external clients
```

### Layer violation checklist

Before writing or approving any code, verify:

- [ ] No persistence import inside a controller / entrypoint.
- [ ] No HTTP/request object referenced inside domain / entity code.
- [ ] No direct database query outside the persistence layer.
- [ ] No business calculation inside a controller — delegate to service or domain.
- [ ] No UI rendering logic coupled to a specific data-fetching strategy.

---

## 2. 🧩 OOP & Clean Code Principles

### 2.1 Single Responsibility Principle (SRP)

| Component | Allowed Responsibilities | Strictly Forbidden |
|---|---|---|
| **Entrypoint / Controller** | Deserialize input → validate schema → dispatch to service → serialize envelope | Business logic, raw queries, data calculations, direct I/O |
| **Application / Service** | Orchestrate domain + persistence + external gateways | Direct HTTP concerns, rendering, raw SQL outside repository calls |
| **Domain Model / Entity** | Encapsulate invariants, expose domain actions as methods | Infrastructure imports, serialization annotations, framework coupling |
| **Repository / Data-Access** | CRUD, query composition, migration management | Business rules, validation beyond schema constraints |

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

### 2.4 Naming & Readability Guardrails

- Names describe **what**, not **how**: `OrderPlacementService`, not `OrderHelper`.
- Boolean names read as assertions: `isActive`, `hasPermission`, `canRetry`.
- Methods returning collections are plural: `getActiveUsers()`, not `getUser()`.
- Abbreviations are banned outside universally-known acronyms (ID, URL, HTTP, DTO).

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
  "data": { "..." : "..." },
  "errors": null
}
```

#### Paginated Collection Contract

```json
{
  "success": true,
  "message": "Query completed",
  "data": [ "..." ],
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

### 3.2 Envelope enforcement rules

- The envelope structure is **framework-level middleware**, not per-endpoint boilerplate.
  Implement it once in a response wrapper / interceptor.
- `message` is always human-readable, never a raw exception or stack trace.
- `errors[]` items always include `field` (nullable for global errors), `code` (machine-readable constant), and `detail` (human sentence).
- `data` is always the expected type for the endpoint — never polymorphic between success and failure.

### 3.3 Semantic Resource Routing & Verbs

| Intent | Verb | URI Pattern | Ideal Status Code |
|---|---|---|---|
| Query collection | `GET` | `/api/v1/{resources}?page=1` | `200 OK` |
| Retrieve by identifier | `GET` | `/api/v1/{resources}/{id}` | `200 OK` or `404 Not Found` |
| Create new resource | `POST` | `/api/v1/{resources}` | `201 Created` with resource |
| Full / Partial update | `PUT` / `PATCH` | `/api/v1/{resources}/{id}` | `200 OK` |
| Deactivate / Remove | `DELETE` | `/api/v1/{resources}/{id}` | `200 OK` or `204 No Content` |

### 3.4 Routing anti-patterns (reject on sight)

- `POST /api/getUsers` — verb in URI.
- `GET /api/v1/user` — singular collection noun.
- Returning `200 OK` with `{ "error": "..." }` — abuse of status codes.
- Nested resources deeper than 2 levels — flatten or use query params.

---

## 4. 🎨 UI/UX & Component Engineering Disciplines

### 4.1 UI State Completeness (The 4 Critical States)

Every dynamic component, screen, or data-driven widget **must** explicitly handle all four states.
Generating a component that lacks any of these is a defect.

| # | State | Requirement | Anti-Pattern |
|---|---|---|---|
| 1 | **Loading** | Render non-jarring layout skeletons matching the final shape | Full-screen blocking spinner, no visual feedback |
| 2 | **Success / Active** | Display resolved content with smooth micro-transitions | Hard content pop-in, layout shift |
| 3 | **Empty** | Clear visual feedback with explicit CTA (create / clear filters) | Blank white space, "No data" with no action |
| 4 | **Error** | Descriptive message explaining the problem + safe recovery action (Retry) | Silent failure, raw error dump, no recovery path |

### 4.2 State implementation checklist

When generating or reviewing a UI component, verify each state is present:

```text
□ Loading  → skeleton / shimmer rendered?     File: ___ Line: ___
□ Success  → data rendered, transitions?      File: ___ Line: ___
□ Empty    → illustration + CTA rendered?     File: ___ Line: ___
□ Error    → message + retry rendered?        File: ___ Line: ___
```

If any box is unchecked, the component is incomplete — do not move on.

### 4.3 Form & Interaction Discipline

- **Instant Validation:** Surface field errors inline immediately after input blur or submission attempt.
- **Prevent Double-Submission:** Disable submit triggers and display inline loading during flight. Re-enable only on response.
- **Accessible & Consistent Spacing:** Use semantic design tokens for padding, colors, typography, hover/focus rings. No magic pixel values.
- **Keyboard Navigation:** Every interactive element must be reachable via Tab and operable via Enter/Space.
- **Optimistic vs Pessimistic UI:** Decide explicitly per mutation. Document which strategy and why.

---

## 5. 🔀 Vertical Slice Feature Workflow (Step-by-Step SOP)

When asked to implement any new feature end-to-end, execute in this precise order.
**Do not skip or reorder steps.** Each step's output is the next step's input.

### Step 1 — Contract & Schema First

- Define the persistent schema (table, collection, or document structure).
- Define immutable DTO / request-response contracts.
- **Gate:** Contracts must be reviewed before any implementation begins.

### Step 2 — Persistence Mapping

- Register repository or data-access bindings.
- Write clean schema migration rules (up + down / rollback).
- **Gate:** Migration runs successfully in a clean environment.

### Step 3 — Domain & Business Logic

- Write interface contracts (abstractions) for the service layer.
- Implement domain validation checks and invariant enforcement.
- Write service orchestration coordinating persistence + domain.
- **Gate:** Unit tests for domain logic pass.

### Step 4 — Endpoint Exposure

- Bind thin entrypoints / controllers adhering strictly to the Response Envelope.
- Wire middleware (auth, validation, error handling).
- **Gate:** Integration test hits the endpoint and receives a valid envelope.

### Step 5 — Client Contracts & API Services

- Declare client-side types mirroring the API DTOs.
- Configure API client methods with proper error unwrapping into the envelope shape.
- **Gate:** Client service can call the endpoint and deserialize the response.

### Step 6 — UI & View Scaffolding

- Build modular components covering **all four UI states** (Loading, Success, Empty, Error).
- Wire to client services from Step 5.
- **Gate:** All four states are visually demonstrable (manually or via storybook / test harness).

---

## 6. 🔍 Self-Audit Protocol

After completing any feature, module, or code change governed by this skill, run this
self-audit before presenting the result:

1. **Layer check:** Can I draw a line between every file and exactly one layer from §1?
2. **Thin controller check:** Does every controller/entrypoint contain zero business logic?
3. **Envelope check:** Does every API response conform to the envelope from §3.1?
4. **Four-state check:** Does every UI component handle Loading, Success, Empty, and Error?
5. **Contract-first check:** Were DTOs / types defined before implementation?
6. **DIP check:** Does core logic depend only on abstractions, never on infrastructure?

If any answer is **no**, fix it before reporting completion.

---

## Operating Rules

- **This skill is language-agnostic.** The patterns apply identically whether the stack is Python, Go, Java, Rust, TypeScript, C#, or anything else. Adapt syntax; never compromise the principle.
- **No partial components.** A UI component missing any of the four states is rejected.
- **No fat controllers.** Any business logic found in an entrypoint/controller triggers an immediate refactor.
- **No naked responses.** Any API response not wrapped in the standard envelope triggers an immediate fix.
- **Contracts are immutable during a sprint.** Once reviewed, DTOs change only via a new versioned contract.
- **Every guardrail violation is a finding.** Surface it immediately; don't bury it in a footnote.
