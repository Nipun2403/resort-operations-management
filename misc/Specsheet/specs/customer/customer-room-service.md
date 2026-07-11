# Specsheet: Customer Room Service Page

## 1. Purpose

- Replace the `PlaceholderCustomerRoomServiceComponent` with a highly modular Room Service page.
- The page acts as a **thin orchestrator** that loads the user’s active booking and delegates all functionality to dedicated, standalone child components.
- Three sub‑sections are exposed via Material tabs:
  - **Food Order** – browse menu, add items to cart, review cart, place order.
  - **Request Service** – request housekeeping or maintenance for a specific room.
  - **My Requests** – view history of personal housekeeping/maintenance requests.
- A reusable `CustomerBookingFacade` is introduced to avoid duplicating the active‑booking lookup logic across Dashboard and Room Service.

## 2. New Shared Facade – `CustomerBookingFacade`

**File:** `src/app/features/user/facades/customer-booking.facade.ts`  
**Provided in:** `root`.

**API:**

```ts
@Injectable({ providedIn: "root" })
export class CustomerBookingFacade {
  private authApi = inject(AuthApiService);
  private bookingApi = inject(BookingApiService);

  getActiveBooking(): Observable<Booking | null> {
    return this.authApi.getMe().pipe(
      switchMap((me) => {
        const email =
          me.claims?.find((c) => c.type === ".../claims/name")?.value ?? "";
        if (!email) return of(null);
        return this.bookingApi
          .getAll({
            guestQuery: email,
            status: "CheckedIn",
            pageNumber: 1,
            pageSize: 1,
          })
          .pipe(map((res) => (res.data.length > 0 ? res.data[0] : null)));
      }),
    );
  }

  getCurrentCustomerProfile(): Observable<CustomerProfile> {
    return this.authApi.getMe().pipe(
      map((me) => ({
        firstName:
          me.claims?.find((c) => c.type === ".../claims/givenname")?.value ??
          "",
        lastName:
          me.claims?.find((c) => c.type === ".../claims/surname")?.value ?? "",
        email:
          me.claims?.find((c) => c.type === ".../claims/name")?.value ?? "",
      })),
    );
  }
}
```

(Use the exact claim URIs as defined earlier.)

Both the **Dashboard** and **Room Service** will inject this facade and use `getActiveBooking()` directly, eliminating duplicate code.

## 3. Route & Navigation

- Path: `/user/room-service` (lazy‑loaded under Customer Shell).
- **Overwrite** the placeholder file: `src/app/features/user/pages/room-service.component.ts`.

## 4. Authorization

- Already protected by `customerGuard`.

## 5. RoomServicePage (Orchestrator)

**Selector:** `app-customer-room-service`  
**Standalone:** `true`  
**Imports:** `CommonModule`, `MatTabsModule`, `MatProgressSpinnerModule`, `AlertComponent`, `FoodOrderComponent`, `RequestServiceComponent`, `MyRequestsComponent`.  
**Exact import paths:** (abbreviated; agent must use full paths).

**Template (exact – using `@if` alias to unwrap signal safely):**

```html
<div class="room-service">
  @if (loadingActiveBooking()) {
  <mat-spinner diameter="40"></mat-spinner>
  } @else if (activeBookingError()) {
  <app-alert
    type="error"
    [message]="activeBookingError()!"
    (closed)="activeBookingError.set(null)"
  >
    <button
      mat-button
      (click)="loadActiveBooking()"
    >
      Retry
    </button>
  </app-alert>
  } @else if (activeBooking(); as booking) {
  <mat-tab-group>
    <mat-tab label="Food Order">
      <app-food-order
        [activeBookingId]="booking.id"
        (orderPlaced)="onOrderPlaced()"
      />
    </mat-tab>
    <mat-tab label="Request Service">
      <app-request-service
        [activeBooking]="booking"
        (requestCreated)="onRequestCreated()"
      />
    </mat-tab>
    <mat-tab label="My Requests">
      <app-my-requests
        [roomIds]="roomIds()"
        [refresh]="refreshRequests()"
      />
    </mat-tab>
  </mat-tab-group>
  } @else {
  <mat-card class="no-booking-card">
    <mat-card-content>
      <mat-icon>info</mat-icon>
      <p>You need an active stay (Checked In) to use room service.</p>
      <p>Please visit <a routerLink="/user/bookings">My Bookings</a>.</p>
    </mat-card-content>
  </mat-card>
  }
</div>
```

**State & Logic:**

