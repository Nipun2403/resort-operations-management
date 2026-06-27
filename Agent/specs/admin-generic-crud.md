# Specsheet: GenericCrudComponent

### 1. Purpose

- A reusable, configuration‑driven component for all admin CRUD pages.
- Provides a consistent UX for listing, searching, filtering, sorting, paginating, and editing entities via a modal.
- Exchanges data with its parent via **inputs** and **outputs**; the parent owns the API calls and state.
- Internally handles modal open/close, confirmation dialogs, and mobile card view.

### 2. Route & Navigation

- This component is **not** a route itself. It will be used inside a page component that is lazy‑loaded under the admin shell.

### 3. Authorization

- None. The parent page already enforces `adminGuard`.

### 4. Component API

**Selector**: `app-generic-crud`

**Standalone**: `true`

**Imports**:

- `CommonModule`, `ReactiveFormsModule`
- Angular Material: `MatTableModule`, `MatSortModule`, `MatPaginatorModule`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatInputModule`, `MatFormFieldModule`, `MatDialogModule`, `MatSlideToggleModule`, `MatProgressSpinnerModule`, `MatTooltipModule`, `MatSnackBarModule`
- `CardsViewComponent` (standalone, same folder)
- `ConfirmDialogComponent` (standalone, shared)
- `AlertComponent` (standalone, shared)

**Inputs**:

```ts
config = input.required<CrudConfig<any>>();
```

**Outputs**:

```ts
searchChange = output<string>();
filterChange = output<Record<string, any>>();
sortChange = output<{ active: string; direction: "asc" | "desc" }>();
pageChange = output<{ pageIndex: number; pageSize: number }>();
save = output<{ formValue: any; isActive: boolean }>(); // emitted from modal on save
```

**Internal signals**:

```ts
isModalOpen = signal(false); // not used directly; handled via MatDialog
editMode = signal(false);
selectedEntity = signal<any | null>(null);
modalLoading = signal(false);
modalError = signal<string | null>(null);
```

### 5. Template Structure

```html
<div class="crud-container">
  <!-- Top bar -->
  <div class="top-bar">
    <h2>{{ config().entityNamePlural }}</h2>
    <button
      mat-raised-button
      color="primary"
      (click)="openAddModal()"
    >
      <mat-icon>add</mat-icon> Add {{ config().entityName }}
    </button>
  </div>

  <!-- Search & Filter Bar -->
  <div class="search-filter-bar">
    <mat-form-field
      appearance="outline"
      class="search-field"
    >
      <mat-label>Search {{ config().entityNamePlural }}</mat-label>
      <input
        matInput
        [formControl]="searchControl"
        (keyup)="onSearchDebounced()"
      />
      <mat-icon matSuffix>search</mat-icon>
    </mat-form-field>
    @for (filter of config().filters; track filter.key) {
    <mat-form-field appearance="outline">
      <mat-label>{{ filter.label }}</mat-label>
      <mat-select [formControl]="filterControls.get(filter.key)!">
        @for (option of filter.options; track option.value) {
        <mat-option [value]="option.value">{{ option.label }}</mat-option>
        }
      </mat-select>
    </mat-form-field>
    } @if (hasActiveFilters()) {
    <button
      mat-button
      (click)="clearFilters()"
    >
      Clear Filters
    </button>
    }
  </div>

  <!-- Loading / Error / Content -->
  @if (config().loading()) {
  <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
  } @else if (config().error()) {
  <app-alert
    type="error"
    [message]="config().error()!"
    (closed)="config().error.set(null)"
  ></app-alert>
  } @else if (!config().data() || config().data().length === 0) {
  <div class="empty-state">
    <img
      src="assets/empty-state.svg"
      alt=""
    />
    <p>No {{ config().entityNamePlural }} found.</p>
    @if (hasActiveFilters()) {
    <p>Try adjusting your filters.</p>
    <button
      mat-button
      (click)="clearFilters()"
    >
      Clear Filters
    </button>
    } @else {
    <button
      mat-raised-button
      (click)="openAddModal()"
    >
      Add your first {{ config().entityName }}
    </button>
    }
  </div>
  } @else {
  <!-- Desktop Table -->
  <div class="desktop-view">
    <table
      mat-table
      [dataSource]="config().data()"
      matSort
      (matSortChange)="onSortChange($event)"
    >
      @for (col of config().columns; track col.field) {
      <ng-container [matColumnDef]="col.field">
        <th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="{{ col.sortable ? col.field : '' }}"
        >
          {{ col.header }}
        </th>
        <td
          mat-cell
          *matCellDef="let row"
        >
          @if (col.cellTemplate) {
          <ng-container
            *ngTemplateOutlet="col.cellTemplate; context: { $implicit: row }"
          ></ng-container>
          } @else { {{ col.getValue(row) }} }
        </td>
      </ng-container>
      }
      <ng-container matColumnDef="actions">
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Actions
        </th>
        <td
          mat-cell
          *matCellDef="let row"
        >
          <button
            mat-icon-button
            (click)="openEditModal(row)"
            aria-label="Edit"
          >
            <mat-icon>edit</mat-icon>
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
      ></tr>
    </table>
  </div>

  <!-- Mobile Card View -->
  <div class="mobile-view">
    <app-cards-view
      [data]="config().data()"
      [columns]="config().columns"
      (edit)="openEditModal($event)"
    ></app-cards-view>
  </div>

  <!-- Paginator -->
  <mat-paginator
    [length]="config().totalCount()"
    [pageIndex]="config().pageIndex()"
    [pageSize]="config().pageSize()"
    [pageSizeOptions]="[10, 25, 50, 100]"
    (page)="onPageChange($event)"
  ></mat-paginator>
  }
