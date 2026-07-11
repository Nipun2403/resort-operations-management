# Specsheet: Shared TaskDashboardComponent

## 1. Purpose

- A reusable, configuration‑driven component that displays a paginated, filterable table of tasks (housekeeping, maintenance, or food orders) for the ground‑staff roles.
- Provides summary cards (Pending, In Progress, Completed) that filter the table on click.
- Clicking a row opens a detail modal with full task information and action buttons to transition the task’s status.
- Status transitions are guarded by a confirmation dialog.
- Designed to support a future Kanban view via a `viewMode` input.

## 2. Files to Create

| File                                                                         | Action                                              |
| ---------------------------------------------------------------------------- | --------------------------------------------------- |
| `src/app/shared/components/task-dashboard/task-dashboard.component.ts`       | Main dashboard component                            |
| `src/app/shared/components/task-dashboard/task-dashboard.component.html`     | Template                                            |
| `src/app/shared/components/task-dashboard/task-dashboard.component.scss`     | Styles                                              |
| `src/app/shared/components/task-dashboard/task-detail-dialog.component.ts`   | Detail modal component                              |
| `src/app/shared/components/task-dashboard/task-detail-dialog.component.html` | Modal template                                      |
| `src/app/shared/models/task.model.ts`                                        | Task, DetailSection, TaskDashboardConfig interfaces |

## 3. Interfaces (to be placed in `src/app/shared/models/task.model.ts`)

```ts
export interface Task {
  id: number;
  status: string; // raw status from API (e.g., 'Pending', 'InProgress', 'Completed')
  location: string; // e.g., 'Room 201', 'Lobby', 'N/A'
  description: string; // e.g., 'AC not working', 'Order #123'
  createdAt: string; // ISO date
  raw: any; // original DTO for detail modal
}

export interface DetailSection {
  title: string; // e.g., 'Basic Information'
  fields: { label: string; value: string }[];
}

export interface TaskDashboardConfig<T extends Task = Task> {
  entityName: string; // 'Food Order', 'Housekeeping Task', etc.
  fetchTasks: (params: {
    pageNumber: number;
    pageSize: number;
    status?: string;
    sortBy?: string;
    sortDescending?: boolean;
  }) => Observable<{ totalCount: number; data: T[] }>;

  updateTaskStatus: (id: number, newStatus: string) => Observable<void>;

  statusOptions: { value: string; label: string }[]; // includes 'All' option

  getLocation: (task: T) => string;
  getDescription: (task: T) => string;
  getDetailSections: (task: T) => DetailSection[];

  // future view mode
  // viewMode?: 'table' | 'kanban';
}
```

## 4. Component API (TaskDashboardComponent)

- **Selector**: `app-task-dashboard`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatFormFieldModule`, `MatSelectModule`, `MatTableModule`, `MatSortModule`, `MatPaginatorModule`, `MatProgressSpinnerModule`, `MatDialogModule`, `AlertComponent` (shared), `TaskDetailDialogComponent`.
- **Inputs**:
  ```ts
  config = input.required<TaskDashboardConfig<any>>();
  viewMode = input<"table" | "kanban">("table"); // reserved for future
  ```
- **No outputs** – the component is fully self‑contained.

## 5. Template Structure (exact – Angular 18 control flow)

```html
<div class="task-dashboard">
  <!-- Summary Cards Row -->
  <div class="summary-row">
    @for (card of summaryCards(); track card.status) {
    <mat-card
      class="summary-card"
      [class.active]="statusFilter() === card.status"
      (click)="setStatusFilter(card.status)"
    >
      <mat-card-header>
        <mat-card-title>{{ card.label }}</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <span class="count">{{ card.count }}</span>
      </mat-card-content>
    </mat-card>
    }
  </div>

  <!-- Status Filter Dropdown -->
  <div class="filter-bar">
    <mat-form-field appearance="outline">
      <mat-label>Status</mat-label>
      <mat-select
        [formControl]="statusFilterControl"
        (selectionChange)="onStatusFilterChange($event.value)"
      >
        @for (opt of config().statusOptions; track opt.value) {
        <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
        }
      </mat-select>
    </mat-form-field>
  </div>

  <!-- Loading / Error / Table or future Kanban -->
  @if (loading() && data().length === 0) {
  <mat-spinner diameter="40"></mat-spinner>
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
  } @if (viewMode() === 'table') { @if (data().length > 0 || loading()) { @if
  (loading()) {
  <mat-progress-bar mode="indeterminate"></mat-progress-bar>
  }
  <table
    mat-table
    [dataSource]="data()"
    matSort
    matSortDisableClear
    (matSortChange)="onSortChange($event)"
    aria-label="Tasks"
  >
    <ng-container matColumnDef="id">
      <th
        mat-header-cell
        *matHeaderCellDef
        mat-sort-header="id"
      >
        ID
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ t.id }}
      </td>
    </ng-container>
    <ng-container matColumnDef="location">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Location
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ config().getLocation(t) }}
      </td>
    </ng-container>
    <ng-container matColumnDef="description">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Description
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ config().getDescription(t) }}
      </td>
    </ng-container>
    <ng-container matColumnDef="status">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Status
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        <span
          class="status-chip"
          [class]="t.status"
          >{{ t.status }}</span
        >
      </td>
    </ng-container>
    <ng-container matColumnDef="actions">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Actions
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        <button
          mat-icon-button
          (click)="openDetail(t)"
          aria-label="View details"
        >
          <mat-icon>visibility</mat-icon>
        </button>
      </td>
    </ng-container>
    <tr
      mat-header-row
      *matHeaderRowDef="displayedColumns"
    ></tr>
    <tr
      mat-row
      *matRowDef="let row; columns: displayedColumns"
      (click)="openDetail(row)"
      class="clickable-row"
    ></tr>
  </table>
  <mat-paginator
    [length]="totalCount()"
    [pageIndex]="pageIndex()"
    [pageSize]="pageSize()"
    [pageSizeOptions]="[10, 25, 50]"
    (page)="onPageChange($event)"
  >
  </mat-paginator>
  } @else {
  <div class="empty-state">
    <p>No {{ config().entityName }}s found.</p>
  </div>
  } } @else {
  <!-- Future Kanban placeholder -->
  <p>Kanban view coming soon.</p>
  }
