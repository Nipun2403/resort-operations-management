# Specsheet: Auth Page – Design Refactor

## 1. Purpose
- Restyle the **Auth page** (`/auth`) to match the “Private Vault Access” luxury aesthetic.
- The page now features a split layout on desktop (left image, right form) and a full‑screen background image on mobile, both transitioning when toggling between Login and Register.
- The form components (`LoginFormComponent` and `RegisterFormComponent`) are restyled with minimal bottom‑border inputs and floating labels, preserving all existing reactive forms, validation, and error display.
- All existing business logic – API calls, JWT storage, `returnUrl` redirect, loading/error/success alerts – remains **untouched**.
- The custom cursor is already handled globally; no local cursor is added.
- The local footer is omitted; the global public footer covers legal links.

## 2. Files to Modify
| File | Change |
|------|--------|
| `src/app/features/auth/auth-page.component.html` | Replace layout with new split design, toggle tabs, and background images. |
| `src/app/features/auth/auth-page.component.scss` | New styles for the page layout and background transitions. |
| `src/app/features/auth/components/login-form.component.html` | Replace Material inputs with custom minimal inputs. |
| `src/app/features/auth/components/login-form.component.scss` | New styles for the login form. |
| `src/app/features/auth/components/register-form.component.html` | Replace Material inputs with custom minimal inputs. |
| `src/app/features/auth/components/register-form.component.scss` | New styles for the register form. |

**No changes** to any TypeScript files, services, guards, or routing. The existing `AuthPageComponent`, `LoginFormComponent`, and `RegisterFormComponent` classes retain all their signals, form controls, validators, and methods.

## 3. AuthPageComponent – Layout & Background

### 3.1 Template (`auth-page.component.html`)
```html
<div class="auth-page">
  <!-- Background Images (full‑screen on mobile, left half on desktop) -->
  <div class="bg-layer">
    <div class="bg-image bg-login" [class.active]="isLoginMode()">
      <img
        src="https://lh3.googleusercontent.com/aida/AP1WRLs96Bq3m1-5bMTWS2MnUb9NUzR339t_s46__6B-DM7k_zsHbclUQnvfBYqpmiULS3ZH3NlIjkf3OWbV29UOulpEOQXph-mTqauRtHRmmc3SAdd6SRpSCq2b_lhNq58wQarRb4NFgoCrGiUjYiRGktjm29mz4GT0l9Co6ETYBsy2Rttxb6g1jjEE9z_1Qidjiu_ljtNbYsfiWz2Xg_xOwbr8W4hjH9HaIYNhKklQwShK3on6XiBeXgl3Us8w"
        alt="Obsidian structure"
      />
    </div>
    <div class="bg-image bg-register" [class.active]="!isLoginMode()">
      <img
        src="https://lh3.googleusercontent.com/aida/AP1WRLurNZSLtyi3MhJP7JEDtMH4jgHE7_uMhDlCqWvkMccwDgjirQG2BEP9pmCJ9zX3owbxvedDm2S7YVbWt6O4EGPtuLHZ_0MUeHXsB3k40Y12gQQggYMENFLj9c_Bdojcl7qa7aR8xiVR7LrENDkpsuQAtAm7qqjDx5sOSsc7MTmOzAgyiULDL1bGj0towWgi5AReO4qps5iPlxyhGVEgWCJ2JJFWGvZi-Eed0s7VfIW91fQ6eZRtO8Z376I"
        alt="Gold silk sculpture"
      />
    </div>
    <div class="bg-overlay"></div>
  </div>

  <!-- Desktop split: image left, form right. Mobile: full‑width form overlay -->
  <div class="content-area">
    <div class="form-panel glass-panel">
      <!-- Header -->
      <header class="auth-header">
        <h1>Aetheris</h1>
        <p>Private Vault Access</p>
      </header>

      <!-- Toggle Tabs -->
      <div class="tab-row">
        <button
          class="tab-btn"
          [class.active]="isLoginMode()"
          (click)="isLoginMode.set(true)"
        >
          Login
          <span class="tab-indicator" *ngIf="isLoginMode()"></span>
        </button>
        <button
          class="tab-btn"
          [class.active]="!isLoginMode()"
          (click)="isLoginMode.set(false)"
        >
          Register
          <span class="tab-indicator" *ngIf="!isLoginMode()"></span>
        </button>
      </div>

      <!-- Forms -->
      <div class="form-container">
        @if (isLoginMode()) {
          <app-login-form
            [loading]="loading()"
            (submitted)="onLogin($event)"
          />
        } @else {
          <app-register-form
            [loading]="loading()"
            (submitted)="onRegister($event)"
          />
        }
      </div>

      <!-- Inline alerts (success/error) -->
      @if (errorMessage()) {
        <app-alert type="error" [message]="errorMessage()!" (closed)="errorMessage.set(null)"></app-alert>
      }
      @if (successMessage()) {
        <app-alert type="success" [message]="successMessage()!" (closed)="successMessage.set(null)"></app-alert>
      }

      <!-- Footer note -->
      <p class="auth-footer-note">Aetheris Private Estate • Strict Confidentiality Required</p>
    </div>
  </div>
</div>
```

