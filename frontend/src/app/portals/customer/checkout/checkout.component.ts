import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="max-w-2xl mx-auto space-y-6">
      <h1 class="text-2xl font-bold text-neutral-900">Checkout & Payment</h1>
      
      <!-- Order Summary Card -->
      <div class="bg-white rounded-2xl border border-surface-200 p-6 shadow-sm space-y-4">
        <h2 class="text-base font-semibold text-neutral-900 border-b border-surface-100 pb-3">Booking Summary</h2>
        <div class="space-y-2 text-sm">
          <div class="flex justify-between">
            <span class="text-neutral-500">Event:</span>
            <span class="font-medium text-neutral-900">TicketWorld Festival 2026</span>
          </div>
          <div class="flex justify-between">
            <span class="text-neutral-500">Seats:</span>
            <span class="font-medium text-neutral-900">Zone A (Row A - Seat 2)</span>
          </div>
          <div class="flex justify-between">
            <span class="text-neutral-500">Hold Timer:</span>
            <span class="font-bold text-amber-600">08:42 remaining</span>
          </div>
          <div class="border-t border-surface-100 pt-3 flex justify-between text-base font-bold">
            <span>Total Payable:</span>
            <span class="text-brand">฿3,500</span>
          </div>
        </div>
      </div>

      <!-- Payment Method Selection Skeleton -->
      <div class="bg-white rounded-2xl border border-surface-200 p-6 shadow-sm space-y-4">
        <h2 class="text-base font-semibold text-neutral-900">Select Payment Method</h2>
        <div class="grid grid-cols-2 gap-4">
          <label class="border-2 border-brand bg-brand/5 p-4 rounded-xl flex items-center gap-3 cursor-pointer">
            <input type="radio" name="payment" checked class="accent-brand" />
            <span class="text-sm font-semibold text-neutral-900">PromptPay QR</span>
          </label>
          <label class="border border-surface-200 p-4 rounded-xl flex items-center gap-3 cursor-pointer">
            <input type="radio" name="payment" class="accent-brand" />
            <span class="text-sm font-medium text-neutral-700">Credit / Debit Card</span>
          </label>
        </div>

        <button
          (click)="confirmPayment()"
          class="w-full mt-4 py-3 bg-brand text-white font-bold rounded-xl hover:bg-brand-dark transition-colors shadow-sm"
        >
          Confirm & Pay ฿3,500
        </button>
      </div>
    </div>
  `
})
export class CheckoutComponent {
  confirmPayment(): void {
    console.log('Initiated Payment Intent (skeleton)');
  }
}
