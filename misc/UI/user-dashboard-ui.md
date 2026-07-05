# Specsheet: Customer Dashboard – Design Refactor (Final Deterministic)

## 1. Purpose
- Restyle the **Customer Dashboard** page (`/user/dashboard`) to match the “Obsidian & Champagne” design system.
- **All existing functionality is preserved** – welcome message, current/upcoming booking cards, quick request buttons, room service status section, pending booking redirect, and all API calls.
- The only changes are to the dashboard’s HTML template and SCSS file. TypeScript stays untouched unless one of the two required helpers (`upcomingRoomTypes`, `getBookingRoomNumbers`) is missing; in that case only those minimal additions are made – existing business logic is never altered.
- The `RequestServiceDialogComponent` (opened by the quick‑request buttons) is **not modified**; its form validations and error cues remain intact.

## 2. Files to Modify
| File | Change |
|------|--------|
| `src/app/features/user/pages/dashboard.component.html` | Replace the dashboard template with the markup below while preserving all existing bindings, router links, event handlers, signals, and Angular control flow. If an existing binding already provides equivalent functionality, reuse it rather than introducing a new one. |
| `src/app/features/user/pages/dashboard.component.scss` | Replace styles with new glass‑panel aesthetic, using existing theme mixins. |
| `src/app/features/user/pages/dashboard.component.ts` | **No changes expected.** If the component lacks `upcomingRoomTypes` or `getBookingRoomNumbers`, add only those minimal helpers. Reuse any existing equivalent. Do not alter any other methods, signals, or form controls. |

**No changes** to services, guards, routing, or any other components.

## 3. Before Making Changes (Agent Instruction)
- Read the existing `dashboard.component.ts` and understand all signals, computed values, and methods.
- Reuse existing signals and helpers wherever possible.
- Do not duplicate any existing logic.
- Preserve all routing and dialog behaviour exactly as found.
- Keep the diff minimal; only add what is strictly necessary for the new design.

## 4. Template (`dashboard.component.html`)

```html
<div class="customer-dashboard">
  <!-- Greeting Section -->
  <section class="greeting">
    <h2 class="greeting-text">
      Welcome back, {{ firstName() || 'Guest' }}
    </h2>
    <div class="divider"></div>
  </section>

  @if (loading() && !currentBooking() && !upcomingBooking()) {
    <div class="loading-spinner">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="loadDashboard()">Retry</button>
    </app-alert>
  } @else {
    <!-- Portfolio of Stays -->
    <section class="stays-section">
      <h3 class="section-label">Portfolio of Stays</h3>
      <div class="stays-grid">
        <!-- Current Stay Card -->
        <div class="stay-card glass-panel current-stay">
          @if (currentBooking()) {
            <div class="card-header">
              <h4 class="room-title">{{ getBookingRoomNumbers(currentBooking()!) || 'Your Room' }}</h4>
              <span class="status-badge">Checked In</span>
            </div>
            <div class="dates">
              <div class="date-item">
                <span class="material-symbols-outlined icon">login</span>
                <span>{{ currentBooking()!.checkInDate }}</span>
              </div>
              <div class="date-item">
                <span class="material-symbols-outlined icon">logout</span>
                <span>{{ currentBooking()!.checkOutDate }}</span>
              </div>
            </div>
            <div class="actions">
              <button class="ghost-button" (click)="openServiceRequest('housekeeping')">
                Request Housekeeping
              </button>
              <button class="ghost-button" (click)="openServiceRequest('maintenance')">
                Request Maintenance
              </button>
            </div>
          } @else {
            <p class="empty-message">No active stay right now.</p>
          }
        </div>

        <!-- Upcoming Stay Card -->
        <div class="stay-card glass-panel upcoming-stay">
          @if (upcomingBooking()) {
            <div class="card-header">
              <h4 class="room-title">Future Residence</h4>
              <span class="material-symbols-outlined arrow-icon">arrow_forward_ios</span>
            </div>
            <div class="dates">
              <div class="date-item">
                <span class="material-symbols-outlined icon">calendar_today</span>
                <span>{{ upcomingBooking()!.checkInDate }} — {{ upcomingBooking()!.checkOutDate }}</span>
              </div>
            </div>
            <div class="tags">
              @for (type of upcomingRoomTypes(); track type) {
                <span class="tag">{{ type }}</span>
              }
            </div>
            <a routerLink="/user/bookings" class="view-link underline-reveal">View Details</a>
          } @else {
            <p class="empty-message">No upcoming bookings.</p>
          }
        </div>
      </div>
    </section>

    <!-- Pulse of Service Section -->
    <section class="service-section">
      <h3 class="section-label">Pulse of Service</h3>
      <div class="service-grid">
        <!-- Housekeeping -->
        <div class="service-card glass-panel">
          <div class="service-header">
            <span class="material-symbols-outlined service-icon">cleaning_services</span>
            <h4 class="service-title">Housekeeping</h4>
          </div>
          <div class="service-items">
            @for (item of pendingHousekeeping(); track item.id) {
              <div class="service-item">
                <span class="item-description">{{ item.description || 'Request #' + item.id }}</span>
                <span class="item-status" [class]="item.status">
                  @if (item.status !== 'Completed') {
                    <span class="pulse-dot"></span>
                  }
                  {{ item.status }}
                </span>
              </div>
            }
            @empty {
              <p class="empty-message">No pending requests.</p>
            }
          </div>
          <a routerLink="/user/room-service" class="service-link underline-reveal">Manage Requests</a>
        </div>

        <!-- Maintenance -->
        <div class="service-card glass-panel">
          <div class="service-header">
            <span class="material-symbols-outlined service-icon">build</span>
            <h4 class="service-title">Maintenance</h4>
          </div>
          <div class="service-items">
            @for (item of pendingMaintenance(); track item.id) {
              <div class="service-item">
                <span class="item-description">{{ item.description || 'Request #' + item.id }}</span>
                <span class="item-status" [class]="item.status">
                  @if (item.status !== 'Completed') {
                    <span class="pulse-dot"></span>
                  }
                  {{ item.status }}
                </span>
              </div>
            }
            @empty {
              <p class="empty-message">No pending requests.</p>
            }
          </div>
          <a routerLink="/user/room-service" class="service-link underline-reveal">Report Issue</a>
        </div>

        <!-- Food Orders -->
        <div class="service-card glass-panel">
          <div class="service-header">
            <span class="material-symbols-outlined service-icon">restaurant</span>
            <h4 class="service-title">Dining</h4>
          </div>
          <div class="service-items">
            @for (order of pendingFoodOrders(); track order.id) {
              <div class="service-item">
                <span class="item-description">Order #{{ order.id }}</span>
                <span class="item-status" [class]="order.status">
                  @if (order.status !== 'Delivered') {
                    <span class="pulse-dot"></span>
                  }
                  {{ order.status }}
                </span>
              </div>
            }
            @empty {
              <p class="empty-message">No pending orders.</p>
            }
          </div>
          <a routerLink="/user/room-service" class="service-link underline-reveal">New Order</a>
        </div>
      </div>
    </section>
  }
</div>
```

