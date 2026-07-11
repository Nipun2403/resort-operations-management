# Specsheet: Public Availability Page & Booking Redirect

## 1. Purpose
- Replace the `PlaceholderAvailabilityComponent` with the Availability search and results page.
- Allows guests to search for available room types by check‑in date, check‑out date, and number of guests.
- Pre‑fills the form from query parameters (`checkIn`, `checkOut`, `guests`, `roomTypeId`) or from `sessionStorage` (keys: `availabilitySearch`, `selectedRoomTypeId`).
- Fetches availability from `GET /room-types/availability` and displays results in a grid of cards showing room image, name, price, available count, and max occupancy.
- Each card has a “Book Now” button.
  - **If the user is logged in**: navigates directly to the customer booking wizard with pre‑filled parameters via query params.
  - **If the user is not logged in**: stores the intended booking details in `sessionStorage` under the key `pendingBooking` and navigates to `/auth?returnUrl=/user/dashboard`. After login, the user portal dashboard will detect the pending booking and open the booking wizard (this detection will be implemented in a subsequent patch to the customer dashboard).
- The page is fully public.

## 2. Route & Navigation
- Path: `/availability` (lazy‑loaded under Public Shell).
- **Overwrite** the placeholder file: `src/app/features/public/pages/availability.component.ts`.

## 3. Authorization
- None – this page is public.

## 4. Component API (AvailabilityComponent)
- **Selector**: `app-availability`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `RouterModule`, `ActivatedRoute`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatDatepickerModule`, `MatNativeDateModule`, `MatFormFieldModule`, `MatInputModule`, `MatProgressSpinnerModule`, `MatSnackBarModule`, `RoomTypeApiService`, `AuthService`, `DestroyRef`.
- **Exact import paths** (abbreviated; agent must use correct paths).

## 5. State Management (All Signals)
```typescript
// Search form
checkIn = new FormControl<Date | null>(null, Validators.required);
checkOut = new FormControl<Date | null>(null, Validators.required);
guests = new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]);

// Results
availableRooms = signal<AvailableRoomType[]>([]);
searchLoading = signal(false);
searchError = signal<string | null>(null);
hasSearched = signal(false);

