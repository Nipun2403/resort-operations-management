# Patch Specsheet: Menu Items – Category Validation Fix

### 1. Purpose

Prevent the category field from accepting purely numeric strings. If a category is provided (non‑empty), it must contain at least one letter. If the field is left empty, it is still considered valid (optional field). This matches the behaviour of the name field while keeping the category optional.

### 2. Files to Modify

- `src/app/features/admin/pages/management/menu-management.component.ts`

### 3. Changes

#### 3.1 Add a custom validator function

Inside the component file (before the class, or inside the class as a static method, but easiest is a standalone function outside the class) add:

```ts
/** Validator that requires at least one letter if a value is present */
function optionalLetterPattern(
  control: AbstractControl,
): ValidationErrors | null {
  const value = control.value as string;
  if (!value || value.trim().length === 0) {
    return null; // empty is valid
  }
  const regex = /^(?=.*[a-zA-Z])[a-zA-Z0-9\s\-']+$/;
  return regex.test(value) ? null : { pattern: true };
}
```

Place this function right after the imports, before `@Component(...)`.

#### 3.2 Update the category field in `crudConfig.formFields`

Locate the category field definition in the `formFields` array and replace its `validators` line:

**Before:**

```ts
validators: [Validators.maxLength(100)],
```

**After:**

```ts
validators: [Validators.maxLength(100), optionalLetterPattern],
```

That’s the only change needed. The field remains optional; if the user types something, it must now contain at least one letter.

### 4. Verification Checklist

- [ ] Leaving the category field empty is allowed (no error).
- [ ] Entering a purely numeric string like “123” shows a pattern error message.
- [ ] Entering a valid category like “Snacks” or “Drinks 2” is accepted.
- [ ] The existing name field still validates correctly (unchanged).
- [ ] Create and update operations still send the correct category value (non‑empty string or null).

### 5. Integration Notes

- Only the menu management component is modified.
- No changes to shared components or models.
- The `optionalLetterPattern` function is local to the file and does not affect any other page.

---

