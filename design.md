---
name: Neo-Mint Ticketing & Operations
colors:
  surface: '#faf8ff'
  surface-dim: '#d2d9f4'
  surface-bright: '#faf8ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f3ff'
  surface-container: '#eaedff'
  surface-container-high: '#e2e7ff'
  surface-container-highest: '#dae2fd'
  on-surface: '#131b2e'
  on-surface-variant: '#3d4a3d'
  inverse-surface: '#283044'
  inverse-on-surface: '#eef0ff'
  outline: '#6d7b6c'
  outline-variant: '#bccbb9'
  surface-tint: '#006e2f'
  primary: '#006e2f'
  on-primary: '#ffffff'
  primary-container: '#22c55e'
  on-primary-container: '#004b1e'
  inverse-primary: '#4ae176'
  secondary: '#4953bc'
  on-secondary: '#ffffff'
  secondary-container: '#8792fe'
  on-secondary-container: '#17228f'
  tertiary: '#944a00'
  on-tertiary: '#ffffff'
  tertiary-container: '#fa913b'
  on-tertiary-container: '#663100'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#6bff8f'
  primary-fixed-dim: '#4ae176'
  on-primary-fixed: '#002109'
  on-primary-fixed-variant: '#005321'
  secondary-fixed: '#e0e0ff'
  secondary-fixed-dim: '#bdc2ff'
  on-secondary-fixed: '#000767'
  on-secondary-fixed-variant: '#2f3aa3'
  tertiary-fixed: '#ffdcc5'
  tertiary-fixed-dim: '#ffb783'
  on-tertiary-fixed: '#301400'
  on-tertiary-fixed-variant: '#713700'
  background: '#faf8ff'
  on-background: '#131b2e'
  surface-variant: '#dae2fd'
  canvas-mint: '#E8F1EB'
  surface-pure: '#FFFFFF'
  surface-subtle: '#F8FAFC'
  mint-soft: '#86EFAC'
  mint-tint: '#D7F4DF'
  mint-gradient-end: '#C8ECD2'
  amber-warning: '#FBBF24'
  hatch-slate: '#94A3B8'
  text-muted: '#64748B'
  border-hairline: '#E2E8F0'
typography:
  display-kpi:
    fontFamily: Inter
    fontSize: 36px
    fontWeight: '900'
    lineHeight: 40px
    letterSpacing: -0.03em
  display-kpi-mobile:
    fontFamily: Inter
    fontSize: 28px
    fontWeight: '900'
    lineHeight: 32px
    letterSpacing: -0.025em
  headline-lg:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '800'
    lineHeight: 32px
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '800'
    lineHeight: 26px
    letterSpacing: -0.02em
  headline-sm:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '700'
    lineHeight: 22px
    letterSpacing: -0.01em
  body-lg:
    fontFamily: Inter
    fontSize: 15px
    fontWeight: '500'
    lineHeight: 22px
    letterSpacing: -0.01em
  body-md:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 19px
    letterSpacing: 0em
  body-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
    letterSpacing: 0em
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '600'
    lineHeight: 14px
    letterSpacing: 0.02em
  caption-tick:
    fontFamily: Inter
    fontSize: 10px
    fontWeight: '500'
    lineHeight: 14px
    letterSpacing: 0.02em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  gutter: 1.25rem
  gutter-mobile: 0.75rem
  margin: 1.5rem
  margin-mobile: 1rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2rem
---

# Design System & UI Style Guidelines: Neo-Mint Ticketing & Operations

