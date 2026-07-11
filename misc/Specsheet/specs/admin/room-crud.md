# Specsheet: Rooms Management Page

## 1. Purpose

- Replace the `PlaceholderRoomManagementComponent` with the full Rooms CRUD page.
- Uses `GenericCrudComponent` for listing, searching, filtering, sorting, pagination, and editing rooms.
- Integrates a **Room Status Grid** on desktop, providing a visual overview of room occupancy.
- Mobile users can toggle between table and grid views.
- The grid syncs with the active room type filter from the table.

## 2. Route & Navigation

- Path: `/operations/admin/management/room` (already lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/management/room-management.component.ts`.

## 3. Authorization

- Inherits `adminGuard` from parent route.

## 4. Component API (RoomManagementComponent)

- **Selector**: `app-room-management` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `GenericCrudComponent`, `MatButtonToggleModule`, `MatIconModule`, `RoomStatusGridComponent`, `CrudConfig`, `ColumnDef`, `FilterDef`, `FormFieldDef` types, `RoomApiService`, `RoomTypeApiService` (for dropdown), `Room`, `RoomStatus` models, `DestroyRef`, `MatSnackBar`.
- **Template**:

```html
<!-- View toggle (mobile only) -->
@if (isMobile()) {
<div class="view-toggle">
  <mat-button-toggle-group
    [formControl]="viewMode"
    aria-label="View mode"
  >
    <mat-button-toggle value="table">
      <mat-icon>table_chart</mat-icon> Table
    </mat-button-toggle>
    <mat-button-toggle value="grid">
      <mat-icon>grid_view</mat-icon> Grid
    </mat-button-toggle>
  </mat-button-toggle-group>
</div>
}

<div
  class="rooms-layout"
  [class.table-only]="isMobile() && viewMode.value === 'table'"
  [class.grid-only]="isMobile() && viewMode.value === 'grid'"
>
  <!-- Desktop or Mobile Table view -->
  <div
    class="table-section"
    [class.hidden]="isMobile() && viewMode.value === 'grid'"
  >
    <app-generic-crud
      [config]="crudConfig"
      (edit)="onEdit($event)"
      (searchChange)="onSearchChange($event)"
      (filterChange)="onFilterChange($event)"
      (sortChange)="onSortChange($event)"
      (pageChange)="onPageChange($event)"
      (save)="onSave($event)"
    ></app-generic-crud>
  </div>

  <!-- Desktop or Mobile Grid view -->
  @defer (on viewport) {
  <div
    class="grid-section"
    [class.hidden]="isMobile() && viewMode.value === 'table'"
  >
    <app-room-status-grid
      [roomTypeId]="roomTypeFilter()"
      (roomClicked)="onGridRoomClicked($event)"
    ></app-room-status-grid>
  </div>
  } @placeholder {
  <div class="grid-placeholder">Loading room status...</div>
  }
</div>
```

## 5. State Management (All Signals)

```ts
// Data for generic CRUD
data = signal<Room[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

// Query params
pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('id');
sortDescending = signal(false);
searchQuery = signal('');
roomTypeFilter = signal<number | null>(null); // null means all
includeRetired = signal(false);
editingEntity = signal<Room | null>(null);

// Mobile
isMobile = toSignal(this.breakpointObserver.observe('(max-width: 768px)').pipe(map(r => r.matches)), {initialValue: false});
viewMode = new FormControl<'table' | 'grid'>('table', {nonNullable: true});

// Session storage key
private readonly STORAGE_KEY = 'roomsState';
```

## 6. Data Flow & API Calls

### Services

- `RoomApiService` (`features/admin/services/room-api.service.ts`)
- `RoomTypeApiService` (already built, reused for room type dropdown)

### Endpoints

| Method        | Endpoint                   | Parameters / Body                                                                        | Response                                                                                                        |
| ------------- | -------------------------- | ---------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| `getAll`      | `GET /api/v1/rooms`        | `pageNumber, pageSize, roomTypeId?, includeRetired, searchQuery, sortBy, sortDescending` | `{ totalCount, data: Room[] }`                                                                                  |
| `create`      | `POST /api/v1/rooms`       | `CreateRoomDTO`                                                                          | `Room` (inferred from sample? Actually POST response sample was not given; assume returns created room with id) |
| `update`      | `PATCH /api/v1/rooms/{id}` | `UpdateRoomDTO`                                                                          | `{ message: string }` (success message)                                                                         |
| `getStatuses` | `GET /api/v1/rooms/status` | `pageNumber=1, pageSize=100, roomTypeId?, sortDescending=false`                          | `{ totalCount, data: RoomStatus[] }`                                                                            |

### DTOs / Models

```ts
// room.model.ts
export interface Room {
  id: number;
  roomNumber: string;
  roomTypeName: string;
  roomTypeId: number;
  basePrice: number;
  maxOccupancy: number;
  isAvailable: boolean; // from response, but we also have isActive from update
  isActive?: boolean; // from create/update, maybe separate
}
// We'll unify: The list response includes isAvailable (derived), but the entity itself has isActive. So the model can have both.

export interface CreateRoomDTO {
  roomNumber: string;
  roomTypeId: number;
  isActive: boolean;
}

export interface UpdateRoomDTO {
  roomNumber?: string;
  roomTypeId?: number;
  isActive?: boolean;
}

// RoomStatus for grid
export interface RoomStatus {
  roomId: number;
  roomNumber: string;
  roomTypeName: string;
  status: "Occupied" | "Available";
  currentBookingId: number | null;
  currentGuestName: string | null;
  nextCheckInDate: string | null;
}
```

### Component Logic

- `ngOnInit()`: restore session state, fetch room types for filter dropdown (once), fetchData().
- `fetchData()`: calls `roomApi.getAll(...)` with all current signals. On success, normalise page if needed.
- `onEdit(entity: Room)`: `editingEntity.set(entity)`.
- `onSave({formValue, isActive})`: if editing, call update with `editingEntity.id`; else create. On success, snackbar, clear editingEntity, refetch.
- `onSearchChange(query)`: update `searchQuery`, reset page, save state, fetch.
- `onFilterChange(filters)`: update `roomTypeFilter` (from `roomTypeId` key), `includeRetired`; reset page; save state; fetch.
- `onSortChange`: update `sortField`, `sortDescending`, reset page, save, fetch.
- `onPageChange`: update `pageIndex`, `pageSize`, save state, fetch.

### Room Status Grid Data

- Grid is loaded lazily. Its internal `RoomStatusGridComponent` will fetch data using `RoomApiService.getStatuses` with the provided `roomTypeId` input. It re-fetches when input changes.

### Syncing Filter

- The `roomTypeFilter` signal is passed as `[roomTypeId]="roomTypeFilter()"` to the grid.

### Highlight & Scroll

- When a room card is clicked, the grid emits `roomClicked` with `roomId`.
- Parent receives it:
  1. If on mobile, set `viewMode.setValue('table')` (auto-switch).
  2. Then use the generic CRUD’s exposed `scrollToRow(roomId)` method? The generic CRUD doesn't have that yet. We'll need to add a method to scroll to a row by entity id. But we don't want to complicate generic.
     Instead, we can use a different approach: pass a `highlightRoomId` signal to the generic CRUD that triggers row highlight? The generic CRUD can accept an optional `highlightId` input and when set, it highlights that row and clears after animation.
     That's cleaner. So we'll add `highlightId = input<number | null>(null)` to `GenericCrudComponent`. When it changes, find the row with that id, scroll into view, apply highlight class, clear after 2s.
     This requires a small patch to the generic component, but it's generic – just an id. We'll include that patch in this specsheet.

- So parent sets a `highlightRoomId = signal<number | null>(null)`.
  Pass it to generic CRUD: `[highlightId]="highlightRoomId()"`.
  On grid click: `this.highlightRoomId.set(roomId)`. Generic component will do the rest.

## 7. Configuration for GenericCrudComponent

```ts
crudConfig: CrudConfig<Room> = {
  entityName: "Room",
  entityNamePlural: "Rooms",
  columns: [
    {
      header: "Room Number",
      field: "roomNumber",
      sortable: false,
      getValue: (r) => r.roomNumber,
    },
    {
      header: "Type",
      field: "roomTypeName",
      sortable: false,
      getValue: (r) => r.roomTypeName,
    },
    {
      header: "Base Price",
      field: "basePrice",
      sortable: true,
      getValue: (r) => `$${r.basePrice}`,
    },
    {
      header: "Max Occ.",
      field: "maxOccupancy",
      sortable: true,
      getValue: (r) => String(r.maxOccupancy),
    },
    {
      header: "Active",
      field: "isActive",
      sortable: false,
      getValue: (r) => (r.isActive ? "Yes" : "No"),
    },
    {
      header: "Available",
      field: "isAvailable",
      sortable: false,
      getValue: (r) => (r.isAvailable ? "Yes" : "No"),
    },
  ],
  filters: [
    {
      key: "roomTypeId",
      label: "Room Type",
      options: [], // fetched dynamically; we'll populate in component
    },
    {
      key: "includeRetired",
      label: "Status",
      options: [
        { value: false, label: "Active Only" },
        { value: true, label: "All" },
      ],
    },
  ],
  formFields: [
    {
      name: "roomNumber",
      label: "Room Number",
      type: "text",
      validators: [Validators.required, Validators.maxLength(100)],
    },
    {
      name: "roomTypeId",
      label: "Room Type",
      type: "select",
      validators: [Validators.required],
      options: [],
    }, // options populated dynamically
    { name: "isActive", label: "Active", type: "toggle", validators: [] },
  ],
  supportsToggle: true, // though we have explicit isActive in form, toggle can be used for status
  data: this.data,
  totalCount: this.totalCount,
  loading: this.loading,
  error: this.error,
  pageIndex: this.pageIndex,
  pageSize: this.pageSize,
};
```

**Dynamic filter/form options**: In `ngOnInit()`, fetch all room types and set `crudConfig.filters[0].options` and `crudConfig.formFields[1].options` with `{ value: rt.id, label: rt.name }`.

**Note**: The generic component must support `type: 'select'` in form fields; we'll add that to `FormFieldDef` (patch to generic). Also `options` property is added. The modal will render a `<mat-select>` for that field.

## 8. RoomStatusGridComponent

### API

- **Selector**: `app-room-status-grid`
- **Standalone**: `true`
- **Inputs**:
  ```ts
  roomTypeId = input<number | null>(null);
  ```
- **Outputs**:
  ```ts
  roomClicked = output<number>(); // emits roomId
  ```
- **Imports**: `CommonModule`, `MatCardModule`, `MatIconModule`, `MatTooltipModule`, `HttpClientModule`? No, service injected.

### Template

```html
<div
  class="status-grid"
  *ngIf="!loading() && !error(); else loadingOrError"
>
  @for (room of rooms(); track room.roomId) {
  <div
    class="room-card"
    [class.occupied]="room.status === 'Occupied'"
    [class.available]="room.status === 'Available'"
    (click)="onCardClick(room.roomId)"
    [matTooltip]="tooltipContent(room)"
    matTooltipPosition="above"
  >
    <span class="room-number">{{ room.roomNumber }}</span>
    <mat-icon>{{ room.status === 'Occupied' ? 'lock' : 'lock_open' }}</mat-icon>
  </div>
  }
</div>

<ng-template #loadingOrError>
  @if (loading()) {
  <mat-spinner diameter="30"></mat-spinner>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
  ></app-alert>
  }
</ng-template>
```

Tooltip content method:

```ts
tooltipContent(room: RoomStatus): string {
  if (room.status === 'Occupied') {
    return `Occupied - ${room.currentGuestName ?? 'Guest'}`;
  }
  return 'Available';
}
```

### State

```ts
rooms = signal<RoomStatus[]>([]);
loading = signal(false);
error = signal<string | null>(null);
private destroyRef = inject(DestroyRef);
private roomApi = inject(RoomApiService);
```

### Lifecycle

- `ngOnInit()`: `fetchStatuses()`.
- On changes to `roomTypeId`, re-fetch (use `effect` or `ngOnChanges` equivalent with signals – we'll use `computed` and `effect` to watch input). We'll have a private method that fetches, and an effect: `effect(() => { this.roomTypeId(); this.fetchStatuses(); });`.
- `fetchStatuses()`: calls `roomApi.getStatuses({ pageNumber:1, pageSize:100, roomTypeId: this.roomTypeId() ?? undefined, sortDescending: false })`, sets rooms.
- `onCardClick(roomId: number)`: emit `roomClicked.emit(roomId)`.

## 9. UI States

- Table loading/error/empty: generic component handles.
- Grid loading: spinner; error: alert; empty: "No room statuses available".
- Highlight animation: row gets class `highlight-row` with a yellow background that transitions out over 2 seconds (CSS `animation: highlight-fade 2s ease-out`).

## 10. Responsive Behaviour

- Desktop: flexbox row, table left (flex: 0 0 70%), grid right (flex: 0 0 30%).
- Mobile: `rooms-layout` stacks vertically. The view toggle button visible only on mobile. The `.hidden` class applied conditionally to the section not active.

## 11. Accessibility

- Grid cards: `aria-label` includes room number and status.
- Lock icon `aria-hidden="true"`.
- Tooltip accessible via `matTooltip`.

## 12. Integration Notes

- **Generic Crud Patch** required:
  1. Add `highlightId` input (optional number).
  2. In the generic component, watch for changes using `effect`. When `highlightId()` changes to a non-null value, find the row index in `data()` that has `.id === highlightId()`. Use Angular Material's `MatTableDataSource`? Actually generic CRUD uses `config().data()` as `[dataSource]`, which likely is an array signal, not `MatTableDataSource`. To scroll to a row, we need a `ViewChildren` of rows. We can implement it with a template reference on each row and a method to scroll to it. Simpler: use the existing paginated list; we'll pass the id and the generic will `querySelector`? That's fragile. Instead, we can set a temporary "selected" id that the row template checks and adds a class. Then we can use `Element.scrollIntoView()` on the element with that class. So we'll implement:
     - Input `highlightId: Signal<number | null>`
     - Effect: when `highlightId()` changes, set a local signal `_highlightedId`. Then after a tick (using `setTimeout`), find the element with `[data-room-id="..."]` and call `scrollIntoView({ behavior: 'smooth', block: 'center' })`. The row template must have `[attr.data-room-id]="row.id"`.
  3. The highlight class fades after 2 seconds by removing the class. We'll manage that in the generic component using a timeout.
     These changes are straightforward and keep the generic component still generic (just an id).
     We'll include this patch spec within this sheet.

- `RoomTypeApiService` is reused from room types.
- Session storage schema:

```json
{
  "roomTypeId": null,
  "includeRetired": false,
  "searchQuery": "",
  "sortField": "id",
  "sortDescending": false,
  "pageIndex": 0,
  "pageSize": 10
}
```

## 13. File Structure

```
src/app/features/admin/
  pages/management/
    room-management.component.ts     (overwrite)
    room-management.component.html
    room-management.component.scss
  components/
    room-status-grid/
      room-status-grid.component.ts
      room-status-grid.component.html
      room-status-grid.component.scss
  services/
    room-api.service.ts
  models/
    room.model.ts
shared/
  components/
    generic-crud/
      generic-crud.component.ts  (patch)
      generic-crud.component.html (patch)
```

## 14. Patch to GenericCrudComponent (included)

- Add input: `highlightId = input<number | null>(null);`
- In template, add `[attr.data-room-id]="row.id"` to each `<tr mat-row>` (and also in `CardsViewComponent` items).
- Effect in component:
  ```ts
  constructor() {
    effect(() => {
      const id = this.highlightId();
      if (id != null) {
        // Wait for render
        setTimeout(() => {
          const el = this.elementRef.nativeElement.querySelector(`[data-room-id="${id}"]`);
          if (el) {
            el.scrollIntoView({ behavior: 'smooth', block: 'center' });
            el.classList.add('highlight-row');
            setTimeout(() => el.classList.remove('highlight-row'), 2000);
          }
        });
      }
    });
  }
  ```
  Add `ElementRef` injection.
- Add CSS: `.highlight-row { animation: highlight-fade 2s ease-out; } @keyframes highlight-fade { 0% { background-color: #fff176; } 100% { background-color: transparent; } }`

## 15. Self‑Review Checklist

- [ ] Rooms table loads with data, pagination, sorting.
- [ ] Search by room number works.
- [ ] Filter by room type syncs grid.
- [ ] Add/Edit modal works with room type dropdown.
- [ ] Room status grid displays on desktop, updates on filter.
- [ ] Clicking grid card highlights row in table (desktop) or auto-switches to table and highlights (mobile).
- [ ] Tooltip on hover shows status and guest name.
- [ ] Highlight animation fades smoothly.
- [ ] Mobile toggle switches views.
- [ ] Lazy loading defers grid.
- [ ] Session storage persists state.
- [ ] No console errors.

## 16. Implementation Constraints (Non‑negotiable)

- Use Angular 18 control flow, standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder files; do not rename.
- Do not install new packages.
- Generic CRUD patch must maintain backward compatibility; the `highlightId` input is optional.
- Grid must use `@defer (on viewport)`.
- Highlight animation must use CSS keyframes, no abrupt changes.# Specsheet: Rooms Management Page

## 1. Purpose

- Replace the `PlaceholderRoomManagementComponent` with the full Rooms CRUD page.
- Uses `GenericCrudComponent` for listing, searching, filtering, sorting, pagination, and editing rooms.
- Integrates a **Room Status Grid** on desktop, providing a visual overview of room occupancy.
- Mobile users can toggle between table and grid views.
- The grid syncs with the active room type filter from the table.

## 2. Route & Navigation

- Path: `/operations/admin/management/room` (already lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/management/room-management.component.ts`.

## 3. Authorization

- Inherits `adminGuard` from parent route.

## 4. Component API (RoomManagementComponent)

- **Selector**: `app-room-management` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `GenericCrudComponent`, `MatButtonToggleModule`, `MatIconModule`, `RoomStatusGridComponent`, `CrudConfig`, `ColumnDef`, `FilterDef`, `FormFieldDef` types, `RoomApiService`, `RoomTypeApiService`, `Room`, `RoomStatus` models, `DestroyRef`, `MatSnackBar`.
- **Template**:

```html
<!-- View toggle (mobile only) -->
@if (isMobile()) {
<div class="view-toggle">
  <mat-button-toggle-group
    [formControl]="viewMode"
    aria-label="View mode"
  >
    <mat-button-toggle value="table">
      <mat-icon>table_chart</mat-icon> Table
    </mat-button-toggle>
    <mat-button-toggle value="grid">
      <mat-icon>grid_view</mat-icon> Grid
    </mat-button-toggle>
  </mat-button-toggle-group>
</div>
}

<div
  class="rooms-layout"
  [class.table-only]="isMobile() && viewMode.value === 'table'"
  [class.grid-only]="isMobile() && viewMode.value === 'grid'"
>
  <!-- Desktop or Mobile Table view -->
  <div
    class="table-section"
    [class.hidden]="isMobile() && viewMode.value === 'grid'"
  >
    <app-generic-crud
      [config]="crudConfig"
      [highlightId]="highlightRoomId()"
      (edit)="onEdit($event)"
      (searchChange)="onSearchChange($event)"
      (filterChange)="onFilterChange($event)"
      (sortChange)="onSortChange($event)"
      (pageChange)="onPageChange($event)"
      (save)="onSave($event)"
    ></app-generic-crud>
  </div>

  <!-- Desktop or Mobile Grid view -->
  @defer (on viewport) {
  <div
    class="grid-section"
    [class.hidden]="isMobile() && viewMode.value === 'table'"
  >
    <app-room-status-grid
      [roomTypeId]="roomTypeFilter()"
      (roomClicked)="onGridRoomClicked($event)"
    ></app-room-status-grid>
  </div>
  } @placeholder {
  <div class="grid-placeholder">
    <mat-spinner diameter="30"></mat-spinner>
    <p>Loading room status...</p>
  </div>
  }
</div>
```

## 5. State Management (All Signals)

```ts
// Data for generic CRUD
data = signal<Room[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

// Query params
pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('id');
sortDescending = signal(false);
searchQuery = signal('');
roomTypeFilter = signal<number | null>(null); // null means all
includeRetired = signal(false);
editingEntity = signal<Room | null>(null);

// Highlight
highlightRoomId = signal<number | null>(null);

// Mobile
isMobile = toSignal(
  this.breakpointObserver.observe('(max-width: 768px)').pipe(map(r => r.matches)),
  { initialValue: false }
);
viewMode = new FormControl<'table' | 'grid'>('table', { nonNullable: true });

// Session storage key
private readonly STORAGE_KEY = 'roomsState';
```

## 6. Data Flow & API Calls

### Services

- `RoomApiService` (`features/admin/services/room-api.service.ts`)
- `RoomTypeApiService` (already built, reused for room type dropdown)

### Endpoints

| Method        | Endpoint                   | Parameters / Body                                                                        | Response                             |
| ------------- | -------------------------- | ---------------------------------------------------------------------------------------- | ------------------------------------ |
| `getAll`      | `GET /api/v1/rooms`        | `pageNumber, pageSize, roomTypeId?, includeRetired, searchQuery, sortBy, sortDescending` | `{ totalCount, data: Room[] }`       |
| `create`      | `POST /api/v1/rooms`       | `CreateRoomDTO`                                                                          | `Room` (created)                     |
| `update`      | `PATCH /api/v1/rooms/{id}` | `UpdateRoomDTO`                                                                          | `{ message: string }`                |
| `getStatuses` | `GET /api/v1/rooms/status` | `pageNumber=1, pageSize=100, roomTypeId?, sortDescending=false`                          | `{ totalCount, data: RoomStatus[] }` |

### DTOs / Models

```ts
// room.model.ts
export interface Room {
  id: number;
  roomNumber: string;
  roomTypeName: string;
  roomTypeId: number;
  basePrice: number;
  maxOccupancy: number;
  isAvailable: boolean;
  isActive: boolean; // may be present in list; keep optional
}

export interface CreateRoomDTO {
  roomNumber: string;
  roomTypeId: number;
  isActive: boolean;
}

export interface UpdateRoomDTO {
  roomNumber?: string;
  roomTypeId?: number;
  isActive?: boolean;
}

export interface RoomStatus {
  roomId: number;
  roomNumber: string;
  roomTypeName: string;
  status: "Occupied" | "Available";
  currentBookingId: number | null;
  currentGuestName: string | null;
  nextCheckInDate: string | null;
}
```

### Component Logic

```ts
constructor() {
  this.fetchData();
  this.restoreState();
  // Load room types for dropdowns
  this.roomTypeApi.getAll({ includeRetired: false, pageNumber: 1, pageSize: 100, sortBy: 'name', sortDescending: false })
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe(res => {
      const options = res.data.map(rt => ({ value: rt.id, label: rt.name }));
      // Update config
      this.crudConfig.filters[0].options = options;
      this.crudConfig.formFields[1].options = options;
    });
}

fetchData(): void {
  this.loading.set(true);
  this.error.set(null);
  this.roomApi.getAll({
    pageNumber: this.pageIndex() + 1,
    pageSize: this.pageSize(),
    roomTypeId: this.roomTypeFilter() ?? undefined,
    includeRetired: this.includeRetired(),
    searchQuery: this.searchQuery() || undefined,
    sortBy: this.sortField(),
    sortDescending: this.sortDescending(),
  }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: res => {
      this.data.set(res.data);
      this.totalCount.set(res.totalCount);
      // Normalize page
      const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
      if (this.pageIndex() > maxPage) {
        this.pageIndex.set(maxPage);
        this.fetchData();
      }
    },
    error: (err: Error) => this.error.set(err.message)
  });
}

// Event handlers (as defined earlier, with save state logic)
onEdit(entity: Room): void { this.editingEntity.set(entity); }
onSave(event: { formValue: any; isActive: boolean }): void {
  const { formValue, isActive } = event;
  if (this.editingEntity()) {
    const dto: UpdateRoomDTO = { ...formValue, isActive };
    this.roomApi.update(this.editingEntity()!.id, dto).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.snackBar.open('Room updated', 'Close', { duration: 3000 });
        this.editingEntity.set(null);
        this.fetchData();
      },
      error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 })
    });
  } else {
    const dto: CreateRoomDTO = formValue;
    this.roomApi.create(dto).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.snackBar.open('Room created', 'Close', { duration: 3000 });
        this.fetchData();
      },
      error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 })
    });
  }
}

