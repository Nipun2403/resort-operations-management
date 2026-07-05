# Specsheet: Room Catalogue – Design Refactor (Editorial Scroll) – Exhaustive

## 1. Purpose
- Restyle the public **Room Catalogue** page (`/rooms`) to match the “Obsidian & Champagne” editorial design.
- Replace the existing grid layout with a horizontally scrollable, asymmetric card layout that dynamically displays all active room types fetched from the backend.
- Add a static newsletter section with an inline success message (no backend call).
- **No existing functionality is modified** – API calls, navigation, session storage, and component methods remain intact.
- The global footer and custom cursor will be updated in a later specsheet; this page relies on the shared `PublicShellComponent` for its navbar and footer.

## 2. Files to Modify
| File | Action |
|------|--------|
| `src/app/features/public/pages/room-catalogue.component.html` | Replace template with new editorial design. |
| `src/app/features/public/pages/room-catalogue.component.scss` | Replace styles with new design using theme tokens and mixins. |
| `src/app/features/public/pages/room-catalogue.component.ts` | Add newsletter `FormControl` and `subscribed` signal; do **not** remove any existing code. |

**No changes** to services, models, guards, or routing.  
The component’s existing signals (`rooms`, `loading`, `error`) and methods (`fetchRooms`, `getFirstImage`, `viewRoom`) remain fully functional.

## 3. Data Source & Image Handling
- Room data is fetched from `GET /api/v1/room-types?includeRetired=false&pageSize=100` via the existing `RoomTypeApiService.getAll()` call inside `fetchRooms()`.
- Each room card displays the first image from the `imageUrls` array using the existing `getFirstImage(room)` helper:
  ```typescript
  getFirstImage(room: RoomType): string {
    return room.imageUrls && room.imageUrls.length > 0
      ? room.imageUrls[0]
      : 'assets/placeholder-room.jpg';
  }
  ```
- The placeholder image `assets/placeholder-room.jpg` must exist; if not, the build will use a fallback solid colour defined in CSS.
- **No hardcoded design image URLs** are used; the page is fully dynamic.

## 4. Template (`room-catalogue.component.html`) – Exact Markup
```html
<div class="catalogue-page">
  <!-- Hero Header -->
  <header class="hero-header">
    <span class="section-label">PRIVATE SANCTUARIES</span>
    <h1>The Villa Collection</h1>
    <p class="hero-description">
      A curated selection of architectural masterpieces nestled within the ancient slopes. Each villa is a private world, designed for silence, reflection, and the quiet pursuit of excellence.
    </p>
  </header>

  <!-- Room Cards – Horizontally Scrollable -->
  <section class="rooms-section">
    @if (loading()) {
      <div class="loading-state">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (error()) {
      <div class="error-state">
        <p>{{ error() }}</p>
        <button class="retry-btn" (click)="fetchRooms()">Retry</button>
      </div>
    } @else {
      <div class="scroll-container">
        @for (room of rooms(); track room.id; let i = $index) {
          <article
            class="room-card"
            [class.first-card]="i === 0"
          >
            <div class="card-image" [style.background-image]="'url(' + getFirstImage(room) + ')'">
              <div class="image-overlay"></div>
              <div class="card-number">
                {{ (i + 1).toString().padStart(2, '0') }} / VILLA
              </div>
            </div>
            <div class="card-body">
              <h2 class="room-name">{{ room.name }}</h2>
              <div class="room-meta">
                <span>Max. {{ room.maxOccupancy }} Guests</span>
                <span class="separator">·</span>
                <span>{{ room.squareFootage || '—' }} sqm</span>
              </div>
              <a [routerLink]="['/rooms', room.id]" class="view-link">
                VIEW DETAILS <span class="arrow">→</span>
              </a>
            </div>
          </article>
        }
      </div>
    }
  </section>

  <!-- Newsletter Section -->
  <section class="newsletter">
    <h2 class="newsletter-title">Stay Informed</h2>
    <div class="newsletter-form">
      <input
        type="email"
        class="newsletter-input"
        placeholder="YOUR EMAIL ADDRESS"
        [formControl]="emailControl"
        (keyup.enter)="subscribe()"
      />
      @if (!subscribed()) {
        <button class="subscribe-btn" (click)="subscribe()">Subscribe</button>
      } @else {
        <span class="success-message">Thank you for your interest.</span>
      }
    </div>
    <!-- TODO: wire up newsletter subscription to backend -->
  </section>
</div>
```

