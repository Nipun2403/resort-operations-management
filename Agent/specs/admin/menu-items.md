# Specsheet: Menu Items Management Page
## 1. Purpose
- Replace the `PlaceholderMenuManagementComponent` with the full Menu Items CRUD page.
- Uses the `GenericCrudComponent` for listing, searching, filtering, sorting, pagination, and editing menu items.
- Create adds a new menu item (name, price, category, availability). Update modifies existing items via PUT.
- Availability is shown as a slide toggle in edit mode; no confirmation on toggle change.

## 2. Route & Navigation
- Path: `/operations/admin/management/menu` (lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/management/menu-management.component.ts`.

## 3. Authorization
- Inherits `adminGuard` from parent route.

## 4. Component API (MenuManagementComponent)
- **Selector**: `app-menu-management` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `GenericCrudComponent`, `CrudConfig`, `ColumnDef`, `FilterDef`, `FormFieldDef` types, `MenuItemApiService`, `MenuItem`, `CreateMenuItemDTO`, `UpdateMenuItemDTO` models, `DestroyRef`, `MatSnackBar`.
- **Exact import paths** (to be used in the component file):
  ```ts
  import { Component, inject, signal } from '@angular/core';
  import { CommonModule } from '@angular/common';
  import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
  import { MatSnackBar } from '@angular/material/snack-bar';
  import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
  import { DestroyRef } from '@angular/core';
  import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
  import { CrudConfig, ColumnDef, FilterDef, FormFieldDef } from '../../../../shared/models/crud-config.model';
  import { MenuItemApiService } from '../../services/menu-item-api.service';
  import { MenuItem, CreateMenuItemDTO, UpdateMenuItemDTO } from '../../models/menu-item.model';
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
data = signal<MenuItem[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('name');
sortDescending = signal(false);
searchQuery = signal('');
availabilityFilter = signal<boolean | null>(null); // null = all, true = available, false = unavailable
editingEntity = signal<MenuItem | null>(null);

private readonly STORAGE_KEY = 'menuState';
```

## 6. Data Flow & API Calls

### Service
- `MenuItemApiService` (root‑provided, `features/admin/services/menu-item-api.service.ts`)

### Endpoints
| Method | Endpoint | Parameters / Body | Response |
|--------|----------|-------------------|----------|
| `getAll` | `GET /api/v1/menu-items` | `pageNumber`, `pageSize`, `searchQuery`, `sortBy`, `sortDescending`, `isAvailable` | `{ totalCount, data: MenuItem[] }` |
| `create` | `POST /api/v1/menu-items` | `CreateMenuItemDTO` | `MenuItem` |
| `update` | `PUT /api/v1/menu-items/{id}` | `id`, `UpdateMenuItemDTO` | `MenuItem` |

**Backend search contract**: `searchQuery` performs **case‑insensitive partial match** on `name` and `category`.  
**Allowed sort fields**: `'name'`, `'price'`, `'isAvailable'`.  
**Filter**: `isAvailable` query parameter (true/false) filters by availability; omit for all.

### DTOs / Models
```ts
// menu-item.model.ts
export interface MenuItem {
  id: number;
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
}

export interface CreateMenuItemDTO {
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
}

export interface UpdateMenuItemDTO {
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
}
```

### Component Logic (Event Handlers)
All handlers use `takeUntilDestroyed` and error normalization (`err instanceof Error ? err.message : 'Unexpected error'`).

```ts
private destroyRef = inject(DestroyRef);
private snackBar = inject(MatSnackBar);
private menuItemApi = inject(MenuItemApiService);

ngOnInit(): void {
  this.restoreState();
  this.fetchData();
}

fetchData(): void {
  this.loading.set(true);
  this.error.set(null);
  this.menuItemApi.getAll({
    pageNumber: this.pageIndex() + 1,
    pageSize: this.pageSize(),
    searchQuery: this.searchQuery() || undefined,
    sortBy: this.sortField(),
    sortDescending: this.sortDescending(),
    isAvailable: this.availabilityFilter() ?? undefined,
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

onEdit(entity: MenuItem): void {
  this.editingEntity.set(entity);
}

onSave(event: { formValue: any; isActive: boolean }): void {
  const { formValue, isActive } = event;
  if (this.editingEntity()) {
    const dto: UpdateMenuItemDTO = {
      name: formValue.name,
      price: formValue.price,
      category: formValue.category ?? '',
      isAvailable: isActive,
    };
    this.menuItemApi.update(this.editingEntity()!.id, dto).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Menu item updated', 'Close', { duration: 3000 });
        this.editingEntity.set(null);
        this.fetchData();
      },
      error: (err: any) => this.snackBar.open(err instanceof Error ? err.message : 'Unexpected error', 'Close', { duration: 5000 })
    });
  } else {
    const dto: CreateMenuItemDTO = {
      name: formValue.name,
      price: formValue.price,
      category: formValue.category ?? '',
      isAvailable: true, // new items are always available by default
    };
    this.menuItemApi.create(dto).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Menu item created', 'Close', { duration: 3000 });
        this.fetchData();
      },
      error: (err: any) => this.snackBar.open(err instanceof Error ? err.message : 'Unexpected error', 'Close', { duration: 5000 })
    });
  }
}

