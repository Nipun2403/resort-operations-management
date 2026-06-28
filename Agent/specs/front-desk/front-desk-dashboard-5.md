# Specsheet: Front Desk Dashboard – Part 5 (Billing Tab & Check‑Out Flow)

## 1. Purpose

- Extend the `BookingActionModalComponent` with a **Billing** tab that displays the folio for the current booking and allows processing payment.
- Implement the full **Check‑Out flow** as a dedicated dialog:
  - Fetch billing details → display folio → if unpaid, show payment form → process payment → call check‑out endpoint → show confirmation.
- The Billing tab is also accessible directly (for viewing or paying without checking out), e.g., when a guest settles their bill early.
- All API interactions use the existing `BillingApiService` and `BookingApiService`.
- The modal’s external contract (close with `true` after mutation) remains unchanged.

## 2. Files to Create / Modify

| File                                                      | Action                                                              |
| --------------------------------------------------------- | ------------------------------------------------------------------- |
| **Modify:** `booking-action-modal.component.ts`           | Add a “Billing” tab to the `MatTabGroup`; host `<app-billing-tab>`. |
| **Modify:** `booking-action-modal.component.html`         | Add third tab.                                                      |
| **New:** `billing-tab/billing-tab.component.ts`           | Billing tab component                                               |
| **New:** `billing-tab/billing-tab.component.html`         | Template                                                            |
| **New:** `payment-form/payment-form.component.ts`         | Reusable payment form                                               |
| **New:** `payment-form/payment-form.component.html`       | Template                                                            |
| **New:** `checkout-dialog/checkout-dialog.component.ts`   | Check‑out flow dialog                                               |
| **New:** `checkout-dialog/checkout-dialog.component.html` | Template                                                            |
| **Modify:** `dashboard.component.ts`                      | No changes; refresh already works.                                  |
| **Modify:** `booking-action-modal.component.ts`           | Wire check‑out button to open `CheckoutDialogComponent`.            |

## 3. BookingActionModalComponent Updates

### 3.1 Template – Add Billing Tab

Insert after the “Room Service” tab:

```html
<mat-tab label="Billing">
  <app-billing-tab [bookingId]="booking().id" />
</mat-tab>
```

### 3.2 Check‑Out Button Logic (Details Tab)

In the details tab, the “Check‑Out” button (visible when `bookingStatus === 'CheckedIn'`) currently does nothing. Now it will open the `CheckoutDialogComponent`:

```ts
checkOut(): void {
  if (this.loading()) return;
  const checkoutRef = this.dialog.open(CheckoutDialogComponent, {
    data: { bookingId: this.booking().id },
    width: '95vw',
    maxWidth: '600px',
    disableClose: true, // user must complete or cancel the flow
  });
  checkoutRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
    if (result === true) {
      this.snackBar.open('Check‑out successful.', 'Close', { duration: 3000 });
      this.dialogRef.close(true); // refresh dashboard
    }
  });
}
```

**Imports:** Add `CheckoutDialogComponent` to the component’s `imports` array.

## 4. BillingTabComponent

### 4.1 API

- **Selector:** `app-billing-tab`
- **Standalone:** `true`
- **Input:** `bookingId = input.required<number>()`
- **Imports:** `CommonModule`, `MatCardModule`, `MatDividerModule`, `MatProgressSpinnerModule`, `MatButtonModule`, `AlertComponent`, `PaymentFormComponent`, `BillingApiService`, `DestroyRef`.

### 4.2 State

```ts
billDetails = signal<any | null>(null); // shape defined by GET /billing/{id}
billLoading = signal(false);
billError = signal<string | null>(null);
showPayment = signal(false); // toggles payment form
```

### 4.3 Template

