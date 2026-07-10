# Frontend Security & Feature Report — Hotel Management System

> **Prepared for:** Architecture & Implementation Review  
> **Scope:** Angular 22 Frontend — AETHERIS Luxury Resort Hotel Management  
> **Base URL:** `http://localhost:5264/api/v1`

---

## Table of Contents

1. [Authentication & Authorization System](#1-authentication--authorization-system)
2. [HTTP Security Interceptors](#2-http-security-interceptors)
3. [Error Handling Architecture](#3-error-handling-architecture)
4. [Route Security & Role-Based Access Control (RBAC)](#4-route-security--role-based-access-control-rbac)
5. [Form Validation & Input Sanitization](#5-form-validation--input-sanitization)
6. [Real-Time Features (SignalR)](#6-real-time-features-signalr)
7. [Signals-Based State Management](#7-signals-based-state-management)
8. [Reusable UI Component Infrastructure](#8-reusable-ui-component-infrastructure)
9. [Responsive Design & Theming](#9-responsive-design--theming)
10. [Admin Feature Suite](#10-admin-feature-suite)
11. [Route Architecture Complete Map](#11-route-architecture-complete-map)
12. [Tech Stack & Dependencies](#12-tech-stack--dependencies)
13. [Backend API Integration Map](#13-backend-api-integration-map)

---

## 1. Authentication & Authorization System

### 1.1 Authentication Flow

The authentication system is built on **JWT (JSON Web Token)** token-based authentication with **Angular Signals** for reactive state management.

| Component | File | Role |
|-----------|------|------|
| `AuthService` | `core/services/auth.service.ts` | Central auth state management |
| `AuthApiService` | `core/services/auth-api.service.ts` | HTTP calls to auth endpoints |
| `jwtDecode` | `core/utils/jwt-decode.ts` | Custom JWT payload decoder |
| `AuthPageComponent` | `features/auth/pages/auth-page.component.ts` | Login/Register orchestration |
| `LoginFormComponent` | `features/auth/components/login-form/login-form.component.ts` | Login form UI + validation |
| `RegisterFormComponent` | `features/auth/components/register-form/register-form.component.ts` | Registration form UI + validation |

**Login Flow (step by step):**

```
User submits credentials
  → LoginFormComponent emits credentials
  → AuthPageComponent.onLogin() called
  → AuthApiService.login(credentials) → POST /auth/login
  → Backend validates credentials, returns JWT
  → AuthService.handleLogin(token):
      1. localStorage.setItem('token', token)          ← Persistence
      2. this.token.set(token)                          ← Signal update
      3. const decoded = jwtDecode(token)               ← Payload extraction
      4. this.role.set(decoded.role)                    ← Role signal
      5. this._decodedToken.set(decoded)                ← Full payload
      6. Calculates isAuthenticated = token exists && !expired  ← Computed
  → 800ms delay (success animation)
  → AuthRedirectGuard reads role → navigates to role-based dashboard
```

### 1.2 Token Storage & Management

| Property | Type | Description |
|----------|------|-------------|
| `token()` | `WritableSignal<string \| null>` | The raw JWT string |
| `role()` | `WritableSignal<string \| null>` | User's role extracted from JWT |
| `_decodedToken()` | `WritableSignal<JwtPayload \| null>` | Full decoded payload |
| `isAuthenticated()` | `ComputedSignal<boolean>` | `true` only if token exists AND not expired |
| `fullName()` | `ComputedSignal<string>` | Derived from JWT claims |

**JWT Payload Structure (`JwtPayload` interface):**

```typescript
interface JwtPayload {
  exp: number;            // Expiry timestamp (Unix epoch)
  role: string;           // User role
  firstName: string;      // User's first name
  lastName: string;       // User's last name
  given_name?: string;    // Fallback name claim
  family_name?: string;   // Fallback name claim
  [key: string]: unknown; // Additional claims
}
```

**Token Expiry Detection:**

```typescript
readonly isAuthenticated: Signal<boolean> = computed(() => {
  const token = this.token();
  if (!token) return false;
  try {
    const decoded = jwtDecode(token);
    return decoded.exp * 1000 > Date.now();  // exp is in seconds, Date.now() in ms
  } catch {
    return false;  // Malformed token → not authenticated
  }
});
```

### 1.3 Custom JWT Decoder Utility

**File:** `core/utils/jwt-decode.ts`

The project implements a **custom JWT decoder** without any third-party library (no `jwt-decode` npm package):

- Splits the JWT into header, payload, signature
- Base64-decodes the payload using `atob()` with URL-safe character replacement (`-` → `+`, `_` → `/`)
- Handles `given_name`/`family_name` as fallback claims if `firstName`/`lastName` are absent
- Returns `JwtPayload` interface

### 1.4 Logout Flow

```typescript
logout(): void {
  localStorage.removeItem('token');  // Clear persistence
  this.token.set(null);              // Reset signals
  this.role.set(null);
  this._decodedToken.set(null);
  // isAuthenticated computed signal automatically recalculates → false
}
```

---

## 2. HTTP Security Interceptors

The application uses three **functional HTTP interceptors** registered in `app.config.ts`:

```typescript
withInterceptors([authInterceptor, errorPageInterceptor, idempotencyInterceptor])
```

**Execution order:** `authInterceptor` → `errorPageInterceptor` → `idempotencyInterceptor`

### 2.1 Auth Interceptor

**File:** `core/interceptors/auth.interceptor.ts`

| Feature | Detail |
|---------|--------|
| **Purpose** | Attaches `Authorization: Bearer <token>` to every HTTP request |
| **Token source** | `AuthService.token()` signal (reactive) |
| **Skip logic** | Excludes `/auth/login` and `/auth/register` to prevent stale-token 401 on login |
| **Implementation** | Functional interceptor using `inject()` for DI |

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.token();

  if (token && !req.url.includes('/auth/login') && !req.url.includes('/auth/register')) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next(req);
};
```

**Security implications:**
- Prevents anonymous API access for protected endpoints
- Reactive token retrieval ensures latest token is always used
- Excludes auth endpoints to avoid circular dependency scenarios

### 2.2 Error Page Interceptor

**File:** `core/interceptors/error-page.interceptor.ts`

| Status Code | Action | Notes |
|-------------|--------|-------|
| `403` | Redirect to `/error/403` | `replaceUrl: true` prevents back-button loop |
| `404` | Redirect to `/error/404` | Only for GET/HEAD (mutation errors surface in-form) |
| `5xx` | Redirect to `/error/500` | Server-side failures |

**Exclusion rules:**
- Only intercepts **GET and HEAD** requests (mutations show inline errors in forms)
- Skips `/auth/login` and `/auth/register` endpoints
- Returns `EMPTY` from RxJS to halt the request pipeline after redirect

### 2.3 Idempotency Interceptor

**File:** `core/interceptors/idempotency.interceptor.ts`

| Feature | Detail |
|---------|--------|
| **Purpose** | Prevents duplicate processing of mutation requests |
| **Header** | `X-Idempotency-Key: <UUID>` |
| **Scope** | POST, PUT, PATCH requests |
| **Generation** | `crypto.randomUUID()` (native browser API) |

**Use case:** If a network failure causes the client to retry a POST request, the backend can detect the duplicate `X-Idempotency-Key` and return the original result instead of processing the request twice. This is critical for financial operations (billing, bookings).

---

## 3. Error Handling Architecture

### 3.1 Global Error Handler

**File:** `core/services/global-error-handler.service.ts`

| Property | Detail |
|----------|--------|
| **Base class** | Angular `ErrorHandler` |
| **Provided-in** | `root` |
| **Event** | Catches unhandled JavaScript errors (not HTTP) |
| **Action** | Navigates to `/error/500` via `queueMicrotask()` |
| **Loop guard** | Skips redirect if already on `/error` path |

```typescript
@Injectable({ providedIn: 'root' })
export class GlobalErrorHandlerService implements ErrorHandler {
  handleError(error: Error): void {
    const router = inject(Router);
    const currentUrl = router.url;

    if (!currentUrl.startsWith('/error')) {
      queueMicrotask(() => router.navigate(['/error', 500], { replaceUrl: true }));
    }
    console.error('Unhandled Error:', error);
  }
}
```

**Registered in `app.config.ts`:**

```typescript
provideAppInitializer(() => {
  const errorHandler = inject(GlobalErrorHandlerService);
  ErrorHandler.prototype.handleError = (error) => errorHandler.handleError(error);
})
```

### 3.2 Error Page Component

**File:** `features/error-page/error-page.component.ts`

| Status | Title | Subtitle | Visual Theme |
|--------|-------|----------|-------------|
| `403` | "Access Restricted" | "Private Vault" | Lock icon, restricted-access metaphor |
| `404` | "Page Not Found" | "Hidden Corridor" | Search icon, hidden-passage metaphor |
| `500` | "Unexpected Error" | "System Fault" | Warning icon, machinery-failure metaphor |

**Features:**
- Mouse-tracking glow effect on the background (CSS `radial-gradient` following cursor)
- "Return Home" button for all error states
- Dynamic status-based messaging from a content map
- `replaceUrl: true` on all error navigations to maintain clean browser history

### 3.3 Error Handling in API Services

The application employs two error-handling strategies in API services:

**Strategy A — Let errors bubble to component (majority of services):**
- Services do NOT use `catchError`
- Components handle errors in `.subscribe({ error: (err) => ... })` callbacks
- Allows per-component error presentation

**Strategy B — Service-level `catchError` (admin services for Staff, Amenity, MenuItem):**
- Uses `catchError` to transform or log errors
- Returns a safe default value
- Prevents errors from propagating to the component if the service can gracefully degrade

---

## 4. Route Security & Role-Based Access Control (RBAC)

### 4.1 Role Guards — Complete Matrix

Six **functional route guards** implement role-based access. All follow the same pattern:

```typescript
export const adminGuard: CanActivateFn & CanMatchFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/auth'], { queryParams: { returnUrl: state.url } });
  }
  if (authService.role() !== 'Admin') {
    return router.createUrlTree(['/error', 403]);
  }
  return true;
};
```

| Guard | Required Role | Not Authenticated | Wrong Role |
|-------|--------------|-------------------|------------|
| `adminGuard` | `Admin` | Redirect `/auth?returnUrl=` | Redirect `/error/403` |
| `customerGuard` | `RegisteredUser` | Redirect `/auth?returnUrl=` | Redirect `/error/403` |
| `frontDeskGuard` | `FrontDesk` | Redirect `/auth?returnUrl=` | Redirect `/error/403` |
| `kitchenGuard` | `Kitchen` | Redirect `/auth?returnUrl=` | Redirect `/error/403` |
| `housekeepingGuard` | `Housekeeping` | Redirect `/auth?returnUrl=` | Redirect `/error/403` |
| `maintenanceGuard` | `Maintenance` | Redirect `/auth?returnUrl=` | Redirect `/error/403` |

**Technical details:**
- All guards implement BOTH `CanActivateFn` and `CanMatchFn` for maximum routing flexibility
- Uses `inject()` for dependency injection (functional pattern, not class-based)
- `returnUrl` query param enables post-login redirect to the originally requested page
- Guards are pure functions — no side effects beyond route decisions

### 4.2 Auth Redirect Guard

**File:** `core/guards/auth-redirect.guard.ts`

**Purpose:** When an already-authenticated user navigates to `/auth`, redirect them to their appropriate dashboard instead of showing the login page.

**Class-based** (unlike the functional role guards) due to its complexity:

```typescript
@Injectable({ providedIn: 'root' })
export class AuthRedirectGuard implements CanActivate {
  private authService = inject(AuthService);
  private router = inject(Router);

  canActivate(route: ActivatedRouteSnapshot): boolean | UrlTree {
    if (!this.authService.isAuthenticated()) return true;  // Show auth page

    const returnUrl = route.queryParams['returnUrl'];
    if (returnUrl) return this.router.parseUrl(returnUrl);  // Respect returnUrl

    return this.getDashboardRoute();
  }

  private getDashboardRoute(): UrlTree {
    const role = this.authService.role();
    const routeMap: Record<string, string> = {
      Admin: '/operations/admin/dashboard',
      RegisteredUser: '/user/dashboard',
      FrontDesk: '/operations/front-desk/dashboard',
      Kitchen: '/operations/kitchen/dashboard',
      Housekeeping: '/operations/housekeeping/dashboard',
      Maintenance: '/operations/maintenance/dashboard',
    };
    return this.router.createUrlTree([routeMap[role] || '/home']);
  }
}
```

### 4.3 Route to Role Mapping Summary

| Path Segment | Role Required | Shell Component |
|-------------|---------------|-----------------|
| `/operations/admin/*` | `Admin` | `AdminShellComponent` |
| `/operations/front-desk/*` | `FrontDesk` | `FrontDeskShellComponent` |
| `/operations/kitchen/*` | `Kitchen` | `KitchenShellComponent` |
| `/operations/housekeeping/*` | `Housekeeping` | `HousekeepingShellComponent` |
| `/operations/maintenance/*` | `Maintenance` | `MaintenanceShellComponent` |
| `/user/*` | `RegisteredUser` | `UserShellComponent` |
| public routes (`/home`, `/rooms`, `/auth`, etc.) | None (public) | `PublicShellComponent` |

---

## 5. Form Validation & Input Sanitization

### 5.1 Auth Forms — Exact Regex Patterns

#### Login Form (`features/auth/components/login-form/`)

| Field | Validators | Pattern | Error Message |
|-------|-----------|---------|---------------|
| **Email** | `Validators.required` + `Validators.pattern()` | `/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/` | "Please enter a valid email address" |
| **Password** | `Validators.required` + `Validators.pattern()` | `/^(?=.*[A-Za-z])(?=.*\d).{8,}$/` | "Password must be at least 8 characters long and contain at least one letter and one number" |

**Login regex breakdown:**
- `^[a-zA-Z0-9._%+-]+` — Local part: alphanumeric, dots, underscores, percent, plus, hyphens
- `@` — At symbol
- `[a-zA-Z0-9.-]+` — Domain: alphanumeric, dots, hyphens
- `\.` — Dot before TLD
- `[a-zA-Z]{2,}$` — TLD: at least 2 alpha characters

**Password regex breakdown:**
- `(?=.*[A-Za-z])` — At least one letter (upper or lowercase)
- `(?=.*\d)` — At least one digit
- `.{8,}` — Minimum 8 characters total

#### Register Form (`features/auth/components/register-form/`)

| Field | Validators | Pattern | Error Message |
|-------|-----------|---------|---------------|
| **First Name** | `Validators.required` + `Validators.pattern()` | `/^[a-zA-ZÀ-ž\s\-']{2,50}$/` | "First name must be 2-50 characters and contain only letters" |
| **Last Name** | `Validators.required` + `Validators.pattern()` | `/^[a-zA-ZÀ-ž\s\-']{2,50}$/` | "Last name must be 2-50 characters and contain only letters" |
| **Email** | `Validators.required` + `Validators.pattern()` | Same as login | Same as login |
| **Password** | `Validators.required` + `Validators.pattern()` | Same as login | Same as login |

**Name regex breakdown:**
- `[a-zA-ZÀ-ž]` — Latin letters + accented characters (À-ž covers European diacritics)
- `[\s\-']` — Allows spaces, hyphens, and apostrophes (e.g., "O'Brien", "Anne-Marie", "Van Der Berg")
- `{2,50}` — Length constraint matching database column size

### 5.2 Profile Forms

| Field | Validators | Pattern |
|-------|-----------|---------|
| **First Name** | `Validators.required` + `Validators.minLength(2)` + `Validators.pattern()` | `/^[a-zA-ZÀ-ž\s\-']+$/` |
| **Last Name** | `Validators.required` + `Validators.minLength(2)` + `Validators.pattern()` | `/^[a-zA-ZÀ-ž\s\-']+$/` |
| **Email** | `Validators.required` + `Validators.email` | Built-in Angular email validator |

### 5.3 Password Change Form — Cross-Field Validation

**File:** `shared/components/profile/` (Password Change section)

| Field | Validators |
|-------|-----------|
| Current Password | `Validators.required` |
| New Password | `Validators.required` + `Validators.minLength(8)` + `Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/)` |
| Confirm New Password | `Validators.required` |

**Custom Cross-Field Validator — `passwordsMatchValidator`:**

```typescript
function passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
  const newPassword = group.get('newPassword')?.value;
  const confirmPassword = group.get('confirmNewPassword')?.value;
  return newPassword && confirmPassword && newPassword !== confirmPassword
    ? { passwordsMismatch: true }
    : null;
}
```

- Applied at the `FormGroup` level, not individual controls
- Error message: "Passwords do not match"
- Re-validates on any change to either password field via `updateOn: 'change'`

### 5.4 Front Desk Booking Wizard Validation

The booking wizard uses a **MatStepper** with multi-step validation:

| Step | Field(s) | Validators |
|------|----------|-----------|
| **Guest Details** | First Name, Last Name, Email | Same name/email patterns as register form |
| **Booking Dates** | Check-In Date, Check-Out Date | `Validators.required` + `futureDateValidator` (check-in not in past) + `checkOutAfterCheckIn` (cross-field: checkout > checkin) |
| **Guest Count** | Adults, Children | `Validators.min(1)`, `Validators.max(20)` |
| **Rooms** | Room type selection + quantities | Requires ≥1 room; `capacityWarning` computed signal prevents exceeding `availableCount` per room type |

**Custom Date Validators:**

```typescript
function futureDateValidator(control: AbstractControl): ValidationErrors | null {
  const date = new Date(control.value);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return date < today ? { pastDate: true } : null;
}

function checkOutAfterCheckIn(group: AbstractControl): ValidationErrors | null {
  const checkIn = group.get('checkInDate')?.value;
  const checkOut = group.get('checkOutDate')?.value;
  if (checkIn && checkOut) {
    return new Date(checkOut) <= new Date(checkIn) ? { invalidRange: true } : null;
  }
  return null;
}
```

### 5.5 Generic CRUD Modal Validation

The `CrudModalComponent` supports dynamic form field types with built-in validation:

| Field Type | Validation |
|-----------|-----------|
| `text` | `Validators.required` + `Validators.minLength` / `Validators.maxLength` (configurable) |
| `number` | `Validators.required` + `Validators.min` / `Validators.max` |
| `email` | `Validators.required` + `Validators.email` or pattern check |
| `password` | Standard password pattern |
| `textarea` | `Validators.required` |
| `date` | `Validators.required` |
| `url` | Pattern: `/^https?:\/\/.+/` (must start with http:// or https://) |
| `select` | `Validators.required` |
| `toggle` | No validation (boolean) |
| `keyValueList` | Key pattern: `/^[a-zA-Z0-9\s\-']+$/`, Value: `Validators.required` + `min(1)` |
| `imageUrlList` | URL pattern validation per entry |

**Error message system:**

```typescript
const errorMessages: Record<string, string> = {
  required: 'This field is required',
  email: 'Please enter a valid email address',
  pattern: 'Invalid format',
  min: 'Value is too low',
  max: 'Value is too high',
  minlength: 'Too short',
  maxlength: 'Too long',
};
```

---

## 6. Real-Time Features (SignalR)

### 6.1 SignalR Notification Service

**File:** `core/services/notification.service.ts`

| Feature | Detail |
|---------|--------|
| **Library** | `@microsoft/signalr` ^10.0.0 |
| **Transport** | WebSockets (fallback to Server-Sent Events and Long Polling) |
| **Auth** | Token-based via `accessTokenFactory` |
| **Hub URL** | Derived from `environment.baseUrl` by replacing `/api/v1` suffix with `/notifications` |
| **Connection** | Established on service construction, reconnects automatically |

```typescript
private hubConnection = new signalR.HubConnectionBuilder()
  .withUrl(this.notificationHubUrl, {
    accessTokenFactory: () => this.authService.token() ?? ''
  })
  .withAutomaticReconnect()
  .build();
```

### 6.2 Real-Time Events

| Hub Method | Purpose | Trigger |
|-----------|---------|---------|
| `ReceiveAlert` | General notification | Server-side entity state changes |
| Task status updates | Kitchen, housekeeping, maintenance task refreshes | Status transitions (e.g., Pending → InProgress → Completed) |

### 6.3 Real-Time Notification Snackbar

**File:** `shared/components/notification-snackbar/`

- Custom Material snackbar component
- Styled consistently with the dark AETHERIS theme
- Displays real-time alerts from SignalR hub
- Auto-dismisses after configurable duration

### 6.4 Task Dashboard Real-Time Refresh

The `TaskDashboardComponent` accepts a `refresh` input signal (`Subject<void>`) that increments to trigger data reload:

```typescript
readonly refreshTrigger = signal(0);

ngOnInit(): void {
  effect(() => {
    this.refreshTrigger();  // Read signal to track it
    this.loadData();
  });

  // SignalR subscription
  this.notificationService.alertReceived$.pipe(takeUntilDestroyed()).subscribe(() => {
    this.refreshTrigger.update(n => n + 1);  // Trigger refresh
    this.showNotification();
  });
}
```

---

## 7. Signals-Based State Management

### 7.1 Architecture Overview

The application uses **Angular Signals** for all local and shared state — no NgRx, no Redux, no external state management library.

| Signal Type | Usage |
|-------------|-------|
| `signal<T>()` | Mutable local component state |
| `computed<T>()` | Derived values (automatic recalculation) |
| `effect()` | Side effects (navigation, API calls, logging) |
| `input<T>()` | Component inputs |
| `output<T>()` | Component outputs |
| `toSignal()` | Convert Observable → Signal (e.g., `BreakpointObserver`, `valueChanges`) |

### 7.2 State Management Patterns

**Local component state pattern:**

```typescript
// Signals for each piece of state
readonly loading = signal(false);
readonly error = signal<string | null>(null);
readonly data = signal<T[]>([]);
readonly selectedId = signal<string | null>(null);

// Computed derived state
readonly isEmpty = computed(() => this.data().length === 0);
readonly hasError = computed(() => this.error() !== null);

// Effect for side effects
constructor() {
  effect(() => {
    if (this.hasError()) {
      console.error('Error state:', this.error());
    }
  });
}
```

**Async operation with loading pattern:**

```typescript
loadData(): void {
  this.loading.set(true);
  this.error.set(null);

  this.apiService.getData().pipe(
    finalize(() => this.loading.set(false)),
    takeUntilDestroyed()
  ).subscribe({
    next: (result) => this.data.set(result),
    error: (err) => this.error.set('Failed to load data')
  });
}
```

### 7.3 Subscription Cleanup

**Every** RxJS subscription is cleaned up using:

- `takeUntilDestroyed()` from `@angular/core/rxjs-interop` — primary mechanism
- Angular 22's automatic cleanup in `async` pipe (used in templates with `| async`)

---

## 8. Reusable UI Component Infrastructure

### 8.1 Component Catalog

#### GenericCrudComponent (`shared/components/generic-crud/`)

A fully reusable CRUD management component that powers all admin management pages (rooms, room types, staff, amenities, menu items).

| Feature | Implementation |
|---------|---------------|
| **Configuration-driven** | `CrudConfig<T>` interface defines columns, form fields, API methods, filters |
| **Sorting** | `MatSort` — any column sortable, toggle asc/desc |
| **Pagination** | `MatPaginator` — configurable page size, total count |
| **Search** | Text input with **300ms debounce** using `debounceTime` + `distinctUntilChanged` |
| **Multi-filter** | Dropdown/select filters configurable via `FilterDef[]` |
| **Add/Edit Modal** | `CrudModalComponent` — dynamic form fields based on config |
| **Deletion Guard** | Confirmation dialog on deactivation/deletion |
| **View Toggle** | Table view ↔ `CardsViewComponent` toggle |
| **Empty State** | Custom empty state display when no data |

**Data flow:**

```
PageComponent provides CrudConfig<T>
  → GenericCrudComponent renders table
  → User clicks Add → MatDialog opens CrudModalComponent with dynamic form
  → User submits → GenericCrudComponent emits form value
  → PageComponent handles save → calls API service → refreshes data signal
```

#### CrudModalComponent (`shared/components/generic-crud/crud-modal/`)

Dynamic form modal supporting 10+ field types:

| Field Type | Control | Config Options |
|-----------|---------|---------------|
| `text` | `<input>` | minLength, maxLength, placeholder |
| `number` | `<input type="number">` | min, max, step |
| `email` | `<input type="email">` | — |
| `password` | `<input type="password">` | Pattern validation |
| `textarea` | `<textarea>` | rows, maxLength |
| `date` | `<input type="date">` | min, max |
| `url` | `<input type="url">` | Pattern `/^https?:\/\/.+/` |
| `select` | `mat-select` | Options array |
| `toggle` | `mat-slide-toggle` | — |
| `keyValueList` | Dynamic list of key-value pairs | Key pattern validation |
| `imageUrlList` | Dynamic list of image URLs | URL validation per entry |

#### TaskDashboardComponent (`shared/components/task-dashboard/`)

Reusable task management component used by Kitchen, Housekeeping, and Maintenance dashboards.

| Feature | Implementation |
|---------|---------------|
| **Status Summary Cards** | `forkJoin` of count queries for each status (Pending, InProgress, Completed) |
| **Filterable Table** | Status dropdown filter with `MatTable` |
| **Status Transitions** | Buttons for status advancement (e.g., Claim → Start → Complete) |
| **Detail Dialog** | `TaskDetailDialogComponent` shows full task info + status transition buttons |
| **Real-Time Refresh** | SignalR-driven via `refreshTrigger` signal |
| **Configuration-driven** | `TaskDashboardConfig` interface customizes columns, labels, API endpoints |

#### Supporting Components

| Component | Purpose |
|-----------|---------|
| `ConfirmDialogComponent` | Generic confirmation dialog with message + confirm/cancel buttons |
| `AlertComponent` | Dismissible alert for success/error messages |
| `NotificationSnackbarComponent` | Styled Material snackbar for real-time notifications |
| `ProfileComponent` | View/edit profile + change password form with cross-field validation |
| `CustomCursorComponent` | Mouse-tracking custom cursor with interactive element detection |

### 8.2 Custom Cursor Feature

**File:** `shared/components/custom-cursor/`

| Feature | Detail |
|---------|--------|
| **Rendering** | SVG circle element following mouse position |
| **Animation** | `requestAnimationFrame` for 60fps smooth tracking |
| **Interactive elements** | Detects `a`, `button`, `input`, `select` tags → enlarges cursor |
| **Input fields** | Cursor becomes oval shape on inputs/selects |
| **Performance** | Uses transform/translate for GPU-accelerated rendering |

---

## 9. Responsive Design & Theming

### 9.1 Dark Luxury Theme — "AETHERIS"

The application uses a custom dark theme with a luxury resort aesthetic.

#### Design Tokens (`styles/theme/_colors.scss`)

| Token | Value | Usage |
|-------|-------|-------|
| `--bg-primary` | `#131411` | Main background (near-black olive) |
| `--bg-secondary` | `#1a1b19` | Card/surface backgrounds |
| `--gold-primary` | `#e4c285` | Primary accent, interactive elements |
| `--gold-hover` | `#d4b275` | Hover state for gold elements |
| `--text-primary` | `#e8e6e3` | Primary text |
| `--glass-bg` | `rgba(255, 255, 255, 0.05)` | Glassmorphism panels |
| `--glass-border` | `rgba(255, 255, 255, 0.1)` | Glass panel borders |

#### Typography (`styles/theme/_typography.scss`)

| Element | Font | Usage |
|---------|------|-------|
| Headlines | `Playfair Display` (serif) | Titles, headings, luxury feel |
| Body | `Manrope` (sans-serif) | Paragraphs, labels, form text |
| Font sizes | 14px body, scale to 48px h1 | Consistent type scale |

#### Glassmorphism (`styles/theme/_glassmorphism.scss`)

```scss
.glass-panel {
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
}
```

Used on:
- Auth page (login/register forms with cinematic background)
- Admin dashboard cards
- Operation shell sidebars

### 9.2 Responsive Breakpoints

| Context | Breakpoint | Behavior |
|---------|-----------|----------|
| **Public shell** | 768px | Navigation collapses to hamburger menu |
| **Admin/Operations shells** | 1024px | Side nav collapses to overlaid drawer |
| **Booking wizard** | 599px | Stepper becomes vertical, single-column layout |
| **CRUD tables** | Various | Columns hide based on priority, horizontal scroll fallback |

**Shell mobile detection:**

```typescript
protected readonly isMobile = toSignal(
  inject(BreakpointObserver).observe('(max-width: 1024px)').pipe(
    map(result => result.matches)
  )
);
```

### 9.3 Shell Components

| Shell | Layout | Mobile Behavior |
|-------|--------|----------------|
| `PublicShellComponent` | Header + router-outlet + footer | Hamburger menu, full-screen drawer |
| `AdminShellComponent` | Sidebar + top-bar + router-outlet | Sidnav → overlaid drawer |
| `FrontDeskShellComponent` | Sidebar + router-outlet | Sidnav → overlaid drawer |
| `KitchenShellComponent` | Sidebar + router-outlet | Sidnav → overlaid drawer |
| `HousekeepingShellComponent` | Sidebar + router-outlet | Sidnav → overlaid drawer |
| `MaintenanceShellComponent` | Sidebar + router-outlet | Sidnav → overlaid drawer |
| `UserShellComponent` | Sidebar + router-outlet | Sidnav → overlaid drawer |

---

## 10. Admin Feature Suite

### 10.1 Admin Management Pages (CRUD)

All management pages use the `GenericCrudComponent` with role-specific configuration:

| Page | Entity | Route | Key Fields |
|------|--------|-------|-----------|
| **Room Management** | Room | `/operations/admin/management/room` | RoomNumber, RoomTypeId, Floor, Status, Price |
| **Room Type Management** | RoomType | `/operations/admin/management/room-type` | Name, Description, BasePrice, Capacity, Amenities |
| **Staff Management** | Staff | `/operations/admin/management/staff` | FirstName, LastName, Email, Role, Phone |
| **Amenities Management** | Amenity | `/operations/admin/management/amenities` | Name, Description, Icon, Category |
| **Menu Management** | MenuItem | `/operations/admin/management/menu` | Name, Description, Price, Category, ImageUrl |

### 10.2 Admin Oversight Pages

| Page | Route | Technology | Features |
|------|-------|-----------|----------|
| **Analytics Dashboard** | `/operations/admin/oversight/analytics` | **ECharts** via `ngx-echarts` | Bar charts, line charts, KPI cards |
| **Audit Logs** | `/operations/admin/oversight/auditlogs` | Custom log viewer | Entity/action filtering, date range, user filter |
| **Billing & Receipts** | `/operations/admin/oversight/billings-receipts` | Receipt table + PDF | PDF invoice download via `/billing/{id}/folio/pdf` |
| **Feedback Moderation** | `/operations/admin/oversight/feedback` | Moderation interface | Approve/reject feedback, view guest ratings |

### 10.3 Analytics KPIs (via ECharts)

The analytics dashboard computes key hotel metrics from the backend:

| KPI | Description |
|-----|-------------|
| **Occupancy Rate (%)** | Percentage of occupied rooms |
| **Average Daily Rate (ADR)** | Average revenue per occupied room |
| **RevPAR** | Revenue Per Available Room |
| **Total Revenue** | Gross revenue across all bookings |
| **Gross Turnover** | Total business turnover |
| **Average Length of Stay** | Average nights per booking |
| **Cancellation Rate** | Percentage of cancelled bookings |
| **Guest Satisfaction Score** | Average feedback rating |
| **Average Housekeeping Turnaround** | Minutes to clean a room after checkout |
| **Non-Room Expenditure** | Revenue from room service, amenities, etc. |

---

## 11. Route Architecture Complete Map

### 11.1 Full Route Table

```
Route Tree:

'' (PublicShellComponent)
├── /auth                          → AuthRedirectGuard
├── /home                          → HomeComponent
├── /rooms                         → RoomCatalogueComponent
├── /rooms/:id                     → RoomDetailComponent
├── /experiences                   → ExperiencesComponent
├── /availability                  → AvailabilityComponent
├── /privacy                       → PrivacyComponent
├── /terms                         → TermsComponent
├── /contact                       → ContactComponent
├── /menu                          → redirect → /experiences
├── /amenities                     → redirect → /experiences
├── /error/:status                 → ErrorPageComponent
├── /error                         → redirect → /error/500
└── /**                            → redirect → /error/404

/operations (no-shell, lazy children)
├── /operations/admin/*            → AdminShellComponent → adminGuard
│   ├── dashboard                  → AdminDashboardComponent
│   ├── management/room            → RoomManagementComponent
│   ├── management/room-type       → RoomTypeManagementComponent
│   ├── management/staff           → StaffManagementComponent
│   ├── management/amenities       → AmenityManagementComponent
│   ├── management/menu            → MenuManagementComponent
│   ├── oversight/analytics        → AnalyticsComponent
│   ├── oversight/auditlogs        → AuditLogComponent
│   ├── oversight/billings-receipts → BillingComponent
│   ├── oversight/feedback         → FeedbackComponent
│   └── profile                    → ProfileComponent
├── /operations/front-desk/*       → FrontDeskShellComponent → frontDeskGuard
│   ├── dashboard                  → FrontDeskDashboardComponent
│   ├── new-booking                → NewBookingComponent
│   ├── guest/:email               → GuestDetailsComponent
│   └── profile                    → ProfileComponent
├── /operations/kitchen/*          → KitchenShellComponent → kitchenGuard
│   ├── dashboard                  → KitchenDashboardComponent
│   ├── menu-items                 → MenuItemsComponent
│   └── profile                    → ProfileComponent
├── /operations/housekeeping/*     → HousekeepingShellComponent → housekeepingGuard
│   ├── dashboard                  → HousekeepingDashboardComponent
│   └── profile                    → ProfileComponent
├── /operations/maintenance/*      → MaintenanceShellComponent → maintenanceGuard
│   ├── dashboard                  → MaintenanceDashboardComponent
│   └── profile                    → ProfileComponent
└── /**                            → redirect → /error/404

/user/*                            → UserShellComponent → customerGuard
├── /user/dashboard                → UserDashboardComponent
├── /user/bookings                 → UserBookingsComponent
├── /user/room-service             → RoomServiceComponent
└── /user/profile                  → ProfileComponent
```

### 11.2 Lazy Loading

All operation routes and user routes use **Angular lazy loading** (`loadComponent`):

```typescript
{
  path: 'operations/admin',
  loadComponent: () => import('./features/admin/admin-shell/admin-shell.component')
    .then(m => m.AdminShellComponent),
  canActivate: [adminGuard],
  canMatch: [adminGuard],
  // ...
}
```

This ensures the initial bundle only includes the public shell and auth code — admin, operations, and user components load on demand.

---

## 12. Tech Stack & Dependencies

### 12.1 Core Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `@angular/core` | ^22.0.0 | Angular framework |
| `@angular/common` | ^22.0.0 | Common directives, pipes |
| `@angular/router` | ^22.0.0 | Client-side routing |
| `@angular/forms` | ^22.0.0 | Reactive forms |
| `@angular/material` | ^22.0.2 | Material Design component library |
| `@angular/cdk` | ^22.0.2 | Component Dev Kit |
| `@angular/animations` | ^22.0.4 | Animation system |
| `rxjs` | ^7.8.0 | Reactive Extensions |
| `@microsoft/signalr` | ^10.0.0 | Real-time WebSocket communication |
| `echarts` | ^6.1.0 | Charting library (analytics) |
| `ngx-echarts` | ^22.0.0 | ECharts Angular wrapper |
| `typescript` | ~6.0.2 | TypeScript compiler |

### 12.2 Dev Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `vitest` | ^4.0.8 | Unit test runner |
| `jsdom` | ^28.0.0 | DOM environment for tests |
| `prettier` | ^3.8.1 | Code formatter |

### 12.3 Angular Material Module Usage

| Module | Component(s) |
|--------|-------------|
| `MatCardModule` | Cards for dashboards, forms, detail views |
| `MatButtonModule` | All buttons |
| `MatIconModule` | Material icons |
| `MatFormFieldModule` | Form field wrappers |
| `MatInputModule` | Text/number/email/password inputs |
| `MatProgressSpinnerModule` | Loading spinners |
| `MatProgressBarModule` | Progress bars |
| `MatSidenavModule` | Side navigation (shells) |
| `MatToolbarModule` | Top toolbars |
| `MatListModule` | Navigation lists |
| `MatMenuModule` | Dropdown menus |
| `MatDividerModule` | Dividers |
| `MatTableModule` | Data tables (CRUD, tasks) |
| `MatSortModule` | Table column sorting |
| `MatPaginatorModule` | Table pagination |
| `MatDialogModule` | Modals (CRUD, confirm, task details) |
| `MatSnackBarModule` | Notifications |
| `MatSlideToggleModule` | Boolean toggles |
| `MatSelectModule` | Dropdown selects |
| `MatStepperModule` | Booking wizard step form |
| `MatDatepickerModule` | Date picker (booking) |
| `MatNativeDateModule` | Native date adapter |
| `MatCheckboxModule` | Checkboxes |
| `MatTooltipModule` | Tooltips |
| `LayoutModule` | `BreakpointObserver` (responsive) |

---

## 13. Backend API Integration Map

### 13.1 Core Authentication

| Method | Endpoint | Service Method | Request Body | Response |
|--------|----------|---------------|-------------|----------|
| POST | `/auth/login` | `login(LoginRequestDTO)` | `{ email, password }` | `{ token, role, firstName, lastName }` |
| POST | `/auth/register` | `register(RegisterRequestDTO)` | `{ email, password, firstName, lastName }` | `{ token, role, firstName, lastName }` |
| GET | `/auth/me` | `getCurrentUser()` | — | `{ id, email, firstName, lastName, role, isActive, claims }` |
| PUT | `/auth/me` | `updateProfile(UpdateProfileRequest)` | `{ firstName, lastName, email }` | Updated profile |
| POST | `/auth/change-password` | `changePassword(ChangePasswordRequest)` | `{ currentPassword, newPassword }` | Success confirmation |

### 13.2 Admin Management

| Method | Endpoint | Service |
|--------|----------|---------|
| GET | `/rooms` | `RoomApiService.getAll()` |
| POST | `/rooms` | `RoomApiService.create()` |
| PATCH | `/rooms/{id}` | `RoomApiService.update()` |
| GET | `/rooms/status` | `RoomApiService.getRoomStatuses()` |
| GET | `/room-types` | `RoomTypeApiService.getAll()` |
| POST | `/room-types` | `RoomTypeApiService.create()` |
| PATCH | `/room-types/{id}` | `RoomTypeApiService.update()` |
| GET | `/room-types/{id}` | `RoomTypeApiService.getById()` |
| GET | `/room-types/availability` | `RoomTypeApiService.getAvailability()` |
| GET | `/staff` | `StaffApiService.getAll()` |
| POST | `/staff` | `StaffApiService.create()` |
| PATCH | `/staff/{id}` | `StaffApiService.update()` |
| GET | `/amenities` | `AmenityApiService.getAll()` |
| POST | `/amenities` | `AmenityApiService.create()` |
| PUT | `/amenities/{id}` | `AmenityApiService.update()` |
| GET | `/menu-items` | `MenuItemApiService.getAll()` (admin) |
| POST | `/menu-items` | `MenuItemApiService.create()` |
| PUT | `/menu-items/{id}` | `MenuItemApiService.update()` |
| PATCH | `/menu-items/{id}` | `MenuItemApiService.patch()` |
| GET | `/bookings` | `BookingApiService.getAll()` |
| POST | `/bookings` | `BookingApiService.create()` |
| DELETE | `/bookings/{id}/cancel` | `BookingApiService.cancel()` |
| POST | `/bookings/{id}/checkin` | `BookingApiService.checkIn()` |
| POST | `/bookings/{id}/extend-stay` | `BookingApiService.extendStay()` |
| POST | `/bookings/{id}/checkout` | `BookingApiService.checkOut()` |
| PATCH | `/bookings/{id}` | `BookingApiService.update()` |

### 13.3 Analytics & Operations

| Method | Endpoint | Service |
|--------|----------|---------|
| GET | `/analytics` | `AnalyticsApiService.getDashboard()` |
| GET | `/auditlogs` | `AuditLogApiService.getAll()` |
| GET | `/billing/receipts` | `BillingApiService.getReceipts()` |
| GET | `/billing/{id}/folio/pdf` | `BillingApiService.downloadFolioPdf()` |
| GET | `/feedback` | `FeedbackApiService.getAll()` |
| PATCH | `/feedback/{id}/moderate` | `FeedbackApiService.moderate()` |
| GET | `/guests?search=` | `GuestApiService.search()` |

### 13.4 Operations Task Management

| Method | Endpoint | Service |
|--------|----------|---------|
| GET | `/housekeeping` | `HousekeepingApiService.getAll()` |
| POST | `/housekeeping` | `HousekeepingApiService.create()` |
| PATCH | `/housekeeping/{id}` | `HousekeepingApiService.update()` |
| POST | `/housekeeping/trigger/{roomId}` | `HousekeepingApiService.trigger()` |
| POST | `/housekeeping/internal` | `HousekeepingApiService.createInternal()` |
| GET | `/maintenance` | `MaintenanceApiService.getAll()` |
| POST | `/maintenance` | `MaintenanceApiService.create()` |
| PATCH | `/maintenance/{id}` | `MaintenanceApiService.update()` |
| POST | `/maintenance/trigger/{roomId}` | `MaintenanceApiService.trigger()` |
| POST | `/maintenance/internal` | `MaintenanceApiService.createInternal()` |
| GET | `/orders` | `OrderApiService.getAll()` |
| POST | `/orders` | `OrderApiService.create()` |
| PATCH | `/orders/{id}` | `OrderApiService.update()` |

---

## Feature Inventory Summary

| Category | Count | Details |
|----------|-------|---------|
| **Route Guards** | 7 | 6 role-based + 1 auth-redirect |
| **HTTP Interceptors** | 3 | Auth, Error Page, Idempotency |
| **Error Handlers** | 2 | Global Error Handler + Error Page Interceptor |
| **Role Types** | 6 | Admin, RegisteredUser, FrontDesk, Kitchen, Housekeeping, Maintenance |
| **API Services** | 20+ | Core auth + admin CRUD + operations + user services |
| **Model Interfaces** | 30+ | Request/Response DTOs, domain models, config interfaces |
| **Feature Pages** | 25+ | Across public, admin, operations, user modules |
| **Reusable Components** | 10+ | CRUD, TaskBoard, Alert, ConfirmDialog, CustomCursor, Profile, etc. |
| **ECharts KPIs** | 10 | Occupancy, ADR, RevPAR, Revenue, etc. |
| **Angular Material Modules** | 24 | Full Material component coverage |
| **Form Validators** | 10+ | Built-in + custom cross-field + regex patterns |
| **Real-Time Hubs** | 1 | SignalR notification hub with auto-reconnect |

---

*End of Report*
