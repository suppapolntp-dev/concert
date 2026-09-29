# Summary

This unit report delivers a comprehensive, rigorous technical audit of **Track 4: frontend_uxui_qa_and_roadmap** across the TicketWorld specification suite. The audit systematically evaluates the interplay between the Angular 18 frontend architecture (`Plan/11`), UX/UI design system tokens and wireframes (`Plan/10`), QA and test automation strategy (`Plan/13`), sprint implementation roadmap (`Plan/14`), master project governance (`Plan/02`), and the core requirement specification (`Plan/01`), cross-referenced against the high-concurrency engine (`Plan/08`) and financial reconciliation engine (`Plan/09`).

### Key Findings & Structural Deficiencies Identified:
1. **Frontend High-Volume Seat Map Bottleneck:** The SVG seat map architecture in `Plan/11` proposes binding 10,000 to 50,000 individual `<circle>` SVG DOM nodes within a standard Angular `@for` template loop using a single reactive `Map` signal. Under realistic concert drop conditions (50+ SignalR updates/sec and rapid pan/zoom), this architecture results in complete DOM collapse, massive Garbage Collection (GC) thrashing, and frame rates dropping below 5 FPS. The specification lacks Level of Detail (LOD) section-clustering, frustum culling, or HTML5 Canvas/WebGL virtualization.
2. **SignalR Connection & Reconnection Gaps:** `Plan/11` lacks connection lifecycle handling, reconnection backoff, and state resynchronization (`GetSeatMapSnapshot`), leaving clients in desynchronized "ghost" seat states after transient network drops.
3. **WebRTC QR Scanner Vulnerability at Gates:** The mobile check-in scanner (`Plan/11`, `Plan/01`) lacks offline roster pre-caching (IndexedDB) and background synchronization. At stadium gates with cell-tower saturation, check-in will stall entirely. Furthermore, `Plan/01` restricts scanner access to `Organizer (Owner)`, inadvertently locking out `ROLE_ORG_STAFF`.
4. **Design System WCAG AA Contrast Failures:** `Plan/10` specifies Primary Brand `#6366F1` (Indigo-500) and Accent `#F59E0B` (Amber-500), both of which fail WCAG 2.1 AA contrast requirements for normal text on white backgrounds. In addition, reliance purely on red/green colors for seat availability fails accessibility standards for colorblind users (WCAG 1.4.1), and breakpoint definitions between Section 4.2 and Section 8 are contradictory.
5. **Flawed k6 Performance Load Testing Thresholds:** In `Plan/13`, the k6 load testing configuration sets `http_req_failed: ['rate<0.30']` for a 500 VU race on 100 seats. In k6, HTTP 409 Conflict is treated as a failed request (`status >= 400`). Since ~80% of concurrent seat contenders are legitimately expected to receive 409 Conflict, the test suite will falsely report CI pipeline failure on correct business behavior. Moreover, the SLA threshold of `< 500ms` at p95 is misaligned with the microsecond RedLock architecture.
6. **Critical Financial & Concurrency Test Suite Gaps:** `Plan/13` omits test specifications for the 3 payment webhook race conditions defined in `Plan/09` (Clean Win, Race Lost auto-refund, 90s Grace Window reclamation), Waiting Room token tampering/bypass, and payment gateway auto-refund failure retries with Dead-Letter Queues (DLQ).
7. **Roadmap Granularity & Sequencing Flaws:** `Plan/14` groups 12 sprints into 6 high-level phases without defining individual sprint scopes or allocating time for k6 concurrency testing, security penetration testing, or DR drills. Crucially, the `bookings` database schema is postponed to Phase 4 (Sprints 7-8), creating a circular dependency because Phase 3 (Sprints 5-6) seat locking and Hangfire hold-expiry workers depend on the `bookings` and `payments` tables.

---

# Potential Mistakes and Improvements

## 1. Angular 18 Frontend Architecture & Real-Time Engine

### [CRITICAL] Finding 1.1: SVG Interactive Seat Map DOM Collapse & Memory Bloat (10k–50k Seats)
- **File & Section:** `Plan/11_frontend_architecture_plan.md` — Section 3: Interactive SVG Seat Map Component (`seat-selection`)
- **Gap & Bottleneck:**  
  Section 3 claims to render "10,000+ seats smoothly using coordinate-mapped SVG circles and reactive Angular Signals" with the snippet:
  ```html
  <circle [class]="getSeatClass(seat)" />
  ```
  In browsers (Chrome, Safari, Firefox), mounting 10,000 to 50,000 SVG DOM elements inside an Angular template loop (`@for (seat of seats())`) consumes 250MB–500MB of DOM memory. Every pan/zoom interaction triggers layout recalculations and style re-evaluations across all 50,000 nodes, dropping rendering performance to < 5 FPS. Furthermore, calling `[class]="getSeatClass(seat)"` as a method inside the template evaluates 50,000 function calls per change detection cycle.
- **Fix Recommendation:**  
  1. Implement a **Two-Tier Level of Detail (LOD) Rendering Architecture**:
     - **LOD 0 (Overview / Zoom < 2.5x):** Render venue Sections / Zones as single SVG polygon paths with aggregate availability heatmaps (e.g. 30 section polygons total). Individual seat circles are NOT mounted in the DOM.
     - **LOD 1 (Section Detail / Zoom ≥ 2.5x):** Only mount individual seat elements for the currently visible section (200–800 seats maximum).
  2. Implement an **HTML5 Canvas 2D / WebGL Fallback Option** for continuous 50k seat rendering without DOM node overhead, using an R-Tree / QuadTree spatial index for $O(\log N)$ cursor hit-testing.
  3. Replace method binding `[class]="getSeatClass(seat)"` with pre-computed class properties or direct attribute manipulation.

