# Specsheet: Room Detail – Design Refactor (Gallery, Parallax, Glass Overlay)

## 1. Purpose
- Restyle the public **Room Detail** page (`/rooms/:id`) to match the editorial luxury aesthetic, blending the provided desktop and mobile designs into our “Obsidian & Champagne” theme.
- Display a horizontal scrollable image gallery with snap points and parallax effect on select images.
- Overlay a glassmorphic info panel on the gallery with room name and description.
- Show metrics row (price, square footage, guests) and a dynamic bed configuration list.
- Full‑width “Check Availability” button linking to the availability page.
- The page uses the shared `PublicShellComponent` for its navbar and footer; no local header/footer are rendered.
- All existing component logic (API fetching, `checkAvailability()`, etc.) is preserved.

## 2. Files to Modify
| File | Action |
|------|--------|
| `src/app/features/public/pages/room-detail.component.html` | Replace template with new design. |
| `src/app/features/public/pages/room-detail.component.scss` | Replace styles with new design. |
| `src/app/features/public/pages/room-detail.component.ts` | Add parallax scroll handler; keep existing code. |

**No changes** to services, models, guards, or routing.

## 3. TypeScript Additions (Keep All Existing Code)

Add the following members and logic inside the `RoomDetailComponent` class:

```typescript
import { AfterViewInit, ElementRef, ViewChild, OnDestroy } from '@angular/core';

// Parallax
@ViewChild('galleryContainer') galleryRef!: ElementRef<HTMLElement>;
private parallaxObserver: ResizeObserver | null = null;

ngAfterViewInit(): void {
  // Bind scroll listener for parallax after view init
  const gallery = this.galleryRef?.nativeElement;
  if (gallery) {
    gallery.addEventListener('scroll', this.onGalleryScroll);
  }
}

ngOnDestroy(): void {
  if (this.galleryRef?.nativeElement) {
    this.galleryRef.nativeElement.removeEventListener('scroll', this.onGalleryScroll);
  }
}

private onGalleryScroll = (): void => {
  const gallery = this.galleryRef?.nativeElement;
  if (!gallery) return;
  const scrollLeft = gallery.scrollLeft;
  const parallaxImages = gallery.querySelectorAll('.parallax-img') as NodeListOf<HTMLElement>;
  parallaxImages.forEach((img) => {
    const speed = parseFloat(img.getAttribute('data-speed') || '0');
    const imgTag = img.querySelector('img') as HTMLImageElement;
    if (imgTag) {
      imgTag.style.transform = `translateX(${scrollLeft * speed}px) scale(1.1)`;
    }
  });
};
```

We also need to pre‑scale parallax images initially via CSS: `.parallax-img img { transform: scale(1.1); }`

## 4. Template (`room-detail.component.html`) — Exact

