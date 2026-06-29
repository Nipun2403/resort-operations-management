# Specsheet: Public Home Page

## 1. Purpose
- Replace the `PlaceholderHomeComponent` with the real Home page.
- Showcases the hotel with a hero banner, featured room types carousel, quick links to Menu and Amenities, and an availability search bar.
- The availability search bar stores dates and guests in `sessionStorage` and navigates to the availability page.
- All data is fetched from the backend; room type images are real URLs from the API.

## 2. Route & Navigation
- Path: `/home` (lazy‑loaded under Public Shell).
- **Overwrite** the placeholder file: `src/app/features/public/pages/home.component.ts`.

## 3. Authorization
- None – this page is public.

## 4. Component API (HomeComponent)
- **Selector**: `app-home`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `RouterModule`, `MatButtonModule`, `MatCardModule`, `MatIconModule`, `MatDatepickerModule`, `MatNativeDateModule`, `MatFormFieldModule`, `MatInputModule`, `MatProgressSpinnerModule`, `RoomTypeApiService`.
- **Exact import paths** (abbreviated; agent must include correct relative paths).

## 5. State Management (All Signals)
```typescript
// Featured rooms
featuredRooms = signal<RoomType[]>([]);
roomsLoading = signal(false);
roomsError = signal<string | null>(null);

// Availability form
checkIn = new FormControl<Date | null>(null, Validators.required);
checkOut = new FormControl<Date | null>(null, Validators.required);
guests = new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]);
```

## 6. Template (exact – Angular 18 control flow)
```html
<div class="home-page">
  <!-- Hero Banner -->
  <section class="hero">
    <div class="hero-content">
      <h1>Experience Luxury Like Never Before</h1>
      <p>Discover our elegant rooms, world‑class dining, and impeccable service.</p>
      <button mat-raised-button color="primary" routerLink="/rooms">View Rooms</button>
    </div>
  </section>

  <!-- Availability Search Bar -->
  <section class="availability-bar">
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
          <button mat-raised-button color="accent" (click)="searchAvailability()" [disabled]="checkIn.invalid || checkOut.invalid || guests.invalid">
            Check Availability
          </button>
        </div>
      </mat-card-content>
    </mat-card>
  </section>

  <!-- Featured Rooms -->
  <section class="featured-rooms">
    <h2>Our Featured Rooms</h2>
    @if (roomsLoading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (roomsError()) {
      <p class="error">{{ roomsError() }}</p>
    } @else {
      <div class="carousel">
        @for (room of featuredRooms(); track room.id) {
          <mat-card class="room-card" (click)="viewRoom(room.id)">
            <img [src]="getFirstImage(room)" alt="{{ room.name }}" class="room-image" />
            <mat-card-header>
              <mat-card-title>{{ room.name }}</mat-card-title>
              <mat-card-subtitle>{{ room.basePrice | currency }}/night – Up to {{ room.maxOccupancy }} guests</mat-card-subtitle>
            </mat-card-header>
          </mat-card>
        }
      </div>
    }
  </section>

  <!-- Quick Links -->
  <section class="quick-links">
    <h2>Discover More</h2>
    <div class="links-grid">
      <mat-card class="link-card" routerLink="/menu">
        <mat-icon>restaurant</mat-icon>
        <h3>Our Restaurant</h3>
        <p>Explore our award‑winning menu.</p>
      </mat-card>
      <mat-card class="link-card" routerLink="/amenities">
        <mat-icon>spa</mat-icon>
        <h3>Amenities</h3>
        <p>Pools, gyms, spa – see what we offer.</p>
      </mat-card>
    </div>
  </section>
</div>
```

