# Specsheet: Amenities Management Page

## 1. Purpose

- Replace the `PlaceholderAmenitiesManagementComponent` with the full Amenities CRUD page.
- Uses the `GenericCrudComponent` for listing, searching, sorting, pagination, and editing amenities.
- Create adds a new amenity (name, description, price). Edit updates all fields including availability (`isAvailable`) via a slide toggle.
- No separate deactivation confirmation; toggling availability is immediate on save.

## 2. Route & Navigation

- Path: `/operations/admin/management/amenities` (lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/management/amenities-management.component.ts`.

## 3. Authorization

- Inherits `adminGuard` from parent route.

## 4. Component API (AmenitiesManagementComponent)

- **Selector**: `app-amenities-management` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `GenericCrudComponent`, `CrudConfig`, `ColumnDef`, `FilterDef`, `FormFieldDef` types, `AmenityApiService`, `Amenity`, `CreateAmenityDTO`, `UpdateAmenityDTO` models, `DestroyRef`, `MatSnackBar`.
- **Exact import paths** (to be used in the component file):

  ```ts
  import { Component, inject, signal } from "@angular/core";
  import { CommonModule } from "@angular/common";
  import { ReactiveFormsModule, FormControl, Validators } from "@angular/forms";
  import { MatSnackBar } from "@angular/material/snack-bar";
  import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
  import { DestroyRef } from "@angular/core";
  import { GenericCrudComponent } from "../../../../shared/components/generic-crud/generic-crud.component";
  import {
    CrudConfig,
    ColumnDef,
    FilterDef,
    FormFieldDef,
  } from "../../../../shared/models/crud-config.model";
  import { AmenityApiService } from "../../services/amenity-api.service";
  import {
    Amenity,
    CreateAmenityDTO,
    UpdateAmenityDTO,
  } from "../../models/amenity.model";
  ```

- **Template**:
  ```html
  <app-generic-crud
    [config]="crudConfig"
    (edit)="onEdit($event)"
    (searchChange)="onSearchChange($event)"
    (filterChange)="onFilterChange($event)"
    (sortChange)="onSortChange($event)"
    (pageChange)="onPageChange($event)"
    (save)="onSave($event)"
  ></app-generic-crud>
  ```

## 5. State Management (All Signals)

```ts
data = signal<Amenity[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('name');
sortDescending = signal(false);
searchQuery = signal('');
editingEntity = signal<Amenity | null>(null);

private readonly STORAGE_KEY = 'amenitiesState';
```

## 6. Data Flow & API Calls

### Service

- `AmenityApiService` (root‑provided, `features/admin/services/amenity-api.service.ts`)

### Endpoints

| Method   | Endpoint                     | Parameters / Body                                                   | Response                          |
| -------- | ---------------------------- | ------------------------------------------------------------------- | --------------------------------- |
| `getAll` | `GET /api/v1/amenities`      | `pageNumber`, `pageSize`, `searchQuery`, `sortBy`, `sortDescending` | `{ totalCount, data: Amenity[] }` |
| `create` | `POST /api/v1/amenities`     | `CreateAmenityDTO`                                                  | `Amenity`                         |
| `update` | `PUT /api/v1/amenities/{id}` | `id`, `UpdateAmenityDTO`                                            | `{ message: string }`             |

**Backend search contract**: `searchQuery` performs **case‑insensitive partial match** on `name` and `description`.  
**Allowed sort fields**: `'name'`, `'price'`, `'isAvailable'`.  
**No dedicated filter endpoint**; filtering is achieved through search.

### DTOs / Models

```ts
// amenity.model.ts
export interface Amenity {
  id: number;
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
}

export interface CreateAmenityDTO {
  name: string;
  description: string;
  price: number;
}

export interface UpdateAmenityDTO {
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
}
```

### Component Logic (Event Handlers)

All handlers use `takeUntilDestroyed` and error normalization (`err instanceof Error ? err.message : 'Unexpected error'`). The code is fully explicit – no implicit references.

```ts
private destroyRef = inject(DestroyRef);
private snackBar = inject(MatSnackBar);
private amenityApi = inject(AmenityApiService);

ngOnInit(): void {
  this.restoreState();
  this.fetchData();
}

fetchData(): void {
  this.loading.set(true);
  this.error.set(null);
  this.amenityApi.getAll({
    pageNumber: this.pageIndex() + 1,
    pageSize: this.pageSize(),
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
      const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
      if (this.pageIndex() > maxPage) {
        this.pageIndex.set(maxPage);
        this.saveState();
      }
    },
    error: (err: any) => this.error.set(err instanceof Error ? err.message : 'Unexpected error')
  });
}

onEdit(entity: Amenity): void {
  this.editingEntity.set(entity);
}

onSave(event: { formValue: any; isActive: boolean }): void {
  const { formValue, isActive } = event;
  if (this.editingEntity()) {
    // For amenities, isActive maps to isAvailable
    const dto: UpdateAmenityDTO = {
      name: formValue.name,
      description: formValue.description,
      price: formValue.price,
      isAvailable: isActive,
    };
    this.amenityApi.update(this.editingEntity()!.id, dto).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Amenity updated', 'Close', { duration: 3000 });
        this.editingEntity.set(null);
        this.fetchData();
      },
      error: (err: any) => this.snackBar.open(err instanceof Error ? err.message : 'Unexpected error', 'Close', { duration: 5000 })
    });
  } else {
    const dto: CreateAmenityDTO = {
      name: formValue.name,
      description: formValue.description,
      price: formValue.price,
    };
    this.amenityApi.create(dto).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Amenity created', 'Close', { duration: 3000 });
        this.fetchData();
      },
      error: (err: any) => this.snackBar.open(err instanceof Error ? err.message : 'Unexpected error', 'Close', { duration: 5000 })
    });
  }
}