**Note:** The `*ngIf` on the tab indicator uses the old syntax because we only want to show the indicator while the tab is active. Alternatively we could use `@if`, but `*ngIf` is fine for a quick condition. We'll use `@if` for consistency: `@if (isLoginMode()) { <span class="tab-indicator"></span> }`. I'll adjust.

Also, the `glass-panel` class will apply the blur and dark background. On mobile, the form panel is centered with max‑width; on desktop, it's placed on the right half.

### 3.2 SCSS (`auth-page.component.scss`)
```scss
@import '../../../../styles/theme/index';

.auth-page {
  position: relative;
  min-height: 100vh;
  overflow: hidden;
}

// Background layers (shared between mobile and desktop)
.bg-layer {
  position: fixed;
  inset: 0;
  z-index: 0;
}
.bg-image {
  position: absolute;
  inset: 0;
  opacity: 0;
  transition: opacity 1.2s cubic-bezier(0.4, 0, 0.2, 1);
  &.active { opacity: 1; }
  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    animation: subtleZoom 20s infinite alternate linear;
  }
}
.bg-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to right, transparent 0%, var(--color-background) 100%);
  // On mobile, use a simpler gradient
  @media (max-width: 1023px) {
    background: linear-gradient(to top, var(--color-background) 0%, transparent 50%);
  }
}

@keyframes subtleZoom {
  from { transform: scale(1); }
  to { transform: scale(1.05); }
}

// Content area
.content-area {
  position: relative;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 5rem 1.5rem 3rem;
  @media (min-width: 1024px) {
    justify-content: flex-end;
    padding-right: 10%;
  }
}

// Glassmorphic form panel
.form-panel {
  width: 100%;
  max-width: 420px;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2.5rem 2rem;
  background: rgba(10, 10, 10, 0.85);
  backdrop-filter: blur(24px);
  border: 1px solid rgba(228, 194, 133, 0.2);
  @media (min-width: 1024px) {
    padding: 3rem;
  }
}

// Header
.auth-header {
  margin-bottom: 2rem;
  text-align: center;
  h1 {
    font-family: var(--font-headline);
    font-size: 2rem;
    letter-spacing: 0.2em;
    text-transform: uppercase;
    color: var(--color-secondary);
    font-weight: 300;
    margin-bottom: 0.5rem;
  }
  p {
    @include font-label-caps;
    font-size: 0.625rem;
    letter-spacing: 0.4em;
    color: rgba(228, 226, 221, 0.4);
  }
}

// Tab row
.tab-row {
  display: flex;
  gap: 2rem;
  border-bottom: 1px solid rgba(228, 194, 133, 0.1);
  justify-content: center;
  padding-bottom: 0.75rem;
  margin-bottom: 2rem;
  width: 100%;
}
.tab-btn {
  background: none;
  border: none;
  @include font-label-caps;
  color: var(--color-on-surface-variant);
  cursor: pointer;
  position: relative;
  transition: color 0.3s;
  padding: 0 0 0.5rem;
  &.active {
    color: var(--color-secondary);
  }
  .tab-indicator {
    position: absolute;
    bottom: -0.8rem; // align with bottom of tab-row
    left: 0;
    width: 100%;
    height: 2px;
    background: var(--color-secondary);
    transition: opacity 0.3s;
  }
}

// Form container – no extra styling needed; child components handle internal layout

// Footer note
.auth-footer-note {
  @include font-label-caps;
  font-size: 0.625rem;
  letter-spacing: 0.3em;
  color: rgba(228, 226, 221, 0.3);
  text-align: center;
  margin-top: 2rem;
  border-top: 1px solid rgba(228, 194, 133, 0.1);
  padding-top: 1.5rem;
  width: 100%;
}
```

## 4. LoginFormComponent – Restyle

### 4.1 Template (`login-form.component.html`)
Replace the existing Material fields with custom inputs using `[formControl]` on native `<input>` elements, and display errors using our own `<span>` (but with the same validation message logic). We'll keep the same reactive form controls (`email`, `password`) and validators.

