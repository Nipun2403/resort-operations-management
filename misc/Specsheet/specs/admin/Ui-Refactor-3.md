# Specsheet: UI Refinements – Confirmation, Responsive Cards, Grid & Title

## 1. Purpose

- Unify the disable‑toggle behaviour across all management entities: a confirmation dialog appears when toggling an entity from active to inactive (same as the existing staff deactivation flow). The confirmation is moved into the generic `CrudModalComponent` so every management page inherits it, and the custom staff confirmation is removed to avoid double dialogs.
- Enable responsive card layouts on all oversight pages (Feedback, Bookings & Receipts, Audit Logs) so that tables automatically transform into card lists on small screens, using Angular’s `@if` control flow driven by an `isMobile` signal.
- Adjust the room status grid on mobile to a **2‑column vertically scrollable** layout, while keeping the table‑vs‑grid toggle intact.
- Ensure all management data cards (both generic CRUD and the new oversight cards) do not allow text overflow; use multi‑line truncation instead of aggressive single‑line ellipsis.
- Make the top‑bar title dynamic: it now shows the current page name (e.g. “Dashboard”, “Rooms”) instead of a static “Hotel Management”.

## 2. Dynamic Title in Admin Shell

**Goal:** The toolbar currently displays “Hotel Management”. It must show the title of the currently active page.

**Files to modify:**

- `src/app/features/admin/admin-shell.component.ts`
- `src/app/features/admin/admin-shell.component.html`
- All admin route definitions (add `data: { title: '...' }`)

**Step 1 – Add `data.title` to each admin route**  
In the admin routing configuration (e.g., `admin.routes.ts` or wherever the admin children are defined), add a `data` object with a `title` property for every route that has a component. For example:

```ts
{ path: '', redirectTo: 'dashboard', pathMatch: 'full' },
{ path: 'dashboard', component: PlaceholderDashboardComponent, data: { title: 'Dashboard' } },
{ path: 'management', children: [
  { path: 'room', component: ..., data: { title: 'Rooms' } },
  { path: 'room-type', component: ..., data: { title: 'Room Types' } },
  { path: 'staff', component: ..., data: { title: 'Staff' } },
  { path: 'amenities', component: ..., data: { title: 'Amenities' } },
  { path: 'menu', component: ..., data: { title: 'Menu Items' } },
]},
{ path: 'oversight', children: [
  { path: 'analytics', component: ..., data: { title: 'Analytics' } },
  { path: 'auditlogs', component: ..., data: { title: 'Audit Logs' } },
  { path: 'billings-receipts', component: ..., data: { title: 'Billing & Receipts' } },
  { path: 'feedback', component: ..., data: { title: 'Feedback' } },
]},
{ path: 'profile', component: ..., data: { title: 'Profile' } },
```

**Step 2 – Update `AdminShellComponent` to extract the title**  
Add a `title` signal and a subscription to the router’s `NavigationEnd` events. Use `ActivatedRoute` to traverse the route tree and get the deepest `data.title`. Ensure the subscription is cleaned up with `takeUntilDestroyed`.

```ts
import { Component, inject, signal } from "@angular/core";
import { BreakpointObserver } from "@angular/cdk/layout";
import { map } from "rxjs/operators";
import { toSignal } from "@angular/core/rxjs-interop";
import { Router, NavigationEnd, ActivatedRoute } from "@angular/router";
import { filter } from "rxjs/operators";
import { DestroyRef } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";

export class AdminShellComponent {
  // ... existing signals
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  title = signal("");

  constructor() {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        let route = this.activatedRoute;
        while (route.firstChild) route = route.firstChild;
        const title = route.snapshot.data["title"] || "Admin";
        this.title.set(title);
      });
  }
}
```

**Step 3 – Update the template**  
Replace the static `<span>Hotel Management</span>` with:

```html
<span>{{ title() }}</span>
```

## 3. Generic Modal – Disable Confirmation (One Confirmation for All)

**Current state:** The staff page manually opens a confirmation dialog in `StaffManagementComponent` before calling the update API when toggling a staff member from active to inactive. No other management page has a confirmation.

**New behavior:** The shared `CrudModalComponent` will handle the confirmation internally. When the user saves and the `isActive` toggle changes from `true` (original) to `false`, the modal itself opens a confirmation dialog. Only after confirmation does it emit the `save` event. This applies to **all** management entities that use the generic modal and have `supportsToggle: true` (Rooms, Room Types, Staff, Amenities, Menu).