```diff
--- a/Plan/11_frontend_architecture_plan.md
+++ b/Plan/11_frontend_architecture_plan.md
@@ -79,16 +79,48 @@
 ## 3. Interactive SVG Seat Map Component (`seat-selection`)
 
-Renders 10,000+ seats smoothly using coordinate-mapped SVG circles and reactive Angular Signals.
+Renders 10,000 to 50,000 seats smoothly using a Two-Tier Level-of-Detail (LOD) SVG architecture with Viewport Frustum Culling and Canvas fallback.
 
+```
+  [ Zoom Level < 2.5x: Macro View ]       [ Zoom Level >= 2.5x: Micro View ]
+  Render Section Polygons (~20-50 nodes)   Mount Seat Circles for Active Section Only (<800 nodes)
+  Availability Heatmap (Signal aggregate)  Granular Seat Selection & SignalR recoloring
+```
+
+### 3.1 Architectural Implementation Rules:
+1. **Viewport Culling:** Elements outside the current SVG `viewBox` bounding box are detached from the render tree.
+2. **Zero Template Method Calls:** Avoid `[class]="getSeatClass(seat)"`. Seat state is tracked via a flattened `Uint8Array` buffer or memoized seat signals to eliminate template function evaluations.
+3. **Pan & Zoom Transformation:** Native SVG `viewBox` coordinate manipulation via `PointerEvent` tracking, utilizing hardware acceleration with `will-change: transform`.
+4. **Canvas 2D Engine (Stadium Scale > 25k seats):** Renders seats directly into an offscreen `<canvas>` buffer; utilizes a flat QuadTree for sub-millisecond mouse/touch hit testing.
+
 ```
        [ SignalR Hub: /hubs/seats ]
                      │
          (SeatStatusChanged Event)
                      │
+                     ▼
+         [ RxJS bufferTime(100ms) ]  <-- Batches high-frequency bursts
+                     │
                      ▼
          [ Angular Signal State ]
-   seats = signal<Map<string, SeatUiState>>()
+   activeSectionSeats = signal<SeatViewModel[]>()
                      │
                      ▼
-   [ SVG Canvas: Zoom & Pan Transform ]
-   <circle [class]="getSeatClass(seat)" />
+   [ Virtualized SVG Section Viewport ]
+   @for (seat of activeSectionSeats(); track seat.id) {
+     <circle [attr.cx]="seat.x" [attr.cy]="seat.y" [attr.r]="seatRadius"
+             [attr.fill]="seat.colorHex" (click)="toggleSeat(seat.id)" />
+   }
 ```
```

---

### [HIGH] Finding 1.2: Angular Signal Reactivity Anti-Pattern with High-Frequency SignalR Updates
- **File & Section:** `Plan/11_frontend_architecture_plan.md` — Section 3: Interactive SVG Seat Map Component
- **Gap & Bottleneck:**  
  Section 3 defines the signal state as:
  ```typescript
  seats = signal<Map<string, SeatUiState>>()
  ```
  In JavaScript, mutating a Map in place (`seats().set(id, state)`) does not change object identity, so Angular Signals will not trigger change detection. To trigger an update, developers typically write:
  ```typescript
  this.seats.update(map => new Map(map.set(seatId, updatedState)));
  ```
  During a major ticket drop with 500+ concurrent users holding seats, the SignalR hub broadcasts 30 to 80 `SeatStatusChanged` events per second. Creating a new `Map` with 10,000–50,000 entries 50 times a second creates massive memory allocation, constant GC pauses, and triggers full template re-renders.
- **Fix Recommendation:**  
  1. Introduce an RxJS ingestion pipeline that buffers incoming SignalR messages with `bufferTime(100)` (batching updates into 100ms windows).
  2. Maintain a flat state store where updates are applied to the active section's granular signal array, or use direct DOM element reference manipulation via element ID (`#seat-${id}`) for color updates without re-evaluating the parent collection.

```diff
--- a/Plan/11_frontend_architecture_plan.md
+++ b/Plan/11_frontend_architecture_plan.md
@@ -89,7 +89,20 @@
                      ▼
          [ Angular Signal State ]
-   seats = signal<Map<string, SeatUiState>>()
+   // Batched reactive update service
+   private seatUpdateSubject = new Subject<SeatStatusUpdate>();
+   
+   ngOnInit() {
+     this.seatUpdateSubject.pipe(
+       bufferTime(100),
+       filter(batch => batch.length > 0),
+       takeUntilDestroyed(this.destroyRef)
+     ).subscribe(batch => {
+       this.applySeatStatusBatch(batch);
+     });
+   }
 ```

---

### [HIGH] Finding 1.3: Missing SignalR Reconnection & State Snapshot Resynchronization
- **File & Section:** `Plan/11_frontend_architecture_plan.md` — Section 1: Project Directory (`core/realtime/SignalRService`) & `Plan/08_realtime_concurrency_plan.md` — Section 3
- **Gap & Bottleneck:**  
  When mobile users switch networks (e.g. Wi-Fi to 5G) or experience momentary packet loss, the SignalR WebSocket connection drops. The specification provides no lifecycle handling for connection drops.
  When SignalR auto-reconnects, ASP.NET Core SignalR does **not** automatically replay events missed during the disconnect window, nor does it guarantee the client re-joined the event group. The user will be looking at a stale seat map (showing seats as "Available" that were already booked by others during the disconnection), leading to false clicks and frustrating 409 Conflict errors.