## 5. TypeScript Additions (to existing `RoomCatalogueComponent`)
**File:** `src/app/features/public/pages/room-catalogue.component.ts`  
Add the following members **inside the class body** (do not delete any existing code):

```typescript
import { FormControl } from '@angular/forms';
import { signal } from '@angular/core';

// ... inside the component class
emailControl = new FormControl('', { nonNullable: true });
subscribed = signal(false);

subscribe(): void {
  if (!this.emailControl.value || this.subscribed()) return;
  this.emailControl.setValue('');
  this.subscribed.set(true);
  // TODO: wire up newsletter subscription to backend
}
```

## 6. SCSS (`room-catalogue.component.scss`) – Exact Rules
```scss
@import '../../../../styles/theme/index';

.catalogue-page {
  overflow-x: hidden;
  padding-top: 5rem; // offset for fixed navbar
}

// ── Hero Header ──────────────────────────────────
.hero-header {
  max-width: var(--container-max);
  margin: 0 auto 4rem;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
  }
  .section-label {
    @include font-label-caps;
    color: var(--color-secondary);
    margin-bottom: 0.5rem;
  }
  h1 {
    @include font-display-lg;
    font-size: clamp(2.5rem, 8vw, 5rem);
    margin-bottom: 1.5rem;
    color: var(--color-on-surface);
  }
  .hero-description {
    @include font-body-lg;
    color: rgba(228, 226, 221, 0.6);
    max-width: 600px;
  }
}

// ── Loading & Error States ───────────────────────
.loading-state,
.error-state {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 4rem 0;
}
.error-state {
  flex-direction: column;
  gap: 1rem;
  p {
    @include font-body-lg;
    color: var(--color-error);
  }
  .retry-btn {
    @include font-label-caps;
    background: transparent;
    border: 1px solid var(--color-secondary);
    color: var(--color-secondary);
    padding: 0.5rem 1.5rem;
    cursor: pointer;
    &:hover {
      background: var(--color-secondary);
      color: var(--color-on-secondary);
    }
  }
}

// ── Scrollable Room Cards ────────────────────────
.rooms-section {
  width: 100%;
  margin-bottom: var(--section-gap);
}

.scroll-container {
  display: flex;
  gap: 2rem;
  overflow-x: auto;
  overflow-y: hidden;
  scroll-snap-type: x mandatory;
  -webkit-overflow-scrolling: touch;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
  }
  &::-webkit-scrollbar { display: none; }
}

.room-card {
  flex: 0 0 42vw;
  scroll-snap-align: start;
  transition: transform 0.4s ease;
  cursor: pointer;
  @media (max-width: 768px) {
    flex: 0 0 75vw;
  }
  &:hover {
    transform: translateY(-4px);
    .card-image { transform: scale(1.03); }
  }
  &.first-card {
    flex: 0 0 60vw;
    @media (max-width: 768px) { flex: 0 0 85vw; }
  }
}

.card-image {
  aspect-ratio: 4/3;
  background-size: cover;
  background-position: center;
  transition: transform 0.6s cubic-bezier(0.2, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
  // If no image, show a dark background
  background-color: var(--color-surface-container-low);
  .first-card & {
    aspect-ratio: 16/9;
  }
}

.image-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, var(--color-background) 0%, transparent 50%);
}

.card-number {
  position: absolute;
  bottom: 1rem;
  left: 1rem;
  @include font-label-caps;
  font-size: 0.625rem;
  letter-spacing: 0.3em;
  color: var(--color-secondary);
}

.card-body {
  padding: 1.5rem 0;
  border-bottom: 1px solid rgba(228, 194, 133, 0.2);
}

.room-name {
  @include font-headline-sm;
  color: var(--color-on-surface);
  margin-bottom: 0.5rem;
}

.room-meta {
  @include font-body-md;
  color: var(--color-outline-variant);
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
  .separator { color: var(--color-outline-variant); }
}

.view-link {
  @include font-label-caps;
  color: var(--color-on-surface);
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  .arrow {
    transition: transform 0.3s;
  }
  &:hover .arrow {
    transform: translateX(4px);
    color: var(--color-secondary);
  }
}

// ── Newsletter Section ────────────────────────────
.newsletter {
  background: var(--color-surface-container-lowest);
  border-top: 1px solid var(--color-surface-container-highest);
  padding: var(--section-gap) var(--margin-desktop);
  text-align: center;
  @media (max-width: 768px) {
    padding: 4rem var(--margin-mobile);
  }
}

.newsletter-title {
  @include font-display-lg-mobile;
  font-size: clamp(2rem, 5vw, 3rem);
  text-transform: uppercase;
  letter-spacing: 0.5em;
  color: var(--color-on-surface);
  margin-bottom: 3rem;
}

.newsletter-form {
  max-width: 600px;
  margin: 0 auto;
  border-bottom: 1px solid rgba(228, 226, 221, 0.2);
  display: flex;
  align-items: center;
  padding-bottom: 0.5rem;
}

.newsletter-input {
  flex: 1;
  background: transparent;
  border: none;
  color: var(--color-on-surface);
  @include font-label-caps;
  letter-spacing: 0.2em;
  padding: 0.5rem 0;
  outline: none;
  &::placeholder {
    color: rgba(228, 226, 221, 0.2);
  }
}

.subscribe-btn {
  @include font-label-caps;
  background: transparent;
  border: none;
  color: var(--color-secondary);
  cursor: pointer;
  transition: letter-spacing 0.5s, opacity 0.3s;
  &:hover { letter-spacing: 0.3em; }
}

.success-message {
  @include font-label-caps;
  color: var(--color-secondary);
  opacity: 0;
  animation: fadeInSuccess 0.6s ease forwards;
}

@keyframes fadeInSuccess {
  from { opacity: 0; transform: translateY(4px); }
  to { opacity: 1; transform: translateY(0); }
}
```

