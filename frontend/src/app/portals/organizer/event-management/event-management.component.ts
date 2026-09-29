import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-event-management',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-neutral-900">Event Management</h1>
          <p class="text-sm text-neutral-500">Configure event details, ticket tiers, and sales windows.</p>
        </div>
        <button class="px-4 py-2 bg-brand text-white font-semibold text-sm rounded-xl hover:bg-brand-dark">
          + New Event
        </button>
      </div>

      <div class="bg-white rounded-2xl border border-surface-200 p-6 shadow-sm">
        <h2 class="text-base font-semibold text-neutral-900 mb-4">Event Form Skeleton</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div>
            <label class="block font-medium text-neutral-700 mb-1">Event Title</label>
            <input type="text" placeholder="e.g. Symphony of Dreams Live" class="w-full px-3 py-2 border border-surface-300 rounded-lg" />
          </div>
          <div>
            <label class="block font-medium text-neutral-700 mb-1">Venue</label>
            <input type="text" placeholder="e.g. Royal Paragon Hall" class="w-full px-3 py-2 border border-surface-300 rounded-lg" />
          </div>
          <div>
            <label class="block font-medium text-neutral-700 mb-1">Sale Start Date</label>
            <input type="datetime-local" class="w-full px-3 py-2 border border-surface-300 rounded-lg" />
          </div>
          <div>
            <label class="block font-medium text-neutral-700 mb-1">Event Date</label>
            <input type="datetime-local" class="w-full px-3 py-2 border border-surface-300 rounded-lg" />
          </div>
        </div>
      </div>
    </div>
  `
})
export class EventManagementComponent {}
