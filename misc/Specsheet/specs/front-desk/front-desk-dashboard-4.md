# Specsheet: Front Desk Dashboard – Part 4 (Booking Action Modal – Room Service)

## 1. Purpose

- Extend the `BookingActionModalComponent` with a **Room Service** tab that allows front desk agents to manage guest requests without leaving the modal.
- The tab is built from four reusable, standalone panels:
  - `FoodOrderPanelComponent` – browse menu, manage cart, place food orders for the booking.
  - `HousekeepingRequestPanelComponent` – request housekeeping for a specific room in the booking.
  - `MaintenanceRequestPanelComponent` – request maintenance for a specific room in the booking.
  - `InternalTicketPanelComponent` – create an internal housekeeping or maintenance ticket (not tied to a room).
- Each panel is self‑contained and communicates only via its inputs and outputs; the parent tab component orchestrates them minimally.
- All panels reuse existing shared components (`MenuGridComponent`, `CartDrawerComponent`) from the customer module where applicable.

## 2. Files to Create / Modify

| File                                                                            | Action                                                                                                                          |
| ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| **Modify:** `booking-action-modal.component.ts`                                 | Add a `MatTabGroup`; move existing details into a “Details” tab; add a “Room Service” tab that hosts `RoomServiceTabComponent`. |
| **Modify:** `booking-action-modal.component.html`                               | Restructure template with `<mat-tab-group>`.                                                                                    |
| **New:** `room-service-tab/room-service-tab.component.ts`                       | Container for the four panels.                                                                                                  |
| **New:** `room-service-tab/room-service-tab.component.html`                     | Template arranging the panels.                                                                                                  |
| **New:** `food-order-panel/food-order-panel.component.ts`                       | Food ordering panel.                                                                                                            |
| **New:** `food-order-panel/food-order-panel.component.html`                     | Template.                                                                                                                       |
| **New:** `housekeeping-request-panel/housekeeping-request-panel.component.ts`   | Housekeeping request form.                                                                                                      |
| **New:** `housekeeping-request-panel/housekeeping-request-panel.component.html` | Template.                                                                                                                       |
| **New:** `maintenance-request-panel/maintenance-request-panel.component.ts`     | Maintenance request form.                                                                                                       |
| **New:** `maintenance-request-panel/maintenance-request-panel.component.html`   | Template.                                                                                                                       |
| **New:** `internal-ticket-panel/internal-ticket-panel.component.ts`             | Internal ticket form.                                                                                                           |
| **New:** `internal-ticket-panel/internal-ticket-panel.component.html`           | Template.                                                                                                                       |

All new components reside under `src/app/features/front-desk/components/booking-action-modal/`.

## 3. BookingActionModalComponent – Template Refactoring

### 3.1 Goal

Wrap the existing modal content inside a `MatTabGroup` with two tabs:

- **Details** – shows booking info and core action buttons (from Part 3).
- **Room Service** – contains `<app-room-service-tab [booking]="booking()" />`.

The rest of the modal (title, close button) remains unchanged.

### 3.2 Updated Template (excerpt)

```html
<h2 mat-dialog-title>Booking #{{ booking().id }}</h2>
<mat-dialog-content>
  <mat-tab-group>
    <mat-tab label="Details">
      <div class="modal-content">
        <!-- existing booking details, actions, error alert exactly as in Part 3 -->
        ...
      </div>
    </mat-tab>
    <mat-tab label="Room Service">
      <app-room-service-tab [booking]="booking()" />
    </mat-tab>
  </mat-tab-group>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button
    mat-button
    mat-dialog-close
  >
    Close
  </button>
</mat-dialog-actions>
```

### 3.3 Component Class Updates

- Add `MatTabsModule` to imports.
- No other changes needed; the `booking` signal remains the source of truth.

## 4. RoomServiceTabComponent (Container)

**Selector:** `app-room-service-tab`  
**Standalone:** `true`  
**Input:** `booking = input.required<Booking>()`  
**Imports:** `CommonModule`, `FoodOrderPanelComponent`, `HousekeepingRequestPanelComponent`, `MaintenanceRequestPanelComponent`, `InternalTicketPanelComponent`, `MatDividerModule`.

