import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (isOpen) {
      <div class="fixed inset-0 z-50 overflow-y-auto bg-black/60 flex items-center justify-center p-4">
        <div class="bg-white rounded-2xl max-w-lg w-full p-6 shadow-xl relative">
          <div class="flex items-center justify-between pb-4 border-b border-surface-200">
            <h3 class="text-lg font-semibold text-neutral-900">{{ title }}</h3>
            <button (click)="close.emit()" class="text-neutral-400 hover:text-neutral-600 text-xl font-bold">&times;</button>
          </div>
          <div class="py-4">
            <ng-content></ng-content>
          </div>
        </div>
      </div>
    }
  `
})
export class ModalComponent {
  @Input() isOpen = false;
  @Input() title = '';
  @Output() close = new EventEmitter<void>();
}
