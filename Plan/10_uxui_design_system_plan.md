# 🎯 UX/UI Design System & Visual Blueprint

> **Platform:** TicketWorld — Multi-Portal Global Ticketing  
> **Framework:** Angular 18 + Tailwind CSS 3.4  
> **Design Philosophy:** Minimal, High-Contrast, Data-Dense yet Breathable  
> **Core Principles:** Limited Palette · Clear Typography · Generous Whitespace · Pixel Alignment · Consistent Components · Predictable Interactions

---

## 1. Design Principles (6 Pillars)

| # | Principle | Actionable Rule |
| :---: | :--- | :--- |
| 1 | **สีไม่เยอะ** | ใช้เฉพาะ 1 Primary + 1 Accent + Neutrals เท่านั้น ห้ามเกิน 5 สีหลัก |
| 2 | **Typography ชัด** | ใช้ฟอนต์ตระกูลเดียว แบ่ง hierarchy ด้วย weight + size เท่านั้น ไม่ใช้หลายฟอนต์ |
| 3 | **Whitespace เยอะพอดี** | ใช้ spacing scale คงที่ (4px base grid) ทุก padding/margin ต้องหาร 4 ลงตัว |
| 4 | **Alignment แม่น** | Grid 12 คอลัมน์ + ทุก element ต้อง snap to grid ห้ามวางอิสระ |
| 5 | **Component consistent** | ปุ่ม/ฟอร์ม/การ์ดทุกที่ต้องเป็นตัวเดียวกัน ใช้ Design Token กลาง |
| 6 | **Interaction predictable** | ผู้ใช้กดปุ่มแล้วต้องเดาผลลัพธ์ได้ Hover/Loading/Success/Error state ครบ |

---

## 2. Color System (Tailwind Design Tokens)

### 2.1 Core Palette (5 สีหลักเท่านั้น)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         COLOR PALETTE                                   │
│                                                                         │
│  Primary (Brand)     ██████  #6366F1  Indigo-500  (CTA, Links, Focus)   │
│  Primary Dark        ██████  #4338CA  Indigo-700  (Hover, Active)       │
│                                                                         │
│  Accent (Highlight)  ██████  #F59E0B  Amber-500   (Badges, Warnings)   │
│                                                                         │
│  Success             ██████  #10B981  Emerald-500 (Confirmed, Available)│
│  Danger              ██████  #EF4444  Red-500     (Error, Sold Out)     │
│                                                                         │
│  Neutral-50          ██████  #FAFAFA              (Page Background)     │
│  Neutral-100         ██████  #F5F5F5              (Card Background)     │
│  Neutral-200         ██████  #E5E5E5              (Borders, Dividers)   │
│  Neutral-500         ██████  #737373              (Secondary Text)      │
│  Neutral-800         ██████  #262626              (Primary Text)        │
│  Neutral-950         ██████  #0A0A0A              (Headings)            │
│                                                                         │
│  Dark Mode Override:                                                    │
│  Surface-900         ██████  #171717              (Dark BG)             │
│  Surface-800         ██████  #262626              (Dark Card BG)        │
│  Surface-700         ██████  #404040              (Dark Border)         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Tailwind Config Extension
```js
// tailwind.config.js
module.exports = {
  theme: {
    extend: {
      colors: {
        brand:   { DEFAULT: '#6366F1', dark: '#4338CA', light: '#A5B4FC' },
        accent:  { DEFAULT: '#F59E0B', dark: '#D97706' },
        success: { DEFAULT: '#10B981' },
        danger:  { DEFAULT: '#EF4444' },
        surface: { 50: '#FAFAFA', 100: '#F5F5F5', 800: '#262626', 900: '#171717' },
      },
    },
  },
};
```

### 2.3 Semantic Usage Map (เมื่อไหร่ใช้สีอะไร)

