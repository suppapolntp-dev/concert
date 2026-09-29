import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface PayoutRequest {
  id: string;
  organizer: string;
  amount: number;
  bankAccount: string;
  status: 'Pending' | 'Processed' | 'Rejected';
}

@Component({
  selector: 'app-finance-payouts',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold text-neutral-900">Organizer Payout Requests</h1>
        <p class="text-sm text-neutral-500">Escrow settlement, commission calculation, and disbursement verification.</p>
      </div>

      <div class="bg-white rounded-2xl border border-surface-200 overflow-hidden shadow-sm">
        <table class="w-full text-left text-sm text-neutral-600">
          <thead class="bg-surface-50 text-xs font-semibold uppercase text-neutral-500 border-b border-surface-200">
            <tr>
              <th class="px-6 py-3">Payout ID</th>
              <th class="px-6 py-3">Organizer</th>
              <th class="px-6 py-3">Bank Details</th>
              <th class="px-6 py-3">Disbursement</th>
              <th class="px-6 py-3 text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-surface-100">
            @for (payout of payouts(); track payout.id) {
              <tr class="hover:bg-surface-50/50">
                <td class="px-6 py-4 font-mono text-xs">{{ payout.id }}</td>
                <td class="px-6 py-4 font-medium text-neutral-900">{{ payout.organizer }}</td>
                <td class="px-6 py-4 text-xs font-mono">{{ payout.bankAccount }}</td>
                <td class="px-6 py-4 font-bold text-neutral-900">฿{{ payout.amount | number }}</td>
                <td class="px-6 py-4 text-right space-x-2">
                  <button class="px-3 py-1 bg-brand text-white rounded-lg text-xs font-medium hover:bg-brand-dark">Execute Wire</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class FinancePayoutsComponent {
  readonly payouts = signal<PayoutRequest[]>([
    { id: 'PAY-8821', organizer: 'Live Nation TH', amount: 8400000, bankAccount: 'KBANK xxxx-4819', status: 'Pending' },
    { id: 'PAY-8822', organizer: 'BEC TERO Show', amount: 3250000, bankAccount: 'SCB xxxx-9120', status: 'Pending' }
  ]);
}