</div>
```

## 6. State Management (All Signals)

```ts
// Data
data = signal<Task[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

// Pagination & sorting
pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('id');
sortDescending = signal(false);

// Status filter
statusFilter = signal('All');          // used for API and summary cards
statusFilterControl = new FormControl('All', { nonNullable: true });

// Summary cards
summaryCards = signal<{ status: string; label: string; count: number }[]>([]);

// Table columns
displayedColumns = ['id', 'location', 'description', 'status', 'actions'];

// Dependencies
private dialog = inject(MatDialog);
private destroyRef = inject(DestroyRef);
```

## 7. Data Flow & Methods

### 7.1 Initialization

In `ngOnInit()`:

- Call `refreshSummaryCounts()`.
- Call `fetchData()`.

### 7.2 `fetchData()`

```ts
private fetchData(): void {
  this.loading.set(true);
  this.error.set(null);
  const params: any = {
    pageNumber: this.pageIndex() + 1,
    pageSize: this.pageSize(),
    sortBy: this.sortField(),
    sortDescending: this.sortDescending(),
  };
  if (this.statusFilter() !== 'All') {
    params.status = this.statusFilter();
  }
  this.config().fetchTasks(params).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: res => {
      this.data.set(res.data);
      this.totalCount.set(res.totalCount);
      const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
      if (this.pageIndex() > maxPage) {
        this.pageIndex.set(maxPage);
      }
    },
    error: (err: any) => this.error.set(this.extractErrorMessage(err))
  });
}
```

### 7.3 `refreshSummaryCounts()`

For each non‑'All' status in `config().statusOptions`, call `fetchTasks` with `pageSize=1`, `status=<that status>`, extract `totalCount`. Use `forkJoin`. Store results in `summaryCards` with labels like "Pending", "In Progress", "Completed". The label mapping can be derived from `statusOptions` (omit 'All'). The component will derive labels: 'Pending' → 'Pending', 'InProgress' → 'In Progress', 'Completed' → 'Completed', etc. We'll use the raw status value and transform for display: e.g., `card.label = status.charAt(0).toUpperCase() + status.slice(1)` but better to use a mapping object from config. We'll add an optional `statusLabelMap` to config? To keep deterministic, we'll map standard statuses: 'Pending' → 'Pending', 'InProgress' → 'In Progress', 'Completed' → 'Completed' (for housekeeping/maintenance); for kitchen, 'Preparing' → 'Preparing', 'Delivered' → 'Delivered'. We'll add a `getStatusLabel(status: string): string` function to config, or simply use the statusOptions labels. The statusOptions array already has label for each value. So we can use that to find the label for the summary cards. So `statusOptions = [{value:'Pending', label:'Pending'}, {value:'InProgress', label:'In Progress'}, {value:'Completed', label:'Completed'}]`. Perfect.

```ts
private refreshSummaryCounts(): void {
  const statuses = this.config().statusOptions.filter(s => s.value !== 'All');
  const requests = statuses.map(s =>
    this.config().fetchTasks({ pageNumber: 1, pageSize: 1, status: s.value }).pipe(
      map(res => ({ status: s.value, label: s.label, count: res.totalCount })),
      catchError(() => of({ status: s.value, label: s.label, count: 0 }))
    )
  );
  forkJoin(requests).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(cards => {
    this.summaryCards.set(cards);
  });
}
```

### 7.4 `setStatusFilter(status: string)`

If the clicked status is already active, reset to 'All'. Else set to that status. Then update `statusFilterControl`, `statusFilter` signal, reset pageIndex, and fetch.

### 7.5 `onStatusFilterChange(value: string)`

Same as setStatusFilter, triggered from dropdown.

### 7.6 `onSortChange(event: Sort)`

Update sortField and sortDescending, reset pageIndex, fetch.

### 7.7 `onPageChange(event: PageEvent)`

Update pageIndex and pageSize, fetch.

### 7.8 `openDetail(task: Task)`

Open `TaskDetailDialogComponent` with data containing task, detail sections (from config), and allowed actions based on status. The dialog's result will be `{ newStatus: string }` or `null`.

```ts
const dialogRef = this.dialog.open(TaskDetailDialogComponent, {
  data: {
    task,
    detailSections: this.config().getDetailSections(task),
    canStart: task.status === "Pending", // adjust to actual pending status
    canComplete: task.status === "InProgress",
  },
  width: "90vw",
  maxWidth: "500px",
});
dialogRef
  .afterClosed()
  .pipe(takeUntilDestroyed(this.destroyRef))
  .subscribe((result: { newStatus: string } | null) => {
    if (result) {
      this.updateStatus(task.id, result.newStatus);
    }
  });
