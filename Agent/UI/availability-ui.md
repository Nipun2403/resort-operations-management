# Specsheet: Availability Page – Design Refactor

## 1. Purpose
- Restyle the public **Availability** page (`/availability`) to match the editorial luxury design while keeping all existing booking‑flow logic untouched.
- The page comprises a refined search form (check‑in, check‑out, guests) and horizontally split result cards with Ken Burns hover effect, slide‑in “Book Now” button, and empty‑state message.
- All functional behaviour – API calls, session‑storage handling, navigation for authenticated/unauthenticated users, and pre‑filling from query parameters – remains **exactly as implemented** in the original availability component.

## 2. Files to Modify
| File | Action |
|------|--------|
| `src/app/features/public/pages/availability.component.html` | Replace template with new design. |
| `src/app/features/public/pages/availability.component.scss` | Replace styles with new design. |

**No changes** to `availability.component.ts`, services, guards, or routing. The existing signals, form controls, and methods are used as‑is.

## 3. Template (`availability.component.html`) – Exact Markup

```html
<div class="availability-page">
  <!-- Search Form Section -->
  <section class="search-section">
    <h1 class="search-title">Curate Your Stay</h1>
    <form class="search-form" (ngSubmit)="searchAvailability()">
      <div class="form-field">
        <label for="checkin" class="field-label">Check‑in</label>
        <input
          id="checkin"
          type="date"
          class="field-input"
          [formControl]="checkIn"
        />
      </div>
      <div class="form-separator"></div>
      <div class="form-field">
        <label for="checkout" class="field-label">Check‑out</label>
        <input
          id="checkout"
          type="date"
          class="field-input"
          [formControl]="checkOut"
        />
      </div>
      <div class="form-separator"></div>
      <div class="form-field">
        <label for="guests" class="field-label">Guests</label>
        <select
          id="guests"
          class="field-input"
          [formControl]="guests"
        >
          <option value="1">1 Guest</option>
          <option value="2">2 Guests</option>
          <option value="3">3 Guests</option>
          <option value="4">4+ Guests</option>
        </select>
      </div>
      <div class="form-separator"></div>
      <button type="submit" class="search-btn arrow-link">
        <span>Search</span>
        <span class="material-symbols-outlined arrow-icon">arrow_right_alt</span>
      </button>
    </form>
  </section>

  <!-- Results Section -->
  <section class="results-section">
    @if (searchLoading()) {
      <div class="loading-spinner">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (searchError()) {
      <p class="error-message">{{ searchError() }}</p>
    } @else if (hasSearched() && availableRooms().length === 0) {
      <!-- Empty State -->
      <div class="empty-state">
        <h2>Awaiting your next visit</h2>
        <p>No residences match these precise dates. We invite you to explore alternative timing or consult our concierge for unlisted availability.</p>
      </div>
    } @else {
      @for (room of availableRooms(); track room.roomTypeId) {
        <article class="result-card group">
          <div class="card-image hover-pan">
            <img [src]="getFirstImage(room)" alt="{{ room.name }}" />
            <div class="image-overlay"></div>
          </div>
          <div class="card-info">
            <div class="info-top">
              <div class="availability-dots">
                <span class="dot filled"></span>
                <span class="dot filled" [class.empty]="room.availableCount < 2"></span>
                <span class="dot" [class.empty]="room.availableCount < 3"></span>
                <span class="availability-label">Limited</span>
              </div>
              <h2 class="room-name">{{ room.name }}</h2>
              <p class="room-description">{{ room.description || 'A private sanctuary of silence and space.' }}</p>
            </div>
            <div class="info-bottom">
              <div class="price">
                <span class="price-label">Per Night</span>
                <span class="price-value">{{ room.basePrice | currency }}</span>
              </div>
              <button class="book-btn slide-in-btn" (click)="bookNow(room)">Book Now</button>
            </div>
          </div>
        </article>
      }
    }
  </section>
</div>
```

