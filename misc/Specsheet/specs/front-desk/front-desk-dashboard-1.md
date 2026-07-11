# Specsheet: Front Desk Dashboard – Part 1 (Shell, Summary Cards & Active Tickets)

## 1. Purpose

- Replace the `PlaceholderDashboardComponent` with the Front Desk Dashboard.
- Provides the top‑level layout for the dashboard, the summary cards (Today’s Arrivals, Today’s Departures, Active Tickets), and the Active Tickets detail dialog.
- The search bar, “Today’s Movement” table, and booking action modal will be delivered in **Part 2**, but the layout already reserves the middle area for them.

## 2. Route & Navigation

- Path: `/operations/front-desk/dashboard` (lazy‑loaded under Front Desk Shell).
- **Overwrite** the placeholder file: `src/app/features/front-desk/pages/dashboard.component.ts`.

## 3. Authorization

- Already protected by `frontDeskGuard`.

## 4. Component API (FrontDeskDashboardComponent)

- **Selector**: `app-front-desk-dashboard` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatDialogModule`, `MatProgressSpinnerModule`, `AlertComponent`.
- **Exact import paths** (abbreviated; agent must include full paths).

**Template (exact – Angular 18 control flow):**

```html
<div class="dashboard">
  <!-- Summary Cards Row -->
  <div class="summary-row">
    <!-- Today’s Arrivals -->
    <mat-card class="summary-card">
      <mat-card-header>
        <mat-card-title>Today’s Arrivals</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <span class="count">{{ arrivalsCount() }}</span>
      </mat-card-content>
    </mat-card>

    <!-- Today’s Departures -->
    <mat-card class="summary-card">
      <mat-card-header>
        <mat-card-title>Today’s Departures</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <span class="count">{{ departuresCount() }}</span>
      </mat-card-content>
    </mat-card>

    <!-- Active Tickets (clickable) -->
    <mat-card
      class="summary-card clickable"
      (click)="openActiveTickets()"
    >
      <mat-card-header>
        <mat-card-title>Active Tickets</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <div class="ticket-breakdown">
          <span>Housekeeping: {{ activeTickets().housekeeping }}</span>
          <span>Maintenance: {{ activeTickets().maintenance }}</span>
          <span>Food Orders: {{ activeTickets().foodOrders }}</span>
        </div>
      </mat-card-content>
    </mat-card>
  </div>

  <!-- Placeholder for Part 2: Search Bar + Table -->
  <div class="table-area">
    <p>Table and search will be inserted by Part 2 spec.</p>
  </div>

  <!-- Placeholder for future Room Status Grid -->
</div>
```

## 5. State Management (All Signals)

```ts
arrivalsCount = signal(0);
departuresCount = signal(0);
activeTickets = signal<{
  housekeeping: number;
  maintenance: number;
  foodOrders: number;
}>({
  housekeeping: 0,
  maintenance: 0,
  foodOrders: 0,
});

