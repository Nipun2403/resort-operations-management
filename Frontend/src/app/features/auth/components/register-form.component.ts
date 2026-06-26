import { Component, Input, Output, EventEmitter, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RegisterRequestDTO } from '../../../core/models/auth.models';

@Component({
  selector: 'app-register-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  template: `
    <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" novalidate>
      <mat-form-field appearance="fill" class="full-width">
        <mat-label>First Name</mat-label>
        <input
          matInput
          type="text"
          formControlName="firstName"
          aria-describedby="first-name-error"
          [readonly]="loading"
          required
        />
        @if (registerForm.controls.firstName.hasError('required')) {
          <mat-error id="first-name-error">First Name is required.</mat-error>
        }
        @if (registerForm.controls.firstName.hasError('pattern')) {
          <mat-error id="first-name-error">
            First name must be between 2 and 50 characters and contain only letters.
          </mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="fill" class="full-width">
        <mat-label>Last Name</mat-label>
        <input
          matInput
          type="text"
          formControlName="lastName"
          aria-describedby="last-name-error"
          [readonly]="loading"
          required
        />
        @if (registerForm.controls.lastName.hasError('required')) {
          <mat-error id="last-name-error">Last Name is required.</mat-error>
        }
        @if (registerForm.controls.lastName.hasError('pattern')) {
          <mat-error id="last-name-error">
            Last name must be between 2 and 50 characters and contain only letters.
          </mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="fill" class="full-width">
        <mat-label>Email Address</mat-label>
        <input
          matInput
          type="email"
          formControlName="email"
          aria-describedby="register-email-error"
          [readonly]="loading"
          required
        />
        @if (registerForm.controls.email.hasError('required')) {
          <mat-error id="register-email-error">Email is required.</mat-error>
        }
        @if (registerForm.controls.email.hasError('pattern')) {
          <mat-error id="register-email-error">Please enter a valid email address.</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="fill" class="full-width">
        <mat-label>Password</mat-label>
        <input
          matInput
          type="password"
          formControlName="password"
          aria-describedby="register-password-error"
          [readonly]="loading"
          required
        />
        @if (registerForm.controls.password.hasError('required')) {
          <mat-error id="register-password-error">Password is required.</mat-error>
        }
        @if (registerForm.controls.password.hasError('pattern')) {
          <mat-error id="register-password-error">
            Password must be at least 8 characters long and contain both letters and numbers.
          </mat-error>
        }
      </mat-form-field>

      <button
        mat-raised-button
        color="primary"
        type="submit"
        class="full-width submit-btn"
        [disabled]="loading"
      >
        @if (!loading) {
          <span>Submit</span>
        } @else {
          <mat-progress-spinner
            mode="indeterminate"
            [diameter]="20"
          ></mat-progress-spinner>
        }
      </button>
    </form>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    .submit-btn {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 48px;
    }
  `]
})
export class RegisterFormComponent {
  @Input() loading = false;
  @Output() submitted = new EventEmitter<RegisterRequestDTO>();

  private el = inject(ElementRef);

  registerForm = new FormGroup({
    email: new FormControl<string>('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/)
      ]
    }),
    password: new FormControl<string>('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/)
      ]
    }),
    firstName: new FormControl<string>('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.pattern(/^[a-zA-ZÀ-ž\s\-']{2,50}$/)
      ]
    }),
    lastName: new FormControl<string>('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.pattern(/^[a-zA-ZÀ-ž\s\-']{2,50}$/)
      ]
    })
  });

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      setTimeout(() => {
        const firstInvalid = this.el.nativeElement.querySelector('.ng-invalid input');
        if (firstInvalid) {
          (firstInvalid as HTMLElement).focus();
        }
      });
      return;
    }

    this.submitted.emit(this.registerForm.getRawValue());
  }
}
