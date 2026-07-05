# Specsheet: Admin Analytics Page – Visual Overhaul

## 1. Purpose
- Restyle the **Admin Analytics** page (`/operations/admin/oversight/analytics`) to match the “Obsidian & Champagne” design system.
- The page retains its existing structure: date presets, category dropdown, KPI cards, and four chart panels (Bar, Line, Radar, Pie). The visual overhaul applies glass‑morphic panels, gold accents, and the dark luxury aesthetic.
- **All existing signals, form controls, API calls, event handlers, and chart logic remain untouched.** Only the HTML template, SCSS, and chart colour configurations are modified.

## 2. Files to Modify
| File | Change |
|------|--------|
| `src/app/features/admin/pages/oversight/analytics.component.html` | Replace template layout with new design sections. |
| `src/app/features/admin/pages/oversight/analytics.component.scss` | Add glass‑panel styles, gold accents, responsive grid. |
| `src/app/features/admin/pages/oversight/analytics.component.ts` | Update chart option colours to `#e4c285` and dark backgrounds. **No logic changes.** |

## 3. Template Structure (`analytics.component.html`)

The template is restructured into the following sections while keeping all Angular bindings intact:

```html
<div class="analytics-page">
  <!-- Temporal Compass (Date Presets + Category Dropdown) -->
  <section class="temporal-compass">
    <div class="compass-left">
      <h2 class="page-title">Performance Oversight</h2>
      <p class="page-subtitle">A panoramic view of the estate's vital signs and fiscal trajectories.</p>
    </div>
    <div class="compass-right glass-panel">
      <div class="preset-buttons">
        <button class="preset-btn" [class.active]="presetControl.value === 'last7'" (click)="presetControl.setValue('last7'); onPresetChange()">LAST 7 DAYS</button>
        <button class="preset-btn" [class.active]="presetControl.value === 'last30'" (click)="presetControl.setValue('last30'); onPresetChange()">30 DAYS</button>
        <button class="preset-btn" [class.active]="presetControl.value === 'thisMonth'" (click)="presetControl.setValue('thisMonth'); onPresetChange()">THIS MONTH</button>
        <button class="preset-btn" [class.active]="presetControl.value === 'custom'" (click)="presetControl.setValue('custom')">CUSTOM</button>
      </div>
      @if (presetControl.value === 'custom') {
        <div class="custom-dates">
          <input type="date" class="minimal-input" [formControl]="startDateCtrl" />
          <input type="date" class="minimal-input" [formControl]="endDateCtrl" />
          <button class="apply-btn" (click)="applyCustomRange()">APPLY</button>
        </div>
      }
      <div class="category-filter">
        <span class="filter-label">CATEGORY: {{ categoryControl.value === 'all' ? 'ALL' : categoryControl.value.toUpperCase() }}</span>
        <mat-select class="category-select" [formControl]="categoryControl" (selectionChange)="onCategoryChange()">
          <mat-option value="all">All</mat-option>
          <mat-option value="revenue">Revenue</mat-option>
          <mat-option value="operations">Operations</mat-option>
          <mat-option value="guests">Guests</mat-option>
        </mat-select>
      </div>
    </div>
  </section>

  <!-- Loading / Error -->
  @if (loading() && !analytics()) {
    <div class="loading-container"><mat-spinner diameter="40"></mat-spinner></div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchData()">Retry</button>
    </app-alert>
  } @else {
    <!-- Pillars of Performance (KPI Cards) -->
    <section class="kpi-grid">
      @for (kpi of kpiCards(); track kpi.label) {
        <div class="kpi-card glass-panel">
          <div class="kpi-header">
            <span class="kpi-label">{{ kpi.label.toUpperCase() }}</span>
            <span class="material-symbols-outlined kpi-icon">insights</span>
          </div>
          <div class="kpi-value">{{ kpi.value }}</div>
          <div class="kpi-divider"></div>
        </div>
      }
    </section>

    <!-- The Grand Tapestry (Charts) -->
    <section class="charts-grid">
      <!-- Revenue Architecture (Bar Chart) -->
      <div class="chart-panel glass-panel">
        <div class="chart-header">
          <h3>Revenue Architecture</h3>
          <span class="chart-meta">BY REVENUE STREAM</span>
        </div>
        <div echarts [options]="revenueChartOptions()" class="chart-container"></div>
      </div>

      <!-- Yield Trajectory (Line Chart) -->
      <div class="chart-panel glass-panel">
        <div class="chart-header">
          <h3>Yield Trajectory</h3>
          <span class="chart-meta">TEMPORAL TRENDS</span>
        </div>
        <div echarts [options]="lineChartOptions()" class="chart-container"></div>
      </div>

      <!-- Service Equilibrium (Radar Chart) -->
      <div class="chart-panel glass-panel">
        <div class="chart-header">
          <h3>Service Equilibrium</h3>
          <span class="chart-meta">MULTI‑FACET PERFORMANCE</span>
        </div>
        <div echarts [options]="radarChartOptions()" class="chart-container"></div>
      </div>

      <!-- Expenditure Portfolio (Pie Chart) -->
      <div class="chart-panel glass-panel" @if="categoryControl.value !== 'revenue' && categoryControl.value !== 'operations'">
        <div class="chart-header">
          <h3>Expenditure Portfolio</h3>
          <span class="chart-meta">ALLOCATION RATIO</span>
        </div>
        <div echarts [options]="pieChartOptions()" class="chart-container"></div>
      </div>
    </section>
  }

  <!-- Atmospheric Banner -->
  <section class="atmospheric-banner">
    <div class="banner-image" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuBZiyyembcRJZsE5MGmBOTEGjfHqeVoxqR9ZdI3z1MBVpuDI1MBJVBc9oX199G0CM2gzv21CKru2WWOGRS8sRDB10ZkTe2PSguPvVNobXI0rVpzsNvXtBAkh1sM1afLvAvu-1-gJ3rTcDwS61yrLtrPem-3_6vXozoUNXehbsRwtqWEWSWDltrVGTgbZ_jjmjf2aOSqqGY5aDyAR5bwNBf9JuwXRxVqTg55e2TCRYfB2MQBR7vGQZHpCFVyfiFpYt9ovXADU2sX-kx')"></div>
    <div class="banner-overlay"></div>
  </section>
</div>
```