```html
<div class="billing-tab">
  @if (billLoading()) {
  <mat-spinner diameter="30"></mat-spinner>
  } @else if (billError()) {
  <app-alert
    type="error"
    [message]="billError()!"
    (closed)="billError.set(null)"
  >
    <button
      mat-button
      (click)="fetchBill()"
    >
      Retry
    </button>
  </app-alert>
  } @else if (billDetails()) {
  <div class="bill-summary">
    <h3>Folio</h3>
    <p><strong>Guest:</strong> {{ billDetails().guestName }}</p>
    <p><strong>Nights Stayed:</strong> {{ billDetails().nightsStayed }}</p>
    <p><strong>Room Total:</strong> {{ billDetails().roomTotal | currency }}</p>
    <p><strong>Food Total:</strong> {{ billDetails().foodTotal | currency }}</p>
    <p>
      <strong>Amenity Total:</strong> {{ billDetails().amenityTotal | currency
      }}
    </p>
    <p><strong>Total Bill:</strong> {{ billDetails().totalBill | currency }}</p>
    <p><strong>Payment Status:</strong> {{ billDetails().paymentStatus }}</p>
  </div>

  <mat-divider></mat-divider>

  <div class="food-items">
    <h4>Food Items</h4>
    @for (item of billDetails().foodItems; track item) {
    <p>{{ item }}</p>
    }
  </div>
  <div class="amenity-items">
    <h4>Amenities</h4>
    @for (item of billDetails().amenityItems; track item) {
    <p>{{ item }}</p>
    }
  </div>

  @if (billDetails().paymentStatus === 'Pending') {
  <button
    mat-raised-button
    color="primary"
    (click)="showPayment.set(!showPayment())"
  >
    {{ showPayment() ? 'Cancel Payment' : 'Make Payment' }}
  </button>
  @if (showPayment()) {
  <app-payment-form
    [bookingId]="bookingId()"
    [amountDue]="billDetails().totalBill"
    (paymentComplete)="onPaymentComplete()"
  />
  } } @else {
  <p class="paid">Fully paid.</p>
  } } @else {
  <p>No billing information available.</p>
  }
</div>
```

### 4.4 Logic

```ts
private billingApi = inject(BillingApiService);
private destroyRef = inject(DestroyRef);

ngOnInit(): void {
  this.fetchBill();
}

fetchBill(): void {
  this.billLoading.set(true);
  this.billError.set(null);
  this.billingApi.getByBookingId(this.bookingId()).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.billLoading.set(false))
  ).subscribe({
    next: (data: any) => this.billDetails.set(data),
    error: (err: any) => this.billError.set(this.extractErrorMessage(err))
  });
}

onPaymentComplete(): void {
  this.showPayment.set(false);
  this.fetchBill(); // refresh to show updated status
}
```

**Note:** `extractErrorMessage` helper must be present (can be duplicated from earlier specs).

## 5. PaymentFormComponent

### 5.1 API

- **Selector:** `app-payment-form`
- **Standalone:** `true`
- **Inputs:** `bookingId = input.required<number>()`, `amountDue = input.required<number>()`
- **Output:** `paymentComplete = output<void>()`
- **Imports:** `CommonModule`, `ReactiveFormsModule`, `MatFormFieldModule`, `MatInputModule`, `MatSelectModule`, `MatButtonModule`, `MatProgressSpinnerModule`, `MatSnackBarModule`, `BillingApiService`, `DestroyRef`.

### 5.2 State

```ts
paymentForm = new FormGroup({
  amount: new FormControl<number>(this.amountDue(), {
    nonNullable: true,
    validators: [Validators.required, Validators.min(0.01)],
  }),
  paymentMethod: new FormControl("", {
    nonNullable: true,
    validators: Validators.required,
  }),
  transactionId: new FormControl("", {
    nonNullable: true,
    validators: Validators.required,
  }),
});
submitting = signal(false);
error = signal<string | null>(null);
```

### 5.3 Template

