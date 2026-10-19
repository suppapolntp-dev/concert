import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

export type MetricTheme = 'emerald' | 'indigo' | 'amber' | 'slate';
export type MetricChartType = 'sparkline-revenue' | 'bars-checkin' | 'wave-hold' | 'meter-staff' | 'custom';

export interface MetricCardData {
  id: string;
  title: string;
  subtitle: string;
  value: string;
  unit?: string;
  badgeText: string;
  trendText?: string;
  trendDirection?: 'up' | 'down' | 'neutral';
  qualitativeText: string;
  insightText: string;
  linkUrl: string;
  linkTitle?: string;
  theme: MetricTheme;
  chartType: MetricChartType;
  timeAxisLabels?: string[];
  iconType?: 'currency' | 'scanner' | 'seats' | 'staff';
}

@Component({
  selector: 'app-metric-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div
      class="bg-white rounded-[24px] border border-stone-200/80 p-5 sm:p-6 shadow-[0_2px_8px_-2px_rgba(15,23,42,0.03),0_8px_20px_-4px_rgba(15,23,42,0.04)] flex flex-col justify-between transition-all group"
      [ngClass]="cardBorderHoverClass"
    >
      <div>
        <!-- 1. Card Header: [Icon] Title + Context Badge -->
        <div class="flex items-center justify-between mb-3.5">
          <div class="flex items-center gap-2.5">
            <div
              class="w-9 h-9 rounded-2xl flex items-center justify-center shrink-0"
              [ngClass]="iconContainerClass"
            >
              <!-- Icon Switch -->
              @switch (cardData.iconType) {
                @case ('currency') {
                  <svg class="w-4.5 h-4.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="12" y1="1" x2="12" y2="23"/>
                    <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/>
                  </svg>
                }
                @case ('scanner') {
                  <svg class="w-4.5 h-4.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                    <rect width="5" height="5" x="3" y="3" rx="1"/><rect width="5" height="5" x="16" y="3" rx="1"/><rect width="5" height="5" x="3" y="16" rx="1"/><path d="M21 16v3a2 2 0 0 1-2 2h-3M21 21v.01M12 7v3a2 2 0 0 1-2 2H7"/>
                  </svg>
                }
                @case ('seats') {
                  <svg class="w-4.5 h-4.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                    <rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18M3 15h18M9 3v18M15 3v18"/>
                  </svg>
                }
                @case ('staff') {
                  <svg class="w-4.5 h-4.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/>
                  </svg>
                }
                @default {
                  <ng-content select="[card-icon]"></ng-content>
                }
              }
            </div>
            <div>
              <h3 class="text-xs font-bold uppercase tracking-wider text-slate-500">{{ cardData.title }}</h3>
              <div class="text-[11px] text-slate-400 font-medium">{{ cardData.subtitle }}</div>
            </div>
          </div>
          <span
            class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold border"
            [ngClass]="badgeClass"
          >
            {{ cardData.badgeText }}
          </span>
        </div>

        <!-- 2. Key Metric / Stat Area -->
        <div class="space-y-1 mb-4">
          <div class="flex items-baseline justify-between gap-2">
            <div class="text-2xl sm:text-3xl font-black text-slate-900 tracking-tight tabular-nums">
              {{ cardData.value }}
              @if (cardData.unit) {
                <span class="text-sm font-semibold text-slate-400">{{ cardData.unit }}</span>
              }
            </div>

            <!-- Trend Indicator Pill -->
            @if (cardData.trendText) {
              <div
                class="inline-flex items-center gap-1 text-xs font-bold px-2 py-0.5 rounded-full"
                [ngClass]="trendPillClass"
              >
                @if (cardData.trendDirection === 'up') {
                  <svg class="w-3 h-3" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                    <polyline points="18 15 12 9 6 15"/>
                  </svg>
                } @else if (cardData.trendDirection === 'down') {
                  <svg class="w-3 h-3" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                    <polyline points="6 9 12 15 18 9"/>
                  </svg>
                }
                <span>{{ cardData.trendText }}</span>
              </div>
            }
          </div>

          <!-- Qualitative Label -->
          <p class="text-xs font-medium text-slate-600 flex items-center gap-1.5">
            <span class="w-1.5 h-1.5 rounded-full" [ngClass]="qualitativeDotClass"></span>
            {{ cardData.qualitativeText }}
          </p>
        </div>

        <!-- 3. Data Visualization (Chart Area) -->
        <div class="py-2">
          @switch (cardData.chartType) {
            @case ('sparkline-revenue') {
              <div class="h-14 w-full">
                <svg class="w-full h-full overflow-visible" viewBox="0 0 260 56" preserveAspectRatio="none">
                  <defs>
                    <linearGradient id="revenue-grad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stop-color="#10B981" stop-opacity="0.22"/>
                      <stop offset="100%" stop-color="#10B981" stop-opacity="0.0"/>
                    </linearGradient>
                  </defs>
                  <path
                    d="M 0,44 Q 35,40 65,34 T 130,26 T 195,14 T 260,6 L 260,56 L 0,56 Z"
                    fill="url(#revenue-grad)"
                  />
                  <path
                    d="M 0,44 Q 35,40 65,34 T 130,26 T 195,14 T 260,6"
                    fill="none"
                    stroke="#059669"
                    stroke-width="2.5"
                    stroke-linecap="round"
                  />
                  <circle cx="260" cy="6" r="4" fill="#059669" class="animate-pulse"/>
                  <circle cx="260" cy="6" r="2" fill="#FFFFFF"/>
                </svg>
              </div>
            }

            @case ('bars-checkin') {
              <div class="h-14 w-full flex items-end justify-between gap-1.5 px-1">
                <div class="flex-1 bg-indigo-100 rounded-t-lg transition-all hover:bg-indigo-300" style="height: 25%"></div>
                <div class="flex-1 bg-indigo-200 rounded-t-lg transition-all hover:bg-indigo-300" style="height: 48%"></div>
                <div class="flex-1 bg-indigo-400 rounded-t-lg transition-all hover:bg-indigo-500" style="height: 75%"></div>
                <div class="flex-1 bg-indigo-600 rounded-t-lg transition-all hover:bg-indigo-700" style="height: 98%"></div>
                <div class="flex-1 bg-indigo-300 rounded-t-lg transition-all hover:bg-indigo-400" style="height: 58%"></div>
                <div class="flex-1 bg-indigo-200 rounded-t-lg transition-all hover:bg-indigo-300" style="height: 32%"></div>
              </div>
            }

            @case ('wave-hold') {
              <div class="h-14 w-full">
                <svg class="w-full h-full overflow-visible" viewBox="0 0 260 56" preserveAspectRatio="none">
                  <defs>
                    <linearGradient id="hold-grad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stop-color="#F59E0B" stop-opacity="0.18"/>
                      <stop offset="100%" stop-color="#F59E0B" stop-opacity="0.0"/>
                    </linearGradient>
                  </defs>
                  <path
                    d="M 0,32 C 45,24 85,38 130,26 C 175,18 215,34 260,24 L 260,56 L 0,56 Z"
                    fill="url(#hold-grad)"
                  />
                  <path
                    d="M 0,32 C 45,24 85,38 130,26 C 175,18 215,34 260,24"
                    fill="none"
                    stroke="#D97706"
                    stroke-width="2.5"
                    stroke-linecap="round"
                  />
                  <circle cx="260" cy="24" r="3.5" fill="#D97706"/>
                </svg>
              </div>
            }

            @case ('meter-staff') {
              <div class="h-14 w-full flex items-end justify-between gap-2 px-1">
                <div class="flex-1 flex flex-col items-center gap-1">
                  <div class="w-full bg-emerald-100 rounded-t-lg transition-all" style="height: 38px"></div>
                </div>
                <div class="flex-1 flex flex-col items-center gap-1">
                  <div class="w-full bg-emerald-200 rounded-t-lg transition-all" style="height: 44px"></div>
                </div>
                <div class="flex-1 flex flex-col items-center gap-1">
                  <div class="w-full bg-emerald-500 rounded-t-lg transition-all" style="height: 50px"></div>
                </div>
                <div class="flex-1 flex flex-col items-center gap-1">
                  <div class="w-full bg-emerald-300 rounded-t-lg transition-all" style="height: 42px"></div>
                </div>
                <div class="flex-1 flex flex-col items-center gap-1">
                  <div class="w-full bg-emerald-400 rounded-t-lg transition-all" style="height: 46px"></div>
                </div>
              </div>
            }

            @default {
              <ng-content select="[chart]"></ng-content>
            }
          }

          <!-- Timeline Axis -->
          @if (cardData.timeAxisLabels && cardData.timeAxisLabels.length > 0) {
            <div class="flex items-center justify-between text-[10px] font-medium text-slate-400 mt-1.5 px-0.5">
              @for (label of cardData.timeAxisLabels; track $index; let last = $last) {
                <span [class.font-bold]="last" [class.text-slate-600]="last">{{ label }}</span>
              }
            </div>
          }
        </div>
      </div>

      <!-- 4. Contextual & AI Insights Footer -->
      <div class="mt-4 pt-3.5 border-t border-slate-100 flex items-center justify-between gap-2">
        <div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-slate-50 border border-slate-200/60 text-[11px] text-slate-600 font-medium max-w-[85%] truncate">
          <span class="text-amber-500 shrink-0">💡</span>
          <span class="truncate">{{ cardData.insightText }}</span>
        </div>
        <a
          [routerLink]="cardData.linkUrl"
          class="text-[11px] font-bold text-slate-400 transition-colors shrink-0"
          [ngClass]="linkHoverClass"
          [title]="cardData.linkTitle || 'View details'"
        >
          →
        </a>
      </div>
    </div>
  `
})
export class MetricCardComponent {
  @Input({ required: true }) cardData!: MetricCardData;

  get cardBorderHoverClass(): string {
    switch (this.cardData.theme) {
      case 'emerald':
        return 'hover:border-emerald-500/30';
      case 'indigo':
        return 'hover:border-indigo-500/30';
      case 'amber':
        return 'hover:border-amber-500/30';
      case 'slate':
      default:
        return 'hover:border-slate-400/40';
    }
  }

  get iconContainerClass(): string {
    switch (this.cardData.theme) {
      case 'emerald':
        return 'bg-[#D7F4DF] text-[#006e2f]';
      case 'indigo':
        return 'bg-indigo-50 text-indigo-700';
      case 'amber':
        return 'bg-amber-50 text-amber-800';
      case 'slate':
      default:
        return 'bg-slate-100 text-slate-700';
    }
  }

  get badgeClass(): string {
    switch (this.cardData.theme) {
      case 'emerald':
        return 'bg-emerald-50 text-emerald-700 border-emerald-200/50';
      case 'indigo':
        return 'bg-indigo-50 text-indigo-700 border-indigo-200/50';
      case 'amber':
        return 'bg-amber-50 text-amber-800 border-amber-200/50';
      case 'slate':
      default:
        return 'bg-slate-100 text-slate-700 border-slate-200/60';
    }
  }

  get trendPillClass(): string {
    switch (this.cardData.theme) {
      case 'emerald':
        return 'text-emerald-600 bg-emerald-50';
      case 'indigo':
        return 'text-indigo-600 bg-indigo-50';
      case 'amber':
      case 'slate':
      default:
        return 'text-slate-600 bg-slate-100';
    }
  }

  get qualitativeDotClass(): string {
    switch (this.cardData.theme) {
      case 'emerald':
        return 'bg-emerald-500';
      case 'indigo':
        return 'bg-indigo-500';
      case 'amber':
        return 'bg-amber-500';
      case 'slate':
      default:
        return 'bg-emerald-500';
    }
  }

  get linkHoverClass(): string {
    switch (this.cardData.theme) {
      case 'emerald':
        return 'group-hover:text-emerald-700';
      case 'indigo':
        return 'group-hover:text-indigo-700';
      case 'amber':
        return 'group-hover:text-amber-700';
      case 'slate':
      default:
        return 'group-hover:text-slate-900';
    }
  }
}