// Search change: update searchQuery, reset page, save state, fetch
onSearchChange(query: string): void {
  this.searchQuery.set(query.trim() || '');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

// Filter change: no filters implemented, but method must exist to satisfy output binding
onFilterChange(filters: Record<string, any>): void {
  // Intentionally empty – the Amenities page has no filter controls
}

// Sort change: update sort field/direction, reset page, save, fetch
onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
  if (!event.active || !event.direction) return;
  this.sortField.set(event.active);
  this.sortDescending.set(event.direction === 'desc');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

// Page change: update page index/size, save state, fetch
onPageChange(event: { pageIndex: number; pageSize: number }): void {
  this.pageIndex.set(event.pageIndex);
  this.pageSize.set(event.pageSize);
  this.saveState();
  this.fetchData();
}
```

## 7. Configuration for GenericCrudComponent

```ts
crudConfig: CrudConfig<Amenity> = {
  entityName: "Amenity",
  entityNamePlural: "Amenities",
  columns: [
    { header: "Name", field: "name", sortable: true, getValue: (r) => r.name },
    {
      header: "Description",
      field: "description",
      sortable: false,
      getValue: (r) => r.description,
    },
    {
      header: "Price",
      field: "price",
      sortable: true,
      getValue: (r) => `$${r.price}`,
    },
    {
      header: "Available",
      field: "isAvailable",
      sortable: true,
      getValue: (r) => (r.isAvailable ? "Yes" : "No"),
    },
  ],
  filters: [], // no filters
  formFields: [
    {
      name: "name",
      label: "Name",
      type: "text",
      validators: [
        Validators.required,
        Validators.maxLength(100),
        Validators.minLength(1),
      ],
      showInAdd: true,
      showInEdit: true,
    },
    {
      name: "description",
      label: "Description",
      type: "textarea",
      validators: [
        Validators.required,
        Validators.maxLength(500),
        Validators.minLength(1),
      ],
      showInAdd: true,
      showInEdit: true,
    },
    {
      name: "price",
      label: "Price",
      type: "number",
      validators: [
        Validators.required,
        Validators.min(0),
        Validators.max(10000),
      ],
      showInAdd: true,
      showInEdit: true,
    },
    {
      name: "isAvailable",
      label: "Available",
      type: "toggle",
      validators: [],
      showInAdd: false, // not shown on creation (defaults to true)
      showInEdit: true,
    },
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

## 8. Session Storage

- **Schema**:
  ```json
  {
    "searchQuery": "",
    "sortField": "name",
    "sortDescending": false,
    "pageIndex": 0,
    "pageSize": 10
  }
  ```
- **Exact validation code**:

  ```ts
  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;
      if (typeof parsed.searchQuery === 'string') this.searchQuery.set(parsed.searchQuery);
      if (['name', 'price', 'isAvailable'].includes(parsed.sortField)) this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean') this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0) this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0) this.pageSize.set(parsed.pageSize);
    } catch { /* fallback silently */ }
  }

  private saveState(): void {
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify({
      searchQuery: this.searchQuery(),
      sortField: this.sortField(),
      sortDescending: this.sortDescending(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
    }));
  }
  ```

## 9. Search Behavior Contract

- The generic component’s search input debounces at **300ms** after the last keystroke.
- On each debounced change, it emits `searchChange` with the trimmed string.
- When the field is cleared, it emits an empty string `''`.
- No submit button; search is automatic.
- The parent uses `onSearchChange` to update `searchQuery` and trigger a fetch.

## 10. UI States

- Loading, error, empty handled by the generic component.
- Add modal: shows name, description, price; no availability toggle.
- Edit modal: shows all fields including a slide toggle for “Available”.
- No confirmation dialog on toggle change – updating availability is instant on save.

## 11. Responsive Behaviour

- Standard: table on desktop, card view on mobile.

## 12. Accessibility

- Form fields properly labelled.
- Toggle accessible via keyboard.

## 13. Integration Notes

- **Overwrite** existing placeholder file.
- `AmenityApiService` and models must be created.
- The generic modal’s validation message mapping and field filtering (`showInAdd`/`showInEdit`) are already in place from previous patches.
- No changes to the generic components are required.

## 14. File Structure

```
src/app/features/admin/
  pages/management/
    amenities-management.component.ts   (overwrite)
    amenities-management.component.html
    amenities-management.component.scss  (optional)
  services/
    amenity-api.service.ts
  models/
    amenity.model.ts
```

## 15. Self‑Review Checklist

- [ ] Amenities table loads with data, pagination, sorting.
- [ ] Search by name/description filters results after 300ms debounce.
- [ ] Sorting by name, price, availability works; direction toggles.
- [ ] Add modal shows name, description, price; no availability toggle.
- [ ] Edit modal shows all fields including availability slide toggle.
- [ ] Toggling availability and saving sends the correct `isAvailable` value.
- [ ] Create request does not include `isAvailable`.
- [ ] Update request includes all fields (PUT).
- [ ] Error messages are specific (required, pattern, min/max).
- [ ] Session storage persists/restores state safely.
- [ ] No duplicate confirmation dialogs.
- [ ] No console errors, subscriptions cleaned.

## 16. Implementation Constraints

- Angular 18 control flow, standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder; do not rename.
- Do not modify shared components; rely on existing `showInAdd`/`showInEdit` and validation message mapping.
- All validation constraints from the DTOs must be applied exactly.
- The `supportsToggle` flag drives the availability toggle in edit mode only.

---