- **Fix Recommendation:**  
  Add explicit connection lifecycle management in `SignalRService`:
  1. On `onreconnected`, invoke hub method `JoinEvent(eventId)` and immediately issue a REST call to `GET /api/v1/events/{id}/seat-map/snapshot` to reconcile local state.
  2. Implement exponential backoff for reconnection.
  3. Ensure `ngOnDestroy` / `DestroyRef` executes `LeaveEvent(eventId)` and gracefully stops the hub connection.

```diff
--- a/Plan/11_frontend_architecture_plan.md
+++ b/Plan/11_frontend_architecture_plan.md
@@ -17,3 +17,29 @@
 │   └── realtime/               # SignalRService (Seat status connection manager)
+│       ├── seat-hub.service.ts # Connection lifecycle, retry backoff, group join
+│       └── seat-state.store.ts # Granular Signal state with snapshot reconciliation
+```
+
+### 1.1 SignalR Reconnection & Snapshot Sync Contract
+```typescript
+@Injectable({ providedIn: 'root' })
+export class SeatRealtimeService {
+  private hubConnection!: HubConnection;
+
+  public async initializeEventConnection(eventId: string): Promise<void> {
+    this.hubConnection = new HubConnectionBuilder()
+      .withUrl(`${environment.apiUrl}/hubs/seats`, { accessTokenFactory: () => this.auth.getAccessToken() })
+      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
+      .build();
+
+    this.hubConnection.onreconnected(async (connectionId) => {
+      // Re-join event hub group and resync state snapshot
+      await this.hubConnection.invoke('JoinEvent', { eventId });
+      await this.resyncSeatSnapshot(eventId);
+    });
+
+    await this.hubConnection.start();
+    await this.hubConnection.invoke('JoinEvent', { eventId });
+  }
+}
 ```

---

