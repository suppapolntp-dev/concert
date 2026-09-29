import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

export type SeatStatus = 'Available' | 'HeldSelf' | 'HeldOther' | 'Sold';

export interface SeatItem {
  id: string;
  row: string;
  number: number;
  price: number;
  status: SeatStatus;
  cx: number;
  cy: number;
}

@Component({
  selector: 'app-seat-selection',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-neutral-900">Select Your Seats</h1>
          <p class="text-sm text-neutral-500">Interactive SVG Seat Map with real-time locking.</p>
        </div>
        <div class="flex items-center gap-4 text-xs font-medium">
          <span class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-emerald-500 inline-block"></span> Available</span>
          <span class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-amber-400 inline-block"></span> Selected (You)</span>
          <span class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-neutral-300 inline-block"></span> Held (Other)</span>
          <span class="flex items-center gap-1.5"><span class="w-3 h-3 rounded-full bg-rose-500 inline-block"></span> Sold Out</span>
        </div>
      </div>

      <!-- SVG Stage & Seat Map Skeleton Canvas -->
      <div class="bg-white rounded-3xl border border-surface-200 p-8 shadow-sm overflow-x-auto flex flex-col items-center">
        <!-- Stage Representation -->
        <div class="w-2/3 h-12 bg-neutral-800 text-white rounded-xl flex items-center justify-center font-bold tracking-widest text-sm mb-12 shadow-sm">
          STAGE / PERFORMANCE AREA
        </div>

        <svg viewBox="0 0 600 200" class="w-full max-w-2xl h-auto border border-dashed border-surface-200 rounded-xl p-4">
          @for (seat of seats(); track seat.id) {
            <circle
              [attr.cx]="seat.cx"
              [attr.cy]="seat.cy"
              r="12"
              [class]="getSeatClass(seat.status)"
              (click)="toggleSeat(seat)"
            />
          }
        </svg>
      </div>

      <!-- Selected Seats Footer & Checkout Action -->
      <div class="bg-white rounded-2xl border border-surface-200 p-5 flex items-center justify-between shadow-sm">
        <div>
          <div class="text-xs text-neutral-500">Selected Seats: {{ selectedSeats().length }} / 6</div>
          <div class="text-xl font-bold text-neutral-900">Total: ฿{{ totalPrice() }}</div>
        </div>
        <a
          routerLink="/checkout"
          class="px-6 py-2.5 bg-brand text-white font-semibold text-sm rounded-xl hover:bg-brand-dark transition-colors"
        >
          Proceed to Checkout
        </a>
      </div>
    </div>
  `
})
export class SeatSelectionComponent {
  readonly seats = signal<SeatItem[]>([
    { id: 'A1', row: 'A', number: 1, price: 3500, status: 'Available', cx: 100, cy: 50 },
    { id: 'A2', row: 'A', number: 2, price: 3500, status: 'HeldSelf', cx: 150, cy: 50 },
    { id: 'A3', row: 'A', number: 3, price: 3500, status: 'HeldOther', cx: 200, cy: 50 },
    { id: 'A4', row: 'A', number: 4, price: 3500, status: 'Sold', cx: 250, cy: 50 },
    { id: 'B1', row: 'B', number: 1, price: 2500, status: 'Available', cx: 100, cy: 100 },
    { id: 'B2', row: 'B', number: 2, price: 2500, status: 'Available', cx: 150, cy: 100 },
    { id: 'B3', row: 'B', number: 3, price: 2500, status: 'Available', cx: 200, cy: 100 },
    { id: 'B4', row: 'B', number: 4, price: 2500, status: 'Available', cx: 250, cy: 100 }
  ]);

  readonly selectedSeats = signal<SeatItem[]>([]);
  readonly totalPrice = signal<number>(3500);

  getSeatClass(status: SeatStatus): string {
    switch (status) {
      case 'Available':
        return 'fill-emerald-500 hover:fill-emerald-600 cursor-pointer transition-colors';
      case 'HeldSelf':
        return 'fill-amber-400 cursor-pointer ring-2 ring-amber-300';
      case 'HeldOther':
        return 'fill-neutral-300 cursor-not-allowed';
      case 'Sold':
        return 'fill-rose-500 cursor-not-allowed';
    }
  }

  toggleSeat(seat: SeatItem): void {
    if (seat.status === 'Sold' || seat.status === 'HeldOther') return;
    // Skeleton toggling
    console.log('Toggled seat', seat.id);
  }
}
