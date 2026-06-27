# Patch Specsheet D: Room Type Management – Dynamic Bed Config & Image URLs + Strong Validation

## 1. Purpose
- Enhance the Room Type add/edit modal to allow users to build **bed configuration** (key‑value pairs) and **image URLs** (multiple strings) through dynamic user‑friendly inputs.
- Add a reusable `keyValueList` and `imageUrlList` field type to the generic CRUD modal, keeping it generic and reusable for any future entity.
- Enforce strict validation: no empty submissions allowed; every required field must pass validation before the modal can be saved.

## 2. Files to Modify

### Generic Components (shared)
| File | Change |
|------|--------|
| `src/app/shared/models/crud-config.model.ts` | Add `'keyValueList'` and `'imageUrlList'` to the `FormFieldDef.type` union. |
| `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts` | Implement rendering and form building for the new types. |
| `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.html` | Add the required templates for dynamic list controls. |

### Room Type Feature
| File | Change |
|------|--------|
| `src/app/features/admin/pages/management/room-type-management.component.ts` | Update `crudConfig.formFields` to use the new field types, and adjust the `onSave` handler to transform the form value into the DTO. |
| `src/app/features/admin/models/room-type.model.ts` | (No change – DTOs already expect `Record<string,number>` and `string[]`.) |

## 3. Changes to Generic Component (new field types)

### 3.1 Extend `FormFieldDef` type
In `crud-config.model.ts`, add the new allowed `type` values:
```ts
export interface FormFieldDef {
  // ... existing properties
  type: 'text' | 'number' | 'email' | 'password' | 'textarea' | 'date' | 'url' | 'select' | 'toggle' | 'keyValueList' | 'imageUrlList';
  // ...
}
```

### 3.2 Render new field types in `CrudModalComponent` template

Add these blocks inside the form, after the existing `@for (field of activeFields; ...)` loop:

**For `keyValueList`:**
```html
@if (field.type === 'keyValueList') {
  <div class="key-value-list" [formGroupName]="field.name">
    <label>{{ field.label }}</label>
    <div formArrayName="pairs">
      @for (pair of getKeyValueArray(field.name).controls; let i = $index; track i) {
        <div class="pair-row" [formGroupName]="i">
          <mat-form-field appearance="outline">
            <mat-label>Bed Type</mat-label>
            <input matInput formControlName="key" placeholder="e.g., King" />
            <mat-error *ngIf="pair.get('key')?.invalid && pair.get('key')?.touched">
              {{ getErrorMessage(field, pair.get('key')!) }}
            </mat-error>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Quantity</mat-label>
            <input matInput type="number" formControlName="value" min="1" />
            <mat-error *ngIf="pair.get('value')?.invalid && pair.get('value')?.touched">
              Quantity is required (min 1)
            </mat-error>
          </mat-form-field>
          <button mat-icon-button type="button" (click)="removeKeyValuePair(field.name, i)" aria-label="Remove bed type">
            <mat-icon>close</mat-icon>
          </button>
        </div>
      }
    </div>
    <button mat-button type="button" (click)="addKeyValuePair(field.name)"><mat-icon>add</mat-icon> Add bed type</button>
  </div>
}
```

**For `imageUrlList`:**
```html
@if (field.type === 'imageUrlList') {
  <div class="image-url-list" [formGroupName]="field.name">
    <label>{{ field.label }}</label>
    <div formArrayName="urls">
      @for (urlCtrl of getImageUrlArray(field.name).controls; let i = $index; track i) {
        <div class="url-row">
          <mat-form-field appearance="outline">
            <mat-label>Image URL</mat-label>
            <input matInput [formControl]="$any(urlCtrl)" placeholder="https://..." />
            <mat-error *ngIf="urlCtrl.invalid && urlCtrl.touched">
              Enter a valid URL
            </mat-error>
          </mat-form-field>
          <button mat-icon-button type="button" (click)="removeImageUrl(field.name, i)" aria-label="Remove URL">
            <mat-icon>close</mat-icon>
          </button>
        </div>
      }
    </div>
    <button mat-button type="button" (click)="addImageUrl(field.name)"><mat-icon>add</mat-icon> Add image URL</button>
  </div>
}
```

### 3.3 Form building in `CrudModalComponent` TypeScript

Add helper methods for dynamic form arrays:

```ts
// In class
getKeyValueArray(fieldName: string): FormArray {
  return this.form.get(fieldName + '.pairs') as FormArray;
}

getImageUrlArray(fieldName: string): FormArray {
  return this.form.get(fieldName + '.urls') as FormArray;
}

addKeyValuePair(fieldName: string): void {
  const pair = new FormGroup({
    key: new FormControl('', [Validators.required, Validators.pattern(/^[a-zA-Z0-9\s\-']+$/)]),
    value: new FormControl(1, [Validators.required, Validators.min(1)])
  });
  this.getKeyValueArray(fieldName).push(pair);
}

removeKeyValuePair(fieldName: string, index: number): void {
  this.getKeyValueArray(fieldName).removeAt(index);
}

addImageUrl(fieldName: string): void {
  this.getImageUrlArray(fieldName).push(new FormControl('', [Validators.pattern(/^https?:\/\/.+/)]));
}

removeImageUrl(fieldName: string, index: number): void {
  this.getImageUrlArray(fieldName).removeAt(index);
}
```

