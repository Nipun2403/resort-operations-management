# Global Design Language Audit & Remediation Plan

**Design System:** "Obsidian & Champagne" (Aetheris)  
**Token Reference:** `src/styles/theme/`  
**Core Palette:** Background `#131411`, Surface `#131411/1f201d`, On-Surface `#e4e2dd`, Secondary/Gold `#e4c285`, Error `#ffb4ab`  
**Fonts:** Playfair Display (headlines), Manrope (body/labels)

---

## Audit Results — Components NOT Aligned to Design Language

The following is a complete, exhaustive inventory of every file with styling violations, grouped by severity and domain.

---

## 🔴 Priority 1 — Shared / Cross-Cutting Components (Affect All Features)

These are used app-wide. Fixing them fixes ALL screens simultaneously.

---

### 1. Alert Component (`app-alert`)

**File:** [alert.component.ts](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/shared/components/alert/alert.component.ts) (inline styles)

**Violations:**
- `success` state: `background #e8f5e9`, `color #2e7d32`, `border #c8e6c9` — bright green, completely outside Obsidian palette.
- `error` state: `background #ffebee`, `color #c62828`, `border #ffcdd2` — bright red-white, clashes with dark theme.
- `border-radius: 4px` is correct but font-size `14px` should use `@include font-label-caps` or `@include font-body-md`.
- No dark glassmorphic surface treatment.

**Required Refactoring:**
- Extract inline styles to a separate `.scss` file.
- `error` type → `background: rgba(255, 180, 171, 0.08)`, `border: 1px solid var(--color-error)`, `color: var(--color-error)`.
- `success` type → `background: rgba(228, 194, 133, 0.08)`, `border: 1px solid var(--color-secondary)`, `color: var(--color-secondary)`.
- Apply `@include font-body-md` to message text.

---

### 2. Notification Snackbar Component

**File:** [notification-snackbar.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/shared/components/notification-snackbar/notification-snackbar.component.scss)

**Violations:**
- `background-color: #2e7d32` — hardcoded bright green, entirely outside the design system.
- `color: white` — must use `var(--color-on-surface)` or `var(--color-secondary)`.
- `border-radius: 8px` — design uses `4px` (no rounding in Aetheris).
- Font sizes are `0.95rem`/`0.85rem` — must use theme typography mixins.
- No glassmorphic panel treatment.

**Required Refactoring:**
- Restructure into a Aetheris-styled toast panel: glassmorphic surface, gold border `var(--glass-border)`, dark background.
- Type-aware coloring: success → gold `var(--color-secondary)`, error → `var(--color-error)`, info → `var(--color-on-surface-variant)`.
- Apply `@include font-label-caps` for title, `@include font-body-md` for message body.
- `border-radius: 0` or `4px` max.

---

### 3. Confirm Dialog Component

**File:** [confirm-dialog.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/shared/components/confirm-dialog/confirm-dialog.component.scss)

**Violations:**
- `color: rgba(0, 0, 0, 0.8)` — hardcoded black text on a dark surface; completely invisible.
- No dialog surface styling (relies entirely on Material defaults which are light-themed).
- `gap: 8px` on actions is acceptable but buttons lack Aetheris styling.
- No `@import` of theme — cannot use design tokens.
- Dialog container itself (Material overlay) uses white by default — no dark glassmorphic override.

**Required Refactoring:**
- Import theme.
- Override `.mat-mdc-dialog-container` with dark surface `var(--color-surface-container)`.
- Set text to `var(--color-on-surface)`.
- Style dialog title with `@include font-headline-sm` and gold color `var(--color-secondary)`.
- Style Cancel/Confirm buttons to match Aetheris (ghost + filled patterns).
- Add `border: 1px solid var(--glass-border)`.

---

### 4. Profile Component

**File:** [profile.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/shared/components/profile/profile.component.scss)

**Violations:**
- Nearly empty — zero design language applied.
- `mat-card` will render with Material light-theme defaults (white background, grey borders).
- No type tokens, no surface tokens, no typography mixins.

