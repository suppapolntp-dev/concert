import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-discovery',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-neutral-900">Explore Concerts</h1>
          <p class="text-sm text-neutral-500">Discover upcoming live shows and reserve your seats.</p>
        </div>
      </div>

      <!-- Search & Filters Skeleton -->
      <div class="flex gap-4">
        <input
          type="text"
          placeholder="Search by artist, venue, or concert name..."
          class="flex-1 px-4 py-2.5 rounded-xl border border-surface-300 text-sm focus:outline-none focus:ring-2 focus:ring-brand/30"
        />
        <button class="px-5 py-2.5 bg-brand text-white font-medium text-sm rounded-xl hover:bg-brand-dark transition-colors">
          Search
        </button>
      </div>

      <!-- Event Grid Skeleton -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div class="bg-white rounded-2xl border border-surface-200 p-5 shadow-sm space-y-4">
          <div class="h-44 bg-surface-100 rounded-xl flex items-center justify-center text-neutral-400">
            Event Banner Image
          </div>
          <div>
            <span class="text-xs font-semibold uppercase tracking-wider text-brand">Rock / Pop</span>
            <h2 class="text-lg font-bold text-neutral-900 mt-1">TicketWorld Festival 2026</h2>
            <p class="text-sm text-neutral-500">Impact Arena, Muang Thong Thani</p>
          </div>
          <div class="flex items-center justify-between pt-2 border-t border-surface-100">
            <span class="text-sm font-semibold text-neutral-900">฿1,500 - ฿6,500</span>
            <a
              [routerLink]="['/waiting-room', 'demo-event-1']"
              class="px-3 py-1.5 text-xs font-semibold bg-brand text-white rounded-lg hover:bg-brand-dark"
            >
              Buy Tickets
            </a>
          </div>
        </div>
      </div>
    </div>
  `
})
export class DiscoveryComponent {}