In the modal’s `ngOnInit` (or wherever the form is built), when processing `activeFields`, handle new types:

```ts
if (field.type === 'keyValueList') {
  const pairsArray = new FormArray([]);
  // If editing and entity has a bedConfiguration object, populate pairs
  const bedConfig = data.editMode ? data.entity?.bedConfiguration : null;
  if (bedConfig && typeof bedConfig === 'object') {
    Object.entries(bedConfig).forEach(([key, value]) => {
      pairsArray.push(new FormGroup({
        key: new FormControl(key, [Validators.required, Validators.pattern(/^[a-zA-Z0-9\s\-']+$/)]),
        value: new FormControl(value, [Validators.required, Validators.min(1)])
      }));
    });
  }
  // If creating and no pairs, optionally add one empty pair by default
  if (!data.editMode && pairsArray.length === 0) {
    // No default pair – user can add manually
  }
  this.form.addControl(field.name, new FormGroup({ pairs: pairsArray }));
}
else if (field.type === 'imageUrlList') {
  const urlsArray = new FormArray([]);
  const urls = data.editMode ? data.entity?.imageUrls : null;
  if (urls && Array.isArray(urls)) {
    urls.forEach(url => urlsArray.push(new FormControl(url, [Validators.pattern(/^https?:\/\/.+/)])));
  }
  this.form.addControl(field.name, new FormGroup({ urls: urlsArray }));
}
```

Before emitting the save event, convert the form value to the expected DTO shape (this happens inside the modal’s submit method). The modal must output the transformed value so that parent receives the correct `bedConfiguration` object and `imageUrls` array.

Add conversion logic in `submit()`:

```ts
const rawValue = this.form.getRawValue();
// Convert keyValueList fields
for (const field of this.activeFields) {
  if (field.type === 'keyValueList') {
    const pairs: { key: string; value: number }[] = rawValue[field.name]?.pairs ?? [];
    const obj: Record<string, number> = {};
    pairs.forEach(p => { if (p.key) obj[p.key] = p.value; });
    rawValue[field.name] = Object.keys(obj).length > 0 ? obj : null; // or empty object as per backend
  } else if (field.type === 'imageUrlList') {
    rawValue[field.name] = (rawValue[field.name]?.urls ?? []).filter(Boolean);
  }
}
this.dialogRef.close({ formValue: rawValue, isActive: ... });
```

This keeps the generic modal fully generic; no domain logic.

### 3.4 Styling
Add minimal CSS to the generic modal’s styles for `.pair-row` and `.url-row` to display flex with gap.

## 4. Changes to Room Type Management Page

### 4.1 Update `crudConfig.formFields`

Replace the old `bedConfiguration` and `imageUrls` field definitions with the new types:

**Remove** any existing field with `name: 'bedConfiguration'` or `name: 'imageUrls'`.

**Add:**
```ts
{
  name: 'bedConfiguration',
  label: 'Bed Configuration',
  type: 'keyValueList',
  validators: [], // overall field is optional, but each pair validated internally
  showInAdd: true,
  showInEdit: true
},
{
  name: 'imageUrls',
  label: 'Images',
  type: 'imageUrlList',
  validators: [],
  showInAdd: true,
  showInEdit: true
}
```

### 4.2 Adjust `onSave` handler (RoomTypeManagementComponent)

The parent now receives `bedConfiguration` as an object (already converted by the modal) and `imageUrls` as a string array. No further transformation needed, but ensure that the Create/Update DTOs receive these correctly:

```ts
const dto: CreateRoomTypeDTO = {
  ...formValue,
  bedConfiguration: formValue.bedConfiguration || null, // can be null if empty
  imageUrls: formValue.imageUrls ?? [],
};
```

This is exactly what the backend expects.

## 5. Strong Validation Enforcement
- All text fields in the modal already show specific error messages.
- For `keyValueList`, each pair’s key is required and must match pattern; value must be ≥1.
- For `imageUrlList`, each URL is optional but if entered must start with `http://` or `https://`.
- The generic modal’s `submit` method already marks all controls as touched and checks `form.invalid`; it will prevent submission if any validation fails.

## 6. Self‑Review Checklist (for the agent)
- [ ] In add/edit room type modal, a dynamic “Bed Configuration” section appears with an “Add bed type” button.
- [ ] Adding a bed type creates a row with “Bed Type” text input and “Quantity” number input.
- [ ] The “Image URLs” section shows an “Add image URL” button; each added URL is a text field with a remove button.
- [ ] Existing bed config and images are populated correctly when editing.
- [ ] Validation errors are displayed for each invalid field (e.g., empty bed type, negative quantity, invalid URL).
- [ ] On save, the modal outputs `bedConfiguration` as a JSON object and `imageUrls` as a string array.
- [ ] The parent component receives these values and creates/updates the room type successfully.
- [ ] The generic modal still works correctly for other entities (no regression).
- [ ] All other management pages are unaffected.

## 7. Integration Notes
- This patch enhances the shared generic modal with two new field types that are entirely generic and reusable.
- The Room Type page is the first consumer; future pages can use the same types if needed.
- No breaking changes to existing functionality.