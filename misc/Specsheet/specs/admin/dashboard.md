# Specsheet: Admin Dashboard (Deterministic v1.0 – Final)

### 1. Purpose

- Replace the `PlaceholderDashboardComponent` with the real admin dashboard.
- Provide an overview of hotel operations:
    - Date‑filterable KPI cards from `/analytics`.
    - Pending housekeeping and maintenance task counts.
    - “Create Internal Ticket” button opening a modal.
    - Two charts (Revenue bar, Expenditure donut) using ngx‑echarts.
    - “Today’s Movement” table showing the latest 5 audit log entries.

### 2. Route & Navigation

- Route already exists: `/operations/admin/dashboard` (lazy‑loaded in Admin Shell).  
- **Do not modify the route config.**  
- The placeholder file at `src/app/features/admin/pages/dashboard.component.ts` will be **overwritten** with the real component.

### 3. Authorization

- Inherits `adminGuard` from parent route. No extra guard needed.

### 4. Component API (DashboardComponent)

- **Selector**: `app-admin-dashboard` (exactly as the placeholder’s selector)
- **Standalone**: `true`
- **Imports** (exact list):  
    `CommonModule`, `ReactiveFormsModule`, `RouterModule` (only if needed for `routerLink` — not strictly required),  
    Angular Material modules: `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatDatepickerModule`, `MatNativeDateModule`, `MatTableModule`, `MatDialogModule`, `MatRadioModule`, `MatFormFieldModule`, `MatInputModule`, `MatProgressSpinnerModule`, `MatTooltipModule`, `MatSnackBarModule`,  
    `NgxEchartsModule`,  
    `AlertComponent` (from `../../auth/components/alert.component`)
- **No inputs/outputs**.

### 5. Template Structure (exact)

