# Patch Specsheet: Customer Shell – Remove Profile from Sidebar

## 1. Purpose
- Remove the “Profile” navigation link from the sidebar in the Customer Shell.
- The only way to access the Profile page is via the top‑right user icon dropdown (already contains “Profile” and “Logout” menu items).
- No other functionality is changed.

## 2. Files to Modify
- `src/app/features/user/user-shell.component.html`

## 3. Changes

### 3.1 Remove the sidebar Profile link
Delete the following block from the `<mat-nav-list>`:

```html
<a mat-list-item routerLink="/user/profile" routerLinkActive="active" (click)="onNavClick()">
  <mat-icon matListItemIcon>account_circle</mat-icon>
  <span matListItemTitle>Profile</span>
</a>
```

If there is a `<mat-divider>` immediately above this link (to separate it from the other items), delete that divider as well.

The remaining sidebar links are:
- Dashboard
- My Bookings
- Room Service

### 3.2 Verify top‑right menu
The top‑right user icon button with `[matMenuTriggerFor]="userMenu"` already contains “Profile” and “Logout” items. No changes are required there.

## 4. Self‑Review Checklist (for the agent)
- [ ] Sidebar no longer shows a “Profile” link.
- [ ] Clicking the top‑right user icon opens a menu with “Profile” and “Logout”.
- [ ] “Profile” menu item navigates to `/user/profile`.
- [ ] “Logout” menu item calls `logout()` and redirects to `/auth`.
- [ ] All other sidebar links still work correctly.
- [ ] No console errors, no broken layout.

## 5. Integration Notes
- This patch is purely cosmetic; no TypeScript logic is altered.
- The profile page remains accessible via its route `/user/profile`, but now only through the dropdown menu, matching the Admin Shell behaviour.