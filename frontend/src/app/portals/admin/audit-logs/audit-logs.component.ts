import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface AuditRecord {
  id: string;
  timestamp: string;
  actor: string;
  action: string;
  target: string;
  ipAddress: string;
}

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold text-neutral-900">Security & Compliance Audit Trail</h1>
        <p class="text-sm text-neutral-500">Immutable chronological log of privileged operations (PDPA / PCI-DSS compliance).</p>
      </div>

      <div class="bg-white rounded-2xl border border-surface-200 overflow-hidden shadow-sm">
        <table class="w-full text-left text-sm text-neutral-600">
          <thead class="bg-surface-50 text-xs font-semibold uppercase text-neutral-500 border-b border-surface-200">
            <tr>
              <th class="px-6 py-3">Timestamp</th>
              <th class="px-6 py-3">Actor</th>
              <th class="px-6 py-3">Action</th>
              <th class="px-6 py-3">Target</th>
              <th class="px-6 py-3">IP Address</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-surface-100 font-mono text-xs">
            @for (log of auditLogs(); track log.id) {
              <tr class="hover:bg-surface-50/50">
                <td class="px-6 py-3 text-neutral-500">{{ log.timestamp }}</td>
                <td class="px-6 py-3 font-semibold text-neutral-900">{{ log.actor }}</td>
                <td class="px-6 py-3 text-brand">{{ log.action }}</td>
                <td class="px-6 py-3">{{ log.target }}</td>
                <td class="px-6 py-3 text-neutral-400">{{ log.ipAddress }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class AuditLogsComponent {
  readonly auditLogs = signal<AuditRecord[]>([
    { id: '1', timestamp: '2026-09-24 14:12:05', actor: 'admin@ticketworld.io', action: 'EVENT_STATUS_APPROVED', target: 'Event:ev-1', ipAddress: '192.168.1.5' },
    { id: '2', timestamp: '2026-09-24 13:45:10', actor: 'sec-guard@ticketworld.io', action: 'PAYOUT_KYC_VERIFIED', target: 'Organizer:org-88', ipAddress: '192.168.1.18' }
  ]);
}
