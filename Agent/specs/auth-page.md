# Specsheet: AuthPage (Login/Register) + Core Auth Infrastructure

### 0. Purpose

- Provide a single‑page interface for **login** and **registration**.
- Switch between forms via toggle buttons without changing route.
- On successful login: decode JWT, extract role, store in `AuthService`, console.log token/role, and redirect to the role‑specific dashboard.
- On successful registration: show success, switch to login tab.
- On error: show inline error message.
- If user already logged in (valid JWT), visiting `/auth` redirects to role dashboard immediately.

### 1. Route & Navigation

- **Path**: `/auth`
- **Route config** (in `app.routes.ts`):
  ```ts
  {
    path: 'auth',
    loadComponent: () => import('./features/auth/auth-page.component')
      .then(m => m.AuthPageComponent),
    canActivate: [() => inject(AuthRedirectGuard).canActivate()]
  }
  ```
- **Not linked from any navbar** – this page is only for unauthenticated users.

### 2. Authorization

- No authentication required to view.
- `AuthRedirectGuard` (functional guard) checks `AuthService.isAuthenticated()`. If `true` → redirect to user’s role dashboard (map below); if `false` → allow.

### 3. Required Supporting Infrastructure (built in this same spec)

- `AuthService` (`core/services/auth.service.ts`)
- `AuthRedirectGuard` (`core/guards/auth-redirect.guard.ts`)
- `jwt-decode` utility (`core/utils/jwt-decode.ts`)
- `LoginResponse` interface (types)

### 4. API Endpoints (Backend)

- **Base URL**: `http://localhost:5264/api/v1` (from `environment.development.ts` / `environment.ts`)
- **Login**: `POST /auth/login`
  - Request body: `LoginRequestDTO` (Swagger: `{ email: string, password: string }`)
  - Response: `{ token": "string","role": "string","firstName": "string","lastName": "string"}` (assumed JWT; we’ll type it as `LoginResponse`)
- **Register**: `POST /auth/register`
  - Request body: `RegisterRequestDTO` (Swagger: `{ email: string, password: string, firstName: string, lastName: string }`)
  - Response: 200 OK with no body (or a success object; we’ll treat 2xx as success)

### 5. Component API (AuthPageComponent)

- **Selector**: `app-auth-page`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `RouterLink`, Angular Material (`MatButtonModule`, `MatInputModule`, `MatFormFieldModule`, `MatIconModule`, `MatProgressSpinnerModule`, `MatCardModule`), `AlertComponent` (inline, built here), `LoginFormComponent`, `RegisterFormComponent` (both built here as inline components or subcomponents – we’ll define them in same file for simplicity).
- **No inputs/outputs** – it’s a page.

### 6. Template Structure

```
<div class="auth-container">
  <mat-card>
    <mat-card-header>
      <div class="toggle-buttons">
        <button mat-button (click)="isLoginMode.set(true)" [class.active]="isLoginMode()">Login</button>
        <button mat-button (click)="isLoginMode.set(false)" [class.active]="!isLoginMode()">Register</button>
      </div>
    </mat-card-header>
    <mat-card-content>
      @if (isLoginMode()) {
        <app-login-form (submitted)="onLogin($event)" [loading]="loading()" />
      } @else {
        <app-register-form (submitted)="onRegister($event)" [loading]="loading()" />
      }
      @if (errorMessage(); as msg) {
        <app-alert type="error" [message]="msg" (closed)="errorMessage.set(null)" />
      }
      @if (successMessage(); as msg) {
        <app-alert type="success" [message]="msg" (closed)="successMessage.set(null)" />
      }
    </mat-card-content>
  </mat-card>
</div>
```

- **LoginFormComponent** and **RegisterFormComponent** are standalone, presentational components that output a `submitted` event with form data.
- **AlertComponent** is a tiny standalone component that shows a styled message (success green, error red) with a close button.

### 7. State Management (All Local Signals)

In `AuthPageComponent`:

- `isLoginMode = signal(true)` – toggled by header buttons.
- `loading = signal(false)` – disables forms and shows spinner.
- `errorMessage = signal<string | null>(null)` – shown in alert.
- `successMessage = signal<string | null>(null)` – shown in alert.

No session persistence needed for UI mode (it’s temporary).

### 8. Data Flow & API Calls

- **AuthApiService** (injectable, provided in root) with two methods:
  ```ts
  login(credentials: LoginRequestDTO): Observable<LoginResponse>
  register(data: RegisterRequestDTO): Observable<void>
  ```
- On **login submit**:
  1. Set `loading = true`, clear messages.
  2. Call `authApi.login({email, password})`.
  3. On success:
     - Pass token to `AuthService.handleLogin(token)`.
     - Console.log token and role.
     - Set `successMessage = 'Login successful! Redirecting...'`.
     - Use `Router` to navigate to the role dashboard after 800ms (use `setTimeout` or `inject(Router).navigate(...)`).
  4. On error:
     - Set `errorMessage = err.error?.message || 'Invalid credentials.'`
  5. Finally, `loading = false` (use `finalize` in observable).
