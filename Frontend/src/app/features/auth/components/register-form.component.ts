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
  templateUrl: './register-form.component.html',
  styleUrls: ['./register-form.component.scss']
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