**Note:** The `[formControl]` bindings on native date inputs will work because Angular’s `FormControl` can sync with a native `<input type="date">` – it expects the value as a string (ISO). Our existing `checkIn` and `checkOut` are `FormControl<Date | null>`. We need to adjust the bindings to work with native date inputs. To preserve the existing logic, we will keep the same `FormControl` instances but add a simple conversion method or use `[value]` with `(input)` instead of `[formControl]`. However, we can create a small helper or change the template to use `[value]="checkIn.value | date:'yyyy-MM-dd'" (ngModelChange)="...`. But we must not change the component code. To keep the logic intact, we will use `[formControl]` and Angular will handle the conversion for date inputs if we use `FormControl<string>`. Our existing controls are `FormControl<Date | null>`. That will cause issues because a date input emits a string.

Simpler: we can keep the existing text inputs from the original design but style them as date inputs? Actually the original availability page used `mat-datepicker`. That's fine, we can keep `mat-datepicker` but style it to look like the design. However the design shows native date inputs. To avoid touching TypeScript, we'll use the same `mat-datepicker` elements but apply custom styles to match the design (borderless, gold‑colored calendar icon). The form fields will look like the design, but the date pickers will still be Material. This preserves functionality and requires no logic changes.

We'll adjust the template accordingly: use `mat-form-field` with `appearance="outline"` but style it minimally. Or we can use the native inputs with a small adapter. Since we're allowed to add small TypeScript members for newsletter, we can add a couple of lines to convert. But the instruction says "no changes to services, guards, or routing" – small template‑only changes are allowed, but we should avoid modifying the component class for this. To stay safe, we'll continue using the existing `mat-datepicker` approach but apply CSS to make them look like the design (transparent background, borderless, gold calendar icon). That's purely CSS and template.

Let's use the existing `mat-datepicker` but with a custom class.

We'll write the template using the existing datepicker and a select for guests (which we already have). We'll keep the existing form controls binding as before (`[formControl]="checkIn"` etc.). The functionality is unchanged.

Now, the guest control was a `FormControl<number>`, but the design uses a `<select>` with string values. We can keep the number input but style it as a select? Or we can change the control to accept numbers from a select? Actually our existing `guests` control is `FormControl<number>` with min/max validators. A `<select>` can bind to it with `[value]="guests.value" (change)="guests.setValue($event.target.value)"`. To avoid adding new methods, we can use `[formControl]="guests"` with a `<select>` – Angular will convert the string value to a number if the control expects a number. That works because `FormControl` coerces. However, the option values must be numbers. So we can do:

```html
<select [formControl]="guests">
  <option [ngValue]="1">1 Guest</option>
  <option [ngValue]="2">2 Guests</option>
  ...
</select>
```

`[ngValue]` will keep the type. Good.

Thus we can replace the `mat-form-field` input with a styled `<select>`.

Now, `mat-datepicker` can be hidden with CSS and we can show the native date input? No, we'll keep the datepicker as is but apply styles to match. We'll wrap in a container and style the underline.

We'll produce the template using the existing date picker but with a minimal look:

```html
<mat-form-field class="date-field" appearance="fill" floatLabel="always">
  <mat-label>Check‑in</mat-label>
  <input matInput [matDatepicker]="picker1" [formControl]="checkIn" />
  <mat-datepicker-toggle matSuffix [for]="picker1"></mat-datepicker-toggle>
  <mat-datepicker #picker1></mat-datepicker>
</mat-form-field>
```

And override styles to remove the standard Material background and borders.

That ensures zero logic changes.

Now we'll incorporate all that into the final specsheet.# Specsheet: Availability Page – Design Refactor

## 1. Purpose
- Restyle the public **Availability** page (`/availability`) to match the editorial luxury design while keeping **all** existing booking‑flow logic untouched.
- The page comprises a refined search form and horizontally split result cards with Ken Burns hover effect, slide‑in “Book Now” button, availability dots, and an elegant empty state.
- All functional behaviour – API calls, session‑storage handling, navigation for authenticated/unauthenticated users, and pre‑filling from query parameters – remains exactly as in the original `AvailabilityComponent`.

## 2. Files to Modify
| File | Action |
|------|--------|
| `src/app/features/public/pages/availability.component.html` | Replace template with new design. |
| `src/app/features/public/pages/availability.component.scss` | Replace styles with new design. |

**No changes** to `availability.component.ts`, services, guards, or routing. The component’s existing signals (`availableRooms`, `searchLoading`, `searchError`, `hasSearched`, `preSelectedRoomTypeId`), form controls (`checkIn`, `checkOut`, `guests`), and methods (`searchAvailability`, `getFirstImage`, `bookNow`, `extractErrorMessage`) are used as‑is.

