import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-checkin-scanner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="max-w-md mx-auto space-y-6">
      <div class="text-center">
        <h1 class="text-2xl font-bold text-neutral-900">QR Check-In Scanner</h1>
        <p class="text-sm text-neutral-500">Venue Gate Entry Validation</p>
      </div>

      <!-- WebRTC Camera Viewport Skeleton -->
      <div class="relative bg-black rounded-3xl overflow-hidden aspect-[3/4] flex flex-col items-center justify-center p-6 text-white text-center shadow-lg">
        <div class="w-64 h-64 border-2 border-brand/80 border-dashed rounded-2xl flex items-center justify-center relative animate-pulse">
          <span class="text-xs text-brand tracking-wider font-mono">ALIGN QR CODE HERE</span>
        </div>
        <p class="text-xs text-neutral-400 mt-6">WebRTC navigator.mediaDevices.getUserMedia</p>
      </div>

      <!-- Scan Result Alert Skeleton -->
      @if (lastScanStatus()) {
        <div [class]="lastScanStatus() === 'VALID' ? 'bg-emerald-50 text-emerald-800 border-emerald-200' : 'bg-rose-50 text-rose-800 border-rose-200'"
             class="p-4 rounded-2xl border text-center font-bold">
          {{ lastScanStatus() === 'VALID' ? '✅ ADMIT: Gate A Passed' : '❌ REJECT: Already Scanned or Invalid' }}
        </div>
      }

      <div class="flex gap-3">
        <button (click)="simulateScan('VALID')" class="flex-1 py-2.5 bg-emerald-600 text-white rounded-xl text-xs font-semibold hover:bg-emerald-700">
          Simulate Valid Scan
        </button>
        <button (click)="simulateScan('INVALID')" class="flex-1 py-2.5 bg-rose-600 text-white rounded-xl text-xs font-semibold hover:bg-rose-700">
          Simulate Invalid Scan
        </button>
      </div>
    </div>
  `
})
export class CheckinScannerComponent {
  readonly lastScanStatus = signal<'VALID' | 'INVALID' | null>(null);

  simulateScan(status: 'VALID' | 'INVALID'): void {
    this.lastScanStatus.set(status);
  }
}