onSearchChange(query: string): void {
  this.searchQuery.set(query.trim() || '');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

onFilterChange(filters: Record<string, any>): void {
  const val = filters['isAvailable'];
  this.availabilityFilter.set(val === '' || val === undefined ? null : val);
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
  if (!event.active || !event.direction) return;
  this.sortField.set(event.active);
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
```

## 7. Configuration for GenericCrudComponent

```ts
crudConfig: CrudConfig<MenuItem> = {
  entityName: 'Menu Item',
  entityNamePlural: 'Menu Items',
  columns: [
    { header: 'Name', field: 'name', sortable: true, getValue: r => r.name },
    { header: 'Category', field: 'category', sortable: false, getValue: r => r.category || '—' },
    { header: 'Price', field: 'price', sortable: true, getValue: r => `$${r.price}` },
    { header: 'Available', field: 'isAvailable', sortable: true, getValue: r => r.isAvailable ? 'Yes' : 'No' },
  ],
  filters: [
    {
      key: 'isAvailable',
      label: 'Availability',
      options: [
        { value: null, label: 'All' },
        { value: true, label: 'Available' },
        { value: false, label: 'Unavailable' },
      ],
    },
  ],
  formFields: [
    {
      name: 'name',
      label: 'Name',
      type: 'text',
      validators: [Validators.required, Validators.maxLength(100), Validators.minLength(1), Validators.pattern(/^(?=.*[a-zA-Z])[a-zA-Z0-9\s\-']+$/)],
      showInAdd: true,
      showInEdit: true
    },
    {
      name: 'category',
      label: 'Category',
      type: 'text',
      validators: [Validators.maxLength(100)], // optional, but max length enforced
      showInAdd: true,
      showInEdit: true
    },
    {
      name: 'price',
      label: 'Price',
      type: 'number',
      validators: [Validators.required, Validators.min(0)],
      showInAdd: true,
      showInEdit: true
    },
    {
      name: 'isAvailable',
      label: 'Available',
      type: 'toggle',
      validators: [],
      showInAdd: false,   // not shown on creation (defaults to true)
      showInEdit: true
    }
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
    "pageSize": 10,
    "availabilityFilter": null
  }
  ```
- **Exact validation code** (reused pattern, adapted keys):
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
      if (parsed.availabilityFilter === null || typeof parsed.availabilityFilter === 'boolean') {
        this.availabilityFilter.set(parsed.availabilityFilter);
      }
    } catch { /* fallback silently */ }
  }

  private saveState(): void {
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify({
      searchQuery: this.searchQuery(),
      sortField: this.sortField(),
      sortDescending: this.sortDescending(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
      availabilityFilter: this.availabilityFilter(),
    }));
  }
  ```

## 9. Search Behavior Contract
- Same as all previous CRUD pages: debounce 300ms, emits trimmed string, empty on clear, no submit button.

## 10. UI States
- Add modal: shows name, category, price; no availability toggle.
- Edit modal: shows all fields including availability slide toggle.
- No confirmation dialog on toggle change.

## 11. Responsive Behaviour
- Standard: table on desktop, card view on mobile.

## 12. Accessibility
- Form fields labelled, toggle keyboard accessible.

## 13. Integration Notes
- **Overwrite** existing placeholder file.
- `MenuItemApiService` and models must be created.
- The generic modal's `'toggle'` rendering and validation message mapping are already present from prior patches.
- No modifications to shared components required.

## 14. File Structure
```
src/app/features/admin/
  pages/management/
    menu-management.component.ts   (overwrite)
    menu-management.component.html
    menu-management.component.scss  (optional)
  services/
    menu-item-api.service.ts
  models/
    menu-item.model.ts
```

## 15. Self‑Review Checklist
- [ ] Menu items table loads with data, pagination, sorting.
- [ ] Search by name/category filters results after debounce.
- [ ] Availability filter (All/Available/Unavailable) works.
- [ ] Sorting by name, price, availability toggles correctly.
- [ ] Add modal shows name, category, price; availability toggle absent.
- [ ] Edit modal shows all fields with toggle; changing toggle updates isAvailable.
- [ ] Create uses POST with default isAvailable=true.
- [ ] Update uses PUT with full DTO including isAvailable.
- [ ] Name validation rejects numbers-only strings.
- [ ] Category is optional but max 100 chars.
- [ ] Session storage restores state safely.
- [ ] No duplicate confirmation dialogs.

## 16. Implementation Constraints
- Angular 18 control flow, standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder; do not rename.
- Use provided DTOs and endpoints exactly.
- Do not modify shared components unless needed for toggle rendering (assumed already present).