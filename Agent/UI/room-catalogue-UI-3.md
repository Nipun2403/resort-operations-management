# Specsheet: Room Catalogue – Paginated Fixed Grid with Gold Wash (Final)

## 1. Purpose
- Restyle the public **Room Catalogue** page (`/rooms`) to match the editorial asymmetric grid from the design while keeping content dynamic.
- Display exactly **4 room cards at a time** in fixed grid positions. The grid itself does not scroll; instead, the user navigates between groups of 4 rooms using horizontal swipe (mobile) / mouse wheel (desktop) or arrow buttons.
- A smooth **gold‑wash animation** sweeps across the cards when the data changes.
- The page layout respects the original design: hero header, asymmetric villa grid with a glass‑morphic overlay on the first card, and a newsletter section.
- All existing component logic (API calls, navigation, session storage) remains untouched.

## 2. Files to Modify
| File | Action |
|------|--------|
| `src/app/features/public/pages/room-catalogue.component.html` | Replace template with new design. |
| `src/app/features/public/pages/room-catalogue.component.scss` | Replace styles with new design. |
| `src/app/features/public/pages/room-catalogue.component.ts` | Add pagination, transition, wheel/touch event handlers; keep all existing code. |

**No changes** to services, models, guards, or routing.

## 3. Component Logic – Additions to TypeScript (do not remove existing code)

Keep all existing members (`rooms`, `loading`, `error`, `fetchRooms`, `getFirstImage`, `viewRoom`, `emailControl`, `subscribed`, `subscribe()`). Add the following inside the class body:

```typescript
// Pagination
currentGroupIndex = signal(0);
roomsPerGroup = 4;
totalGroups = computed(() => Math.ceil(this.rooms().length / this.roomsPerGroup));
displayedRooms = computed(() => {
  const start = this.currentGroupIndex() * this.roomsPerGroup;
  return this.rooms().slice(start, start + this.roomsPerGroup);
});

// Transition state for gold wash animation
isTransitioning = signal(false);
private readonly ANIMATION_DURATION = 600; // ms

// Navigation methods
nextGroup(): void {
  if (this.currentGroupIndex() < this.totalGroups() - 1 && !this.isTransitioning()) {
    this.triggerTransition(() => this.currentGroupIndex.update(i => i + 1));
  }
}
previousGroup(): void {
  if (this.currentGroupIndex() > 0 && !this.isTransitioning()) {
    this.triggerTransition(() => this.currentGroupIndex.update(i => i - 1));
  }
}
private triggerTransition(updateFn: () => void): void {
  this.isTransitioning.set(true);
  setTimeout(() => {
    updateFn();
    setTimeout(() => this.isTransitioning.set(false), this.ANIMATION_DURATION);
  }, 100);
}

// Touch & wheel detection
private touchStartX = 0;
onTouchStart(event: TouchEvent): void {
  this.touchStartX = event.changedTouches[0].screenX;
}
onTouchEnd(event: TouchEvent): void {
  const deltaX = event.changedTouches[0].screenX - this.touchStartX;
  if (deltaX < -50) this.nextGroup();
  else if (deltaX > 50) this.previousGroup();
}
onWheel(event: WheelEvent): void {
  if (Math.abs(event.deltaX) > Math.abs(event.deltaY) && Math.abs(event.deltaX) > 30) {
    event.preventDefault();
    if (event.deltaX > 0) this.nextGroup();
    else this.previousGroup();
  }
}

// Get CSS classes for each card position (0‑3)
getCardClass(index: number): string {
  const classes = ['card-large', 'card-small', 'card-medium', 'card-wide'];
  return classes[index] || '';
}
```

