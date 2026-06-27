# Specsheet: Room Types Management Page

### 1. Purpose

- Replace the `PlaceholderRoomTypeManagementComponent` with a fully functional Room Types CRUD page.
- Uses the `GenericCrudComponent` to list, filter, sort, paginate, add, and edit room types.
- All data fetched from the backend; state managed in the page component.

### 2. Route & Navigation

- Existing route: `/operations/admin/management/room-type` (lazy‑loaded child in Admin Shell).
- **Do not change the route.**
- Overwrite the placeholder file `src/app/features/admin/pages/management/room-type-management.component.ts`.

### 3. Authorization

- Already protected by `adminGuard` from parent route.

### 4. Component API (RoomTypeManagementComponent)

- **Selector**: `app-room-type-management` (exact match to placeholder)
- **Standalone**: `true`
- **Imports**:
  - `CommonModule`, `ReactiveFormsModule`
  - `GenericCrudComponent` (from `shared/components/generic-crud/generic-crud.component`)
  - `CrudConfig`, `ColumnDef`, `FilterDef`, `FormFieldDef` types from `shared/models/crud-config.model`
  - `RoomTypeApiService` (new, see file structure)
  - `RoomType`, `CreateRoomTypeDTO`, `UpdateRoomTypeDTO` models (from `./models/room-type.model.ts`)
  - `AuthService` (not used here, but available)
  - `DestroyRef` from `@angular/core`
- **Template**:
  ```html
  <app-generic-crud
    [config]="crudConfig"
    (searchChange)="onSearchChange($event)"
    (filterChange)="onFilterChange($event)"
    (sortChange)="onSortChange($event)"
    (pageChange)="onPageChange($event)"
    (save)="onSave($event)"
  ></app-generic-crud>
  ```

### 5. State Management (All Signals)

```ts
// Data
data = signal<RoomType[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

// Query params
pageIndex = signal(0); // 0-based
pageSize = signal(10);
sortField = signal("name");
sortDescending = signal(false);
includeRetired = signal(false); // filter: false = active only, true = all
searchQuery = signal(""); // not used, but required by config? We'll set to empty and ignore search.
```

### 6. Data Flow & API Calls

**Service**: `RoomTypeApiService` (root‑provided, in `features/admin/services/room-type-api.service.ts`)

- **List**: `GET /api/v1/room-types?includeRetired={includeRetired}&pageNumber={page+1}&pageSize={pageSize}&sortBy={sortField}&sortDescending={sortDescending}`  
  Response: `{ totalCount: number, pageNumber: number, pageSize: number, data: RoomType[] }`  
  Method: `getAll(params: { includeRetired: boolean; pageNumber: number; pageSize: number; sortBy: string; sortDescending: boolean }): Observable<PaginatedResponse<RoomType>>`

- **Create**: `POST /api/v1/room-types`  
  Body: `CreateRoomTypeDTO`  
  Response: `RoomType` (created)

- **Update**: `PATCH /api/v1/room-types/{id}`  
  Body: `UpdateRoomTypeDTO`  
  Response: `RoomType` (updated)

**DTOs** (exact, from swagger/samples, placed in `features/admin/models/room-type.model.ts`):

```ts
export interface RoomType {
  id: number;
  name: string;
  description: string | null;
  basePrice: number;
  maxOccupancy: number;
  imageUrls: string[];
  squareFootage: number | null;
  bedConfiguration: Record<string, number> | null;
  isActive: boolean;
}

export interface CreateRoomTypeDTO {
  name: string;
  description?: string;
  basePrice: number;
  maxOccupancy: number;
  imageUrls?: string[];
  squareFootage?: number;
  bedConfiguration?: Record<string, number>;
}

export interface UpdateRoomTypeDTO {
  name?: string;
  description?: string;
  basePrice?: number;
  maxOccupancy?: number;
  imageUrls?: string[];
  squareFootage?: number;
  bedConfiguration?: Record<string, number>;
  isActive?: boolean;
}

export interface PaginatedResponse<T> {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  data: T[];
}
```

**Component methods (event handlers)**:

```ts
private destroyRef = inject(DestroyRef);
private roomTypeApi = inject(RoomTypeApiService);

// Load data based on current signals
private fetchData(): void {
  this.loading.set(true);
  this.error.set(null);
  this.roomTypeApi.getAll({
    includeRetired: this.includeRetired(),
    pageNumber: this.pageIndex() + 1,
    pageSize: this.pageSize(),
    sortBy: this.sortField(),
    sortDescending: this.sortDescending(),
  }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: res => {
      this.data.set(res.data);
      this.totalCount.set(res.totalCount);
    },
    error: (err: Error) => this.error.set(err.message)
  });
}

// On initialization
ngOnInit(): void {
  // Restore state from session storage if available
  this.restoreState();
  this.fetchData();
}

// Search not supported by backend, so ignore.
onSearchChange(_: string): void {}

onFilterChange(filters: Record<string, any>): void {
  // Expected filter: { includeRetired: boolean }
  if ('includeRetired' in filters) {
    this.includeRetired.set(filters['includeRetired']);
  }
  this.pageIndex.set(0); // reset to first page
  this.saveState();
  this.fetchData();
}

onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
  this.sortField.set(event.active || 'name');
  this.sortDescending.set(event.direction === 'desc');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

onPageChange(event: { pageIndex: number; pageSize: number }): void {
  this.pageIndex.set(event.pageIndex);
  this.pageSize.set(event.pageSize);
  this.saveState(); // but don't reset page here
  this.fetchData();
}

onSave(event: { formValue: any; isActive: boolean }): void {
  const { formValue, isActive } = event;
  // Transform form value to DTO
  const imageUrls = formValue.imageUrl ? [formValue.imageUrl] : [];
  let bedConfig: Record<string, number> | undefined;
  if (formValue.bedType && formValue.bedCount) {
    bedConfig = { [formValue.bedType]: formValue.bedCount };
  }
  if (this.editMode) { // edit mode? Need to track if editing existing. We'll check if selectedEntity exists.
    const id = this.selectedEntity()?.id;
    if (!id) return;
    const dto: UpdateRoomTypeDTO = {
      name: formValue.name,
      description: formValue.description,
      basePrice: formValue.basePrice,
      maxOccupancy: formValue.maxOccupancy,
      imageUrls: imageUrls,
      squareFootage: formValue.squareFootage,
      bedConfiguration: bedConfig,
      isActive: isActive,
    };
    this.roomTypeApi.update(id, dto).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.snackBar.open('Room type updated', 'Close', { duration: 3000 });
        this.fetchData();
      },
      error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 })
    });
  } else {
    const dto: CreateRoomTypeDTO = {
      name: formValue.name,
      description: formValue.description,
      basePrice: formValue.basePrice,
      maxOccupancy: formValue.maxOccupancy,
      imageUrls: imageUrls,
      squareFootage: formValue.squareFootage,
      bedConfiguration: bedConfig,
    };
    this.roomTypeApi.create(dto).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.snackBar.open('Room type created', 'Close', { duration: 3000 });
        this.fetchData();
      },
      error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 })
    });
  }
}
```

We need a way for the generic component to tell us if it's edit mode and the selected entity. The generic CRUD component doesn't directly expose that; it emits `save` with formValue and isActive, but we also need the entity's id for update. So we need to adjust the contract. The `save` output should include the entity id if editing. Or we can track it internally in the page component. The generic component opens modal and on save, it could include the entity id in the result. Let's modify the generic component spec's `CrudModalResult` to include `entityId?: number`. That's easy. I'll add that.

Updated generic component save output:

```ts
save = output<{ formValue: any; isActive: boolean; entityId?: number }>();
```

And in the generic component's modal result handling, we'll pass `entityId` from the selected entity. So in the generic spec, we need to update that. I'll note this change. For now, in the Room Types spec, we'll rely on this new field.

We'll need to track edit mode via a local signal:

```ts
editingId = signal<number | null>(null);
```

Set in `onSave` when editing.

But the generic component doesn't directly tell us which entity is being edited; it opens modal with that data. We could store it when the edit modal is opened. But that happens inside generic. Better: the generic component passes the entity id back in the save event. So we'll update the generic spec to do that. I'll include that change in a patch note.

