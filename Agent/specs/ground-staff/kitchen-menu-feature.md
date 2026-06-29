# Patch Specsheet: Kitchen – Menu Item Availability Toggle

## 1. Purpose

- Add a **Menu Items** page to the Kitchen portal, allowing kitchen staff to quickly toggle the availability of menu items when ingredients run out.
- The page displays all menu items grouped by category, with a slide toggle for `isAvailable`.
- Toggling availability calls the existing `PATCH /api/v1/menu-items/{id}/status` endpoint.
- A confirmation dialog appears only when disabling an item (turning it off). No confirmation for enabling.

## 2. Files to Modify / Create

| File                                                                | Action                                    |
| ------------------------------------------------------------------- | ----------------------------------------- |
| `src/app/features/kitchen/kitchen-shell.component.ts`               | Add a “Menu Items” sidebar link.          |
| `src/app/features/kitchen/kitchen-shell.component.html`             | Add the new nav item.                     |
| **New:** `src/app/features/kitchen/pages/menu-items.component.ts`   | New component for menu item availability. |
| **New:** `src/app/features/kitchen/pages/menu-items.component.html` | Template.                                 |
| **New:** `src/app/features/kitchen/pages/menu-items.component.scss` | Styles.                                   |
| `src/app/app.routes.ts` (or kitchen route config)                   | Add `menu-items` child route.             |

## 3. Route Configuration

Add a new child route under the Kitchen shell:

```typescript
{
  path: 'menu-items',
  loadComponent: () => import('./features/kitchen/pages/menu-items.component')
    .then(m => m.KitchenMenuItemsComponent),
  canActivate: [kitchenGuard]
}
```

(Insert it before the wildcard route.)

## 4. Kitchen Shell – Sidebar Update

In `kitchen-shell.component.html`, add the following list item **after** the Dashboard link:

```html
<a
  mat-list-item
  routerLink="./menu-items"
  routerLinkActive="active"
  (click)="onNavClick()"
>
  <mat-icon matListItemIcon>restaurant_menu</mat-icon>
  <span matListItemTitle>Menu Items</span>
</a>
```

No changes to the TypeScript needed.

## 5. KitchenMenuItemsComponent

### 5.1 API

- **Selector**: `app-kitchen-menu-items`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatCardModule`, `MatSlideToggleModule`, `MatFormFieldModule`, `MatSelectModule`, `MatInputModule`, `MatProgressSpinnerModule`, `MatSnackBarModule`, `MatDialogModule`, `AlertComponent`, `MenuItemApiService`, `ConfirmDialogComponent` (shared).
- **Exact import paths**: (abbreviated; agent must use correct paths).

### 5.2 State (signals)

```typescript
menuItems = signal<MenuItem[]>([]);
filteredItems = signal<MenuItem[]>([]);
loading = signal(false);
error = signal<string | null>(null);
categoryFilter = new FormControl("All", { nonNullable: true });
searchControl = new FormControl("", { nonNullable: true });

categories = computed(() => {
  const cats = new Set(this.menuItems().map((i) => i.category || "Other"));
  return Array.from(cats).sort();
});
```

### 5.3 Template (exact – Angular 18 control flow)

```html
<div class="menu-items-page">
  <h1>Menu Items</h1>

  <div class="filter-bar">
    <mat-form-field appearance="outline">
      <mat-label>Category</mat-label>
      <mat-select
        [formControl]="categoryFilter"
        (selectionChange)="applyFilters()"
      >
        <mat-option value="All">All</mat-option>
        @for (cat of categories(); track cat) {
        <mat-option [value]="cat">{{ cat }}</mat-option>
        }
      </mat-select>
    </mat-form-field>
    <mat-form-field appearance="outline">
      <mat-label>Search</mat-label>
      <input
        matInput
        [formControl]="searchControl"
        (keyup)="applyFiltersDebounced()"
      />
      <mat-icon matSuffix>search</mat-icon>
    </mat-form-field>
  </div>

  @if (loading()) {
  <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchMenuItems()"
    >
      Retry
    </button>
  </app-alert>
  } @else if (filteredItems().length === 0) {
  <p>No menu items found.</p>
  } @else {
  <div class="menu-grid">
    @for (item of filteredItems(); track item.id) {
    <mat-card class="menu-card">
      <mat-card-header>
        <mat-card-title>{{ item.name }}</mat-card-title>
        <mat-card-subtitle
          >{{ item.category || 'Other' }} – {{ item.price | currency
          }}</mat-card-subtitle
        >
      </mat-card-header>
      <mat-card-actions>
        <mat-slide-toggle
          [checked]="item.isAvailable"
          (change)="onToggleAvailability(item, $event.checked)"
          color="primary"
        >
          {{ item.isAvailable ? 'Available' : 'Unavailable' }}
        </mat-slide-toggle>
      </mat-card-actions>
    </mat-card>
    }
  </div>
  }