**Required Refactoring:**
- Style the `.profile-card` as a glassmorphic panel `@include glass-panel`.
- Apply `var(--color-surface-container)` background.
- Apply `var(--color-on-surface)` to all text.
- Style form fields using same approach as `crud-modal.component.scss` (bottom-border only, dark inputs).
- Action buttons aligned to Aetheris pattern.

---

### 5. Task Dashboard Component

**File:** [task-dashboard.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/shared/components/task-dashboard/task-dashboard.component.scss)

**Violations:**
- Summary cards: `border-color: var(--mdc-theme-primary, #3f51b5)` — Material blue fallback, wrong palette.
- Active card: `background-color: rgba(63, 81, 181, 0.04)` — Material indigo, wrong.
- Card titles: `color: rgba(0, 0, 0, 0.6)` — black on dark background.
- Count: `color: rgba(0, 0, 0, 0.87)` — black on dark background.
- Table border: `border: 1px solid rgba(0, 0, 0, 0.08)` — barely visible on dark surface.
- Hover: `background-color: rgba(0, 0, 0, 0.03)` — invisible on dark.
- Status chips: ALL use light green/blue/orange/red with black text — clashes with dark theme entirely.
- Empty state: `background-color: #fafafa` — white block on dark background.

**Required Refactoring:**
- Summary cards → `@include glass-panel`, border `var(--glass-border)`.
- Active card border → `var(--color-secondary)`, background → `rgba(228, 194, 133, 0.08)`.
- All text → `var(--color-on-surface)` / `var(--color-on-surface-variant)`.
- Status chips → dark surface with colored border and appropriately themed text:
  - `Pending` → border/text `#f59e0b` (amber)
  - `InProgress`/`Preparing` → border/text `var(--color-secondary)` (gold)
  - `Completed`/`Delivered` → border/text with a muted green compatible with dark theme
- Table headers → `@include font-label-caps`, `var(--color-on-surface-variant)`.
- Empty state → glass panel with dashed gold border.

---

## 🟠 Priority 2 — Admin Domain (Internal Staff Dashboards)

---

### 6. Admin Oversight: Analytics Page

**File:** [analytics.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/admin/pages/oversight/analytics.component.scss)

**Violations:**
- No `@import` of theme — uses no design tokens.
- Material default `mat-card` components render white in light theme.
- `mat-form-field` inputs use default Material light styling (white background, grey outlines).
- Chart cards have no glassmorphic treatment.
- Date controls have no Aetheris styling.
- Text colors inherited from Material defaults (near-black on dark background — potentially invisible).

**Required Refactoring:**
- Add `@import '../../../../../styles/theme/index'`.
- Override `mat-card` to use glassmorphic dark panel.
- Override `mat-form-field` with bottom-border-only dark style.
- Apply typographic tokens to all headers and labels.
- Style chart card titles with `@include font-label-caps` and gold accent.

---

### 7. Admin Oversight: Audit Logs Page

**File:** [audit-logs.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/admin/pages/oversight/audit-logs.component.scss)

**Violations:**
- `mat-card` uses Material light default.
- `mat-card-subtitle: color: rgba(0, 0, 0, 0.54)` — black on dark.
- Mobile card view: `border: 1px solid rgba(0, 0, 0, 0.08)` — invisible on dark.
- `box-shadow: 0 4px 8px rgba(0, 0, 0, 0.08)` — barely visible on dark surfaces.
- No theme tokens used anywhere.

**Required Refactoring:**
- Same pattern as analytics — add theme import, glassmorphic panels, gold borders, light text tokens.

---

### 8. Admin Oversight: Billing & Receipts Page

**File:** [billing-receipts.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/admin/pages/oversight/billing-receipts.component.scss)

**Violations (most severe in admin oversight):**
- `table { background: #ffffff }` — white table on a dark page.
- `th { color: #424242 }` — dark grey text on white background.
- Hover: `background-color: #f5f5f5` — light grey.
- Status chips: `Booked #e3f2fd / #1565c0`, `CheckedIn #e8f5e9 / #2e7d32`, `CheckedOut #eceff1 / #37474f`, `Cancelled #ffebee / #c62828` — ALL light-theme palette.
- Empty state: `background: #fdfdfd`, `border: 1px dashed #ccc`, `color: #666` — entirely light theme.
- Mobile cards: light borders, light box shadows.

