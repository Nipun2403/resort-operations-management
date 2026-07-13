# Option D Implementation Plan: Floating Concierge Button + Slide Panel in User Shell

## Overview
Add a floating concierge button in the top app-bar and a slide-out panel accessible from all user pages, following the existing glass-panel/material design system.

## Architecture

```
user-shell (layout wrapper)
├── Top App Bar
│   ├── Mobile menu toggle
│   ├── Brand logo
│   └── Profile menu + 🤖 Concierge FAB (NEW)
├── Side Navigation (mobile drawer)
├── Main Content
│   └── <router-outlet> (dashboard, bookings, room-service, etc.)
└── Concierge Slide Panel (NEW - fixed overlay)
    ├── Header (title + close button)
    ├── <app-concierge-chat> component
    └── Backdrop (mobile only)
```

## Files to Modify

| File | Changes |
|------|---------|
| `user-shell.component.ts` | Add `showConcierge` signal, `toggleConcierge()` method |
| `user-shell.component.html` | Add FAB in app-bar + `<app-concierge-chat>` overlay with backdrop |
| `user-shell.component.scss` | Slide panel animation, backdrop, responsive breakpoints |
| `concierge-chat.component.scss` | Panel styling (glass panel, shadows, max-width) |

## Design Specifications

### Colors & Theme (match existing)
- Panel background: `var(--mat-sys-surface-container)` with `backdrop-filter: blur(16px)`
- Border: `1px solid var(--mat-sys-outline-variant)`
- Shadows: `box-shadow: 0 8px 32px rgba(0,0,0,0.12)`
- FAB: `var(--mat-sys-primary-container)` with `var(--mat-sys-on-primary-container)`
- Text: `var(--mat-sys-on-surface)`

### Responsive Behavior

| Breakpoint | Behavior |
|------------|----------|
| ≥ 1024px (desktop) | Side panel (400px), pushes content slightly |
| 600px - 1023px (tablet) | Slide-over panel (90% width, right-aligned) |
| < 600px (mobile) | Full-height slide-over (100% width), backdrop required |

### Animation
- Panel: `transform: translateX(100%)` → `translateX(0)` (300ms ease-out)
- Backdrop: `opacity: 0` → `opacity: 1` (200ms)
- FAB: rotate 45° on open (150ms)

---

## Implementation Steps

1. **Update `user-shell.component.ts`** - Add `showConcierge` signal + `toggleConcierge()` method
2. **Update `user-shell.component.html`** - Add FAB in app-bar + slide panel overlay with backdrop
3. **Update `user-shell.component.scss`** - Slide panel styles, backdrop, responsive breakpoints, FAB animation
4. **Update `concierge-chat.component.scss`** - Panel max-width, glass panel styling
4. **Build & verify** - `dotnet build` + `npm run build`