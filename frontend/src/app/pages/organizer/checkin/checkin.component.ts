import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-organizer-checkin-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div class="bg-white rounded-3xl border border-slate-200/80 p-6 sm:p-8 shadow-[0_1px_3px_0_rgba(15,23,42,0.03),0_6px_16px_-4px_rgba(15,23,42,0.04)]">
        <div class="flex items-center gap-3 mb-2">
          <div class="w-10 h-10 rounded-2xl bg-[#D7F4DF] text-[#006e2f] flex items-center justify-center shrink-0">
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
              <rect width="5" height="5" x="3" y="3" rx="1"/><rect width="5" height="5" x="16" y="3" rx="1"/><rect width="5" height="5" x="3" y="16" rx="1"/><path d="M21 16v3a2 2 0 0 1-2 2h-3M21 21v.01M12 7v3a2 2 0 0 1-2 2H7"/>
            </svg>
          </div>
          <div>
            <h1 class="text-xl font-black text-slate-900 tracking-tight">Check-in Scanner</h1>
            <p class="text-xs text-slate-500 font-medium">Turnstile scanner interface and real-time admission verification</p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class OrganizerCheckinComponent {}
