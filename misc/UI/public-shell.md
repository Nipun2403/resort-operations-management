# Specsheet: Public Shell & Home Page Refactor (Exhaustive, Deterministic)

## 1. Purpose
- Replace the existing `PublicShellComponent` and `HomeComponent` with the merged “Obsidian & Champagne” design.
- The public shell now includes a sticky glass‑morphic navbar (desktop with underline‑reveal links, mobile drawer) and a minimal footer.
- The home page integrates the hero (with booking bar), ethos section, featured rooms grid, heritage section, and cinematic gallery break.
- All existing functionality (API calls, session storage, navigation) is preserved; only HTML and SCSS change.

## 2. Files to Modify

| File | Action |
|------|--------|
| `src/app/features/public/public-shell.component.ts` | Replace template, styles, and add drawer/signal logic. |
| `src/app/features/public/public-shell.component.html` | New navbar, footer, and router‑outlet wrapper. |
| `src/app/features/public/public-shell.component.scss` | New styles using theme tokens and mixins. |
| `src/app/features/public/pages/home.component.ts` | Keep existing logic; adjust if needed for new markup. |
| `src/app/features/public/pages/home.component.html` | New sections: hero, ethos, sanctuaries, heritage, gallery. |
| `src/app/features/public/pages/home.component.scss` | New styles matching design. |

No changes to services, guards, or routing.

## 3. PublicShellComponent – Exact Implementation

### 3.1 TypeScript (`public-shell.component.ts`)
```typescript
import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-public-shell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './public-shell.component.html',
  styleUrls: ['./public-shell.component.scss']
})
export class PublicShellComponent {
  private breakpointObserver = inject(BreakpointObserver);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 768px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );
  drawerOpen = signal(false);

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 50);
  }
  isScrolled = signal(false);

  closeDrawer() {
    this.drawerOpen.set(false);
  }
}
```

### 3.2 Template (`public-shell.component.html`)
```html
<header class="main-nav" [class.scrolled]="isScrolled()">
  <nav class="nav-container">
    <div class="logo" routerLink="/home">AETHERIS</div>
    <div class="desktop-links">
      <a routerLink="/home" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}" class="nav-link underline-reveal">The Estate</a>
      <a routerLink="/rooms" routerLinkActive="active" class="nav-link underline-reveal">Villas</a>
      <a routerLink="/experiences" routerLinkActive="active" class="nav-link underline-reveal">Dining &amp; Amenities</a>
      <a routerLink="/availability" routerLinkActive="active" class="nav-link underline-reveal">Reservations</a>
    </div>
    <div class="nav-actions">
      <a class="inquire-btn" routerLink="/auth">Login</a>
      <button class="menu-btn" (click)="drawerOpen.set(true)" aria-label="Menu">
        <span class="material-symbols-outlined">menu</span>
      </button>
    </div>
  </nav>
</header>

<!-- Mobile Drawer -->
@if (drawerOpen()) {
  <div class="drawer-overlay" (click)="closeDrawer()"></div>
  <aside class="mobile-drawer" [class.open]="drawerOpen()">
    <div class="drawer-header">
      <span class="logo">AETHERIS</span>
      <button class="close-btn" (click)="closeDrawer()" aria-label="Close menu">
        <span class="material-symbols-outlined">close</span>
      </button>
    </div>
    <nav>
      <a routerLink="/home" (click)="closeDrawer()">
        <span class="material-symbols-outlined">explore</span>
        The Estate
      </a>
      <a routerLink="/rooms" (click)="closeDrawer()">
        <span class="material-symbols-outlined">villa</span>
        Villas
      </a>
      <a routerLink="/experiences" (click)="closeDrawer()">
        <span class="material-symbols-outlined">spa</span>
        Dining &amp; Amenities
      </a>
      <a routerLink="/availability" (click)="closeDrawer()">
        <span class="material-symbols-outlined">calendar_month</span>
        Reservations
      </a>
    </nav>
  </aside>
}

<main>
  <router-outlet></router-outlet>
</main>

<footer class="site-footer">
  <div class="footer-logo">AETHERIS</div>
  <div class="footer-info">
    <span>1 AETHERIS PEAK, THE SILENT VALLEY</span>
    <span class="separator"></span>
    <span>&copy; 2024 AETHERIS. ALL RIGHTS RESERVED.</span>
  </div>
</footer>
```

