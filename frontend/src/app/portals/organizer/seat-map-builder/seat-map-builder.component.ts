import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-seat-map-builder',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-2xl font-bold text-neutral-900">Seat Map Builder</h1>
          <p class="text-sm text-neutral-500">Drag-and-drop SVG layout designer for tiers, rows, and seats.</p>
        </div>
        <button class="px-4 py-2 bg-brand text-white font-semibold text-sm rounded-xl hover:bg-brand-dark">
          Save Layout
        </button>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-4 gap-6">
        <!-- Tools Palette -->
        <div class="bg-white p-5 rounded-2xl border border-surface-200 shadow-sm space-y-4">
          <h3 class="font-bold text-sm text-neutral-900">Tier Setup</h3>
          <div class="space-y-2">
            <button class="w-full py-2 px-3 text-left text-xs font-semibold rounded-lg bg-emerald-50 text-emerald-800 border border-emerald-200">
              Zone VIP (฿4,500)
            </button>
            <button class="w-full py-2 px-3 text-left text-xs font-semibold rounded-lg bg-blue-50 text-blue-800 border border-blue-200">
              Zone A (฿3,000)
            </button>
            <button class="w-full py-2 px-3 text-left text-xs font-semibold rounded-lg bg-purple-50 text-purple-800 border border-purple-200">
              Zone B (฿1,800)
            </button>
          </div>
        </div>

        <!-- SVG Design Canvas Skeleton -->
        <div class="lg:col-span-3 bg-white p-6 rounded-2xl border border-surface-200 shadow-sm min-h-[400px] flex items-center justify-center">
          <div class="text-center text-neutral-400">
            <p class="font-medium">SVG Seating Grid Workspace</p>
            <p class="text-xs mt-1">(Coordinate mapping & grid snap tools)</p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class SeatMapBuilderComponent {}