## 7. Responsive Behaviour (Summary)
- **Desktop:** first card occupies 60vw, other cards 42vw; scroll bar hidden.
- **Mobile/Tablet (≤768px):** first card 85vw, others 75vw; margins adjusted.
- Newsletter form remains horizontally aligned; on very narrow screens (<400px) the flex items may wrap (acceptable).
- Hero text scales with viewport via `clamp()`.

## 8. Integration Notes
- The component’s `ngOnInit` already calls `fetchRooms()`. No changes there.
- The `subscribe()` method is pure frontend; a `TODO` comment marks future backend integration.
- The template uses `mat-spinner` from `MatProgressSpinnerModule`, which is already imported in the component.
- No custom cursor or footer changes are included; those will be delivered in the next global specsheet.

## 9. Self‑Review Checklist
- [ ] Hero header displays with label, title, and description.
- [ ] Room cards are horizontally scrollable (scrollbar hidden).
- [ ] First card has a larger aspect ratio (16/9) and wider width.
- [ ] Each card shows the room’s first image (or placeholder), a numbered overlay label, room name, metadata, and a “VIEW DETAILS” link.
- [ ] Clicking “VIEW DETAILS” navigates to `/rooms/:id` using the actual room type ID.
- [ ] Loading spinner and error state work correctly.
- [ ] Newsletter section: email input and Subscribe button; after subscribe, success message appears with fade animation.
- [ ] No console errors; existing API calls, navigation, and session storage remain unaffected.
- [ ] Responsive layout works on mobile and tablet.
- [ ] Global footer and cursor are unchanged (deferred to later specsheet).