### [HIGH] Finding 1.4: WebRTC QR Scanner Missing Offline Roster Cache & Role Guard Misconfiguration
- **File & Section:** `Plan/11_frontend_architecture_plan.md` — Section 4: Mobile QR Check-In Scanner & `Plan/01_concert_requirement_spec.md` — Section 4.1 (Route #9)
- **Gap & Bottleneck:**  
  1. **Offline Fragility:** Stadium gates frequently suffer from cellular congestion and intermittent Wi-Fi. Section 4 specifies only live camera stream and sub-200ms scan latency, with no offline caching mechanism. If the network drops, staff cannot validate tickets.
  2. **Access Guard Lockout:** In `Plan/01` Section 4.1 line 921, the route `/organizer/events/:id/checkin` is guarded by `Organizer (Owner)`. However, ticket scanning is performed by event staff (`ROLE_ORG_STAFF` with claim `checkin.scan`). With the current guard specification, actual staff members assigned to check-in gates will receive `403 Forbidden`!
- **Fix Recommendation:**  
  1. Equip the scanner with an **Offline Roster Engine**:
     - Pre-download the event's encrypted ticket index into browser `IndexedDB` (using Dexie.js) when staff logs in before gates open.
     - Verify the ticket's HMAC-SHA256 / Ed25519 cryptographic signature locally against the event's public key (already specified in `Plan/09` Section 4).
     - Record admissions locally in an IndexedDB append-only ledger to prevent immediate local duplicate scans, and queue scans for background synchronization (`navigator.serviceWorker.sync` or RxJS sync worker).
  2. Update the route guard from `Organizer (Owner)` to `RoleGuard` allowing `Organizer` OR `OrgStaff` with claim `checkin.scan`.
  3. Ensure camera tracks (`MediaStreamTrack`) are explicitly stopped on component destruction (`ngOnDestroy`) to avoid camera hardware lock and device battery drain.

```diff
--- a/Plan/01_concert_requirement_spec.md
+++ b/Plan/01_concert_requirement_spec.md
@@ -921,1 +921,1 @@
-| 9 | Check-in Scanner | `/organizer/events/:id/checkin` | Organizer (Owner) | สแกน QR Code ผ่านกล้อง (ngx-scanner) |
+| 9 | Check-in Scanner | `/organizer/events/:id/checkin` | Organizer / OrgStaff (`checkin.scan`) | สแกน QR Code ผ่านกล้อง (@zxing/browser + IndexedDB offline mode) |
--- a/Plan/11_frontend_architecture_plan.md
+++ b/Plan/11_frontend_architecture_plan.md
@@ -113,3 +113,11 @@
 * **Instant Audio & Visual Feedback:**
   * ✅ **Green Screen + Chime Sound:** Valid ticket, admission granted.
   * ❌ **Red Screen + Buzzer Sound:** Duplicate scan or invalid signature.
+* **Offline-First Resilience:**
+  * Pre-caches event ticket hashes into client `IndexedDB` (Dexie.js).
+  * Validates cryptographic ticket signature locally when offline.
+  * Maintains local admission ledger to prevent duplicate scans across gates.
+  * Queues scans in IndexedDB and syncs to backend via Background Sync API upon reconnection.
+* **Hardware Management:**
+  * Explicitly terminates `MediaStreamTrack.stop()` in `ngOnDestroy` to release mobile camera hardware.
```

---

## 2. Design System Tokens & UX Wireframes

### [HIGH] Finding 2.1: WCAG 2.1 AA Contrast Failures on Brand & Accent Tokens
- **File & Section:** `Plan/10_uxui_design_system_plan.md` — Section 2.1: Core Palette & Section 5.1: Button System
- **Gap & Bottleneck:**  
  1. **Primary Brand `#6366F1` (Indigo-500):**  
     Used for Primary Buttons (`bg-brand text-white`) and Links.
     The relative luminance calculation yields a contrast ratio of **4.46:1** against `#FFFFFF`.
     WCAG 2.1 Success Criterion 1.4.3 (Contrast Minimum) requires at least **4.5:1** for standard body and button text (< 18pt / < 14pt bold). The default 14px button text fails WCAG 2.1 AA compliance.
  2. **Accent `#F59E0B` (Amber-500):**  
     Used for Badges, Warnings, and Held Seats.
     The contrast ratio of `#F59E0B` against `#FFFFFF` text is approximately **2.1:1**, severely failing readability standards.
  3. **Booked/Sold Out Seat Opacity (40% in Dark Mode):**  
     `#EF4444` at 40% opacity on `#171717` (Surface-900) yields a contrast ratio of **< 2.2:1**, making sold seat boundaries nearly invisible on dark mode displays.
- **Fix Recommendation:**  
  - Upgrade Primary Brand to **Indigo-600 (`#4F46E5`)**, which delivers a contrast ratio of **6.3:1** against `#FFFFFF`.
  - For Amber-500 badges, enforce dark text `text-neutral-950` (`#0A0A0A`) rather than white text, achieving a **9.8:1** contrast ratio.
  - Set Dark Mode Sold-Out seats to `surface-700` (`#404040`) fill with a `danger` (`#EF4444`) subtle boundary stroke rather than low-opacity red fill.

```diff
--- a/Plan/10_uxui_design_system_plan.md
+++ b/Plan/10_uxui_design_system_plan.md
@@ -31,3 +31,3 @@
-│  Primary (Brand)     ██████  #6366F1  Indigo-500  (CTA, Links, Focus)   │
-│  Primary Dark        ██████  #4338CA  Indigo-700  (Hover, Active)       │
+│  Primary (Brand)     ██████  #4F46E5  Indigo-600  (CTA, Links, Focus — WCAG 6.3:1) │
+│  Primary Dark        ██████  #3730A3  Indigo-800  (Hover, Active)       │
@@ -60,2 +60,2 @@
-        brand:   { DEFAULT: '#6366F1', dark: '#4338CA', light: '#A5B4FC' },
-        accent:  { DEFAULT: '#F59E0B', dark: '#D97706' },
+        brand:   { DEFAULT: '#4F46E5', dark: '#3730A3', light: '#818CF8' },
+        accent:  { DEFAULT: '#F59E0B', dark: '#D97706', text: '#0A0A0A' },
```

---

### [MEDIUM] Finding 2.2: Seat Map Inaccessibility for Colorblind Users (WCAG 1.4.1)
- **File & Section:** `Plan/10_uxui_design_system_plan.md` — Section 2.3 & Section 7.2; `Plan/11_frontend_architecture_plan.md` — Section 3.1
- **Gap & Bottleneck:**  
  Seat statuses are represented solely by color fills:
  - 🟢 Available: Emerald-500 (`#10B981`)
  - 🔴 Booked / Sold Out: Red-500 (`#EF4444`)
  - 🟡 Held (Self): Amber-500 (`#F59E0B`)
  - ⚪ Held (Other): Grey
  For users with red-green color blindness (protanopia and deuteranopia, affecting ~8% of males), Emerald-500 and Red-500 have almost indistinguishable hues. Relying purely on color fill violates WCAG 2.1 Guideline 1.4.1 (*"Color is not used as the only visual means of conveying information"*).
- **Fix Recommendation:**  
  Introduce secondary visual and geometric differentiators for seat states:
  - **Available:** Solid colored circle.
  - **Held (Self):** Solid amber circle with internal white countdown clock icon or pulsing border.
  - **Held (Other):** Dashed stroke outline with diagonal hatch pattern.
  - **Sold Out:** Solid neutral/muted circle with an embossed diagonal slash (`/`) or `X` icon.
  - Add explicit SVG `aria-label` attribute on each seat (e.g. `aria-label="Section A, Row 12, Seat 14, Available, ฿2,500"`).

---

### [MEDIUM] Finding 2.3: Contradictory Responsive Breakpoint Definitions
- **File & Section:** `Plan/10_uxui_design_system_plan.md` — Section 4.2: Layout Grid vs Section 8: Responsive Breakpoints & Behavior
- **Gap & Bottleneck:**  
  - Section 4.2 states:
    - *Desktop (≥1280px): 12 columns*
    - *Tablet (768–1279px): 8 columns*
    - *Mobile (< 768px): 4 columns*
  - Section 8 states:
    - *sm (Mobile): < 640px (4 cols)*
    - *md (Tablet): 640–1023px (8 cols)*
    - *lg (Desktop): 1024–1279px (12 cols)*
    - *xl (Wide): ≥ 1280px (12 cols)*
  Section 4.2 defines Mobile as `< 768px`, while Section 8 defines Mobile as `< 640px`. Furthermore, Section 8 introduces `lg` (1024–1279px) which is completely missing from Section 4.2. This causes major CSS inconsistencies when frontend engineers configure Tailwind container queries.
- **Fix Recommendation:**  
  Align Section 4.2 with Tailwind CSS default breakpoints and Section 8.

```diff
--- a/Plan/10_uxui_design_system_plan.md
+++ b/Plan/10_uxui_design_system_plan.md
@@ -140,4 +140,5 @@
-### 4.2 Layout Grid
-* **Desktop (≥1280px):** 12-column grid, gutter `24px`, max-width `1280px`, centered.
-* **Tablet (768-1279px):** 8-column grid, gutter `16px`.
-* **Mobile (< 768px):** 4-column grid, gutter `16px`, padding-x `16px`.
+### 4.2 Layout Grid (Standard Tailwind 3.4 Alignment)
+* **Wide / xl (≥1280px):** 12-column grid, gutter `24px`, max-width `1280px`, centered.
+* **Desktop / lg (1024-1279px):** 12-column grid, gutter `20px`.
+* **Tablet / md (768-1023px):** 8-column grid, gutter `16px`.
+* **Mobile / sm (< 768px):** 4-column grid, gutter `16px`, padding-x `16px`.
```

---

### [MEDIUM] Finding 2.4: Missing Wireframe Specifications for Virtual Waiting Room & Staff Scanner
- **File & Section:** `Plan/10_uxui_design_system_plan.md` — Section 7: Page-by-Page Layout Specifications
- **Gap & Bottleneck:**  
  Section 7 provides wireframes for Customer Discovery (7.1), Seat Map (7.2), and Organizer Dashboard (7.3). However, it omits layout specifications for:
  1. **Virtual Waiting Room (`/queue/:eventId`):** The highest-traffic screen in the application (handling up to 50,000 users concurrently).
  2. **Staff Mobile Scanner (`/organizer/events/:id/checkin`):** The primary gate operations screen.
  Without wireframe specifications, frontend developers lack guidance on queue progress indicators, token expiration timers, scanner viewfinders, and admission flash overlays.
- **Fix Recommendation:**  
  Add layout wireframes 7.4 (Virtual Waiting Room) and 7.5 (Staff Mobile Check-In Scanner) to `Plan/10`.

```diff
--- a/Plan/10_uxui_design_system_plan.md
+++ b/Plan/10_uxui_design_system_plan.md
@@ -399,2 +399,44 @@
 └──────┴──────────────────────────────────────────────────────────────────┘
 ```
+
+### 7.4 Customer Portal — Virtual Waiting Room (`/queue/:eventId`)
+```
+┌─────────────────────────────────────────────────────────────────────────┐
+│  [ TicketWorld Logo ]                                  Auto-refresh: ON │
+├─────────────────────────────────────────────────────────────────────────┤
+│                                                                         │
+│                      You are in the queue for                           │
+│                      Coldplay: Music of the Spheres                     │
+│                                                                         │
+│                    ┌─────────────────────────┐                          │
+│                    │   YOUR NUMBER: # 1,420  │                          │
+│                    └─────────────────────────┘                          │
+│                                                                         │
+│        [████████████████████████░░░░░░░░░░░░░░]  68% Ahead of you       │
+│                                                                         │
+│        Estimated wait time: ~4 minutes                                  │
+│        People ahead: 1,419 | Admitting: 100 users / 5s                  │
+│                                                                         │
+│        ⚠️ Do not refresh or close this tab. You will be automatically    │
+│           redirected to seat selection when your turn arrives.          │
+│                                                                         │
+└─────────────────────────────────────────────────────────────────────────┘
+```
+
+### 7.5 Staff Mobile QR Check-In Scanner (`/organizer/events/:id/checkin`)
+```
+┌─────────────────────────────────────────────────────────────────────────┐
+│  Gate 2 — West Entrance                     [ ⚡ Flash ] [ 🔄 Flip Cam ] │
+├─────────────────────────────────────────────────────────────────────────┤
+│                                                                         │
+│                     ┌─────────────────────┐                             │
+│                     │  [  VIEWFINDER  ]   │  <-- Sub-200ms QR detection │
+│                     │    Aim at QR Code   │                             │
+│                     └─────────────────────┘                             │
+│                                                                         │
+│  ADMISSION STATUS:                                                      │
+│  ┌───────────────────────────────────────────────────────────────────┐  │
+│  │  ✅ ADMITTED — VIP Zone A, Row 2, Seat 14                          │  │
+│  │  Attendee: John Doe | Scanned at: 18:42:10 (Offline Cache Valid)   │  │
+│  └───────────────────────────────────────────────────────────────────┘  │
+│  Total Admitted: 1,240 / 5,000 (24.8%)        [ Enter Code Manually ]   │
+└─────────────────────────────────────────────────────────────────────────┘
+```
```

---

## 3. QA & Performance Testing Strategy

### [CRITICAL] Finding 3.1: Flawed k6 Load Test Configuration Flagging 409 Conflict as Test Failure
- **File & Section:** `Plan/13_testing_qa_plan.md` — Section 5.1: Scenario: Hot On-Sale Seat Hold Race
- **Gap & Bottleneck:**  
  Lines 292–296 define the k6 thresholds:
  ```javascript
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.30'],
    'checks': ['rate>0.60'],
  }
  ```
  And the test function executes:
  ```javascript
  const seatId = `seat-${Math.floor(Math.random() * 100)}`; // 500 VUs fighting for 100 seats
  ```
  1. **False-Positive CI Failure:** When 500 concurrent VUs compete for 100 seats, at least 400 requests (~80%) will receive HTTP `409 Conflict`. In k6, any response with status code $\ge 400$ is automatically counted towards `http_req_failed`. Under this test, `http_req_failed` will reach ~0.80, which **fails** the `rate<0.30` threshold every time! The CI/CD pipeline will fail despite the system executing concurrency locking perfectly.
  2. **Sub-Optimal SLA Target:** The threshold `p(95)<500` (500ms) is too lenient. Redis RedLock distributed seat locking is designed to operate in under 20–50ms. A 500ms p95 latency indicates thread-pool starvation or database transaction locking bottlenecks.
- **Fix Recommendation:**  
  1. Use custom k6 metrics (`Rate` and `Trend`) that distinguish expected business conflicts (`409 Conflict`) from actual server errors (`500`, `502`, `504`).
  2. Tighten the SLA threshold for the seat hold endpoint to `p(95)<200` to guarantee high-concurrency throughput under traffic surges.

```diff
--- a/Plan/13_testing_qa_plan.md
+++ b/Plan/13_testing_qa_plan.md
@@ -275,6 +275,10 @@
 import http from 'k6/http';