**Template:**

```html
<div class="room-service-tab">
  <app-food-order-panel [bookingId]="booking().id" />
  <mat-divider></mat-divider>
  <app-housekeeping-request-panel [rooms]="booking().rooms" />
  <mat-divider></mat-divider>
  <app-maintenance-request-panel [rooms]="booking().rooms" />
  <mat-divider></mat-divider>
  <app-internal-ticket-panel />
</div>
```

**No additional logic.** The container simply passes data down.

## 5. FoodOrderPanelComponent

### 5.1 API

- **Selector:** `app-food-order-panel`
- **Standalone:** `true`
- **Input:** `bookingId = input.required<number>()`
- **Imports:** `CommonModule`, `MenuGridComponent`, `CartDrawerComponent`, `MatSnackBarModule`, `OrderApiService`, `MenuItemApiService`, `DestroyRef`.

### 5.2 State

```ts
menuItems = signal<MenuItem[]>([]);
cartItems = signal<OrderItem[]>([]);
cartOpen = signal(false);
loading = signal(false);
error = signal<string | null>(null);
```

### 5.3 Template

```html
<div class="food-order-panel">
  <h3>Order Food</h3>
  @if (loading() && menuItems().length === 0) {
  <mat-spinner diameter="30"></mat-spinner>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="loadMenu()"
    >
      Retry
    </button>
  </app-alert>
  } @else {
  <app-menu-grid
    [menuItems]="menuItems()"
    (addToCart)="onAddToCart($event)"
    (updateQuantity)="onUpdateCartQty($event)"
  />
  }
  <app-cart-drawer
    [cartItems]="cartItems()"
    [isOpen]="cartOpen()"
    (cartToggle)="cartOpen.set(!cartOpen())"
    (checkout)="placeOrder()"
    (updateQuantity)="onUpdateCartQty($event)"
  />
</div>
```

### 5.4 Logic

```ts
private menuItemApi = inject(MenuItemApiService);
private orderApi = inject(OrderApiService);
private snackBar = inject(MatSnackBar);
private destroyRef = inject(DestroyRef);

ngOnInit(): void {
  this.loadMenu();
}

loadMenu(): void {
  this.loading.set(true);
  this.menuItemApi.getAll({ isAvailable: true, pageSize: 200 }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: res => this.menuItems.set(res.data),
    error: (err: any) => this.error.set(this.extractErrorMessage(err))
  });
}

onAddToCart(item: MenuItem): void {
  this.cartItems.update(items => {
    const existing = items.find(i => i.menuItemId === item.id);
    if (existing) {
      return items.map(i => i.menuItemId === item.id ? { ...i, quantity: i.quantity + 1 } : i);
    }
    return [...items, { menuItemId: item.id, name: item.name, price: item.price, quantity: 1 }];
  });
  this.snackBar.open(`${item.name} added to cart`, 'View Cart', { duration: 2000 }).onAction().subscribe(() => {
    this.cartOpen.set(true);
  });
}

onUpdateCartQty(event: { menuItemId: number; delta: number }): void {
  this.cartItems.update(items => {
    return items.map(i => i.menuItemId === event.menuItemId ? { ...i, quantity: Math.max(0, i.quantity + event.delta) } : i)
                .filter(i => i.quantity > 0);
  });
}

placeOrder(): void {
  if (this.cartItems().length === 0) return;
  const confirmRef = this.dialog.open(ConfirmDialogComponent, {
    data: { title: 'Confirm Order', message: 'Place this food order for the guest?' },
  });
  confirmRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (!confirmed) return;
    this.orderApi.create({
      bookingId: this.bookingId(),
      items: this.cartItems().map(i => ({ menuItemId: i.menuItemId, quantity: i.quantity })),
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.snackBar.open('Order placed successfully', 'Close', { duration: 3000 });
        this.cartItems.set([]);  // clear cart
      },
      error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  });
}
```