**Important:** The `@if` condition on the pie chart panel must use Angular control flow syntax correctly. If the condition is `categoryControl.value !== 'revenue' && categoryControl.value !== 'operations'`, we can use a computed signal in the component class to derive a boolean. Since we cannot change the TS file, we can wrap the pie chart div with an `*ngIf` or use the existing condition. The original component already had a conditional for the pie chart; we'll replicate that using the existing approach.

## 4. SCSS (`analytics.component.scss`)

```scss
@import '../../../../styles/theme/index';

.analytics-page {
  padding: 2rem var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 2rem var(--margin-mobile);
  }
  background: var(--color-background);
}

// ── Temporal Compass ──────────────────────────────
.temporal-compass {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: flex-end;
  gap: 2rem;
  margin-bottom: 3rem;
}
.compass-left {
  .page-title {
    @include font-headline-md;
    color: var(--color-secondary);
    font-style: italic;
    margin-bottom: 0.5rem;
  }
  .page-subtitle {
    @include font-body-md;
    color: var(--color-on-surface-variant);
    max-width: 400px;
  }
}
.compass-right {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 1.5rem;
  padding: 0.75rem 1.5rem;
  .preset-buttons {
    display: flex;
    gap: 0.5rem;
    border-right: 1px solid rgba(228, 194, 133, 0.1);
    padding-right: 1.5rem;
  }
  .preset-btn {
    @include font-label-caps;
    font-size: 0.625rem;
    background: transparent;
    border: none;
    color: var(--color-on-surface-variant);
    cursor: pointer;
    padding: 0.25rem 0.5rem;
    transition: color 0.3s;
    &.active {
      color: var(--color-secondary);
      position: relative;
      &::after {
        content: '';
        position: absolute;
        bottom: -4px;
        left: 0;
        width: 100%;
        height: 1px;
        background: var(--color-secondary);
      }
    }
    &:hover { color: var(--color-secondary); }
  }
  .custom-dates {
    display: flex;
    gap: 0.5rem;
    .minimal-input {
      background: transparent;
      border: none;
      border-bottom: 1px solid rgba(228, 194, 133, 0.4);
      color: var(--color-on-surface);
      font-family: var(--font-body);
      font-size: 0.75rem;
      padding: 0.25rem 0;
      outline: none;
    }
    .apply-btn {
      @include font-label-caps;
      font-size: 0.625rem;
      background: var(--color-secondary);
      color: var(--color-on-secondary);
      border: none;
      padding: 0.25rem 0.75rem;
      cursor: pointer;
    }
  }
  .category-filter {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    .filter-label {
      @include font-label-caps;
      font-size: 0.625rem;
      color: var(--color-on-surface-variant);
    }
    .category-select {
      // Override Material select to be minimal
      ::ng-deep .mat-mdc-select-value { color: var(--color-secondary); }
      ::ng-deep .mat-mdc-form-field-underline { display: none; }
    }
  }
}

// ── Glass Panel ───────────────────────────────────
.glass-panel {
  background: rgba(26, 26, 26, 0.6);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(228, 194, 133, 0.1);
  transition: transform 0.4s cubic-bezier(0.25, 0.46, 0.45, 0.94), border-color 0.4s, box-shadow 0.4s;
  &:hover {
    transform: translateY(-4px);
    border-color: rgba(228, 194, 133, 0.3);
    box-shadow: 0 0 40px rgba(228, 194, 133, 0.05);
  }
}

// ── KPI Grid ──────────────────────────────────────
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--gutter);
  margin-bottom: 3rem;
  @media (max-width: 1200px) { grid-template-columns: repeat(2, 1fr); }
  @media (max-width: 600px) { grid-template-columns: 1fr; }
}
.kpi-card {
  padding: 2rem;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  min-height: 200px;
  border-top: 2px solid rgba(228, 194, 133, 0.4);
  .kpi-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    .kpi-label {
      @include font-label-caps;
      font-size: 0.625rem;
      letter-spacing: 0.2em;
      color: var(--color-on-surface-variant);
    }
    .kpi-icon {
      color: rgba(228, 194, 133, 0.4);
      font-size: 1.25rem;
    }
  }
  .kpi-value {
    @include font-display-lg;
    font-size: 2.5rem;
    color: var(--color-secondary);
    font-style: italic;
    margin: 0.5rem 0;
  }
  .kpi-divider {
    height: 1px;
    width: 100%;
    background: rgba(228, 194, 133, 0.1);
    margin-top: 0.5rem;
  }
}

// ── Charts Grid ───────────────────────────────────
.charts-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--gutter);
  margin-bottom: 3rem;
  @media (max-width: 1024px) { grid-template-columns: 1fr; }
}
.chart-panel {
  padding: 2rem;
  min-height: 500px;
  display: flex;
  flex-direction: column;
  .chart-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
    h3 {
      @include font-headline-sm;
      color: var(--color-on-surface);
      font-style: italic;
    }
    .chart-meta {
      @include font-label-caps;
      font-size: 0.625rem;
      color: var(--color-on-surface-variant);
    }
  }
  .chart-container {
    flex: 1;
    width: 100%;
    min-height: 350px;
  }
}

// ── Atmospheric Banner ─────────────────────────────
.atmospheric-banner {
  position: relative;
  width: 100%;
  height: 500px;
  overflow: hidden;
  margin-top: 4rem;
  filter: grayscale(1) contrast(1.25);
  opacity: 0.4;
  .banner-image {
    position: absolute;
    inset: 0;
    background-size: cover;
    background-position: center;
  }
  .banner-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(to top, var(--color-background) 0%, transparent 50%, var(--color-background) 100%);
  }
}

// ── Loading ───────────────────────────────────────
.loading-container {
  display: flex;
  justify-content: center;
  padding: 4rem 0;
}
```

