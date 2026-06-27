# Specsheet: Staff Management Page

## 1. Purpose

- Replace the `PlaceholderStaffManagementComponent` with the full Staff CRUD page.
- Uses the `GenericCrudComponent` for listing, searching, filtering, sorting, pagination, and editing staff members.
- Staff creation uses a registration DTO with password; editing uses a separate DTO without password.
- The form in the modal adapts dynamically: in add mode it shows all fields including password; in edit mode it hides email and password.
- Toggling a staff member’s active status triggers a confirmation dialog before deactivation.

## 2. Route & Navigation

- Path: `/operations/admin/management/staff` (lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/management/staff-management.component.ts`.

## 3. Authorization

- Inherits `adminGuard` from parent route.

## 4. Component API (StaffManagementComponent)

- **Selector**: `app-staff-management` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `GenericCrudComponent`, `CrudConfig`, `ColumnDef`, `FilterDef`, `FormFieldDef` types, `StaffApiService`, `Staff`, `CreateStaffDTO`, `UpdateStaffDTO` models, `DestroyRef`, `MatSnackBar`, `MatDialog`, `ConfirmDialogComponent`.
- **Import paths** (exact):
  ```ts
  import { GenericCrudComponent } from "../../../../shared/components/generic-crud/generic-crud.component";
  import {
    CrudConfig,
    ColumnDef,
    FilterDef,
    FormFieldDef,
  } from "../../../../shared/models/crud-config.model";
  import { StaffApiService } from "../../services/staff-api.service";
  import {
    Staff,
    CreateStaffDTO,
    UpdateStaffDTO,
  } from "../../models/staff.model";
  import { ConfirmDialogComponent } from "../../../../shared/components/confirm-dialog/confirm-dialog.component";
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
  **No `[searchQuery]` input binding** – the generic component owns its internal search state.

## 5. State Management (All Signals)

```ts
data = signal<Staff[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

// Query params
pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('isActive');
sortDescending = signal(false);
searchQuery = signal('');          // parent's own search state, updated via searchChange
includeFired = signal(false);      // false = active only, true = all
editingEntity = signal<Staff | null>(null);

private readonly STORAGE_KEY = 'staffState';
```

## 6. Data Flow & API Calls

### Service

- `StaffApiService` (root‑provided, `features/admin/services/staff-api.service.ts`)

### Endpoints

| Method   | Endpoint                   | Parameters / Body                                                                   | Response                        |
| -------- | -------------------------- | ----------------------------------------------------------------------------------- | ------------------------------- |
| `getAll` | `GET /api/v1/staff`        | `includeFired`, `pageNumber`, `pageSize`, `sortBy`, `sortDescending`, `searchQuery` | `{ totalCount, data: Staff[] }` |
| `create` | `POST /api/v1/staff`       | `CreateStaffDTO`                                                                    | `Staff` (created)               |
| `update` | `PATCH /api/v1/staff/{id}` | `id`, `UpdateStaffDTO`                                                              | `void` (success)                |

**Backend search contract**: `searchQuery` performs **case‑insensitive partial match** on `firstName`, `lastName`, and `email` fields.

### DTOs / Models

```ts
// staff.model.ts
export type StaffRole =
  | "Admin"
  | "FrontDesk"
  | "Kitchen"
  | "Housekeeping"
  | "Maintenance";

export interface Staff {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: StaffRole;
  isActive: boolean;
  createdAt: string;
}

export interface CreateStaffDTO {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: StaffRole;
}

export interface UpdateStaffDTO {
  firstName?: string;
  lastName?: string;
  role?: StaffRole;
  isActive?: boolean;
}
```

### Component Logic (Event Handlers)

```ts
ngOnInit(): void {
  this.restoreState();
  this.fetchData();
}

fetchData(): void {
  this.loading.set(true);
  this.error.set(null);
  this.staffApi.getAll({
    includeFired: this.includeFired(),
    pageNumber: this.pageIndex() + 1,
    pageSize: this.pageSize(),
    sortBy: this.sortField(),
    sortDescending: this.sortDescending(),
    searchQuery: this.searchQuery() || undefined,
  }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: res => {
      this.data.set(res.data);
      this.totalCount.set(res.totalCount);
      // Page normalization – only after successful data update
      const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
      if (this.pageIndex() > maxPage) {
        this.pageIndex.set(maxPage);
        this.saveState();
      }
    },
    error: (err: Error) => this.error.set(err.message)
  });
}

onEdit(entity: Staff): void {
  this.editingEntity.set(entity);
}

onSave(event: { formValue: any; isActive: boolean }): void {
  const { formValue, isActive } = event;
  if (this.editingEntity()) {
    // Deactivation confirmation: compare original state vs submitted isActive
    if (this.editingEntity()!.isActive && !isActive) {
      this.showDisableConfirmation(formValue, isActive);
      return;
    }
    this.performUpdate(formValue, isActive);
  } else {
    this.performCreate(formValue);
  }
}

private showDisableConfirmation(formValue: any, isActive: boolean): void {
  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    data: {
      title: 'Deactivate Staff Member',
      message: `Are you sure you want to deactivate ${this.editingEntity()!.firstName} ${this.editingEntity()!.lastName}?`,
    },
  });
  dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (confirmed) {
      this.performUpdate(formValue, isActive);
    }
  });
}

private performUpdate(formValue: any, isActive: boolean): void {
  const dto: UpdateStaffDTO = {
    firstName: formValue.firstName,
    lastName: formValue.lastName,
    role: formValue.role as StaffRole,
    isActive: isActive,
  };
  this.staffApi.update(this.editingEntity()!.id, dto).pipe(
    takeUntilDestroyed(this.destroyRef)
  ).subscribe({
    next: () => {
      this.snackBar.open('Staff updated', 'Close', { duration: 3000 });
      this.editingEntity.set(null);
      this.fetchData();
    },
    error: (err: any) => {
      const message = err instanceof Error ? err.message : 'Unexpected error';
      this.snackBar.open(message, 'Close', { duration: 5000 });
    }
  });
}

private performCreate(formValue: any): void {
  const dto: CreateStaffDTO = {
    email: formValue.email,
    password: formValue.password,
    firstName: formValue.firstName,
    lastName: formValue.lastName,
    role: formValue.role as StaffRole,
  };
  this.staffApi.create(dto).pipe(
    takeUntilDestroyed(this.destroyRef)
  ).subscribe({
    next: () => {
      this.snackBar.open('Staff created', 'Close', { duration: 3000 });
      this.fetchData();
    },
    error: (err: any) => {
      const message = err instanceof Error ? err.message : 'Unexpected error';
      this.snackBar.open(message, 'Close', { duration: 5000 });
    }
  });
}

onSearchChange(query: string): void {
  this.searchQuery.set(query.trim() || '');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}

onFilterChange(filters: Record<string, any>): void {
  if ('includeFired' in filters) {
    this.includeFired.set(filters['includeFired'] ?? false);
  }
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

### Updated FormFieldDef interface

```ts
export interface FormFieldDef {
  name: string;
  label: string;
  type:
    | "text"
    | "number"
    | "email"
    | "password"
    | "textarea"
    | "date"
    | "url"
    | "select"
    | "toggle";
  validators: ValidatorFn[];
  options?: { value: any; label: string }[];
  showInAdd?: boolean; // defaults to true if omitted
  showInEdit?: boolean; // defaults to true if omitted
}
```

### Staff crudConfig

```ts
crudConfig: CrudConfig<Staff> = {
  entityName: "Staff",
  entityNamePlural: "Staff",
  columns: [
    {
      header: "First Name",
      field: "firstName",
      sortable: true,
      getValue: (r) => r.firstName,
    },
    {
      header: "Last Name",
      field: "lastName",
      sortable: true,
      getValue: (r) => r.lastName,
    },
    {
      header: "Email",
      field: "email",
      sortable: true,
      getValue: (r) => r.email,
    },
    { header: "Role", field: "role", sortable: true, getValue: (r) => r.role },
    {
      header: "Active",
      field: "isActive",
      sortable: true,
      getValue: (r) => (r.isActive ? "Yes" : "No"),
    },
  ],
  filters: [
    {
      key: "includeFired",
      label: "Status",
      options: [
        { value: false, label: "Active Only" },
        { value: true, label: "All" },
      ],
    },
  ],
  formFields: [
    {
      name: "email",
      label: "Email",
      type: "email",
      validators: [
        Validators.required,
        Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/),
      ],
      showInAdd: true,
      showInEdit: false,
    },
    {
      name: "password",
      label: "Password",
      type: "password",
      validators: [
        Validators.required,
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/),
      ],
      showInAdd: true,
      showInEdit: false,
    },
    {
      name: "firstName",
      label: "First Name",
      type: "text",
      validators: [
        Validators.required,
        Validators.pattern(/^[a-zA-ZÀ-ž\s\-']{2,50}$/),
      ],
      showInAdd: true,
      showInEdit: true,
    },
    {
      name: "lastName",
      label: "Last Name",
      type: "text",
      validators: [
        Validators.required,
        Validators.pattern(/^[a-zA-ZÀ-ž\s\-']{2,50}$/),
      ],
      showInAdd: true,
      showInEdit: true,
    },
    {
      name: "role",
      label: "Role",
      type: "select",
      options: [
        { value: "Admin", label: "Admin" },
        { value: "FrontDesk", label: "Front Desk" },
        { value: "Kitchen", label: "Kitchen" },
        { value: "Housekeeping", label: "Housekeeping" },
        { value: "Maintenance", label: "Maintenance" },
      ],
      validators: [Validators.required],
      showInAdd: true,
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

## 8. Hard Rules for Generic Modal Field Rendering

**Target file**: `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts`

**MANDATORY field filtering logic implemented inside the modal component:**

- The modal receives `editMode` (boolean) from `GenericCrudComponent`:
  - `openAddModal()` → `editMode = false`
  - `openEditModal(row)` → `editMode = true`
- When building the form, the modal must compute the list of active fields using the following exact code:
  ```ts
  const activeFields = data.formFields.filter((f) => {
    if (data.editMode) {
      return f.showInEdit !== false;
    } else {
      return f.showInAdd !== false;
    }
  });
  ```
- If `showInAdd` or `showInEdit` is absent, treat it as `true`.

**No other logic determines field visibility.**

## 9. Search Behavior Contract (Enforced in GenericCrudComponent)

- The search input is a text field that emits `searchChange` events to the parent.
- **Debounce**: 300ms after the last keystroke.
- **Emit on every change**: after debounce, emits the trimmed input string.
- **Clearing the field**: emits an empty string `''`.
- **No submit button**; search triggers automatically.
- The parent must listen to `searchChange` and perform the API call with the new query.
- The generic component does **not** call any API itself.

## 10. Toggle Behavior (isActive) in Modal

- The modal renders a `mat-slide-toggle` only when `supportsToggle` is `true` and the form field list includes a field of type `'toggle'`? Actually we rely on the `supportsToggle` flag.
- The toggle binds to a form control named `isActive` (always present when `supportsToggle` is true).
- In **add mode**, the toggle is **not rendered**.
- In **edit mode**, the toggle reflects the current `isActive` value from the entity and can be changed.
- The toggle value is included in the `save` event's `isActive` property.

## 11. Page Normalization Rule

- Normalization occurs **only inside the `next` callback of a successful API response**.
- If `pageIndex > maxPage`, the signal is updated and **no further immediate fetch is triggered**. The UI will show the updated page index, and the paginator will reflect the change. If data is needed, the paginator’s `pageChange` will eventually fire.
- No `setTimeout` or additional requests are made from normalization logic.

## 12. Error Handling Contract

- All API error objects are normalized:
  ```ts
  catchError((err) => {
    const message = err instanceof Error ? err.message : "Unexpected error";
    return throwError(() => new Error(message));
  });
  ```
- In the component, the error signal is set with `err.message`.
- The snackbar displays the same normalized message.

## 13. Session Storage

- **Schema**:
  ```json
  {
    "includeFired": false,
    "searchQuery": "",
    "sortField": "isActive",
    "sortDescending": false,
    "pageIndex": 0,
    "pageSize": 10
  }
  ```
- **Exact validation code** (to be placed in `restoreState()` method):

  ```ts
  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;
      // Validate types
      if (typeof parsed.includeFired === 'boolean') this.includeFired.set(parsed.includeFired);
      if (typeof parsed.searchQuery === 'string') this.searchQuery.set(parsed.searchQuery);
      if (['firstName', 'lastName', 'email', 'role', 'isActive'].includes(parsed.sortField)) this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean') this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0) this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0) this.pageSize.set(parsed.pageSize);
    } catch {
      // fallback silently to defaults
    }
  }

  private saveState(): void {
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify({
      includeFired: this.includeFired(),
      searchQuery: this.searchQuery(),
      sortField: this.sortField(),
      sortDescending: this.sortDescending(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
    }));
  }
  ```

## 14. UI States

- Table loading/error/empty handled by generic component.
- Add modal: shows email, password, name, role fields; no active toggle.
- Edit modal: shows name, role, and the Active slide toggle.
- Deactivation flow: confirmation dialog appears only when the original entity is active and the user sets the toggle to inactive.

## 15. Responsive Behaviour

- Same as other CRUD pages: table on desktop, card view on mobile.

## 16. Accessibility

- All form fields have proper labels and error associations.
- Toggle button accessible.
- Modal focus trapped.

## 17. Integration Notes

- **Overwrite** existing placeholder file.
- `StaffApiService` and models must be created.
- The `FormFieldDef` interface must be updated in `src/app/shared/models/crud-config.model.ts` to include `showInAdd` and `showInEdit` properties.
- The file `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts` is the **sole location** where the field filtering logic (Section 8) must be implemented.
- The `ConfirmDialogComponent` import must point exactly to `src/app/shared/components/confirm-dialog/confirm-dialog.component`.
- No other changes to `GenericCrudComponent` are needed.

## 18. File Structure

```
src/app/features/admin/
  pages/management/
    staff-management.component.ts   (overwrite)
    staff-management.component.html
    staff-management.component.scss  (optional)
  services/
    staff-api.service.ts
  models/
    staff.model.ts
```

## 19. Self‑Review Checklist

- [ ] Staff table loads with data, pagination, sorting.
- [ ] Search by name/email triggers API call after 300ms debounce.
- [ ] Filter by status (Active/All) works.
- [ ] Add modal shows email/password/name/role with validation; no isActive toggle.
- [ ] Edit modal shows only name/role and the Active toggle; email/password are absent.
- [ ] Toggling Active from true to false shows confirmation dialog; on confirm, update proceeds.
- [ ] If confirmation cancelled, no API call occurs.
- [ ] Create and update API calls use exact DTOs with `StaffRole` type.
- [ ] Error messages are normalized and displayed.
- [ ] Session storage persists/restores state using the exact validation code; invalid data falls back to defaults without error.
- [ ] Page index is normalized after fetch but no recursive loop.
- [ ] All subscriptions use `takeUntilDestroyed`.
- [ ] No console errors.

## 20. Implementation Constraints

- Angular 18 control flow, standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder; do not rename.
- Only change to shared code is the optional `showInAdd`/`showInEdit` properties on `FormFieldDef` and the field filtering logic inside `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts`.
- All validation regex must match exactly those provided.
- `StaffRole` must be used as a union type; no plain `string` for role fields.