```html
<div class="detail-page">
  @if (loading()) {
    <div class="loading-spinner">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <div class="error-state">
      <p>{{ error() }}</p>
      <a routerLink="/rooms" class="back-link">← Back to Villas</a>
    </div>
  } @else if (room()) {
    <!-- Image Gallery Section -->
    <section class="gallery-section">
      <div class="gallery-scroll" #galleryContainer>
        <!-- Design images (static, shown first) -->
        <div class="gallery-item">
          <img
            class="gallery-image"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuD4LaZWIfgwL2Qyr-FRyKY7s3ZeBGdcJ16QgAMFvM14nKYRywPkhIaU2IWn24HPjp6FabFMVsRVkSlYZgbPwza5q8S8FKjd3LW1NJ1WvliItKWNfFcjLufO14MorWS5QFvR-huAFxK8aQWwI6XLxUojdGW1ka_1KnjpW2IlR3xTfYX00SJljqN2J0yEzYcHqY9oZgtdpPSwUT_FBdy8eskDso2wqiH8Pdncrvu4MQEDm1mQgpqxdNm5n2X6kLCDzsWeaHvrR4Si-wt7"
            alt="{{ room()!.name }}"
          />
          <div class="image-overlay-gallery"></div>
        </div>
        <div class="gallery-item parallax-img" data-speed="0.15">
          <img
            class="gallery-image"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuB7BpuMqO3vUONIUaw5w7liiaHIjRBVk5uM3wMsr1SJSlC-y30i-GvGNqosr8gunZDJjTQX3N2e44Pm_25lOPjb_jxPxaeBcin63o8VaFITR-OCYBj1N5eusmPOzg50z8cObybGuAeKATQD9nOTX0owuSacbbcWycidCo5IcC4CCvlps17fvnqa-oHeFMgBX8940Rqk9iBqib83dQzq8MOzfJG1qYi4xQGYVV1Ky2lZLPgu14peo7JlDKH6A414AbvPV-16NSN1vwnZ"
            alt="{{ room()!.name }}"
          />
          <div class="image-overlay-gallery"></div>
        </div>
        <div class="gallery-item">
          <img
            class="gallery-image"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuCvXqkGL5KS6CGCmVgFcRdnC4k2mB07aJG91FFP664rBxYXjtsLZpD_qXdyGMwn4yX8HqMWXb8nc7wqjRSy0ssglhwB6mnoVscAlxXLPGU54uwGSJbQ6o_DCe4PVPwBgHqsQwu-CpTEGYt_vpt9BOCet9ptZWuogIxNj7LYWPtLK_qf6bUuMRv8FCnewP5TDk-y1qxAmfCV8BWM6WxqK31BHExQYqN5NZdVeRQOAVsStLFtyZLClvyfkilL2FeN2HAn7LSt0RMuEmHi"
            alt="{{ room()!.name }}"
          />
          <div class="image-overlay-gallery"></div>
        </div>
        <div class="gallery-item parallax-img" data-speed="-0.1">
          <img
            class="gallery-image"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuBT07wf-yfThimt-RyGEG4UoP6X4fPA3lsiVxRhhLS_DQHFrxspAnNt2a_RSA16Xaws4_P45R-mVRavRKdMpqEI5IXLcjY20Vlr4b3qgWCZShh7ktDXh_DzZeY6ahQgCe0OumlVj1NMU-Bn_Io4q2ZAI0ne_eFlfYMraQ3tOKlY7Z7n331cQhKm2auuwpoKdKfH1ib45hR6eVHexYS7I4CO7cH189s3Rm-eaGHmVDIOo_uj_DL04rESW_57Md2Y8LslN1VOd1mo4seb"
            alt="{{ room()!.name }}"
          />
          <div class="image-overlay-gallery"></div>
        </div>
        <!-- API images (dynamic) -->
        @for (imgUrl of room()!.imageUrls; track imgUrl) {
          <div class="gallery-item">
            <img class="gallery-image" [src]="imgUrl" alt="{{ room()!.name }}" />
            <div class="image-overlay-gallery"></div>
          </div>
        }
      </div>

      <!-- Glass Overlay Info Panel -->
      <div class="glass-info-panel">
        <h1 class="room-title">{{ room()!.name }}</h1>
        <p class="room-description">{{ room()!.description || 'No description available.' }}</p>
        <div class="divider-line"></div>
      </div>
    </section>

    <!-- Metrics & Configuration Section -->
    <section class="details-section">
      <div class="details-grid">
        <!-- Metrics -->
        <div class="metrics-column">
          <div class="metric-item">
            <span class="metric-value">{{ room()!.basePrice | currency }}</span>
            <div class="metric-divider"></div>
            <span class="metric-label">Per Night</span>
          </div>
          <div class="metric-item">
            <span class="metric-value">{{ room()!.squareFootage || '—' }}</span>
            <div class="metric-divider"></div>
            <span class="metric-label">Sq. Ft.</span>
          </div>
          <div class="metric-item">
            <span class="metric-value">{{ room()!.maxOccupancy }}</span>
            <div class="metric-divider"></div>
            <span class="metric-label">Max Guests</span>
          </div>
        </div>
        <!-- Bed Configuration -->
        <div class="config-column">
          <h3 class="config-title">Configuration</h3>
          <ul class="config-list">
            @for (entry of getBedEntries(); track entry[0]) {
              <li class="config-item">
                <span class="config-icon material-symbols-outlined">{{ getBedIcon(entry[0]) }}</span>
                <span class="config-text">{{ entry[1] }} {{ entry[0] }} Bed{{ entry[1] > 1 ? 's' : '' }}</span>
              </li>
            }
            @empty {
              <li class="config-item">
                <span class="config-icon material-symbols-outlined">bed</span>
                <span class="config-text">Ask for details</span>
              </li>
            }
          </ul>
        </div>
      </div>

      <!-- Full‑width CTA -->
      <div class="cta-section">
        <button class="cta-button" (click)="checkAvailability()">
          <span class="cta-text">Check Availability</span>
          <div class="cta-hover-fill"></div>
        </button>
      </div>
    </section>
  }
</div>
```

