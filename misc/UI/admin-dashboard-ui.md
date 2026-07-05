# Specsheet: Admin Dashboard – Visual Overhaul

## 1. Purpose
- Restyle the **Admin Dashboard** (`/operations/admin/dashboard`) to match the “Obsidian & Champagne” design system.
- This spec covers **only** HTML and SCSS changes. All existing signals, API calls, form controls, event handlers, and business logic remain untouched.
- The dashboard already contains: summary cards (arrivals, departures, active tickets), date filter bar, charts (revenue bar, expenditure donut), department health cards (housekeeping/maintenance pending), “Today’s Movement” table, and the “Create Internal Ticket” button. They are restyled in place without changing their functionality.

## 2. Files to Modify
| File | Change |
|------|--------|
| `src/app/features/admin/pages/dashboard.component.html` | Re‑structure layout, replace card/chart/table markup with new classes. |
| `src/app/features/admin/pages/dashboard.component.scss` | Add new styles, import theme tokens. |
| **No changes** to `dashboard.component.ts` or any services. |

## 3. Component Template – Structure (Angular 18 control flow)

The overall layout remains, but each section is wrapped with new CSS classes. We’ll only describe the markup changes conceptually; the agent must replace the existing template sections with the corresponding redesigned blocks while keeping all Angular bindings and directives intact.

**General layout:**
```html
<div class="dashboard">
  <!-- Briefing Bar (Date Filter + Create Internal Ticket) -->
  <section class="briefing-bar">
    <!-- existing date pickers and buttons, now styled as minimal inputs -->
    <div class="date-fields">
      <!-- start date -->
      <!-- end date -->
      <button class="btn-pill-primary" (click)="applyDateFilter()">APPLY</button>
      <button class="btn-pill-outline" (click)="clearDateFilter()">CLEAR</button>
    </div>
    <button class="btn-internal-ticket" (click)="openCreateTicketDialog()">CREATE INTERNAL TICKET</button>
  </section>

  <!-- Pillars of Performance (KPI Cards) -->
  <section class="kpi-grid">
    <!-- 6 cards, each a glass card with label, value, trend indicator -->
    <div class="kpi-card glass-card">
      <p class="kpi-label">OCCUPANCY RATE</p>
      <div class="kpi-value">{{ kpiCards()[0].value }}</div>
      ...
    </div>
    <!-- repeat for all 6 cards -->
  </section>

  <!-- Visionary Layer (Charts + Health Cards) -->
  <section class="visionary-layer">
    <div class="charts-column">
      <!-- Revenue bar chart container -->
      <div class="chart-panel glass-card">
        <h4>REVENUE OVERVIEW</h4>
        <div echarts [options]="revenueChartOptions()" class="chart-bar"></div>
      </div>
      <!-- Expenditure donut chart container -->
      <div class="chart-panel glass-card">
        <h4>EXPENDITURE DISTRIBUTION</h4>
        <div echarts [options]="expenditureChartOptions()" class="chart-donut"></div>
      </div>
    </div>
    <div class="health-column">
      <!-- Housekeeping Pending card -->
      <div class="health-card glass-card" (click)="openActiveTickets()">
        <span class="material-symbols-outlined">cleaning_services</span>
        <div class="health-number">{{ housekeepingPendingCount() }}</div>
        <p>HOUSEKEEPING PENDING</p>
        <a class="underline-reveal">VIEW ASSIGNMENTS</a>
      </div>
      <!-- Maintenance Pending card -->
      <div class="health-card glass-card" (click)="openActiveTickets()">
        <span class="material-symbols-outlined">construction</span>
        <div class="health-number">{{ maintenancePendingCount() }}</div>
        <p>MAINTENANCE PENDING</p>
        <a class="underline-reveal">VIEW TICKETS</a>
      </div>
    </div>
  </section>

  <!-- Ledger of Movement (Today's Movement table) -->
  <section class="ledger-section glass-card">
    <div class="gold-progress"></div>
    <div class="ledger-header">
      <h4>REAL-TIME OPERATIONS</h4>
      <p>Today's Movement</p>
    </div>
    <div class="table-scroll">
      <table mat-table ... class="ledger-table">
        <!-- existing columns -->
      </table>
    </div>
    <mat-paginator ...></mat-paginator>
  </section>
</div>
```

**Important:** The exact Angular bindings (`[dataSource]`, `(matSortChange)`, `*matHeaderCellDef`, etc.) must be preserved. The agent should only add CSS classes to the existing elements and wrap sections with divs.

## 4. SCSS ( `dashboard.component.scss` )

We'll import the global theme, then define styles matching the design.