```html
<div class="dashboard">
   
  <!-- Top bar -->
   
  <div class="top-bar">
       
    <div class="date-filter">
           
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
        (click)="applyDateFilter()"
      >
        Apply
      </button>
           
      <button
        mat-button
        (click)="clearDateFilter()"
      >
        Clear
      </button>
         
    </div>
       
    <button
      mat-raised-button
      color="accent"
      (click)="openCreateTicketDialog()"
    >
            <mat-icon>add_task</mat-icon> Create Internal Ticket    
    </button>
     
  </div>

   
  <!-- KPI Cards (6) -->
   
  <div class="kpi-row">
        @for (kpi of kpiCards(); track kpi.label) {      
    <mat-card class="kpi-card">
              <mat-card-title>{{ kpi.label }}</mat-card-title>        
      <mat-card-content>
                  <span class="value">{{ kpi.value }}</span>        
      </mat-card-content>
           
    </mat-card>
        }  
  </div>

   
  <!-- Middle row: Charts + Department Health -->
   
  <div class="middle-row">
       
    <div class="charts">
           
      <div class="chart-container">
                @if (analyticsLoading()) {          
        <mat-spinner diameter="40"></mat-spinner>         } @else if
        (analyticsError()) {          
        <app-alert
          type="error"
          [message]="analyticsError()!"
          (closed)="analyticsError.set(null)"
        >
                     
          <button
            mat-button
            (click)="loadAnalytics()"
          >
            Retry
          </button>
                   
        </app-alert>
                } @else {          
        <div
          echarts
          [options]="revenueChartOptions()"
          class="chart"
        ></div>
                }      
      </div>
           
      <div class="chart-container">
                @if (!analyticsLoading() && !analyticsError()) {          
        <div
          echarts
          [options]="expenditureChartOptions()"
          class="chart"
        ></div>
                }      
      </div>
         
    </div>
       
    <div class="health-cards">
            @if (pendingError()) {        
      <app-alert
        type="error"
        [message]="pendingError()!"
        (closed)="pendingError.set(null)"
      >
                 
        <button
          mat-button
          (click)="loadPendingCounts()"
        >
          Retry
        </button>
               
      </app-alert>
            }      
      <mat-card>
                <mat-card-title>Housekeeping Pending</mat-card-title>        
        <mat-card-content class="count">
                    @if (pendingLoading()) {
          <mat-spinner diameter="30"></mat-spinner> }           @else { {{
          housekeepingPendingCount() }} }        
        </mat-card-content>
             
      </mat-card>
           
      <mat-card>
                <mat-card-title>Maintenance Pending</mat-card-title>        
        <mat-card-content class="count">
                    @if (pendingLoading()) {
          <mat-spinner diameter="30"></mat-spinner> }           @else { {{
          maintenancePendingCount() }} }        
        </mat-card-content>
             
      </mat-card>
         
    </div>
     
  </div>

   
  <!-- Today's Movement Table -->
   
  <div class="movement-table">
       
    <h2>Today's Movement</h2>
        @if (auditLoading()) {       <mat-spinner diameter="30"></mat-spinner>  
      } @else if (auditError()) {      
    <app-alert
      type="error"
      [message]="auditError()!"
      (closed)="auditError.set(null)"
    >
             
      <button
        mat-button
        (click)="loadAuditLogs()"
      >
        Retry
      </button>
           
    </app-alert>
        } @else {      
    <table
      mat-table
      [dataSource]="auditEntries()"
    >
             
      <ng-container matColumnDef="timestamp">
                 
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Time
        </th>
                 
        <td
          mat-cell
          *matCellDef="let entry"
        >
          {{ entry.timestamp | date:'shortTime' }}
        </td>
               
      </ng-container>
             
      <ng-container matColumnDef="entity">
                 
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Entity
        </th>
                 
        <td
          mat-cell
          *matCellDef="let entry"
        >
          {{ entry.entityName }}
        </td>
               
      </ng-container>
             
      <ng-container matColumnDef="action">
                 
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Action
        </th>
                 
        <td
          mat-cell
          *matCellDef="let entry"
        >
          {{ entry.action }}
        </td>
               
      </ng-container>
             
      <ng-container matColumnDef="changedBy">
                 
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Changed By
        </th>
                 
        <td
          mat-cell
          *matCellDef="let entry"
        >
          {{ entry.changedByName }}
        </td>
               
      </ng-container>
             
      <ng-container matColumnDef="summary">
                 
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Summary
        </th>
                 
        <td
          mat-cell
          *matCellDef="let entry"
        >
          {{ getAuditSummary(entry) }}
        </td>
               
      </ng-container>
             
      <tr
        mat-header-row
        *matHeaderRowDef="displayedColumns"
      ></tr>
             
      <tr
        mat-row
        *matRowDef="let row; columns: displayedColumns"
      ></tr>
           
    </table>
          @if (auditEntries().length === 0 && !auditLoading()) {        
    <p class="empty-state">No recent activity.</p>
          }     }  
  </div>
</div>
```

### 6. State Management (All Signals & Form Controls)

**FormControls:**

```ts
startDateCtrl = new FormControl<Date | null>(null);
endDateCtrl = new FormControl<Date | null>(null);
```

**Signals:**

```ts
// Analytics
analytics = signal<AnalyticsDashboardDTO | null>(null);
analyticsLoading = signal(false);
analyticsError = signal<string | null>(null);

// Pending counts
housekeepingPendingCount = signal(0);
maintenancePendingCount = signal(0);
pendingLoading = signal(false);
pendingError = signal<string | null>(null);

// Audit logs
auditEntries = signal<AuditLogEntry[]>([]);
auditLoading = signal(false);
auditError = signal<string | null>(null);

// Ticket creation feedback
ticketCreatedMessage = signal<string | null>(null);

// Table column definition
displayedColumns = ["timestamp", "entity", "action", "changedBy", "summary"];
```

**KPI Cards Computed:**
Always return an array of 6 items; before analytics load, show placeholder dashes.

```ts
kpiCards = computed(() => {
  const a = this.analytics();
  if (!a) {
    return [
      { label: "Occupancy Rate", value: "—" },
      { label: "Avg Daily Rate", value: "—" },
      { label: "RevPAR", value: "—" },
      { label: "Guest Satisfaction", value: "—" },
      { label: "Cancellation Rate", value: "—" },
      { label: "Avg Length of Stay", value: "—" },
    ];
  }
  return [
    { label: "Occupancy Rate", value: `${a.occupancyRate}%` },
    { label: "Avg Daily Rate", value: `$${a.averageDailyRate}` },
    { label: "RevPAR", value: `$${a.revPAR}` },
    { label: "Guest Satisfaction", value: `${a.guestSatisfactionScore}%` },
    { label: "Cancellation Rate", value: `${a.cancellationRate}%` },
    { label: "Avg Length of Stay", value: `${a.averageLengthOfStay} days` },
  ];
});
```