- On **register submit**:
  1. Set `loading = true`, clear messages.
  2. Call `authApi.register(data)`.
  3. On success:
     - Set `successMessage = 'Registration successful! Please log in.'`
     - Switch `isLoginMode.set(true)`.
  4. On error:
     - Set `errorMessage = err.error?.message || 'Registration failed.'`
  5. Finally, `loading = false`.

### 9. Forms & Validation

**LoginFormComponent**:

- Form: `FormGroup<{email: FormControl<string>, password: FormControl<string>}>`
- Validators:
  - `email`: required, pattern: `/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/`
  - `password`: required, pattern: `/^(?=.*[A-Za-z])(?=.*\d).{8,}$/` (min 8 chars, at least 1 letter, 1 digit)
- Mark all touched on submit; show per‑field `mat-error` using `hasError`.

**RegisterFormComponent**:

- Form: `FormGroup<{email: FormControl<string>, password: FormControl<string>, firstName: FormControl<string>, lastName: FormControl<string>}>`
- Same email/password validators.
- `firstName` and `lastName`: required, pattern: `/^[a-zA-ZÀ-ž\s\-']{2,50}$/`

### 10. UI States

- **Default**: blank form, no alert.
- **Loading**: submit button shows `MatProgressSpinner` (diameter 20) inside, disabled, all inputs disabled.
- **Success (login)**: green alert “Login successful! Redirecting…”, then redirect.
- **Success (register)**: green alert “Registration successful! Please log in.”, form switches to login.
- **Error**: red alert with server message or fallback. Alert dismissible.
- **Already logged in**: guard redirects away, never see page.

### 11. Responsive Behaviour

- **Mobile (<768px)**: full width, mat-card margin 16px, buttons stacked vertically.
- **Desktop**: centered card with max-width 450px, toggle buttons side by side.

### 12. Accessibility

- `aria-live="polite"` region on the alert container to announce success/error.
- Form fields have `aria-describedby` linking to error messages.
- Focus automatically moves to the first invalid field on failed validation (using `focus()` in component).
- Toggle buttons have `aria-pressed` binding.
- All inputs have associated `<mat-label>`.

### 13. Integration Notes (Critical for AI Agent)

- This spec builds the **core auth foundation**. It must produce:
  1. `AuthService` (root‑provided) with:
     - `token = signal<string | null>(null)` – read from `localStorage` on init.
     - `role = signal<string | null>(null)` – decoded from token.
     - `isAuthenticated = computed(() => !!this.token() && !this.isTokenExpired())`
     - `handleLogin(token: string)`: saves to `localStorage`, decodes, sets signals.
     - `logout()`: clears storage, resets signals.
     - `private isTokenExpired()`: checks `exp` claim.
  2. `jwtDecode()` utility (in `core/utils/jwt-decode.ts`): base64 decode payload, return object with `exp`, `role`, etc.
  3. `AuthRedirectGuard` (functional, using `inject(AuthService)` and `inject(Router)`):
     - If not authenticated → `true`.
     - If authenticated → return `UrlTree` to role‑based path:
       - Role mapping:
         - `'RegisteredUser'` → `/user/dashboard`
         - `'Admin'` → `/operations/admin/dashboard`
         - `'FrontDesk'` → `/operations/front-desk/dashboard`
         - `'Kitchen'` → `/operations/kitchen/dashboard`
         - `'Housekeeping'` → `/operations/housekeeping/dashboard`
         - `'Maintenance'` → `/operations/maintenance/dashboard`
         - Fallback → `/user/dashboard`
  4. `LoginResponse` interface: `{ token: string }`.
- **AuthApiService** must be created (`core/services/auth-api.service.ts`) with `login` and `register` methods using `HttpClient`. Base URL from environment.
- The route config for `/auth` must be placed in `app.routes.ts` as described.
- The AI agent must **not** build the role dashboards – they are future tasks; redirection to those paths will show 404 for now (acceptable until built).

### 14. Dependencies (Imports for AuthPageComponent)

- `CommonModule`, `ReactiveFormsModule`, `RouterLink`
- `MatCardModule`, `MatButtonModule`, `MatFormFieldModule`, `MatInputModule`, `MatIconModule`, `MatProgressSpinnerModule`
- `AlertComponent`, `LoginFormComponent`, `RegisterFormComponent` (all declared as standalone and imported directly)
- `AuthService` (injected)
- `AuthApiService` (injected)
- `Router` (injected for redirect)
- `DestroyRef` (if needed for observable cleanup – use `takeUntilDestroyed`)

### 15. File Structure (all created by AI agent)

