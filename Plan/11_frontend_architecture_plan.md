# 🎨 Frontend Architecture & Multi-Portal Plan

> **Framework:** Angular 18 (Standalone Components, Signals, Zoneless/RxJS) + Tailwind CSS  
> **Portals:** Customer Portal (`/`), Organizer Portal (`/organizer`), Platform Admin (`/admin`)  
> **Source Spec:** [Plan/concert.md](file:///c:/Users/wwwwww/Documents/project-test/concert/Plan/concert.md) (Section 4)

---

## 1. Project Directory & Multi-Portal Topology

```
src/app/
├── core/                       # Singleton services, guards, interceptors
│   ├── auth/                   # AuthService, TokenService, 2FA handler
│   ├── guards/                 # AuthGuard, RoleGuard, MfaGuard
│   ├── interceptors/           # JwtInterceptor, ErrorInterceptor, CsrfInterceptor
│   └── realtime/               # SignalRService (Seat status connection manager)
│
├── shared/                     # Reusable UI components & directives
│   ├── components/             # Navbar, Footer, Modal, Toast, LoadingSpinner
│   ├── directives/             # HasPermissionDirective (*hasPermission="'events.create'")
│   └── pipes/                  # CurrencyFormatPipe, LocalizedDatePipe
│
└── portals/
    ├── customer/               # Customer Portal (Public / Buyer)
    │   ├── discovery/          # Event Catalog, Search, Artist Profile
    │   ├── waiting-room/       # Virtual Queue Screen with animated progress
    │   ├── seat-selection/     # Interactive SVG Seat Map Viewer
    │   ├── checkout/           # Payment Intent & Confirmation
    │   └── my-tickets/         # Ticket wallet, QR Code view, Transfer modal
    │
    ├── organizer/              # Organizer & Staff Portal
    │   ├── dashboard/          # Sales analytics & revenue graphs
    │   ├── event-management/   # Create / Edit Event, Tier pricing
    │   ├── seat-map-builder/   # Drag-and-drop SVG layout designer
    │   ├── staff-management/   # Invite staff & scoped permissions
    │   └── checkin-scanner/    # Mobile WebRTC QR Code Scanner
    │
    └── admin/                  # Platform Admin Portal
        ├── event-moderation/   # Approve / Reject organizer events
        ├── finance-payouts/    # Organizer payout review & bank transfers
        ├── audit-logs/         # Security audit trail viewer
        └── system-health/      # APM telemetry & Redis/DB monitor
```

---

## 2. Authentication, Guards & Security

### 2.1 Route Guard Strategy
```typescript
export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const expectedRoles = route.data['roles'] as string[];

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  if (!authService.hasAnyRole(expectedRoles)) {
    return router.createUrlTree(['/unauthorized']);
  }

  return true;
};
```

### 2.2 Fine-Grained Permission Directive (`*hasPermission`)
```html
<!-- Visible only to Organizer owners or Admin, hidden for basic staff -->
<button *hasPermission="'finance.payout:request'" (click)="requestPayout()">
  Request Payout
</button>
```

---

## 3. Interactive SVG Seat Map Component (`seat-selection`)

Renders 10,000+ seats smoothly using coordinate-mapped SVG circles and reactive Angular Signals.

```
       [ SignalR Hub: /hubs/seats ]
                     │
         (SeatStatusChanged Event)
                     │
                     ▼
         [ Angular Signal State ]
   seats = signal<Map<string, SeatUiState>>()
                     │
                     ▼
   [ SVG Canvas: Zoom & Pan Transform ]
   <circle [class]="getSeatClass(seat)" />
```

### 3.1 Key Features
* **Color States:**
  * 🟢 **Available:** Clickable, adds to user selection (max 6 seats).
  * 🟡 **Held (Self):** Yellow with ticking 10-minute countdown.
  * ⚪ **Held (Other):** Disabled / greyed out.
  * 🔴 **Booked / Sold Out:** Non-interactive red.
* **Performance:** Uses CSS transforms for high-performance pan/zoom; avoids heavy DOM reflows.

---

## 4. Mobile QR Check-In Scanner (`organizer/checkin-scanner`)

Designed for staff mobile devices at venue entry gates.

* **Camera Stream:** WebRTC `navigator.mediaDevices.getUserMedia`.
* **Scanning Engine:** `@zxing/browser` or HTML5-QRCode with sub-200ms scan latency.
* **Instant Audio & Visual Feedback:**
  * ✅ **Green Screen + Chime Sound:** Valid ticket, admission granted.
  * ❌ **Red Screen + Buzzer Sound:** Duplicate scan or invalid signature.
