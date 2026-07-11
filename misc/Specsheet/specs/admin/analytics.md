# Specsheet: Admin Analytics Page (Final Deterministic)

## 1. Purpose

- Replace the `PlaceholderAnalyticsComponent` with the redesigned Analytics page.
- Single view with a category dropdown (All, Revenue, Operations, Guests) that changes the data displayed in four fixed chart panels: Bar, Line, Radar, Pie.
- Date range selection via presets and custom picker; initial load without date params.
- No gauge charts, no comparison mode.
- Uses `ngx-echarts` with global `provideEchartsCore` already configured; only `NgxEchartsDirective` is imported standalone.

## 2. Route & Navigation

- Path: `/operations/admin/oversight/analytics` (lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/oversight/analytics.component.ts`.

## 3. Authorization

- Inherits `adminGuard` from parent route.

## 4. Component API (AnalyticsComponent)

- **Selector**: `app-analytics` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatDatepickerModule`, `MatNativeDateModule`, `MatFormFieldModule`, `MatInputModule`, `MatProgressSpinnerModule`, `MatButtonToggleModule`, `MatSelectModule`, `NgxEchartsDirective`, `AlertComponent`.
- **Exact import paths** (use these verbatim):
  ```ts
  import { CommonModule } from "@angular/common";
  import { Component, inject, signal, computed } from "@angular/core";
  import { ReactiveFormsModule, FormControl } from "@angular/forms";
  import { MatCardModule } from "@angular/material/card";
  import { MatButtonModule } from "@angular/material/button";
  import { MatIconModule } from "@angular/material/icon";
  import { MatDatepickerModule } from "@angular/material/datepicker";
  import { MatNativeDateModule } from "@angular/material/core";
  import { MatFormFieldModule } from "@angular/material/form-field";
  import { MatInputModule } from "@angular/material/input";
  import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
  import { MatButtonToggleModule } from "@angular/material/button-toggle";
  import { MatSelectModule } from "@angular/material/select";
  import { NgxEchartsDirective } from "ngx-echarts";
  import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
  import { DestroyRef } from "@angular/core";
  import { AnalyticsApiService } from "../../services/analytics-api.service";
  import { AnalyticsDashboardDTO } from "../../models/analytics-dashboard.dto";
  import { AlertComponent } from "../../../../shared/components/alert/alert.component";
  ```
- **No `providers` array** – `provideEchartsCore({ echarts })` is already in `app.config.ts` globally.

## 5. Template Structure

```html
<div class="analytics-page">
  <!-- Controls: Presets + Custom Date + Category Dropdown -->
  <div class="controls">
    <div class="date-controls">
      <mat-button-toggle-group
        [formControl]="presetControl"
        (change)="onPresetChange()"
      >
        <mat-button-toggle value="last7">Last 7 days</mat-button-toggle>
        <mat-button-toggle value="last30">Last 30 days</mat-button-toggle>
        <mat-button-toggle value="thisMonth">This month</mat-button-toggle>
        <mat-button-toggle value="custom">Custom</mat-button-toggle>
      </mat-button-toggle-group>
      @if (presetControl.value === 'custom') {
      <mat-form-field appearance="outline">
        <mat-label>Start date</mat-label>
        <input
          matInput
          [matDatepicker]="startPicker"
          [formControl]="startDateCtrl"
        />
        <mat-datepicker-toggle
          matSuffix
          [for]="startPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker #startPicker></mat-datepicker>
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>End date</mat-label>
        <input
          matInput
          [matDatepicker]="endPicker"
          [formControl]="endDateCtrl"
        />
        <mat-datepicker-toggle
          matSuffix
          [for]="endPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker #endPicker></mat-datepicker>
      </mat-form-field>
      <button
        mat-raised-button
        color="primary"
        (click)="applyCustomRange()"
      >
        Apply
      </button>
      }
    </div>
    <mat-form-field
      appearance="outline"
      class="category-select"
    >
      <mat-label>Category</mat-label>
      <mat-select
        [formControl]="categoryControl"
        (selectionChange)="onCategoryChange()"
      >
        <mat-option value="all">All</mat-option>
        <mat-option value="revenue">Revenue</mat-option>
        <mat-option value="operations">Operations</mat-option>
        <mat-option value="guests">Guests</mat-option>
      </mat-select>
    </mat-form-field>
  </div>

  <!-- Loading / Error -->
  @if (loading() && !analytics()) {
  <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchData()"
    >
      Retry
    </button>
  </app-alert>
  } @else {
  <!-- KPI Summary Cards (top row) -->
  <div class="kpi-row">
    <mat-card
      ><mat-card-title>Total Revenue</mat-card-title
      ><mat-card-content
        >{{ analytics()?.totalRevenue | currency }}</mat-card-content
      ></mat-card
    >
    <mat-card
      ><mat-card-title>Occupancy Rate</mat-card-title
      ><mat-card-content
        >{{ analytics()?.occupancyRate }}%</mat-card-content
      ></mat-card
    >
    <mat-card
      ><mat-card-title>Guest Satisfaction</mat-card-title
      ><mat-card-content
        >{{ analytics()?.guestSatisfactionScore }}%</mat-card-content
      ></mat-card
    >
    <mat-card
      ><mat-card-title>Avg Daily Rate</mat-card-title
      ><mat-card-content
        >{{ analytics()?.averageDailyRate | currency }}</mat-card-content
      ></mat-card
    >
  </div>

  <!-- Fixed Chart Grid -->
  <div class="charts-grid">
    <div class="chart-container">
      <div
        echarts
        [options]="barChartOptions()"
        class="chart"
      ></div>
    </div>
    <div class="chart-container">
      <div
        echarts
        [options]="lineChartOptions()"
        class="chart"
      ></div>
    </div>
    <div class="chart-container">
      <div
        echarts
        [options]="radarChartOptions()"
        class="chart"
      ></div>
    </div>
    <div class="chart-container">
      <div
        echarts
        [options]="pieChartOptions()"
        class="chart"
      ></div>
    </div>
  </div>
  }
</div>
```

## 6. State Management (All Signals)

```ts
// Data
analytics = signal<AnalyticsDashboardDTO | null>(null);
loading = signal(false);
error = signal<string | null>(null);

// Date controls
presetControl = new FormControl<"last7" | "last30" | "thisMonth" | "custom">(
  "last7",
  { nonNullable: true },
);
startDateCtrl = new FormControl<Date | null>(null);
endDateCtrl = new FormControl<Date | null>(null);

// Category dropdown
categoryControl = new FormControl<"all" | "revenue" | "operations" | "guests">(
  "all",
  { nonNullable: true },
);
```

## 7. Data Flow & API Calls

- `AnalyticsApiService` (already built, root‑provided). Method:
  ```ts
  getAnalytics(params?: { startDate?: string; endDate?: string }): Observable<AnalyticsDashboardDTO>
  ```
- Initial load: `ngOnInit` calls `fetchData()` with no dates.
- `fetchData(startDate?: string, endDate?: string)`:
  ```ts
  this.loading.set(true);
  this.error.set(null);
  this.analyticsApi
    .getAnalytics({ startDate, endDate })
    .pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false)),
    )
    .subscribe({
      next: (data) => this.analytics.set(data),
      error: (err: any) =>
        this.error.set(err instanceof Error ? err.message : "Unexpected error"),
    });
  ```
- Preset changes: `onPresetChange()` calculates dates, calls `fetchData`.
- Custom range: `applyCustomRange()` converts Date objects to ISO strings (start `T00:00:00.000Z`, end `T23:59:59.999Z`), calls `fetchData`.
- Category change: no API call; only recomputes chart options.

## 8. Category‑Dependent Chart Options (All Computed Signals)

**Exact mapping of data fields to categories:**

| Category   | Bar & Line charts show                                        | Radar chart shows                                   | Pie chart shows          |
| ---------- | ------------------------------------------------------------- | --------------------------------------------------- | ------------------------ |
| All        | Revenue, Turnover, RevPAR, AvgDailyRate                       | Occupancy, Cancellation, LengthOfStay, Satisfaction | Food vs Amenity spend    |
| Revenue    | Revenue, Turnover, RevPAR, AvgDailyRate                       | Occupancy, RevPAR, AvgDailyRate, Turnover           | _hidden_ (empty options) |
| Operations | Occupancy, Cancellation, LengthOfStay, HousekeepingTurnaround | Occupancy, Cancellation, LengthOfStay, Satisfaction | _hidden_                 |
| Guests     | Satisfaction, FoodSpend, AmenitySpend                         | Satisfaction, Occupancy, LengthOfStay               | Food vs Amenity spend    |

**Bar Chart Options:**

```ts
barChartOptions = computed(() => {
  const d = this.analytics();
  if (!d) return {};
  const cat = this.categoryControl.value;
  let xData: string[] = [];
  let yData: number[] = [];
  switch (cat) {
    case "all":
    case "revenue":
      xData = ["Total Revenue", "Gross Turnover", "RevPAR", "Avg Daily Rate"];
      yData = [d.totalRevenue, d.grossTurnover, d.revPAR, d.averageDailyRate];
      break;
    case "operations":
      xData = ["Occupancy", "Cancellation", "Length of Stay", "HK Turnaround"];
      yData = [
        d.occupancyRate,
        d.cancellationRate,
        d.averageLengthOfStay,
        d.averageHousekeepingTurnaroundMinutes,
      ];
      break;
    case "guests":
      xData = ["Satisfaction", "Food Spend", "Amenity Spend"];
      yData = [
        d.guestSatisfactionScore,
        d.nonRoomExpenditure.totalFoodSpend,
        d.nonRoomExpenditure.totalAmenitySpend,
      ];
      break;
  }
  return {
    title: { text: "Overview" },
    tooltip: { trigger: "axis" },
    xAxis: { type: "category", data: xData },
    yAxis: { type: "value" },
    series: [{ type: "bar", data: yData, color: "#1976d2" }],
  };
});
```

**Line Chart Options (same data as bar, but line):**

```ts
lineChartOptions = computed(() => {
  // identical data extraction as bar, but series type: 'line'
  const d = this.analytics();
  if (!d) return {};
  const cat = this.categoryControl.value;
  let xData: string[] = [];
  let yData: number[] = [];
  // ... same switch logic as above ...
  return {
    title: { text: "Trend" },
    tooltip: { trigger: "axis" },
    xAxis: { type: "category", data: xData },
    yAxis: { type: "value" },
    series: [{ type: "line", data: yData, color: "#388e3c" }],
  };
});
```

**Radar Chart Options:**

```ts
radarChartOptions = computed(() => {
  const d = this.analytics();
  if (!d) return {};
  const cat = this.categoryControl.value;
  let indicator: any[] = [];
  let value: number[] = [];
  switch (cat) {
    case "all":
      indicator = [
        { name: "Occupancy", max: 100 },
        { name: "Cancellation", max: 50 },
        { name: "Length of Stay", max: 30 },
        { name: "Satisfaction", max: 100 },
      ];
      value = [
        d.occupancyRate,
        d.cancellationRate,
        d.averageLengthOfStay,
        d.guestSatisfactionScore,
      ];
      break;
    case "revenue":
      indicator = [
        { name: "Occupancy", max: 100 },
        { name: "RevPAR", max: 2000 },
        { name: "Avg Daily Rate", max: 500 },
        { name: "Turnover", max: 20000 },
      ];
      value = [d.occupancyRate, d.revPAR, d.averageDailyRate, d.grossTurnover];
      break;
    case "operations":
      indicator = [
        { name: "Occupancy", max: 100 },
        { name: "Cancellation", max: 50 },
        { name: "Length of Stay", max: 30 },
        { name: "Satisfaction", max: 100 },
      ];
      value = [
        d.occupancyRate,
        d.cancellationRate,
        d.averageLengthOfStay,
        d.guestSatisfactionScore,
      ];
      break;
    case "guests":
      indicator = [
        { name: "Satisfaction", max: 100 },
        { name: "Occupancy", max: 100 },
        { name: "Length of Stay", max: 30 },
      ];
      value = [
        d.guestSatisfactionScore,
        d.occupancyRate,
        d.averageLengthOfStay,
      ];
      break;
  }
  return {
    title: { text: "Radar Overview" },
    radar: { indicator },
    series: [{ type: "radar", data: [{ value, name: "Current" }] }],
  };
});
```

**Pie Chart Options:**

```ts
pieChartOptions = computed(() => {
  const d = this.analytics();
  if (!d) return {};
  const cat = this.categoryControl.value;
  if (cat === "revenue" || cat === "operations") {
    return {}; // hidden (no data)
  }
  return {
    title: { text: "Expenditure Breakdown" },
    tooltip: { trigger: "item" },
    series: [
      {
        type: "pie",
        data: [
          { name: "Food", value: d.nonRoomExpenditure.totalFoodSpend },
          { name: "Amenities", value: d.nonRoomExpenditure.totalAmenitySpend },
        ],
        label: { formatter: "{b}: {c} ({d}%)" },
      },
    ],
  };
});
```

**Note:** When a chart is meant to be hidden (e.g., pie for revenue/operations), returning an empty object `{}` will render an empty div; it’s acceptable. The container will not show a chart. To be extra safe, we can use `*ngIf` on the container, but using signals and empty options works. In the template, we can wrap the pie chart with `@if (pieChartOptions() && pieChartOptions().series) { ... }` or simply let echarts handle empty. We'll use a conditional wrapper to avoid any rendering issues:

```html
@if (categoryControl.value !== 'revenue' && categoryControl.value !==
'operations') {
<div class="chart-container">
  <div
    echarts
    [options]="pieChartOptions()"
    class="chart"
  ></div>
</div>
}
```

This is simpler and deterministic.

## 9. Date Presets Logic

```ts
private getPresetDates(preset: string): { start: string; end: string } | null {
  const now = new Date();
  let start: Date, end: Date = now;
  switch (preset) {
    case 'last7':
      start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
      break;
    case 'last30':
      start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
      break;
    case 'thisMonth':
      start = new Date(now.getFullYear(), now.getMonth(), 1);
      break;
    default:
      return null;
  }
  start.setHours(0, 0, 0, 0);
  end.setHours(23, 59, 59, 999);
  return { start: start.toISOString(), end: end.toISOString() };
}
```

Called in `onPresetChange()` and, if custom, in `applyCustomRange()` via date pickers.

## 10. UI States

- Loading: full‑page spinner only when no data loaded yet; else, the charts simply re‑render with new data (no extra spinner needed, but the KPI cards will update immediately).
- Error: `app-alert` with retry button.
- If analytics is null, show a message “No data available”.

## 11. Responsive Behaviour

- Charts use `width: 100%; height: 400px;` by default; on mobile, height can be 300px.
- The grid of four charts collapses to two columns on tablet, one column on mobile.
- KPI cards stack in a single column on small screens.
- Date controls and category dropdown stack vertically.

## 12. Accessibility

- Each chart div has `aria-label` describing the chart (e.g., “Bar chart of revenue metrics”).
- Category dropdown and date pickers are labelled.
- KPI cards have appropriate heading levels.

## 13. Integration Notes

- **Overwrite** placeholder: `src/app/features/admin/pages/oversight/analytics.component.ts`.
- The global echarts setup (`provideEchartsCore({ echarts })`) must exist in `app.config.ts`; the dashboard already relies on it. If not present, add it: `import { provideEchartsCore } from 'ngx-echarts';` and include it in `providers`.
- No additional `providers` or `provideEcharts` inside the component.
- Only `NgxEchartsDirective` is imported; the component template uses `[echarts]` binding.

## 14. File Structure

```
src/app/features/admin/
  pages/oversight/
    analytics.component.ts   (overwrite)
    analytics.component.html
    analytics.component.scss
  services/
    analytics-api.service.ts (already exists)
  models/
    analytics-dashboard.dto.ts (already exists)
```

## 15. Self‑Review Checklist

- [ ] Page loads with unrestricted analytics (no date params).
- [ ] Changing presets fetches data with calculated dates.
- [ ] Custom date picker works, ISO strings correctly formatted.
- [ ] Category dropdown changes chart data, but not chart arrangement.
- [ ] “All” shows revenue bar/line, radar with occupancy/satisfaction/etc., pie with food/amenities.
- [ ] “Revenue” hides pie, shows revenue KPIs in bar/line/radar.
- [ ] “Operations” hides pie, shows operational KPIs.
- [ ] “Guests” shows guest satisfaction, spend, and pie.
- [ ] KPI cards update with new data on fetch.
- [ ] Loading/error states handled.
- [ ] Responsive layout adapts.
- [ ] No console errors; all subscriptions use `takeUntilDestroyed`.
- [ ] No `providers` array in the component; echarts works globally.

## 16. Implementation Constraints

- Angular 18 control flow, standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder; do not rename.
- Use `NgxEchartsDirective` directly; no `NgxEchartsModule` or `provideEcharts` in component.
- All chart options must match the provided code exactly.
- No external state management; all signals local.
- The `AnalyticsDashboardDTO` and `AnalyticsApiService` must already exist (from dashboard) or be created in this spec using the same definitions.