| Context | Light Mode | Dark Mode | ห้ามทำ |
| :--- | :--- | :--- | :--- |
| **Page Background** | `surface-50` | `surface-900` | ❌ ห้ามใช้ Pure White `#FFFFFF` |
| **Card / Panel** | `white` | `surface-800` | ❌ ห้ามใส่ Shadow เกิน `shadow-sm` |
| **Primary Button** | `brand` text `white` | `brand` text `white` | ❌ ห้ามใช้ gradient |
| **Secondary Button** | `white` border `neutral-200` | `surface-800` border `surface-700` | ❌ ห้ามใช้ outline-dashed |
| **Destructive Action** | `danger` text `white` | `danger` text `white` | ❌ ห้ามใช้สีแดงกับ non-destructive action |
| **Available Seat** | `success` | `success` | ❌ ห้ามใช้สีน้ำเงินกับที่นั่งว่าง |
| **Held Seat (Self)** | `accent` (Amber) | `accent` | — |
| **Booked/Sold Out** | `danger` opacity 60% | `danger` opacity 40% | — |
| **Body Text** | `neutral-800` | `neutral-200` | ❌ ห้ามใช้ pure black `#000` |
| **Secondary Text** | `neutral-500` | `neutral-400` | ❌ ห้ามต่ำกว่า WCAG AA contrast |
| **Links** | `brand` | `brand-light` | ❌ ห้ามใช้สีเดียวกับ body text |

---

## 3. Typography System

### 3.1 Font Stack
```css
/* Single Font Family Only — No Mixing */
--font-sans: 'Inter', 'Noto Sans Thai', system-ui, -apple-system, sans-serif;
--font-mono: 'JetBrains Mono', 'Fira Code', monospace;  /* Code/QR/Reference only */
```

### 3.2 Type Scale (Major Third — 1.25 ratio)

| Token | Size | Weight | Line Height | Use Case |
| :--- | :---: | :---: | :---: | :--- |
| `display` | 36px / 2.25rem | 700 Bold | 1.2 | Hero banner headline |
| `h1` | 30px / 1.875rem | 700 Bold | 1.25 | Page title |
| `h2` | 24px / 1.5rem | 600 Semi | 1.3 | Section heading |
| `h3` | 20px / 1.25rem | 600 Semi | 1.35 | Card title, Modal title |
| `h4` | 16px / 1rem | 600 Semi | 1.4 | Sub-section heading |
| `body` | 14px / 0.875rem | 400 Regular | 1.5 | Default body text |
| `body-sm` | 13px / 0.8125rem | 400 Regular | 1.5 | Secondary descriptions |
| `caption` | 12px / 0.75rem | 400 Regular | 1.4 | Timestamps, metadata |
| `overline` | 11px / 0.6875rem | 500 Medium | 1.4 | Labels, uppercase tags |
| `mono` | 13px / 0.8125rem | 400 Regular | 1.5 | Booking ref, QR payload |

### 3.3 Typography Rules
* ❌ ห้ามใช้ bold + uppercase + สีพร้อมกัน (เลือกวิธีเน้นได้แค่ 1-2 อย่าง)
* ❌ ห้ามเกิน 3 ขนาดตัวอักษรใน Component เดียว
* ✅ Body text line-height ต้อง ≥ 1.5 เพื่อ readability
* ✅ Heading ใช้ letter-spacing `-0.02em` เพื่อความกระชับ

---

## 4. Spacing & Layout Grid

### 4.1 Spacing Scale (4px Base Unit)

```
  4px   ──  space-1   (Inline padding, icon gap)
  8px   ──  space-2   (Input internal padding, tight gap)
 12px   ──  space-3   (Between related elements)
 16px   ──  space-4   (Card internal padding, form field gap)
 20px   ──  space-5   (Section padding mobile)
 24px   ──  space-6   (Card padding desktop)
 32px   ──  space-8   (Between sections)
 48px   ──  space-12  (Major section breaks)
 64px   ──  space-16  (Page top/bottom margin)
 96px   ──  space-24  (Hero section vertical padding)
```

