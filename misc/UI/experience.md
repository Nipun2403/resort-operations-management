# Specsheet: Dining & Amenities Page ( `/experiences` ) – Exhaustive

## 1. Purpose
- Create the combined **Dining & Amenities** page at `/experiences` as a standalone public component.
- The page includes:
  - Hero editorial header.
  - Dynamic horizontal accordion menu (grouped by category from the API).
  - Paginated bento‑grid amenities section (3 cards per view) with gold‑wash transition, chevron navigation, and hover overlay effects.
  - Static philosophy / archive section with provided image and text.
- The page relies on the shared `PublicShellComponent` for header and footer; no local navbar or footer are rendered.
- All existing API services are reused; no logic changes to services.

## 2. Files to Create / Modify
| File | Action |
|------|--------|
| `src/app/features/public/pages/experiences.component.ts` | New standalone component with state, API calls, accordion, pagination. |
| `src/app/features/public/pages/experiences.component.html` | Full template. |
| `src/app/features/public/pages/experiences.component.scss` | Full styles. |
| `src/app/app.routes.ts` (or the public route file) | Already has `/experiences` route; ensure it points to the new component (replace placeholder). |

## 3. Component Logic (exact TypeScript)
```typescript
import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { forkJoin, finalize } from 'rxjs';
import { MenuItemApiService } from '../../../admin/services/menu-item-api.service';
import { AmenityApiService } from '../../../admin/services/amenity-api.service';
import { MenuItem } from '../../../admin/models/menu-item.model';
import { Amenity } from '../../../admin/models/amenity.model';

@Component({
  selector: 'app-experiences',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatProgressSpinnerModule],
  templateUrl: './experiences.component.html',
  styleUrls: ['./experiences.component.scss']
})
export class ExperiencesComponent {
  private menuItemApi = inject(MenuItemApiService);
  private amenityApi = inject(AmenityApiService);
  private destroyRef = inject(DestroyRef);

  // Menu
  menuLoading = signal(false);
  menuError = signal<string | null>(null);
  menuGroups = signal<{ category: string; items: MenuItem[] }[]>([]);
  expandedCategory = signal<string | null>(null);

  // Amenities
  amenitiesLoading = signal(false);
  amenitiesError = signal<string | null>(null);
  allAmenities = signal<Amenity[]>([]);
  amenityPageIndex = signal(0);
  itemsPerPage = 3;
  totalAmenityPages = computed(() => Math.ceil(this.allAmenities().length / this.itemsPerPage));
  displayAmenities = computed(() => {
    const start = this.amenityPageIndex() * this.itemsPerPage;
    return this.allAmenities().slice(start, start + this.itemsPerPage);
  });

  // Transition state
  amenityIsTransitioning = signal(false);
  private readonly ANIMATION_DURATION = 600;

  ngOnInit(): void {
    this.fetchData();
  }

  private fetchData(): void {
    this.menuLoading.set(true);
    this.amenitiesLoading.set(true);

    const menu$ = this.menuItemApi.getAll({ isAvailable: true, pageSize: 200 }).pipe(
      map(res => {
        const groups: Record<string, MenuItem[]> = {};
        for (const item of res.data) {
          const cat = item.category || 'Other';
          if (!groups[cat]) groups[cat] = [];
          groups[cat].push(item);
        }
        return Object.entries(groups).map(([category, items]) => ({ category, items }));
      })
    );
    const amenities$ = this.amenityApi.getAll({ isAvailable: true, pageSize: 100 }).pipe(
      map(res => res.data)
    );

    forkJoin([menu$, amenities$]).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.menuLoading.set(false);
        this.amenitiesLoading.set(false);
      })
    ).subscribe({
      next: ([groups, amenities]) => {
        this.menuGroups.set(groups);
        this.allAmenities.set(amenities);
      },
      error: (err: any) => {
        this.menuError.set(this.extractErrorMessage(err));
        this.amenitiesError.set(this.extractErrorMessage(err));
      }
    });
  }

  // Menu accordion
  toggleCategory(category: string): void {
    this.expandedCategory.set(this.expandedCategory() === category ? null : category);
  }

  // Amenity pagination
  nextAmenityPage(): void {
    if (this.amenityPageIndex() < this.totalAmenityPages() - 1 && !this.amenityIsTransitioning()) {
      this.triggerAmenityTransition(() => this.amenityPageIndex.update(i => i + 1));
    }
  }
  prevAmenityPage(): void {
    if (this.amenityPageIndex() > 0 && !this.amenityIsTransitioning()) {
      this.triggerAmenityTransition(() => this.amenityPageIndex.update(i => i - 1));
    }
  }
  private triggerAmenityTransition(updateFn: () => void): void {
    this.amenityIsTransitioning.set(true);
    setTimeout(() => {
      updateFn();
      setTimeout(() => this.amenityIsTransitioning.set(false), this.ANIMATION_DURATION);
    }, 100);
  }

  // Amenity image fallback (TODO: real images from backend later)
  getAmenityImage(amenity: Amenity): string {
    // TODO: backend will provide amenity images; for now, duplicate design images
    const designImages = [
      'https://lh3.googleusercontent.com/aida-public/AB6AXuAdW5i14tYjpRFDsySVWECF6hlJhhTBDM_2iyrGdU2-XAB3bXyzD3yVLXHWZyo2e2LZ3uX1G1jSLZwlItX3dGYPS913zkA-FfA1LByafCBqsTY6IvyqHbvD3bqkQVrbrp1bpHP8PgE5jpFQ_Z64hfgMg0oqcs7DYWc51yLI8NhbHew3ODOZnYr6tNUqKlwV7UL9hGKdUxzpi8nDVixmT_rpoGbYFScKbbT1JJVZHHqX7kQI5bi2Ez1s8oRBXtMW4VIRwPPAcUwTthmL',
      'https://lh3.googleusercontent.com/aida-public/AB6AXuCm9-2a_nAuBChLPNbo_8xZPTuxw_sFMl7WV7DjfWPM5PPHMj1QV-LZ9macLM4UelYuBaCxBWZwcG28rGYVooVP0oh1o7__5O7HWtlcGStiL5cX7gmdw_8I5oY0eyZA0iNYfCnefdLJnh0kXszMos0_kYvAUIOfaO4th3XshoyUFrcqhWJbaCGyjim0v_tfmL2IA-xYP0KOMCojfpJ5q4h28YTgUupgt7h4lj1NGlO2wTmhoHtWKnW2aHj9oq8pOic2OWFK4O8F7FZV',
      'https://lh3.googleusercontent.com/aida-public/AB6AXuAwLAmuLY0fR8D6Oh6SSzFHVLR_yq9yeaTuwjzoG_aEN9SGxscZpdZW7TlMXwfwROcjG47GVnu9MWhZd0yWinvSFVKPgxbp1N-7sJdU69q1Z5C8ref4bCIN2C38sZE1bGrPg9Qc4N56qylrsex2kE5wbmNtevNEZZQB_Qyt2pUFnILPsymu8OLj9PGfiBy5PJPY0GZfxapYekH-qSydKwwAPbNMxnyd9zMkvntWmPEvGgmgPEzBH0aCk-_wkqJeBk_KMcYVZ3IKk7MY'
    ];
    return designImages[0]; // simply reuse first design image; later will use amenity.imageUrls
  }

  // Amenity number label (global index)
  getAmenityNumber(index: number): string {
    const globalIndex = this.amenityPageIndex() * this.itemsPerPage + index + 1;
    return `[ ${globalIndex.toString().padStart(2, '0')} ]`;
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
```

