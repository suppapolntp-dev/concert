import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface CustomerTicket {
  id: string;
  eventName: string;
  dateTime: string;
  venue: string;
  seatInfo: string;
  qrPayload: string;
  status: 'Valid' | 'Used' | 'Transferred';
}

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <h1 class="text-2xl font-bold text-neutral-900">My Tickets & Passes</h1>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        @for (ticket of tickets(); track ticket.id) {
          <div class="bg-white rounded-2xl border border-surface-200 p-6 shadow-sm flex flex-col justify-between space-y-4">
            <div>
              <div class="flex items-center justify-between">
                <span class="text-xs px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-700 font-semibold">{{ ticket.status }}</span>
                <span class="text-xs text-neutral-400 font-mono">#{{ ticket.id }}</span>
              </div>
              <h3 class="text-lg font-bold text-neutral-900 mt-2">{{ ticket.eventName }}</h3>
              <p class="text-sm text-neutral-500">{{ ticket.dateTime }} &bull; {{ ticket.venue }}</p>
              <p class="text-sm font-semibold text-neutral-800 mt-1">{{ ticket.seatInfo }}</p>
            </div>

            <!-- QR Code Skeleton Preview -->
            <div class="p-4 bg-surface-50 border border-surface-100 rounded-xl flex items-center justify-center">
              <div class="w-36 h-36 bg-neutral-900 text-white rounded-lg flex items-center justify-center text-xs font-mono">
                [QR: {{ ticket.qrPayload }}]
              </div>
            </div>

            <div class="flex gap-2">
              <button class="flex-1 py-2 text-xs font-semibold rounded-lg bg-surface-100 hover:bg-surface-200 text-neutral-800 transition-colors">
                Transfer Ticket
              </button>
              <button class="flex-1 py-2 text-xs font-semibold rounded-lg bg-surface-100 hover:bg-surface-200 text-neutral-800 transition-colors">
                Download PDF
              </button>
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class MyTicketsComponent {
  readonly tickets = signal<CustomerTicket[]>([
    {
      id: 'TCK-2026-0091',
      eventName: 'TicketWorld Festival 2026',
      dateTime: '2026-11-20 19:00',
      venue: 'Impact Arena',
      seatInfo: 'Zone VIP - Row A - Seat 2',
      qrPayload: 'HMAC_SHA256:TCK-2026-0091',
      status: 'Valid'
    }
  ]);
}