> **กฎเหล็ก:** ทุก margin/padding ต้องเป็นค่าจาก scale ข้างบนเท่านั้น ห้ามใช้ค่ากำหนดเอง เช่น `13px`, `22px`, `37px`

### 4.2 Layout Grid
* **Desktop (≥1280px):** 12-column grid, gutter `24px`, max-width `1280px`, centered.
* **Tablet (768-1279px):** 8-column grid, gutter `16px`.
* **Mobile (< 768px):** 4-column grid, gutter `16px`, padding-x `16px`.

### 4.3 Container & Content Width

| Surface | Width | Tailwind Class | Usage |
| :--- | :--- | :--- | :--- |
| Full-width Container | 1280px max | `max-w-7xl mx-auto px-6` | Page container |
| Content Area | 960px max | `max-w-5xl` | Text-heavy pages (Checkout, Profile) |
| Narrow Content | 560px max | `max-w-lg` | Login, Register, 2FA forms |
| Modal Dialog (S) | 400px | `max-w-sm` | Confirmation dialog |
| Modal Dialog (M) | 560px | `max-w-lg` | Promo code, Transfer ticket |
| Modal Dialog (L) | 800px | `max-w-3xl` | Seat map zoom, Event details |

---

## 5. Component Library (Shared Design Tokens)

### 5.1 Button System

```
┌──────────────────────────────────────────────────────────────────┐
│ BUTTON VARIANTS (ใช้ได้แค่ 4 แบบนี้เท่านั้น)                     │
│                                                                  │
│  [  Primary  ]  bg-brand text-white                              │
│                 hover:bg-brand-dark                               │
│                 active:ring-2 ring-brand/30                      │
│                                                                  │
│  [ Secondary ]  bg-white border border-neutral-200 text-neutral-800 │
│                 hover:bg-neutral-50                               │
│                 active:ring-2 ring-neutral-200                   │
│                                                                  │
│  [  Danger   ]  bg-danger text-white                             │
│                 hover:bg-red-600                                  │
│                 active:ring-2 ring-danger/30                     │
│                                                                  │
│  [   Ghost   ]  bg-transparent text-brand                        │
│                 hover:bg-brand/5                                  │
│                 (ใช้กับ inline action เช่น "View all", "Cancel") │
└──────────────────────────────────────────────────────────────────┘
```

**Button States (ทุกปุ่มต้องมีครบ):**
* `default` → `hover` → `active/pressed` → `focus-visible` (ring)
* `disabled`: opacity 50%, cursor-not-allowed
* `loading`: แสดง spinner แทน label, ปิด pointer-events

**Button Sizing:**
| Size | Height | Padding X | Font Size | Usage |
| :--- | :---: | :---: | :---: | :--- |
| `sm` | 32px | 12px | 13px | Inline actions, table row buttons |
| `md` | 40px | 16px | 14px | **Default — ใช้ทั่วไป** |
| `lg` | 48px | 24px | 16px | CTA, Checkout, Hero actions |

### 5.2 Card Component

```
┌─────────────────────────────────────────────────┐
│  ┌───────────────────────────────────────────┐  │  ← rounded-xl (12px)
│  │           Event Banner Image              │  │  ← aspect-[16/9]
│  └───────────────────────────────────────────┘  │
│                                                 │
│  Overline Label          12px  neutral-500      │  ← space-1 below
│  Card Title              20px  neutral-950 semi │  ← space-2 below
│  Description text max 2 lines  14px neutral-500 │
│                                                 │  ← space-4 above
│  [ Primary Action ]       [ Secondary ]         │  ← padding 24px all
│                                                 │
└─────────────────────────────────────────────────┘
     border: 1px solid neutral-200
     shadow: shadow-sm (ห้ามเกิน shadow-md)
     hover: shadow-md + translateY(-1px) transition 150ms
```

**Card Rules:**
* ❌ ห้ามมี shadow เกิน `shadow-md` ไม่ว่า state ไหน
* ❌ ห้ามมี border-radius ต่ำกว่า `rounded-lg` (8px)
* ✅ ทุกการ์ดในหน้าเดียวกันต้อง height เท่ากัน (ใช้ grid + `h-full`)
* ✅ Image aspect ratio ต้องคงที่ (`aspect-video` หรือ `aspect-[3/4]`)

