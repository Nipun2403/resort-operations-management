# Specsheet: Complete Staff UI – Dark Theme Overhaul (Exhaustive)

## 1. Purpose
- Apply the **“Obsidian & Champagne”** design system (from `design.md`) to **all staff‑facing user interfaces**: Admin, Front Desk, Kitchen, Housekeeping, Maintenance.
- This is done exclusively through **CSS and template changes** – **no TypeScript logic, API calls, services, or route configurations are modified**.
- Every visual element – sidebars, toolbars, tables, cards, modals, snackbars, pagination, sort arrows, and shared reusable components – will match the dark luxury aesthetic.
- The spec covers:
  - Global Material overrides
  - Role shell (sidebar + toolbar) styles
  - All shared components (Generic CRUD, Task Dashboard, dialogs, alerts, etc.)
  - Oversight page tables (Audit Logs, Billing & Receipts, Feedback)

## 2. Files to Modify (exhaustive list)

| Category | File | Change |
|----------|------|--------|
| **Global Styles** | `src/styles.scss` | Add Material overrides for dialogs, snackbar, paginator, sort arrows. |
| **Role Shells** | `src/app/features/admin/admin-shell.component.scss` | Already dark; ensure consistency. |
| | `src/app/features/front-desk/front-desk-shell.component.scss` | New dark styles. |
| | `src/app/features/kitchen/kitchen-shell.component.scss` | New dark styles. |
| | `src/app/features/housekeeping/housekeeping-shell.component.scss` | New dark styles. |
| | `src/app/features/maintenance/maintenance-shell.component.scss` | New dark styles. |
| | `src/app/features/user/user-shell.component.scss` | Already partially dark; update footer links colour. |
| **Shared Components** | `src/app/shared/components/generic-crud/generic-crud.component.html` | Add CSS classes; no structural changes. |
| | `src/app/shared/components/generic-crud/generic-crud.component.scss` | Full restyle. |
| | `src/app/shared/components/generic-crud/cards-view/cards-view.component.html` | Apply glass‑card classes. |
| | `src/app/shared/components/generic-crud/cards-view/cards-view.component.scss` | Full restyle. |
| | `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.html` | Replace Material inputs with custom minimal inputs; adjust button classes. |
| | `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.scss` | Full restyle. |
| | `src/app/shared/components/task-dashboard/task-dashboard.component.html` | Add glass‑card, ledger‑table classes. |
| | `src/app/shared/components/task-dashboard/task-dashboard.component.scss` | Full restyle. |
| | `src/app/shared/components/task-dashboard/task-detail-dialog.component.html` | Dark dialog styling. |
| | `src/app/shared/components/task-dashboard/task-detail-dialog.component.scss` | New styles. |
| | `src/app/shared/components/confirm-dialog/confirm-dialog.component.html` | Dark dialog. |
| | `src/app/shared/components/confirm-dialog/confirm-dialog.component.scss` | New styles. |
| | `src/app/shared/components/alert/alert.component.html` | Keep structure; style update. |
| | `src/app/shared/components/alert/alert.component.scss` | Dark theme colors. |
| | `src/app/shared/components/notification-snackbar/notification-snackbar.component.html` | Small tweak. |
| | `src/app/shared/components/notification-snackbar/notification-snackbar.component.scss` | Dark background. |
| **Oversight Pages** | `src/app/features/admin/pages/oversight/audit-logs.component.html` | Add container class. |
| | `src/app/features/admin/pages/oversight/audit-logs.component.scss` | Dark table. |
| | `src/app/features/admin/pages/oversight/billing-receipts.component.html` | Add container class. |
| | `src/app/features/admin/pages/oversight/billing-receipts.component.scss` | Dark tables. |
| | `src/app/features/admin/pages/oversight/feedback.component.html` | Add container class. |
| | `src/app/features/admin/pages/oversight/feedback.component.scss` | Dark table. |

## 3. Global Material Overrides (`src/styles.scss`)

Add the following at the end of `styles.scss` (after the theme imports):

```scss
// ── Material Overrides for Dark Theme ──────────

// Dialogs
.mat-mdc-dialog-container {
  --mdc-dialog-container-color: var(--color-surface-container);
  --mdc-dialog-subhead-color: var(--color-on-surface);
  --mdc-dialog-supporting-text-color: var(--color-on-surface-variant);
}

// Snackbar
.mat-mdc-snack-bar-container {
  --mdc-snackbar-container-color: var(--color-surface-container-high);
  --mdc-snackbar-supporting-text-color: var(--color-on-surface);
}

// Paginator
.mat-mdc-paginator {
  background: transparent;
  color: var(--color-on-surface-variant);
}
.mat-mdc-paginator .mat-mdc-icon-button {
  color: var(--color-on-surface-variant);
}

// Sort arrows
.mat-sort-header-arrow {
  color: var(--color-secondary) !important;
}

// Input fields (global minimal look) – override Material's default
.mat-mdc-text-field-wrapper {
  background: transparent !important;
  border-radius: 0 !important;
  border-bottom: 1px solid rgba(228, 194, 133, 0.4) !important;
}
.mat-mdc-form-field-focus-overlay { display: none; }
.mdc-line-ripple { display: none; }
```