```ts
private facade = inject(CustomerBookingFacade);
activeBooking = signal<Booking | null>(null);
loadingActiveBooking = signal(false);
activeBookingError = signal<string | null>(null);
refreshRequests = signal(0);

roomIds = computed(() => {
  const booking = this.activeBooking();
  return booking ? booking.rooms.map(r => r.roomId).filter(id => id != null) as number[] : [];
});

ngOnInit(): void {
  this.loadActiveBooking();
}

loadActiveBooking(): void {
  this.loadingActiveBooking.set(true);
  this.activeBookingError.set(null);
  this.facade.getActiveBooking().pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loadingActiveBooking.set(false))
  ).subscribe({
    next: booking => this.activeBooking.set(booking),
    error: (err: any) => this.activeBookingError.set(this.extractErrorMessage(err))
  });
}

onOrderPlaced(): void {
  // Only show a snackbar or log – no need to refresh My Requests tab.
}

onRequestCreated(): void {
  this.refreshRequests.update(n => n + 1);
}
```

## 6. FoodOrderComponent

**Selector:** `app-food-order`  
**Standalone:** `true`  
**Inputs:** `activeBookingId = input.required<number>()`  
**Outputs:** `orderPlaced = output<void>()`

**Internal composition:**

```html
<div class="food-order">
  <app-menu-grid
    [menuItems]="menuItems()"
    (addToCart)="onAddToCart($event)"
  />
  <app-cart-drawer
    [cartItems]="cartItems()"
    [isOpen]="cartOpen()"
    (cartToggle)="cartOpen.set(!cartOpen())"
    (checkout)="placeOrder()"
  />
</div>
```

**State:**

- `menuItems = signal<MenuItem[]>([])` (fetched on init: `GET /menu-items?isAvailable=true&pageSize=200`)
- `cartItems = signal<OrderItem[]>([])` where `OrderItem = { menuItemId: number; name: string; price: number; quantity: number }`
- `cartOpen = signal(false)` – toggles cart visibility on mobile.
- `canCheckout = computed(() => this.cartItems().length > 0)`

**Methods:**

- `onAddToCart(item: MenuItem)`: adds to cart or increments quantity; shows snackbar “Added to cart [View Cart]” with an action that opens the cart.
- `placeOrder()`: if canCheckout, call `POST /orders` with `{ bookingId: activeBookingId(), items: cartItems().map(i => ({ menuItemId: i.menuItemId, quantity: i.quantity })) }`. On success, snackbar “Order placed”, clear cart, emit `orderPlaced`. On error, snackbar error.

**Validation:** cart must not be empty.

## 7. MenuGridComponent

**Selector:** `app-menu-grid`  
**Standalone:** `true`  
**Inputs:** `menuItems = input.required<MenuItem[]>()`  
**Outputs:** `addToCart = output<MenuItem>()`

**Template:** Grid of `mat-card`s. Each card shows name, price, and an “Add” button. On click, emit `addToCart`.

**Responsive:** CSS Grid: `grid-template-columns: repeat(3, 1fr)` on desktop, `repeat(1, 1fr)` on mobile.

## 8. CartDrawerComponent

**Selector:** `app-cart-drawer`  
**Standalone:** `true`  
**Inputs:** `cartItems = input.required<OrderItem[]>()`, `isOpen = input.required<boolean>()`  
**Outputs:** `cartToggle = output<void>()`, `checkout = output<void>()`

**Template:**

```html
<div
  class="cart-drawer"
  [class.open]="isOpen()"
>
  <button
    mat-raised-button
    (click)="cartToggle.emit()"
  >
    <mat-icon>shopping_cart</mat-icon>
    Cart ({{ itemCount() }}) – {{ subtotal() | currency }}
  </button>
  @if (isOpen()) {
  <div class="cart-panel">
    @for (item of cartItems(); track item.menuItemId) {
    <div class="cart-item">
      <span>{{ item.name }} x{{ item.quantity }}</span>
      <span>{{ item.price * item.quantity | currency }}</span>
    </div>
    }
    <button
      mat-raised-button
      color="primary"
      (click)="checkout.emit()"
      [disabled]="cartItems().length === 0"
    >
      Place Order
    </button>
  </div>
  }
</div>
```

**Computed signals:**

- `itemCount = computed(() => this.cartItems().reduce((s, i) => s + i.quantity, 0))`
- `subtotal = computed(() => this.cartItems().reduce((s, i) => s + i.price * i.quantity, 0))`

**Mobile behaviour:** On screens ≤767px, the cart drawer is a bottom sheet that slides up. Use a combination of CSS and `isOpen` to animate.

## 9. RequestServiceComponent – Updated with Room Dropdown

**Selector:** `app-request-service`  
**Standalone:** `true`  
**Inputs:** `activeBooking = input.required<Booking>()`  
**Outputs:** `requestCreated = output<void>()`

**Template (exact – Angular 18 control flow only):**