loadingSummary = signal(false);
error = signal<string | null>(null);
```

## 6. Data Flow & API Calls

### 6.1 Services Used (already existing)

- `BookingApiService`
- `HousekeepingApiService`
- `MaintenanceApiService`
- `OrderApiService`

All root‑provided.

### 6.2 Endpoints & Parameters

| Card                | Endpoint                   | Parameters                                                                     |
| ------------------- | -------------------------- | ------------------------------------------------------------------------------ |
| Arrivals count      | `GET /api/v1/bookings`     | `movementStatus=incoming`, `pageSize=1`                                        |
| Departures count    | `GET /api/v1/bookings`     | `movementStatus=outgoing`, `pageSize=1`                                        |
| Active Housekeeping | `GET /api/v1/housekeeping` | `status=Pending`, `pageSize=1` + `status=InProgress`, `pageSize=1` (two calls) |
| Active Maintenance  | `GET /api/v1/maintenance`  | `status=Pending`, `pageSize=1` + `status=InProgress`, `pageSize=1`             |
| Active Food Orders  | `GET /api/v1/orders`       | `status=Pending`, `pageSize=1` + `status=Preparing`, `pageSize=1`              |

**Note:** The `movementStatus` parameter filters bookings that are arriving/departing today without the need for date strings.

### 6.3 Fetch Logic (exact)

```ts
private loadSummary(): void {
  this.loadingSummary.set(true);
  this.error.set(null);

  const arrivals$ = this.bookingApi.getAll({ movementStatus: 'incoming', pageNumber: 1, pageSize: 1 }).pipe(map(r => r.totalCount));
  const departures$ = this.bookingApi.getAll({ movementStatus: 'outgoing', pageNumber: 1, pageSize: 1 }).pipe(map(r => r.totalCount));

  const hkPending$ = this.housekeepingApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(map(r => r.totalCount));
  const hkInProgress$ = this.housekeepingApi.getAll({ status: 'InProgress', pageSize: 1 }).pipe(map(r => r.totalCount));
  const mtPending$ = this.maintenanceApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(map(r => r.totalCount));
  const mtInProgress$ = this.maintenanceApi.getAll({ status: 'InProgress', pageSize: 1 }).pipe(map(r => r.totalCount));
  const foodPending$ = this.orderApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(map(r => r.totalCount));
  const foodPreparing$ = this.orderApi.getAll({ status: 'Preparing', pageSize: 1 }).pipe(map(r => r.totalCount));

  forkJoin({
    arrivals: arrivals$,
    departures: departures$,
    hk: forkJoin([hkPending$, hkInProgress$]).pipe(map(([p, ip]) => p + ip)),
    mt: forkJoin([mtPending$, mtInProgress$]).pipe(map(([p, ip]) => p + ip)),
    food: forkJoin([foodPending$, foodPreparing$]).pipe(map(([p, ip]) => p + ip)),
  }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loadingSummary.set(false))
  ).subscribe({
    next: ({ arrivals, departures, hk, mt, food }) => {
      this.arrivalsCount.set(arrivals);
      this.departuresCount.set(departures);
      this.activeTickets.set({ housekeeping: hk, maintenance: mt, foodOrders: food });
    },
    error: (err: any) => this.error.set(this.extractErrorMessage(err))
  });
}
```

**Important:** Use `pageSize=1` to minimise response size; we only need `totalCount`. Each service method must accept the new `movementStatus` parameter (add to `BookingApiService.getAll` if not present).

## 7. Active Tickets Dialog

### 7.1 Opening the Dialog

```ts
openActiveTickets(): void {
  this.dialog.open(ActiveTicketsDialogComponent, {
    data: {
      housekeepingCount: this.activeTickets().housekeeping,
      maintenanceCount: this.activeTickets().maintenance,
      foodOrdersCount: this.activeTickets().foodOrders,
    },
    width: '90vw',
    maxWidth: '800px',
  });
}
```

### 7.2 ActiveTicketsDialogComponent

**File:** `src/app/features/front-desk/components/active-tickets-dialog/active-tickets-dialog.component.ts`

**Selector:** `app-active-tickets-dialog`  
**Standalone:** `true`  
**Imports:** `MatDialogModule`, `MatTabsModule`, `MatButtonModule`, `MatIconModule`, `TicketListComponent`.

**Template:**

```html
<h2 mat-dialog-title>Active Tickets</h2>
<mat-dialog-content>
  <mat-tab-group>
    <mat-tab label="Housekeeping ({{ data.housekeepingCount }})">
      <app-ticket-list type="housekeeping"></app-ticket-list>
    </mat-tab>
    <mat-tab label="Maintenance ({{ data.maintenanceCount }})">
      <app-ticket-list type="maintenance"></app-ticket-list>
    </mat-tab>
    <mat-tab label="Food Orders ({{ data.foodOrdersCount }})">
      <app-ticket-list type="foodOrder"></app-ticket-list>
    </mat-tab>
  </mat-tab-group>
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

### 7.3 TicketListComponent

**File:** `src/app/features/front-desk/components/ticket-list/ticket-list.component.ts`

**Selector:** `app-ticket-list`  
**Standalone:** `true`  
**Input:** `type = input.required<'housekeeping' | 'maintenance' | 'foodOrder'>()`  
**Imports:** `MatTableModule`, `MatSortModule`, `MatProgressSpinnerModule`, `MatPaginatorModule`, `CommonModule`.

**State:** `tickets = signal<any[]>([])`, `loading`, `error`.

**Fetch logic:**

