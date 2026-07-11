# Patch Specsheet: Rooms – Fix Grid Click with Persistent Search

### 1. Purpose
Replace the non-functional “highlight room in table” feature with a reliable mechanism. When a room card is clicked in the status grid:

- The search field is populated with the clicked room’s number.
- A search is immediately executed, showing only the matching room(s) in the table.
- The search remains active until the user manually clears the search field or clicks “Clear Filters”.
- No automatic reversion occurs.

This ensures the clicked room is always visible on the first page without any complex pagination logic, and the user remains in control.

### 2. Files to Modify
- `src/app/shared/components/generic-crud/generic-crud.component.ts`
- `src/app/shared/components/generic-crud/generic-crud.component.html` (minor, if needed)
- `src/app/features/admin/pages/management/room-management.component.ts`
- `src/app/features/admin/pages/management/room-management.component.html`
- `src/app/features/admin/components/room-status-grid/room-status-grid.component.ts`

### 3. Changes to `GenericCrudComponent` (New Input)

**Add a new input** to allow the parent to programmatically set the search box text:
```ts
searchQuery = input<string>('');
```

**Sync the input with the internal `searchControl`** using an effect (in the constructor or a method):
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

**Important:** `searchControl` is the `FormControl<string>` already used in the template. Setting its value with `emitEvent: false` prevents an unwanted `searchChange` emission from the generic component, avoiding a double API call. The parent will trigger the data fetch directly.

### 4. Changes to `RoomStatusGridComponent`

**Modify the output** to emit the entire `RoomStatus` object instead of just the `roomId`. This gives the parent direct access to the room number.

```ts
// Old
roomClicked = output<number>();

// New
roomClicked = output<RoomStatus>();
```

**Update the card click handler** in the template:
```html
<div class="room-card"
     (click)="roomClicked.emit(room)"   <!-- emit full object -->
     ... >
```

### 5. Changes to `RoomManagementComponent`

**Remove highlight‑related code**:
- Delete the `highlightRoomId` signal.
- Remove `[highlightId]` binding from `<app-generic-crud>` in the template.

**Update the grid event binding** in the template:
```html
<app-room-status-grid
  [roomTypeId]="roomTypeFilter()"
  (roomClicked)="onGridRoomClicked($event)">
</app-room-status-grid>
```

**Add a new handler** that populates the search field and refreshes data:
```ts
onGridRoomClicked(room: RoomStatus): void {
  // Set the search query to the clicked room's number
  this.searchQuery.set(room.roomNumber);
  // Manually trigger a data fetch; the generic component will display the search text
  this.pageIndex.set(0);
  this.saveState();  // optional, can still persist this search if desired
  this.fetchData();
}
```

**Note:** The `searchQuery` signal is already bound to the `GenericCrudComponent` via `[searchQuery]="searchQuery()"`. We must add that binding to the template.

Add the input binding in the `app-generic-crud` element:
```html
<app-generic-crud
  [config]="crudConfig"
  [searchQuery]="searchQuery()"   <!-- new binding -->
  (edit)="onEdit($event)"
  (searchChange)="onSearchChange($event)"
  (filterChange)="onFilterChange($event)"
  (sortChange)="onSortChange($event)"
  (pageChange)="onPageChange($event)"
  (save)="onSave($event)">
</app-generic-crud>
```

**Update `onSearchChange`** to keep the parent’s `searchQuery` signal in sync with user‑typed changes (already does that). Nothing else changes.

### 6. Verification Checklist
- [ ] Clicking a room card in the grid immediately populates the search field with that room number.
- [ ] The table updates to show only the matching room(s).
- [ ] The search text remains in the search field; the table stays filtered.
- [ ] Manually clearing the search field or clicking “Clear Filters” restores the full list.
- [ ] Clicking another room card while a search is active replaces the search with the new room number.
- [ ] The generic component’s search input value stays in sync when set programmatically.
- [ ] No automatic reversion or timer – the user is in full control.
- [ ] The `highlightId` logic is completely removed and no console errors appear.
- [ ] Session storage still saves/restores the search query as part of normal state (optional but consistent).

### 7. Integration Note
This patch permanently replaces the broken highlight feature with a straightforward, user‑driven search. The generic CRUD component’s new `searchQuery` input is a generic enhancement that can be reused by any other management page in the future. No timer or temporary state is required, keeping the code simple and predictable.