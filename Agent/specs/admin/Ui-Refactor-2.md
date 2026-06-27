# Specsheet: UI Responsiveness Patch 2 – Tablet Sidebar, Mobile Grid & Breakpoint Fixes (Revised)

## 1. Purpose

- Extend the responsive behaviour of the application to properly support tablet and small mobile devices (320‑500px).
- Collapse the sidebar into a hamburger overlay on **tablet** screens (≤1024px), matching the mobile behaviour.
- Fix layout breakage on screens narrower than 500px, ensuring no horizontal overflow and all elements remain usable.
- Change the room status grid on **mobile** from a horizontally scrollable strip to a **vertically scrollable 3‑column grid**.
- Ensure the “Today’s Movement” table on the Admin Dashboard is fully responsive across all screen sizes.
- Adopt explicit, consistent breakpoint ranges.

## 2. Reference Breakpoints (Advisory)

| Category      | Width range     |
| ------------- | --------------- |
| Small Mobile  | < 480px         |
| Mobile        | 480px – 767px   |
| Tablet        | 768px – 1024px  |
| Desktop       | 1025px – 1199px |
| Large Desktop | ≥ 1200px        |

All responsive rules will be based on these breakpoints.

## 3. Files to Modify

| File                                                                                 | Change                                                                                            |
| ------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------- |
| `src/app/features/admin/admin-shell.component.ts`                                    | Update `isMobile` breakpoint to 1024px.                                                           |
| `src/app/features/admin/admin-shell.component.html`                                  | Ensure hamburger button visible when `isMobile()` is true.                                        |
| `src/app/features/admin/admin-shell.component.scss`                                  | Adjust sidebar width/overlay for tablet.                                                          |
| `src/app/features/admin/pages/management/room-management.component.html`             | No change (mobile toggle remains).                                                                |
| `src/app/features/admin/components/room-status-grid/room-status-grid.component.scss` | Add mobile media query for vertical 3‑column grid.                                                |
| `src/styles.scss` (or global styles)                                                 | Add safe responsive fixes for small screens.                                                      |
| `src/app/features/admin/pages/dashboard.component.scss`                              | Enhance “Today’s Movement” table responsiveness, ensure charts and cards adapt using breakpoints. |
| Various management page SCSS files                                                   | Ensure filter bars wrap correctly on small screens.                                               |

## 4. Sidebar Collapse on Tablet

### 4.1 AdminShellComponent – `isMobile` Breakpoint

**Current:** `isMobile` observes `(max-width: 768px)`.  
**New:** Change to `(max-width: 1024px)` so that tablets also see the overlay sidebar with hamburger.

In `admin-shell.component.ts`:

```ts
isMobile = toSignal(
  this.breakpointObserver
    .observe("(max-width: 1024px)")
    .pipe(map((r) => r.matches)),
  { initialValue: false },
);
```

### 4.2 Template & Styles

The existing template already shows a hamburger button when `isMobile()` is true, and the sidebar mode toggles between `'over'` and `'side'`. No template changes are required, but verify that the `mode` binding is:

```html
<mat-sidenav
  [mode]="isMobile() ? 'over' : 'side'"
  ...
></mat-sidenav>
```

If not, adjust accordingly.

**Styles:** Ensure the sidebar overlay on tablet has an appropriate width (e.g., `250px`). The default material styles handle this.

## 5. Global Fixes for Screens < 500px

### 5.1 Prevent Overflow and Improve Usability

Add the following to the global `styles.scss` (or equivalent shared stylesheet):

```scss
html,
body {
  -webkit-text-size-adjust: 100%;
  touch-action: manipulation;
}

// Never use `* { max-width: 100% !important; }`. Instead, target specific elements that could overflow.
img,
video,
canvas,
svg {
  max-width: 100%;
  height: auto;
}

@media (max-width: 500px) {
  // Ensure all form fields and buttons are full width and stack
  mat-form-field,
  mat-button-toggle-group,
  .mat-button-toggle-group {
    width: 100%;
  }
  // Tables and containers scroll horizontally
  .table-section,
  .mat-table-container {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }
  // Adjust card and container margins/paddings
  .mat-card,
  .kpi-card,
  .health-cards .mat-card {
    margin: 8px 0;
    padding: 12px;
  }
}
```