**TypeScript addition for bed icon mapping:**
```typescript
getBedIcon(bedType: string): string {
  const icons: Record<string, string> = {
    'King': 'king_bed',
    'Queen': 'bed',
    'Twin': 'single_bed',
    'Double': 'bed',
  };
  return icons[bedType] || 'bed';
}
```

## 5. SCSS (`room-detail.component.scss`)

```scss
@import '../../../../styles/theme/index';

.detail-page {
  overflow-x: hidden;
}

// ── Loading / Error ──────────────────────────────
.loading-spinner, .error-state {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 60vh;
}
.error-state {
  flex-direction: column;
  gap: 1rem;
  p { @include font-body-lg; color: var(--color-error); }
  .back-link { @include font-body-md; color: var(--color-secondary); text-decoration: none; }
}

// ── Gallery Section ──────────────────────────────
.gallery-section {
  position: relative;
  height: 100vh;
  min-height: 700px;
  overflow: hidden;
  @media (max-width: 768px) {
    height: 75vh;
    min-height: 500px;
  }
}

.gallery-scroll {
  display: flex;
  overflow-x: auto;
  overflow-y: hidden;
  scroll-snap-type: x mandatory;
  -webkit-overflow-scrolling: touch;
  height: 100%;
  cursor: grab;
  &:active { cursor: grabbing; }
  &::-webkit-scrollbar { display: none; }
}

.gallery-item {
  flex: 0 0 85vw;
  scroll-snap-align: center;
  position: relative;
  margin: 0 0.5rem;
  @media (min-width: 768px) {
    flex: 0 0 70vw;
    margin: 0 1rem;
  }
}

.gallery-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: grayscale(30%) brightness(0.75);
  transition: filter 0.7s, transform 0.1s linear; // transform will be set by parallax JS
  &:hover { filter: brightness(1); }
}

// Parallax initial scale
.parallax-img .gallery-image {
  transform: scale(1.1);
}

.image-overlay-gallery {
  position: absolute;
  inset: 0;
  background: rgba(19, 20, 17, 0.2); // subtle dark overlay
  pointer-events: none;
}

// Glass info panel (overlapping bottom‑left)
.glass-info-panel {
  position: absolute;
  bottom: 3rem;
  left: var(--margin-mobile);
  width: calc(100% - 2 * var(--margin-mobile));
  max-width: 500px;
  @include glass-panel;
  padding: 2rem;
  animation: fadeUp 1s cubic-bezier(0.16, 1, 0.3, 1) 0.2s both;
  @media (min-width: 768px) {
    left: var(--margin-desktop);
    width: 500px;
    padding: 3rem;
  }
  .room-title {
    @include font-display-lg-mobile;
    font-size: clamp(2rem, 6vw, 4rem);
    color: var(--color-secondary);
    line-height: 1;
    margin-bottom: 1rem;
  }
  .room-description {
    @include font-body-md;
    color: var(--color-on-surface-variant);
    margin-bottom: 1.5rem;
  }
  .divider-line {
    height: 1px;
    width: 100%;
    background: rgba(228, 226, 221, 0.3);
  }
}

// ── Details Section ──────────────────────────────
.details-section {
  max-width: var(--container-max);
  margin: var(--section-gap) auto;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin: 4rem auto;
  }
}

.details-grid {
  display: grid;
  grid-template-columns: 7fr 5fr;
  gap: 2rem;
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

// Metrics
.metrics-column {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
  @media (max-width: 600px) {
    grid-template-columns: 1fr;
  }
}
.metric-item {
  .metric-value {
    @include font-headline-md;
    color: var(--color-secondary);
  }
  .metric-divider {
    width: 3rem;
    height: 1px;
    background: rgba(228, 194, 133, 0.5);
    margin: 0.5rem 0;
  }
  .metric-label {
    @include font-label-caps;
    color: var(--color-on-surface-variant);
  }
}

// Bed Configuration
.config-column {
  margin-top: 0;
  @media (min-width: 768px) {
    margin-top: 0; // align with metrics
  }
}
.config-title {
  @include font-label-caps;
  color: var(--color-secondary);
  letter-spacing: 0.3em;
  margin-bottom: 2rem;
  text-transform: uppercase;
}
.config-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}
.config-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  @include font-body-md;
  color: var(--color-on-surface);
  transition: color 0.3s;
  &:hover { color: var(--color-secondary); }
  .config-icon {
    font-size: 1.5rem;
    color: var(--color-secondary);
    transition: transform 0.2s;
  }
  &:hover .config-icon { transform: scale(1.1); }
}

// ── CTA Section ──────────────────────────────────
.cta-section {
  margin-top: var(--section-gap);
  width: 100%;
}
.cta-button {
  width: 100%;
  padding: 3rem 1rem;
  border: 1px solid var(--color-secondary);
  background: transparent;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  display: flex;
  justify-content: center;
  align-items: center;
  .cta-text {
    @include font-display-lg-mobile;
    font-size: clamp(1.5rem, 5vw, 2.5rem);
    color: var(--color-secondary);
    letter-spacing: 0.1em;
    text-transform: uppercase;
    position: relative;
    z-index: 10;
    transition: color 0.5s;
  }
  .cta-hover-fill {
    position: absolute;
    inset: 0;
    background: var(--color-secondary);
    transform: translateY(100%);
    transition: transform 0.7s ease;
  }
  &:hover {
    .cta-text { color: var(--color-background); }
    .cta-hover-fill { transform: translateY(0); }
  }
}

// Fade‑up animation for glass panel
@keyframes fadeUp {
  from { opacity: 0; transform: translateY(2rem); }
  to { opacity: 1; transform: translateY(0); }
}
```