```
src/
  app/
    core/
      services/
        auth.service.ts
        auth-api.service.ts
      guards/
        auth-redirect.guard.ts
      utils/
        jwt-decode.ts
      models/
        auth.models.ts
    features/
      auth/
        auth-page.component.ts
        auth-page.component.html
        auth-page.component.scss
        components/
          login-form.component.ts
          register-form.component.ts
          alert.component.ts
```

### 16. Testing Hints (Optional for AI agent, but recommend)

- `AuthService` unit tests: token storage, decode, expiry check.
- `AuthRedirectGuard` test: mock AuthService, expect redirect UrlTree or true.
- Form validation tests: invalid email, short password, etc.
- Integration test for `AuthPageComponent`: toggle forms, submit, handle success/error.

### 17. Final Self-Review (Mandatory)

Before completing the task, the AI agent **must perform a full implementation audit** against this specsheet. Do **not** skip this step.

#### Verification Checklist

Verify that all of the following have been completed:

##### Project Structure

- [ ] All required files were created in the correct locations.
- [ ] No unnecessary files or folders were added.
- [ ] Standalone component architecture was preserved.

##### Auth Infrastructure

- [ ] `AuthService` implemented exactly as specified.
- [ ] `AuthApiService` implemented using `HttpClient`.
- [ ] `jwtDecode()` utility implemented.
- [ ] `LoginResponse` interface created.
- [ ] `AuthRedirectGuard` implemented as a functional guard.
- [ ] JWT expiry checking implemented.
- [ ] LocalStorage persistence implemented.
- [ ] Role extraction implemented.
- [ ] Role-to-route mapping matches the specification exactly.

##### Routing

- [ ] `/auth` route added exactly as specified.
- [ ] Guard attached correctly.
- [ ] Authenticated users are redirected.
- [ ] Unauthenticated users can access `/auth`.

##### AuthPageComponent

- [ ] Uses Signals for local state.
- [ ] Uses Reactive Forms.
- [ ] Uses Angular Material components.
- [ ] Uses standalone components.
- [ ] Login/Register toggle implemented.
- [ ] Loading state implemented.
- [ ] Success state implemented.
- [ ] Error state implemented.

##### Login Form

- [ ] Email validation matches the regex exactly.
- [ ] Password validation matches the regex exactly.
- [ ] Invalid controls display `mat-error`.
- [ ] First invalid control receives focus.
- [ ] Submit emits only valid form data.

##### Register Form

- [ ] Email validation implemented.
- [ ] Password validation implemented.
- [ ] First Name validation implemented.
- [ ] Last Name validation implemented.
- [ ] Submit emits only valid form data.

##### Alert Component

- [ ] Supports success alerts.
- [ ] Supports error alerts.
- [ ] Dismiss button implemented.
- [ ] `aria-live="polite"` included.

##### API Integration

- [ ] Login calls `/auth/login`.
- [ ] Register calls `/auth/register`.
- [ ] Base URL comes from `environment`.
- [ ] `finalize()` used to reset loading.
- [ ] Proper error handling implemented.
- [ ] Success handling implemented.

##### Login Flow

- [ ] Token stored.
- [ ] JWT decoded.
- [ ] Role extracted.
- [ ] Token logged.
- [ ] Role logged.
- [ ] Success message shown.
- [ ] Redirect occurs after ~800 ms.

##### Register Flow

- [ ] Success message displayed.
- [ ] Login tab selected after success.
- [ ] Errors displayed inline.

##### Accessibility

- [ ] `aria-live` implemented.
- [ ] `aria-describedby` implemented.
- [ ] Toggle buttons expose `aria-pressed`.
- [ ] All inputs have `mat-label`.
- [ ] Focus moves to first invalid field.

##### Responsive Design

- [ ] Mobile layout implemented.
- [ ] Desktop layout implemented.
- [ ] Max width and spacing match specification.

##### Cleanup

- [ ] Uses `takeUntilDestroyed()` where appropriate.
- [ ] No memory leaks introduced.

### Final Compliance Report

After completing the implementation, provide a report in the following format:

```text
Specification Compliance Report

Files Created:
✓ auth.service.ts
✓ auth-api.service.ts
✓ auth-redirect.guard.ts
✓ jwt-decode.ts
✓ auth.models.ts
✓ auth-page.component.ts
✓ auth-page.component.html
✓ auth-page.component.scss
✓ login-form.component.ts
✓ register-form.component.ts
✓ alert.component.ts

Requirements Implemented:
✓ Auth infrastructure
✓ JWT decoding
✓ Route guard
✓ Login flow
✓ Registration flow
✓ Angular Material UI
✓ Reactive Forms
✓ Signals
✓ Accessibility
✓ Responsive layout

Known Deviations:
None

OR

Known Deviations:
- Dashboard routes intentionally redirect to currently non-existent pages (expected per specification).
- Backend response assumed to match documented API.
```

### Critical Rule

**Do not claim that a requirement has been implemented unless it actually exists in the generated code. If any requirement cannot be implemented, explicitly list it under "Known Deviations" with the reason. Do not silently omit or partially implement any requirement.**

---