**Required Refactoring:**
- Full table reskin: dark glassmorphic surface, champagne-gold headers, gold-on-dark hover.
- Status chips → dark-surface badges with colored borders matching Aetheris error/secondary/outline palette.
- Empty state → glassmorphic panel with dashed gold border, `var(--color-on-surface-variant)` text.

---

### 9. Admin Oversight: Feedback Page

**File:** [feedback.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/admin/pages/oversight/feedback.component.scss)

**Violations:**
- `table { background: #ffffff }` — white table.
- `th { color: #424242 }` — near-black text.
- `tr:hover { background-color: #f9f9f9 }` — light hover.
- Mobile cards: `border: 1px solid rgba(0, 0, 0, 0.08)`, light subtitle text.
- Empty state: `background #fdfdfd`, `border #ccc`, `color: #666`.

**Required Refactoring:**
- Same reskin as billing-receipts.

---

### 10. Admin Management: Amenities / Menu / Staff Pages

**Files:**
- [amenities-management.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/admin/pages/management/amenities-management.component.scss)
- [menu-management.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/admin/pages/management/menu-management.component.scss)
- [staff-management.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/admin/pages/management/staff-management.component.scss)

**Violations:**
- All three files are **completely empty** (comment-only stubs).
- These pages render using `app-generic-crud` which already has Aetheris styling, BUT the outer page shell (header, page title, add button) relies on the generic CRUD's `top-bar` div — this needs the same outer layout treatment applied to room-type-management (custom header, `hideHeader: true`).

**Required Refactoring:**
- Apply same wrapper pattern as `room-type-management.component.html/scss` to each:
  - Custom styled page header with Playfair title + Manrope subtitle.
  - External gold "ADD" button linked to `#crud` ref.
  - `hideHeader: true` + `modalTitle`/`modalSubtitle` in their respective `crudConfig`.
- Add `@import` and layout SCSS to each file.

---

### 11. Create Internal Ticket Dialog

**File:** [create-internal-ticket-dialog.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/admin/components/create-internal-ticket-dialog.component.scss)

**Violations:**
- `border-bottom: 1px solid #e0e0e0` — Material light grey, invisible on dark.
- `.dialog-error { background: #ffebee; color: #c62828; border: #ffcdd2 }` — light error styling.
- `.field-label { color: rgba(0, 0, 0, 0.6) }` — near-black label on dark dialog.
- `mat-dialog-actions border-top: 1px solid #e0e0e0` — near-invisible.
- No theme import.

**Required Refactoring:**
- Add `@import` of theme.
- Dialog surface → `var(--color-surface-container)`, `border: 1px solid var(--glass-border)`.
- Error block → `rgba(255, 180, 171, 0.08)` background, `var(--color-error)` text/border.
- All labels → `var(--color-on-surface-variant)`.
- Dividers → `rgba(228, 194, 133, 0.15)`.
- Buttons → Aetheris pattern.

---

## 🟠 Priority 3 — Front-Desk Domain

---

### 12. Front-Desk Shell

**File:** [front-desk-shell.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/front-desk/front-desk-shell.component.scss)

**Violations:**
- `mat-sidenav { border-right: 1px solid rgba(0, 0, 0, 0.12) }` — black border, invisible on dark.
- No background color overrides — Material defaults to white.
- `mat-toolbar` renders Material default blue/grey toolbar.
- No navigation link styling (Aetheris gold hover/active states).
- Completely blank theme tokens.

**Required Refactoring:**
- Dark sidenav background (`var(--color-surface)`).
- Gold border divider.
- Nav links: `var(--color-on-surface-variant)` idle, `var(--color-secondary)` active/hover.
- Toolbar → dark background with gold text.

---

