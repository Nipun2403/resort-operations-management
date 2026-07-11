# Specsheet: Front Desk Dashboard – Part 2 (Movement Table)

## 1. Purpose
- Replace the placeholder inside the Front Desk Dashboard with the **MovementTableComponent**.
- Provides a search bar, an Arrivals/Departures toggle, and a server‑side paginated, sortable table.
- When no search term is entered, the table uses the `movementStatus` parameter to fetch today’s arrivals or departures.
- When a search term is entered, the toggle is disabled and the table uses `guestQuery` to search all bookings, with the title changing to “Search Results”.
- Clicking a row or the eye icon emits the selected `Booking` to the parent dashboard, which will later open the booking action modal (Part 3+). The movement table itself does **not** contain any modal logic.

## 2. Files to Create / Modify

| File | Action |
|------|--------|
| `src/app/features/front-desk/components/movement-table/movement-table.component.ts` | New component |
| `src/app/features/front-desk/components/movement-table/movement-table.component.html` | New template |
| `src/app/features/front-desk/components/movement-table/movement-table.component.scss` | New styles |
| `src/app/features/front-desk/pages/dashboard.component.ts` | Remove placeholder, integrate `MovementTableComponent`. |
| `src/app/features/front-desk/pages/dashboard.component.html` | Replace placeholder with `<app-movement-table>`. |

## 3. MovementTableComponent API
- **Selector**: `app-movement-table`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatFormFieldModule`, `MatInputModule`, `MatIconModule`, `MatButtonToggleModule`, `MatTableModule`, `MatSortModule`, `MatPaginatorModule`, `MatProgressSpinnerModule`, `MatButtonModule`, `AlertComponent`.
- **Exact import paths** (abbreviated; agent must include full paths).
- **Inputs**:
  ```ts
  refresh = input(0); // Incremented by parent to trigger a re‑fetch after a modal action
  ```
- **Outputs**:
  ```ts
  bookingSelected = output<Booking>();
  ```

## 4. State Management (All Signals)
```ts
// Search
searchControl = new FormControl('', { nonNullable: true });
isSearching = signal(false);

// Toggle
activeFilter = new FormControl<'arrivals' | 'departures'>('arrivals', { nonNullable: true });

