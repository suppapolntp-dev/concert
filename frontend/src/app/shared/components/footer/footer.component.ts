import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: true,
  template: `
    <footer class="border-t border-surface-200 py-6 px-6 text-center text-xs text-neutral-500 bg-white">
      TicketWorld Enterprise Ticketing Platform &copy; 2026. All rights reserved.
    </footer>
  `
})
export class FooterComponent {}