-import { check, sleep } from 'k6';
+import { check, sleep } from 'k6';
+import { Rate, Trend } from 'k6/metrics';
+
+const systemErrors = new Rate('system_errors');
+const holdDuration = new Trend('seat_hold_duration');
 
 export const options = {
   scenarios: {
@@ -291,5 +295,6 @@
   thresholds: {
-    http_req_duration: ['p(95)<500'],        // 95th percentile < 500ms
-    http_req_failed: ['rate<0.30'],          // Allow up to 30% 409 Conflict (expected)
-    'checks': ['rate>0.60'],                 // 60%+ should succeed (not all — seats are limited)
+    seat_hold_duration: ['p(95)<200', 'p(99)<400'], // Strict SLA: p95 < 200ms
+    system_errors: ['rate<0.01'],                   // 5xx / timeouts must be < 1%
+    'checks{status:200_or_409}': ['rate>0.99'],     // 99%+ must be valid Hold (200) or Conflict (409)
   },
 };
@@ -311,7 +316,11 @@
   );
 
+  holdDuration.add(res.timings.duration);
+  const isExpectedResponse = res.status === 200 || res.status === 409;
+  systemErrors.add(res.status >= 500 || res.status === 0);
+
   check(res, {
     'valid response (200 or 409)': () => isExpectedResponse,
     'not rate limited (429)': (r) => r.status !== 429,
-  });
+  }, { status: '200_or_409' });
```

---

### [CRITICAL] Finding 3.2: Complete Omission of Payment Webhook Race Conditions from Test Suites
- **File & Section:** `Plan/13_testing_qa_plan.md` — Section 3.2: What to Integration Test
- **Gap & Bottleneck:**  
  `Plan/09` dedicates its core architecture to resolving 3 high-concurrency payment webhook race conditions:
  - Scenario 1: Clean Win (seat still held).
  - Scenario 2: Race Lost (seat acquired by someone else $\rightarrow$ auto-refund).
  - Scenario 3: Grace Window (seat expired but available: paid within 90s $\rightarrow$ confirm; paid $>90$s $\rightarrow$ auto-refund).
  However, in `Plan/13` Section 3.2, the integration tests only cover the basic happy path (`POST /seats/hold -> POST /bookings/{id}/checkout`). **None of the 3 race scenarios are included in the test plan!**
  If this reconciliation logic is not tested with automated integration tests, silent financial discrepancies (e.g. double-charging customers without issuing seats, or issuing seats without valid payments) will escape into production.
- **Fix Recommendation:**  
  Add concrete integration test cases in `tests/WebAPI.IntegrationTests/PaymentReconciliationTests.cs` using `WebApplicationFactory` and `Testcontainers`.

```diff
--- a/Plan/13_testing_qa_plan.md
+++ b/Plan/13_testing_qa_plan.md
@@ -212,2 +212,6 @@
 | EF Core Migrations | Database schema applies cleanly on empty DB |