## 3. Template (`availability.component.html`) – Exact Markup

```html
<div class="availability-page">
  <!-- Search Form Section -->
  <section class="search-section">
    <h1 class="search-title">Curate Your Stay</h1>
    <form class="search-form" (ngSubmit)="searchAvailability()">
      <div class="form-field">
        <label class="field-label">Check‑in</label>
        <mat-form-field class="date-field" floatLabel="never">
          <input matInput [matDatepicker]="picker1" [formControl]="checkIn" />
          <mat-datepicker-toggle matSuffix [for]="picker1"></mat-datepicker-toggle>
          <mat-datepicker #picker1></mat-datepicker>
        </mat-form-field>
      </div>
      <div class="form-separator"></div>
      <div class="form-field">
        <label class="field-label">Check‑out</label>
        <mat-form-field class="date-field" floatLabel="never">
          <input matInput [matDatepicker]="picker2" [formControl]="checkOut" />
          <mat-datepicker-toggle matSuffix [for]="picker2"></mat-datepicker-toggle>
          <mat-datepicker #picker2></mat-datepicker>
        </mat-form-field>
      </div>
      <div class="form-separator"></div>
      <div class="form-field">
        <label class="field-label">Guests</label>
        <select class="guest-select" [formControl]="guests">
          <option [ngValue]="1">1 Guest</option>
          <option [ngValue]="2">2 Guests</option>
          <option [ngValue]="3">3 Guests</option>
          <option [ngValue]="4">4+ Guests</option>
        </select>
      </div>
      <div class="form-separator"></div>
      <button type="submit" class="search-btn arrow-link" [disabled]="checkIn.invalid || checkOut.invalid || guests.invalid || searchLoading()">
        <span>Search</span>
        <span class="material-symbols-outlined arrow-icon">arrow_right_alt</span>
      </button>
    </form>
  </section>

  <!-- Results Section -->
  <section class="results-section">
    @if (searchLoading()) {
      <div class="loading-spinner">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (searchError()) {
      <p class="error-message">{{ searchError() }}</p>
    } @else if (hasSearched() && availableRooms().length === 0) {
      <!-- Empty State -->
      <div class="empty-state">
        <h2>Awaiting your next visit</h2>
        <p>No residences match these precise dates. We invite you to explore alternative timing or consult our concierge for unlisted availability.</p>
      </div>
    } @else {
      @for (room of availableRooms(); track room.roomTypeId) {
        <article class="result-card group">
          <div class="card-image hover-pan">
            <img [src]="getFirstImage(room)" alt="{{ room.name }}" />
            <div class="image-overlay"></div>
          </div>
          <div class="card-info">
            <div class="info-top">
              <div class="availability-dots">
                <span class="dot filled"></span>
                <span class="dot" [class.filled]="room.availableCount >= 2"></span>
                <span class="dot" [class.filled]="room.availableCount >= 3"></span>
                <span class="availability-label">
                  {{ room.availableCount <= 1 ? 'Last Chance' : (room.availableCount <= 2 ? 'Limited' : 'Available') }}
                </span>
              </div>
              <h2 class="room-name">{{ room.name }}</h2>
              <p class="room-description">{{ room.description || 'A private sanctuary of silence and space.' }}</p>
            </div>
            <div class="info-bottom">
              <div class="price">
                <span class="price-label">Per Night</span>
                <span class="price-value">{{ room.basePrice | currency }}</span>
              </div>
              <button class="book-btn slide-in-btn" (click)="bookNow(room)">Book Now</button>
            </div>
          </div>
        </article>
      }
    }
  </section>
</div>
```

## 4. SCSS (`availability.component.scss`)