## 6. Responsive Adaptations (Mobile)
- The mobile design replaces the horizontal gallery with a single hero image (we keep the horizontal gallery because it works well on mobile, but the glass overlay adjusts).
- Metrics stack vertically on small screens.
- The glass panel moves to bottom‑left with appropriate sizing.
- Navigation: the fixed bottom CTA from the mobile design is omitted; we use the same “Check Availability” button at the bottom of the details section, which is already responsive.
- The page does NOT include its own navbar or footer; the shared shell provides those.

## 7. Integration Notes
- The `checkAvailability()` method already exists and stores the room type ID in `sessionStorage`.
- The gallery scroll event listener is cleaned up in `ngOnDestroy`.
- The parallax effect uses the gallery’s native scroll; no additional libraries required.
- Images from the design are hardcoded; if API also returns images, they are appended after the design images. The design images remain even if the room changes; this is temporary until real images are available.

## 8. Self‑Review Checklist
- [ ] Horizontal gallery loads with design images first, then any API images.
- [ ] Parallax effect works on images 2 and 4 (if enough items).
- [ ] Glass overlay displays room name and description with fade‑up animation.
- [ ] Metrics show price, square footage, and guest count.
- [ ] Bed configuration list dynamically renders from API data with correct icons.
- [ ] "Check Availability" button stores `selectedRoomTypeId` and navigates to `/availability`.
- [ ] On mobile, layout adapts: gallery remains horizontal, metrics stack, glass panel resizes.
- [ ] No console errors; all existing functionality (API calls, navigation) intact.
- [ ] No local navbar or footer rendered; global shell provides them.

