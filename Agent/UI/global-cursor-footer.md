# Specsheet: Global Footer & Custom Cursor

## 1. Purpose
- **Global Footer** – update the shared footer in the `PublicShellComponent` (used by all public pages) and `UserShellComponent` (customer portal) to include placeholder links for “Privacy Policy”, “Terms of Service”, “Press”, “Careers”, and “Contact”. No social icons. The design matches the “Obsidian & Champagne” aesthetic.
- **Custom Cursor** – implement a luxury gold‑circle cursor that appears globally on every page (public, user, and admin/staff). The cursor enlarges over buttons/actions, becomes a vertical oval over text inputs, and reverts to a circle otherwise. On touch devices, the cursor is hidden.
- Both changes are purely cosmetic and do not alter any business logic, API calls, or navigation.

## 2. Files to Modify / Create

| File | Action |
|------|--------|
| `src/app/features/public/public-shell.component.html` | Update footer section with link row. |
| `src/app/features/public/public-shell.component.scss` | Adjust footer styles. |
| `src/app/features/user/user-shell.component.html` | Update footer section with link row (if not already present). |
| `src/app/features/user/user-shell.component.scss` | Adjust footer styles. |
| **New:** `src/app/shared/components/custom-cursor/custom-cursor.component.ts` | Standalone cursor component with logic. |
| **New:** `src/app/shared/components/custom-cursor/custom-cursor.component.html` | Template (empty – just a `<div>`). |
| **New:** `src/app/shared/components/custom-cursor/custom-cursor.component.scss` | Cursor styles. |
| `src/app/app.component.html` | Add `<app-custom-cursor></app-custom-cursor>` so it’s present on every route. |

## 3. Updated Footer – Public Shell

### 3.1 Template (`public-shell.component.html`)
Replace the current `<footer>` section with:

```html
<footer class="site-footer">
  <div class="footer-links">
    <a href="#">Privacy Policy</a>
    <a href="#">Terms of Service</a>
    <a href="#">Press</a>
    <a href="#">Careers</a>
    <a href="#">Contact</a>
  </div>
  <div class="footer-logo">AETHERIS</div>
  <div class="footer-info">
    <span>1 AETHERIS PEAK, THE SILENT VALLEY</span>
    <span class="separator"></span>
    <span>&copy; 2024 AETHERIS. ALL RIGHTS RESERVED.</span>
  </div>
</footer>
```

### 3.2 SCSS (`public-shell.component.scss`)
Append or adjust the footer styles:

