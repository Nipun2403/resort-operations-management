# Patch Specsheet: Front Desk – Final Fixes

## 1. Purpose
- Fix the Extend Stay date picker so the calendar opens properly.
- Remove the Internal Ticket panel from the Room Service tab in the Guest Details page.
- Improve the Billing tab to show the latest folio in full, plus a collapsible “Old Folios” section with a table of past billing records, each row opening a detailed modal.

## 2. Files to Modify

| File | Change |
|------|--------|
| `src/app/features/front-desk/components/extend-stay-dialog/extend-stay-dialog.component.ts` | Ensure `MatNativeDateModule` is imported and the dialog has a minimum width. |
| `src/app/features/front-desk/components/extend-stay-dialog/extend-stay-dialog.component.html` | No change needed if the template already has a date picker; only imports matter. |
| `src/app/features/front-desk/components/booking-action-modal/room-service-tab/room-service-tab.component.html` | Remove `<app-internal-ticket-panel />` and its divider. |
| `src/app/features/front-desk/components/booking-action-modal/room-service-tab/room-service-tab.component.ts` | Remove `InternalTicketPanelComponent` from imports. |
| **New:** `src/app/features/front-desk/components/guest-billing/guest-billing.component.ts` | New component to display latest folio and old folios table. |
| **New:** `src/app/features/front-desk/components/guest-billing/guest-billing.component.html` | Template. |
| `src/app/features/front-desk/pages/guest-details.component.ts` | Replace `<app-billing-tab>` with `<app-guest-billing>`, passing all bookings. |
| `src/app/features/front-desk/pages/guest-details.component.html` | Update Billing tab content. |

## 3. Fix Extend Stay Date Picker

### Root Cause
The `ExtendStayDialogComponent` likely lacks the `MatNativeDateModule` (or `MatDatepickerModule`) in its `imports` array, causing the date picker to malfunction or not open. Also, the dialog width may be too small.

### Changes
In `extend-stay-dialog.component.ts`:
- Verify that `MatDatepickerModule` and `MatNativeDateModule` are both imported.
- In the component class, ensure the dialog opening width is at least `400px` (already set in the GuestDetailsComponent when calling `this.dialog.open(ExtendStayDialogComponent, { width: '400px', ... })`). No change needed there if already set. But to be safe, we can also set a minimum width in the dialog component's own SCSS or host metadata.

Add to the `@Component` metadata:
```typescript
host: { 'style': 'min-width: 350px; display: block;' }
```

Or in the SCSS file add `:host { min-width: 350px; }`.

Also confirm the date picker input has the `[matDatepicker]` binding and the `mat-datepicker` element is present. The previous spec already included these; if they're missing, add them from the original extend-stay spec.

## 4. Remove Internal Ticket from Room Service Tab

### File: `room-service-tab.component.html`
Delete the following lines:
```html
<mat-divider></mat-divider>
<app-internal-ticket-panel />
```

### File: `room-service-tab.component.ts`
- Remove `InternalTicketPanelComponent` from the `imports` array.
- Remove any corresponding import statement.

The internal ticket creation is now only available from the dashboard button.

## 5. Guest Billing Component (Replaces BillingTab in Guest Details)

### 5.1 New Component: `GuestBillingComponent`
**File:** `src/app/features/front-desk/components/guest-billing/guest-billing.component.ts`  
**Selector:** `app-guest-billing`  
**Standalone:** `true`  
**Input:** `bookings = input.required<Booking[]>()`  
**Imports:** `CommonModule`, `MatCardModule`, `MatDividerModule`, `MatTableModule`, `MatButtonModule`, `MatIconModule`, `MatProgressSpinnerModule`, `AlertComponent`, `BillingApiService`, `MatExpansionModule` (for collapsible), `MatDialogModule`.

### 5.2 State (signals)
```typescript
billingRecords = signal<BillingRecord[]>([]); // fetched for all bookings
loading = signal(false);
error = signal<string | null>(null);
latestBilling = computed(() => this.billingRecords()[0] || null);
oldBilling = computed(() => this.billingRecords().slice(1));
```

### 5.3 Data Fetching
On init, for each booking in `bookings()`, call `GET /api/v1/billing/{bookingId}` and aggregate results. Sort by date descending (by booking check-out or creation). Assume each booking has at most one billing record.

```typescript
private billingApi = inject(BillingApiService);
private destroyRef = inject(DestroyRef);

ngOnInit(): void {
  this.fetchAllBilling();
}

private fetchAllBilling(): void {
  const bookings = this.bookings();
  if (bookings.length === 0) return;
  this.loading.set(true);
  this.error.set(null);
  const requests = bookings.map(b => 
    this.billingApi.getByBookingId(b.id).pipe(
      map(data => ({ ...data, bookingId: b.id, checkOutDate: b.checkOutDate })),
      catchError(() => of(null))
    )
  );
  forkJoin(requests).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe(results => {
    const valid = results.filter((r): r is any => r !== null);
    // sort by checkOutDate descending
    valid.sort((a, b) => new Date(b.checkOutDate).getTime() - new Date(a.checkOutDate).getTime());
    this.billingRecords.set(valid);
  });
}
```

