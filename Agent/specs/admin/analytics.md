# Specsheet: Admin Analytics Page (Final Deterministic)

## 1. Purpose

- Replace the `PlaceholderAnalyticsComponent` with a full‑featured Analytics page.
- Displays KPIs from the `/analytics` endpoint, organised into tabbed sections: **Revenue**, **Operations**, **Guests**, **Comparison**.
- Date range selection with presets (“Last 7 days”, “Last 30 days”, “This month”, “Custom”) and a custom date picker.
- Initial load fetches data without date parameters (unrestricted).
- Comparison mode allows selecting two date ranges and viewing side‑by‑side KPI cards and a bar chart.
- All charts use `ngx-echarts` and are described with exact ECharts options.

## 2. Route & Navigation

- Path: `/operations/admin/oversight/analytics` (lazy‑loaded under Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/oversight/analytics.component.ts`.

## 3. Authorization

- Inherits `adminGuard` from parent route.

## 4. Component API (AnalyticsComponent)

- **Selector**: `app-analytics` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatTabsModule`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatDatepickerModule`, `MatNativeDateModule`, `MatFormFieldModule`, `MatInputModule`, `MatProgressSpinnerModule`, `MatButtonToggleModule`, `NgxEchartsModule`, `AlertComponent`, `AnalyticsApiService`, `DestroyRef`.
- **Exact import paths**:
  ```ts
  import { Component, inject, signal, computed } from "@angular/core";
  import { CommonModule } from "@angular/common";
  import { ReactiveFormsModule, FormControl } from "@angular/forms";
  import { MatTabsModule } from "@angular/material/tabs";
  import { MatCardModule } from "@angular/material/card";
  import { MatButtonModule } from "@angular/material/button";
  import { MatIconModule } from "@angular/material/icon";
  import { MatDatepickerModule } from "@angular/material/datepicker";
  import { MatNativeDateModule } from "@angular/material/core";
  import { MatFormFieldModule } from "@angular/material/form-field";
  import { MatInputModule } from "@angular/material/input";
  import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
  import { MatButtonToggleModule } from "@angular/material/button-toggle";
  import { NgxEchartsModule } from "ngx-echarts";
  import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
  import { DestroyRef } from "@angular/core";
  import { AnalyticsApiService } from "../../services/analytics-api.service";
  import { AnalyticsDashboardDTO } from "../../models/analytics-dashboard.dto";
  import { AlertComponent } from "../../../../shared/components/alert/alert.component";
  ```
- **Template** (high‑level):

  ```html
  <div class="analytics-page">
    <!-- Date range controls -->
    <div class="controls">
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
    <!-- Tab group -->
    <mat-tab-group (selectedTabChange)="onTabChange($event)">
      <mat-tab label="Revenue"> ... revenue KPI cards & charts ... </mat-tab>
      <mat-tab label="Operations"> ... operations cards & charts ... </mat-tab>
      <mat-tab label="Guests"> ... guest cards & charts ... </mat-tab>
      <mat-tab label="Comparison">
        @if (!comparisonMode()) {
        <button
          mat-raised-button
          (click)="comparisonMode.set(true)"
        >
          Enable Comparison
        </button>
        } @else {
        <!-- comparison date pickers, fetch second set, side‑by‑side KPI cards and bar chart -->
        }
      </mat-tab>
    </mat-tab-group>
    }
  </div>
  ```

## 5. State Management (All Signals)

```ts
// Data
analytics = signal<AnalyticsDashboardDTO | null>(null);
loading = signal(false);
error = signal<string | null>(null);

// Date range
presetControl = new FormControl<"last7" | "last30" | "thisMonth" | "custom">(
  "last7",
  { nonNullable: true },
);
startDateCtrl = new FormControl<Date | null>(null);
endDateCtrl = new FormControl<Date | null>(null);

// Comparison
comparisonMode = signal(false);
comparisonAnalytics = signal<AnalyticsDashboardDTO | null>(null);
comparisonStartDateCtrl = new FormControl<Date | null>(null);
comparisonEndDateCtrl = new FormControl<Date | null>(null);
comparisonPresetControl = new FormControl<
  "last7" | "last30" | "thisMonth" | "custom"
