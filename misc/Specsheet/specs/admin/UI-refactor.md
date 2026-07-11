# Specsheet: UI Refactoring & Responsiveness Patch

## 1. Purpose

- Overhaul the UI to be perfectly accessible and aesthetic on **mobile**, **tablet**, and **desktop**.
- Fix layout overflow issues, especially near breakpoints.
- Re‑arrange the Rooms page: move the status grid **above** the table, displayed as a 3‑row horizontally scrollable strip.
- Fix the **white card** bug in the room status grid (robust status handling).
- Ensure table columns keep consistent widths regardless of sorting selection.
- Make the Admin Dashboard fully responsive: rearrange KPI cards, charts, and tables for tablet and mobile.
- Resolve the `[ECharts] Can't get DOM width or height` error on the dashboard.

## 2. Global Breakpoints & Layout Rules

**Breakpoints (defined in CSS variables or shared SCSS):**

- **Mobile**: `<= 599px`
- **Tablet**: `600px – 959px`
- **Desktop**: `>= 960px`

**General layout rules:**

- All pages must use a responsive grid/flex layout that adapts at these breakpoints.
- No horizontal overflow; use `overflow-x: auto` only on tables when necessary.
- All interactive elements must have minimum touch target `48dp`.
- Use CSS media queries consistent with Angular Material’s breakpoints.
- **Global containment rule** (to be added in `styles.scss` or equivalent global stylesheet):
  ```scss
  *,
  *::before,
  *::after {
    box-sizing: border-box;
  }
  body {
    overflow-x: hidden;
  }
  .table-section {
    max-width: 100%;
    overflow-x: auto;
  }
  ```

## 3. Room Page – Status Grid Relocation & Fixes

### 3.1 Move Status Grid Above Table (Desktop & Tablet)

**Current:** The status grid is on the right side of the table (70/30 layout).  
**New:** The status grid appears **above** the table as a **3‑row, horizontally scrollable** strip.  
The grid should be limited to a height of three rows (with a fixed card height) and scroll horizontally if more rooms exist.

**Template changes** (`room-management.component.html`):

Replace the old structure with Angular control flow blocks:

```html
@if (!isMobile() || viewMode.value === 'grid') {
<div class="status-grid-row">
  <app-room-status-grid
    [roomTypeId]="roomTypeFilter()"
    (roomClicked)="onGridRoomClicked($event)"
  ></app-room-status-grid>
</div>
} @if (!isMobile() || viewMode.value === 'table') {
<div class="table-section">
  <app-generic-crud ...></app-generic-crud>
</div>
}
```

_Note: On mobile, the existing toggle between table and grid remains; when grid is selected, the `app-room-status-grid` will be shown alone (full width) because the table block will be hidden._

### 3.2 Style the Status Grid as Horizontal Scrollable 3‑Row Strip

In `room-status-grid.component.scss`, apply these exact rules:

```scss
.status-grid {
  display: grid;
  grid-auto-flow: column;
  grid-template-rows: repeat(3, 1fr);
  grid-auto-columns: 120px; // fixed card width
  gap: 8px;
  overflow-x: auto;
  overflow-y: hidden;
  height: calc(3 * 68px); // explicit 3-row height constraint
  padding: 8px 0;
}
.room-card {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  border-radius: 4px;
  // background colors are applied via classes (see section 3.3)
}
```

On mobile, when the grid is displayed instead of the table, the same horizontal scrollable strip is used.

### 3.3 Fix White Card Bug – Robust Status Handling

**Root cause:** The card background may not be applied if the status value does not exactly match the expected casing (e.g., `'Occupied'` vs `'occupied'`).

**Fix:** Replace the template class binding with a component method that normalises the status.

In `room-status-grid.component.ts`:

```ts
getStatusClass(status: string | null | undefined): string {
  const normalized = (status ?? '').toLowerCase();
  if (normalized === 'occupied') return 'occupied';
  if (normalized === 'available') return 'available';
  return 'neutral';
}
```

In `room-status-grid.component.html`, change the card div to:

```html
<div
  class="room-card"
  [class]="getStatusClass(room.status)"
  (click)="roomClicked.emit(room.roomId)"
  [matTooltip]="tooltipContent(room)"
  matTooltipPosition="above"
  [attr.aria-label]="room.roomNumber + ' - ' + room.status"
>
  <span class="room-number">{{ room.roomNumber }}</span>
  <mat-icon
    >{{ (room.status ?? '').toLowerCase() === 'occupied' ? 'lock' : 'lock_open'
    }}</mat-icon
  >
</div>
```

Add CSS classes for the three possible states:

```scss
.room-card {
  &.occupied {
    background-color: #ef9a9a;
  }
  &.available {
    background-color: #a5d6a7;
  }
  &.neutral {
    background-color: #eeeeee;
  }
}
```

This ensures cards never appear white (neutral grey fallback) and status casing differences are handled.

## 4. Table Column Width Consistency

**Problem:** When sorting changes, column widths may shift because `mat-sort-header` adds an arrow indicator, or because the data length varies.  
**Solution:** Apply `table-layout: fixed` to all tables, and optionally specify column widths via a new `width` property on `ColumnDef`.

### 4.1 Global Table Fix

In the **generic CRUD component**’s styles (and also apply to the oversight pages’ tables), add:

```scss
table {
  table-layout: fixed;
  width: 100%;
}
th,
td {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
```

### 4.2 Optional Column Width Property

Add an optional `width?: string` to the `ColumnDef` interface in `crud-config.model.ts`:

```ts
export interface ColumnDef<T> {
  // ... existing properties
  width?: string; // e.g., '10rem', '15%'
}
```

In the generic component’s template, apply the width to both header and cell:

```html
<th
  mat-header-cell
  *matHeaderCellDef
  [style.width]="col.width"
>
  {{ col.header }}
</th>
<td
  mat-cell
  *matCellDef="let row"
  [style.width]="col.width"
>
  ...
</td>
```

When `width` is not provided, the column will take an equal fraction of the remaining space (because of `table-layout: fixed`). To prevent uneven columns when no widths are set, the generic component should compute a default width for each column:

```ts
// In the template or via a helper
const columnWidth = col.width ?? `${100 / config.columns.length}%`;
```

And apply `[style.width]="columnWidth"`. This can be done by computing the widths in the component and storing in a signal, or by using a method in the template. For simplicity, we'll compute a `columnWidths` array in the generic component and bind to it.

_Implementation detail:_ In the generic component, compute a `columnWidths` array based on the config.columns and any specified widths. For columns without width, assign `100 / totalColumns + '%'`. This guarantees consistent widths regardless of sorting.

## 5. Admin Dashboard – Responsive Layout

### 5.1 ECharts Error Fix

**Root cause:** The chart container is re‑created by `@if` blocks, and ECharts initializes before the new DOM elements have layout dimensions.

**Fix:** Always render the chart containers, and use `AfterViewInit` to manually trigger a resize event after the view initialises.

In `dashboard.component.ts`:

```ts
import { AfterViewInit, ElementRef, ViewChildren, QueryList } from '@angular/core';

@Component({...})
export class DashboardComponent implements AfterViewInit {
  @ViewChildren('chartRef') charts!: QueryList<ElementRef>;

  ngAfterViewInit() {
    // Force ECharts to recalculate dimensions after view initialisation
    setTimeout(() => {
      window.dispatchEvent(new Event('resize'));
    });
  }
}
```

In the template, replace the `@if` blocks that conditionally show the charts with static containers containing the chart directive and a reference:

```html
<div
  echarts
  [options]="revenueChartOptions()"
  #chartRef
  class="chart"
  style="height: 400px; width: 100%;"
></div>
<div
  echarts
  [options]="expenditureChartOptions()"
  #chartRef
  class="chart"
  style="height: 400px; width: 100%;"
></div>
```

Remove the `@if` around charts entirely. The `revenueChartOptions()` and `expenditureChartOptions()` should return a minimal valid configuration when `analytics()` is null, e.g.:

```ts
if (!data)
  return {
    xAxis: { type: "category", data: [] },
    yAxis: { type: "value" },
    series: [],
  };
```

This way ECharts always has a valid option set and a container with dimensions.

### 5.2 Rearranging Dashboard for Tablet & Mobile

**KPI Cards layout** – Replace flexbox with CSS grid for deterministic row counts.

In `dashboard.component.scss`:

```scss
.kpi-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
}
@media (max-width: 959px) {
  .kpi-row {
    grid-template-columns: repeat(2, 1fr);
  }
}
@media (max-width: 599px) {
  .kpi-row {
    grid-template-columns: 1fr;
  }
}
```

**Middle row (charts + health cards)** – Use flexbox with `flex-wrap: wrap` and define widths to stack correctly.

```scss
.middle-row {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}
.charts {
  flex: 1 1 60%;
  min-width: 300px;
}
.health-cards {
  flex: 1 1 30%;
  min-width: 250px;
}
@media (max-width: 959px) {
  .middle-row {
    flex-direction: column;
  }
}
```

**“Today’s Movement” table** – ensure it can scroll horizontally on small screens:

```scss
.movement-table {
  overflow-x: auto;
}
```

### 5.3 Global Breakpoint Enforcement

All other dashboard elements (date filters, buttons) should follow the same breakpoints with flex-wrap.

## 6. Ensure No Overflow at Breakpoints

- All containers use `box-sizing: border-box` (global rule).
- Tables have `overflow-x: auto` within a container.
- Use the `table-layout: fixed` and column widths to prevent column expansion.
- At every breakpoint transition (599px, 600px, 959px, 960px), verify that no element exceeds viewport width.

## 7. Self‑Review Checklist (for the agent)

- [ ] Room status grid appears above the table as a 3‑row horizontally scrollable strip on desktop/tablet.
- [ ] Grid cards are coloured correctly: occupied = red, available = green, unknown = grey (no white cards).
- [ ] Mobile room view toggle still switches between table and grid; grid view shows scrollable cards.
- [ ] Table columns do not change width when sorting; `table-layout: fixed` and column width fallback implemented.
- [ ] Dashboard ECharts error is gone; charts render without console errors.
- [ ] Dashboard KPI cards: desktop = 4 per row, tablet = 2, mobile = 1.
- [ ] Dashboard charts and health cards stack vertically on tablet/mobile.
- [ ] No horizontal overflow on any page at any breakpoint.
- [ ] All functionality (sorting, filtering, clicking room cards) remains intact.
- [ ] Global `box-sizing: border-box` applied; body overflow-x hidden.

## 8. Integration Notes

- The `width` property on `ColumnDef` is optional; the generic component must compute equal widths if not specified.
- The ECharts fix requires changes to the dashboard component’s chart option signals (to return a minimal config when data is null) and the addition of `AfterViewInit`.
- The room status grid relocation requires moving the component in the parent template and updating CSS; the mobile toggle logic stays the same.
- No new dependencies are added.

