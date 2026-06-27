# Patch Specsheet A: GenericCrudComponent – Sort Cycle & Search State

## 1. Purpose

- Fix two bugs affecting all CRUD management pages:
  1. **Sort blank click**: clicking a column header a third time clears the sort direction (empty string), which is ignored by the parent, making the sort appear stuck.
  2. **Search text lost on navigation**: the search input is empty when returning to a page, even though the parent’s `searchQuery` signal still holds the previous value from session storage. The table shows no data until a keystroke re‑triggers the search.

## 2. Sort Toggle Fix – Use Angular Material’s Built‑In Flag

### Root cause

`MatSort` by default cycles `asc` → `desc` → `''` (clear). The parent ignores empty direction events, so the third click does nothing.

### Fix – Use `matSortDisableClear`

Angular Material provides the `matSortDisableClear` input. When set to `true`, the sort never emits an empty direction; it cycles `asc` → `desc` → `asc` endlessly.

**File**: `src/app/shared/components/generic-crud/generic-crud.component.html`

**Change**: Locate the `<table>` element that has `matSort`. Add the attribute:

```html
<table
  mat-table
  [dataSource]="config().data()"
  matSort
  matSortDisableClear
  (matSortChange)="onSortChange($event)"
></table>
```

That’s it – no TypeScript changes required. The generic component already emits the event to the parent via `sortChange`. Now the parent always receives `'asc'` or `'desc'`, never `''`.

**Result**: Every column click toggles direction; no blank click.

## 3. Search State Persistence Fix – Synchronise Input with Parent Signal

### Root cause

The generic component’s internal `searchControl` is not updated when the parent restores `searchQuery` from session storage. The input field remains empty, so the table appears empty until the user interacts with the search box.

### Fix – Add Optional `searchQuery` Input and Sync via Effect

**File**: `src/app/shared/components/generic-crud/generic-crud.component.ts`

**Changes**:

1. **Add the input** (already exists? Actually we need to add it; it wasn’t there before). Add:

   ```ts
   import { effect } from "@angular/core";

   // inside component class
   searchQuery = input<string>("", {
     transform: (value: string) => value ?? "",
   });
   ```

2. **Sync the internal search control** with the input value **without triggering additional search emissions**. Use an `effect` in the constructor:

   ```ts
   constructor() {
     effect(() => {
       const query = this.searchQuery();
       if (query !== this.searchControl.value) {
         this.searchControl.setValue(query, { emitEvent: false });
       }
     });
   }
   ```

   - `emitEvent: false` prevents the control’s `valueChanges` from firing, so no spurious `searchChange` emissions are generated.
   - The condition `query !== this.searchControl.value` avoids unnecessary updates when the values already match.

**Important design notes to prevent feedback loops**:

- The generic component’s `searchChange` output is only triggered by user keystrokes (via the debounced `valueChanges` subscription). Setting the control programmatically with `emitEvent: false` does **not** trigger that output.
- The parent must ensure that its `onSearchChange` handler does **not** update the `searchQuery` signal if the new value is identical to the current signal value. (Already implemented in all management pages – they call `this.searchQuery.set(query.trim() || '')` – that’s fine.)

## 4. Management Pages – Add `searchQuery` Binding

Each management page that uses the generic CRUD component must now pass its `searchQuery` signal to the new input. The following files need a one‑line addition in their template:

| Page       | File to modify                            | New attribute to add on `<app-generic-crud>` |
| ---------- | ----------------------------------------- | -------------------------------------------- |
| Room Types | `.../room-type-management.component.html` | `[searchQuery]="searchQuery()"`              |
| Rooms      | `.../room-management.component.html`      | `[searchQuery]="searchQuery()"`              |
| Staff      | `.../staff-management.component.html`     | `[searchQuery]="searchQuery()"`              |
| Amenities  | `.../amenities-management.component.html` | `[searchQuery]="searchQuery()"`              |
| Menu Items | `.../menu-management.component.html`      | `[searchQuery]="searchQuery()"`              |

**Exact line** (place immediately after `[config]="crudConfig"`):

```html
<app-generic-crud
  [config]="crudConfig"
  [searchQuery]="searchQuery()"
  (edit)="onEdit($event)"
  ...
></app-generic-crud>
```

**Verification**: After navigating away and returning to the page, the search field will display the previously stored query and the table will already be filtered, matching the restored session state.

## 5. Imports & Type Safety

- In the generic component, import `effect` from `@angular/core`.
- The `matSortChange` event type used in the template and emitted by the generic component should be `Sort` from `@angular/material/sort`. The generic component already imports this.

## 6. Self‑Review Checklist (for the agent)

- [ ] In any management page, clicking a column header toggles `asc` ↔ `desc` indefinitely; no blank third click.
- [ ] Adding `matSortDisableClear` does not break other behaviour (sort still works).
- [ ] The search input in every CRUD page shows the previously saved query after navigating away and back.
- [ ] The table reflects the filtered results corresponding to the displayed search text.
- [ ] Typing a new search term still emits `searchChange` and triggers a fetch.
- [ ] No console errors, no flickering, no infinite loops.

## 7. Integration Notes

- This patch modifies the **shared** generic CRUD component. The sort fix (`matSortDisableClear`) is a one‑attribute template change. The search sync is a small TypeScript addition and is backwards‑compatible (the input is optional; if a parent doesn’t provide it, the generic component behaves exactly as before).
- No changes are needed in the parent TypeScript code beyond adding the `[searchQuery]` binding in the HTML templates.

---

