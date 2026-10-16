import {
  Component,
  ElementRef,
  HostListener,
  computed,
  effect,
  input,
  model,
  output,
  signal,
  viewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

export interface NavItem {
  id: string;
  label: string;
  shortLabel: string;
  route?: string;
  exact?: boolean;
  iconType: 'dashboard' | 'events' | 'seat-map' | 'checkin' | 'staff' | 'custom' | 'settings' | 'overview';
}

export interface NavGroup {
  groupName: string;
  items: NavItem[];
}

const DEFAULT_ORGANIZER_NAV_GROUPS: NavGroup[] = [
  {
    groupName: 'Workspace',
    items: [
      {
        id: 'dashboard',
        label: 'Overview',
        shortLabel: 'Overview',
        route: '/organizer',
        exact: true,
        iconType: 'dashboard'
      },
      {
        id: 'events',
        label: 'Event Management',
        shortLabel: 'Events',
        route: '/organizer/events',
        iconType: 'events'
      },
      {
        id: 'seat-map',
        label: 'Seat Map Builder',
        shortLabel: 'Seats',
        route: '/organizer/seat-map',
        iconType: 'seat-map'
      }
    ]
  },
  {
    groupName: 'Operations',
    items: [
      {
        id: 'checkin',
        label: 'Check-in Scanner',
        shortLabel: 'Scanner',
        route: '/organizer/checkin',
        iconType: 'checkin'
      },
      {
        id: 'staff',
        label: 'Staff Permissions',
        shortLabel: 'Staff',
        route: '/organizer/staff',
        iconType: 'staff'
      }
    ]
  }
];

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <!-- Mobile Backdrop -->
    @if (isMobileOpen()) {
      <div
        class="fixed inset-0 z-40 bg-black/40 backdrop-blur-sm md:hidden transition-opacity"
        (click)="closeMobile()"
        aria-hidden="true"
      ></div>
    }

    <!-- 
      SIDEBAR CONTAINER
      - Neo-Mint Ticketing & Operations Design System
      - Dual state: 264px expanded <-> 72px collapsed rail
      - Grounded geometry & precise vertical alignment
      - Neo-Mint brand tokens: Canvas (#E8F1EB), Primary (#006e2f), Primary-Container (#22C55E), Mint-Tint (#D7F4DF)
    -->
    <aside
      #sidebarAside
      id="side"
      [class.min]="isCollapsed()"
      [class.translate-x-0]="isMobileOpen()"
      [class.-translate-x-full]="!isMobileOpen()"
      class="fixed inset-y-0 left-0 z-40 md:sticky md:top-3 md:m-3 md:h-[calc(100dvh-24px)] flex flex-col bg-white border border-slate-200/80 rounded-3xl shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] transition-[width,transform] duration-300 ease-[cubic-bezier(0.32,0.72,0,1)] select-none md:translate-x-0 shrink-0 min-w-0"
      [class.px-2]="isCollapsed()"
      [class.py-3]="isCollapsed()"
      [class.p-2.5]="!isCollapsed()"
      [style.width]="isCollapsed() ? '72px' : '264px'"
      [style.min-width]="isCollapsed() ? '72px' : '264px'"
      [style.max-width]="isCollapsed() ? '72px' : '264px'"
      aria-label="Main navigation"
    >
      <!-- Top Section -->
      <div class="flex flex-col min-h-0 flex-1">
        <!-- 1. Header: Brand Logo & Collapse Toggle -->
        <div
          class="h-12 flex items-center shrink-0 mb-2"
          [class.justify-center]="isCollapsed()"
          [class.px-1]="!isCollapsed()"
          [class.justify-between]="!isCollapsed()"
        >
          @if (isCollapsed()) {
            <!-- Collapsed Rail Logo (Centered, Click to Expand) -->
            <button
              type="button"
              (click)="toggleCollapse()"
              class="w-11 h-11 rounded-2xl bg-gradient-to-br from-[#22C55E] to-[#16A34A] text-white flex items-center justify-center font-black shadow-sm shadow-emerald-500/25 hover:shadow-md hover:scale-[1.03] transition-all relative focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 cursor-pointer"
              data-tip="Expand sidebar (Ctrl+B)"
              aria-label="Expand sidebar"
            >
              <svg class="w-5 h-5 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5" />
              </svg>
            </button>
          } @else {
            <!-- Expanded Brand Link & Logo -->
            <a
              [routerLink]="previewMode() ? null : '/organizer'"
              (click)="onBrandClick()"
              class="flex items-center gap-2.5 min-w-0 group focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 rounded-xl px-1 cursor-pointer"
            >
              <!-- TicketWorld Neo-Mint Brand Icon -->
              <div class="w-8 h-8 rounded-xl bg-gradient-to-br from-[#22C55E] to-[#16A34A] text-white flex items-center justify-center font-black shrink-0 shadow-sm shadow-emerald-500/25 group-hover:scale-105 transition-transform">
                <svg class="w-5 h-5 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5" />
                </svg>
              </div>

              <div class="min-w-0 leading-tight">
                <b class="block font-bold text-sm tracking-tight text-[#131b2e] truncate">TicketWorld</b>
                <span class="text-[11px] text-slate-500 font-medium truncate block">{{ hubLabel() }}</span>
              </div>
            </a>

            <!-- Collapse Toggle Button -->
            <button
              type="button"
              (click)="toggleCollapse()"
              class="hidden md:flex items-center justify-center w-8 h-8 rounded-lg text-slate-400 hover:text-slate-800 hover:bg-slate-100 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 shrink-0 cursor-pointer"
              aria-label="Collapse sidebar (Ctrl+B)"
              title="Collapse sidebar (Ctrl+B)"
            >
              <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="3" y="3" width="18" height="18" rx="4" />
                <path d="M9 3v18" />
                <path d="m14 9-3 3 3 3" />
              </svg>
            </button>

            <!-- Mobile Close Button -->
            <button
              type="button"
              (click)="closeMobile()"
              class="md:hidden ml-auto w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:text-slate-800 hover:bg-slate-100 cursor-pointer"
              aria-label="Close menu"
            >
              <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>
          }
        </div>

        <!-- 2. Search Trigger (Command Palette) -->
        @if (isCollapsed()) {
          <button
            type="button"
            (click)="openPalette()"
            class="w-11 h-11 mx-auto my-1.5 rounded-2xl bg-slate-100/90 hover:bg-slate-200/80 text-slate-500 hover:text-slate-900 flex items-center justify-center transition-colors relative focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 cursor-pointer"
            data-tip="Search (Ctrl+K)"
            aria-label="Search"
          >
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="11" cy="11" r="7" />
              <path d="m21 21-4.3-4.3" />
            </svg>
          </button>
        } @else {
          <button
            type="button"
            (click)="openPalette()"
            class="relative flex items-center gap-2.5 w-full h-10 px-1.5 my-1.5 bg-slate-100/90 hover:bg-slate-200/70 rounded-xl text-slate-500 hover:text-slate-900 transition-colors shadow-[inset_0_0_0_1px_rgba(15,23,42,0.04)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 text-left cursor-pointer"
          >
            <div class="w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-slate-400">
              <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="11" cy="11" r="7" />
                <path d="m21 21-4.3-4.3" />
              </svg>
            </div>
            <span class="text-xs font-medium text-slate-500">Search pages...</span>
            <span class="ml-auto pr-1">
              <kbd class="font-sans text-[10px] font-medium text-slate-400 border border-slate-200/90 bg-white/90 rounded px-1.5 py-0.5 shadow-2xs">Ctrl K</kbd>
            </span>
          </button>
        }

        <!-- 3. Navigation List (Badges completely removed) -->
        <nav
          #navContainer
          id="nav"
          class="relative flex-1 overflow-y-auto overflow-x-hidden custom-scrollbar space-y-2 mt-1"
          aria-label="Main navigation links"
        >
          @for (group of effectiveNavGroups(); track group.groupName; let gIdx = $index) {
            <div class="space-y-1">
              @if (!isCollapsed()) {
                <div class="h-6 px-3 flex items-center text-[10px] font-bold tracking-wider uppercase text-slate-400 mt-2 mb-0.5">
                  {{ group.groupName }}
                </div>
              } @else {
                <div class="h-px bg-slate-200/80 my-2 mx-1.5"></div>
              }

              @for (item of group.items; track item.id; let iIdx = $index) {
                <div class="relative">
                  @if (isCollapsed()) {
                    <!-- Collapsed Nav Item: Centered Squircle with Tooltip -->
                    @if (previewMode()) {
                      <button
                        type="button"
                        (click)="onSelectTab(item.id)"
                        class="nav-item w-11 h-11 mx-auto rounded-2xl flex items-center justify-center transition-all relative text-slate-500 hover:text-slate-900 hover:bg-slate-100/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-neutral-900 cursor-pointer"
                        [class.on]="activeTabId() === item.id"
                        [attr.data-tip]="item.label"
                      >
                        <ng-container *ngTemplateOutlet="itemIcon; context: { item: item, isActive: activeTabId() === item.id }"></ng-container>
                      </button>
                    } @else {
                      <a
                        [routerLink]="item.route"
                        routerLinkActive="on"
                        [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
                        (click)="closeMobile()"
                        #rla="routerLinkActive"
                        class="nav-item w-11 h-11 mx-auto rounded-2xl flex items-center justify-center transition-all relative text-slate-500 hover:text-slate-900 hover:bg-slate-100/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-neutral-900 cursor-pointer"
                        [attr.data-tip]="item.label"
                      >
                        <ng-container *ngTemplateOutlet="itemIcon; context: { item: item, isActive: rla.isActive }"></ng-container>
                      </a>
                    }
                  } @else {
                    <!-- Expanded Nav Item: Full-Width Row -->
                    @if (previewMode()) {
                      <button
                        type="button"
                        (click)="onSelectTab(item.id)"
                        class="nav-item w-full h-10 px-1.5 rounded-xl flex items-center gap-2.5 text-xs font-medium transition-all relative text-slate-600 hover:text-slate-900 hover:bg-slate-100/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-neutral-900 cursor-pointer"
                        [class.on]="activeTabId() === item.id"
                      >
                        <ng-container *ngTemplateOutlet="itemIcon; context: { item: item, isActive: activeTabId() === item.id }"></ng-container>
                        <span class="truncate flex-1 text-left font-medium">{{ item.label }}</span>
                      </button>
                    } @else {
                      <a
                        [routerLink]="item.route"
                        routerLinkActive="on"
                        [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
                        (click)="closeMobile()"
                        #rla="routerLinkActive"
                        class="nav-item w-full h-10 px-1.5 rounded-xl flex items-center gap-2.5 text-xs font-medium transition-all relative text-slate-600 hover:text-slate-900 hover:bg-slate-100/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-neutral-900 cursor-pointer"
                      >
                        <ng-container *ngTemplateOutlet="itemIcon; context: { item: item, isActive: rla.isActive }"></ng-container>
                        <span class="truncate flex-1 text-left font-medium">{{ item.label }}</span>
                      </a>
                    }
                  }
                </div>
              }
            </div>
          }
        </nav>
      </div>

      <!-- Footer Section: Expand toggle & Profile Popover -->
      <div class="relative mt-auto pt-2 border-t border-slate-200/80 flex flex-col gap-1 shrink-0">
        @if (isCollapsed()) {
          <!-- Expand Sidebar Button in Rail Mode -->
          <button
            type="button"
            (click)="toggleCollapse()"
            class="w-11 h-11 mx-auto rounded-2xl flex items-center justify-center text-slate-400 hover:text-slate-800 hover:bg-slate-100/80 transition-colors relative focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-neutral-900 cursor-pointer"
            data-tip="Expand sidebar (Ctrl+B)"
            aria-label="Expand sidebar"
          >
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="3" width="18" height="18" rx="4" />
              <path d="M9 3v18" />
              <path d="m12 9 3 3-3 3" />
            </svg>
          </button>

          <!-- Profile Button Trigger in Rail Mode -->
          <button
            type="button"
            (click)="toggleProfilePopover($event)"
            class="w-11 h-11 mx-auto rounded-2xl flex items-center justify-center relative hover:bg-slate-100/80 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 cursor-pointer"
            [attr.data-tip]="userName()"
            aria-haspopup="menu"
          >
            <div class="w-8 h-8 rounded-xl bg-slate-900 text-white flex items-center justify-center font-bold text-xs shrink-0 relative shadow-2xs">
              {{ userInitials() }}
              <span class="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full bg-[#22C55E] ring-2 ring-white"></span>
            </div>
          </button>
        } @else {
          <!-- Profile Button Trigger in Expanded Mode -->
          <button
            type="button"
            (click)="toggleProfilePopover($event)"
            class="w-full h-11 px-1.5 rounded-xl flex items-center gap-2.5 hover:bg-slate-100/80 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600 text-left cursor-pointer"
            aria-haspopup="menu"
          >
            <div class="w-8 h-8 rounded-lg bg-slate-900 text-white flex items-center justify-center font-bold text-xs shrink-0 relative shadow-2xs">
              {{ userInitials() }}
              <span class="absolute -bottom-0.5 -right-0.5 w-2.5 h-2.5 rounded-full bg-[#22C55E] ring-2 ring-white"></span>
            </div>
            <div class="min-w-0 flex-1 leading-tight">
              <b class="block font-semibold text-xs text-slate-900 truncate">{{ userName() }}</b>
              <span class="text-[11px] text-slate-500 truncate block">{{ userRole() }}</span>
            </div>
            <svg class="w-4 h-4 text-slate-400 mr-1 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="m7 15 5 5 5-5M7 9l5-5 5 5" />
            </svg>
          </button>
        }

        <!-- Profile Popover -->
        @if (isProfileOpen()) {
          <div
            class="pop absolute z-50 left-[calc(100%+12px)] bottom-0 w-60 p-1.5 bg-white border border-slate-200/90 rounded-2xl shadow-[0_16px_36px_-12px_rgba(15,23,42,0.2)] animate-in fade-in zoom-in-95 duration-150"
            role="menu"
            (click)="$event.stopPropagation()"
          >
            <div class="p-2.5 border-b border-slate-100 mb-1.5 text-xs text-slate-500">
              <b class="block font-semibold text-sm text-slate-900">{{ userName() }}</b>
              <span class="text-[11px] text-slate-400 truncate block">organizer&#64;ticketworld.io</span>
            </div>
            <a
              routerLink="/organizer"
              (click)="isProfileOpen.set(false)"
              class="flex items-center gap-2.5 w-full h-9 px-2.5 rounded-xl text-xs font-medium text-slate-700 hover:text-slate-900 hover:bg-slate-100 transition-colors"
              role="menuitem"
            >
              <svg class="w-4 h-4 text-slate-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/>
              </svg>
              <span>Workspace Profile</span>
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
              <span>Dev Workbench</span>
            </a>
            <a
              routerLink="/"
              (click)="isProfileOpen.set(false)"
              class="flex items-center gap-2.5 w-full h-9 px-2.5 rounded-xl text-xs font-medium text-rose-600 hover:bg-rose-50 transition-colors"
              role="menuitem"
            >
              <svg class="w-4 h-4 text-rose-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9"/>
              </svg>
              <span>Exit to Customer Portal</span>
            </a>
          </div>
        }
      </div>
    </aside>

    <!-- Command Palette Overlay -->
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
              <circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/>
            </svg>
            <input
              type="text"
              #paletteInput
              [value]="paletteQuery()"
              (input)="onPaletteInput($event)"
              placeholder="Search pages and quick actions..."
              class="w-full h-14 bg-transparent outline-none text-sm text-slate-900 placeholder:text-slate-400 font-medium"
            />
          </div>

          <div class="max-h-72 overflow-y-auto p-2 space-y-1">
            @for (action of filteredPaletteActions(); track action.label; let i = $index) {
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

    <!-- Template for Nav Icon -->
    <ng-template #itemIcon let-item="item" let-isActive="isActive">
      <!-- Icon Container: Centered in collapsed rail; neatly boxed in expanded drawer -->
      <div
        class="nav-icon-wrapper flex items-center justify-center shrink-0 transition-all"
        [class.w-5]="isCollapsed()"
        [class.h-5]="isCollapsed()"
        [class.w-8]="!isCollapsed()"
        [class.h-8]="!isCollapsed()"
        [class.rounded-xl]="!isCollapsed()"
        [class.bg-[#22C55E]]="!isCollapsed() && isActive"
        [class.text-white]="!isCollapsed() && isActive"
        [class.shadow-2xs]="!isCollapsed() && isActive"
        [class.text-[#006e2f]]="isCollapsed() && isActive"
        [class.text-slate-500]="!isActive"
        [class.group-hover:text-slate-900]="!isActive"
      >
        @switch (item.iconType) {
          @case ('dashboard') {
            <svg class="w-5 h-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="3" width="7" height="9" rx="1.5"/><rect x="14" y="3" width="7" height="5" rx="1.5"/><rect x="14" y="12" width="7" height="9" rx="1.5"/><rect x="3" y="16" width="7" height="5" rx="1.5"/>
            </svg>
          }
          @case ('events') {
            <svg class="w-5 h-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <path d="M6 2 3 6v13a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><path d="M3 6h18"/><path d="M16 10a4 4 0 0 1-8 0"/>
            </svg>
          }
          @case ('seat-map') {
            <svg class="w-5 h-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18M3 15h18M9 3v18M15 3v18"/>
            </svg>
          }
          @case ('checkin') {
            <svg class="w-5 h-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <rect width="5" height="5" x="3" y="3" rx="1"/><rect width="5" height="5" x="16" y="3" rx="1"/><rect width="5" height="5" x="3" y="16" rx="1"/><path d="M21 16v3a2 2 0 0 1-2 2h-3M21 21v.01M12 7v3a2 2 0 0 1-2 2H7M3 12h.01M12 3h.01M12 16v.01M16 12h1M21 12v.01M12 21v-4"/>
            </svg>
          }
          @case ('staff') {
            <svg class="w-5 h-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/>
            </svg>
          }
          @default {
            <svg class="w-5 h-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="9"/>
            </svg>
          }
        }
      </div>
    </ng-template>
  `,
  styles: [
    `
      :host {
        display: contents;
      }
      .custom-scrollbar::-webkit-scrollbar {
        width: 3px;
      }
      .custom-scrollbar::-webkit-scrollbar-thumb {
        background-color: rgba(15, 23, 42, 0.12);
        border-radius: 9999px;
      }

      /* Active Link State in Collapsed Rail (Neo-Mint Token: mint-tint #D7F4DF & primary #006e2f) */
      .min ::ng-deep .nav-item.on {
        background-color: #D7F4DF !important;
        color: #006e2f !important;
        box-shadow: 0 1px 3px rgba(0, 110, 47, 0.1), inset 0 0 0 1px rgba(34, 197, 94, 0.35) !important;
      }
      .min ::ng-deep .nav-item.on svg {
        color: #006e2f !important;
      }

      /* Active Link State in Expanded Drawer (Neo-Mint Token: soft mint highlight) */
      :not(.min) ::ng-deep .nav-item.on {
        background-color: rgba(215, 244, 223, 0.7) !important;
        color: #006e2f !important;
        box-shadow: inset 0 0 0 1px rgba(34, 197, 94, 0.3) !important;
        font-weight: 600 !important;
      }
      :not(.min) ::ng-deep .nav-item.on .nav-icon-wrapper {
        background-color: #22c55e !important;
        color: #ffffff !important;
        box-shadow: 0 1px 3px rgba(34, 197, 94, 0.28) !important;
      }
      :not(.min) ::ng-deep .nav-item.on .nav-icon-wrapper svg {
        color: #ffffff !important;
      }

      /* Tooltip for Rail Mode */
      .min [data-tip]::after {
        content: attr(data-tip);
        position: absolute;
        left: calc(100% + 12px);
        top: 50%;
        transform: translateY(-50%) scale(0.95);
        padding: 5px 9px;
        border-radius: 8px;
        background: #0f172a;
        color: #ffffff;
        font-size: 11px;
        font-weight: 500;
        white-space: nowrap;
        opacity: 0;
        pointer-events: none;
        transition: opacity 0.15s ease, transform 0.15s ease;
        z-index: 60;
        box-shadow: 0 4px 12px rgba(15, 23, 42, 0.18);
      }
      .min [data-tip]:hover::after {
        opacity: 1;
        transform: translateY(-50%) scale(1);
        transition-delay: 0.05s;
      }
    `
  ]
})
export class SidebarComponent {
  // Inputs / Models
  isCollapsed = model<boolean>(false);
  isMobileOpen = model<boolean>(false);
  previewMode = input<boolean>(false);
  activeTabId = input<string>('dashboard');
  hubLabel = input<string>('Organizer Hub');
  eventContext = input<string | null>('Coastal Arena Summit 2026');
  navGroups = input<NavGroup[] | null>(null);
  userName = input<string>('Ajendra S.');
  userRole = input<string>('Lead Organizer');
  userInitials = input<string>('AS');

  // Outputs
  tabChange = output<string>();

  // Interactive UI states
  isProfileOpen = signal<boolean>(false);
  isPaletteOpen = signal<boolean>(false);
  paletteQuery = signal<string>('');

  navContainer = viewChild<ElementRef<HTMLElement>>('navContainer');
  paletteInput = viewChild<ElementRef<HTMLInputElement>>('paletteInput');

  private readonly STORAGE_KEY = 'ticketworld_organizer_sidebar_collapsed';

  constructor(private router: Router) {
    try {
      const saved = localStorage.getItem(this.STORAGE_KEY);
      if (saved !== null) {
        this.isCollapsed.set(saved === 'true');
      }
    } catch {}

    effect(() => {
      const state = this.isCollapsed();
      try {
        localStorage.setItem(this.STORAGE_KEY, String(state));
      } catch {}
    });
  }

  effectiveNavGroups = computed(() => {
    return this.navGroups() ?? DEFAULT_ORGANIZER_NAV_GROUPS;
  });

  filteredPaletteActions = computed(() => {
    const q = this.paletteQuery().toLowerCase();
    const actions = [
      { label: 'Overview / Dashboard', route: '/organizer', shortcut: '1' },
      { label: 'Event Management', route: '/organizer/events', shortcut: '2' },
      { label: 'Seat Map Builder', route: '/organizer/seat-map', shortcut: '3' },
      { label: 'Check-in Scanner', route: '/organizer/checkin', shortcut: '4' },
      { label: 'Staff Permissions', route: '/organizer/staff', shortcut: '5' },
      { label: 'Dev Workbench', route: '/developer', shortcut: 'D' },
      { label: 'Exit to Customer Portal', route: '/', shortcut: 'Esc' }
    ];
    if (!q) return actions;
    return actions.filter(a => a.label.toLowerCase().includes(q));
  });

  @HostListener('window:keydown', ['$event'])
  handleKeyDown(event: KeyboardEvent) {
    const isMeta = event.ctrlKey || event.metaKey;
    const key = event.key.toLowerCase();

    if (isMeta && key === 'b') {
      event.preventDefault();
      this.toggleCollapse();
    } else if (isMeta && key === 'k') {
      event.preventDefault();
      this.openPalette();
    } else if (key === 'escape') {
      this.closePalette();
      this.isProfileOpen.set(false);
    }
  }

  @HostListener('document:click')
  onDocumentClick() {
    this.isProfileOpen.set(false);
  }

  toggleCollapse() {
    this.isCollapsed.update(v => !v);
  }

  closeMobile() {
    this.isMobileOpen.set(false);
  }

  onBrandClick() {
    if (this.isCollapsed()) {
      this.toggleCollapse();
    }
    this.closeMobile();
  }

  onSelectTab(id: string) {
    this.tabChange.emit(id);
    this.closeMobile();
  }

  toggleProfilePopover(event: MouseEvent) {
    event.stopPropagation();
    this.isProfileOpen.update(v => !v);
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
}
