import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="min-h-screen flex bg-surface-50 text-neutral-800 antialiased">
      <!-- Admin Sidebar (Dark Slate styled) -->
      <aside
        [class.translate-x-0]="isMobileMenuOpen()"
        [class.-translate-x-full]="!isMobileMenuOpen()"
        class="fixed inset-y-0 left-0 z-40 w-64 bg-surface-900 text-neutral-100 transition-transform duration-200 ease-in-out md:translate-x-0 md:static flex flex-col justify-between"
      >
        <div>
          <!-- Admin Brand Header -->
          <div class="h-16 flex items-center justify-between px-5 border-b border-surface-800">
            <a routerLink="/" class="flex items-center gap-2.5">
              <div class="w-8 h-8 rounded-lg bg-indigo-500 flex items-center justify-center text-white font-bold text-base shadow-sm">
                A
              </div>
              <div>
                <div class="font-bold text-sm text-white tracking-tight leading-none">TicketWorld</div>
                <div class="text-[10px] font-semibold text-indigo-400 uppercase tracking-wider mt-0.5">Platform Admin</div>
              </div>
            </a>
            <button
              (click)="isMobileMenuOpen.set(false)"
              class="md:hidden text-neutral-400 hover:text-white"
              aria-label="Close menu"
            >
              ✕
            </button>
          </div>

          <!-- Admin Navigation Items -->
          <nav class="p-3 space-y-1">
            <a
              routerLink="/admin/moderation"
              routerLinkActive="bg-indigo-600 text-white font-semibold"
              (click)="isMobileMenuOpen.set(false)"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium text-neutral-300 hover:bg-surface-800 hover:text-white transition-colors"
            >
              <span class="text-base">🛡️</span>
              <span>Event Moderation</span>
            </a>

            <a
              routerLink="/admin/payouts"
              routerLinkActive="bg-indigo-600 text-white font-semibold"
              (click)="isMobileMenuOpen.set(false)"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium text-neutral-300 hover:bg-surface-800 hover:text-white transition-colors"
            >
              <span class="text-base">💰</span>
              <span>Finance & Payouts</span>
            </a>

            <a
              routerLink="/admin/audit-logs"
              routerLinkActive="bg-indigo-600 text-white font-semibold"
              (click)="isMobileMenuOpen.set(false)"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium text-neutral-300 hover:bg-surface-800 hover:text-white transition-colors"
            >
              <span class="text-base">📜</span>
              <span>Security Audit Trail</span>
            </a>

            <a
              routerLink="/admin/system-health"
              routerLinkActive="bg-indigo-600 text-white font-semibold"
              (click)="isMobileMenuOpen.set(false)"
              class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium text-neutral-300 hover:bg-surface-800 hover:text-white transition-colors"
            >
              <span class="text-base">⚡</span>
              <span>System & Telemetry</span>
            </a>
          </nav>
        </div>

        <!-- Sidebar Footer Status -->
        <div class="p-4 border-t border-surface-800 space-y-3">
          <div class="flex items-center justify-between text-xs text-neutral-400">
            <span class="flex items-center gap-1.5">
              <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
              API Gateway
            </span>
            <span class="text-emerald-400 font-mono text-[11px]">Online</span>
          </div>

          <a
            routerLink="/test-uxui"
            class="flex items-center justify-between px-3 py-2 rounded-xl bg-amber-500/20 text-amber-300 text-xs font-semibold hover:bg-amber-500/30 transition-colors"
          >
            <span>🎨 Test UX/UI Gallery</span>
            <span>→</span>
          </a>

          <a
            routerLink="/"
            class="flex items-center gap-2 text-xs font-medium text-neutral-400 hover:text-white"
          >
            <span>← Exit to Public Portal</span>
          </a>
        </div>
      </aside>

      <!-- Main Admin Content -->
      <div class="flex-1 flex flex-col min-w-0">
        <!-- Top Admin Header -->
        <header class="h-16 bg-white border-b border-surface-200 px-4 sm:px-6 flex items-center justify-between sticky top-0 z-30">
          <div class="flex items-center gap-3">
            <button
              (click)="isMobileMenuOpen.set(true)"
              class="md:hidden p-2 rounded-lg text-neutral-500 hover:bg-surface-100"
              aria-label="Open navigation"
            >
              ☰
            </button>
            <div class="flex items-center gap-2 text-xs text-neutral-500 font-medium">
              <span class="text-neutral-400">Governance</span>
              <span>/</span>
              <span class="text-neutral-900 font-semibold">Super Admin Console</span>
            </div>
          </div>

          <div class="flex items-center gap-3">
            <span class="text-xs px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-700 font-semibold border border-emerald-200">
              Cluster: ap-southeast-1
            </span>
            <div class="h-5 w-px bg-surface-200"></div>
            <div class="flex items-center gap-2">
              <div class="w-8 h-8 rounded-full bg-surface-900 text-white flex items-center justify-center font-bold text-xs">
                SA
              </div>
              <span class="hidden sm:inline text-xs font-semibold text-neutral-700">System Admin</span>
            </div>
          </div>
        </header>

        <!-- Nested Admin Routes -->
        <main class="flex-1 p-4 sm:p-6 lg:p-8 max-w-7xl w-full mx-auto">
          <router-outlet></router-outlet>
        </main>
      </div>

      <!-- Backdrop for mobile sidebar -->
      @if (isMobileMenuOpen()) {
        <div
          (click)="isMobileMenuOpen.set(false)"
          class="fixed inset-0 bg-black/40 z-30 md:hidden"
        ></div>
      }
    </div>
  `
})
export class AdminLayoutComponent {
  readonly isMobileMenuOpen = signal(false);
}
