# Patch Specsheet: Amenities – Availability Filter, Name Validation, Toggle Rendering

### 1. Purpose

- Add backend availability filter (`isAvailable` query param) to the Amenities list.
- Add a filter dropdown for “Availability” with options All / Available / Unavailable.
- Fix name validation to reject purely numeric values (must contain at least one letter).
- Ensure the edit modal shows the availability field only as a slide toggle, never as a text input.

### 2. Files to Modify

- `src/app/features/admin/pages/management/amenities-management.component.ts`
- `src/app/features/admin/services/amenity-api.service.ts` (only if method signature change needed; otherwise, just update call in component)

### 3. Changes to `AmenitiesManagementComponent`

#### 3.1 Add new state signal

```ts
availabilityFilter = signal<boolean | null>(null); // null = all, true = available, false = unavailable
```

#### 3.2 Update `crudConfig` filters

Replace `filters: []` with:

```ts
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
```

#### 3.3 Update `formFields` name validation

Change the name field validator to include a pattern requiring at least one letter:

```ts
{
  name: 'name',
  label: 'Name',
  type: 'text',
  validators: [
    Validators.required,
    Validators.maxLength(100),
    Validators.minLength(1),
    Validators.pattern(/^(?=.*[a-zA-Z])[a-zA-Z0-9\s\-']+$/), // must contain at least one letter
  ],
  showInAdd: true,
  showInEdit: true
}
```

#### 3.4 Update `fetchData` to include availabilityFilter

Modify the API call inside `fetchData()`:

```ts
this.amenityApi.getAll({
  pageNumber: this.pageIndex() + 1,
  pageSize: this.pageSize(),
  searchQuery: this.searchQuery() || undefined,
  sortBy: this.sortField(),
  sortDescending: this.sortDescending(),
  isAvailable: this.availabilityFilter() ?? undefined, // new
});
```

#### 3.5 Update `onFilterChange`

Replace the empty `onFilterChange` with:

```ts
onFilterChange(filters: Record<string, any>): void {
  const val = filters['isAvailable'];
  this.availabilityFilter.set(val === '' || val === null ? null : val);
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}
```

#### 3.6 Update session storage

Add `availabilityFilter` to the schema and methods.

**Schema**:

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

**In `saveState`**:

```ts
sessionStorage.setItem(
  this.STORAGE_KEY,
  JSON.stringify({
    searchQuery: this.searchQuery(),
    sortField: this.sortField(),
    sortDescending: this.sortDescending(),
    pageIndex: this.pageIndex(),
    pageSize: this.pageSize(),
    availabilityFilter: this.availabilityFilter(),
  }),
);
```

**In `restoreState`**:
Add:

```ts
if (
  parsed.availabilityFilter === null ||
  typeof parsed.availabilityFilter === "boolean"
) {
  this.availabilityFilter.set(parsed.availabilityFilter);
}
```

#### 3.7 Ensure toggle rendering in edit modal

No code change needed here, but add a **verification note**: The `isAvailable` field is already typed `'toggle'` and has `showInEdit: true`. The generic modal must render a `mat-slide-toggle` for any field of type `'toggle'`. If it currently renders an `<input>`, patch `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.html` to conditionally output:

```html
@if (field.type === 'toggle') {
<mat-slide-toggle [formControlName]="field.name"
  >{{ field.label }}</mat-slide-toggle
>
} @else {
<!-- existing input field rendering -->
}
```

But to keep the patch minimal, we assume the generic modal already supports `'toggle'` as per earlier specs. If the implementation is missing, apply that template change.

### 4. Changes to `AmenityApiService`

If the service method `getAll` does not already accept an optional `isAvailable` parameter, update its signature and HTTP params:

```ts
getAll(params: {
  pageNumber: number;
  pageSize: number;
  searchQuery?: string;
  sortBy?: string;
  sortDescending?: boolean;
  isAvailable?: boolean;   // new
}): Observable<{ totalCount: number; data: Amenity[] }> {
  let httpParams = new HttpParams()
    .set('pageNumber', params.pageNumber)
    .set('pageSize', params.pageSize);
  if (params.searchQuery) httpParams = httpParams.set('searchQuery', params.searchQuery);
  if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
  if (params.sortDescending !== undefined) httpParams = httpParams.set('sortDescending', params.sortDescending);
  if (params.isAvailable !== undefined) httpParams = httpParams.set('isAvailable', params.isAvailable);
  return this.http.get<...>(`${this.baseUrl}/amenities`, { params: httpParams }).pipe(...);
}
```

### 5. Verification Checklist

- [ ] Amenities table now includes an “Availability” filter dropdown with All/Available/Unavailable.
- [ ] Selecting “Available” sends `isAvailable=true` and shows only available amenities.
- [ ] Selecting “Unavailable” sends `isAvailable=false`.
- [ ] Clearing the filter (or selecting “All”) omits the `isAvailable` param.
- [ ] Session storage stores and restores the availability filter.
- [ ] Name field now rejects purely numeric strings (e.g., “123” shows pattern error).
- [ ] Names like “Swimming Pool 12” are accepted (contains letters).
- [ ] In edit modal, the availability field is only a slide toggle; no text input.
- [ ] Toggle changes update the DTO correctly on save.
- [ ] Sorting by “Available” column works.

### 6. Integration Notes

- No changes to other CRUD pages; this patch only touches the Amenities feature files.
- The generic modal toggle handling is assumed to exist; if not, the additional template snippet above must be applied.

---