**Angular control flow:** This template uses Angular’s built‑in `@if`, `@for`, `@else`, `@empty` syntax. Do not mix with `*ngIf`/`*ngFor` unless explicitly required. The pulse indicator inside `@for` uses `@if` for simplicity.

## 5. SCSS (`dashboard.component.scss`)

```scss
@import '../../../../styles/theme/index';

.customer-dashboard {
  padding: 2rem;
  @media (max-width: 768px) {
    padding: 1.5rem 1rem;
  }
}

// ── Greeting ──────────────────────────────────────
.greeting {
  margin-bottom: 2rem;
  .greeting-text {
    @include font-display-lg;
    font-size: clamp(2rem, 6vw, 3.5rem);
    font-style: italic;
    color: var(--color-on-surface);
    font-weight: 300;
    margin-bottom: 1rem;
  }
  .divider {
    width: 6rem;
    height: 1px;
    background: rgba(228, 194, 133, 0.4);
    margin-top: 0.5rem;
  }
}

.section-label {
  @include font-label-caps;
  color: var(--color-on-tertiary-container);
  letter-spacing: 0.2em;
  margin-bottom: 1.5rem;
  text-transform: uppercase;
}

// ── Glass Panel (use theme mixin) ────────────────
.glass-panel {
  @include glass-panel;
  padding: 2rem;
  transition: transform 0.4s ease;
  &:hover { transform: translateY(-4px); }
}

// ── Stays Grid ────────────────────────────────────
.stays-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

.stay-card {
  min-height: 300px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;

  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 1rem;
  }
  .room-title {
    @include font-headline-md;
    color: var(--color-on-surface);
  }
  .status-badge {
    padding: 0.25rem 0.75rem;
    background: rgba(228, 194, 133, 0.1);
    border: 1px solid rgba(228, 194, 133, 0.3);
    color: var(--color-secondary);
    @include font-label-caps;
    font-size: 0.625rem;
  }
  .dates {
    margin-bottom: 1.5rem;
    .date-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 0.5rem;
      @include font-body-md;
      color: var(--color-on-surface-variant);
      .icon {
        color: rgba(228, 194, 133, 0.7);
        font-size: 1.1rem;
      }
    }
  }
  .actions {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    margin-top: auto;
  }
  .empty-message {
    @include font-body-md;
    color: var(--color-on-surface-variant);
    text-align: center;
    margin: auto;
  }
}

.ghost-button {
  border: 1px solid rgba(228, 194, 133, 0.4);
  color: var(--color-on-surface);
  background: transparent;
  @include font-label-caps;
  padding: 0.75rem 1.5rem;
  cursor: pointer;
  transition: border-color 0.4s, color 0.4s, background 0.4s;
  &:hover {
    border-color: var(--color-secondary);
    color: var(--color-secondary);
    background: rgba(228, 194, 133, 0.05);
  }
}

.upcoming-stay {
  opacity: 0.7;
  &:hover { opacity: 1; }
  .tags {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    margin: 1rem 0;
    .tag {
      padding: 0.25rem 0.5rem;
      background: rgba(228, 194, 133, 0.05);
      border: 1px solid rgba(228, 194, 133, 0.2);
      color: var(--color-secondary);
      @include font-label-caps;
      font-size: 0.625rem;
      cursor: default;
    }
  }
}

.view-link, .service-link {
  @include font-label-caps;
  color: var(--color-secondary);
  text-decoration: none;
  margin-top: 1rem;
  display: inline-block;
}

// ── Underline Reveal (use theme mixin) ────────────
.underline-reveal {
  @include underline-reveal;
}

// ── Pulse of Service ─────────────────────────────
.service-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
  @media (max-width: 1024px) {
    grid-template-columns: 1fr 1fr;
  }
  @media (max-width: 600px) {
    grid-template-columns: 1fr;
  }
}

.service-card {
  .service-header {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 1.5rem;
    .service-icon {
      font-size: 2rem;
      color: var(--color-secondary);
    }
    .service-title {
      @include font-headline-sm;
      color: var(--color-on-surface);
    }
  }
  .service-items {
    margin-bottom: 1.5rem;
    .service-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem 0;
      border-bottom: 1px solid rgba(228, 194, 133, 0.1);
      &:last-child { border-bottom: none; }
      .item-description {
        @include font-body-md;
        color: var(--color-on-surface-variant);
      }
      .item-status {
        @include font-label-caps;
        font-size: 0.625rem;
        letter-spacing: 0.1em;
        display: flex;
        align-items: center;
        gap: 0.5rem;
        &.Pending { color: var(--color-secondary); }
        &.InProgress, &.Preparing { color: var(--color-primary-fixed); }
        &.Completed, &.Delivered { color: var(--color-on-surface-variant); opacity: 0.5; }
      }
      .pulse-dot {
        width: 0.375rem;
        height: 0.375rem;
        border-radius: 50%;
        background: var(--color-secondary);
        animation: softPulse 2s infinite ease-in-out;
      }
    }
    .empty-message {
      @include font-body-md;
      color: var(--color-on-surface-variant);
    }
  }
}

@keyframes softPulse {
  0%, 100% { opacity: 0.4; transform: scale(1); }
  50% { opacity: 1; transform: scale(1.3); }
}

// Loading
.loading-spinner {
  display: flex;
  justify-content: center;
  padding: 4rem 0;
}
```

