import { Component, signal, inject, DestroyRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/services/auth.service';
import { AuthApiService } from '../../core/services/auth-api.service';
import { LoginRequestDTO, RegisterRequestDTO } from '../../core/models/auth.models';
import { AlertComponent } from './components/alert.component';
import { LoginFormComponent } from './components/login-form.component';
import { RegisterFormComponent } from './components/register-form.component';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    AlertComponent,
    LoginFormComponent,
    RegisterFormComponent
  ],
  templateUrl: './auth-page.component.html',
  styleUrls: ['./auth-page.component.scss']
})
export class AuthPageComponent implements OnInit {
  private authService = inject(AuthService);
  private authApi = inject(AuthApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);

  private returnUrl: string | null = null;

  ngOnInit(): void {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      this.returnUrl = params['returnUrl'] || null;
    });
  }

  isLoginMode = signal(true);
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  onLogin(credentials: LoginRequestDTO): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.authApi.login(credentials)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (response) => {
          this.authService.handleLogin(response.token);
          
          console.log('Token:', response.token);
          console.log('Role:', this.authService.role());

          // TODO: optionally trigger subtle sound effect for accessibility or user feedback
          this.successMessage.set('Login successful! Redirecting...');
          
          setTimeout(() => {
            if (this.returnUrl && this.returnUrl.startsWith('/')) {
              this.router.navigateByUrl(this.returnUrl);
            } else {
              let targetRoute = '/user/dashboard';
              const role = this.authService.role();
              switch (role) {
                case 'RegisteredUser':
                  targetRoute = '/user/dashboard';
                  break;
                case 'Admin':
                  targetRoute = '/operations/admin/dashboard';
                  break;
                case 'FrontDesk':
                  targetRoute = '/operations/front-desk/dashboard';
                  break;
                case 'Kitchen':
                  targetRoute = '/operations/kitchen/dashboard';
                  break;
                case 'Housekeeping':
                  targetRoute = '/operations/housekeeping/dashboard';
                  break;
                case 'Maintenance':
                  targetRoute = '/operations/maintenance/dashboard';
                  break;
                default:
                  targetRoute = '/user/dashboard';
                  break;
              }
              this.router.navigate([targetRoute]);
            }
          }, 800);
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Invalid credentials.');
        }
      });
  }

  onRegister(data: RegisterRequestDTO): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.authApi.register(data)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: () => {
          // TODO: optionally trigger subtle sound effect for accessibility or user feedback
          this.successMessage.set('Registration successful! Please log in.');
          this.isLoginMode.set(true);
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Registration failed.');
        }
      });
  }
}