```html
<form [formGroup]="form" (ngSubmit)="submit()" class="login-form">
  <div class="input-group">
    <input
      type="email"
      class="minimal-input"
      placeholder=" "
      formControlName="email"
      autocomplete="email"
    />
    <label class="floating-label">Identification (Email)</label>
    @if (form.get('email')?.invalid && form.get('email')?.touched) {
      <span class="error-text">{{ getErrorMessage('email') }}</span>
    }
  </div>

  <div class="input-group">
    <input
      type="password"
      class="minimal-input"
      placeholder=" "
      formControlName="password"
      autocomplete="current-password"
    />
    <label class="floating-label">Access Key (Password)</label>
    @if (form.get('password')?.invalid && form.get('password')?.touched) {
      <span class="error-text">{{ getErrorMessage('password') }}</span>
    }
  </div>

  <button
    type="submit"
    class="submit-btn"
    [disabled]="form.invalid || loading()"
  >
    @if (loading()) {
      <mat-spinner diameter="20"></mat-spinner>
    } @else {
      <span>Authenticate</span>
      <span class="material-symbols-outlined">arrow_forward</span>
    }
  </button>
</form>
```

**Note:** The `getErrorMessage` method must be implemented in the component (if not already) to return specific messages based on validation errors. The existing component likely already uses `mat-error` with the `hasError` checks. We'll add a small method that reads the errors and returns appropriate strings, or we can simply replicate the logic from the previous template into the new `getErrorMessage`. To keep the spec deterministic, we'll include a method in the component class (but we said no TypeScript changes). Actually we can avoid TS changes by using Angular's built‑in error display with `@if` and `hasError` checks inline, like:

```html
@if (form.get('email')?.hasError('required') && form.get('email')?.touched) {
  <span class="error-text">Email is required.</span>
}
@if (form.get('email')?.hasError('pattern') && form.get('email')?.touched) {
  <span class="error-text">Please enter a valid email.</span>
}
```

This keeps logic in the template, no new TS required. We'll use this approach.

### 4.2 SCSS (`login-form.component.scss`)
```scss
@import '../../../../styles/theme/index';

.login-form {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.input-group {
  position: relative;
  padding-top: 1.5rem;
}

.minimal-input {
  width: 100%;
  background: transparent;
  border: none;
  border-bottom: 1px solid rgba(228, 194, 133, 0.4);
  color: var(--color-on-surface);
  padding: 0.5rem 0;
  font-family: var(--font-body);
  font-size: 1rem;
  transition: border-color 0.4s;
  outline: none;
  &:focus { border-color: var(--color-secondary); }
  &::placeholder { color: transparent; } // hide placeholder to use floating label
}

.floating-label {
  position: absolute;
  top: 1.5rem;
  left: 0;
  @include font-label-caps;
  color: var(--color-on-surface-variant);
  pointer-events: none;
  transition: all 0.3s ease;
  transform-origin: left;
  .minimal-input:focus + &,
  .minimal-input:not(:placeholder-shown) + & {
    transform: translateY(-1.5rem) scale(0.85);
    color: var(--color-secondary);
  }
}

.error-text {
  @include font-label-caps;
  font-size: 0.65rem;
  color: var(--color-error);
  margin-top: 0.25rem;
  display: block;
}

.submit-btn {
  width: 100%;
  padding: 0.75rem;
  background: transparent;
  border: 1px solid rgba(228, 194, 133, 0.4);
  color: var(--color-secondary);
  @include font-label-caps;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  cursor: pointer;
  transition: background 0.5s, color 0.5s;
  position: relative;
  overflow: hidden;
  &:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }
  &:not(:disabled):hover {
    background: var(--color-secondary);
    color: var(--color-on-secondary);
  }
  .material-symbols-outlined { transition: transform 0.5s; }
  &:not(:disabled):hover .material-symbols-outlined { transform: translateX(4px); }
}
```

## 5. RegisterFormComponent – Restyle