```scss
@import '../../../../styles/theme/index';

.availability-page {
  padding-top: 6rem;
  overflow-x: hidden;
}

// ── Search Section ───────────────────────────────
.search-section {
  padding: 4rem var(--margin-mobile);
  border-bottom: 1px solid rgba(228, 194, 133, 0.05);
  background: var(--color-surface-container-lowest);
  @media (min-width: 768px) {
    padding: 6rem var(--margin-desktop);
  }
}

.search-title {
  @include font-headline-md;
  font-size: clamp(2rem, 5vw, 2.5rem);
  color: var(--color-secondary);
  text-align: center;
  font-style: italic;
  margin-bottom: 3rem;
}

.search-form {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: 2rem;
  max-width: 1000px;
  margin: 0 auto;
  @media (min-width: 1024px) {
    flex-direction: row;
    align-items: center;
    gap: 1rem;
  }
}

.form-field {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.field-label {
  @include font-label-caps;
  font-size: 0.625rem;
  color: rgba(228, 194, 133, 0.5);
  margin-bottom: 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.2em;
}

// Override Material date field to be borderless
.date-field {
  ::ng-deep .mat-mdc-form-field-focus-overlay { display: none; }
  ::ng-deep .mat-mdc-text-field-wrapper {
    background: transparent !important;
    padding: 0;
  }
  ::ng-deep .mat-mdc-form-field-infix {
    padding: 0;
  }
  ::ng-deep .mdc-line-ripple { display: none; }
  ::ng-deep .mat-mdc-form-field-subscript-wrapper { display: none; }
  ::ng-deep input {
    color: var(--color-on-surface) !important;
    @include font-body-lg;
    cursor: pointer;
  }
  ::ng-deep .mat-datepicker-toggle .mat-mdc-icon-button svg {
    fill: var(--color-secondary);
  }
}

.guest-select {
  background: transparent;
  border: none;
  color: var(--color-on-surface);
  @include font-body-lg;
  padding: 0;
  outline: none;
  cursor: pointer;
  appearance: none;
  // custom arrow via background? We'll leave for simplicity.
  option {
    background: var(--color-surface-container);
    color: var(--color-on-surface);
  }
}

.form-separator {
  width: 1px;
  height: 40px;
  background: linear-gradient(to bottom, transparent, var(--color-secondary), transparent);
  opacity: 0.3;
  display: none;
  @media (min-width: 1024px) {
    display: block;
    flex-shrink: 0;
  }
}

.search-btn {
  background: transparent;
  border: none;
  color: var(--color-secondary);
  @include font-label-caps;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  padding: 0;
  transition: opacity 0.3s;
  &:disabled { opacity: 0.3; cursor: not-allowed; }
  .arrow-icon {
    transition: transform 0.4s cubic-bezier(0.23, 1, 0.32, 1);
  }
  &:hover:not(:disabled) .arrow-icon {
    transform: translateX(8px) scaleX(1.2);
  }
}

// ── Results Section ──────────────────────────────
.results-section {
  max-width: var(--container-max);
  margin: 0 auto;
  padding: var(--section-gap) var(--margin-mobile);
  @media (min-width: 768px) {
    padding: var(--section-gap) var(--margin-desktop);
  }
  display: flex;
  flex-direction: column;
  gap: 3rem;
}

.loading-spinner, .error-message {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 4rem 0;
}
.error-message { @include font-body-lg; color: var(--color-error); }

// Empty State
.empty-state {
  text-align: center;
  padding: 8rem 1rem;
  h2 {
    @include font-display-lg-mobile;
    font-size: clamp(2rem, 6vw, 3rem);
    color: var(--color-on-surface);
    font-style: italic;
    margin-bottom: 1rem;
    position: relative;
    display: inline-block;
    &::after {
      content: '';
      position: absolute;
      left: 0;
      bottom: -8px;
      width: 100%;
      height: 1px;
      background: var(--color-secondary);
      animation: underlineGrow 2s ease forwards;
    }
  }
  p {
    @include font-body-lg;
    color: rgba(228, 226, 221, 0.5);
    max-width: 500px;
    margin: 1.5rem auto 0;
  }
}

@keyframes underlineGrow {
  from { transform: scaleX(0); transform-origin: left; }
  to { transform: scaleX(1); }
}

// Result Card
.result-card {
  display: flex;
  flex-direction: column;
  border: 1px solid rgba(228, 194, 133, 0.1);
  background: var(--color-surface-container-low);
  cursor: pointer;
  overflow: hidden;
  transition: border-color 0.3s;
  &:hover { border-color: rgba(228, 194, 133, 0.3); }
  @media (min-width: 768px) {
    flex-direction: row;
    height: 580px;
  }
}

.card-image {
  position: relative;
  width: 100%;
  height: 350px;
  overflow: hidden;
  @media (min-width: 768px) {
    width: 60%;
    height: 100%;
  }
  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 3s cubic-bezier(0.4, 0, 0.2, 1);
  }
  .hover-pan:hover img {
    animation: kenBurns 15s ease-in-out infinite alternate;
  }
  .image-overlay {
    position: absolute;
    inset: 0;
    background: rgba(0, 0, 0, 0.2);
  }
}

@keyframes kenBurns {
  0% { transform: scale(1) translate(0, 0); }
  100% { transform: scale(1.1) translate(-2%, -2%); }
}

.card-info {
  flex: 1;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  background: var(--color-surface-container);
  @media (min-width: 768px) {
    padding: 3rem;
  }
}

.availability-dots {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-bottom: 1.5rem;
  .dot {
    width: 0.5rem;
    height: 0.5rem;
    border: 1px solid rgba(228, 194, 133, 0.3);
    &.filled { background: var(--color-secondary); border-color: var(--color-secondary); }
  }
  .availability-label {
    @include font-label-caps;
    font-size: 0.625rem;
    color: rgba(228, 194, 133, 0.6);
    margin-left: 1rem;
  }
}

.room-name {
  @include font-headline-md;
  font-size: clamp(1.8rem, 4vw, 2.5rem);
  color: var(--color-on-surface);
  margin-bottom: 1rem;
  line-height: 1.2;
}

.room-description {
  @include font-body-md;
  color: rgba(228, 226, 221, 0.6);
  max-width: 400px;
}

.info-bottom {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  border-top: 1px solid rgba(228, 194, 133, 0.1);
  padding-top: 2rem;
  margin-top: 2rem;
}

.price-label {
  display: block;
  @include font-label-caps;
  font-size: 0.625rem;
  color: rgba(228, 194, 133, 0.4);
  margin-bottom: 0.5rem;
}
.price-value {
  @include font-headline-sm;
  font-size: 1.75rem;
  color: var(--color-secondary);
  font-weight: 300;
}

.book-btn {
  background: var(--color-secondary);
  color: var(--color-background);
  border: none;
  @include font-label-caps;
  padding: 0.75rem 2rem;
  cursor: pointer;
  transition: background 0.3s;
  &:hover { background: rgba(228, 194, 133, 0.9); }
}

.slide-in-btn {
  transform: translateX(40px);
  opacity: 0;
  transition: all 0.6s cubic-bezier(0.23, 1, 0.32, 1);
}
.group:hover .slide-in-btn {
  transform: translateX(0);
  opacity: 1;
}

// Responsive – book button always visible on mobile
@media (max-width: 768px) {
  .slide-in-btn {
    transform: none;
    opacity: 1;
  }
}
```