## 4. Template (`experiences.component.html`)
```html
<div class="experiences-page">
  <!-- Hero Section -->
  <section class="hero">
    <h1>Culinary Art &amp;<br><em>Absolute Stillness.</em></h1>
    <p>Aetheris dining is an exercise in restraint. From the private cellar to the starlit conservatory, every ingredient is sourced from the estate's own heritage soil or the deep Atlantic shelf.</p>
  </section>

  <!-- Menu Section (Accordion) -->
  <section class="menu-section">
    <div class="section-label">EPICUREAN SELECTIONS</div>

    @if (menuLoading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (menuError()) {
      <p class="error">{{ menuError() }}</p>
    } @else {
      @for (group of menuGroups(); track group.category; let i = $index) {
        <div class="menu-row" [class.active]="expandedCategory() === group.category">
          <div class="menu-row-header" (click)="toggleCategory(group.category)">
            <h2>{{ group.category }}</h2>
            <span class="category-number">[ {{ (i + 1).toString().padStart(2, '0') }} ]</span>
            <span class="expand-icon material-symbols-outlined">{{ expandedCategory() === group.category ? 'expand_less' : 'expand_more' }}</span>
          </div>
          <div class="menu-content">
            @for (item of group.items; track item.id) {
              <div class="menu-item">
                <span class="item-name">{{ item.name }}</span>
                <span class="dotted-line"></span>
              </div>
            }
          </div>
        </div>
      }
    }
  </section>

  <!-- Amenities Section (Bento Grid with Pagination) -->
  <section class="amenities-section">
    <div class="section-label">PRIVATE AMENITIES</div>

    @if (amenitiesLoading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (amenitiesError()) {
      <p class="error">{{ amenitiesError() }}</p>
    } @else {
      <div class="amenities-grid-container" [class.transitioning]="amenityIsTransitioning()">
        <!-- Chevron Navigation -->
        <button class="nav-chevron left" (click)="prevAmenityPage()" [disabled]="amenityPageIndex() === 0">
          <span class="material-symbols-outlined">chevron_left</span>
        </button>
        <button class="nav-chevron right" (click)="nextAmenityPage()" [disabled]="amenityPageIndex() === totalAmenityPages() - 1">
          <span class="material-symbols-outlined">chevron_right</span>
        </button>

        <div class="bento-grid">
          @for (amenity of displayAmenities(); track amenity.id; let i = $index) {
            <div class="amenity-card" [ngClass]="'card-' + i">
              <div class="amenity-image" [style.background-image]="'url(' + getAmenityImage(amenity) + ')'">
                <div class="image-overlay"></div>
                <div class="card-number">{{ getAmenityNumber(i) }}</div>
                <!-- Hover overlay -->
                <div class="hover-overlay">
                  <h3>{{ amenity.name }}</h3>
                  <p>{{ amenity.description || 'Indulge in quiet luxury.' }}</p>
                </div>
              </div>
            </div>
          }
        </div>

        <!-- Page indicator -->
        <div class="page-indicator">
          {{ amenityPageIndex() + 1 }} / {{ totalAmenityPages() }}
        </div>
      </div>
    }
  </section>

  <!-- Archive / Philosophy Section -->
  <section class="archive-section">
    <div class="archive-image" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuBcsrNli-Okq5zfPIxKgYeYhE_LK_uzemUGThdLc6zjw4pLEyJoDD5vOPjJLUF5LaK3qhIWcf59hlxQyDJ24Si6HYUpBvsfVOZYN4eNrJl-PGygA4awKDqaKCKzq_HnljgeiOdsWoUY6qrDR76iNBnoV_QoatCSBws27OYJZFTkUdJqLQYpS4-QXL_SkDrTNybanRN0yPZRPcboei3Wa-m5mhIEpHV6Kwi6Y-Zfqdqa5wuVDCkoYZCJtgew-BJlAUhr7x85SimSrV2x')"></div>
    <div class="archive-content">
      <h2>The Philosophy of <br><em>Permanent Quality.</em></h2>
      <ul>
        <li><span class="label">ORIGIN</span><p>Every harvest is cataloged and preserved in our subterranean vault, accessible only to resident guests.</p></li>
        <li><span class="label">RITUAL</span><p>Dining is a timed performance, lasting precisely four hours from sunset to stellar zenith.</p></li>
        <li><span class="label">SILENCE</span><p>The Dining Room maintains a zero-decibel acoustic standard outside of orchestrated service.</p></li>
      </ul>
    </div>
  </section>
</div>
```