### 5.3 Form Input System

```
┌──────────────────────────────────────────────┐
│  Label (14px, semi, neutral-800)             │  ← space-1 below label
│  ┌────────────────────────────────────────┐  │
│  │  Placeholder text...                   │  │  ← h-10 (40px), px-3
│  └────────────────────────────────────────┘  │    border-neutral-200
│  Helper text or error (12px)                 │    rounded-lg (8px)
│                                              │    focus:ring-2 ring-brand
└──────────────────────────────────────────────┘
     Error State:  border-danger + text-danger on helper
     Disabled:     bg-neutral-50 + opacity-60
     Gap between fields: space-4 (16px)
```

### 5.4 Toast / Notification System

| Type | Icon | Border Left Color | Duration | Position |
| :--- | :--- | :--- | :--- | :--- |
| Success | ✓ Checkmark | `success` | 3 seconds | Top-right |
| Error | ✕ Cross | `danger` | 5 seconds (manual dismiss) | Top-right |
| Warning | ⚠ Triangle | `accent` | 4 seconds | Top-right |
| Info | ℹ Circle | `brand` | 3 seconds | Top-right |

**Rules:** Max 3 stacked toasts. Newest on top. Auto-collapse after duration. Smooth slide-in from right with 300ms ease-out.

### 5.5 Modal / Dialog

* **Backdrop:** `bg-black/50` with `backdrop-blur-sm`
* **Animation:** Scale from 95% to 100% + fade-in (200ms `ease-out`)
* **Close:** Always have X icon top-right + ESC key + backdrop click
* **Action Buttons:** Right-aligned. Primary right, Cancel left.
* **Focus Trap:** Tab cycling stays within modal while open.

---

## 6. Interaction & Motion Standards

### 6.1 Transition Durations
| Interaction | Duration | Easing | Example |
| :--- | :---: | :--- | :--- |
| Hover color change | 150ms | `ease` | Button hover, link hover |
| Modal open/close | 200ms | `ease-out` / `ease-in` | Dialog entrance |
| Page route change | 250ms | `ease-in-out` | Cross-fade between routes |
| Toast slide-in | 300ms | `ease-out` | Notification entry |
| Seat status change | 300ms | `ease-in-out` | SVG circle fill color transition |
| Skeleton shimmer | 1.5s | `linear` infinite | Loading placeholder pulse |

### 6.2 Loading States (ทุกหน้าต้องมี)

```
State Flow: Skeleton → Content Loaded → Interactive
         ↘ Error State (Retry Button)
```

* **Page Load:** Full skeleton placeholder matching content layout shape.
* **Button Action:** Spinner inside button + disabled state. ห้ามปล่อยให้ user กดซ้ำ.
* **Infinite Scroll / Pagination:** Skeleton cards ต่อท้ายระหว่าง fetch.

### 6.3 Empty States
ทุกรายการว่าง (ตาราง, การ์ด, ตั๋ว) ต้องแสดง:
* Illustration icon (outline style, `neutral-300`, 64px)
* Heading: บอกว่าอะไรว่าง ("No events found")
* Subtext: บอกว่าทำอะไรได้ ("Try adjusting your search filters")
* CTA button (ถ้ามี action ที่ช่วยได้)

### 6.4 Predictable Interactions Checklist
| Action | Feedback ที่ต้องมี |
| :--- | :--- |
| กดปุ่ม Submit | Spinner ใน button + disable ทันที |
| กดเลือกที่นั่ง | เปลี่ยนสีทันที + แสดงสรุปด้านล่าง |
| กด Hold ที่นั่ง | Countdown timer ปรากฏ (10:00 → 0:00) |
| กดจ่ายเงินสำเร็จ | Redirect ไปหน้า Confirmation + confetti animation |
| กดลบ/ยกเลิก | Confirmation modal ก่อนทำงานเสมอ |
| กด Follow ศิลปิน | Heart icon toggle + counter +1 แบบ Optimistic UI |
| Form validation error | Inline error ใต้ field ทันที (ไม่ใช่ alert box) |
| Network error | Toast error + Retry button |
| กดสแกน QR สำเร็จ | ✅ Green full-screen flash + chime sound |
| กดสแกน QR ซ้ำ/ไม่ถูกต้อง | ❌ Red full-screen flash + buzzer sound |

