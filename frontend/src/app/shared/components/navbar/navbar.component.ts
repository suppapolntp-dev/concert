import {
  Component,
  ElementRef,
  HostListener,
  computed,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

export interface NavLink {
  label: string;
  route: string;
  exact?: boolean;
  iconType: 'concerts' | 'organizer' | 'admin' | 'dev';
}

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <!-- Top Floating Navbar Container matching Neo-Mint Design System -->
    <header class="sticky top-0 z-40 w-full px-3 sm:px-6 pt-3 pb-2 select-none">
      <div
        class="max-w-7xl mx-auto h-16 bg-white/95 backdrop-blur-md border border-slate-200/80 rounded-2xl sm:rounded-3xl shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] px-3 sm:px-5 flex items-center justify-between transition-all"
      >
        <!-- 1. Left: Brand Mark (Squircle matching Sidebar) -->
        <div class="flex items-center gap-3 shrink-0">
          <a
            routerLink="/"
            class="flex items-center gap-2.5 group focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 rounded-xl px-1 cursor-pointer"
          >
            <!-- Emerald Gradient Squircle Icon (Identical to Sidebar) -->
            <div
              class="w-10 h-10 rounded-2xl bg-gradient-to-br from-[#22C55E] to-[#16A34A] text-white flex items-center justify-center font-black shrink-0 shadow-sm shadow-emerald-500/25 group-hover:scale-105 group-hover:shadow-md transition-all"
            >
              <svg class="w-5 h-5 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5" />
              </svg>
            </div>

            <div class="min-w-0 leading-tight">
              <b class="font-black text-base tracking-tight text-[#131b2e] group-hover:text-emerald-700 transition-colors block">TicketWorld</b>
              <span class="text-[11px] text-slate-500 font-medium truncate block">Live Experience & Operations</span>
            </div>
          </a>
        </div>

        <!-- 2. Middle: Navigation Links (Clean Pill Track, Zero Badges, Pro SVG Icons) -->
        <nav
          class="hidden md:flex items-center p-1 bg-slate-100/80 rounded-2xl border border-slate-200/50"
          aria-label="Desktop primary navigation"
        >
          @for (link of navLinks; track link.route) {
            <a
              [routerLink]="link.route"
              routerLinkActive="active-nav-pill"
              [routerLinkActiveOptions]="{ exact: link.exact ?? false }"
              class="nav-link-pill flex items-center gap-2 h-9 px-3.5 rounded-xl text-xs font-semibold text-slate-600 hover:text-slate-900 hover:bg-white/60 transition-all cursor-pointer relative"
            >
              <ng-container *ngTemplateOutlet="linkIcon; context: { type: link.iconType }"></ng-container>
              <span>{{ link.label }}</span>
            </a>
          }
        </nav>

        <!-- 3. Right: Command Search, Auth / Profile, Mobile Toggle -->
        <div class="flex items-center gap-2 sm:gap-2.5 shrink-0">
          <!-- Command Palette Trigger (Identical styling to Sidebar) -->
          <button
            type="button"
            (click)="openPalette()"
            class="hidden lg:flex items-center gap-2.5 h-10 px-3 bg-slate-100/90 hover:bg-slate-200/70 rounded-xl text-slate-500 hover:text-slate-900 transition-colors shadow-[inset_0_0_0_1px_rgba(15,23,42,0.04)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 cursor-pointer"
            aria-label="Search events"
          >
            <svg class="w-4 h-4 text-slate-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="11" cy="11" r="7" />
              <path d="m21 21-4.3-4.3" />
            </svg>
            <span class="text-xs font-medium text-slate-500">Search events...</span>
            <kbd class="font-sans text-[10px] font-medium text-slate-400 border border-slate-200/90 bg-white/90 rounded px-1.5 py-0.5 shadow-2xs">Ctrl K</kbd>
          </button>

          <!-- Compact Search Icon for medium screens -->
          <button
            type="button"
            (click)="openPalette()"
            class="lg:hidden flex md:flex items-center justify-center w-10 h-10 rounded-xl bg-slate-100/90 hover:bg-slate-200/80 text-slate-500 hover:text-slate-900 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 cursor-pointer"
            aria-label="Search events"
          >
            <svg class="w-4 h-4 text-slate-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="11" cy="11" r="7" />
              <path d="m21 21-4.3-4.3" />
            </svg>
          </button>

          <!-- Auth Status & Controls -->
          @if (auth.isAuthenticated()) {
            <!-- My Tickets Link -->
            <a
              routerLink="/my-tickets"
              routerLinkActive="bg-[#D7F4DF] text-[#006e2f] border-[#22c55e]/30"
              class="hidden sm:inline-flex items-center gap-1.5 h-10 px-3.5 rounded-xl border border-slate-200/90 bg-white text-slate-700 hover:bg-slate-50 text-xs font-semibold shadow-2xs transition-all cursor-pointer"
            >
              <svg class="w-4 h-4 text-[#22C55E]" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <rect x="2" y="6" width="20" height="12" rx="2" />
                <path d="M12 12h.01M16 12h.01M8 12h.01" />
              </svg>
              <span>My Tickets</span>
            </a>

            <!-- User Profile Avatar with Popover -->
            <div class="relative">
              <button
                type="button"
                (click)="toggleProfilePopover($event)"
                class="w-10 h-10 rounded-2xl flex items-center justify-center relative hover:bg-slate-100 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 cursor-pointer"
                aria-haspopup="menu"
                aria-label="Open profile menu"
              >
                <div class="w-8 h-8 rounded-xl bg-slate-900 text-white flex items-center justify-center font-bold text-xs shrink-0 relative shadow-2xs">
                  {{ userInitials() }}
                  <span class="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full bg-[#22C55E] ring-2 ring-white"></span>
                </div>
              </button>

              <!-- Profile Popover -->
              @if (isProfileOpen()) {
                <div
                  class="pop absolute z-50 right-0 mt-2 w-64 p-1.5 bg-white border border-slate-200/90 rounded-2xl shadow-[0_16px_36px_-12px_rgba(15,23,42,0.2)] animate-in fade-in zoom-in-95 duration-150"
                  role="menu"
                  (click)="$event.stopPropagation()"
                >
                  <div class="p-2.5 border-b border-slate-100 mb-1.5 text-xs text-slate-500">
                    <b class="block font-semibold text-sm text-slate-900">{{ userName() }}</b>
                    <span class="text-[11px] text-slate-400 truncate block">{{ userEmail() }}</span>
                  </div>
                  <a
                    routerLink="/my-tickets"
                    (click)="isProfileOpen.set(false)"
                    class="flex items-center gap-2.5 w-full h-9 px-2.5 rounded-xl text-xs font-medium text-slate-700 hover:text-slate-900 hover:bg-slate-100 transition-colors"
                    role="menuitem"
                  >
                    <svg class="w-4 h-4 text-emerald-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                      <rect x="2" y="6" width="20" height="12" rx="2" />
                      <circle cx="12" cy="12" r="2" />
                    </svg>
                    <span>My Ticket Wallet</span>
                  </a>
                  <a
                    routerLink="/organizer"
                    (click)="isProfileOpen.set(false)"
                    class="flex items-center gap-2.5 w-full h-9 px-2.5 rounded-xl text-xs font-medium text-slate-700 hover:text-slate-900 hover:bg-slate-100 transition-colors"
                    role="menuitem"
                  >
                    <svg class="w-4 h-4 text-slate-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                      <rect x="3" y="3" width="7" height="9" rx="1.5"/><rect x="14" y="3" width="7" height="5" rx="1.5"/><rect x="14" y="12" width="7" height="9" rx="1.5"/><rect x="3" y="16" width="7" height="5" rx="1.5"/>
                    </svg>
                    <span>Organizer Portal</span>
                  </a>
                  <a
                    routerLink="/developer"
                    (click)="isProfileOpen.set(false)"
                    class="flex items-center gap-2.5 w-full h-9 px-2.5 rounded-xl text-xs font-medium text-amber-700 hover:bg-amber-50 transition-colors"
                    role="menuitem"
                  >
                    <svg class="w-4 h-4 text-amber-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                      <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/>
                    </svg>
                    <span>Developer Workbench</span>
                  </a>
                  <button
                    type="button"
                    (click)="onLogout()"
                    class="flex items-center gap-2.5 w-full h-9 px-2.5 rounded-xl text-xs font-medium text-rose-600 hover:bg-rose-50 transition-colors cursor-pointer"
                    role="menuitem"
                  >
                    <svg class="w-4 h-4 text-rose-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                      <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9"/>
                    </svg>
                    <span>Log Out</span>
                  </button>
                </div>
              }
            </div>
          } @else {
            <a
              routerLink="/organizer"
              class="hidden sm:inline-flex items-center justify-center h-10 px-3.5 text-xs font-semibold text-slate-700 hover:text-slate-900 hover:bg-slate-100 rounded-xl transition-colors cursor-pointer"
            >
              Organizer Hub
            </a>
            <a
              routerLink="/organizer"
              class="inline-flex items-center justify-center h-10 px-4 text-xs font-bold text-white bg-slate-900 hover:bg-slate-800 rounded-xl shadow-sm hover:shadow transition-all cursor-pointer"
            >
              Sign In
            </a>
          }

          <!-- Mobile Hamburger Toggle -->
          <button
            type="button"
            (click)="toggleMobileMenu()"
            class="md:hidden flex items-center justify-center w-10 h-10 rounded-xl text-slate-600 hover:text-slate-900 hover:bg-slate-100 transition-colors cursor-pointer"
            aria-label="Toggle mobile menu"
          >
            @if (isMobileMenuOpen()) {
              <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            } @else {
              <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="3" y1="12" x2="21" y2="12" />
                <line x1="3" y1="6" x2="21" y2="6" />
                <line x1="3" y1="18" x2="21" y2="18" />
              </svg>
            }
          </button>
        </div>
      </div>

      <!-- Mobile Dropdown Menu -->
      @if (isMobileMenuOpen()) {
        <div
          class="md:hidden mt-2 max-w-7xl mx-auto bg-white border border-slate-200/90 rounded-2xl shadow-xl p-3 animate-in fade-in slide-in-from-top-2 duration-150"
        >
          <div class="space-y-1">
            @for (link of navLinks; track link.route) {
              <a
                [routerLink]="link.route"
                (click)="isMobileMenuOpen.set(false)"
                routerLinkActive="bg-[#D7F4DF] text-[#006e2f] font-bold"
                [routerLinkActiveOptions]="{ exact: link.exact ?? false }"
                class="flex items-center gap-3 h-11 px-3 rounded-xl text-sm font-medium text-slate-700 hover:bg-slate-100 transition-colors"
              >
                <div class="w-7 h-7 rounded-lg bg-slate-100 flex items-center justify-center text-slate-500">
                  <ng-container *ngTemplateOutlet="linkIcon; context: { type: link.iconType }"></ng-container>
                </div>
                <span>{{ link.label }}</span>
              </a>
            }
          </div>

          <div class="pt-3 mt-3 border-t border-slate-100 flex flex-col gap-2">
            @if (auth.isAuthenticated()) {
              <button
                (click)="onLogout()"
                class="w-full h-10 px-3 rounded-xl text-xs font-semibold text-rose-600 bg-rose-50 flex items-center justify-center gap-2"
              >
                Log Out
              </button>
            } @else {
              <a
                routerLink="/organizer"
                (click)="isMobileMenuOpen.set(false)"
                class="w-full h-10 px-3 rounded-xl text-xs font-bold text-white bg-slate-900 flex items-center justify-center"
              >
                Sign In
              </a>
            }
          </div>
        </div>
      }
    </header>

    <!-- Command Palette Modal (Same architecture as Sidebar) -->
    @if (isPaletteOpen()) {
      <div
        class="fixed inset-0 z-50 flex items-start justify-center pt-[14vh] bg-slate-900/40 backdrop-blur-sm animate-in fade-in duration-150"
        (click)="closePalette()"
      >
        <div
          class="w-[min(520px,92vw)] bg-white border border-slate-200 rounded-2xl shadow-[0_30px_80px_-20px_rgba(0,0,0,0.35)] overflow-hidden"
          (click)="$event.stopPropagation()"
        >
          <div class="relative border-b border-slate-200 px-4 flex items-center">
            <svg class="w-5 h-5 text-slate-400 mr-2" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="11" cy="11" r="7" /><path d="m21 21-4.3-4.3" />
            </svg>
            <input
              type="text"
              #paletteInput
              [value]="paletteQuery()"
              (input)="onPaletteInput($event)"
              placeholder="Search concerts, venues, or tools..."
              class="w-full h-14 bg-transparent outline-none text-sm text-slate-900 placeholder:text-slate-400 font-medium"
            />
          </div>

          <div class="max-h-72 overflow-y-auto p-2 space-y-1">
            @for (action of filteredPaletteActions(); track action.label) {
              <div
                (click)="runPaletteAction(action)"
                class="flex items-center justify-between h-10 px-3 rounded-xl text-xs font-medium text-slate-700 hover:bg-slate-100 cursor-pointer transition-colors"
              >
                <span>{{ action.label }}</span>
                <kbd class="text-[10px] text-slate-400 bg-slate-100 border border-slate-200 px-1.5 py-0.5 rounded font-mono">
                  {{ action.shortcut }}
                </kbd>
              </div>
            }
            @if (filteredPaletteActions().length === 0) {
              <div class="p-4 text-center text-xs text-slate-400">No results found</div>
            }
          </div>
        </div>
      </div>
    }

    <!-- Template for Navigation Icons (Professional Clean SVG icons, No Emoji) -->
    <ng-template #linkIcon let-type="type">
      @switch (type) {
        @case ('concerts') {
          <svg class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <path d="M6 2 3 6v13a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><path d="M3 6h18"/><path d="M16 10a4 4 0 0 1-8 0"/>
          </svg>
        }
        @case ('organizer') {
          <svg class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <rect x="3" y="3" width="7" height="9" rx="1.5"/><rect x="14" y="3" width="7" height="5" rx="1.5"/><rect x="14" y="12" width="7" height="9" rx="1.5"/><rect x="3" y="16" width="7" height="5" rx="1.5"/>
          </svg>
        }
        @case ('admin') {
          <svg class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
          </svg>
        }
        @case ('dev') {
          <svg class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="16 18 22 12 16 6" />
            <polyline points="8 6 2 12 8 18" />
          </svg>
        }
        @default {
          <svg class="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="9"/>
          </svg>
        }
      }
    </ng-template>
  `,
  styles: [
    `
      /* Neo-Mint Active Pill Highlight matching Sidebar token #D7F4DF & #006e2f */
      .active-nav-pill {
        background-color: #ffffff !important;
        color: #006e2f !important;
        box-shadow: 0 1px 3px rgba(0, 110, 47, 0.08), inset 0 0 0 1px rgba(34, 197, 94, 0.3) !important;
      }
      .active-nav-pill svg {
        color: #006e2f !important;
      }
    `
  ]
})
export class NavbarComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  isMobileMenuOpen = signal<boolean>(false);
  isProfileOpen = signal<boolean>(false);
  isPaletteOpen = signal<boolean>(false);
  paletteQuery = signal<string>('');

  paletteInput = viewChild<ElementRef<HTMLInputElement>>('paletteInput');

  readonly userName = computed(() => this.auth.currentUser()?.fullName ?? 'Ajendra S.');
  readonly userEmail = computed(() => this.auth.currentUser()?.email ?? 'organizer@ticketworld.io');
  readonly userInitials = computed(() => {
    const name = this.userName();
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  });

  // Clean Navigation Links - Badges removed completely like Sidebar
  readonly navLinks: NavLink[] = [
    { label: 'Concerts & Events', route: '/', exact: true, iconType: 'concerts' },
    { label: 'Organizer Portal', route: '/organizer', iconType: 'organizer' },
    { label: 'Admin Ops', route: '/admin', iconType: 'admin' },
    { label: 'Dev Workbench', route: '/developer', iconType: 'dev' }
  ];

  filteredPaletteActions = computed(() => {
    const q = this.paletteQuery().toLowerCase();
    const actions = [
      { label: 'Browse Concerts & Live Events', route: '/', shortcut: '1' },
      { label: 'Organizer Dashboard & Management', route: '/organizer', shortcut: '2' },
      { label: 'Event Creation & Seating', route: '/organizer/events', shortcut: '3' },
      { label: 'Admin Operations', route: '/admin', shortcut: '4' },
      { label: 'Developer Workbench', route: '/developer', shortcut: 'D' },
      { label: 'Customer Ticket Wallet', route: '/my-tickets', shortcut: 'T' }
    ];
    if (!q) return actions;
    return actions.filter(a => a.label.toLowerCase().includes(q));
  });

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent) {
    const isMeta = event.ctrlKey || event.metaKey;
    const key = event.key.toLowerCase();

    if (isMeta && key === 'b') {
      // reserved
    } else if (isMeta && key === 'k') {
      event.preventDefault();
      this.openPalette();
    } else if (key === 'escape') {
      this.closePalette();
      this.isProfileOpen.set(false);
      this.isMobileMenuOpen.set(false);
    }
  }

  @HostListener('document:click')
  onDocumentClick() {
    this.isProfileOpen.set(false);
  }

  toggleProfilePopover(event: MouseEvent) {
    event.stopPropagation();
    this.isProfileOpen.update(v => !v);
  }

  toggleMobileMenu() {
    this.isMobileMenuOpen.update(v => !v);
  }

  openPalette() {
    this.isPaletteOpen.set(true);
    this.paletteQuery.set('');
    setTimeout(() => {
      this.paletteInput()?.nativeElement.focus();
    }, 50);
  }

  closePalette() {
    this.isPaletteOpen.set(false);
  }

  onPaletteInput(event: Event) {
    const val = (event.target as HTMLInputElement).value;
    this.paletteQuery.set(val);
  }

  runPaletteAction(action: { route: string }) {
    this.closePalette();
    this.router.navigateByUrl(action.route);
  }

  onLogout() {
    this.isProfileOpen.set(false);
    this.isMobileMenuOpen.set(false);
    this.auth.logout();
  }
}
