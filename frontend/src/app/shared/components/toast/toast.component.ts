import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-5 right-5 z-50 flex flex-col gap-2 max-w-sm w-full pointer-events-none">
      @for (toast of toastService.toasts(); track toast.id) {
        <div [class]="getToastClass(toast.type)" class="pointer-events-auto transform transition-all duration-300 ease-out shadow-lg">
          <div class="flex items-center gap-2">
            @if (toast.type === 'success') {
              <span class="text-base font-bold">✓</span>
            } @else if (toast.type === 'error') {
              <span class="text-base font-bold">✕</span>
            } @else if (toast.type === 'warning') {
              <span class="text-base font-bold">⚠</span>
            } @else {
              <span class="text-base font-bold">ℹ</span>
            }
            <span class="text-sm font-medium">{{ toast.text }}</span>
          </div>
          <button (click)="toastService.remove(toast.id)" class="text-sm font-bold opacity-70 hover:opacity-100 px-1">&times;</button>
        </div>
      }
    </div>
  `
})
export class ToastComponent {
  readonly toastService = inject(ToastService);

  getToastClass(type: ToastMessage['type']): string {
    const base = 'px-4 py-3 rounded-xl flex items-center justify-between text-sm font-medium border';
    switch (type) {
      case 'success':
        return `${base} bg-emerald-500 text-white border-emerald-600`;
      case 'error':
        return `${base} bg-rose-500 text-white border-rose-600`;
      case 'warning':
        return `${base} bg-amber-500 text-white border-amber-600`;
      default:
        return `${base} bg-neutral-900 text-white border-neutral-800`;
    }
  }
}