</div>
```

### 6. Internal State & Modal Lifecycle

The modal is **strictly a MatDialog component‑based modal** using a separate `CrudModalComponent` (see file structure). The generic CRUD component does **not** use an inline template.

**Modal open methods**:

```ts
private dialog = inject(MatDialog);

openAddModal(): void {
  this.editMode.set(false);
  this.selectedEntity.set(null);
  const data: CrudModalData = {
    editMode: false,
    entity: null,
    formFields: this.config().formFields,
    supportsToggle: this.config().supportsToggle,
  };
  this.dialogRef = this.dialog.open(CrudModalComponent, { data });
  this.handleModalClose();
}

openEditModal(row: any): void {
  this.editMode.set(true);
  this.selectedEntity.set(row);
  const data: CrudModalData = {
    editMode: true,
    entity: row,
    formFields: this.config().formFields,
    supportsToggle: this.config().supportsToggle,
  };
  this.dialogRef = this.dialog.open(CrudModalComponent, { data });
  this.handleModalClose();
}

private handleModalClose(): void {
  this.dialogRef!.afterClosed().pipe(
    takeUntilDestroyed(this.destroyRef)
  ).subscribe((result: CrudModalResult | null) => {
    if (result) {
      // If toggle changed to false, show confirmation first
      if (result.isActive === false && result.previousIsActive === true) {
        this.showDisableConfirmation(result);
      } else {
        this.save.emit({ formValue: result.formValue, isActive: result.isActive });
      }
    }
    // Reset modal state
    this.modalError.set(null);
    this.selectedEntity.set(null);
  });
}
```

**Form reset is handled by `CrudModalComponent`** – when the dialog closes, its own `modalForm` is destroyed. The generic component only clears `modalError` and `selectedEntity` (as above).

**Disable confirmation**:

```ts
private showDisableConfirmation(result: CrudModalResult): void {
  const confirmRef = this.dialog.open(ConfirmDialogComponent, {
    data: {
      title: `Disable ${this.config().entityName}?`,
      message: `This will make it unavailable to guests.`,
    },
  });
  confirmRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (confirmed) {
      this.save.emit({ formValue: result.formValue, isActive: false });
    }
  });
}
```

### 7. Pagination Reset Rule

When any of the following events occur, the parent must reset `pageIndex` to `0` before re‑fetching data. The generic component emits the event; the parent is responsible for resetting its own signal. For clarity, the generic component will enforce that on search/filter/sort emission, the parent should receive the event and then the parent must set the page index signal to 0.

We add a clear rule in the parent integration instructions: **On any searchChange, filterChange, or sortChange, set `pageIndex` signal to 0 before calling the API**. The generic component does not directly change the config signals.

### 8. Data Flow & API Calls (Parent responsibility)

The parent component provides all data signals and handles API calls. The generic component is a pure view layer.

### 9. Type Definitions (unchanged)

```ts
export interface CrudModalData {
  editMode: boolean;
  entity: any | null;
  formFields: FormFieldDef[];
  supportsToggle: boolean;
}

export interface CrudModalResult {
  formValue: any;
  isActive: boolean;
  previousIsActive: boolean; // needed for disable confirmation check
}
```

### 10. Responsive Behaviour

- Desktop: table, paginator.
- Mobile: cards view, same paginator.
  Switching via CSS media queries.

### 11. Accessibility

As before.

### 12. Integration Notes

- The `CrudModalComponent` must be created as a standalone component inside `shared/components/generic-crud/crud-modal/`.
- The `ConfirmDialogComponent` and `AlertComponent` already exist.
- The `CrudConfig` interface is defined in `shared/models/crud-config.model.ts`.
- Parent pages (e.g., `RoomTypesPageComponent`) will inject their API service, create signals, and pass config.

### 13. File Structure (Created in this spec)

```
src/app/
  shared/
    components/
      generic-crud/
        generic-crud.component.ts
        generic-crud.component.html
        generic-crud.component.scss
        cards-view/
          cards-view.component.ts
          cards-view.component.html
          cards-view.component.scss
        crud-modal/
          crud-modal.component.ts
          crud-modal.component.html
          crud-modal.component.scss
      confirm-dialog/
        confirm-dialog.component.ts   (if not already)
        ...
      alert/
        alert.component.ts           (if not already)
        ...
    models/
      crud-config.model.ts
```

### 14. Implementation Constraints

- Use Angular 18 control flow.
- Use standalone components everywhere.
- All subscriptions use `takeUntilDestroyed(this.destroyRef)`.
- No direct API calls in generic component; all via outputs.
- On search/filter/sort, the parent must reset `pageIndex` to 0.
- Modal is exclusively `MatDialog` component‑based, no inline template.

### 15. Self‑Review Checklist

- [ ] Add modal opens with empty form, edit modal with pre‑filled data.
- [ ] Modal close resets form and clears error/selection.
- [ ] Disable toggle triggers confirmation dialog.
- [ ] Search/filter/sort events cause pageIndex reset in parent.
- [ ] Pagination, sorting, filtering all functional.
- [ ] Mobile card view appears correctly.
- [ ] No direct subscriptions without `takeUntilDestroyed`.

---