```scss
.site-footer {
  background: var(--color-surface-container-lowest);
  padding: 6rem 1rem 3rem;
  text-align: center;
  border-top: 1px solid var(--glass-border);

  .footer-links {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 2rem;
    margin-bottom: 3rem;
    a {
      @include font-body-md;
      color: var(--color-on-tertiary-container);
      text-decoration: none;
      transition: color 0.3s;
      &:hover { color: var(--color-secondary); }
    }
    @media (max-width: 768px) {
      gap: 1.2rem;
      a { font-size: 0.85rem; }
    }
  }

  .footer-logo {
    font-family: var(--font-headline);
    font-size: clamp(3rem, 10vw, 7.5rem);
    letter-spacing: 0.3em;
    color: var(--color-on-surface);
    margin-bottom: 1.5rem;
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

## 4. Updated Footer – User Shell (Customer Portal)

Repeat exactly the same HTML and SCSS changes in `UserShellComponent`, using the identical class names and structure. The footer appears only for the customer portal, matching the public aesthetic.

## 5. Custom Cursor Component

### 5.1 Component (`custom-cursor.component.ts`)
```typescript
import { Component, HostListener, ElementRef, Renderer2, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { DOCUMENT } from '@angular/common';

@Component({
  selector: 'app-custom-cursor',
  standalone: true,
  imports: [],
  template: `<div class="custom-cursor" #cursor></div>`,
  styleUrls: ['./custom-cursor.component.scss'],
})
export class CustomCursorComponent {
  private cursorEl = inject(ElementRef).nativeElement.querySelector('.custom-cursor') as HTMLElement;
  private renderer = inject(Renderer2);
  private document = inject(DOCUMENT);
  private platformId = inject(PLATFORM_ID);

  private readonly INTERACTIVE_SELECTOR = 'a, button, .cursor-hover, mat-slide-toggle, mat-icon-button, [role="button"]';
  private readonly INPUT_SELECTOR = 'input, textarea, select, mat-select, .mat-mdc-input-element';

  private rafId: number | null = null;
  private mouseX = 0;
  private mouseY = 0;

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.document.addEventListener('mousemove', this.onMouseMove);
    this.document.addEventListener('mouseover', this.onMouseOver);
    this.document.addEventListener('mouseout', this.onMouseOut);
    this.updatePosition();
  }

  ngOnDestroy(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.document.removeEventListener('mousemove', this.onMouseMove);
    this.document.removeEventListener('mouseover', this.onMouseOver);
    this.document.removeEventListener('mouseout', this.onMouseOut);
    if (this.rafId) cancelAnimationFrame(this.rafId);
  }

  private onMouseMove = (e: MouseEvent): void => {
    this.mouseX = e.clientX;
    this.mouseY = e.clientY;
  };

  private onMouseOver = (e: MouseEvent): void => {
    const target = e.target as HTMLElement;
    if (!target) return;
    if (target.matches(this.INTERACTIVE_SELECTOR) || target.closest(this.INTERACTIVE_SELECTOR)) {
      this.renderer.addClass(this.cursorEl, 'enlarged');
    } else if (target.matches(this.INPUT_SELECTOR) || target.closest(this.INPUT_SELECTOR)) {
      this.renderer.addClass(this.cursorEl, 'oval');
    }
  };

  private onMouseOut = (e: MouseEvent): void => {
    const target = e.target as HTMLElement;
    if (!target) return;
    if (target.matches(this.INTERACTIVE_SELECTOR) || target.closest(this.INTERACTIVE_SELECTOR)) {
      this.renderer.removeClass(this.cursorEl, 'enlarged');
    }
    if (target.matches(this.INPUT_SELECTOR) || target.closest(this.INPUT_SELECTOR)) {
      this.renderer.removeClass(this.cursorEl, 'oval');
    }
  };

  private updatePosition(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.rafId = requestAnimationFrame(() => {
        this.renderer.setStyle(this.cursorEl, 'left', `${this.mouseX}px`);
        this.renderer.setStyle(this.cursorEl, 'top', `${this.mouseY}px`);
        this.updatePosition();
      });
    }
  }
}
```

### 5.2 Styles (`custom-cursor.component.scss`)
```scss
.custom-cursor {
  position: fixed;
  pointer-events: none;
  z-index: 9999;
  width: 12px;
  height: 12px;
  border: 1px solid #e4c285;
  border-radius: 50%;
  background: transparent;
  transform: translate(-50%, -50%);
  transition: width 0.2s, height 0.2s, background-color 0.2s, border-radius 0.2s;
  mix-blend-mode: difference;

  &.enlarged {
    width: 24px;
    height: 24px;
    background: rgba(228, 194, 133, 0.2);
    border-radius: 50%;
  }

  &.oval {
    width: 4px;
    height: 20px;
    border-radius: 2px;
    border-color: #e4c285;
    background: transparent;
  }
}

// Hide cursor on touch devices
@media (any-pointer: coarse) {
  .custom-cursor { display: none; }
}
```

### 5.3 Global integration
In `app.component.html`, add `<app-custom-cursor></app-custom-cursor>` at the very beginning, so it sits above all content. Ensure the `CustomCursorComponent` is imported in `AppComponent`'s imports array (or use standalone component import in the component).

## 6. Self‑Review Checklist
- [ ] Public shell footer now shows the new links row in addition to the existing logo and copyright.
- [ ] User shell footer now shows the same link row.
- [ ] Links are dead (`#`) and do not navigate away.
- [ ] Custom cursor appears on all pages (public, user, admin) as a gold circle.
- [ ] Hovering over a button/action makes the cursor enlarge (24px, semi‑transparent).
- [ ] Hovering over an input/text area makes the cursor an oval (4px × 20px).
- [ ] Cursor is hidden on touch devices.
- [ ] Cursor movement is smooth and uses `requestAnimationFrame`.
- [ ] No console errors; no interference with existing interactions.
- [ ] No changes to any business logic or API calls.

