# Patch Specsheet: Room Edit – Fix Incorrect API Call (POST instead of PATCH)

## 1. Purpose
Fix a bug where editing a room triggers a POST request to the create endpoint (`POST /api/v1/rooms`) instead of the update endpoint (`PATCH /api/v1/rooms/{id}`). This causes a 400 Bad Request because the backend rejects the creation of a duplicate room or missing required fields.

## 2. Root Cause
The `GenericCrudComponent` currently does **not** emit an `edit` output to the parent when the user clicks the edit button. The parent (`RoomManagementComponent`) relies on the `(edit)` event to store the entity being edited (`editingEntity` signal). Because the event is never emitted, `editingEntity` stays `null`, and the save handler incorrectly falls into the **create** branch.

## 3. Files to Modify
- `src/app/shared/components/generic-crud/generic-crud.component.ts`
- `src/app/shared/components/generic-crud/generic-crud.component.html` (if the edit button is not wired)

## 4. Changes to `GenericCrudComponent`

### 4.1 Add a new output
```ts
edit = output<any>();
```

### 4.2 Modify the `openEditModal` method
The method currently receives the row and opens the modal. We must emit the row to the parent just before opening the modal so the parent can store the entity.

```ts
openEditModal(row: any): void {
  this.edit.emit(row);            // <-- new: notify parent of the entity being edited
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
```

### 4.3 Verify edit button in template
In `generic-crud.component.html`, ensure the edit button calls `openEditModal(row)`:
```html
<button mat-icon-button (click)="openEditModal(row)" aria-label="Edit">
  <mat-icon>edit</mat-icon>
</button>
```
(This is already present; no change needed.)

## 5. Effects on Parent Components
All pages using the generic CRUD will now receive the `edit` event. They must handle it to store the editing entity. The `RoomManagementComponent` already has the handler, as per its spec:
```ts
onEdit(entity: Room): void { this.editingEntity.set(entity); }
```
No change needed there. Other CRUD pages (Staff, Amenities, Menu) will need similar handlers when they are built, but that will be captured in their specsheets.

## 6. Verification Checklist
- [ ] Clicking the edit button on a room row emits the `edit` event with the room object.
- [ ] The parent stores the entity, and the save handler correctly calls the PATCH endpoint with the room ID.
- [ ] The network request is `PATCH /api/v1/rooms/{id}` with the updated body.
- [ ] Creating a new room still works (no edit event emitted, so `editingEntity` is null → POST).
- [ ] No regression in sorting, filtering, pagination, or other CRUD actions.
- [ ] Other management pages (Room Types) continue to work; they must also handle the `edit` event if they use the generic CRUD for editing (they do). The Room Types page already has an `onEdit` handler that sets `editingEntity`. So it will automatically benefit.

## 7. Integration Note
After applying this patch, all existing and future CRUD pages will correctly receive the editing entity. The `edit` output is a pure generic addition that does not leak domain logic; it simply passes the row data back to the parent.

This fix resolves the incorrect API method call permanently.