*Note:* The input field override is global; all Material form fields in staff pages will become minimal border‑bottom inputs. This matches the design and is safe because we already restyled auth and other public pages similarly. It may affect admin dashboards, but that is desired.

## 4. Role Shells – Dark Theme

All role shells share the same structure: a `mat-sidenav-container` with a sidebar and a toolbar. The following SCSS block must be **copied into the respective shell’s `.scss` file** (replace the existing contents). Replace the role name placeholder with the appropriate value for each role.

**File:** `admin-shell.component.scss` (already done; ensure it matches this block).  
**Files:** `front-desk-shell.component.scss`, `kitchen-shell.component.scss`, `housekeeping-shell.component.scss`, `maintenance-shell.component.scss`.

```scss
@import '../../../../styles/theme/index';

:host {
  --mdc-sidenav-container-background-color: var(--color-background);
}

// Sidebar
mat-sidenav {
  background: var(--color-surface-container-low) !important;
  border-right: 1px solid rgba(228, 194, 133, 0.15) !important;
  .mat-toolbar {
    background: var(--color-background) !important;
    color: var(--color-secondary) !important;
  }
  .mat-nav-list a {
    color: var(--color-on-surface-variant) !important;
    &.active {
      color: var(--color-secondary) !important;
      background: rgba(228, 194, 133, 0.05) !important;
    }
    &:hover { color: var(--color-secondary) !important; }
  }
  .mat-divider { border-color: rgba(228, 194, 133, 0.15) !important; }
}

// Toolbar
mat-toolbar {
  background: var(--color-surface-container-low) !important;
  color: var(--color-on-surface) !important;
  border-bottom: 1px solid rgba(228, 194, 133, 0.15) !important;
}

// Content
.mat-sidenav-content {
  background: var(--color-background) !important;
  color: var(--color-on-background);
}

// User menu (if present)
.mdc-menu-surface {
  background: var(--color-surface-container-high) !important;
  color: var(--color-on-surface) !important;
}
```

For the **Customer shell** (`user-shell.component.scss`), we need to update the footer links to match the public footer style. We can add the same footer styles as in the public shell. However, the customer shell already uses a similar footer; ensure the link colours use `var(--color-on-tertiary-container)` and hover to `var(--color-secondary)`.

## 5. Shared Components – Exhaustive Restyle

### 5.1 GenericCrudComponent

**Template changes (`generic-crud.component.html`):**  
- Add `class="crud-container dark-theme"` to the outer div.  
- Wrap the table in a div with class `table-scroll`.  
- Add `glass-card` class to the top‑bar and filter bar containers.  
- Replace Material form field appearances with `appearance="fill"` and add `floatLabel="never"`? Actually we'll keep the existing Material fields but rely on the global override to style them as minimal inputs. No template change needed for inputs.  
- Ensure the empty‑state image uses a dark version or is omitted; the existing placeholder is fine.  
- Add `class="ledger-table"` to the `<table>` element.

**SCSS (`generic-crud.component.scss`):**  

```scss
@import '../../../../styles/theme/index';

.crud-container {
  background: var(--color-background);
  padding: 2rem;
  @media (max-width: 768px) { padding: 1rem; }
}

.top-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  h2 {
    @include font-headline-sm;
    color: var(--color-secondary);
    letter-spacing: 0.2em;
    text-transform: uppercase;
  }
  button {
    border: 1px solid var(--color-secondary);
    background: transparent;
    color: var(--color-secondary);
    @include font-label-caps;
    padding: 0.5rem 1.5rem;
    cursor: pointer;
    transition: background 0.5s, color 0.5s;
    &:hover { background: var(--color-secondary); color: var(--color-on-secondary); }
  }
}

.search-filter-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  align-items: flex-end;
  margin-bottom: 1.5rem;
  mat-form-field {
    // The global override already makes it minimal; we can set width
    flex: 1 1 200px;
    .mat-mdc-text-field-wrapper {
      padding: 0;
    }
  }
  button {
    @include font-label-caps;
    border: none;
    background: transparent;
    color: var(--color-secondary);
    cursor: pointer;
    &:hover { text-decoration: underline; }
  }
}

.table-scroll {
  overflow-x: auto;
}

.ledger-table {
  width: 100%;
  border-collapse: collapse;
  th {
    @include font-label-caps;
    font-size: 0.75rem;
    color: var(--color-on-surface-variant);
    letter-spacing: 0.15em;
    padding: 0.75rem 1rem;
    text-align: left;
    border-bottom: 1px solid rgba(228, 194, 133, 0.1);
    background: rgba(26,26,26,0.4);
  }
  td {
    padding: 0.75rem 1rem;
    border-bottom: 1px solid rgba(228, 194, 133, 0.05);
    @include font-body-md;
    color: var(--color-on-surface);
  }
  tr:hover td { background: rgba(228, 194, 133, 0.03); }
}

// Paginator override already global; but we can add to the component's styles if needed.

// Loading / Error / Empty states
.loading, .empty-state {
  text-align: center;
  padding: 4rem 0;
  color: var(--color-on-surface-variant);
  @include font-body-md;
}
```