```ts
ngOnInit(): void {
  this.fetch();
}
private fetch(): void {
  this.loading.set(true);
  let request$: Observable<any[]>;
  switch (this.type()) {
    case 'housekeeping':
      request$ = forkJoin([
        this.hkApi.getAll({ status: 'Pending', pageSize: 200 }).pipe(map(r => r.data)),
        this.hkApi.getAll({ status: 'InProgress', pageSize: 200 }).pipe(map(r => r.data)),
      ]).pipe(map(([p, ip]) => [...p, ...ip]));
      break;
    case 'maintenance':
      request$ = forkJoin([
        this.mtApi.getAll({ status: 'Pending', pageSize: 200 }).pipe(map(r => r.data)),
        this.mtApi.getAll({ status: 'InProgress', pageSize: 200 }).pipe(map(r => r.data)),
      ]).pipe(map(([p, ip]) => [...p, ...ip]));
      break;
    case 'foodOrder':
      request$ = forkJoin([
        this.orderApi.getAll({ status: 'Pending', pageSize: 200 }),
        this.orderApi.getAll({ status: 'Preparing', pageSize: 200 }),
      ]).pipe(map(([p, ip]) => [...p.data, ...ip.data]));
      break;
  }
  request$.pipe(takeUntilDestroyed(this.destroyRef), finalize(() => this.loading.set(false))).subscribe({
    next: data => this.tickets.set(data),
    error: err => this.error.set(this.extractErrorMessage(err))
  });
}
```

**Template:**

```html
@if (loading()) { <mat-spinner diameter="30"></mat-spinner> } @else if (error())
{
<app-alert
  type="error"
  [message]="error()!"
  (closed)="error.set(null)"
  ><button
    mat-button
    (click)="fetch()"
  >
    Retry
  </button></app-alert
>
} @else {
<table
  mat-table
  [dataSource]="tickets()"
  matSort
  matSortDisableClear
>
  <ng-container matColumnDef="id"
    ><th
      mat-header-cell
      *matHeaderCellDef
    >
      ID
    </th>
    <td
      mat-cell
      *matCellDef="let t"
    >
      {{ t.id }}
    </td></ng-container
  >
  <ng-container matColumnDef="room"
    ><th
      mat-header-cell
      *matHeaderCellDef
    >
      Room
    </th>
    <td
      mat-cell
      *matCellDef="let t"
    >
      {{ t.roomNumber ?? t.location ?? '—' }}
    </td></ng-container
  >
  <ng-container matColumnDef="description"
    ><th
      mat-header-cell
      *matHeaderCellDef
    >
      Description
    </th>
    <td
      mat-cell
      *matCellDef="let t"
    >
      {{ t.description ?? 'Order #'+t.id }}
    </td></ng-container
  >
  <ng-container matColumnDef="status"
    ><th
      mat-header-cell
      *matHeaderCellDef
    >
      Status
    </th>
    <td
      mat-cell
      *matCellDef="let t"
    >
      {{ t.status }}
    </td></ng-container
  >
  <ng-container matColumnDef="createdAt"
    ><th
      mat-header-cell
      *matHeaderCellDef
    >
      Created
    </th>
    <td
      mat-cell
      *matCellDef="let t"
    >
      {{ t.createdAt | date:'short' }}
    </td></ng-container
  >
  <tr
    mat-header-row
    *matHeaderRowDef="['id','room','description','status','createdAt']"
  ></tr>
  <tr
    mat-row
    *matRowDef="let row; columns: ['id','room','description','status','createdAt']"
  ></tr>
</table>
}
```

## 8. Responsive Behaviour

- Summary cards wrap; on mobile each card full width.
- Dialog is full‑width on mobile (already 90vw).

## 9. Integration Notes

- **BookingApiService** must accept `movementStatus` as an optional parameter. If it doesn’t, add it; the service simply passes it as a query param.
- **Part 2** will insert the search bar, movement table, and booking action modal into the placeholder area without removing the summary cards.
- The ticket list components are reusable only inside this dashboard; they reside in the front-desk feature folder.

## 10. File Structure (Part 1)

```
src/app/features/front-desk/
  pages/
    dashboard.component.ts
    dashboard.component.html
    dashboard.component.scss
  components/
    active-tickets-dialog/
      active-tickets-dialog.component.ts
      active-tickets-dialog.component.html
    ticket-list/
      ticket-list.component.ts
      ticket-list.component.html
      ticket-list.component.scss
```

## 11. Self‑Review Checklist (Part 1)

- [ ] Dashboard loads and shows correct counts for today’s arrivals and departures using `movementStatus`.
- [ ] Active Tickets card shows correct counts; clicking opens dialog with three tabs.
- [ ] Each tab loads and displays the list of pending/in‑progress tickets.
- [ ] Responsive layout works.
- [ ] No console errors, subscriptions cleaned.

---

