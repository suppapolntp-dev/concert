import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  text: string;
}

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed bottom-4 right-4 z-50 flex flex-col gap-2 max-w-sm w-full">
      @for (toast of toasts(); track toast.id) {
        <div [class]="getToastClass(toast.type)">
          <span>{{ toast.text }}</span>
          <button (click)="removeToast(toast.id)" class="text-sm font-bold opacity-75 hover:opacity-100">&times;</button>
        </div>
      }
    </div>
  `
})
export class ToastComponent {
  readonly toasts = signal<ToastMessage[]>([]);

  show(text: string, type: ToastMessage['type'] = 'info'): void {
    const id = Math.random().toString(36).substring(2, 9);
    this.toasts.update((current) => [...current, { id, type, text }]);
    setTimeout(() => this.removeToast(id), 4000);
  }

  removeToast(id: string): void {
    this.toasts.update((current) => current.filter((t) => t.id !== id));
  }

  getToastClass(type: ToastMessage['type']): string {
    const base = 'px-4 py-3 rounded-xl shadow-lg flex items-center justify-between text-sm font-medium';
    switch (type) {
      case 'success':
        return `${base} bg-emerald-600 text-white`;
      case 'error':
        return `${base} bg-rose-600 text-white`;
      case 'warning':
        return `${base} bg-amber-500 text-white`;
      default:
        return `${base} bg-neutral-900 text-white`;
    }
  }
}
