import { Routes } from '@angular/router';

export const routes: Routes = [
  // Customer Portal
  {
    path: '',
    loadComponent: () => import('./portals/customer/discovery/discovery.component').then(m => m.DiscoveryComponent)
  },
  {
    path: 'waiting-room/:eventId',
    loadComponent: () => import('./portals/customer/waiting-room/waiting-room.component').then(m => m.WaitingRoomComponent)
  },
  {
    path: 'seat-selection/:eventId',
    loadComponent: () => import('./portals/customer/seat-selection/seat-selection.component').then(m => m.SeatSelectionComponent)
  },
  {
    path: 'checkout',
    loadComponent: () => import('./portals/customer/checkout/checkout.component').then(m => m.CheckoutComponent)
  },
  {
    path: 'my-tickets',
    loadComponent: () => import('./portals/customer/my-tickets/my-tickets.component').then(m => m.MyTicketsComponent)
  },

  // Organizer Portal
  {
    path: 'organizer',
    children: [
      {
        path: '',
        loadComponent: () => import('./portals/organizer/dashboard/dashboard.component').then(m => m.OrganizerDashboardComponent)
      },
      {
        path: 'events',
        loadComponent: () => import('./portals/organizer/event-management/event-management.component').then(m => m.EventManagementComponent)
      },
      {
        path: 'events/create',
        loadComponent: () => import('./portals/organizer/event-management/event-management.component').then(m => m.EventManagementComponent)
      },
      {
        path: 'seat-map',
        loadComponent: () => import('./portals/organizer/seat-map-builder/seat-map-builder.component').then(m => m.SeatMapBuilderComponent)
      },
      {
        path: 'staff',
        loadComponent: () => import('./portals/organizer/staff-management/staff-management.component').then(m => m.StaffManagementComponent)
      },
      {
        path: 'checkin',
        loadComponent: () => import('./portals/organizer/checkin-scanner/checkin-scanner.component').then(m => m.CheckinScannerComponent)
      }
    ]
  },

  // Platform Admin Portal
  {
    path: 'admin',
    children: [
      {
        path: '',
        loadComponent: () => import('./portals/admin/event-moderation/event-moderation.component').then(m => m.EventModerationComponent)
      },
      {
        path: 'moderation',
        loadComponent: () => import('./portals/admin/event-moderation/event-moderation.component').then(m => m.EventModerationComponent)
      },
      {
        path: 'payouts',
        loadComponent: () => import('./portals/admin/finance-payouts/finance-payouts.component').then(m => m.FinancePayoutsComponent)
      },
      {
        path: 'audit-logs',
        loadComponent: () => import('./portals/admin/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent)
      },
      {
        path: 'system-health',
        loadComponent: () => import('./portals/admin/system-health/system-health.component').then(m => m.SystemHealthComponent)
      }
    ]
  },

  {
    path: '**',
    redirectTo: ''
  }
];
