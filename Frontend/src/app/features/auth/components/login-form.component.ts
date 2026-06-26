import { Component, Input, Output, EventEmitter, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoginRequestDTO } from '../../../core/models/auth.models';

@Component({
  selector: 'app-login-form',
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
    <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" novalidate>
      <mat-form-field appearance="fill" class="full-width">
        <mat-label>Email Address</mat-label>
        <input
          matInput
          type="email"
          formControlName="email"
          aria-describedby="email-error"
          [readonly]="loading"
          required
        />
        @if (loginForm.controls.email.hasError('required')) {
          <mat-error id="email-error">Email is required.</mat-error>
        }
        @if (loginForm.controls.email.hasError('pattern')) {
          <mat-error id="email-error">Please enter a valid email address.</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="fill" class="full-width">
        <mat-label>Password</mat-label>
        <input
          matInput
          type="password"
          formControlName="password"
          aria-describedby="password-error"
          [readonly]="loading"
          required
        />
        @if (loginForm.controls.password.hasError('required')) {
          <mat-error id="password-error">Password is required.</mat-error>
        }
        @if (loginForm.controls.password.hasError('pattern')) {
          <mat-error id="password-error">Password must be at least 8 characters long and contain both letters and numbers.</mat-error>
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
export class LoginFormComponent {
  @Input() loading = false;
  @Output() submitted = new EventEmitter<LoginRequestDTO>();

  private el = inject(ElementRef);

  loginForm = new FormGroup({
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
    })
  });

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      setTimeout(() => {
        const firstInvalid = this.el.nativeElement.querySelector('.ng-invalid input');
        if (firstInvalid) {
          (firstInvalid as HTMLElement).focus();
        }
      });
      return;
    }

    this.submitted.emit(this.loginForm.getRawValue());
  }
}
