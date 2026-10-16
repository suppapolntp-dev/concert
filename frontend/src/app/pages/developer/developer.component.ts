import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-developer-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen bg-slate-900 text-slate-100 p-8 font-sans antialiased">
      <div class="max-w-5xl mx-auto space-y-6">
        <!-- Header -->
        <div class="flex items-center justify-between border-b border-slate-800 pb-6">
          <div>
            <div class="flex items-center gap-2">
              <span class="text-xs font-mono uppercase tracking-wider px-2 py-0.5 rounded bg-emerald-500/20 text-emerald-400 border border-emerald-500/30">
                Dev Environment
              </span>
              <span class="text-xs text-slate-400">TicketWorld Studio</span>
            </div>
            <h1 class="text-3xl font-bold tracking-tight text-white mt-2">Developer Workbench</h1>
            <p class="text-sm text-slate-400 mt-1">
              หมวดสำหรับนักพัฒนา: ทดสอบ UI Kit, ตรวจสอบ Component, หรือ Mock Data
            </p>
          </div>

          <div class="flex items-center gap-3">
            <a
              routerLink="/organizer"
              class="px-4 py-2 rounded-xl text-xs font-semibold bg-emerald-600 hover:bg-emerald-500 text-white transition-colors"
            >
              เปิดดู Organizer Layout & Sidebar &rarr;
            </a>
          </div>
        </div>

        <!-- Quick Links Grid -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 pt-2">
          <!-- Customer Portal Card -->
          <div class="p-6 rounded-2xl bg-slate-800/60 border border-slate-700/60 space-y-3">
            <div class="w-10 h-10 rounded-xl bg-blue-500/10 text-blue-400 flex items-center justify-center font-bold text-lg">
              👤
            </div>
            <h3 class="text-base font-semibold text-white">Customer Pages</h3>
            <p class="text-xs text-slate-400">
              โครงสร้างหน้าฝั่งลูกค้า (Discovery, Seats, Checkout, My Tickets)
            </p>
            <a routerLink="/" class="inline-block text-xs text-blue-400 hover:text-blue-300 font-medium pt-2">
              เปิดหน้า Customer &rarr;
            </a>
          </div>

          <!-- Organizer Portal Card -->
          <div class="p-6 rounded-2xl bg-slate-800/60 border border-slate-700/60 space-y-3">
            <div class="w-10 h-10 rounded-xl bg-emerald-500/10 text-emerald-400 flex items-center justify-center font-bold text-lg">
              📊
            </div>
            <h3 class="text-base font-semibold text-white">Organizer Pages</h3>
            <p class="text-xs text-slate-400">
              โครงสร้างหน้าฝั่งผู้จัดงาน พร้อม Sidebar Navigation
            </p>
            <a routerLink="/organizer" class="inline-block text-xs text-emerald-400 hover:text-emerald-300 font-medium pt-2">
              เปิดหน้า Organizer (Sidebar) &rarr;
            </a>
          </div>

          <!-- Developer Sandbox Card -->
          <div class="p-6 rounded-2xl bg-slate-800/60 border border-slate-700/60 space-y-3">
            <div class="w-10 h-10 rounded-xl bg-amber-500/10 text-amber-400 flex items-center justify-center font-bold text-lg">
              🛠️
            </div>
            <h3 class="text-base font-semibold text-white">Dev Sandbox</h3>
            <p class="text-xs text-slate-400">
              พื้นที่ทดลองวาง component และ preview หน้าต่างๆ
            </p>
            <span class="inline-block text-xs text-amber-400/80 font-mono pt-2">
              Ready for experiments
            </span>
          </div>
        </div>
      </div>
    </div>
  `
})
export class DeveloperComponent {}