### 3.3 SCSS (`public-shell.component.scss`)
```scss
@import '../../../../styles/theme/index';

.main-nav {
  position: fixed;
  top: 0;
  width: 100%;
  z-index: 50;
  transition: background 0.5s, backdrop-filter 0.5s, border 0.5s;
  &.scrolled {
    background: rgba(10, 10, 10, 0.8);
    backdrop-filter: blur(24px);
    border-bottom: 1px solid var(--glass-border);
  }
  .nav-container {
    display: flex;
    justify-content: space-between;
    align-items: center;
    max-width: var(--container-max);
    margin: 0 auto;
    padding: 1.5rem var(--margin-desktop);
    @media (max-width: 768px) {
      padding: 1rem var(--margin-mobile);
    }
  }
  .logo {
    font-family: var(--font-headline);
    font-size: 1.5rem;
    letter-spacing: 0.3em;
    color: var(--color-on-surface);
    text-transform: uppercase;
    cursor: pointer;
    user-select: none;
  }
  .desktop-links {
    display: none;
    gap: 2.5rem;
    @media (min-width: 768px) {
      display: flex;
    }
    .nav-link {
      @include font-label-caps;
      color: var(--color-on-surface);
      text-decoration: none;
      padding-bottom: 4px;
      transition: color 0.3s;
      &:hover,
      &.active {
        color: var(--color-secondary);
      }
    }
  }
  .nav-actions {
    display: flex;
    align-items: center;
    gap: 1.5rem;
    .inquire-btn {
      @include font-label-caps;
      color: var(--color-secondary);
      text-decoration: none;
      transition: opacity 0.2s;
      &:hover { opacity: 0.8; }
    }
    .menu-btn {
      background: none;
      border: none;
      color: var(--color-on-surface);
      cursor: pointer;
      @media (min-width: 768px) { display: none; }
      .material-symbols-outlined { font-size: 1.5rem; }
    }
  }
}

// Underline reveal for desktop links
.underline-reveal {
  position: relative;
  &::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 1px;
    background: var(--color-secondary);
    transform: scaleX(0);
    transform-origin: right;
    transition: transform 0.6s cubic-bezier(0.19, 1, 0.22, 1);
  }
  &:hover::after,
  &.active::after {
    transform: scaleX(1);
    transform-origin: left;
  }
}

// Drawer
.drawer-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 55;
}
.mobile-drawer {
  position: fixed;
  top: 0;
  right: 0;
  width: min(400px, 80vw);
  height: 100%;
  @include glass-panel;
  z-index: 60;
  transform: translateX(100%);
  transition: transform 0.5s cubic-bezier(0.16, 1, 0.3, 1);
  &.open {
    transform: translateX(0);
  }
  padding: 2rem;
  display: flex;
  flex-direction: column;
  .drawer-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    .logo {
      font-family: var(--font-headline);
      font-size: 1.5rem;
      letter-spacing: 0.3em;
      color: var(--color-on-surface);
    }
    .close-btn {
      background: none;
      border: none;
      color: var(--color-on-surface);
      cursor: pointer;
      .material-symbols-outlined { font-size: 1.75rem; }
    }
  }
  nav {
    display: flex;
    flex-direction: column;
    margin-top: 3rem;
    gap: 2rem;
    a {
      @include font-label-caps;
      color: var(--color-on-surface-variant);
      text-decoration: none;
      display: flex;
      align-items: center;
      gap: 1rem;
      transition: color 0.3s;
      .material-symbols-outlined {
        font-variation-settings: 'FILL' 1;
        font-size: 1.25rem;
      }
      &:hover {
        color: var(--color-primary);
      }
    }
  }
}

// Footer
.site-footer {
  background: var(--color-surface-container-lowest);
  padding: 10rem 1rem;
  text-align: center;
  border-top: 1px solid var(--glass-border);
  .footer-logo {
    font-family: var(--font-headline);
    font-size: clamp(3rem, 10vw, 7.5rem);
    letter-spacing: 0.3em;
    color: var(--color-on-surface);
    margin-bottom: 2rem;
    text-transform: uppercase;
  }
  .footer-info {
    font-family: var(--font-body);
    font-size: 0.625rem;
    font-weight: 500;
    letter-spacing: 0.3em;
    text-transform: uppercase;
    color: rgba(228, 226, 221, 0.4);
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 1.5rem;
    .separator {
      display: inline-block;
      width: 4px;
      height: 4px;
      border-radius: 50%;
      background: rgba(228, 226, 221, 0.2);
    }
  }
}
```