**Implication:** The custom confirmation in `StaffManagementComponent` must be removed to prevent a double prompt.

### 3.1 Remove Staff‑Specific Confirmation

**File:** `src/app/features/admin/pages/management/staff-management.component.ts`

- Delete the `showDisableConfirmation` method entirely.
- In the `onSave` method, remove the conditional that calls `showDisableConfirmation`. After this change, `onSave` will directly call `performUpdate` or `performCreate` without any extra dialogs.

### 3.2 Add Confirmation to `CrudModalComponent`

**Files:** `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts` and its HTML (if a confirmation template is needed, but we use `MatDialog` programmatically).

**Modifications:**

- Inject `MatDialog`.
- In the `submit` method, after validation, check if the modal is in edit mode and the toggle has been switched from `true` to `false`. We have the original entity’s `isActive` in `this.data.entity.isActive` (or the `supportsToggle` field). Compare with the new form value.
- If `isActive` changed from `true` to `false`, open a `ConfirmDialogComponent` (the shared one). On positive confirmation, proceed with closing the dialog and emitting the save event. Otherwise, do nothing.

**Exact code addition in `submit()`:**

```ts
const raw = this.form.getRawValue();
const newIsActive = this.supportsToggle ? raw.isActive : true;
const originalIsActive = this.data.editMode
  ? (this.data.entity?.isActive ?? true)
  : true;

if (this.data.editMode && originalIsActive && !newIsActive) {
  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    data: {
      title: "Confirm Deactivation",
      message: `Are you sure you want to disable this ${this.data.entityName ?? "item"}?`,
    },
  });
  dialogRef.afterClosed().subscribe((confirmed) => {
    if (confirmed) {
      this.dialogRef.close({
        formValue: raw,
        isActive: newIsActive,
      });
    }
  });
} else {
  this.dialogRef.close({
    formValue: raw,
    isActive: newIsActive,
  });
}
```

**Ensure the modal has access to `ConfirmDialogComponent`:** Import it in the modal’s `imports` array.

**Note:** The modal’s `data` object should include the `entityName` (we can pass it from the generic crud config). Add `entityName` to `CrudModalData` interface and pass it when opening the modal. This is a small generic change; we’ll add it.

**Update `CrudModalData` interface (in `crud-config.model.ts`):**

```ts
export interface CrudModalData {
  // ... existing
  entityName?: string; // for confirmation message
}
```

**Pass `entityName` from `GenericCrudComponent` when opening modal:**
In `openEditModal` and `openAddModal`, include `entityName: this.config().entityName`.

**Import `ConfirmDialogComponent` in the modal’s standalone imports.**

## 4. Feedback – Row Toggle Confirmation

**File:** `src/app/features/admin/pages/oversight/feedback.component.ts`

**Change:** In the `onToggleHidden` method, when toggling from visible (`isHidden === false`) to hidden (`isHidden === true`), open a confirmation dialog before proceeding. Also ensure `DestroyRef` is injected.

**Exact implementation:**

```ts
import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
// ...
private destroyRef = inject(DestroyRef);

onToggleHidden(feedback: Feedback, isHidden: boolean): void {
  if (!feedback.isHidden && isHidden) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Hide Feedback',
        message: 'Are you sure you want to hide this feedback?',
      },
    });
    dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
      if (confirmed) {
        this.performToggle(feedback, isHidden);
      } else {
        // Revert the optimistic UI toggle
        this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden: false } : f));
      }
    });
  } else {
    this.performToggle(feedback, isHidden);
  }
}

private performToggle(feedback: Feedback, isHidden: boolean): void {
  // Optimistic update
  this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden } : f));

  this.feedbackApi.moderate(feedback.id, { isHidden }).pipe(
    takeUntilDestroyed(this.destroyRef)
  ).subscribe({
    next: () => {
      this.snackBar.open(
        isHidden ? 'Feedback hidden' : 'Feedback visible',
        'Close',
        { duration: 2000 }
      );
    },
    error: (err: any) => {
      // Revert on failure
      this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden: !isHidden } : f));
      this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 });
    }
  });
}
```

## 5. Oversight Pages – Responsive Table‑to‑Card Transformation (Angular `@if`)

We will add a card view to each of the three oversight pages, using Angular’s `@if` control flow bound to an `isMobile` signal (detected via `BreakpointObserver`). This avoids rendering two heavy DOM structures simultaneously, unlike CSS `display:none`. The paginator remains outside the conditional blocks to serve both views.