---

## 7. Page-by-Page Layout Specifications

### 7.1 Customer Portal — Event Discovery (`/`)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  [ Logo ]              Search [ 🔍 ]              [ Login ] [ Sign Up ] │  ← Navbar h-16
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│                     ██████████████████████████                           │
│                     █   HERO BANNER (Event)  █                          │  ← h-[400px]
│                     █   Event Name (display)  █                         │    bg gradient overlay
│                     █   [ Get Tickets → ]     █                         │
│                     ██████████████████████████                           │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│  Trending Events                                    [ View All → ]      │
│                                                                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐               │  ← 4-col grid
│  │  Image   │  │  Image   │  │  Image   │  │  Image   │               │    gap-6 (24px)
│  │  Title   │  │  Title   │  │  Title   │  │  Title   │               │
│  │  Date    │  │  Date    │  │  Date    │  │  Date    │               │
│  │  ฿Price  │  │  ฿Price  │  │  ฿Price  │  │  ฿Price  │               │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘               │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│  Artists You Follow                                                     │
│  ○ ○ ○ ○ ○ ○ ○ ○  (Horizontal scroll avatar circles)                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Interactive Seat Map (`/events/{id}/seats`)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ← Back to Event                              Hold Timer: 09:42        │
├──────────────────────────────────────────┬──────────────────────────────┤
│                                          │                              │
│         S T A G E                        │  Event Title (h3)            │
│    ╭─────────────────────╮               │  📅 Date & Time              │
│    │                     │               │  📍 Venue Name               │
│    ╰─────────────────────╯               │                              │
│                                          │ ─────────────────────────── │
│    ● ● ● ● ● ● ● ● ● ●  Section A      │  Selected Seats:             │
│    ● ● ● ● ● ● ● ● ● ●                 │  ┌─────────────────────────┐ │
│    ● ● ● ○ ○ ● ● ● ● ●  Section B      │  │ A-12  VIP   ฿4,500     │ │
│    ● ● ● ● ● ● ● ● ● ●                 │  │ A-13  VIP   ฿4,500     │ │
│    ● ● ● ● ○ ○ ○ ● ● ●  Section C      │  └─────────────────────────┘ │
│                                          │                              │
│    Legend:                               │  Subtotal:         ฿9,000   │
│    🟢 Available  🟡 Your Hold            │  Service Fee:        ฿450   │
│    ⚪ Others     🔴 Sold                 │  Total:            ฿9,450   │
│                                          │                              │
│    [ − Zoom + ]  [ Reset View ]          │  [ Proceed to Checkout → ]  │
│                                          │                              │
├──────────────────────────────────────────┴──────────────────────────────┤
│  💡 Select up to 6 seats. Hold expires in 10 minutes.                   │
└─────────────────────────────────────────────────────────────────────────┘
     Left panel:  70% width (seat map SVG)
     Right panel: 30% width (order summary — sticky)
     Mobile: Full-width map, bottom sheet summary