## 7. Logic (exact TypeScript)
```typescript
import { Component, inject, signal } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../../../services/room-type-api.service'; // adjust path
import { RoomType } from '../../../../models/room-type.model'; // adjust path

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ /* as listed in Section 4 */ ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  featuredRooms = signal<RoomType[]>([]);
  roomsLoading = signal(false);
  roomsError = signal<string | null>(null);

  checkIn = new FormControl<Date | null>(null, Validators.required);
  checkOut = new FormControl<Date | null>(null, Validators.required);
  guests = new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]);

  ngOnInit(): void {
    this.fetchFeaturedRooms();
  }

  private fetchFeaturedRooms(): void {
    this.roomsLoading.set(true);
    this.roomTypeApi.getAll({ includeRetired: false, pageSize: 6, sortBy: 'basePrice', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.roomsLoading.set(false))
    ).subscribe({
      next: res => this.featuredRooms.set(res.data),
      error: (err: any) => this.roomsError.set(this.extractErrorMessage(err))
    });
  }

  getFirstImage(room: RoomType): string {
    return room.imageUrls && room.imageUrls.length > 0 ? room.imageUrls[0] : 'assets/placeholder-room.jpg';
  }

  viewRoom(roomId: number): void {
    this.router.navigate(['/rooms', roomId]);
  }

  searchAvailability(): void {
    if (this.checkIn.invalid || this.checkOut.invalid || this.guests.invalid) return;
    const checkIn = this.checkIn.value!.toISOString();
    const checkOut = this.checkOut.value!.toISOString();
    const guestCount = this.guests.value!;
    // Store for later booking flow
    sessionStorage.setItem('availabilitySearch', JSON.stringify({ checkIn, checkOut, guests: guestCount }));
    this.router.navigate(['/availability'], { queryParams: { checkIn, checkOut, guests: guestCount } });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
```

## 8. Styling (home.component.scss)
```scss
.hero {
  background: linear-gradient(rgba(0,0,0,0.4), rgba(0,0,0,0.4)), url('/assets/hero.jpg') center/cover no-repeat;
  height: 70vh;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  text-align: center;
  .hero-content {
    max-width: 600px;
    h1 { font-size: 2.5rem; margin-bottom: 16px; }
    p { font-size: 1.2rem; margin-bottom: 24px; }
  }
}

.availability-bar {
  position: relative;
  margin-top: -40px; // overlap hero slightly
  z-index: 5;
  .search-form {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
    align-items: center;
    mat-form-field { flex: 1 1 200px; }
  }
}

.featured-rooms {
  padding: 48px 16px;
  h2 { text-align: center; margin-bottom: 24px; }
  .carousel {
    display: flex;
    gap: 16px;
    overflow-x: auto;
    scroll-snap-type: x mandatory;
    padding-bottom: 16px;
    .room-card {
      flex: 0 0 300px;
      scroll-snap-align: start;
      cursor: pointer;
      transition: transform 0.2s;
      &:hover { transform: translateY(-4px); }
      img.room-image {
        width: 100%;
        height: 200px;
        object-fit: cover;
      }
    }
  }
}

.quick-links {
  padding: 48px 16px;
  background: #f9f9f9;
  h2 { text-align: center; margin-bottom: 24px; }
  .links-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 16px;
    .link-card {
      text-align: center;
      cursor: pointer;
      transition: box-shadow 0.2s;
      &:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.15); }
      mat-icon { font-size: 48px; width: 48px; height: 48px; margin-top: 16px; color: #1976d2; }
      h3 { margin: 12px 0 8px; }
    }
  }
}
```

**Responsive adjustments:**
- Hero height: 50vh on tablets, 40vh on phones.
- Search bar margin‑top: 0 on mobile (no overlap).
- Carousel card width: 280px on small screens.

Add media queries as needed.

## 9. Self‑Review Checklist
- [ ] Hero banner loads with hotel image and text; CTA navigates to `/rooms`.
- [ ] Availability form fields are required; submit navigates to `/availability` with query params and stores data in session storage.
- [ ] Featured rooms carousel fetches 6 active room types, displays image, name, price, and occupancy.
- [ ] Clicking a room card navigates to `/rooms/:id`.
- [ ] Quick links navigate to `/menu` and `/amenities`.
- [ ] Responsive layout works on mobile and tablet.
- [ ] No console errors.

## 10. Integration Notes
- The hero image (`/assets/hero.jpg`) must be present in the assets folder; if not, use a placeholder color.
- `RoomTypeApiService` is already available from admin; reuse it.
- The availability page will later read the query params and session storage.