### 5.1 Audit Logs

**File:** `src/app/features/admin/pages/oversight/audit-logs.component.ts` – inject `BreakpointObserver` and create `isMobile` signal.

```ts
private breakpointObserver = inject(BreakpointObserver);
isMobile = toSignal(
  this.breakpointObserver.observe('(max-width: 767px)').pipe(map(r => r.matches)),
  { initialValue: false }
);
```

**Template (`audit-logs.component.html`):**

```html
<!-- Use Angular control flow, not CSS hide/show -->
@if (isMobile()) {
<div class="mobile-card-view">
  @for (entry of entries(); track entry.id) {
  <mat-card
    (click)="openDetail(entry)"
    class="audit-card"
  >
    <mat-card-header>
      <mat-card-title
        >{{ entry.entityName }} – {{ entry.action }}</mat-card-title
      >
      <mat-card-subtitle
        >{{ entry.timestamp | date:'short' }}</mat-card-subtitle
      >
    </mat-card-header>
    <mat-card-content>
      <p>Changed by: {{ entry.changedByName }}</p>
    </mat-card-content>
  </mat-card>
  } @empty {
  <p>No audit logs found.</p>
  }
</div>
} @else {
<div class="desktop-view">
  <table
    mat-table
    ...
  >
    ...
  </table>
</div>
}
<mat-paginator ...></mat-paginator>
```

**CSS:** Add a maximum height and overflow-y auto to the mobile card view, and ensure cards don't overflow.

### 5.2 Feedback

**File:** `feedback.component.ts` – add `isMobile` signal (same pattern).

**Template (`feedback.component.html`):**

```html
@if (isMobile()) {
<div class="mobile-card-view">
  @for (f of entries(); track f.id) {
  <mat-card class="feedback-card">
    <mat-card-header>
      <mat-card-title
        >Booking #{{ f.bookingId }} – {{ f.rating }}/5</mat-card-title
      >
      <mat-card-subtitle>{{ f.createdAt | date:'short' }}</mat-card-subtitle>
    </mat-card-header>
    <mat-card-content>
      <p>{{ f.comments || '—' }}</p>
    </mat-card-content>
    <mat-card-actions>
      <mat-slide-toggle
        [checked]="f.isHidden"
        (change)="onToggleHidden(f, $event.checked)"
        [aria-label]="f.isHidden ? 'Show feedback' : 'Hide feedback'"
        matTooltip="Toggle visibility"
      >
      </mat-slide-toggle>
    </mat-card-actions>
  </mat-card>
  }
</div>
} @else {
<div class="desktop-view">
  <table
    mat-table
    ...
  >
    ...
  </table>
</div>
}
<mat-paginator ...></mat-paginator>
```

### 5.3 Billing & Receipts

**File:** `billing-receipts.component.ts` – add `isMobile` signal.

**Template (`billing-receipts.component.html`):**  
Wrap each table’s view toggle with `@if (isMobile())` for card lists and `@else` for tables.

For bookings:

```html
@if (activeView.value === 'bookings') { @if (isMobile()) {
<div class="mobile-card-view">
  @for (b of bookings(); track b.id) {
  <mat-card (click)="openBookingDetail(b)">
    <mat-card-header>
      <mat-card-title>{{ b.guestName }}</mat-card-title>
      <mat-card-subtitle
        >{{ b.checkInDate }} – {{ b.checkOutDate }}</mat-card-subtitle
      >
    </mat-card-header>
    <mat-card-content>
      <p>Status: {{ b.bookingStatus }} | Rooms: {{ getRoomsSummary(b) }}</p>
    </mat-card-content>
  </mat-card>
  }
</div>
} @else {
<div class="desktop-view">
  <table
    mat-table
    ...
  >
    ...
  </table>
</div>
}
<mat-paginator ...></mat-paginator>
}
```

Receipts similarly.

## 6. Room Page – Mobile Grid with 2 Columns

**File:** `room-status-grid.component.scss`

In the media query for `max-width: 767px`, change the grid layout to 2 columns. Keep vertical scroll.

```scss
@media (max-width: 767px) {
  .status-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    grid-auto-rows: minmax(60px, auto);
    gap: 8px;
    overflow-y: auto;
    overflow-x: hidden;
    max-height: 70vh;
    padding: 8px;
  }
}
```

The toggle between table and grid remains; the existing mobile view toggle code in `room-management.component.html` still works.