### 5.2 Management Pages – Filter Bar Wrapping

For all CRUD management pages, ensure the search/filter bar wraps properly. Add to the generic CRUD component’s styles (or apply globally):

```scss
.search-filter-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  mat-form-field {
    flex: 1 1 200px;
    min-width: 150px;
  }
}
```

If the generic CRUD component doesn't have this style, add it there so all management pages inherit it.

## 6. Room Status Grid – Mobile Vertical Scroll with 3 Columns

### 6.1 Override Grid Layout on Mobile

In `room-status-grid.component.scss`, add a media query for screens ≤ 767px:

```scss
@media (max-width: 767px) {
  .status-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    grid-auto-rows: minmax(60px, auto);
    gap: 8px;
    overflow-y: auto;
    overflow-x: hidden;
    height: auto; // allow vertical expansion
    max-height: 60vh; // limit height to prevent extremely long lists, then scroll
    padding: 8px;
  }
  .room-card {
    height: 60px;
    flex: none;
  }
}
```

This overrides the desktop horizontal scrollable strip and makes it a vertical 3‑column grid. The grid will scroll vertically if content exceeds `max-height`.

The mobile toggle on the room page still works (table vs grid). When “Grid” is selected, this vertical grid will be shown.

## 7. Dashboard “Today’s Movement” Table Responsiveness

### 7.1 Ensure Table Container Scrolls

In `dashboard.component.scss`:

```scss
.movement-table {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  table {
    min-width: 600px; // prevent columns from collapsing too much
  }
}
```

This ensures the table can be scrolled horizontally on small screens.

### 7.2 Charts Responsiveness

Replace inline styles with CSS classes for deterministic height control. Update `dashboard.component.html` to use a class `chart` instead of inline style:

```html
<div
  echarts
  [options]="revenueChartOptions()"
  #chartRef
  class="chart"
></div>
```

In `dashboard.component.scss`:

```scss
.chart {
  width: 100%;
  height: 400px;
}
@media (max-width: 599px) {
  .chart {
    height: 300px;
  }
}
```

This removes inline styles and enables responsive height.

## 8. KPI Cards Grid – Determined Breakpoints

Replace the old KPI card layout with explicit breakpoints that match the global definitions. In `dashboard.component.scss`:

```scss
.kpi-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
}
@media (max-width: 1024px) {
  .kpi-row {
    grid-template-columns: repeat(2, 1fr); // tablet: 2 per row
  }
}
@media (max-width: 767px) {
  .kpi-row {
    grid-template-columns: 1fr; // mobile: 1 per row
  }
}
```

This eliminates ambiguity and matches the tablet/mobile breakpoints used elsewhere.

## 9. Self‑Review Checklist (for the agent)

- [ ] Sidebar collapses into hamburger on tablets (768‑1024px) and mobile (<768px), persistent on desktop ≥1025px.
- [ ] Hamburger button visible and functional on tablet and mobile.
- [ ] No horizontal overflow on screens as narrow as 320px; all pages scroll without content cut‑off.
- [ ] Management pages’ filter bars wrap gracefully on small screens.
- [ ] Room status grid on mobile appears as a vertical 3‑column scrollable list (instead of horizontal strip).
- [ ] “Today’s Movement” table is horizontally scrollable on small screens; columns do not overlap.
- [ ] Dashboard charts resize appropriately on mobile (height 300px).
- [ ] All breakpoint transitions (480, 768, 1024) produce a polished layout without broken elements.
- [ ] No regression in desktop or tablet views.

## 10. Integration Notes

- The sidebar breakpoint change is isolated to `AdminShellComponent`.
- Global styles are added to `styles.scss`; the dangerous wildcard `* { max-width: 100% !important; }` is **not** included.
- The room status grid mobile layout change only affects `room-status-grid`.
- Dashboard chart height uses CSS classes instead of inline styles.
- All KPI grid breakpoints now use 1024px and 767px explicitly, leaving no room for interpretation.