## 5. Chart Colour Configuration Updates (TypeScript)

In `analytics.component.ts`, update the chart option signals to use the gold colour palette. These are minor value changes to the existing `revenueChartOptions`, `lineChartOptions`, `radarChartOptions`, and `pieChartOptions` computed signals. **No new imports or logic changes.**

**Revenue Chart (Bar):**
- `series.color: '#e4c285'`
- `backgroundColor: 'transparent'`
- `textStyle: { color: '#c4c7c7' }`
- `xAxis.axisLine.lineStyle.color: 'rgba(228,194,133,0.3)'`
- `yAxis.splitLine.lineStyle.color: 'rgba(228,194,133,0.1)'`

**Line Chart:**
- `series.color: '#e4c285'`
- Same background/text adjustments as bar.

**Radar Chart:**
- `series[0].lineStyle.color: '#e4c285'`
- `series[0].areaStyle.color: 'rgba(228,194,133,0.05)'`
- `radar.axisLine.lineStyle.color: 'rgba(228,194,133,0.2)'`

**Pie Chart:**
- `series[0].data` items use colours `#e4c285`, `#d5b478`, `#5d4514`.
- `backgroundColor: 'transparent'`

The agent must update the existing option objects in the component file without altering any control flow, signals, or methods.

## 6. Responsive Behaviour
- KPI grid: 4 columns on desktop → 2 on tablet → 1 on mobile.
- Charts grid: 2 columns → 1 on screens ≤ 1024px.
- Temporal compass wraps on narrow screens.
- Glass panels retain hover effects on desktop.

## 7. Integration Notes
- The existing `presetControl`, `categoryControl`, `startDateCtrl`, `endDateCtrl` form controls are bound exactly as before.
- The `kpiCards()`, `revenueChartOptions()`, etc. computed signals are used unchanged.
- The `categoryControl` dropdown uses a minimal Material select; the global overrides from the shared component specsheet will further style it.
- The pie chart conditional visibility logic is preserved from the original component.
- No new dependencies or services are added.

## 8. Self‑Review Checklist
- [ ] Date presets and custom date picker work as before.
- [ ] Category dropdown filters the charts correctly.
- [ ] KPI cards display dynamic data from the `analytics` signal.
- [ ] Four chart panels render with gold colour scheme and dark backgrounds.
- [ ] Pie chart hides for Revenue/Operations categories (existing logic preserved).
- [ ] Atmospheric banner displays at the bottom of the page.
- [ ] Responsive layout adapts to mobile and tablet.
- [ ] No console errors; all API calls remain functional.
- [ ] No changes to services, guards, or routing.

