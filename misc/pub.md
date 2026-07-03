# amenities.component.html

<div class="amenities-page">
  <div class="hero-small">
    <h1>Amenities</h1>
    <p>Relax and enjoy our premium facilities designed for your comfort.</p>
  </div>

  @if (loading()) {
    <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
    <p class="error">{{ error() }}</p>
    <button mat-button (click)="fetchAmenities()">Retry</button>
  } @else {
    <div class="amenities-grid">
      @for (amenity of amenities(); track amenity.id) {
        <mat-card class="amenity-card">
          <div class="card-icon">
            <mat-icon>spa</mat-icon>
          </div>
          <mat-card-header>
            <mat-card-title>{{ amenity.name }}</mat-card-title>
            <mat-card-subtitle>{{ amenity.price ? (amenity.price | currency) : 'Complimentary' }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>{{ amenity.description || 'No description available.' }}</p>
          </mat-card-content>
        </mat-card>
      }
    </div>
  }
</div>


# amenities.component.scss

.amenities-page {
  .hero-small {
    background: linear-gradient(rgba(0,0,0,0.5), rgba(0,0,0,0.5)), url('/assets/amenities-hero.jpg') center/cover no-repeat;
    height: 35vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: white;
    text-align: center;
    h1 { font-size: 2.5rem; margin-bottom: 8px; }
    p { font-size: 1.2rem; max-width: 500px; }
  }
  .amenities-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 24px;
    padding: 32px 16px;
  }
  .amenity-card {
    text-align: center;
    padding: 24px;
    .card-icon {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      background: #e3f2fd;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 16px;
      mat-icon { font-size: 40px; width: 40px; height: 40px; color: #1976d2; }
    }
  }
}

// Responsive adjustments
@media (max-width: 768px) {
  .amenities-page {
    .hero-small {
      height: 25vh;
    }
  }
}


# amenities.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { AmenityApiService } from '../../admin/services/amenity-api.service';
import { Amenity } from '../../admin/models/amenity.model';

@Component({
  selector: 'app-public-amenities',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './amenities.component.html',
  styleUrls: ['./amenities.component.scss']
})
export class AmenitiesComponent implements OnInit {
  private amenityApi = inject(AmenityApiService);
  private destroyRef = inject(DestroyRef);

  amenities = signal<Amenity[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchAmenities();
  }

  fetchAmenities(): void {
    this.loading.set(true);
    this.amenityApi.getAll({ isAvailable: true, pageNumber: 1, pageSize: 200, sortBy: 'name', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => this.amenities.set(res.data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# availability.component.html

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


# availability.component.scss

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

// Responsive adjustments
@media (max-width: 768px) {
  .availability-page {
    .search-section {
      margin-top: 0;
    }
  }
}


# availability.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { AuthService } from '../../../core/services/auth.service';
import { AvailableRoomType } from '../../user/models/available-room-type.model';

@Component({
  selector: 'app-availability',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule, MatDatepickerModule,
    MatNativeDateModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './availability.component.html',
  styleUrls: ['./availability.component.scss']
})
export class AvailabilityComponent implements OnInit {
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


# home.component.html

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


# home.component.scss

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

// Responsive adjustments
@media (max-width: 768px) {
  .hero {
    height: 50vh;
  }
  .availability-bar {
    margin-top: 0;
  }
  .featured-rooms .carousel .room-card {
    flex: 0 0 280px;
  }
}

@media (max-width: 480px) {
  .hero {
    height: 40vh;
    .hero-content {
      h1 { font-size: 1.8rem; }
      p { font-size: 1rem; }
    }
  }
}


# home.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatButtonModule, MatCardModule, MatIconModule, MatDatepickerModule,
    MatNativeDateModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
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
    this.roomTypeApi.getAll({ includeRetired: false, pageNumber: 1, pageSize: 6, sortBy: 'basePrice', sortDescending: false }).pipe(
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


# menu.component.html

<div class="menu-page">
  <div class="hero-small">
    <h1>Our Restaurant</h1>
    <p>Indulge in culinary excellence crafted by our world‑class chefs.</p>
  </div>

  @if (loading()) {
    <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
    <p class="error">{{ error() }}</p>
    <button mat-button (click)="fetchMenu()">Retry</button>
  } @else {
    @for (group of groupedMenu(); track group.category) {
      <section class="category-section">
        <h2>{{ group.category }}</h2>
        <div class="menu-grid">
          @for (item of group.items; track item.id) {
            <mat-card class="menu-card">
              <div class="card-image">
                <mat-icon class="food-icon">restaurant</mat-icon>
              </div>
              <mat-card-header>
                <mat-card-title>{{ item.name }}</mat-card-title>
                <mat-card-subtitle>{{ item.price | currency }}</mat-card-subtitle>
              </mat-card-header>
            </mat-card>
          }
        </div>
      </section>
    }
  }
</div>


# menu.component.scss

.menu-page {
  .hero-small {
    background: linear-gradient(rgba(0,0,0,0.5), rgba(0,0,0,0.5)), url('/assets/restaurant-hero.jpg') center/cover no-repeat;
    height: 35vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: white;
    text-align: center;
    h1 { font-size: 2.5rem; margin-bottom: 8px; }
    p { font-size: 1.2rem; max-width: 500px; }
  }
  .category-section {
    padding: 32px 16px;
    h2 {
      font-size: 1.8rem;
      margin-bottom: 16px;
      padding-bottom: 8px;
      border-bottom: 2px solid #1976d2;
      display: inline-block;
    }
    .menu-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 16px;
    }
  }
  .menu-card {
    display: flex;
    align-items: center;
    padding: 12px;
    .card-image {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      background: #f5f5f5;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 16px;
      .food-icon { font-size: 32px; width: 32px; height: 32px; color: #1976d2; }
    }
    mat-card-header { flex: 1; }
  }
}

// Responsive adjustments
@media (max-width: 768px) {
  .menu-page {
    .hero-small {
      height: 25vh;
    }
  }
}


# menu.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { MenuItemApiService } from '../../admin/services/menu-item-api.service';
import { MenuItem } from '../../admin/models/menu-item.model';

@Component({
  selector: 'app-public-menu',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent implements OnInit {
  private menuItemApi = inject(MenuItemApiService);
  private destroyRef = inject(DestroyRef);

  groupedMenu = signal<{ category: string; items: MenuItem[] }[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchMenu();
  }

  fetchMenu(): void {
    this.loading.set(true);
    this.menuItemApi.getAll({ isAvailable: true, pageNumber: 1, pageSize: 200, sortBy: 'name', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => {
        const groups: Record<string, MenuItem[]> = {};
        for (const item of res.data) {
          const cat = item.category || 'Other';
          if (!groups[cat]) groups[cat] = [];
          groups[cat].push(item);
        }
        this.groupedMenu.set(
          Object.entries(groups).map(([category, items]) => ({ category, items }))
        );
      },
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# room-catalogue.component.html

<div class="room-catalogue">
  <h1>Our Rooms</h1>

  @if (loading()) {
    <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
    <p class="error">{{ error() }}</p>
    <button mat-button (click)="fetchRooms()">Retry</button>
  } @else {
    <div class="rooms-grid">
      @for (room of rooms(); track room.id) {
        <mat-card class="room-card" (click)="viewRoom(room.id)">
          <img [src]="getFirstImage(room)" alt="{{ room.name }}" class="room-image" />
          <mat-card-header>
            <mat-card-title>{{ room.name }}</mat-card-title>
            <mat-card-subtitle>{{ room.basePrice | currency }}/night – Up to {{ room.maxOccupancy }} guests</mat-card-subtitle>
          </mat-card-header>
          <mat-card-actions>
            <button mat-raised-button color="primary" (click)="viewRoom(room.id); $event.stopPropagation()">View Details</button>
          </mat-card-actions>
        </mat-card>
      }
    </div>
  }
</div>


# room-catalogue.component.scss

.room-catalogue {
  padding: 32px 16px;
  h1 { text-align: center; margin-bottom: 24px; }
  .rooms-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 24px;
  }
  .room-card {
    cursor: pointer;
    transition: transform 0.2s;
    &:hover { transform: translateY(-4px); }
    .room-image {
      width: 100%;
      height: 200px;
      object-fit: cover;
    }
  }
}


# room-catalogue.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-room-catalogue',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './room-catalogue.component.html',
  styleUrls: ['./room-catalogue.component.scss']
})
export class RoomCatalogueComponent implements OnInit {
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  rooms = signal<RoomType[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchRooms();
  }

  fetchRooms(): void {
    this.loading.set(true);
    this.roomTypeApi.getAll({ includeRetired: false, pageNumber: 1, pageSize: 100, sortBy: 'name', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => this.rooms.set(res.data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  getFirstImage(room: RoomType): string {
    return room.imageUrls && room.imageUrls.length > 0 ? room.imageUrls[0] : 'assets/placeholder-room.jpg';
  }

  viewRoom(roomId: number): void {
    this.router.navigate(['/rooms', roomId]);
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# room-detail.component.html

<div class="room-detail">
  @if (loading()) {
    <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
    <p class="error">{{ error() }}</p>
    <button mat-button routerLink="/rooms">Back to Rooms</button>
  } @else if (room()) {
    <!-- Image Gallery -->
    <div class="image-gallery">
      @for (imgUrl of room()!.imageUrls; track imgUrl) {
        <img [src]="imgUrl" alt="{{ room()!.name }}" class="gallery-image" />
      }
    </div>

    <!-- Room Info -->
    <div class="room-info">
      <h1>{{ room()!.name }}</h1>
      <p class="description">{{ room()!.description || 'No description available.' }}</p>
      <div class="details-grid">
        <div class="detail-item">
          <mat-icon>attach_money</mat-icon>
          <span><strong>Price:</strong> {{ room()!.basePrice | currency }}/night</span>
        </div>
        <div class="detail-item">
          <mat-icon>people</mat-icon>
          <span><strong>Max Occupancy:</strong> {{ room()!.maxOccupancy }} guests</span>
        </div>
        <div class="detail-item">
          <mat-icon>square_foot</mat-icon>
          <span><strong>Square Footage:</strong> {{ room()!.squareFootage || 'N/A' }} sq ft</span>
        </div>
      </div>

      <!-- Bed Configuration -->
      @if (room()!.bedConfiguration && getBedEntries().length > 0) {
        <div class="bed-config">
          <h3>Bed Configuration</h3>
          <ul>
            @for (entry of getBedEntries(); track entry[0]) {
              <li>{{ entry[0] }} x {{ entry[1] }}</li>
            }
          </ul>
        </div>
      }

      <!-- Check Availability CTA -->
      <button mat-raised-button color="accent" (click)="checkAvailability()">
        Check Availability
      </button>
    </div>
  }
</div>


# room-detail.component.scss

.room-detail {
  .image-gallery {
    display: flex;
    overflow-x: auto;
    scroll-snap-type: x mandatory;
    gap: 8px;
    padding: 16px 0;
    .gallery-image {
      flex: 0 0 80%;
      max-height: 400px;
      object-fit: cover;
      scroll-snap-align: start;
    }
  }
  .room-info {
    padding: 0 16px;
    h1 { font-size: 2rem; margin: 24px 0 8px; }
    .description { font-size: 1.1rem; color: #555; margin-bottom: 16px; }
    .details-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 12px;
      margin: 16px 0;
      .detail-item {
        display: flex;
        align-items: center;
        gap: 8px;
        mat-icon { color: #1976d2; }
      }
    }
    .bed-config {
      margin: 24px 0;
      ul { list-style: none; padding: 0; }
      li { padding: 4px 0; }
    }
    button { margin-top: 16px; }
  }
}


# room-detail.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-room-detail',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './room-detail.component.html',
  styleUrls: ['./room-detail.component.scss']
})
export class RoomDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  room = signal<RoomType | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.fetchRoom(id);
    } else {
      this.error.set('Room not found.');
    }
  }

  private fetchRoom(id: number): void {
    this.loading.set(true);
    this.roomTypeApi.getById(id).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (data: any) => this.room.set(data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  getBedEntries(): [string, number][] {
    const config = this.room()?.bedConfiguration;
    if (!config) return [];
    return Object.entries(config).filter(([, v]) => v > 0);
  }

  checkAvailability(): void {
    const roomId = this.room()?.id;
    if (roomId) {
      // Store room type ID for later booking flow
      sessionStorage.setItem('selectedRoomTypeId', String(roomId));
      this.router.navigate(['/availability'], { queryParams: { roomTypeId: roomId } });
    }
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# public-shell.component.html

<!-- Header -->
<mat-toolbar color="primary" class="public-header">
  <!-- Logo / Hotel Name -->
  <span class="logo" routerLink="/home">Hotel Name</span>
  <span class="spacer"></span>

  <!-- Desktop Navigation -->
  @if (!isMobile()) {
    <nav class="desktop-nav">
      <a mat-button routerLink="/home" routerLinkActive="active">Home</a>
      <a mat-button routerLink="/rooms" routerLinkActive="active">Rooms</a>
      <a mat-button routerLink="/menu" routerLinkActive="active">Menu</a>
      <a mat-button routerLink="/amenities" routerLinkActive="active">Amenities</a>
      <a mat-raised-button color="accent" routerLink="/availability">Check Availability</a>
      <a mat-stroked-button routerLink="/auth">Login</a>
    </nav>
  }

  <!-- Mobile Hamburger -->
  @if (isMobile()) {
    <button mat-icon-button [matMenuTriggerFor]="mobileMenu" aria-label="Menu">
      <mat-icon>menu</mat-icon>
    </button>
    <mat-menu #mobileMenu="matMenu">
      <a mat-menu-item routerLink="/home">Home</a>
      <a mat-menu-item routerLink="/rooms">Rooms</a>
      <a mat-menu-item routerLink="/menu">Menu</a>
      <a mat-menu-item routerLink="/amenities">Amenities</a>
      <a mat-menu-item routerLink="/availability">Check Availability</a>
      <a mat-menu-item routerLink="/auth">Login</a>
    </mat-menu>
  }
</mat-toolbar>

<!-- Main Content -->
<main>
  <router-outlet></router-outlet>
</main>

<!-- Footer -->
<footer class="public-footer">
  <p>&copy; 2026 Hotel Name. All rights reserved.</p>
  <p>123 Luxury Lane, Paradise City</p>
</footer>


# public-shell.component.scss

.public-header {
  position: sticky;
  top: 0;
  z-index: 10;
  .logo {
    font-size: 1.4rem;
    font-weight: 600;
    cursor: pointer;
    text-decoration: none;
    color: white;
  }
  .spacer { flex: 1 1 auto; }
  .desktop-nav { display: flex; gap: 8px; }
  a.active { font-weight: bold; border-bottom: 2px solid white; }
}
.public-footer {
  background: #f5f5f5;
  text-align: center;
  padding: 16px;
  margin-top: 48px;
  p { margin: 4px 0; color: #666; }
}


# public-shell.component.ts

import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-public-shell',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatToolbarModule, MatButtonModule, MatIconModule, MatMenuModule
  ],
  templateUrl: './public-shell.component.html',
  styleUrls: ['./public-shell.component.scss']
})
export class PublicShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 768px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );
}


# src/styles.scss

// Include theming for Angular Material with `mat.theme()`.
// This Sass mixin will define CSS variables that are used for styling Angular Material
// components according to the Material 3 design spec.
// Learn more about theming and how to use it for your application's
// custom components at https://material.angular.dev/guide/theming
@use '@angular/material' as mat;

html {
  height: 100%;
  @include mat.theme(
    (
      color: (
        primary: mat.$azure-palette,
        tertiary: mat.$blue-palette,
      ),
      typography: Roboto,
      density: 0,
    )
  );
}

// Global containment rule (spec §2)
*,
*::before,
*::after {
  box-sizing: border-box;
}

body {
  // Default the application to a light color theme. This can be changed to
  // `dark` to enable the dark color theme, or to `light dark` to defer to the
  // user's system settings.
  color-scheme: light;

  // Set a default background, font and text colors for the application using
  // Angular Material's system-level CSS variables. Learn more about these
  // variables at https://material.angular.dev/guide/system-variables
  background-color: var(--mat-sys-surface);
  color: var(--mat-sys-on-surface);
  font: var(--mat-sys-body-medium);

  // Reset the user agent margin.
  margin: 0;
  height: 100%;
  overflow-x: hidden;
}

.table-section {
  max-width: 100%;
  overflow-x: auto;
}

// Prevent text inflation and improve touch usability (spec §5.1)
html,
body {
  -webkit-text-size-adjust: 100%;
  touch-action: manipulation;
}

// Prevent media elements from causing overflow (spec §5.1)
img,
video,
canvas,
svg {
  max-width: 100%;
  height: auto;
}

@media (max-width: 500px) {
  mat-form-field,
  mat-button-toggle-group,
  .mat-button-toggle-group {
    width: 100%;
  }

  .table-section,
  .mat-table-container {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .mat-card,
  .kpi-card,
  .health-cards .mat-card {
    margin: 8px 0;
    padding: 12px;
  }
}
/* You can add global styles to this file, and also import other style files */

@media (max-width: 360px) {
  body {
    font-size: 14px;
  }
  mat-card {
    margin: 4px;
    padding: 8px;
  }
}

.notification-snackbar {
  background: transparent !important;
  box-shadow: none !important;
  .mat-mdc-snackbar-surface { background: transparent; box-shadow: none; }
}

# room-type.model.ts

export interface RoomType {
  id: number;
  name: string;
  description: string | null;
  basePrice: number;
  maxOccupancy: number;
  imageUrls: string[];
  squareFootage: number | null;
  bedConfiguration: Record<string, number> | null;
  isActive: boolean;
}

export interface CreateRoomTypeDTO {
  name: string;
  description?: string;
  basePrice: number;
  maxOccupancy: number;
  imageUrls?: string[];
  squareFootage?: number;
  bedConfiguration?: Record<string, number>;
}

export interface UpdateRoomTypeDTO {
  name?: string;
  description?: string;
  basePrice?: number;
  maxOccupancy?: number;
  imageUrls?: string[];
  squareFootage?: number;
  bedConfiguration?: Record<string, number>;
  isActive?: boolean;
}


