import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-organizer-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="space-y-6">
      <!-- 1. Header Hero Card (Clean Bento Module, Zero Status Pills) -->
      <div
        class="bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-8 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)]"
      >
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 class="text-2xl font-black tracking-tight text-slate-900">Project Operations Hub</h1>
            <p class="text-xs sm:text-sm text-slate-500 mt-1 font-medium">
              Real-time telemetry, stadium seat allocation, and live entry monitoring.
            </p>
          </div>

          <div class="flex items-center gap-2.5">
            <a
              routerLink="/organizer/events/create"
              class="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-slate-900 text-white text-xs font-semibold hover:bg-slate-800 transition-colors shadow-sm cursor-pointer"
            >
              <svg class="w-4 h-4 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="12" y1="5" x2="12" y2="19"/>
                <line x1="5" y1="12" x2="19" y2="12"/>
              </svg>
              <span>Create Event</span>
            </a>
          </div>
        </div>
      </div>

      <!-- 2. Bento Grid KPI Metrics (Inter, tabular-nums, Clean SVG Icons, Zero Badges) -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- Metric 1: Total Gross Revenue -->
        <div
          class="bg-white rounded-3xl border border-slate-200/80 p-5 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] flex flex-col justify-between"
        >
          <div class="flex items-center justify-between mb-3">
            <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Total Gross</span>
            <div class="w-8 h-8 rounded-xl bg-[#D7F4DF] text-[#006e2f] flex items-center justify-center">
              <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="12" y1="1" x2="12" y2="23"/>
                <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
              </svg>
            </div>
          </div>
          <div>
            <div class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight tabular-nums">฿14,280,000</div>
            <div class="text-xs text-slate-500 mt-1 font-medium">94.8% of aggregate event target</div>
          </div>
        </div>

        <!-- Metric 2: Live Ingress Check-in -->
        <div
          class="bg-white rounded-3xl border border-slate-200/80 p-5 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] flex flex-col justify-between"
        >
          <div class="flex items-center justify-between mb-3">
            <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Gate Check-in</span>
            <div class="w-8 h-8 rounded-xl bg-slate-100 text-slate-700 flex items-center justify-center">
              <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <rect width="5" height="5" x="3" y="3" rx="1"/><rect width="5" height="5" x="16" y="3" rx="1"/><rect width="5" height="5" x="3" y="16" rx="1"/><path d="M21 16v3a2 2 0 0 1-2 2h-3M21 21v.01M12 7v3a2 2 0 0 1-2 2H7"/>
              </svg>
            </div>
          </div>
          <div>
            <div class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight tabular-nums">18,450 / 22,000</div>
            <div class="text-xs text-slate-500 mt-1 font-medium">83.8% scanned through main turnstiles</div>
          </div>
        </div>

        <!-- Metric 3: Active Holdouts & Queue -->
        <div
          class="bg-white rounded-3xl border border-slate-200/80 p-5 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] flex flex-col justify-between"
        >
          <div class="flex items-center justify-between mb-3">
            <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Inventory Holds</span>
            <div class="w-8 h-8 rounded-xl bg-slate-100 text-slate-700 flex items-center justify-center">
              <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18M3 15h18M9 3v18M15 3v18"/>
              </svg>
            </div>
          </div>
          <div>
            <div class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight tabular-nums">412 Seats</div>
            <div class="text-xs text-slate-500 mt-1 font-medium">Temporary 10-minute cart locks</div>
          </div>
        </div>

        <!-- Metric 4: Operational Staff -->
        <div
          class="bg-white rounded-3xl border border-slate-200/80 p-5 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] flex flex-col justify-between"
        >
          <div class="flex items-center justify-between mb-3">
            <span class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Staff Active</span>
            <div class="w-8 h-8 rounded-xl bg-slate-100 text-slate-700 flex items-center justify-center">
              <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/>
              </svg>
            </div>
          </div>
          <div>
            <div class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight tabular-nums">48 Personnel</div>
            <div class="text-xs text-slate-500 mt-1 font-medium">Assigned across 6 stadium zones</div>
          </div>
        </div>
      </div>

      <!-- 3. Operations Quick Navigation (Bento Row Cards, Zero Badges) -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- Card 1: Event Management -->
        <a
          routerLink="/organizer/events"
          class="group bg-white rounded-3xl border border-slate-200/80 p-5 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] hover:border-emerald-500/40 hover:shadow-md transition-all cursor-pointer block"
        >
          <div class="w-10 h-10 rounded-2xl bg-slate-100 group-hover:bg-[#D7F4DF] text-slate-700 group-hover:text-[#006e2f] flex items-center justify-center transition-colors mb-3">
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
              <path d="M6 2 3 6v13a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><path d="M3 6h18"/><path d="M16 10a4 4 0 0 1-8 0"/>
            </svg>
          </div>
          <h2 class="font-bold text-sm text-slate-900 group-hover:text-emerald-700 transition-colors">Event Management</h2>
          <p class="text-xs text-slate-500 mt-1 font-medium">Concert schedules, tiers, and multi-round sales.</p>
        </a>

        <!-- Card 2: Seat Map Builder -->
        <a
          routerLink="/organizer/seat-map"
          class="group bg-white rounded-3xl border border-slate-200/80 p-5 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] hover:border-emerald-500/40 hover:shadow-md transition-all cursor-pointer block"
        >
          <div class="w-10 h-10 rounded-2xl bg-slate-100 group-hover:bg-[#D7F4DF] text-slate-700 group-hover:text-[#006e2f] flex items-center justify-center transition-colors mb-3">
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
              <rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18M3 15h18M9 3v18M15 3v18"/>
            </svg>
          </div>
          <h2 class="font-bold text-sm text-slate-900 group-hover:text-emerald-700 transition-colors">Seat Map Builder</h2>
          <p class="text-xs text-slate-500 mt-1 font-medium">Stage zoning, pricing layers, and quota holds.</p>
        </a>

        <!-- Card 3: Check-in Scanner -->
        <a
          routerLink="/organizer/checkin"
          class="group bg-white rounded-3xl border border-slate-200/80 p-5 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] hover:border-emerald-500/40 hover:shadow-md transition-all cursor-pointer block"
        >
          <div class="w-10 h-10 rounded-2xl bg-slate-100 group-hover:bg-[#D7F4DF] text-slate-700 group-hover:text-[#006e2f] flex items-center justify-center transition-colors mb-3">
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
              <rect width="5" height="5" x="3" y="3" rx="1"/><rect width="5" height="5" x="16" y="3" rx="1"/><rect width="5" height="5" x="3" y="16" rx="1"/><path d="M21 16v3a2 2 0 0 1-2 2h-3M21 21v.01M12 7v3a2 2 0 0 1-2 2H7"/>
            </svg>
          </div>
          <h2 class="font-bold text-sm text-slate-900 group-hover:text-emerald-700 transition-colors">Check-in Scanner</h2>
          <p class="text-xs text-slate-500 mt-1 font-medium">Turnstile scanner and access verification terminal.</p>
        </a>

        <!-- Card 4: Staff Permissions -->
        <a
          routerLink="/organizer/staff"
          class="group bg-white rounded-3xl border border-slate-200/80 p-5 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)] hover:border-emerald-500/40 hover:shadow-md transition-all cursor-pointer block"
        >
          <div class="w-10 h-10 rounded-2xl bg-slate-100 group-hover:bg-[#D7F4DF] text-slate-700 group-hover:text-[#006e2f] flex items-center justify-center transition-colors mb-3">
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
              <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/>
            </svg>
          </div>
          <h2 class="font-bold text-sm text-slate-900 group-hover:text-emerald-700 transition-colors">Staff Permissions</h2>
          <p class="text-xs text-slate-500 mt-1 font-medium">Zone delegations, scanners, and role auditing.</p>
        </a>
      </div>
    </div>
  `
})
export class OrganizerComponent {}
