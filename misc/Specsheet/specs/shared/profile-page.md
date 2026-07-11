# Specsheet: Shared Profile Page (All Roles)

## 1. Purpose
- Create a single reusable `ProfileComponent` that works for all roles (Admin, FrontDesk, Kitchen, Housekeeping, Maintenance, RegisteredUser).
- The component automatically determines editability based on the logged‑in user’s role:
  - **Admin** and **RegisteredUser** can edit first name, last name, and email.
  - **All other staff** can only change their password.
- The component will be placed into every portal’s route, replacing existing placeholder profile pages.
- The user menu in every shell will link to the profile page.

## 2. New Component
**File:** `src/app/shared/components/profile/profile.component.ts`  
**Selector:** `app-profile`  
**Standalone:** `true`  
**Imports:** `CommonModule`, `ReactiveFormsModule`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatDividerModule`, `MatFormFieldModule`, `MatInputModule`, `MatProgressSpinnerModule`, `MatSnackBarModule`, `AlertComponent`, `AuthApiService`, `AuthService`, `DestroyRef`.

## 3. Component Logic (exact)

```typescript
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators, FormGroup, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { AuthService } from '../../../core/services/auth.service';
import { AlertComponent } from '../alert/alert.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatCardModule, MatButtonModule, MatIconModule, MatDividerModule,
    MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
    MatSnackBarModule, AlertComponent
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent {
  private authApi = inject(AuthApiService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);
  private destroyRef = inject(DestroyRef);

  // Profile data
  profile = signal<{ id: number; email: string; firstName: string; lastName: string; role: string; isActive: boolean } | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  // Editability
  canEditProfile = computed(() => {
    const role = this.authService.role();
    return role === 'Admin' || role === 'RegisteredUser';
  });

  // Profile edit form
  editMode = signal(false);
  profileForm = new FormGroup({
    firstName: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-ZÀ-ž\s\-']+$/)] }),
    lastName: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-ZÀ-ž\s\-']+$/)] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
  });
  profileSubmitting = signal(false);
  profileError = signal<string | null>(null);

  // Password change form
  passwordForm = new FormGroup({
    currentPassword: new FormControl('', { nonNullable: true, validators: Validators.required }),
    newPassword: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/)] }),
    confirmNewPassword: new FormControl('', { nonNullable: true, validators: Validators.required }),
  }, { validators: this.passwordsMatchValidator });
  passwordSubmitting = signal(false);
  passwordError = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchProfile();
  }

  fetchProfile(): void {
    this.loading.set(true);
    this.authApi.getMe().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (data: any) => {
        this.profile.set(data);
        // Pre-fill profile form
        this.profileForm.patchValue({
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email
        });
      },
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  // Profile Edit
  toggleEditMode(): void {
    this.editMode.set(!this.editMode());
    if (!this.editMode()) {
      // Reset form to current profile values
      const p = this.profile();
      if (p) {
        this.profileForm.patchValue({ firstName: p.firstName, lastName: p.lastName, email: p.email });
      }
    }
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    this.profileSubmitting.set(true);
    this.profileError.set(null);
    const dto = this.profileForm.getRawValue();
    this.authApi.updateProfile(dto).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.profileSubmitting.set(false))
    ).subscribe({
      next: () => {
        this.snackBar.open('Profile updated.', 'Close', { duration: 3000 });
        this.editMode.set(false);
        this.fetchProfile(); // refresh
      },
      error: (err: any) => this.profileError.set(this.extractErrorMessage(err))
    });
  }

  // Password Change
  changePassword(): void {
    if (this.passwordForm.invalid) return;
    this.passwordSubmitting.set(true);
    this.passwordError.set(null);
    const dto = this.passwordForm.getRawValue();
    this.authApi.changePassword(dto).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.passwordSubmitting.set(false))
    ).subscribe({
      next: () => {
        this.snackBar.open('Password changed.', 'Close', { duration: 3000 });
        this.passwordForm.reset();
      },
      error: (err: any) => this.passwordError.set(this.extractErrorMessage(err))
    });
  }

  private passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const newPwd = group.get('newPassword')?.value;
    const confirmPwd = group.get('confirmNewPassword')?.value;
    return newPwd === confirmPwd ? null : { passwordsMismatch: true };
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
```

## 4. Template (exact)

```html
<div class="profile-page">
  @if (loading()) {
    <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchProfile()">Retry</button>
    </app-alert>
  } @else if (profile()) {
    <mat-card class="profile-card">
      <mat-card-header>
        <mat-card-title>My Profile</mat-card-title>
        @if (canEditProfile() && !editMode()) {
          <button mat-icon-button (click)="toggleEditMode()" aria-label="Edit profile">
            <mat-icon>edit</mat-icon>
          </button>
        }
      </mat-card-header>
      <mat-card-content>
        @if (editMode() && canEditProfile()) {
          <form [formGroup]="profileForm" (ngSubmit)="saveProfile()">
            <mat-form-field appearance="outline">
              <mat-label>First Name</mat-label>
              <input matInput formControlName="firstName" />
              <mat-error *ngIf="profileForm.get('firstName')?.invalid">Required, min 2 letters.</mat-error>
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Last Name</mat-label>
              <input matInput formControlName="lastName" />
              <mat-error *ngIf="profileForm.get('lastName')?.invalid">Required, min 2 letters.</mat-error>
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Email</mat-label>
              <input matInput formControlName="email" type="email" />
              <mat-error *ngIf="profileForm.get('email')?.invalid">Valid email required.</mat-error>
            </mat-form-field>
            @if (profileError()) {
              <app-alert type="error" [message]="profileError()!" (closed)="profileError.set(null)"></app-alert>
            }
            <div class="form-actions">
              <button mat-button type="button" (click)="toggleEditMode()">Cancel</button>
              <button mat-raised-button color="primary" type="submit" [disabled]="profileForm.invalid || profileSubmitting()">
                @if (profileSubmitting()) { <mat-spinner diameter="20"></mat-spinner> }
                Save
              </button>
            </div>
          </form>
        } @else {
          <div class="info-grid">
            <p><strong>First Name:</strong> {{ profile()!.firstName }}</p>
            <p><strong>Last Name:</strong> {{ profile()!.lastName }}</p>
            <p><strong>Email:</strong> {{ profile()!.email }}</p>
            <p><strong>Role:</strong> {{ profile()!.role }}</p>
          </div>
        }

        <mat-divider></mat-divider>

        <!-- Change Password Section -->
        <h3>Change Password</h3>
        <form [formGroup]="passwordForm" (ngSubmit)="changePassword()">
          <mat-form-field appearance="outline">
            <mat-label>Current Password</mat-label>
            <input matInput type="password" formControlName="currentPassword" />
            <mat-error *ngIf="passwordForm.get('currentPassword')?.invalid">Required.</mat-error>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>New Password</mat-label>
            <input matInput type="password" formControlName="newPassword" />
            <mat-error *ngIf="passwordForm.get('newPassword')?.invalid">Min 8 characters, at least 1 letter and 1 digit.</mat-error>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Confirm New Password</mat-label>
            <input matInput type="password" formControlName="confirmNewPassword" />
            <mat-error *ngIf="passwordForm.get('confirmNewPassword')?.invalid">Required.</mat-error>
            <mat-error *ngIf="passwordForm.hasError('passwordsMismatch') && passwordForm.get('confirmNewPassword')?.touched">Passwords do not match.</mat-error>
          </mat-form-field>
          @if (passwordError()) {
            <app-alert type="error" [message]="passwordError()!" (closed)="passwordError.set(null)"></app-alert>
          }
          <button mat-raised-button color="primary" type="submit" [disabled]="passwordForm.invalid || passwordSubmitting()">
            @if (passwordSubmitting()) { <mat-spinner diameter="20"></mat-spinner> }
            Change Password
          </button>
        </form>
      </mat-card-content>
    </mat-card>
  }
