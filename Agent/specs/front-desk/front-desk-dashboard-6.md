# Specsheet: Front Desk Dashboard – Part 6 (Final Integration & Responsive Polish)

## 1. Purpose
- Wire up the complete Front Desk Dashboard so that all parts work together seamlessly.
- Ensure that the summary cards and movement table refresh correctly after any action that affects data, regardless of whether the booking action modal was used for a major action (check‑in, cancel, extend stay, checkout) or only for room service and internal tickets.
- Apply final responsive polish to the dashboard, movement table, and modals.
- Provide a consolidated testing checklist covering all dashboard features from Parts 1 to 5.

## 2. Files to Modify

| File | Change |
|------|--------|
| `src/app/features/front-desk/pages/dashboard.component.ts` | Adjust modal `afterClosed` handler to always refresh summary cards; keep movement table refresh only on `true` result. |
| `src/app/features/front-desk/pages/dashboard.component.html` | No change (still hosts summary cards and movement table). |
| `src/app/features/front-desk/pages/dashboard.component.scss` | Add final responsive adjustments for dashboard layout. |
| `src/app/features/front-desk/components/movement-table/movement-table.component.scss` | Ensure table and toggle stack correctly on small screens. |
| `src/app/features/front-desk/components/booking-action-modal/booking-action-modal.component.scss` | Ensure tabs scroll on mobile, modal full‑width on narrow screens. |

## 3. Dashboard Refresh Logic Enhancement

### 3.1 Current Behavior (from Parts 2–5)
- The dashboard’s `openBookingModal` method opens the `BookingActionModalComponent` and subscribes to `afterClosed`.
- If the result is `true`, it increments `refreshTable` and calls `loadSummary()`.
- `loadSummary()` reloads the three summary cards (arrivals, departures, active tickets).

### 3.2 Problem
Room service actions (food orders, housekeeping, maintenance, internal tickets) do not close the modal with `true`; they happen inside the modal while it stays open. Consequently, the active tickets count and possibly other summary data are not refreshed until a later main action occurs or the user manually re‑opens the page.

### 3.3 Solution
Always refresh the summary cards when the modal closes, regardless of the result. The movement table is refreshed only after a major mutation (result `true`).

**Updated `openBookingModal` method in `dashboard.component.ts`:**

```ts
openBookingModal(booking: Booking): void {
  const dialogRef = this.dialog.open(BookingActionModalComponent, {
    data: { booking },
    width: '95vw',
    maxWidth: '700px',
    panelClass: 'booking-action-modal',
  });
  dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
    // Always refresh summary cards – they may have changed due to room service actions
    this.loadSummary();

    if (result === true) {
      // Major mutation – also refresh the movement table
      this.refreshTable.update(n => n + 1);
    }
  });
}
```

**Note:** `refreshTable` is already passed to `<app-movement-table [refresh]="refreshTable()">` and causes a refetch via an `effect()` inside the movement table component.

## 4. Responsive Polish

### 4.1 Dashboard Layout (`dashboard.component.scss`)
Add media queries to ensure the three summary cards stack on small screens:

```scss
.dashboard {
  padding: 16px;
}
.summary-row {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 16px;
}
.summary-card {
  flex: 1 1 250px;
  cursor: pointer; // for the active tickets card
  &.clickable:hover {
    box-shadow: 0 4px 8px rgba(0,0,0,0.15);
  }
  .ticket-breakdown {
    display: flex;
    flex-direction: column;
    gap: 4px;
    span {
      font-size: 0.95rem;
    }
  }
}
@media (max-width: 599px) {
  .summary-card {
    flex: 1 1 100%;
  }
}
```

### 4.2 Movement Table (`movement-table.component.scss`)
Already has responsive rules; verify they work down to 320px. Add explicit `max-width: 100%` to table container:

```scss
.movement-table {
  .table-container {
    overflow-x: auto;
    max-width: 100%;
  }
  table {
    min-width: 600px; // ensures horizontal scroll on small screens rather than squeezing columns
  }
}
```

### 4.3 Booking Action Modal (`booking-action-modal.component.scss`)
Ensure tabs do not overflow on mobile:

