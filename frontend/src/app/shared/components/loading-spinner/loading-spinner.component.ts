import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  template: `
    <div class="flex flex-col items-center justify-center p-8 gap-3">
      <div class="w-8 h-8 border-4 border-brand/20 border-t-brand rounded-full animate-spin"></div>
      @if (message) {
        <p class="text-xs font-medium text-neutral-500">{{ message }}</p>
      }
    </div>
  `
})
export class LoadingSpinnerComponent {
  @Input() message = '';
}
