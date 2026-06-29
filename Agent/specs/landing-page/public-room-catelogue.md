# Specsheet: Public Room Catalogue & Room Detail

## 1. Purpose
- Replace the placeholders for **Room Catalogue** (`/rooms`) and **Room Detail** (`/rooms/:id`) with fully functional public pages.
- **Room Catalogue**: a responsive grid of all active room types, each card showing the primary image, name, price, max occupancy, and a “View Details” button that navigates to the detail page.
- **Room Detail**: a full‑width image gallery (horizontal scroll) of all `imageUrls`, plus complete information (name, description, price, max occupancy, square footage, bed configuration rendered as a list with icons). A “Check Availability” button stores the room type ID in `sessionStorage` and navigates to the availability page.
- Both pages are public and use the existing `RoomTypeApiService`.

## 2. Files to Create / Modify
| File | Action |
|------|--------|
| `src/app/features/public/pages/room-catalogue.component.ts` | New component |
| `src/app/features/public/pages/room-catalogue.component.html` | Template |
| `src/app/features/public/pages/room-catalogue.component.scss` | Styles |
| `src/app/features/public/pages/room-detail.component.ts` | New component |
| `src/app/features/public/pages/room-detail.component.html` | Template |
| `src/app/features/public/pages/room-detail.component.scss` | Styles |
| **Delete** placeholder files for these two pages. |

## 3. RoomCatalogueComponent

### 3.1 API
- **Selector**: `app-room-catalogue`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `RouterModule`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatProgressSpinnerModule`, `RoomTypeApiService`, `DestroyRef`.
- **Exact import paths** (abbreviated; agent must use correct paths).

### 3.2 State (signals)
```typescript
rooms = signal<RoomType[]>([]);
loading = signal(false);
error = signal<string | null>(null);
```

### 3.3 Template (exact)
```html
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
```

### 3.4 Logic (exact)
```typescript
export class RoomCatalogueComponent {
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  rooms = signal<RoomType[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchRooms();
  }

  private fetchRooms(): void {
    this.loading.set(true);
    this.roomTypeApi.getAll({ includeRetired: false, pageSize: 100, sortBy: 'name', sortDescending: false }).pipe(
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
```

## 4. RoomDetailComponent

### 4.1 API
- **Selector**: `app-room-detail`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `RouterModule`, `ActivatedRoute`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatProgressSpinnerModule`, `RoomTypeApiService`, `DestroyRef`.

### 4.2 State
```typescript
room = signal<RoomType | null>(null);
loading = signal(false);
error = signal<string | null>(null);
```

### 4.3 Template (exact)
```html
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
```

### 4.4 Logic (exact)
```typescript
export class RoomDetailComponent {
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
```

## 5. Styling

### 5.1 RoomCatalogue (`room-catalogue.component.scss`)
```scss
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
```

### 5.2 RoomDetail (`room-detail.component.scss`)
```scss
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
```

## 6. Self‑Review Checklist
- [ ] Room Catalogue fetches all active room types and displays them in a grid.
- [ ] Each card shows the first image (or placeholder), name, price, and occupancy.
- [ ] Clicking “View Details” navigates to `/rooms/:id`.
- [ ] Room Detail loads the room type by ID and displays all images in a horizontally scrollable gallery.
- [ ] Detail page shows name, description, price, max occupancy, square footage, and bed configuration.
- [ ] “Check Availability” button stores the room type ID in `sessionStorage` and navigates to `/availability?roomTypeId=X`.
- [ ] Responsive layout works on mobile (grid single column, gallery images full width).
- [ ] Placeholder images display when no images are available.
- [ ] No console errors.

## 7. Integration Notes
- `RoomTypeApiService.getById` must exist; if not, add it as a simple `GET /api/v1/room-types/{id}` call.
- The availability page will later read the `roomTypeId` from query params and `selectedRoomTypeId` from session storage to pre‑fill the search.
- The “Back to Rooms” link in the detail page ensures easy navigation.

---

