import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface EventCardData {
  id?: string | number;
  title: string;
  artist: string;
  venue: string;
  day: string;
  month: string;
  category: string;
  price: number;
  image: string;
  tag?: string;
  isFavorite?: boolean;
}

@Component({
  selector: 'app-event-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <article
      class="group relative flex flex-col overflow-hidden rounded-2xl bg-white border border-slate-200/80 shadow-[0_2px_8px_-2px_rgba(15,23,42,0.06)] hover:shadow-[0_12px_24px_-4px_rgba(15,23,42,0.12)] hover:-translate-y-1 transition-all duration-300 cursor-pointer"
      (click)="onCardClick()"
    >
      <!-- Media Header -->
      <div class="relative w-full aspect-[16/10] overflow-hidden bg-slate-100">
        <img
          [src]="event.image"
          [alt]="event.title"
          class="h-full w-full object-cover transition-transform duration-500 ease-out group-hover:scale-105"
          loading="lazy"
        />
        <div class="absolute inset-0 bg-gradient-to-t from-black/60 via-black/10 to-transparent opacity-80"></div>

        <!-- Date Squircle Badge -->
        <div class="absolute top-3 left-3 flex flex-col items-center justify-center min-w-[48px] px-2.5 py-1.5 rounded-xl bg-white/95 backdrop-blur-md shadow-md text-slate-900 border border-white/50">
          <span class="text-sm font-black leading-none tracking-tight">{{ event.day }}</span>
          <span class="text-[10px] font-bold tracking-wider uppercase text-emerald-600 leading-none mt-0.5">{{ event.month }}</span>
        </div>

        <!-- Top Right Actions: Tag & Favorite -->
        <div class="absolute top-3 right-3 flex items-center gap-2">
          @if (event.tag) {
            <span
              class="inline-flex items-center px-2.5 py-1 rounded-full text-[10px] font-bold tracking-wider uppercase backdrop-blur-md shadow-sm"
              [ngClass]="tagClasses"
            >
              {{ event.tag }}
            </span>
          }
          <button
            type="button"
            class="w-8 h-8 rounded-full bg-black/40 hover:bg-black/60 backdrop-blur-md text-white flex items-center justify-center transition-transform active:scale-90"
            [class.text-rose-500]="isFav"
            (click)="onFavoriteClick($event)"
            [attr.aria-label]="isFav ? 'Remove from favorites' : 'Add to favorites'"
          >
            <svg
              class="w-4 h-4 transition-colors"
              [attr.fill]="isFav ? 'currentColor' : 'none'"
              viewBox="0 0 24 24"
              stroke="currentColor"
              stroke-width="2"
            >
              <path stroke-linecap="round" stroke-linejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
            </svg>
          </button>
        </div>

        <!-- Category Floating Pill -->
        <div class="absolute bottom-3 left-3">
          <span class="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-emerald-500/90 text-white backdrop-blur-md shadow-sm">
            {{ event.category }}
          </span>
        </div>
      </div>

      <!-- Content Area -->
      <div class="flex flex-1 flex-col p-4 sm:p-5">
        <h3 class="text-base sm:text-lg font-bold text-slate-900 group-hover:text-emerald-700 transition-colors line-clamp-1">
          {{ event.title }}
        </h3>
        
        <p class="text-xs sm:text-sm font-medium text-slate-600 mt-1 line-clamp-1 flex items-center gap-1.5">
          <svg class="w-3.5 h-3.5 text-slate-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3" />
          </svg>
          <span>{{ event.artist }}</span>
        </p>

        <p class="text-xs text-slate-500 mt-2 line-clamp-1 flex items-center gap-1.5">
          <svg class="w-3.5 h-3.5 text-slate-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          <span>{{ event.venue }}</span>
        </p>

        <!-- Divider & Footer -->
        <div class="mt-4 pt-3.5 border-t border-slate-100 flex items-center justify-between">
          <div>
            <span class="block text-[11px] font-medium text-slate-400 uppercase tracking-wider">Starts from</span>
            <div class="text-base sm:text-lg font-black text-slate-900 leading-none">
              ฿{{ event.price | number }}
            </div>
          </div>

          <button
            type="button"
            class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-slate-900 text-white text-xs font-semibold group-hover:bg-emerald-600 transition-colors shadow-sm"
          >
            <span>Book</span>
            <svg class="w-3.5 h-3.5 group-hover:translate-x-0.5 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 5l7 7m0 0l-7 7m7-7H3" />
            </svg>
          </button>
        </div>
      </div>
    </article>
  `
})
export class EventCardComponent {
  @Input({ required: true }) event!: EventCardData;
  @Output() cardClick = new EventEmitter<EventCardData>();
  @Output() favoriteToggle = new EventEmitter<{ event: EventCardData; isFavorite: boolean }>();

  isFav = false;

  get tagClasses(): string {
    const tag = (this.event?.tag || '').toUpperCase();
    if (tag.includes('FAST') || tag.includes('HOT')) {
      return 'bg-amber-500/90 text-white';
    }
    if (tag.includes('SOLD')) {
      return 'bg-rose-500/90 text-white';
    }
    return 'bg-emerald-500/90 text-white';
  }

  onCardClick(): void {
    this.cardClick.emit(this.event);
  }

  onFavoriteClick(e: MouseEvent): void {
    e.stopPropagation();
    this.isFav = !this.isFav;
    this.favoriteToggle.emit({ event: this.event, isFavorite: this.isFav });
  }
}
