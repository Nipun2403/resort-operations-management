# Specsheet: Front Desk – New Booking Wizard

## 1. Purpose
- Replace the `PlaceholderNewBookingComponent` with a dedicated New Booking wizard for the front desk.
- The wizard is a multi‑step form (Guest → Dates & Guests → Rooms → Amenities → Review → Confirm) that creates a booking for any guest (walk‑in or registered).
- After successful creation, the agent is presented with a success dialog containing the booking ID and a **“Check‑In Now”** button. Checking in immediately assigns a room and returns the room number.
- The wizard uses reusable components for room selection, amenities, and review steps, but the guest details step is custom for front desk (manual entry, no JWT pre‑fill).

## 2. Route & Navigation
- Path: `/operations/front-desk/new-booking` (already lazy‑loaded under Front Desk Shell).
- **Overwrite** the placeholder file: `src/app/features/front-desk/pages/new-booking.component.ts`.

## 3. Authorization
- Already protected by `frontDeskGuard`.

## 4. Component API (FrontDeskBookingWizardComponent)
- **Selector**: `app-front-desk-new-booking` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatStepperModule`, `MatFormFieldModule`, `MatInputModule`, `MatButtonModule`, `MatIconModule`, `MatDatepickerModule`, `MatNativeDateModule`, `MatSelectModule`, `MatCheckboxModule`, `MatDividerModule`, `MatProgressSpinnerModule`, `MatSnackBarModule`, `MatDialogModule`, `AlertComponent`, `BookingApiService`, `RoomTypeApiService`, `AmenityApiService`, `ConfirmDialogComponent`, `DestroyRef`.
- **Exact import paths** (abbreviated; agent must include full paths).

## 5. Template (exact)
```html
<div class="new-booking-page">
  <h1>New Booking</h1>

  <mat-stepper linear #stepper orientation="horizontal">
    <!-- Step 1: Guest Details -->
    <mat-step [stepControl]="guestForm" label="Guest Details">
      <form [formGroup]="guestForm">
        <mat-form-field appearance="outline">
          <mat-label>First Name</mat-label>
          <input matInput formControlName="firstName" />
          <mat-error *ngIf="guestForm.get('firstName')?.invalid && guestForm.get('firstName')?.touched">First name is required (min 2 characters).</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Last Name</mat-label>
          <input matInput formControlName="lastName" />
          <mat-error *ngIf="guestForm.get('lastName')?.invalid && guestForm.get('lastName')?.touched">Last name is required (min 2 characters).</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" type="email" />
          <mat-error *ngIf="guestForm.get('email')?.invalid && guestForm.get('email')?.touched">A valid email is required.</mat-error>
        </mat-form-field>
        <div class="step-actions">
          <button mat-button matStepperNext>Next</button>
        </div>
      </form>
    </mat-step>

    <!-- Step 2: Dates & Guests -->
    <mat-step [stepControl]="datesForm" label="Dates & Guests">
      <form [formGroup]="datesForm">
        <mat-form-field appearance="outline">
          <mat-label>Check‑in Date</mat-label>
          <input matInput [matDatepicker]="cinPicker" formControlName="checkInDate" />
          <mat-datepicker-toggle matSuffix [for]="cinPicker"></mat-datepicker-toggle>
          <mat-datepicker #cinPicker></mat-datepicker>
          <mat-error *ngIf="datesForm.get('checkInDate')?.invalid && datesForm.get('checkInDate')?.touched">Check‑in date is required and must be today or later.</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Check‑out Date</mat-label>
          <input matInput [matDatepicker]="coutPicker" formControlName="checkOutDate" />
          <mat-datepicker-toggle matSuffix [for]="coutPicker"></mat-datepicker-toggle>
          <mat-datepicker #coutPicker></mat-datepicker>
          <mat-error *ngIf="datesForm.get('checkOutDate')?.invalid && datesForm.get('checkOutDate')?.touched">Check‑out date is required and must be after check‑in.</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Number of Guests</mat-label>
          <input matInput type="number" formControlName="guestCount" min="1" max="20" />
          <mat-error *ngIf="datesForm.get('guestCount')?.invalid && datesForm.get('guestCount')?.touched">Guests must be between 1 and 20.</mat-error>
        </mat-form-field>
        <div class="step-actions">
          <button mat-button matStepperPrevious>Back</button>
          <button mat-button matStepperNext>Next</button>
        </div>
      </form>
    </mat-step>

    <!-- Step 3: Room Selection -->
    <mat-step [stepControl]="roomsForm" label="Select Rooms">
      <form [formGroup]="roomsForm">
        @if (roomsLoading()) {
          <mat-spinner diameter="30"></mat-spinner>
        } @else if (roomsError()) {
          <app-alert type="error" [message]="roomsError()!" (closed)="roomsError.set(null)"></app-alert>
        } @else {
          <div class="room-list">
            @for (room of availableRooms(); track room.roomTypeId) {
              <div class="room-item">
                <p>{{ room.name }} – {{ room.basePrice | currency }}/night – Max occupancy: {{ room.maxOccupancy }} – Available: {{ room.availableCount }}</p>
                <div class="qty-controls">
                  <button type="button" mat-icon-button (click)="decrementRoom(room.roomTypeId)" [disabled]="getRoomQuantity(room.roomTypeId) <= 0">
                    <mat-icon>remove</mat-icon>
                  </button>
                  <span>{{ getRoomQuantity(room.roomTypeId) }}</span>
                  <button type="button" mat-icon-button (click)="incrementRoom(room.roomTypeId)" [disabled]="getRoomQuantity(room.roomTypeId) >= room.availableCount">
                    <mat-icon>add</mat-icon>
                  </button>
                </div>
              </div>
            }
          </div>
          @if (capacityWarning()) {
            <p class="warning">{{ capacityWarning() }}</p>
          }
          <button mat-button matStepperNext [disabled]="totalSelectedQuantity() === 0 || capacityWarning()">Next</button>
        }
      </form>
    </mat-step>

    <!-- Step 4: Amenities -->
    <mat-step [stepControl]="amenitiesForm" label="Add Amenities">
      <form [formGroup]="amenitiesForm">
        @if (amenitiesLoading()) {
          <mat-spinner diameter="30"></mat-spinner>
        } @else {
          <div class="amenity-list">
            @for (amenity of availableAmenities(); track amenity.id; let i = $index) {
              <mat-checkbox [formControl]="getAmenityControl(i)">{{ amenity.name }} – {{ amenity.price | currency }}</mat-checkbox>
            }
          </div>
          <button mat-button matStepperNext>Next</button>
        }
      </form>
    </mat-step>

    <!-- Step 5: Review & Confirm -->
    <mat-step label="Review & Confirm">
      <div class="summary">
        <h3>Guest: {{ guestForm.value.firstName }} {{ guestForm.value.lastName }} ({{ guestForm.value.email }})</h3>
        <p>Check‑in: {{ datesForm.value.checkInDate | date }}</p>
        <p>Check‑out: {{ datesForm.value.checkOutDate | date }}</p>
        <p>Nights: {{ nights() }}</p>
        <p>Guests: {{ datesForm.value.guestCount }}</p>
        <h4>Rooms:</h4>
        <ul>
          @for (item of selectedRoomEntries(); track item.roomTypeId) {
            <li>{{ item.name }} x{{ item.quantity }} – {{ item.basePrice | currency }}/night – Subtotal: {{ item.quantity * item.basePrice * nights() | currency }}</li>
          }
        </ul>
        <h4>Amenities:</h4>
        <ul>
          @for (item of selectedAmenityEntries(); track item.id) {
            <li>{{ item.name }} – {{ item.price | currency }}</li>
          }
        </ul>
        <p><strong>Total Estimated: {{ estimatedTotal() | currency }}</strong></p>
      </div>
      <button mat-raised-button color="primary" (click)="confirmBooking()">Confirm Booking</button>
    </mat-step>
  </mat-stepper>
