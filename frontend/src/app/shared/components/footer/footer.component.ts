import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink],
  template: `
    <footer class="border-t border-surface-200 py-8 px-6 bg-white text-xs text-neutral-500">
      <div class="max-w-7xl mx-auto flex flex-col sm:flex-row items-center justify-between gap-4">
        <div class="flex items-center gap-2">
          <div class="w-6 h-6 rounded-lg bg-brand flex items-center justify-center text-white font-bold text-xs">
            T
          </div>
          <span class="font-bold text-neutral-800 text-sm">TicketWorld</span>
          <span>&copy; 2026. High-concurrency ticketing platform.</span>
        </div>

        <div class="flex flex-wrap items-center gap-4 text-xs">
          <a routerLink="/" class="hover:text-neutral-900 transition-colors">Concerts</a>
          <a routerLink="/organizer" class="hover:text-neutral-900 transition-colors">Organizer Hub</a>
          <a routerLink="/admin" class="hover:text-neutral-900 transition-colors">Platform Admin</a>
          <a routerLink="/test-uxui" class="text-brand font-semibold hover:underline">Dev Workbench</a>
        </div>
      </div>
    </footer>
  `
})
export class FooterComponent {}