```html
<form
  [formGroup]="paymentForm"
  (ngSubmit)="submitPayment()"
  class="payment-form"
>
  <mat-form-field appearance="outline">
    <mat-label>Amount</mat-label>
    <input
      matInput
      type="number"
      formControlName="amount"
    />
    <mat-error *ngIf="paymentForm.get('amount')?.invalid"
      >Enter a valid amount.</mat-error
    >
  </mat-form-field>
  <mat-form-field appearance="outline">
    <mat-label>Payment Method</mat-label>
    <mat-select formControlName="paymentMethod">
      <mat-option value="Cash">Cash</mat-option>
      <mat-option value="Credit Card">Credit Card</mat-option>
      <mat-option value="Bank Transfer">Bank Transfer</mat-option>
    </mat-select>
    <mat-error *ngIf="paymentForm.get('paymentMethod')?.invalid"
      >Select a payment method.</mat-error
    >
  </mat-form-field>
  <mat-form-field appearance="outline">
    <mat-label>Transaction ID</mat-label>
    <input
      matInput
      formControlName="transactionId"
    />
    <mat-error *ngIf="paymentForm.get('transactionId')?.invalid"
      >Enter a transaction ID.</mat-error
    >
  </mat-form-field>
  @if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  ></app-alert>
  }
  <button
    mat-raised-button
    color="primary"
    type="submit"
    [disabled]="paymentForm.invalid || submitting()"
  >
    @if (submitting()) { <mat-spinner diameter="20"></mat-spinner> } Pay {{
    paymentForm.get('amount')?.value | currency }}
  </button>
</form>
```

### 5.4 Logic

```ts
private billingApi = inject(BillingApiService);
private snackBar = inject(MatSnackBar);
private destroyRef = inject(DestroyRef);

submitPayment(): void {
  if (this.submitting() || this.paymentForm.invalid) return;
  this.submitting.set(true);
  this.error.set(null);
  const dto = this.paymentForm.getRawValue();
  this.billingApi.pay(this.bookingId(), dto).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.submitting.set(false))
  ).subscribe({
    next: () => {
      this.snackBar.open('Payment processed.', 'Close', { duration: 3000 });
      this.paymentComplete.emit();
    },
    error: (err: any) => this.error.set(this.extractErrorMessage(err))
  });
}
```

**API Methods:** `BillingApiService` must provide `pay(bookingId: number, dto: PaymentRequestDTO): Observable<void>`.

## 6. CheckoutDialogComponent (Full Flow)

### 6.1 API

- **Selector:** `app-checkout-dialog`
- **Standalone:** `true`
- **Imports:** `CommonModule`, `MatDialogModule`, `MatButtonModule`, `MatDividerModule`, `MatProgressSpinnerModule`, `AlertComponent`, `PaymentFormComponent`, `BillingApiService`, `BookingApiService`, `DestroyRef`.
- **Injected Data:** `{ bookingId: number }` via `MAT_DIALOG_DATA`.

### 6.2 State & Steps

```ts
step = signal<"folio" | "payment" | "confirm" | "error">("folio");
billDetails = signal<any | null>(null);
loading = signal(false);
error = signal<string | null>(null);
bookingId = signal<number>(this.data.bookingId);
```

### 6.3 Template (exact – Angular 18 control flow)