```html
<div class="request-service">
  <mat-card>
    <mat-card-header
      ><mat-card-title
        >Request Housekeeping or Maintenance</mat-card-title
      ></mat-card-header
    >
    <mat-card-content>
      <mat-button-toggle-group
        [formControl]="requestType"
        aria-label="Service type"
      >
        <mat-button-toggle value="housekeeping"
          ><mat-icon>cleaning_services</mat-icon>
          Housekeeping</mat-button-toggle
        >
        <mat-button-toggle value="maintenance"
          ><mat-icon>build</mat-icon> Maintenance</mat-button-toggle
        >
      </mat-button-toggle-group>

      <mat-form-field appearance="outline">
        <mat-label>Room</mat-label>
        <mat-select [formControl]="selectedRoomId">
          @for (room of activeBooking().rooms; track room.roomId) {
          <mat-option [value]="room.roomId">
            {{ room.roomNumber ?? 'Room ' + room.roomId }}
          </mat-option>
          }
        </mat-select>
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Description</mat-label>
        <textarea
          matInput
          [formControl]="description"
          rows="3"
        ></textarea>
        @if (description.invalid && description.touched) {
        <mat-error>Description is required (min 5 characters).</mat-error>
        }
      </mat-form-field>
    </mat-card-content>
    <mat-card-actions>
      <button
        mat-raised-button
        color="primary"
        (click)="submitRequest()"
        [disabled]="description.invalid || submitting()"
      >
        @if (submitting()) { <mat-spinner diameter="20"></mat-spinner> } Submit
        Request
      </button>
    </mat-card-actions>
  </mat-card>
</div>
```

**State:**

- `requestType = new FormControl<'housekeeping' | 'maintenance'>('housekeeping', { nonNullable: true })`
- `selectedRoomId = new FormControl<number>(this.activeBooking().rooms[0]?.roomId ?? 0, { nonNullable: true, validators: [Validators.required] })`
- `description = new FormControl('', [Validators.required, Validators.minLength(5)])`
- `submitting = signal(false)`

The `roomId` and `roomNumber` used for submission are derived from `selectedRoomId` value by looking up in `activeBooking().rooms`.

**Submit:** Use `selectedRoomId.value` to get the room ID, then call appropriate endpoint (`POST /housekeeping/trigger/{roomId}` or `POST /maintenance/trigger/{roomId}`) with `{ description: description.value }`. On success, snackbar, reset form (description, keep room and type), emit `requestCreated`. On error, snackbar error.

## 10. MyRequestsComponent – Secure per‑room fetching

**Selector:** `app-my-requests`  
**Standalone:** `true`  
**Inputs:** `roomIds = input.required<number[]>()`, `refresh = input(0)`  
**Outputs:** none.

**Template:**

```html
<div class="my-requests">
  @if (loading()) {
  <mat-spinner diameter="30"></mat-spinner>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchRequests()"
    >
      Retry
    </button>
  </app-alert>
  } @if (requests().length > 0) {
  <table
    mat-table
    [dataSource]="requests()"
    matSort
    matSortDisableClear
  >
    <ng-container matColumnDef="type"
      ><th
        mat-header-cell
        *matHeaderCellDef
      >
        Type
      </th>
      <td
        mat-cell
        *matCellDef="let r"
      >
        {{ r.type }}
      </td></ng-container
    >
    <ng-container matColumnDef="room"
      ><th
        mat-header-cell
        *matHeaderCellDef
      >
        Room
      </th>
      <td
        mat-cell
        *matCellDef="let r"
      >
        {{ r.roomNumber }}
      </td></ng-container
    >
    <ng-container matColumnDef="description"
      ><th
        mat-header-cell
        *matHeaderCellDef
      >
        Description
      </th>
      <td
        mat-cell
        *matCellDef="let r"
      >
        {{ r.description }}
      </td></ng-container
    >
    <ng-container matColumnDef="status"
      ><th
        mat-header-cell
        *matHeaderCellDef
      >
        Status
      </th>
      <td
        mat-cell
        *matCellDef="let r"
      >
        {{ r.status }}
      </td></ng-container
    >
    <ng-container matColumnDef="createdAt"
      ><th
        mat-header-cell
        *matHeaderCellDef
      >
        Created
      </th>
      <td
        mat-cell
        *matCellDef="let r"
      >
        {{ r.createdAt | date:'short' }}
      </td></ng-container
    >
    <tr
      mat-header-row
      *matHeaderRowDef="displayedColumns"
    ></tr>
    <tr
      mat-row
      *matRowDef="let row; columns: displayedColumns"
    ></tr>
  </table>
  <!-- Client-side paginator if needed, or no pagination for small dataset -->
  } @else {
  <p>No requests found.</p>
  }
</div>
```

**State:**

