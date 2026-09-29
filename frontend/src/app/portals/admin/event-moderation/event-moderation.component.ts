import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface PendingEvent {
  id: string;
  title: string;
  organizerName: string;
  submittedAt: string;
  estimatedRevenue: number;
}

@Component({
  selector: 'app-event-moderation',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold text-neutral-900">Event Moderation & Approvals</h1>
        <p class="text-sm text-neutral-500">Review organizer event listings before public sale launch.</p>
      </div>

      <div class="bg-white rounded-2xl border border-surface-200 overflow-hidden shadow-sm">
        <table class="w-full text-left text-sm text-neutral-600">
          <thead class="bg-surface-50 text-xs font-semibold uppercase text-neutral-500 border-b border-surface-200">
            <tr>
              <th class="px-6 py-3">Event</th>
              <th class="px-6 py-3">Organizer</th>
              <th class="px-6 py-3">Est. Revenue</th>
              <th class="px-6 py-3 text-right">Moderation Action</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-surface-100">
            @for (event of pendingEvents(); track event.id) {
              <tr class="hover:bg-surface-50/50">
                <td class="px-6 py-4 font-medium text-neutral-900">{{ event.title }}</td>
                <td class="px-6 py-4">{{ event.organizerName }}</td>
                <td class="px-6 py-4 font-semibold">฿{{ event.estimatedRevenue | number }}</td>
                <td class="px-6 py-4 text-right space-x-2">
                  <button class="px-3 py-1 bg-emerald-600 text-white rounded-lg text-xs font-medium hover:bg-emerald-700">Approve</button>
                  <button class="px-3 py-1 bg-rose-600 text-white rounded-lg text-xs font-medium hover:bg-rose-700">Reject</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class EventModerationComponent {
  readonly pendingEvents = signal<PendingEvent[]>([
    { id: 'ev-1', title: 'Neon Lights EDM Fest', organizerName: 'SoundWave Live Co.', submittedAt: '2026-09-24', estimatedRevenue: 12000000 },
    { id: 'ev-2', title: 'Acoustic Sunday Bangkok', organizerName: 'Indie Brew Asia', submittedAt: '2026-09-23', estimatedRevenue: 450000 }
  ]);
}
