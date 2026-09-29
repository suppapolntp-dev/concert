import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <header class="h-16 border-b border-surface-200 bg-white/90 backdrop-blur-md sticky top-0 z-50 px-6 flex items-center justify-between">
      <div class="flex items-center gap-3">
        <a routerLink="/" class="flex items-center gap-2">
          <div class="w-9 h-9 rounded-xl bg-brand flex items-center justify-center text-white font-bold text-lg shadow-sm">
            T
          </div>
          <span class="font-bold text-lg tracking-tight text-neutral-900">TicketWorld</span>
          <span class="text-xs px-2 py-0.5 rounded-full bg-brand/10 text-brand font-medium">v2.1</span>
        </a>
      </div>

      <nav class="hidden md:flex items-center gap-6 text-sm font-medium text-neutral-600">
        <a routerLink="/" routerLinkActive="text-brand font-semibold" [routerLinkActiveOptions]="{ exact: true }" class="hover:text-brand transition-colors">Concerts</a>
        <a routerLink="/organizer" routerLinkActive="text-brand font-semibold" class="hover:text-brand transition-colors">Organizer</a>
        <a routerLink="/admin" routerLinkActive="text-brand font-semibold" class="hover:text-brand transition-colors">Admin</a>
      </nav>

      <div class="flex items-center gap-3">
        @if (auth.isAuthenticated()) {
          <a routerLink="/my-tickets" class="px-3 py-1.5 text-sm font-medium text-neutral-700 hover:text-neutral-900">My Tickets</a>
          <button (click)="auth.logout()" class="px-3 py-1.5 text-sm font-medium text-red-600 hover:text-red-700">Logout</button>
        } @else {
          <button class="px-4 py-2 text-sm font-medium text-neutral-700 hover:text-neutral-900 transition-colors">
            Sign In
          </button>
          <button class="px-4 py-2 text-sm font-semibold text-white bg-brand hover:bg-brand-dark rounded-lg shadow-sm transition-colors">
            Get Started
          </button>
        }
      </div>
    </header>
  `
})
export class NavbarComponent {
  readonly auth = inject(AuthService);
}