**Theme usage:** The mixins `glass-panel` and `underline-reveal` are already defined in the global theme (`_glassmorphism.scss` and `_mixins.scss`). Do **not** redefine them; use `@include` to pull them in.

## 6. TypeScript (Only If Helpers Are Missing)

The component is expected to already provide:
- `firstName = signal<string>('')`
- `currentBooking`, `upcomingBooking`, `pendingHousekeeping`, `pendingMaintenance`, `pendingFoodOrders`
- `loading`, `error`, `loadDashboard()`, `openServiceRequest(type)`, `fetchBookings()` etc.

**If** the component does **not** expose `upcomingRoomTypes` or a method to get room numbers, add them minimally:

```typescript
// Add only if not already present
upcomingRoomTypes = computed(() => {
  const booking = this.upcomingBooking();
  if (!booking?.rooms) return [];
  return booking.rooms
    .map(r => r.roomTypeName ?? `Room Type ${r.roomTypeId}`)
    .filter(Boolean);
});

getBookingRoomNumbers(booking: Booking): string {
  return booking.rooms?.filter(r => r.roomNumber).map(r => r.roomNumber).join(', ') || '';
}
```

Do **not** modify any other part of the component class.

## 7. Integration Notes
- The `glass-panel` and `underline-reveal` classes **must** use the existing theme mixins to stay consistent across the app.
- The pulse dot is only added for non‑completed items. Completed/Delivered items show no dot and have reduced opacity.
- All existing router links and click handlers are preserved as‑is.
- The mobile bottom navigation bar from the design is **not** implemented; the existing shell burger menu remains the sole navigation.

## 8. Shared Components Impact
- **`RequestServiceDialogComponent`** – unchanged; inherits global dark dialog styles.
- **`AlertComponent`** – already dark‑themed.
- No other shared components used.

## 9. Cross‑Role Consistency
- This dashboard is unique to the customer portal. No other role shares its layout.

## 10. Self‑Review Checklist
- [ ] Welcome message uses first name from JWT (no “Mr.”).
- [ ] Current stay card shows room number(s) and dates; action buttons trigger existing dialog.
- [ ] Upcoming stay card shows room type names as non‑clickable tags.
- [ ] Service status cards display real data from signals, with soft pulse on active items.
- [ ] Links navigate to the correct routes.
- [ ] Glass panels and underline reveals use theme mixins.
- [ ] All form validations and error messages remain unchanged.
- [ ] No console errors; all existing logic intact.