### 13. Front-Desk Dashboard

**File:** [front-desk/pages/dashboard.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/front-desk/pages/dashboard.component.scss)

**Violations:**
- Summary cards: `border: 1px solid rgba(0, 0, 0, 0.08)`, light shadow — invisible on dark surface.
- `mat-card` renders white by default.
- Hover: `box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15)` — barely visible on dark.
- No theme tokens.

**Required Refactoring:**
- Cards → `@include glass-panel`.
- Hover → `box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4)` + slight gold border glow.
- Text → Aetheris typography tokens.

---

### 14. Guest Details Page

**File:** [guest-details.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/front-desk/pages/guest-details.component.scss)

**Violations:**
- `.booking-item { border: 1px solid #ddd }` — light grey, invisible on dark.
- No theme tokens.

---

### 15. New Booking Page

**File:** [new-booking.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/front-desk/pages/new-booking.component.scss)

**Violations:**
- Default Material `mat-form-field` styling — light theme.
- No dark surface treatment.
- No theme tokens.

---

### 16. Booking Action Modal

**File:** [booking-action-modal.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/front-desk/components/booking-action-modal/booking-action-modal.component.scss)

**Violations:**
- `.room-card { border: 1px solid #ddd; background: #fafafa }` — white card on dark modal.
- No glassmorphic treatment.
- No theme tokens.

---

### 17. Checkout Dialog

**File:** [checkout-dialog.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/front-desk/components/booking-action-modal/checkout-dialog/checkout-dialog.component.scss)

**Violations:**
- `.confirmation p { color: #2e7d32 }` — Material green, outside Aetheris palette.

---

### 18. Guest Billing Component

**File:** [guest-billing.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/front-desk/components/guest-billing/guest-billing.component.scss)

**Violations:**
- `.Paid { color: #2e7d32; background: #e8f5e9 }` — light green status badge.
- `.Pending { color: #c62828; background: #ffebee }` — light red status badge.
- Both are light-theme chips incompatible with dark surfaces.

---

### 19. Ticket List Component

**File:** [ticket-list.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/front-desk/components/ticket-list/ticket-list.component.scss)

**Violations:**
- `.mat-mdc-header-cell { font-weight: bold }` — generic override, no Aetheris label styling.
- No dark table surface treatment.

---

## 🟡 Priority 4 — Housekeeping / Kitchen / Maintenance Shells

---

### 20–22. Domain Shell Components (Identical violations)

**Files:**
- [housekeeping-shell.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/housekeeping/housekeeping-shell.component.scss)
- [kitchen-shell.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/kitchen/kitchen-shell.component.scss)
- [maintenance-shell.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/maintenance/maintenance-shell.component.scss)

**Violations (all three are identical copies):**
- `mat-sidenav { border-right: 1px solid rgba(0, 0, 0, 0.12) }` — light border.
- `mat-toolbar` renders Material blue/grey.
- No dark background treatment for sidenav content.
- No navigation link styling.
- Identical to front-desk-shell violations.

**Refactoring Strategy:**
- Create a shared Aetheris shell styling pattern (mixin or base class) applied identically to all three.

---

### 23. Kitchen Menu Items Page

**File:** [menu-items.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/kitchen/pages/menu-items.component.scss)

**Violations:**
- Generic `mat-form-field` with default Material styling.
- No dark treatment for menu grid cards.
- No theme tokens.

---

## 🟡 Priority 5 — User Domain Components

---

### 24. Food Order Component

**File:** [food-order.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/user/components/food-order/food-order.component.scss)

**Violations:**
- No `@import` of theme tokens.
- Multiple sections lack dark surface treatments.
- Typography likely inherits from global body but uses no explicit Aetheris mixins.

> **Note:** `cart-drawer.component.scss` is **correctly self-declaring** CSS variables in `:host`. However, it should use `@import` of the shared theme instead of re-declaring variables — this is a maintenance risk (tokens can drift).

---

### 25. Request Service / My Requests Components

**Files:**
- [request-service.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/user/components/request-service/request-service.component.scss)
- [my-requests.component.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/app/features/user/components/my-requests/my-requests.component.scss)