</div>
```

## 5. API Service Additions
Add to `AuthApiService` (if not already present):

```typescript
updateProfile(dto: { firstName: string; lastName: string; email: string }): Observable<void> {
  return this.http.put<void>(`${this.baseUrl}/auth/me`, dto);
}

changePassword(dto: { currentPassword: string; newPassword: string; confirmNewPassword: string }): Observable<void> {
  return this.http.post<void>(`${this.baseUrl}/auth/change-password`, dto);
}
```

## 6. Route Configuration Updates

For each portal, update the route to load the shared `ProfileComponent`.

### 6.1 Admin
Already has `/operations/admin/profile`. Replace placeholder with:
```typescript
{
  path: 'profile',
  loadComponent: () => import('../../shared/components/profile/profile.component').then(m => m.ProfileComponent)
}
```

### 6.2 Front Desk
Add to its children:
```typescript
{
  path: 'profile',
  loadComponent: () => import('../../../shared/components/profile/profile.component').then(m => m.ProfileComponent)
}
```

### 6.3 Customer (User)
Already has `/user/profile`. Replace placeholder with the same lazy load.

### 6.4 Kitchen, Housekeeping, Maintenance
Add a profile route inside each shell's children (currently they don't have one). Ensure the guard is applied.

Example for Kitchen:
```typescript
{
  path: 'profile',
  loadComponent: () => import('../../../shared/components/profile/profile.component').then(m => m.ProfileComponent),
  canActivate: [kitchenGuard]
}
```

## 7. User Menu Updates
Ensure each shell's user menu includes a "Profile" menu item that navigates to the correct profile route.

- **Admin**: Already has Profile link in menu.
- **Front Desk**: Already has Profile link.
- **Kitchen**: Add `<button mat-menu-item routerLink="./profile">Profile</button>` inside the user menu.
- **Housekeeping**: Same.
- **Maintenance**: Same.
- **Customer**: Already has Profile link.

## 8. Styling (profile.component.scss)
```scss
.profile-card {
  max-width: 600px;
  margin: 0 auto;
}
.info-grid p {
  margin: 8px 0;
}
.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 16px;
}
```

## 9. Responsive Behaviour
- Card full width on mobile.
- Forms stack vertically.

## 10. Self‑Review Checklist
- [ ] Profile page loads for all roles and displays correct user data.
- [ ] Admin and RegisteredUser see an edit button; can edit name/email and save.
- [ ] Staff roles (FrontDesk, Kitchen, Housekeeping, Maintenance) cannot edit profile fields; only see read-only info.
- [ ] Password change works for all roles.
- [ ] Validations enforce required fields, password pattern, and password match.
- [ ] Snackbar confirms successful updates; errors shown inline.
- [ ] Navigation to profile works from user menu in each shell.
- [ ] Guards still protect the routes.
- [ ] No console errors.

## 11. Integration Notes
- The `AuthApiService` must be extended with the two new methods.
- The shared `ProfileComponent` replaces all existing placeholder profile components; remove them.
- The `canEditProfile` is derived from `AuthService.role()`, which decodes the JWT; ensure the role claim is present.
- The profile component is standalone and needs no configuration input; it adapts automatically to the user's role.

This single component now serves as the profile page for every portal, completing the user management functionality across the application.