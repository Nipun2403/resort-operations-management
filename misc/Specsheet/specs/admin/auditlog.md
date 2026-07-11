# Specsheet: Audit Logs Page

## 1. Purpose

- Replace the `PlaceholderAuditLogsComponent` with the full Audit Logs read‑only page.
- Displays a paginated, sortable, searchable table of all audit log entries.
- Clicking a row opens a detail modal showing the full old/new values and metadata.
- No create, edit, or delete actions – purely informational.

## 2. Route & Navigation

- Path: `/operations/admin/oversight/auditlogs` (lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/oversight/audit-logs.component.ts`.

## 3. Authorization

- Inherits `adminGuard` from parent route.

## 4. Component API (AuditLogsComponent)

- **Selector**: `app-audit-logs` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatTableModule`, `MatSortModule`, `MatPaginatorModule`, `MatButtonModule`, `MatIconModule`, `MatFormFieldModule`, `MatInputModule`, `MatProgressSpinnerModule`, `MatDialogModule`, `AlertComponent`.
- **Exact import paths** (use these verbatim):
  ```ts
  import { CommonModule } from "@angular/common";
  import { Component, inject, signal } from "@angular/core";
  import { ReactiveFormsModule, FormControl } from "@angular/forms";
  import { MatTableModule } from "@angular/material/table";
  import { MatSortModule, Sort } from "@angular/material/sort";
  import { MatPaginatorModule, PageEvent } from "@angular/material/paginator";
  import { MatButtonModule } from "@angular/material/button";
  import { MatIconModule } from "@angular/material/icon";
  import { MatFormFieldModule } from "@angular/material/form-field";
  import { MatInputModule } from "@angular/material/input";
  import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
  import { MatDialogModule, MatDialog } from "@angular/material/dialog";
  import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
  import { DestroyRef } from "@angular/core";
  import { debounceTime, distinctUntilChanged, finalize } from "rxjs";
  import { AuditLogApiService } from "../../services/audit-log-api.service";
  import { AuditLogEntry } from "../../models/audit-log-entry.model";
  import { AlertComponent } from "../../../../shared/components/alert/alert.component";
  import { AuditLogDetailDialogComponent } from "./audit-log-detail-dialog.component";
  ```
- **Template** (full – Angular 18 control flow only):

```html
<div class="audit-logs-page">
  <!-- Search & Controls -->
  <div class="controls">
    <mat-form-field
      appearance="outline"
      class="search"
    >
      <mat-label>Search by user, entity, or action</mat-label>
      <input
        matInput
        [formControl]="searchControl"
        (keyup)="onSearchDebounced()"
      />
      <mat-icon matSuffix>search</mat-icon>
    </mat-form-field>
    @if (searchControl.value) {
    <button
      mat-button
      (click)="clearSearch()"
    >
      Clear Search
    </button>
    }
  </div>

  <!-- Loading / Error / Content -->
  @if (loading() && entries().length === 0) {
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
  } @if (entries().length > 0 || loading()) { @if (loading()) {
  <mat-progress-bar mode="indeterminate"></mat-progress-bar>
  }
  <table
    mat-table
    [dataSource]="entries()"
    matSort
    (matSortChange)="onSortChange($event)"
    aria-label="Audit logs"
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
        *matCellDef="let e"
      >
        {{ e.id }}
      </td>
    </ng-container>
    <ng-container matColumnDef="entityName">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Entity
      </th>
      <td
        mat-cell
        *matCellDef="let e"
      >
        {{ e.entityName }}
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
        *matCellDef="let e"
      >
        {{ e.action }}
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
        *matCellDef="let e"
      >
        {{ e.changedByName }}
      </td>
    </ng-container>
    <ng-container matColumnDef="timestamp">
      <th
        mat-header-cell
        *matHeaderCellDef
        mat-sort-header="timestamp"
      >
        Timestamp
      </th>
      <td
        mat-cell
        *matCellDef="let e"
      >
        {{ e.timestamp | date:'medium' }}
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
        *matCellDef="let e"
      >
        <button
          mat-icon-button
          (click)="openDetail(e)"
          aria-label="View audit detail"
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
    <p>No audit log entries found.</p>
    @if (searchControl.value) {
    <p>Try adjusting your search.</p>
    <button
      mat-button
      (click)="clearSearch()"
    >
      Clear Search
    </button>
    }
  </div>
  }
</div>
```

## 5. State Management (All Signals)

**Rule:** Signals are canonical state. `FormControl` instances are UI inputs only.

```ts
// Table columns
displayedColumns = ['id', 'entityName', 'action', 'changedBy', 'timestamp', 'actions'];

// Data (canonical signals)
entries = signal<AuditLogEntry[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

// Query state (canonical signals)
pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('timestamp');
sortDescending = signal(false);

// UI input (form control)
searchControl = new FormControl('', { nonNullable: true });

// Session storage key
private readonly STORAGE_KEY = 'auditLogsState';
```

## 6. Data Flow & API Calls

### Service

- `AuditLogApiService` (root‑provided, `features/admin/services/audit-log-api.service.ts`)

### Endpoint

| Method   | Endpoint                | Parameters                                                         | Response                                |
| -------- | ----------------------- | ------------------------------------------------------------------ | --------------------------------------- |
| `getAll` | `GET /api/v1/auditlogs` | `guestQuery`, `pageNumber`, `pageSize`, `sortBy`, `sortDescending` | `{ totalCount, data: AuditLogEntry[] }` |

**Note:** The backend parameter is named `guestQuery`. The front‑end service must use exactly this name when constructing query params. The UI label may show a user‑friendly name ("Search by user, entity, or action") but the HTTP request must send `guestQuery`.

### DTO / Model (exact)

```ts
// audit-log-entry.model.ts
export interface AuditLogEntry {
  id: number;
  entityName: string;
  action: string;
  recordId: { Id: number };
  oldValues: Record<string, any> | null;
  newValues: Record<string, any> | null;
  changedByEmail: string;
  changedByName: string;
  timestamp: string; // ISO 8601
}
```

### API Error Handling (same as Billing & Receipts)

```ts
private extractErrorMessage(err: any): string {
  if (typeof err === 'string') return err;
  if (err?.error?.message) return err.error.message;
  if (err?.message) return err.message;
  return 'An unexpected error occurred.';
}
```

### Component Methods (exact code)

```ts
private destroyRef = inject(DestroyRef);
private auditLogApi = inject(AuditLogApiService);
private dialog = inject(MatDialog);

ngOnInit(): void {
  this.restoreState();
  this.fetchData();
}

fetchData(): void {
  this.loading.set(true);
  this.error.set(null);
  this.auditLogApi.getAll({
    guestQuery: this.searchControl.value?.trim() || undefined,
    pageNumber: this.pageIndex() + 1,
    pageSize: this.pageSize(),
    sortBy: this.sortField(),
    sortDescending: this.sortDescending(),
  }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: res => {
      this.entries.set(res.data);
      this.totalCount.set(res.totalCount);
      const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
      if (this.pageIndex() > maxPage) {
        this.pageIndex.set(maxPage);
        this.saveState();
      }
    },
    error: (err: any) => this.error.set(this.extractErrorMessage(err))
  });
}

onSearchDebounced(): void {
  // debounce is handled by a dedicated subscription in ngOnInit
}

ngOnInit(): void {
  // ... restore state, fetch data
  this.searchControl.valueChanges.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    takeUntilDestroyed(this.destroyRef)
  ).subscribe(() => {
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  });
}

clearSearch(): void {
  this.searchControl.setValue('', { emitEvent: false });
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

onSortChange(event: Sort): void {
  if (!event.active || !event.direction) return;
  this.sortField.set(event.active);
  this.sortDescending.set(event.direction === 'desc');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

onPageChange(event: PageEvent): void {
  this.pageIndex.set(event.pageIndex);
  this.pageSize.set(event.pageSize);
  this.saveState();
  this.fetchData();
}

openDetail(entry: AuditLogEntry): void {
  this.dialog.open(AuditLogDetailDialogComponent, {
    data: entry,
    maxWidth: '700px',
    width: '90%',
  });
}
```

**Correction on `ngOnInit`:** Only one `ngOnInit` must exist. The search debounce setup and restore/fetch are combined in a single method:

```ts
ngOnInit(): void {
  this.restoreState();
  this.fetchData();
  this.searchControl.valueChanges.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    takeUntilDestroyed(this.destroyRef)
  ).subscribe(() => {
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  });
}
```

## 7. Detail Modal Component

### AuditLogDetailDialogComponent

- **Selector**: `app-audit-log-detail-dialog`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `MatDialogModule`, `MatButtonModule`, `MatIconModule`, `MatCardModule`, `MatDividerModule`, `MatChipsModule`.
- **Input**: receives `AuditLogEntry` via `MAT_DIALOG_DATA`.

**Template** (exact – Angular 18 control flow):

```html
<h2 mat-dialog-title>Audit Entry #{{ data.id }}</h2>
<mat-dialog-content>
  <div class="detail-section">
    <h3>General Information</h3>
    <p><strong>Entity:</strong> {{ data.entityName }}</p>
    <p><strong>Action:</strong> {{ data.action }}</p>
    <p>
      <strong>Changed By:</strong> {{ data.changedByName }} ({{
      data.changedByEmail }})
    </p>
    <p><strong>Timestamp:</strong> {{ data.timestamp | date:'medium' }}</p>
  </div>
  <mat-divider></mat-divider>
  <div class="values-row">
    <div class="values-column">
      <h3>Old Values</h3>
      @if (data.oldValues && getKeys(data.oldValues).length > 0) {
      <div class="value-list">
        @for (key of getKeys(data.oldValues); track key) {
        <div class="value-item">
          <span class="key">{{ key }}:</span>
          <span class="val">{{ formatValue(data.oldValues[key]) }}</span>
        </div>
        }
      </div>
      } @else {
      <p><em>None (created)</em></p>
      }
    </div>
    <div class="values-column">
      <h3>New Values</h3>
      @if (data.newValues && getKeys(data.newValues).length > 0) {
      <div class="value-list">
        @for (key of getKeys(data.newValues); track key) {
        <div class="value-item">
          <span class="key">{{ key }}:</span>
          <span class="val">{{ formatValue(data.newValues[key]) }}</span>
        </div>
        }
      </div>
      } @else {
      <p><em>None</em></p>
      }
    </div>
  </div>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button
    mat-button
    mat-dialog-close
  >
    Close
  </button>
</mat-dialog-actions>
```

**Dialog Component Class (exact):**

```ts
import { Component, Inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { MatDialogModule, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { MatButtonModule } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatDividerModule } from "@angular/material/divider";
import { AuditLogEntry } from "../../models/audit-log-entry.model";

@Component({
  selector: "app-audit-log-detail-dialog",
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
  ],
  templateUrl: "./audit-log-detail-dialog.component.html",
})
export class AuditLogDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: AuditLogEntry) {}

  getKeys(obj: Record<string, any>): string[] {
    return Object.keys(obj);
  }

  formatValue(value: any): string {
    if (value === null || value === undefined) return "null";
    if (typeof value === "boolean") return value ? "Yes" : "No";
    if (typeof value === "object") return JSON.stringify(value);
    return String(value);
  }
}
```

## 8. Session Storage – Deterministic Implementation

**Schema:**

```json
{
  "searchQuery": "",
  "sortField": "timestamp",
  "sortDescending": false,
  "pageIndex": 0,
  "pageSize": 10
}
```

**Exact restore logic:**

```ts
private restoreState(): void {
  try {
    const stored = sessionStorage.getItem(this.STORAGE_KEY);
    if (!stored) return;
    const parsed = JSON.parse(stored);
    if (typeof parsed !== 'object' || parsed === null) return;

    if (typeof parsed.searchQuery === 'string') this.searchControl.setValue(parsed.searchQuery);
    if (typeof parsed.sortField === 'string') this.sortField.set(parsed.sortField);
    if (typeof parsed.sortDescending === 'boolean') this.sortDescending.set(parsed.sortDescending);
    if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0) this.pageIndex.set(parsed.pageIndex);
    if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0) this.pageSize.set(parsed.pageSize);
  } catch { /* fallback silently */ }
}

private saveState(): void {
  sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify({
    searchQuery: this.searchControl.value,
    sortField: this.sortField(),
    sortDescending: this.sortDescending(),
    pageIndex: this.pageIndex(),
    pageSize: this.pageSize(),
  }));
}
```

## 9. UI States

- **Initial load**: full‑page spinner when entries array is empty and loading is true.
- **Refetch (search/sort/page)**: `mat-progress-bar` shown while loading; table remains mounted.
- **Error**: `app-alert` with retry button.
- **Empty (no data at all)**: “No audit log entries found.”
- **Empty (search returned nothing)**: “No audit log entries found. Try adjusting your search.” + clear button.

## 10. Responsive Behaviour

- Table scrolls horizontally on mobile.
- Search field full width on mobile.
- Detail modal width 90% on small screens.

## 11. Accessibility

- Table has `aria-label="Audit logs"`.
- Clickable rows; icon buttons have `aria-label`.
- Detail dialog traps focus.

## 12. Integration Notes

- **Overwrite** placeholder: `src/app/features/admin/pages/oversight/audit-logs.component.ts`.
- `AuditLogApiService` and model must be created.
- The `guestQuery` parameter name must be used exactly in the API service; the UI label can be user‑friendly.
- Detail dialog component is standalone and created alongside the page.
- No modifications to shared components.
- No `*ngIf` / `*ngFor` – only `@if`, `@for`.

## 13. File Structure

```
src/app/features/admin/
  pages/oversight/
    audit-logs.component.ts   (overwrite)
    audit-logs.component.html
    audit-logs.component.scss
    audit-log-detail-dialog.component.ts
    audit-log-detail-dialog.component.html
  services/
    audit-log-api.service.ts
  models/
    audit-log-entry.model.ts
```

## 14. Self‑Review Checklist

- [ ] Audit logs table loads with data, pagination, server‑side sorting.
- [ ] Search debounces at 300ms; clears and resets page on new search.
- [ ] "Clear Search" button visible only when search has text.
- [ ] Clicking row opens detail modal with old/new values side‑by‑side.
- [ ] `oldValues` null shows "None (created)" in modal.
- [ ] `newValues` null shows "None" in modal.
- [ ] Boolean values formatted as "Yes"/"No".
- [ ] Session storage persists and restores search, sort, page state.
- [ ] Loading/error/empty states work correctly.
- [ ] API calls send `guestQuery` parameter exactly (not renamed).
- [ ] No old control flow (`*ngIf`, `*ngFor`).
- [ ] No console errors, subscriptions cleaned.

## 15. Implementation Constraints

- Angular 18 control flow (`@if`, `@for`) ONLY.
- Standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder; do not rename.
- Raw array as `[dataSource]`; `matSort` for sort change events only.
- `guestQuery` must be the query parameter name in HTTP requests.
- `extractErrorMessage` helper used in all API error handling.
- All session storage code copied verbatim.