**Chart Options Computed:**

```ts
revenueChartOptions = computed(() => {
  const a = this.analytics();
  if (!a) return {}; // empty object hides chart, spinner shown instead
  return {
    title: { text: "Revenue Overview" },
    tooltip: { trigger: "axis" },
    xAxis: { type: "category", data: ["Total Revenue", "Gross Turnover"] },
    yAxis: { type: "value" },
    series: [
      {
        type: "bar",
        data: [a.totalRevenue, a.grossTurnover],
        color: "#1976d2",
      },
    ],
  };
});

expenditureChartOptions = computed(() => {
  const a = this.analytics();
  if (!a) return {};
  return {
    title: { text: "Non‑Room Expenditure" },
    tooltip: { trigger: "item" },
    series: [
      {
        type: "pie",
        data: [
          { name: "Food", value: a.nonRoomExpenditure.totalFoodSpend },
          { name: "Amenities", value: a.nonRoomExpenditure.totalAmenitySpend },
        ],
        label: { formatter: "{b}: {c} ({d}%)" },
      },
    ],
  };
});
```

### 7. Data Flow & API Calls

**Service locations:** `src/app/features/admin/services/`

**API Services (root‑provided):**

- `AnalyticsApiService`
- `HousekeepingApiService`
- `MaintenanceApiService`
- `AuditLogApiService`

**Endpoints & DTOs:**

| Service                  | Method                  | Endpoint                             | Parameters                                                   | Response                                  |
| ------------------------ | ----------------------- | ------------------------------------ | ------------------------------------------------------------ | ----------------------------------------- |
| `AnalyticsApiService`    | `getAnalytics(params?)` | `GET /api/v1/analytics`              | `startDate?` (ISO 8601 string), `endDate?` (ISO 8601 string) | `AnalyticsDashboardDTO`                   |
| `HousekeepingApiService` | `getAll()`              | `GET /api/v1/housekeeping`           | `status=Pending`, `pageNumber=1`, `pageSize=1000`            | `HousekeepingTask[]` (from response body) |
| `MaintenanceApiService`  | `getAll()`              | `GET /api/v1/maintenance`            | `status=Pending`, `pageNumber=1`, `pageSize=1000`            | `MaintenanceTask[]`                       |
| `AuditLogApiService`     | `getAll(params)`        | `GET /api/v1/auditlogs`              | `sortBy=timestamp`, `sortDescending=true`, `pageSize=5`      | `AuditLogEntry[]`                         |
| `HousekeepingApiService` | `createInternal(body)`  | `POST /api/v1/housekeeping/internal` | `CreateInternalTicketRequest`                                | `void` (200 OK)                           |
| `MaintenanceApiService`  | `createInternal(body)`  | `POST /api/v1/maintenance/internal`  | `CreateInternalTicketRequest`                                | `void`                                    |

**DTOs (exact):**

```ts
// analytics-dashboard.dto.ts
export interface AnalyticsDashboardDTO {
  occupancyRate: number;
  averageDailyRate: number;
  revPAR: number;
  totalRevenue: number;
  grossTurnover: number;
  averageLengthOfStay: number;
  cancellationRate: number;
  guestSatisfactionScore: number;
  averageHousekeepingTurnaroundMinutes: number;
  nonRoomExpenditure: {
    totalFoodSpend: number;
    totalAmenitySpend: number;
    highestSpendCategory: string;
  };
}

// audit-log-entry.model.ts
export interface AuditLogEntry {
  id: number;
  entityName: string;
  action: string;
  recordId: { Id: number };
  oldValues: Record<string, any>;
  newValues: Record<string, any>;
  changedByEmail: string;
  changedByName: string;
  timestamp: string; // ISO 8601
}

// housekeeping-task.model.ts
export interface HousekeepingTask {
  id: number;
  roomId: number;
  location: string | null;
  description: string | null;
  originType: string;
  status: "Pending" | "InProgress" | "Completed";
  createdAt: string;
  startedAt: string | null;
  finishedAt: string | null;
}

// maintenance-task.model.ts
export interface MaintenanceTask {
  id: number;
  roomId: number;
  location: string;
  originType: string;
  status: "Pending" | "InProgress" | "Completed";
  description: string;
  createdAt: string;
  startedAt: string | null;
  finishedAt: string | null;
}

// create-internal-ticket-request.dto.ts
export interface CreateInternalTicketRequest {
  location: string;
  description: string;
}
```