## 7. Prevent Text Overflow in Cards – Multi‑Line Truncation

### 7.1 Generic CRUD CardsViewComponent

**File:** `src/app/shared/components/generic-crud/cards-view/cards-view.component.scss`

Add:

```scss
.card-item {
  overflow: hidden;
  .card-content {
    p,
    span {
      display: -webkit-box;
      -webkit-line-clamp: 3; // allows up to 3 lines, then ellipsis
      -webkit-box-orient: vertical;
      overflow: hidden;
      text-overflow: ellipsis;
      word-break: break-word; // prevents long unbreakable strings from spilling out
    }
  }
}
```

### 7.2 Oversight Pages’ Card Views

Apply the same multi‑line truncation CSS to each oversight page’s SCSS for the mobile cards. For example, in `audit-logs.component.scss`:

```scss
.mobile-card-view {
  .audit-card {
    margin-bottom: 12px;
    .mat-card-content p {
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      text-overflow: ellipsis;
      word-break: break-word;
    }
  }
}
```

Similarly for feedback and billing cards.

## 8. File Changes Summary

**New/Modified Files:**

- `src/app/features/admin/admin-shell.component.ts` – add title logic.
- `src/app/features/admin/admin-shell.component.html` – show `title()`.
- `src/app/features/admin/admin.routes.ts` (or wherever routes are defined) – add `data.title`.
- `src/app/shared/models/crud-config.model.ts` – add `entityName?` to `CrudModalData`.
- `src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts` – add confirmation.
- `src/app/shared/components/generic-crud/generic-crud.component.ts` – pass `entityName` to modal data.
- `src/app/features/admin/pages/management/staff-management.component.ts` – remove confirmation.
- `src/app/features/admin/pages/oversight/feedback.component.ts` – add row toggle confirmation, inject `DestroyRef`.
- `src/app/features/admin/pages/oversight/audit-logs.component.ts` – add `isMobile` signal.
- `src/app/features/admin/pages/oversight/audit-logs.component.html` – add card view with `@if`.
- `src/app/features/admin/pages/oversight/audit-logs.component.scss` – card truncation styles.
- `src/app/features/admin/pages/oversight/feedback.component.ts` – add `isMobile` signal.
- `src/app/features/admin/pages/oversight/feedback.component.html` – add card view.
- `src/app/features/admin/pages/oversight/feedback.component.scss` – card truncation.
- `src/app/features/admin/pages/oversight/billing-receipts.component.ts` – add `isMobile` signal.
- `src/app/features/admin/pages/oversight/billing-receipts.component.html` – add card views for both tables.
- `src/app/features/admin/pages/oversight/billing-receipts.component.scss` – card truncation.
- `src/app/features/admin/components/room-status-grid/room-status-grid.component.scss` – mobile 2 columns.
- `src/app/shared/components/generic-crud/cards-view/cards-view.component.scss` – multi‑line truncation.

## 9. Self‑Review Checklist

- [ ] Every management modal for room types, rooms, staff, amenities, menu shows a confirmation when toggling the active/inactive slide from active to inactive.
- [ ] Staff deactivation no longer shows a duplicate dialog (only the generic modal’s dialog appears).
- [ ] Feedback page row toggle shows confirmation when hiding a comment; cancellation reverts the toggle.
- [ ] Audit Logs, Feedback, Billing & Receipts pages use `@if (isMobile())` to render card lists, and `@else` for the table; no redundant DOM elements.
- [ ] The room status grid on mobile shows 2 columns and scrolls vertically; the table/grid toggle still works.
- [ ] No text overflows any card; long strings are truncated with a vertical line‑clamp and word‑break.
- [ ] The top toolbar shows the correct page title for every admin page, updating on navigation.
- [ ] Router subscription is cleaned up with `takeUntilDestroyed`; no memory leaks.
- [ ] No console errors; all subscriptions properly managed.
- [ ] Responsive breakpoints exactly as defined: 767px for card transformation, 1024px for sidebar collapse, etc.

## 10. Integration Notes

- The `data.title` approach requires that all admin routes are declared in one place; if routes are lazy‑loaded with `loadChildren`, you can still add `data.title` to the route configuration object. The title extraction logic will still work because it traverses the activated route tree.
- The generic modal change is backwards‑compatible; existing pages without toggle will not trigger the confirmation.
- The oversight card views reuse the existing paginator; ensure the paginator’s output events still work on mobile (they will, since the same component instance is used).
- No new dependencies.