</div>
```

### 5.4 Logic (exact)

```typescript
import {
  Component,
  inject,
  signal,
  computed,
  OnInit,
  DestroyRef,
} from "@angular/core";
import { FormControl } from "@angular/forms";
import { MatSnackBar } from "@angular/material/snack-bar";
import { MatDialog } from "@angular/material/dialog";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { debounceTime, distinctUntilChanged, finalize } from "rxjs";
import { MenuItemApiService } from "../../../admin/services/menu-item-api.service"; // adjust path
import { MenuItem } from "../../../admin/models/menu-item.model"; // adjust path
import { ConfirmDialogComponent } from "../../../../shared/components/confirm-dialog/confirm-dialog.component"; // adjust path

@Component({
  selector: "app-kitchen-menu-items",
  standalone: true,
  imports: [
    /* as above */
  ],
  templateUrl: "./menu-items.component.html",
  styleUrls: ["./menu-items.component.scss"],
})
export class KitchenMenuItemsComponent implements OnInit {
  private menuItemApi = inject(MenuItemApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  menuItems = signal<MenuItem[]>([]);
  filteredItems = signal<MenuItem[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  categoryFilter = new FormControl("All", { nonNullable: true });
  searchControl = new FormControl("", { nonNullable: true });

  categories = computed(() => {
    const cats = new Set(this.menuItems().map((i) => i.category || "Other"));
    return Array.from(cats).sort();
  });

  ngOnInit(): void {
    this.fetchMenuItems();
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.applyFilters());
  }

  fetchMenuItems(): void {
    this.loading.set(true);
    this.menuItemApi
      .getAll({ pageSize: 200, isAvailable: undefined })
      .pipe(
        // fetch all regardless of availability
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.menuItems.set(res.data);
          this.applyFilters();
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  applyFilters(): void {
    const category = this.categoryFilter.value;
    const search = this.searchControl.value.toLowerCase();
    let items = this.menuItems();
    if (category !== "All") {
      items = items.filter((i) => (i.category || "Other") === category);
    }
    if (search) {
      items = items.filter((i) => i.name.toLowerCase().includes(search));
    }
    this.filteredItems.set(items);
  }

  onToggleAvailability(item: MenuItem, newValue: boolean): void {
    if (!newValue) {
      // disabling — show confirmation
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: "Disable Menu Item",
          message: `Are you sure you want to make "${item.name}" unavailable?`,
        },
      });
      dialogRef
        .afterClosed()
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe((confirmed) => {
          if (confirmed) {
            this.updateAvailability(item, false);
          } else {
            // revert the toggle visually — need to force change detection on the item? We'll reassign the array to trigger change.
            this.menuItems.update((items) =>
              items.map((i) =>
                i.id === item.id ? { ...i, isAvailable: true } : i,
              ),
            );
            this.applyFilters(); // refresh filtered list
          }
        });
    } else {
      // enabling — no confirmation
      this.updateAvailability(item, true);
    }
  }

  private updateAvailability(item: MenuItem, isAvailable: boolean): void {
    this.menuItemApi
      .updateStatus(item.id, isAvailable)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.snackBar.open(
            `"${item.name}" is now ${isAvailable ? "available" : "unavailable"}.`,
            "Close",
            { duration: 3000 },
          );
          this.menuItems.update((items) =>
            items.map((i) => (i.id === item.id ? { ...i, isAvailable } : i)),
          );
          this.applyFilters();
        },
        error: (err: any) => {
          this.snackBar.open(this.extractErrorMessage(err), "Close", {
            duration: 5000,
          });
          // revert
          this.menuItems.update((items) =>
            items.map((i) =>
              i.id === item.id ? { ...i, isAvailable: !isAvailable } : i,
            ),
          );
          this.applyFilters();
        },
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === "string") return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return "An unexpected error occurred.";
  }
}
```

**Note:** The `MenuItemApiService` must already have an `updateStatus(id: number, isAvailable: boolean): Observable<void>` method that calls `PATCH /api/v1/menu-items/{id}/status?isAvailable=...`. If not, add it.

## 6. Styling

Add basic responsiveness:

```scss
.menu-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
  margin-top: 16px;
}
.filter-bar {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
}
```

## 7. Self‑Review Checklist

- [ ] “Menu Items” link appears in Kitchen sidebar and navigates to the page.
- [ ] Page fetches all menu items and groups them by category.
- [ ] Category filter and search work correctly.
- [ ] Each item shows a slide toggle reflecting its current availability.
- [ ] Toggling off shows a confirmation dialog; on confirm, the item becomes unavailable and snackbar confirms.
- [ ] Toggling on shows no confirmation; immediate update.
- [ ] Error snackbar appears on failure, and toggle visually reverts.
- [ ] Responsive grid works on mobile.
- [ ] Guard prevents non‑kitchen access.

## 8. Integration Notes

- The `MenuItemApiService.updateStatus` method is required; if missing, add it using the endpoint `PATCH /api/v1/menu-items/{id}/status?isAvailable=boolean`.
- No other roles or pages are affected.
- The kitchen shell remains lightweight; this is the only additional page.

---