// Data
data = signal<Booking[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

// Pagination & sorting
pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('bookedAt');
sortDescending = signal(true);

// Derived table title
tableTitle = computed(() => this.isSearching() ? 'Search Results' : 'Today’s Movement');

// Table columns
displayedColumns = ['guestName', 'status', 'roomNumber', 'actions'];
```

## 5. Template (exact – Angular 18 control flow)
```html
<div class="movement-table">
  <!-- Top Bar: Search on left, Toggle on right -->
  <div class="top-bar">
    <div class="search-box">
      <mat-form-field appearance="outline" class="search-field">
        <mat-label>Search guest name or email</mat-label>
        <input matInput [formControl]="searchControl" />
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>
      @if (isSearching()) {
        <button mat-button (click)="clearSearch()">Clear</button>
      }
    </div>
    <mat-button-toggle-group 
      [formControl]="activeFilter" 
      (change)="onToggleChange()"
      [disabled]="isSearching()"
      class="movement-toggle"
      aria-label="Movement filter">
      <mat-button-toggle value="arrivals">Arrivals</mat-button-toggle>
      <mat-button-toggle value="departures">Departures</mat-button-toggle>
    </mat-button-toggle-group>
  </div>

  <!-- Table Title -->
  <h2 class="table-title">{{ tableTitle() }}</h2>

  <!-- Loading, Error, Empty, Data states -->
  @if (loading() && data().length === 0) {
    <div class="loading-container">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchData()">Retry</button>
    </app-alert>
  } @else if (data().length === 0 && !loading()) {
    <div class="empty-state">
      <p>No records found.</p>
      @if (isSearching()) {
        <p>Try a different search term.</p>
      }
    </div>
  } @else {
    @if (loading()) {
      <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    <table mat-table [dataSource]="data()" matSort matSortDisableClear (matSortChange)="onSortChange($event)" aria-label="Movement table">
      <!-- Guest Name -->
      <ng-container matColumnDef="guestName">
        <th mat-header-cell *matHeaderCellDef>Guest Name</th>
        <td mat-cell *matCellDef="let b">{{ b.guestName }}</td>
      </ng-container>

      <!-- Status -->
      <ng-container matColumnDef="status">
        <th mat-header-cell *matHeaderCellDef>Status</th>
        <td mat-cell *matCellDef="let b">
          @if (isSearching()) {
            {{ b.bookingStatus }}
          } @else {
            {{ activeFilter.value === 'arrivals' ? 'Arrival' : 'Departure' }}
          }
        </td>
      </ng-container>

      <!-- Room Number -->
      <ng-container matColumnDef="roomNumber">
        <th mat-header-cell *matHeaderCellDef>Room</th>
        <td mat-cell *matCellDef="let b">{{ getRoomNumbers(b) || 'Unassigned' }}</td>
      </ng-container>

      <!-- Actions -->
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef>Actions</th>
        <td mat-cell *matCellDef="let b">
          <button mat-icon-button (click)="onRowClick(b)" aria-label="View booking details">
            <mat-icon>visibility</mat-icon>
          </button>
        </td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns" (click)="onRowClick(row)" class="clickable-row"></tr>
    </table>

    <mat-paginator
      [length]="totalCount()"
      [pageIndex]="pageIndex()"
      [pageSize]="pageSize()"
      [pageSizeOptions]="[10, 25, 50]"
      (page)="onPageChange($event)">
    </mat-paginator>
  }
</div>
```

## 6. Component Logic (exact TypeScript)

```ts
import { Component, inject, signal, computed, input, output, OnDestroy, OnInit, effect } from '@angular/core';
import { FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged, finalize, Subscription } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import { BookingApiService } from '../../../services/booking-api.service'; // adjust path
import { Booking } from '../../../models/booking.model'; // adjust path
import { DestroyRef } from '@angular/core';

@Component({
  selector: 'app-movement-table',
  standalone: true,
  imports: [ /* as listed in Section 3 */ ],
  templateUrl: './movement-table.component.html',
  styleUrls: ['./movement-table.component.scss']
})
export class MovementTableComponent {
  // Dependencies
  private bookingApi = inject(BookingApiService);
  private destroyRef = inject(DestroyRef);

  // Inputs & Outputs
  refresh = input(0);
  bookingSelected = output<Booking>();

  // State
  searchControl = new FormControl('', { nonNullable: true });
  isSearching = signal(false);
  activeFilter = new FormControl<'arrivals' | 'departures'>('arrivals', { nonNullable: true });
  data = signal<Booking[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('bookedAt');
  sortDescending = signal(true);

  // Derived
  tableTitle = computed(() => this.isSearching() ? 'Search Results' : 'Today’s Movement');
  displayedColumns = ['guestName', 'status', 'roomNumber', 'actions'];

  // Lifecycle
  constructor() {
    // React to search input changes
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(val => {
      const trimmed = val.trim();
      this.isSearching.set(trimmed.length > 0);
      this.pageIndex.set(0);
      this.fetchData();
    });

    // React to parent refresh signal
    effect(() => {
      this.refresh();
      this.pageIndex.set(0);
      this.fetchData();
    });
  }

  // ── Data Fetching ────────────────────────────────
  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);

    const params: any = {
      pageNumber: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      sortDescending: this.sortDescending(),
    };

    if (this.isSearching()) {
      params.guestQuery = this.searchControl.value.trim();
    } else {
      params.movementStatus = this.activeFilter.value === 'arrivals' ? 'incoming' : 'outgoing';
    }

    this.bookingApi.getAll(params).pipe(
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

  // ── Event Handlers ───────────────────────────────
  onToggleChange(): void {
    this.pageIndex.set(0);
    this.fetchData();
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.isSearching.set(false);
    this.pageIndex.set(0);
    this.fetchData();
  }

  onSortChange(event: Sort): void {
    if (!event.active || !event.direction) return;
    this.sortField.set(event.active);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.fetchData();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.fetchData();
  }

  onRowClick(booking: Booking): void {
    this.bookingSelected.emit(booking);
  }

  // ── Helpers ──────────────────────────────────────
  getRoomNumbers(booking: Booking): string {
    return booking.rooms?.filter(r => r.roomNumber).map(r => r.roomNumber).join(', ') || '';
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
```

**Important notes:**
- The `(keyup)` handler is **not used**. Search is entirely driven by `valueChanges` with debounce.
- There is no unused `Subscription` property; `takeUntilDestroyed` manages cleanup.
- The `effect` watching `refresh` input triggers a re‑fetch; this allows the parent dashboard to refresh the table after a modal action (e.g., check‑in, check‑out) by incrementing a `refreshTable` signal.

## 7. Integration with Dashboard (Parent)

### 7.1 Dashboard Template Update
Replace the placeholder in `dashboard.component.html`:
```html
<app-movement-table 
  [refresh]="refreshTable()" 
  (bookingSelected)="openBookingModal($event)" 
/>
```

### 7.2 Dashboard TypeScript Additions
Add the following to `dashboard.component.ts`:
```ts
refreshTable = signal(0);

private dialog = inject(MatDialog);

openBookingModal(booking: Booking): void {
  // Placeholder – will be replaced by Part 3 modal spec
  // For now, we can just log the booking or open a placeholder modal
  console.log('Selected booking:', booking.id);
}

// After any future action (check‑in, check‑out, etc.) that modifies data,
// increment refreshTable and reload summary:
// this.refreshTable.update(n => n + 1);
// this.loadSummary();
```

This keeps the dashboard functional while waiting for Parts 3‑6.

## 8. BookingApiService Contract
The service must support the `movementStatus` query parameter. If it doesn’t already, add it to the `getAll` method’s params interface:

```ts
getAll(params: {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
  status?: string;
  guestQuery?: string;
  movementStatus?: 'incoming' | 'outgoing'; // NEW
  checkInDate?: string;
  checkOutDate?: string;
}): Observable<{ totalCount: number; data: Booking[] }>
```

The service method should pass all params as HTTP query parameters.

## 9. Responsive Behaviour
- The top bar uses flexbox; on screens ≤599px, the search box and toggle stack vertically.
- The table has horizontal scroll on small screens (`overflow-x: auto` on a wrapper).
- The toggle buttons are touch‑friendly (minimum 48dp).

Add to `movement-table.component.scss`:
```scss
.top-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
}
.search-box {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1 1 300px;
}
.movement-toggle {
  flex-shrink: 0;
}
@media (max-width: 599px) {
  .top-bar {
    flex-direction: column;
    align-items: stretch;
  }
  .search-box {
    flex: 1 1 100%;
  }
}
.table-container {
  overflow-x: auto;
}
.clickable-row {
  cursor: pointer;
  &:hover {
    background-color: rgba(0,0,0,0.04);
  }
}
```

## 10. Self‑Review Checklist (Part 2)
- [ ] MovementTableComponent compiles standalone and renders inside the dashboard.
- [ ] On initial load, the table fetches today’s arrivals using `movementStatus=incoming`.
- [ ] Toggling to “Departures” fetches `movementStatus=outgoing` and updates the table.
- [ ] Typing a search term disables the toggle, sets the title to “Search Results”, and fetches using `guestQuery`.
- [ ] Clearing the search re‑enables the toggle and restores “Today’s Movement”.
- [ ] Clicking a row or the eye icon emits the `Booking` object via `bookingSelected`.
- [ ] Pagination and sorting work via server‑side parameters.
- [ ] Parent dashboard’s `refreshTable` signal triggers a re‑fetch when incremented.
- [ ] Responsive layout works on mobile.
- [ ] No console errors; no unused subscriptions.

## 11. Integration Notes for Future Parts
- **Part 3** will replace the placeholder `openBookingModal` method with the actual `BookingActionModalComponent`.
- **Parts 4 & 5** will add the Room Service and Billing tabs to the modal.
- **Part 6** will finalize the refresh wiring so that after any modal action (check‑in, check‑out, payment), the movement table and summary cards reload automatically.
- The `movementStatus` parameter must be added to `BookingApiService` if not already present; this is the only service change required.
- No other shared components are modified.

---