```scss
@import '../../../../styles/theme/index';

.dashboard {
  padding: 2rem var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 2rem var(--margin-mobile);
  }
  display: flex;
  flex-direction: column;
  gap: 4rem;
}

// Briefing Bar
.briefing-bar {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: 2rem;
}
.date-fields {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  gap: 1.5rem;
  // Minimal underline date inputs
  .date-input-wrapper {
    border-bottom: 1px solid rgba(228, 226, 221, 0.4);
    transition: border-color 0.3s;
    &:focus-within { border-color: var(--color-secondary); }
    label {
      @include font-label-caps;
      font-size: 0.625rem;
      color: var(--color-on-tertiary-fixed-variant);
      margin-bottom: 0.25rem;
      display: block;
    }
    input {
      background: transparent;
      border: none;
      color: var(--color-on-surface);
      font-family: var(--font-body);
      font-size: 0.875rem;
      padding: 0.25rem 0;
      outline: none;
      width: 150px;
    }
  }
}
.btn-pill-primary {
  background: var(--color-secondary);
  color: var(--color-on-secondary-fixed);
  @include font-label-caps;
  font-size: 0.625rem;
  letter-spacing: 0.2em;
  padding: 0.5rem 1.5rem;
  border: none;
  border-radius: 999px;
  cursor: pointer;
  transition: filter 0.2s;
  &:hover { filter: brightness(1.1); }
}
.btn-pill-outline {
  background: transparent;
  border: 1px solid rgba(228, 194, 133, 0.2);
  color: var(--color-secondary);
  @include font-label-caps;
  font-size: 0.625rem;
  letter-spacing: 0.2em;
  padding: 0.5rem 1.5rem;
  border-radius: 999px;
  cursor: pointer;
  &:hover { background: rgba(228, 194, 133, 0.1); }
}
.btn-internal-ticket {
  background: transparent;
  border: 1px solid var(--color-secondary);
  color: var(--color-secondary);
  @include font-label-caps;
  font-size: 0.75rem;
  letter-spacing: 0.3em;
  padding: 1rem 2rem;
  cursor: pointer;
  transition: background 0.5s, color 0.5s;
  &:hover {
    background: var(--color-secondary);
    color: var(--color-on-secondary);
  }
}

// Glass Card
.glass-card {
  background: rgba(26, 26, 26, 0.6);
  backdrop-filter: blur(20px);
  border-top: 1px solid rgba(228, 194, 133, 0.2);
  transition: transform 0.4s cubic-bezier(0.2, 0.8, 0.2, 1);
  &:hover { transform: translateY(-4px); }
}

// KPI Grid
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: var(--gutter);
  @media (max-width: 1200px) { grid-template-columns: repeat(3, 1fr); }
  @media (max-width: 768px) { grid-template-columns: repeat(2, 1fr); }
  @media (max-width: 480px) { grid-template-columns: 1fr; }
}
.kpi-card {
  padding: 2rem;
  .kpi-label {
    @include font-label-caps;
    font-size: 0.625rem;
    color: var(--color-on-tertiary-fixed-variant);
    margin-bottom: 0.5rem;
    text-transform: uppercase;
    letter-spacing: 0.1em;
  }
  .kpi-value {
    font-family: var(--font-headline);
    font-size: 2.5rem;
    color: var(--color-secondary);
    margin-bottom: 0.5rem;
  }
  .kpi-trend {
    display: flex;
    align-items: center;
    gap: 0.25rem;
    font-size: 0.625rem;
    @include font-label-caps;
    color: var(--color-secondary);
    .material-symbols-outlined { font-size: 0.875rem; }
  }
}

// Visionary Layer
.visionary-layer {
  display: grid;
  grid-template-columns: 6fr 4fr;
  gap: var(--gutter);
  @media (max-width: 1024px) { grid-template-columns: 1fr; }
}
.charts-column {
  display: flex;
  flex-direction: column;
  gap: var(--gutter);
}
.chart-panel {
  padding: 2rem;
  h4 {
    @include font-label-caps;
    font-size: 0.75rem;
    color: var(--color-on-tertiary-fixed-variant);
    margin-bottom: 0.5rem;
    letter-spacing: 0.2em;
  }
  p {
    @include font-headline-sm;
    color: var(--color-on-surface);
    margin-bottom: 2rem;
  }
  .chart-bar, .chart-donut {
    height: 300px;
    width: 100%;
  }
}
.health-column {
  display: flex;
  flex-direction: column;
  gap: var(--gutter);
}
.health-card {
  flex: 1;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  cursor: pointer;
  .material-symbols-outlined {
    font-size: 2rem;
    color: var(--color-secondary);
  }
  .health-number {
    font-family: var(--font-headline);
    font-size: 7.5rem;
    line-height: 1;
    color: var(--color-secondary);
    margin: 0.5rem 0;
  }
  p {
    @include font-label-caps;
    font-size: 0.75rem;
    color: var(--color-on-surface);
    letter-spacing: 0.3em;
  }
  .underline-reveal {
    @include font-label-caps;
    font-size: 0.625rem;
    color: var(--color-secondary);
    letter-spacing: 0.2em;
    position: relative;
    display: inline-block;
    margin-top: 1rem;
    &::after {
      content: '';
      position: absolute;
      width: 0;
      height: 1px;
      bottom: -2px;
      left: 0;
      background: var(--color-secondary);
      transition: width 0.5s ease;
    }
    &:hover::after { width: 100%; }
  }
}

// Ledger Section
.ledger-section {
  overflow: hidden;
  .gold-progress {
    height: 2px;
    background: linear-gradient(90deg, transparent, var(--color-secondary), transparent);
    background-size: 200% 100%;
    animation: moveGradient 3s linear infinite;
  }
  .ledger-header {
    padding: 2rem;
    border-bottom: 1px solid rgba(228, 194, 133, 0.1);
    h4 {
      @include font-label-caps;
      font-size: 0.75rem;
      color: var(--color-on-tertiary-fixed-variant);
      letter-spacing: 0.2em;
    }
    p {
      @include font-headline-sm;
      color: var(--color-on-surface);
    }
  }
  .table-scroll {
    overflow-x: auto;
  }
  .ledger-table {
    width: 100%;
    border-collapse: collapse;
    th {
      @include font-label-caps;
      font-size: 0.625rem;
      color: var(--color-on-tertiary-fixed-variant);
      letter-spacing: 0.2em;
      padding: 1rem;
      text-align: left;
      border-bottom: 1px solid rgba(228, 194, 133, 0.1);
    }
    td {
      padding: 1rem;
      border-bottom: 1px solid rgba(228, 194, 133, 0.05);
      font-family: var(--font-body);
      font-size: 0.875rem;
      color: var(--color-on-surface);
    }
    .status-chip {
      padding: 0.25rem 0.75rem;
      font-size: 0.625rem;
      font-weight: 500;
      letter-spacing: 0.1em;
      background: rgba(228, 194, 133, 0.1);
      color: var(--color-secondary);
    }
  }
}

@keyframes moveGradient {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
```

