import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MetricCardComponent, MetricCardData } from '../../shared/components/metric-card/metric-card.component';

@Component({
  selector: 'app-organizer-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MetricCardComponent],
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

      <!-- 2. Mindful Metrics Grid (Freud v2 Pattern Adapted for Concert Operations) -->
      <div class="space-y-3">
        <!-- Telemetry Controls Header -->
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2 px-1">
          <div class="flex items-center gap-2">
            <span class="inline-block w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
            <span class="text-xs font-bold uppercase tracking-wider text-slate-500">Live Telemetry Metrics</span>
          </div>

          <!-- Time Range Filter Pills (Glanceable / Interactive) -->
          <div class="inline-flex items-center p-1 bg-slate-100 rounded-xl border border-slate-200/60 self-start sm:self-auto">
            <button
              type="button"
              (click)="selectedPeriod = 'realtime'"
              [class.bg-white]="selectedPeriod === 'realtime'"
              [class.text-slate-900]="selectedPeriod === 'realtime'"
              [class.shadow-xs]="selectedPeriod === 'realtime'"
              [class.text-slate-500]="selectedPeriod !== 'realtime'"
              class="px-2.5 py-1 text-[11px] font-semibold rounded-lg transition-all"
            >
              Realtime
            </button>
            <button
              type="button"
              (click)="selectedPeriod = 'today'"
              [class.bg-white]="selectedPeriod === 'today'"
              [class.text-slate-900]="selectedPeriod === 'today'"
              [class.shadow-xs]="selectedPeriod === 'today'"
              [class.text-slate-500]="selectedPeriod !== 'today'"
              class="px-2.5 py-1 text-[11px] font-semibold rounded-lg transition-all"
            >
              Today
            </button>
            <button
              type="button"
              (click)="selectedPeriod = 'tour'"
              [class.bg-white]="selectedPeriod === 'tour'"
              [class.text-slate-900]="selectedPeriod === 'tour'"
              [class.shadow-xs]="selectedPeriod === 'tour'"
              [class.text-slate-500]="selectedPeriod !== 'tour'"
              class="px-2.5 py-1 text-[11px] font-semibold rounded-lg transition-all"
            >
              Tour Total
            </button>
          </div>
        </div>

        <!-- 4 Reusable Metric Cards (Clean Architecture) -->
        <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4 sm:gap-5">
          @for (metric of metrics; track metric.id) {
            <app-metric-card [cardData]="metric" />
          }
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
export class OrganizerComponent {
  selectedPeriod: 'realtime' | 'today' | 'tour' = 'realtime';

  metrics: MetricCardData[] = [
    {
      id: 'gross-revenue',
      title: 'Gross Revenue',
      subtitle: 'Ticket sales aggregate',
      value: '฿14,280,000',
      badgeText: 'Target 95%',
      trendText: '+14.2%',
      trendDirection: 'up',
      qualitativeText: 'Pacing ahead of target projection',
      insightText: 'VIP velocity +22% after stage plan release',
      linkUrl: '/organizer/events',
      linkTitle: 'View event revenue breakdown',
      theme: 'emerald',
      chartType: 'sparkline-revenue',
      iconType: 'currency',
      timeAxisLabels: ['Presale', 'Early', 'General', 'VIP', 'Door']
    },
    {
      id: 'gate-checkin',
      title: 'Gate Check-in',
      subtitle: 'Turnstile ingress pace',
      value: '18,450',
      unit: '/ 22k',
      badgeText: 'Live Sync',
      trendText: '+8.2%/h',
      trendDirection: 'up',
      qualitativeText: 'Fluid ingress • 83.8% scanned through',
      insightText: 'Turnstile B & West corridor moving smoothly',
      linkUrl: '/organizer/checkin',
      linkTitle: 'View turnstile terminal',
      theme: 'indigo',
      chartType: 'bars-checkin',
      iconType: 'scanner',
      timeAxisLabels: ['16:00', '17:00', '18:00', '19:00', '20:00', 'Now']
    },
    {
      id: 'inventory-holds',
      title: 'Inventory Holds',
      subtitle: 'Cart reservations buffer',
      value: '412',
      unit: 'Seats',
      badgeText: '10m TTL',
      trendText: 'Stable',
      trendDirection: 'neutral',
      qualitativeText: 'Balanced rotation • 96.4% checkout lock rate',
      insightText: 'Expired holds smoothly recirculated within 400ms',
      linkUrl: '/organizer/seat-map',
      linkTitle: 'View seat map inventory',
      theme: 'amber',
      chartType: 'wave-hold',
      iconType: 'seats',
      timeAxisLabels: ['10m', '8m', '6m', '4m', '2m', 'Now']
    },
    {
      id: 'staff-active',
      title: 'Staff Active',
      subtitle: 'Zone delegations & safety',
      value: '48',
      unit: 'Personnel',
      badgeText: '6/6 Zones',
      trendText: '100% Ready',
      trendDirection: 'neutral',
      qualitativeText: 'Full zone perimeter coverage',
      insightText: 'Turnstile stations fully staffed for evening rush',
      linkUrl: '/organizer/staff',
      linkTitle: 'View staff allocations',
      theme: 'slate',
      chartType: 'meter-staff',
      iconType: 'staff',
      timeAxisLabels: ['Gate A', 'Gate B', 'VIP', 'Floor', 'Sec']
    }
  ];
}