## 4. Template (`room-catalogue.component.html`)

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

  <!-- Room Cards – Fixed Grid with Pagination -->
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
      <div
        class="grid-container"
        [class.transitioning]="isTransitioning()"
        (wheel)="onWheel($event)"
        (touchstart)="onTouchStart($event)"
        (touchend)="onTouchEnd($event)"
        tabindex="0"
      >
        <!-- Navigation arrows (desktop) -->
        <button class="nav-arrow left" (click)="previousGroup()" [disabled]="currentGroupIndex() === 0">
          <span class="material-symbols-outlined">arrow_back_ios</span>
        </button>
        <button class="nav-arrow right" (click)="nextGroup()" [disabled]="currentGroupIndex() === totalGroups() - 1">
          <span class="material-symbols-outlined">arrow_forward_ios</span>
        </button>

        <!-- Asymmetric Grid (12 columns) -->
        <div class="villa-grid">
          @for (room of displayedRooms(); track room.id; let i = $index) {
            <article class="room-card {{ getCardClass(i) }}">
              <!-- Image area -->
              <div class="card-image" [style.background-image]="'url(' + getFirstImage(room) + ')'">
                <div class="image-overlay"></div>
                <div class="card-number">
                  {{ ((currentGroupIndex() * roomsPerGroup) + i + 1).toString().padStart(2, '0') }} / VILLA
                </div>
                @if (i === 0) {
                  <!-- Glass overlay for large card only -->
                  <div class="glass-overlay">
                    <span class="overlay-label">{{ ((currentGroupIndex() * roomsPerGroup) + 1).toString().padStart(2, '0') }} / VILLA</span>
                    <h2 class="overlay-title">{{ room.name }}</h2>
                    <div class="overlay-stats">
                      <div>
                        <span class="stat-label">GUESTS</span>
                        <span class="stat-value">{{ room.maxOccupancy }}</span>
                      </div>
                      <div>
                        <span class="stat-label">SQ METERS</span>
                        <span class="stat-value">{{ room.squareFootage || '—' }}</span>
                      </div>
                    </div>
                  </div>
                }
              </div>
              <!-- Card body for cards 2-4 (card 1 info is in overlay) -->
              @if (i !== 0) {
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
              }
            </article>
          }
        </div>
      </div>

      <!-- Group indicator -->
      <div class="group-indicator">
        {{ currentGroupIndex() + 1 }} / {{ totalGroups() }}
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

## 5. SCSS (`room-catalogue.component.scss`)