### 5.2 CardsViewComponent

**Template:** Wrap each card in a div with class `glass-card card-item`. No other changes.

**SCSS:**  

```scss
@import '../../../../styles/theme/index';

.glass-card {
  background: rgba(26, 26, 26, 0.7);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(228, 194, 133, 0.15);
  padding: 1rem;
  margin-bottom: 1rem;
  cursor: pointer;
  transition: transform 0.3s;
  &:hover { transform: translateY(-4px); }
}
.card-content {
  p, span {
    color: var(--color-on-surface);
    @include font-body-md;
  }
}
```

### 5.3 CrudModalComponent

**Template changes:**  
- Replace all Material `<mat-form-field appearance="outline">` with minimal input wrappers using the same technique as in the auth forms. We'll create a reusable pattern:  
  ```html
  <div class="input-group">
    <input class="minimal-input" type="text" [formControlName]="field.name" placeholder=" " />
    <label class="floating-label">{{ field.label }}</label>
    <span class="error-text" *ngIf="...">...</span>
  </div>
  ```  
  But to keep it deterministic and not require rewriting the entire dynamic form logic, we can rely on the global Material overrides to convert the existing fields to minimal style. The existing `mat-form-field` with `appearance="fill"` and the overridden CSS will already look minimal. The dialog already uses `mat-form-field`; after the global override, they become border‑bottom inputs. So we don't need to change the HTML, only the SCSS.

- For the dialog container, add `class="dark-dialog"` to the title and content.

**SCSS:**  

```scss
@import '../../../../styles/theme/index';

.dark-dialog {
  background: var(--color-surface-container);
  color: var(--color-on-surface);
  .mat-mdc-dialog-title {
    color: var(--color-secondary);
    @include font-headline-sm;
  }
  .mat-mdc-dialog-content {
    color: var(--color-on-surface);
    .input-group {
      margin-bottom: 1.5rem;
      .minimal-input { ... } // not needed with global override
    }
  }
  .actions {
    button {
      border: 1px solid var(--color-secondary);
      color: var(--color-secondary);
      background: transparent;
      @include font-label-caps;
      padding: 0.5rem 1.5rem;
      &:hover {
        background: var(--color-secondary);
        color: var(--color-on-secondary);
      }
    }
  }
}
```

*Note:* The global Material overrides will already handle the form fields, so the modal SCSS only needs to set background, title color, and button styles.

### 5.4 TaskDashboardComponent

**Template:**  
- Wrap the entire dashboard with `class="task-dashboard dark-theme"`.  
- Summary cards: use `class="summary-card glass-card"`.  
- Status filter dropdown: keep Material select; global overrides will style it.  
- Table: add `class="ledger-table"`.  
- The modal already uses `TaskDetailDialogComponent`, which we'll update separately.

**SCSS:**  

```scss
@import '../../../../styles/theme/index';

.task-dashboard {
  background: var(--color-background);
  padding: 2rem;
}

.summary-row {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin-bottom: 2rem;
}
.summary-card {
  flex: 1 1 200px;
  padding: 1.5rem;
  cursor: pointer;
  text-align: center;
  .card-label {
    @include font-label-caps;
    color: var(--color-on-surface-variant);
    margin-bottom: 0.5rem;
  }
  .card-count {
    @include font-display-lg-mobile;
    color: var(--color-secondary);
    font-size: 2.5rem;
  }
}

.filter-bar {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
  mat-form-field { flex: 0 1 200px; }
}

.ledger-table { /* same as in GenericCrud */ }

// Group indicator, empty state etc. remain similar.
```

### 5.5 TaskDetailDialogComponent