</div>
```

## 6. State Management (All Signals & Forms)

```typescript
// Step 1: Guest
guestForm = new FormGroup({
  firstName: new FormControl('', [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-ZÀ-ž\s\-']+$/)]),
  lastName: new FormControl('', [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-ZÀ-ž\s\-']+$/)]),
  email: new FormControl('', [Validators.required, Validators.email]),
});

// Step 2: Dates & Guests
datesForm = new FormGroup({
  checkInDate: new FormControl<Date | null>(null, [Validators.required, this.futureDateValidator]),
  checkOutDate: new FormControl<Date | null>(null, [Validators.required]),
  guestCount: new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]),
}, { validators: this.checkOutAfterCheckIn });

// Step 3: Rooms
availableRooms = signal<AvailableRoomType[]>([]);
selectedRoomQuantities = signal<Record<number, number>>({});
roomsLoading = signal(false);
roomsError = signal<string | null>(null);

// Step 4: Amenities
availableAmenities = signal<Amenity[]>([]);
selectedAmenities = new FormArray<FormControl<boolean>>([]);
amenitiesForm = new FormGroup({ amenities: this.selectedAmenities });
amenitiesLoading = signal(false);

// Computed
totalSelectedQuantity = computed(() => Object.values(this.selectedRoomQuantities()).reduce((a,b)=>a+b,0));
capacityWarning = computed(() => { ... }); // same as customer wizard
nights = computed(() => { ... }); // from dates
estimatedTotal = computed(() => { ... }); // from rooms, amenities, nights
selectedRoomEntries = computed(() => { ... });
selectedAmenityEntries = computed(() => { ... });

// Submission
submitting = signal(false);
```

## 7. Data Flow & API Calls

### 7.1 Services
- `BookingApiService` – `create(dto: CreateBookingRequestDTO): Observable<Booking>`, `checkIn(id: number): Observable<Booking>`
- `RoomTypeApiService` – `getAvailability(params): Observable<{ data: AvailableRoomType[] }>`
- `AmenityApiService` – `getAll(params): Observable<{ data: Amenity[] }>`

### 7.2 Step 2 → Fetch Rooms
When entering step 2, after the dates are filled and valid, fetch available rooms:
```typescript
onStepChange(event: StepperSelectionEvent): void {
  if (event.selectedIndex === 2) { // step 3 (0‑based)
    this.fetchAvailableRooms();
  }
  if (event.selectedIndex === 3) {
    this.fetchAmenities();
  }
}
```

`fetchAvailableRooms()`: use `datesForm.value.checkInDate` and `checkOutDate` formatted as ISO strings, call `roomTypeApi.getAvailability`.

`fetchAmenities()`: call `amenityApi.getAll({ isAvailable: true, pageSize: 100 })` and populate form array.

### 7.3 Confirm & Create Booking
`confirmBooking()` opens a confirmation dialog, then on confirm calls `POST /api/v1/bookings` with the built DTO:
```typescript
const dto: CreateBookingRequestDTO = {
  guestName: `${guestForm.value.firstName} ${guestForm.value.lastName}`,
  guestEmail: guestForm.value.email,
  guestCount: datesForm.value.guestCount,
  checkInDate: datesForm.value.checkInDate.toISOString(),
  checkOutDate: datesForm.value.checkOutDate.toISOString(),
  roomTypeIds: flattenRoomIds(), // from selectedRoomQuantities
  amenityIds: getSelectedAmenityIds(),
};
```
On success, store the returned `Booking` (which contains `id`), reset the wizard, and open a success dialog.

## 8. Success Dialog & Check‑In Flow

### 8.1 SuccessDialogComponent
A simple standalone dialog that receives `{ bookingId: number, guestName: string }`. It displays “Booking #id created successfully” and offers two buttons: **“Check‑In Now”** and **“Close”**.

- **Check‑In Now** calls `bookingApi.checkIn(bookingId)`, then shows a snackbar with the assigned room number, and closes the dialog with `true`.
- **Close** simply closes the dialog.

### 8.2 Implementation
Create `src/app/features/front-desk/components/success-dialog/success-dialog.component.ts`:

```typescript
@Component({
  selector: 'app-success-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, CommonModule, MatProgressSpinnerModule],
  template: `
    <h2 mat-dialog-title>Booking Created</h2>
    <mat-dialog-content>
      <p>Booking #{{ data.bookingId }} for {{ data.guestName }} has been created.</p>
      <p>Would you like to check in now?</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="close()">Close</button>
      <button mat-raised-button color="primary" (click)="checkInNow()" [disabled]="checkingIn()">
        @if (checkingIn()) { <mat-spinner diameter="20"></mat-spinner> }
        Check‑In Now
      </button>
    </mat-dialog-actions>
  `
})
export class SuccessDialogComponent {
  data: { bookingId: number; guestName: string } = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<SuccessDialogComponent>);
  private bookingApi = inject(BookingApiService);
  private snackBar = inject(MatSnackBar);
  checkingIn = signal(false);

  checkInNow(): void {
    this.checkingIn.set(true);
    this.bookingApi.checkIn(this.data.bookingId).pipe(
      finalize(() => this.checkingIn.set(false))
    ).subscribe({
      next: (updated) => {
        this.snackBar.open(`Checked in. Room: ${updated.rooms?.[0]?.roomNumber || 'assigned'}`, 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (err) => this.snackBar.open('Check‑in failed: ' + err.message, 'Close', { duration: 5000 })
    });
  }

  close(): void {
    this.dialogRef.close(false);
  }
}
```

### 8.3 Open Dialog After Creation
In `confirmBooking()`:
```typescript
this.bookingApi.create(dto).subscribe({
  next: (booking) => {
    const dialogRef = this.dialog.open(SuccessDialogComponent, {
      data: { bookingId: booking.id, guestName: booking.guestName },
      width: '400px',
    });
    dialogRef.afterClosed().subscribe(() => {
      this.resetWizard(); // reset all forms and signals
    });
  },
  error: (err) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
});
```

## 9. Responsive Behaviour
- Stepper orientation: use `isMobile` signal (≤599px) → vertical, else horizontal.
- Room list stacks vertically on mobile.
- Forms full width.

## 10. Self‑Review Checklist
- [ ] Wizard steps: Guest → Dates → Rooms → Amenities → Review → Confirm.
- [ ] Guest details step enforces name and email validation.
- [ ] Dates step prevents past check‑in, ensures check‑out > check‑in.
- [ ] Room selection shows available counts, capacity warning prevents proceeding with insufficient space.
- [ ] Amenities are optional.
- [ ] Review shows complete breakdown with total.
- [ ] On confirm, booking is created, success dialog appears with “Check‑In Now” option.
- [ ] Check‑In Now calls API and displays room number.
- [ ] After dialog closes, the wizard resets to initial state.
- [ ] Responsive layout works on mobile.

## 11. Integration Notes
- The `RoomTypeApiService` must support the availability endpoint as used in the customer module.
- The `BookingApiService` must have `create` and `checkIn` methods.
- The `SuccessDialogComponent` is new and must be created in the front desk feature.
- No modifications to the dashboard or other pages required.