>("last7", { nonNullable: true });
```

## 6. Data Flow & API Calls

- `AnalyticsApiService.getAnalytics(params?: { startDate?: string; endDate?: string }): Observable<AnalyticsDashboardDTO>`
- Initial load: `ngOnInit` calls `fetchData()` without dates.
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
- Preset changes: `onPresetChange()` calculates start/end based on preset and current date, calls `fetchData(start, end)`.
- Custom range: `applyCustomRange()` converts picker values to ISO strings (start at 00:00:00, end at 23:59:59) and calls `fetchData`.
- Comparison: similar logic but for the second set, stored in `comparisonAnalytics`. Two fetch calls managed.

## 7. Tabs & Exact Chart Configurations (ngx-echarts)

### 7.1 Revenue Tab

**KPI Cards**: Total Revenue, Gross Turnover, RevPAR, Average Daily Rate.  
**Charts**:

- **Bar chart** (Revenue vs Turnover):
  ```ts
  revenueBarOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      title: { text: "Revenue & Turnover" },
      tooltip: { trigger: "axis" },
      xAxis: { type: "category", data: ["Total Revenue", "Gross Turnover"] },
      yAxis: { type: "value" },
      series: [
        {
          type: "bar",
          data: [d.totalRevenue, d.grossTurnover],
          color: "#1976d2",
        },
      ],
    };
  });
  ```
- **Gauge chart** (RevPAR):
  ```ts
  revParGaugeOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      series: [
        {
          type: "gauge",
          min: 0,
          max: Math.max(d.revPAR * 1.5, 1000),
          detail: { formatter: "${value}" },
          data: [{ value: d.revPAR, name: "RevPAR" }],
        },
      ],
    };
  });
  ```

### 7.2 Operations Tab

**KPI Cards**: Occupancy Rate, Average Housekeeping Turnaround, Cancellation Rate, Average Length of Stay.  
**Charts**:

- **Gauge** (Occupancy Rate):
  ```ts
  occupancyGaugeOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      series: [
        {
          type: "gauge",
          min: 0,
          max: 100,
          detail: { formatter: "{value}%" },
          data: [{ value: d.occupancyRate, name: "Occupancy" }],
        },
      ],
    };
  });
  ```
- **Radar chart** (Operational KPIs):
  ```ts
  radarOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      title: { text: "Operational Overview" },
      radar: {
        indicator: [
          { name: "Occupancy", max: 100 },
          { name: "Cancellation", max: 50 },
          { name: "Length of Stay", max: 30 },
          { name: "Satisfaction", max: 100 },
        ],
      },
      series: [
        {
          type: "radar",
          data: [
            {
              value: [
                d.occupancyRate,
                d.cancellationRate,
                d.averageLengthOfStay,
                d.guestSatisfactionScore,
              ],
              name: "Current",
            },
          ],
        },
      ],
    };
  });
  ```

### 7.3 Guests Tab

**KPI Cards**: Guest Satisfaction Score, Total Food Spend, Total Amenity Spend, Highest Spend Category.  
**Charts**:

- **Gauge** (Satisfaction):
  ```ts
  satisfactionGaugeOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      series: [
        {
          type: "gauge",
          min: 0,
          max: 100,
          detail: { formatter: "{value}%" },
          data: [{ value: d.guestSatisfactionScore, name: "Satisfaction" }],
        },
      ],
    };
  });
  ```
- **Pie chart** (Non‑Room Expenditure):
  ```ts
  expenditurePieOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      title: { text: "Non‑Room Expenditure" },
      tooltip: { trigger: "item" },
      series: [
        {
          type: "pie",
          data: [
            { name: "Food", value: d.nonRoomExpenditure.totalFoodSpend },
            {
              name: "Amenities",
              value: d.nonRoomExpenditure.totalAmenitySpend,
            },
          ],
          label: { formatter: "{b}: {c} ({d}%)" },
        },
      ],
    };
  });
  ```

### 7.4 Comparison Tab

- User can enable comparison mode. Two preset/date selectors appear for “Period 1” and “Period 2”.
- When both periods are selected, fetch two separate analytics results (stored in `analytics` and `comparisonAnalytics`).
- Display two sets of KPI cards side‑by‑side (total revenue, occupancy, etc.).
- **Bar chart** comparing key metrics between periods:
  ```ts
  comparisonBarOptions = computed(() => {
    const a = this.analytics();
    const b = this.comparisonAnalytics();
    if (!a || !b) return {};
    return {
      title: { text: "Comparison" },
      tooltip: { trigger: "axis" },
      legend: { data: ["Period 1", "Period 2"] },
      xAxis: {
        type: "category",
        data: ["Revenue", "Occupancy", "Satisfaction"],
      },
      yAxis: { type: "value" },
      series: [
        {
          name: "Period 1",
          type: "bar",
          data: [a.totalRevenue, a.occupancyRate, a.guestSatisfactionScore],
        },
        {
          name: "Period 2",
          type: "bar",
          data: [b.totalRevenue, b.occupancyRate, b.guestSatisfactionScore],
        },
      ],
    };
  });
  ```

## 8. Date Presets Logic

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

## 9. UI States

- Loading: full‑page spinner only if no data loaded yet; else a linear progress bar over the tabs.
- Error: `app-alert` with retry.
- Empty: not applicable, as the endpoint always returns an object.

## 10. Responsive Behaviour

- Tabs work normally on all screens; charts resize via echarts responsive options (set width/height 100%).
- KPI cards stack vertically on mobile.
- Date controls stack vertically on narrow screens.

## 11. Accessibility

- Charts have `aria-label`.
- Tabs have keyboard navigation.
- Date pickers labelled.

## 12. Integration Notes

- **Overwrite** existing placeholder: `src/app/features/admin/pages/oversight/analytics.component.ts`.
- `AnalyticsApiService` must be created (already used in dashboard, so we can reuse that service if it exists, or create a new one). We'll assume it exists under `features/admin/services/analytics-api.service.ts` (same as dashboard). If not, create it.
- Reuse `AlertComponent` from shared.
- No session storage; date range is transient.

## 13. File Structure

```
src/app/features/admin/
  pages/oversight/
    analytics.component.ts   (overwrite)
    analytics.component.html
    analytics.component.scss
  services/
    analytics-api.service.ts (if not already from dashboard)
  models/
    analytics-dashboard.dto.ts (already exists)
```

## 14. Self‑Review Checklist

- [ ] Page loads with unrestricted analytics data (no dates).
- [ ] Changing preset fetches data with correct date range.
- [ ] Custom date picker works; start/end times correctly formatted.
- [ ] Revenue tab shows KPI cards and bar + gauge charts.
- [ ] Operations tab shows gauge (occupancy) and radar chart.
- [ ] Guests tab shows gauge (satisfaction) and pie chart (expenditure).
- [ ] Comparison tab: enabling shows two date pickers; selecting dates fetches second dataset; side‑by‑side KPI cards and comparison bar chart appear.
- [ ] Loading/error states handled.
- [ ] Responsive layout works.
- [ ] No console errors, all subscriptions cleaned.

## 15. Implementation Constraints

- Angular 18 control flow, standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder; do not rename.
- Use `ngx-echarts` (already installed).
- Exact chart options must match the code provided.
- No external state management; all signals local.
- No modifications to shared components required.