**Component lifecycle (`ngOnInit` / constructor):**

- Call `loadAnalytics()` without parameters.
- Call `loadPendingCounts()`.
- Call `loadAuditLogs()`.

**Loading methods (exact patterns):**

```ts
loadAnalytics(params?: { startDate?: string; endDate?: string }) {
  this.analyticsLoading.set(true);
  this.analyticsError.set(null);
  this.analyticsApi.getAnalytics(params).pipe(
    finalize(() => this.analyticsLoading.set(false))
  ).subscribe({
    next: (data) => this.analytics.set(data),
    error: (err) => this.analyticsError.set(err.error?.message || 'Failed to load analytics')
  });
}

loadPendingCounts() {
  this.pendingLoading.set(true);
  this.pendingError.set(null);
  forkJoin({
    hk: this.housekeepingApi.getAll({ status: 'Pending', pageNumber: 1, pageSize: 1000 }),
    mt: this.maintenanceApi.getAll({ status: 'Pending', pageNumber: 1, pageSize: 1000 })
  }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.pendingLoading.set(false))
  ).subscribe({
    next: ({ hk, mt }) => {
      this.housekeepingPendingCount.set(hk.length);
      this.maintenancePendingCount.set(mt.length);
    },
    error: (err: Error) => this.pendingError.set(err.message)
  });
}

loadAuditLogs() {
  this.auditLoading.set(true);
  this.auditError.set(null);
  this.auditLogApi.getAll({ sortBy: 'timestamp', sortDescending: true, pageSize: 5 }).pipe(
    finalize(() => this.auditLoading.set(false))
  ).subscribe({
    next: (data) => this.auditEntries.set(data),
    error: (err) => this.auditError.set(err.error?.message || 'Failed to load audit logs')
  });
}
```

**Date filter actions:**

```ts
applyDateFilter() {
  const start = this.startDateCtrl.value;
  const end = this.endDateCtrl.value;
  if (!start || !end) return;
  const startISO = `${start.toISOString().split('T')[0]}T00:00:00Z`;
  const endISO = `${end.toISOString().split('T')[0]}T23:59:59Z`;
  this.loadAnalytics({ startDate: startISO, endDate: endISO });
}

clearDateFilter() {
  this.startDateCtrl.reset();
  this.endDateCtrl.reset();
  this.loadAnalytics();
}
```

**Dialog opening:**

```ts
openCreateTicketDialog() {
  const dialogRef = this.dialog.open(CreateInternalTicketDialogComponent);
  dialogRef.afterClosed().subscribe(result => {
    if (result === true) {
      this.ticketCreatedMessage.set('Ticket created successfully');
      this.loadPendingCounts(); // refresh counts
      setTimeout(() => this.ticketCreatedMessage.set(null), 3000);
    }
  });
}
```

### 8. Charts (ngx-echarts Options) – Already defined in computed signals above.

### 9. UI States Summary

- **KPI cards**: while loading show `—` (already handled in computed).
- **Charts**: spinners during load, error alert on failure.
- **Department health**: spinners during load, error alert.
- **Audit table**: spinner during load, error alert; empty message if no entries.
- **Ticket creation success**: using `MatSnackBar` is optional; in this spec we use a local `ticketCreatedMessage` signal displayed as a temporary banner (implement as a simple div with class `.toast.success` with `position: fixed; top: 16px; right: 16px`). **If you prefer a standard Material component**, swap to `MatSnackBar` and adjust accordingly. I'll specify a simple local banner to avoid extra dependencies, but note that `MatSnackBarModule` is already imported.

### 10. Dialog Component (`CreateInternalTicketDialogComponent`)

