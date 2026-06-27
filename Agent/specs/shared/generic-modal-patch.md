# Patch Specsheet: Generic Modal – Validation Messages & Remove Duplicate Confirmation

### 1. Purpose

- Fix two bugs in the shared `CrudModalComponent` that affect all CRUD pages:
  1. Validation errors only show `"{field} is required"` regardless of the actual error (pattern, min, email, etc.).
  2. When `supportsToggle` is true, changing the toggle from active to inactive triggers a confirmation dialog **inside the modal** before emitting the save event, causing a duplicate confirmation when the parent page also handles deactivation confirmation.
- This patch is **generic** and does not introduce any domain‑specific logic.

### 2. Files to Modify

- `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts`
- `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.html`

### 3. Changes to `CrudModalComponent` (TypeScript)

**3.1 Add a validation message mapping method**

```ts
getErrorMessage(field: FormFieldDef, control: AbstractControl): string | null {
  if (!control.errors || !control.touched) return null;
  const errors = control.errors;
  if (errors['required']) {
    return `${field.label} is required.`;
  }
  if (errors['email'] || (field.type === 'email' && errors['pattern'])) {
    return 'Please enter a valid email address.';
  }
  if (errors['pattern']) {
    // Generic pattern error
    return `${field.label} contains invalid characters or format.`;
  }
  if (errors['min']) {
    return `${field.label} must be at least ${errors['min'].min}.`;
  }
  if (errors['max']) {
    return `${field.label} must be at most ${errors['max'].max}.`;
  }
  if (errors['minlength']) {
    return `${field.label} must be at least ${errors['minlength'].requiredLength} characters.`;
  }
  if (errors['maxlength']) {
    return `${field.label} must be at most ${errors['maxlength'].requiredLength} characters.`;
  }
  return `${field.label} is invalid.`;
}
```

**Note:** The `control` parameter is the specific `FormControl`. The method can be called from the template.

**3.2 Remove internal confirmation dialog**

- Locate any logic that opens a confirmation dialog when `isActive` changes from `true` to `false`. This is likely inside the `submit()` method or a separate method that checks the toggle.
- Delete that logic entirely. The modal must **never** show a confirmation dialog. It should simply emit the save event with the form value and `isActive`.

The `submit()` method should look like:

```ts
submit(): void {
  if (this.form.invalid) {
    this.form.markAllAsTouched();
    return;
  }
  const raw = this.form.getRawValue();
  const isActive = this.supportsToggle ? raw.isActive : true;
  this.dialogRef.close({
    formValue: raw,
    isActive,
    previousIsActive: this.data.editMode ? this.data.entity?.isActive : true,
  });
}
```

If there is currently an `if (isActive === false && previousIsActive === true)` check that opens a confirmation, remove it completely.

**3.3 Ensure search behavior contract is explicit in the generic component**

- This is not part of the modal, but to eliminate ambiguity, we add a comment/annotation in the `GenericCrudComponent` that defines the search debounce (300ms), emission on every change, empty string on clear, and no submit button. However, since we're patching the modal, we can leave the search contract to the existing generic CRUD spec. The staff spec sheet already references it; no further action here unless you want me to include it in this patch. I'll omit it to keep the patch focused on the modal.

### 4. Changes to `CrudModalComponent` (HTML Template)

**4.1 Update error messages**

Currently, each `mat-error` element might show something like:

```html
<mat-error
  *ngIf="form.get(field.name)?.invalid && form.get(field.name)?.touched"
>
  {{ field.label }} is required.
</mat-error>
```

Replace with:

```html
<mat-error
  *ngIf="form.get(field.name)?.invalid && form.get(field.name)?.touched"
>
  {{ getErrorMessage(field, form.get(field.name)!) }}
</mat-error>
```

**4.2 Remove any confirmation-related template elements**
If there's a section that shows a confirmation message or an extra dialog, remove it. The save button should directly call `submit()`.

### 5. Verification Checklist

- [ ] In add/edit modals, different validation errors (required, email, pattern, min) show appropriate, distinct messages.
- [ ] The error message for an invalid email is specific ("Please enter a valid email address.").
- [ ] Pattern errors show a generic but meaningful message.
- [ ] When disabling a staff member (edit modal), clicking "Save" immediately emits the save event without showing any confirmation dialog inside the modal.
- [ ] The parent's confirmation dialog for deactivation still works as before.
- [ ] No confirmation dialog appears for other entities (room types, rooms) where the parent might not handle it – but the generic modal no longer blocks the save, and the parent's `onSave` will receive the event; if the parent doesn't handle deactivation, the toggle change will just be applied immediately. That's acceptable for now (we can later add confirmation to those pages if needed).
- [ ] Existing CRUD pages (Room Types, Rooms) continue to work without regressions.

### 6. Integration Notes

- This patch affects all CRUD pages that use the generic modal. It does not change any page‑specific logic.
- No new domain properties or dependencies are added.
- The `showInAdd`/`showInEdit` logic remains unchanged.

### 7. File Structure

- Only modifications to `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts` and its HTML template.

---

