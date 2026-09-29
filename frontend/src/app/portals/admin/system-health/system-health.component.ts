import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ServiceHealth {
  name: string;
  status: 'Healthy' | 'Degraded' | 'Down';
  latencyMs: number;
}

@Component({
  selector: 'app-system-health',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold text-neutral-900">APM & Infrastructure Health</h1>
        <p class="text-sm text-neutral-500">Live telemetry for PostgreSQL, Redis Cluster, SignalR and Hangfire workers.</p>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        @for (srv of services(); track srv.name) {
          <div class="bg-white p-5 rounded-2xl border border-surface-200 shadow-sm space-y-3">
            <div class="flex items-center justify-between">
              <span class="font-bold text-neutral-900 text-sm">{{ srv.name }}</span>
              <span class="w-3 h-3 rounded-full"
                    [class]="srv.status === 'Healthy' ? 'bg-emerald-500' : 'bg-rose-500'"></span>
            </div>
            <div class="text-xs text-neutral-500">Latency: {{ srv.latencyMs }}ms</div>
            <div class="text-xs px-2 py-1 rounded bg-surface-100 font-semibold inline-block">
              {{ srv.status }}
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class SystemHealthComponent {
  readonly services = signal<ServiceHealth[]>([
    { name: 'PostgreSQL Primary', status: 'Healthy', latencyMs: 2 },
    { name: 'Redis RedLock Cluster', status: 'Healthy', latencyMs: 1 },
    { name: 'SignalR Socket Gateway', status: 'Healthy', latencyMs: 8 },
    { name: 'Hangfire Background Workers', status: 'Healthy', latencyMs: 12 }
  ]);
}