- Standalone.
- Imports: `CommonModule`, `ReactiveFormsModule`, `MatDialogModule`, `MatRadioModule`, `MatButtonModule`, `MatFormFieldModule`, `MatInputModule`, `MatIconModule`, `MatProgressSpinnerModule`.
- **Form**:
    `ts
  form = new FormGroup({
    type: new FormControl<'housekeeping' | 'maintenance'>('housekeeping', Validators.required),
    location: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    description: new FormControl('', [Validators.required, Validators.minLength(5)]),
  });
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  `
- **Template**:
    - Dialog title `<h2 mat-dialog-title>Create Internal Ticket</h2>`
    - Close button in top-right corner: `<button mat-icon-button mat-dialog-close><mat-icon>close</mat-icon></button>`
    - Content: radio group, location input, description textarea.
    - Actions: Cancel (`mat-dialog-close`), Submit button (disabled when form invalid or loading).
- **Submit logic**:
    - Mark all touched, return if invalid.
    - `loading.set(true)`, clear error.
    - Based on form `type`, call `housekeepingApi.createInternal(...)` or `maintenanceApi.createInternal(...)`.
    - On success: `dialogRef.close(true)`.
    - On error: set `errorMessage` signal, keep dialog open.
    - `finalize` sets loading false.

### 11. Responsive Behaviour

- Mobile (<768px): Top bar stacked; KPI cards full width; charts and health stacked; table horizontally scrollable.
- Desktop: 3 KPI cards per row; charts and health side-by-side (flex/grid).

### 12. Accessibility

- Charts containers have `aria-label="Revenue chart"` and `aria-label="Expenditure chart"`.
- Table has `aria-label="Recent audit logs"`.
- Dialog uses proper `aria-labelledby` and focus trap.

### 13. File Structure (exact)

```
src/app/features/admin/
  pages/
    dashboard.component.ts          (overwrite placeholder)
    dashboard.component.html
    dashboard.component.scss
  components/
    create-internal-ticket-dialog.component.ts
    create-internal-ticket-dialog.component.html
    create-internal-ticket-dialog.component.scss
  services/
    analytics-api.service.ts
    housekeeping-api.service.ts
    maintenance-api.service.ts
    audit-log-api.service.ts
  models/
    analytics-dashboard.dto.ts
    audit-log-entry.model.ts
    housekeeping-task.model.ts
    maintenance-task.model.ts
    create-internal-ticket-request.dto.ts
```

### 14. Self‑Review Checklist

- [ ] Overwrites placeholder, selector stays `app-admin-dashboard`.
- [ ] On load, fetches analytics (no dates), pending counts (via forkJoin), and audit logs.
- [ ] KPI cards show `—` while loading, real values after.
- [ ] Charts render only when data available.
- [ ] Date picker apply/clear works with ISO strings as defined.
- [ ] Dialog opens, radio toggles, form validates, correct endpoint called, closes with `true` on success.
- [ ] Ticket creation success triggers `ticketCreatedMessage` and refreshes counts.
- [ ] Error states show alerts with retry buttons.
- [ ] Empty audit table shows “No recent activity.”
- [ ] Responsive layout on mobile.
- [ ] All imports exact, no extra modules.

### 15. Implementation Constraints (Non‑negotiable)

- **Do NOT install any packages.** `ngx-echarts` and `echarts` are assumed present.
- **Do NOT change any route paths or route config.**
- **Do NOT create new files outside the specified structure.**
- **Overwrite** the existing `dashboard.component.ts` placeholder; do not rename it.
- **Use standalone components.**
- **Use Angular 18 control flow** (`@if`, `@for`).
- **Use signals** for all component state; no RxJS in templates.
- **Do NOT introduce NgRx, Signal Store, or any external state management.**
- **Use the provided DTOs exactly**; do not invent extra fields.
- **Do NOT modify `AuthService` or any other existing service.**
- **All API services use `HttpClient` and environment `baseUrl`.**
- **Use `forkJoin` for parallel requests.**
- **Every HTTP request must use `pipe(finalize(...))` for cleanup and `catchError` (or error callback) for error handling.**
- **No nested subscriptions.**
- **`RouterModule` may be imported only if actually needed by the template; default is to omit it.**

### 16. Output Requirements

- Generate each file separately.
- Do not truncate code.
- Do not omit imports.
- Do not use placeholders in code.
- Every file must compile.
- Print the file path before every file.

---