**Violations:**
- Both correctly self-declare CSS variables in `:host` — same maintenance risk as cart-drawer.
- `request-service`: The `.request-card` uses `border-radius: 4px` (correct) but likely renders on a Material card with light background.
- `my-requests`: Table is glassmorphic (`rgba(26,26,26,0.5)`, gold border) — **partially aligned**, but:
  - Status chips / status colors may still use Material palette.
  - Table header typography not using `@include font-label-caps`.

---

## 🔵 Risk Flags & Cross-Cutting Issues

---

### RF-1: CSS Variable Re-Declaration Anti-Pattern

**Affected Files:**
- `cart-drawer.component.scss`
- `request-service.component.scss`
- `my-requests.component.scss`
- `room-service.component.scss`

These files re-declare all CSS variables in `:host { --color-secondary: #e4c285; ... }` instead of using `@import '../../../styles/theme/index'`. If a token value ever changes globally, these files will **silently drift** out of sync.

**Refactoring Required:** Replace `:host { --color-xxx }` blocks with `@import` of the theme index and use `var(--color-xxx)` directly.

---

### RF-2: Material Snackbar Global Override

**File:** [styles.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/styles.scss) (line ~102)

The global `.notification-snackbar` class sets `background: transparent` on the MDC snackbar surface. This is intentional but relies on the custom component always rendering the correct themed container. Ensure the snackbar notification types (success/error/info/warning) each apply distinct gold-vs-red semantic colors.

---

### RF-3: Material Theme Contamination — No Global mat-card / mat-select Dark Override

