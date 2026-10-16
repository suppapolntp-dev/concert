import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';

interface EventCard {
  title: string;
  artist: string;
  venue: string;
  day: string;
  month: string;
  category: string;
  price: number;
  image: string;
  tag?: string;
}

@Component({
  selector: 'app-customer-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="home-shell">
      <section class="welcome-row">
        <div>
          <p class="eyebrow"><span class="live-dot"></span> YOUR NEXT LIVE MOMENT</p>
          <h1>Good music.<br class="mobile-break"> Better nights.</h1>
          <p class="welcome-copy">Find the shows worth leaving home for.</p>
        </div>
        <a class="tickets-shortcut" href="/my-tickets"><span class="ticket-icon">▤</span><span><b>Your tickets</b><small>Ready when you are</small></span><span class="arrow">↗</span></a>
      </section>

      <section class="search-panel" aria-label="Find an event">
        <div class="search-input-wrap"><span class="search-icon" aria-hidden="true">⌕</span><input aria-label="Search events" placeholder="Artist, event, or venue" [value]="query()" (input)="query.set($any($event.target).value)" (keydown.enter)="scrollToResults()"><kbd>↵</kbd></div>
        <div class="search-divider"></div>
        <label class="location-picker"><span class="pin">⌖</span><span><small>YOUR CITY</small><b>Bangkok</b></span><span class="chevron">⌄</span></label>
        <button class="search-button" type="button" (click)="scrollToResults()">Find events <span>→</span></button>
      </section>

      <section class="feature" aria-label="Featured event">
        <div class="feature-image"></div><div class="feature-overlay"></div>
        <div class="feature-content"><span class="feature-kicker"><span class="sun">✳</span> THE SHOW EVERYONE'S TALKING ABOUT</span><h2>One night.<br>All the feelings.</h2><p>Three hours of live music, lights down low, and a room full of strangers singing every word.</p><div class="feature-meta"><span>◷ &nbsp; SAT, NOV 21 · 7:30 PM</span><span>⌖ &nbsp; IMPACT ARENA</span></div><a class="feature-cta" href="#events">Explore the lineup <span>↗</span></a></div>
        <div class="feature-index"><span>01</span><i></i><span>03</span></div>
      </section>

      <section class="discover-section">
        <div class="section-head"><div><p class="eyebrow section-eyebrow">A GOOD PLACE TO START</p><h2>Find your kind of night<span class="mint-period">.</span></h2></div><a class="browse-link" href="#events">Browse all events <span>→</span></a></div>
        <div class="category-row" aria-label="Filter by event category">
          @for (category of categories; track category) {
            <button type="button" class="category-chip" [class.selected]="activeCategory() === category" (click)="activeCategory.set(category)">{{ category }}</button>
          }
        </div>
        <div id="events" class="event-grid">
          @for (event of filteredEvents(); track event.title) {
            <article class="event-card"><a class="event-image-link" href="/events"><div class="event-image" [style.background-image]="'url(' + event.image + ')'"><span class="event-image-shade"></span>
              @if (event.tag) { <span class="event-tag">{{ event.tag }}</span> }
              <div class="date-badge"><b>{{ event.day }}</b><small>{{ event.month }}</small></div>
            </div></a><div class="event-details"><span class="event-category">{{ event.category }}</span><h3>{{ event.title }}</h3><p class="artist-name">{{ event.artist }}</p><div class="event-bottom"><span class="venue">⌖ &nbsp;{{ event.venue }}</span><span class="price">from <b>฿{{ event.price | number }}</b></span></div></div></article>
          } @empty { <div class="empty-state"><span>⌕</span><h3>No shows found just yet.</h3><p>Try another search or category — your next night out is out there.</p><button type="button" (click)="query.set(''); activeCategory.set('All events')">Clear filters</button></div> }
        </div>
      </section>

      <section class="bottom-note"><span class="note-star">✳</span><p>Plans change. Great nights don't have to.<br><small>Secure booking · Straightforward checkout · Tickets in your pocket</small></p><a href="#events" aria-label="Explore all events">↗</a></section>
    </div>
  `,
  styleUrl: './customer.component.css'
})
export class CustomerComponent {
  query = signal('');
  activeCategory = signal('All events');
  readonly categories = ['All events', 'Concerts', 'Festivals', 'Live music', 'Theatre', 'Comedy'];
  readonly events: EventCard[] = [
    { title: 'The Eras of Us', artist: 'NONT TANONT', venue: 'IMPACT Arena', day: '21', month: 'NOV', category: 'Concerts', price: 2500, image: 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=900&q=80', tag: 'SELLING FAST' },
    { title: 'Neon Garden Festival', artist: 'A weekend under the lights', venue: 'Rajamangala Stadium', day: '28', month: 'NOV', category: 'Festivals', price: 1800, image: 'https://images.unsplash.com/photo-1459749411175-04bf5292ceea?auto=format&fit=crop&w=900&q=80', tag: '2-DAY FESTIVAL' },
    { title: 'An Evening in Bloom', artist: 'INK WARUNTORN', venue: 'Queen Sirikit Convention Centre', day: '05', month: 'DEC', category: 'Live music', price: 1200, image: 'https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=900&q=80' },
    { title: 'After Hours, Live', artist: 'TILLY BIRDS', venue: 'Lido Connect', day: '12', month: 'DEC', category: 'Concerts', price: 950, image: 'https://images.unsplash.com/photo-1506157786151-b8491531f063?auto=format&fit=crop&w=900&q=80', tag: 'JUST ANNOUNCED' },
    { title: 'The Room Where We Laugh', artist: 'A night of live stand-up', venue: 'M Theater', day: '19', month: 'DEC', category: 'Comedy', price: 650, image: 'https://images.unsplash.com/photo-1524368535928-5b5e00ddc76b?auto=format&fit=crop&w=900&q=80' },
    { title: 'A Little More Broadway', artist: 'Bangkok Theatre Collective', venue: 'Thailand Cultural Centre', day: '09', month: 'JAN', category: 'Theatre', price: 1600, image: 'https://images.unsplash.com/photo-1503095396549-807759245b35?auto=format&fit=crop&w=900&q=80' }
  ];
  readonly filteredEvents = computed(() => {
    const term = this.query().trim().toLowerCase();
    return this.events.filter(event => {
      const matchesCategory = this.activeCategory() === 'All events' || event.category === this.activeCategory();
      const matchesQuery = !term || `${event.title} ${event.artist} ${event.venue} ${event.category}`.toLowerCase().includes(term);
      return matchesCategory && matchesQuery;
    });
  });
  scrollToResults(): void {
    document.getElementById('events')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}
