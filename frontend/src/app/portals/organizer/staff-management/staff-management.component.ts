import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface StaffMember {
  id: string;
  name: string;
  email: string;
  role: 'Scanner' | 'Supervisor' | 'Manager';
  status: 'Active' | 'Pending';
}

@Component({
  selector: 'app-staff-management',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-neutral-900">Staff & Scanner Delegation</h1>
          <p class="text-sm text-neutral-500">Manage gatekeepers, scanner operators, and access roles.</p>
        </div>
        <button class="px-4 py-2 bg-brand text-white font-semibold text-sm rounded-xl hover:bg-brand-dark">
          + Invite Staff Member
        </button>
      </div>

      <div class="bg-white rounded-2xl border border-surface-200 overflow-hidden shadow-sm">
        <table class="w-full text-left text-sm text-neutral-600">
          <thead class="bg-surface-50 text-xs font-semibold uppercase text-neutral-500 border-b border-surface-200">
            <tr>
              <th class="px-6 py-3">Staff Name</th>
              <th class="px-6 py-3">Role</th>
              <th class="px-6 py-3">Status</th>
              <th class="px-6 py-3 text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-surface-100">
            @for (staff of staffList(); track staff.id) {
              <tr class="hover:bg-surface-50/50">
                <td class="px-6 py-4 font-medium text-neutral-900">
                  {{ staff.name }}
                  <div class="text-xs text-neutral-400">{{ staff.email }}</div>
                </td>
                <td class="px-6 py-4">{{ staff.role }}</td>
                <td class="px-6 py-4">
                  <span class="px-2 py-0.5 text-xs rounded-full bg-emerald-50 text-emerald-700 font-medium">{{ staff.status }}</span>
                </td>
                <td class="px-6 py-4 text-right">
                  <button class="text-xs text-red-600 hover:text-red-800 font-medium">Revoke</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `
})
export class StaffManagementComponent {
  readonly staffList = signal<StaffMember[]>([
    { id: '1', name: 'Kitti Chai', email: 'kitti@eventstaff.co.th', role: 'Scanner', status: 'Active' },
    { id: '2', name: 'Nattaporn S.', email: 'nat@eventstaff.co.th', role: 'Supervisor', status: 'Active' }
  ]);
}
