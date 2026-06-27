# Patch Specsheet C: Feedback Page – UI Uniformity

## 1. Purpose

Replace the “Show hidden feedback” slide toggle with a dropdown selector aligned to the right side of the filter bar, matching the layout of all other oversight/management pages. No functional changes to filtering logic; only the UI control and its placement change.

## 2. Files to Modify

- `src/app/features/admin/pages/oversight/feedback.component.html`
- `src/app/features/admin/pages/oversight/feedback.component.ts`

## 3. Changes

### 3.1 Replace `mat-slide-toggle` with a `mat-select` dropdown

**Remove the toggle:**

```html
<mat-slide-toggle
  [formControl]="includeHiddenControl"
  (change)="onIncludeHiddenToggle()"
>
  Show hidden feedback
</mat-slide-toggle>
```

**Add a `mat-select` inside a `mat-form-field`. Place it after all other filter controls, visually aligned to the right side using flexbox.**

Exact new HTML to be inserted at the end of the `<div class="controls">`, before any “Clear” button or after the search filter if present (the feedback page currently only has the includeHidden toggle; after removal, the controls div will contain only this dropdown and any future search controls – but for now, we just add the dropdown):

```html
<span class="spacer"></span>
<mat-form-field appearance="outline">
  <mat-label>Visibility</mat-label>
  <mat-select
    [formControl]="includeHiddenControl"
    (selectionChange)="onIncludeHiddenToggle($event.value)"
  >
    <mat-option [value]="false">Visible only</mat-option>
    <mat-option [value]="true">All (including hidden)</mat-option>
  </mat-select>
</mat-form-field>
```

**Update the controls container CSS** to use flexbox and push the spacer to the right:

In `feedback.component.scss` (create if it doesn’t exist, or add to existing styles):

```css
.controls {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}
.spacer {
  flex: 1 1 auto;
}
```

### 3.2 TypeScript Changes – Update Event Handler

Modify the `onIncludeHiddenToggle` method to accept a `boolean` parameter (the selected value). The handler is called from the `(selectionChange)` event.

**Replace the existing method signature (if it takes no arguments) with:**

```ts
onIncludeHiddenToggle(value: boolean): void {
  // No need to read includeHiddenControl.value, use the passed value
  // The FormControl already has the new value; just trigger fetch
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}
```

The method no longer needs to read the control’s value; the passed argument ensures we react to the exact selection.

### 3.3 Import Cleanup – Safer Approach

- **Add** `MatSelectModule` and `MatFormFieldModule` to the `@Component.imports` array if they are not already present. (The feedback component likely has `MatFormFieldModule` from other inputs, but verify. `MatSelectModule` is new.)
- **Remove** `MatSlideToggleModule` from the imports array **only if** there are no remaining `<mat-slide-toggle>` elements in the template. After applying the template change, check the entire `feedback.component.html` for any occurrence of `mat-slide-toggle`. If none, remove the import.

Add this explicit instruction in the spec for the agent:

> After the template changes, search the file for `mat-slide-toggle`. If not found, remove `MatSlideToggleModule` from the `imports` array. Ensure `MatSelectModule` and `MatFormFieldModule` are present.

## 4. Responsive Behaviour

The flex container wraps on small screens; the dropdown will move to the next line. That’s acceptable.

## 5. Self‑Review Checklist (for the agent)

- [ ] The “Show hidden feedback” toggle is gone.
- [ ] A dropdown labeled “Visibility” appears on the right side of the filter bar, with options “Visible only” (value `false`) and “All (including hidden)” (value `true`).
- [ ] Changing the dropdown triggers `onIncludeHiddenToggle(true/false)` and immediately refetches data.
- [ ] The layout matches the pattern of other pages (controls left, spacer, filter dropdown right).
- [ ] `MatSlideToggleModule` is removed from imports if and only if no `mat-slide-toggle` remains in the template.
- [ ] `MatSelectModule` and `MatFormFieldModule` are added to imports if not already present.
- [ ] No console errors, no regressions.

## 6. Integration Notes

- This patch is purely cosmetic and does not change the backend API calls.
- The `includeHiddenControl` form control remains the same; only the presentation changes.
- No other files are affected.

