# Specsheet: Admin Feedback Page
## 1. Purpose
- Replace the `PlaceholderFeedbackComponent` with the full Feedback oversight page.
- Displays a paginated, sortable, searchable table of guest feedback entries.
- Allows moderators to hide/unhide feedback directly via a toggle switch in each row (no typing required).
- No create, edit, or delete actions – only moderation (hide/unhide).

## 2. Route & Navigation
- Path: `/operations/admin/oversight/feedback` (lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/oversight/feedback.component.ts`.

## 3. Authorization
- Inherits `adminGuard` from parent route.

## 4. Component API (FeedbackComponent)
- **Selector**: `app-feedback` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatTableModule`, `MatSortModule`, `MatPaginatorModule`, `MatButtonModule`, `MatIconModule`, `MatFormFieldModule`, `MatInputModule`, `MatProgressSpinnerModule`, `MatSlideToggleModule`, `MatTooltipModule`, `MatSnackBarModule`, `AlertComponent`.
- **Exact import paths** (use verbatim):
  ```ts
  import { CommonModule } from '@angular/common';
  import { Component, inject, signal } from '@angular/core';
  import { ReactiveFormsModule, FormControl } from '@angular/forms';
  import { MatTableModule } from '@angular/material/table';
  import { MatSortModule, Sort } from '@angular/material/sort';
  import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
  import { MatButtonModule } from '@angular/material/button';
  import { MatIconModule } from '@angular/material/icon';
  import { MatFormFieldModule } from '@angular/material/form-field';
  import { MatInputModule } from '@angular/material/input';
  import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
  import { MatSlideToggleModule } from '@angular/material/slide-toggle';
  import { MatTooltipModule } from '@angular/material/tooltip';
  import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
  import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
  import { DestroyRef } from '@angular/core';
  import { debounceTime, distinctUntilChanged, finalize } from 'rxjs';
  import { FeedbackApiService } from '../../services/feedback-api.service';
  import { Feedback } from '../../models/feedback.model';
  import { AlertComponent } from '../../../../shared/components/alert/alert.component';
  ```

- **Template** (full – Angular 18 control flow only):

```html
<div class="feedback-page">
  <!-- Controls: Search + Include Hidden toggle -->
  <div class="controls">
    <mat-form-field appearance="outline" class="search">
      <mat-label>Search comments or booking ID</mat-label>
      <input matInput [formControl]="searchControl" (keyup)="onSearchDebounced()" />
      <mat-icon matSuffix>search</mat-icon>
    </mat-form-field>
    <mat-slide-toggle [formControl]="includeHiddenControl" (change)="onIncludeHiddenToggle()">
      Show hidden feedback
    </mat-slide-toggle>
    @if (searchControl.value) {
      <button mat-button (click)="clearSearch()">Clear Search</button>
    }
  </div>

  <!-- Loading / Error / Content -->
  @if (loading() && entries().length === 0) {
    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchData()">Retry</button>
    </app-alert>
  }

  @if (entries().length > 0 || loading()) {
    @if (loading()) {
      <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    <table mat-table [dataSource]="entries()" matSort (matSortChange)="onSortChange($event)" aria-label="Feedback">
      <ng-container matColumnDef="id">
        <th mat-header-cell *matHeaderCellDef mat-sort-header="id">ID</th>
        <td mat-cell *matCellDef="let f">{{ f.id }}</td>
      </ng-container>
      <ng-container matColumnDef="bookingId">
        <th mat-header-cell *matHeaderCellDef>Booking ID</th>
        <td mat-cell *matCellDef="let f">{{ f.bookingId }}</td>
      </ng-container>
      <ng-container matColumnDef="rating">
        <th mat-header-cell *matHeaderCellDef mat-sort-header="rating">Rating</th>
        <td mat-cell *matCellDef="let f">{{ f.rating }}/5</td>
      </ng-container>
      <ng-container matColumnDef="comments">
        <th mat-header-cell *matHeaderCellDef>Comments</th>
        <td mat-cell *matCellDef="let f">{{ f.comments || '—' }}</td>
      </ng-container>
      <ng-container matColumnDef="createdAt">
        <th mat-header-cell *matHeaderCellDef mat-sort-header="createdAt">Created</th>
        <td mat-cell *matCellDef="let f">{{ f.createdAt | date:'short' }}</td>
      </ng-container>
      <ng-container matColumnDef="isHidden">
        <th mat-header-cell *matHeaderCellDef>Hidden</th>
        <td mat-cell *matCellDef="let f">{{ f.isHidden ? 'Yes' : 'No' }}</td>
      </ng-container>
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef>Moderate</th>
        <td mat-cell *matCellDef="let f">
          <mat-slide-toggle
            [checked]="f.isHidden"
            (change)="onToggleHidden(f, $event.checked)"
            [aria-label]="f.isHidden ? 'Show feedback' : 'Hide feedback'"
            matTooltip="Toggle visibility">
          </mat-slide-toggle>
        </td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
    </table>
    <mat-paginator
      [length]="totalCount()"
      [pageIndex]="pageIndex()"
      [pageSize]="pageSize()"
      [pageSizeOptions]="[10, 25, 50]"
      (page)="onPageChange($event)">
    </mat-paginator>
  } @else {
    <div class="empty-state">
      <p>No feedback found.</p>
      @if (includeHiddenControl.value) {
        <p>Try unchecking "Show hidden feedback" or adjusting your search.</p>
      } @else {
        <p>No visible feedback available.</p>
      }
    </div>
  }
</div>
```

## 5. State Management (All Signals)

```ts
// Table columns
displayedColumns = ['id', 'bookingId', 'rating', 'comments', 'createdAt', 'isHidden', 'actions'];

// Data (canonical signals)
entries = signal<Feedback[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

// Query state (canonical signals)
pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('createdAt');
sortDescending = signal(true);

// UI inputs (form controls)
searchControl = new FormControl('', { nonNullable: true });
includeHiddenControl = new FormControl(false, { nonNullable: true });

// Session storage key
private readonly STORAGE_KEY = 'feedbackState';
```

## 6. Data Flow & API Calls

### Service
- `FeedbackApiService` (root‑provided, `features/admin/services/feedback-api.service.ts`)

### Endpoints
| Method | Endpoint | Parameters / Body | Response |
|--------|----------|-------------------|----------|
| `getAll` | `GET /api/v1/feedback` | `pageNumber`, `pageSize`, `includeHidden`, `sortBy`, `sortDescending`, possibly `searchQuery` (if backend supports; we'll use the guestQuery pattern? The swagger didn't show a search param for feedback. We'll need to check: the user gave only includeHidden, sortBy, etc. No search param was mentioned. But they showed search in other endpoints. I'll add a note: if backend supports search, use the `guestQuery` param as in audit logs; else we'll filter client-side? Better ask for clarification. However, the user likely expects search. Since it's not specified, I'll assume there is a `searchQuery` param based on previous patterns. But to be safe, I'll use a generic approach: if the API doesn't support search, we can't implement it. The user gave no search parameter in the sample. I'll stick with the parameters given: only includeHidden, sort, pagination. No search. But we can still have a frontend search that filters the current page client-side? No, that's not good practice. I'll follow the given contract exactly: no search. I'll remove the search bar or add a note. The user said "use the below endpoints for backend api calls", they gave the GET with only includeHidden and sortBy. They didn't include a search. So I will omit the search input. I'll adjust the template to have only the includeHidden toggle, and no search field. That simplifies the page. I'll remove the search control entirely. The user only mentioned a toggle for includeHidden. Yes, they didn't mention search. So the page will be simple: table with toggle for hidden feedback, and moderate toggle per row. I'll redesign accordingly. I'll keep the template simple: top control is the slide toggle "Show hidden feedback". No search.

Thus, we remove search control, clear search button, etc. The state will only have includeHidden, sort, pagination.

Let's rewrite accordingly.After re-examining the provided endpoint, the Feedback page will be simpler than previously outlined: the GET endpoint supports only `includeHidden`, pagination, and sorting – **no search parameter**. Therefore, the specsheet is revised to remove search completely. The page will consist of a single toggle to show/hide moderated feedback, a table with a per‑row moderation toggle, and standard sorting/pagination.

---

# Specsheet: Admin Feedback Page (Final Deterministic)

## 1. Purpose
- Replace the `PlaceholderFeedbackComponent` with the full Feedback oversight page.
- Displays a paginated, sortable table of guest feedback entries.
- Allows moderators to hide/unhide feedback directly via a toggle switch in each row (no typing required).
- A “Show hidden feedback” toggle at the top controls whether moderated (hidden) entries are included.
- No search, create, edit, or delete actions – only moderation (hide/unhide).

## 2. Route & Navigation
- Path: `/operations/admin/oversight/feedback` (lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/oversight/feedback.component.ts`.

## 3. Authorization
- Inherits `adminGuard` from parent route.

## 4. Component API (FeedbackComponent)
- **Selector**: `app-feedback` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatTableModule`, `MatSortModule`, `MatPaginatorModule`, `MatButtonModule`, `MatIconModule`, `MatSlideToggleModule`, `MatTooltipModule`, `MatSnackBarModule`, `MatProgressSpinnerModule`, `AlertComponent`.
- **Exact import paths** (use verbatim):
  ```ts
  import { CommonModule } from '@angular/common';
  import { Component, inject, signal } from '@angular/core';
  import { ReactiveFormsModule, FormControl } from '@angular/forms';
  import { MatTableModule } from '@angular/material/table';
  import { MatSortModule, Sort } from '@angular/material/sort';
  import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
  import { MatButtonModule } from '@angular/material/button';
  import { MatIconModule } from '@angular/material/icon';
  import { MatSlideToggleModule } from '@angular/material/slide-toggle';
  import { MatTooltipModule } from '@angular/material/tooltip';
  import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
  import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
  import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
  import { DestroyRef } from '@angular/core';
  import { finalize } from 'rxjs';
  import { FeedbackApiService } from '../../services/feedback-api.service';
  import { Feedback } from '../../models/feedback.model';
  import { AlertComponent } from '../../../../shared/components/alert/alert.component';
  ```

- **Template** (full – Angular 18 control flow only):

```html
<div class="feedback-page">
  <!-- Top control: Show hidden feedback toggle -->
  <div class="controls">
    <mat-slide-toggle [formControl]="includeHiddenControl" (change)="onIncludeHiddenToggle()">
      Show hidden feedback
    </mat-slide-toggle>
  </div>

  <!-- Loading / Error / Content -->
  @if (loading() && entries().length === 0) {
    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchData()">Retry</button>
    </app-alert>
  }

  @if (entries().length > 0 || loading()) {
    @if (loading()) {
      <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    <table mat-table [dataSource]="entries()" matSort (matSortChange)="onSortChange($event)" aria-label="Feedback">
      <ng-container matColumnDef="id">
        <th mat-header-cell *matHeaderCellDef mat-sort-header="id">ID</th>
        <td mat-cell *matCellDef="let f">{{ f.id }}</td>
      </ng-container>
      <ng-container matColumnDef="bookingId">
        <th mat-header-cell *matHeaderCellDef>Booking ID</th>
        <td mat-cell *matCellDef="let f">{{ f.bookingId }}</td>
      </ng-container>
      <ng-container matColumnDef="rating">
        <th mat-header-cell *matHeaderCellDef mat-sort-header="rating">Rating</th>
        <td mat-cell *matCellDef="let f">{{ f.rating }}/5</td>
      </ng-container>
      <ng-container matColumnDef="comments">
        <th mat-header-cell *matHeaderCellDef>Comments</th>
        <td mat-cell *matCellDef="let f">{{ f.comments || '—' }}</td>
      </ng-container>
      <ng-container matColumnDef="createdAt">
        <th mat-header-cell *matHeaderCellDef mat-sort-header="createdAt">Created</th>
        <td mat-cell *matCellDef="let f">{{ f.createdAt | date:'short' }}</td>
      </ng-container>
      <ng-container matColumnDef="isHidden">
        <th mat-header-cell *matHeaderCellDef>Hidden</th>
        <td mat-cell *matCellDef="let f">{{ f.isHidden ? 'Yes' : 'No' }}</td>
      </ng-container>
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef>Moderate</th>
        <td mat-cell *matCellDef="let f">
          <mat-slide-toggle
            [checked]="f.isHidden"
            (change)="onToggleHidden(f, $event.checked)"
            [aria-label]="f.isHidden ? 'Show feedback' : 'Hide feedback'"
            matTooltip="Toggle visibility">
          </mat-slide-toggle>
        </td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
    </table>
    <mat-paginator
      [length]="totalCount()"
      [pageIndex]="pageIndex()"
      [pageSize]="pageSize()"
      [pageSizeOptions]="[10, 25, 50]"
      (page)="onPageChange($event)">
    </mat-paginator>
  } @else {
    <div class="empty-state">
      <p>No feedback found.</p>
      @if (includeHiddenControl.value) {
        <p>Even with hidden feedback included, no entries exist.</p>
      } @else {
        <p>No visible feedback available. Try enabling "Show hidden feedback".</p>
      }
    </div>
  }
</div>
```

## 5. State Management (All Signals)

```ts
// Table columns
displayedColumns = ['id', 'bookingId', 'rating', 'comments', 'createdAt', 'isHidden', 'actions'];

// Data (canonical signals)
entries = signal<Feedback[]>([]);
totalCount = signal(0);
loading = signal(false);
error = signal<string | null>(null);

// Query state (canonical signals)
pageIndex = signal(0);
pageSize = signal(10);
sortField = signal('createdAt');
sortDescending = signal(true);

// UI input (form control)
includeHiddenControl = new FormControl(false, { nonNullable: true });

// Session storage key
private readonly STORAGE_KEY = 'feedbackState';
```

## 6. Data Flow & API Calls

### Service
- `FeedbackApiService` (root‑provided, `features/admin/services/feedback-api.service.ts`)

### Endpoints
| Method | Endpoint | Parameters / Body | Response |
|--------|----------|-------------------|----------|
| `getAll` | `GET /api/v1/feedback` | `includeHidden` (boolean), `pageNumber`, `pageSize`, `sortBy`, `sortDescending` | `{ totalCount, data: Feedback[] }` |
| `moderate` | `PATCH /api/v1/feedback/{id}/moderate` | `{ isHidden: boolean }` | `void` (success) |

**Important:** The `includeHidden` parameter is sent exactly as the query key. The UI label is “Show hidden feedback”.

### DTOs / Models (exact)
```ts
// feedback.model.ts
export interface Feedback {
  id: number;
  bookingId: number;
  rating: number;
  comments: string | null;
  createdAt: string;   // ISO 8601
  isHidden: boolean;
}

export interface ModerateFeedbackRequest {
  isHidden: boolean;
}
```

### API Error Handling (same as all previous pages)
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
private feedbackApi = inject(FeedbackApiService);
private snackBar = inject(MatSnackBar);

ngOnInit(): void {
  this.restoreState();
  this.fetchData();
}

fetchData(): void {
  this.loading.set(true);
  this.error.set(null);
  this.feedbackApi.getAll({
    includeHidden: this.includeHiddenControl.value,
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

onIncludeHiddenToggle(): void {
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

onToggleHidden(feedback: Feedback, isHidden: boolean): void {
  // Immediately update the local state to reflect the toggle change (optimistic UI)
  this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden } : f));

  this.feedbackApi.moderate(feedback.id, { isHidden }).pipe(
    takeUntilDestroyed(this.destroyRef)
  ).subscribe({
    next: () => {
      this.snackBar.open(
        isHidden ? 'Feedback hidden' : 'Feedback visible',
        'Close',
        { duration: 2000 }
      );
    },
    error: (err: any) => {
      // Revert on failure
      this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden: !isHidden } : f));
      this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 });
    }
  });
}
```

## 7. Session Storage – Deterministic Implementation

**Schema:**
```json
{
  "includeHidden": false,
  "sortField": "createdAt",
  "sortDescending": true,
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

    if (typeof parsed.includeHidden === 'boolean') this.includeHiddenControl.setValue(parsed.includeHidden);
    if (typeof parsed.sortField === 'string') this.sortField.set(parsed.sortField);
    if (typeof parsed.sortDescending === 'boolean') this.sortDescending.set(parsed.sortDescending);
    if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0) this.pageIndex.set(parsed.pageIndex);
    if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0) this.pageSize.set(parsed.pageSize);
  } catch { /* fallback silently */ }
}