```scss
.mat-mdc-tab-group {
  max-width: 100%;
}
.mat-mdc-tab-body {
  overflow-y: auto;
}
@media (max-width: 599px) {
  .modal-content {
    padding: 8px;
  }
  .actions {
    flex-direction: column;
    button {
      width: 100%;
    }
  }
}
```

## 5. Additional API Method Requirement
The `BookingApiService` must implement the following method for the checkout flow (if not already present):

```ts
checkOut(id: number): Observable<void> {
  return this.http.post<void>(`${this.baseUrl}/bookings/${id}/checkout`, {});
}
```

This endpoint is `POST /api/v1/bookings/{id}/checkout`.

## 6. Consolidated Testing Checklist (Parts 1–6)

### Summary Cards
- [ ] Arrivals count shows the number of bookings with `movementStatus=incoming` for today.
- [ ] Departures count shows the number with `movementStatus=outgoing` for today.
- [ ] Active Tickets card shows separate counts for pending/in‑progress housekeeping, maintenance, and food orders.
- [ ] Clicking the Active Tickets card opens a dialog with three tabs, each listing the corresponding tickets.

### Movement Table
- [ ] Table loads today’s arrivals by default.
- [ ] Toggling to “Departures” updates the table using `movementStatus=outgoing`.
- [ ] Typing a search term disables the toggle, changes the title to “Search Results”, and fetches using `guestQuery`.
- [ ] Clearing the search restores the “Today’s Movement” title and re‑enables the toggle.
- [ ] Pagination and sorting work via server‑side parameters.
- [ ] Clicking a row or the eye icon emits the booking to the parent.

### Booking Action Modal – Details Tab
- [ ] Opens on booking selection; shows all booking details.
- [ ] Check‑In button (when `Booked`) asks for confirmation, calls API, shows snackbar with room number, closes modal.
- [ ] Cancel button (when `Booked`) asks for confirmation, calls API, shows snackbar, closes modal.
- [ ] Extend Stay button (when `CheckedIn`) opens a date picker dialog with a minimum date constraint; on submit, calls API and refreshes.

### Booking Action Modal – Room Service Tab
- [ ] Contains Food Order, Housekeeping, Maintenance, and Internal Ticket panels.
- [ ] Food Order loads menu, allows adding/removing items, and placing orders with confirmation.
- [ ] Housekeeping and Maintenance panels allow selecting a room and submitting a request with confirmation.
- [ ] Internal Ticket panel creates a housekeeping or maintenance ticket with location and description.

### Booking Action Modal – Billing Tab
- [ ] Displays folio with guest name, totals, food and amenity items.
- [ ] “Make Payment” button reveals a payment form; successful payment updates the folio.

### Check‑Out Flow
- [ ] Check‑Out button (from Details tab) opens a dedicated dialog.
- [ ] Dialog shows folio; if unpaid, allows proceeding to payment; if paid, allows direct check‑out.
- [ ] After successful payment, the folio refreshes; check‑out API is called and confirmation is shown.
- [ ] On closing the confirmation step, the dialog returns `true`, which causes the modal to close with `true`.

### Post‑Action Refresh
- [ ] After the booking action modal closes (for any reason), the summary cards (arrivals, departures, active tickets) are reloaded.
- [ ] If the modal closed with `true` (major mutation), the movement table is also refreshed.

### Responsive Behaviour
- [ ] On screens down to 320px, all controls and tables remain usable without horizontal viewport overflow.
- [ ] Summary cards stack vertically on mobile.
- [ ] Movement table scrolls horizontally on small screens.
- [ ] Booking action modal uses full width on mobile, tabs do not cause overflow.
- [ ] All interactive elements have minimum touch target 48dp.

## 7. Integration Notes
- No new components are created; this spec only adjusts the dashboard’s refresh logic and adds final CSS.
- The `BookingApiService` must include the `checkOut` method.
- The `extractErrorMessage` helper remains defined locally in each component that needs it; no consolidation is required unless desired for future maintenance.
- The dashboard is now complete and ready for the next role (Kitchen, Housekeeping, Maintenance). 

