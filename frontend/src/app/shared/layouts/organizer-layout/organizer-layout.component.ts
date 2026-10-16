import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../../components/sidebar/sidebar.component';

@Component({
  selector: 'app-organizer-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterOutlet, SidebarComponent],
  template: `
    <div class="min-h-screen flex bg-[#E8F1EB] text-slate-900 antialiased">
      <!-- Reusable Organizer Sidebar (Dual-State Rail/Drawer + Mobile Drawer) -->
      <app-sidebar
        [(isMobileOpen)]="isMobileMenuOpen"
        eventContext="Coastal Arena Summit 2026"
      />

      <!-- Right Content Viewport: Header + Outlet -->
      <div class="flex-1 flex flex-col min-w-0">
        <!-- Top App Bar (Floating Bento Header matching Sidebar & Neo-Mint System) -->
        <header
          class="sticky top-3 z-30 mx-3 mt-3 h-16 bg-white/95 backdrop-blur-md border border-slate-200/80 rounded-2xl sm:rounded-3xl shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] px-4 sm:px-6 flex items-center justify-between transition-all"
        >
          <div class="flex items-center gap-3">
            <!-- Mobile Toggle -->
            <button
              type="button"
              (click)="isMobileMenuOpen.set(true)"
              class="md:hidden p-2 rounded-xl text-slate-600 hover:bg-slate-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-600"
              aria-label="Open mobile navigation"
            >
              <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="3" y1="12" x2="21" y2="12"/>
                <line x1="3" y1="6" x2="21" y2="6"/>
                <line x1="3" y1="18" x2="21" y2="18"/>
              </svg>
            </button>

            <!-- Breadcrumbs -->
            <div class="flex items-center gap-2 text-xs font-medium text-slate-500">
              <span class="text-slate-400">Organizer Portal</span>
              <svg class="w-3.5 h-3.5 text-slate-300" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="m9 18 6-6-6-6"/>
              </svg>
              <span class="text-slate-900 font-bold">Dashboard & Operations</span>
            </div>
          </div>

          <!-- Header Right Actions: Clean Pro Icons, Zero Badges/Pills -->
          <div class="flex items-center gap-2.5 sm:gap-3">
            <!-- Direct Link to Developer Workbench -->
            <a
              routerLink="/developer"
              class="inline-flex items-center gap-1.5 px-3.5 py-1.5 rounded-full border border-slate-200/90 bg-white text-slate-700 hover:text-slate-900 text-xs font-semibold hover:bg-slate-50 transition-colors shadow-2xs"
            >
              <svg class="w-3.5 h-3.5 text-slate-500" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <polyline points="16 18 22 12 16 6" />
                <polyline points="8 6 2 12 8 18" />
              </svg>
              <span>Dev Workbench</span>
            </a>

            <!-- Quick Action: + New Event -->
            <a
              routerLink="/organizer/events/create"
              class="inline-flex items-center gap-1.5 px-4 py-1.5 rounded-full bg-slate-900 text-white text-xs font-semibold hover:bg-slate-800 shadow-sm transition-colors cursor-pointer"
            >
              <svg class="w-3.5 h-3.5 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2">
                <line x1="12" y1="5" x2="12" y2="19"/>
                <line x1="5" y1="12" x2="19" y2="12"/>
              </svg>
              <span class="hidden sm:inline">New Event</span>
            </a>
          </div>
        </header>

        <!-- Nested Content Outlet -->
        <main class="flex-1 p-3 sm:p-4 lg:p-6 max-w-7xl w-full mx-auto">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class OrganizerLayoutComponent {
  isMobileMenuOpen = signal(false);
}
