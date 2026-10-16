import { Routes } from '@angular/router';
import { CustomerLayoutComponent } from './shared/layouts/customer-layout/customer-layout.component';
import { OrganizerLayoutComponent } from './shared/layouts/organizer-layout/organizer-layout.component';

export const routes: Routes = [
  // 1. หมวด Customer (Customer Pages with CustomerLayout & Navbar)
  {
    path: '',
    component: CustomerLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/customer/customer.component').then(m => m.CustomerComponent)
      }
    ]
  },

  // 2. หมวด Organizer (Organizer Pages - ใช้งานร่วมกับ Sidebar Component)
  {
    path: 'organizer',
    component: OrganizerLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/organizer/organizer.component').then(m => m.OrganizerComponent)
      },
      {
        path: 'events',
        loadComponent: () =>
          import('./pages/organizer/events/events.component').then(m => m.OrganizerEventsComponent)
      },
      {
        path: 'events/create',
        loadComponent: () =>
          import('./pages/organizer/events/events.component').then(m => m.OrganizerEventsComponent)
      },
      {
        path: 'seat-map',
        loadComponent: () =>
          import('./pages/organizer/seat-map/seat-map.component').then(m => m.OrganizerSeatMapComponent)
      },
      {
        path: 'checkin',
        loadComponent: () =>
          import('./pages/organizer/checkin/checkin.component').then(m => m.OrganizerCheckinComponent)
      },
      {
        path: 'staff',
        loadComponent: () =>
          import('./pages/organizer/staff/staff.component').then(m => m.OrganizerStaffComponent)
      }
    ]
  },

  // 3. หมวด Developer (Developer Tools & Sandbox)
  {
    path: 'developer',
    loadComponent: () =>
      import('./pages/developer/developer.component').then(m => m.DeveloperComponent)
  },
  {
    path: 'dev',
    redirectTo: 'developer',
    pathMatch: 'full'
  },
  {
    path: 'test-uxui',
    redirectTo: 'developer',
    pathMatch: 'full'
  },

  // Fallback wildcard
  {
    path: '**',
    redirectTo: ''
  }
];