For now, we'll define the page to expect `entityId` in the save output.

### 7. Configuration for GenericCrudComponent

```ts
crudConfig: CrudConfig<RoomType> = {
  entityName: "Room Type",
  entityNamePlural: "Room Types",
  columns: [
    { header: "Name", field: "name", sortable: true, getValue: (r) => r.name },
    {
      header: "Base Price",
      field: "basePrice",
      sortable: true,
      getValue: (r) => `$${r.basePrice}`,
    },
    {
      header: "Max Occupancy",
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
  ],
  filters: [
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
      name: "name",
      label: "Name",
      type: "text",
      validators: [Validators.required, Validators.maxLength(100)],
    },
    {
      name: "description",
      label: "Description",
      type: "textarea",
      validators: [Validators.maxLength(500)],
    },
    {
      name: "basePrice",
      label: "Base Price",
      type: "number",
      validators: [Validators.required, Validators.min(0)],
    },
    {
      name: "maxOccupancy",
      label: "Max Occupancy",
      type: "number",
      validators: [Validators.required, Validators.min(1)],
    },
    { name: "imageUrl", label: "Image URL", type: "text", validators: [] }, // optional, single URL
    {
      name: "squareFootage",
      label: "Square Footage",
      type: "number",
      validators: [],
    },
    { name: "bedType", label: "Bed Type", type: "text", validators: [] }, // optional
    { name: "bedCount", label: "Bed Count", type: "number", validators: [] },
  ],
  supportsToggle: true, // allows isActive toggle in modal
  data: this.data,
  totalCount: this.totalCount,
  loading: this.loading,
  error: this.error,
  pageIndex: this.pageIndex,
  pageSize: this.pageSize,
};
```

Note: The generic component's `formFields` must support `textarea` type. We'll add that to the `FormFieldDef` type.

### 8. Session State Persistence

- On `filterChange`, `sortChange`, `pageChange`, we call `saveState()` which writes the current `includeRetired`, `sortField`, `sortDescending`, `pageIndex`, `pageSize` to `sessionStorage` keyed by `roomTypesState`.
- On init, `restoreState()` reads and applies those values (if present) before fetching.

### 9. UI States

- Loading: handled by generic component.
- Empty: “No room types found. Add your first room type.” with button.
- Filter empty: “No room types match. Try adjusting filters.” with clear button.
- Error: inline alert.

### 10. Integration Notes

- **Overwrite** existing placeholder file: `src/app/features/admin/pages/management/room-type-management.component.ts`.
- The `GenericCrudComponent` must be updated to include `entityId` in the `save` output (see patch to generic spec).
- `RoomTypeApiService` must be created.
- Models file: `src/app/features/admin/models/room-type.model.ts`.

### 11. File Structure (Created/Modified)

```
src/app/features/admin/
  pages/management/
    room-type-management.component.ts   (overwrite)
    room-type-management.component.html
    room-type-management.component.scss   (optional)
  services/
    room-type-api.service.ts
  models/
    room-type.model.ts
```

### 12. Patch to GenericCrudComponent Spec

- In `CrudModalResult`, add `entityId?: number`.
- In `CrudModalComponent`, when emitting result, include `entityId` from input data (if editing). The modal already receives `entity`; we can pass its `id`.
- Update the generic component's save output type accordingly.

### 13. Implementation Constraints

- Same as before; no new packages; use signals, standalone, Angular 18 control flow.
- Use `takeUntilDestroyed` for all subscriptions.
- `SessionStorage` for state.
- On any filter/sort change, `pageIndex` reset to 0.

### 14. Self‑Review Checklist

- [ ] Page displays room types from API.
- [ ] Filtering by status (Active/All) works, resets page.
- [ ] Sorting by name, price, occupancy works.
- [ ] Pagination works correctly.
- [ ] Add modal opens with empty form; creates a new room type.
- [ ] Edit modal opens pre-filled; update works.
- [ ] Disable toggle triggers confirmation; disabling works.
- [ ] Form validations enforce required fields, min values.
- [ ] Image URL and bed configuration correctly transformed to DTO.
- [ ] State persists in session storage and restores on reload.

---