## 5. SCSS (`experiences.component.scss`)
```scss
@import '../../../../styles/theme/index';

.experiences-page {
  overflow-x: hidden;
}

// ── Hero ────────────────────────────────────────
.hero {
  max-width: var(--container-max);
  margin: 0 auto 6rem;
  padding: 10rem var(--margin-desktop) 0;
  @media (max-width: 768px) {
    padding: 6rem var(--margin-mobile) 0;
    margin-bottom: 3rem;
  }
  h1 {
    @include font-display-lg;
    font-size: clamp(2.5rem, 8vw, 4.5rem);
    color: var(--color-on-surface);
    margin-bottom: 1rem;
    em { font-style: italic; color: var(--color-secondary); }
  }
  p {
    @include font-body-lg;
    color: rgba(228, 226, 221, 0.7);
    max-width: 600px;
  }
}

.section-label {
  @include font-label-caps;
  color: var(--color-secondary);
  margin-bottom: 2rem;
  letter-spacing: 0.2em;
}

// ── Menu Accordion ──────────────────────────────
.menu-section {
  max-width: var(--container-max);
  margin: 0 auto 6rem;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin-bottom: 3rem;
  }
}

.menu-row {
  border-bottom: 1px solid rgba(228, 194, 133, 0.2);
  transition: all 0.6s cubic-bezier(0.22, 1, 0.36, 1);
  &.active { padding-bottom: 2.5rem; }
}

.menu-row-header {
  display: flex;
  align-items: flex-end;
  padding: 2rem 0;
  cursor: pointer;
  transition: color 0.3s;
  &:hover h2 { color: var(--color-secondary); }
  h2 {
    @include font-display-lg;
    font-size: clamp(1.5rem, 5vw, 2.5rem);
    color: var(--color-on-surface);
    text-transform: uppercase;
    margin: 0;
    transition: color 0.5s;
  }
  .category-number {
    @include font-label-caps;
    color: rgba(228, 226, 221, 0.4);
    margin-left: auto;
    margin-right: 1rem;
  }
  .expand-icon {
    color: var(--color-on-surface-variant);
    transition: transform 0.3s;
  }
}

.menu-content {
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.8s ease;
  max-width: 800px;
}
.menu-row.active .menu-content {
  max-height: 2000px; // large enough
  margin-top: 1rem;
}

.menu-item {
  display: flex;
  align-items: flex-end;
  padding: 0.5rem 0;
  .item-name {
    @include font-headline-sm;
    color: var(--color-on-surface);
  }
  .dotted-line {
    flex-grow: 1;
    border-bottom: 1px dotted rgba(228, 194, 133, 0.4);
    margin: 0 1rem 0.3rem;
  }
}

// ── Amenities Bento Grid ─────────────────────────
.amenities-section {
  max-width: var(--container-max);
  margin: 0 auto 6rem;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin-bottom: 3rem;
  }
}

.amenities-grid-container {
  position: relative;
  &.transitioning .amenity-image::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(105deg, transparent 40%, rgba(228, 194, 133, 0.3), transparent 60%);
    animation: goldSweep 0.6s ease forwards;
  }
}

.bento-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  grid-template-rows: repeat(2, 1fr);
  gap: 2rem;
  height: 800px;
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    grid-template-rows: auto;
    height: auto;
  }
}

.amenity-card {
  position: relative;
  overflow: hidden;
  cursor: pointer;
  &:hover .hover-overlay { transform: translateY(0); }
  &.card-0 {
    grid-column: span 7;
    grid-row: span 2;
    @media (max-width: 768px) { grid-column: span 1; grid-row: auto; height: 500px; }
  }
  &.card-1 {
    grid-column: span 5;
    grid-row: span 1;
    @media (max-width: 768px) { grid-column: span 1; grid-row: auto; height: 400px; }
  }
  &.card-2 {
    grid-column: span 5;
    grid-row: span 1;
    @media (max-width: 768px) { grid-column: span 1; grid-row: auto; height: 400px; }
  }
}

.amenity-image {
  position: absolute;
  inset: 0;
  background-size: cover;
  background-position: center;
  filter: grayscale(30%) brightness(0.7);
  transition: transform 1s, filter 0.7s;
  .amenity-card:hover & { transform: scale(1.05); filter: grayscale(0%) brightness(1); }
}

.image-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, var(--color-background) 0%, transparent 50%);
}

.card-number {
  position: absolute;
  top: 1.5rem;
  left: 1.5rem;
  @include font-label-caps;
  color: var(--color-secondary);
  transition: color 0.3s;
}

.hover-overlay {
  position: absolute;
  inset: 0;
  @include glass-panel;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  padding: 2rem;
  transform: translateY(100%);
  transition: transform 0.7s cubic-bezier(0.19, 1, 0.22, 1);
  h3 {
    @include font-headline-md;
    color: var(--color-secondary);
    margin-bottom: 0.5rem;
  }
  p {
    @include font-body-md;
    color: var(--color-on-surface);
  }
}

// Chevron Navigation
.nav-chevron {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  z-index: 10;
  background: rgba(26, 26, 26, 0.7);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(228, 194, 133, 0.3);
  color: var(--color-on-surface);
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  opacity: 0;
  transition: opacity 0.3s;
  &:hover { border-color: var(--color-secondary); color: var(--color-secondary); }
  &:disabled { opacity: 0; pointer-events: none; }
  &.left { left: -1rem; }
  &.right { right: -1rem; }
}
.amenities-grid-container:hover .nav-chevron { opacity: 1; }
@media (max-width: 768px) {
  .nav-chevron { display: none; }
}

.page-indicator {
  text-align: center;
  @include font-label-caps;
  color: var(--color-outline);
  margin-top: 1.5rem;
  letter-spacing: 0.3em;
}

@keyframes goldSweep {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

// ── Archive Section ──────────────────────────────
.archive-section {
  display: grid;
  grid-template-columns: 4fr 6fr;
  gap: 2rem;
  max-width: var(--container-max);
  margin: 0 auto;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    padding: 0 var(--margin-mobile);
  }
}
.archive-image {
  height: 600px;
  background-size: cover;
  background-position: center;
  filter: grayscale(1);
  @media (max-width: 768px) { height: 400px; }
}
.archive-content {
  display: flex;
  flex-direction: column;
  justify-content: center;
  h2 {
    @include font-headline-md;
    font-size: clamp(1.5rem, 4vw, 2rem);
    color: var(--color-on-surface);
    margin-bottom: 2rem;
    em { color: var(--color-secondary); }
  }
  ul {
    list-style: none;
    li {
      display: flex;
      gap: 1rem;
      padding: 1.5rem 0;
      border-bottom: 1px solid rgba(228, 226, 221, 0.1);
      .label {
        @include font-label-caps;
        color: var(--color-secondary);
        min-width: 80px;
      }
      p { @include font-body-md; color: rgba(228, 226, 221, 0.7); }
    }
  }
}

// Error
.error {
  @include font-body-lg;
  color: var(--color-error);
}
```