**Note:** `MenuItem`, `OrderItem`, `MenuGridComponent`, `CartDrawerComponent` are imported from the customer module. The `ConfirmDialogComponent` is imported from shared.

## 6. HousekeepingRequestPanelComponent

### 6.1 API

- **Selector:** `app-housekeeping-request-panel`
- **Standalone:** `true`
- **Input:** `rooms = input.required<BookingRoom[]>()`
- **Imports:** `CommonModule`, `ReactiveFormsModule`, `MatFormFieldModule`, `MatInputModule`, `MatSelectModule`, `MatButtonModule`, `MatProgressSpinnerModule`, `MatSnackBarModule`, `HousekeepingApiService`, `ConfirmDialogComponent`, `DestroyRef`.

### 6.2 State

```ts
selectedRoomId = new FormControl<number>(this.rooms()[0]?.roomId ?? 0, {
  nonNullable: true,
  validators: Validators.required,
});
description = new FormControl("", [
  Validators.required,
  Validators.minLength(5),
]);
submitting = signal(false);
```

### 6.3 Template

```html
<div class="request-panel">
  <h3>Request Housekeeping</h3>
  <form (ngSubmit)="submit()">
    <mat-form-field appearance="outline">
      <mat-label>Room</mat-label>
      <mat-select [formControl]="selectedRoomId">
        @for (room of rooms(); track room.roomId) {
        <mat-option [value]="room.roomId"
          >{{ room.roomNumber ?? 'Room ' + room.roomId }}</mat-option
        >
        }
      </mat-select>
    </mat-form-field>
    <mat-form-field appearance="outline">
      <mat-label>Description</mat-label>
      <textarea
        matInput
        [formControl]="description"
        rows="2"
      ></textarea>
      <mat-error *ngIf="description.invalid && description.touched"
        >Min 5 characters required</mat-error
      >
    </mat-form-field>
    <button
      mat-raised-button
      color="primary"
      type="submit"
      [disabled]="description.invalid || submitting()"
    >
      @if (submitting()) { <mat-spinner diameter="20"></mat-spinner> } Submit
      Request
    </button>
  </form>
</div>
```

### 6.4 Logic

```ts
private hkApi = inject(HousekeepingApiService);
private snackBar = inject(MatSnackBar);
private dialog = inject(MatDialog);
private destroyRef = inject(DestroyRef);

submit(): void {
  if (this.submitting() || this.selectedRoomId.invalid || this.description.invalid) return;
  const confirmRef = this.dialog.open(ConfirmDialogComponent, {
    data: { title: 'Confirm Request', message: 'Send a housekeeping request for the selected room?' },
  });
  confirmRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (!confirmed) return;
    this.submitting.set(true);
    this.hkApi.trigger(this.selectedRoomId.value, { description: this.description.value }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.submitting.set(false))
    ).subscribe({
      next: () => {
        this.snackBar.open('Housekeeping request sent', 'Close', { duration: 3000 });
        this.description.reset();
      },
      error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  });
}
```

## 7. MaintenanceRequestPanelComponent

Identical to housekeeping except it uses `MaintenanceApiService` and the endpoint `POST /maintenance/trigger/{roomId}`. Follow the same pattern.

## 8. InternalTicketPanelComponent

### 8.1 API

- **Selector:** `app-internal-ticket-panel`
- **Standalone:** `true`
- **Inputs:** none
- **Imports:** `CommonModule`, `ReactiveFormsModule`, `MatButtonToggleModule`, `MatFormFieldModule`, `MatInputModule`, `MatButtonModule`, `MatProgressSpinnerModule`, `MatSnackBarModule`, `HousekeepingApiService`, `MaintenanceApiService`, `ConfirmDialogComponent`, `DestroyRef`.

### 8.2 State

```ts
ticketType = new FormControl<"housekeeping" | "maintenance">("housekeeping", {
  nonNullable: true,
});
location = new FormControl("", [
  Validators.required,
  Validators.maxLength(200),
]);
description = new FormControl("", [
  Validators.required,
  Validators.minLength(5),
]);
submitting = signal(false);
```