> **Reference Source:** [Dribbble UI Design (Event Management Platform — Project Dashboard)](https://cdn.dribbble.com/userupload/48175942/file/1ab6c0a07485501ac252837b59778743.png)  
> **Platform:** TicketWorld Enterprise Event Management Platform  
> **Target Audience:** Concert Organizers, Venue Operators, and Operations Leads  
> **Document Purpose:** Canonical UI/UX specification, Design Tokens, and Component Architecture  

---

## 1. Brand & Style

### 1.1 Personality & Purpose
The design system combines the clarity of enterprise SaaS analytics with the emotional momentum of live entertainment ticketing. Its character is confident, calm, and hyper-legible under peak load. It removes anxiety from high-stress scenarios—such as stadium queue drops, real-time seat inventory holdouts, and multi-tier checkout timeouts—by grounding the user in an open, organic atmosphere.

### 1.2 Aesthetic Movement: Modern Data-Dense SaaS with Organic Tactility
The visual framework balances crisp, structured Bento geometry with organic softening:
- **Neo-Mint Atmosphere:** Replacing cold grays and clinical whites with a soothing sage-mint foundation (`#E8F1EB`) that reduces screen glare during extended operational monitoring and frantic customer ticket queues.
- **Squircle Enclosures & Pill Terminals:** Large radius perimeters (`24px` to `36px`) enclose dense telemetry, while interactive controls and status chips use full pill terminals.
- **Dual-Encoded 45° Micro-Hatching:** Diagonal stripe patterns convey pending, reserved, or held seat inventory and telemetry. This provides an instantly recognizable physical metaphor for tickets and states without depending solely on color hue.
- **Target Audience:** Live concert attendees seeking seamless seat discovery and rapid checkout, alongside venue box office directors, promoters, and real-time operations engineers.

---

## 2. Colors

### 2.1 Hierarchy & Role Distribution
The system relies on high contrast between pitch-charcoal typography, pure white surfaces, and an atmospheric sage backdrop:

- **Canvas (`#E8F1EB`):** Foundational sage-mint background establishing the spatial ambient depth.
- **Surface (`#FFFFFF`):** High-clarity foreground panels, cards, and modal sheets ensuring maximum optical contrast.
- **Primary (`#22C55E`):** Represents positive state progression, paid orders, confirmed seat locks, primary action CTAs, and peak ingress volume.
- **Secondary (`#818CF8`):** Applied to telemetry arcs, progress metrics, queue positions, and interactive seat tiers (VIP/Tier 1).
- **Tertiary (`#FB923C`) & Warning (`#FBBF24`):** Used for draft stages, time-running-out checkout counters, inventory depletion warnings, and zone hold indicators.
- **Neutral (`#0F172A`):** Deep charcoal slate delivering a 16:1 contrast ratio against `#FFFFFF` surfaces for titles, hero numerical metrics, and dark CTA states.
- **Text Muted (`#64748B`):** Supporting metadata, venue rules, labels, and chart axes. Exceeds WCAG AA contrast standards.

### 2.2 The 45° Diagonal Hatch Fill Pattern
For pending seats, held allocations, and unconfirmed states, UI components leverage a CSS/SVG pattern using `hatch-slate` (`#94A3B8`) over `#F1F5F9`:
```css
repeating-linear-gradient(
  45deg,
  rgba(148, 163, 184, 0.45),
  rgba(148, 163, 184, 0.45) 1.5px,
  transparent 1.5px,
  transparent 5.5px
)
```

---

## 3. Navigation Rails & Sidebars

### 3.1 Global Left Icon Rail (72px)
- **Desktop Specification:** Global icon rail fixed at `72px` width inside the `#E8F1EB` canvas.
- **Top Brand Mark:** `w-10 h-10` or `w-11 h-11 rounded-2xl` emerald squircle with pure white isometric mark. Centered on the 36px/48px vertical rail axis.
- **Navigation Squircles:**
  - Normal state: `w-11 h-11 rounded-2xl flex items-center justify-center text-slate-400 hover:text-slate-900 hover:bg-slate-100/70 transition-all`.
  - Active state: Highlighted squircle with mint-tinted fill (`bg-[#D7F4DF] text-[#006e2f] font-bold shadow-xs ring-1 ring-[#22c55e]/30` or `bg-[#22C55E] text-white`).
  - Zero overlapping badges or floating indicator pips.
- **Bottom Profile:** `w-10 h-10 rounded-2xl flex items-center justify-center` with user avatar and emerald status dot.
