# Specsheet: Shared Generic Components – Dark Theme Refactor

## 1. Purpose
- Restyle all shared, reusable components to match the “Obsidian & Champagne” design system defined in `design.md`.
- This ensures that every role’s dashboard, management table, and common dialogs adopt the dark luxury aesthetic without repeating styles.
- **Only HTML templates and SCSS files are modified.** All TypeScript logic, inputs/outputs, and service calls remain unchanged.

## 2. Components to Update
The following components are the core shared UI pieces used across Admin, Front Desk, Kitchen, Housekeeping, Maintenance, and Customer portals.

| Component | File(s) to modify |
|-----------|-------------------|
| `GenericCrudComponent` | `src/app/shared/components/generic-crud/generic-crud.component.html`<br>`src/app/shared/components/generic-crud/generic-crud.component.scss` |
| `CardsViewComponent` (used inside generic CRUD) | `src/app/shared/components/generic-crud/cards-view/cards-view.component.html`<br>`src/app/shared/components/generic-crud/cards-view/cards-view.component.scss` |
| `CrudModalComponent` (used by generic CRUD) | `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.html`<br>`src/app/shared/components/generic-crud/crud-modal/crud-modal.component.scss` |
| `TaskDashboardComponent` (used by kitchen/housekeeping/maintenance) | `src/app/shared/components/task-dashboard/task-dashboard.component.html`<br>`src/app/shared/components/task-dashboard/task-dashboard.component.scss` |
| `TaskDetailDialogComponent` (part of task dashboard) | `src/app/shared/components/task-dashboard/task-detail-dialog.component.html`<br>`src/app/shared/components/task-dashboard/task-detail-dialog.component.scss` |
| `ConfirmDialogComponent` (shared confirmation dialog) | `src/app/shared/components/confirm-dialog/confirm-dialog.component.html`<br>`src/app/shared/components/confirm-dialog/confirm-dialog.component.scss` |
| `AlertComponent` (shared inline alert) | `src/app/shared/components/alert/alert.component.html`<br>`src/app/shared/components/alert/alert.component.scss` |
| `NotificationSnackbarComponent` (SignalR notification toast) | `src/app/shared/components/notification-snackbar/notification-snackbar.component.html`<br>`src/app/shared/components/notification-snackbar/notification-snackbar.component.scss` |

## 3. Global Theme Integration
All SCSS files will import the global theme file:
```scss
@import '../../../../styles/theme/index';
```
This makes CSS custom properties (`--color-background`, `--color-secondary`, etc.) and mixins available.

## 4. Component‑by‑Component Changes

### 4.1 GenericCrudComponent
- **Background:** The page container should have `background: var(--color-background);`.
- **Table:** Use `table-layout: fixed;` (already present). Header cells should have `color: var(--color-on-surface-variant); font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.1em;`. Row cells use `font-size: 0.9rem;`.
- **Search/filter bar:** Input fields should be borderless bottom‑border style (`border-bottom: 1px solid rgba(228,194,133,0.4);`). Buttons follow the pill or outline style from the admin dashboard.
- **Pagination:** Material paginator will be overridden via deep CSS selectors or by setting the background via `::ng-deep` (if necessary). We'll add a global override in `styles.scss` for all paginators to match the dark theme.
- **Cards view (mobile):** Each card becomes a `glass-card` (dark semi‑transparent background, subtle gold border).

### 4.2 CardsViewComponent
- Use the `glass-card` class: `background: rgba(26,26,26,0.7); backdrop-filter: blur(10px); border: 1px solid rgba(228,194,133,0.2);`.
- Text inside cards uses the theme’s `body-md` mixin.

### 4.3 CrudModalComponent
- **Dialog background:** `background: var(--color-surface-container);`.
- **Title and content:** Gold‑coloured title, light body text.
- **Form fields:** Same minimal underline inputs as in the auth page.
- **Buttons:** Outline style with gold border and text, hover fills background.

### 4.4 TaskDashboardComponent
- **Summary cards:** Same glass‑card style as admin dashboard KPI cards.
- **Table:** Match the “Ledger” table style from the admin dashboard (thin gold separator lines, uppercase header labels).
- **Status chips:** Gold‑tinged backgrounds for Pending/InProgress/Completed.

### 4.5 TaskDetailDialogComponent
- Match the `CrudModalComponent` dialog style (dark background, gold heading).
- Action buttons: outline for Start/Complete.

### 4.6 ConfirmDialogComponent
- Already a simple dialog; wrap with dark background and gold‑accented title.
- Confirm button: gold fill; Cancel button: gold outline.

### 4.7 AlertComponent
- Keep existing functionality. Restyle with dark theme: error alerts use a dark red background with light red text; success alerts use dark green with light text.

### 4.8 NotificationSnackbarComponent
- Already uses a green background for notifications. Change to the Obsidian palette: success = dark green (`#1b5e20`) with gold border; error = dark red (`#b71c1c`) with gold border.

## 5. Global Overrides (Optional but Recommended)
To ensure consistency across all pages, add the following in `src/styles.scss`:

- **MatDialog:** `background: var(--color-surface-container);` for all dialogs.
- **MatSnackBar:** Override default snackbar background to dark.
- **MatPaginator:** Set background to transparent and text to light.
- **MatSort arrows:** Gold colour for active sort arrows.

Since these are global overrides, they can be added in the global stylesheet and will affect all components without further modification.

## 6. Integration Notes
- No TypeScript changes are needed. All bindings remain identical.
- The `GenericCrudComponent` already uses signals and outputs; those are untouched.
- After this refactor, every management page (admin’s CRUD pages, kitchen/housekeeping/maintenance dashboards) will automatically inherit the dark theme.
- The `AlertComponent` is used throughout the app; its styles are updated globally.

## 7. Self‑Review Checklist
- [ ] Generic CRUD tables now have dark backgrounds, gold‑highlighted headers, and thin separators.
- [ ] The CRUD modal has a dark glass‑panel look with minimal inputs.
- [ ] TaskDashboard cards and table match the Obsidian & Champagne look.
- [ ] ConfirmDialog and Alert are visually consistent with the theme.
- [ ] All role dashboards (admin, kitchen, etc.) that use these shared components now display correctly in dark mode.
- [ ] No console errors; existing interactive behaviour (sorting, filtering, pagination) works as before.
- [ ] No white backgrounds remain in any shared component.

---