## 4. HomeComponent – Exact Implementation

### 4.1 TypeScript (`home.component.ts`)
No changes required; the existing component already has:
- `featuredRooms`, `roomsLoading`, `roomsError` signals
- `checkIn`, `checkOut`, `guests` form controls
- `fetchFeaturedRooms()`, `getFirstImage()`, `viewRoom()`, `searchAvailability()`
Keep them exactly as is.

### 4.2 Template (`home.component.html`)
```html
<div class="home-page">
  <!-- Hero Section -->
  <section class="hero">
    <div class="hero-bg" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuAdLKWcaXoRzaSRLAXTT-aFheM_lrhFYW9u9Abt2sRFujWP3KddhG_Akj0P6IOduWEFFu-mDxD2Zp4dpIQBKJtdunZueQMAVvfq9JiEW3oI_NRiQcsqp14_Yh34YNUG5zo_pmyET_pQ4TIqdDKreqjZJa4v5OyKZXsRy8UZG8tcCCZrzpNVJ2GWRAukLp7Bu4xKZEVxiYwUN9kLUHOjj9c5NNzy82Sd_xoFjXKt_4nUnCutSLvPb56bP4-KBDObY9xJRnkzvLBu-yXX')"></div>
    <div class="hero-overlay"></div>
    <div class="hero-content">
      <h1>The Silent Peak of Luxury</h1>
      <p class="hero-subtitle">PRIVATE ESTATE &amp; REFUGE</p>
    </div>

    <!-- Booking Bar -->
    <div class="booking-bar glass-panel">
      <div class="booking-field">
        <label class="field-label">ARRIVAL</label>
        <input type="text" class="field-input" [formControl]="checkIn" placeholder="Select date" />
      </div>
      <div class="booking-field">
        <label class="field-label">DEPARTURE</label>
        <input type="text" class="field-input" [formControl]="checkOut" placeholder="Select date" />
      </div>
      <div class="booking-field">
        <label class="field-label">GUESTS</label>
        <input type="number" class="field-input" [formControl]="guests" min="1" max="20" />
      </div>
      <button class="booking-btn" (click)="searchAvailability()">RESERVE SANCTUARY</button>
    </div>
  </section>

  <!-- Ethos Section -->
  <section class="ethos">
    <div class="section-label">01. PHILOSOPHY</div>
    <h2 class="ethos-headline">The Ethos of <em>Aetheris</em></h2>
    <div class="ethos-grid">
      <div class="ethos-image">
        <img src="https://lh3.googleusercontent.com/aida-public/AB6AXuDJzoJgDT9zi_2aiEYAWO2EU0rvMI06PbHoowjw2aCipSYPYUWxOu7tAyZxY_9Jv_JIJtjSgIULOm3g5IKbvQvTiCwZkG3rqQFhbQFdQNRLguMXGIwr_xqUVtzi6P8YaSYbx1ZmgKQi94JqYqpZEKJeOMLA8P3r1ZqdRL9Rj1Sxlb5of5ik9gjJ8T4a8YllXDMXv8utaUSz-pPexBO49GhAk1ul4D5Q8oTQSOtw3RektEY-DrDA5Urt0WEVV-4qjr3Yp_-5qldmQPSO" alt="Alabaster sculpture" />
      </div>
      <div class="ethos-text">
        <p>At Aetheris, luxury is defined by what is absent. Noise, intrusion, and the mundane are replaced by a profound stillness. We provide a sanctuary where time is the ultimate currency and discretion is our highest law.</p>
        <a routerLink="/rooms" class="cta-link">
          The Vision <span class="line"></span>
        </a>
      </div>
    </div>
  </section>

  <!-- Private Sanctuaries Section -->
  <section class="sanctuaries">
    <div class="section-header">
      <div class="section-label">02. ACCOMMODATIONS</div>
      <div class="count-label">{{ featuredRooms().length }} ESTATES</div>
    </div>
    <h2 class="section-title">Private Sanctuaries</h2>

    @if (roomsLoading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (roomsError()) {
      <p class="error">{{ roomsError() }}</p>
    } @else {
      <div class="rooms-grid">
        @for (room of featuredRooms(); track room.id; let i = $index) {
          <div class="room-card" [class.large]="i === 0">
            <div class="card-image" [style.background-image]="'url(' + getFirstImage(room) + ')'" (click)="viewRoom(room.id)"></div>
            <div class="card-info">
              <h3>{{ room.name }}</h3>
              <div class="meta">
                <span>Max. {{ room.maxOccupancy }} Guests &bull; {{ room.squareFootage || '&mdash;' }} sqm</span>
                <span class="price">From {{ room.basePrice | currency }}/Night</span>
              </div>
            </div>
          </div>
        }
      </div>
      <a routerLink="/rooms" class="view-all">
        VIEW ALL ACCOMMODATIONS
        <span class="material-symbols-outlined">arrow_forward</span>
      </a>
    }
  </section>

  <!-- Heritage Section (Legacy of Discretion) -->
  <section class="heritage">
    <div class="heritage-header">
      <div class="section-label">Our Heritage</div>
      <h2>A Legacy of <br/>Discretion</h2>
      <p class="heritage-subtitle">Est. 1924. Serving the world’s most discerning figures with unwavering privacy.</p>
    </div>
    <div class="heritage-grid">
      <!-- Item 1 -->
      <div class="heritage-item">
        <div class="heritage-img" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuDPXJ1BklsW9n0PL5s9KUzjVQwn9bKCYfPqz7sKmIfH8846GMhTlUSG9oJzS4rslkF2ikJHqTEMliQVEXH1oS-KcX24_nYrfRFt-x7J0Dds0DoFT_YSy2m7Rw-nhIiMpP5887yaRrlmvkJ94dzPMsi1p6XH-8zGil4aqyzXigxyQJWh0uzWCRVzpBRpzWs0K7esx8SgEBJNwKq87CjDYvsecF1XtAwlWQSJpYp1iJunayndSL1JpC8blW2MTl3XmRYynMnLtwqEtZpF')"></div>
        <h3>Unseen Service</h3>
        <p>Our staff is trained in the art of invisibility, ensuring your needs are met before you even realize they exist.</p>
      </div>
      <!-- Item 2 -->
      <div class="heritage-item">
        <div class="heritage-img" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuBIsM6GzTOdAofZq4-fCpVsVFJVgTsT1v1lVExmDQsRM6SDJXn3KyseX0n3GyBzvjuyS2QFkhF_S32eyt1kxm2tOyyIU6wKpK_yMSyG9EBWoRguoXAGLWdK0u7nlSemLoc69-vNEvgYVz2X27HMzDsLWjDWm8b5EG8GlrJ-ZYOsG5d7fC3U4FK09-FahO70xKPQBIiP11-H_MqSIPfdOdF_oqa8NvVV_E9n4oOuHpAJifApZ43dYdM-9KttRVtl6GkgV0bLelzlqKOh')"></div>
        <h3>Private Corridors</h3>
        <p>Architectural layouts designed specifically to prevent crossing paths, maintaining absolute isolation for all guests.</p>
      </div>
      <!-- Item 3 -->
      <div class="heritage-item">
        <div class="heritage-img" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuDww5Dr5sbq3-Eu7dCIXq947SV0_TR1vtRdYxMIWuCNWrYmlJX3rAboz5drtoWRCZ5RvCfCfDcnx4LybMGp_0YnykwrU9ik65pYIbHVQBUtAiZT5zIyBisXkm8zqKDwn3nOnfRjijlOnF5H5Ubyv6iGH0LtQNcuquBkQUaDYX8t2UUaR7Pa_L1CJMQBOE2ErUXnpiLsD8rUQgZu4xi3p-2x1Zvih-4VLs8hmFMKCAuZX1WZyffDdmNdr-nJqF_g4ssaGTBzDwnfeasx')"></div>
        <h3>Trusted Custody</h3>
        <p>We safeguard not just your physical self, but your history, your privacy, and your future legacy.</p>
      </div>
    </div>
  </section>

  <!-- Cinematic Gallery Break -->
  <section class="gallery-break" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuC0jdUrR_d5kCrH_hA-VlFicyjfehGem9pHb4Z8526ztNY5GROodFNYf1W2cTR2sEdqm1B1-OoKOc4pLQ_W316--SNsc4uL-EVvj1aaY_DTZdmVko9XrpxWuo7dP3VRZCayR9NCVYvdxXVpfhUqIOPUBwORFton4M1685vgc5ZeNHVpCL1XmKeBQEyNkTSL1vRauQMjzrLNJndBAVRppOBKHKZ7HjYdc1OQ7_cu09zyzI5rVE4-e-rPPPpOW3QR2H0CCFpKtjOx0HRI')">
    <div class="gallery-overlay">
      <div class="section-label">VILLA NO. IV</div>
      <h2>The Celestial Pavilion</h2>
      <a routerLink="/rooms" class="gallery-cta">VIEW RESIDENCE</a>
    </div>
  </section>
</div>
```