```

### 7.3 Organizer Dashboard (`/organizer`)

```
┌──────┬──────────────────────────────────────────────────────────────────┐
│      │  Dashboard                                     Admin ▾  🔔 3    │
│  🏠  ├──────────────────────────────────────────────────────────────────┤
│  📊  │                                                                  │
│  🎤  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐           │
│  🎫  │  │ Total Rev │ │Tickets   │ │ Events   │ │ Avg Price│           │
│  👥  │  │ ฿2.4M    │ │ 12,450   │ │ 8 Active │ │ ฿1,920  │           │
│  💰  │  │ +12.4% ▲ │ │ +8.2% ▲  │ │          │ │ -2.1% ▼ │           │
│  ⚙️  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘           │
│      │                                                                  │
│ Side │  Revenue Trend (Line Chart)                                      │
│ bar  │  ┌──────────────────────────────────────────────────────┐       │
│ 240px│  │                         ╱╲                           │       │
│ fixed│  │              ╱╲   ╱╲  ╱  ╲╱╲                        │       │
│      │  │    ╱╲   ╱╲  ╱  ╲ ╱  ╲╱      ╲                       │       │
│      │  │╱╲ ╱  ╲ ╱  ╲╱                  ╲                      │       │
│      │  └──────────────────────────────────────────────────────┘       │
│      │   Jan  Feb  Mar  Apr  May  Jun  Jul  Aug  Sep                   │
│      │                                                                  │
│      │  Recent Events (Table)                                           │
│      │  ┌───────────────────────────────────────────────────────┐      │
│      │  │ Event Name     │ Date      │ Sold   │ Status  │ ••• │      │
│      │  │ Coldplay BKK   │ 15 Oct    │ 98%    │ On Sale │     │      │
│      │  │ BNK48 Concert  │ 22 Nov    │ 45%    │ Draft   │     │      │
│      │  └───────────────────────────────────────────────────────┘      │
│      │                                                                  │
└──────┴──────────────────────────────────────────────────────────────────┘
```

---

## 8. Responsive Breakpoints & Behavior

| Breakpoint | Width | Grid Cols | Seat Map | Sidebar | Font Scale |
| :--- | :--- | :---: | :--- | :--- | :---: |
| `sm` (Mobile) | < 640px | 4 | Full-width + bottom sheet | Hidden (hamburger) | 90% |
| `md` (Tablet) | 640-1023px | 8 | Full-width + collapsible panel | Collapsible | 95% |
| `lg` (Desktop) | 1024-1279px | 12 | 65% / 35% split | Visible 200px | 100% |
| `xl` (Wide) | ≥ 1280px | 12 | 70% / 30% split | Visible 240px | 100% |

---

## 9. Accessibility (WCAG 2.1 AA Minimum)

| Requirement | Implementation |
| :--- | :--- |
| **Color Contrast** | All text ≥ 4.5:1 ratio (body) and ≥ 3:1 (large text). Test with WebAIM contrast checker. |
| **Focus Visible** | `focus-visible:ring-2 ring-brand ring-offset-2` on every interactive element. ห้ามลบ outline. |
| **Keyboard Navigation** | ทุก action ทำได้โดยไม่ต้องใช้เมาส์ (Tab, Enter, Space, Escape). |
| **Screen Reader** | ใช้ `aria-label` บนปุ่ม icon-only, `aria-live="polite"` บน toast และ seat counter. |
| **Seat Map** | Seat grid ใช้ `role="grid"` + `role="gridcell"` พร้อม arrow key navigation. |
| **Motion Sensitivity** | เช็ค `prefers-reduced-motion` ลด/ปิด animation สำหรับ user ที่ต้องการ. |

---

## 10. Dark Mode Strategy

| Element | Toggle Mechanism |
| :--- | :--- |
| **Trigger** | System preference auto-detect (`prefers-color-scheme`) + manual toggle in settings. |
| **Class Strategy** | Tailwind `darkMode: 'class'` → Toggle `.dark` class on `<html>`. |
| **Persistence** | Store in `localStorage` (`theme: 'light' | 'dark' | 'system'`). |

**Dark Mode Color Swap:**
```css
:root {
  --bg-page: theme('colors.surface.50');
  --bg-card: #FFFFFF;
  --text-primary: theme('colors.neutral.800');
  --border: theme('colors.neutral.200');
}
.dark {
  --bg-page: theme('colors.surface.900');
  --bg-card: theme('colors.surface.800');
  --text-primary: theme('colors.neutral.100');
  --border: theme('colors.surface.700');
}
```