// Pre‑filled room type ID from session storage / query params
preSelectedRoomTypeId = signal<number | null>(null);
```

## 6. Template (exact – Angular 18 control flow)
```html
<div class="availability-page">
  <div class="hero-small">
    <h1>Check Availability</h1>
    <p>Find the perfect room for your stay.</p>
  </div>

  <!-- Search Form -->
  <section class="search-section">
    <mat-card>
      <mat-card-content>
        <div class="search-form">
          <mat-form-field appearance="outline">
            <mat-label>Check‑in</mat-label>
            <input matInput [matDatepicker]="cinPicker" [formControl]="checkIn" />
            <mat-datepicker-toggle matSuffix [for]="cinPicker"></mat-datepicker-toggle>
            <mat-datepicker #cinPicker></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Check‑out</mat-label>
            <input matInput [matDatepicker]="coutPicker" [formControl]="checkOut" />
            <mat-datepicker-toggle matSuffix [for]="coutPicker"></mat-datepicker-toggle>
            <mat-datepicker #coutPicker></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Guests</mat-label>
            <input matInput type="number" [formControl]="guests" min="1" max="20" />
          </mat-form-field>
          <button mat-raised-button color="primary" (click)="searchAvailability()" [disabled]="checkIn.invalid || checkOut.invalid || guests.invalid || searchLoading()">
            Search
          </button>
        </div>
      </mat-card-content>
    </mat-card>
  </section>

  <!-- Results -->
  <section class="results-section">
    @if (searchLoading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (searchError()) {
      <p class="error">{{ searchError() }}</p>
    } @else if (hasSearched() && availableRooms().length === 0) {
      <p class="empty">No rooms available for the selected dates and guests.</p>
    } @else if (availableRooms().length > 0) {
      <h2>Available Rooms</h2>
      <div class="results-grid">
        @for (room of availableRooms(); track room.roomTypeId) {
          <mat-card class="room-card">
            <img [src]="getFirstImage(room)" alt="{{ room.name }}" class="room-image" />
            <mat-card-header>
              <mat-card-title>{{ room.name }}</mat-card-title>
              <mat-card-subtitle>
                {{ room.basePrice | currency }}/night – Up to {{ room.maxOccupancy }} guests
                <br />Available: <strong>{{ room.availableCount }}</strong> room(s)
              </mat-card-subtitle>
            </mat-card-header>
            <mat-card-actions>
              <button mat-raised-button color="accent" (click)="bookNow(room)">Book Now</button>
            </mat-card-actions>
          </mat-card>
        }
      </div>
    }
  </section>
</div>
```

## 7. Logic (exact TypeScript)
```typescript
export class AvailabilityComponent {
  private roomTypeApi = inject(RoomTypeApiService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  private snackBar = inject(MatSnackBar);

  checkIn = new FormControl<Date | null>(null, Validators.required);
  checkOut = new FormControl<Date | null>(null, Validators.required);
  guests = new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]);

  availableRooms = signal<AvailableRoomType[]>([]);
  searchLoading = signal(false);
  searchError = signal<string | null>(null);
  hasSearched = signal(false);
  preSelectedRoomTypeId = signal<number | null>(null);

  ngOnInit(): void {
    // Pre‑fill form from query params
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['checkIn']) this.checkIn.setValue(new Date(params['checkIn']), { emitEvent: false });
      if (params['checkOut']) this.checkOut.setValue(new Date(params['checkOut']), { emitEvent: false });
      if (params['guests']) this.guests.setValue(+params['guests'], { emitEvent: false });
      if (params['roomTypeId']) {
        this.preSelectedRoomTypeId.set(+params['roomTypeId']);
      }
    });
    // Also check session storage for pre‑selected room type ID (from detail page)
    const storedRoomId = sessionStorage.getItem('selectedRoomTypeId');
    if (storedRoomId && !this.preSelectedRoomTypeId()) {
      this.preSelectedRoomTypeId.set(Number(storedRoomId));
    }
    // Pre‑fill from availability search session storage (from home page)
    const storedSearch = sessionStorage.getItem('availabilitySearch');
    if (storedSearch) {
      try {
        const data = JSON.parse(storedSearch);
        if (data.checkIn && !this.checkIn.value) this.checkIn.setValue(new Date(data.checkIn));
        if (data.checkOut && !this.checkOut.value) this.checkOut.setValue(new Date(data.checkOut));
        if (data.guests && this.guests.value === 1) this.guests.setValue(data.guests);
      } catch { /* ignore */ }
    }
    // If dates are pre‑filled, optionally auto‑search? We'll let the user press Search.
  }

  searchAvailability(): void {
    if (this.checkIn.invalid || this.checkOut.invalid || this.guests.invalid) return;
    this.searchLoading.set(true);
    this.searchError.set(null);
    const params = {
      checkIn: this.checkIn.value!.toISOString(),
      checkOut: this.checkOut.value!.toISOString(),
      pageSize: 50,
    };
    this.roomTypeApi.getAvailability(params).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.searchLoading.set(false))
    ).subscribe({
      next: res => {
        this.availableRooms.set(res.data);
        this.hasSearched.set(true);
      },
      error: (err: any) => this.searchError.set(this.extractErrorMessage(err))
    });
  }

  getFirstImage(room: AvailableRoomType): string {
    return room.imageUrls && room.imageUrls.length > 0 ? room.imageUrls[0] : 'assets/placeholder-room.jpg';
  }

  bookNow(room: AvailableRoomType): void {
    const checkIn = this.checkIn.value!.toISOString();
    const checkOut = this.checkOut.value!.toISOString();
    const guestCount = this.guests.value!;
    const roomTypeId = room.roomTypeId;

    if (this.authService.isAuthenticated()) {
      // Navigate directly to user booking wizard with pre‑filled params
      this.router.navigate(['/user/bookings'], {
        queryParams: {
          new: true,
          roomTypeId,
          checkIn,
          checkOut,
          guests: guestCount
        }
      });
    } else {
      // Store pending booking and redirect to login
      sessionStorage.setItem('pendingBooking', JSON.stringify({
        roomTypeId,
        checkIn,
        checkOut,
        guests: guestCount
      }));
      this.router.navigate(['/auth'], { queryParams: { returnUrl: '/user/dashboard' } });
    }
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
```

## 8. Styling (`availability.component.scss`)
```scss
.availability-page {
  .hero-small {
    background: linear-gradient(rgba(0,0,0,0.5), rgba(0,0,0,0.5)), url('/assets/availability-hero.jpg') center/cover no-repeat;
    height: 30vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: white;
    text-align: center;
    h1 { font-size: 2.5rem; margin-bottom: 8px; }
    p { font-size: 1.2rem; }
  }
  .search-section {
    position: relative;
    margin-top: -30px;
    z-index: 5;
    .search-form {
      display: flex;
      flex-wrap: wrap;
      gap: 16px;
      align-items: center;
      mat-form-field { flex: 1 1 200px; }
    }
  }
  .results-section {
    padding: 32px 16px;
    h2 { margin-bottom: 16px; }
    .results-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 24px;
    }
    .room-card {
      .room-image {
        width: 100%;
        height: 200px;
        object-fit: cover;
      }
      mat-card-actions { justify-content: center; }
    }
  }
}
```

## 9. Subsequent Patch Specifications (not included in this spec, but required)
- **Auth Page Patch:** handle `returnUrl` query param; on successful login, navigate to `returnUrl` if present, else to role dashboard.
- **Customer Dashboard Patch:** on load, check for `pendingBooking` in session storage; if present, open the booking wizard with pre‑filled data and clear the storage.
- **Customer Booking Wizard Patch:** accept query params `new`, `roomTypeId`, `checkIn`, `checkOut`, `guests`; automatically switch to new booking mode and pre‑fill steps 1 and 2.

These will be delivered as separate small specsheets after the public site is complete.

## 10. Self‑Review Checklist
- [ ] Availability page pre‑fills dates and guest count from query params and session storage.
- [ ] Search calls `/room-types/availability` and displays results with image, name, price, available count, and max occupancy.
- [ ] “Book Now” for authenticated user navigates to `/user/bookings` with correct query params.
- [ ] “Book Now” for unauthenticated user stores `pendingBooking` in session storage and navigates to `/auth?returnUrl=/user/dashboard`.
- [ ] Empty state message when no rooms available.
- [ ] Responsive layout works on mobile.
- [ ] No console errors; subscriptions cleaned.

## 11. Integration Notes
- `RoomTypeApiService.getAvailability` must accept the same parameters as defined in the customer booking wizard (checkIn, checkOut, pageSize, etc.).
- `AuthService.isAuthenticated()` returns a boolean based on the JWT token presence and expiry.
- The `AvailableRoomType` interface is already defined in the customer booking wizard spec; reuse it.
- The hero image `/assets/availability-hero.jpg` should be present; if not, use a gradient.