+| `POST /webhooks/payment` (Scenario 1: Clean Win) | Webhook arrives while seat is Held -> Marks Booked, issues QR |
+| `POST /webhooks/payment` (Scenario 2: Race Lost) | Webhook arrives after seat held by User B -> Triggers auto-refund, User B keeps seat |
+| `POST /webhooks/payment` (Scenario 3A: Grace <=90s) | Webhook arrives 45s post-expiry while Available -> Reclaims seat, marks Booked |
+| `POST /webhooks/payment` (Scenario 3B: Grace >90s) | Webhook arrives 95s post-expiry -> Executes auto-refund, seat remains Available |
```

---

### [HIGH] Finding 3.3: Missing Security & Bypass Tests for Virtual Waiting Room Tokens
- **File & Section:** `Plan/13_testing_qa_plan.md` — Section 3.2 & Section 6
- **Gap & Bottleneck:**  
  `Plan/08` specifies that during traffic surges, direct access to `/seats/hold` must be guarded by an `X-WaitingRoom-Token` header.
  However, `Plan/13` has no test case verifying that:
  - Direct HTTP calls to `/api/v1/seats/hold` without `X-WaitingRoom-Token` return `403 Forbidden` / `429 Too Many Requests`.
  - Forged, tampered, or expired tokens (> 15 minutes) are rejected.
  - Rate limiting protects `/queue/position` heartbeat polling.
  Without automated test coverage, scalper bot bypasses can circumvent the queue entirely.
- **Fix Recommendation:**  
  Add test coverage in Section 3.2 for waiting room token verification middleware.

---

### [HIGH] Finding 3.4: Lack of Payment Gateway Auto-Refund Failure & Dead-Letter Queue (DLQ) Testing
- **File & Section:** `Plan/13_testing_qa_plan.md` — Section 3.2 & `Plan/09_payment_reconciliation_plan.md` — Section 3
- **Gap & Bottleneck:**  
  In Scenario 2 (Race Lost), the backend triggers `_refundService.ExecuteAutoRefundAsync()`.
  If Stripe/Omise returns an HTTP 500, network timeout, or connection reset during this call, the system must not swallow the error or leave the customer unrefunded. `Plan/13` contains zero test cases simulating payment gateway refund failures, idempotency retries, or routing to a Dead-Letter Queue (DLQ) with alert dispatch.
- **Fix Recommendation:**  
  Specify integration tests verifying Hangfire background retry policies with exponential backoff (3 attempts) and failure escalation to `admin_alerts` / Seq on refund exhaustion.

---

### [MEDIUM] Finding 3.5: Omission of SignalR WebSocket Load Testing Tooling
- **File & Section:** `Plan/13_testing_qa_plan.md` — Section 5.2: Key Load Test Scenarios
- **Gap & Bottleneck:**  
  Line 332 specifies:
  *`SignalR Connections: 2,000 concurrent | 120s | All receive seat updates within 1s`*
  However, standard k6 runs HTTP/HTTPS requests. Testing ASP.NET Core SignalR requires either `xk6-signalr` (a custom k6 binary extension supporting SignalR protocol handshakes and WebSockets) or specialized tooling like Crank / Artillery. `Plan/13` provides no script, tooling definition, or protocol handshake instructions for SignalR load testing.
- **Fix Recommendation:**  
  Explicitly specify `xk6-signalr` in Section 5 and provide a test harness script for connecting to `/hubs/seats`.

---

## 4. Sprint Delivery Roadmap & Dependencies

### [CRITICAL] Finding 4.1: Missing 12-Sprint Granular Breakdown & Omission of Testing Deliverables
- **File & Section:** `Plan/14_sprint_roadmap_plan.md` — Section 1 & Section 2
- **Gap & Bottleneck:**  
  `Plan/14` claims to define a 12-sprint delivery roadmap (2 weeks per sprint, 24 weeks total).
  However, Section 2 is broken down only into **6 high-level Phases**, without defining individual Sprints (Sprint 1, Sprint 2, ..., Sprint 12).
  Furthermore, testing deliverables (unit test suites, k6 load testing, security scanning, Playwright E2E) are **completely omitted** from the roadmap deliverables in all 6 phases. If QA and performance testing are not scheduled within the sprints, teams will defer load testing until the very end of the project.
- **Fix Recommendation:**  
  Expand Section 2 into a detailed, sprint-by-sprint delivery matrix (Sprints 1 through 12), explicitly allocating QA automation, k6 stress testing, and security hardening into each respective sprint.

```diff
--- a/Plan/14_sprint_roadmap_plan.md
+++ b/Plan/14_sprint_roadmap_plan.md
@@ -23,3 +23,43 @@
+### Sprint-by-Sprint Execution Matrix:
+* **Sprint 1 (Phase 1A):** Clean Architecture skeleton, Docker Compose environment, EF Core setup, User entity, Argon2id auth.
+* **Sprint 2 (Phase 1B):** JWT RS256 rotation, TOTP 2FA, RBAC/ABAC guards, Angular 18 base shell & Tailwind tokens. Architecture tests.
+* **Sprint 3 (Phase 2A):** Venues, Artists, Seat Layouts schema, Event lifecycle state machine. Organizer Event Wizard.
+* **Sprint 4 (Phase 2B):** SVG Seat Map Builder UI, Bookings & BookingItems core schema migration, Customer Event Discovery.
+* **Sprint 5 (Phase 3A):** Redis Virtual Waiting Room (ZADD queue, heartbeat, bypass token middleware). Waiting screen UI.
+* **Sprint 6 (Phase 3B):** RedLock 3s distributed seat lock, SignalR `/hubs/seats` hub, Hangfire seat expiry worker. **Deliverable: k6 500 VU seat-hold stress test validation.**
+* **Sprint 7 (Phase 4A):** Stripe & Omise gateway integration, checkout UI, HMAC QR ticket generator.
+* **Sprint 8 (Phase 4B):** Critical webhook race reconciliation engine (Scenarios 1-3), auto-refund service. **Deliverable: Testcontainers webhook race test suite.**
+* **Sprint 9 (Phase 5A):** Mobile WebRTC QR Scanner with IndexedDB offline roster cache and background sync.
+* **Sprint 10 (Phase 5B):** Ticket Transfer state machine with QR re-generation, Waitlist FCFS engine, Staff management UI.
+* **Sprint 11 (Phase 6A):** Organizer Payouts, Commission calculations, Admin Portal moderation desk, PDPA anonymization.
+* **Sprint 12 (Phase 6B - Hardening & Launch Gate):** Full system E2E Playwright tests, PCI-DSS SAQ A compliance verification, 50,000 VU Waiting Room load drill, Disaster Recovery simulation.
```

---

### [HIGH] Finding 4.2: Dependency Sequencing Flaw: Booking Entity Postponed After Concurrency Engine
- **File & Section:** `Plan/14_sprint_roadmap_plan.md` — Phase 3 vs Phase 4
- **Gap & Bottleneck:**  
  - In Phase 3 (Sprints 5–6), the team builds the high-concurrency seat locking engine (`POST /seats/hold`) and the Hangfire hold-expiry worker.
  - However, in Phase 3 Section 2, the `bookings` and `booking_items` tables have not been migrated yet! The roadmap postpones `bookings` and `payments` migrations to Phase 4 (Sprints 7–8).
  - This creates an architectural block: In `Plan/08` Section 4.1, the Hangfire seat expiry worker executes a query joining the `seats` table with `payments` (`LEFT JOIN payments p ON p.payment_intent_id = s.payment_intent_id`) to verify pending checkouts before releasing seats. If `bookings` and `payments` tables do not exist in Phase 3, the seat hold engine cannot compile, execute, or be load tested with k6 in Phase 3!
- **Fix Recommendation:**  
  Shift the EF Core schema migration for `bookings`, `booking_items`, and `payments` into Phase 2 (Sprint 4), establishing the relational persistence layer before implementing high-concurrency seat holds and expiry workers in Phase 3.

---

### [HIGH] Finding 4.3: Zero Allocation for Security Penetration Testing, PCI-DSS Audit & Hardening
- **File & Section:** `Plan/14_sprint_roadmap_plan.md` — Phase 6: Enterprise & Compliance
- **Gap & Bottleneck:**  
  Phase 6 (Sprints 11–12) packs major business features into the final month: Organizer Payouts, Full/Partial Refund Engine, PDPA Data Erasure, Audit Logging, and Platform Admin UI.
  There is **zero buffer** allocated for:
  - External vulnerability scanning (PCI-DSS ASV scan).
  - Third-party security penetration testing.
  - High-volume simulated ticket drop rehearsals.
  In enterprise ticketing projects, launching immediately after completing feature development without a dedicated hardening and compliance sprint guarantees production outages and regulatory exposure.
- **Fix Recommendation:**  
  Rebalance the sprint roadmap by moving Organizer Payouts and Admin Analytics into Sprint 11, and reserving Sprint 12 exclusively as a **Hardening, Compliance Gate, and Launch Rehearsal Sprint** (as detailed in the diff for Finding 4.1).

---

# Minor Corrections and Typos

### 1. Broken Markdown File Links (`Plan/concert.md`)
- **Files Affected:**  
  - `Plan/11_frontend_architecture_plan.md` (Line 5)  
  - `Plan/13_testing_qa_plan.md` (Line 5)  
  - `Plan/14_sprint_roadmap_plan.md` (Line 5)  
  - `Plan/08_realtime_concurrency_plan.md` (Line 5)  
  - `Plan/09_payment_reconciliation_plan.md` (Line 4)
- **Error:**  
  All five documents reference `[Plan/concert.md](file:///.../Plan/concert.md)`.  
  The file `Plan/concert.md` does not exist in the repository (it was replaced by `Plan/01_concert_requirement_spec.md`).
- **Correction:**  
  Update all occurrences to reference `Plan/01_concert_requirement_spec.md`.

---

### 2. QR Code Scanner Library Inconsistency Across Plans
- **Files Affected:**  
  - `Plan/01_concert_requirement_spec.md` (Line 921): Specifies `ngx-scanner`.  
  - `Plan/02_master_project_plan.md` (Line 187): Specifies `@zxing/browser`.  
  - `Plan/11_frontend_architecture_plan.md` (Line 112): Specifies `@zxing/browser or HTML5-QRCode`.
- **Error:**  
  Three different npm packages are specified across the documents. `ngx-scanner` is an older Angular wrapper that has compatibility issues with Angular 18 Zoneless/Signals.
- **Correction:**  
  Standardize on **`@zxing/browser`** across all plans for standalone Angular 18 compatibility.

---

### 3. Check-In Scanner Route Path Inconsistency
- **Files Affected:**  
  - `Plan/01_concert_requirement_spec.md` (Line 921): `/organizer/events/:id/checkin`  
  - `Plan/02_master_project_plan.md` (Line 187): `/organizer/checkin`  
  - `Plan/11_frontend_architecture_plan.md` (Line 37): `organizer/checkin-scanner`
- **Error:**  
  Route path differs between `/organizer/events/:id/checkin`, `/organizer/checkin`, and `checkin-scanner`.
- **Correction:**  
  Standardize on `/organizer/events/:id/checkin` (route scoped to specific event ID).

---

### 4. Missing Semantic Color Token for "Held (Other)"
- **File Affected:** `Plan/10_uxui_design_system_plan.md` — Section 2.3: Semantic Usage Map
- **Error:**  
  Section 2.3 defines tokens for *Available Seat* (`success`), *Held Seat (Self)* (`accent`), and *Booked/Sold Out* (`danger opacity 60%`), but omits *Held by Other Users*, which is referenced in Wireframe 7.2 and Plan/11 Section 3.1.
- **Correction:**  
  Add to Section 2.3 Semantic Usage Table:  
  `| Held by Other | surface-200 border surface-400 | surface-700 border surface-600 | ❌ ห้ามใช้สีเดียวกับ Available หรือ Self Hold |`

---

### 5. Obsolete Karma Test Runner Reference in Testing Plan
- **File Affected:** `Plan/13_testing_qa_plan.md` — Section 7: CI/CD Pipeline Integration (Line 384)
- **Error:**  
  `npm run test -- --watch=false` in Angular 18 defaults to Karma unless configured, but Angular 18 officially deprecates Karma.
- **Correction:**  
  Specify modern Angular test tooling: `ng test --builder=@angular-devkit/build-angular:application` with Vitest or Jest.
