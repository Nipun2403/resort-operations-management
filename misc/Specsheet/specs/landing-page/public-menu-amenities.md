# Specsheet: Public Menu & Amenities Showcase

## 1. Purpose
- Replace the placeholder pages for **Menu** (`/menu`) and **Amenities** (`/amenities`) with fully functional public showcase pages.
- **Menu Page**: fetches available menu items, groups them by category, and displays them in elegantly styled cards with placeholder food icons.
- **Amenities Page**: fetches available amenities and displays them in a grid with name, description, price, and a placeholder amenity icon.
- Both pages are public, use existing API services, and follow the hotel’s visual identity.

## 2. Files to Create / Modify
| File | Action |
|------|--------|
| `src/app/features/public/pages/menu.component.ts` | New component |
| `src/app/features/public/pages/menu.component.html` | Template |
| `src/app/features/public/pages/menu.component.scss` | Styles |
| `src/app/features/public/pages/amenities.component.ts` | New component |
| `src/app/features/public/pages/amenities.component.html` | Template |
| `src/app/features/public/pages/amenities.component.scss` | Styles |
| **Delete** placeholder files for these two pages. |

## 3. MenuComponent

### 3.1 API
- **Selector**: `app-public-menu`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `MatCardModule`, `MatIconModule`, `MatProgressSpinnerModule`, `MenuItemApiService`, `DestroyRef`.
- **Exact import paths** (abbreviated; agent must use correct paths).

### 3.2 State (signals)
```typescript
groupedMenu = signal<{ category: string; items: MenuItem[] }[]>([]);
loading = signal(false);
error = signal<string | null>(null);
```

### 3.3 Template (exact)
```html
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
```

### 3.4 Logic (exact)
```typescript
export class MenuComponent {
  private menuItemApi = inject(MenuItemApiService);
  private destroyRef = inject(DestroyRef);

  groupedMenu = signal<{ category: string; items: MenuItem[] }[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchMenu();
  }

  private fetchMenu(): void {
    this.loading.set(true);
    this.menuItemApi.getAll({ isAvailable: true, pageSize: 200 }).pipe(
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
```

## 4. AmenitiesComponent

### 4.1 API
- **Selector**: `app-public-amenities`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `MatCardModule`, `MatIconModule`, `MatProgressSpinnerModule`, `AmenityApiService`, `DestroyRef`.

### 4.2 State (signals)
```typescript
amenities = signal<Amenity[]>([]);
loading = signal(false);
error = signal<string | null>(null);
```

### 4.3 Template (exact)
```html
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
```

### 4.4 Logic (exact)
```typescript
export class AmenitiesComponent {
  private amenityApi = inject(AmenityApiService);
  private destroyRef = inject(DestroyRef);

  amenities = signal<Amenity[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchAmenities();
  }

  private fetchAmenities(): void {
    this.loading.set(true);
    this.amenityApi.getAll({ isAvailable: true, pageSize: 200 }).pipe(
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
```

## 5. Styling

### 5.1 Menu (`menu.component.scss`)
```scss
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
```

### 5.2 Amenities (`amenities.component.scss`)
```scss
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
```

**Responsive adjustments:**
- Hero height: 25vh on mobile.
- Grid columns: single column on very small screens.

## 6. Placeholder Images Note
The hero images (`/assets/restaurant-hero.jpg` and `/assets/amenities-hero.jpg`) must be present in the assets folder; if not, use a fallback solid colour or gradient.

## 7. Self‑Review Checklist
- [ ] Menu page fetches available menu items and groups them by category.
- [ ] Each menu card displays a placeholder food icon, name, and price.
- [ ] Amenities page fetches available amenities and displays them in a grid with a placeholder icon, name, description, and price (or “Complimentary”).
- [ ] Both pages handle loading and error states.
- [ ] Responsive layout works on mobile.
- [ ] No console errors.

## 8. Integration Notes
- `MenuItemApiService` and `AmenityApiService` are reused from admin module; they already have `getAll` methods.
- No authentication required; these are public pages.
- The styling uses a consistent hero section similar to the home page for brand identity.
- Next: Availability Page & Booking Redirect Flow.

---

