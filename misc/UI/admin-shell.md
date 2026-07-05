## Patch Specsheet: Admin Shell – Dark Theme Fix

### 1. Purpose
- Ensure the entire Admin portal (sidebar, toolbar, content area) uses the dark “Obsidian & Champagne” aesthetic.
- The previous dashboard spec only restyled the dashboard page; the surrounding shell still had white backgrounds.

### 2. Files to Modify
| File | Change |
|------|--------|
| `src/app/features/admin/admin-shell.component.html` | (no change – keep existing structure) |
| `src/app/features/admin/admin-shell.component.scss` | Override all background, text, and border colours to use theme variables. |
| `src/app/features/admin/admin-shell.component.ts` | (no change) |

### 3. Changes

Replace the entire SCSS file content with:

```scss
@import '../../../../styles/theme/index';

// ── Sidenav ──────────────────────────────────────
:host {
  --mdc-sidenav-container-background-color: var(--color-background);
  --mdc-sidenav-scrim-color: rgba(0, 0, 0, 0.7);
}

mat-sidenav {
  background: var(--color-surface-container-low) !important;
  border-right: 1px solid rgba(228, 194, 133, 0.15) !important;
  .mat-toolbar {
    background: var(--color-background) !important;
    color: var(--color-secondary) !important;
  }
  .mat-nav-list a {
    color: var(--color-on-surface-variant) !important;
    &.active {
      color: var(--color-secondary) !important;
      background: rgba(228, 194, 133, 0.05) !important;
    }
    &:hover { color: var(--color-secondary) !important; }
  }
  .mat-divider { border-color: rgba(228, 194, 133, 0.15) !important; }
}

// ── Toolbar ──────────────────────────────────────
mat-toolbar {
  background: var(--color-surface-container-low) !important;
  color: var(--color-on-surface) !important;
  border-bottom: 1px solid rgba(228, 194, 133, 0.15) !important;
}

// ── Content area ─────────────────────────────────
.mat-sidenav-content {
  background: var(--color-background) !important;
  color: var(--color-on-background);
}

// ── User menu ────────────────────────────────────
.mdc-menu-surface {
  background: var(--color-surface-container-high) !important;
  color: var(--color-on-surface) !important;
}
```

If the template uses `<mat-toolbar>` with `color="primary"`, that will set an Angular Material background. Removing the `color` attribute or keeping it and overriding with `!important` is fine. Ensure the template does not force a light color. (If it does, we can adjust later, but this CSS will override.)

### 4. Self‑Review Checklist
- [ ] Admin sidebar background is dark (`#131411` or similar).
- [ ] Toolbar background is dark; text is light.
- [ ] Active sidebar item has gold text.
- [ ] Page content area is dark.
- [ ] No white backgrounds remain.
- [ ] Existing functionality (navigation, logout, etc.) unaffected.