```html
<h2 mat-dialog-title>Check‑Out</h2>
<mat-dialog-content>
  @if (step() === 'folio') { @if (loading()) {
  <mat-spinner diameter="30"></mat-spinner>
  } @else if (billDetails()) {
  <div class="bill-summary">
    <p><strong>Guest:</strong> {{ billDetails().guestName }}</p>
    <p><strong>Total Bill:</strong> {{ billDetails().totalBill | currency }}</p>
    <p><strong>Payment Status:</strong> {{ billDetails().paymentStatus }}</p>
  </div>
  <div class="actions">
    @if (billDetails().paymentStatus === 'Pending') {
    <button
      mat-raised-button
      color="primary"
      (click)="step.set('payment')"
    >
      Proceed to Payment
    </button>
    } @else {
    <button
      mat-raised-button
      color="primary"
      (click)="processCheckOut()"
    >
      Check‑Out Now
    </button>
    }
  </div>
  } @else {
  <p>Unable to load billing details.</p>
  } } @if (step() === 'payment') {
  <app-payment-form
    [bookingId]="bookingId()"
    [amountDue]="billDetails()?.totalBill ?? 0"
    (paymentComplete)="onPaymentComplete()"
  />
  } @if (step() === 'confirm') {
  <div class="confirmation">
    <mat-icon color="primary">check_circle</mat-icon>
    <p>Check‑out successful!</p>
  </div>
  } @if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="processCheckOut()"
    >
      Retry Check‑Out
    </button>
  </app-alert>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  @if (step() === 'folio' || step() === 'payment') {
  <button
    mat-button
    mat-dialog-close
  >
    Cancel
  </button>
  } @if (step() === 'confirm') {
  <button
    mat-button
    mat-dialog-close
    [mat-dialog-close]="true"
  >
    Close
  </button>
  }
</mat-dialog-actions>
```

### 6.4 Logic

```ts
private billingApi = inject(BillingApiService);
private bookingApi = inject(BookingApiService);
private destroyRef = inject(DestroyRef);

ngOnInit(): void {
  this.loadBill();
}

loadBill(): void {
  this.loading.set(true);
  this.billingApi.getByBookingId(this.bookingId()).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: data => this.billDetails.set(data),
    error: (err: any) => this.error.set(this.extractErrorMessage(err))
  });
}

onPaymentComplete(): void {
  this.step.set('folio');
  this.loadBill(); // refresh to see updated status
}

processCheckOut(): void {
  this.loading.set(true);
  this.bookingApi.checkOut(this.bookingId()).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: () => this.step.set('confirm'),
    error: (err: any) => this.error.set(this.extractErrorMessage(err))
  });
}
```

**Note:** The `BookingApiService` must provide `checkOut(id: number): Observable<void>` which calls `POST /api/v1/bookings/{id}/checkout`.

## 7. Integration Notes

- The `BillingTabComponent` and `CheckoutDialogComponent` both use `PaymentFormComponent`; ensure it is exported correctly.
- The `BillingApiService` must implement:
  - `getByBookingId(bookingId: number): Observable<any>` → `GET /api/v1/billing/{bookingId}`
  - `pay(bookingId: number, dto: PaymentRequestDTO): Observable<void>` → `POST /api/v1/billing/{bookingId}/pay`
- The `PaymentRequestDTO` is `{ amount: number, paymentMethod: string, transactionId: string }` as per Swagger.
- The check‑out flow is a separate dialog, so the booking action modal stays clean and focused. The modal itself never needs to know about the checkout dialog's internal steps.

## 8. Responsive Behaviour

- Payment form fields stack vertically (default Angular Material behavior).
- Check‑out dialog uses `width: 95vw` on mobile.

## 9. Self‑Review Checklist

- [ ] Billing tab loads and displays folio details (guest, totals, items).
- [ ] Payment form shows/hides on button toggle; submits correct data.
- [ ] After successful payment, the bill details refresh and show “Fully paid”.
- [ ] Check‑Out button in details tab opens the check‑out dialog.
- [ ] Dialog shows folio; if unpaid, allows proceeding to payment; if paid, allows direct check‑out.
- [ ] Payment in the dialog works and upon completion, the folio refreshes.
- [ ] Check‑out API is called and on success, confirmation is shown.
- [ ] On closing the confirmation step, the dialog returns `true` to the modal, which in turn closes the modal with `true`, refreshing the dashboard.
- [ ] All error states handled gracefully.
- [ ] No console errors; subscriptions cleaned.

## 10. Future Part 6 (Final Integration)

The final part will ensure:

- The dashboard’s refresh mechanism is robust after all actions.
- Any remaining responsive polish.
- Consolidation of duplicate `extractErrorMessage` into a shared utility (optional but recommended).
- Final testing checklist.

