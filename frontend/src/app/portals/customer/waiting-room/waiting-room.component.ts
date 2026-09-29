import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-waiting-room',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="max-w-xl mx-auto py-12 px-6 bg-white rounded-3xl border border-surface-200 shadow-sm text-center space-y-6">
      <div class="inline-flex p-3 rounded-2xl bg-amber-50 text-amber-600">
        <svg class="w-8 h-8 animate-pulse" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
      </div>

      <div>
        <h1 class="text-2xl font-bold text-neutral-900">Virtual Waiting Room</h1>
        <p class="text-sm text-neutral-500 mt-1">Please do not refresh this page. You are in queue.</p>
      </div>

      <div class="p-6 bg-surface-50 rounded-2xl border border-surface-200 space-y-3">
        <div class="text-xs uppercase font-semibold text-neutral-500">Your Position in Queue</div>
        <div class="text-5xl font-black text-brand tracking-tight">#{{ queuePosition() }}</div>
        <div class="text-xs text-neutral-400">Estimated wait time: ~{{ estimatedWaitSeconds() }}s</div>
      </div>

      <div class="w-full bg-surface-200 h-2 rounded-full overflow-hidden">
        <div class="bg-brand h-full transition-all duration-500" [style.width.%]="progress()"></div>
      </div>

      <div class="pt-4">
        <a
          [routerLink]="['/seat-selection', 'demo-event-1']"
          class="inline-block px-6 py-2.5 bg-brand text-white font-semibold text-sm rounded-xl hover:bg-brand-dark transition-colors"
        >
          Proceed to Seat Selection (Simulate Queue Pass)
        </a>
      </div>
    </div>
  `
})
export class WaitingRoomComponent implements OnInit {
  readonly queuePosition = signal<number>(142);
  readonly estimatedWaitSeconds = signal<number>(25);
  readonly progress = signal<number>(65);

  ngOnInit(): void {
    // Skeleton heartbeat / polling simulation
  }
}