**File:** [styles.scss](file:///Users/peewee/personal/repos/Hotel_Management_Full/Frontend/src/styles.scss)

There is currently **no global override for `mat-card`, `mat-select`, `mat-option`, `mat-menu`, `mat-tab`** to enforce dark backgrounds. These components all render with Material light-theme defaults unless explicitly overridden per-component. A global `::ng-deep` or theme override in `styles.scss` would normalize all instances across the entire application.

---

### RF-4: Status Chip Inconsistency

Multiple components define their own status chip colors (task-dashboard, billing-receipts, feedback, audit-logs, guest-billing) using light-theme palette. There is no shared `status-chip` global style in `styles.scss` or a shared component.

**Recommendation:** Introduce a global `.status-chip` utility class in `styles.scss` with Aetheris-compatible state variants:
```scss
.status-chip { ... }
.status-chip.Pending { ... }
.status-chip.Active / .Completed / .Cancelled / .InProgress { ... }
```

---

## Proposed Files to be Affected

| File | Action | Priority |
|------|--------|----------|
| `shared/components/alert/alert.component.ts` | Extract inline styles → `.scss`, retheme | 🔴 1 |
| `shared/components/notification-snackbar/notification-snackbar.component.scss` | Full reskin | 🔴 1 |
| `shared/components/confirm-dialog/confirm-dialog.component.scss` | Add theme import, dark dialog surface | 🔴 1 |
| `shared/components/profile/profile.component.scss` | Full glassmorphic reskin | 🔴 1 |
| `shared/components/task-dashboard/task-dashboard.component.scss` | Full reskin: cards, chips, table, empty state | 🔴 1 |
| `styles.scss` | Add global `mat-card`, `mat-select`, `mat-option`, `mat-menu` dark overrides + global `.status-chip` | 🔴 1 |
| `admin/pages/oversight/analytics.component.scss` | Add theme, retheme cards/forms/typography | 🟠 2 |
| `admin/pages/oversight/audit-logs.component.scss` | Add theme, retheme cards/table/chips | 🟠 2 |
| `admin/pages/oversight/billing-receipts.component.scss` | Full reskin: table, chips, cards, empty state | 🟠 2 |
| `admin/pages/oversight/feedback.component.scss` | Full reskin: table, chips, cards, empty state | 🟠 2 |
| `admin/pages/management/amenities-management.component.scss` | Add page wrapper layout + theme | 🟠 2 |
| `admin/pages/management/menu-management.component.scss` | Add page wrapper layout + theme | 🟠 2 |
| `admin/pages/management/staff-management.component.scss` | Add page wrapper layout + theme | 🟠 2 |
| `admin/pages/management/amenities-management.component.html` | Apply `hideHeader`, page title pattern | 🟠 2 |
| `admin/pages/management/menu-management.component.html` | Apply `hideHeader`, page title pattern | 🟠 2 |
| `admin/pages/management/staff-management.component.html` | Apply `hideHeader`, page title pattern | 🟠 2 |
| `admin/pages/management/amenities-management.component.ts` | Add `modalTitle`, `modalSubtitle`, `hideHeader` to crudConfig | 🟠 2 |
| `admin/pages/management/menu-management.component.ts` | Add `modalTitle`, `modalSubtitle`, `hideHeader` to crudConfig | 🟠 2 |
| `admin/pages/management/staff-management.component.ts` | Add `modalTitle`, `modalSubtitle`, `hideHeader` to crudConfig | 🟠 2 |
| `admin/components/create-internal-ticket-dialog.component.scss` | Add theme, dark dialog, Aetheris error block, gold dividers | 🟠 2 |
| `front-desk/front-desk-shell.component.scss` | Dark sidenav, gold nav, dark toolbar | 🟠 3 |
| `front-desk/pages/dashboard.component.scss` | Glassmorphic cards, dark surfaces | 🟠 3 |
| `front-desk/pages/guest-details.component.scss` | Dark card surface, gold borders | 🟠 3 |
| `front-desk/pages/new-booking.component.scss` | Dark form fields, Aetheris input style | 🟠 3 |
| `front-desk/components/booking-action-modal/booking-action-modal.component.scss` | Dark modal, remove white card | 🟠 3 |
| `front-desk/components/booking-action-modal/checkout-dialog/checkout-dialog.component.scss` | Replace green with `var(--color-secondary)` | 🟠 3 |
| `front-desk/components/guest-billing/guest-billing.component.scss` | Retheme status badges | 🟠 3 |
| `front-desk/components/ticket-list/ticket-list.component.scss` | Dark table, label-caps headers | 🟠 3 |
| `housekeeping/housekeeping-shell.component.scss` | Dark shell (same as front-desk) | 🟡 4 |
| `kitchen/kitchen-shell.component.scss` | Dark shell (same as front-desk) | 🟡 4 |
| `kitchen/pages/menu-items.component.scss` | Dark form fields, card grid | 🟡 4 |
| `maintenance/maintenance-shell.component.scss` | Dark shell (same as front-desk) | 🟡 4 |
| `user/components/food-order/food-order.component.scss` | Add theme import, ensure dark surface | 🟡 5 |
| `user/components/food-order/cart-drawer.component.scss` | Replace `:host` variable declarations with `@import` | 🟡 5 |
| `user/components/request-service/request-service.component.scss` | Replace `:host` variable declarations with `@import` | 🟡 5 |
| `user/components/my-requests/my-requests.component.scss` | Replace `:host` variable declarations with `@import`, verify status chips | 🟡 5 |
| `user/pages/room-service.component.scss` | Replace `:host` variable declarations with `@import` | 🟡 5 |

---

## Verification Plan

### Per-Component
- Visual comparison of each route in browser against Aetheris reference designs.
- Check that no hardcoded colors remain (`#ffffff`, `rgba(0,0,0,...)`, `#2e7d32`, `#c62828`, `#e8f5e9`, `#ffebee` etc.).

### Automated
- `npm run build` after each priority group to confirm zero TypeScript/SCSS compilation errors.
- `git diff` after each group to verify only styling files are touched — no logic, service, or routing changes.

### Cross-Cutting
- Open Snackbar notification (success + error) and confirm Aetheris gold/red styling.
- Open each CRUD modal and confirm dark surface + visible labels.
- Open each sidenav on mobile and confirm dark background + gold active state.
- Open status chips on all table pages and confirm dark-theme semantic colors.