### 4.3 SCSS (`home.component.scss`)
```scss
@import '../../../../styles/theme/index';

.home-page {
  overflow-x: hidden;
}

// Hero
.hero {
  position: relative;
  height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  .hero-bg {
    position: absolute;
    inset: 0;
    background-size: cover;
    background-position: center;
    transform: scale(1.05);
    animation: kenburns 20s infinite alternate;
  }
  .hero-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(to top, var(--color-background) 0%, transparent 50%, rgba(0,0,0,0.4) 100%);
  }
  .hero-content {
    position: relative;
    z-index: 10;
    text-align: center;
    padding: 0 var(--margin-mobile);
    h1 {
      @include font-display-lg;
      font-size: clamp(2.5rem, 10vw, 7.5rem);
      color: var(--color-on-surface);
      margin-bottom: 1rem;
    }
    .hero-subtitle {
      @include font-label-caps;
      color: var(--color-secondary);
      letter-spacing: 0.5em;
    }
  }
  .booking-bar {
    position: absolute;
    bottom: 3rem;
    left: 50%;
    transform: translateX(-50%);
    width: calc(100% - 2 * var(--margin-mobile));
    max-width: 1000px;
    @include glass-panel;
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    align-items: end;
    gap: 2rem;
    padding: 2rem;
    @media (max-width: 768px) {
      grid-template-columns: 1fr;
      gap: 1rem;
      width: calc(100% - 2 * var(--margin-mobile));
      bottom: 2rem;
    }
    .booking-field {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      .field-label {
        @include font-label-caps;
        color: var(--color-outline);
        font-size: 0.75rem;
      }
      .field-input {
        background: transparent;
        border: none;
        border-bottom: 1px solid rgba(228, 194, 133, 0.4);
        color: var(--color-on-surface);
        padding: 0.5rem 0;
        font-family: var(--font-body);
        font-size: 1rem;
        outline: none;
        &:focus { border-color: var(--color-secondary); }
      }
    }
    .booking-btn {
      @include font-label-caps;
      background: transparent;
      border: 1px solid var(--color-secondary);
      color: var(--color-secondary);
      padding: 0.75rem 1.5rem;
      cursor: pointer;
      transition: background 0.5s, color 0.5s;
      &:hover {
        background: var(--color-secondary);
        color: var(--color-on-secondary);
      }
    }
  }
}

// Ethos
.ethos {
  max-width: var(--container-max);
  margin: var(--section-gap) auto;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin: 4rem auto;
  }
  .section-label {
    @include font-label-caps;
    color: var(--color-secondary);
    margin-bottom: 1.5rem;
  }
  .ethos-headline {
    @include font-headline-md;
    font-size: clamp(2rem, 6vw, 4.5rem);
    max-width: 800px;
    margin-bottom: 3rem;
    em { font-style: italic; color: var(--color-secondary); }
  }
  .ethos-grid {
    display: grid;
    grid-template-columns: 7fr 5fr;
    gap: 2rem;
    @media (max-width: 768px) {
      grid-template-columns: 1fr;
    }
  }
  .ethos-image {
    overflow: hidden;
    img {
      width: 100%;
      height: 600px;
      object-fit: cover;
      transition: transform 1s;
      @media (max-width: 768px) { height: 400px; }
      &:hover { transform: scale(1.1); }
    }
  }
  .ethos-text {
    display: flex;
    flex-direction: column;
    justify-content: center;
    padding-left: 2rem;
    @media (max-width: 768px) { padding-left: 0; }
    p {
      @include font-body-lg;
      color: rgba(228, 226, 221, 0.8);
      margin-bottom: 2rem;
    }
    .cta-link {
      @include font-label-caps;
      color: var(--color-secondary);
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
      .line {
        width: 3rem;
        height: 1px;
        background: var(--color-secondary);
        transition: width 0.5s;
      }
      &:hover .line { width: 6rem; }
    }
  }
}

// Sanctuaries
.sanctuaries {
  max-width: var(--container-max);
  margin: var(--section-gap) auto;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin: 4rem auto;
  }
  .section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 1px solid rgba(228, 194, 133, 0.2);
    padding-bottom: 1rem;
    margin-bottom: 2rem;
    .section-label { @include font-label-caps; color: var(--color-secondary); }
    .count-label { @include font-label-caps; color: var(--color-outline); }
  }
  .section-title {
    @include font-display-lg-mobile;
    font-style: italic;
    margin-bottom: 3rem;
  }
  .rooms-grid {
    display: grid;
    grid-template-columns: 6fr 4fr;
    gap: 2rem;
    @media (max-width: 768px) {
      grid-template-columns: 1fr;
    }
    .room-card {
      &:first-child {
        grid-row: span 2;
      }
      .card-image {
        aspect-ratio: 4/5;
        background-size: cover;
        background-position: center;
        cursor: pointer;
        transition: transform 1.2s cubic-bezier(0.2, 0, 0.2, 1);
        &:hover { transform: scale(1.03); }
      }
      .card-info {
        margin-top: 1rem;
        h3 { @include font-headline-sm; }
        .meta {
          display: flex;
          justify-content: space-between;
          font-size: 0.9rem;
          color: var(--color-outline-variant);
          margin-top: 0.5rem;
          .price { color: var(--color-secondary); }
        }
      }
      &.large .card-image { aspect-ratio: 4/5; }
    }
  }
  .view-all {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    @include font-label-caps;
    color: var(--color-on-surface);
    text-decoration: none;
    margin-top: 3rem;
    padding-bottom: 4px;
    border-bottom: 1px solid rgba(228, 194, 133, 0.5);
    transition: color 0.3s, border-color 0.3s;
    &:hover { color: var(--color-secondary); border-color: var(--color-secondary); }
  }
}

// Heritage
.heritage {
  background: var(--color-surface-container-lowest);
  padding: var(--section-gap) var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 4rem var(--margin-mobile);
  }
  .heritage-header {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    border-bottom: 1px solid rgba(228, 194, 133, 0.1);
    padding-bottom: 2rem;
    margin-bottom: 3rem;
    @media (min-width: 768px) {
      flex-direction: row;
      justify-content: space-between;
      align-items: flex-end;
    }
    .section-label { @include font-label-caps; color: var(--color-secondary); margin-bottom: 1rem; }
    h2 { @include font-headline-md; font-size: clamp(2rem, 5vw, 3.5rem); }
    .heritage-subtitle {
      @include font-body-md;
      color: var(--color-on-tertiary-container);
      max-width: 300px;
      text-align: right;
    }
  }
  .heritage-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 2rem;
    @media (max-width: 768px) { grid-template-columns: 1fr; }
    .heritage-item {
      &:nth-child(2) { margin-top: 6rem; @media (max-width: 768px) { margin-top: 0; } }
      .heritage-img {
        height: 320px;
        background-size: cover;
        background-position: center;
        margin-bottom: 1.5rem;
        transition: transform 0.7s;
        &:hover { transform: scale(1.05); }
      }
      h3 { @include font-headline-sm; margin-bottom: 0.5rem; }
      p { @include font-body-md; color: rgba(228, 226, 221, 0.6); }
    }
  }
}

// Gallery Break
.gallery-break {
  height: 100vh;
  background-size: cover;
  background-position: center;
  background-attachment: fixed;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  position: relative;
  .gallery-overlay {
    background: rgba(0, 0, 0, 0.4);
    padding: 3rem;
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    .section-label { @include font-label-caps; color: var(--color-secondary); margin-bottom: 1rem; }
    h2 { @include font-display-lg; font-size: clamp(2.5rem, 7vw, 5rem); }
    .gallery-cta {
      @include font-label-caps;
      font-size: 0.625rem;
      letter-spacing: 0.4em;
      color: var(--color-on-surface);
      border-bottom: 1px solid var(--color-on-surface);
      padding-bottom: 0.5rem;
      text-decoration: none;
      margin-top: 2rem;
      transition: color 0.3s, border-color 0.3s;
      &:hover { color: var(--color-secondary); border-color: var(--color-secondary); }
    }
  }
}

// Ken Burns animation
@keyframes kenburns {
  from { transform: scale(1.05); }
  to { transform: scale(1.1); }
}
```

## 5. Responsive Notes
- The breakpoint for mobile/desktop is 768px (`max-width: 768px` is used for mobile).
- All grid layouts collapse to single column on mobile.
- Font sizes scale using `clamp()` for responsive headlines.
- The booking bar switches from 4 columns to 1 column on mobile.

## 6. Self‑Review Checklist
- [ ] Navbar: desktop links with underline‑reveal, sticky on scroll, mobile drawer with icons.
- [ ] Hero: Design 2 image and text; Design 1 glassmorphic booking bar at bottom.
- [ ] Booking bar: inputs bound to existing form controls; "RESERVE SANCTUARY" calls `searchAvailability()`.
- [ ] Ethos section: image + text in bento grid; CTA links to `/rooms`.
- [ ] Featured rooms: dynamic data from API; first card larger; count label shows actual number.
- [ ] Heritage: three items with exact images and text; stagger effect on desktop.
- [ ] Gallery break: fixed background image with overlay and link to `/rooms`.
- [ ] Footer: minimal with large logo and fine‑print info.
- [ ] All existing logic (API calls, session storage) functions unchanged.
- [ ] No console errors; responsive layout works on mobile and tablet.