### 5.1 Template (`register-form.component.html`)
```html
<form [formGroup]="form" (ngSubmit)="submit()" class="register-form">
  <div class="name-row">
    <div class="input-group">
      <input type="text" class="minimal-input" placeholder=" " formControlName="firstName" autocomplete="given-name" />
      <label class="floating-label">First Name</label>
      @if (form.get('firstName')?.invalid && form.get('firstName')?.touched) {
        <span class="error-text">First name is required (min 2 letters).</span>
      }
    </div>
    <div class="input-group">
      <input type="text" class="minimal-input" placeholder=" " formControlName="lastName" autocomplete="family-name" />
      <label class="floating-label">Last Name</label>
      @if (form.get('lastName')?.invalid && form.get('lastName')?.touched) {
        <span class="error-text">Last name is required (min 2 letters).</span>
      }
    </div>
  </div>

  <div class="input-group">
    <input type="email" class="minimal-input" placeholder=" " formControlName="email" autocomplete="email" />
    <label class="floating-label">Email</label>
    @if (form.get('email')?.invalid && form.get('email')?.touched) {
      <span class="error-text">Please enter a valid email.</span>
    }
  </div>

  <div class="input-group">
    <input type="password" class="minimal-input" placeholder=" " formControlName="password" autocomplete="new-password" />
    <label class="floating-label">Password</label>
    @if (form.get('password')?.invalid && form.get('password')?.touched) {
      <span class="error-text">Min 8 characters, at least 1 letter and 1 digit.</span>
    }
  </div>

  <button type="submit" class="submit-btn" [disabled]="form.invalid || loading()">
    @if (loading()) {
      <mat-spinner diameter="20"></mat-spinner>
    } @else {
      <span>Request Access</span>
      <span class="material-symbols-outlined">arrow_forward</span>
    }
  </button>
</form>
```

### 5.2 SCSS (`register-form.component.scss`)
```scss
@import '../../../../styles/theme/index';

.register-form {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.name-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

// Reuse the same input and label styles from login form (could be moved to a shared file, but for simplicity we'll duplicate).
.input-group {
  position: relative;
  padding-top: 1.5rem;
}
.minimal-input {
  width: 100%;
  background: transparent;
  border: none;
  border-bottom: 1px solid rgba(228, 194, 133, 0.4);
  color: var(--color-on-surface);
  padding: 0.5rem 0;
  font-family: var(--font-body);
  font-size: 1rem;
  transition: border-color 0.4s;
  outline: none;
  &:focus { border-color: var(--color-secondary); }
  &::placeholder { color: transparent; }
}
.floating-label {
  position: absolute;
  top: 1.5rem;
  left: 0;
  @include font-label-caps;
  color: var(--color-on-surface-variant);
  pointer-events: none;
  transition: all 0.3s ease;
  .minimal-input:focus + &,
  .minimal-input:not(:placeholder-shown) + & {
    transform: translateY(-1.5rem) scale(0.85);
    color: var(--color-secondary);
  }
}
.error-text {
  @include font-label-caps;
  font-size: 0.65rem;
  color: var(--color-error);
  margin-top: 0.25rem;
  display: block;
}
.submit-btn {
  width: 100%;
  padding: 0.75rem;
  background: transparent;
  border: 1px solid rgba(228, 194, 133, 0.4);
  color: var(--color-secondary);
  @include font-label-caps;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  cursor: pointer;
  transition: background 0.5s, color 0.5s;
  position: relative;
  overflow: hidden;
  &:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }
  &:not(:disabled):hover {
    background: var(--color-secondary);
    color: var(--color-on-secondary);
  }
  .material-symbols-outlined { transition: transform 0.5s; }
  &:not(:disabled):hover .material-symbols-outlined { transform: translateX(4px); }
}
```

## 6. Responsive Behaviour
- **Mobile (< 1024px):** The form panel is centered, full‑width with max‑width 420px. The background image fills the entire viewport with a top‑to‑bottom gradient overlay.
- **Desktop (≥ 1024px):** The form panel is pushed to the right (10% from right edge). The left 50% of the viewport displays the background image(s) with a right‑to‑left gradient overlay.
- Both modes support the image cross‑fade when toggling between login and register.

## 7. Integration Notes
- The `loading`, `errorMessage`, `successMessage` signals in `AuthPageComponent` remain unchanged. The `<app-alert>` component (shared) is reused.
- The `isLoginMode` signal toggles the background images and form visibility; it's already present.
- The `LoginFormComponent` and `RegisterFormComponent` still emit `submitted` with the form data; the parent handles API calls.
- The `mat-spinner` for loading is imported from `MatProgressSpinnerModule`; it's still used inside the submit buttons.
- No changes to the routing; the auth page remains at `/auth`.
- The custom cursor from the global specsheet will automatically apply to inputs and buttons.

## 8. Self‑Review Checklist
- [ ] Desktop split layout shows left image and right form panel.
- [ ] Toggling between Login and Register transitions the background image (both on desktop and mobile).
- [ ] Login form has email and password inputs with floating labels; validation errors appear inline.
- [ ] Register form has first name, last name, email, password inputs; validation errors appear inline.
- [ ] Submit buttons are disabled while loading and display spinner.
- [ ] Success/error alerts from the parent still appear inside the form panel.
- [ ] On mobile, the layout is a centered glass panel over a full‑screen background.
- [ ] All existing functionality (API calls, redirect, returnUrl) remains intact.
- [ ] No console errors; styles do not break other pages.