- `requests = signal<CustomerRequest[]>([])`
- `loading = signal(false)`, `error = signal<string | null>(null)`
- `displayedColumns = ['type', 'room', 'description', 'status', 'createdAt']`

**Normalised DTO:**

```ts
interface CustomerRequest {
  id: number;
  type: "Housekeeping" | "Maintenance";
  roomId: number;
  roomNumber: string;
  description: string;
  status: string;
  createdAt: string;
}
```

**Fetch logic (secure, per room):**

- Effect watches `refresh` and `roomIds`.
- For each `roomId` in `roomIds()`, create two API calls:
  - `this.housekeepingApi.getAll({ roomId, pageSize: 100 })` // assuming API supports roomId filter
  - `this.maintenanceApi.getAll({ roomId, pageSize: 100 })`  
    (If the API does not support `roomId` filter, we can fallback to fetching `pageSize=200` and filter client‑side, but the user confirmed the backend supports filtering by room ID, so we use that.)
- Collect all observables into a flat array, use `forkJoin` to execute them in parallel.
- On result, map each response to `CustomerRequest` objects (housekeeping entries have `type: 'Housekeeping'`, maintenance have `type: 'Maintenance'`), extract room number from the active booking context? Since the API response includes `roomId` and `location` (room number?), we can map accordingly. The housekeeping/maintenance DTOs include `location` which may be room number. We'll map:
  ```ts
  housekeepingResponse.data.map((hk) => ({
    id: hk.id,
    type: "Housekeeping" as const,
    roomId: hk.roomId,
    roomNumber: hk.location ?? `Room ${hk.roomId}`,
    description: hk.description ?? "",
    status: hk.status,
    createdAt: hk.createdAt,
  }));
  ```
  Similarly for maintenance.
- Merge both arrays, sort by `createdAt` descending, set `requests`.

**Important:** The `requests` signal is used directly as `[dataSource]`. For client‑side pagination, we could use `MatTableDataSource` but it's simpler to show all (the number of requests per guest is typically small). We'll display all with no paginator, or add a simple `MatPaginator` with `length` bound to `requests().length` and `pageSize` options. For now, omit paginator.

**Auto-refresh:** The `refresh` input triggers a fetch. The `RequestServiceComponent` emit `requestCreated`, which calls `onRequestCreated` in parent, incrementing `refreshRequests`. So new requests appear automatically.

## 11. Models & Folder Structure

```
src/app/features/user/
  facades/
    customer-booking.facade.ts
  pages/
    room-service.component.ts
    room-service.component.html
    room-service.component.scss
  components/
    food-order/
      food-order.component.ts, .html, .scss
      menu-grid.component.ts, .html, .scss
      cart-drawer.component.ts, .html, .scss
    request-service/
      request-service.component.ts, .html, .scss
    my-requests/
      my-requests.component.ts, .html, .scss
  models/
    customer-request.model.ts
    order-item.model.ts
```

## 12. Responsive Behaviour

- **Food Order:** Desktop shows menu grid (left) and cart panel (right); tablet shows menu grid full width and a floating cart button that opens a drawer; mobile shows single column menu and a bottom sheet cart.
- **Request Service:** Card full width.
- **My Requests:** Table scrolls horizontally on mobile.

Use CSS Grid and Flexbox with media queries at 767px and 1024px.

## 13. Self‑Review Checklist

- [ ] Room Service page only loads if user has an active booking; otherwise shows informative message.
- [ ] Food Order: menu loads, items can be added to cart via snackbar feedback, cart shows correct totals, order can be placed, and cart clears.
- [ ] Request Service: housekeeping/maintenance tickets created with selected room and description; room dropdown populated correctly.
- [ ] My Requests: only requests for the user's rooms are fetched (not all hotel requests); after creating a request, the requests tab refreshes automatically.
- [ ] All child components are standalone and decoupled; the orchestrator is under 80 lines.
- [ ] No client‑side filtering of excessive records; security‑safe.
- [ ] Angular 18 control flow used exclusively; no `*ngIf`.
- [ ] Responsive layout adapts correctly across mobile, tablet, and desktop.
- [ ] No console errors, subscriptions properly cleaned.

## 14. Implementation Constraints

- Angular 18 control flow, standalone components, signals, `takeUntilDestroyed`.
- Use `CustomerBookingFacade` for active booking retrieval.
- Food order must use optimistic cart UX (snackbar on add, cart opened manually).
- Cart drawer on mobile must be a bottom sheet.
- My Requests must fetch per‑room using `forkJoin` and normalise into `CustomerRequest[]`. No bulk fetch of other guests' data.
- All forms must use specific error messages as specified.
- Room selection dropdown in Request Service must default to the first room of the booking, but allow the guest to choose any room.