```scss
@import '../../../../styles/theme/index';

.catalogue-page {
  overflow-x: hidden;
  padding-top: 10rem; // space from fixed navbar (design's pt-40)
}

// ── Hero Header ──────────────────────────────────
.hero-header {
  max-width: var(--container-max);
  margin: 0 auto 8rem; // design's mb-32
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin-bottom: 4rem;
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
  p { @include font-body-lg; color: var(--color-error); }
  .retry-btn {
    @include font-label-caps;
    background: transparent;
    border: 1px solid var(--color-secondary);
    color: var(--color-secondary);
    padding: 0.5rem 1.5rem;
    cursor: pointer;
    &:hover { background: var(--color-secondary); color: var(--color-on-secondary); }
  }
}

// ── Grid Section ─────────────────────────────────
.rooms-section {
  width: 100%;
  margin-bottom: var(--section-gap);
}

.grid-container {
  position: relative;
  padding: 0 var(--margin-desktop);
  max-width: var(--container-max);
  margin: 0 auto;
  outline: none; // for focus to receive wheel events
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
  }
}

// Asymmetric Villa Grid (12‑column)
.villa-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: 2rem; // 32px as design
}

// Base card
.room-card {
  cursor: pointer;
  transition: transform 0.4s ease;
  &:hover {
    transform: translateY(-4px);
    .card-image { transform: scale(1.03); }
  }
}

// Card sizes & offsets (exactly mirror design)
.room-card.card-large {
  grid-column: span 8;
  margin-bottom: 8rem; // design's mb-32
  .card-image { aspect-ratio: 16 / 9; }
  @media (max-width: 768px) {
    grid-column: span 12;
    margin-bottom: 2rem;
  }
}
.room-card.card-small {
  grid-column: span 4;
  align-self: center;    // vertically centered in the row
  margin-bottom: 8rem;
  .card-image { aspect-ratio: 3 / 4; }
  @media (max-width: 768px) {
    grid-column: span 12;
    margin-bottom: 2rem;
    align-self: auto;
  }
}
.room-card.card-medium {
  grid-column: span 5;
  margin-bottom: 8rem;
  .card-image { aspect-ratio: 4 / 5; }
  @media (max-width: 768px) {
    grid-column: span 12;
    margin-bottom: 2rem;
  }
}
.room-card.card-wide {
  grid-column: span 7;
  margin-top: 8rem;      // design's lg:mt-32
  margin-bottom: 8rem;
  .card-image { aspect-ratio: 16 / 10; }
  @media (max-width: 768px) {
    grid-column: span 12;
    margin-top: 0;
    margin-bottom: 2rem;
  }
}

// Image area
.card-image {
  background-size: cover;
  background-position: center;
  transition: transform 0.6s cubic-bezier(0.2, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
  background-color: var(--color-surface-container-low); // fallback
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

// Glass overlay on large card only
.glass-overlay {
  position: absolute;
  bottom: 0;
  left: 0;
  max-width: 28rem;
  padding: 3rem;
  background: rgba(26, 26, 26, 0.7);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border-top: 1px solid rgba(228, 194, 133, 0.2);
  color: var(--color-on-surface);
  .overlay-label {
    @include font-label-caps;
    font-size: 0.625rem;
    letter-spacing: 0.3em;
    color: var(--color-secondary);
    display: block;
    margin-bottom: 1rem;
  }
  .overlay-title {
    @include font-headline-md;
    margin-bottom: 1.5rem;
    color: var(--color-on-surface);
  }
  .overlay-stats {
    display: flex;
    gap: 2rem;
    border-top: 1px solid rgba(228, 226, 221, 0.1);
    padding-top: 1.5rem;
    .stat-label {
      @include font-label-caps;
      font-size: 0.625rem;
      color: rgba(228, 226, 221, 0.4);
      display: block;
    }
    .stat-value {
      @include font-body-md;
      color: var(--color-on-surface);
    }
  }
}

// Card body for cards 2‑4
.card-body {
  padding: 1.5rem 0 0;
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

// ── Gold Wash Animation ──────────────────────────
.grid-container.transitioning .card-image::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(
    105deg,
    transparent 40%,
    rgba(228, 194, 133, 0.3),
    transparent 60%
  );
  animation: goldSweep 0.6s ease forwards;
}

@keyframes goldSweep {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

// ── Navigation Arrows ────────────────────────────
.nav-arrow {
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
  &:hover {
    border-color: var(--color-secondary);
    color: var(--color-secondary);
  }
  &:disabled {
    opacity: 0;
    pointer-events: none;
  }
  &.left { left: 0; }
  &.right { right: 0; }
}
.grid-container:hover .nav-arrow {
  opacity: 1;
}
@media (max-width: 768px) {
  .nav-arrow { display: none; }
}

// Group indicator
.group-indicator {
  text-align: center;
  @include font-label-caps;
  color: var(--color-outline);
  margin-top: 2rem;
  letter-spacing: 0.3em;
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
  &::placeholder { color: rgba(228, 226, 221, 0.2); }
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

## 6. Responsive Behaviour Summary
- **Desktop (> 768px):** asymmetric grid with offset margins, navigation arrows on hover, wheel‑based pagination.
- **Mobile/Tablet (≤ 768px):** all cards stack vertically with equal margins, navigation arrows hidden, touch swipe and wheel still active.
- The glass overlay remains on card 1 regardless of viewport.
- The gold wash animation works across all devices.

## 7. Integration Notes
- The existing `fetchRooms()` method is called in `ngOnInit` (unchanged). It fetches all active room types.
- The `getFirstImage()` helper is unchanged.
- The `subscribe()` method and newsletter functionality are unchanged.
- The star icon from the original design is intentionally omitted.

## 8. Self‑Review Checklist
- [ ] Hero header renders with padding from navbar.
- [ ] Grid shows exactly 4 cards at a time in the exact positions from the design.
- [ ] Card 1 has a glass overlay with room name and stats; cards 2‑4 have a card body with “VIEW DETAILS”.
- [ ] Navigation arrows appear on desktop hover and cycle groups.
- [ ] Horizontal swipe on mobile and mouse wheel on desktop navigate between groups.
- [ ] Gold wash animation sweeps across images during transition.
- [ ] Group indicator shows current page.
- [ ] Newsletter section works with success message.
- [ ] All existing API calls and session storage work unaffected.
- [ ] No console errors; responsive layout works on mobile and tablet.