## 5. Responsive Behaviour
- Search form stacks vertically on mobile; on desktop, fields are inline with gold separators.
- Result cards stack vertically on mobile; the slide‑in button remains always visible (no hidden effect on small screens).
- The date pickers retain full functionality; styles override Material defaults to match the design.
- The guest selector uses a native `<select>` styled with the design’s typography.

## 6. Integration Notes
- All existing signals, form controls, and methods are untouched. The `searchAvailability()` method is triggered on form submit; it uses `checkIn.value`, `checkOut.value`, `guests.value` as before.
- `getFirstImage(room)` is unchanged; it returns the first image URL or a placeholder.
- `bookNow(room)` contains the existing logic for authenticated vs. unauthenticated users and session storage. No modifications needed.
- The empty state condition (`hasSearched() && availableRooms().length === 0`) is unchanged.
- The global footer and header are provided by the public shell.

## 7. Self‑Review Checklist
- [ ] Search form looks borderless with gold accents; date pickers and guest select work correctly.
- [ ] Clicking Search triggers the same API call and updates results.
- [ ] Result cards display room image, name, description, price, and availability dots.
- [ ] “Book Now” button calls the existing `bookNow()` logic (session storage / navigation).
- [ ] Slide‑in animation works on desktop; button is always visible on mobile.
- [ ] Empty state shows the message.
- [ ] Loading and error states display correctly.
- [ ] Responsive layout works on mobile and tablet.
- [ ] No console errors; all existing functionality intact.

