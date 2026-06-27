# Patch Specsheet: GenericCrudComponent – Preserve MatSort State During Loading

### 1. Purpose
Fix a critical UI bug where sorting direction toggles (asc ↔ desc) never appear because the table is destroyed and re‑created on every API call. This is caused by the `@if (loading())` condition replacing the entire table with a spinner, which resets Angular Material’s `MatSort` instance.  

After this patch, the table remains in the DOM while data is being refreshed; only a lightweight loading indicator is shown (e.g., a linear progress bar). The full‑page spinner is used exclusively for the **initial load** when no data is yet present.

### 2. Files to Modify
- `src/app/shared/components/generic-crud/generic-crud.component.html`
- `src/app/shared/components/generic-crud/generic-crud.component.ts` (small addition)

### 3. Current Problematic Code
```html
@if (config().loading()) {
  <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
} @else if (config().error()) { ... }
@else if (!config().data() || config().data().length === 0) { ... }
@else {
  <!-- table, paginator, etc. -->
}
```
Every sort, filter, or page change sets `loading = true`, which destroys the table and rebuilds it after the response arrives. This causes `MatSort` to lose its state and default back to ascending.

### 4. Fix

We introduce a subtle change:

- **Initial load** (no data yet): keep showing the full‑page spinner.
- **Subsequent loads** (data already exists): leave the table in place, but overlay a **linear progress bar** at the top of the data area and optionally disable interactive elements (like sort headers) while loading. We’ll use a simple `mat-progress-bar` that appears when `loading()` is true and data already exists.

The template is restructured as follows:

```html
<!-- Show full-page spinner ONLY when loading AND no data yet -->
@if (config().loading() && (!config().data() || config().data().length === 0)) {
  <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
} @else {
  <!-- Always show the content area, even when loading (if data exists) -->
  <div class="crud-content">
    <!-- Top bar with entity name and add button (unchanged) -->
    <!-- ... -->

    <!-- Search & Filter Bar (unchanged) -->
    <!-- ... -->

    <!-- Loading indicator for data refresh -->
    @if (config().loading()) {
      <mat-progress-bar mode="indeterminate" color="primary"></mat-progress-bar>
    }

    <!-- Error state -->
    @if (config().error()) {
      <app-alert type="error" [message]="config().error()!" (closed)="config().error.set(null)"></app-alert>
    }

    <!-- Empty state -->
    @if (!config().loading() && (!config().data() || config().data().length === 0)) {
      <div class="empty-state">
        <!-- ... existing empty state logic ... -->
      </div>
    }

    <!-- Table or Card View (always mounted if data exists) -->
    @if (config().data() && config().data().length > 0) {
      <div class="desktop-view">
        <table mat-table [dataSource]="config().data()" matSort (matSortChange)="onSortChange($event)">
          <!-- ... columns ... -->
        </table>
      </div>
      <div class="mobile-view">
        <app-cards-view ...></app-cards-view>
      </div>
      <!-- Paginator (always mounted when data exists) -->
      <mat-paginator ...></mat-paginator>
    }
  </div>
}
```

**Key points:**
- The full‑page spinner only shows when `loading()` is `true` **and** there is no existing data (`!config().data() || config().data().length === 0`). This preserves the initial loading UX.
- During refreshes (sort, filter, page, search), the table and paginator remain in the DOM. A `mat-progress-bar` appears at the top of the content area to indicate activity.
- The sort headers are still interactive while loading? It’s acceptable; they will fire new events. To prevent race conditions, we could disable them, but it’s not strictly necessary; the old behavior also allowed clicking while loading. No change needed.
- The error state is shown inside the same container, not replacing the table, so the table stays mounted even on error. However, the current spec often replaces the table with error. To avoid destroying MatSort, we should not use `@if` that removes the table. The above structure shows error below the progress bar, and the table remains visible. If an error occurs, the user can see the last known data and an error message, which is better UX.

**Note:** The `CardsViewComponent` and paginator also need to stay mounted. They are inside the `@if (data && data.length > 0)` block, which is always true after the first successful load. So they persist.

**Impact on existing functionality:**  
- Initial load behavior unchanged.  
- Error handling improved: table stays, error message appears above it.  
- Sorting now correctly toggles because `MatSort` is never destroyed.  
- All outputs (searchChange, filterChange, etc.) remain identical.

### 5. Changes to TypeScript (if any)
No new signals are strictly required, but for clarity we could add a computed property `isInitialLoad`:
```ts
isInitialLoad = computed(() => this.config().loading() && (!this.config().data() || this.config().data().length === 0));
```
This is used in the template to conditionally show the full spinner. The template can inline the expression, but using a computed is cleaner. We'll add that to the generic component class.

### 6. Verification Checklist
- [ ] On first page load with no data, full‑page spinner appears.
- [ ] After data loads, table appears.
- [ ] Click a column header to sort ascending; indicator (arrow) appears, progress bar shows briefly, data refreshes, and table stays.
- [ ] Click same column header again; direction toggles to descending, arrow changes, data refreshes.
- [ ] Change page or filter; progress bar shows, table stays.
- [ ] When an error occurs during refresh, the table remains visible and an error alert appears above it.
- [ ] Empty state still works when no data after filters.
- [ ] Mobile card view also remains mounted during refreshes.
- [ ] No regression in any other CRUD page using the generic component.

### 7. Learning Note for Future Specsheets
**Never destroy a data‑display component (table, list, etc.) during a loading state if it uses stateful Angular Material directives like `MatSort`, `MatPaginator`, or `MatTab`.**
Instead, always keep the component mounted and overlay a loading indicator. Only replace the entire content with a spinner when there is genuinely nothing to show (i.e., no data has ever been loaded). This preserves directive state and prevents UI glitches.

