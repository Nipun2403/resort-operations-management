# Patch Specsheet: Room Types – Enable Search with `searchQuery`

### 1. Purpose
- Activate the previously disabled search bar on the Room Types management page.
- Add the `searchQuery` parameter to the backend request, so typing in the search box filters room types by name or description.
- Persist the search query in session storage so it’s retained across navigations.

### 2. Files to Modify
- `src/app/features/admin/pages/management/room-type-management.component.ts`
- No changes to the generic CRUD component, API service, or models.

### 3. Changes to RoomTypeManagementComponent

**Add a new signal** for the search query:
```ts
searchQuery = signal('');
```

**Update `fetchData()`** to include `searchQuery` in the API call parameters:
```ts
this.roomTypeApi.getAll({
  includeRetired: this.includeRetired(),
  pageNumber: this.pageIndex() + 1,
  pageSize: this.pageSize(),
  sortBy: this.sortField(),
  sortDescending: this.sortDescending(),
  searchQuery: this.searchQuery() || undefined,  // only send if non-empty
})
```

**Replace the stub `onSearchChange`** with a real implementation:
```ts
onSearchChange(query: string): void {
  this.searchQuery.set(query.trim() || '');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}
```

**Update session storage schema** to include `searchQuery`:
```json
{
  "includeRetired": false,
  "sortField": "name",
  "sortDescending": false,
  "pageIndex": 0,
  "pageSize": 10,
  "searchQuery": ""
}
```

Update `saveState()` and `restoreState()` accordingly.

### 4. UI Behaviour
- The search input (already present in the generic CRUD component) will now trigger API calls with a 300ms debounce (handled by the generic component). As the user types, the list filters dynamically.
- If the search box is cleared, the full list is reloaded.

### 5. Integration Notes
- The `RoomTypeApiService.getAll()` method signature must be updated to accept an optional `searchQuery` parameter and pass it to the HTTP request as a query param.
- The generic CRUD component already emits `searchChange` – no changes required.

### 6. Verification Checklist
- [ ] Typing in the search box triggers a new API call with `searchQuery` after debounce.
- [ ] Filtering by name/description works as expected.
- [ ] Clearing the search box restores the full list.
- [ ] Search query persists in session storage and is restored on page reload.
- [ ] Page index resets to 0 when a new search is performed.
- [ ] No regression in other filters, sorting, or pagination.

---