```

### 7.9 `updateStatus(id: number, newStatus: string)`

Show a confirmation dialog (`ConfirmDialogComponent`) with appropriate message. On confirm, call `config().updateTaskStatus(id, newStatus)`. On success, snackbar, refreshData() and refreshSummaryCounts(). On error, snackbar.

## 8. TaskDetailDialogComponent

- **Selector**: `app-task-detail-dialog`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `MatDialogModule`, `MatButtonModule`, `MatIconModule`, `MatDividerModule`, `MatListModule`.
- **Injected Data**: via `MAT_DIALOG_DATA`:
  ```ts
  {
    task: Task;
    detailSections: DetailSection[];
    canStart: boolean;
    canComplete: boolean;
  }
  ```

**Template (exact):**

```html
<h2 mat-dialog-title>
  {{ data.task.type ? data.task.type + ' #' : '' }}{{ data.task.id }}
</h2>
<mat-dialog-content>
  @for (section of data.detailSections; track section.title) {
  <div class="detail-section">
    <h3>{{ section.title }}</h3>
    @for (field of section.fields; track field.label) {
    <p><strong>{{ field.label }}:</strong> {{ field.value }}</p>
    }
  </div>
  <mat-divider *ngIf="!$last"></mat-divider>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  @if (data.canStart) {
  <button
    mat-raised-button
    color="primary"
    (click)="start()"
  >
    Start
  </button>
  } @if (data.canComplete) {
  <button
    mat-raised-button
    color="accent"
    (click)="complete()"
  >
    Complete
  </button>
  }
  <button
    mat-button
    mat-dialog-close
  >
    Close
  </button>
</mat-dialog-actions>
```

**Logic:**

```ts
start() { this.dialogRef.close({ newStatus: 'InProgress' }); }
complete() { this.dialogRef.close({ newStatus: 'Completed' }); }
```

The `newStatus` values are hardcoded; they must match the backend expectations. In the config, the parent can map status transitions. To keep it generic, the dialog will emit the string based on context: `start` emits `'InProgress'` (or `'Preparing'` for kitchen), `complete` emits `'Completed'` (or `'Delivered'`). That's acceptable.

## 9. Responsive Behaviour

- Summary cards: flex wrap, each card `flex: 1 1 200px;`, stack on mobile.
- Table horizontally scrolls on small screens (`overflow-x: auto` on container).
- Detail modal full‑width on mobile (already 90vw).

## 10. Self‑Review Checklist

- [ ] Component compiles standalone and accepts configuration.
- [ ] Summary cards show correct counts and filter the table on click.
- [ ] Status dropdown filters the table and resets page.
- [ ] Table displays columns: ID, Location, Description, Status (chip), Actions.
- [ ] Clicking a row opens detail modal with all information sections.
- [ ] Modal shows “Start” button for Pending tasks, “Complete” for InProgress.
- [ ] Status transitions are confirmed via dialog and update the API.
- [ ] After status change, table and summary counts refresh.
- [ ] Pagination and sorting work via server‑side parameters.
- [ ] Responsive: cards stack, table scrolls.
- [ ] No console errors, subscriptions cleaned.

## 11. Integration Notes

- The component will be used by three role pages: Kitchen, Housekeeping, Maintenance.
- Each page will create a `TaskDashboardConfig` using role‑specific services (already existing `OrderApiService`, `HousekeepingApiService`, `MaintenanceApiService`).
- The `getDetailSections` function will pull relevant fields from the raw DTO to build a rich detail view.
- The status values for summary cards and transitions must be aligned with the backend enum (e.g., `Preparing` for kitchen’s “In Progress”).
- Future Kanban toggle will reuse the same data and config without breaking the table view.

---