**Template:** Dark dialog wrapper; content uses `p` with styling. Buttons: “Start” / “Complete” as outline gold.

**SCSS:**  

```scss
@import '../../../../styles/theme/index';

:host {
  background: var(--color-surface-container);
  color: var(--color-on-surface);
  display: block;
  padding: 1.5rem;
  h2 { color: var(--color-secondary); }
  p { margin: 0.5rem 0; }
  .actions {
    margin-top: 1.5rem;
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    button {
      border: 1px solid var(--color-secondary);
      color: var(--color-secondary);
      background: transparent;
      @include font-label-caps;
      padding: 0.5rem 1rem;
      &:hover {
        background: var(--color-secondary);
        color: var(--color-on-secondary);
      }
    }
  }
}
```

### 5.6 ConfirmDialogComponent

**Template:** Already simple; just wrap in dark theme.

**SCSS:**  

```scss
@import '../../../../styles/theme/index';

:host {
  background: var(--color-surface-container);
  color: var(--color-on-surface);
  display: block;
  padding: 1.5rem;
  h2 { color: var(--color-secondary); }
  p { margin: 1rem 0; }
  .actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    button {
      border: 1px solid var(--color-secondary);
      background: transparent;
      color: var(--color-secondary);
      @include font-label-caps;
      padding: 0.5rem 1rem;
      &:hover {
        background: var(--color-secondary);
        color: var(--color-on-secondary);
      }
    }
  }
}
```

### 5.7 AlertComponent

**Template:** Keep as is; the alert has a `type` class. Adjust colors.

**SCSS:**  

```scss
@import '../../../../styles/theme/index';

:host {
  display: block;
  padding: 1rem;
  border-left: 4px solid;
  &.error {
    background: rgba(255, 180, 171, 0.1);
    border-color: var(--color-error);
    color: var(--color-error);
  }
  &.success {
    background: rgba(200, 230, 201, 0.1);
    border-color: var(--color-secondary);
    color: var(--color-secondary);
  }
  .close-btn {
    background: none;
    border: none;
    color: inherit;
    cursor: pointer;
    float: right;
    margin-left: 1rem;
    font-size: 1.2rem;
  }
}
```

### 5.8 NotificationSnackbarComponent

**Template:** The existing green background; change to dark.

**SCSS:**  

```scss
@import '../../../../styles/theme/index';

.notification-container {
  background: var(--color-surface-container-high);
  border: 1px solid var(--color-secondary);
  color: var(--color-on-surface);
  padding: 1rem;
  .icon-container { color: var(--color-secondary); }
  .close-btn { color: var(--color-on-surface-variant); }
}
```

## 6. Oversight Pages (Audit Logs, Billing & Receipts, Feedback)

These pages have their own tables. We'll add a container class and apply ledger‑table styles.

For each page, in their HTML template, wrap the table section with `<div class="oversight-page">` and add the `ledger-table` class to the `<table>`.

In each page's SCSS, add:

```scss
@import '../../../../styles/theme/index';

.oversight-page {
  background: var(--color-background);
  padding: 2rem;
  h2 { color: var(--color-secondary); @include font-headline-sm; }
  .controls {
    display: flex;
    gap: 1rem;
    margin-bottom: 1.5rem;
    mat-form-field { flex: 1 1 200px; }
  }
  .ledger-table {
    // reuse same table styles as GenericCrud
    width: 100%;
    border-collapse: collapse;
    th {
      @include font-label-caps;
      font-size: 0.75rem;
      color: var(--color-on-surface-variant);
      letter-spacing: 0.15em;
      padding: 0.75rem 1rem;
      text-align: left;
      border-bottom: 1px solid rgba(228, 194, 133, 0.1);
      background: rgba(26,26,26,0.4);
    }
    td {
      padding: 0.75rem 1rem;
      border-bottom: 1px solid rgba(228, 194, 133, 0.05);
      @include font-body-md;
      color: var(--color-on-surface);
    }
  }
}
```

## 7. Self‑Review Checklist

- [ ] All role shells have dark sidebars, toolbars, and content areas.
- [ ] Global Material overrides make dialogs, snackbars, paginators, and form fields dark.
- [ ] Generic CRUD tables and cards use glass‑morphic dark styling with gold accents.
- [ ] Task Dashboard displays summary cards and table in dark theme.
- [ ] All shared dialogs (Confirm, Alert, Notification) have dark backgrounds and gold accents.
- [ ] Oversight pages (Audit Logs, Billing, Feedback) tables are restyled.
- [ ] No white backgrounds remain in any staff interface.
- [ ] All existing functionality (CRUD operations, sorting, filtering, pagination, navigation) works unchanged.
- [ ] No console errors, no broken styles.

---