onSearchChange(query: string): void {
  this.searchQuery.set(query.trim());
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

onFilterChange(filters: Record<string, any>): void {
  this.roomTypeFilter.set(filters['roomTypeId'] ?? null);
  this.includeRetired.set(filters['includeRetired'] ?? false);
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
  this.sortField.set(event.active || 'id');
  this.sortDescending.set(event.direction === 'desc');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

onPageChange(event: { pageIndex: number; pageSize: number }): void {
  this.pageIndex.set(event.pageIndex);
  this.pageSize.set(event.pageSize);
  this.saveState();
  this.fetchData();
}

// Grid click
onGridRoomClicked(roomId: number): void {
  if (this.isMobile() && this.viewMode.value === 'grid') {
    this.viewMode.setValue('table'); // auto-switch
    // Wait for view to render, then highlight
    setTimeout(() => this.highlightRoomId.set(roomId));
  } else {
    this.highlightRoomId.set(roomId);
  }
}
```

### Session Storage Schema

```json
{
  "roomTypeId": null,
  "includeRetired": false,
  "searchQuery": "",
  "sortField": "id",
  "sortDescending": false,
  "pageIndex": 0,
  "pageSize": 10
}
```

`saveState()` / `restoreState()` implement this schema exactly.

## 7. Configuration for GenericCrudComponent

```ts
crudConfig: CrudConfig<Room> = {
  entityName: "Room",
  entityNamePlural: "Rooms",
  columns: [
    {
      header: "Room #",
      field: "roomNumber",
      sortable: false,
      getValue: (r) => r.roomNumber,
    },
    {
      header: "Type",
      field: "roomTypeName",
      sortable: false,
      getValue: (r) => r.roomTypeName,
    },
    {
      header: "Base Price",
      field: "basePrice",
      sortable: true,
      getValue: (r) => `$${r.basePrice}`,
    },
    {
      header: "Max Occ.",
      field: "maxOccupancy",
      sortable: true,
      getValue: (r) => String(r.maxOccupancy),
    },
    {
      header: "Active",
      field: "isActive",
      sortable: false,
      getValue: (r) => (r.isActive ? "Yes" : "No"),
    },
    {
      header: "Available",
      field: "isAvailable",
      sortable: false,
      getValue: (r) => (r.isAvailable ? "Yes" : "No"),
    },
  ],
  filters: [
    {
      key: "roomTypeId",
      label: "Room Type",
      options: [], // populated dynamically
    },
    {
      key: "includeRetired",
      label: "Status",
      options: [
        { value: false, label: "Active Only" },
        { value: true, label: "All" },
      ],
    },
  ],
  formFields: [
    {
      name: "roomNumber",
      label: "Room Number",
      type: "text",
      validators: [Validators.required, Validators.maxLength(100)],
    },
    {
      name: "roomTypeId",
      label: "Room Type",
      type: "select",
      validators: [Validators.required],
      options: [],
    },
    { name: "isActive", label: "Active", type: "toggle", validators: [] },
  ],
  supportsToggle: true,
  data: this.data,
  totalCount: this.totalCount,
  loading: this.loading,
  error: this.error,
  pageIndex: this.pageIndex,
  pageSize: this.pageSize,
};
```

- Options for the `roomTypeId` filter and form select are loaded in `ngOnInit` and updated directly on the config object.

## 8. RoomStatusGridComponent

### API

- **Selector**: `app-room-status-grid`
- **Standalone**: `true`
- **Inputs**: `roomTypeId = input<number | null>(null);`
- **Outputs**: `roomClicked = output<number>();`
- **Imports**: `CommonModule`, `MatCardModule`, `MatIconModule`, `MatTooltipModule`, `MatProgressSpinnerModule`, `AlertComponent`.

### Template

```html
@if (loading()) {
<mat-spinner diameter="30"></mat-spinner>
} @else if (error()) {
<app-alert
  type="error"
  [message]="error()!"
  (closed)="error.set(null)"
>
  <button
    mat-button
    (click)="fetchStatuses()"
  >
    Retry
  </button>
</app-alert>
} @else {
<div class="status-grid">
  @for (room of rooms(); track room.roomId) {
  <div
    class="room-card"
    [class.occupied]="room.status === 'Occupied'"
    [class.available]="room.status === 'Available'"
    (click)="roomClicked.emit(room.roomId)"
    [matTooltip]="tooltipContent(room)"
    matTooltipPosition="above"
    [attr.aria-label]="room.roomNumber + ' - ' + room.status"
  >
    <span class="room-number">{{ room.roomNumber }}</span>
    <mat-icon>{{ room.status === 'Occupied' ? 'lock' : 'lock_open' }}</mat-icon>
  </div>
  } @empty {
  <p>No room statuses available.</p>
  }
</div>
}
```

### State & Logic

```ts
rooms = signal<RoomStatus[]>([]);
loading = signal(false);
error = signal<string | null>(null);
private roomApi = inject(RoomApiService);
private destroyRef = inject(DestroyRef);

constructor() {
  effect(() => {
    // re-fetch when roomTypeId changes
    this.roomTypeId();
    this.fetchStatuses();
  });
}

fetchStatuses(): void {
  this.loading.set(true);
  this.error.set(null);
  this.roomApi.getStatuses({
    pageNumber: 1,
    pageSize: 100,
    roomTypeId: this.roomTypeId() ?? undefined,
    sortDescending: false,
  }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: res => this.rooms.set(res.data),
    error: (err: Error) => this.error.set(err.message)
  });
}

tooltipContent(room: RoomStatus): string {
  if (room.status === 'Occupied') {
    return `Occupied - ${room.currentGuestName ?? 'Guest'}`;
  }
  return 'Available';
}
```

## 9. UI States

- **Table section**: loading, error, empty, filter‑empty are all managed by `GenericCrudComponent`.
- **Grid section**: loading spinner, error with retry, empty list message.
- **Highlight**: row background smoothly fades from yellow to transparent over 2 seconds.

## 10. Responsive Behaviour

- Desktop: `rooms-layout` uses flexbox; `.table-section` width 70%, `.grid-section` 30%.
- Mobile: view toggle appears. `.hidden` class uses `display: none`. Stacked vertically.
- The grid uses CSS Grid with `auto-fill` and `minmax(80px, 1fr)` for cards.

## 11. Accessibility

- Grid cards have `aria-label`.
- Icons are `aria-hidden="true"`.
- Tooltips are standard Material tooltips.

## 12. Integration Notes

- **GenericCrudComponent Patch** (included):
  1. Add input `highlightId = input<number | null>(null);`
  2. Add `[attr.data-room-id]="row.id"` to each table row and card in `CardsViewComponent`.
  3. Effect in generic:
     ```ts
     effect(() => {
       const id = this.highlightId();
       if (id !== null && id !== undefined) {
         setTimeout(() => {
           const el = this.elementRef.nativeElement.querySelector(
             `[data-room-id="${id}"]`,
           );
           if (el) {
             el.scrollIntoView({ behavior: "smooth", block: "center" });
             el.classList.add("highlight-row");
             setTimeout(() => el.classList.remove("highlight-row"), 2000);
           }
         });
       }
     });
     ```
  4. CSS keyframes:
     ```css
     @keyframes highlight-fade {
       0% {
         background-color: #fff176;
       }
       100% {
         background-color: transparent;
       }
     }
     .highlight-row {
       animation: highlight-fade 2s ease-out;
     }
     ```
- The `highlightRoomId` signal in parent is cleared after 2 seconds by the generic component (it doesn't reset the input, but it will stop highlighting after the class is removed; the input remains set until changed, but that's fine).

## 13. File Structure

```
src/app/features/admin/
  pages/management/
    room-management.component.ts     (overwrite)
    room-management.component.html
    room-management.component.scss
  components/
    room-status-grid/
      room-status-grid.component.ts
      room-status-grid.component.html
      room-status-grid.component.scss
  services/
    room-api.service.ts
  models/
    room.model.ts
shared/
  components/
    generic-crud/
      generic-crud.component.ts  (patch)
      generic-crud.component.html (patch)
```

## 14. Self‑Review Checklist

- [ ] Rooms table loads, paginated, sorted.
- [ ] Search by room number works.
- [ ] Room type filter syncs grid.
- [ ] Add/Edit modal includes room type dropdown.
- [ ] Room status grid appears on desktop, updates on filter change.
- [ ] Clicking grid card highlights table row (desktop) or auto‑switches to table view and highlights (mobile).
- [ ] Tooltip on hover shows status and guest name.
- [ ] Highlight animation fades smoothly.
- [ ] Mobile toggle switches between table and grid.
- [ ] Lazy loading defers grid until viewport.
- [ ] Session storage persists state correctly.
- [ ] No console errors, all subscriptions clean.

## 15. Implementation Constraints

- Use Angular 18 control flow, standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder files; do not rename.
- Do not install new packages.
- Generic CRUD patch must maintain backward compatibility; `highlightId` input is optional.
- Grid must use `@defer (on viewport)`.
- Highlight animation must be CSS‑only, smooth fade.