### 8.3 Template

```html
<div class="internal-ticket-panel">
  <h3>Create Internal Ticket</h3>
  <form (ngSubmit)="submit()">
    <mat-button-toggle-group [formControl]="ticketType">
      <mat-button-toggle value="housekeeping">Housekeeping</mat-button-toggle>
      <mat-button-toggle value="maintenance">Maintenance</mat-button-toggle>
    </mat-button-toggle-group>
    <mat-form-field appearance="outline">
      <mat-label>Location</mat-label>
      <input
        matInput
        [formControl]="location"
      />
      <mat-error *ngIf="location.invalid && location.touched"
        >Location is required</mat-error
      >
    </mat-form-field>
    <mat-form-field appearance="outline">
      <mat-label>Description</mat-label>
      <textarea
        matInput
        [formControl]="description"
        rows="2"
      ></textarea>
      <mat-error *ngIf="description.invalid && description.touched"
        >Min 5 characters required</mat-error
      >
    </mat-form-field>
    <button
      mat-raised-button
      color="primary"
      type="submit"
      [disabled]="location.invalid || description.invalid || submitting()"
    >
      @if (submitting()) { <mat-spinner diameter="20"></mat-spinner> } Create
      Ticket
    </button>
  </form>
</div>
```

### 8.4 Logic

```ts
private hkApi = inject(HousekeepingApiService);
private mtApi = inject(MaintenanceApiService);
private snackBar = inject(MatSnackBar);
private dialog = inject(MatDialog);
private destroyRef = inject(DestroyRef);

submit(): void {
  if (this.submitting() || this.location.invalid || this.description.invalid) return;
  const confirmRef = this.dialog.open(ConfirmDialogComponent, {
    data: { title: 'Confirm Ticket', message: `Create an internal ${this.ticketType.value} ticket?` },
  });
  confirmRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (!confirmed) return;
    this.submitting.set(true);
    const body = { location: this.location.value, description: this.description.value };
    const request$ = this.ticketType.value === 'housekeeping'
      ? this.hkApi.createInternal(body)
      : this.mtApi.createInternal(body);
    request$.pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.submitting.set(false))
    ).subscribe({
      next: () => {
        this.snackBar.open('Internal ticket created', 'Close', { duration: 3000 });
        this.location.reset();
        this.description.reset();
      },
      error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  });
}
```

**Note:** The `HousekeepingApiService` and `MaintenanceApiService` must provide `createInternal(body: { location: string; description: string }): Observable<void>`.

## 9. Integration Notes

- All four panels are imported into `RoomServiceTabComponent`, which is imported into `BookingActionModalComponent`.
- The `BookingActionModalComponent` now requires `MatTabsModule`, and the tab structure wraps the existing details content.
- The `extractErrorMessage` helper should be defined in a shared utility or duplicated in each panel (to keep components self‑contained, we can define it locally in each panel component).
- No changes are needed to the dashboard; the modal’s external API (close with `true`/`undefined`) remains unchanged.
- The food ordering panel reuses `MenuGridComponent` and `CartDrawerComponent` from the customer feature; ensure they are exported and importable.

## 10. Responsive Behaviour

- Panels stack vertically, full width on mobile.
- Food order menu grid and cart drawer handle their own responsive behaviour as already implemented.

## 11. Self‑Review Checklist

- [ ] Modal now shows two tabs: Details and Room Service.
- [ ] Details tab retains all previous functionality (check‑in, cancel, extend).
- [ ] Room Service tab contains four panels separated by dividers.
- [ ] Food Order panel loads menu, allows adding/removing items, and places orders with confirmation.
- [ ] Housekeeping and Maintenance panels allow selecting a room and submitting a request with confirmation.
- [ ] Internal Ticket panel creates a ticket for the selected type with location and description.
- [ ] All panels handle errors gracefully (snackbar or alert).
- [ ] No console errors, all subscriptions cleaned.

This spec sheet adds the room service capability without bloating the modal, maintaining the granularity set by earlier parts.