**Note:** The existing chart options (`revenueChartOptions`, `expenditureChartOptions`) remain unchanged; the ECharts container will inherit the new size and the dark theme automatically. If needed, we can adjust the chart options to use gold color (`#e4c285`) in a separate patch, but the design's charts are quite different (e.g., bars with gold fill). We'll add a small TypeScript change to update the chart colors to match the theme, but we said no logic changes. However, chart options are configuration; we can modify the options in the dashboard component's computed signals. That is acceptable because it's purely visual configuration. We'll include instructions to update `revenueChartOptions` and `expenditureChartOptions` to use `#e4c285` for series color, and set dark background/text. This does not affect API or form logic.

**Chart options updates:**

In `revenueChartOptions`, set:
- `series.color: '#e4c285'`
- `backgroundColor: 'transparent'`
- `textStyle.color: '#c4c7c7'`
- `axisLine.lineStyle.color: 'rgba(228, 194, 133, 0.3)'`
- `splitLine.lineStyle.color: 'rgba(228, 194, 133, 0.1)'`

Similarly for `expenditureChartOptions`, set pie colors to `#e4c285`, `#8e9192`, etc.

We'll include these configuration changes in the specsheet as they are visual-only and don't affect business logic.

## 5. Responsive Behaviour
- The KPI grid adapts from 6 columns to 3, 2, 1 based on breakpoints.
- The visionary layer collapses to single column on screens < 1024px.
- The ledger table has horizontal scroll.
- The briefing bar wraps on smaller screens.

## 6. Integration Notes
- The `glass-card` class is used on cards; the existing `mat-card` elements can be replaced with `<div>` having `glass-card` class, or we can keep `mat-card` and apply the class. To keep Angular Material functionality (e.g., ripple) we might keep `mat-card`, but the design's cards are simpler. We'll switch to plain `<div>` to have full control. The existing card content is already inside; no functional loss.
- The health cards (housekeeping/maintenance pending) will still trigger the `openActiveTickets()` dialog on click, which is already wired. We'll add the `(click)` event to the card.
- The "Create Internal Ticket" button already exists and calls the same method.
- The table and paginator bindings remain identical.
- All `@if`, `@for` blocks, and signals (`kpiCards`, `auditEntries`, etc.) are preserved.

## 7. Self‑Review Checklist
- [ ] Dashboard layout matches the design’s spacing and grid.
- [ ] KPI cards display data with correct labels, values, and trend indicators.
- [ ] Health cards show housekeeping and maintenance counts, and clicking opens the active tickets dialog.
- [ ] Date filter uses minimal underline inputs and pill buttons; Apply/Clear work as before.
- [ ] Charts render with gold colours and dark background.
- [ ] Today’s Movement table has the new glass card style, gold progress bar, and styled rows.
- [ ] Responsive breakpoints ensure proper layout on tablet and mobile.
- [ ] No console errors; all existing functionality (API fetches, session storage) remains intact.