private saveState(): void {
  sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify({
    includeHidden: this.includeHiddenControl.value,
    sortField: this.sortField(),
    sortDescending: this.sortDescending(),
    pageIndex: this.pageIndex(),
    pageSize: this.pageSize(),
  }));
}
```

## 8. UI States
- **Initial load**: full‑page spinner when entries array empty and loading true.
- **Refetch**: `mat-progress-bar` shown while loading; table remains.
- **Error**: `app-alert` with retry.
- **Empty (no data at all)**: “No feedback found.”
- **Empty (hidden feedback excluded)**: “No visible feedback available. Try enabling ‘Show hidden feedback’.”
- **Toggle moderation**: Optimistic UI update; success snackbar; revert on failure.

## 9. Responsive Behaviour
- Table horizontally scrollable on mobile.
- Toggle controls stack vertically on narrow screens.

## 10. Accessibility
- Table has `aria-label="Feedback"`.
- Slide toggles have `aria-label` and tooltip.
- Error/snackbar announcements live via `aria-live` regions.

## 11. Integration Notes
- **Overwrite** placeholder: `src/app/features/admin/pages/oversight/feedback.component.ts`.
- `FeedbackApiService` and model must be created.
- The `includeHidden` query parameter is sent exactly as shown; UI uses friendly label.
- No search functionality – by design.
- No modifications to shared components.

## 12. File Structure
```
src/app/features/admin/
  pages/oversight/
    feedback.component.ts   (overwrite)
    feedback.component.html
    feedback.component.scss
  services/
    feedback-api.service.ts
  models/
    feedback.model.ts
```

## 13. Self‑Review Checklist
- [ ] Feedback table loads with data, pagination, sorting.
- [ ] “Show hidden feedback” toggle updates the table, resets page to 0.
- [ ] Toggling a row’s slider sends PATCH request with correct `isHidden`.
- [ ] Successful moderation updates UI instantly, shows snackbar.
- [ ] Failed moderation reverts the slider and shows error snackbar.
- [ ] Session storage persists includeHidden, sort, page settings.
- [ ] Loading/error/empty states display correctly.
- [ ] No old control flow (`*ngIf`, `*ngFor`); only Angular 18 control flow.
- [ ] No console errors, subscriptions cleaned.

## 14. Implementation Constraints
- Angular 18 control flow (`@if`, `@for`) ONLY.
- Standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder; do not rename.
- Raw array as `[dataSource]`; `matSort` only for sort events.
- `includeHidden` must be the exact query parameter key.
- Optimistic UI update with revert on error for moderation.
- `extractErrorMessage` helper used in all error handling.