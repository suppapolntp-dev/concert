import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-organizer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-neutral-900">Organizer Dashboard</h1>
          <p class="text-sm text-neutral-500">Real-time ticket sales metrics and event overview.</p>
        </div>
        <div class="flex gap-3">
          <a routerLink="/organizer/events/create" class="px-4 py-2 bg-brand text-white text-sm font-semibold rounded-xl hover:bg-brand-dark transition-colors">
            + Create New Event
          </a>
          <a routerLink="/organizer/checkin" class="px-4 py-2 bg-surface-200 text-neutral-800 text-sm font-semibold rounded-xl hover:bg-surface-300 transition-colors">
            Gate Check-In
          </a>
        </div>
      </div>

      <!-- KPI Summary Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div class="bg-white p-5 rounded-2xl border border-surface-200 shadow-sm">
          <span class="text-xs font-semibold text-neutral-400 uppercase">Gross Revenue</span>
          <div class="text-2xl font-black text-neutral-900 mt-2">฿4,280,000</div>
          <span class="text-xs text-emerald-600 font-medium">+14.2% from last week</span>
        </div>
        <div class="bg-white p-5 rounded-2xl border border-surface-200 shadow-sm">
          <span class="text-xs font-semibold text-neutral-400 uppercase">Tickets Sold</span>
          <div class="text-2xl font-black text-neutral-900 mt-2">1,842 / 2,000</div>
          <span class="text-xs text-brand font-medium">92.1% Capacity</span>
        </div>
        <div class="bg-white p-5 rounded-2xl border border-surface-200 shadow-sm">
          <span class="text-xs font-semibold text-neutral-400 uppercase">Active Sessions</span>
          <div class="text-2xl font-black text-neutral-900 mt-2">328 Users</div>
          <span class="text-xs text-neutral-500 font-medium">In waiting room</span>
        </div>
        <div class="bg-white p-5 rounded-2xl border border-surface-200 shadow-sm">
          <span class="text-xs font-semibold text-neutral-400 uppercase">Checked In</span>
          <div class="text-2xl font-black text-neutral-900 mt-2">0</div>
          <span class="text-xs text-neutral-400 font-medium">Gates not yet open</span>
        </div>
      </div>

      <!-- Quick Navigations -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <a routerLink="/organizer/seat-map" class="p-5 bg-white rounded-2xl border border-surface-200 shadow-sm hover:border-brand transition-all block">
          <h3 class="font-bold text-neutral-900">Seat Map Designer</h3>
          <p class="text-xs text-neutral-500 mt-1">Configure interactive seating tiers and SVG coordinates.</p>
        </a>
        <a routerLink="/organizer/staff" class="p-5 bg-white rounded-2xl border border-surface-200 shadow-sm hover:border-brand transition-all block">
          <h3 class="font-bold text-neutral-900">Staff & Permissions</h3>
          <p class="text-xs text-neutral-500 mt-1">Invite gatekeepers and scanner operators.</p>
        </a>
        <a routerLink="/organizer/events" class="p-5 bg-white rounded-2xl border border-surface-200 shadow-sm hover:border-brand transition-all block">
          <h3 class="font-bold text-neutral-900">Manage Events</h3>
          <p class="text-xs text-neutral-500 mt-1">Edit pricing, schedules, and publish statuses.</p>
        </a>
      </div>
    </div>
  `
})
export class OrganizerDashboardComponent {}