## 6. Mobile Adaptations
- Amenities bento grid collapses to vertical stack. Chevrons hidden; swipe or touch? For mobile, user can scroll vertically, pagination via dots indicator. We keep the chevrons hidden and rely on the page indicator; user can scroll naturally. (No swipe interaction needed; the grid simply becomes a long vertical list on mobile, showing all amenities at once. The pagination controls are only for desktop bento grid. To avoid confusion, on mobile we disable pagination and show all amenities vertically. We can achieve this by setting `itemsPerPage` to a large number on mobile, or simply skip pagination on mobile. For simplicity, on mobile we'll set `displayAmenities = allAmenities` and remove chevrons. We'll detect viewport using a media query in SCSS to hide pagination controls on mobile, but the component still paginates. That's okay; the user will only see 3 at a time even on mobile unless we change the logic. The design's mobile view shows two amenity cards. We'll keep 3, but vertically stacked, no pagination needed. So on mobile we can just set `itemsPerPage` to `allAmenities().length` for mobile. But the component doesn't know the viewport. Simpler: in the template, we can wrap the bento grid and pagination inside a `@media` query? No, CSS can't change Angular logic. We'll add a `isMobile` signal using `BreakpointObserver` and dynamically adjust `itemsPerPage`. Add to component:

```typescript
isMobile = toSignal(this.breakpointObserver.observe('(max-width: 768px)').pipe(map(r => r.matches)), { initialValue: false });
itemsPerPageComputed = computed(() => this.isMobile() ? this.allAmenities().length : 3);
displayAmenities = computed(() => {
  const start = this.amenityPageIndex() * this.itemsPerPageComputed();
  return this.allAmenities().slice(start, start + this.itemsPerPageComputed());
});
```

Then `totalAmenityPages` also computed similarly. That way mobile shows all at once and pagination controls hide via CSS (since `totalAmenityPages() <= 1`). Good.

We'll incorporate this in the component logic.

## 7. Integration Notes
- The page uses existing `MenuItemApiService` and `AmenityApiService` from admin module; ensure they are provided in root.
- The custom cursor is omitted; global footer from shell applies.
- The amenity images temporarily use a duplicated design image; `// TODO` comments guide future backend integration.
- No prices are displayed in menu.
- The archive section is static.

## 8. Self‑Review Checklist
- [ ] Hero text and description render correctly.
- [ ] Menu accordion groups by category; only one open at a time; smooth transition.
- [ ] Menu items shown with dotted connector, no price.
- [ ] Amenities bento grid shows 3 cards on desktop; first card large (7 cols), other two stacked (5 cols each).
- [ ] Chevron navigation cycles through amenity pages; gold wash animation on transition.
- [ ] Hover over amenity card reveals glass overlay with name and description.
- [ ] On mobile, amenities display all cards vertically without pagination controls.
- [ ] Archive section displays image and philosophy list.
- [ ] No local header/footer; global shell provides them.
- [ ] No console errors; API calls function correctly.

