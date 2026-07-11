# Patch Specsheet: Admin Shell – Profile Navigation & User Display Update

### 1. Purpose
Modify the already‑built `AdminShellComponent` and `AuthService` to:

- Remove the “Profile” link from the sidebar.
- Make the top‑right profile icon the **only** way to navigate to `/operations/admin/profile`.
- Display the user’s **first and last name** (extracted from the JWT token) instead of the hardcoded `Admin User` text.
- On mobile screens, hide the user’s name and show only the profile icon.

No new components or routes are created. Only existing files are patched.

---

### 2. Files to Modify

| File | Action |
|------|--------|
| `src/app/core/services/auth.service.ts` | Add `fullName` signal (derived from JWT payload). |
| `src/app/core/utils/jwt-decode.ts` | Ensure decoded payload exposes `firstName` and `lastName` claims (if not already). |
| `src/app/features/admin/admin-shell.component.ts` | Replace `userDisplayName` signal with `authService.fullName`. |
| `src/app/features/admin/admin-shell.component.html` | Remove sidebar profile link, adjust toolbar markup for mobile/desktop name visibility. |

---

### 3. Changes to `AuthService`

**Add a `fullName` signal** that is a computed concatenation of the JWT claims `firstName` and `lastName`.  
Assumption: the JWT payload contains `firstName` and `lastName` (or `given_name`/`family_name` – we'll standardise on `firstName`, `lastName`).  
If the token is not present or decoding fails, fall back to `'Admin'` (the role).

Implementation details:
- After `handleLogin(token)` stores the token, the service should decode the payload and set a private `_decodedToken = signal<TokenPayload | null>(null)`.
- `fullName = computed(() => { const t = this._decodedToken(); return t ? `${t.firstName} ${t.lastName}` : ''; })`.
- The `handleLogin` method must call `this._decodeAndStore(token)`.
- Ensure the `jwt-decode` utility returns a typed object with at least `firstName: string`, `lastName: string`, `role: string`, `exp: number`.

**No changes to `login()`, `logout()`, or other methods.**

---

### 4. Changes to `jwt-decode.ts`

If the current utility only extracts generic `any`, update it to return a typed `JwtPayload` interface:
```ts
export interface JwtPayload {
  exp: number;
  role: string;
  firstName: string;
  lastName: string;
  // any other fields
}
```
The function `jwtDecode(token: string): JwtPayload` should parse the base64 payload and return this interface.

---

### 5. Changes to `AdminShellComponent` (TypeScript)

Replace:
```ts
userDisplayName = signal('Admin User');  // old
```
With:
```ts
private auth = inject(AuthService);
userDisplayName = this.auth.fullName; // computed signal, read-only
```
Remove the old `signal` import if no longer needed.

---

### 6. Changes to `AdminShellComponent` (HTML Template)

**Sidebar**:  
Delete the entire “Profile” navigation item block:
```html
<!-- REMOVE THIS -->
<a mat-list-item routerLink="/operations/admin/profile" routerLinkActive="active" (click)="onNavClick()">
  <mat-icon matListItemIcon>account_circle</mat-icon>
  <span matListItemTitle>Profile</span>
</a>
```
(The `<mat-divider>` immediately above it should also be removed to keep the visual grouping correct.)

**Top Toolbar**:  
Replace the user name display:
```html
<span>{{ userDisplayName() }}</span>
```
With a conditional that hides the text on mobile:
```html
@if (!isMobile()) {
  <span>{{ userDisplayName() }}</span>
}
```
This ensures that on screens ≤768px, only the profile icon button (with `mat-icon`) is visible. The icon button already has `aria-label="Open user menu"`, which is sufficient for mobile.

The profile icon button remains unchanged; its `[matMenuTriggerFor]` still opens the menu containing “Profile” and “Logout”. The “Profile” menu item already uses `routerLink="/operations/admin/profile"`, so it remains the sole navigation entry.

---

### 7. Verification / Self‑Review Checklist (Agent must test)

- [ ] Sidebar no longer contains a “Profile” link.
- [ ] Clicking the top‑right user icon opens a menu with “Profile” and “Logout”.
- [ ] Selecting “Profile” navigates to `/operations/admin/profile` (placeholder page loads).
- [ ] If logged in, the top bar displays the user’s actual first and last name (e.g. “Jane Smith”).
- [ ] If token doesn’t contain name fields, fallback to empty string or ‘Admin’ gracefully.
- [ ] Resize to mobile width: user name disappears, only profile icon remains.
- [ ] On mobile, tapping the icon still opens the menu and “Profile” link works.
- [ ] No regression: all other sidebar links still navigate correctly.
- [ ] No new imports or dependencies beyond what already existed.

---

### 8. Integration Notes

- This patch does **not** create any new components or routes.
- It relies on the JWT token containing `firstName` and `lastName`. If the backend uses different claim names (e.g., `given_name`, `family_name`), the `jwtDecode` function should map them to `firstName`/`lastName`. The spec assumes the standard `firstName`/`lastName` as used in the registration DTO.
- The `AuthService.fullName` signal becomes available for all future pages that need to display the user’s name.

---

### 9. Testing Hints

- Mock `AuthService.fullName` in shell component test to verify template rendering.
- Test responsive behavior by toggling `isMobile` signal in test.
- Ensure the sidebar mat-divider removal doesn’t break the visual groupings (Management / Oversight). It’s safe because the divider after Oversight already provides separation.

---
