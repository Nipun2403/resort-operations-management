import { Component, inject, signal, computed, OnInit } from '@angular/core';
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
export class ProfileComponent implements OnInit {
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
  passwordSuccess = signal(false);

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
    this.passwordSuccess.set(false);
    const dto = this.passwordForm.getRawValue();
    this.authApi.changePassword(dto).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.passwordSubmitting.set(false))
    ).subscribe({
      next: () => {
        this.passwordSuccess.set(true);
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
