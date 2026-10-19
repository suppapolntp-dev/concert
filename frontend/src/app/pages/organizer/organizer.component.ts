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

        <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4 sm:gap-5">
          <!-- Metric Card 1: Total Gross Revenue -->
          <div
            class="bg-white rounded-[24px] border border-stone-200/80 p-5 sm:p-6 shadow-[0_2px_8px_-2px_rgba(15,23,42,0.03),0_8px_20px_-4px_rgba(15,23,42,0.04)] flex flex-col justify-between hover:border-emerald-500/30 transition-all group"
          >
            <!-- Card Header: [Icon] Title + Context Badge -->
            <div>
              <div class="flex items-center justify-between mb-3.5">
                <div class="flex items-center gap-2.5">
                  <div class="w-9 h-9 rounded-2xl bg-[#D7F4DF] text-[#006e2f] flex items-center justify-center shrink-0">
                    <svg class="w-4.5 h-4.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <line x1="12" y1="1" x2="12" y2="23"/>
                      <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
                    </svg>
                  </div>
                  <div>
                    <h3 class="text-xs font-bold uppercase tracking-wider text-slate-500">Gross Revenue</h3>
                    <div class="text-[11px] text-slate-400 font-medium">Ticket sales aggregate</div>
                  </div>
                </div>
                <span class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-50 text-emerald-700 border border-emerald-200/50">
                  Target 95%
                </span>
              </div>

              <!-- Key Metric / Stat Area -->
              <div class="space-y-1 mb-4">
                <div class="flex items-baseline justify-between gap-2">
                  <div class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight tabular-nums">
                    ฿14,280,000
                  </div>
                  <div class="inline-flex items-center gap-1 text-xs font-bold text-emerald-600 bg-emerald-50 px-2 py-0.5 rounded-full">
                    <svg class="w-3 h-3" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                      <polyline points="18 15 12 9 6 15"/>
                    </svg>
                    <span>+14.2%</span>
                  </div>
                </div>
                <!-- Qualitative Label -->
                <p class="text-xs font-medium text-emerald-700/90 flex items-center gap-1.5">
                  <span class="w-1.5 h-1.5 rounded-full bg-emerald-500"></span>
                  Pacing ahead of target projection
                </p>
              </div>

              <!-- Data Visualization: Curved Line Chart with Soft Gradient Fill -->
              <div class="py-2">
                <div class="h-14 w-full">
                  <svg class="w-full h-full overflow-visible" viewBox="0 0 260 56" preserveAspectRatio="none">
                    <defs>
                      <linearGradient id="revenue-grad" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stop-color="#10B981" stop-opacity="0.22"/>
                        <stop offset="100%" stop-color="#10B981" stop-opacity="0.0"/>
                      </linearGradient>
                    </defs>
                    <!-- Gradient Area -->
                    <path
                      d="M 0,44 Q 35,40 65,34 T 130,26 T 195,14 T 260,6 L 260,56 L 0,56 Z"
                      fill="url(#revenue-grad)"
                    />
                    <!-- Smooth Line -->
                    <path
                      d="M 0,44 Q 35,40 65,34 T 130,26 T 195,14 T 260,6"
                      fill="none"
                      stroke="#059669"
                      stroke-width="2.5"
                      stroke-linecap="round"
                    />
                    <!-- Peak Point Halo -->
                    <circle cx="260" cy="6" r="4" fill="#059669" class="animate-pulse"/>
                    <circle cx="260" cy="6" r="2" fill="#FFFFFF"/>
                  </svg>
                </div>
                <!-- Timeline Axis -->
                <div class="flex items-center justify-between text-[10px] font-medium text-slate-400 mt-1.5 px-0.5">
                  <span>Presale</span>
                  <span>Early</span>
                  <span>General</span>
                  <span>VIP</span>
                  <span class="text-slate-600 font-bold">Door</span>
                </div>
              </div>
            </div>

            <!-- Contextual & AI Insights Footer -->
            <div class="mt-4 pt-3.5 border-t border-slate-100 flex items-center justify-between gap-2">
              <div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-slate-50 border border-slate-200/60 text-[11px] text-slate-600 font-medium max-w-[85%] truncate">
                <span class="text-amber-500 shrink-0">💡</span>
                <span class="truncate">VIP velocity +22% after stage plan release</span>
              </div>
              <a
                routerLink="/organizer/events"
                class="text-[11px] font-bold text-slate-400 group-hover:text-emerald-700 transition-colors shrink-0"
                title="View breakdown"
              >
                →
              </a>
            </div>
          </div>

          <!-- Metric Card 2: Live Gate Check-in -->
          <div
            class="bg-white rounded-[24px] border border-stone-200/80 p-5 sm:p-6 shadow-[0_2px_8px_-2px_rgba(15,23,42,0.03),0_8px_20px_-4px_rgba(15,23,42,0.04)] flex flex-col justify-between hover:border-indigo-500/30 transition-all group"
          >
            <!-- Card Header: [Icon] Title + Context Badge -->
            <div>
              <div class="flex items-center justify-between mb-3.5">
                <div class="flex items-center gap-2.5">
                  <div class="w-9 h-9 rounded-2xl bg-indigo-50 text-indigo-700 flex items-center justify-center shrink-0">
                    <svg class="w-4.5 h-4.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                      <rect width="5" height="5" x="3" y="3" rx="1"/><rect width="5" height="5" x="16" y="3" rx="1"/><rect width="5" height="5" x="3" y="16" rx="1"/><path d="M21 16v3a2 2 0 0 1-2 2h-3M21 21v.01M12 7v3a2 2 0 0 1-2 2H7"/>
                    </svg>
                  </div>
                  <div>
                    <h3 class="text-xs font-bold uppercase tracking-wider text-slate-500">Gate Check-in</h3>
                    <div class="text-[11px] text-slate-400 font-medium">Turnstile ingress pace</div>
                  </div>
                </div>
                <span class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-indigo-50 text-indigo-700 border border-indigo-200/50">
                  Live Sync
                </span>
              </div>

              <!-- Key Metric / Stat Area -->
              <div class="space-y-1 mb-4">
                <div class="flex items-baseline justify-between gap-2">
                  <div class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight tabular-nums">
                    18,450 <span class="text-sm font-semibold text-slate-400">/ 22k</span>
                  </div>
                  <div class="inline-flex items-center gap-1 text-xs font-bold text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded-full">
                    <svg class="w-3 h-3" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                      <polyline points="18 15 12 9 6 15"/>
                    </svg>
                    <span>+8.2%/h</span>
                  </div>
                </div>
                <!-- Qualitative Label -->
                <p class="text-xs font-medium text-slate-600 flex items-center gap-1.5">
                  <span class="w-1.5 h-1.5 rounded-full bg-indigo-500"></span>
                  Fluid ingress • 83.8% scanned through
                </p>
              </div>

              <!-- Data Visualization: Rounded Pill Bars -->
              <div class="py-2">
                <div class="h-14 w-full flex items-end justify-between gap-1.5 px-1">
                  <!-- Bar 16:00 -->
                  <div class="flex-1 bg-indigo-100 rounded-t-lg transition-all hover:bg-indigo-300" style="height: 25%"></div>
                  <!-- Bar 17:00 -->
                  <div class="flex-1 bg-indigo-200 rounded-t-lg transition-all hover:bg-indigo-300" style="height: 48%"></div>
                  <!-- Bar 18:00 -->
                  <div class="flex-1 bg-indigo-400 rounded-t-lg transition-all hover:bg-indigo-500" style="height: 75%"></div>
                  <!-- Bar 19:00 (Peak) -->
                  <div class="flex-1 bg-indigo-600 rounded-t-lg transition-all hover:bg-indigo-700" style="height: 98%"></div>
                  <!-- Bar 20:00 -->
                  <div class="flex-1 bg-indigo-300 rounded-t-lg transition-all hover:bg-indigo-400" style="height: 58%"></div>
                  <!-- Bar Now -->
                  <div class="flex-1 bg-indigo-200 rounded-t-lg transition-all hover:bg-indigo-300" style="height: 32%"></div>
                </div>
                <!-- Timeline Axis -->
                <div class="flex items-center justify-between text-[10px] font-medium text-slate-400 mt-1.5 px-0.5">
                  <span>16:00</span>
                  <span>17:00</span>
                  <span>18:00</span>
                  <span class="text-indigo-600 font-bold">19:00</span>
                  <span>20:00</span>
                  <span>Now</span>
                </div>
              </div>
            </div>

            <!-- Contextual & AI Insights Footer -->
            <div class="mt-4 pt-3.5 border-t border-slate-100 flex items-center justify-between gap-2">
              <div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-slate-50 border border-slate-200/60 text-[11px] text-slate-600 font-medium max-w-[85%] truncate">
                <span class="text-amber-500 shrink-0">💡</span>
                <span class="truncate">Turnstile B & West corridor moving smoothly</span>
              </div>
              <a
                routerLink="/organizer/checkin"
                class="text-[11px] font-bold text-slate-400 group-hover:text-indigo-700 transition-colors shrink-0"
                title="View scanner terminal"
              >
                →
              </a>
            </div>
          </div>

          <!-- Metric Card 3: Active Inventory Holds -->
          <div
            class="bg-white rounded-[24px] border border-stone-200/80 p-5 sm:p-6 shadow-[0_2px_8px_-2px_rgba(15,23,42,0.03),0_8px_20px_-4px_rgba(15,23,42,0.04)] flex flex-col justify-between hover:border-amber-500/30 transition-all group"
          >
            <!-- Card Header: [Icon] Title + Context Badge -->
            <div>
              <div class="flex items-center justify-between mb-3.5">
                <div class="flex items-center gap-2.5">
                  <div class="w-9 h-9 rounded-2xl bg-amber-50 text-amber-800 flex items-center justify-center shrink-0">
                    <svg class="w-4.5 h-4.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                      <rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18M3 15h18M9 3v18M15 3v18"/>
                    </svg>
                  </div>
                  <div>
                    <h3 class="text-xs font-bold uppercase tracking-wider text-slate-500">Inventory Holds</h3>
                    <div class="text-[11px] text-slate-400 font-medium">Cart reservations buffer</div>
                  </div>
                </div>
                <span class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-amber-50 text-amber-800 border border-amber-200/50">
                  10m TTL
                </span>
              </div>

              <!-- Key Metric / Stat Area -->
              <div class="space-y-1 mb-4">
                <div class="flex items-baseline justify-between gap-2">
                  <div class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight tabular-nums">
                    412 <span class="text-sm font-semibold text-slate-400">Seats</span>
                  </div>
                  <!-- Soothing Trend (No Panic Red) -->
                  <div class="inline-flex items-center gap-1 text-xs font-bold text-slate-600 bg-slate-100 px-2 py-0.5 rounded-full">
                    <span>Stable</span>
                  </div>
                </div>
                <!-- Qualitative Label -->
                <p class="text-xs font-medium text-slate-600 flex items-center gap-1.5">
                  <span class="w-1.5 h-1.5 rounded-full bg-amber-500"></span>
                  Balanced rotation • 96.4% checkout lock rate
                </p>
              </div>

              <!-- Data Visualization: Steady Flow Wave Chart -->
              <div class="py-2">
                <div class="h-14 w-full">
                  <svg class="w-full h-full overflow-visible" viewBox="0 0 260 56" preserveAspectRatio="none">
                    <defs>
                      <linearGradient id="hold-grad" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stop-color="#F59E0B" stop-opacity="0.18"/>
                        <stop offset="100%" stop-color="#F59E0B" stop-opacity="0.0"/>
                      </linearGradient>
                    </defs>
                    <!-- Gradient Area -->
                    <path
                      d="M 0,32 C 45,24 85,38 130,26 C 175,18 215,34 260,24 L 260,56 L 0,56 Z"
                      fill="url(#hold-grad)"
                    />
                    <!-- Smooth Wave -->
                    <path
                      d="M 0,32 C 45,24 85,38 130,26 C 175,18 215,34 260,24"
                      fill="none"
                      stroke="#D97706"
                      stroke-width="2.5"
                      stroke-linecap="round"
                    />
                    <!-- Dot at Current Point -->
                    <circle cx="260" cy="24" r="3.5" fill="#D97706"/>
                  </svg>
                </div>
                <!-- Timeline Axis -->
                <div class="flex items-center justify-between text-[10px] font-medium text-slate-400 mt-1.5 px-0.5">
                  <span>10m</span>
                  <span>8m</span>
                  <span>6m</span>
                  <span>4m</span>
                  <span>2m</span>
                  <span class="text-amber-700 font-bold">Now</span>
                </div>
              </div>
            </div>

            <!-- Contextual & AI Insights Footer -->
            <div class="mt-4 pt-3.5 border-t border-slate-100 flex items-center justify-between gap-2">
              <div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-slate-50 border border-slate-200/60 text-[11px] text-slate-600 font-medium max-w-[85%] truncate">
                <span class="text-amber-500 shrink-0">💡</span>
                <span class="truncate">Expired holds smoothly recirculated within 400ms</span>
              </div>
              <a
                routerLink="/organizer/seat-map"
                class="text-[11px] font-bold text-slate-400 group-hover:text-amber-700 transition-colors shrink-0"
                title="View seat map holds"
              >
                →
              </a>
            </div>
          </div>

          <!-- Metric Card 4: Operational Staff & Security Coverage -->
          <div
            class="bg-white rounded-[24px] border border-stone-200/80 p-5 sm:p-6 shadow-[0_2px_8px_-2px_rgba(15,23,42,0.03),0_8px_20px_-4px_rgba(15,23,42,0.04)] flex flex-col justify-between hover:border-slate-400/40 transition-all group"
          >
            <!-- Card Header: [Icon] Title + Context Badge -->
            <div>
              <div class="flex items-center justify-between mb-3.5">
                <div class="flex items-center gap-2.5">
                  <div class="w-9 h-9 rounded-2xl bg-slate-100 text-slate-700 flex items-center justify-center shrink-0">
                    <svg class="w-4.5 h-4.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                      <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/>
                    </svg>
                  </div>
                  <div>
                    <h3 class="text-xs font-bold uppercase tracking-wider text-slate-500">Staff Active</h3>
                    <div class="text-[11px] text-slate-400 font-medium">Zone delegations & safety</div>
                  </div>
                </div>
                <span class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-slate-100 text-slate-700 border border-slate-200/60">
                  6/6 Zones
                </span>
              </div>

              <!-- Key Metric / Stat Area -->
              <div class="space-y-1 mb-4">
                <div class="flex items-baseline justify-between gap-2">
                  <div class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight tabular-nums">
                    48 <span class="text-sm font-semibold text-slate-400">Personnel</span>
                  </div>
                  <div class="inline-flex items-center gap-1 text-xs font-bold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-full">
                    <span>100% Ready</span>
                  </div>
                </div>
                <!-- Qualitative Label -->
                <p class="text-xs font-medium text-slate-600 flex items-center gap-1.5">
                  <span class="w-1.5 h-1.5 rounded-full bg-emerald-500"></span>
                  Full zone perimeter coverage
                </p>
              </div>

              <!-- Data Visualization: Zone Telemetry Meter Bars -->
              <div class="py-2">
                <div class="h-14 w-full flex items-end justify-between gap-2 px-1">
                  <!-- Gate A -->
                  <div class="flex-1 flex flex-col items-center gap-1">
                    <div class="w-full bg-emerald-100 rounded-t-lg transition-all" style="height: 38px"></div>
                  </div>
                  <!-- Gate B -->
                  <div class="flex-1 flex flex-col items-center gap-1">
                    <div class="w-full bg-emerald-200 rounded-t-lg transition-all" style="height: 44px"></div>
                  </div>
                  <!-- VIP -->
                  <div class="flex-1 flex flex-col items-center gap-1">
                    <div class="w-full bg-emerald-500 rounded-t-lg transition-all" style="height: 50px"></div>
                  </div>
                  <!-- Floor -->
                  <div class="flex-1 flex flex-col items-center gap-1">
                    <div class="w-full bg-emerald-300 rounded-t-lg transition-all" style="height: 42px"></div>
                  </div>
                  <!-- Perimeter -->
                  <div class="flex-1 flex flex-col items-center gap-1">
                    <div class="w-full bg-emerald-400 rounded-t-lg transition-all" style="height: 46px"></div>
                  </div>
                </div>
                <!-- Timeline / Zone Axis -->
                <div class="flex items-center justify-between text-[10px] font-medium text-slate-400 mt-1.5 px-0.5">
                  <span>Gate A</span>
                  <span>Gate B</span>
                  <span class="text-emerald-700 font-bold">VIP</span>
                  <span>Floor</span>
                  <span>Sec</span>
                </div>
              </div>
            </div>

            <!-- Contextual & AI Insights Footer -->
            <div class="mt-4 pt-3.5 border-t border-slate-100 flex items-center justify-between gap-2">
              <div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-slate-50 border border-slate-200/60 text-[11px] text-slate-600 font-medium max-w-[85%] truncate">
                <span class="text-amber-500 shrink-0">💡</span>
                <span class="truncate">Turnstile stations fully staffed for evening rush</span>
              </div>
              <a
                routerLink="/organizer/staff"
                class="text-[11px] font-bold text-slate-400 group-hover:text-slate-900 transition-colors shrink-0"
                title="View staff permissions"
              >
                →
              </a>
            </div>
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
export class OrganizerComponent {
  selectedPeriod: 'realtime' | 'today' | 'tour' = 'realtime';
}