### 5.4 Template
```html
<div class="guest-billing">
  @if (loading()) {
    <mat-spinner diameter="30"></mat-spinner>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)"></app-alert>
  } @else {
    <!-- Latest Folio -->
    @if (latestBilling()) {
      <h3>Latest Folio (Booking #{{ latestBilling()!.bookingId }})</h3>
      <div class="bill-summary">
        <p><strong>Guest:</strong> {{ latestBilling()!.guestName }}</p>
        <p><strong>Total Bill:</strong> {{ latestBilling()!.totalBill | currency }}</p>
        <p><strong>Payment Status:</strong> {{ latestBilling()!.paymentStatus }}</p>
        <!-- other details as desired -->
      </div>
    } @else {
      <p>No billing information available.</p>
    }

    <!-- Old Folios (collapsible) -->
    @if (oldBilling().length > 0) {
      <mat-accordion>
        <mat-expansion-panel>
          <mat-expansion-panel-header>
            <mat-panel-title>Old Folios ({{ oldBilling().length }})</mat-panel-title>
          </mat-expansion-panel-header>
          <table mat-table [dataSource]="oldBilling()">
            <ng-container matColumnDef="bookingId">
              <th mat-header-cell *matHeaderCellDef>Booking ID</th>
              <td mat-cell *matCellDef="let b">{{ b.bookingId }}</td>
            </ng-container>
            <ng-container matColumnDef="checkOutDate">
              <th mat-header-cell *matHeaderCellDef>Check‑Out Date</th>
              <td mat-cell *matCellDef="let b">{{ b.checkOutDate }}</td>
            </ng-container>
            <ng-container matColumnDef="totalBill">
              <th mat-header-cell *matHeaderCellDef>Total</th>
              <td mat-cell *matCellDef="let b">{{ b.totalBill | currency }}</td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Details</th>
              <td mat-cell *matCellDef="let b">
                <button mat-icon-button (click)="openFolioDetail(b)" aria-label="View folio"><mat-icon>visibility</mat-icon></button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="['bookingId','checkOutDate','totalBill','actions']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['bookingId','checkOutDate','totalBill','actions']"></tr>
          </table>
        </mat-expansion-panel>
      </mat-accordion>
    }
  }
</div>
```

### 5.5 Folio Detail Modal
The `openFolioDetail(billingRecord)` method opens a dialog displaying the full folio details (like the existing billing summary). Reuse the `BillingTabComponent` as a dialog? Or create a simple dialog that receives the billing record. We'll use a small standalone dialog `FolioDetailDialogComponent`:

```typescript
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
@Component({...})
export class FolioDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: any) {}
}
```
Template:
```html
<h2 mat-dialog-title>Folio for Booking #{{ data.bookingId }}</h2>
<mat-dialog-content>
  <p><strong>Guest:</strong> {{ data.guestName }}</p>
  <p><strong>Total Bill:</strong> {{ data.totalBill | currency }}</p>
  <p><strong>Payment Status:</strong> {{ data.paymentStatus }}</p>
  <!-- more details from billing data -->
</mat-dialog-content>
```

Add this component to the `GuestBillingComponent`'s imports and open it in `openFolioDetail`.

### 5.6 Integration into GuestDetailsComponent
In `guest-details.component.html`, replace the Billing tab content:
```html
<mat-tab label="Billing" [disabled]="!activeBooking()">
  <app-guest-billing [bookings]="bookings()" />
</mat-tab>
```
Note: We might want to pass all bookings, not just active, to show history. So use `bookings()`.

Remove `BillingTabComponent` from imports of `GuestDetailsComponent` (unless used elsewhere) and add `GuestBillingComponent`.

## 6. Self‑Review Checklist
- [ ] Extend Stay dialog now opens the calendar when clicking the date field.
- [ ] The Room Service tab in the Guest Details page no longer shows the Internal Ticket panel.
- [ ] The Billing tab now shows the latest folio for the most recent booking, and a collapsible “Old Folios” panel with a table of past billing records.
- [ ] Clicking the eye icon in an old folio row opens a modal with full details.
- [ ] No console errors; all subscriptions cleaned up.

## 7. Integration Notes
- The `GuestBillingComponent` fetches billing for all bookings of the guest, which may be multiple API calls. Use `forkJoin` to handle them efficiently.
- The `FolioDetailDialogComponent` is a simple presentational dialog; it can be placed in the same file as `GuestBillingComponent` or in a separate file for clarity. We'll put it in `guest-billing/folio-detail-dialog.component.ts`.
- The `ExtendStayDialogComponent` fix may also require adding `MatNativeDateModule` to its imports array if missing. Verify by checking the existing component code. If missing, add it explicitly.
- No other components are affected.

---

