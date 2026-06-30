# /Frontend/src/app/app.config.ts

import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { authInterceptor } from './core/interceptors/auth.interceptor';
import { routes } from './app.routes';
import { provideEchartsCore } from 'ngx-echarts';
import * as echarts from 'echarts';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimationsAsync(),
    provideEchartsCore({ echarts }),
  ],
};


# /Frontend/src/app/app.html

<app-custom-cursor></app-custom-cursor>
<router-outlet></router-outlet>

# /Frontend/src/app/app.routes.ts

import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { AuthRedirectGuard } from './core/guards/auth-redirect.guard';
import { adminGuard } from './core/guards/admin.guard';
import { customerGuard } from './core/guards/customer.guard';
import { frontDeskGuard } from './core/guards/front-desk.guard';
import { kitchenGuard } from './core/guards/kitchen.guard';
import { housekeepingGuard } from './core/guards/housekeeping.guard';
import { maintenanceGuard } from './core/guards/maintenance.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () => import('./features/auth/auth-page.component')
      .then(m => m.AuthPageComponent),
    canActivate: [AuthRedirectGuard]
  },
  {
    path: 'operations/admin',
    canMatch: [adminGuard],
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/admin-shell.component')
      .then(m => m.AdminShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/admin/pages/dashboard.component')
          .then(m => m.DashboardComponent),
        data: { title: 'Dashboard' }
      },
      {
        path: 'management',
        children: [
          { path: 'room', loadComponent: () => import('./features/admin/pages/management/room-management.component').then(m => m.RoomManagementComponent), data: { title: 'Rooms' } },
          { path: 'room-type', loadComponent: () => import('./features/admin/pages/management/room-type-management.component').then(m => m.RoomTypeManagementComponent), data: { title: 'Room Types' } },
          { path: 'staff', loadComponent: () => import('./features/admin/pages/management/staff-management.component').then(m => m.StaffManagementComponent), data: { title: 'Staff' } },
          { path: 'amenities', loadComponent: () => import('./features/admin/pages/management/amenities-management.component').then(m => m.AmenitiesManagementComponent), data: { title: 'Amenities' } },
          { path: 'menu', loadComponent: () => import('./features/admin/pages/management/menu-management.component').then(m => m.MenuManagementComponent), data: { title: 'Menu Items' } },
        ]
      },
      {
        path: 'oversight',
        children: [
          { path: 'analytics', loadComponent: () => import('./features/admin/pages/oversight/analytics.component').then(m => m.AnalyticsComponent), data: { title: 'Analytics' } },
          { path: 'auditlogs', loadComponent: () => import('./features/admin/pages/oversight/audit-logs.component').then(m => m.AuditLogsComponent), data: { title: 'Audit Logs' } },
          { path: 'billings-receipts', loadComponent: () => import('./features/admin/pages/oversight/billing-receipts.component').then(m => m.BillingReceiptsComponent), data: { title: 'Billing & Receipts' } },
          { path: 'feedback', loadComponent: () => import('./features/admin/pages/oversight/feedback.component').then(m => m.FeedbackComponent), data: { title: 'Feedback' } },
        ]
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent),
        data: { title: 'Profile' }
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  },
  {
    path: 'user',
    canMatch: [customerGuard],
    canActivate: [customerGuard],
    loadComponent: () => import('./features/user/user-shell.component')
      .then(m => m.UserShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/user/pages/dashboard.component')
          .then(m => m.PlaceholderCustomerDashboardComponent)
      },
      {
        path: 'bookings',
        loadComponent: () => import('./features/user/pages/bookings.component')
          .then(m => m.BookingsComponent)
      },
      {
        path: 'room-service',
        loadComponent: () => import('./features/user/pages/room-service.component')
          .then(m => m.RoomServiceComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent)
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  },
  {
    path: 'operations/front-desk',
    canMatch: [frontDeskGuard],
    canActivate: [frontDeskGuard],
    loadComponent: () => import('./features/front-desk/front-desk-shell.component')
      .then(m => m.FrontDeskShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/front-desk/pages/dashboard.component')
          .then(m => m.PlaceholderDashboardComponent)
      },
      {
        path: 'new-booking',
        loadComponent: () => import('./features/front-desk/pages/new-booking.component')
          .then(m => m.FrontDeskBookingWizardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent)
      },
      {
        path: 'guest/:email',
        loadComponent: () => import('./features/front-desk/pages/guest-details.component')
          .then(m => m.GuestDetailsComponent),
        canActivate: [frontDeskGuard]
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  },
  {
    path: 'operations/kitchen',
    canMatch: [kitchenGuard],
    canActivate: [kitchenGuard],
    loadComponent: () => import('./features/kitchen/kitchen-shell.component')
      .then(m => m.KitchenShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/kitchen/pages/dashboard.component')
          .then(m => m.KitchenDashboardComponent)
      },
      {
        path: 'menu-items',
        loadComponent: () => import('./features/kitchen/pages/menu-items.component')
          .then(m => m.KitchenMenuItemsComponent),
        canActivate: [kitchenGuard]
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent),
        canActivate: [kitchenGuard]
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  },
  {
    path: 'operations/housekeeping',
    canMatch: [housekeepingGuard],
    canActivate: [housekeepingGuard],
    loadComponent: () => import('./features/housekeeping/housekeeping-shell.component')
      .then(m => m.HousekeepingShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/housekeeping/pages/dashboard.component')
          .then(m => m.HousekeepingDashboardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent),
        canActivate: [housekeepingGuard]
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  },
  {
    path: 'operations/maintenance',
    canMatch: [maintenanceGuard],
    canActivate: [maintenanceGuard],
    loadComponent: () => import('./features/maintenance/maintenance-shell.component')
      .then(m => m.MaintenanceShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/maintenance/pages/dashboard.component')
          .then(m => m.MaintenanceDashboardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent),
        canActivate: [maintenanceGuard]
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  },
  {
    path: '',
    loadComponent: () => import('./features/public/public-shell.component')
      .then(m => m.PublicShellComponent),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      {
        path: 'home',
        loadComponent: () => import('./features/public/pages/home.component')
          .then(m => m.HomeComponent)
      },
      {
        path: 'rooms',
        loadComponent: () => import('./features/public/pages/room-catalogue.component')
          .then(m => m.RoomCatalogueComponent)
      },
      {
        path: 'rooms/:id',
        loadComponent: () => import('./features/public/pages/room-detail.component')
          .then(m => m.RoomDetailComponent)
      },
      {
        path: 'experiences',
        loadComponent: () => import('./features/public/pages/experiences.component')
          .then(m => m.ExperiencesComponent)
      },
      { path: 'menu', redirectTo: 'experiences', pathMatch: 'full' },
      { path: 'amenities', redirectTo: 'experiences', pathMatch: 'full' },
      {
        path: 'availability',
        loadComponent: () => import('./features/public/pages/availability.component')
          .then(m => m.AvailabilityComponent)
      },
      { path: '**', redirectTo: 'home' }
    ]
  }
];




# /Frontend/src/app/app.scss



# /Frontend/src/app/app.spec.ts

import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });
});


# /Frontend/src/app/app.ts

import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationService } from './core/services/notification.service';
import { CustomCursorComponent } from './shared/components/custom-cursor/custom-cursor.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CustomCursorComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('Frontend');
  private readonly notificationService = inject(NotificationService);
}


# /Frontend/src/app/core/guards/admin.guard.ts

import { inject } from "@angular/core";
import { CanActivateFn, CanMatchFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

export const adminGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === "Admin") {
    return true;
  }
  return router.createUrlTree(["/auth"]);
};


# /Frontend/src/app/core/guards/auth-redirect.guard.ts

import { Injectable, inject } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthRedirectGuard {
  private authService = inject(AuthService);
  private router = inject(Router);

  canActivate(
    route?: import('@angular/router').ActivatedRouteSnapshot,
    state?: import('@angular/router').RouterStateSnapshot
  ): boolean | UrlTree {
    if (this.authService.isAuthenticated()) {
      const url = state?.url ?? this.router.routerState.snapshot.url;
      const urlTree = this.router.parseUrl(url);
      const returnUrl = urlTree.queryParams['returnUrl'];
      if (returnUrl && typeof returnUrl === 'string' && returnUrl.startsWith('/')) {
        return this.router.parseUrl(returnUrl);
      }

      const role = this.authService.role();
      let targetRoute = '/user/dashboard';

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

      return this.router.parseUrl(targetRoute);
    }

    return true;
  }
}


# /Frontend/src/app/core/guards/customer.guard.ts

import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const customerGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === 'RegisteredUser') {
    return true;
  }
  return router.createUrlTree(['/auth']);
};


# /Frontend/src/app/core/guards/front-desk.guard.ts

import { inject } from "@angular/core";
import { CanActivateFn, CanMatchFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

export const frontDeskGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === "FrontDesk") {
    return true;
  }
  return router.createUrlTree(["/auth"]);
};


# /Frontend/src/app/core/guards/housekeeping.guard.ts

import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const housekeepingGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === 'Housekeeping') {
    return true;
  }
  return router.createUrlTree(['/auth']);
};


# /Frontend/src/app/core/guards/kitchen.guard.ts

import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const kitchenGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === 'Kitchen') {
    return true;
  }
  return router.createUrlTree(['/auth']);
};


# /Frontend/src/app/core/guards/maintenance.guard.ts

import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const maintenanceGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === 'Maintenance') {
    return true;
  }
  return router.createUrlTree(['/auth']);
};


# /Frontend/src/app/core/interceptors/auth.interceptor.ts

import { HttpRequest, HttpHandlerFn, HttpEvent, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);
  const token = authService.token();

  if (token) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
    return next(authReq);
  }

  return next(req);
};

# /Frontend/src/app/core/models/auth-me-response.model.ts

export interface Claim {
  type: string;
  value: string;
}

export interface AuthMeResponse {
  claims: Claim[];
}


# /Frontend/src/app/core/models/auth.models.ts

export interface LoginRequestDTO {
  email: string;
  password: string;
}

export interface RegisterRequestDTO {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginResponse {
  token: string;
  role: string;
  firstName: string;
  lastName: string;
}


# /Frontend/src/app/core/models/paginated-response.model.ts

export interface PaginatedResponse<T> {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  data: T[];
}

# /Frontend/src/app/core/services/auth-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginRequestDTO, RegisterRequestDTO, LoginResponse } from '../models/auth.models';
import { environment } from '../../../environments/environment';
import { AuthMeResponse } from '../models/auth-me-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.baseUrl;

  login(credentials: LoginRequestDTO): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, credentials);
  }

  register(data: RegisterRequestDTO): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/register`, data);
  }

  getMe(): Observable<AuthMeResponse> {
    return this.http.get<AuthMeResponse>(`${this.baseUrl}/auth/me`);
  }

  updateProfile(dto: { firstName: string; lastName: string; email: string }): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/auth/me`, dto);
  }

  changePassword(dto: { currentPassword: string; newPassword: string; confirmNewPassword: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/change-password`, dto);
  }
}


# /Frontend/src/app/core/services/auth.service.ts

import { Injectable, signal, computed } from '@angular/core';
import { jwtDecode, JwtPayload } from '../utils/jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  token = signal<string | null>(null);
  role = signal<string | null>(null);
  private _decodedToken = signal<JwtPayload | null>(null);

  isAuthenticated = computed(() => !!this.token() && !this.isTokenExpired());
  fullName = computed(() => {
    const t = this._decodedToken();
    if (!t) {
      return 'Admin';
    }
    const name = `${t.firstName || ''} ${t.lastName || ''}`.trim();
    return name || t.role || 'Admin';
  });

  constructor() {
    const savedToken = localStorage.getItem('token');
    if (savedToken) {
      this.token.set(savedToken);
      this.decodeAndStore(savedToken);
    }
  }

  handleLogin(token: string): void {
    localStorage.setItem('token', token);
    this.token.set(token);
    this.decodeAndStore(token);
  }

  logout(): void {
    localStorage.removeItem('token');
    this.token.set(null);
    this.decodeAndStore(null);
  }

  private decodeAndStore(token: string | null): void {
    if (token) {
      const decoded = jwtDecode(token);
      this._decodedToken.set(decoded);
      if (decoded && decoded.role) {
        this.role.set(decoded.role);
      } else {
        this.role.set(null);
      }
    } else {
      this._decodedToken.set(null);
      this.role.set(null);
    }
  }

  private isTokenExpired(): boolean {
    const currentToken = this.token();
    if (!currentToken) {
      return true;
    }
    const decoded = jwtDecode(currentToken);
    if (!decoded || !decoded.exp) {
      return true;
    }
    return Date.now() >= decoded.exp * 1000;
  }
}


# /Frontend/src/app/core/services/notification.service.ts

import { Injectable, inject, OnDestroy, effect } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HttpTransportType, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NotificationSnackbarComponent } from '../../shared/components/notification-snackbar/notification-snackbar.component';

export interface NewTaskNotification {
  id: number;
  type: 'FoodOrder' | 'Housekeeping' | 'Maintenance';
  description: string;
  roomNumber?: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private hubConnection: HubConnection | null = null;
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  // Event streams
  readonly onAlert = new Subject<NewTaskNotification>();

  constructor() {
    effect(() => {
      const token = this.authService.token();
      if (token) {
        this.startConnection();
      } else {
        this.stopConnection();
      }
    });


  }

  startConnection(): void {
    if (this.hubConnection) return;
    const token = this.authService.token();
    if (!token) return;

    const hubUrl = environment.baseUrl.replace(/\/api\/v1$/, '') + '/notifications';
    // Example: 'http://localhost:5264/api/v1' → 'http://localhost:5264/notifications'

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token,
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveAlert', (message: string) => {
      const notification: NewTaskNotification = {
        id: 0,
        type: 'FoodOrder',
        description: message,
        roomNumber: undefined
      };
      if (message.toLowerCase().includes('housekeeping')) {
        notification.type = 'Housekeeping';
      } else if (message.toLowerCase().includes('maintenance')) {
        notification.type = 'Maintenance';
      } else if (message.toLowerCase().includes('order') || message.toLowerCase().includes('food')) {
        notification.type = 'FoodOrder';
      }
      this.onAlert.next(notification);
    });

    this.hubConnection.start().catch(err => console.error('SignalR connection error:', err));
  }

  stopConnection(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }

  showNotification(title: string, message: string): void {
    this.snackBar.openFromComponent(NotificationSnackbarComponent, {
      data: { title, message },
      duration: 5000,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: 'notification-snackbar',
    });
  }
}


# /Frontend/src/app/core/utils/jwt-decode.ts

export interface JwtPayload {
  exp: number;
  role: string;
  firstName: string;
  lastName: string;
  [key: string]: unknown;
}

export function jwtDecode(token: string): JwtPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) {
      return null;
    }
    const payload = parts[1];
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    const parsed = JSON.parse(jsonPayload);
    
    const firstName = parsed.firstName || parsed.given_name || '';
    const lastName = parsed.lastName || parsed.family_name || '';
    
    return {
      ...parsed,
      exp: parsed.exp ? Number(parsed.exp) : 0,
      role: parsed.role || '',
      firstName,
      lastName
    } as JwtPayload;
  } catch (e) {
    return null;
  }
}


# /Frontend/src/app/features/admin/admin-shell.component.html

<mat-sidenav-container>
  <mat-sidenav
    #sidenav
    [mode]="isMobile() ? 'over' : 'side'"
    [opened]="isMobile() ? sidebarOpen() : true"
    aria-label="Main navigation"
  >
    <mat-toolbar color="primary">Admin Panel</mat-toolbar>

    <mat-nav-list>
      <!-- Dashboard -->
      <a
        mat-list-item
        routerLink="/operations/admin/dashboard"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
      <mat-divider></mat-divider>

      <h3 matSubheader>Management</h3>
      <a
        mat-list-item
        routerLink="/operations/admin/management/room"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">meeting_room</mat-icon>
        <span matListItemTitle>Rooms</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/management/room-type"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">bed</mat-icon>
        <span matListItemTitle>Room Types</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/management/staff"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">people</mat-icon>
        <span matListItemTitle>Staff</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/management/amenities"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">spa</mat-icon>
        <span matListItemTitle>Amenities</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/management/menu"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">restaurant_menu</mat-icon>
        <span matListItemTitle>Menu</span>
      </a>
      <mat-divider></mat-divider>

      <h3 matSubheader>Oversight</h3>
      <a
        mat-list-item
        routerLink="/operations/admin/oversight/analytics"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">insights</mat-icon>
        <span matListItemTitle>Analytics</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/oversight/auditlogs"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">history</mat-icon>
        <span matListItemTitle>Audit Logs</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/oversight/billings-receipts"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">receipt</mat-icon>
        <span matListItemTitle>Billing & Receipts</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/oversight/feedback"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon aria-hidden="true">feedback</mat-icon>
        <span matListItemTitle>Feedback</span>
      </a>

    </mat-nav-list>
  </mat-sidenav>

  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
      <button
        mat-icon-button
        (click)="sidebarOpen.set(!sidebarOpen())"
      >
        <mat-icon aria-hidden="true">menu</mat-icon>
      </button>
      }
      <span>{{ title() }}</span>
      <span class="spacer"></span>
      @if (!isMobile()) {
      <span>{{ userDisplayName() }}</span>
      }
      <button
        mat-icon-button
        [matMenuTriggerFor]="userMenu"
        aria-label="Open user menu"
      >
        <mat-icon aria-hidden="true">account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button
          mat-menu-item
          routerLink="/operations/admin/profile"
        >
          <mat-icon aria-hidden="true">manage_accounts</mat-icon> Profile
        </button>
        <button
          mat-menu-item
          (click)="logout()"
        >
          <mat-icon aria-hidden="true">logout</mat-icon> Logout
        </button>
      </mat-menu>
    </mat-toolbar>

    <div class="content">
      <router-outlet></router-outlet>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>


# /Frontend/src/app/features/admin/admin-shell.component.scss

mat-sidenav-container {
  height: 100vh;
  width: 100%;
}

mat-sidenav {
  width: 250px;
  border-right: 1px solid rgba(0, 0, 0, 0.12);

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

mat-sidenav-content {
  display: flex;
  flex-direction: column;
  height: 100%;

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

.spacer {
  flex: 1 1 auto;
}

.content {
  padding: 24px;
  flex-grow: 1;
  overflow-y: auto;
  box-sizing: border-box;
}

.active {
  background-color: rgba(63, 81, 181, 0.08);
  color: #3f51b5 !important;
  font-weight: 500;

  mat-icon {
    color: #3f51b5;
  }
}

h3[matSubheader] {
  padding-left: 16px;
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.8px;
  color: rgba(0, 0, 0, 0.54);
  margin-top: 16px;
  margin-bottom: 8px;
}

@media (max-width: 1024px) {
  .content {
    padding: 16px;
  }
}


# /Frontend/src/app/features/admin/admin-shell.component.ts

import { Component, signal, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { BreakpointObserver, LayoutModule } from '@angular/cdk/layout';
import { toSignal, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map, filter } from 'rxjs/operators';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
    LayoutModule
  ],
  templateUrl: './admin-shell.component.html',
  styleUrls: ['./admin-shell.component.scss']
})
export class AdminShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 1024px)').pipe(
      map(result => result.matches)
    ),
    { initialValue: false }
  );

  sidebarOpen = signal(false);
  userDisplayName = this.authService.fullName;
  title = signal('Admin');

  constructor() {
    // Initial extraction
    this.updateTitle();

    this.router.events
      .pipe(
        filter(e => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.updateTitle();
      });
  }

  private updateTitle(): void {
    let route = this.activatedRoute;
    while (route.firstChild) {
      route = route.firstChild;
    }
    const title = route.snapshot?.data?.['title'] || 'Admin';
    this.title.set(title);
  }

  onNavClick(): void {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }
}


# /Frontend/src/app/features/admin/components/create-internal-ticket-dialog.component.html

<h2 mat-dialog-title>Create Internal Ticket</h2>
<button mat-icon-button mat-dialog-close class="close-btn" aria-label="Close dialog">
  <mat-icon>close</mat-icon>
</button>

<mat-dialog-content>
  @if (errorMessage()) {
    <div class="dialog-error">{{ errorMessage() }}</div>
  }

  <form [formGroup]="form" (ngSubmit)="submit()">
    <div class="field-group">
      <label class="field-label">Ticket Type</label>
      <mat-radio-group formControlName="type" class="radio-group">
        <mat-radio-button value="housekeeping">Housekeeping</mat-radio-button>
        <mat-radio-button value="maintenance">Maintenance</mat-radio-button>
      </mat-radio-group>
    </div>

    <mat-form-field appearance="outline" class="full-width">
      <mat-label>Location</mat-label>
      <input matInput formControlName="location" placeholder="e.g. Room 101, Lobby" />
      @if (form.get('location')?.hasError('required') && form.get('location')?.touched) {
        <mat-error>Location is required.</mat-error>
      }
      @if (form.get('location')?.hasError('maxlength')) {
        <mat-error>Location must not exceed 200 characters.</mat-error>
      }
    </mat-form-field>

    <mat-form-field appearance="outline" class="full-width">
      <mat-label>Description</mat-label>
      <textarea matInput formControlName="description" rows="4" placeholder="Describe the issue..."></textarea>
      @if (form.get('description')?.hasError('required') && form.get('description')?.touched) {
        <mat-error>Description is required.</mat-error>
      }
      @if (form.get('description')?.hasError('minlength')) {
        <mat-error>Description must be at least 5 characters.</mat-error>
      }
    </mat-form-field>
  </form>
</mat-dialog-content>

<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Cancel</button>
  <button
    mat-raised-button
    color="primary"
    [disabled]="form.invalid || loading()"
    (click)="submit()"
  >
    @if (loading()) {
      <mat-spinner diameter="20"></mat-spinner>
    } @else {
      Submit
    }
  </button>
</mat-dialog-actions>


# /Frontend/src/app/features/admin/components/create-internal-ticket-dialog.component.scss

:host {
  display: block;
}

h2[mat-dialog-title] {
  margin: 0;
  padding: 16px 48px 16px 24px;
  font-size: 1.25rem;
  font-weight: 600;
  border-bottom: 1px solid #e0e0e0;
  position: relative;
}

.close-btn {
  position: absolute;
  top: 8px;
  right: 8px;
}

mat-dialog-content {
  padding: 16px 24px;
  min-width: 400px;
  max-width: 560px;

  @media (max-width: 768px) {
    min-width: unset;
    width: 100%;
  }
}

.dialog-error {
  background: #ffebee;
  color: #c62828;
  border: 1px solid #ffcdd2;
  border-radius: 4px;
  padding: 10px 14px;
  margin-bottom: 16px;
  font-size: 0.875rem;
}

.field-group {
  margin-bottom: 16px;
}

.field-label {
  display: block;
  font-size: 0.875rem;
  font-weight: 500;
  color: rgba(0, 0, 0, 0.6);
  margin-bottom: 8px;
}

.radio-group {
  display: flex;
  gap: 24px;
}

.full-width {
  width: 100%;
  margin-bottom: 8px;
}

mat-dialog-actions {
  padding: 12px 24px 16px;
  border-top: 1px solid #e0e0e0;
  gap: 8px;
}

mat-spinner {
  display: inline-block;
}


# /Frontend/src/app/features/admin/components/create-internal-ticket-dialog.component.ts

import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs/operators';
import { HousekeepingApiService } from '../services/housekeeping-api.service';
import { MaintenanceApiService } from '../services/maintenance-api.service';

@Component({
  selector: 'app-create-internal-ticket-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatRadioModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './create-internal-ticket-dialog.component.html',
  styleUrls: ['./create-internal-ticket-dialog.component.scss']
})
export class CreateInternalTicketDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<CreateInternalTicketDialogComponent>);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);

  form = new FormGroup({
    type: new FormControl<'housekeeping' | 'maintenance'>('housekeeping', Validators.required),
    location: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    description: new FormControl('', [Validators.required, Validators.minLength(5)])
  });

  loading = signal(false);
  errorMessage = signal<string | null>(null);

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const { type, location, description } = this.form.value;
    const body = { location: location!, description: description! };

    this.loading.set(true);
    this.errorMessage.set(null);

    const request$ = type === 'maintenance'
      ? this.maintenanceApi.createInternal(body)
      : this.housekeepingApi.createInternal(body);

    request$.pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => this.errorMessage.set(err.error?.message || 'Failed to create ticket. Please try again.')
    });
  }
}


# /Frontend/src/app/features/admin/components/room-status-grid/room-status-grid.component.html

@if (loading()) {
  <mat-spinner diameter="30"></mat-spinner>
} @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button mat-button (click)="fetchStatuses()">Retry</button>
  </app-alert>
} @else {
  <div class="status-grid">
    @for (room of rooms(); track room.roomId) {
      <div
        class="room-card"
        [ngClass]="getStatusClass(room.status)"
        (click)="roomClicked.emit(room)"
        [matTooltip]="tooltipContent(room)"
        matTooltipPosition="above"
        [attr.aria-label]="room.roomNumber + ' - ' + room.status"
      >
        <span class="room-number">{{ room.roomNumber }}</span>
        <mat-icon
          >{{ (room.status ?? '').toLowerCase() === 'occupied' ? 'lock' : 'lock_open'
          }}</mat-icon
        >
      </div>
    } @empty {
      <p>No room statuses available.</p>
    }
  </div>
}


# /Frontend/src/app/features/admin/components/room-status-grid/room-status-grid.component.scss

.status-grid {
  display: grid;
  grid-auto-flow: column;
  grid-template-rows: repeat(3, 1fr);
  grid-auto-columns: 120px;
  gap: 8px;
  overflow-x: auto;
  overflow-y: hidden;
  height: calc(3 * 68px);
  padding: 8px 0;
}

.room-card {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  border-radius: 4px;

  &.occupied {
    background-color: #ef9a9a;
  }

  &.available {
    background-color: #a5d6a7;
  }

  &.neutral {
    background-color: #eeeeee;
  }

  .room-number {
    font-size: 0.75rem;
    font-weight: 600;
    text-align: center;
  }

  mat-icon {
    font-size: 1.1rem;
    width: 1.1rem;
    height: 1.1rem;
  }
}

p {
  color: rgba(0, 0, 0, 0.5);
  font-size: 0.875rem;
  text-align: center;
  padding: 16px 0;
}

@media (max-width: 767px) {
  .status-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);   /* 2 columns */
    grid-template-rows: auto;                /* override the 3‑row fixed height */
    grid-auto-flow: row;                     /* override column → rows flow vertically */
    grid-auto-rows: minmax(60px, auto);      /* each row at least 60px, grows with content */
    gap: 8px;
    overflow-y: auto;
    overflow-x: hidden;
    height: auto;                            /* let content define height */
    max-height: 70vh;                        /* then clip with scroll */
    padding: 8px;
    align-items: stretch;                    /* optional: make all cards fill row height */
  }

  .room-card {
    min-height: 60px;      /* use min instead of fixed height */
    /* height: 60px; */    /* remove */
    /* flex: none; */      /* remove – it’s for flex, not grid */
  }
}



# /Frontend/src/app/features/admin/components/room-status-grid/room-status-grid.component.ts

import { Component, inject, signal, input, output, effect, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { RoomApiService } from '../../services/room-api.service';
import { RoomStatus } from '../../models/room.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

@Component({
  selector: 'app-room-status-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    AlertComponent,
  ],
  templateUrl: './room-status-grid.component.html',
  styleUrls: ['./room-status-grid.component.scss'],
})
export class RoomStatusGridComponent {
  roomTypeId = input<number | null>(null);
  roomClicked = output<RoomStatus>();

  rooms = signal<RoomStatus[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  private readonly roomApi = inject(RoomApiService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    effect(() => {
      // re-fetch when roomTypeId changes
      this.roomTypeId();
      this.fetchStatuses();
    });
  }

  fetchStatuses(): void {
    this.loading.set(true);
    this.error.set(null);
    this.roomApi
      .getStatuses({
        pageNumber: 1,
        pageSize: 100,
        roomTypeId: this.roomTypeId() ?? undefined,
        sortDescending: false,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => this.rooms.set(res.data),
        error: (err: Error) => this.error.set(err.message),
      });
  }

  tooltipContent(room: RoomStatus): string {
    if (room.status === 'Occupied') {
      return `Occupied - ${room.currentGuestName ?? 'Guest'}`;
    }
    return 'Available';
  }

  getStatusClass(status: string | null | undefined): string {
    const normalized = (status ?? '').toLowerCase();
    if (normalized === 'occupied') return 'occupied';
    if (normalized === 'available') return 'available';
    return 'neutral';
  }
}


# /Frontend/src/app/features/admin/models/amenity.model.ts

export interface Amenity {
  id: number;
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
}

export interface CreateAmenityDTO {
  name: string;
  description: string;
  price: number;
}

export interface UpdateAmenityDTO {
  name: string;
  description: string;
  price: number;
  isAvailable: boolean;
}


# /Frontend/src/app/features/admin/models/analytics-dashboard.dto.ts

export interface AnalyticsDashboardDTO {
  occupancyRate: number;
  averageDailyRate: number;
  revPAR: number;
  totalRevenue: number;
  grossTurnover: number;
  averageLengthOfStay: number;
  cancellationRate: number;
  guestSatisfactionScore: number;
  averageHousekeepingTurnaroundMinutes: number;
  nonRoomExpenditure: {
    totalFoodSpend: number;
    totalAmenitySpend: number;
    highestSpendCategory: string;
  };
}


# /Frontend/src/app/features/admin/models/audit-log-entry.model.ts

export interface AuditLogEntry {
  id: number;
  entityName: string;
  action: string;
  recordId: { Id: number };
  oldValues: Record<string, any> | null;
  newValues: Record<string, any> | null;
  changedByEmail: string;
  changedByName: string;
  timestamp: string; // ISO 8601
}


# /Frontend/src/app/features/admin/models/booking.model.ts

export interface BookingRoom {
  id: number;
  bookingId: number;
  roomTypeId: number;
  roomId: number | null;
  roomNumber: string | null;
  lockedInPrice: number;
}

export interface Booking {
  id: number;
  guestCount: number;
  rooms: BookingRoom[];
  guestName: string;
  guestEmail: string;
  checkInDate: string; // "dd-MM-yyyy"
  checkOutDate: string; // "dd-MM-yyyy"
  bookingStatus: 'Booked' | 'CheckedIn' | 'CheckedOut' | 'Cancelled';
  userId: number | null;
  origin: 'WalkIn' | 'RegisteredUser' | 'Guest';
  bookedAt: string; // ISO 8601
  amenityIds: number[];
}


# /Frontend/src/app/features/admin/models/create-internal-ticket-request.dto.ts

export interface CreateInternalTicketRequest {
  location: string;
  description: string;
}


# /Frontend/src/app/features/admin/models/feedback.model.ts

export interface Feedback {
  id: number;
  bookingId: number;
  rating: number;
  comments: string | null;
  createdAt: string; // ISO 8601
  isHidden: boolean;
}

export interface ModerateFeedbackRequest {
  isHidden: boolean;
}


# /Frontend/src/app/features/admin/models/housekeeping-task.model.ts

export interface HousekeepingTask {
  id: number;
  roomId: number;
  location: string | null;
  description: string | null;
  originType: string;
  status: 'Pending' | 'InProgress' | 'Completed';
  createdAt: string;
  startedAt: string | null;
  finishedAt: string | null;
}


# /Frontend/src/app/features/admin/models/maintenance-task.model.ts

export interface MaintenanceTask {
  id: number;
  roomId: number;
  location: string;
  originType: string;
  status: 'Pending' | 'InProgress' | 'Completed';
  description: string;
  createdAt: string;
  startedAt: string | null;
  finishedAt: string | null;
}


# /Frontend/src/app/features/admin/models/menu-item.model.ts

export interface MenuItem {
  id: number;
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
}

export interface CreateMenuItemDTO {
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
}

export interface UpdateMenuItemDTO {
  name: string;
  price: number;
  category: string;
  isAvailable: boolean;
}


# /Frontend/src/app/features/admin/models/receipt.model.ts

export interface Receipt {
  id: number;
  bookingId: number;
  amountPaid: number;
  paymentMethod: string;
  transactionId: string;
  paidAt: string; // ISO 8601
}


# /Frontend/src/app/features/admin/models/room-type.model.ts

export interface RoomType {
  id: number;
  name: string;
  description: string | null;
  basePrice: number;
  maxOccupancy: number;
  imageUrls: string[];
  squareFootage: number | null;
  bedConfiguration: Record<string, number> | null;
  isActive: boolean;
}

export interface CreateRoomTypeDTO {
  name: string;
  description?: string;
  basePrice: number;
  maxOccupancy: number;
  imageUrls?: string[];
  squareFootage?: number;
  bedConfiguration?: Record<string, number>;
}

export interface UpdateRoomTypeDTO {
  name?: string;
  description?: string;
  basePrice?: number;
  maxOccupancy?: number;
  imageUrls?: string[];
  squareFootage?: number;
  bedConfiguration?: Record<string, number>;
  isActive?: boolean;
}


# /Frontend/src/app/features/admin/models/room.model.ts

export interface Room {
  id: number;
  roomNumber: string;
  roomTypeName: string;
  roomTypeId: number;
  basePrice: number;
  maxOccupancy: number;
  isAvailable: boolean;

}

export interface CreateRoomDTO {
  roomNumber: string;
  roomTypeId: number;
  isActive: boolean;
}

export interface UpdateRoomDTO {
  roomNumber?: string;
  roomTypeId?: number;
  isActive?: boolean;
}

export interface RoomStatus {
  roomId: number;
  roomNumber: string;
  roomTypeName: string;
  status: 'Occupied' | 'Available';
  currentBookingId: number | null;
  currentGuestName: string | null;
  nextCheckInDate: string | null;
}


# /Frontend/src/app/features/admin/models/staff.model.ts

export type StaffRole =
  | 'Admin'
  | 'FrontDesk'
  | 'Kitchen'
  | 'Housekeeping'
  | 'Maintenance';

export interface Staff {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: StaffRole;
  isActive: boolean;
  createdAt: string;
}

export interface CreateStaffDTO {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: StaffRole;
}

export interface UpdateStaffDTO {
  firstName?: string;
  lastName?: string;
  role?: StaffRole;
  isActive?: boolean;
}


# /Frontend/src/app/features/admin/pages/dashboard.component.html

<div class="dashboard">

  <!-- Toast notification -->
  @if (ticketCreatedMessage()) {
    <div class="toast success" role="status" aria-live="polite">
      <mat-icon>check_circle</mat-icon>
      {{ ticketCreatedMessage() }}
    </div>
  }

  <!-- Top bar -->
  <div class="top-bar">
    <div class="date-filter">
      <mat-form-field appearance="outline">
        <mat-label>Start date</mat-label>
        <input
          matInput
          [matDatepicker]="startPicker"
          [formControl]="startDateCtrl"
        />
        <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
        <mat-datepicker #startPicker></mat-datepicker>
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>End date</mat-label>
        <input
          matInput
          [matDatepicker]="endPicker"
          [formControl]="endDateCtrl"
        />
        <mat-datepicker-toggle matSuffix [for]="endPicker"></mat-datepicker-toggle>
        <mat-datepicker #endPicker></mat-datepicker>
      </mat-form-field>

      <button
        mat-raised-button
        color="primary"
        (click)="applyDateFilter()"
      >
        Apply
      </button>

      <button
        mat-button
        (click)="clearDateFilter()"
      >
        Clear
      </button>
    </div>

    <button
      mat-raised-button
      color="accent"
      (click)="openCreateTicketDialog()"
    >
      <mat-icon>add_task</mat-icon> Create Internal Ticket
    </button>
  </div>

  <!-- KPI Cards (6) -->
  <div class="kpi-row">
    @for (kpi of kpiCards(); track kpi.label) {
      <mat-card class="kpi-card">
        <mat-card-title>{{ kpi.label }}</mat-card-title>
        <mat-card-content>
          <span class="value">{{ kpi.value }}</span>
        </mat-card-content>
      </mat-card>
    }
  </div>

  <!-- Middle row: Charts + Department Health -->
  <div class="middle-row">
    <div class="charts">
      <div class="chart-container" aria-label="Revenue chart">
        @if (analyticsLoading()) {
          <mat-spinner diameter="40"></mat-spinner>
        } @else if (analyticsError()) {
          <app-alert
            type="error"
            [message]="analyticsError()!"
            (closed)="analyticsError.set(null)"
          >
            <button mat-button (click)="loadAnalytics()">Retry</button>
          </app-alert>
        }
        <div
          echarts
          [options]="revenueChartOptions()"
          #chartRef
          class="chart"
        ></div>
      </div>

      <div class="chart-container" aria-label="Expenditure chart">
        <div
          echarts
          [options]="expenditureChartOptions()"
          #chartRef
          class="chart"
        ></div>
      </div>
    </div>

    <div class="health-cards">
      @if (pendingError()) {
        <app-alert
          type="error"
          [message]="pendingError()!"
          (closed)="pendingError.set(null)"
        >
          <button mat-button (click)="loadPendingCounts()">Retry</button>
        </app-alert>
      }
      <mat-card>
        <mat-card-title>Housekeeping Pending</mat-card-title>
        <mat-card-content class="count">
          @if (pendingLoading()) {
            <mat-spinner diameter="30"></mat-spinner>
          } @else {
            {{ housekeepingPendingCount() }}
          }
        </mat-card-content>
      </mat-card>

      <mat-card>
        <mat-card-title>Maintenance Pending</mat-card-title>
        <mat-card-content class="count">
          @if (pendingLoading()) {
            <mat-spinner diameter="30"></mat-spinner>
          } @else {
            {{ maintenancePendingCount() }}
          }
        </mat-card-content>
      </mat-card>
    </div>
  </div>

  <!-- Today's Movement Table -->
  <div class="movement-table">
    <h2>Today's Movement</h2>
    @if (auditLoading()) {
      <mat-spinner diameter="30"></mat-spinner>
    } @else if (auditError()) {
      <app-alert
        type="error"
        [message]="auditError()!"
        (closed)="auditError.set(null)"
      >
        <button mat-button (click)="loadAuditLogs()">Retry</button>
      </app-alert>
    } @else {
      <table
        mat-table
        [dataSource]="auditEntries()"
        aria-label="Recent audit logs"
      >
        <ng-container matColumnDef="timestamp">
          <th mat-header-cell *matHeaderCellDef>Time</th>
          <td mat-cell *matCellDef="let entry">{{ entry.timestamp | date:'shortTime' }}</td>
        </ng-container>

        <ng-container matColumnDef="entity">
          <th mat-header-cell *matHeaderCellDef>Entity</th>
          <td mat-cell *matCellDef="let entry">{{ entry.entityName }}</td>
        </ng-container>

        <ng-container matColumnDef="action">
          <th mat-header-cell *matHeaderCellDef>Action</th>
          <td mat-cell *matCellDef="let entry">{{ entry.action }}</td>
        </ng-container>

        <ng-container matColumnDef="changedBy">
          <th mat-header-cell *matHeaderCellDef>Changed By</th>
          <td mat-cell *matCellDef="let entry">{{ entry.changedByName }}</td>
        </ng-container>

        <ng-container matColumnDef="summary">
          <th mat-header-cell *matHeaderCellDef>Summary</th>
          <td mat-cell *matCellDef="let entry">{{ getAuditSummary(entry) }}</td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
      </table>
      @if (auditEntries().length === 0 && !auditLoading()) {
        <p class="empty-state">No recent activity.</p>
      }
    }
  </div>
</div>


# /Frontend/src/app/features/admin/pages/dashboard.component.scss

.dashboard {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  position: relative;

  // Toast notification
  .toast {
    position: fixed;
    top: 16px;
    right: 16px;
    z-index: 1000;
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 20px;
    border-radius: 4px;
    font-size: 0.9rem;
    font-weight: 500;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
    animation: slideIn 0.3s ease;

    &.success {
      background-color: #e8f5e9;
      color: #2e7d32;
      border: 1px solid #a5d6a7;
    }
  }

  @keyframes slideIn {
    from {
      opacity: 0;
      transform: translateX(40px);
    }
    to {
      opacity: 1;
      transform: translateX(0);
    }
  }

  // Top bar
  .top-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 16px;

    .date-filter {
      display: flex;
      align-items: center;
      gap: 12px;
      flex-wrap: wrap;

      mat-form-field {
        width: 160px;
      }
    }
  }

  // KPI Row
  .kpi-row {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;

    @media (max-width: 1024px) {
      grid-template-columns: repeat(2, 1fr);
    }

    @media (max-width: 767px) {
      grid-template-columns: 1fr;
    }

    .kpi-card {
      padding: 8px;

      mat-card-title {
        font-size: 0.85rem;
        color: rgba(0, 0, 0, 0.6);
        margin-bottom: 8px;
      }

      .value {
        font-size: 2rem;
        font-weight: 700;
        color: #1976d2;
      }
    }
  }

  // Middle row
  .middle-row {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;

    @media (max-width: 959px) {
      flex-direction: column;
    }

    .charts {
      flex: 1 1 60%;
      min-width: 300px;
      display: flex;
      gap: 16px;

      @media (max-width: 768px) {
        flex-direction: column;
      }

      .chart-container {
        flex: 1;
        min-height: 280px;
        display: flex;
        flex-direction: column;
        background: #fff;
        border-radius: 8px;
        padding: 16px;
        box-shadow: 0 1px 4px rgba(0, 0, 0, 0.1);

        .chart {
          width: 100%;
          height: 400px;
        }
      }
    }

    .health-cards {
      flex: 1 1 30%;
      min-width: 250px;
      display: flex;
      flex-direction: column;
      gap: 16px;

      mat-card {
        mat-card-title {
          font-size: 0.9rem;
          color: rgba(0, 0, 0, 0.6);
          margin-bottom: 8px;
        }

        .count {
          display: flex;
          align-items: center;
          font-size: 2.5rem;
          font-weight: 700;
          color: #f57c00;
          padding: 8px 0;
        }
      }
    }
  }

  // Movement table
  .movement-table {
    background: #fff;
    border-radius: 8px;
    padding: 16px;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.1);
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;

    h2 {
      margin: 0 0 16px;
      font-size: 1.1rem;
      font-weight: 600;
    }

    table {
      width: 100%;
      min-width: 600px;

      th {
        font-weight: 600;
        color: rgba(0, 0, 0, 0.7);
      }

      td {
        font-size: 0.875rem;
      }
    }

    .empty-state {
      text-align: center;
      color: rgba(0, 0, 0, 0.4);
      padding: 24px 0;
      margin: 0;
    }
  }
}

@media (max-width: 599px) {
  .chart {
    height: 300px;
  }
}


# /Frontend/src/app/features/admin/pages/dashboard.component.ts

import { AfterViewInit, Component, ElementRef, OnInit, QueryList, ViewChildren, inject, signal, computed, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatRadioModule } from '@angular/material/radio';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { NgxEchartsDirective } from 'ngx-echarts';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AlertComponent } from '../../auth/components/alert.component';
import { AnalyticsApiService } from '../services/analytics-api.service';
import { HousekeepingApiService } from '../services/housekeeping-api.service';
import { MaintenanceApiService } from '../services/maintenance-api.service';
import { AuditLogApiService } from '../services/audit-log-api.service';
import { AnalyticsDashboardDTO } from '../models/analytics-dashboard.dto';
import { AuditLogEntry } from '../models/audit-log-entry.model';
import { CreateInternalTicketDialogComponent } from '../components/create-internal-ticket-dialog.component';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTableModule,
    MatDialogModule,
    MatRadioModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSnackBarModule,
    NgxEchartsDirective,
    AlertComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit, AfterViewInit {
  @ViewChildren('chartRef') charts!: QueryList<ElementRef>;
  private readonly analyticsApi = inject(AnalyticsApiService);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly auditLogApi = inject(AuditLogApiService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  // Form controls
  startDateCtrl = new FormControl<Date | null>(null);
  endDateCtrl = new FormControl<Date | null>(null);

  // Analytics signals
  analytics = signal<AnalyticsDashboardDTO | null>(null);
  analyticsLoading = signal(false);
  analyticsError = signal<string | null>(null);

  // Pending counts signals
  housekeepingPendingCount = signal(0);
  maintenancePendingCount = signal(0);
  pendingLoading = signal(false);
  pendingError = signal<string | null>(null);

  // Audit log signals
  auditEntries = signal<AuditLogEntry[]>([]);
  auditLoading = signal(false);
  auditError = signal<string | null>(null);

  // Ticket creation feedback
  ticketCreatedMessage = signal<string | null>(null);

  // Table column definition
  displayedColumns = ['timestamp', 'entity', 'action', 'changedBy', 'summary'];

  // KPI cards computed
  kpiCards = computed(() => {
    const a = this.analytics();
    if (!a) {
      return [
        { label: 'Occupancy Rate', value: '—' },
        { label: 'Avg Daily Rate', value: '—' },
        { label: 'RevPAR', value: '—' },
        { label: 'Guest Satisfaction', value: '—' },
        { label: 'Cancellation Rate', value: '—' },
        { label: 'Avg Length of Stay', value: '—' },
      ];
    }
    return [
      { label: 'Occupancy Rate', value: `${a.occupancyRate}%` },
      { label: 'Avg Daily Rate', value: `$${a.averageDailyRate}` },
      { label: 'RevPAR', value: `$${a.revPAR}` },
      { label: 'Guest Satisfaction', value: `${a.guestSatisfactionScore}%` },
      { label: 'Cancellation Rate', value: `${a.cancellationRate}%` },
      { label: 'Avg Length of Stay', value: `${a.averageLengthOfStay} days` },
    ];
  });

  // Revenue chart options computed
  revenueChartOptions = computed(() => {
    const a = this.analytics();
    if (!a)
      return {
        xAxis: { type: 'category', data: [] },
        yAxis: { type: 'value' },
        series: [],
      };
    return {
      title: { text: 'Revenue Overview' },
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: ['Total Revenue', 'Gross Turnover'] },
      yAxis: { type: 'value' },
      series: [
        {
          type: 'bar',
          data: [a.totalRevenue, a.grossTurnover],
          color: '#1976d2',
        },
      ],
    };
  });

  // Expenditure chart options computed
  expenditureChartOptions = computed(() => {
    const a = this.analytics();
    if (!a)
      return {
        xAxis: { type: 'category', data: [] },
        yAxis: { type: 'value' },
        series: [],
      };
    return {
      title: { text: 'Non‑Room Expenditure' },
      tooltip: { trigger: 'item' },
      series: [
        {
          type: 'pie',
          data: [
            { name: 'Food', value: a.nonRoomExpenditure.totalFoodSpend },
            { name: 'Amenities', value: a.nonRoomExpenditure.totalAmenitySpend },
          ],
          label: { formatter: '{b}: {c} ({d}%)' },
        },
      ],
    };
  });

  ngOnInit(): void {
    this.loadAnalytics();
    this.loadPendingCounts();
    this.loadAuditLogs();
  }

  ngAfterViewInit(): void {
    // Force ECharts to recalculate dimensions after view initialisation
    setTimeout(() => {
      window.dispatchEvent(new Event('resize'));
    });
  }

  loadAnalytics(params?: { startDate?: string; endDate?: string }): void {
    this.analyticsLoading.set(true);
    this.analyticsError.set(null);
    this.analyticsApi
      .getAnalytics(params)
      .pipe(finalize(() => this.analyticsLoading.set(false)))
      .subscribe({
        next: (data) => this.analytics.set(data),
        error: (err) => this.analyticsError.set(err.error?.message || 'Failed to load analytics'),
      });
  }

  loadPendingCounts(): void {
    this.pendingLoading.set(true);
    this.pendingError.set(null);
    forkJoin({
      hk: this.housekeepingApi.getAll({ status: 'Pending', pageNumber: 1, pageSize: 10 }),
      mt: this.maintenanceApi.getAll({ status: 'Pending', pageNumber: 1, pageSize: 10 }),
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.pendingLoading.set(false)),
      )
      .subscribe({
        next: ({ hk, mt }) => {
          this.housekeepingPendingCount.set(hk.totalCount);
          this.maintenancePendingCount.set(mt.totalCount);
        },
        error: (err: Error) => this.pendingError.set(err.message),
      });
  }

  loadAuditLogs(): void {
    this.auditLoading.set(true);
    this.auditError.set(null);
    this.auditLogApi
      .getAll({ sortBy: 'timestamp', sortDescending: true, pageSize: 5, pageNumber: 1 })
      .pipe(finalize(() => this.auditLoading.set(false)))
      .subscribe({
        next: (res) => this.auditEntries.set(res && Array.isArray(res.data) ? res.data : []),
        error: (err) => this.auditError.set(err.error?.message || 'Failed to load audit logs'),
      });
  }

  applyDateFilter(): void {
    const start = this.startDateCtrl.value;
    const end = this.endDateCtrl.value;
    if (!start || !end) return;
    const startISO = `${start.toISOString().split('T')[0]}T00:00:00Z`;
    const endISO = `${end.toISOString().split('T')[0]}T23:59:59Z`;
    this.loadAnalytics({ startDate: startISO, endDate: endISO });
  }

  clearDateFilter(): void {
    this.startDateCtrl.reset();
    this.endDateCtrl.reset();
    this.loadAnalytics();
  }

  openCreateTicketDialog(): void {
    const dialogRef = this.dialog.open(CreateInternalTicketDialogComponent);
    dialogRef.afterClosed().subscribe((result) => {
      if (result === true) {
        this.ticketCreatedMessage.set('Ticket created successfully');
        this.loadPendingCounts();
        setTimeout(() => this.ticketCreatedMessage.set(null), 3000);
      }
    });
  }

  getAuditSummary(entry: AuditLogEntry): string {
    const newKeys = Object.keys(entry.newValues ?? {});
    if (newKeys.length > 0) {
      return `${entry.action} on ${entry.entityName}: ${newKeys.slice(0, 2).join(', ')}`;
    }
    return `${entry.action} on ${entry.entityName}`;
  }
}


# /Frontend/src/app/features/admin/pages/management/amenities-management.component.html

<app-generic-crud
  [config]="crudConfig"
  [searchQuery]="searchQuery()"
  (edit)="onEdit($event)"
  (searchChange)="onSearchChange($event)"
  (filterChange)="onFilterChange($event)"
  (sortChange)="onSortChange($event)"
  (pageChange)="onPageChange($event)"
  (save)="onSave($event)"
></app-generic-crud>


# /Frontend/src/app/features/admin/pages/management/amenities-management.component.scss

// Component-scoped styles for amenities management if needed


# /Frontend/src/app/features/admin/pages/management/amenities-management.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs/operators';

import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
import {
  CrudConfig,
  ColumnDef,
  FilterDef,
  FormFieldDef,
} from '../../../../shared/models/crud-config.model';
import { AmenityApiService } from '../../services/amenity-api.service';
import {
  Amenity,
  CreateAmenityDTO,
  UpdateAmenityDTO,
} from '../../models/amenity.model';

@Component({
  selector: 'app-amenities-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    GenericCrudComponent,
  ],
  templateUrl: './amenities-management.component.html',
  styleUrls: ['./amenities-management.component.scss'],
})
export class AmenitiesManagementComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly snackBar = inject(MatSnackBar);
  private readonly amenityApi = inject(AmenityApiService);

  private readonly STORAGE_KEY = 'amenitiesState';

  data = signal<Amenity[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('name');
  sortDescending = signal(false);
  searchQuery = signal('');
  availabilityFilter = signal<boolean | null>(null);
  editingEntity = signal<Amenity | null>(null);

  crudConfig: CrudConfig<Amenity> = {
    entityName: 'Amenity',
    entityNamePlural: 'Amenities',
    columns: [
      { header: 'Name', field: 'name', sortable: true, getValue: (r) => r.name },
      {
        header: 'Description',
        field: 'description',
        sortable: false,
        getValue: (r) => r.description,
      },
      {
        header: 'Price',
        field: 'price',
        sortable: true,
        getValue: (r) => `$${r.price}`,
      },
      {
        header: 'Available',
        field: 'isAvailable',
        sortable: true,
        getValue: (r) => (r.isAvailable ? 'Yes' : 'No'),
      },
    ],
    filters: [
      {
        key: 'isAvailable',
        label: 'Availability',
        options: [
          { value: null, label: 'All' },
          { value: true, label: 'Available' },
          { value: false, label: 'Unavailable' },
        ],
      },
    ],
    formFields: [
      {
        key: 'name',
        label: 'Name',
        type: 'text',
        validators: [
          Validators.required,
          Validators.maxLength(100),
          Validators.minLength(1),
          Validators.pattern(/^(?=.*[a-zA-Z])[a-zA-Z0-9\s\-']+$/),
        ],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'description',
        label: 'Description',
        type: 'textarea',
        validators: [
          Validators.required,
          Validators.maxLength(500),
          Validators.minLength(1),
        ],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'price',
        label: 'Price',
        type: 'number',
        validators: [
          Validators.required,
          Validators.min(0),
          Validators.max(10000),
        ],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'isAvailable',
        label: 'Available',
        type: 'toggle',
        validators: [],
        showInAdd: false, // not shown on creation (defaults to true)
        showInEdit: true,
      },
    ],
    supportsToggle: true,
    data: this.data,
    totalCount: this.totalCount,
    loading: this.loading,
    error: this.error,
    pageIndex: this.pageIndex,
    pageSize: this.pageSize,
  };

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.amenityApi
      .getAll({
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        searchQuery: this.searchQuery() || undefined,
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
        isAvailable: this.availabilityFilter() ?? undefined,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.data);
          this.totalCount.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
            this.saveState();
          }
        },
        error: (err: any) =>
          this.error.set(err instanceof Error ? err.message : 'Unexpected error'),
      });
  }

  onEdit(entity: Amenity): void {
    this.editingEntity.set(entity);
  }

  onSave(event: { formValue: any; isActive: boolean }): void {
    const { formValue, isActive } = event;
    if (this.editingEntity()) {
      // For amenities, isActive maps to isAvailable
      const dto: UpdateAmenityDTO = {
        name: formValue.name,
        description: formValue.description,
        price: formValue.price,
        isAvailable: isActive,
      };
      this.amenityApi
        .update(this.editingEntity()!.id, dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Amenity updated', 'Close', { duration: 3000 });
            this.editingEntity.set(null);
            this.fetchData();
          },
          error: (err: any) =>
            this.snackBar.open(
              err instanceof Error ? err.message : 'Unexpected error',
              'Close',
              { duration: 5000 },
            ),
        });
    } else {
      const dto: CreateAmenityDTO = {
        name: formValue.name,
        description: formValue.description,
        price: formValue.price,
      };
      this.amenityApi
        .create(dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Amenity created', 'Close', { duration: 3000 });
            this.fetchData();
          },
          error: (err: any) =>
            this.snackBar.open(
              err instanceof Error ? err.message : 'Unexpected error',
              'Close',
              { duration: 5000 },
            ),
        });
    }
  }

  // Search change: update searchQuery, reset page, save state, fetch
  onSearchChange(query: string): void {
    this.searchQuery.set(query.trim() || '');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onFilterChange(filters: Record<string, any>): void {
    const val = filters['isAvailable'];
    this.availabilityFilter.set(val === '' || val === null ? null : val);
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  // Sort change: update sort field/direction, reset page, save, fetch
  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    if (!event.active || !event.direction) return;
    this.sortField.set(event.active);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  // Page change: update page index/size, save state, fetch
  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.saveState();
    this.fetchData();
  }

  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;
      if (typeof parsed.searchQuery === 'string') this.searchQuery.set(parsed.searchQuery);
      if (['name', 'price', 'isAvailable'].includes(parsed.sortField))
        this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean')
        this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0)
        this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0)
        this.pageSize.set(parsed.pageSize);
      if (parsed.availabilityFilter === null || typeof parsed.availabilityFilter === 'boolean') {
        this.availabilityFilter.set(parsed.availabilityFilter);
      }
    } catch {
      /* fallback silently */
    }
  }

  private saveState(): void {
    sessionStorage.setItem(
      this.STORAGE_KEY,
      JSON.stringify({
        searchQuery: this.searchQuery(),
        sortField: this.sortField(),
        sortDescending: this.sortDescending(),
        pageIndex: this.pageIndex(),
        pageSize: this.pageSize(),
        availabilityFilter: this.availabilityFilter(),
      }),
    );
  }
}


# /Frontend/src/app/features/admin/pages/management/menu-management.component.html

<app-generic-crud
  [config]="crudConfig"
  [searchQuery]="searchQuery()"
  (edit)="onEdit($event)"
  (searchChange)="onSearchChange($event)"
  (filterChange)="onFilterChange($event)"
  (sortChange)="onSortChange($event)"
  (pageChange)="onPageChange($event)"
  (save)="onSave($event)"
></app-generic-crud>


# /Frontend/src/app/features/admin/pages/management/menu-management.component.scss

// Component-scoped styles for menu management if needed


# /Frontend/src/app/features/admin/pages/management/menu-management.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs/operators';

import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
import {
  CrudConfig,
  ColumnDef,
  FilterDef,
  FormFieldDef,
} from '../../../../shared/models/crud-config.model';
import { MenuItemApiService } from '../../services/menu-item-api.service';
import { MenuItem, CreateMenuItemDTO, UpdateMenuItemDTO } from '../../models/menu-item.model';

/** Validator that requires at least one letter if a value is present */
function optionalLetterPattern(
  control: AbstractControl,
): ValidationErrors | null {
  const value = control.value as string;
  if (!value || value.trim().length === 0) {
    return null; // empty is valid
  }
  const regex = /^(?=.*[a-zA-Z])[a-zA-Z0-9\s\-']+$/;
  return regex.test(value) ? null : { pattern: true };
}

@Component({
  selector: 'app-menu-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    GenericCrudComponent,
  ],
  templateUrl: './menu-management.component.html',
  styleUrls: ['./menu-management.component.scss'],
})
export class MenuManagementComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly snackBar = inject(MatSnackBar);
  private readonly menuItemApi = inject(MenuItemApiService);

  private readonly STORAGE_KEY = 'menuState';

  data = signal<MenuItem[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('name');
  sortDescending = signal(false);
  searchQuery = signal('');
  availabilityFilter = signal<boolean | null>(null); // null = all, true = available, false = unavailable
  editingEntity = signal<MenuItem | null>(null);

  crudConfig: CrudConfig<MenuItem> = {
    entityName: 'Menu Item',
    entityNamePlural: 'Menu Items',
    columns: [
      { header: 'Name', field: 'name', sortable: true, getValue: (r) => r.name },
      {
        header: 'Category',
        field: 'category',
        sortable: false,
        getValue: (r) => r.category || '—',
      },
      { header: 'Price', field: 'price', sortable: true, getValue: (r) => `$${r.price}` },
      {
        header: 'Available',
        field: 'isAvailable',
        sortable: true,
        getValue: (r) => (r.isAvailable ? 'Yes' : 'No'),
      },
    ],
    filters: [
      {
        key: 'isAvailable',
        label: 'Availability',
        options: [
          { value: null, label: 'All' },
          { value: true, label: 'Available' },
          { value: false, label: 'Unavailable' },
        ],
      },
    ],
    formFields: [
      {
        key: 'name',
        label: 'Name',
        type: 'text',
        validators: [
          Validators.required,
          Validators.maxLength(100),
          Validators.minLength(1),
          Validators.pattern(/^(?=.*[a-zA-Z])[a-zA-Z0-9\s\-']+$/),
        ],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'category',
        label: 'Category',
        type: 'text',
        validators: [Validators.maxLength(100), optionalLetterPattern],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'price',
        label: 'Price',
        type: 'number',
        validators: [Validators.required, Validators.min(0)],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'isAvailable',
        label: 'Available',
        type: 'toggle',
        validators: [],
        showInAdd: false, // not shown on creation (defaults to true)
        showInEdit: true,
      },
    ],
    supportsToggle: true,
    data: this.data,
    totalCount: this.totalCount,
    loading: this.loading,
    error: this.error,
    pageIndex: this.pageIndex,
    pageSize: this.pageSize,
  };

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.menuItemApi
      .getAll({
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        searchQuery: this.searchQuery() || undefined,
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
        isAvailable: this.availabilityFilter() ?? undefined,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.data);
          this.totalCount.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
            this.saveState();
          }
        },
        error: (err: any) =>
          this.error.set(err instanceof Error ? err.message : 'Unexpected error'),
      });
  }

  onEdit(entity: MenuItem): void {
    this.editingEntity.set(entity);
  }

  onSave(event: { formValue: any; isActive: boolean }): void {
    const { formValue, isActive } = event;
    if (this.editingEntity()) {
      const dto: UpdateMenuItemDTO = {
        name: formValue.name,
        price: formValue.price,
        category: formValue.category ?? '',
        isAvailable: isActive,
      };
      this.menuItemApi
        .update(this.editingEntity()!.id, dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Menu item updated', 'Close', { duration: 3000 });
            this.editingEntity.set(null);
            this.fetchData();
          },
          error: (err: any) =>
            this.snackBar.open(
              err instanceof Error ? err.message : 'Unexpected error',
              'Close',
              { duration: 5000 },
            ),
        });
    } else {
      const dto: CreateMenuItemDTO = {
        name: formValue.name,
        price: formValue.price,
        category: formValue.category ?? '',
        isAvailable: true, // new items are always available by default
      };
      this.menuItemApi
        .create(dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Menu item created', 'Close', { duration: 3000 });
            this.fetchData();
          },
          error: (err: any) =>
            this.snackBar.open(
              err instanceof Error ? err.message : 'Unexpected error',
              'Close',
              { duration: 5000 },
            ),
        });
    }
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query.trim() || '');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onFilterChange(filters: Record<string, any>): void {
    const val = filters['isAvailable'];
    this.availabilityFilter.set(val === '' || val === undefined ? null : val);
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    if (!event.active || !event.direction) return;
    this.sortField.set(event.active);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.saveState();
    this.fetchData();
  }

  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;
      if (typeof parsed.searchQuery === 'string') this.searchQuery.set(parsed.searchQuery);
      if (['name', 'price', 'isAvailable'].includes(parsed.sortField))
        this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean')
        this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0)
        this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0)
        this.pageSize.set(parsed.pageSize);
      if (parsed.availabilityFilter === null || typeof parsed.availabilityFilter === 'boolean') {
        this.availabilityFilter.set(parsed.availabilityFilter);
      }
    } catch {
      /* fallback silently */
    }
  }

  private saveState(): void {
    sessionStorage.setItem(
      this.STORAGE_KEY,
      JSON.stringify({
        searchQuery: this.searchQuery(),
        sortField: this.sortField(),
        sortDescending: this.sortDescending(),
        pageIndex: this.pageIndex(),
        pageSize: this.pageSize(),
        availabilityFilter: this.availabilityFilter(),
      }),
    );
  }
}


# /Frontend/src/app/features/admin/pages/management/room-management.component.html

<!-- View toggle (mobile only) -->
@if (isMobile()) {
<div class="view-toggle">
  <mat-button-toggle-group [formControl]="viewMode" aria-label="View mode">
    <mat-button-toggle value="table">
      <mat-icon>table_chart</mat-icon> Table
    </mat-button-toggle>
    <mat-button-toggle value="grid">
      <mat-icon>grid_view</mat-icon> Grid
    </mat-button-toggle>
  </mat-button-toggle-group>
</div>
}

@if (!isMobile() || viewMode.value === 'grid') {
<div class="status-grid-row">
  <app-room-status-grid
    [roomTypeId]="roomTypeFilter()"
    (roomClicked)="onGridRoomClicked($event)"
  ></app-room-status-grid>
</div>
} @if (!isMobile() || viewMode.value === 'table') {
<div class="table-section">
  <app-generic-crud [config]="crudConfig" [searchQuery]="searchQuery()" (edit)="onEdit($event)"
    (searchChange)="onSearchChange($event)" (filterChange)="onFilterChange($event)"
    (sortChange)="onSortChange($event)" (pageChange)="onPageChange($event)"
    (save)="onSave($event)"></app-generic-crud>
</div>
}

# /Frontend/src/app/features/admin/pages/management/room-management.component.scss

:host {
  display: block;
  height: 100%;
}

.view-toggle {
  display: flex;
  justify-content: center;
  padding: 12px 16px 0;
}

.status-grid-row {
  padding: 16px 16px 0;
  overflow-x: auto;
}

.table-section {
  max-width: 100%;
  overflow-x: auto;
}

.hidden {
  display: none !important;
}


# /Frontend/src/app/features/admin/pages/management/room-management.component.ts

import { Component, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BreakpointObserver } from '@angular/cdk/layout';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { map } from 'rxjs/operators';

import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
import { CrudConfig } from '../../../../shared/models/crud-config.model';
import { RoomStatusGridComponent } from '../../components/room-status-grid/room-status-grid.component';
import { RoomApiService } from '../../services/room-api.service';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { Room, CreateRoomDTO, UpdateRoomDTO, RoomStatus } from '../../models/room.model';

interface RoomsState {
  roomTypeId: number | null;
  includeRetired: boolean;
  searchQuery: string;
  sortField: string;
  sortDescending: boolean;
  pageIndex: number;
  pageSize: number;
}

@Component({
  selector: 'app-room-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonToggleModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    GenericCrudComponent,
    RoomStatusGridComponent,
  ],
  templateUrl: './room-management.component.html',
  styleUrls: ['./room-management.component.scss'],
})
export class RoomManagementComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly roomApi = inject(RoomApiService);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly breakpointObserver = inject(BreakpointObserver);

  private readonly STORAGE_KEY = 'roomsState';

  // Data signals
  data = signal<Room[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query param signals
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('id');
  sortDescending = signal(false);
  searchQuery = signal('');
  roomTypeFilter = signal<number | null>(null);
  includeRetired = signal(false);
  editingEntity = signal<Room | null>(null);

  // Mobile
  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 768px)').pipe(map((r) => r.matches)),
    { initialValue: false },
  );
  viewMode = new FormControl<'table' | 'grid'>('table', { nonNullable: true });

  // CrudConfig
  crudConfig: CrudConfig<Room> = {
    entityName: 'Room',
    entityNamePlural: 'Rooms',
    columns: [
      {
        header: 'Room #',
        field: 'roomNumber',
        sortable: false,
        getValue: (r: Room) => r.roomNumber,
      },
      {
        header: 'Type',
        field: 'roomTypeName',
        sortable: false,
        getValue: (r: Room) => r.roomTypeName,
      },
      {
        header: 'Base Price',
        field: 'basePrice',
        sortable: true,
        getValue: (r: Room) => `$${r.basePrice}`,
      },
      {
        header: 'Max Occ.',
        field: 'maxOccupancy',
        sortable: true,
        getValue: (r: Room) => String(r.maxOccupancy),
      },
      // {
      //   header: 'Active',
      //   field: 'isActive',
      //   sortable: false,
      //   getValue: (r: Room) => (r.isActive ? 'Yes' : 'No'),
      // },
      {
        header: 'Available',
        field: 'isAvailable',
        sortable: false,
        getValue: (r: Room) => (r.isAvailable ? 'Yes' : 'No'),
      },
    ],
    filters: [
      {
        key: 'roomTypeId',
        label: 'Room Type',
        options: [], // populated dynamically
      },
      {
        key: 'includeRetired',
        label: 'Status',
        options: [
          { value: false, label: 'Active Only' },
          { value: true, label: 'All' },
        ],
      },
    ],
    formFields: [
      {
        key: 'roomNumber',
        label: 'Room Number',
        type: 'text',
        validators: [Validators.required, Validators.maxLength(100)],
      },
      {
        key: 'roomTypeId',
        label: 'Room Type',
        type: 'select',
        validators: [Validators.required],
        options: [], // populated dynamically
      },
    ],
    supportsToggle: true,
    data: this.data,
    totalCount: this.totalCount,
    loading: this.loading,
    error: this.error,
    pageIndex: this.pageIndex,
    pageSize: this.pageSize,
  };

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
    // Load room types for dropdowns
    this.roomTypeApi
      .getAll({
        includeRetired: false,
        pageNumber: 1,
        pageSize: 100,
        sortBy: 'name',
        sortDescending: false,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((res) => {
        const options = res.data.map((rt) => ({ value: rt.id, label: rt.name }));
        this.crudConfig.filters[0].options = options;
        this.crudConfig.formFields[1].options = options;
      });
  }

  private fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.roomApi
      .getAll({
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        roomTypeId: this.roomTypeFilter() ?? undefined,
        includeRetired: this.includeRetired(),
        searchQuery: this.searchQuery() || undefined,
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.data);
          this.totalCount.set(res.totalCount);
          // Normalize page if out of bounds
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
            this.fetchData();
          }
        },
        error: (err: Error) => this.error.set(err.message),
      });
  }

  private saveState(): void {
    const state: RoomsState = {
      roomTypeId: this.roomTypeFilter(),
      includeRetired: this.includeRetired(),
      searchQuery: this.searchQuery(),
      sortField: this.sortField(),
      sortDescending: this.sortDescending(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
    };
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
  }

  private restoreState(): void {
    const raw = sessionStorage.getItem(this.STORAGE_KEY);
    if (!raw) return;
    try {
      const state: RoomsState = JSON.parse(raw);
      this.roomTypeFilter.set(state.roomTypeId ?? null);
      this.includeRetired.set(state.includeRetired ?? false);
      this.searchQuery.set(state.searchQuery ?? '');
      this.sortField.set(state.sortField ?? 'id');
      this.sortDescending.set(state.sortDescending ?? false);
      this.pageIndex.set(state.pageIndex ?? 0);
      this.pageSize.set(state.pageSize ?? 10);
    } catch {
      // Ignore corrupt state
    }
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query.trim());
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onFilterChange(filters: Record<string, any>): void {
    this.roomTypeFilter.set(filters['roomTypeId'] ?? null);
    this.includeRetired.set(filters['includeRetired'] ?? false);
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    this.sortField.set(event.active || 'id');
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.saveState();
    this.fetchData();
  }

  onEdit(entity: Room): void {
    this.editingEntity.set(entity);
  }

  onSave(event: { formValue: any; isActive: boolean; entityId?: number }): void {
    const { formValue, isActive } = event;
    if (this.editingEntity()) {
      const dto: UpdateRoomDTO = { ...formValue, isActive };
      this.roomApi
        .update(this.editingEntity()!.id, dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Room updated', 'Close', { duration: 3000 });
            this.editingEntity.set(null);
            this.fetchData();
          },
          error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 }),
        });
    } else {
      const dto: CreateRoomDTO = formValue;
      this.roomApi
        .create(dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Room created', 'Close', { duration: 3000 });
            this.fetchData();
          },
          error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 }),
        });
    }
  }

  onGridRoomClicked(room: RoomStatus): void {
    this.searchQuery.set(room.roomNumber);
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }
}


# /Frontend/src/app/features/admin/pages/management/room-type-management.component.html

<app-generic-crud
  [config]="crudConfig"
  [searchQuery]="searchQuery()"
  (searchChange)="onSearchChange($event)"
  (filterChange)="onFilterChange($event)"
  (sortChange)="onSortChange($event)"
  (pageChange)="onPageChange($event)"
  (save)="onSave($event)"
></app-generic-crud>


# /Frontend/src/app/features/admin/pages/management/room-type-management.component.scss

:host {
  display: block;
  height: 100%;
}


# /Frontend/src/app/features/admin/pages/management/room-type-management.component.ts

import { Component, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
import { CrudConfig } from '../../../../shared/models/crud-config.model';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { RoomType, CreateRoomTypeDTO, UpdateRoomTypeDTO } from '../../models/room-type.model';

const STATE_KEY = 'roomTypesState';

interface RoomTypesState {
  includeRetired: boolean;
  sortField: string;
  sortDescending: boolean;
  pageIndex: number;
  pageSize: number;
  searchQuery: string;
}

@Component({
  selector: 'app-room-type-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatSnackBarModule, GenericCrudComponent],
  templateUrl: './room-type-management.component.html',
  styleUrls: ['./room-type-management.component.scss'],
})
export class RoomTypeManagementComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly snackBar = inject(MatSnackBar);

  // Data signals
  data = signal<RoomType[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query param signals
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('name');
  sortDescending = signal(false);
  includeRetired = signal(false);
  searchQuery = signal('');

  // CrudConfig
  crudConfig: CrudConfig<RoomType> = {
    entityName: 'Room Type',
    entityNamePlural: 'Room Types',
    columns: [
      { header: 'Name', field: 'name', sortable: true, getValue: (r: RoomType) => r.name },
      {
        header: 'Base Price',
        field: 'basePrice',
        sortable: true,
        getValue: (r: RoomType) => `$${r.basePrice}`,
      },
      {
        header: 'Max Occupancy',
        field: 'maxOccupancy',
        sortable: true,
        getValue: (r: RoomType) => String(r.maxOccupancy),
      },
      {
        header: 'Active',
        field: 'isActive',
        sortable: false,
        getValue: (r: RoomType) => (r.isActive ? 'Yes' : 'No'),
      },
    ],
    filters: [
      {
        key: 'includeRetired',
        label: 'Status',
        options: [
          { value: false, label: 'Active Only' },
          { value: true, label: 'All' },
        ],
      },
    ],
    formFields: [
      {
        key: 'name',
        label: 'Name',
        type: 'text',
        validators: [Validators.required, Validators.maxLength(100)],
      },
      {
        key: 'description',
        label: 'Description',
        type: 'textarea',
        validators: [Validators.maxLength(500)],
      },
      {
        key: 'basePrice',
        label: 'Base Price',
        type: 'number',
        validators: [Validators.required, Validators.min(0)],
      },
      {
        key: 'maxOccupancy',
        label: 'Max Occupancy',
        type: 'number',
        validators: [Validators.required, Validators.min(1)],
      },
      {
        key: 'squareFootage',
        label: 'Square Footage',
        type: 'number',
        validators: [],
      },
      {
        key: 'bedConfiguration',
        label: 'Bed Configuration',
        type: 'keyValueList',
        validators: [],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'imageUrls',
        label: 'Images',
        type: 'imageUrlList',
        validators: [],
        showInAdd: true,
        showInEdit: true,
      },
    ],
    supportsToggle: true,
    data: this.data,
    totalCount: this.totalCount,
    loading: this.loading,
    error: this.error,
    pageIndex: this.pageIndex,
    pageSize: this.pageSize,
  };

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
  }

  private fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.roomTypeApi
      .getAll({
        includeRetired: this.includeRetired(),
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
        searchQuery: this.searchQuery() || undefined,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.data);
          this.totalCount.set(res.totalCount);
        },
        error: (err: Error) => this.error.set(err.message),
      });
  }

  private saveState(): void {
    const state: RoomTypesState = {
      includeRetired: this.includeRetired(),
      sortField: this.sortField(),
      sortDescending: this.sortDescending(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
      searchQuery: this.searchQuery(),
    };
    sessionStorage.setItem(STATE_KEY, JSON.stringify(state));
  }

  private restoreState(): void {
    const raw = sessionStorage.getItem(STATE_KEY);
    if (!raw) return;
    try {
      const state: RoomTypesState = JSON.parse(raw);
      this.includeRetired.set(state.includeRetired ?? false);
      this.sortField.set(state.sortField ?? 'name');
      this.sortDescending.set(state.sortDescending ?? false);
      this.pageIndex.set(state.pageIndex ?? 0);
      this.pageSize.set(state.pageSize ?? 10);
      this.searchQuery.set(state.searchQuery ?? '');
    } catch {
      // Ignore corrupt state
    }
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query.trim() || '');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onFilterChange(filters: Record<string, any>): void {
    if ('includeRetired' in filters) {
      this.includeRetired.set(filters['includeRetired']);
    }
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    this.sortField.set(event.active || 'name');
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.saveState();
    this.fetchData();
  }

  onSave(event: { formValue: any; isActive: boolean; entityId?: number }): void {
    const { formValue, isActive, entityId } = event;

    const imageUrls = formValue.imageUrls ?? [];
    const bedConfig = formValue.bedConfiguration || null;

    if (entityId != null) {
      // Edit mode
      const dto: UpdateRoomTypeDTO = {
        name: formValue.name,
        description: formValue.description,
        basePrice: formValue.basePrice,
        maxOccupancy: formValue.maxOccupancy,
        imageUrls: imageUrls,
        squareFootage: formValue.squareFootage,
        bedConfiguration: bedConfig,
        isActive: isActive,
      };
      this.roomTypeApi
        .update(entityId, dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Room type updated', 'Close', { duration: 3000 });
            this.fetchData();
          },
          error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 }),
        });
    } else {
      // Create mode
      const dto: CreateRoomTypeDTO = {
        name: formValue.name,
        description: formValue.description,
        basePrice: formValue.basePrice,
        maxOccupancy: formValue.maxOccupancy,
        imageUrls: imageUrls,
        squareFootage: formValue.squareFootage,
        bedConfiguration: bedConfig,
      };
      this.roomTypeApi
        .create(dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Room type created', 'Close', { duration: 3000 });
            this.fetchData();
          },
          error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 }),
        });
    }
  }
}


# /Frontend/src/app/features/admin/pages/management/staff-management.component.html

<app-generic-crud
  [config]="crudConfig"
  [searchQuery]="searchQuery()"
  (edit)="onEdit($event)"
  (searchChange)="onSearchChange($event)"
  (filterChange)="onFilterChange($event)"
  (sortChange)="onSortChange($event)"
  (pageChange)="onPageChange($event)"
  (save)="onSave($event)"
></app-generic-crud>


# /Frontend/src/app/features/admin/pages/management/staff-management.component.scss

// Component-scoped styles for staff management if needed


# /Frontend/src/app/features/admin/pages/management/staff-management.component.ts

import { Component, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
import {
  CrudConfig,
  ColumnDef,
  FilterDef,
  FormFieldDef,
} from '../../../../shared/models/crud-config.model';
import { StaffApiService } from '../../services/staff-api.service';
import {
  Staff,
  CreateStaffDTO,
  UpdateStaffDTO,
  StaffRole,
} from '../../models/staff.model';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-staff-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    MatDialogModule,
    GenericCrudComponent,
  ],
  templateUrl: './staff-management.component.html',
  styleUrls: ['./staff-management.component.scss'],
})
export class StaffManagementComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly staffApi = inject(StaffApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  private readonly STORAGE_KEY = 'staffState';

  data = signal<Staff[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query params
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('isActive');
  sortDescending = signal(false);
  searchQuery = signal('');          // parent's own search state, updated via searchChange
  includeFired = signal(false);      // false = active only, true = all
  editingEntity = signal<Staff | null>(null);

  crudConfig: CrudConfig<Staff> = {
    entityName: 'Staff',
    entityNamePlural: 'Staff',
    columns: [
      {
        header: 'First Name',
        field: 'firstName',
        sortable: true,
        getValue: (r: Staff) => r.firstName,
      },
      {
        header: 'Last Name',
        field: 'lastName',
        sortable: true,
        getValue: (r: Staff) => r.lastName,
      },
      {
        header: 'Email',
        field: 'email',
        sortable: true,
        getValue: (r: Staff) => r.email,
      },
      { header: 'Role', field: 'role', sortable: true, getValue: (r: Staff) => r.role },
      {
        header: 'Active',
        field: 'isActive',
        sortable: true,
        getValue: (r: Staff) => (r.isActive ? 'Yes' : 'No'),
      },
    ],
    filters: [
      {
        key: 'includeFired',
        label: 'Status',
        options: [
          { value: false, label: 'Active Only' },
          { value: true, label: 'All' },
        ],
      },
    ],
    formFields: [
      {
        key: 'email',
        label: 'Email',
        type: 'email',
        validators: [
          Validators.required,
          Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/),
        ],
        showInAdd: true,
        showInEdit: false,
      },
      {
        key: 'password',
        label: 'Password',
        type: 'password',
        validators: [
          Validators.required,
          Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/),
        ],
        showInAdd: true,
        showInEdit: false,
      },
      {
        key: 'firstName',
        label: 'First Name',
        type: 'text',
        validators: [
          Validators.required,
          Validators.pattern(/^[a-zA-ZÀ-ž\s\-']{2,50}$/),
        ],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'lastName',
        label: 'Last Name',
        type: 'text',
        validators: [
          Validators.required,
          Validators.pattern(/^[a-zA-ZÀ-ž\s\-']{2,50}$/),
        ],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'role',
        label: 'Role',
        type: 'select',
        options: [
          { value: 'Admin', label: 'Admin' },
          { value: 'FrontDesk', label: 'Front Desk' },
          { value: 'Kitchen', label: 'Kitchen' },
          { value: 'Housekeeping', label: 'Housekeeping' },
          { value: 'Maintenance', label: 'Maintenance' },
        ],
        validators: [Validators.required],
        showInAdd: true,
        showInEdit: true,
      },
    ],
    supportsToggle: true,
    data: this.data,
    totalCount: this.totalCount,
    loading: this.loading,
    error: this.error,
    pageIndex: this.pageIndex,
    pageSize: this.pageSize,
  };

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.staffApi.getAll({
      includeFired: this.includeFired(),
      pageNumber: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      sortDescending: this.sortDescending(),
      searchQuery: this.searchQuery() || undefined,
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => {
        this.data.set(res.data);
        this.totalCount.set(res.totalCount);
        // Page normalization – only after successful data update
        const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
        if (this.pageIndex() > maxPage) {
          this.pageIndex.set(maxPage);
          this.saveState();
        }
      },
      error: (err: Error) => this.error.set(err.message)
    });
  }

  onEdit(entity: Staff): void {
    this.editingEntity.set(entity);
  }

  onSave(event: { formValue: any; isActive: boolean }): void {
    const { formValue, isActive } = event;
    if (this.editingEntity()) {
      this.performUpdate(formValue, isActive);
    } else {
      this.performCreate(formValue);
    }
  }

  private performUpdate(formValue: any, isActive: boolean): void {
    const dto: UpdateStaffDTO = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      role: formValue.role as StaffRole,
      isActive: isActive,
    };
    this.staffApi.update(this.editingEntity()!.id, dto).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Staff updated', 'Close', { duration: 3000 });
        this.editingEntity.set(null);
        this.fetchData();
      },
      error: (err: any) => {
        const message = err instanceof Error ? err.message : 'Unexpected error';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  private performCreate(formValue: any): void {
    const dto: CreateStaffDTO = {
      email: formValue.email,
      password: formValue.password,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      role: formValue.role as StaffRole,
    };
    this.staffApi.create(dto).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Staff created', 'Close', { duration: 3000 });
        this.fetchData();
      },
      error: (err: any) => {
        const message = err instanceof Error ? err.message : 'Unexpected error';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query.trim() || '');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onFilterChange(filters: Record<string, any>): void {
    if ('includeFired' in filters) {
      this.includeFired.set(filters['includeFired'] ?? false);
    }
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    if (!event.active || !event.direction) return;
    this.sortField.set(event.active);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.saveState();
    this.fetchData();
  }

  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;
      // Validate types
      if (typeof parsed.includeFired === 'boolean') this.includeFired.set(parsed.includeFired);
      if (typeof parsed.searchQuery === 'string') this.searchQuery.set(parsed.searchQuery);
      if (['firstName', 'lastName', 'email', 'role', 'isActive'].includes(parsed.sortField)) this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean') this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0) this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0) this.pageSize.set(parsed.pageSize);
    } catch {
      // fallback silently to defaults
    }
  }

  private saveState(): void {
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify({
      includeFired: this.includeFired(),
      searchQuery: this.searchQuery(),
      sortField: this.sortField(),
      sortDescending: this.sortDescending(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
    }));
  }
}


# /Frontend/src/app/features/admin/pages/oversight/analytics.component.html

<div class="analytics-page">
  <!-- Controls: Presets + Custom Date + Category Dropdown -->
  <div class="controls">
    <div class="date-controls">
      <mat-button-toggle-group
        [formControl]="presetControl"
        (change)="onPresetChange()"
      >
        <mat-button-toggle value="last7">Last 7 days</mat-button-toggle>
        <mat-button-toggle value="last30">Last 30 days</mat-button-toggle>
        <mat-button-toggle value="thisMonth">This month</mat-button-toggle>
        <mat-button-toggle value="custom">Custom</mat-button-toggle>
      </mat-button-toggle-group>
      @if (presetControl.value === 'custom') {
      <mat-form-field appearance="outline">
        <mat-label>Start date</mat-label>
        <input
          matInput
          [matDatepicker]="startPicker"
          [formControl]="startDateCtrl"
        />
        <mat-datepicker-toggle
          matSuffix
          [for]="startPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker #startPicker></mat-datepicker>
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>End date</mat-label>
        <input
          matInput
          [matDatepicker]="endPicker"
          [formControl]="endDateCtrl"
        />
        <mat-datepicker-toggle
          matSuffix
          [for]="endPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker #endPicker></mat-datepicker>
      </mat-form-field>
      <button
        mat-raised-button
        color="primary"
        (click)="applyCustomRange()"
      >
        Apply
      </button>
      }
    </div>
    <mat-form-field
      appearance="outline"
      class="category-select"
    >
      <mat-label>Category</mat-label>
      <mat-select
        [formControl]="categoryControl"
        (selectionChange)="onCategoryChange()"
      >
        <mat-option value="all">All</mat-option>
        <mat-option value="revenue">Revenue</mat-option>
        <mat-option value="operations">Operations</mat-option>
        <mat-option value="guests">Guests</mat-option>
      </mat-select>
    </mat-form-field>
  </div>

  <!-- Loading / Error -->
  @if (loading() && !analytics()) {
  <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchData()"
    >
      Retry
    </button>
  </app-alert>
  } @else {
  <!-- KPI Summary Cards (top row) -->
  <div class="kpi-row">
    <mat-card
      ><mat-card-title>Total Revenue</mat-card-title
      ><mat-card-content
        >{{ analytics()?.totalRevenue | currency }}</mat-card-content
      ></mat-card
    >
    <mat-card
      ><mat-card-title>Occupancy Rate</mat-card-title
      ><mat-card-content
        >{{ analytics()?.occupancyRate }}%</mat-card-content
      ></mat-card
    >
    <mat-card
      ><mat-card-title>Guest Satisfaction</mat-card-title
      ><mat-card-content
        >{{ analytics()?.guestSatisfactionScore }}%</mat-card-content
      ></mat-card
    >
    <mat-card
      ><mat-card-title>Avg Daily Rate</mat-card-title
      ><mat-card-content
        >{{ analytics()?.averageDailyRate | currency }}</mat-card-content
      ></mat-card
    >
  </div>

  <!-- Fixed Chart Grid -->
  <div class="charts-grid">
    <div class="chart-container">
      <div
        echarts
        [options]="barChartOptions()"
        class="chart"
      ></div>
    </div>
    <div class="chart-container">
      <div
        echarts
        [options]="lineChartOptions()"
        class="chart"
      ></div>
    </div>
    <div class="chart-container">
      <div
        echarts
        [options]="radarChartOptions()"
        class="chart"
      ></div>
    </div>
    @if (categoryControl.value !== 'revenue' && categoryControl.value !== 'operations') {
    <div class="chart-container">
      <div
        echarts
        [options]="pieChartOptions()"
        class="chart"
      ></div>
    </div>
    }
  </div>
  }
</div>


# /Frontend/src/app/features/admin/pages/oversight/analytics.component.scss

.analytics-page {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;

  .controls {
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 16px;

    .date-controls {
      display: flex;
      align-items: center;
      flex-wrap: wrap;
      gap: 12px;

      mat-form-field {
        width: 160px;
        margin-bottom: -1.25em; /* alignment fix for inline display */
      }

      button {
        height: 48px;
        align-self: center;
      }
    }

    .category-select {
      width: 200px;
      margin-bottom: -1.25em;
    }
  }

  .loading {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 100px 0;
  }

  .kpi-row {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: 16px;

    mat-card {
      padding: 20px;
      border-radius: 12px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
      border: 1px solid rgba(0, 0, 0, 0.05);

      mat-card-title {
        font-size: 14px;
        font-weight: 500;
        color: #757575;
        margin-bottom: 8px;
      }

      mat-card-content {
        font-size: 28px;
        font-weight: 700;
        color: #212121;
      }
    }
  }

  .charts-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 24px;

    .chart-container {
      background: #ffffff;
      border-radius: 12px;
      padding: 20px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
      border: 1px solid rgba(0, 0, 0, 0.05);
      min-height: 440px;

      .chart {
        width: 100%;
        height: 400px;
      }
    }
  }
}

/* Responsive Overrides */
@media (max-width: 1024px) {
  .analytics-page {
    .charts-grid {
      grid-template-columns: 1fr;
    }
  }
}

@media (max-width: 768px) {
  .analytics-page {
    padding: 16px;

    .controls {
      flex-direction: column;
      align-items: stretch;

      .date-controls {
        flex-direction: column;
        align-items: stretch;

        mat-button-toggle-group {
          align-self: center;
        }

        mat-form-field {
          width: 100%;
        }
      }

      .category-select {
        width: 100%;
      }
    }

    .charts-grid {
      .chart-container {
        min-height: 340px;
        padding: 12px;

        .chart {
          height: 300px;
        }
      }
    }
  }
}


# /Frontend/src/app/features/admin/pages/oversight/analytics.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSelectModule } from '@angular/material/select';
import { NgxEchartsDirective } from 'ngx-echarts';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs/operators';

import { AnalyticsApiService } from '../../services/analytics-api.service';
import { AnalyticsDashboardDTO } from '../../models/analytics-dashboard.dto';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

function optionalLetterPattern() {
  return null;
}

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatButtonToggleModule,
    MatSelectModule,
    NgxEchartsDirective,
    AlertComponent,
  ],
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss'],
})
export class AnalyticsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly analyticsApi = inject(AnalyticsApiService);

  // Data Signals
  analytics = signal<AnalyticsDashboardDTO | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  // Date controls
  presetControl = new FormControl<'last7' | 'last30' | 'thisMonth' | 'custom'>(
    'last7',
    { nonNullable: true },
  );
  startDateCtrl = new FormControl<Date | null>(null);
  endDateCtrl = new FormControl<Date | null>(null);

  // Category dropdown and reactive signal
  categoryControl = new FormControl<'all' | 'revenue' | 'operations' | 'guests'>(
    'all',
    { nonNullable: true },
  );
  categorySignal = signal<'all' | 'revenue' | 'operations' | 'guests'>('all');

  barChartOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    const cat = this.categorySignal();
    let xData: string[] = [];
    let yData: number[] = [];
    switch (cat) {
      case 'all':
      case 'revenue':
        xData = ['Total Revenue', 'Gross Turnover', 'RevPAR', 'Avg Daily Rate'];
        yData = [d.totalRevenue, d.grossTurnover, d.revPAR, d.averageDailyRate];
        break;
      case 'operations':
        xData = ['Occupancy', 'Cancellation', 'Length of Stay', 'HK Turnaround'];
        yData = [
          d.occupancyRate,
          d.cancellationRate,
          d.averageLengthOfStay,
          d.averageHousekeepingTurnaroundMinutes,
        ];
        break;
      case 'guests':
        xData = ['Satisfaction', 'Food Spend', 'Amenity Spend'];
        yData = [
          d.guestSatisfactionScore,
          d.nonRoomExpenditure.totalFoodSpend,
          d.nonRoomExpenditure.totalAmenitySpend,
        ];
        break;
    }
    return {
      title: { text: 'Overview' },
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: xData },
      yAxis: { type: 'value' },
      series: [{ type: 'bar', data: yData, color: '#1976d2' }],
    };
  });

  lineChartOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    const cat = this.categorySignal();
    let xData: string[] = [];
    let yData: number[] = [];
    switch (cat) {
      case 'all':
      case 'revenue':
        xData = ['Total Revenue', 'Gross Turnover', 'RevPAR', 'Avg Daily Rate'];
        yData = [d.totalRevenue, d.grossTurnover, d.revPAR, d.averageDailyRate];
        break;
      case 'operations':
        xData = ['Occupancy', 'Cancellation', 'Length of Stay', 'HK Turnaround'];
        yData = [
          d.occupancyRate,
          d.cancellationRate,
          d.averageLengthOfStay,
          d.averageHousekeepingTurnaroundMinutes,
        ];
        break;
      case 'guests':
        xData = ['Satisfaction', 'Food Spend', 'Amenity Spend'];
        yData = [
          d.guestSatisfactionScore,
          d.nonRoomExpenditure.totalFoodSpend,
          d.nonRoomExpenditure.totalAmenitySpend,
        ];
        break;
    }
    return {
      title: { text: 'Trend' },
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: xData },
      yAxis: { type: 'value' },
      series: [{ type: 'line', data: yData, color: '#388e3c' }],
    };
  });

  radarChartOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    const cat = this.categorySignal();
    let indicator: any[] = [];
    let value: number[] = [];
    switch (cat) {
      case 'all':
        indicator = [
          { name: 'Occupancy', max: 100 },
          { name: 'Cancellation', max: 50 },
          { name: 'Length of Stay', max: 30 },
          { name: 'Satisfaction', max: 100 },
        ];
        value = [
          d.occupancyRate,
          d.cancellationRate,
          d.averageLengthOfStay,
          d.guestSatisfactionScore,
        ];
        break;
      case 'revenue':
        indicator = [
          { name: 'Occupancy', max: 100 },
          { name: 'RevPAR', max: 2000 },
          { name: 'Avg Daily Rate', max: 500 },
          { name: 'Turnover', max: 20000 },
        ];
        value = [d.occupancyRate, d.revPAR, d.averageDailyRate, d.grossTurnover];
        break;
      case 'operations':
        indicator = [
          { name: 'Occupancy', max: 100 },
          { name: 'Cancellation', max: 50 },
          { name: 'Length of Stay', max: 30 },
          { name: 'Satisfaction', max: 100 },
        ];
        value = [
          d.occupancyRate,
          d.cancellationRate,
          d.averageLengthOfStay,
          d.guestSatisfactionScore,
        ];
        break;
      case 'guests':
        indicator = [
          { name: 'Satisfaction', max: 100 },
          { name: 'Occupancy', max: 100 },
          { name: 'Length of Stay', max: 30 },
        ];
        value = [
          d.guestSatisfactionScore,
          d.occupancyRate,
          d.averageLengthOfStay,
        ];
        break;
    }
    return {
      title: { text: 'Radar Overview' },
      radar: { indicator },
      series: [{ type: 'radar', data: [{ value, name: 'Current' }] }],
    };
  });

  pieChartOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    const cat = this.categorySignal();
    if (cat === 'revenue' || cat === 'operations') {
      return {}; // hidden (no data)
    }
    return {
      title: { text: 'Expenditure Breakdown' },
      tooltip: { trigger: 'item' },
      series: [
        {
          type: 'pie',
          data: [
            { name: 'Food', value: d.nonRoomExpenditure.totalFoodSpend },
            { name: 'Amenities', value: d.nonRoomExpenditure.totalAmenitySpend },
          ],
          label: { formatter: '{b}: {c} ({d}%)' },
        },
      ],
    };
  });

  ngOnInit(): void {
    this.fetchData();
  }

  fetchData(startDate?: string, endDate?: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.analyticsApi
      .getAnalytics({ startDate, endDate })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => this.analytics.set(data),
        error: (err: any) =>
          this.error.set(err instanceof Error ? err.message : 'Unexpected error'),
      });
  }

  onPresetChange(): void {
    const preset = this.presetControl.value;
    if (preset === 'custom') {
      return;
    }
    const dates = this.getPresetDates(preset);
    if (dates) {
      this.fetchData(dates.start, dates.end);
    } else {
      this.fetchData();
    }
  }

  applyCustomRange(): void {
    const start = this.startDateCtrl.value;
    const end = this.endDateCtrl.value;
    if (start && end) {
      const startDate = new Date(start);
      startDate.setHours(0, 0, 0, 0);
      const endDate = new Date(end);
      endDate.setHours(23, 59, 59, 999);
      this.fetchData(startDate.toISOString(), endDate.toISOString());
    }
  }

  onCategoryChange(): void {
    this.categorySignal.set(this.categoryControl.value);
  }

  private getPresetDates(preset: string): { start: string; end: string } | null {
    const now = new Date();
    let start: Date;
    let end: Date = now;
    switch (preset) {
      case 'last7':
        start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
        break;
      case 'last30':
        start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
        break;
      case 'thisMonth':
        start = new Date(now.getFullYear(), now.getMonth(), 1);
        break;
      default:
        return null;
    }
    start.setHours(0, 0, 0, 0);
    end.setHours(23, 59, 59, 999);
    return { start: start.toISOString(), end: end.toISOString() };
  }
}


# /Frontend/src/app/features/admin/pages/oversight/audit-log-detail-dialog.component.html

<h2 mat-dialog-title>Audit Entry #{{ data.id }}</h2>
<mat-dialog-content>
  <div class="detail-section">
    <h3>General Information</h3>
    <p><strong>Entity:</strong> {{ data.entityName }}</p>
    <p><strong>Action:</strong> {{ data.action }}</p>
    <p>
      <strong>Changed By:</strong> {{ data.changedByName }} ({{
      data.changedByEmail }})
    </p>
    <p><strong>Timestamp:</strong> {{ data.timestamp | date:'medium' }}</p>
  </div>
  <mat-divider></mat-divider>
  <div class="values-row">
    <div class="values-column">
      <h3>Old Values</h3>
      @if (data.oldValues && getKeys(data.oldValues).length > 0) {
      <div class="value-list">
        @for (key of getKeys(data.oldValues); track key) {
        <div class="value-item">
          <span class="key">{{ key }}:</span>
          <span class="val">{{ formatValue(data.oldValues[key]) }}</span>
        </div>
        }
      </div>
      } @else {
      <p><em>None (created)</em></p>
      }
    </div>
    <div class="values-column">
      <h3>New Values</h3>
      @if (data.newValues && getKeys(data.newValues).length > 0) {
      <div class="value-list">
        @for (key of getKeys(data.newValues); track key) {
        <div class="value-item">
          <span class="key">{{ key }}:</span>
          <span class="val">{{ formatValue(data.newValues[key]) }}</span>
        </div>
        }
      </div>
      } @else {
      <p><em>None</em></p>
      }
    </div>
  </div>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button
    mat-button
    mat-dialog-close
  >
    Close
  </button>
</mat-dialog-actions>


# /Frontend/src/app/features/admin/pages/oversight/audit-log-detail-dialog.component.ts

import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { AuditLogEntry } from '../../models/audit-log-entry.model';

@Component({
  selector: 'app-audit-log-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
  ],
  templateUrl: './audit-log-detail-dialog.component.html',
  styles: [`
    .detail-section {
      margin-bottom: 16px;
      p { margin: 6px 0; }
    }
    .values-row {
      display: flex;
      gap: 24px;
      margin-top: 16px;
    }
    .values-column {
      flex: 1;
      background: #fcfcfc;
      padding: 12px;
      border-radius: 8px;
      border: 1px solid #eee;
      h3 { margin-top: 0; margin-bottom: 12px; font-size: 14px; font-weight: 600; color: #555; }
      .value-list {
        display: flex;
        flex-direction: column;
        gap: 6px;
        .value-item {
          display: flex;
          justify-content: space-between;
          gap: 8px;
          font-size: 13px;
          border-bottom: 1px dashed #eee;
          padding-bottom: 4px;
          .key { font-weight: 500; color: #666; word-break: break-all; }
          .val { color: #333; text-align: right; word-break: break-all; }
        }
      }
    }
    @media (max-width: 600px) {
      .values-row {
        flex-direction: column;
      }
    }
  `]
})
export class AuditLogDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: AuditLogEntry) {}

  getKeys(obj: Record<string, any>): string[] {
    return Object.keys(obj);
  }

  formatValue(value: any): string {
    if (value === null || value === undefined) return 'null';
    if (typeof value === 'boolean') return value ? 'Yes' : 'No';
    if (typeof value === 'object') return JSON.stringify(value);
    return String(value);
  }
}


# /Frontend/src/app/features/admin/pages/oversight/audit-logs.component.html

<div class="audit-logs-page">
  <!-- Search & Controls -->
  <div class="controls">
    <mat-form-field
      appearance="outline"
      class="search"
    >
      <mat-label>Search by user, entity, or action</mat-label>
      <input
        matInput
        [formControl]="searchControl"
        (keyup)="onSearchDebounced()"
      />
      <mat-icon matSuffix>search</mat-icon>
    </mat-form-field>
    @if (searchControl.value) {
    <button
      mat-button
      (click)="clearSearch()"
    >
      Clear Search
    </button>
    }
  </div>

  <!-- Loading / Error / Content -->
  @if (loading() && entries().length === 0) {
  <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchData()"
    >
      Retry
    </button>
  </app-alert>
  } @if (entries().length > 0 || loading()) { @if (loading()) {
  <mat-progress-bar mode="indeterminate"></mat-progress-bar>
  }

  @if (isMobile()) {
  <div class="mobile-card-view">
    @for (entry of entries(); track entry.id) {
    <mat-card
      (click)="openDetail(entry)"
      class="audit-card"
    >
      <mat-card-header>
        <mat-card-title
          >{{ entry.entityName }} – {{ entry.action }}</mat-card-title
        >
        <mat-card-subtitle
          >{{ entry.timestamp | date:'short' }}</mat-card-subtitle
        >
      </mat-card-header>
      <mat-card-content>
        <p>Changed by: {{ entry.changedByName }}</p>
      </mat-card-content>
    </mat-card>
    } @empty {
    <p>No audit logs found.</p>
    }
  </div>
  } @else {
  <div class="desktop-view">
    <table
      mat-table
      [dataSource]="entries()"
      matSort
      matSortDisableClear
      (matSortChange)="onSortChange($event)"
      aria-label="Audit logs"
    >
      <ng-container matColumnDef="id">
        <th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="id"
        >
          ID
        </th>
        <td
          mat-cell
          *matCellDef="let e"
        >
          {{ e.id }}
        </td>
      </ng-container>
      <ng-container matColumnDef="entityName">
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Entity
        </th>
        <td
          mat-cell
          *matCellDef="let e"
        >
          {{ e.entityName }}
        </td>
      </ng-container>
      <ng-container matColumnDef="action">
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Action
        </th>
        <td
          mat-cell
          *matCellDef="let e"
        >
          {{ e.action }}
        </td>
      </ng-container>
      <ng-container matColumnDef="changedBy">
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Changed By
        </th>
        <td
          mat-cell
          *matCellDef="let e"
        >
          {{ e.changedByName }}
        </td>
      </ng-container>
      <ng-container matColumnDef="timestamp">
        <th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="timestamp"
        >
          Timestamp
        </th>
        <td
          mat-cell
          *matCellDef="let e"
        >
          {{ e.timestamp | date:'medium' }}
        </td>
      </ng-container>
      <ng-container matColumnDef="actions">
        <th
          mat-header-cell
          *matHeaderCellDef
        >
          Actions
        </th>
        <td
          mat-cell
          *matCellDef="let e"
        >
          <button
            mat-icon-button
            (click)="openDetail(e); $event.stopPropagation()"
            aria-label="View audit detail"
          >
            <mat-icon>visibility</mat-icon>
          </button>
        </td>
      </ng-container>
      <tr
        mat-header-row
        *matHeaderRowDef="displayedColumns"
      ></tr>
      <tr
        mat-row
        *matRowDef="let row; columns: displayedColumns"
        (click)="openDetail(row)"
        class="clickable-row"
      ></tr>
    </table>
  </div>
  }

  <mat-paginator
    [length]="totalCount()"
    [pageIndex]="pageIndex()"
    [pageSize]="pageSize()"
    [pageSizeOptions]="[10, 25, 50]"
    (page)="onPageChange($event)"
  >
  </mat-paginator>
  } @else {
  <div class="empty-state">
    <p>No audit log entries found.</p>
    @if (searchControl.value) {
    <p>Try adjusting your search.</p>
    <button
      mat-button
      (click)="clearSearch()"
    >
      Clear Search
    </button>
    }
  </div>
  }
</div>


# /Frontend/src/app/features/admin/pages/oversight/audit-logs.component.scss

.audit-logs-page {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;

  .controls {
    display: flex;
    align-items: center;
    gap: 16px;
    flex-wrap: wrap;

    mat-form-field.search {
      width: 360px;
      margin-bottom: -1.25em; /* alignment fix for inline button */
    }

    button {
      height: 48px;
    }
  }

  .loading {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 64px 0;
  }

  table {
    width: 100%;
    background: #ffffff;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid rgba(0, 0, 0, 0.05);
    table-layout: fixed;

    th,
    td {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    th {
      font-weight: 600;
      color: #424242;
    }

    // 6 columns: id, entityName, action, changedBy, timestamp, actions
    th:nth-child(1) { width: 10%; }  // id
    th:nth-child(2) { width: 15%; }  // entity
    th:nth-child(3) { width: 15%; }  // action
    th:nth-child(4) { width: 20%; }  // changedBy
    th:nth-child(5) { width: 25%; }  // timestamp
    th:nth-child(6) { width: 15%; }  // actions

    tr.clickable-row {
      cursor: pointer;
      transition: background-color 0.2s ease;

      &:hover {
        background-color: #f5f5f5;
      }
    }
  }

  .empty-state {
    text-align: center;
    padding: 48px;
    background: #fdfdfd;
    border: 1px dashed #ccc;
    border-radius: 8px;
    color: #666;
  }

  .mobile-card-view {
    display: flex;
    flex-direction: column;
    gap: 12px;
    max-height: 70vh;
    overflow-y: auto;
    padding: 4px;

    .audit-card {
      cursor: pointer;
      border: 1px solid rgba(0, 0, 0, 0.08);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
      transition: transform 0.2s, box-shadow 0.2s;

      &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.08);
      }

      mat-card-title {
        font-size: 0.95rem;
        font-weight: 600;
        margin-bottom: 4px;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
        text-overflow: ellipsis;
        word-break: break-word;
      }

      mat-card-subtitle {
        font-size: 0.8rem;
        color: rgba(0, 0, 0, 0.54);
      }

      .mat-card-content p {
        margin: 8px 0 0;
        font-size: 0.85rem;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
        text-overflow: ellipsis;
        word-break: break-word;
      }
    }
  }
}

@media (max-width: 768px) {
  .audit-logs-page {
    padding: 16px;

    .controls {
      flex-direction: column;
      align-items: stretch;

      mat-form-field.search {
        width: 100%;
      }
    }
  }
}


# /Frontend/src/app/features/admin/pages/oversight/audit-logs.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { debounceTime, distinctUntilChanged, finalize, map } from 'rxjs';

import { AuditLogApiService } from '../../services/audit-log-api.service';
import { AuditLogEntry } from '../../models/audit-log-entry.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { AuditLogDetailDialogComponent } from './audit-log-detail-dialog.component';

type AuditSortField = 'id' | 'timestamp';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDialogModule,
    MatCardModule,
    AlertComponent,
  ],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.scss'],
})
export class AuditLogsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly auditLogApi = inject(AuditLogApiService);
  private readonly dialog = inject(MatDialog);
  private readonly breakpointObserver = inject(BreakpointObserver);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 767px)').pipe(map((r) => r.matches)),
    { initialValue: false },
  );

  private readonly STORAGE_KEY = 'auditLogsState';

  // Table columns
  displayedColumns = ['id', 'entityName', 'action', 'changedBy', 'timestamp', 'actions'];

  // Data (canonical signals)
  entries = signal<AuditLogEntry[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query state (canonical signals)
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal<AuditSortField>('timestamp');
  sortDescending = signal(false);

  // UI input (form control)
  searchControl = new FormControl('', { nonNullable: true });

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.pageIndex.set(0);
        this.saveState();
        this.fetchData();
      });
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.auditLogApi
      .getAll({
        guestQuery: this.searchControl.value?.trim() || undefined,
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.entries.set(res.data);
          this.totalCount.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
            this.saveState();
          }
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  onSearchDebounced(): void {
    // debounce is handled by a dedicated subscription in ngOnInit
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: Sort): void {
    if (!event.active || !event.direction) return;
    const field = event.active as AuditSortField;
    if (!['id', 'timestamp'].includes(field)) return;
    this.sortField.set(field);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.saveState();
    this.fetchData();
  }

  openDetail(entry: AuditLogEntry): void {
    this.dialog.open(AuditLogDetailDialogComponent, {
      data: entry,
      maxWidth: '700px',
      width: '90%',
    });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }

  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;

      if (typeof parsed.searchQuery === 'string') this.searchControl.setValue(parsed.searchQuery);
      if (parsed.sortField === 'id' || parsed.sortField === 'timestamp') this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean')
        this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0)
        this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0)
        this.pageSize.set(parsed.pageSize);
    } catch {
      /* fallback silently */
    }
  }

  private saveState(): void {
    sessionStorage.setItem(
      this.STORAGE_KEY,
      JSON.stringify({
        searchQuery: this.searchControl.value,
        sortField: this.sortField(),
        sortDescending: this.sortDescending(),
        pageIndex: this.pageIndex(),
        pageSize: this.pageSize(),
      }),
    );
  }
}


# /Frontend/src/app/features/admin/pages/oversight/billing-receipts.component.html

<div class="billing-receipts">
  <!-- Toggle -->
  <div class="toggle-row">
    <mat-button-toggle-group
      [formControl]="activeView"
      (change)="onViewToggle()"
      aria-label="View"
    >
      <mat-button-toggle value="bookings">Bookings</mat-button-toggle>
      <mat-button-toggle value="receipts">Receipts</mat-button-toggle>
    </mat-button-toggle-group>
  </div>

  <!-- Bookings View -->
  @if (activeView.value === 'bookings') {
  <div class="bookings-view">
    <!-- Search & Filters -->
    <div class="controls">
      <mat-form-field
        appearance="outline"
        class="search"
      >
        <mat-label>Search guest name or email</mat-label>
        <input
          matInput
          [formControl]="bookingSearch"
          (keyup)="onBookingSearchDebounced()"
        />
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>Status</mat-label>
        <mat-select [formControl]="bookingStatus">
          <mat-option value="">All</mat-option>
          <mat-option value="Booked">Booked</mat-option>
          <mat-option value="CheckedIn">Checked In</mat-option>
          <mat-option value="CheckedOut">Checked Out</mat-option>
          <mat-option value="Cancelled">Cancelled</mat-option>
        </mat-select>
      </mat-form-field>
      @if (bookingStatus.value || bookingSearch.value) {
      <button
        mat-button
        (click)="clearBookingFilters()"
      >
        Clear Filters
      </button>
      }
    </div>

    <!-- Loading / Error / Content -->
    @if (bookingsLoading() && bookings().length === 0) {
    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
    } @else if (bookingsError()) {
    <app-alert
      type="error"
      [message]="bookingsError()!"
      (closed)="bookingsError.set(null)"
    >
      <button
        mat-button
        (click)="fetchBookings()"
      >
        Retry
      </button>
    </app-alert>
    } @if (bookings().length > 0 || bookingsLoading()) { @if (bookingsLoading())
    {
    <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    @if (isMobile()) {
    <div class="mobile-card-view">
      @for (b of bookings(); track b.id) {
      <mat-card (click)="openBookingDetail(b)" class="booking-card">
        <mat-card-header>
          <mat-card-title>{{ b.guestName }}</mat-card-title>
          <mat-card-subtitle
            >{{ b.checkInDate }} – {{ b.checkOutDate }}</mat-card-subtitle
          >
        </mat-card-header>
        <mat-card-content>
          <p>
            Status: <span class="status-chip" [class]="b.bookingStatus">{{ b.bookingStatus }}</span>
            <span class="divider">|</span> Rooms: {{ getRoomsSummary(b) }}
          </p>
        </mat-card-content>
      </mat-card>
      }
    </div>
    } @else {
    <table
      mat-table
      [dataSource]="bookings()"
      matSort
      matSortDisableClear
      (matSortChange)="onBookingSort($event)"
      aria-label="Bookings"
    >
      <ng-container matColumnDef="id"
        ><th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="id"
        >
          ID
        </th>
        <td
          mat-cell
          *matCellDef="let b"
        >
          {{ b.id }}
        </td></ng-container
      >
      <ng-container matColumnDef="guestName"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Guest
        </th>
        <td
          mat-cell
          *matCellDef="let b"
        >
          {{ b.guestName }}
        </td></ng-container
      >
      <ng-container matColumnDef="checkIn"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Check-In
        </th>
        <td
          mat-cell
          *matCellDef="let b"
        >
          {{ b.checkInDate }}
        </td></ng-container
      >
      <ng-container matColumnDef="checkOut"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Check-Out
        </th>
        <td
          mat-cell
          *matCellDef="let b"
        >
          {{ b.checkOutDate }}
        </td></ng-container
      >
      <ng-container matColumnDef="status"
        ><th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="bookingStatus"
        >
          Status
        </th>
        <td
          mat-cell
          *matCellDef="let b"
        >
          <span
            class="status-chip"
            [class]="b.bookingStatus"
            >{{ b.bookingStatus }}</span
          >
        </td></ng-container
      >
      <ng-container matColumnDef="rooms"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Rooms
        </th>
        <td
          mat-cell
          *matCellDef="let b"
        >
          {{ getRoomsSummary(b) }}
        </td></ng-container
      >
      <ng-container matColumnDef="actions"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Actions
        </th>
        <td
          mat-cell
          *matCellDef="let b"
        >
          <button
            mat-icon-button
            (click)="openBookingDetail(b); $event.stopPropagation()"
            aria-label="View booking"
          >
            <mat-icon>visibility</mat-icon>
          </button>
        </td></ng-container
      >
      <tr
        mat-header-row
        *matHeaderRowDef="['id','guestName','checkIn','checkOut','status','rooms','actions']"
      ></tr>
      <tr
        mat-row
        *matRowDef="let row; columns: ['id','guestName','checkIn','checkOut','status','rooms','actions']"
        (click)="openBookingDetail(row)"
        class="clickable-row"
      ></tr>
    </table>
    }
    <mat-paginator
      [length]="bookingsTotal()"
      [pageIndex]="bookingPage()"
      [pageSize]="bookingPageSize()"
      [pageSizeOptions]="[10,25,50]"
      (page)="onBookingPage($event)"
    ></mat-paginator>
    } @else {
    <div class="empty-state"><p>No bookings found.</p></div>
    }
  </div>
  }

  <!-- Receipts View -->
  @if (activeView.value === 'receipts') {
  <div class="receipts-view">
    <!-- Date Filters -->
    <div class="controls">
      <mat-form-field appearance="outline">
        <mat-label>Start date</mat-label>
        <input
          matInput
          [matDatepicker]="recStartPicker"
          [formControl]="receiptStartDate"
        />
        <mat-datepicker-toggle
          matSuffix
          [for]="recStartPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker #recStartPicker></mat-datepicker>
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>End date</mat-label>
        <input
          matInput
          [matDatepicker]="recEndPicker"
          [formControl]="receiptEndDate"
        />
        <mat-datepicker-toggle
          matSuffix
          [for]="recEndPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker #recEndPicker></mat-datepicker>
      </mat-form-field>
      <button
        mat-raised-button
        color="primary"
        (click)="applyReceiptDateFilter()"
      >
        Apply
      </button>
      <button
        mat-button
        (click)="clearReceiptDateFilter()"
      >
        Clear
      </button>
    </div>

    @if (receiptsLoading() && receipts().length === 0) {
    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
    } @else if (receiptsError()) {
    <app-alert
      type="error"
      [message]="receiptsError()!"
      (closed)="receiptsError.set(null)"
    >
      <button
        mat-button
        (click)="fetchReceipts()"
      >
        Retry
      </button>
    </app-alert>
    } @if (receipts().length > 0 || receiptsLoading()) { @if (receiptsLoading())
    {
    <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    @if (isMobile()) {
    <div class="mobile-card-view">
      @for (r of receipts(); track r.id) {
      <mat-card (click)="openReceiptDetail(r)" class="receipt-card">
        <mat-card-header>
          <mat-card-title>Receipt #{{ r.id }} – Booking #{{ r.bookingId }}</mat-card-title>
          <mat-card-subtitle
            >{{ r.paidAt | date:'medium' }}</mat-card-subtitle
          >
        </mat-card-header>
        <mat-card-content>
          <p>
            Amount: {{ r.amountPaid | currency }}
            <span class="divider">|</span> Method: {{ r.paymentMethod }}
          </p>
        </mat-card-content>
      </mat-card>
      }
    </div>
    } @else {
    <table
      mat-table
      [dataSource]="receipts()"
      matSort
      matSortDisableClear
      (matSortChange)="onReceiptSort($event)"
      aria-label="Receipts"
    >
      <ng-container matColumnDef="id"
        ><th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="id"
        >
          ID
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          {{ r.id }}
        </td></ng-container
      >
      <ng-container matColumnDef="bookingId"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Booking ID
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          {{ r.bookingId }}
        </td></ng-container
      >
      <ng-container matColumnDef="amountPaid"
        ><th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="amountPaid"
        >
          Amount
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          {{ r.amountPaid | currency }}
        </td></ng-container
      >
      <ng-container matColumnDef="paymentMethod"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Payment Method
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          {{ r.paymentMethod }}
        </td></ng-container
      >
      <ng-container matColumnDef="paidAt"
        ><th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="paidAt"
        >
          Paid At
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          {{ r.paidAt | date:'medium' }}
        </td></ng-container
      >
      <ng-container matColumnDef="actions"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Actions
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          <button
            mat-icon-button
            (click)="openReceiptDetail(r); $event.stopPropagation()"
            aria-label="View receipt"
          >
            <mat-icon>visibility</mat-icon>
          </button>
        </td></ng-container
      >
      <tr
        mat-header-row
        *matHeaderRowDef="['id','bookingId','amountPaid','paymentMethod','paidAt','actions']"
      ></tr>
      <tr
        mat-row
        *matRowDef="let row; columns: ['id','bookingId','amountPaid','paymentMethod','paidAt','actions']"
        (click)="openReceiptDetail(row)"
        class="clickable-row"
      ></tr>
    </table>
    }
    <mat-paginator
      [length]="receiptsTotal()"
      [pageIndex]="receiptPage()"
      [pageSize]="receiptPageSize()"
      [pageSizeOptions]="[10,25,50]"
      (page)="onReceiptPage($event)"
    ></mat-paginator>
    } @else {
    <div class="empty-state"><p>No receipts found.</p></div>
    }
  </div>
  }
</div>


# /Frontend/src/app/features/admin/pages/oversight/billing-receipts.component.scss

.billing-receipts {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;

  .toggle-row {
    display: flex;
    justify-content: flex-start;
  }

  .controls {
    display: flex;
    align-items: center;
    gap: 16px;
    flex-wrap: wrap;
    margin-bottom: 8px;

    mat-form-field {
      width: 240px;
      margin-bottom: -1.25em; /* alignment fix for inline controls */

      &.search {
        width: 320px;
      }
    }

    button {
      height: 48px;
    }
  }

  .loading {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 64px 0;
  }

  table {
    width: 100%;
    background: #ffffff;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid rgba(0, 0, 0, 0.05);

    th {
      font-weight: 600;
      color: #424242;
    }

    tr.clickable-row {
      cursor: pointer;
      transition: background-color 0.2s ease;

      &:hover {
        background-color: #f5f5f5;
      }
    }
  }

  .bookings-view {
    table {
      table-layout: fixed;

      th,
      td {
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      // 7 columns: id, guestName, checkIn, checkOut, status, rooms, actions
      th:nth-child(1) { width: 8%; }   // id
      th:nth-child(2) { width: 18%; }  // guestName
      th:nth-child(3) { width: 14%; }  // checkIn
      th:nth-child(4) { width: 14%; }  // checkOut
      th:nth-child(5) { width: 12%; }  // status
      th:nth-child(6) { width: 19%; }  // rooms
      th:nth-child(7) { width: 15%; }  // actions
    }
  }

  .receipts-view {
    table {
      table-layout: fixed;

      th,
      td {
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      // 6 columns: id, bookingId, amountPaid, paymentMethod, paidAt, actions
      th:nth-child(1) { width: 10%; }   // id
      th:nth-child(2) { width: 15%; }   // bookingId
      th:nth-child(3) { width: 15%; }   // amountPaid
      th:nth-child(4) { width: 20%; }   // paymentMethod
      th:nth-child(5) { width: 25%; }   // paidAt
      th:nth-child(6) { width: 15%; }   // actions
    }
  }

  .status-chip {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 16px;
    font-size: 12px;
    font-weight: 500;

    &.Booked {
      background-color: #e3f2fd;
      color: #1565c0;
    }
    &.CheckedIn {
      background-color: #e8f5e9;
      color: #2e7d32;
    }
    &.CheckedOut {
      background-color: #eceff1;
      color: #37474f;
    }
    &.Cancelled {
      background-color: #ffebee;
      color: #c62828;
    }
  }

  .empty-state {
    text-align: center;
    padding: 48px;
    background: #fdfdfd;
    border: 1px dashed #ccc;
    border-radius: 8px;
    color: #666;
  }

  .mobile-card-view {
    display: flex;
    flex-direction: column;
    gap: 12px;
    max-height: 70vh;
    overflow-y: auto;
    padding: 4px;

    .booking-card,
    .receipt-card {
      cursor: pointer;
      border: 1px solid rgba(0, 0, 0, 0.08);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
      transition: transform 0.2s, box-shadow 0.2s;

      &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.08);
      }

      mat-card-title {
        font-size: 0.95rem;
        font-weight: 600;
        margin-bottom: 4px;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
        text-overflow: ellipsis;
        word-break: break-word;
      }

      mat-card-subtitle {
        font-size: 0.8rem;
        color: rgba(0, 0, 0, 0.54);
      }

      .mat-card-content p {
        margin: 8px 0 0;
        font-size: 0.85rem;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
        text-overflow: ellipsis;
        word-break: break-word;

        .divider {
          margin: 0 8px;
          color: rgba(0, 0, 0, 0.2);
        }
      }
    }
  }
}

@media (max-width: 768px) {
  .billing-receipts {
    padding: 16px;

    .controls {
      flex-direction: column;
      align-items: stretch;

      mat-form-field {
        width: 100% !important;
      }
    }
  }
}


# /Frontend/src/app/features/admin/pages/oversight/billing-receipts.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { debounceTime, distinctUntilChanged, finalize, map } from 'rxjs';

import { BookingApiService } from '../../services/booking-api.service';
import { BillingApiService } from '../../services/billing-api.service';
import { Booking } from '../../models/booking.model';
import { Receipt } from '../../models/receipt.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { BookingDetailDialogComponent } from './booking-detail-dialog.component';
import { ReceiptDetailDialogComponent } from './receipt-detail-dialog.component';

type BookingSortField = 'id' | 'bookingStatus' | 'bookedAt';
type ReceiptSortField = 'id' | 'amountPaid' | 'paidAt';

@Component({
  selector: 'app-billing-receipts',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonToggleModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDialogModule,
    MatCardModule,
    MatDividerModule,
    MatChipsModule,
    AlertComponent,
  ],
  templateUrl: './billing-receipts.component.html',
  styleUrls: ['./billing-receipts.component.scss'],
})
export class BillingReceiptsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly bookingApi = inject(BookingApiService);
  private readonly billingApi = inject(BillingApiService);
  private readonly dialog = inject(MatDialog);
  private readonly breakpointObserver = inject(BreakpointObserver);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 767px)').pipe(map((r) => r.matches)),
    { initialValue: false },
  );

  private readonly STORAGE_KEY = 'billingReceiptsState';

  // Active view toggle
  activeView = new FormControl<'bookings' | 'receipts'>('bookings', { nonNullable: true });

  // Bookings state
  bookings = signal<Booking[]>([]);
  bookingsTotal = signal(0);
  bookingsLoading = signal(false);
  bookingsError = signal<string | null>(null);
  bookingPage = signal(0);
  bookingPageSize = signal(10);
  bookingSortField = signal<BookingSortField>('bookedAt');
  bookingSortDescending = signal(true);

  // Bookings filter controls
  bookingSearch = new FormControl('', { nonNullable: true });
  bookingStatus = new FormControl('', { nonNullable: true });

  // Receipts state
  receipts = signal<Receipt[]>([]);
  receiptsTotal = signal(0);
  receiptsLoading = signal(false);
  receiptsError = signal<string | null>(null);
  receiptPage = signal(0);
  receiptPageSize = signal(10);
  receiptSortField = signal<ReceiptSortField>('id');
  receiptSortDescending = signal(true);

  // Receipts filter controls
  receiptStartDate = new FormControl<Date | null>(null);
  receiptEndDate = new FormControl<Date | null>(null);

  ngOnInit(): void {
    this.restoreState();
    this.setupBookingSearchDebounce();
    this.setupBookingStatusListener();

    if (this.activeView.value === 'bookings') {
      this.fetchBookings();
    } else {
      this.fetchReceipts();
    }
  }

  private setupBookingSearchDebounce(): void {
    this.bookingSearch.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.bookingPage.set(0);
        this.saveState();
        this.fetchBookings();
      });
  }

  private setupBookingStatusListener(): void {
    this.bookingStatus.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.bookingPage.set(0);
        this.saveState();
        this.fetchBookings();
      });
  }

  onBookingSearchDebounced(): void {
    // Subscribed via setupBookingSearchDebounce valueChanges.
  }

  onViewToggle(): void {
    this.saveState();
    if (this.activeView.value === 'bookings') {
      this.fetchBookings();
    } else {
      this.fetchReceipts();
    }
  }

  fetchBookings(): void {
    this.bookingsLoading.set(true);
    this.bookingsError.set(null);
    this.bookingApi
      .getAll({
        status: this.bookingStatus.value || undefined,
        guestQuery: this.bookingSearch.value || undefined,
        pageNumber: this.bookingPage() + 1,
        pageSize: this.bookingPageSize(),
        sortBy: this.bookingSortField(),
        sortDescending: this.bookingSortDescending(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.bookingsLoading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.bookings.set(res.data);
          this.bookingsTotal.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.bookingPageSize()) - 1);
          if (this.bookingPage() > maxPage) {
            this.bookingPage.set(maxPage);
            this.saveState();
          }
        },
        error: (err) => this.bookingsError.set(this.extractErrorMessage(err)),
      });
  }

  fetchReceipts(): void {
    this.receiptsLoading.set(true);
    this.receiptsError.set(null);
    const startStr = this.formatDate(this.receiptStartDate.value);
    const endStr = this.formatDate(this.receiptEndDate.value);
    this.billingApi
      .getReceipts({
        startDate: startStr,
        endDate: endStr,
        pageNumber: this.receiptPage() + 1,
        pageSize: this.receiptPageSize(),
        sortBy: this.receiptSortField(),
        sortDescending: this.receiptSortDescending(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.receiptsLoading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.receipts.set(res.data);
          this.receiptsTotal.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.receiptPageSize()) - 1);
          if (this.receiptPage() > maxPage) {
            this.receiptPage.set(maxPage);
            this.saveState();
          }
        },
        error: (err) => this.receiptsError.set(this.extractErrorMessage(err)),
      });
  }

  clearBookingFilters(): void {
    this.bookingSearch.setValue('');
    this.bookingStatus.setValue('');
    this.bookingPage.set(0);
    this.saveState();
    this.fetchBookings();
  }

  applyReceiptDateFilter(): void {
    this.receiptPage.set(0);
    this.saveState();
    this.fetchReceipts();
  }

  clearReceiptDateFilter(): void {
    this.receiptStartDate.setValue(null);
    this.receiptEndDate.setValue(null);
    this.receiptPage.set(0);
    this.saveState();
    this.fetchReceipts();
  }

  onBookingSort(event: Sort): void {
    if (!event.active || !event.direction) return;
    const field = event.active as BookingSortField;
    if (!['id', 'bookingStatus', 'bookedAt'].includes(field)) return;
    this.bookingSortField.set(field);
    this.bookingSortDescending.set(event.direction === 'desc');
    this.bookingPage.set(0);
    this.saveState();
    this.fetchBookings();
  }

  onReceiptSort(event: Sort): void {
    if (!event.active || !event.direction) return;
    const field = event.active as ReceiptSortField;
    if (!['id', 'amountPaid', 'paidAt'].includes(field)) return;
    this.receiptSortField.set(field);
    this.receiptSortDescending.set(event.direction === 'desc');
    this.receiptPage.set(0);
    this.saveState();
    this.fetchReceipts();
  }

  onBookingPage(event: any): void {
    this.bookingPage.set(event.pageIndex);
    this.bookingPageSize.set(event.pageSize);
    this.saveState();
    this.fetchBookings();
  }

  onReceiptPage(event: any): void {
    this.receiptPage.set(event.pageIndex);
    this.receiptPageSize.set(event.pageSize);
    this.saveState();
    this.fetchReceipts();
  }

  openBookingDetail(booking: Booking): void {
    this.dialog.open(BookingDetailDialogComponent, {
      data: booking,
      width: '500px',
    });
  }

  openReceiptDetail(receipt: Receipt): void {
    this.dialog.open(ReceiptDetailDialogComponent, {
      data: receipt,
      width: '400px',
    });
  }

  getRoomsSummary(booking: Booking): string {
    if (!booking.rooms || booking.rooms.length === 0) return '—';
    return booking.rooms
      .map((r) => r.roomNumber || `Room type #${r.roomTypeId}`)
      .join(', ');
  }

  private formatDate(date: Date | null): string | undefined {
    if (!date) return undefined;
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}-${month}-${year}`;
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }

  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;

      if (parsed.activeView === 'bookings' || parsed.activeView === 'receipts')
        this.activeView.setValue(parsed.activeView);

      if (Number.isInteger(parsed.bookingPage) && parsed.bookingPage >= 0)
        this.bookingPage.set(parsed.bookingPage);
      if (Number.isInteger(parsed.bookingPageSize) && parsed.bookingPageSize > 0)
        this.bookingPageSize.set(parsed.bookingPageSize);
      if (parsed.bookingSortField === 'id' || parsed.bookingSortField === 'bookingStatus' || parsed.bookingSortField === 'bookedAt')
        this.bookingSortField.set(parsed.bookingSortField);
      if (typeof parsed.bookingSortDescending === 'boolean')
        this.bookingSortDescending.set(parsed.bookingSortDescending);
      if (typeof parsed.bookingSearch === 'string')
        this.bookingSearch.setValue(parsed.bookingSearch);
      if (typeof parsed.bookingStatus === 'string')
        this.bookingStatus.setValue(parsed.bookingStatus);

      if (Number.isInteger(parsed.receiptPage) && parsed.receiptPage >= 0)
        this.receiptPage.set(parsed.receiptPage);
      if (Number.isInteger(parsed.receiptPageSize) && parsed.receiptPageSize > 0)
        this.receiptPageSize.set(parsed.receiptPageSize);
      if (parsed.receiptSortField === 'id' || parsed.receiptSortField === 'amountPaid' || parsed.receiptSortField === 'paidAt')
        this.receiptSortField.set(parsed.receiptSortField);
      if (typeof parsed.receiptSortDescending === 'boolean')
        this.receiptSortDescending.set(parsed.receiptSortDescending);
      if (
        parsed.receiptStartDate === null ||
        (typeof parsed.receiptStartDate === 'string' && !isNaN(Date.parse(parsed.receiptStartDate)))
      )
        this.receiptStartDate.setValue(
          parsed.receiptStartDate ? new Date(parsed.receiptStartDate) : null,
        );
      if (
        parsed.receiptEndDate === null ||
        (typeof parsed.receiptEndDate === 'string' && !isNaN(Date.parse(parsed.receiptEndDate)))
      )
        this.receiptEndDate.setValue(
          parsed.receiptEndDate ? new Date(parsed.receiptEndDate) : null,
        );
    } catch {
      /* fallback silently */
    }
  }

  private saveState(): void {
    sessionStorage.setItem(
      this.STORAGE_KEY,
      JSON.stringify({
        activeView: this.activeView.value,
        bookingPage: this.bookingPage(),
        bookingPageSize: this.bookingPageSize(),
        bookingSortField: this.bookingSortField(),
        bookingSortDescending: this.bookingSortDescending(),
        bookingSearch: this.bookingSearch.value,
        bookingStatus: this.bookingStatus.value,
        receiptPage: this.receiptPage(),
        receiptPageSize: this.receiptPageSize(),
        receiptSortField: this.receiptSortField(),
        receiptSortDescending: this.receiptSortDescending(),
        receiptStartDate: this.receiptStartDate.value?.toISOString() ?? null,
        receiptEndDate: this.receiptEndDate.value?.toISOString() ?? null,
      }),
    );
  }
}


# /Frontend/src/app/features/admin/pages/oversight/booking-detail-dialog.component.html

<h2 mat-dialog-title>Booking Details (#{{ data.id }})</h2>
<mat-dialog-content>
  <div class="booking-dialog-container">
    <div class="info-section">
      <h3>Guest Info</h3>
      <p><strong>Name:</strong> {{ data.guestName }}</p>
      <p><strong>Email:</strong> {{ data.guestEmail }}</p>
      <p><strong>Guest Count:</strong> {{ data.guestCount }}</p>
    </div>

    <mat-divider></mat-divider>

    <div class="info-section">
      <h3>Stay Dates</h3>
      <p><strong>Check-In:</strong> {{ data.checkInDate }}</p>
      <p><strong>Check-Out:</strong> {{ data.checkOutDate }}</p>
      <p><strong>Booked At:</strong> {{ data.bookedAt | date:'medium' }}</p>
      <p>
        <strong>Status:</strong>
        <span class="status-chip" [class]="data.bookingStatus">{{ data.bookingStatus }}</span>
      </p>
    </div>

    <mat-divider></mat-divider>

    <div class="info-section">
      <h3>Rooms</h3>
      <mat-list>
        @for (room of data.rooms; track room.id) {
          <mat-list-item>
            <mat-icon matListItemIcon>meeting_room</mat-icon>
            <span matListItemTitle>Room {{ room.roomNumber || 'Not Assigned' }}</span>
            <span matListItemLine>Price: {{ room.lockedInPrice | currency }}</span>
          </mat-list-item>
        }
      </mat-list>
    </div>
  </div>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Close</button>
</mat-dialog-actions>


# /Frontend/src/app/features/admin/pages/oversight/booking-detail-dialog.component.ts

import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatListModule } from '@angular/material/list';
import { Booking } from '../../models/booking.model';

@Component({
  selector: 'app-booking-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDividerModule,
    MatChipsModule,
    MatListModule,
  ],
  templateUrl: './booking-detail-dialog.component.html',
  styles: [`
    .booking-dialog-container {
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 8px 0;
    }
    .info-section {
      h3 {
        margin-top: 0;
        margin-bottom: 8px;
        color: #1976d2;
        font-size: 16px;
        font-weight: 500;
      }
      p {
        margin: 4px 0;
      }
    }
    .status-chip {
      display: inline-block;
      padding: 4px 12px;
      border-radius: 16px;
      font-size: 12px;
      font-weight: 500;
      &.Booked { background-color: #e3f2fd; color: #1565c0; }
      &.CheckedIn { background-color: #e8f5e9; color: #2e7d32; }
      &.CheckedOut { background-color: #eceff1; color: #37474f; }
      &.Cancelled { background-color: #ffebee; color: #c62828; }
    }
  `]
})
export class BookingDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: Booking) {}
}


# /Frontend/src/app/features/admin/pages/oversight/feedback.component.html

<div class="feedback-page">
  <!-- Controls -->
  <div class="controls">
    <span class="spacer"></span>
    <mat-form-field appearance="outline">
      <mat-label>Visibility</mat-label>
      <mat-select
        [formControl]="includeHiddenControl"
        (selectionChange)="onIncludeHiddenToggle($event.value)"
      >
        <mat-option [value]="false">Visible only</mat-option>
        <mat-option [value]="true">All (including hidden)</mat-option>
      </mat-select>
    </mat-form-field>
  </div>

  <!-- Loading / Error / Content -->
  @if (loading() && entries().length === 0) {
    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchData()">Retry</button>
    </app-alert>
  }

  @if (entries().length > 0 || loading()) {
    @if (loading()) {
      <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    @if (isMobile()) {
      <div class="mobile-card-view">
        @for (f of entries(); track f.id) {
          <mat-card class="feedback-card">
            <mat-card-header>
              <mat-card-title>Booking #{{ f.bookingId }} – {{ f.rating }}/5</mat-card-title>
              <mat-card-subtitle>{{ f.createdAt | date:'short' }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p>{{ f.comments || '—' }}</p>
            </mat-card-content>
            <mat-card-actions>
              <mat-slide-toggle
                [checked]="f.isHidden"
                (change)="onToggleHidden(f, $event.checked)"
                [aria-label]="f.isHidden ? 'Show feedback' : 'Hide feedback'"
                matTooltip="Toggle visibility">
              </mat-slide-toggle>
            </mat-card-actions>
          </mat-card>
        }
      </div>
    } @else {
      <table mat-table [dataSource]="entries()" matSort matSortDisableClear (matSortChange)="onSortChange($event)" aria-label="Feedback">
        <ng-container matColumnDef="id">
          <th mat-header-cell *matHeaderCellDef mat-sort-header="id">ID</th>
          <td mat-cell *matCellDef="let f">{{ f.id }}</td>
        </ng-container>
        <ng-container matColumnDef="bookingId">
          <th mat-header-cell *matHeaderCellDef>Booking ID</th>
          <td mat-cell *matCellDef="let f">{{ f.bookingId }}</td>
        </ng-container>
        <ng-container matColumnDef="rating">
          <th mat-header-cell *matHeaderCellDef mat-sort-header="rating">Rating</th>
          <td mat-cell *matCellDef="let f">{{ f.rating }}/5</td>
        </ng-container>
        <ng-container matColumnDef="comments">
          <th mat-header-cell *matHeaderCellDef>Comments</th>
          <td mat-cell *matCellDef="let f">{{ f.comments || '—' }}</td>
        </ng-container>
        <ng-container matColumnDef="createdAt">
          <th mat-header-cell *matHeaderCellDef mat-sort-header="createdAt">Created</th>
          <td mat-cell *matCellDef="let f">{{ f.createdAt | date:'short' }}</td>
        </ng-container>
        <ng-container matColumnDef="isHidden">
          <th mat-header-cell *matHeaderCellDef>Hidden</th>
          <td mat-cell *matCellDef="let f">{{ f.isHidden ? 'Yes' : 'No' }}</td>
        </ng-container>
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>Moderate</th>
          <td mat-cell *matCellDef="let f">
            <mat-slide-toggle
              [checked]="f.isHidden"
              (change)="onToggleHidden(f, $event.checked)"
              [aria-label]="f.isHidden ? 'Show feedback' : 'Hide feedback'"
              matTooltip="Toggle visibility">
            </mat-slide-toggle>
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
      </table>
    }
    <mat-paginator
      [length]="totalCount()"
      [pageIndex]="pageIndex()"
      [pageSize]="pageSize()"
      [pageSizeOptions]="[10, 25, 50]"
      (page)="onPageChange($event)">
    </mat-paginator>
  } @else {
    <div class="empty-state">
      <p>No feedback found.</p>
      @if (includeHiddenControl.value) {
        <p>Even with hidden feedback included, no entries exist.</p>
      } @else {
        <p>No visible feedback available. Try enabling "Show hidden feedback".</p>
      }
    </div>
  }
</div>


# /Frontend/src/app/features/admin/pages/oversight/feedback.component.scss

.feedback-page {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;

  .controls {
    display: flex;
    align-items: center;
    gap: 16px;
    flex-wrap: wrap;
    margin-bottom: 8px;

    .spacer {
      flex: 1 1 auto;
    }
  }

  .loading {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 64px 0;
  }

  table {
    width: 100%;
    background: #ffffff;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid rgba(0, 0, 0, 0.05);
    table-layout: fixed;

    th,
    td {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    th {
      font-weight: 600;
      color: #424242;
    }

    // 7 columns: id, bookingId, rating, comments, createdAt, isHidden, actions
    th:nth-child(1) { width: 8%; }   // id
    th:nth-child(2) { width: 10%; }  // bookingId
    th:nth-child(3) { width: 10%; }  // rating
    th:nth-child(4) { width: 30%; }  // comments (longer)
    th:nth-child(5) { width: 17%; }  // createdAt
    th:nth-child(6) { width: 10%; }  // isHidden
    th:nth-child(7) { width: 15%; }  // actions

    tr.mat-row {
      transition: background-color 0.2s ease;

      &:hover {
        background-color: #f9f9f9;
      }
    }
  }

  .empty-state {
    text-align: center;
    padding: 48px;
    background: #fdfdfd;
    border: 1px dashed #ccc;
    border-radius: 8px;
    color: #666;
  }

  .mobile-card-view {
    display: flex;
    flex-direction: column;
    gap: 12px;
    max-height: 70vh;
    overflow-y: auto;
    padding: 4px;

    .feedback-card {
      border: 1px solid rgba(0, 0, 0, 0.08);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);

      mat-card-title {
        font-size: 0.95rem;
        font-weight: 600;
        margin-bottom: 4px;
      }

      mat-card-subtitle {
        font-size: 0.8rem;
        color: rgba(0, 0, 0, 0.54);
      }

      .mat-card-content p {
        margin: 8px 0 0;
        font-size: 0.85rem;
        display: -webkit-box;
        -webkit-line-clamp: 3;
        -webkit-box-orient: vertical;
        overflow: hidden;
        text-overflow: ellipsis;
        word-break: break-word;
      }

      mat-card-actions {
        padding: 8px 16px;
        display: flex;
        justify-content: flex-end;
      }
    }
  }
}

@media (max-width: 768px) {
  .feedback-page {
    padding: 16px;

    .controls {
      flex-direction: column;
      align-items: stretch;
    }
  }
}


# /Frontend/src/app/features/admin/pages/oversight/feedback.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { finalize, map } from 'rxjs';

import { FeedbackApiService } from '../../services/feedback-api.service';
import { Feedback } from '../../models/feedback.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

type FeedbackSortField = 'id' | 'rating' | 'createdAt';

@Component({
  selector: 'app-feedback',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDialogModule,
    MatCardModule,
    AlertComponent,
  ],
  templateUrl: './feedback.component.html',
  styleUrls: ['./feedback.component.scss'],
})
export class FeedbackComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly feedbackApi = inject(FeedbackApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly breakpointObserver = inject(BreakpointObserver);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 767px)').pipe(map((r) => r.matches)),
    { initialValue: false },
  );

  private readonly STORAGE_KEY = 'feedbackState';

  // Table columns
  displayedColumns = ['id', 'bookingId', 'rating', 'comments', 'createdAt', 'isHidden', 'actions'];

  // Data (canonical signals)
  entries = signal<Feedback[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query state (canonical signals)
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal<FeedbackSortField>('createdAt');
  sortDescending = signal(true);

  // UI input (form control)
  includeHiddenControl = new FormControl(false, { nonNullable: true });

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.feedbackApi
      .getAll({
        includeHidden: this.includeHiddenControl.value,
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.entries.set(res.data);
          this.totalCount.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
            this.saveState();
          }
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  onIncludeHiddenToggle(value: boolean): void {
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: Sort): void {
    if (!event.active || !event.direction) return;
    const field = event.active as FeedbackSortField;
    if (!['id', 'rating', 'createdAt'].includes(field)) return;
    this.sortField.set(field);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.saveState();
    this.fetchData();
  }

  onToggleHidden(feedback: Feedback, isHidden: boolean): void {
    if (!feedback.isHidden && isHidden) {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Hide Feedback',
          message: 'Are you sure you want to hide this feedback?',
        },
      });
      dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
        if (confirmed) {
          this.performToggle(feedback, isHidden);
        } else {
          // Revert the optimistic UI toggle
          this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden: false } : f));
        }
      });
    } else {
      this.performToggle(feedback, isHidden);
    }
  }

  private performToggle(feedback: Feedback, isHidden: boolean): void {
    // Optimistic update
    this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden } : f));

    this.feedbackApi.moderate(feedback.id, { isHidden }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open(
          isHidden ? 'Feedback hidden' : 'Feedback visible',
          'Close',
          { duration: 2000 }
        );
      },
      error: (err: any) => {
        // Revert on failure
        this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden: !isHidden } : f));
        this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 });
      }
    });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }

  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;

      if (typeof parsed.includeHidden === 'boolean')
        this.includeHiddenControl.setValue(parsed.includeHidden);
      if (parsed.sortField === 'id' || parsed.sortField === 'rating' || parsed.sortField === 'createdAt') this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean')
        this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0)
        this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0)
        this.pageSize.set(parsed.pageSize);
    } catch {
      /* fallback silently */
    }
  }

  private saveState(): void {
    sessionStorage.setItem(
      this.STORAGE_KEY,
      JSON.stringify({
        includeHidden: this.includeHiddenControl.value,
        sortField: this.sortField(),
        sortDescending: this.sortDescending(),
        pageIndex: this.pageIndex(),
        pageSize: this.pageSize(),
      }),
    );
  }
}


# /Frontend/src/app/features/admin/pages/oversight/receipt-detail-dialog.component.html

<h2 mat-dialog-title>Receipt Details (#{{ data.id }})</h2>
<mat-dialog-content>
  <div class="receipt-dialog-container">
    <div class="info-section">
      <p><strong>Booking ID:</strong> {{ data.bookingId }}</p>
      <p><strong>Amount Paid:</strong> {{ data.amountPaid | currency }}</p>
      <p><strong>Payment Method:</strong> {{ data.paymentMethod }}</p>
      <p><strong>Transaction ID:</strong> {{ data.transactionId || '—' }}</p>
      <p><strong>Paid At:</strong> {{ data.paidAt | date:'medium' }}</p>
    </div>
  </div>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Close</button>
</mat-dialog-actions>


# /Frontend/src/app/features/admin/pages/oversight/receipt-detail-dialog.component.ts

import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { Receipt } from '../../models/receipt.model';

@Component({
  selector: 'app-receipt-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDividerModule,
  ],
  templateUrl: './receipt-detail-dialog.component.html',
  styles: [`
    .receipt-dialog-container {
      display: flex;
      flex-direction: column;
      gap: 12px;
      padding: 8px 0;
    }
    .info-section {
      p {
        margin: 8px 0;
        font-size: 14px;
        color: #333;
      }
    }
  `]
})
export class ReceiptDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: Receipt) {}
}


# /Frontend/src/app/features/admin/services/amenity-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Amenity, CreateAmenityDTO, UpdateAmenityDTO } from '../models/amenity.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class AmenityApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/amenities`;

  getAll(params: {
    pageNumber: number;
    pageSize: number;
    searchQuery?: string;
    sortBy: string;
    sortDescending: boolean;
    isAvailable?: boolean;
  }): Observable<PaginatedResponse<Amenity>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }

    if (params.isAvailable !== undefined) {
      httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
    }

    return this.http.get<PaginatedResponse<Amenity>>(this.baseUrl, { params: httpParams }).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  create(dto: CreateAmenityDTO): Observable<Amenity> {
    return this.http.post<Amenity>(this.baseUrl, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  update(id: number, dto: UpdateAmenityDTO): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.baseUrl}/${id}`, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }
}


# /Frontend/src/app/features/admin/services/analytics-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AnalyticsDashboardDTO } from '../models/analytics-dashboard.dto';

@Injectable({ providedIn: 'root' })
export class AnalyticsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/analytics`;

  getAnalytics(params?: { startDate?: string; endDate?: string }): Observable<AnalyticsDashboardDTO> {
    let httpParams = new HttpParams();
    if (params?.startDate) {
      httpParams = httpParams.set('startDate', params.startDate);
    }
    if (params?.endDate) {
      httpParams = httpParams.set('endDate', params.endDate);
    }
    return this.http.get<AnalyticsDashboardDTO>(this.baseUrl, { params: httpParams });
  }
}


# /Frontend/src/app/features/admin/services/audit-log-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuditLogEntry } from '../models/audit-log-entry.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class AuditLogApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/auditlogs`;

  getAll(params: {
    guestQuery?: string;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<AuditLogEntry>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.guestQuery) {
      httpParams = httpParams.set('guestQuery', params.guestQuery);
    }

    return this.http.get<PaginatedResponse<AuditLogEntry>>(this.baseUrl, { params: httpParams });
  }
}


# /Frontend/src/app/features/admin/services/billing-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Receipt } from '../models/receipt.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class BillingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/billing`;

  getReceipts(params: {
    startDate?: string;
    endDate?: string;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<Receipt>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.startDate) {
      httpParams = httpParams.set('startDate', params.startDate);
    }
    if (params.endDate) {
      httpParams = httpParams.set('endDate', params.endDate);
    }

    return this.http.get<PaginatedResponse<Receipt>>(`${this.baseUrl}/receipts`, {
      params: httpParams,
    });
  }
}


# /Frontend/src/app/features/admin/services/booking-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Booking } from '../models/booking.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class BookingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/bookings`;

  getAll(params: {
    status?: string;
    guestQuery?: string;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<Booking>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.status) {
      httpParams = httpParams.set('bookingstatus', params.status);
    }
    if (params.guestQuery) {
      httpParams = httpParams.set('guestQuery', params.guestQuery);
    }

    return this.http.get<PaginatedResponse<Booking>>(this.baseUrl, { params: httpParams });
  }
}


# /Frontend/src/app/features/admin/services/feedback-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Feedback, ModerateFeedbackRequest } from '../models/feedback.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class FeedbackApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/feedback`;

  getAll(params: {
    includeHidden: boolean;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<Feedback>> {
    const httpParams = new HttpParams()
      .set('includeHidden', params.includeHidden.toString())
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    return this.http.get<PaginatedResponse<Feedback>>(this.baseUrl, {
      params: httpParams,
    });
  }

  moderate(id: number, request: ModerateFeedbackRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/moderate`, request);
  }
}


# /Frontend/src/app/features/admin/services/housekeeping-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { HousekeepingTask } from '../models/housekeeping-task.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';
import { CreateInternalTicketRequest } from '../models/create-internal-ticket-request.dto';

@Injectable({ providedIn: 'root' })
export class HousekeepingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/housekeeping`;

  getAll(params?: { status?: string; pageNumber?: number; pageSize?: number }): Observable<PaginatedResponse<HousekeepingTask>> {
    let httpParams = new HttpParams();
    if (params?.status) {
      httpParams = httpParams.set('status', params.status);
    }
    if (params?.pageNumber != null) {
      httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    }
    if (params?.pageSize != null) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }
    return this.http.get<PaginatedResponse<HousekeepingTask>>(this.baseUrl, { params: httpParams });
  }

  createInternal(body: CreateInternalTicketRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/internal`, body);
  }
}


# /Frontend/src/app/features/admin/services/maintenance-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { MaintenanceTask } from '../models/maintenance-task.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';
import { CreateInternalTicketRequest } from '../models/create-internal-ticket-request.dto';

@Injectable({ providedIn: 'root' })
export class MaintenanceApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/maintenance`;

  getAll(params?: { status?: string; pageNumber?: number; pageSize?: number }): Observable<PaginatedResponse<MaintenanceTask>> {
    let httpParams = new HttpParams();
    if (params?.status) {
      httpParams = httpParams.set('status', params.status);
    }
    if (params?.pageNumber != null) {
      httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    }
    if (params?.pageSize != null) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }
    return this.http.get<PaginatedResponse<MaintenanceTask>>(this.baseUrl, { params: httpParams });
  }

  createInternal(body: CreateInternalTicketRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/internal`, body);
  }
}


# /Frontend/src/app/features/admin/services/menu-item-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { MenuItem, CreateMenuItemDTO, UpdateMenuItemDTO } from '../models/menu-item.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class MenuItemApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/menu-items`;

  getAll(params: {
    pageNumber: number;
    pageSize: number;
    searchQuery?: string;
    sortBy: string;
    sortDescending: boolean;
    isAvailable?: boolean;
  }): Observable<PaginatedResponse<MenuItem>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }

    if (params.isAvailable !== undefined) {
      httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
    }

    return this.http.get<PaginatedResponse<MenuItem>>(this.baseUrl, { params: httpParams }).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  create(dto: CreateMenuItemDTO): Observable<MenuItem> {
    return this.http.post<MenuItem>(this.baseUrl, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  update(id: number, dto: UpdateMenuItemDTO): Observable<MenuItem> {
    return this.http.put<MenuItem>(`${this.baseUrl}/${id}`, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  updateStatus(id: number, isAvailable: boolean): Observable<void> {
    const params = new HttpParams().set('isAvailable', isAvailable.toString());
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, null, { params }).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }
}


# /Frontend/src/app/features/admin/services/room-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Room, CreateRoomDTO, UpdateRoomDTO, RoomStatus } from '../models/room.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class RoomApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/rooms`;

  getAll(params: {
    pageNumber: number;
    pageSize: number;
    roomTypeId?: number;
    includeRetired: boolean;
    searchQuery?: string;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<Room>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('includeRetired', params.includeRetired.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());
    if (params.roomTypeId != null) {
      httpParams = httpParams.set('roomTypeId', params.roomTypeId.toString());
    }
    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }
    return this.http.get<PaginatedResponse<Room>>(this.baseUrl, { params: httpParams });
  }

  create(dto: CreateRoomDTO): Observable<Room> {
    return this.http.post<Room>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateRoomDTO): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.baseUrl}/${id}`, dto);
  }

  getStatuses(params: {
    pageNumber: number;
    pageSize: number;
    roomTypeId?: number;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<RoomStatus>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortDescending', params.sortDescending.toString());
    if (params.roomTypeId != null) {
      httpParams = httpParams.set('roomTypeId', params.roomTypeId.toString());
    }
    return this.http.get<PaginatedResponse<RoomStatus>>(`${this.baseUrl}/status`, {
      params: httpParams,
    });
  }
}


# /Frontend/src/app/features/admin/services/room-type-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RoomType, CreateRoomTypeDTO, UpdateRoomTypeDTO } from '../models/room-type.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';
import { AvailableRoomType } from '../../user/models/available-room-type.model';

@Injectable({ providedIn: 'root' })
export class RoomTypeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/room-types`;

  getAll(params: {
    includeRetired: boolean;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
    searchQuery?: string;
  }): Observable<PaginatedResponse<RoomType>> {
    let httpParams = new HttpParams()
      .set('includeRetired', params.includeRetired.toString())
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());
    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }
    return this.http.get<PaginatedResponse<RoomType>>(this.baseUrl, { params: httpParams });
  }

  create(dto: CreateRoomTypeDTO): Observable<RoomType> {
    return this.http.post<RoomType>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateRoomTypeDTO): Observable<RoomType> {
    return this.http.patch<RoomType>(`${this.baseUrl}/${id}`, dto);
  }

  getById(id: number): Observable<RoomType> {
    return this.http.get<RoomType>(`${this.baseUrl}/${id}`);
  }

  getAvailability(params: {
    checkIn: string;
    checkOut: string;
    pageSize?: number;
    pageNumber?: number;
  }): Observable<PaginatedResponse<AvailableRoomType>> {
    const pageNum = params.pageNumber || 1;
    const size = params.pageSize || 50;
    let httpParams = new HttpParams()
      .set('checkIn', params.checkIn)
      .set('checkOut', params.checkOut)
      .set('pageNumber', pageNum.toString())
      .set('pageSize', size.toString());
    return this.http.get<PaginatedResponse<AvailableRoomType>>(`${this.baseUrl}/availability`, { params: httpParams });
  }
}


# /Frontend/src/app/features/admin/services/staff-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Staff, CreateStaffDTO, UpdateStaffDTO } from '../models/staff.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class StaffApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/staff`;

  getAll(params: {
    includeFired: boolean;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
    searchQuery?: string;
  }): Observable<PaginatedResponse<Staff>> {
    let httpParams = new HttpParams()
      .set('includeFired', params.includeFired.toString())
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }

    return this.http.get<PaginatedResponse<Staff>>(this.baseUrl, { params: httpParams }).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  create(dto: CreateStaffDTO): Observable<Staff> {
    return this.http.post<Staff>(this.baseUrl, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  update(id: number, dto: UpdateStaffDTO): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}`, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }
}


# /Frontend/src/app/features/auth/auth-page.component.html

<div class="auth-container">
  <mat-card>
    <mat-card-header>
      <div class="toggle-buttons">
        <button mat-button (click)="isLoginMode.set(true)" [class.active]="isLoginMode()" [attr.aria-pressed]="isLoginMode()">Login</button>
        <button mat-button (click)="isLoginMode.set(false)" [class.active]="!isLoginMode()" [attr.aria-pressed]="!isLoginMode()">Register</button>
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


# /Frontend/src/app/features/auth/auth-page.component.scss

.auth-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: 16px;
  box-sizing: border-box;

  mat-card {
    width: 100%;
    max-width: 450px;
    box-sizing: border-box;
  }

  mat-card-header {
    display: flex;
    justify-content: center;
    margin-bottom: 24px;
    width: 100%;
  }

  .toggle-buttons {
    display: flex;
    width: 100%;
    gap: 8px;

    button {
      flex: 1;
      padding: 12px;
      font-weight: 500;
      border-radius: 4px;
      transition: background-color 0.2s, color 0.2s;

      &.active {
        background-color: #3f51b5;
        color: #fff;
      }
    }
  }

  mat-card-content {
    display: flex;
    flex-direction: column;
  }
}

@media (max-width: 767px) {
  .auth-container {
    align-items: flex-start;
    padding-top: 48px;

    mat-card {
      margin: 0;
    }

    .toggle-buttons {
      flex-direction: column;
      
      button {
        width: 100%;
      }
    }
  }
}


# /Frontend/src/app/features/auth/auth-page.component.ts

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


# /Frontend/src/app/features/auth/components/alert.component.ts

import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div [class]="'alert-box ' + type" role="alert" aria-live="polite">
      <span class="alert-message">{{ message }}</span>
      <button mat-icon-button type="button" aria-label="Close alert" (click)="closed.emit()">
        <mat-icon>close</mat-icon>
      </button>
    </div>
  `,
  styles: [`
    .alert-box {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      margin: 16px 0;
      border-radius: 4px;
      font-size: 14px;
    }
    .alert-box.success {
      background-color: #e8f5e9;
      color: #2e7d32;
      border: 1px solid #c8e6c9;
    }
    .alert-box.error {
      background-color: #ffebee;
      color: #c62828;
      border: 1px solid #ffcdd2;
    }
    .alert-message {
      flex-grow: 1;
    }
    button {
      color: inherit;
    }
  `]
})
export class AlertComponent {
  @Input() type: 'success' | 'error' = 'success';
  @Input() message = '';
  @Output() closed = new EventEmitter<void>();
}


# /Frontend/src/app/features/auth/components/login-form.component.ts

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


# /Frontend/src/app/features/auth/components/register-form.component.ts

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


# /Frontend/src/app/features/front-desk/components/active-tickets-dialog/active-tickets-dialog.component.html

<h2 mat-dialog-title>Active Tickets</h2>
<mat-dialog-content>
  <mat-tab-group>
    <mat-tab label="Housekeeping ({{ data.housekeepingCount }})">
      <app-ticket-list type="housekeeping"></app-ticket-list>
    </mat-tab>
    <mat-tab label="Maintenance ({{ data.maintenanceCount }})">
      <app-ticket-list type="maintenance"></app-ticket-list>
    </mat-tab>
    <mat-tab label="Food Orders ({{ data.foodOrdersCount }})">
      <app-ticket-list type="foodOrder"></app-ticket-list>
    </mat-tab>
  </mat-tab-group>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button
    mat-button
    mat-dialog-close
  >
    Close
  </button>
</mat-dialog-actions>


# /Frontend/src/app/features/front-desk/components/active-tickets-dialog/active-tickets-dialog.component.ts

import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TicketListComponent } from '../ticket-list/ticket-list.component';

@Component({
  selector: 'app-active-tickets-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    TicketListComponent,
  ],
  templateUrl: './active-tickets-dialog.component.html',
})
export class ActiveTicketsDialogComponent {
  data = inject<{
    housekeepingCount: number;
    maintenanceCount: number;
    foodOrdersCount: number;
  }>(MAT_DIALOG_DATA);
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/billing-tab/billing-tab.component.html

<div class="billing-tab">
  @if (billLoading()) {
    <div style="display: flex; justify-content: center; padding: 16px;">
      <mat-spinner diameter="30"></mat-spinner>
    </div>
  } @else if (billError()) {
    <app-alert
      type="error"
      [message]="billError()!"
      (closed)="billError.set(null)"
    >
      <button
        mat-button
        (click)="fetchBill()"
      >
        Retry
      </button>
    </app-alert>
  } @else if (billDetails()) {
    <div class="bill-summary">
      <h3>Folio</h3>
      <p><strong>Guest:</strong> {{ billDetails().guestName }}</p>
      <p><strong>Nights Stayed:</strong> {{ billDetails().nightsStayed }}</p>
      <p><strong>Room Total:</strong> {{ billDetails().roomTotal | currency }}</p>
      <p><strong>Food Total:</strong> {{ billDetails().foodTotal | currency }}</p>
      <p><strong>Amenity Total:</strong> {{ billDetails().amenityTotal | currency }}</p>
      <p><strong>Total Bill:</strong> {{ billDetails().totalBill | currency }}</p>
      <p>
        <strong>Payment Status:</strong>
        <span
          class="status-chip"
          [class]="billDetails().paymentStatus"
          style="margin-left: 8px;"
        >{{ billDetails().paymentStatus }}</span>
      </p>
    </div>

    <mat-divider></mat-divider>

    @if (billDetails().foodItems && billDetails().foodItems.length > 0) {
      <div class="food-items" style="margin-top: 16px;">
        <h4>Food Items</h4>
        @for (item of billDetails().foodItems; track item) {
          <p>{{ item }}</p>
        }
      </div>
    }
    @if (billDetails().amenityItems && billDetails().amenityItems.length > 0) {
      <div class="amenity-items" style="margin-top: 16px;">
        <h4>Amenities</h4>
        @for (item of billDetails().amenityItems; track item) {
          <p>{{ item }}</p>
        }
      </div>
    }

    <mat-divider style="margin: 24px 0;"></mat-divider>

    @if (billDetails().paymentStatus === 'Pending') {
      <button
        mat-raised-button
        color="primary"
        (click)="showPayment.set(!showPayment())"
      >
        {{ showPayment() ? 'Cancel Payment' : 'Make Payment' }}
      </button>
      @if (showPayment()) {
        <app-payment-form
          [bookingId]="bookingId()"
          [amountDue]="billDetails().totalBill"
          (paymentComplete)="onPaymentComplete()"
        />
      }
    } @else {
      <p class="paid">Fully paid.</p>
    }
  } @else {
    <p>No billing information available.</p>
  }
</div>


# /Frontend/src/app/features/front-desk/components/booking-action-modal/billing-tab/billing-tab.component.scss

.billing-tab {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.bill-summary {
  h3 {
    margin-top: 0;
    margin-bottom: 12px;
    font-size: 1.2rem;
    font-weight: 500;
  }
  p {
    margin: 6px 0;
  }
}

.food-items,
.amenity-items {
  h4 {
    margin-top: 0;
    margin-bottom: 8px;
    font-size: 1rem;
    font-weight: 500;
  }
  p {
    margin: 4px 0;
    font-size: 0.9rem;
    color: #555;
  }
}

.status-chip {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.85rem;
  font-weight: 500;

  &.Paid {
    background: #c8e6c9;
    color: #2e7d32;
  }
  &.Pending {
    background: #ffcdd2;
    color: #c62828;
  }
}

.paid {
  font-weight: 500;
  color: #2e7d32;
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/billing-tab/billing-tab.component.ts

import { Component, input, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BillingApiService } from '../../../../user/services/billing-api.service';
import { AlertComponent } from '../../../../auth/components/alert.component';
import { PaymentFormComponent } from '../payment-form/payment-form.component';

@Component({
  selector: 'app-billing-tab',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    AlertComponent,
    PaymentFormComponent,
  ],
  templateUrl: './billing-tab.component.html',
  styleUrls: ['./billing-tab.component.scss'],
})
export class BillingTabComponent implements OnInit {
  bookingId = input.required<number>();

  billDetails = signal<any | null>(null);
  billLoading = signal(false);
  billError = signal<string | null>(null);
  showPayment = signal(false);

  private billingApi = inject(BillingApiService);
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.fetchBill();
  }

  fetchBill(): void {
    this.billLoading.set(true);
    this.billError.set(null);
    this.billingApi
      .getByBookingId(this.bookingId())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.billLoading.set(false))
      )
      .subscribe({
        next: (data: any) => this.billDetails.set(data),
        error: (err: any) => this.billError.set(this.extractErrorMessage(err)),
      });
  }

  onPaymentComplete(): void {
    this.showPayment.set(false);
    this.fetchBill();
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/booking-action-modal.component.html

<h2 mat-dialog-title>Booking #{{ booking().id }}</h2>
<mat-dialog-content>
  <mat-tab-group>
    <mat-tab label="Details">
      <div class="modal-content" style="padding: 16px 0;">
        <div class="booking-details">
          <h3>Guest Information</h3>
          <p><strong>Name:</strong> {{ booking().guestName ?? '—' }}</p>
          <p><strong>Email:</strong> {{ booking().guestEmail ?? '—' }}</p>
          <p><strong>Guest Count:</strong> {{ booking().guestCount }}</p>
          <p><strong>Origin:</strong> {{ booking().origin ?? '—' }}</p>

          <mat-divider></mat-divider>

          <h3>Booking Details</h3>
          <p>
            <strong>Status:</strong>
            <span
              class="status-chip"
              [class]="booking().bookingStatus"
              >{{ booking().bookingStatus }}</span
            >
          </p>
          <p><strong>Check‑In:</strong> {{ booking().checkInDate }}</p>
          <p><strong>Check‑Out:</strong> {{ booking().checkOutDate }}</p>
          <p>
            <strong>Booked At:</strong> {{ booking().bookedAt | date:'medium' }}
          </p>

          <mat-divider></mat-divider>

          <h3>Rooms</h3>
          @if (booking().rooms && booking().rooms.length > 0) {
            <div class="rooms-list">
              @for (room of booking().rooms; track room.id) {
                <div class="room-card">
                  <p>
                    <strong>Room Number:</strong> {{ room.roomNumber ?? 'Unassigned' }}
                  </p>
                  <p><strong>Room Type ID:</strong> {{ room.roomTypeId }}</p>
                  <p>
                    <strong>Locked Price:</strong> {{ room.lockedInPrice | currency }}
                  </p>
                </div>
              }
            </div>
          } @else {
            <p>No rooms assigned.</p>
          }

          <mat-divider></mat-divider>

          <h3>Amenities</h3>
          @if (booking().amenityIds && booking().amenityIds.length > 0) {
            <mat-chip-listbox aria-label="Amenities list">
              @for (id of booking().amenityIds; track id) {
                <mat-chip-option>Amenity #{{ id }}</mat-chip-option>
              }
            </mat-chip-listbox>
          } @else {
            <p>No amenities.</p>
          }
        </div>

        <!-- Action buttons – separated for future tab integration -->
        <div class="actions">
          @if (booking().bookingStatus === 'Booked') {
            <button
              mat-raised-button
              color="primary"
              (click)="checkIn()"
              [disabled]="loading()"
            >
              <mat-icon>login</mat-icon> Check‑In
            </button>
            <button
              mat-raised-button
              color="warn"
              (click)="cancelBooking()"
              [disabled]="loading()"
            >
              <mat-icon>cancel</mat-icon> Cancel Booking
            </button>
          }
          @if (booking().bookingStatus === 'CheckedIn') {
            <button
              mat-raised-button
              (click)="extendStay()"
              [disabled]="loading()"
              style="margin-right: 8px;"
            >
              <mat-icon>edit_calendar</mat-icon> Extend Stay
            </button>
            <button
              mat-raised-button
              color="primary"
              (click)="checkOut()"
              [disabled]="loading()"
            >
              <mat-icon>logout</mat-icon> Check‑Out
            </button>
          }
        </div>

        @if (error()) {
          <div style="margin-top: 16px;">
            <app-alert
              type="error"
              [message]="error()!"
              (closed)="error.set(null)"
            ></app-alert>
          </div>
        }
      </div>
    </mat-tab>
    <mat-tab label="Room Service">
      <div class="modal-content" style="padding: 16px 0;">
        <app-room-service-tab [booking]="booking()" />
      </div>
    </mat-tab>
    <mat-tab label="Billing">
      <div class="modal-content" style="padding: 16px 0;">
        <app-billing-tab [bookingId]="booking().id" />
      </div>
    </mat-tab>
  </mat-tab-group>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button
    mat-button
    mat-dialog-close
  >
    Close
  </button>
</mat-dialog-actions>


# /Frontend/src/app/features/front-desk/components/booking-action-modal/booking-action-modal.component.scss

.modal-content {
  max-height: 70vh;
  overflow-y: auto;
}

.booking-details {
  h3 {
    margin-top: 16px;
    margin-bottom: 8px;
    font-size: 1.1rem;
    font-weight: 500;
  }
  p {
    margin: 8px 0;
  }
}

.rooms-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.room-card {
  flex: 1 1 200px;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 12px;
  background: #fafafa;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-top: 24px;
  margin-bottom: 8px;
  button {
    min-width: 140px;
  }
}

.status-chip {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.85rem;
  font-weight: 500;

  &.CheckedIn {
    background: #c8e6c9;
    color: #2e7d32;
  }
  &.CheckedOut {
    background: #ffe0b2;
    color: #e65100;
  }
  &.Booked {
    background: #b3e5fc;
    color: #0277bd;
  }
  &.Cancelled {
    background: #ffcdd2;
    color: #c62828;
  }
}

mat-divider {
  margin: 16px 0;
}

.mat-mdc-tab-group {
  max-width: 100%;
}

.mat-mdc-tab-body {
  overflow-y: auto;
}

@media (max-width: 599px) {
  .modal-content {
    padding: 8px;
  }
  .actions {
    flex-direction: column;
    button {
      width: 100%;
    }
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/booking-action-modal.component.ts

import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';

import { BookingApiService } from '../../../user/services/booking-api.service';
import { Booking } from '../../../admin/models/booking.model';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AlertComponent } from '../../../auth/components/alert.component';
import { ExtendStayDialogComponent } from '../extend-stay-dialog/extend-stay-dialog.component';

import { MatTabsModule } from '@angular/material/tabs';
import { RoomServiceTabComponent } from './room-service-tab/room-service-tab.component';
import { BillingTabComponent } from './billing-tab/billing-tab.component';
import { CheckoutDialogComponent } from './checkout-dialog/checkout-dialog.component';

@Component({
  selector: 'app-booking-action-modal',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    AlertComponent,
    MatTabsModule,
    RoomServiceTabComponent,
    BillingTabComponent,
  ],
  templateUrl: './booking-action-modal.component.html',
  styleUrls: ['./booking-action-modal.component.scss'],
})
export class BookingActionModalComponent {
  data: { booking: Booking } = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<BookingActionModalComponent>);
  private bookingApi = inject(BookingApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  booking = signal<Booking>(this.data.booking);
  loading = signal(false);
  error = signal<string | null>(null);

  // ── Check‑In ────────────────────────────────────
  checkIn(): void {
    if (this.loading()) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Check‑In',
        message: `Check in guest: ${this.booking().guestName}?`,
      },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.loading.set(true);
        this.error.set(null);
        this.bookingApi
          .checkIn(this.booking().id)
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.loading.set(false))
          )
          .subscribe({
            next: (updatedBooking: Booking) => {
              const roomNumber = updatedBooking.rooms?.[0]?.roomNumber || 'assigned';
              this.snackBar.open(`Checked in successfully. Room: ${roomNumber}`, 'Close', { duration: 3000 });
              this.dialogRef.close(true); // signal parent to refresh
            },
            error: (err: any) => this.error.set(this.extractErrorMessage(err)),
          });
      });
  }

  // ── Cancel Booking ──────────────────────────────
  cancelBooking(): void {
    if (this.loading()) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Cancel Booking',
        message: `Are you sure you want to cancel booking #${this.booking().id} for ${this.booking().guestName}? This cannot be undone.`,
      },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.loading.set(true);
        this.error.set(null);
        this.bookingApi
          .cancel(this.booking().id)
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.loading.set(false))
          )
          .subscribe({
            next: () => {
              this.snackBar.open('Booking cancelled.', 'Close', { duration: 3000 });
              this.dialogRef.close(true);
            },
            error: (err: any) => this.error.set(this.extractErrorMessage(err)),
          });
      });
  }

  // ── Extend Stay ─────────────────────────────────
  extendStay(): void {
    if (this.loading()) return;
    const extendRef = this.dialog.open(ExtendStayDialogComponent, {
      data: {
        bookingId: this.booking().id,
        currentCheckOut: this.booking().checkOutDate,
      },
    });
    extendRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        if (result === true) {
          this.snackBar.open('Stay extended successfully.', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        }
      });
  }

  // ── Check‑Out ───────────────────────────────────
  checkOut(): void {
    if (this.loading()) return;
    const checkoutRef = this.dialog.open(CheckoutDialogComponent, {
      data: { bookingId: this.booking().id },
      width: '95vw',
      maxWidth: '600px',
      disableClose: true,
    });
    checkoutRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        if (result === true) {
          this.snackBar.open('Check‑out successful.', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        }
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/checkout-dialog/checkout-dialog.component.html

<h2 mat-dialog-title>Check‑Out</h2>
<mat-dialog-content>
  @if (step() === 'folio') {
    @if (loading()) {
      <div style="display: flex; justify-content: center; padding: 16px;">
        <mat-spinner diameter="30"></mat-spinner>
      </div>
    } @else if (billDetails()) {
      <div class="bill-summary">
        <p><strong>Guest:</strong> {{ billDetails().guestName }}</p>
        <p><strong>Total Bill:</strong> {{ billDetails().totalBill | currency }}</p>
        <p><strong>Payment Status:</strong> {{ billDetails().paymentStatus }}</p>
      </div>
      <div class="actions" style="margin-top: 24px; display: flex; gap: 12px;">
        @if (billDetails().paymentStatus === 'Pending') {
          <button
            mat-raised-button
            color="primary"
            (click)="step.set('payment')"
          >
            Proceed to Payment
          </button>
        } @else {
          <button
            mat-raised-button
            color="primary"
            (click)="processCheckOut()"
          >
            Check‑Out Now
          </button>
        }
      </div>
    } @else {
      <p>Unable to load billing details.</p>
    }
  }

  @if (step() === 'payment') {
    <app-payment-form
      [bookingId]="bookingId()"
      [amountDue]="billDetails()?.totalBill ?? 0"
      (paymentComplete)="onPaymentComplete()"
    />
  }

  @if (step() === 'confirm') {
    <div class="confirmation" style="display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 24px; gap: 16px;">
      <mat-icon color="primary" style="transform: scale(2.5); width: 24px; height: 24px;">check_circle</mat-icon>
      <p style="font-size: 1.2rem; font-weight: 500; margin-top: 16px;">Check‑out successful!</p>
    </div>
  }

  @if (step() === 'error' && error()) {
    <div style="margin-top: 16px;">
      <app-alert
        type="error"
        [message]="error()!"
        (closed)="error.set(null)"
      >
        <button
          mat-button
          (click)="processCheckOut()"
        >
          Retry Check‑Out
        </button>
      </app-alert>
    </div>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  @if (step() === 'folio' || step() === 'payment' || step() === 'error') {
    <button
      mat-button
      mat-dialog-close
    >
      Cancel
    </button>
  }
  @if (step() === 'confirm') {
    <button
      mat-button
      [mat-dialog-close]="true"
    >
      Close
    </button>
  }
</mat-dialog-actions>


# /Frontend/src/app/features/front-desk/components/booking-action-modal/checkout-dialog/checkout-dialog.component.scss

.bill-summary {
  p {
    margin: 8px 0;
  }
}

.actions {
  button {
    min-width: 140px;
  }
}

.confirmation {
  text-align: center;
  p {
    font-size: 1.1rem;
    color: #2e7d32;
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/checkout-dialog/checkout-dialog.component.ts

import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BillingApiService } from '../../../../user/services/billing-api.service';
import { BookingApiService } from '../../../../user/services/booking-api.service';
import { AlertComponent } from '../../../../auth/components/alert.component';
import { PaymentFormComponent } from '../payment-form/payment-form.component';

@Component({
  selector: 'app-checkout-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatIconModule,
    AlertComponent,
    PaymentFormComponent,
  ],
  templateUrl: './checkout-dialog.component.html',
  styleUrls: ['./checkout-dialog.component.scss'],
})
export class CheckoutDialogComponent implements OnInit {
  data: { bookingId: number } = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<CheckoutDialogComponent>);
  private billingApi = inject(BillingApiService);
  private bookingApi = inject(BookingApiService);
  private destroyRef = inject(DestroyRef);

  step = signal<'folio' | 'payment' | 'confirm' | 'error'>('folio');
  billDetails = signal<any | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  bookingId = signal<number>(this.data.bookingId);

  ngOnInit(): void {
    this.loadBill();
  }

  loadBill(): void {
    this.loading.set(true);
    this.error.set(null);
    this.billingApi
      .getByBookingId(this.bookingId())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: data => this.billDetails.set(data),
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  onPaymentComplete(): void {
    this.step.set('folio');
    this.loadBill();
  }

  processCheckOut(): void {
    this.loading.set(true);
    this.error.set(null);
    this.bookingApi
      .checkOut(this.bookingId())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: () => this.step.set('confirm'),
        error: (err: any) => {
          this.step.set('error');
          this.error.set(this.extractErrorMessage(err));
        },
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/food-order-panel/food-order-panel.component.html

<div class="food-order-panel">
  <h3>Order Food</h3>
  @if (loading() && menuItems().length === 0) {
    <div style="display: flex; justify-content: center; padding: 16px;">
      <mat-spinner diameter="30"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert
      type="error"
      [message]="error()!"
      (closed)="error.set(null)"
    >
      <button
        mat-button
        (click)="loadMenu()"
      >
        Retry
      </button>
    </app-alert>
  } @else {
    <mat-form-field appearance="outline" style="width: 100%; max-width: 300px; margin-bottom: 16px; display: block;">
      <mat-label>Deliver to Room</mat-label>
      <mat-select [formControl]="selectedRoomId">
        @for (room of validRooms(); track room.roomId) {
          <mat-option [value]="room.roomId">
            {{ room.roomNumber ?? 'Room ' + room.roomId }}
          </mat-option>
        }
      </mat-select>
      @if (selectedRoomId.invalid && selectedRoomId.touched) {
        <mat-error>Please select a room for delivery.</mat-error>
      }
    </mat-form-field>

    <app-menu-grid
      [menuItems]="menuItems()"
      [cartItems]="cartItems()"
      (addToCart)="onAddToCart($event)"
      (updateQuantity)="onUpdateCartQty($event)"
    />
  }
  <app-cart-drawer
    [cartItems]="cartItems()"
    [isOpen]="cartOpen()"
    (cartToggle)="cartOpen.set(!cartOpen())"
    (checkout)="placeOrder()"
    (updateQuantity)="onUpdateCartQty($event)"
  />
</div>


# /Frontend/src/app/features/front-desk/components/booking-action-modal/food-order-panel/food-order-panel.component.ts

import { Component, input, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { MenuGridComponent } from '../../../../user/components/food-order/menu-grid.component';
import { CartDrawerComponent } from '../../../../user/components/food-order/cart-drawer.component';
import { OrderApiService } from '../../../../user/services/order-api.service';
import { MenuItemApiService } from '../../../../user/services/menu-item-api.service';
import { MenuItem } from '../../../../admin/models/menu-item.model';
import { BookingRoom } from '../../../../admin/models/booking.model';
import { OrderItem } from '../../../../user/models/order-item.model';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AlertComponent } from '../../../../auth/components/alert.component';

@Component({
  selector: 'app-food-order-panel',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MenuGridComponent,
    CartDrawerComponent,
    MatSnackBarModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    AlertComponent,
  ],
  templateUrl: './food-order-panel.component.html',
})
export class FoodOrderPanelComponent implements OnInit {
  bookingId = input.required<number>();
  rooms = input.required<BookingRoom[]>();

  menuItems = signal<MenuItem[]>([]);
  cartItems = signal<OrderItem[]>([]);
  cartOpen = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);

  selectedRoomId = new FormControl<number>(0, { nonNullable: true, validators: Validators.required });

  private menuItemApi = inject(MenuItemApiService);
  private orderApi = inject(OrderApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  validRooms = computed(() => this.rooms().filter((r): r is typeof r & { roomId: number } => r.roomId !== null));

  ngOnInit(): void {
    this.loadMenu();
    const roomsList = this.validRooms();
    if (roomsList.length > 0) {
      this.selectedRoomId.setValue(roomsList[0].roomId);
    }
  }

  loadMenu(): void {
    this.loading.set(true);
    this.error.set(null);
    this.menuItemApi
      .getAll({ isAvailable: true, pageSize: 200 })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: res => this.menuItems.set(res.data),
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  onAddToCart(item: MenuItem): void {
    this.cartItems.update(items => {
      const existing = items.find(i => i.menuItemId === item.id);
      if (existing) {
        return items.map(i => (i.menuItemId === item.id ? { ...i, quantity: i.quantity + 1 } : i));
      }
      return [...items, { menuItemId: item.id, name: item.name, price: item.price, quantity: 1 }];
    });
    this.snackBar
      .open(`${item.name} added to cart`, 'View Cart', { duration: 2000 })
      .onAction()
      .subscribe(() => {
        this.cartOpen.set(true);
      });
  }

  onUpdateCartQty(event: { menuItemId: number; delta: number }): void {
    this.cartItems.update(items => {
      return items
        .map(i => (i.menuItemId === event.menuItemId ? { ...i, quantity: Math.max(0, i.quantity + event.delta) } : i))
        .filter(i => i.quantity > 0);
    });
  }

  placeOrder(): void {
    if (this.cartItems().length === 0) return;
    if (this.selectedRoomId.invalid) {
      this.selectedRoomId.markAsTouched();
      return;
    }

    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Confirm Order', message: 'Place this food order for the guest?' },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.orderApi
          .create({
            bookingId: this.bookingId(),
            roomId: this.selectedRoomId.value,
            items: this.cartItems().map(i => ({ menuItemId: i.menuItemId, quantity: i.quantity })),
          })
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.snackBar.open('Order placed successfully', 'Close', { duration: 3000 });
              this.cartItems.set([]); // clear cart
            },
            error: (err: any) => {
              const msg = typeof err.error === 'string' ? err.error : (err.error?.message || 'Failed to place order.');
              this.snackBar.open(msg, 'Close', { duration: 5000 });
            },
          });
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/housekeeping-request-panel/housekeeping-request-panel.component.html

<div class="request-panel">
  <h3>Request Housekeeping</h3>
  <form (ngSubmit)="submit()">
    <mat-form-field appearance="outline" style="width: 100%;">
      <mat-label>Room</mat-label>
      <mat-select [formControl]="selectedRoomId">
        @for (room of rooms(); track room.id) {
          @if (room.roomId !== null) {
            <mat-option [value]="room.roomId">
              {{ room.roomNumber ?? 'Room ' + room.roomId }}
            </mat-option>
          }
        }
      </mat-select>
    </mat-form-field>
    <mat-form-field appearance="outline" style="width: 100%;">
      <mat-label>Description</mat-label>
      <textarea
        matInput
        [formControl]="description"
        rows="2"
      ></textarea>
      @if (description.invalid && description.touched) {
        <mat-error>Min 5 characters required</mat-error>
      }
    </mat-form-field>
    <button
      mat-raised-button
      color="primary"
      type="submit"
      [disabled]="description.invalid || selectedRoomId.invalid || submitting()"
    >
      @if (submitting()) {
        <mat-spinner diameter="20" style="display: inline-block; margin-right: 8px; vertical-align: middle;"></mat-spinner>
      }
      Submit Request
    </button>
  </form>
</div>


# /Frontend/src/app/features/front-desk/components/booking-action-modal/housekeeping-request-panel/housekeeping-request-panel.component.ts

import { Component, input, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { HousekeepingApiService } from '../../../../user/services/housekeeping-api.service';
import { BookingRoom } from '../../../../admin/models/booking.model';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-housekeeping-request-panel',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
  ],
  templateUrl: './housekeeping-request-panel.component.html',
})
export class HousekeepingRequestPanelComponent implements OnInit {
  rooms = input.required<BookingRoom[]>();

  selectedRoomId = new FormControl<number | null>(null, {
    validators: Validators.required,
  });
  description = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(5)],
  });
  submitting = signal(false);

  private hkApi = inject(HousekeepingApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    const validRooms = this.rooms().filter(r => r.roomId !== null);
    if (validRooms.length > 0) {
      this.selectedRoomId.setValue(validRooms[0].roomId);
    }
  }

  submit(): void {
    if (this.submitting() || this.selectedRoomId.invalid || this.description.invalid) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Confirm Request', message: 'Send a housekeeping request for the selected room?' },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.submitting.set(true);

        const roomId = this.selectedRoomId.value!;
        this.hkApi
          .trigger(roomId, { description: this.description.value })
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.submitting.set(false))
          )
          .subscribe({
            next: () => {
              this.snackBar.open('Housekeeping request sent', 'Close', { duration: 3000 });
              this.description.reset();
            },
            error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 }),
          });
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/internal-ticket-panel/internal-ticket-panel.component.html

<div class="internal-ticket-panel">
  <h3>Create Internal Ticket</h3>
  <form (ngSubmit)="submit()">
    <mat-button-toggle-group [formControl]="ticketType" style="margin-bottom: 16px;">
      <mat-button-toggle value="housekeeping">Housekeeping</mat-button-toggle>
      <mat-button-toggle value="maintenance">Maintenance</mat-button-toggle>
    </mat-button-toggle-group>
    <mat-form-field appearance="outline" style="width: 100%;">
      <mat-label>Location</mat-label>
      <input
        matInput
        [formControl]="location"
      />
      @if (location.invalid && location.touched) {
        <mat-error>Location is required</mat-error>
      }
    </mat-form-field>
    <mat-form-field appearance="outline" style="width: 100%;">
      <mat-label>Description</mat-label>
      <textarea
        matInput
        [formControl]="description"
        rows="2"
      ></textarea>
      @if (description.invalid && description.touched) {
        <mat-error>Min 5 characters required</mat-error>
      }
    </mat-form-field>
    <button
      mat-raised-button
      color="primary"
      type="submit"
      [disabled]="location.invalid || description.invalid || submitting()"
    >
      @if (submitting()) {
        <mat-spinner diameter="20" style="display: inline-block; margin-right: 8px; vertical-align: middle;"></mat-spinner>
      }
      Create Ticket
    </button>
  </form>
</div>


# /Frontend/src/app/features/front-desk/components/booking-action-modal/internal-ticket-panel/internal-ticket-panel.component.ts

import { Component, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { HousekeepingApiService } from '../../../../user/services/housekeeping-api.service';
import { MaintenanceApiService } from '../../../../user/services/maintenance-api.service';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-internal-ticket-panel',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonToggleModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
  ],
  templateUrl: './internal-ticket-panel.component.html',
})
export class InternalTicketPanelComponent {
  ticketType = new FormControl<'housekeeping' | 'maintenance'>('housekeeping', {
    nonNullable: true,
  });
  location = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(200)],
  });
  description = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(5)],
  });
  submitting = signal(false);

  private hkApi = inject(HousekeepingApiService);
  private mtApi = inject(MaintenanceApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  submit(): void {
    if (this.submitting() || this.location.invalid || this.description.invalid) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Confirm Ticket', message: `Create an internal ${this.ticketType.value} ticket?` },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.submitting.set(true);

        const body = { location: this.location.value, description: this.description.value };
        const request$ =
          this.ticketType.value === 'housekeeping'
            ? this.hkApi.createInternal(body)
            : this.mtApi.createInternal(body);

        request$
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.submitting.set(false))
          )
          .subscribe({
            next: () => {
              this.snackBar.open('Internal ticket created', 'Close', { duration: 3000 });
              this.location.reset();
              this.description.reset();
            },
            error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 }),
          });
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/maintenance-request-panel/maintenance-request-panel.component.html

<div class="request-panel">
  <h3>Request Maintenance</h3>
  <form (ngSubmit)="submit()">
    <mat-form-field appearance="outline" style="width: 100%;">
      <mat-label>Room</mat-label>
      <mat-select [formControl]="selectedRoomId">
        @for (room of rooms(); track room.id) {
          @if (room.roomId !== null) {
            <mat-option [value]="room.roomId">
              {{ room.roomNumber ?? 'Room ' + room.roomId }}
            </mat-option>
          }
        }
      </mat-select>
    </mat-form-field>
    <mat-form-field appearance="outline" style="width: 100%;">
      <mat-label>Description</mat-label>
      <textarea
        matInput
        [formControl]="description"
        rows="2"
      ></textarea>
      @if (description.invalid && description.touched) {
        <mat-error>Min 5 characters required</mat-error>
      }
    </mat-form-field>
    <button
      mat-raised-button
      color="primary"
      type="submit"
      [disabled]="description.invalid || selectedRoomId.invalid || submitting()"
    >
      @if (submitting()) {
        <mat-spinner diameter="20" style="display: inline-block; margin-right: 8px; vertical-align: middle;"></mat-spinner>
      }
      Submit Request
    </button>
  </form>
</div>


# /Frontend/src/app/features/front-desk/components/booking-action-modal/maintenance-request-panel/maintenance-request-panel.component.ts

import { Component, input, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { MaintenanceApiService } from '../../../../user/services/maintenance-api.service';
import { BookingRoom } from '../../../../admin/models/booking.model';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-maintenance-request-panel',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
  ],
  templateUrl: './maintenance-request-panel.component.html',
})
export class MaintenanceRequestPanelComponent implements OnInit {
  rooms = input.required<BookingRoom[]>();

  selectedRoomId = new FormControl<number | null>(null, {
    validators: Validators.required,
  });
  description = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(5)],
  });
  submitting = signal(false);

  private mtApi = inject(MaintenanceApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    const validRooms = this.rooms().filter(r => r.roomId !== null);
    if (validRooms.length > 0) {
      this.selectedRoomId.setValue(validRooms[0].roomId);
    }
  }

  submit(): void {
    if (this.submitting() || this.selectedRoomId.invalid || this.description.invalid) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Confirm Request', message: 'Send a maintenance request for the selected room?' },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.submitting.set(true);

        const roomId = this.selectedRoomId.value!;
        this.mtApi
          .trigger(roomId, { description: this.description.value })
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.submitting.set(false))
          )
          .subscribe({
            next: () => {
              this.snackBar.open('Maintenance request sent', 'Close', { duration: 3000 });
              this.description.reset();
            },
            error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 }),
          });
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/payment-form/payment-form.component.html

<form
  [formGroup]="paymentForm"
  (ngSubmit)="submitPayment()"
  class="payment-form"
  style="display: flex; flex-direction: column; gap: 16px; margin-top: 16px;"
>
  <mat-form-field appearance="outline" style="width: 100%;">
    <mat-label>Amount</mat-label>
    <input
      matInput
      type="number"
      formControlName="amount"
      step="0.01"
    />
    @if (paymentForm.get('amount')?.invalid) {
      <mat-error>Enter a valid amount.</mat-error>
    }
  </mat-form-field>
  <mat-form-field appearance="outline" style="width: 100%;">
    <mat-label>Payment Method</mat-label>
    <mat-select formControlName="paymentMethod">
      <mat-option value="Cash">Cash</mat-option>
      <mat-option value="Credit Card">Credit Card</mat-option>
      <mat-option value="Bank Transfer">Bank Transfer</mat-option>
    </mat-select>
    @if (paymentForm.get('paymentMethod')?.invalid) {
      <mat-error>Select a payment method.</mat-error>
    }
  </mat-form-field>
  <mat-form-field appearance="outline" style="width: 100%;">
    <mat-label>Transaction ID</mat-label>
    <input
      matInput
      formControlName="transactionId"
    />
    @if (paymentForm.get('transactionId')?.invalid) {
      <mat-error>Enter a transaction ID.</mat-error>
    }
  </mat-form-field>
  @if (error()) {
    <app-alert
      type="error"
      [message]="error()!"
      (closed)="error.set(null)"
    ></app-alert>
  }
  <button
    mat-raised-button
    color="primary"
    type="submit"
    [disabled]="paymentForm.invalid || submitting()"
    style="align-self: flex-start; min-width: 140px;"
  >
    @if (submitting()) {
      <mat-spinner diameter="20" style="display: inline-block; margin-right: 8px; vertical-align: middle;"></mat-spinner>
    }
    Pay {{ paymentForm.get('amount')?.value | currency }}
  </button>
</form>


# /Frontend/src/app/features/front-desk/components/booking-action-modal/payment-form/payment-form.component.ts

import { Component, input, output, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BillingApiService } from '../../../../user/services/billing-api.service';
import { AlertComponent } from '../../../../auth/components/alert.component';

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    AlertComponent,
  ],
  templateUrl: './payment-form.component.html',
})
export class PaymentFormComponent implements OnInit {
  bookingId = input.required<number>();
  amountDue = input.required<number>();
  paymentComplete = output<void>();

  paymentForm = new FormGroup({
    amount: new FormControl<number>(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    paymentMethod: new FormControl<string>('', {
      nonNullable: true,
      validators: Validators.required,
    }),
    transactionId: new FormControl<string>('', {
      nonNullable: true,
      validators: Validators.required,
    }),
  });

  submitting = signal(false);
  error = signal<string | null>(null);

  private billingApi = inject(BillingApiService);
  private snackBar = inject(MatSnackBar);
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.paymentForm.patchValue({
      amount: this.amountDue(),
    });
  }

  submitPayment(): void {
    if (this.submitting() || this.paymentForm.invalid) return;
    this.submitting.set(true);
    this.error.set(null);

    const dto = this.paymentForm.getRawValue();
    this.billingApi
      .pay(this.bookingId(), dto)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.submitting.set(false))
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Payment processed.', 'Close', { duration: 3000 });
          this.paymentComplete.emit();
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/room-service-tab/room-service-tab.component.html

<div class="room-service-tab">
  <app-food-order-panel [bookingId]="booking().id" [rooms]="booking().rooms" />
  <mat-divider></mat-divider>
  <app-housekeeping-request-panel [rooms]="booking().rooms" />
  <mat-divider></mat-divider>
  <app-maintenance-request-panel [rooms]="booking().rooms" />
</div>


# /Frontend/src/app/features/front-desk/components/booking-action-modal/room-service-tab/room-service-tab.component.scss

.room-service-tab {
  display: flex;
  flex-direction: column;
  gap: 24px;
  padding: 16px 0;
}

mat-divider {
  margin: 8px 0;
}


# /Frontend/src/app/features/front-desk/components/booking-action-modal/room-service-tab/room-service-tab.component.ts

import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';

import { Booking } from '../../../../admin/models/booking.model';
import { FoodOrderPanelComponent } from '../food-order-panel/food-order-panel.component';
import { HousekeepingRequestPanelComponent } from '../housekeeping-request-panel/housekeeping-request-panel.component';
import { MaintenanceRequestPanelComponent } from '../maintenance-request-panel/maintenance-request-panel.component';
@Component({
  selector: 'app-room-service-tab',
  standalone: true,
  imports: [
    CommonModule,
    MatDividerModule,
    FoodOrderPanelComponent,
    HousekeepingRequestPanelComponent,
    MaintenanceRequestPanelComponent,
  ],
  templateUrl: './room-service-tab.component.html',
  styleUrls: ['./room-service-tab.component.scss'],
})
export class RoomServiceTabComponent {
  booking = input.required<Booking>();
}


# /Frontend/src/app/features/front-desk/components/extend-stay-dialog/extend-stay-dialog.component.html

<h2 mat-dialog-title>Extend Stay</h2>
<mat-dialog-content>
  <p>Current check‑out: {{ data.currentCheckOut }}</p>
  <mat-form-field appearance="outline" style="width: 100%; margin-top: 8px;">
    <mat-label>New check‑out date</mat-label>
    <input
      matInput
      [matDatepicker]="picker"
      [formControl]="newCheckOut"
      [min]="minDate"
      (click)="picker.open()"
    />
    <mat-datepicker-toggle
      matSuffix
      [for]="picker"
    ></mat-datepicker-toggle>
    <mat-datepicker #picker></mat-datepicker>
    @if (newCheckOut.invalid && newCheckOut.touched) {
      <mat-error>Please select a future date after the current check‑out.</mat-error>
    }
  </mat-form-field>
  @if (error()) {
    <app-alert
      type="error"
      [message]="error()!"
      (closed)="error.set(null)"
    ></app-alert>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button
    mat-button
    mat-dialog-close
  >
    Cancel
  </button>
  <button
    mat-raised-button
    color="primary"
    (click)="submit()"
    [disabled]="newCheckOut.invalid || submitting()"
  >
    @if (submitting()) {
      <mat-spinner diameter="20" style="display: inline-block; margin-right: 8px; vertical-align: middle;"></mat-spinner>
    }
    Extend Stay
  </button>
</mat-dialog-actions>


# /Frontend/src/app/features/front-desk/components/extend-stay-dialog/extend-stay-dialog.component.ts

import { Component, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, provideNativeDateAdapter } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BookingApiService } from '../../../user/services/booking-api.service';
import { AlertComponent } from '../../../auth/components/alert.component';

@Component({
  selector: 'app-extend-stay-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    AlertComponent,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './extend-stay-dialog.component.html',
  host: { 'style': 'min-width: 350px; display: block;' }
})
export class ExtendStayDialogComponent {
  data: { bookingId: number; currentCheckOut: string } = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<ExtendStayDialogComponent>);
  private bookingApi = inject(BookingApiService);
  private destroyRef = inject(DestroyRef);

  minDate: Date = this.parseDate(this.data.currentCheckOut);
  newCheckOut = new FormControl<Date | null>(null, { validators: Validators.required });
  submitting = signal(false);
  error = signal<string | null>(null);

  private parseDate(dateStr: string): Date {
    const parts = dateStr.split('-');
    if (parts.length === 3) {
      return new Date(+parts[2], +parts[1] - 1, +parts[0]);
    }
    return new Date(dateStr);
  }

  submit(): void {
    if (this.submitting() || this.newCheckOut.invalid) return;
    this.submitting.set(true);
    this.error.set(null);

    const newDate = this.newCheckOut.value!;
    const dto = { checkOutDate: newDate.toISOString() };

    this.bookingApi
      .extendStay(this.data.bookingId, dto)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.submitting.set(false))
      )
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (err: any) => this.error.set(err.error?.message || err.message || 'Extend stay failed.'),
      });
  }
}


# /Frontend/src/app/features/front-desk/components/guest-billing/folio-detail-dialog.component.ts

import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-folio-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Folio for Booking #{{ data.bookingId }}</h2>
    <mat-dialog-content>
      <p><strong>Guest:</strong> {{ data.guestName }}</p>
      <p><strong>Nights Stayed:</strong> {{ data.nightsStayed }}</p>
      <p><strong>Room Total:</strong> {{ data.roomTotal | currency }} ({{ data.roomBasePrice | currency }}/night)</p>
      <p><strong>Food Total:</strong> {{ data.foodTotal | currency }}</p>
      @if (data.foodItems && data.foodItems.length > 0) {
        <ul>
          @for (item of data.foodItems; track item) {
            <li>{{ item }}</li>
          }
        </ul>
      }
      <p><strong>Amenity Total:</strong> {{ data.amenityTotal | currency }}</p>
      @if (data.amenityItems && data.amenityItems.length > 0) {
        <ul>
          @for (item of data.amenityItems; track item) {
            <li>{{ item }}</li>
          }
        </ul>
      }
      <p><strong>Total Bill:</strong> {{ data.totalBill | currency }}</p>
      <p><strong>Payment Status:</strong> {{ data.paymentStatus }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Close</button>
    </mat-dialog-actions>
  `
})
export class FolioDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: any) {}
}


# /Frontend/src/app/features/front-desk/components/guest-billing/guest-billing.component.html

<div class="guest-billing">
  @if (loading()) {
    <div style="display: flex; justify-content: center; padding: 16px;">
      <mat-spinner diameter="30"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)"></app-alert>
  } @else {
    <!-- Latest Folio -->
    @if (latestBilling()) {
      <mat-card class="latest-folio-card" style="margin-bottom: 24px;">
        <mat-card-header>
          <mat-card-title>Latest Folio (Booking #{{ latestBilling()!.bookingId }})</mat-card-title>
        </mat-card-header>
        <mat-card-content style="padding-top: 16px;">
          <div class="bill-summary">
            <p><strong>Guest:</strong> {{ latestBilling()!.guestName }}</p>
            <p><strong>Nights Stayed:</strong> {{ latestBilling()!.nightsStayed }}</p>
            <p><strong>Room Total:</strong> {{ latestBilling()!.roomTotal | currency }} ({{ latestBilling()!.roomBasePrice | currency }}/night)</p>
            <p><strong>Food Total:</strong> {{ latestBilling()!.foodTotal | currency }}</p>
            <p><strong>Amenity Total:</strong> {{ latestBilling()!.amenityTotal | currency }}</p>
            <mat-divider style="margin: 12px 0;"></mat-divider>
            <p style="font-size: 1.2rem; font-weight: 500;">
              <strong>Total Bill:</strong> {{ latestBilling()!.totalBill | currency }}
            </p>
            <p>
              <strong>Payment Status:</strong>
              <span [class]="latestBilling()!.paymentStatus" style="margin-left: 8px; font-weight: 500;">
                {{ latestBilling()!.paymentStatus }}
              </span>
            </p>
          </div>
        </mat-card-content>
      </mat-card>
    } @else {
      <p>No billing information available.</p>
    }

    <!-- Old Folios (collapsible) -->
    @if (oldBilling().length > 0) {
      <mat-accordion>
        <mat-expansion-panel>
          <mat-expansion-panel-header>
            <mat-panel-title>Old Folios ({{ oldBilling().length }})</mat-panel-title>
          </mat-expansion-panel-header>
          
          <table mat-table [dataSource]="oldBilling()" class="mat-elevation-z0" style="width: 100%;">
            <ng-container matColumnDef="bookingId">
              <th mat-header-cell *matHeaderCellDef>Booking ID</th>
              <td mat-cell *matCellDef="let b">#{{ b.bookingId }}</td>
            </ng-container>
            <ng-container matColumnDef="checkOutDate">
              <th mat-header-cell *matHeaderCellDef>Check‑Out Date</th>
              <td mat-cell *matCellDef="let b">{{ b.checkOutDate | date:'mediumDate' }}</td>
            </ng-container>
            <ng-container matColumnDef="totalBill">
              <th mat-header-cell *matHeaderCellDef>Total</th>
              <td mat-cell *matCellDef="let b">{{ b.totalBill | currency }}</td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Details</th>
              <td mat-cell *matCellDef="let b">
                <button mat-icon-button (click)="openFolioDetail(b)" aria-label="View folio">
                  <mat-icon>visibility</mat-icon>
                </button>
              </td>
            </ng-container>
            
            <tr mat-header-row *matHeaderRowDef="['bookingId','checkOutDate','totalBill','actions']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['bookingId','checkOutDate','totalBill','actions']"></tr>
          </table>
        </mat-expansion-panel>
      </mat-accordion>
    }
  }
</div>


# /Frontend/src/app/features/front-desk/components/guest-billing/guest-billing.component.scss

.guest-billing {
  .bill-summary {
    p {
      margin: 8px 0;
      font-size: 0.95rem;
    }
  }

  .Paid {
    color: #2e7d32;
    background-color: #e8f5e9;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 0.85rem;
  }

  .Pending {
    color: #c62828;
    background-color: #ffebee;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 0.85rem;
  }

  table {
    margin-top: 16px;
    background: transparent;
  }

  th.mat-mdc-header-cell {
    font-weight: 500;
  }
}


# /Frontend/src/app/features/front-desk/components/guest-billing/guest-billing.component.ts

import { Component, input, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, of } from 'rxjs';
import { map, catchError, finalize } from 'rxjs/operators';

import { BillingApiService } from '../../../../features/user/services/billing-api.service';
import { Booking } from '../../../../features/admin/models/booking.model';
import { BillingFolio } from '../../../../features/user/models/billing-folio.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { FolioDetailDialogComponent } from './folio-detail-dialog.component';

interface BillingRecord extends BillingFolio {
  checkOutDate: Date;
}

@Component({
  selector: 'app-guest-billing',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatDividerModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatDialogModule,
    AlertComponent,
  ],
  templateUrl: './guest-billing.component.html',
  styleUrls: ['./guest-billing.component.scss'],
})
export class GuestBillingComponent implements OnInit {
  bookings = input.required<Booking[]>();

  billingRecords = signal<BillingRecord[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  latestBilling = computed(() => this.billingRecords()[0] || null);
  oldBilling = computed(() => this.billingRecords().slice(1));

  private readonly billingApi = inject(BillingApiService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.fetchAllBilling();
  }

  private parseDate(dateStr: string): Date {
    const parts = dateStr.split('-');
    if (parts.length === 3) {
      return new Date(+parts[2], +parts[1] - 1, +parts[0]);
    }
    return new Date(dateStr);
  }

  private fetchAllBilling(): void {
    const bookings = this.bookings();
    if (bookings.length === 0) return;

    this.loading.set(true);
    this.error.set(null);

    const requests = bookings.map(b =>
      this.billingApi.getByBookingId(b.id).pipe(
        map(data => ({
          ...data,
          bookingId: b.id,
          checkOutDate: this.parseDate(b.checkOutDate)
        } as unknown as BillingRecord)),
        catchError(() => of(null))
      )
    );

    forkJoin(requests)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (results) => {
          const valid = results.filter((r): r is BillingRecord => r !== null);
          valid.sort((a, b) => b.checkOutDate.getTime() - a.checkOutDate.getTime());
          this.billingRecords.set(valid);
        },
        error: (err) => {
          this.error.set(err?.message || 'Failed to fetch billing records.');
        }
      });
  }

  openFolioDetail(record: BillingRecord): void {
    this.dialog.open(FolioDetailDialogComponent, {
      data: record,
      width: '95vw',
      maxWidth: '600px',
    });
  }
}


# /Frontend/src/app/features/front-desk/components/success-dialog/success-dialog.component.ts

import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { BookingApiService } from '../../../../features/user/services/booking-api.service';

@Component({
  selector: 'app-success-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, CommonModule, MatProgressSpinnerModule],
  template: `
    <h2 mat-dialog-title>Booking Created</h2>
    <mat-dialog-content>
      <p>Booking #{{ data.bookingId }} for {{ data.guestName }} has been created.</p>
      <p>Would you like to check in now?</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="close()">Close</button>
      <button mat-raised-button color="primary" (click)="checkInNow()" [disabled]="checkingIn()">
        @if (checkingIn()) { <mat-spinner diameter="20"></mat-spinner> }
        Check-In Now
      </button>
    </mat-dialog-actions>
  `
})
export class SuccessDialogComponent {
  data: { bookingId: number; guestName: string } = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<SuccessDialogComponent>);
  private bookingApi = inject(BookingApiService);
  private snackBar = inject(MatSnackBar);
  checkingIn = signal(false);

  checkInNow(): void {
    this.checkingIn.set(true);
    this.bookingApi.checkIn(this.data.bookingId).pipe(
      finalize(() => this.checkingIn.set(false))
    ).subscribe({
      next: (updated) => {
        this.snackBar.open(`Checked in. Room: ${updated.rooms?.[0]?.roomNumber || 'assigned'}`, 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (err: HttpErrorResponse) => {
        const message = this.extractCheckInError(err);
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  private extractCheckInError(err: HttpErrorResponse): string {
    // If the response body is a plain string, use it directly.
    if (typeof err.error === 'string') {
      return err.error;
    }
    // If it's an object with a message property (e.g., JSON error)
    if (err.error?.message) {
      return err.error.message;
    }
    // Fallback to the HTTP status text or generic message
    return `Check-in failed (${err.status})`;
  }

  close(): void {
    this.dialogRef.close(false);
  }
}


# /Frontend/src/app/features/front-desk/components/ticket-list/ticket-list.component.html

@if (loading()) {
  <div style="display: flex; justify-content: center; padding: 24px;">
    <mat-spinner diameter="30"></mat-spinner>
  </div>
} @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetch()"
    >
      Retry
    </button>
  </app-alert>
} @else {
  <table
    mat-table
    [dataSource]="tickets()"
    matSort
    matSortDisableClear
  >
    <!-- ID Column -->
    <ng-container matColumnDef="id">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        ID
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ t.id }}
      </td>
    </ng-container>

    <!-- Room Column -->
    <ng-container matColumnDef="room">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Room
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ t.roomNumber ?? t.location ?? '—' }}
      </td>
    </ng-container>

    <!-- Description Column -->
    <ng-container matColumnDef="description">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Description
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ t.description ?? 'Order #' + t.id }}
      </td>
    </ng-container>

    <!-- Status Column -->
    <ng-container matColumnDef="status">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Status
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ t.status }}
      </td>
    </ng-container>

    <!-- Created Column -->
    <ng-container matColumnDef="createdAt">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Created
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ t.createdAt | date:'short' }}
      </td>
    </ng-container>

    <tr
      mat-header-row
      *matHeaderRowDef="['id','room','description','status','createdAt']"
    ></tr>
    <tr
      mat-row
      *matRowDef="let row; columns: ['id','room','description','status','createdAt']"
    ></tr>
  </table>
}


# /Frontend/src/app/features/front-desk/components/ticket-list/ticket-list.component.scss

table {
  width: 100%;
  margin-top: 8px;
}

.mat-mdc-header-cell {
  font-weight: bold;
}


# /Frontend/src/app/features/front-desk/components/ticket-list/ticket-list.component.ts

import { Component, input, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, Observable, of } from 'rxjs';
import { map, finalize, catchError } from 'rxjs/operators';
import { HousekeepingApiService } from '../../../user/services/housekeeping-api.service';
import { MaintenanceApiService } from '../../../user/services/maintenance-api.service';
import { OrderApiService } from '../../../user/services/order-api.service';
import { AlertComponent } from '../../../auth/components/alert.component';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    AlertComponent,
  ],
  templateUrl: './ticket-list.component.html',
  styleUrls: ['./ticket-list.component.scss'],
})
export class TicketListComponent implements OnInit {
  type = input.required<'housekeeping' | 'maintenance' | 'foodOrder'>();

  private readonly hkApi = inject(HousekeepingApiService);
  private readonly mtApi = inject(MaintenanceApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly destroyRef = inject(DestroyRef);

  tickets = signal<any[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetch();
  }

  fetch(): void {
    this.loading.set(true);
    this.error.set(null);
    let request$: Observable<any[]>;

    switch (this.type()) {
      case 'housekeeping':
        request$ = forkJoin([
          this.hkApi
            .getAll({ status: 'Pending', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
          this.hkApi
            .getAll({ status: 'InProgress', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
        ]).pipe(map(([p, ip]) => [...(p || []), ...(ip || [])]));
        break;
      case 'maintenance':
        request$ = forkJoin([
          this.mtApi
            .getAll({ status: 'Pending', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
          this.mtApi
            .getAll({ status: 'InProgress', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
        ]).pipe(map(([p, ip]) => [...(p || []), ...(ip || [])]));
        break;
      case 'foodOrder':
        request$ = forkJoin([
          this.orderApi
            .getAll({ status: 'Pending', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
          this.orderApi
            .getAll({ status: 'Preparing', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
        ]).pipe(map(([p, ip]) => [...(p || []), ...(ip || [])]));
        break;
    }

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => {
          const normalized = data.map((t) => {
            let description = t.description;
            if (this.type() === 'foodOrder') {
              const itemsArray = t.orderItems || [];
              description =
                itemsArray.length > 0
                  ? itemsArray
                      .map(
                        (i: any) => `${i.quantity}x ${i.menuItemName ?? 'Item #' + i.menuItemId}`,
                      )
                      .join(', ')
                  : `Order #${t.id}`;
            }
            return {
              ...t,
              status: t.orderStatus ?? t.status ?? 'Pending',
              roomNumber: t.roomNumber ?? (t.roomId ? 'Room ' + t.roomId : 'N/A'),
              description: description ?? `Order #${t.id}`,
              createdAt: t.generatedAt ?? t.createdAt ?? '',
            };
          });
          this.tickets.set(normalized);
        },
        error: (err) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  private extractErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'Failed to fetch tickets.';
  }
}


# /Frontend/src/app/features/front-desk/front-desk-shell.component.html

<mat-sidenav-container>
  <mat-sidenav
    #sidenav
    [mode]="isMobile() ? 'over' : 'side'"
    [opened]="isMobile() ? sidebarOpen() : true"
    aria-label="Front Desk navigation"
  >
    <mat-toolbar color="primary">Front Desk</mat-toolbar>
    <mat-nav-list>
      <a
        mat-list-item
        routerLink="/operations/front-desk/dashboard"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/front-desk/new-booking"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>add_circle</mat-icon>
        <span matListItemTitle>New Booking</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
      <button
        mat-icon-button
        (click)="sidebarOpen.set(!sidebarOpen())"
      >
        <mat-icon>menu</mat-icon>
      </button>
      }
      <span>{{ title() }}</span>
      <span class="spacer"></span>
      <button
        mat-icon-button
        [matMenuTriggerFor]="userMenu"
        aria-label="Open user menu"
      >
        <mat-icon>account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button
          mat-menu-item
          routerLink="/operations/front-desk/profile"
        >
          <mat-icon>manage_accounts</mat-icon> Profile
        </button>
        <button
          mat-menu-item
          (click)="logout()"
        >
          <mat-icon>logout</mat-icon> Logout
        </button>
      </mat-menu>
    </mat-toolbar>

    <div class="content">
      <router-outlet></router-outlet>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>


# /Frontend/src/app/features/front-desk/front-desk-shell.component.scss

mat-sidenav-container {
  height: 100vh;
  width: 100%;
}

mat-sidenav {
  width: 250px;
  border-right: 1px solid rgba(0, 0, 0, 0.12);

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

mat-sidenav-content {
  display: flex;
  flex-direction: column;
  height: 100%;

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

.spacer {
  flex: 1 1 auto;
}

.content {
  padding: 24px;
  flex-grow: 1;
  overflow-y: auto;
  box-sizing: border-box;
}

.active {
  background-color: rgba(63, 81, 181, 0.08);
  color: #3f51b5 !important;
  font-weight: 500;

  mat-icon {
    color: #3f51b5;
  }
}

@media (max-width: 1024px) {
  .content {
    padding: 16px;
  }
}


# /Frontend/src/app/features/front-desk/front-desk-shell.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-front-desk-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './front-desk-shell.component.html',
  styleUrls: ['./front-desk-shell.component.scss'],
})
export class FrontDeskShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  sidebarOpen = signal(false);
  title = signal('Front Desk');

  onNavClick(): void {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }
}


# /Frontend/src/app/features/front-desk/models/guest-profile.model.ts

export interface GuestProfile {
  guestName: string;
  guestEmail: string;
  totalStays: number;
  lastCheckInDate: string;
}


# /Frontend/src/app/features/front-desk/pages/dashboard.component.html

<div class="dashboard">
  @if (loadingSummary()) {
    <div style="display: flex; justify-content: center; padding: 32px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert
      type="error"
      [message]="error()!"
      (closed)="error.set(null)"
    >
      <button
        mat-button
        (click)="ngOnInit()"
      >
        Retry
      </button>
    </app-alert>
  } @else {
    <!-- Summary Cards Row -->
    <div class="summary-row">
      <!-- Today's Arrivals -->
      <mat-card class="summary-card">
        <mat-card-header>
          <mat-card-title>Today's Arrivals</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <span class="count">{{ arrivalsCount() }}</span>
        </mat-card-content>
      </mat-card>

      <!-- Today's Departures -->
      <mat-card class="summary-card">
        <mat-card-header>
          <mat-card-title>Today's Departures</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <span class="count">{{ departuresCount() }}</span>
        </mat-card-content>
      </mat-card>

      <!-- Active Tickets (clickable) -->
      <mat-card
        class="summary-card clickable"
        (click)="openActiveTickets()"
      >
        <mat-card-header>
          <mat-card-title>Active Tickets</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="ticket-breakdown">
            <span>Housekeeping: {{ activeTickets().housekeeping }}</span>
            <span>Maintenance: {{ activeTickets().maintenance }}</span>
            <span>Food Orders: {{ activeTickets().foodOrders }}</span>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  }

  <!-- Internal Ticket button (floating right) -->
  <div class="top-actions">
    <button
      mat-raised-button
      color="accent"
      (click)="openInternalTicket()"
    >
      <mat-icon>add_task</mat-icon> Create Internal Ticket
    </button>
  </div>

  <!-- Search Bar -->
  <div class="search-box">
    <mat-form-field
      appearance="outline"
      class="search-field"
    >
      <mat-label>Search guest name or email</mat-label>
      <input
        matInput
        [formControl]="searchControl"
      />
      <mat-icon matSuffix>search</mat-icon>
    </mat-form-field>
  </div>

  <!-- Search Results Table -->
  <h2>Search Results</h2>
  @if (searchLoading()) {
  <mat-spinner diameter="30"></mat-spinner>
  } @else if (searchError()) {
  <app-alert
    type="error"
    [message]="searchError()!"
    (closed)="searchError.set(null)"
  ></app-alert>
  } @else if (searchResults().length > 0) {
  <table
    mat-table
    [dataSource]="searchResults()"
    class="search-table"
  >
    <ng-container matColumnDef="guestName">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Guest Name
      </th>
      <td
        mat-cell
        *matCellDef="let r"
      >
        {{ r.guestName }}
      </td>
    </ng-container>
    <ng-container matColumnDef="guestEmail">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Email
      </th>
      <td
        mat-cell
        *matCellDef="let r"
      >
        {{ r.guestEmail }}
      </td>
    </ng-container>
    <ng-container matColumnDef="currentStatus">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Current Status
      </th>
      <td
        mat-cell
        *matCellDef="let r"
      >
        {{ r.currentStatus }}
      </td>
    </ng-container>
    <ng-container matColumnDef="actions">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Actions
      </th>
      <td
        mat-cell
        *matCellDef="let r"
      >
        <button
          mat-icon-button
          (click)="navigateToGuest(r.guestEmail)"
          aria-label="View guest details"
        >
          <mat-icon>visibility</mat-icon>
        </button>
      </td>
    </ng-container>
    <tr
      mat-header-row
      *matHeaderRowDef="['guestName','guestEmail','currentStatus','actions']"
    ></tr>
    <tr
      mat-row
      *matRowDef="let row; columns: ['guestName','guestEmail','currentStatus','actions']"
      class="clickable-row"
      (click)="navigateToGuest(row.guestEmail)"
    ></tr>
  </table>
  } @else { @if (searchControl.value) {
  <p>No results found.</p>
  } }

  <!-- Today's Movement Table -->
  <h2>Today's Movement</h2>
  <div class="movement-controls">
    <mat-button-toggle-group
      [formControl]="movementActiveFilter"
      (change)="onMovementToggle()"
    >
      <mat-button-toggle value="arrivals">Arrivals</mat-button-toggle>
      <mat-button-toggle value="departures">Departures</mat-button-toggle>
    </mat-button-toggle-group>
  </div>

  @if (movementLoading() && movementData().length === 0) {
  <mat-spinner diameter="30"></mat-spinner>
  } @else if (movementError()) {
  <app-alert
    type="error"
    [message]="movementError()!"
    (closed)="movementError.set(null)"
  ></app-alert>
  } @else if (movementData().length > 0) {
  <table
    mat-table
    [dataSource]="movementData()"
    class="movement-table"
  >
    <ng-container matColumnDef="guestName">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Guest Name
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        {{ b.guestName }}
      </td>
    </ng-container>
    <ng-container matColumnDef="room">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Room
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        {{ getRoomNumbers(b) || 'Unassigned' }}
      </td>
    </ng-container>
    <ng-container matColumnDef="status">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Status
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        {{ b.bookingStatus }}
      </td>
    </ng-container>
    <ng-container matColumnDef="actions">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Actions
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        <button
          mat-icon-button
          (click)="navigateToGuest(b.guestEmail)"
          aria-label="View guest details"
        >
          <mat-icon>visibility</mat-icon>
        </button>
      </td>
    </ng-container>
    <tr
      mat-header-row
      *matHeaderRowDef="['guestName','room','status','actions']"
    ></tr>
    <tr
      mat-row
      *matRowDef="let row; columns: ['guestName','room','status','actions']"
      (click)="navigateToGuest(row.guestEmail)"
      class="clickable-row"
    ></tr>
  </table>
  <mat-paginator
    [length]="movementTotal()"
    [pageIndex]="movementPage()"
    [pageSize]="movementPageSize()"
    [pageSizeOptions]="[10,25,50]"
    (page)="onMovementPageChange($event)"
  ></mat-paginator>
  } @else {
  <p>No {{ movementActiveFilter.value }} today.</p>
  }
</div>


# /Frontend/src/app/features/front-desk/pages/dashboard.component.scss

.dashboard {
  padding: 16px;
}

.summary-row {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 16px;
}

.summary-card {
  flex: 1 1 250px;
  cursor: pointer; // for the active tickets card
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
  border-radius: 8px;
  border: 1px solid rgba(0, 0, 0, 0.08);
  transition: transform 0.2s, box-shadow 0.2s;

  &.clickable:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
  }

  mat-card-header {
    margin-bottom: 12px;
  }

  mat-card-title {
    font-size: 1.1rem;
    font-weight: 500;
    color: rgba(0, 0, 0, 0.6);
  }

  .count {
    font-size: 3rem;
    font-weight: bold;
    color: #3f51b5;
  }

  .ticket-breakdown {
    display: flex;
    flex-direction: column;
    gap: 4px;

    span {
      font-size: 0.95rem;
      font-weight: 500;
      color: rgba(0, 0, 0, 0.7);
    }
  }
}

@media (max-width: 599px) {
  .summary-card {
    flex: 1 1 100%;
  }
}

.top-actions {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 12px;
}

.search-box {
  margin-bottom: 16px;
}

.search-field {
  width: 100%;
  max-width: 400px;
}

.clickable-row {
  cursor: pointer;
  &:hover {
    background-color: rgba(0, 0, 0, 0.04);
  }
}



# /Frontend/src/app/features/front-desk/pages/dashboard.component.ts

import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin, of, debounceTime, distinctUntilChanged } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { BookingApiService } from '../../user/services/booking-api.service';
import { HousekeepingApiService } from '../../user/services/housekeeping-api.service';
import { MaintenanceApiService } from '../../user/services/maintenance-api.service';
import { OrderApiService } from '../../user/services/order-api.service';
import { AlertComponent } from '../../auth/components/alert.component';
import { ActiveTicketsDialogComponent } from '../components/active-tickets-dialog/active-tickets-dialog.component';
import { InternalTicketPanelComponent } from '../components/booking-action-modal/internal-ticket-panel/internal-ticket-panel.component';
import { Booking } from '../../admin/models/booking.model';

interface SearchResult {
  guestName: string;
  guestEmail: string;
  currentStatus: string;
  bookings: Booking[];
}

@Component({
  selector: 'app-front-desk-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatButtonToggleModule,
    MatPaginatorModule,
    AlertComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class PlaceholderDashboardComponent implements OnInit {
  // Summary card signals (keep intact)
  arrivalsCount = signal(0);
  departuresCount = signal(0);
  activeTickets = signal<{
    housekeeping: number;
    maintenance: number;
    foodOrders: number;
  }>({
    housekeeping: 0,
    maintenance: 0,
    foodOrders: 0,
  });

  loadingSummary = signal(false);
  error = signal<string | null>(null);

  // Search
  searchControl = new FormControl('', { nonNullable: true });
  searchResults = signal<SearchResult[]>([]);
  searchLoading = signal(false);
  searchError = signal<string | null>(null);

  // Movement table (today's arrivals/departures)
  movementData = signal<Booking[]>([]);
  movementTotal = signal(0);
  movementLoading = signal(false);
  movementError = signal<string | null>(null);
  movementPage = signal(0);
  movementPageSize = signal(10);
  movementActiveFilter = new FormControl<'arrivals' | 'departures'>('arrivals', {
    nonNullable: true,
  });

  private readonly bookingApi = inject(BookingApiService);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.loadSummary();
    this.fetchMovement();

    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(value => this.onSearch(value.trim()));
  }

  private loadSummary(): void {
    this.loadingSummary.set(true);
    this.error.set(null);

    const arrivals$ = this.bookingApi.getAll({ movementStatus: 'incoming', pageNumber: 1, pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const departures$ = this.bookingApi.getAll({ movementStatus: 'outgoing', pageNumber: 1, pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );

    const hkPending$ = this.housekeepingApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const hkInProgress$ = this.housekeepingApi.getAll({ status: 'InProgress', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const mtPending$ = this.maintenanceApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const mtInProgress$ = this.maintenanceApi.getAll({ status: 'InProgress', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const foodPending$ = this.orderApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const foodPreparing$ = this.orderApi.getAll({ status: 'Preparing', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );

    forkJoin({
      arrivals: arrivals$,
      departures: departures$,
      hk: forkJoin([hkPending$, hkInProgress$]).pipe(map(([p, ip]) => p + ip)),
      mt: forkJoin([mtPending$, mtInProgress$]).pipe(map(([p, ip]) => p + ip)),
      food: forkJoin([foodPending$, foodPreparing$]).pipe(map(([p, ip]) => p + ip)),
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loadingSummary.set(false))
      )
      .subscribe({
        next: ({ arrivals, departures, hk, mt, food }) => {
          this.arrivalsCount.set(arrivals);
          this.departuresCount.set(departures);
          this.activeTickets.set({ housekeeping: hk, maintenance: mt, foodOrders: food });
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  openActiveTickets(): void {
    this.dialog.open(ActiveTicketsDialogComponent, {
      data: {
        housekeepingCount: this.activeTickets().housekeeping,
        maintenanceCount: this.activeTickets().maintenance,
        foodOrdersCount: this.activeTickets().foodOrders,
      },
      width: '90vw',
      maxWidth: '800px',
    });
  }

  private onSearch(query: string): void {
    if (!query) {
      this.searchResults.set([]);
      return;
    }
    this.searchLoading.set(true);
    this.bookingApi.getAll({ guestQuery: query, pageSize: 200 }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.searchLoading.set(false))
    ).subscribe({
      next: res => {
        const grouped = this.groupByEmail(res.data);
        this.searchResults.set(grouped);
      },
      error: (err: any) => this.searchError.set(this.extractErrorMessage(err))
    });
  }

  private groupByEmail(bookings: Booking[]): SearchResult[] {
    const map = new Map<string, Booking[]>();
    bookings.forEach(b => {
      if (!b.guestEmail) return;
      const arr = map.get(b.guestEmail) || [];
      arr.push(b);
      map.set(b.guestEmail, arr);
    });
    return Array.from(map.entries()).map(([email, bookings]) => {
      const statuses = bookings.map(b => b.bookingStatus);
      let currentStatus = 'Cancelled';
      if (statuses.includes('CheckedIn')) currentStatus = 'CheckedIn';
      else if (statuses.includes('Booked')) currentStatus = 'Booked';
      else if (statuses.includes('CheckedOut')) currentStatus = 'CheckedOut';
      return {
        guestName: bookings[0].guestName,
        guestEmail: email,
        currentStatus,
        bookings
      };
    });
  }

  fetchMovement(): void {
    this.movementLoading.set(true);
    const params: any = {
      pageNumber: this.movementPage() + 1,
      pageSize: this.movementPageSize(),
    };
    if (this.movementActiveFilter.value === 'arrivals') {
      params.movementStatus = 'incoming';
      params.bookingStatus = 'Booked';
    } else {
      params.movementStatus = 'outgoing';
      params.bookingStatus = 'CheckedIn';
    }
    this.bookingApi.getAll(params).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.movementLoading.set(false))
    ).subscribe({
      next: res => {
        this.movementData.set(res.data);
        this.movementTotal.set(res.totalCount);
      },
      error: (err: any) => this.movementError.set(this.extractErrorMessage(err))
    });
  }

  onMovementToggle(): void {
    this.movementPage.set(0);
    this.fetchMovement();
  }

  onMovementPageChange(event: PageEvent): void {
    this.movementPage.set(event.pageIndex);
    this.movementPageSize.set(event.pageSize);
    this.fetchMovement();
  }

  getRoomNumbers(booking: Booking): string {
    return booking.rooms?.filter(r => r.roomNumber).map(r => r.roomNumber).join(', ') || '';
  }

  navigateToGuest(email: string): void {
    const encoded = encodeURIComponent(email);
    this.router.navigate(['/operations/front-desk/guest', encoded]);
  }

  openInternalTicket(): void {
    this.dialog.open(InternalTicketPanelComponent, {
      width: '95vw',
      maxWidth: '500px',
    });
  }

  private extractErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/pages/guest-details.component.html

<div class="guest-details">
  <!-- Header with back button -->
  <div class="header">
    <button
      mat-icon-button
      routerLink="/operations/front-desk/dashboard"
      aria-label="Back to Dashboard"
    >
      <mat-icon>arrow_back</mat-icon>
    </button>
    <h1>Guest: {{ email() }}</h1>
  </div>

  @if (loading()) {
  <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchBookings()"
    >
      Retry
    </button>
  </app-alert>
  } @else {
  <mat-tab-group>
    <mat-tab label="Overview">
      <div class="tab-content">
        @if (guestProfile()) {
        <div class="profile-info">
          <h3>{{ guestProfile()!.guestName }}</h3>
          <p><strong>Email:</strong> {{ guestProfile()!.guestEmail }}</p>
          <p><strong>Total Stays:</strong> {{ guestProfile()!.totalStays }}</p>
          <p>
            <strong>Last Check-In:</strong> {{ guestProfile()!.lastCheckInDate }}
          </p>
          <p><strong>Current Status:</strong> {{ getOverallStatus() }}</p>
        </div>
        } @else {
        <p>Loading guest profile...</p>
        }
      </div>
    </mat-tab>

    <mat-tab label="Bookings">
      <div class="tab-content">
        @for (b of bookings(); track b.id) {
        <div class="booking-item">
          <p>
            <strong>ID:</strong> {{ b.id }} | <strong>Status:</strong> {{
            b.bookingStatus }} | <strong>Rooms:</strong> {{ getRoomNumbers(b) }}
          </p>
          <div class="actions">
            @if (b.bookingStatus === 'Booked') {
            <button
              mat-raised-button
              color="primary"
              (click)="checkIn(b)"
            >
              Check-In
            </button>
            <button
              mat-raised-button
              color="warn"
              (click)="cancelBooking(b)"
            >
              Cancel
            </button>
            } @if (b.bookingStatus === 'CheckedIn') {
            <button
              mat-raised-button
              (click)="extendStay(b)"
            >
              Extend Stay
            </button>
            <button
              mat-raised-button
              color="accent"
              (click)="checkOut(b)"
            >
              Check-Out
            </button>
            }
          </div>
        </div>
        }
      </div>
    </mat-tab>

    <mat-tab
      label="Room Service"
      [disabled]="!activeBooking()"
    >
      <div class="tab-content">
        @if (activeBooking()) {
        <app-room-service-tab [booking]="activeBooking()!" />
        } @else {
        <p>No active booking for room service.</p>
        }
      </div>
    </mat-tab>

    <mat-tab
      label="Billing"
      [disabled]="!activeBooking()"
    >
      <div class="tab-content">
        <app-guest-billing [bookings]="bookings()" />
      </div>
    </mat-tab>
  </mat-tab-group>
  }
</div>


# /Frontend/src/app/features/front-desk/pages/guest-details.component.scss

.guest-details {
  padding: 16px;

  .header {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 16px;
  }

  .tab-content {
    padding-top: 16px;
  }

  .booking-item {
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 12px;
    margin-bottom: 12px;

    .actions {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }
  }
}


# /Frontend/src/app/features/front-desk/pages/guest-details.component.ts

import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BookingApiService } from '../../user/services/booking-api.service';
import { BillingApiService } from '../../user/services/billing-api.service';
import { GuestApiService } from '../services/guest-api.service';
import { GuestProfile } from '../models/guest-profile.model';
import { Booking } from '../../admin/models/booking.model';
import { AlertComponent } from '../../auth/components/alert.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ExtendStayDialogComponent } from '../components/extend-stay-dialog/extend-stay-dialog.component';
import { CheckoutDialogComponent } from '../components/booking-action-modal/checkout-dialog/checkout-dialog.component';
import { RoomServiceTabComponent } from '../components/booking-action-modal/room-service-tab/room-service-tab.component';
import { GuestBillingComponent } from '../components/guest-billing/guest-billing.component';

@Component({
  selector: 'app-guest-details',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    AlertComponent,
    RoomServiceTabComponent,
    GuestBillingComponent,
  ],
  templateUrl: './guest-details.component.html',
  styleUrls: ['./guest-details.component.scss'],
})
export class GuestDetailsComponent implements OnInit {
  email = signal('');
  bookings = signal<Booking[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  guestProfile = signal<GuestProfile | null>(null);

  activeBooking = computed(() => {
    return (
      this.bookings().find(b => b.bookingStatus === 'CheckedIn') ||
      this.bookings()[0] ||
      null
    );
  });
  activeBookingId = computed(() => this.activeBooking()?.id ?? 0);

  private readonly route = inject(ActivatedRoute);
  private readonly bookingApi = inject(BookingApiService);
  private readonly billingApi = inject(BillingApiService);
  private readonly guestApi = inject(GuestApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    const encodedEmail = this.route.snapshot.paramMap.get('email') || '';
    const decodedEmail = decodeURIComponent(encodedEmail);
    this.email.set(decodedEmail);
    this.fetchBookings();
    this.guestApi.search(decodedEmail).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: res => {
        if (res.data && res.data.length > 0) {
          this.guestProfile.set(res.data[0]);
        }
      },
      error: (err: any) => console.error('Failed to load guest profile', err)
    });
  }

  fetchBookings(): void {
    this.loading.set(true);
    this.bookingApi.getAll({ guestQuery: this.email(), pageSize: 200 }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => this.bookings.set(res.data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  getOverallStatus(): string {
    const statuses = this.bookings().map(b => b.bookingStatus);
    if (statuses.includes('CheckedIn')) return 'CheckedIn';
    if (statuses.includes('Booked')) return 'Booked';
    if (statuses.includes('CheckedOut')) return 'CheckedOut';
    return 'Cancelled';
  }

  getRoomNumbers(booking: Booking): string {
    return booking.rooms?.filter(r => r.roomNumber).map(r => r.roomNumber).join(', ') || 'Unassigned';
  }

  checkIn(booking: Booking): void {
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Confirm Check-In', message: `Check in guest: ${booking.guestName}?` }
    });
    confirmRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
      if (!confirmed) return;
      this.bookingApi.checkIn(booking.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (updated) => {
          this.snackBar.open(`Checked in. Room: ${updated.rooms?.[0]?.roomNumber || 'assigned'}`, 'Close', { duration: 3000 });
          this.fetchBookings();
        },
        error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
      });
    });
  }

  cancelBooking(booking: Booking): void {
    this.bookingApi.cancel(booking.id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Booking cancelled.', 'Close', { duration: 3000 });
        this.fetchBookings();
      },
      error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  }

  extendStay(booking: Booking): void {
    const extendRef = this.dialog.open(ExtendStayDialogComponent, {
      data: { bookingId: booking.id, currentCheckOut: booking.checkOutDate },
      width: '400px',
      maxWidth: '90vw',
    });
    extendRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
      if (result) {
        this.snackBar.open('Stay extended.', 'Close', { duration: 3000 });
        this.fetchBookings();
      }
    });
  }

  checkOut(booking: Booking): void {
    const checkoutRef = this.dialog.open(CheckoutDialogComponent, {
      data: { bookingId: booking.id },
      width: '95vw',
      maxWidth: '600px',
      disableClose: true,
    });
    checkoutRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
      if (result === true) {
        this.snackBar.open('Check-out successful.', 'Close', { duration: 3000 });
        this.fetchBookings();
      }
    });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/pages/new-booking.component.html

<div class="new-booking-page">
  <h1>New Booking</h1>

  <mat-stepper linear #stepper [orientation]="isMobile() ? 'vertical' : 'horizontal'" (selectionChange)="onStepChange($event)">
    <!-- Step 1: Guest Details -->
    <mat-step [stepControl]="guestForm" label="Guest Details">
      <form [formGroup]="guestForm">
        <mat-form-field appearance="outline">
          <mat-label>First Name</mat-label>
          <input matInput formControlName="firstName" />
          <mat-error *ngIf="guestForm.get('firstName')?.invalid && guestForm.get('firstName')?.touched">First name is required (min 2 characters).</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Last Name</mat-label>
          <input matInput formControlName="lastName" />
          <mat-error *ngIf="guestForm.get('lastName')?.invalid && guestForm.get('lastName')?.touched">Last name is required (min 2 characters).</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" type="email" />
          <mat-error *ngIf="guestForm.get('email')?.invalid && guestForm.get('email')?.touched">A valid email is required.</mat-error>
        </mat-form-field>
        <div class="step-actions">
          <button mat-button matStepperNext>Next</button>
        </div>
      </form>
    </mat-step>

    <!-- Step 2: Dates & Guests -->
    <mat-step [stepControl]="datesForm" label="Dates & Guests">
      <form [formGroup]="datesForm">
        <mat-form-field appearance="outline">
          <mat-label>Check-in Date</mat-label>
          <input matInput [matDatepicker]="cinPicker" formControlName="checkInDate" />
          <mat-datepicker-toggle matSuffix [for]="cinPicker"></mat-datepicker-toggle>
          <mat-datepicker #cinPicker></mat-datepicker>
          <mat-error *ngIf="datesForm.get('checkInDate')?.invalid && datesForm.get('checkInDate')?.touched">Check-in date is required and must be today or later.</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Check-out Date</mat-label>
          <input matInput [matDatepicker]="coutPicker" formControlName="checkOutDate" />
          <mat-datepicker-toggle matSuffix [for]="coutPicker"></mat-datepicker-toggle>
          <mat-datepicker #coutPicker></mat-datepicker>
          <mat-error *ngIf="datesForm.get('checkOutDate')?.invalid && datesForm.get('checkOutDate')?.touched">Check-out date is required and must be after check-in.</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Number of Guests</mat-label>
          <input matInput type="number" formControlName="guestCount" min="1" max="20" />
          <mat-error *ngIf="datesForm.get('guestCount')?.invalid && datesForm.get('guestCount')?.touched">Guests must be between 1 and 20.</mat-error>
        </mat-form-field>
        <div class="step-actions">
          <button mat-button matStepperPrevious>Back</button>
          <button mat-button matStepperNext>Next</button>
        </div>
      </form>
    </mat-step>

    <!-- Step 3: Room Selection -->
    <mat-step [stepControl]="roomsForm" label="Select Rooms">
      <form [formGroup]="roomsForm">
        @if (roomsLoading()) {
          <mat-spinner diameter="30"></mat-spinner>
        } @else if (roomsError()) {
          <app-alert type="error" [message]="roomsError()!" (closed)="roomsError.set(null)"></app-alert>
        } @else {
          <div class="room-list">
            @for (room of availableRooms(); track room.roomTypeId) {
              <div class="room-item">
                <p>{{ room.name }} – {{ room.basePrice | currency }}/night – Max occupancy: {{ room.maxOccupancy }} – Available: {{ room.availableCount }}</p>
                <div class="qty-controls">
                  <button type="button" mat-icon-button (click)="decrementRoom(room.roomTypeId)" [disabled]="getRoomQuantity(room.roomTypeId) <= 0">
                    <mat-icon>remove</mat-icon>
                  </button>
                  <span>{{ getRoomQuantity(room.roomTypeId) }}</span>
                  <button type="button" mat-icon-button (click)="incrementRoom(room.roomTypeId)" [disabled]="getRoomQuantity(room.roomTypeId) >= room.availableCount">
                    <mat-icon>add</mat-icon>
                  </button>
                </div>
              </div>
            }
          </div>
          @if (capacityWarning()) {
            <p class="warning">{{ capacityWarning() }}</p>
          }
          <button mat-button matStepperNext [disabled]="totalSelectedQuantity() === 0 || capacityWarning()">Next</button>
        }
      </form>
    </mat-step>

    <!-- Step 4: Amenities -->
    <mat-step [stepControl]="amenitiesForm" label="Add Amenities">
      <form [formGroup]="amenitiesForm">
        @if (amenitiesLoading()) {
          <mat-spinner diameter="30"></mat-spinner>
        } @else {
          <div class="amenity-list">
            @for (amenity of availableAmenities(); track amenity.id; let i = $index) {
              <mat-checkbox [formControl]="getAmenityControl(i)">{{ amenity.name }} – {{ amenity.price | currency }}</mat-checkbox>
            }
          </div>
          <button mat-button matStepperNext>Next</button>
        }
      </form>
    </mat-step>

    <!-- Step 5: Review & Confirm -->
    <mat-step label="Review & Confirm">
      <div class="summary">
        <h3>Guest: {{ guestForm.value.firstName }} {{ guestForm.value.lastName }} ({{ guestForm.value.email }})</h3>
        <p>Check-in: {{ datesForm.value.checkInDate | date }}</p>
        <p>Check-out: {{ datesForm.value.checkOutDate | date }}</p>
        <p>Nights: {{ nights() }}</p>
        <p>Guests: {{ datesForm.value.guestCount }}</p>
        <h4>Rooms:</h4>
        <ul>
          @for (item of selectedRoomEntries(); track item.roomTypeId) {
            <li>{{ item.name }} x{{ item.quantity }} – {{ item.basePrice | currency }}/night – Subtotal: {{ item.quantity * item.basePrice * nights() | currency }}</li>
          }
        </ul>
        <h4>Amenities:</h4>
        <ul>
          @for (item of selectedAmenityEntries(); track item.id) {
            <li>{{ item.name }} – {{ item.price | currency }}</li>
          }
        </ul>
        <p><strong>Total Estimated: {{ estimatedTotal() | currency }}</strong></p>
      </div>
      <button mat-raised-button color="primary" (click)="confirmBooking()">Confirm Booking</button>
    </mat-step>
  </mat-stepper>
</div>


# /Frontend/src/app/features/front-desk/pages/new-booking.component.scss

.new-booking-page {
  padding: 24px;
  max-width: 900px;
  margin: 0 auto;

  h1 {
    margin-bottom: 24px;
    font-weight: 500;
  }

  form {
    display: flex;
    flex-direction: column;
    gap: 16px;
    padding: 16px 0;
    max-width: 500px;
  }

  mat-form-field {
    width: 100%;
  }

  .step-actions {
    display: flex;
    gap: 12px;
    margin-top: 16px;
  }

  .room-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-bottom: 24px;

    .room-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px;
      border: 1px solid rgba(0, 0, 0, 0.12);
      border-radius: 8px;
      background-color: #fafafa;
      gap: 16px;

      p {
        margin: 0;
        flex: 1;
      }

      .qty-controls {
        display: flex;
        align-items: center;
        gap: 12px;
        font-weight: 500;
      }
    }
  }

  .amenity-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin-bottom: 24px;
  }

  .warning {
    color: #f44336;
    font-weight: 500;
    margin-bottom: 16px;
  }

  .summary {
    background-color: #f5f5f5;
    padding: 20px;
    border-radius: 8px;
    margin-bottom: 24px;
    border: 1px solid rgba(0, 0, 0, 0.08);

    h3 {
      margin-top: 0;
      margin-bottom: 16px;
    }

    h4 {
      margin-top: 16px;
      margin-bottom: 8px;
      border-bottom: 1px solid rgba(0, 0, 0, 0.12);
      padding-bottom: 4px;
    }

    ul {
      margin: 0;
      padding-left: 20px;
    }

    li {
      margin-bottom: 6px;
    }
  }
}

@media (max-width: 599px) {
  .new-booking-page {
    padding: 16px;

    form {
      max-width: 100%;
    }

    .room-list {
      .room-item {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;

        .qty-controls {
          align-self: flex-end;
        }
      }
    }
  }
}


# /Frontend/src/app/features/front-desk/pages/new-booking.component.ts

import { Component, inject, signal, computed, ViewChild, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, FormArray, Validators, AbstractControl } from '@angular/forms';
import { MatStepperModule, MatStepper } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map, finalize } from 'rxjs/operators';

import { BookingApiService } from '../../user/services/booking-api.service';
import { RoomTypeApiService } from '../../user/services/room-type-api.service';
import { AmenityApiService } from '../../user/services/amenity-api.service';
import { AvailableRoomType } from '../../user/models/available-room-type.model';
import { Amenity } from '../../admin/models/amenity.model';
import { AlertComponent } from '../../auth/components/alert.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { SuccessDialogComponent } from '../components/success-dialog/success-dialog.component';

@Component({
  selector: 'app-front-desk-new-booking',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    MatCheckboxModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    AlertComponent,
  ],
  templateUrl: './new-booking.component.html',
  styleUrls: ['./new-booking.component.scss'],
})
export class FrontDeskBookingWizardComponent {
  @ViewChild('stepper') stepper!: MatStepper;

  private readonly bookingApi = inject(BookingApiService);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly amenityApi = inject(AmenityApiService);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 599px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  // Step 1: Guest Details
  guestForm = new FormGroup({
    firstName: new FormControl('', {
      validators: [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-ZÀ-ž\s\-']+$/)],
      nonNullable: true
    }),
    lastName: new FormControl('', {
      validators: [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-ZÀ-ž\s\-']+$/)],
      nonNullable: true
    }),
    email: new FormControl('', {
      validators: [Validators.required, Validators.email],
      nonNullable: true
    }),
  });

  // Step 2: Dates & Guests
  datesForm = new FormGroup({
    checkInDate: new FormControl<Date | null>(null, [Validators.required, this.futureDateValidator]),
    checkOutDate: new FormControl<Date | null>(null, [Validators.required]),
    guestCount: new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]),
  }, { validators: this.checkOutAfterCheckIn });

  // Step 3: Rooms
  roomsForm = new FormGroup({
    dummy: new FormControl<boolean>(false, { validators: [Validators.requiredTrue], nonNullable: true })
  });

  availableRooms = signal<AvailableRoomType[]>([]);
  selectedRoomQuantities = signal<Record<number, number>>({});
  roomsLoading = signal(false);
  roomsError = signal<string | null>(null);

  // Step 4: Amenities
  availableAmenities = signal<Amenity[]>([]);
  selectedAmenities = new FormArray<FormControl<boolean>>([]);
  amenitiesForm = new FormGroup({ amenities: this.selectedAmenities });
  amenitiesLoading = signal(false);

  // Submission
  submitting = signal(false);

  // Convert form values to signals so computed reacts
  private datesValues = toSignal(this.datesForm.valueChanges, { initialValue: this.datesForm.value });
  private amenitiesValues = toSignal(this.amenitiesForm.valueChanges, { initialValue: this.amenitiesForm.value });

  // Computed signals
  totalSelectedQuantity = computed(() => Object.values(this.selectedRoomQuantities()).reduce((a, b) => a + b, 0));

  capacityWarning = computed(() => {
    const totalCap = this.availableRooms().reduce(
      (sum, r) => sum + (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.maxOccupancy,
      0
    );
    const dates = this.datesValues();
    const guests = dates?.guestCount ?? 0;
    if (this.totalSelectedQuantity() > 0 && totalCap < guests) {
      return `The selected rooms can only accommodate ${totalCap} guests. You need ${guests}.`;
    }
    return null;
  });

  nights = computed(() => {
    const dates = this.datesValues();
    if (!dates || !dates.checkInDate || !dates.checkOutDate) return 0;
    const cin = new Date(dates.checkInDate);
    const cout = new Date(dates.checkOutDate);
    return Math.max(0, Math.ceil((cout.getTime() - cin.getTime()) / (1000 * 3600 * 24)));
  });

  estimatedTotal = computed(() => {
    const amenitiesVal = this.amenitiesValues();
    const nights = this.nights();
    const roomCost = this.availableRooms().reduce(
      (sum, r) => sum + (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.basePrice * nights,
      0
    );
    const selectedList = amenitiesVal?.amenities || [];
    const amenityCost = this.availableAmenities().reduce(
      (sum, a, i) => sum + (selectedList[i] ? a.price : 0),
      0
    );
    return roomCost + amenityCost;
  });

  selectedRoomEntries = computed(() => {
    const quantities = this.selectedRoomQuantities();
    return this.availableRooms()
      .filter(r => (quantities[r.roomTypeId] || 0) > 0)
      .map(r => ({
        roomTypeId: r.roomTypeId,
        name: r.name,
        basePrice: r.basePrice,
        maxOccupancy: r.maxOccupancy,
        quantity: quantities[r.roomTypeId]
      }));
  });

  selectedAmenityEntries = computed(() => {
    const list = this.availableAmenities();
    const amenitiesVal = this.amenitiesValues();
    const selectedList = amenitiesVal?.amenities || [];
    return list.filter((_, i) => selectedList[i] === true);
  });

  onStepChange(event: any): void {
    if (event.selectedIndex === 2) { // step 3 (0-based)
      this.fetchAvailableRooms();
    }
    if (event.selectedIndex === 3) {
      this.fetchAmenities();
    }
  }

  fetchAvailableRooms(): void {
    const cin = this.datesForm.value.checkInDate;
    const cout = this.datesForm.value.checkOutDate;
    if (!cin || !cout) return;

    this.roomsLoading.set(true);
    this.roomsError.set(null);

    this.roomTypeApi.getAvailable(this.formatDate(cin), this.formatDate(cout))
      .pipe(
        finalize(() => this.roomsLoading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (res) => {
          this.availableRooms.set(res.data);
          const quantities: Record<number, number> = {};
          res.data.forEach(r => {
            quantities[r.roomTypeId] = 0;
          });
          this.selectedRoomQuantities.set(quantities);
          this.updateRoomsFormValidity();
        },
        error: (err) => {
          this.roomsError.set(err.error?.message || err.message || 'Failed to load available rooms.');
        }
      });
  }

  fetchAmenities(): void {
    this.amenitiesLoading.set(true);
    this.amenityApi.getAll({ pageNumber: 1, pageSize: 100, isAvailable: true })
      .pipe(
        finalize(() => this.amenitiesLoading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (res) => {
          this.availableAmenities.set(res.data);
          this.selectedAmenities.clear();
          res.data.forEach(() => {
            this.selectedAmenities.push(new FormControl<boolean>(false, { nonNullable: true }));
          });
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || err.message || 'Failed to load amenities.', 'Close', { duration: 5000 });
        }
      });
  }

  incrementRoom(roomTypeId: number): void {
    const current = this.selectedRoomQuantities();
    const limit = this.availableRooms().find(r => r.roomTypeId === roomTypeId)?.availableCount ?? 0;
    const val = current[roomTypeId] || 0;
    if (val < limit) {
      this.selectedRoomQuantities.set({
        ...current,
        [roomTypeId]: val + 1
      });
      this.updateRoomsFormValidity();
    }
  }

  decrementRoom(roomTypeId: number): void {
    const current = this.selectedRoomQuantities();
    const val = current[roomTypeId] || 0;
    if (val > 0) {
      this.selectedRoomQuantities.set({
        ...current,
        [roomTypeId]: val - 1
      });
      this.updateRoomsFormValidity();
    }
  }

  getRoomQuantity(roomTypeId: number): number {
    return this.selectedRoomQuantities()[roomTypeId] || 0;
  }

  getAmenityControl(index: number): FormControl<boolean> {
    return this.selectedAmenities.at(index) as FormControl<boolean>;
  }

  updateRoomsFormValidity(): void {
    const isValid = this.totalSelectedQuantity() > 0 && !this.capacityWarning();
    this.roomsForm.controls.dummy.setValue(isValid);
    this.roomsForm.updateValueAndValidity();
  }

  confirmBooking(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Booking',
        message: `Create this booking? Total estimated: $${this.estimatedTotal().toFixed(2)}`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.performBooking();
      }
    });
  }

  private performBooking(): void {
    this.submitting.set(true);
    const roomTypeIds: number[] = [];
    const quantities = this.selectedRoomQuantities();
    Object.keys(quantities).forEach(key => {
      const typeId = Number(key);
      const qty = quantities[typeId] || 0;
      for (let i = 0; i < qty; i++) {
        roomTypeIds.push(typeId);
      }
    });

    const amenityIds = this.selectedAmenityEntries().map(a => a.id);

    const dto = {
      roomTypeIds,
      guestCount: this.datesForm.value.guestCount!,
      checkInDate: this.datesForm.value.checkInDate!.toISOString(),
      checkOutDate: this.datesForm.value.checkOutDate!.toISOString(),
      guestName: `${this.guestForm.value.firstName} ${this.guestForm.value.lastName}`,
      guestEmail: this.guestForm.value.email!,
      amenityIds
    };

    this.bookingApi.create(dto).pipe(
      finalize(() => this.submitting.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (booking) => {
        const successRef = this.dialog.open(SuccessDialogComponent, {
          data: { bookingId: booking.id, guestName: booking.guestName },
          width: '400px',
        });
        successRef.afterClosed().subscribe(() => {
          this.resetWizard();
        });
      },
      error: (err) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  }

  resetWizard(): void {
    this.guestForm.reset();
    this.datesForm.reset({ guestCount: 1 });
    this.selectedRoomQuantities.set({});
    this.availableRooms.set([]);
    this.availableAmenities.set([]);
    this.selectedAmenities.clear();
    this.updateRoomsFormValidity();
    if (this.stepper) {
      this.stepper.reset();
    }
  }

  formatDate(date: Date): string {
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
  }

  futureDateValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const value = control.value;
    if (!value) return null;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const dateVal = new Date(value);
    dateVal.setHours(0, 0, 0, 0);
    if (dateVal < today) {
      return { checkInInPast: true };
    }
    return null;
  }

  checkOutAfterCheckIn(control: AbstractControl): { [key: string]: boolean } | null {
    const cin = control.get('checkInDate')?.value as Date | null;
    const cout = control.get('checkOutDate')?.value as Date | null;
    if (!cin || !cout) return null;

    const cinDate = new Date(cin);
    cinDate.setHours(0, 0, 0, 0);
    const coutDate = new Date(cout);
    coutDate.setHours(0, 0, 0, 0);

    if (coutDate <= cinDate) {
      return { checkOutBeforeCheckIn: true };
    }
    return null;
  }

  private extractErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/front-desk/services/guest-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GuestProfile } from '../models/guest-profile.model';

@Injectable({ providedIn: 'root' })
export class GuestApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.baseUrl;

  search(
    query: string,
  ): Observable<{ totalCount: number; data: GuestProfile[] }> {
    return this.http.get<{ totalCount: number; data: GuestProfile[] }>(
      `${this.baseUrl}/guests`,
      { params: { search: query, pageSize: 25 } },
    );
  }
}


# /Frontend/src/app/features/housekeeping/housekeeping-shell.component.html

<mat-sidenav-container>
  <mat-sidenav
    #sidenav
    [mode]="isMobile() ? 'over' : 'side'"
    [opened]="isMobile() ? sidebarOpen() : true"
    aria-label="Navigation"
  >
    <mat-toolbar color="primary">{{ roleTitle }}</mat-toolbar>
    <mat-nav-list>
      <a
        mat-list-item
        routerLink="./dashboard"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
      <button
        mat-icon-button
        (click)="sidebarOpen.set(!sidebarOpen())"
      >
        <mat-icon>menu</mat-icon>
      </button>
      }
      <span>{{ roleTitle }}</span>
      <span class="spacer"></span>
      <button
        mat-icon-button
        [matMenuTriggerFor]="userMenu"
        aria-label="Open user menu"
      >
        <mat-icon>account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button
          mat-menu-item
          routerLink="./profile"
        >
          Profile
        </button>
        <button
          mat-menu-item
          (click)="logout()"
        >
          <mat-icon>logout</mat-icon> Logout
        </button>
      </mat-menu>
    </mat-toolbar>

    <div class="content">
      <router-outlet></router-outlet>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>


# /Frontend/src/app/features/housekeeping/housekeeping-shell.component.scss

mat-sidenav-container {
  height: 100vh;
  width: 100%;
}

mat-sidenav {
  width: 250px;
  border-right: 1px solid rgba(0, 0, 0, 0.12);

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

mat-sidenav-content {
  display: flex;
  flex-direction: column;
  height: 100%;

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

.spacer {
  flex: 1 1 auto;
}

.content {
  padding: 24px;
  flex-grow: 1;
  overflow-y: auto;
  box-sizing: border-box;
}

.active {
  background-color: rgba(63, 81, 181, 0.08);
  color: #3f51b5 !important;
  font-weight: 500;

  mat-icon {
    color: #3f51b5;
  }
}

@media (max-width: 1024px) {
  .content {
    padding: 16px;
  }
}


# /Frontend/src/app/features/housekeeping/housekeeping-shell.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-housekeeping-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './housekeeping-shell.component.html',
  styleUrls: ['./housekeeping-shell.component.scss'],
})
export class HousekeepingShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  sidebarOpen = signal(false);
  roleTitle = 'Housekeeping';
  role = 'housekeeping';

  onNavClick(): void {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }
}


# /Frontend/src/app/features/housekeeping/pages/dashboard.component.ts

import { Component, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { TaskDashboardComponent } from '../../../shared/components/task-dashboard/task-dashboard.component';
import { TaskDashboardConfig, Task, DetailSection } from '../../../shared/models/task.model';
import { HousekeepingApiService } from '../../user/services/housekeeping-api.service';
import { HousekeepingTask } from '../../admin/models/housekeeping-task.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-housekeeping-dashboard',
  standalone: true,
  imports: [TaskDashboardComponent],
  template: `<app-task-dashboard [config]="config" [refresh]="refreshTrigger()" />`,
})
export class HousekeepingDashboardComponent {
  private housekeepingApi = inject(HousekeepingApiService);
  private notificationService = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  refreshTrigger = signal(0);

  constructor() {
    this.notificationService.startConnection();

    this.notificationService.onAlert
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(notification => {
        this.refreshTrigger.update(n => n + 1);
        this.notificationService.showNotification('New Task', notification.description);
      });
  }

  config: TaskDashboardConfig = {
    entityName: 'Housekeeping Task',
    fetchTasks: (params: any) =>
      this.housekeepingApi.getAll(params).pipe(
        map((res: any) => ({
          totalCount: res.totalCount,
          data: res.data.map(
            (task: HousekeepingTask) =>
              ({
                id: task.id,
                status: task.status, // Pending, InProgress, Completed
                location: task.location || `Room ${task.roomId}`,
                description: task.description || 'No description provided.',
                createdAt: task.createdAt,
                raw: task,
              } as Task)
          ),
        }))
      ),
    updateTaskStatus: (id: number, newStatus: string) =>
      this.housekeepingApi.updateStatus(id, { status: newStatus }),
    statusOptions: [
      { value: 'All', label: 'All' },
      { value: 'Pending', label: 'Pending' },
      { value: 'InProgress', label: 'In Progress' },
      { value: 'Completed', label: 'Completed' },
    ],
    getLocation: (t: Task) => t.location,
    getDescription: (t: Task) => t.description,
    getDetailSections: (t: Task) => {
      const task = t.raw as HousekeepingTask;
      return [
        {
          title: 'Task Details',
          fields: [
            { label: 'Task ID', value: String(task.id) },
            { label: 'Room ID', value: task.roomId ? String(task.roomId) : 'N/A' },
            { label: 'Location', value: task.location || 'N/A' },
            { label: 'Origin Type', value: task.originType },
            { label: 'Status', value: task.status },
            { label: 'Description', value: task.description || 'N/A' },
            {
              label: 'Created At',
              value: task.createdAt ? new Date(task.createdAt).toLocaleString() : 'N/A',
            },
            {
              label: 'Started At',
              value: task.startedAt ? new Date(task.startedAt).toLocaleString() : 'N/A',
            },
            {
              label: 'Finished At',
              value: task.finishedAt ? new Date(task.finishedAt).toLocaleString() : 'N/A',
            },
          ],
        },
      ] as DetailSection[];
    },
  };
}


# /Frontend/src/app/features/kitchen/kitchen-shell.component.html

<mat-sidenav-container>
  <mat-sidenav
    #sidenav
    [mode]="isMobile() ? 'over' : 'side'"
    [opened]="isMobile() ? sidebarOpen() : true"
    aria-label="Navigation"
  >
    <mat-toolbar color="primary">{{ roleTitle }}</mat-toolbar>
    <mat-nav-list>
      <a
        mat-list-item
        routerLink="./dashboard"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
      <a
        mat-list-item
        routerLink="./menu-items"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>restaurant_menu</mat-icon>
        <span matListItemTitle>Menu Items</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
      <button
        mat-icon-button
        (click)="sidebarOpen.set(!sidebarOpen())"
      >
        <mat-icon>menu</mat-icon>
      </button>
      }
      <span>{{ roleTitle }}</span>
      <span class="spacer"></span>
      <button
        mat-icon-button
        [matMenuTriggerFor]="userMenu"
        aria-label="Open user menu"
      >
        <mat-icon>account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button
          mat-menu-item
          routerLink="./profile"
        >
          Profile
        </button>
        <button
          mat-menu-item
          (click)="logout()"
        >
          <mat-icon>logout</mat-icon> Logout
        </button>
      </mat-menu>
    </mat-toolbar>

    <div class="content">
      <router-outlet></router-outlet>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>


# /Frontend/src/app/features/kitchen/kitchen-shell.component.scss

mat-sidenav-container {
  height: 100vh;
  width: 100%;
}

mat-sidenav {
  width: 250px;
  border-right: 1px solid rgba(0, 0, 0, 0.12);

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

mat-sidenav-content {
  display: flex;
  flex-direction: column;
  height: 100%;

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

.spacer {
  flex: 1 1 auto;
}

.content {
  padding: 24px;
  flex-grow: 1;
  overflow-y: auto;
  box-sizing: border-box;
}

.active {
  background-color: rgba(63, 81, 181, 0.08);
  color: #3f51b5 !important;
  font-weight: 500;

  mat-icon {
    color: #3f51b5;
  }
}

@media (max-width: 1024px) {
  .content {
    padding: 16px;
  }
}


# /Frontend/src/app/features/kitchen/kitchen-shell.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-kitchen-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './kitchen-shell.component.html',
  styleUrls: ['./kitchen-shell.component.scss'],
})
export class KitchenShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  sidebarOpen = signal(false);
  roleTitle = 'Kitchen';
  role = 'kitchen';

  onNavClick(): void {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }
}


# /Frontend/src/app/features/kitchen/pages/dashboard.component.ts

import { Component, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { TaskDashboardComponent } from '../../../shared/components/task-dashboard/task-dashboard.component';
import { TaskDashboardConfig, Task, DetailSection } from '../../../shared/models/task.model';
import { OrderApiService } from '../../user/services/order-api.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-kitchen-dashboard',
  standalone: true,
  imports: [TaskDashboardComponent],
  template: `<app-task-dashboard [config]="config" [refresh]="refreshTrigger()" />`,
})
export class KitchenDashboardComponent {
  private orderApi = inject(OrderApiService);
  private notificationService = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  refreshTrigger = signal(0);

  constructor() {
    this.notificationService.startConnection();

    this.notificationService.onAlert
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(notification => {
        this.refreshTrigger.update(n => n + 1);
        this.notificationService.showNotification(
          notification.type === 'FoodOrder' ? 'New Order!' : 'Alert',
          notification.description
        );
      });
  }

  config: TaskDashboardConfig = {
    entityName: 'Food Order',
    fetchTasks: (params: any) =>
      this.orderApi.getAll(params).pipe(
        map((res: any) => ({
          totalCount: res.totalCount,
          data: res.data.map(
            (order: any) =>
              ({
                id: order.id,
                status: order.orderStatus ?? 'Pending',
                location: order.roomNumber ?? (order.roomId ? `Room ${order.roomId}` : 'N/A'),
                description: `Order #${order.id}`,
                createdAt: order.generatedAt ?? '',
                raw: order,
              } as Task)
          ),
        }))
      ),
    updateTaskStatus: (id: number, newStatus: string) =>
      this.orderApi.updateStatus(id, { status: newStatus }),
    statusOptions: [
      { value: 'All', label: 'All' },
      { value: 'Pending', label: 'Pending' },
      { value: 'Preparing', label: 'Preparing' },
      { value: 'Delivered', label: 'Delivered' },
    ],
    getLocation: (t: Task) => t.location,
    getDescription: (t: Task) => t.description,
    getDetailSections: (t: Task) => {
      const order = t.raw as any;
      const itemsArray = order.orderItems || [];
      const items = itemsArray.length > 0
        ? itemsArray.map((i: any) => `${i.quantity}x ${i.menuItemName ?? 'Item #' + i.menuItemId}`).join(', ')
        : 'None';
      return [
        {
          title: 'Order Information',
          fields: [
            { label: 'Order ID', value: String(order.id) },
            { label: 'Status', value: t.status },
            { label: 'Room', value: t.location },
            { label: 'Items', value: items },
            {
              label: 'Created At',
              value: t.createdAt ? new Date(t.createdAt).toLocaleString() : 'N/A',
            },
          ],
        },
      ] as DetailSection[];
    },
  };
}


# /Frontend/src/app/features/kitchen/pages/menu-items.component.html

<div class="menu-items-page">
  <h1>Menu Items</h1>

  <div class="filter-bar">
    <mat-form-field appearance="outline">
      <mat-label>Category</mat-label>
      <mat-select
        [formControl]="categoryFilter"
        (selectionChange)="applyFilters()"
      >
        <mat-option value="All">All</mat-option>
        @for (cat of categories(); track cat) {
        <mat-option [value]="cat">{{ cat }}</mat-option>
        }
      </mat-select>
    </mat-form-field>
    <mat-form-field appearance="outline">
      <mat-label>Search</mat-label>
      <input
        matInput
        [formControl]="searchControl"
        (keyup)="applyFiltersDebounced()"
      />
      <mat-icon matSuffix>search</mat-icon>
    </mat-form-field>
  </div>

  @if (loading()) {
  <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchMenuItems()"
    >
      Retry
    </button>
  </app-alert>
  } @else if (filteredItems().length === 0) {
  <p>No menu items found.</p>
  } @else {
  <div class="menu-grid">
    @for (item of filteredItems(); track item.id) {
    <mat-card class="menu-card">
      <mat-card-header>
        <mat-card-title>{{ item.name }}</mat-card-title>
        <mat-card-subtitle
          >{{ item.category || 'Other' }} – {{ item.price | currency
          }}</mat-card-subtitle
        >
      </mat-card-header>
      <mat-card-actions>
        <mat-slide-toggle
          [checked]="item.isAvailable"
          (change)="onToggleAvailability(item, $event.checked)"
          color="primary"
        >
          {{ item.isAvailable ? 'Available' : 'Unavailable' }}
        </mat-slide-toggle>
      </mat-card-actions>
    </mat-card>
    }
  </div>
  }
</div>


# /Frontend/src/app/features/kitchen/pages/menu-items.component.scss

.menu-items-page {
  padding: 8px 0;

  h1 {
    margin-bottom: 24px;
    font-weight: 500;
  }
}

.filter-bar {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
  margin-bottom: 24px;

  mat-form-field {
    width: 250px;
  }
}

.menu-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
  margin-top: 16px;

  .menu-card {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    min-height: 120px;
    transition: transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out;

    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    }

    mat-card-header {
      padding: 16px;

      mat-card-title {
        font-size: 1.1rem;
        font-weight: 500;
        margin-bottom: 4px;
      }

      mat-card-subtitle {
        color: rgba(0, 0, 0, 0.54);
      }
    }

    mat-card-actions {
      padding: 8px 16px 16px;
      display: flex;
      justify-content: flex-end;
    }
  }
}

@media (max-width: 599px) {
  .filter-bar {
    flex-direction: column;
    gap: 8px;

    mat-form-field {
      width: 100%;
    }
  }

  .menu-grid {
    grid-template-columns: 1fr;
  }
}


# /Frontend/src/app/features/kitchen/pages/menu-items.component.ts

import {
  Component,
  inject,
  signal,
  computed,
  OnInit,
  DestroyRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs';

import { MenuItemApiService } from '../../admin/services/menu-item-api.service';
import { MenuItem } from '../../admin/models/menu-item.model';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AlertComponent } from '../../auth/components/alert.component';

@Component({
  selector: 'app-kitchen-menu-items',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    AlertComponent,
  ],
  templateUrl: './menu-items.component.html',
  styleUrls: ['./menu-items.component.scss'],
})
export class KitchenMenuItemsComponent implements OnInit {
  private menuItemApi = inject(MenuItemApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  menuItems = signal<MenuItem[]>([]);
  filteredItems = signal<MenuItem[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  categoryFilter = new FormControl('All', { nonNullable: true });
  searchControl = new FormControl('', { nonNullable: true });

  categories = computed(() => {
    const cats = new Set(this.menuItems().map((i) => i.category || 'Other'));
    return Array.from(cats).sort();
  });

  ngOnInit(): void {
    this.fetchMenuItems();
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.applyFilters());
  }

  fetchMenuItems(): void {
    this.loading.set(true);
    this.error.set(null);
    this.menuItemApi
      .getAll({ pageNumber: 1, pageSize: 200, sortBy: 'id', sortDescending: false, isAvailable: undefined })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (res) => {
          this.menuItems.set(res.data);
          this.applyFilters();
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  applyFilters(): void {
    const category = this.categoryFilter.value;
    const search = this.searchControl.value.toLowerCase();
    let items = this.menuItems();
    if (category !== 'All') {
      items = items.filter((i) => (i.category || 'Other') === category);
    }
    if (search) {
      items = items.filter((i) => i.name.toLowerCase().includes(search));
    }
    this.filteredItems.set(items);
  }

  applyFiltersDebounced(): void {
    // Value changes debounced handled in ngOnInit.
  }

  onToggleAvailability(item: MenuItem, newValue: boolean): void {
    if (!newValue) {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Disable Menu Item',
          message: `Are you sure you want to make "${item.name}" unavailable?`,
        },
      });
      dialogRef
        .afterClosed()
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe((confirmed) => {
          if (confirmed) {
            this.updateAvailability(item, false);
          } else {
            this.menuItems.update((items) =>
              items.map((i) =>
                i.id === item.id ? { ...i, isAvailable: true } : i
              )
            );
            this.applyFilters();
          }
        });
    } else {
      this.updateAvailability(item, true);
    }
  }

  private updateAvailability(item: MenuItem, isAvailable: boolean): void {
    this.menuItemApi
      .updateStatus(item.id, isAvailable)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.snackBar.open(
            `"${item.name}" is now ${isAvailable ? 'available' : 'unavailable'}.`,
            'Close',
            { duration: 3000 }
          );
          this.menuItems.update((items) =>
            items.map((i) => (i.id === item.id ? { ...i, isAvailable } : i))
          );
          this.applyFilters();
        },
        error: (err: any) => {
          this.snackBar.open(this.extractErrorMessage(err), 'Close', {
            duration: 5000,
          });
          this.menuItems.update((items) =>
            items.map((i) =>
              i.id === item.id ? { ...i, isAvailable: !isAvailable } : i
            )
          );
          this.applyFilters();
        },
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/maintenance/maintenance-shell.component.html

<mat-sidenav-container>
  <mat-sidenav
    #sidenav
    [mode]="isMobile() ? 'over' : 'side'"
    [opened]="isMobile() ? sidebarOpen() : true"
    aria-label="Navigation"
  >
    <mat-toolbar color="primary">{{ roleTitle }}</mat-toolbar>
    <mat-nav-list>
      <a
        mat-list-item
        routerLink="./dashboard"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
      <button
        mat-icon-button
        (click)="sidebarOpen.set(!sidebarOpen())"
      >
        <mat-icon>menu</mat-icon>
      </button>
      }
      <span>{{ roleTitle }}</span>
      <span class="spacer"></span>
      <button
        mat-icon-button
        [matMenuTriggerFor]="userMenu"
        aria-label="Open user menu"
      >
        <mat-icon>account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button
          mat-menu-item
          routerLink="./profile"
        >
          Profile
        </button>
        <button
          mat-menu-item
          (click)="logout()"
        >
          <mat-icon>logout</mat-icon> Logout
        </button>
      </mat-menu>
    </mat-toolbar>

    <div class="content">
      <router-outlet></router-outlet>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>


# /Frontend/src/app/features/maintenance/maintenance-shell.component.scss

mat-sidenav-container {
  height: 100vh;
  width: 100%;
}

mat-sidenav {
  width: 250px;
  border-right: 1px solid rgba(0, 0, 0, 0.12);

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

mat-sidenav-content {
  display: flex;
  flex-direction: column;
  height: 100%;

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

.spacer {
  flex: 1 1 auto;
}

.content {
  padding: 24px;
  flex-grow: 1;
  overflow-y: auto;
  box-sizing: border-box;
}

.active {
  background-color: rgba(63, 81, 181, 0.08);
  color: #3f51b5 !important;
  font-weight: 500;

  mat-icon {
    color: #3f51b5;
  }
}

@media (max-width: 1024px) {
  .content {
    padding: 16px;
  }
}


# /Frontend/src/app/features/maintenance/maintenance-shell.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-maintenance-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './maintenance-shell.component.html',
  styleUrls: ['./maintenance-shell.component.scss'],
})
export class MaintenanceShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  sidebarOpen = signal(false);
  roleTitle = 'Maintenance';
  role = 'maintenance';

  onNavClick(): void {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }
}


# /Frontend/src/app/features/maintenance/pages/dashboard.component.ts

import { Component, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { TaskDashboardComponent } from '../../../shared/components/task-dashboard/task-dashboard.component';
import { TaskDashboardConfig, Task, DetailSection } from '../../../shared/models/task.model';
import { MaintenanceApiService } from '../../user/services/maintenance-api.service';
import { MaintenanceTask } from '../../admin/models/maintenance-task.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-maintenance-dashboard',
  standalone: true,
  imports: [TaskDashboardComponent],
  template: `<app-task-dashboard [config]="config" [refresh]="refreshTrigger()" />`,
})
export class MaintenanceDashboardComponent {
  private maintenanceApi = inject(MaintenanceApiService);
  private notificationService = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  refreshTrigger = signal(0);

  constructor() {
    this.notificationService.startConnection();

    this.notificationService.onAlert
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(notification => {
        this.refreshTrigger.update(n => n + 1);
        this.notificationService.showNotification('New Task', notification.description);
      });
  }

  config: TaskDashboardConfig = {
    entityName: 'Maintenance Task',
    fetchTasks: (params: any) =>
      this.maintenanceApi.getAll(params).pipe(
        map((res: any) => ({
          totalCount: res.totalCount,
          data: res.data.map(
            (task: MaintenanceTask) =>
              ({
                id: task.id,
                status: task.status, // Pending, InProgress, Completed
                location: task.location || `Room ${task.roomId}`,
                description: task.description || 'No description provided.',
                createdAt: task.createdAt,
                raw: task,
              } as Task)
          ),
        }))
      ),
    updateTaskStatus: (id: number, newStatus: string) =>
      this.maintenanceApi.updateStatus(id, { status: newStatus }),
    statusOptions: [
      { value: 'All', label: 'All' },
      { value: 'Pending', label: 'Pending' },
      { value: 'InProgress', label: 'In Progress' },
      { value: 'Completed', label: 'Completed' },
    ],
    getLocation: (t: Task) => t.location,
    getDescription: (t: Task) => t.description,
    getDetailSections: (t: Task) => {
      const task = t.raw as MaintenanceTask;
      return [
        {
          title: 'Task Details',
          fields: [
            { label: 'Task ID', value: String(task.id) },
            { label: 'Room ID', value: task.roomId ? String(task.roomId) : 'N/A' },
            { label: 'Location', value: task.location || 'N/A' },
            { label: 'Origin Type', value: task.originType },
            { label: 'Status', value: task.status },
            { label: 'Description', value: task.description || 'N/A' },
            {
              label: 'Created At',
              value: task.createdAt ? new Date(task.createdAt).toLocaleString() : 'N/A',
            },
            {
              label: 'Started At',
              value: task.startedAt ? new Date(task.startedAt).toLocaleString() : 'N/A',
            },
            {
              label: 'Finished At',
              value: task.finishedAt ? new Date(task.finishedAt).toLocaleString() : 'N/A',
            },
          ],
        },
      ] as DetailSection[];
    },
  };
}


# /Frontend/src/app/features/public/pages/amenities.component.html

<div class="amenities-page">
  <div class="hero-small">
    <h1>Amenities</h1>
    <p>Relax and enjoy our premium facilities designed for your comfort.</p>
  </div>

  @if (loading()) {
    <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
    <p class="error">{{ error() }}</p>
    <button mat-button (click)="fetchAmenities()">Retry</button>
  } @else {
    <div class="amenities-grid">
      @for (amenity of amenities(); track amenity.id) {
        <mat-card class="amenity-card">
          <div class="card-icon">
            <mat-icon>spa</mat-icon>
          </div>
          <mat-card-header>
            <mat-card-title>{{ amenity.name }}</mat-card-title>
            <mat-card-subtitle>{{ amenity.price ? (amenity.price | currency) : 'Complimentary' }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>{{ amenity.description || 'No description available.' }}</p>
          </mat-card-content>
        </mat-card>
      }
    </div>
  }
</div>


# /Frontend/src/app/features/public/pages/amenities.component.scss

.amenities-page {
  .hero-small {
    background: linear-gradient(rgba(0,0,0,0.5), rgba(0,0,0,0.5)), url('/assets/amenities-hero.jpg') center/cover no-repeat;
    height: 35vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: white;
    text-align: center;
    h1 { font-size: 2.5rem; margin-bottom: 8px; }
    p { font-size: 1.2rem; max-width: 500px; }
  }
  .amenities-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 24px;
    padding: 32px 16px;
  }
  .amenity-card {
    text-align: center;
    padding: 24px;
    .card-icon {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      background: #e3f2fd;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 16px;
      mat-icon { font-size: 40px; width: 40px; height: 40px; color: #1976d2; }
    }
  }
}

// Responsive adjustments
@media (max-width: 768px) {
  .amenities-page {
    .hero-small {
      height: 25vh;
    }
  }
}


# /Frontend/src/app/features/public/pages/amenities.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { AmenityApiService } from '../../admin/services/amenity-api.service';
import { Amenity } from '../../admin/models/amenity.model';

@Component({
  selector: 'app-public-amenities',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './amenities.component.html',
  styleUrls: ['./amenities.component.scss']
})
export class AmenitiesComponent implements OnInit {
  private amenityApi = inject(AmenityApiService);
  private destroyRef = inject(DestroyRef);

  amenities = signal<Amenity[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchAmenities();
  }

  fetchAmenities(): void {
    this.loading.set(true);
    this.amenityApi.getAll({ isAvailable: true, pageNumber: 1, pageSize: 200, sortBy: 'name', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => this.amenities.set(res.data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/public/pages/availability.component.html

<div class="availability-page">
  <!-- Search Form Section -->
  <section class="search-section">
    <h1 class="search-title">Curate Your Stay</h1>
    <form class="search-form" (ngSubmit)="searchAvailability()">
      <div class="form-field">
        <label class="field-label">Check-in</label>
        <mat-form-field class="date-field">
          <input
            matInput
            [matDatepicker]="picker1"
            [formControl]="checkIn"
            [min]="minDate"
            (click)="picker1.open()"
            readonly
          />
          <mat-datepicker-toggle matSuffix [for]="picker1"></mat-datepicker-toggle>
          <mat-datepicker #picker1></mat-datepicker>
        </mat-form-field>
      </div>
      <div class="form-separator"></div>
      <div class="form-field">
        <label class="field-label">Check-out</label>
        <mat-form-field class="date-field">
          <input
            matInput
            [matDatepicker]="picker2"
            [formControl]="checkOut"
            [min]="getMinCheckOutDate()"
            (click)="picker2.open()"
            readonly
          />
          <mat-datepicker-toggle matSuffix [for]="picker2"></mat-datepicker-toggle>
          <mat-datepicker #picker2></mat-datepicker>
        </mat-form-field>
      </div>
      <div class="form-separator"></div>
      <div class="form-field">
        <label class="field-label">Guests</label>
        <select class="guest-select" [formControl]="guests">
          <option [ngValue]="1">1 Guest</option>
          <option [ngValue]="2">2 Guests</option>
          <option [ngValue]="3">3 Guests</option>
          <option [ngValue]="4">4+ Guests</option>
        </select>
      </div>
      <div class="form-separator"></div>
      <button type="submit" class="search-btn arrow-link" [disabled]="checkIn.invalid || checkOut.invalid || guests.invalid || searchLoading()">
        <span>Search</span>
        <span class="material-symbols-outlined arrow-icon">arrow_right_alt</span>
      </button>
    </form>
  </section>

  <!-- Results Section -->
  <section class="results-section">
    @if (searchLoading()) {
      <div class="loading-spinner">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (searchError()) {
      <p class="error-message">{{ searchError() }}</p>
    } @else if (hasSearched() && availableRooms().length === 0) {
      <!-- Empty State -->
      <div class="empty-state">
        <h2>Awaiting your next visit</h2>
        <p>No residences match these precise dates. We invite you to explore alternative timing or consult our concierge for unlisted availability.</p>
      </div>
    } @else {
      @for (room of availableRooms(); track room.roomTypeId) {
        <article class="result-card group">
          <div class="card-image hover-pan">
            <img [src]="getFirstImage(room)" alt="{{ room.name }}" />
            <div class="image-overlay"></div>
          </div>
          <div class="card-info">
            <div class="info-top">
              <div class="availability-dots">
                <span class="dot filled"></span>
                <span class="dot" [class.filled]="room.availableCount >= 2"></span>
                <span class="dot" [class.filled]="room.availableCount >= 3"></span>
                <span class="availability-label">
                  {{ room.availableCount <= 1 ? 'Last Chance' : (room.availableCount <= 2 ? 'Limited' : 'Available') }}
                </span>
              </div>
              <h2 class="room-name">{{ room.name }}</h2>
              <p class="room-description">{{ room.description || 'A private sanctuary of silence and space.' }}</p>
            </div>
            <div class="info-bottom">
              <div class="price">
                <span class="price-label">Per Night</span>
                <span class="price-value">{{ room.basePrice | currency }}</span>
              </div>
              <button class="book-btn slide-in-btn" (click)="bookNow(room)">Book Now</button>
            </div>
          </div>
        </article>
      }
    }
  </section>
</div>


# /Frontend/src/app/features/public/pages/availability.component.scss

@import '../../../../styles/theme/index';

.availability-page {
  padding-top: 6rem;
  overflow-x: hidden;
}

// ── Search Section ───────────────────────────────
.search-section {
  padding: 4rem var(--margin-mobile);
  border-bottom: 1px solid rgba(228, 194, 133, 0.05);
  background: var(--color-surface-container-lowest);
  @media (min-width: 768px) {
    padding: 6rem var(--margin-desktop);
  }
}

.search-title {
  @include font-headline-md;
  font-size: clamp(2rem, 5vw, 2.5rem);
  color: var(--color-secondary);
  text-align: center;
  font-style: italic;
  margin-bottom: 3rem;
}

.search-form {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: 2rem;
  max-width: 1000px;
  margin: 0 auto;
  @media (min-width: 1024px) {
    flex-direction: row;
    align-items: center;
    gap: 1rem;
  }
}

.form-field {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.field-label {
  @include font-label-caps;
  font-size: 0.625rem;
  color: rgba(228, 194, 133, 0.5);
  margin-bottom: 0.5rem;
  text-transform: uppercase;
  letter-spacing: 0.2em;
}

// Override Material date field to be borderless
.date-field {
  ::ng-deep .mat-mdc-form-field-focus-overlay { display: none; }
  ::ng-deep .mat-mdc-text-field-wrapper {
    background: transparent !important;
    padding: 0;
  }
  ::ng-deep .mat-mdc-form-field-infix {
    padding: 0;
  }
  ::ng-deep .mdc-line-ripple { display: none; }
  ::ng-deep .mat-mdc-form-field-subscript-wrapper { display: none; }
  ::ng-deep input {
    color: var(--color-on-surface) !important;
    @include font-body-lg;
    cursor: pointer;
  }
  ::ng-deep .mat-datepicker-toggle .mat-mdc-icon-button svg {
    fill: var(--color-secondary);
  }
}

.guest-select {
  background: transparent;
  border: none;
  color: var(--color-on-surface);
  @include font-body-lg;
  padding: 0;
  outline: none;
  cursor: pointer;
  appearance: none;
  option {
    background: var(--color-surface-container);
    color: var(--color-on-surface);
  }
}

.form-separator {
  width: 1px;
  height: 40px;
  background: linear-gradient(to bottom, transparent, var(--color-secondary), transparent);
  opacity: 0.3;
  display: none;
  @media (min-width: 1024px) {
    display: block;
    flex-shrink: 0;
  }
}

.search-btn {
  background: transparent;
  border: none;
  color: var(--color-secondary);
  @include font-label-caps;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  padding: 0;
  transition: opacity 0.3s;
  &:disabled { opacity: 0.3; cursor: not-allowed; }
  .arrow-icon {
    transition: transform 0.4s cubic-bezier(0.23, 1, 0.32, 1);
  }
  &:hover:not(:disabled) .arrow-icon {
    transform: translateX(8px) scaleX(1.2);
  }
}

// ── Results Section ──────────────────────────────
.results-section {
  max-width: var(--container-max);
  margin: 0 auto;
  padding: var(--section-gap) var(--margin-mobile);
  @media (min-width: 768px) {
    padding: var(--section-gap) var(--margin-desktop);
  }
  display: flex;
  flex-direction: column;
  gap: 3rem;
}

.loading-spinner, .error-message {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 4rem 0;
}
.error-message { @include font-body-lg; color: var(--color-error); }

// Empty State
.empty-state {
  text-align: center;
  padding: 8rem 1rem;
  h2 {
    @include font-display-lg-mobile;
    font-size: clamp(2rem, 6vw, 3rem);
    color: var(--color-on-surface);
    font-style: italic;
    margin-bottom: 1rem;
    position: relative;
    display: inline-block;
    &::after {
      content: '';
      position: absolute;
      left: 0;
      bottom: -8px;
      width: 100%;
      height: 1px;
      background: var(--color-secondary);
      animation: underlineGrow 2s ease forwards;
    }
  }
  p {
    @include font-body-lg;
    color: rgba(228, 226, 221, 0.5);
    max-width: 500px;
    margin: 1.5rem auto 0;
  }
}

@keyframes underlineGrow {
  from { transform: scaleX(0); transform-origin: left; }
  to { transform: scaleX(1); }
}

// Result Card
.result-card {
  display: flex;
  flex-direction: column;
  border: 1px solid rgba(228, 194, 133, 0.1);
  background: var(--color-surface-container-low);
  cursor: pointer;
  overflow: hidden;
  transition: border-color 0.3s;
  &:hover { border-color: rgba(228, 194, 133, 0.3); }
  @media (min-width: 768px) {
    flex-direction: row;
    height: 580px;
  }
}

.card-image {
  position: relative;
  width: 100%;
  height: 350px;
  overflow: hidden;
  @media (min-width: 768px) {
    width: 60%;
    height: 100%;
  }
  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 3s cubic-bezier(0.4, 0, 0.2, 1);
  }
  .image-overlay {
    position: absolute;
    inset: 0;
    background: rgba(0, 0, 0, 0.2);
  }
}

.result-card:hover {
  .card-image img {
    animation: kenBurns 15s ease-in-out infinite alternate;
  }
}

@keyframes kenBurns {
  0% { transform: scale(1) translate(0, 0); }
  100% { transform: scale(1.1) translate(-2%, -2%); }
}

.card-info {
  flex: 1;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  background: var(--color-surface-container);
  @media (min-width: 768px) {
    padding: 3rem;
  }
}

.availability-dots {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-bottom: 1.5rem;
  .dot {
    width: 0.5rem;
    height: 0.5rem;
    border: 1px solid rgba(228, 194, 133, 0.3);
    &.filled { background: var(--color-secondary); border-color: var(--color-secondary); }
  }
  .availability-label {
    @include font-label-caps;
    font-size: 0.625rem;
    color: rgba(228, 194, 133, 0.6);
    margin-left: 1rem;
  }
}

.room-name {
  @include font-headline-md;
  font-size: clamp(1.8rem, 4vw, 2.5rem);
  color: var(--color-on-surface);
  margin-bottom: 1rem;
  line-height: 1.2;
}

.room-description {
  @include font-body-md;
  color: rgba(228, 226, 221, 0.6);
  max-width: 400px;
}

.info-bottom {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  border-top: 1px solid rgba(228, 194, 133, 0.1);
  padding-top: 2rem;
  margin-top: 2rem;
}

.price-label {
  display: block;
  @include font-label-caps;
  font-size: 0.625rem;
  color: rgba(228, 194, 133, 0.4);
  margin-bottom: 0.5rem;
}
.price-value {
  @include font-headline-sm;
  font-size: 1.75rem;
  color: var(--color-secondary);
  font-weight: 300;
}

.book-btn {
  background: var(--color-secondary);
  color: var(--color-background);
  border: none;
  @include font-label-caps;
  padding: 0.75rem 2rem;
  cursor: pointer;
  transition: background 0.3s;
  &:hover { background: rgba(228, 194, 133, 0.9); }
}

.slide-in-btn {
  transform: translateX(40px);
  opacity: 0;
  transition: all 0.6s cubic-bezier(0.23, 1, 0.32, 1);
}
.group:hover .slide-in-btn {
  transform: translateX(0);
  opacity: 1;
}

// Responsive – book button always visible on mobile
@media (max-width: 768px) {
  .slide-in-btn {
    transform: none;
    opacity: 1;
  }
}


# /Frontend/src/app/features/public/pages/availability.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { AuthService } from '../../../core/services/auth.service';
import { AvailableRoomType } from '../../user/models/available-room-type.model';

@Component({
  selector: 'app-availability',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule, MatDatepickerModule,
    MatNativeDateModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './availability.component.html',
  styleUrls: ['./availability.component.scss']
})
export class AvailabilityComponent implements OnInit {
  private roomTypeApi = inject(RoomTypeApiService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  private snackBar = inject(MatSnackBar);

  minDate = new Date();

  checkIn = new FormControl<Date | null>(null, Validators.required);
  checkOut = new FormControl<Date | null>(null, Validators.required);
  guests = new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]);

  availableRooms = signal<AvailableRoomType[]>([]);
  searchLoading = signal(false);
  searchError = signal<string | null>(null);
  hasSearched = signal(false);
  preSelectedRoomTypeId = signal<number | null>(null);

  ngOnInit(): void {
    // Automatically reset check-out if check-in changes to a date at or after check-out
    this.checkIn.valueChanges.subscribe(val => {
      if (val && this.checkOut.value && this.checkOut.value <= val) {
        this.checkOut.setValue(null);
      }
    });

    // Pre‑fill form from query params
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['checkIn']) this.checkIn.setValue(new Date(params['checkIn']), { emitEvent: false });
      if (params['checkOut']) this.checkOut.setValue(new Date(params['checkOut']), { emitEvent: false });
      if (params['guests']) this.guests.setValue(+params['guests'], { emitEvent: false });
      if (params['roomTypeId']) {
        this.preSelectedRoomTypeId.set(+params['roomTypeId']);
      }

      // Also check session storage for pre‑selected room type ID (from detail page)
      const storedRoomId = sessionStorage.getItem('selectedRoomTypeId');
      if (storedRoomId && !this.preSelectedRoomTypeId()) {
        this.preSelectedRoomTypeId.set(Number(storedRoomId));
      }

      // Pre‑fill from availability search session storage (from home page)
      const storedSearch = sessionStorage.getItem('availabilitySearch');
      if (storedSearch) {
        try {
          const data = JSON.parse(storedSearch);
          if (data.checkIn && !this.checkIn.value) this.checkIn.setValue(new Date(data.checkIn));
          if (data.checkOut && !this.checkOut.value) this.checkOut.setValue(new Date(data.checkOut));
          if (data.guests && this.guests.value === 1) this.guests.setValue(data.guests);
        } catch { /* ignore */ }
      }

      // Auto-trigger search if both check-in and check-out dates are successfully set
      if (this.checkIn.value && this.checkOut.value) {
        this.searchAvailability();
      }
    });
  }

  searchAvailability(): void {
    if (this.checkIn.invalid || this.checkOut.invalid || this.guests.invalid) return;
    this.searchLoading.set(true);
    this.searchError.set(null);

    const formatDate = (date: Date): string => {
      const d = String(date.getDate()).padStart(2, '0');
      const m = String(date.getMonth() + 1).padStart(2, '0');
      const y = date.getFullYear();
      return `${d}-${m}-${y}`;
    };

    const params = {
      checkIn: formatDate(this.checkIn.value!),
      checkOut: formatDate(this.checkOut.value!),
      pageSize: 50,
    };
    this.roomTypeApi.getAvailability(params).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.searchLoading.set(false))
    ).subscribe({
      next: res => {
        this.availableRooms.set(res.data);
        this.hasSearched.set(true);
      },
      error: (err: any) => this.searchError.set(this.extractErrorMessage(err))
    });
  }

  getFirstImage(room: AvailableRoomType): string {
    return room.imageUrls && room.imageUrls.length > 0 ? room.imageUrls[0] : 'assets/placeholder-room.jpg';
  }

  getMinCheckOutDate(): Date {
    if (this.checkIn.value) {
      const checkInDate = new Date(this.checkIn.value);
      checkInDate.setDate(checkInDate.getDate() + 1);
      return checkInDate;
    }
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow;
  }

  bookNow(room: AvailableRoomType): void {
    const checkIn = this.checkIn.value!.toISOString();
    const checkOut = this.checkOut.value!.toISOString();
    const guestCount = this.guests.value!;
    const roomTypeId = room.roomTypeId;

    if (this.authService.isAuthenticated()) {
      // Navigate directly to user booking wizard with pre‑filled params
      this.router.navigate(['/user/bookings'], {
        queryParams: {
          new: true,
          roomTypeId,
          checkIn,
          checkOut,
          guests: guestCount
        }
      });
    } else {
      // Store pending booking and redirect to login
      sessionStorage.setItem('pendingBooking', JSON.stringify({
        roomTypeId,
        checkIn,
        checkOut,
        guests: guestCount
      }));
      this.router.navigate(['/auth'], { queryParams: { returnUrl: '/user/dashboard' } });
    }
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/public/pages/experiences.component.html

<div class="experiences-page">
  <!-- Hero Section -->
  <section class="hero">
    <h1>Culinary Art &amp;<br><em>Absolute Stillness.</em></h1>
    <p>Aetheris dining is an exercise in restraint. From the private cellar to the starlit conservatory, every ingredient is sourced from the estate's own heritage soil or the deep Atlantic shelf.</p>
  </section>

  <!-- Menu Section (Accordion) -->
  <section class="menu-section">
    <div class="section-label">EPICUREAN SELECTIONS</div>

    @if (menuLoading()) {
      <div class="spinner-container">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (menuError()) {
      <p class="error">{{ menuError() }}</p>
    } @else {
      @for (group of menuGroups(); track group.category; let i = $index) {
        <div class="menu-row" [class.active]="expandedCategory() === group.category">
          <div class="menu-row-header" (click)="toggleCategory(group.category)">
            <h2>{{ group.category }}</h2>
            <span class="category-number">[ {{ (i + 1).toString().padStart(2, '0') }} ]</span>
            <span class="expand-icon material-symbols-outlined">{{ expandedCategory() === group.category ? 'expand_less' : 'expand_more' }}</span>
          </div>
          <div class="menu-content">
            @for (item of group.items; track item.id) {
              <div class="menu-item">
                <span class="item-name">{{ item.name }}</span>
                <span class="dotted-line"></span>
              </div>
            }
          </div>
        </div>
      }
    }
  </section>

  <!-- Amenities Section (Bento Grid with Pagination) -->
  <section class="amenities-section">
    <div class="section-label">PRIVATE AMENITIES</div>

    @if (amenitiesLoading()) {
      <div class="spinner-container">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (amenitiesError()) {
      <p class="error">{{ amenitiesError() }}</p>
    } @else {
      <div
        class="amenities-grid-container"
        [class.transitioning]="amenityIsTransitioning()"
        (touchstart)="onTouchStart($event)"
        (touchend)="onTouchEnd($event)"
      >
        <!-- Bento Grid -->
        <div class="bento-grid">
          @for (amenity of displayAmenities(); track amenity.id; let i = $index) {
            <div class="amenity-card" [ngClass]="'card-' + i">
              <div class="amenity-image" [style.background-image]="'url(' + getAmenityImage(amenity) + ')'">
                <div class="image-overlay"></div>
                <div class="card-number">{{ getAmenityNumber(i) }}</div>
                <!-- Hover overlay -->
                <div class="hover-overlay">
                  <h3>{{ amenity.name }}</h3>
                  <p>{{ amenity.description || 'Indulge in quiet luxury.' }}</p>
                </div>
              </div>
            </div>
          }
        </div>

        <!-- Pagination Indicator Container (Visible on Mobile) -->
        <div class="pagination-container">
          <button class="small-nav-arrow left" (click)="prevAmenityPage()" [disabled]="amenityPageIndex() === 0">
            <span class="material-symbols-outlined">chevron_left</span>
          </button>
          <div class="group-indicator">
            {{ amenityPageIndex() + 1 }} / {{ totalAmenityPages() }}
          </div>
          <button class="small-nav-arrow right" (click)="nextAmenityPage()" [disabled]="amenityPageIndex() === totalAmenityPages() - 1">
            <span class="material-symbols-outlined">chevron_right</span>
          </button>
        </div>
      </div>
    }
  </section>

  <!-- Archive / Philosophy Section -->
  <section class="archive-section">
    <div class="archive-image" style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuBcsrNli-Okq5zfPIxKgYeYhE_LK_uzemUGThdLc6zjw4pLEyJoDD5vOPjJLUF5LaK3qhIWcf59hlxQyDJ24Si6HYUpBvsfVOZYN4eNrJl-PGygA4awKDqaKCKzq_HnljgeiOdsWoUY6qrDR76iNBnoV_QoatCSBws27OYJZFTkUdJqLQYpS4-QXL_SkDrTNybanRN0yPZRPcboei3Wa-m5mhIEpHV6Kwi6Y-Zfqdqa5wuVDCkoYZCJtgew-BJlAUhr7x85SimSrV2x')"></div>
    <div class="archive-content">
      <h2>The Philosophy of <br><em>Permanent Quality.</em></h2>
      <ul>
        <li>
          <span class="label">ORIGIN</span>
          <p>Every harvest is cataloged and preserved in our subterranean vault, accessible only to resident guests.</p>
        </li>
        <li>
          <span class="label">RITUAL</span>
          <p>Dining is a timed performance, lasting precisely four hours from sunset to stellar zenith.</p>
        </li>
        <li>
          <span class="label">SILENCE</span>
          <p>The Dining Room maintains a zero-decibel acoustic standard outside of orchestrated service.</p>
        </li>
      </ul>
    </div>
  </section>
</div>


# /Frontend/src/app/features/public/pages/experiences.component.scss

@import '../../../../styles/theme/index';

.experiences-page {
  overflow-x: hidden;
}

// ── Hero ────────────────────────────────────────
.hero {
  max-width: var(--container-max);
  margin: 0 auto 6rem;
  padding: 10rem var(--margin-desktop) 0;
  @media (max-width: 768px) {
    padding: 6rem var(--margin-mobile) 0;
    margin-bottom: 3rem;
  }
  h1 {
    @include font-display-lg;
    font-size: clamp(2.5rem, 8vw, 4.5rem);
    color: var(--color-on-surface);
    margin-bottom: 1rem;
    em { font-style: italic; color: var(--color-secondary); }
  }
  p {
    @include font-body-lg;
    color: rgba(228, 226, 221, 0.7);
    max-width: 600px;
  }
}

.section-label {
  @include font-label-caps;
  color: var(--color-secondary);
  margin-bottom: 2rem;
  letter-spacing: 0.2em;
}

// ── Menu Accordion ──────────────────────────────
.menu-section {
  max-width: var(--container-max);
  margin: 0 auto 6rem;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin-bottom: 3rem;
  }
}

.spinner-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 4rem 0;
}

.menu-row {
  border-bottom: 1px solid rgba(228, 194, 133, 0.2);
  transition: all 0.6s cubic-bezier(0.22, 1, 0.36, 1);
  &.active { padding-bottom: 2.5rem; }
}

.menu-row-header {
  display: flex;
  align-items: flex-end;
  padding: 2rem 0;
  cursor: pointer;
  transition: color 0.3s;
  &:hover h2 { color: var(--color-secondary); }
  h2 {
    @include font-display-lg;
    font-size: clamp(1.5rem, 5vw, 2.5rem);
    color: var(--color-on-surface);
    text-transform: uppercase;
    margin: 0;
    transition: color 0.5s;
  }
  .category-number {
    @include font-label-caps;
    color: rgba(228, 226, 221, 0.4);
    margin-left: auto;
    margin-right: 1rem;
  }
  .expand-icon {
    color: var(--color-on-surface-variant);
    transition: transform 0.3s;
  }
}

.menu-content {
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.8s ease;
  max-width: 800px;
}
.menu-row.active .menu-content {
  max-height: 2000px; // large enough
  margin-top: 1rem;
}

.menu-item {
  display: flex;
  align-items: flex-end;
  padding: 0.5rem 0;
  .item-name {
    @include font-headline-sm;
    color: var(--color-on-surface);
  }
  .dotted-line {
    flex-grow: 1;
    border-bottom: 1px dotted rgba(228, 194, 133, 0.4);
    margin: 0 1rem 0.3rem;
  }
}

// ── Amenities Bento Grid ─────────────────────────
.amenities-section {
  max-width: var(--container-max);
  margin: 0 auto 6rem;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin-bottom: 3rem;
  }
}

.amenities-grid-container {
  position: relative;
  &.transitioning .amenity-image::after {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(105deg, transparent 40%, rgba(228, 194, 133, 0.3), transparent 60%);
    animation: goldSweep 0.6s ease forwards;
    z-index: 5;
  }
}

.bento-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  grid-template-rows: repeat(2, 1fr);
  gap: 2rem;
  height: 800px;
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    grid-template-rows: auto;
    height: auto;
  }
}

.amenity-card {
  position: relative;
  overflow: hidden;
  cursor: pointer;
  &:hover .hover-overlay { transform: translateY(0); }
  &.card-0 {
    grid-column: span 7;
    grid-row: span 2;
    @media (max-width: 768px) { grid-column: span 1; grid-row: auto; height: 500px; }
  }
  &.card-1 {
    grid-column: span 5;
    grid-row: span 1;
    @media (max-width: 768px) { grid-column: span 1; grid-row: auto; height: 400px; }
  }
  &.card-2 {
    grid-column: span 5;
    grid-row: span 1;
    @media (max-width: 768px) { grid-column: span 1; grid-row: auto; height: 400px; }
  }
}

.amenity-image {
  position: absolute;
  inset: 0;
  background-size: cover;
  background-position: center;
  filter: grayscale(30%) brightness(0.7);
  transition: transform 1s, filter 0.7s;
  .amenity-card:hover & { transform: scale(1.05); filter: grayscale(0%) brightness(1); }
}

.image-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, var(--color-background) 0%, transparent 50%);
}

.card-number {
  position: absolute;
  top: 1.5rem;
  left: 1.5rem;
  @include font-label-caps;
  color: var(--color-secondary);
  transition: color 0.3s;
}

.hover-overlay {
  position: absolute;
  inset: 0;
  @include glass-panel;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  padding: 2rem;
  transform: translateY(100%);
  transition: transform 0.7s cubic-bezier(0.19, 1, 0.22, 1);
  h3 {
    @include font-headline-md;
    color: var(--color-secondary);
    margin-bottom: 0.5rem;
  }
  p {
    @include font-body-md;
    color: var(--color-on-surface);
  }
  @media (max-width: 768px) {
    top: 70%;
    transform: translateY(0);
    justify-content: center;
    padding: 1rem 1.5rem;
    h3 {
      @include font-headline-sm;
      font-size: 1.1rem;
      margin-bottom: 0.2rem;
    }
    p {
      @include font-body-md;
      font-size: 0.85rem;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
  }
}

// ── Pagination Container ─────────────────────────
.pagination-container {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  margin-top: 3rem;
}

.small-nav-arrow {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  background: rgba(26, 26, 26, 0.7);
  border: 1px solid rgba(228, 194, 133, 0.3);
  border-radius: 50%;
  color: var(--color-on-surface);
  cursor: pointer;
  transition: all 0.3s ease;
  .material-symbols-outlined {
    font-size: 18px;
  }
  &:hover:not(:disabled),
  &:active:not(:disabled) {
    border-color: var(--color-secondary);
    color: var(--color-secondary);
  }
  &:disabled {
    opacity: 0.15;
    cursor: not-allowed;
  }
}

.group-indicator {
  text-align: center;
  @include font-label-caps;
  color: var(--color-outline);
  letter-spacing: 0.3em;
}

@keyframes goldSweep {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

// ── Archive Section ──────────────────────────────
.archive-section {
  display: grid;
  grid-template-columns: 4fr 6fr;
  gap: 2rem;
  max-width: var(--container-max);
  margin: 0 auto var(--section-gap);
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    padding: 0 var(--margin-mobile);
    margin-bottom: 6rem;
  }
}
.archive-image {
  height: 600px;
  background-size: cover;
  background-position: center;
  filter: grayscale(1);
  @media (max-width: 768px) { height: 400px; }
}
.archive-content {
  display: flex;
  flex-direction: column;
  justify-content: center;
  h2 {
    @include font-headline-md;
    font-size: clamp(1.5rem, 4vw, 2rem);
    color: var(--color-on-surface);
    margin-bottom: 2rem;
    em { color: var(--color-secondary); }
  }
  ul {
    list-style: none;
    li {
      display: flex;
      gap: 1rem;
      padding: 1.5rem 0;
      border-bottom: 1px solid rgba(228, 226, 221, 0.1);
      .label {
        @include font-label-caps;
        color: var(--color-secondary);
        min-width: 80px;
      }
      p { @include font-body-md; color: rgba(228, 226, 221, 0.7); }
    }
  }
}

// Error
.error {
  @include font-body-lg;
  color: var(--color-error);
}


# /Frontend/src/app/features/public/pages/experiences.component.ts

import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { forkJoin, finalize, map } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { MenuItemApiService } from '../../admin/services/menu-item-api.service';
import { AmenityApiService } from '../../admin/services/amenity-api.service';
import { MenuItem } from '../../admin/models/menu-item.model';
import { Amenity } from '../../admin/models/amenity.model';

@Component({
  selector: 'app-experiences',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatProgressSpinnerModule],
  templateUrl: './experiences.component.html',
  styleUrls: ['./experiences.component.scss']
})
export class ExperiencesComponent implements OnInit {
  private menuItemApi = inject(MenuItemApiService);
  private amenityApi = inject(AmenityApiService);
  private destroyRef = inject(DestroyRef);
  private breakpointObserver = inject(BreakpointObserver);

  // Menu
  menuLoading = signal(false);
  menuError = signal<string | null>(null);
  menuGroups = signal<{ category: string; items: MenuItem[] }[]>([]);
  expandedCategory = signal<string | null>(null);

  // Amenities
  amenitiesLoading = signal(false);
  amenitiesError = signal<string | null>(null);
  allAmenities = signal<Amenity[]>([]);
  amenityPageIndex = signal(0);

  // Breakpoint observation & pagination
  isMobile = toSignal(this.breakpointObserver.observe('(max-width: 768px)').pipe(map(r => r.matches)), { initialValue: false });
  itemsPerPageComputed = computed(() => this.isMobile() ? 1 : 3);
  totalAmenityPages = computed(() => Math.ceil(this.allAmenities().length / this.itemsPerPageComputed()));
  displayAmenities = computed(() => {
    const total = this.totalAmenityPages();
    const pageIndex = Math.min(this.amenityPageIndex(), Math.max(0, total - 1));
    const start = pageIndex * this.itemsPerPageComputed();
    return this.allAmenities().slice(start, start + this.itemsPerPageComputed());
  });

  // Transition state
  amenityIsTransitioning = signal(false);
  private readonly ANIMATION_DURATION = 600;

  ngOnInit(): void {
    this.fetchData();
  }

  private fetchData(): void {
    this.menuLoading.set(true);
    this.amenitiesLoading.set(true);

    const menu$ = this.menuItemApi.getAll({
      isAvailable: true,
      pageNumber: 1,
      pageSize: 200,
      sortBy: 'name',
      sortDescending: false
    }).pipe(
      map(res => {
        const groups: Record<string, MenuItem[]> = {};
        for (const item of res.data) {
          const cat = item.category || 'Other';
          if (!groups[cat]) groups[cat] = [];
          groups[cat].push(item);
        }
        return Object.entries(groups).map(([category, items]) => ({ category, items }));
      })
    );
    const amenities$ = this.amenityApi.getAll({
      isAvailable: true,
      pageNumber: 1,
      pageSize: 100,
      sortBy: 'name',
      sortDescending: false
    }).pipe(
      map(res => res.data)
    );

    forkJoin([menu$, amenities$]).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.menuLoading.set(false);
        this.amenitiesLoading.set(false);
      })
    ).subscribe({
      next: ([groups, amenities]) => {
        this.menuGroups.set(groups);
        this.allAmenities.set(amenities);
      },
      error: (err: any) => {
        this.menuError.set(this.extractErrorMessage(err));
        this.amenitiesError.set(this.extractErrorMessage(err));
      }
    });
  }

  // Menu accordion
  toggleCategory(category: string): void {
    this.expandedCategory.set(this.expandedCategory() === category ? null : category);
  }

  // Amenity pagination
  nextAmenityPage(): void {
    if (this.amenityPageIndex() < this.totalAmenityPages() - 1 && !this.amenityIsTransitioning()) {
      this.triggerAmenityTransition(() => this.amenityPageIndex.update(i => i + 1));
    }
  }
  prevAmenityPage(): void {
    if (this.amenityPageIndex() > 0 && !this.amenityIsTransitioning()) {
      this.triggerAmenityTransition(() => this.amenityPageIndex.update(i => i - 1));
    }
  }
  private triggerAmenityTransition(updateFn: () => void): void {
    this.amenityIsTransitioning.set(true);
    setTimeout(() => {
      updateFn();
      setTimeout(() => this.amenityIsTransitioning.set(false), this.ANIMATION_DURATION);
    }, 100);
  }

  // Touch swipe detection
  private touchStartX = 0;
  onTouchStart(event: TouchEvent): void {
    this.touchStartX = event.changedTouches[0].screenX;
  }
  onTouchEnd(event: TouchEvent): void {
    const deltaX = event.changedTouches[0].screenX - this.touchStartX;
    if (deltaX < -50) this.nextAmenityPage();
    else if (deltaX > 50) this.prevAmenityPage();
  }

  // Amenity image fallback (TODO: real images from backend later)
  getAmenityImage(amenity: Amenity): string {
    const designImages = [
      'https://lh3.googleusercontent.com/aida-public/AB6AXuAdW5i14tYjpRFDsySVWECF6hlJhhTBDM_2iyrGdU2-XAB3bXyzD3yVLXHWZyo2e2LZ3uX1G1jSLZwlItX3dGYPS913zkA-FfA1LByafCBqsTY6IvyqHbvD3bqkQVrbrp1bpHP8PgE5jpFQ_Z64hfgMg0oqcs7DYWc51yLI8NhbHew3ODOZnYr6tNUqKlwV7UL9hGKdUxzpi8nDVixmT_rpoGbYFScKbbT1JJVZHHqX7kQI5bi2Ez1s8oRBXtMW4VIRwPPAcUwTthmL',
      'https://lh3.googleusercontent.com/aida-public/AB6AXuCm9-2a_nAuBChLPNbo_8xZPTuxw_sFMl7WV7DjfWPM5PPHMj1QV-LZ9macLM4UelYuBaCxBWZwcG28rGYVooVP0oh1o7__5O7HWtlcGStiL5cX7gmdw_8I5oY0eyZA0iNYfCnefdLJnh0kXszMos0_kYvAUIOfaO4th3XshoyUFrcqhWJbaCGyjim0v_tfmL2IA-xYP0KOMCojfpJ5q4h28YTgUupgt7h4lj1NGlO2wTmhoHtWKnW2aHj9oq8pOic2OWFK4O8F7FZV',
      'https://lh3.googleusercontent.com/aida-public/AB6AXuAwLAmuLY0fR8D6Oh6SSzFHVLR_yq9yeaTuwjzoG_aEN9SGxscZpdZW7TlMXwfwROcjG47GVnu9MWhZd0yWinvSFVKPgxbp1N-7sJdU69q1Z5C8ref4bCIN2C38sZE1bGrPg9Qc4N56qylrsex2kE5wbmNtevNEZZQB_Qyt2pUFnILPsymu8OLj9PGfiBy5PJPY0GZfxapYekH-qSydKwwAPbNMxnyd9zMkvntWmPEvGgmgPEzBH0aCk-_wkqJeBk_KMcYVZ3IKk7MY'
    ];
    const idx = amenity.id % designImages.length;
    return designImages[idx];
  }

  // Amenity number label (global index)
  getAmenityNumber(index: number): string {
    const globalIndex = this.amenityPageIndex() * this.itemsPerPageComputed() + index + 1;
    return `[ ${globalIndex.toString().padStart(2, '0')} ]`;
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/public/pages/home.component.html

<div class="home-page">
  <!-- Hero Section -->
  <section class="hero">
    <div class="hero-bg"
      style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuAdLKWcaXoRzaSRLAXTT-aFheM_lrhFYW9u9Abt2sRFujWP3KddhG_Akj0P6IOduWEFFu-mDxD2Zp4dpIQBKJtdunZueQMAVvfq9JiEW3oI_NRiQcsqp14_Yh34YNUG5zo_pmyET_pQ4TIqdDKreqjZJa4v5OyKZXsRy8UZG8tcCCZrzpNVJ2GWRAukLp7Bu4xKZEVxiYwUN9kLUHOjj9c5NNzy82Sd_xoFjXKt_4nUnCutSLvPb56bP4-KBDObY9xJRnkzvLBu-yXX')">
    </div>
    <div class="hero-overlay"></div>
    <div class="hero-content">
      <h1>The Silent Peak of Luxury</h1>
      <p class="hero-subtitle">PRIVATE ESTATE &amp; REFUGE</p>
    </div>

    <!-- Booking Bar -->
    <div class="booking-bar glass-panel">
      <div class="booking-field">
        <label class="field-label">ARRIVAL</label>
        <input
          type="text"
          class="field-input"
          [formControl]="checkIn"
          placeholder="Select date"
          [matDatepicker]="cinPicker"
          (click)="cinPicker.open()"
          [min]="minDate"
          readonly
        />
        <mat-datepicker #cinPicker></mat-datepicker>
      </div>
      <div class="booking-field">
        <label class="field-label">DEPARTURE</label>
        <input
          type="text"
          class="field-input"
          [formControl]="checkOut"
          placeholder="Select date"
          [matDatepicker]="coutPicker"
          (click)="coutPicker.open()"
          [min]="getMinCheckOutDate()"
          readonly
        />
        <mat-datepicker #coutPicker></mat-datepicker>
      </div>
      <div class="booking-field">
        <label class="field-label">GUESTS</label>
        <input type="number" class="field-input" [formControl]="guests" min="1" max="20" />
      </div>
      <button class="booking-btn" (click)="searchAvailability()">RESERVE SANCTUARY</button>
    </div>
  </section>

  <!-- Ethos Section -->
  <section class="ethos">
    <div class="section-label">01. PHILOSOPHY</div>
    <h2 class="ethos-headline">The Ethos of <em>Aetheris</em></h2>
    <div class="ethos-grid">
      <div class="ethos-image">
        <img
          src="https://lh3.googleusercontent.com/aida-public/AB6AXuDJzoJgDT9zi_2aiEYAWO2EU0rvMI06PbHoowjw2aCipSYPYUWxOu7tAyZxY_9Jv_JIJtjSgIULOm3g5IKbvQvTiCwZkG3rqQFhbQFdQNRLguMXGIwr_xqUVtzi6P8YaSYbx1ZmgKQi94JqYqpZEKJeOMLA8P3r1ZqdRL9Rj1Sxlb5of5ik9gjJ8T4a8YllXDMXv8utaUSz-pPexBO49GhAk1ul4D5Q8oTQSOtw3RektEY-DrDA5Urt0WEVV-4qjr3Yp_-5qldmQPSO"
          alt="Alabaster sculpture" />
      </div>
      <div class="ethos-text">
        <p>At Aetheris, luxury is defined by what is absent. Noise, intrusion, and the mundane are replaced by a
          profound stillness. We provide a sanctuary where time is the ultimate currency and discretion is our highest
          law.</p>
        <a routerLink="/rooms" class="cta-link">
          The Vision <span class="line"></span>
        </a>
      </div>
    </div>
  </section>

  <!-- Private Sanctuaries Section -->
  <!-- <section class="sanctuaries">
    <div class="section-header">
      <div class="section-label">02. ACCOMMODATIONS</div>
      <div class="count-label">{{ featuredRooms().length }} ESTATES</div>
    </div>
    <h2 class="section-title">Private Sanctuaries</h2>

    @if (roomsLoading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (roomsError()) {
      <p class="error">{{ roomsError() }}</p>
    } @else {
      <div class="rooms-grid">
        @for (room of featuredRooms(); track room.id; let i = $index) {
          <div class="room-card" [class.large]="i === 0">
            <div class="card-image" [style.background-image]="'url(' + getFirstImage(room) + ')'" (click)="viewRoom(room.id)"></div>
            <div class="card-info">
              <h3>{{ room.name }}</h3>
              <div class="meta">
                <span>Max. {{ room.maxOccupancy }} Guests &bull; {{ room.squareFootage || '&mdash;' }} sqm</span>
                <span class="price">From {{ room.basePrice | currency }}/Night</span>
              </div>
            </div>
          </div>
        }
      </div>
      <a routerLink="/rooms" class="view-all">
        VIEW ALL ACCOMMODATIONS
        <span class="material-symbols-outlined">arrow_forward</span>
      </a>
    }
  </section> -->

  <!-- Heritage Section (Legacy of Discretion) -->
  <section class="heritage">
    <div class="heritage-header">
      <div class="section-label">Our Heritage</div>
      <h2>A Legacy of <br />Discretion</h2>
      <p class="heritage-subtitle">Est. 1924. Serving the world’s most discerning figures with unwavering privacy.</p>
    </div>
    <div class="heritage-grid">
      <!-- Item 1 -->
      <div class="heritage-item">
        <div class="heritage-img"
          style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuDPXJ1BklsW9n0PL5s9KUzjVQwn9bKCYfPqz7sKmIfH8846GMhTlUSG9oJzS4rslkF2ikJHqTEMliQVEXH1oS-KcX24_nYrfRFt-x7J0Dds0DoFT_YSy2m7Rw-nhIiMpP5887yaRrlmvkJ94dzPMsi1p6XH-8zGil4aqyzXigxyQJWh0uzWCRVzpBRpzWs0K7esx8SgEBJNwKq87CjDYvsecF1XtAwlWQSJpYp1iJunayndSL1JpC8blW2MTl3XmRYynMnLtwqEtZpF')">
        </div>
        <h3>Unseen Service</h3>
        <p>Our staff is trained in the art of invisibility, ensuring your needs are met before you even realize they
          exist.</p>
      </div>
      <!-- Item 2 -->
      <div class="heritage-item">
        <div class="heritage-img"
          style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuBIsM6GzTOdAofZq4-fCpVsVFJVgTsT1v1lVExmDQsRM6SDJXn3KyseX0n3GyBzvjuyS2QFkhF_S32eyt1kxm2tOyyIU6wKpK_yMSyG9EBWoRguoXAGLWdK0u7nlSemLoc69-vNEvgYVz2X27HMzDsLWjDWm8b5EG8GlrJ-ZYOsG5d7fC3U4FK09-FahO70xKPQBIiP11-H_MqSIPfdOdF_oqa8NvVV_E9n4oOuHpAJifApZ43dYdM-9KttRVtl6GkgV0bLelzlqKOh')">
        </div>
        <h3>Private Corridors</h3>
        <p>Architectural layouts designed specifically to prevent crossing paths, maintaining absolute isolation for all
          guests.</p>
      </div>
      <!-- Item 3 -->
      <div class="heritage-item">
        <div class="heritage-img"
          style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuDww5Dr5sbq3-Eu7dCIXq947SV0_TR1vtRdYxMIWuCNWrYmlJX3rAboz5drtoWRCZ5RvCfCfDcnx4LybMGp_0YnykwrU9ik65pYIbHVQBUtAiZT5zIyBisXkm8zqKDwn3nOnfRjijlOnF5H5Ubyv6iGH0LtQNcuquBkQUaDYX8t2UUaR7Pa_L1CJMQBOE2ErUXnpiLsD8rUQgZu4xi3p-2x1Zvih-4VLs8hmFMKCAuZX1WZyffDdmNdr-nJqF_g4ssaGTBzDwnfeasx')">
        </div>
        <h3>Trusted Custody</h3>
        <p>We safeguard not just your physical self, but your history, your privacy, and your future legacy.</p>
      </div>
    </div>
  </section>

  <!-- Cinematic Gallery Break -->
  <section class="gallery-break"
    style="background-image: url('https://lh3.googleusercontent.com/aida-public/AB6AXuC0jdUrR_d5kCrH_hA-VlFicyjfehGem9pHb4Z8526ztNY5GROodFNYf1W2cTR2sEdqm1B1-OoKOc4pLQ_W316--SNsc4uL-EVvj1aaY_DTZdmVko9XrpxWuo7dP3VRZCayR9NCVYvdxXVpfhUqIOPUBwORFton4M1685vgc5ZeNHVpCL1XmKeBQEyNkTSL1vRauQMjzrLNJndBAVRppOBKHKZ7HjYdc1OQ7_cu09zyzI5rVE4-e-rPPPpOW3QR2H0CCFpKtjOx0HRI')">
    <div class="gallery-overlay">
      <div class="section-label">CURATED CATALOG</div>
      <h2>The Celestial Pavilion</h2>
      <a routerLink="/rooms" class="gallery-cta">VIEW RESIDENCE</a>
    </div>
  </section>
</div>

# /Frontend/src/app/features/public/pages/home.component.scss

@import '../../../../styles/theme/index';

.home-page {
  overflow-x: hidden;
}

// Hero
.hero {
  position: relative;
  height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  .hero-bg {
    position: absolute;
    inset: 0;
    background-size: cover;
    background-position: center;
    transform: scale(1.05);
    animation: kenburns 20s infinite alternate;
  }
  .hero-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(to top, var(--color-background) 0%, transparent 50%, rgba(0,0,0,0.4) 100%);
  }
  .hero-content {
    position: relative;
    z-index: 10;
    text-align: center;
    padding: 0 var(--margin-mobile);
    h1 {
      @include font-display-lg;
      font-size: clamp(2.5rem, 10vw, 7.5rem);
      color: var(--color-on-surface);
      margin-bottom: 1rem;
    }
    .hero-subtitle {
      @include font-label-caps;
      color: var(--color-secondary);
      letter-spacing: 0.5em;
    }
  }
  .booking-bar {
    position: absolute;
    bottom: 3rem;
    left: 50%;
    transform: translateX(-50%);
    width: calc(100% - 2 * var(--margin-mobile));
    max-width: 1000px;
    @include glass-panel;
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    align-items: end;
    gap: 2rem;
    padding: 2rem;
    @media (max-width: 768px) {
      grid-template-columns: 1fr;
      gap: 1rem;
      width: calc(100% - 2 * var(--margin-mobile));
      bottom: 2rem;
    }
    .booking-field {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      .field-label {
        @include font-label-caps;
        color: var(--color-outline);
        font-size: 0.75rem;
      }
      .field-input {
        background: transparent;
        border: none;
        border-bottom: 1px solid rgba(228, 194, 133, 0.4);
        color: var(--color-on-surface);
        padding: 0.5rem 0;
        font-family: var(--font-body);
        font-size: 1rem;
        outline: none;
        &:focus { border-color: var(--color-secondary); }
      }
    }
    .booking-btn {
      @include font-label-caps;
      background: transparent;
      border: 1px solid var(--color-secondary);
      color: var(--color-secondary);
      padding: 0.75rem 1.5rem;
      cursor: pointer;
      transition: background 0.5s, color 0.5s;
      &:hover {
        background: var(--color-secondary);
        color: var(--color-on-secondary);
      }
    }
  }
}

// Ethos
.ethos {
  max-width: var(--container-max);
  margin: var(--section-gap) auto;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin: 4rem auto;
  }
  .section-label {
    @include font-label-caps;
    color: var(--color-secondary);
    margin-bottom: 1.5rem;
  }
  .ethos-headline {
    @include font-headline-md;
    font-size: clamp(2rem, 6vw, 4.5rem);
    max-width: 800px;
    margin-bottom: 3rem;
    em { font-style: italic; color: var(--color-secondary); }
  }
  .ethos-grid {
    display: grid;
    grid-template-columns: 7fr 5fr;
    gap: 2rem;
    @media (max-width: 768px) {
      grid-template-columns: 1fr;
    }
  }
  .ethos-image {
    overflow: hidden;
    img {
      width: 100%;
      height: 600px;
      object-fit: cover;
      transition: transform 1s;
      @media (max-width: 768px) { height: 400px; }
      &:hover { transform: scale(1.1); }
    }
  }
  .ethos-text {
    display: flex;
    flex-direction: column;
    justify-content: center;
    padding-left: 2rem;
    @media (max-width: 768px) { padding-left: 0; }
    p {
      @include font-body-lg;
      color: rgba(228, 226, 221, 0.8);
      margin-bottom: 2rem;
    }
    .cta-link {
      @include font-label-caps;
      color: var(--color-secondary);
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
      .line {
        width: 3rem;
        height: 1px;
        background: var(--color-secondary);
        transition: width 0.5s;
      }
      &:hover .line { width: 6rem; }
    }
  }
}

// Sanctuaries
.sanctuaries {
  max-width: var(--container-max);
  margin: var(--section-gap) auto;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin: 4rem auto;
  }
  .section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 1px solid rgba(228, 194, 133, 0.2);
    padding-bottom: 1rem;
    margin-bottom: 2rem;
    .section-label { @include font-label-caps; color: var(--color-secondary); }
    .count-label { @include font-label-caps; color: var(--color-outline); }
  }
  .section-title {
    @include font-display-lg-mobile;
    font-style: italic;
    margin-bottom: 3rem;
  }
  .rooms-grid {
    display: grid;
    grid-template-columns: 6fr 4fr;
    gap: 2rem;
    @media (max-width: 768px) {
      grid-template-columns: 1fr;
    }
    .room-card {
      &:first-child {
        grid-row: span 2;
      }
      .card-image {
        aspect-ratio: 4/5;
        background-size: cover;
        background-position: center;
        cursor: pointer;
        transition: transform 1.2s cubic-bezier(0.2, 0, 0.2, 1);
        &:hover { transform: scale(1.03); }
      }
      .card-info {
        margin-top: 1rem;
        h3 { @include font-headline-sm; }
        .meta {
          display: flex;
          justify-content: space-between;
          font-size: 0.9rem;
          color: var(--color-outline-variant);
          margin-top: 0.5rem;
          .price { color: var(--color-secondary); }
        }
      }
      &.large .card-image { aspect-ratio: 4/5; }
    }
  }
  .view-all {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    @include font-label-caps;
    color: var(--color-on-surface);
    text-decoration: none;
    margin-top: 3rem;
    padding-bottom: 4px;
    border-bottom: 1px solid rgba(228, 194, 133, 0.5);
    transition: color 0.3s, border-color 0.3s;
    &:hover { color: var(--color-secondary); border-color: var(--color-secondary); }
  }
}

// Heritage
.heritage {
  background: var(--color-surface-container-lowest);
  padding: var(--section-gap) var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 4rem var(--margin-mobile);
  }
  .heritage-header {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    border-bottom: 1px solid rgba(228, 194, 133, 0.1);
    padding-bottom: 2rem;
    margin-bottom: 3rem;
    @media (min-width: 768px) {
      flex-direction: row;
      justify-content: space-between;
      align-items: flex-end;
    }
    .section-label { @include font-label-caps; color: var(--color-secondary); margin-bottom: 1rem; }
    h2 { @include font-headline-md; font-size: clamp(2rem, 5vw, 3.5rem); }
    .heritage-subtitle {
      @include font-body-md;
      color: var(--color-on-tertiary-container);
      max-width: 300px;
      text-align: right;
    }
  }
  .heritage-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 2rem;
    @media (max-width: 768px) { grid-template-columns: 1fr; }
    .heritage-item {
      &:nth-child(2) { margin-top: 6rem; @media (max-width: 768px) { margin-top: 0; } }
      .heritage-img {
        height: 320px;
        background-size: cover;
        background-position: center;
        margin-bottom: 1.5rem;
        transition: transform 0.7s;
        &:hover { transform: scale(1.05); }
      }
      h3 { @include font-headline-sm; margin-bottom: 0.5rem; }
      p { @include font-body-md; color: rgba(228, 226, 221, 0.6); }
    }
  }
}

// Gallery Break
.gallery-break {
  height: 100vh;
  background-size: cover;
  background-position: center;
  background-attachment: fixed;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  position: relative;
  .gallery-overlay {
    background: rgba(0, 0, 0, 0.4);
    padding: 3rem;
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    .section-label { @include font-label-caps; color: var(--color-secondary); margin-bottom: 1rem; }
    h2 { @include font-display-lg; font-size: clamp(2.5rem, 7vw, 5rem); }
    .gallery-cta {
      @include font-label-caps;
      font-size: 0.625rem;
      letter-spacing: 0.4em;
      color: var(--color-on-surface);
      border-bottom: 1px solid var(--color-on-surface);
      padding-bottom: 0.5rem;
      text-decoration: none;
      margin-top: 2rem;
      transition: color 0.3s, border-color 0.3s;
      &:hover { color: var(--color-secondary); border-color: var(--color-secondary); }
    }
  }
}

// Ken Burns animation
@keyframes kenburns {
  from { transform: scale(1.05); }
  to { transform: scale(1.1); }
}


# /Frontend/src/app/features/public/pages/home.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatButtonModule, MatCardModule, MatIconModule, MatDatepickerModule,
    MatNativeDateModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  featuredRooms = signal<RoomType[]>([]);
  roomsLoading = signal(false);
  roomsError = signal<string | null>(null);

  minDate = new Date();

  checkIn = new FormControl<Date | null>(null, Validators.required);
  checkOut = new FormControl<Date | null>(null, Validators.required);
  guests = new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]);

  ngOnInit(): void {
    this.fetchFeaturedRooms();
    
    // Automatically reset check-out if check-in changes to a date at or after check-out
    this.checkIn.valueChanges.subscribe(val => {
      if (val && this.checkOut.value && this.checkOut.value <= val) {
        this.checkOut.setValue(null);
      }
    });
  }

  getMinCheckOutDate(): Date {
    if (this.checkIn.value) {
      const checkInDate = new Date(this.checkIn.value);
      checkInDate.setDate(checkInDate.getDate() + 1);
      return checkInDate;
    }
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow;
  }

  private fetchFeaturedRooms(): void {
    this.roomsLoading.set(true);
    this.roomTypeApi.getAll({ includeRetired: false, pageNumber: 1, pageSize: 6, sortBy: 'basePrice', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.roomsLoading.set(false))
    ).subscribe({
      next: res => this.featuredRooms.set(res.data),
      error: (err: any) => this.roomsError.set(this.extractErrorMessage(err))
    });
  }

  getFirstImage(room: RoomType): string {
    return room.imageUrls && room.imageUrls.length > 0 ? room.imageUrls[0] : 'assets/placeholder-room.jpg';
  }

  viewRoom(roomId: number): void {
    this.router.navigate(['/rooms', roomId]);
  }

  searchAvailability(): void {
    if (this.checkIn.value && this.checkOut.value) {
      const checkIn = this.checkIn.value.toISOString();
      const checkOut = this.checkOut.value.toISOString();
      const guestCount = this.guests.value || 1;
      // Store for later booking flow
      sessionStorage.setItem('availabilitySearch', JSON.stringify({ checkIn, checkOut, guests: guestCount }));
      this.router.navigate(['/availability'], { queryParams: { checkIn, checkOut, guests: guestCount } });
    } else {
      this.router.navigate(['/availability']);
    }
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/public/pages/menu.component.html

<div class="menu-page">
  <div class="hero-small">
    <h1>Our Restaurant</h1>
    <p>Indulge in culinary excellence crafted by our world‑class chefs.</p>
  </div>

  @if (loading()) {
    <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
    <p class="error">{{ error() }}</p>
    <button mat-button (click)="fetchMenu()">Retry</button>
  } @else {
    @for (group of groupedMenu(); track group.category) {
      <section class="category-section">
        <h2>{{ group.category }}</h2>
        <div class="menu-grid">
          @for (item of group.items; track item.id) {
            <mat-card class="menu-card">
              <div class="card-image">
                <mat-icon class="food-icon">restaurant</mat-icon>
              </div>
              <mat-card-header>
                <mat-card-title>{{ item.name }}</mat-card-title>
                <mat-card-subtitle>{{ item.price | currency }}</mat-card-subtitle>
              </mat-card-header>
            </mat-card>
          }
        </div>
      </section>
    }
  }
</div>


# /Frontend/src/app/features/public/pages/menu.component.scss

.menu-page {
  .hero-small {
    background: linear-gradient(rgba(0,0,0,0.5), rgba(0,0,0,0.5)), url('/assets/restaurant-hero.jpg') center/cover no-repeat;
    height: 35vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: white;
    text-align: center;
    h1 { font-size: 2.5rem; margin-bottom: 8px; }
    p { font-size: 1.2rem; max-width: 500px; }
  }
  .category-section {
    padding: 32px 16px;
    h2 {
      font-size: 1.8rem;
      margin-bottom: 16px;
      padding-bottom: 8px;
      border-bottom: 2px solid #1976d2;
      display: inline-block;
    }
    .menu-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 16px;
    }
  }
  .menu-card {
    display: flex;
    align-items: center;
    padding: 12px;
    .card-image {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      background: #f5f5f5;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 16px;
      .food-icon { font-size: 32px; width: 32px; height: 32px; color: #1976d2; }
    }
    mat-card-header { flex: 1; }
  }
}

// Responsive adjustments
@media (max-width: 768px) {
  .menu-page {
    .hero-small {
      height: 25vh;
    }
  }
}


# /Frontend/src/app/features/public/pages/menu.component.ts

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { MenuItemApiService } from '../../admin/services/menu-item-api.service';
import { MenuItem } from '../../admin/models/menu-item.model';

@Component({
  selector: 'app-public-menu',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent implements OnInit {
  private menuItemApi = inject(MenuItemApiService);
  private destroyRef = inject(DestroyRef);

  groupedMenu = signal<{ category: string; items: MenuItem[] }[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchMenu();
  }

  fetchMenu(): void {
    this.loading.set(true);
    this.menuItemApi.getAll({ isAvailable: true, pageNumber: 1, pageSize: 200, sortBy: 'name', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => {
        const groups: Record<string, MenuItem[]> = {};
        for (const item of res.data) {
          const cat = item.category || 'Other';
          if (!groups[cat]) groups[cat] = [];
          groups[cat].push(item);
        }
        this.groupedMenu.set(
          Object.entries(groups).map(([category, items]) => ({ category, items }))
        );
      },
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/public/pages/room-catalogue.component.html

<div class="catalogue-page">
  <!-- Hero Header -->
  <header class="hero-header">
    <span class="section-label">PRIVATE SANCTUARIES</span>
    <h1>The Villa Collection</h1>
    <p class="hero-description">
      A curated selection of architectural masterpieces nestled within the ancient slopes. Each villa is a private world, designed for silence, reflection, and the quiet pursuit of excellence.
    </p>
  </header>

  <!-- Room Cards – Fixed Grid with Pagination -->
  <section class="rooms-section">
    @if (loading()) {
      <div class="loading-state">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (error()) {
      <div class="error-state">
        <p>{{ error() }}</p>
        <button class="retry-btn" (click)="fetchRooms()">Retry</button>
      </div>
    } @else {
      <div
        class="grid-container"
        [class.transitioning]="isTransitioning()"
        (wheel)="onWheel($event)"
        (touchstart)="onTouchStart($event)"
        (touchend)="onTouchEnd($event)"
        tabindex="0"
      >
        <!-- Asymmetric Grid (12 columns) -->
        <div class="villa-grid">
          @for (room of displayedRooms(); track room.id; let i = $index) {
            <article class="room-card {{ getCardClass(i) }}">
              <!-- Image area -->
              <div class="card-image" [style.background-image]="'url(' + getFirstImage(room) + ')'">
                <div class="image-overlay"></div>
                <div class="card-number">
                  {{ ((currentGroupIndex() * roomsPerGroup) + i + 1).toString().padStart(2, '0') }} / VILLA
                </div>
              </div>
              <!-- Card body for all cards -->
              <div class="card-body">
                <h2 class="room-name">{{ room.name }}</h2>
                <div class="room-meta">
                  <span>Max. {{ room.maxOccupancy }} Guests</span>
                  <span class="separator">·</span>
                  <span>{{ room.squareFootage || '—' }} sqm</span>
                </div>
                <a [routerLink]="['/rooms', room.id]" class="view-link">
                  VIEW DETAILS <span class="arrow">→</span>
                </a>
              </div>
            </article>
          }
        </div>
      </div>

      <!-- Pagination controls with group indicator -->
      <div class="pagination-container">
        <button class="small-nav-arrow left" (click)="previousGroup()" [disabled]="currentGroupIndex() === 0">
          <span class="material-symbols-outlined">chevron_left</span>
        </button>
        <div class="group-indicator">
          {{ currentGroupIndex() + 1 }} / {{ totalGroups() }}
        </div>
        <button class="small-nav-arrow right" (click)="nextGroup()" [disabled]="currentGroupIndex() === totalGroups() - 1">
          <span class="material-symbols-outlined">chevron_right</span>
        </button>
      </div>
    }
  </section>

  <!-- Newsletter Section -->
  <section class="newsletter">
    <h2 class="newsletter-title">Stay Informed</h2>
    <div class="newsletter-form">
      <input
        type="email"
        class="newsletter-input"
        placeholder="YOUR EMAIL ADDRESS"
        [formControl]="emailControl"
        (keyup.enter)="subscribe()"
      />
      @if (!subscribed()) {
        <button class="subscribe-btn" (click)="subscribe()">Subscribe</button>
      } @else {
        <span class="success-message">Thank you for your interest.</span>
      }
    </div>
    <!-- TODO: wire up newsletter subscription to backend -->
  </section>
</div>


# /Frontend/src/app/features/public/pages/room-catalogue.component.scss

@import '../../../../styles/theme/index';

.catalogue-page {
  overflow-x: hidden;
  padding-top: 10rem; // space from fixed navbar (design's pt-40)
  @media (max-width: 768px) {
    padding-top: 6rem;
  }
}

// ── Hero Header ──────────────────────────────────
.hero-header {
  max-width: var(--container-max);
  margin: 0 auto 8rem; // design's mb-32
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin-bottom: 4rem;
  }
  @media (max-width: 375px) {
    padding: 0 1rem;
  }
  .section-label {
    @include font-label-caps;
    color: var(--color-secondary);
    margin-bottom: 0.5rem;
  }
  h1 {
    @include font-display-lg;
    font-size: clamp(2.5rem, 8vw, 5rem);
    margin-bottom: 1.5rem;
    color: var(--color-on-surface);
  }
  .hero-description {
    @include font-body-lg;
    color: rgba(228, 226, 221, 0.6);
    max-width: 600px;
  }
}

// ── Loading & Error States ───────────────────────
.loading-state,
.error-state {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 4rem 0;
}
.error-state {
  flex-direction: column;
  gap: 1rem;
  p { @include font-body-lg; color: var(--color-error); }
  .retry-btn {
    @include font-label-caps;
    background: transparent;
    border: 1px solid var(--color-secondary);
    color: var(--color-secondary);
    padding: 0.5rem 1.5rem;
    cursor: pointer;
    &:hover { background: var(--color-secondary); color: var(--color-on-secondary); }
  }
}

// ── Grid Section ─────────────────────────────────
.rooms-section {
  width: 100%;
  margin-bottom: var(--section-gap);
}

.grid-container {
  position: relative;
  padding: 0 var(--margin-desktop);
  max-width: var(--container-max);
  margin: 0 auto;
  outline: none; // for focus to receive wheel events
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
  }
  @media (max-width: 375px) {
    padding: 0 1rem;
  }
}

// Asymmetric Villa Grid (12‑column)
.villa-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: 2rem; // 32px as design
  @media (max-width: 340px) {
    gap: 1rem;
  }
}

// Base card
.room-card {
  cursor: pointer;
  transition: transform 0.4s ease;
  &:hover {
    transform: translateY(-4px);
    .card-image { transform: scale(1.03); }
  }
}

// Card sizes & offsets (exactly mirror design)
.room-card.card-large {
  grid-column: span 8;
  margin-bottom: 8rem; // design's mb-32
  .card-image { aspect-ratio: 16 / 9; }
  @media (max-width: 768px) {
    grid-column: span 12;
    margin-bottom: 2rem;
  }
}
.room-card.card-small {
  grid-column: span 4;
  align-self: center;    // vertically centered in the row
  margin-bottom: 8rem;
  .card-image { aspect-ratio: 3 / 4; }
  @media (max-width: 768px) {
    grid-column: span 12;
    margin-bottom: 2rem;
    align-self: auto;
  }
}
.room-card.card-medium {
  grid-column: span 5;
  margin-bottom: 8rem;
  .card-image { aspect-ratio: 4 / 5; }
  @media (max-width: 768px) {
    grid-column: span 12;
    margin-bottom: 2rem;
  }
}
.room-card.card-wide {
  grid-column: span 7;
  margin-top: 8rem;      // design's lg:mt-32
  margin-bottom: 8rem;
  .card-image { aspect-ratio: 16 / 10; }
  @media (max-width: 768px) {
    grid-column: span 12;
    margin-top: 0;
    margin-bottom: 2rem;
  }
}

// Image area
.card-image {
  background-size: cover;
  background-position: center;
  transition: transform 0.6s cubic-bezier(0.2, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
  background-color: var(--color-surface-container-low); // fallback
}

.image-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, var(--color-background) 0%, transparent 50%);
}

.card-number {
  position: absolute;
  bottom: 1rem;
  left: 1rem;
  @include font-label-caps;
  font-size: 0.625rem;
  letter-spacing: 0.3em;
  color: var(--color-secondary);
}

// Card body for all cards
.card-body {
  padding: 1.5rem 0 0;
  border-bottom: 1px solid rgba(228, 194, 133, 0.2);
}

.room-name {
  @include font-headline-sm;
  color: var(--color-on-surface);
  margin-bottom: 0.5rem;
}

.room-meta {
  @include font-body-md;
  color: var(--color-outline-variant);
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
  .separator { color: var(--color-outline-variant); }
}

.view-link {
  @include font-label-caps;
  color: var(--color-on-surface);
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  .arrow {
    transition: transform 0.3s;
  }
  &:hover .arrow {
    transform: translateX(4px);
    color: var(--color-secondary);
  }
}

// ── Gold Wash Animation ──────────────────────────
.grid-container.transitioning .card-image::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(
    105deg,
    transparent 40%,
    rgba(228, 194, 133, 0.3),
    transparent 60%
  );
  animation: goldSweep 0.6s ease forwards;
}

@keyframes goldSweep {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

// ── Pagination Container ─────────────────────────
.pagination-container {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  margin-top: 2rem;
}

// Small arrows (centered pagination controls)
.small-nav-arrow {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  background: rgba(26, 26, 26, 0.7);
  border: 1px solid rgba(228, 194, 133, 0.3);
  border-radius: 50%;
  color: var(--color-on-surface);
  cursor: pointer;
  transition: all 0.3s ease;
  .material-symbols-outlined {
    font-size: 18px;
  }
  &:hover:not(:disabled),
  &:active:not(:disabled) {
    border-color: var(--color-secondary);
    color: var(--color-secondary);
  }
  &:disabled {
    opacity: 0.15;
    cursor: not-allowed;
  }
}

// Group indicator
.group-indicator {
  text-align: center;
  @include font-label-caps;
  color: var(--color-outline);
  letter-spacing: 0.3em;
}

// ── Newsletter Section ────────────────────────────
.newsletter {
  background: var(--color-surface-container-lowest);
  border-top: 1px solid var(--color-surface-container-highest);
  padding: var(--section-gap) var(--margin-desktop);
  text-align: center;
  @media (max-width: 768px) {
    padding: 4rem var(--margin-mobile);
  }
  @media (max-width: 375px) {
    padding: 4rem 1rem;
  }
}
.newsletter-title {
  @include font-display-lg-mobile;
  font-size: clamp(2rem, 5vw, 3rem);
  text-transform: uppercase;
  letter-spacing: 0.5em;
  color: var(--color-on-surface);
  margin-bottom: 3rem;
}
.newsletter-form {
  max-width: 600px;
  margin: 0 auto;
  border-bottom: 1px solid rgba(228, 226, 221, 0.2);
  display: flex;
  align-items: center;
  padding-bottom: 0.5rem;
  @media (max-width: 480px) {
    flex-direction: column;
    gap: 1rem;
    border-bottom: none;
    .newsletter-input {
      width: 100%;
      border-bottom: 1px solid rgba(228, 226, 221, 0.2);
      text-align: center;
    }
    .subscribe-btn, .success-message {
      width: 100%;
      text-align: center;
    }
  }
}
.newsletter-input {
  flex: 1;
  background: transparent;
  border: none;
  color: var(--color-on-surface);
  @include font-label-caps;
  letter-spacing: 0.2em;
  padding: 0.5rem 0;
  outline: none;
  &::placeholder { color: rgba(228, 226, 221, 0.2); }
}
.subscribe-btn {
  @include font-label-caps;
  background: transparent;
  border: none;
  color: var(--color-secondary);
  cursor: pointer;
  transition: letter-spacing 0.5s, opacity 0.3s;
  &:hover { letter-spacing: 0.3em; }
}
.success-message {
  @include font-label-caps;
  color: var(--color-secondary);
  opacity: 0;
  animation: fadeInSuccess 0.6s ease forwards;
}
@keyframes fadeInSuccess {
  from { opacity: 0; transform: translateY(4px); }
  to { opacity: 1; transform: translateY(0); }
}


# /Frontend/src/app/features/public/pages/room-catalogue.component.ts

import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-room-catalogue',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './room-catalogue.component.html',
  styleUrls: ['./room-catalogue.component.scss']
})
export class RoomCatalogueComponent implements OnInit {
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  rooms = signal<RoomType[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  emailControl = new FormControl('', { nonNullable: true });
  subscribed = signal(false);

  // Pagination
  currentGroupIndex = signal(0);
  roomsPerGroup = 4;
  totalGroups = computed(() => Math.ceil(this.rooms().length / this.roomsPerGroup));

  // Rooms to display in the current grid
  displayedRooms = computed(() => {
    const start = this.currentGroupIndex() * this.roomsPerGroup;
    return this.rooms().slice(start, start + this.roomsPerGroup);
  });

  // Transition state for gold wash animation
  isTransitioning = signal(false);
  private readonly ANIMATION_DURATION = 600; // ms

  ngOnInit(): void {
    this.fetchRooms();
    window.scrollTo({ top: 0 });
  }

  fetchRooms(): void {
    this.loading.set(true);
    this.roomTypeApi.getAll({ includeRetired: false, pageNumber: 1, pageSize: 100, sortBy: 'name', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => this.rooms.set(res.data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  getFirstImage(room: RoomType): string {
    return room.imageUrls && room.imageUrls.length > 0 ? room.imageUrls[0] : 'assets/placeholder-room.jpg';
  }

  viewRoom(roomId: number): void {
    this.router.navigate(['/rooms', roomId]);
  }

  subscribe(): void {
    if (!this.emailControl.value || this.subscribed()) return;
    this.emailControl.setValue('');
    this.subscribed.set(true);
    // TODO: wire up newsletter subscription to backend
  }

  // Navigation methods
  nextGroup(): void {
    if (this.currentGroupIndex() < this.totalGroups() - 1 && !this.isTransitioning()) {
      this.triggerTransition(() => this.currentGroupIndex.update(i => i + 1));
    }
  }

  previousGroup(): void {
    if (this.currentGroupIndex() > 0 && !this.isTransitioning()) {
      this.triggerTransition(() => this.currentGroupIndex.update(i => i - 1));
    }
  }

  private triggerTransition(updateFn: () => void): void {
    this.isTransitioning.set(true);
    setTimeout(() => {
      updateFn();
      
      // Scroll the room section into view, accounting for the fixed navbar
      const element = document.querySelector('.rooms-section');
      if (element) {
        const yOffset = -120; // approximate navbar height offset
        const y = element.getBoundingClientRect().top + window.scrollY + yOffset;
        window.scrollTo({ top: y, behavior: 'smooth' });
      }

      setTimeout(() => this.isTransitioning.set(false), this.ANIMATION_DURATION);
    }, 100);
  }

  // Touch & wheel detection
  private touchStartX = 0;
  onTouchStart(event: TouchEvent): void {
    this.touchStartX = event.changedTouches[0].screenX;
  }
  onTouchEnd(event: TouchEvent): void {
    const deltaX = event.changedTouches[0].screenX - this.touchStartX;
    if (deltaX < -50) this.nextGroup();
    else if (deltaX > 50) this.previousGroup();
  }
  onWheel(event: WheelEvent): void {
    if (Math.abs(event.deltaX) > Math.abs(event.deltaY) && Math.abs(event.deltaX) > 30) {
      event.preventDefault();
      if (event.deltaX > 0) this.nextGroup();
      else this.previousGroup();
    }
  }

  // Get CSS classes for each card position (0-3)
  getCardClass(index: number): string {
    const classes = ['card-large', 'card-small', 'card-medium', 'card-wide'];
    return classes[index] || '';
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/public/pages/room-detail.component.html

<div class="detail-page">
  @if (loading()) {
    <div class="loading-spinner">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <div class="error-state">
      <p>{{ error() }}</p>
      <a routerLink="/rooms" class="back-link">← Back to Villas</a>
    </div>
  } @else if (room()) {
    <!-- Image Gallery Section -->
    <section class="gallery-section">
      <a routerLink="/rooms" class="back-to-villas-btn">
        <span class="material-symbols-outlined">arrow_back</span>
        <span>BACK TO VILLAS</span>
      </a>

      <div class="gallery-scroll" #galleryContainer>
        <!-- Design images (static, shown first) -->
        <div class="gallery-item">
          <img
            class="gallery-image"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuD4LaZWIfgwL2Qyr-FRyKY7s3ZeBGdcJ16QgAMFvM14nKYRywPkhIaU2IWn24HPjp6FabFMVsRVkSlYZgbPwza5q8S8FKjd3LW1NJ1WvliItKWNfFcjLufO14MorWS5QFvR-huAFxK8aQWwI6XLxUojdGW1ka_1KnjpW2IlR3xTfYX00SJljqN2J0yEzYcHqY9oZgtdpPSwUT_FBdy8eskDso2wqiH8Pdncrvu4MQEDm1mQgpqxdNm5n2X6kLCDzsWeaHvrR4Si-wt7"
            alt="{{ room()!.name }}"
          />
          <div class="image-overlay-gallery"></div>
        </div>
        <div class="gallery-item parallax-img" data-speed="0.15">
          <img
            class="gallery-image"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuB7BpuMqO3vUONIUaw5w7liiaHIjRBVk5uM3wMsr1SJSlC-y30i-GvGNqosr8gunZDJjTQX3N2e44Pm_25lOPjb_jxPxaeBcin63o8VaFITR-OCYBj1N5eusmPOzg50z8cObybGuAeKATQD9nOTX0owuSacbbcWycidCo5IcC4CCvlps17fvnqa-oHeFMgBX8940Rqk9iBqib83dQzq8MOzfJG1qYi4xQGYVV1Ky2lZLPgu14peo7JlDKH6A414AbvPV-16NSN1vwnZ"
            alt="{{ room()!.name }}"
          />
          <div class="image-overlay-gallery"></div>
        </div>
        <div class="gallery-item">
          <img
            class="gallery-image"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuCvXqkGL5KS6CGCmVgFcRdnC4k2mB07aJG91FFP664rBxYXjtsLZpD_qXdyGMwn4yX8HqMWXb8nc7wqjRSy0ssglhwB6mnoVscAlxXLPGU54uwGSJbQ6o_DCe4PVPwBgHqsQwu-CpTEGYt_vpt9BOCet9ptZWuogIxNj7LYWPtLK_qf6bUuMRv8FCnewP5TDk-y1qxAmfCV8BWM6WxqK31BHExQYqN5NZdVeRQOAVsStLFtyZLClvyfkilL2FeN2HAn7LSt0RMuEmHi"
            alt="{{ room()!.name }}"
          />
          <div class="image-overlay-gallery"></div>
        </div>
        <div class="gallery-item parallax-img" data-speed="-0.1">
          <img
            class="gallery-image"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuBT07wf-yfThimt-RyGEG4UoP6X4fPA3lsiVxRhhLS_DQHFrxspAnNt2a_RSA16Xaws4_P45R-mVRavRKdMpqEI5IXLcjY20Vlr4b3qgWCZShh7ktDXh_DzZeY6ahQgCe0OumlVj1NMU-Bn_Io4q2ZAI0ne_eFlfYMraQ3tOKlY7Z7n331cQhKm2auuwpoKdKfH1ib45hR6eVHexYS7I4CO7cH189s3Rm-eaGHmVDIOo_uj_DL04rESW_57Md2Y8LslN1VOd1mo4seb"
            alt="{{ room()!.name }}"
          />
          <div class="image-overlay-gallery"></div>
        </div>
        <!-- API images (dynamic) -->
        @for (imgUrl of room()!.imageUrls; track imgUrl) {
          <div class="gallery-item">
            <img class="gallery-image" [src]="imgUrl" alt="{{ room()!.name }}" />
            <div class="image-overlay-gallery"></div>
          </div>
        }
      </div>

      <!-- Glass Overlay Info Panel -->
      <div class="glass-info-panel">
        <h1 class="room-title">{{ room()!.name }}</h1>
        <p class="room-description">{{ room()!.description || 'No description available.' }}</p>
        <div class="divider-line"></div>
      </div>
    </section>

    <!-- Metrics & Configuration Section -->
    <section class="details-section">
      <div class="details-grid">
        <!-- Metrics -->
        <div class="metrics-column">
          <div class="metric-item">
            <span class="metric-value">{{ room()!.basePrice | currency }}</span>
            <div class="metric-divider"></div>
            <span class="metric-label">Per Night</span>
          </div>
          <div class="metric-item">
            <span class="metric-value">{{ room()!.squareFootage || '—' }}</span>
            <div class="metric-divider"></div>
            <span class="metric-label">Sq. Ft.</span>
          </div>
          <div class="metric-item">
            <span class="metric-value">{{ room()!.maxOccupancy }}</span>
            <div class="metric-divider"></div>
            <span class="metric-label">Max Guests</span>
          </div>
        </div>
        <!-- Bed Configuration -->
        <div class="config-column">
          <h3 class="config-title">Configuration</h3>
          <ul class="config-list">
            @for (entry of getBedEntries(); track entry[0]) {
              <li class="config-item">
                <span class="config-icon material-symbols-outlined">{{ getBedIcon(entry[0]) }}</span>
                <span class="config-text">{{ entry[1] }} {{ entry[0] }} Bed{{ entry[1] > 1 ? 's' : '' }}</span>
              </li>
            }
            @empty {
              <li class="config-item">
                <span class="config-icon material-symbols-outlined">bed</span>
                <span class="config-text">Ask for details</span>
              </li>
            }
          </ul>
        </div>
      </div>

      <!-- Full‑width CTA -->
      <div class="cta-section">
        <button class="cta-button" (click)="checkAvailability()">
          <span class="cta-text">Check Availability</span>
          <div class="cta-hover-fill"></div>
        </button>
      </div>
    </section>
  }
</div>


# /Frontend/src/app/features/public/pages/room-detail.component.scss

@import '../../../../styles/theme/index';

.detail-page {
  overflow-x: hidden;
}

// ── Loading / Error ──────────────────────────────
.loading-spinner, .error-state {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 60vh;
}
.error-state {
  flex-direction: column;
  gap: 1rem;
  p { @include font-body-lg; color: var(--color-error); }
  .back-link { @include font-body-md; color: var(--color-secondary); text-decoration: none; }
}

// ── Gallery Section ──────────────────────────────
.gallery-section {
  position: relative;
  height: 100vh;
  min-height: 700px;
  overflow: hidden;
  @media (max-width: 768px) {
    height: 75vh;
    min-height: 500px;
  }
}

.gallery-scroll {
  display: flex;
  overflow-x: auto;
  overflow-y: hidden;
  scroll-snap-type: x mandatory;
  -webkit-overflow-scrolling: touch;
  height: 100%;
  cursor: grab;
  &:active { cursor: grabbing; }
  &::-webkit-scrollbar { display: none; }
}

.gallery-item {
  flex: 0 0 85vw;
  scroll-snap-align: center;
  position: relative;
  margin: 0 0.5rem;
  @media (min-width: 768px) {
    flex: 0 0 70vw;
    margin: 0 1rem;
  }
}

.gallery-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: grayscale(30%) brightness(0.75);
  transition: filter 0.7s, transform 0.1s linear; // transform will be set by parallax JS
  &:hover { filter: brightness(1); }
}

// Parallax initial scale
.parallax-img .gallery-image {
  transform: scale(1.1);
}

.image-overlay-gallery {
  position: absolute;
  inset: 0;
  background: rgba(19, 20, 17, 0.2); // subtle dark overlay
  pointer-events: none;
}

// Glass info panel (overlapping bottom‑left)
.glass-info-panel {
  position: absolute;
  bottom: 3rem;
  left: var(--margin-mobile);
  width: calc(100% - 2 * var(--margin-mobile));
  max-width: 500px;
  @include glass-panel;
  padding: 2rem;
  animation: fadeUp 1s cubic-bezier(0.16, 1, 0.3, 1) 0.2s both;
  @media (min-width: 768px) {
    left: var(--margin-desktop);
    width: 500px;
    padding: 3rem;
  }
  .room-title {
    @include font-display-lg-mobile;
    font-size: clamp(2rem, 6vw, 4rem);
    color: var(--color-secondary);
    line-height: 1;
    margin-bottom: 1rem;
  }
  .room-description {
    @include font-body-md;
    color: var(--color-on-surface-variant);
    margin-bottom: 1.5rem;
  }
  .divider-line {
    height: 1px;
    width: 100%;
    background: rgba(228, 226, 221, 0.3);
  }
}

// ── Details Section ──────────────────────────────
.details-section {
  max-width: var(--container-max);
  margin: var(--section-gap) auto;
  padding: 0 var(--margin-desktop);
  @media (max-width: 768px) {
    padding: 0 var(--margin-mobile);
    margin: 4rem auto;
  }
}

.details-grid {
  display: grid;
  grid-template-columns: 7fr 5fr;
  gap: 2rem;
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

// Metrics
.metrics-column {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
  @media (max-width: 600px) {
    grid-template-columns: 1fr;
  }
}
.metric-item {
  .metric-value {
    @include font-headline-md;
    color: var(--color-secondary);
  }
  .metric-divider {
    width: 3rem;
    height: 1px;
    background: rgba(228, 194, 133, 0.5);
    margin: 0.5rem 0;
  }
  .metric-label {
    @include font-label-caps;
    color: var(--color-on-surface-variant);
  }
}

// Bed Configuration
.config-column {
  margin-top: 0;
  @media (min-width: 768px) {
    margin-top: 0; // align with metrics
  }
}
.config-title {
  @include font-label-caps;
  color: var(--color-secondary);
  letter-spacing: 0.3em;
  margin-bottom: 2rem;
  text-transform: uppercase;
}
.config-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}
.config-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  @include font-body-md;
  color: var(--color-on-surface);
  transition: color 0.3s;
  &:hover { color: var(--color-secondary); }
  .config-icon {
    font-size: 1.5rem;
    color: var(--color-secondary);
    transition: transform 0.2s;
  }
  &:hover .config-icon { transform: scale(1.1); }
}

// ── Navigation Arrow Hover Reset ─────────────────
// We no longer have desktop nav-arrow overlay styling

// ── CTA Section ──────────────────────────────────
.cta-section {
  margin-top: var(--section-gap);
  width: 100%;
}
.cta-button {
  width: 100%;
  padding: 3rem 1rem;
  border: 1px solid var(--color-secondary);
  background: transparent;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  display: flex;
  justify-content: center;
  align-items: center;
  .cta-text {
    @include font-display-lg-mobile;
    font-size: clamp(1.5rem, 5vw, 2.5rem);
    color: var(--color-secondary);
    letter-spacing: 0.1em;
    text-transform: uppercase;
    position: relative;
    z-index: 10;
    transition: color 0.5s;
  }
  .cta-hover-fill {
    position: absolute;
    inset: 0;
    background: var(--color-secondary);
    transform: translateY(100%);
    transition: transform 0.7s ease;
  }
  &:hover {
    .cta-text { color: var(--color-background); }
    .cta-hover-fill { transform: translateY(0); }
  }
}

// Fade‑up animation for glass panel
@keyframes fadeUp {
  from { opacity: 0; transform: translateY(2rem); }
  to { opacity: 1; transform: translateY(0); }
}

.back-to-villas-btn {
  position: absolute;
  top: 2rem;
  left: var(--margin-mobile);
  z-index: 20;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  @include font-label-caps;
  color: var(--color-on-surface);
  text-decoration: none;
  background: rgba(26, 26, 26, 0.6);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 1px solid rgba(228, 194, 133, 0.3);
  padding: 0.5rem 1rem;
  border-radius: 20px;
  transition: all 0.3s ease;
  @media (min-width: 768px) {
    top: 3rem;
    left: var(--margin-desktop);
  }
  &:hover {
    border-color: var(--color-secondary);
    color: var(--color-secondary);
    background: rgba(26, 26, 26, 0.9);
    transform: translateX(-4px);
  }
}


# /Frontend/src/app/features/public/pages/room-detail.component.ts

import { Component, inject, signal, OnInit, AfterViewInit, ElementRef, ViewChild, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-room-detail',
  standalone: true,
  imports: [
    CommonModule, RouterModule, MatProgressSpinnerModule
  ],
  templateUrl: './room-detail.component.html',
  styleUrls: ['./room-detail.component.scss']
})
export class RoomDetailComponent implements OnInit, AfterViewInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  room = signal<RoomType | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  // Parallax
  @ViewChild('galleryContainer') galleryRef!: ElementRef<HTMLElement>;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.fetchRoom(id);
    } else {
      this.error.set('Room not found.');
    }
  }

  ngAfterViewInit(): void {
    // Bind scroll listener for parallax after view init
    const gallery = this.galleryRef?.nativeElement;
    if (gallery) {
      gallery.addEventListener('scroll', this.onGalleryScroll);
    }
  }

  ngOnDestroy(): void {
    if (this.galleryRef?.nativeElement) {
      this.galleryRef.nativeElement.removeEventListener('scroll', this.onGalleryScroll);
    }
  }

  private fetchRoom(id: number): void {
    this.loading.set(true);
    this.roomTypeApi.getById(id).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (data: any) => this.room.set(data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  getBedEntries(): [string, number][] {
    const config = this.room()?.bedConfiguration;
    if (!config) return [];
    return Object.entries(config).filter(([, v]) => v > 0);
  }

  getBedIcon(bedType: string): string {
    const icons: Record<string, string> = {
      'King': 'king_bed',
      'Queen': 'bed',
      'Twin': 'single_bed',
      'Double': 'bed',
    };
    return icons[bedType] || 'bed';
  }

  checkAvailability(): void {
    const roomId = this.room()?.id;
    if (roomId) {
      // Store room type ID for later booking flow
      sessionStorage.setItem('selectedRoomTypeId', String(roomId));
      this.router.navigate(['/availability'], { queryParams: { roomTypeId: roomId } });
    }
  }

  private onGalleryScroll = (): void => {
    const gallery = this.galleryRef?.nativeElement;
    if (!gallery) return;
    const scrollLeft = gallery.scrollLeft;
    const parallaxImages = gallery.querySelectorAll('.parallax-img') as NodeListOf<HTMLElement>;
    parallaxImages.forEach((img) => {
      const speed = parseFloat(img.getAttribute('data-speed') || '0');
      const imgTag = img.querySelector('img') as HTMLImageElement;
      if (imgTag) {
        imgTag.style.transform = `translateX(${scrollLeft * speed}px) scale(1.1)`;
      }
    });
  };

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/public/public-shell.component.html

<header class="main-nav" [class.scrolled]="isScrolled()">
  <nav class="nav-container">
    <div class="logo" routerLink="/home">AETHERIS</div>
    <div class="desktop-links">
      <a routerLink="/home" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}" class="nav-link underline-reveal">The Estate</a>
      <a routerLink="/rooms" routerLinkActive="active" class="nav-link underline-reveal">Villas</a>
      <a routerLink="/experiences" routerLinkActive="active" class="nav-link underline-reveal">Dining &amp; Amenities</a>
      <a routerLink="/availability" routerLinkActive="active" class="nav-link underline-reveal">Reservations</a>
    </div>
    <div class="nav-actions">
      <a class="inquire-btn" routerLink="/auth">Login</a>
      <button class="menu-btn" (click)="drawerOpen.set(true)" aria-label="Menu">
        <span class="material-symbols-outlined">menu</span>
      </button>
    </div>
  </nav>
</header>

<!-- Mobile Drawer -->
@if (drawerOpen()) {
  <div class="drawer-overlay" (click)="closeDrawer()"></div>
  <aside class="mobile-drawer" [class.open]="drawerOpen()">
    <div class="drawer-header">
      <span class="logo">AETHERIS</span>
      <button class="close-btn" (click)="closeDrawer()" aria-label="Close menu">
        <span class="material-symbols-outlined">close</span>
      </button>
    </div>
    <nav>
      <a routerLink="/home" (click)="closeDrawer()">
        <span class="material-symbols-outlined">explore</span>
        The Estate
      </a>
      <a routerLink="/rooms" (click)="closeDrawer()">
        <span class="material-symbols-outlined">villa</span>
        Villas
      </a>
      <a routerLink="/experiences" (click)="closeDrawer()">
        <span class="material-symbols-outlined">spa</span>
        Dining &amp; Amenities
      </a>
      <a routerLink="/availability" (click)="closeDrawer()">
        <span class="material-symbols-outlined">calendar_month</span>
        Reservations
      </a>
    </nav>
  </aside>
}

<main>
  <router-outlet></router-outlet>
</main>

<footer class="site-footer">
  <div class="footer-links">
    <a href="#">Privacy Policy</a>
    <a href="#">Terms of Service</a>
    <a href="#">Press</a>
    <a href="#">Careers</a>
    <a href="#">Contact</a>
  </div>
  <div class="footer-logo">AETHERIS</div>
  <div class="footer-info">
    <span>1 AETHERIS PEAK, THE SILENT VALLEY</span>
    <span class="separator"></span>
    <span>&copy; 2024 AETHERIS. ALL RIGHTS RESERVED.</span>
  </div>
</footer>


# /Frontend/src/app/features/public/public-shell.component.scss

@import '../../../styles/theme/index';

.main-nav {
  position: fixed;
  top: 0;
  width: 100%;
  z-index: 50;
  transition: background 0.5s, backdrop-filter 0.5s, border 0.5s;
  &.scrolled {
    background: rgba(10, 10, 10, 0.8);
    backdrop-filter: blur(24px);
    border-bottom: 1px solid var(--glass-border);
  }
  .nav-container {
    display: flex;
    justify-content: space-between;
    align-items: center;
    max-width: var(--container-max);
    margin: 0 auto;
    padding: 1.5rem var(--margin-desktop);
    @media (max-width: 768px) {
      padding: 1rem var(--margin-mobile);
    }
  }
  .logo {
    font-family: var(--font-headline);
    font-size: 1.5rem;
    letter-spacing: 0.3em;
    color: var(--color-on-surface);
    text-transform: uppercase;
    cursor: pointer;
    user-select: none;
  }
  .desktop-links {
    display: none;
    gap: 2.5rem;
    @media (min-width: 768px) {
      display: flex;
    }
    .nav-link {
      @include font-label-caps;
      color: var(--color-on-surface);
      text-decoration: none;
      padding-bottom: 4px;
      transition: color 0.3s;
      &:hover,
      &.active {
        color: var(--color-secondary);
      }
    }
  }
  .nav-actions {
    display: flex;
    align-items: center;
    gap: 1.5rem;
    .inquire-btn {
      @include font-label-caps;
      color: var(--color-secondary);
      text-decoration: none;
      transition: opacity 0.2s;
      &:hover { opacity: 0.8; }
    }
    .menu-btn {
      background: none;
      border: none;
      color: var(--color-on-surface);
      cursor: pointer;
      @media (min-width: 768px) { display: none; }
      .material-symbols-outlined { font-size: 1.5rem; }
    }
  }
}

// Underline reveal for desktop links
.underline-reveal {
  position: relative;
  &::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 1px;
    background: var(--color-secondary);
    transform: scaleX(0);
    transform-origin: right;
    transition: transform 0.6s cubic-bezier(0.19, 1, 0.22, 1);
  }
  &:hover::after,
  &.active::after {
    transform: scaleX(1);
    transform-origin: left;
  }
}

// Drawer
.drawer-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 55;
}
.mobile-drawer {
  position: fixed;
  top: 0;
  right: 0;
  width: min(400px, 80vw);
  height: 100%;
  @include glass-panel;
  z-index: 60;
  transform: translateX(100%);
  transition: transform 0.5s cubic-bezier(0.16, 1, 0.3, 1);
  &.open {
    transform: translateX(0);
  }
  padding: 2rem;
  display: flex;
  flex-direction: column;
  .drawer-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    .logo {
      font-family: var(--font-headline);
      font-size: 1.5rem;
      letter-spacing: 0.3em;
      color: var(--color-on-surface);
    }
    .close-btn {
      background: none;
      border: none;
      color: var(--color-on-surface);
      cursor: pointer;
      .material-symbols-outlined { font-size: 1.75rem; }
    }
  }
  nav {
    display: flex;
    flex-direction: column;
    margin-top: 3rem;
    gap: 2rem;
    a {
      @include font-label-caps;
      color: var(--color-on-surface-variant);
      text-decoration: none;
      display: flex;
      align-items: center;
      gap: 1rem;
      transition: color 0.3s;
      .material-symbols-outlined {
        font-variation-settings: 'FILL' 1;
        font-size: 1.25rem;
      }
      &:hover {
        color: var(--color-primary);
      }
    }
  }
}

// Footer
.site-footer {
  background: var(--color-surface-container-lowest);
  padding: 6rem 1rem 3rem;
  text-align: center;
  border-top: 1px solid var(--glass-border);

  .footer-links {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 2rem;
    margin-bottom: 3rem;
    a {
      @include font-body-md;
      color: var(--color-on-tertiary-container);
      text-decoration: none;
      transition: color 0.3s;
      &:hover { color: var(--color-secondary); }
    }
    @media (max-width: 768px) {
      gap: 1.2rem;
      a { font-size: 0.85rem; }
    }
  }

  .footer-logo {
    font-family: var(--font-headline);
    font-size: clamp(3rem, 10vw, 7.5rem);
    letter-spacing: 0.3em;
    color: var(--color-on-surface);
    margin-bottom: 1.5rem;
    text-transform: uppercase;
  }

  .footer-info {
    font-family: var(--font-body);
    font-size: 0.625rem;
    font-weight: 500;
    letter-spacing: 0.3em;
    text-transform: uppercase;
    color: rgba(228, 226, 221, 0.4);
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 1.5rem;
    .separator {
      display: inline-block;
      width: 4px;
      height: 4px;
      border-radius: 50%;
      background: rgba(228, 226, 221, 0.2);
    }
  }
}


# /Frontend/src/app/features/public/public-shell.component.ts

import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-public-shell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './public-shell.component.html',
  styleUrls: ['./public-shell.component.scss']
})
export class PublicShellComponent {
  private breakpointObserver = inject(BreakpointObserver);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 768px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );
  drawerOpen = signal(false);

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled.set(window.scrollY > 50);
  }
  isScrolled = signal(false);

  closeDrawer() {
    this.drawerOpen.set(false);
  }
}


# /Frontend/src/app/features/user/components/billing-dialog/billing-dialog.component.html

<h2 mat-dialog-title>Billing Folio – Booking #{{ bookingId }}</h2>
<mat-dialog-content class="billing-content">
  @if (loading()) {
    <div style="display: flex; justify-content: center; padding: 24px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchBilling()">Retry</button>
    </app-alert>
  } @else if (folio()) {
    <div class="bill-row">
      <span class="bill-label">Guest:</span>
      <span class="bill-value">{{ folio()!.guestName }}</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Nights:</span>
      <span class="bill-value">{{ folio()!.nightsStayed }}</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Room Rate:</span>
      <span class="bill-value">{{ folio()!.roomBasePrice | currency }}/night</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Room Subtotal:</span>
      <span class="bill-value">{{ folio()!.roomTotal | currency }}</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Room Services / Food:</span>
      <span class="bill-value">{{ folio()!.foodTotal | currency }}</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Amenities Subtotal:</span>
      <span class="bill-value">{{ folio()!.amenityTotal | currency }}</span>
    </div>
    <div class="bill-row total-bill-row">
      <span>Total Bill:</span>
      <span>{{ folio()!.totalBill | currency }}</span>
    </div>
    <div class="bill-row" style="margin-top: 12px;">
      <span class="bill-label">Payment Status:</span>
      <span class="bill-value" [style.color]="folio()!.paymentStatus === 'Paid' ? 'green' : 'red'">
        {{ folio()!.paymentStatus }}
      </span>
    </div>

    @if (folio()!.foodItems && folio()!.foodItems.length > 0) {
      <h3>Room Service Orders</h3>
      <ul>
        @for (item of folio()!.foodItems; track item) {
          <li>{{ item }}</li>
        }
      </ul>
    }

    @if (folio()!.amenityItems && folio()!.amenityItems.length > 0) {
      <h3>Amenities Subscribed</h3>
      <ul>
        @for (item of folio()!.amenityItems; track item) {
          <li>{{ item }}</li>
        }
      </ul>
    }
  } @else {
    <p>No billing details found.</p>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Close</button>
</mat-dialog-actions>


# /Frontend/src/app/features/user/components/billing-dialog/billing-dialog.component.ts

import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BillingApiService } from '../../services/billing-api.service';
import { BillingFolio } from '../../models/billing-folio.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-billing-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatProgressSpinnerModule, AlertComponent],
  templateUrl: './billing-dialog.component.html',
  styles: [`
    .billing-content {
      min-width: 320px;
    }
    .bill-row {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
      font-size: 0.95rem;
    }
    .bill-label {
      font-weight: 500;
      color: rgba(0, 0, 0, 0.6);
    }
    .bill-value {
      font-weight: 600;
      color: rgba(0, 0, 0, 0.87);
    }
    .total-bill-row {
      border-top: 1px solid rgba(0,0,0,0.12);
      padding-top: 8px;
      margin-top: 8px;
      font-weight: bold;
      font-size: 1.1rem;
    }
    h3 {
      margin: 16px 0 8px 0;
      font-size: 1rem;
      border-bottom: 1px solid #f0f0f0;
      padding-bottom: 4px;
    }
    ul {
      margin: 0;
      padding-left: 20px;
      font-size: 0.9rem;
      color: rgba(0,0,0,0.7);
    }
  `]
})
export class BillingDialogComponent implements OnInit {
  readonly bookingId: number = inject(MAT_DIALOG_DATA);
  private readonly billingApi = inject(BillingApiService);

  folio = signal<BillingFolio | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchBilling();
  }

  fetchBilling(): void {
    this.loading.set(true);
    this.error.set(null);

    this.billingApi.getByBookingId(this.bookingId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.folio.set(data),
        error: (err) => {
          const message = err.error?.message || err.message || 'Could not fetch billing details.';
          this.error.set(message);
        }
      });
  }
}


# /Frontend/src/app/features/user/components/booking-detail-dialog/booking-detail-dialog.component.html

<h2 mat-dialog-title>Booking Details – #{{ booking.id }}</h2>
<mat-dialog-content>
  <div class="detail-section">
    <div class="detail-row">
      <span class="detail-label">Status:</span>
      <span class="detail-value"><strong>{{ booking.bookingStatus }}</strong></span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Guest Name:</span>
      <span class="detail-value">{{ booking.guestName }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Guest Email:</span>
      <span class="detail-value">{{ booking.guestEmail }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Guests:</span>
      <span class="detail-value">{{ booking.guestCount }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Check‑in:</span>
      <span class="detail-value">{{ booking.checkInDate }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Check‑out:</span>
      <span class="detail-value">{{ booking.checkOutDate }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Booked At:</span>
      <span class="detail-value">{{ booking.bookedAt | date:'medium' }}</span>
    </div>
  </div>

  <mat-divider></mat-divider>

  <div class="detail-section" style="margin-top: 16px;">
    <h3>Rooms Included</h3>
    @if (enrichedRooms().length > 0) {
      @for (room of enrichedRooms(); track room.id) {
        <div class="room-item" style="margin-bottom: 12px; padding: 8px; border-left: 3px solid #1976d2; background: #f9f9f9; border-radius: 0 4px 4px 0;">
          <p style="margin: 0 0 4px 0;"><strong>Room:</strong> {{ room.roomNumber ?? 'Unassigned' }}</p>
          <p style="margin: 0 0 4px 0;"><strong>Type:</strong> {{ room.roomTypeName }}</p>
          <p style="margin: 0;"><strong>Price:</strong> {{ room.lockedInPrice | currency }}</p>
        </div>
      }
    } @else if (booking.rooms && booking.rooms.length > 0) {
      <ul>
        @for (room of booking.rooms; track room.id) {
          <li>
            Room Number: {{ room.roomNumber || 'Pending Assignment' }}
            (Locked‑in Price: {{ room.lockedInPrice | currency }})
          </li>
        }
      </ul>
    } @else {
      <p>No rooms assigned.</p>
    }
  </div>

  <mat-divider></mat-divider>

  <div class="detail-section" style="margin-top: 16px;">
    <h3>Amenities Subscribed</h3>
    @if (booking.amenityIds && booking.amenityIds.length > 0) {
      <ul>
        @for (id of booking.amenityIds; track id) {
          <li>Amenity ID: {{ id }} (TODO: Resolve Amenity Names)</li>
        }
      </ul>
    } @else {
      <p>No amenities selected.</p>
    }
  </div>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Close</button>
</mat-dialog-actions>


# /Frontend/src/app/features/user/components/booking-detail-dialog/booking-detail-dialog.component.ts

import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { Booking, BookingRoom } from '../../models/booking.model';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-booking-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatListModule, MatDividerModule],
  templateUrl: './booking-detail-dialog.component.html',
  styles: [`
    .detail-section {
      margin-bottom: 16px;
    }
    .detail-row {
      display: flex;
      margin-bottom: 8px;
      font-size: 0.95rem;
    }
    .detail-label {
      font-weight: 600;
      width: 140px;
      color: rgba(0, 0, 0, 0.6);
    }
    .detail-value {
      color: rgba(0, 0, 0, 0.87);
    }
    ul {
      margin: 4px 0 0 0;
      padding-left: 20px;
    }
  `]
})
export class BookingDetailDialogComponent implements OnInit {
  readonly booking: Booking = inject(MAT_DIALOG_DATA);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly destroyRef = inject(DestroyRef);

  enrichedRooms = signal<(BookingRoom & { roomTypeName: string })[]>([]);

  ngOnInit(): void {
    this.enrichRooms();
  }

  private enrichRooms(): void {
    const rooms = this.booking.rooms ?? [];
    if (rooms.length === 0) return;

    const requests = rooms.map(room =>
      this.roomTypeApi.getById(room.roomTypeId).pipe(
        map(roomType => ({
          ...room,
          roomTypeName: roomType?.name ?? `Room Type ${room.roomTypeId}`
        })),
        catchError(() => of({
          ...room,
          roomTypeName: `Room Type ${room.roomTypeId}`
        }))
      )
    );

    forkJoin(requests).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(enriched => {
      this.enrichedRooms.set(enriched);
    });
  }
}


# /Frontend/src/app/features/user/components/booking-history/booking-history.component.html

<div class="history-view">
  <div class="controls">
    <mat-form-field appearance="outline">
      <mat-label>Status</mat-label>
      <mat-select
        [formControl]="statusFilter"
        (selectionChange)="onFilterChange()"
      >
        <mat-option value="">All</mat-option>
        <mat-option value="Booked">Booked</mat-option>
        <mat-option value="CheckedIn">Checked In</mat-option>
        <mat-option value="CheckedOut">Checked Out</mat-option>
        <mat-option value="Cancelled">Cancelled</mat-option>
      </mat-select>
    </mat-form-field>
    @if (statusFilter.value) {
      <button
        mat-button
        (click)="clearFilter()"
      >
        Clear Filter
      </button>
    }
  </div>

  @if (loading() && bookings().length === 0) {
    <div style="display: flex; justify-content: center; padding: 24px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert
      type="error"
      [message]="error()!"
      (closed)="error.set(null)"
    >
      <button
        mat-button
        (click)="fetchData()"
      >
        Retry
      </button>
    </app-alert>
  }

  @if (bookings().length > 0 || loading()) {
    @if (loading()) {
      <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    <div class="table-container">
      <table
        mat-table
        [dataSource]="bookings()"
        matSort
        matSortDisableClear
        (matSortChange)="onSortChange($event)"
      >
        <!-- ID Column -->
        <ng-container matColumnDef="id">
          <th
            mat-header-cell
            *matHeaderCellDef
            mat-sort-header="id"
          >
            ID
          </th>
          <td
            mat-cell
            *matCellDef="let b"
          >
            {{ b.id }}
          </td>
        </ng-container>

        <!-- Check-in Column -->
        <ng-container matColumnDef="checkIn">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Check‑in
          </th>
          <td
            mat-cell
            *matCellDef="let b"
          >
            {{ b.checkInDate }}
          </td>
        </ng-container>

        <!-- Check-out Column -->
        <ng-container matColumnDef="checkOut">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Check‑out
          </th>
          <td
            mat-cell
            *matCellDef="let b"
          >
            {{ b.checkOutDate }}
          </td>
        </ng-container>

        <!-- Status Column -->
        <ng-container matColumnDef="status">
          <th
            mat-header-cell
            *matHeaderCellDef
            mat-sort-header="bookingStatus"
          >
            Status
          </th>
          <td
            mat-cell
            *matCellDef="let b"
          >
            {{ b.bookingStatus }}
          </td>
        </ng-container>

        <!-- Rooms Column -->
        <ng-container matColumnDef="rooms">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Rooms
          </th>
          <td
            mat-cell
            *matCellDef="let b"
          >
            {{ getRoomsSummary(b) }}
          </td>
        </ng-container>

        <!-- Actions Column -->
        <ng-container matColumnDef="actions">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Actions
          </th>
          <td
            mat-cell
            *matCellDef="let b"
          >
            <button
              mat-icon-button
              (click)="openDetail(b)"
              aria-label="View"
              matTooltip="View Details"
            >
              <mat-icon>visibility</mat-icon>
            </button>
            @if (b.bookingStatus === 'Booked') {
              <button
                mat-icon-button
                (click)="cancelBooking(b)"
                aria-label="Cancel"
                matTooltip="Cancel Booking"
              >
                <mat-icon>cancel</mat-icon>
              </button>
            }
            @if (b.bookingStatus === 'CheckedOut') {
              <button
                mat-icon-button
                (click)="openFeedback(b)"
                aria-label="Feedback"
                matTooltip="Leave Feedback"
              >
                <mat-icon>feedback</mat-icon>
              </button>
            }
            <button
              mat-icon-button
              (click)="openBilling(b)"
              aria-label="Billing"
              matTooltip="View Billing"
            >
              <mat-icon>receipt</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr
          mat-header-row
          *matHeaderRowDef="displayedColumns"
        ></tr>
        <tr
          mat-row
          *matRowDef="let row; columns: displayedColumns"
          [class.highlight]="highlightRowId() === row.id"
        ></tr>
      </table>
    </div>

    <mat-paginator
      [length]="totalCount()"
      [pageSize]="pageSize"
      [pageIndex]="pageIndex"
      [pageSizeOptions]="[5, 10, 20]"
      (page)="onPageChange($event)"
      aria-label="Select page of bookings"
    ></mat-paginator>
  } @else if (!loading()) {
    <p class="no-bookings">No bookings found.</p>
  }
</div>


# /Frontend/src/app/features/user/components/booking-history/booking-history.component.scss

.history-view {
  .controls {
    display: flex;
    gap: 16px;
    align-items: center;
    margin-bottom: 16px;
  }

  .table-container {
    overflow-x: auto;
    width: 100%;
    margin-bottom: 16px;
    border: 1px solid rgba(0, 0, 0, 0.12);
    border-radius: 4px;
  }

  table {
    width: 100%;
    border-collapse: collapse;

    tr.highlight {
      background-color: #e8eaf6 !important;
      animation: flash-highlight 2s ease-out;
    }
  }

  .no-bookings {
    padding: 24px;
    text-align: center;
    color: rgba(0, 0, 0, 0.54);
    font-size: 1.1rem;
  }
}

@keyframes flash-highlight {
  0% {
    background-color: #c5cae9;
  }
  100% {
    background-color: transparent;
  }
}


# /Frontend/src/app/features/user/components/booking-history/booking-history.component.ts

import { Component, inject, signal, input, effect, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, MatSort, Sort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { BookingApiService } from '../../services/booking-api.service';
import { Booking } from '../../../../features/admin/models/booking.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { BookingDetailDialogComponent } from '../booking-detail-dialog/booking-detail-dialog.component';
import { BillingDialogComponent } from '../billing-dialog/billing-dialog.component';
import { FeedbackDialogComponent } from '../feedback-dialog/feedback-dialog.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { finalize } from 'rxjs/operators';

interface HistoryState {
  status: string;
  sortField: string;
  sortDescending: boolean;
  pageIndex: number;
  pageSize: number;
}

@Component({
  selector: 'app-booking-history',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDialogModule,
    MatSnackBarModule,
    AlertComponent
  ],
  templateUrl: './booking-history.component.html',
  styleUrls: ['./booking-history.component.scss']
})
export class BookingHistoryComponent implements AfterViewInit {
  userEmail = input.required<string>();
  refresh = input(0);
  highlightBookingId = input<number | null>(null);

  private readonly bookingApi = inject(BookingApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  bookings = signal<Booking[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);
  highlightRowId = signal<number | null>(null);

  statusFilter = new FormControl<string>('', { nonNullable: true });
  displayedColumns = ['id', 'checkIn', 'checkOut', 'status', 'rooms', 'actions'];

  pageIndex = 0;
  pageSize = 10;
  sortField = 'id';
  sortDescending = true;

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  private readonly STORAGE_KEY = 'customerBookingsState';

  constructor() {
    this.loadState();

    // Effect to trigger load when email or refresh trigger changes
    effect(() => {
      const email = this.userEmail();
      const ref = this.refresh();
      if (email) {
        this.fetchData();
      }
    });

    // Effect to trigger row highlighting
    effect(() => {
      const highlightId = this.highlightBookingId();
      if (highlightId != null) {
        this.highlightRowId.set(highlightId);
        setTimeout(() => {
          this.highlightRowId.set(null);
          // Also try to scroll to the highlighted element
          const rowEl = document.querySelector(`.highlight`);
          if (rowEl) {
            rowEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
          }
        }, 100);
      }
    });
  }

  ngAfterViewInit(): void {
    // Restore sort state visually — sort may be undefined on first render because
    // the table is wrapped in an @if block (no data yet), so guard before access.
    if (this.sort) {
      this.sort.active = this.sortField;
      this.sort.direction = this.sortDescending ? 'desc' : 'asc';
    }
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);

    this.bookingApi.getAll({
      guestQuery: this.userEmail(),
      status: this.statusFilter.value || undefined,
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sortField,
      sortDescending: this.sortDescending
    }).pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (res) => {
        this.bookings.set(res.data);
        this.totalCount.set(res.totalCount);
        this.saveState();

        // If we are highlighting a row, let's schedule a scroll after DOM is updated
        const highlightId = this.highlightBookingId();
        if (highlightId != null) {
          setTimeout(() => {
            const rowEl = document.querySelector(`.highlight`);
            if (rowEl) {
              rowEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
          }, 300);
        }
      },
      error: (err) => {
        const message = err.error?.message || err.message || 'Failed to load bookings.';
        this.error.set(message);
      }
    });
  }

  onFilterChange(): void {
    this.pageIndex = 0;
    if (this.paginator) {
      this.paginator.pageIndex = 0;
    }
    this.fetchData();
  }

  clearFilter(): void {
    this.statusFilter.setValue('');
    this.onFilterChange();
  }

  onSortChange(sort: Sort): void {
    this.sortField = sort.active;
    this.sortDescending = sort.direction === 'desc';
    this.pageIndex = 0;
    if (this.paginator) {
      this.paginator.pageIndex = 0;
    }
    this.fetchData();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.fetchData();
  }

  getRoomsSummary(booking: Booking): string {
    return booking.rooms
      .filter(r => r.roomNumber !== null)
      .map(r => r.roomNumber as string)
      .join(', ') || 'Pending Assignment';
  }

  openDetail(booking: Booking): void {
    this.dialog.open(BookingDetailDialogComponent, {
      data: booking,
      width: '500px'
    });
  }

  cancelBooking(booking: Booking): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Cancel Booking',
        message: `Are you sure you want to cancel booking #${booking.id}?`
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.loading.set(true);
        this.bookingApi.cancel(booking.id)
          .pipe(finalize(() => this.loading.set(false)))
          .subscribe({
            next: () => {
              this.snackBar.open('Booking successfully cancelled.', 'Close', { duration: 4000 });
              this.fetchData();
            },
            error: (err) => {
              const message = err.error?.message || err.message || 'Failed to cancel booking.';
              this.snackBar.open(message, 'Close', { duration: 5000 });
            }
          });
      }
    });
  }

  openFeedback(booking: Booking): void {
    this.dialog.open(FeedbackDialogComponent, {
      data: booking.id,
      width: '450px'
    });
  }

  openBilling(booking: Booking): void {
    this.dialog.open(BillingDialogComponent, {
      data: booking.id,
      width: '500px'
    });
  }

  private loadState(): void {
    try {
      const stateStr = sessionStorage.getItem(this.STORAGE_KEY);
      if (stateStr) {
        const state: HistoryState = JSON.parse(stateStr);
        this.statusFilter.setValue(state.status);
        this.sortField = state.sortField;
        this.sortDescending = state.sortDescending;
        this.pageIndex = state.pageIndex;
        this.pageSize = state.pageSize;
      }
    } catch (e) {
      console.error('Error loading history state:', e);
    }
  }

  private saveState(): void {
    try {
      const state: HistoryState = {
        status: this.statusFilter.value,
        sortField: this.sortField,
        sortDescending: this.sortDescending,
        pageIndex: this.pageIndex,
        pageSize: this.pageSize
      };
      sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
    } catch (e) {
      console.error('Error saving history state:', e);
    }
  }
}


# /Frontend/src/app/features/user/components/booking-wizard/booking-wizard.component.html

<mat-stepper
  linear
  #stepper
  [orientation]="isMobile() ? 'vertical' : 'horizontal'"
  (selectionChange)="onStepChange($event)"
>
  <!-- Step 1: Dates & Guests -->
  <mat-step
    [stepControl]="datesForm"
    label="Dates & Guests"
  >
    <form [formGroup]="datesForm" class="stepper-form">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Check‑in</mat-label>
        <input
          matInput
          [matDatepicker]="cinPicker"
          formControlName="checkInDate"
          required
        />
        <mat-datepicker-toggle matIconSuffix [for]="cinPicker"></mat-datepicker-toggle>
        <mat-datepicker #cinPicker></mat-datepicker>
        @if (datesForm.get('checkInDate')?.touched && datesForm.get('checkInDate')?.invalid) {
          <mat-error>Check‑in date is required.</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Check‑out</mat-label>
        <input
          matInput
          [matDatepicker]="coutPicker"
          formControlName="checkOutDate"
          required
        />
        <mat-datepicker-toggle matIconSuffix [for]="coutPicker"></mat-datepicker-toggle>
        <mat-datepicker #coutPicker></mat-datepicker>
        @if (datesForm.get('checkOutDate')?.touched && datesForm.get('checkOutDate')?.invalid) {
          <mat-error>Check‑out date is required.</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Guests</mat-label>
        <input
          matInput
          type="number"
          formControlName="guestCount"
          min="1"
          max="20"
          required
        />
        @if (datesForm.get('guestCount')?.touched && datesForm.get('guestCount')?.invalid) {
          <mat-error>Guests count must be between 1 and 20.</mat-error>
        }
      </mat-form-field>

      @if (datesForm.touched && datesForm.errors) {
        <div class="form-error">
          @if (datesForm.errors['checkInInPast']) {
            <p>Check‑in date cannot be in the past.</p>
          }
          @if (datesForm.errors['checkOutBeforeCheckIn']) {
            <p>Check‑out date must be strictly after Check‑in date.</p>
          }
        </div>
      }

      <div class="actions">
        <button
          mat-raised-button
          color="primary"
          matStepperNext
          [disabled]="datesForm.invalid"
        >
          Next
        </button>
      </div>
    </form>
  </mat-step>

  <!-- Step 2: Room Selection -->
  <mat-step
    [stepControl]="roomsForm"
    label="Select Rooms"
  >
    @if (loading()) {
      <div style="display: flex; justify-content: center; padding: 24px;">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (error()) {
      <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
        <button mat-button (click)="loadRooms()">Retry</button>
      </app-alert>
    } @else {
      <form [formGroup]="roomsForm" class="stepper-form">
        <div class="room-list">
          @for (room of availableRooms(); track room.roomTypeId) {
            <div class="room-item">
              <div class="room-info">
                <h3>{{ room.name }}</h3>
                <p>{{ room.description || 'No description available.' }}</p>
                <div class="room-meta">
                  <span><strong>{{ room.basePrice | currency }}</strong>/night</span>
                  <span class="divider">|</span>
                  <span>Max guests: {{ room.maxOccupancy }}</span>
                  <span class="divider">|</span>
                  <span [style.color]="room.availableCount > 0 ? 'green' : 'red'">
                    Available: {{ room.availableCount }}
                  </span>
                </div>
              </div>
              <div class="quantity-selector">
                <button
                  mat-icon-button
                  type="button"
                  (click)="decrementRoom(room.roomTypeId)"
                  [disabled]="getRoomQuantity(room.roomTypeId) === 0"
                  aria-label="Remove room"
                >
                  <mat-icon>remove</mat-icon>
                </button>
                <span class="qty-display">{{ getRoomQuantity(room.roomTypeId) }}</span>
                <button
                  mat-icon-button
                  type="button"
                  (click)="incrementRoom(room.roomTypeId)"
                  [disabled]="getRoomQuantity(room.roomTypeId) >= room.availableCount"
                  aria-label="Add room"
                >
                  <mat-icon>add</mat-icon>
                </button>
              </div>
            </div>
          }
        </div>

        @if (capacityWarning()) {
          <p class="warning-text"><mat-icon>warning</mat-icon> {{ capacityWarning() }}</p>
        }

        <div class="actions" style="margin-top: 16px;">
          <button mat-button matStepperPrevious>Back</button>
          <button
            mat-raised-button
            color="primary"
            matStepperNext
            [disabled]="totalSelectedQuantity() === 0 || capacityWarning() !== null"
          >
            Next
          </button>
        </div>
      </form>
    }
  </mat-step>

  <!-- Step 3: Amenities -->
  <mat-step
    [stepControl]="amenitiesForm"
    label="Add Amenities"
  >
    @if (loading()) {
      <div style="display: flex; justify-content: center; padding: 24px;">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (error()) {
      <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
        <button mat-button (click)="loadAmenities()">Retry</button>
      </app-alert>
    } @else {
      <form [formGroup]="amenitiesForm" class="stepper-form">
        <div class="amenity-list">
          @for (amenity of availableAmenities(); track amenity.id; let i = $index) {
            <div class="amenity-item">
              <mat-checkbox [formControl]="getAmenityControl(i)">
                <div class="amenity-info">
                  <strong>{{ amenity.name }}</strong> – {{ amenity.price | currency }}
                  <p class="amenity-desc">{{ amenity.description }}</p>
                </div>
              </mat-checkbox>
            </div>
          }
        </div>
        <div class="actions" style="margin-top: 16px;">
          <button mat-button matStepperPrevious>Back</button>
          <button
            mat-raised-button
            color="primary"
            matStepperNext
          >
            Next
          </button>
        </div>
      </form>
    }
  </mat-step>

  <!-- Step 4: Review & Confirm -->
  <mat-step label="Review & Confirm">
    <div class="summary-container">
      <h3>Stay Overview</h3>
      <div class="overview-grid">
        <div><strong>Guest Name:</strong> {{ userProfile().firstName }} {{ userProfile().lastName }}</div>
        <div><strong>Email:</strong> {{ userProfile().email }}</div>
        <div><strong>Check‑in:</strong> {{ datesForm.value.checkInDate | date:'mediumDate' }}</div>
        <div><strong>Check‑out:</strong> {{ datesForm.value.checkOutDate | date:'mediumDate' }}</div>
        <div><strong>Nights Stayed:</strong> {{ nights() }}</div>
        <div><strong>Guest Count:</strong> {{ datesForm.value.guestCount }}</div>
      </div>

      <mat-divider></mat-divider>

      <h3>Rooms Selected</h3>
      @if (selectedRoomEntries().length > 0) {
        <ul class="summary-list">
          @for (item of selectedRoomEntries(); track item.roomTypeId) {
            <li>
              <div class="list-item-content">
                <span>{{ item.name }} x{{ item.quantity }}</span>
                <span>{{ item.quantity * item.basePrice * nights() | currency }}</span>
              </div>
            </li>
          }
        </ul>
      }

      <mat-divider></mat-divider>

      <h3>Amenities Selected</h3>
      @if (selectedAmenityEntries().length > 0) {
        <ul class="summary-list">
          @for (item of selectedAmenityEntries(); track item.id) {
            <li>
              <div class="list-item-content">
                <span>{{ item.name }}</span>
                <span>{{ item.price | currency }}</span>
              </div>
            </li>
          }
        </ul>
      } @else {
        <p class="empty-text">No amenities selected.</p>
      }

      <mat-divider></mat-divider>

      <div class="estimated-total-row">
        <span>Estimated Total:</span>
        <span class="total-price">{{ estimatedTotal() | currency }}</span>
      </div>

      @if (error()) {
        <app-alert type="error" [message]="error()!" (closed)="error.set(null)"></app-alert>
      }

      <div class="actions" style="margin-top: 24px;">
        <button mat-button matStepperPrevious [disabled]="loading()">Back</button>
        <button
          mat-raised-button
          color="primary"
          [disabled]="loading()"
          (click)="submitBooking()"
        >
          @if (loading()) {
            <mat-spinner diameter="18" style="display: inline-block; margin-right: 8px;"></mat-spinner>
          }
          Confirm Booking
        </button>
      </div>
    </div>
  </mat-step>
</mat-stepper>


# /Frontend/src/app/features/user/components/booking-wizard/booking-wizard.component.scss

.stepper-form {
  padding: 8px 0;
  max-width: 600px;
}

.full-width {
  width: 100%;
}

.form-error {
  color: #f44336;
  font-size: 0.85rem;
  margin-bottom: 16px;
  p {
    margin: 4px 0;
  }
}

.room-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 16px;
}

.room-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  border: 1px solid rgba(0, 0, 0, 0.12);
  border-radius: 8px;
  background-color: #fafafa;
}

.room-info {
  flex-grow: 1;
  padding-right: 16px;

  h3 {
    margin: 0 0 4px 0;
    font-size: 1.1rem;
    font-weight: 500;
  }

  p {
    margin: 0 0 8px 0;
    font-size: 0.875rem;
    color: rgba(0, 0, 0, 0.54);
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
}

.room-meta {
  display: flex;
  align-items: center;
  font-size: 0.85rem;
  color: rgba(0, 0, 0, 0.6);

  .divider {
    margin: 0 8px;
    color: rgba(0, 0, 0, 0.2);
  }
}

.quantity-selector {
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid rgba(0, 0, 0, 0.12);
  border-radius: 20px;
  padding: 2px;
  background: white;

  .qty-display {
    font-weight: 600;
    min-width: 24px;
    text-align: center;
  }
}

.warning-text {
  color: #ff9800;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
  margin-top: 12px;

  mat-icon {
    font-size: 20px;
    width: 20px;
    height: 20px;
  }
}

.amenity-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.amenity-item {
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 6px;
  padding: 12px;
  background-color: #fafafa;

  mat-checkbox {
    width: 100%;
  }

  .amenity-info {
    display: flex;
    flex-direction: column;
    margin-left: 8px;
  }

  .amenity-desc {
    margin: 4px 0 0 0;
    font-size: 0.8rem;
    color: rgba(0, 0, 0, 0.54);
  }
}

.summary-container {
  max-width: 600px;
  padding: 16px;
  border: 1px solid rgba(0, 0, 0, 0.12);
  border-radius: 8px;

  h3 {
    margin: 16px 0 12px 0;
    font-size: 1.1rem;
    font-weight: 500;
    color: #3f51b5;

    &:first-of-type {
      margin-top: 0;
    }
  }

  .overview-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
    margin-bottom: 16px;
    font-size: 0.9rem;
  }

  .summary-list {
    list-style: none;
    padding: 0;
    margin: 0 0 16px 0;

    li {
      padding: 8px 0;
      font-size: 0.95rem;
    }
  }

  .list-item-content {
    display: flex;
    justify-content: space-between;
  }

  .empty-text {
    font-style: italic;
    color: rgba(0,0,0,0.54);
    margin-bottom: 16px;
  }

  .estimated-total-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 1.15rem;
    font-weight: 600;
    margin-top: 16px;

    .total-price {
      font-size: 1.3rem;
      color: #3f51b5;
    }
  }
}

@media (max-width: 767px) {
  .room-item {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }

  .room-info {
    padding-right: 0;
  }

  .quantity-selector {
    align-self: flex-end;
  }

  .summary-container .overview-grid {
    grid-template-columns: 1fr;
  }
}


# /Frontend/src/app/features/user/components/booking-wizard/booking-wizard.component.ts

import { Component, inject, signal, computed, input, output, ChangeDetectorRef, DestroyRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, FormArray, Validators, AbstractControl } from '@angular/forms';
import { MatStepperModule } from '@angular/material/stepper';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { AmenityApiService } from '../../services/amenity-api.service';
import { BookingApiService } from '../../services/booking-api.service';
import { AvailableRoomType } from '../../models/available-room-type.model';
import { Amenity } from '../../../../features/admin/models/amenity.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { MatDividerModule } from '@angular/material/divider';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-booking-wizard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatDialogModule,
    AlertComponent
  ],
  templateUrl: './booking-wizard.component.html',
  styleUrls: ['./booking-wizard.component.scss']
})
export class BookingWizardComponent implements OnInit {
  userProfile = input.required<{ firstName: string; lastName: string; email: string }>();
  bookingCreated = output<number>();

  initialCheckIn = input<Date | null>(null);
  initialCheckOut = input<Date | null>(null);
  initialGuests = input<number | null>(null);
  initialRoomTypeId = input<number | null>(null);

  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly amenityApi = inject(AmenityApiService);
  private readonly bookingApi = inject(BookingApiService);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 767px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  private initialRoomApplied = false;

  ngOnInit(): void {
    if (this.initialCheckIn() && this.initialCheckOut() && this.initialGuests()) {
      this.datesForm.patchValue({
        checkInDate: this.initialCheckIn(),
        checkOutDate: this.initialCheckOut(),
        guestCount: this.initialGuests() ?? 1
      });
      this.loadRooms();
    }
  }

  loading = signal(false);
  error = signal<string | null>(null);

  availableRooms = signal<AvailableRoomType[]>([]);
  availableAmenities = signal<Amenity[]>([]);
  selectedRoomQuantities = signal<Record<number, number>>({});

  // Forms definition
  datesForm = new FormGroup({
    checkInDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    checkOutDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    guestCount: new FormControl<number>(1, { validators: [Validators.required, Validators.min(1), Validators.max(20)], nonNullable: true })
  }, { validators: this.dateRangeValidator });

  roomsForm = new FormGroup({
    dummy: new FormControl<boolean>(false, { validators: [Validators.requiredTrue], nonNullable: true })
  });

  amenitiesForm = new FormGroup({
    selectedAmenities: new FormArray<FormControl<boolean>>([])
  });

  get amenityControls(): FormControl<boolean>[] {
    return (this.amenitiesForm.get('selectedAmenities') as FormArray).controls as FormControl<boolean>[];
  }

  getAmenityControl(index: number): FormControl<boolean> {
    return this.amenityControls[index];
  }

  // Convert form values to signals so computed reacts
  private datesValues = toSignal(this.datesForm.valueChanges, { initialValue: this.datesForm.value });
  private amenitiesValues = toSignal(this.amenitiesForm.valueChanges, { initialValue: this.amenitiesForm.value });

  // Computed signals
  nights = computed(() => {
    const dates = this.datesValues();
    if (!dates || !dates.checkInDate || !dates.checkOutDate) return 0;
    const cin = new Date(dates.checkInDate);
    const cout = new Date(dates.checkOutDate);
    return Math.max(0, Math.ceil((cout.getTime() - cin.getTime()) / (1000 * 3600 * 24)));
  });

  totalSelectedQuantity = computed(() => {
    return Object.values(this.selectedRoomQuantities()).reduce((a, b) => a + b, 0);
  });

  capacityWarning = computed(() => {
    const totalCap = this.availableRooms().reduce(
      (sum, r) => sum + (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.maxOccupancy,
      0
    );
    const dates = this.datesValues();
    const guests = dates?.guestCount ?? 0;
    if (this.totalSelectedQuantity() > 0 && totalCap < guests) {
      return `The selected rooms can only accommodate ${totalCap} guests. You need ${guests}.`;
    }
    return null;
  });

  selectedRoomEntries = computed(() => {
    const quantities = this.selectedRoomQuantities();
    return this.availableRooms()
      .filter(r => (quantities[r.roomTypeId] || 0) > 0)
      .map(r => ({
        roomTypeId: r.roomTypeId,
        name: r.name,
        basePrice: r.basePrice,
        maxOccupancy: r.maxOccupancy,
        quantity: quantities[r.roomTypeId]
      }));
  });

  selectedAmenityEntries = computed(() => {
    const list = this.availableAmenities();
    const amenitiesVal = this.amenitiesValues();
    const selectedList = amenitiesVal?.selectedAmenities || [];
    return list.filter((_, i) => selectedList[i] === true);
  });

  estimatedTotal = computed(() => {
    const amenitiesVal = this.amenitiesValues();
    const nights = this.nights();
    const roomCost = this.availableRooms().reduce(
      (sum, r) => sum + (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.basePrice * nights,
      0
    );
    const selectedList = amenitiesVal?.selectedAmenities || [];
    const amenityCost = this.availableAmenities().reduce(
      (sum, a, i) => sum + (selectedList[i] ? a.price : 0),
      0
    );
    return roomCost + amenityCost;
  });

  onStepChange(event: any): void {
    if (event.selectedIndex === 1) {
      this.loadRooms();
    } else if (event.selectedIndex === 2) {
      this.loadAmenities();
    }
  }

  loadRooms(): void {
    const cin = this.datesForm.value.checkInDate;
    const cout = this.datesForm.value.checkOutDate;
    if (!cin || !cout) return;

    this.loading.set(true);
    this.error.set(null);

    this.roomTypeApi.getAvailable(this.formatDate(cin), this.formatDate(cout))
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.availableRooms.set(res.data);
          // Pre-populate empty quantities
          const quantities: Record<number, number> = {};
          res.data.forEach(r => {
            quantities[r.roomTypeId] = 0;
          });
          this.selectedRoomQuantities.set(quantities);

          if (!this.initialRoomApplied && this.initialRoomTypeId()) {
            const room = res.data.find(r => r.roomTypeId === this.initialRoomTypeId());
            if (room && room.availableCount > 0) {
              this.selectedRoomQuantities.update(q => ({
                ...q,
                [room.roomTypeId]: 1
              }));
              this.initialRoomApplied = true;
            }
          }

          this.updateRoomsFormValidity();
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to load available rooms.';
          this.error.set(message);
        }
      });
  }

  loadAmenities(): void {
    this.loading.set(true);
    this.error.set(null);

    this.amenityApi.getAll({ pageNumber: 1, pageSize: 100, isAvailable: true })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.availableAmenities.set(res.data);
          const formArray = this.amenitiesForm.get('selectedAmenities') as FormArray;
          formArray.clear();
          res.data.forEach(() => {
            formArray.push(new FormControl<boolean>(false, { nonNullable: true }));
          });
          this.cdr.detectChanges();
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to load amenities.';
          this.error.set(message);
        }
      });
  }

  incrementRoom(roomTypeId: number): void {
    const current = this.selectedRoomQuantities();
    const limit = this.availableRooms().find(r => r.roomTypeId === roomTypeId)?.availableCount ?? 0;
    const val = current[roomTypeId] || 0;
    if (val < limit) {
      this.selectedRoomQuantities.set({
        ...current,
        [roomTypeId]: val + 1
      });
      this.updateRoomsFormValidity();
    }
  }

  decrementRoom(roomTypeId: number): void {
    const current = this.selectedRoomQuantities();
    const val = current[roomTypeId] || 0;
    if (val > 0) {
      this.selectedRoomQuantities.set({
        ...current,
        [roomTypeId]: val - 1
      });
      this.updateRoomsFormValidity();
    }
  }

  getRoomQuantity(roomTypeId: number): number {
    return this.selectedRoomQuantities()[roomTypeId] || 0;
  }

  updateRoomsFormValidity(): void {
    const isValid = this.totalSelectedQuantity() > 0 && !this.capacityWarning();
    this.roomsForm.controls.dummy.setValue(isValid);
    this.roomsForm.updateValueAndValidity();
  }

  submitBooking(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Booking',
        message: `Create this booking? Total estimated: $${this.estimatedTotal().toFixed(2)}`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.performBooking();
      }
    });
  }

  private performBooking(): void {
    this.loading.set(true);
    this.error.set(null);

    const roomTypeIds: number[] = [];
    const quantities = this.selectedRoomQuantities();
    Object.keys(quantities).forEach(key => {
      const typeId = Number(key);
      const qty = quantities[typeId] || 0;
      for (let i = 0; i < qty; i++) {
        roomTypeIds.push(typeId);
      }
    });

    const amenityIds = this.selectedAmenityEntries().map(a => a.id);
    const profile = this.userProfile();

    const bookingDto = {
      roomTypeIds,
      guestCount: this.datesForm.value.guestCount!,
      checkInDate: this.datesForm.value.checkInDate!.toISOString(),
      checkOutDate: this.datesForm.value.checkOutDate!.toISOString(),
      guestName: `${profile.firstName} ${profile.lastName}`,
      guestEmail: profile.email,
      amenityIds
    };

    this.bookingApi.create(bookingDto)
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (response) => {
          this.bookingCreated.emit(response.id);
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to confirm booking.';
          this.error.set(message);
        }
      });
  }

  formatDate(date: Date): string {
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
  }

  private dateRangeValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const cin = control.get('checkInDate')?.value as Date | null;
    const cout = control.get('checkOutDate')?.value as Date | null;
    if (!cin || !cout) return null;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const cinDate = new Date(cin);
    cinDate.setHours(0, 0, 0, 0);

    if (cinDate < today) {
      return { checkInInPast: true };
    }

    const coutDate = new Date(cout);
    coutDate.setHours(0, 0, 0, 0);

    if (coutDate <= cinDate) {
      return { checkOutBeforeCheckIn: true };
    }

    return null;
  }
}


# /Frontend/src/app/features/user/components/feedback-dialog/feedback-dialog.component.html

<h2 mat-dialog-title>Booking Feedback – Booking #{{ bookingId }}</h2>
<mat-dialog-content class="feedback-content">
  @if (loading()) {
    <div style="display: flex; justify-content: center; padding: 24px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      @if (!submitting()) {
        <button mat-button (click)="checkExistingFeedback()">Retry</button>
      }
    </app-alert>
  } @else if (existingFeedback()) {
    <div class="read-only-feedback">
      <p>Thank you for submitting feedback for this stay!</p>
      <p><strong>Rating:</strong> <span class="rating-value">{{ existingFeedback()!.rating }} / 5</span></p>
      <p><strong>Comments:</strong></p>
      <p style="white-space: pre-wrap; font-style: italic; background: #f9f9f9; padding: 12px; border-radius: 4px;">
        {{ existingFeedback()!.comments || 'No comments provided.' }}
      </p>
      <p style="font-size: 0.8rem; color: rgba(0,0,0,0.54);">Submitted on {{ existingFeedback()!.createdAt | date:'mediumDate' }}</p>
    </div>
  } @else {
    <form [formGroup]="feedbackForm">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Rating</mat-label>
        <mat-select formControlName="rating">
          <mat-option [value]="5">5 – Excellent</mat-option>
          <mat-option [value]="4">4 – Good</mat-option>
          <mat-option [value]="3">3 – Average</mat-option>
          <mat-option [value]="2">2 – Poor</mat-option>
          <mat-option [value]="1">1 – Terrible</mat-option>
        </mat-select>
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Comments (Optional)</mat-label>
        <textarea
          matInput
          formControlName="comments"
          rows="4"
          placeholder="Tell us about your stay..."
          maxlength="500"
        ></textarea>
        <mat-hint align="end">{{ feedbackForm.value.comments?.length || 0 }}/500</mat-hint>
      </mat-form-field>
    </form>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Cancel</button>
  @if (!existingFeedback() && !loading()) {
    <button
      mat-raised-button
      color="primary"
      [disabled]="feedbackForm.invalid || submitting()"
      (click)="submitFeedback()"
    >
      @if (submitting()) {
        <mat-spinner diameter="18" style="display: inline-block; margin-right: 8px;"></mat-spinner>
      }
      Submit
    </button>
  }
</mat-dialog-actions>


# /Frontend/src/app/features/user/components/feedback-dialog/feedback-dialog.component.ts

import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FeedbackApiService } from '../../services/feedback-api.service';
import { Feedback, CreateFeedbackDTO } from '../../models/feedback.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-feedback-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    AlertComponent
  ],
  templateUrl: './feedback-dialog.component.html',
  styles: [`
    .feedback-content {
      min-width: 320px;
      max-width: 480px;
    }
    .read-only-feedback {
      font-size: 1rem;
      line-height: 1.5;
    }
    .rating-value {
      font-size: 1.2rem;
      font-weight: 600;
      color: #3f51b5;
    }
    .full-width {
      width: 100%;
    }
  `]
})
export class FeedbackDialogComponent implements OnInit {
  readonly bookingId: number = inject(MAT_DIALOG_DATA);
  private readonly feedbackApi = inject(FeedbackApiService);
  private readonly dialogRef = inject(MatDialogRef<FeedbackDialogComponent>);

  existingFeedback = signal<Feedback | null>(null);
  loading = signal(false);
  submitting = signal(false);
  error = signal<string | null>(null);

  feedbackForm = new FormGroup({
    rating: new FormControl<number>(5, { validators: [Validators.required, Validators.min(1), Validators.max(5)], nonNullable: true }),
    comments: new FormControl<string>('', { nonNullable: true })
  });

  ngOnInit(): void {
    this.checkExistingFeedback();
  }

  checkExistingFeedback(): void {
    this.loading.set(true);
    this.error.set(null);

    this.feedbackApi.getByBookingId(this.bookingId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => {
          // If the backend returns 204 or null or empty object, existingFeedback will be null.
          if (data && data.id) {
            this.existingFeedback.set(data);
          } else {
            this.existingFeedback.set(null);
          }
        },
        error: (err) => {
          // A 404 might mean no feedback exists yet. Let's inspect status
          if (err.status === 404) {
            this.existingFeedback.set(null);
          } else {
            const message = err.error?.message || err.message || 'Error checking existing feedback.';
            this.error.set(message);
          }
        }
      });
  }

  submitFeedback(): void {
    if (this.feedbackForm.invalid) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    const dto: CreateFeedbackDTO = {
      bookingId: this.bookingId,
      rating: this.feedbackForm.value.rating!,
      comments: this.feedbackForm.value.comments ?? ''
    };

    this.feedbackApi.submit(dto)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (response) => {
          this.dialogRef.close(response);
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to submit feedback.';
          this.error.set(message);
        }
      });
  }
}


# /Frontend/src/app/features/user/components/food-order/cart-drawer.component.html

<div
  class="cart-drawer"
  [class.open]="isOpen()"
>
  <button
    mat-raised-button
    class="cart-toggle-btn"
    (click)="cartToggle.emit()"
    aria-label="Toggle shopping cart"
  >
    <mat-icon>shopping_cart</mat-icon>
    Cart ({{ itemCount() }}) – {{ subtotal() | currency }}
  </button>

  @if (isOpen()) {
    <div class="cart-panel">
      <div class="cart-header">
        <h3>Shopping Cart</h3>
        <button mat-icon-button (click)="cartToggle.emit()" aria-label="Close cart">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div class="cart-items-list">
        @for (item of cartItems(); track item.menuItemId) {
          <div class="cart-item">
            <span class="item-name">{{ item.name }}</span>
            <div class="qty-controls">
              <button type="button" mat-icon-button (click)="decrementQty(item.menuItemId)" aria-label="Decrease quantity">
                <mat-icon>remove</mat-icon>
              </button>
              <span class="qty">{{ item.quantity }}</span>
              <button type="button" mat-icon-button (click)="incrementQty(item.menuItemId)" aria-label="Increase quantity">
                <mat-icon>add</mat-icon>
              </button>
            </div>
            <span class="item-price">{{ item.price * item.quantity | currency }}</span>
          </div>
        } @empty {
          <p class="empty-cart">Your cart is empty.</p>
        }
      </div>

      <div class="cart-footer">
        <div class="total-row">
          <span>Total:</span>
          <span class="total-price">{{ subtotal() | currency }}</span>
        </div>
        <button
          mat-raised-button
          color="primary"
          class="checkout-btn"
          (click)="checkout.emit()"
          [disabled]="cartItems().length === 0"
        >
          Place Order
        </button>
      </div>
    </div>
  }
</div>


# /Frontend/src/app/features/user/components/food-order/cart-drawer.component.scss

.cart-drawer {
  position: relative;

  .cart-toggle-btn {
    width: 100%;
    height: 48px;
    font-weight: 500;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    border-radius: 8px;
    background-color: #f5f5f5;
    color: #333;
  }

  .cart-panel {
    position: absolute;
    top: 56px;
    right: 0;
    width: 320px;
    background: white;
    border: 1px solid rgba(0, 0, 0, 0.12);
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    z-index: 100;
    display: flex;
    flex-direction: column;
    max-height: 400px;
  }

  .cart-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 16px;
    border-bottom: 1px solid #f0f0f0;

    h3 {
      margin: 0;
      font-size: 1.1rem;
      font-weight: 500;
    }
  }

  .cart-items-list {
    flex-grow: 1;
    overflow-y: auto;
    padding: 16px;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .cart-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.95rem;
    gap: 8px;

    .item-name {
      color: rgba(0, 0, 0, 0.87);
      flex: 1 1 auto;
      min-width: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .qty-controls {
      display: flex;
      align-items: center;
      gap: 4px;
      flex-shrink: 0;

      button {
        width: 28px;
        height: 28px;
        line-height: 28px;
        display: flex;
        align-items: center;
        justify-content: center;

        ::ng-deep .mat-mdc-button-touch-target {
          display: none;
        }

        mat-icon {
          font-size: 18px;
          width: 18px;
          height: 18px;
        }
      }

      .qty {
        font-weight: 500;
        min-width: 16px;
        text-align: center;
      }
    }
    .item-price {
      font-weight: 500;
      flex-shrink: 0;
      min-width: 60px;
      text-align: right;
    }
  }

  .empty-cart {
    text-align: center;
    color: rgba(0, 0, 0, 0.54);
    font-style: italic;
    margin: 16px 0;
  }

  .cart-footer {
    padding: 16px;
    border-top: 1px solid #f0f0f0;
    background: #fafafa;
    border-bottom-left-radius: 8px;
    border-bottom-right-radius: 8px;

    .total-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-weight: 600;
      font-size: 1.05rem;
      margin-bottom: 12px;

      .total-price {
        color: #3f51b5;
        font-size: 1.1rem;
      }
    }

    .checkout-btn {
      width: 100%;
    }
  }
}

// Mobile bottom sheet styles
@media (max-width: 767px) {
  .cart-drawer {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    background: white;
    box-shadow: 0 -4px 16px rgba(0, 0, 0, 0.15);
    border-top-left-radius: 16px;
    border-top-right-radius: 16px;
    z-index: 1000;
    padding: 12px 16px;
    display: flex;
    flex-direction: column;

    .cart-toggle-btn {
      margin-bottom: 8px;
    }

    &.open {
      height: 70vh;
    }

    .cart-panel {
      position: static;
      width: 100%;
      box-shadow: none;
      border: none;
      flex-grow: 1;
      max-height: none;
      display: flex;
    }

    .cart-items-list {
      max-height: calc(70vh - 180px);
    }
  }
}


# /Frontend/src/app/features/user/components/food-order/cart-drawer.component.ts

import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { OrderItem } from '../../models/order-item.model';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './cart-drawer.component.html',
  styleUrls: ['./cart-drawer.component.scss']
})
export class CartDrawerComponent {
  cartItems = input.required<OrderItem[]>();
  isOpen = input.required<boolean>();

  cartToggle = output<void>();
  checkout = output<void>();
  updateQuantity = output<{ menuItemId: number; delta: number }>();

  itemCount = computed(() => this.cartItems().reduce((s, i) => s + i.quantity, 0));
  subtotal = computed(() => this.cartItems().reduce((s, i) => s + i.price * i.quantity, 0));

  incrementQty(menuItemId: number): void {
    this.updateQuantity.emit({ menuItemId, delta: 1 });
  }

  decrementQty(menuItemId: number): void {
    this.updateQuantity.emit({ menuItemId, delta: -1 });
  }
}


# /Frontend/src/app/features/user/components/food-order/food-order.component.html

<div class="food-order-container">
  @if (loading()) {
    <div class="spinner-container">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchMenuItems()">Retry</button>
    </app-alert>
  } @else {
    <div class="food-order-layout">
      <div class="menu-section">
        <mat-form-field appearance="outline" style="width: 100%; max-width: 300px; margin-bottom: 16px; display: block;">
          <mat-label>Deliver to Room</mat-label>
          <mat-select [formControl]="selectedRoomId">
            @for (room of validRooms(); track room.roomId) {
              <mat-option [value]="room.roomId">
                {{ room.roomNumber ?? 'Room ' + room.roomId }}
              </mat-option>
            }
          </mat-select>
          @if (selectedRoomId.invalid && selectedRoomId.touched) {
            <mat-error>Please select a room for delivery.</mat-error>
          }
        </mat-form-field>

        <app-menu-grid
          [menuItems]="menuItems()"
          [cartItems]="cartItems()"
          (addToCart)="onAddToCart($event)"
          (updateQuantity)="onUpdateCartQty($event)"
        />
      </div>

      <div class="cart-section">
        <app-cart-drawer
          [cartItems]="cartItems()"
          [isOpen]="cartOpen()"
          (cartToggle)="cartOpen.set(!cartOpen())"
          (checkout)="placeOrder()"
          (updateQuantity)="onUpdateCartQty($event)"
        />
      </div>
    </div>
  }
</div>


# /Frontend/src/app/features/user/components/food-order/food-order.component.scss

.food-order-container {
  padding: 16px 0;

  .spinner-container {
    display: flex;
    justify-content: center;
    padding: 32px;
  }

  .food-order-layout {
    display: flex;
    gap: 24px;
    align-items: flex-start;
  }

  .menu-section {
    flex-grow: 1;
  }

  .cart-section {
    width: 320px;
    flex-shrink: 0;
    position: sticky;
    top: 24px;
  }
}

@media (max-width: 1024px) {
  .food-order-container {
    .food-order-layout {
      flex-direction: column;
      align-items: stretch;
    }

    .cart-section {
      width: 100%;
      position: static;
    }
  }
}

@media (max-width: 767px) {
  .food-order-container {
    padding-bottom: 72px; /* make space for fixed bottom sheet cart drawer */
  }
}


# /Frontend/src/app/features/user/components/food-order/food-order.component.ts

import { Component, OnInit, inject, signal, computed, input, output, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MenuItemApiService } from '../../services/menu-item-api.service';
import { OrderApiService } from '../../services/order-api.service';
import { MenuGridComponent } from './menu-grid.component';
import { CartDrawerComponent } from './cart-drawer.component';
import { MenuItem } from '../../../../features/admin/models/menu-item.model';
import { BookingRoom } from '../../../../features/admin/models/booking.model';
import { OrderItem } from '../../models/order-item.model';
import { finalize } from 'rxjs/operators';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

@Component({
  selector: 'app-food-order',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MenuGridComponent,
    CartDrawerComponent,
    AlertComponent
  ],
  templateUrl: './food-order.component.html',
  styleUrls: ['./food-order.component.scss']
})
export class FoodOrderComponent implements OnInit {
  activeBookingId = input.required<number>();
  rooms = input.required<BookingRoom[]>();
  orderPlaced = output<void>();

  private readonly menuApi = inject(MenuItemApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  selectedRoomId = new FormControl<number>(0, { nonNullable: true, validators: Validators.required });

  menuItems = signal<MenuItem[]>([]);
  cartItems = signal<OrderItem[]>([]);
  cartOpen = signal(false);

  loading = signal(false);
  error = signal<string | null>(null);
  submitting = signal(false);

  validRooms = computed(() => this.rooms().filter((r): r is typeof r & { roomId: number } => r.roomId !== null));
  canCheckout = computed(() => this.cartItems().length > 0);
  subtotal = computed(() => this.cartItems().reduce((s, i) => s + i.price * i.quantity, 0));

  ngOnInit(): void {
    this.fetchMenuItems();
    const roomsList = this.validRooms();
    if (roomsList.length > 0) {
      this.selectedRoomId.setValue(roomsList[0].roomId);
    }
  }

  fetchMenuItems(): void {
    this.loading.set(true);
    this.error.set(null);

    this.menuApi.getAll({ isAvailable: true, pageSize: 200 })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (res) => this.menuItems.set(res.data),
        error: (err) => {
          const msg = err.error?.message || err.message || 'Failed to load menu items.';
          this.error.set(msg);
        }
      });
  }

  onAddToCart(item: MenuItem): void {
    this.cartItems.update((items) => {
      const idx = items.findIndex((i) => i.menuItemId === item.id);
      if (idx > -1) {
        const updated = [...items];
        updated[idx] = {
          ...updated[idx],
          quantity: updated[idx].quantity + 1
        };
        return updated;
      } else {
        return [...items, { menuItemId: item.id, name: item.name, price: item.price, quantity: 1 }];
      }
    });

    const snackRef = this.snackBar.open(`Added ${item.name} to cart.`, 'View Cart', {
      duration: 4000
    });

    snackRef.onAction().subscribe(() => {
      this.cartOpen.set(true);
    });
  }

  onUpdateCartQty(event: { menuItemId: number; delta: number }): void {
    this.cartItems.update(items => {
      const index = items.findIndex(i => i.menuItemId === event.menuItemId);
      if (index === -1) return items;
      const newQty = items[index].quantity + event.delta;
      if (newQty <= 0) {
        return items.filter(i => i.menuItemId !== event.menuItemId);
      }
      return items.map(i => i.menuItemId === event.menuItemId ? { ...i, quantity: newQty } : i);
    });
  }

  placeOrder(): void {
    if (!this.canCheckout() || this.submitting()) {
      return;
    }
    if (this.selectedRoomId.invalid) {
      this.selectedRoomId.markAsTouched();
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Order',
        message: `Place this order? Total: $${this.subtotal().toFixed(2)}`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.submitOrder();
      }
    });
  }

  private submitOrder(): void {
    if (this.selectedRoomId.invalid) {
      this.selectedRoomId.markAsTouched();
      return;
    }
    this.submitting.set(true);
    const dto = {
      bookingId: this.activeBookingId(),
      roomId: this.selectedRoomId.value,
      items: this.cartItems().map((i) => ({
        menuItemId: i.menuItemId,
        quantity: i.quantity
      }))
    };

    this.orderApi.create(dto)
      .pipe(
        finalize(() => this.submitting.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Order placed successfully!', 'Close', { duration: 4000 });
          this.cartItems.set([]);
          this.cartOpen.set(false);
          this.orderPlaced.emit();
        },
        error: (err) => {
          const msg = typeof err.error === 'string' ? err.error : (err.error?.message || 'Failed to place order.');
          this.snackBar.open(msg, 'Close', { duration: 5000 });
        }
      });
  }
}


# /Frontend/src/app/features/user/components/food-order/menu-grid.component.html

<div class="menu-categories">
  <div class="filter-row">
    <mat-form-field appearance="outline" class="category-select">
      <mat-label>Category</mat-label>
      <mat-select [formControl]="categoryFilter">
        <mat-option value="All">All</mat-option>
        @for (cat of categories(); track cat) {
          <mat-option [value]="cat">{{ cat }}</mat-option>
        }
      </mat-select>
    </mat-form-field>
  </div>

  @for (group of filteredGroups(); track group.category) {
    <div class="category-section">
      <h3 class="category-title">{{ group.category }}</h3>
      <div class="menu-grid">
        @for (item of group.items; track item.id) {
          <mat-card class="menu-item-card">
            <mat-card-header>
              <mat-card-title>{{ item.name }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="price-row">
                <span class="price">{{ item.price | currency }}</span>
              </div>
            </mat-card-content>
            <mat-card-actions>
              @if (getQuantity(item.id) === 0) {
                <button
                  mat-raised-button
                  color="primary"
                  (click)="increment(item)"
                  aria-label="Add to cart"
                  class="action-btn"
                >
                  <mat-icon>add_shopping_cart</mat-icon> Add to Cart
                </button>
              } @else {
                <div class="inline-qty-controls">
                  <button
                    type="button"
                    mat-icon-button
                    (click)="decrement(item)"
                    aria-label="Decrease quantity"
                  >
                    <mat-icon>remove</mat-icon>
                  </button>
                  <span class="qty-display">{{ getQuantity(item.id) }}</span>
                  <button
                    type="button"
                    mat-icon-button
                    (click)="increment(item)"
                    aria-label="Increase quantity"
                  >
                    <mat-icon>add</mat-icon>
                  </button>
                </div>
              }
            </mat-card-actions>
          </mat-card>
        }
      </div>
    </div>
  } @empty {
    <p class="no-items">No menu items available at the moment.</p>
  }
</div>


# /Frontend/src/app/features/user/components/food-order/menu-grid.component.scss

.menu-categories {
  .category-section {
    margin-bottom: 32px;
  }

  .category-title {
    font-size: 1.4rem;
    font-weight: 500;
    margin: 16px 0 12px;
    padding-bottom: 8px;
    border-bottom: 2px solid #e0e0e0;
    color: rgba(0, 0, 0, 0.87);
  }

  .filter-row {
    margin-bottom: 24px;
    
    .category-select {
      width: 200px;
      @media (max-width: 599px) {
        width: 100%;
      }
    }
  }

  .menu-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 16px;
    padding: 8px 0;

    .menu-item-card {
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      height: 100%;

      mat-card-header {
        margin-bottom: 8px;
      }

      .price-row {
        font-size: 1.2rem;
        font-weight: 600;
        color: #3f51b5;
      }

      mat-card-actions {
        padding: 8px 16px 16px 16px;

        .action-btn {
          width: 100%;
        }

        .inline-qty-controls {
          display: flex;
          align-items: center;
          justify-content: space-between;
          width: 100%;
          border: 1px solid rgba(0, 0, 0, 0.12);
          border-radius: 4px;
          height: 36px;
          box-sizing: border-box;

          button {
            width: 36px;
            height: 36px;
            display: flex;
            align-items: center;
            justify-content: center;
            line-height: 36px;
            border-radius: 0;

            ::ng-deep .mat-mdc-button-touch-target {
              display: none;
            }
          }

          .qty-display {
            font-weight: 600;
            font-size: 1rem;
            color: rgba(0, 0, 0, 0.87);
          }
        }
      }
    }
  }

  .no-items {
    text-align: center;
    padding: 32px;
    color: rgba(0, 0, 0, 0.54);
    font-style: italic;
  }
}

@media (max-width: 1024px) {
  .menu-categories {
    .menu-grid {
      grid-template-columns: repeat(2, 1fr);
    }
  }
}

@media (max-width: 767px) {
  .menu-categories {
    .menu-grid {
      grid-template-columns: 1fr;
    }
  }
}


# /Frontend/src/app/features/user/components/food-order/menu-grid.component.ts

import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MenuItem } from '../../../../features/admin/models/menu-item.model';
import { OrderItem } from '../../models/order-item.model';

@Component({
  selector: 'app-menu-grid',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule
  ],
  templateUrl: './menu-grid.component.html',
  styleUrls: ['./menu-grid.component.scss']
})
export class MenuGridComponent {
  menuItems = input.required<MenuItem[]>();
  cartItems = input<OrderItem[]>([]);
  addToCart = output<MenuItem>();
  updateQuantity = output<{ menuItemId: number; delta: number }>();

  categoryFilter = new FormControl('All', { nonNullable: true });
  private categoryFilterSignal = toSignal(this.categoryFilter.valueChanges, { initialValue: this.categoryFilter.value });

  cartMap = computed(() => {
    const map: Record<number, number> = {};
    const items = this.cartItems() || [];
    for (const item of items) {
      map[item.menuItemId] = item.quantity;
    }
    return map;
  });

  getQuantity(menuItemId: number): number {
    return this.cartMap()[menuItemId] || 0;
  }

  increment(item: MenuItem): void {
    const current = this.getQuantity(item.id);
    if (current === 0) {
      this.addToCart.emit(item);
    } else {
      this.updateQuantity.emit({ menuItemId: item.id, delta: 1 });
    }
  }

  decrement(item: MenuItem): void {
    const current = this.getQuantity(item.id);
    if (current > 0) {
      this.updateQuantity.emit({ menuItemId: item.id, delta: -1 });
    }
  }

  categories = computed(() => {
    const cats = new Set(this.menuItems().map(i => i.category || 'Other'));
    return Array.from(cats).sort();
  });

  filteredGroups = computed(() => {
    const selected = this.categoryFilterSignal() ?? 'All';
    const items = selected === 'All' 
      ? this.menuItems() 
      : this.menuItems().filter(i => (i.category || 'Other') === selected);
    
    const groups: Record<string, MenuItem[]> = {};
    for (const item of items) {
      const cat = item.category || 'Other';
      if (!groups[cat]) groups[cat] = [];
      groups[cat].push(item);
    }
    return Object.entries(groups).map(([category, items]) => ({ category, items }));
  });
}


# /Frontend/src/app/features/user/components/my-requests/my-requests.component.html

<div class="my-requests">
  @if (loading() && requests().length === 0) {
    <div class="spinner-container">
      <mat-spinner diameter="30"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert
      type="error"
      [message]="error()!"
      (closed)="error.set(null)"
    >
      <button
        mat-button
        (click)="fetchRequests()"
      >
        Retry
      </button>
    </app-alert>
  }

  @if (requests().length > 0) {
    <div class="table-container">
      <table
        mat-table
        [dataSource]="requests()"
        matSort
        matSortDisableClear
      >
        <!-- Type Column -->
        <ng-container matColumnDef="type">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Type
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            {{ r.type }}
          </td>
        </ng-container>

        <!-- Room Column -->
        <ng-container matColumnDef="room">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Room
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            {{ r.roomNumber }}
          </td>
        </ng-container>

        <!-- Description Column -->
        <ng-container matColumnDef="description">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Description
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            {{ r.description }}
          </td>
        </ng-container>

        <!-- Status Column -->
        <ng-container matColumnDef="status">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Status
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            <span class="status-badge" [class]="r.status.toLowerCase()">
              {{ r.status }}
            </span>
          </td>
        </ng-container>

        <!-- Created Column -->
        <ng-container matColumnDef="createdAt">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Created
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            {{ r.createdAt | date:'short' }}
          </td>
        </ng-container>

        <tr
          mat-header-row
          *matHeaderRowDef="displayedColumns"
        ></tr>
        <tr
          mat-row
          *matRowDef="let row; columns: displayedColumns"
        ></tr>
      </table>
    </div>
  } @else if (!loading()) {
    <p class="no-requests">No housekeeping, maintenance, or food order requests found.</p>
  }
</div>


# /Frontend/src/app/features/user/components/my-requests/my-requests.component.scss

.my-requests {
  padding: 16px 0;

  .spinner-container {
    display: flex;
    justify-content: center;
    padding: 24px;
  }

  .table-container {
    overflow-x: auto;
    width: 100%;
    border: 1px solid rgba(0, 0, 0, 0.12);
    border-radius: 4px;
  }

  table {
    width: 100%;
    border-collapse: collapse;

    .clickable-row {
      cursor: pointer;
      transition: background-color 0.2s ease;
      &:hover {
        background-color: rgba(0, 0, 0, 0.04);
      }
    }
  }

  .status-badge {
    display: inline-block;
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 0.85rem;
    font-weight: 500;

    &.pending {
      background-color: #fff3e0;
      color: #e65100;
    }

    &.inprogress {
      background-color: #e8eaf6;
      color: #1a237e;
    }

    &.completed {
      background-color: #e8f5e9;
      color: #1b5e20;
    }
  }

  .no-requests {
    padding: 24px;
    text-align: center;
    color: rgba(0, 0, 0, 0.54);
    font-size: 1.1rem;
    font-style: italic;
  }
}

@media (max-width: 599px) {
  .my-requests {
    overflow-x: auto;
    table {
      min-width: 600px;
    }
    .mat-mdc-cell, .mat-mdc-header-cell {
      padding: 8px 4px;
      font-size: 0.85rem;
    }
  }
}


# /Frontend/src/app/features/user/components/my-requests/my-requests.component.ts

import { Component, input, effect, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, Observable, of } from 'rxjs';
import { map, finalize, catchError, switchMap } from 'rxjs/operators';
import { HousekeepingApiService } from '../../services/housekeeping-api.service';
import { MaintenanceApiService } from '../../services/maintenance-api.service';
import { OrderApiService } from '../../services/order-api.service';
import { CustomerRequest } from '../../models/customer-request.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

@Component({
  selector: 'app-my-requests',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    AlertComponent
  ],
  templateUrl: './my-requests.component.html',
  styleUrls: ['./my-requests.component.scss']
})
export class MyRequestsComponent {
  roomIds = input.required<number[]>();
  bookingId = input<number | null>(null);
  refresh = input(0);

  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly destroyRef = inject(DestroyRef);

  requests = signal<CustomerRequest[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  displayedColumns = ['type', 'room', 'description', 'status', 'createdAt'];

  constructor() {
    effect(() => {
      // Trigger fetch when roomIds or refresh trigger changes
      const ids = this.roomIds();
      const ref = this.refresh();
      if (ids && ids.length > 0) {
        this.fetchRequests();
      } else {
        this.requests.set([]);
      }
    });
  }

  fetchRequests(): void {
    const ids = this.roomIds();
    if (ids.length === 0) {
      this.requests.set([]);
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const obsList: Observable<CustomerRequest[]>[] = [];
    ids.forEach((roomId) => {
      obsList.push(
        this.housekeepingApi.getAll({ pageSize: 100 }).pipe(
          map((res) =>
            res.data
              .filter((hk) => hk.roomId === roomId)
              .map((hk) => ({
                id: hk.id,
                type: 'Housekeeping' as const,
                roomId: hk.roomId,
                roomNumber: hk.location ?? `Room ${hk.roomId}`,
                description: hk.description ?? '',
                status: hk.status,
                createdAt: hk.createdAt
              }))
          ),
          catchError(() => of([]))
        )
      );
      obsList.push(
        this.maintenanceApi.getAll({ pageSize: 100 }).pipe(
          map((res) =>
            res.data
              .filter((m) => m.roomId === roomId)
              .map((m) => ({
                id: m.id,
                type: 'Maintenance' as const,
                roomId: m.roomId,
                roomNumber: m.location ?? `Room ${m.roomId}`,
                description: m.description ?? '',
                status: m.status,
                createdAt: m.createdAt
              }))
          ),
          catchError(() => of([]))
        )
      );
    });

    // Add food orders if bookingId is available
    const bId = this.bookingId();
    if (bId != null) {
      const food$ = this.orderApi.getAll({ status: 'Pending', pageSize: 50 }).pipe(
        switchMap((res: any) =>
          this.orderApi.getAll({ status: 'Preparing', pageSize: 50 }).pipe(
            map((res2: any) =>
              [...res.data, ...res2.data]
                .filter((o: any) => o.bookingId === bId)
                .map((o: any) => ({
                  id: o.id,
                  type: 'Food Order' as const,
                  roomId: o.roomId ?? 0,
                  roomNumber: o.roomNumber ?? 'N/A',
                  description: `Order #${o.id}`,
                  status: o.orderStatus ?? 'Pending',
                  createdAt: o.generatedAt ?? new Date().toISOString()
                }))
            )
          )
        ),
        catchError(() => of([]))
      );
      obsList.push(food$);
    }

    forkJoin(obsList)
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (results) => {
          const merged = results.reduce((acc, curr) => acc.concat(curr), []);
          // Sort by createdAt descending
          merged.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
          this.requests.set(merged);
        },
        error: (err) => {
          const msg = err.error?.message || err.message || 'Failed to fetch requests.';
          this.error.set(msg);
        }
      });
  }
}


# /Frontend/src/app/features/user/components/request-service-dialog.component.html

<div class="dialog-container">
  <h2 mat-dialog-title>
    {{ data.type === 'housekeeping' ? 'Request Housekeeping' : 'Request Maintenance' }}
  </h2>
  <mat-dialog-content>
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>Room</mat-label>
      <input matInput [value]="data.roomNumber" readonly aria-label="Room number (read only)" />
    </mat-form-field>
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>Description</mat-label>
      <textarea
        matInput
        [formControl]="descriptionControl"
        rows="4"
        placeholder="Describe your request..."
        aria-label="Request description"
      ></textarea>
      @if (descriptionControl.invalid && descriptionControl.touched) {
        <mat-error>Description is required.</mat-error>
      }
    </mat-form-field>
  </mat-dialog-content>
  <mat-dialog-actions align="end">
    <button mat-button mat-dialog-close aria-label="Cancel request">Cancel</button>
    <button mat-raised-button color="primary" (click)="submit()" aria-label="Submit request">Submit</button>
  </mat-dialog-actions>
</div>


# /Frontend/src/app/features/user/components/request-service-dialog.component.ts

import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface RequestServiceDialogData {
  roomNumber: string;
  roomId: number;
  type: 'housekeeping' | 'maintenance';
}

export interface RequestServiceDialogResult {
  description: string;
}

@Component({
  selector: 'app-request-service-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './request-service-dialog.component.html',
})
export class RequestServiceDialogComponent {
  readonly data: RequestServiceDialogData = inject(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<RequestServiceDialogComponent>);

  descriptionControl = new FormControl<string>('', { validators: [Validators.required], nonNullable: true });

  submit(): void {
    this.descriptionControl.markAsTouched();
    if (this.descriptionControl.invalid) {
      return;
    }
    const result: RequestServiceDialogResult = { description: this.descriptionControl.value };
    this.dialogRef.close(result);
  }
}


# /Frontend/src/app/features/user/components/request-service/request-service.component.html

<div class="request-service">
  <mat-card class="request-card">
    <mat-card-header>
      <mat-card-title>Request Housekeeping or Maintenance</mat-card-title>
    </mat-card-header>
    <mat-card-content class="form-container">
      <div class="form-row">
        @if (isMobile()) {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Service Type</mat-label>
            <mat-select [formControl]="requestType">
              <mat-option value="housekeeping">Housekeeping</mat-option>
              <mat-option value="maintenance">Maintenance</mat-option>
            </mat-select>
          </mat-form-field>
        } @else {
          <mat-button-toggle-group
            [formControl]="requestType"
            aria-label="Service type"
            class="type-toggle-group"
          >
            <mat-button-toggle value="housekeeping">
              <mat-icon>cleaning_services</mat-icon> Housekeeping
            </mat-button-toggle>
            <mat-button-toggle value="maintenance">
              <mat-icon>build</mat-icon> Maintenance
            </mat-button-toggle>
          </mat-button-toggle-group>
        }
      </div>

      <div class="form-row">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Room</mat-label>
          <mat-select [formControl]="selectedRoomId">
            @for (room of activeBooking().rooms; track room.roomId) {
              <mat-option [value]="room.roomId">
                {{ room.roomNumber ?? 'Room ' + room.roomId }}
              </mat-option>
            }
          </mat-select>
        </mat-form-field>
      </div>

      <div class="form-row">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea
            matInput
            [formControl]="description"
            rows="3"
            placeholder="Please detail your request..."
          ></textarea>
          @if (description.invalid && description.touched) {
            <mat-error>Description is required (minimum 5 characters).</mat-error>
          }
        </mat-form-field>
      </div>
    </mat-card-content>
    <mat-card-actions>
      <button
        mat-raised-button
        color="primary"
        (click)="submitRequest()"
        [disabled]="description.invalid || submitting()"
      >
        @if (submitting()) {
          <mat-spinner diameter="20" style="display: inline-block; margin-right: 8px;"></mat-spinner>
        }
        Submit Request
      </button>
    </mat-card-actions>
  </mat-card>
</div>


# /Frontend/src/app/features/user/components/request-service/request-service.component.scss

.request-service {
  padding: 16px 0;
  max-width: 600px;
  margin: 0 auto;

  .request-card {
    padding: 16px;
  }

  .form-container {
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-top: 16px;
  }

  .type-toggle-group {
    width: 100%;
    display: flex;

    mat-button-toggle {
      flex: 1;
      text-align: center;
    }
  }

  .full-width {
    width: 100%;
  }

  mat-card-actions {
    justify-content: flex-end;
    padding: 8px 16px 16px 16px;
  }
}

@media (max-width: 599px) {
  .request-service {
    mat-card {
      margin: 8px;
      padding: 12px;
    }
    mat-form-field {
      width: 100%;
    }
    mat-button-toggle-group {
      width: 100%;
      display: flex;
      mat-button-toggle {
        flex: 1 1 50%;
      }
    }
  }
}


# /Frontend/src/app/features/user/components/request-service/request-service.component.ts

import { Component, OnInit, inject, signal, input, output, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { HousekeepingApiService } from '../../services/housekeeping-api.service';
import { MaintenanceApiService } from '../../services/maintenance-api.service';
import { Booking } from '../../../../features/admin/models/booking.model';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-request-service',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonToggleModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './request-service.component.html',
  styleUrls: ['./request-service.component.scss']
})
export class RequestServiceComponent implements OnInit {
  activeBooking = input.required<Booking>();
  requestCreated = output<void>();

  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 599px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  requestType = new FormControl<'housekeeping' | 'maintenance'>('housekeeping', { nonNullable: true });
  selectedRoomId = new FormControl<number>(0, { nonNullable: true, validators: [Validators.required] });
  description = new FormControl<string>('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(5)]
  });

  submitting = signal(false);

  ngOnInit(): void {
    const rooms = this.activeBooking().rooms || [];
    if (rooms.length > 0 && rooms[0].roomId != null) {
      this.selectedRoomId.setValue(rooms[0].roomId);
    }
  }

  submitRequest(): void {
    if (this.description.invalid || this.submitting()) {
      this.description.markAsTouched();
      return;
    }

    const roomId = this.selectedRoomId.value;
    if (!roomId) return;

    const room = this.activeBooking().rooms.find(r => r.roomId === roomId);
    const roomLabel = room?.roomNumber ?? 'selected room';
    const typeLabel = this.requestType.value === 'housekeeping' ? 'Housekeeping' : 'Maintenance';

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Service Request',
        message: `Send a ${typeLabel} request for ${roomLabel}?`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.performSubmit(roomId);
      }
    });
  }

  private performSubmit(roomId: number): void {
    this.submitting.set(true);
    const type = this.requestType.value;
    const desc = this.description.value;

    const request$ = type === 'housekeeping'
      ? this.housekeepingApi.trigger(roomId, { description: desc })
      : this.maintenanceApi.trigger(roomId, { description: desc });

    request$.pipe(
      finalize(() => this.submitting.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open(`${type === 'housekeeping' ? 'Housekeeping' : 'Maintenance'} request submitted successfully.`, 'Close', {
          duration: 4000
        });
        this.description.reset('');
        this.requestCreated.emit();
      },
      error: (err) => {
        const msg = err.error?.message || err.message || 'Failed to submit request.';
        this.snackBar.open(msg, 'Close', { duration: 5000 });
      }
    });
  }
}


# /Frontend/src/app/features/user/facades/customer-booking.facade.ts

import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingApiService } from '../services/booking-api.service';
import { Booking } from '../../../features/admin/models/booking.model';

export interface CustomerProfile {
  firstName: string;
  lastName: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class CustomerBookingFacade {
  private readonly authApi = inject(AuthApiService);
  private readonly bookingApi = inject(BookingApiService);

  getActiveBooking(): Observable<Booking | null> {
    return this.authApi.getMe().pipe(
      switchMap((me) => {
        const email =
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '';
        if (!email) return of(null);
        return this.bookingApi
          .getAll({
            guestQuery: email,
            status: 'CheckedIn',
            pageNumber: 1,
            pageSize: 1,
            sortBy: 'bookedAt',
            sortDescending: true
          })
          .pipe(map((res) => (res.data.length > 0 ? res.data[0] : null)));
      })
    );
  }

  getCurrentCustomerProfile(): Observable<CustomerProfile> {
    return this.authApi.getMe().pipe(
      map((me) => ({
        firstName:
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname')?.value ??
          '',
        lastName:
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname')?.value ?? '',
        email:
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '',
      }))
    );
  }
}


# /Frontend/src/app/features/user/models/auth-me-response.model.ts

export type { Claim, AuthMeResponse } from '../../../core/models/auth-me-response.model';


# /Frontend/src/app/features/user/models/available-room-type.model.ts

export interface AvailableRoomType {
  roomTypeId: number;
  name: string;
  basePrice: number;
  maxOccupancy: number;
  description: string | null;
  imageUrls: string[] | null;
  squareFootage: number | null;
  bedConfiguration: Record<string, number> | null;
  availableCount: number;
}


# /Frontend/src/app/features/user/models/billing-folio.model.ts

export interface BillingFolio {
  bookingId: number;
  guestName: string;
  nightsStayed: number;
  roomBasePrice: number;
  roomTotal: number;
  foodTotal: number;
  amenityTotal: number;
  totalBill: number;
  paymentStatus: string;
  foodItems: string[];
  amenityItems: string[];
}


# /Frontend/src/app/features/user/models/booking.model.ts

export type { Booking, BookingRoom } from '../../../features/admin/models/booking.model';


# /Frontend/src/app/features/user/models/customer-request.model.ts

export interface CustomerRequest {
  id: number;
  type: 'Housekeeping' | 'Maintenance' | 'Food Order';
  roomId: number;
  roomNumber: string;
  description: string;
  status: string;
  createdAt: string;
}


# /Frontend/src/app/features/user/models/feedback.model.ts

export type { Feedback } from '../../../features/admin/models/feedback.model';

export interface CreateFeedbackDTO {
  bookingId: number;
  rating: number;
  comments: string;
}


# /Frontend/src/app/features/user/models/order-item.model.ts

export interface OrderItem {
  menuItemId: number;
  name: string;
  price: number;
  quantity: number;
}


# /Frontend/src/app/features/user/pages/bookings.component.html

<div class="bookings-page">
  <div class="toggle-row">
    <mat-button-toggle-group
      [formControl]="viewMode"
      aria-label="View"
    >
      <mat-button-toggle value="new">New Booking</mat-button-toggle>
      <mat-button-toggle value="history">My Bookings</mat-button-toggle>
    </mat-button-toggle-group>
  </div>

  @if (userProfile()) {
    @if (viewMode.value === 'history') {
      <app-booking-history
        [userEmail]="userEmail()"
        [refresh]="refreshTrigger()"
        [highlightBookingId]="newBookingId()"
      />
    } @if (viewMode.value === 'new') {
      <app-booking-wizard
        [userProfile]="userProfile()!"
        [initialCheckIn]="initialCheckIn"
        [initialCheckOut]="initialCheckOut"
        [initialGuests]="initialGuests"
        [initialRoomTypeId]="initialRoomTypeId"
        (bookingCreated)="onBookingCreated($event)"
      />
    }
  } @else {
    <div style="display: flex; justify-content: center; padding: 24px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  }
</div>


# /Frontend/src/app/features/user/pages/bookings.component.scss

.bookings-page {
  padding: 24px;

  .toggle-row {
    margin-bottom: 24px;

    mat-button-toggle-group {
      border-radius: 24px;
      overflow: hidden;
      border: 1px solid rgba(0, 0, 0, 0.12);

      .mat-button-toggle {
        border-radius: 0;
        border: none;
      }

      ::ng-deep .mat-button-toggle-checked {
        background-color: #1976d2 !important; // primary color
        color: white !important;
      }
    }
  }
}


# /Frontend/src/app/features/user/pages/bookings.component.ts

import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingHistoryComponent } from '../components/booking-history/booking-history.component';
import { BookingWizardComponent } from '../components/booking-wizard/booking-wizard.component';

@Component({
  selector: 'app-customer-bookings',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonToggleModule,
    MatProgressSpinnerModule,
    BookingHistoryComponent,
    BookingWizardComponent
  ],
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss']
})
export class BookingsComponent implements OnInit {
  viewMode = new FormControl<'history' | 'new'>('new', { nonNullable: true });
  userEmail = signal('');
  userProfile = signal<{ firstName: string; lastName: string; email: string } | null>(null);

  refreshTrigger = signal(0);
  newBookingId = signal<number | null>(null);

  initialCheckIn: Date | null = null;
  initialCheckOut: Date | null = null;
  initialGuests: number | null = null;
  initialRoomTypeId: number | null = null;

  private readonly authApi = inject(AuthApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['new'] === 'true') {
        this.viewMode.setValue('new'); // switch to new booking view
        // Set pre‑fill values
        this.initialCheckIn = params['checkIn'] ? new Date(params['checkIn']) : null;
        this.initialCheckOut = params['checkOut'] ? new Date(params['checkOut']) : null;
        this.initialGuests = params['guests'] ? +params['guests'] : null;
        this.initialRoomTypeId = params['roomTypeId'] ? +params['roomTypeId'] : null;
      }
    });

    this.authApi.getMe().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(me => {
      const given = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname')?.value ?? '';
      const surname = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname')?.value ?? '';
      const email = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '';
      this.userEmail.set(email);
      this.userProfile.set({ firstName: given, lastName: surname, email });
    });
  }

  onBookingCreated(bookingId: number): void {
    this.newBookingId.set(bookingId);
    this.refreshTrigger.update(n => n + 1);
    this.viewMode.setValue('history');
  }
}


# /Frontend/src/app/features/user/pages/dashboard.component.html

<div class="dashboard">
  <!-- Welcome message -->
  @if (loading()) {
    <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="loadDashboard()">Retry</button>
    </app-alert>
  } @else {
    <h1>Welcome back, Mr {{ firstName() }}</h1>

    <div class="booking-cards">
      <!-- Current Booking (CheckedIn) -->
      @if (currentBooking()) {
        <mat-card class="booking-card current">
          <mat-card-header>
            <mat-card-title>Current Stay</mat-card-title>
            <mat-card-subtitle>Room: {{ getRoomNumbers(currentBooking()!) }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p><strong>Check&#8209;in:</strong> {{ currentBooking()!.checkInDate }}</p>
            <p><strong>Check&#8209;out:</strong> {{ currentBooking()!.checkOutDate }}</p>
            <p><strong>Status:</strong> {{ currentBooking()!.bookingStatus }}</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="accent" (click)="openServiceRequest('housekeeping')" aria-label="Request housekeeping">
              <mat-icon>cleaning_services</mat-icon> Request Housekeeping
            </button>
            <button mat-raised-button color="warn" (click)="openServiceRequest('maintenance')" aria-label="Request maintenance">
              <mat-icon>build</mat-icon> Request Maintenance
            </button>
          </mat-card-actions>
        </mat-card>
      } @else {
        <mat-card class="booking-card no-booking">
          <mat-card-content>
            <p>No active stay right now.</p>
          </mat-card-content>
        </mat-card>
      }

      <!-- Upcoming Booking (Booked) -->
      @if (upcomingBooking()) {
        <mat-card class="booking-card upcoming">
          <mat-card-header>
            <mat-card-title>Upcoming Stay</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p><strong>Check&#8209;in:</strong> {{ upcomingBooking()!.checkInDate }}</p>
            <p><strong>Check&#8209;out:</strong> {{ upcomingBooking()!.checkOutDate }}</p>
            <p><strong>Status:</strong> {{ upcomingBooking()!.bookingStatus }}</p>
            @if (upcomingRoomTypes().length > 0) {
              <p><strong>Room Type(s):</strong> {{ upcomingRoomTypes().join(', ') }}</p>
            }
          </mat-card-content>
        </mat-card>
      } @else {
        <mat-card class="booking-card no-booking">
          <mat-card-content>
            <p>No upcoming bookings.</p>
          </mat-card-content>
        </mat-card>
      }
    </div>

    @if (currentBooking()) {
      <div class="room-service-status">
        <h2>Room Service Status</h2>
        <div class="status-grid">
          <!-- Housekeeping -->
          <mat-card class="status-card">
            <mat-card-header>
              <mat-card-title>Housekeeping</mat-card-title>
              <mat-card-subtitle>{{ pendingHousekeeping().length }} pending / in-progress</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @for (item of pendingHousekeeping(); track item.id) {
                <div class="status-item">
                  <p class="status-line">
                    <span>{{ item.description || 'No description' }}</span>
                    <span class="badge" [class]="item.status.toLowerCase()">{{ item.status }}</span>
                  </p>
                </div>
              } @empty {
                <p class="no-status-items">No pending requests.</p>
              }
            </mat-card-content>
          </mat-card>

          <!-- Maintenance -->
          <mat-card class="status-card">
            <mat-card-header>
              <mat-card-title>Maintenance</mat-card-title>
              <mat-card-subtitle>{{ pendingMaintenance().length }} pending / in-progress</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @for (item of pendingMaintenance(); track item.id) {
                <div class="status-item">
                  <p class="status-line">
                    <span>{{ item.description || 'No description' }}</span>
                    <span class="badge" [class]="item.status.toLowerCase()">{{ item.status }}</span>
                  </p>
                </div>
              } @empty {
                <p class="no-status-items">No pending requests.</p>
              }
            </mat-card-content>
          </mat-card>

          <!-- Food Orders -->
          <mat-card class="status-card">
            <mat-card-header>
              <mat-card-title>Food Orders</mat-card-title>
              <mat-card-subtitle>{{ pendingFoodOrders().length }} preparing / pending</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @for (order of pendingFoodOrders(); track order.id) {
                <div class="status-item">
                  <p class="status-line">
                    <span>Order #{{ order.id }}</span>
                    <span class="badge" [class]="(order.orderStatus || 'pending').toLowerCase()">{{ order.orderStatus || 'Pending' }}</span>
                  </p>
                </div>
              } @empty {
                <p class="no-status-items">No pending orders.</p>
              }
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    }
  }
</div>


# /Frontend/src/app/features/user/pages/dashboard.component.scss

.dashboard {
  padding: 24px;

  h1 {
    margin-bottom: 24px;
    font-size: 1.75rem;
    font-weight: 500;
  }

  .booking-cards {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;

    .booking-card {
      flex: 1 1 300px;

      mat-card-actions {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        padding: 8px 16px 16px;
      }
    }
  }

  .room-service-status {
    margin-top: 32px;

    h2 {
      font-size: 1.4rem;
      font-weight: 500;
      margin-bottom: 16px;
    }

    .status-grid {
      display: flex;
      flex-wrap: wrap;
      gap: 16px;

      .status-card {
        flex: 1 1 300px;
        max-width: 100%;
        box-shadow: 0 2px 4px rgba(0,0,0,0.05);

        mat-card-header {
          margin-bottom: 12px;
          border-bottom: 1px solid #f0f0f0;
          padding-bottom: 8px;
        }

        .status-item {
          padding: 8px 0;
          border-bottom: 1px dashed #f0f0f0;
          &:last-child {
            border-bottom: none;
          }

          p {
            margin: 0;
          }

          .status-line {
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: 0.95rem;
          }

          .description {
            font-size: 0.85rem;
            color: rgba(0, 0, 0, 0.54);
            margin-top: 4px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
          }

          .badge {
            display: inline-block;
            padding: 2px 6px;
            border-radius: 4px;
            font-size: 0.8rem;
            font-weight: 500;

            &.pending {
              background-color: #fff3e0;
              color: #e65100;
            }
            &.inprogress, &.preparing {
              background-color: #e8eaf6;
              color: #1a237e;
            }
            &.completed {
              background-color: #e8f5e9;
              color: #1b5e20;
            }
          }
        }

        .no-status-items {
          text-align: center;
          color: rgba(0, 0, 0, 0.54);
          font-style: italic;
          margin: 16px 0 8px;
        }
      }
    }
  }
}

@media (max-width: 599px) {
  .dashboard {
    .room-service-status {
      .status-grid {
        flex-direction: column;
      }
    }
  }
}


# /Frontend/src/app/features/user/pages/dashboard.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize, forkJoin } from 'rxjs';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingApiService } from '../services/booking-api.service';
import { HousekeepingApiService } from '../services/housekeeping-api.service';
import { MaintenanceApiService } from '../services/maintenance-api.service';
import { CustomerBookingFacade } from '../facades/customer-booking.facade';
import { of, switchMap } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Booking } from '../../admin/models/booking.model';
import { AlertComponent } from '../../auth/components/alert.component';
import { RequestServiceDialogComponent, RequestServiceDialogData, RequestServiceDialogResult } from '../components/request-service-dialog.component';
import { RoomTypeApiService } from '../services/room-type-api.service';
import { OrderApiService } from '../services/order-api.service';
import { CustomerRequest } from '../models/customer-request.model';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    AlertComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class PlaceholderCustomerDashboardComponent implements OnInit {
  firstName = signal('');
  loading = signal(false);
  error = signal<string | null>(null);
  currentBooking = signal<Booking | null>(null);
  upcomingBooking = signal<Booking | null>(null);
  upcomingRoomTypes = signal<string[]>([]);
  pendingHousekeeping = signal<CustomerRequest[]>([]);
  pendingMaintenance = signal<CustomerRequest[]>([]);
  pendingFoodOrders = signal<any[]>([]);

  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly authApi = inject(AuthApiService);
  private readonly bookingApi = inject(BookingApiService);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly bookingFacade = inject(CustomerBookingFacade);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  ngOnInit(): void {
    const pending = sessionStorage.getItem('pendingBooking');
    if (pending) {
      try {
        const data = JSON.parse(pending);
        sessionStorage.removeItem('pendingBooking'); // clear immediately
        // Navigate to bookings with pre‑fill query params
        this.router.navigate(['/user/bookings'], {
          queryParams: {
            new: true,
            roomTypeId: data.roomTypeId,
            checkIn: data.checkIn,
            checkOut: data.checkOut,
            guests: data.guests
          }
        });
        return; // skip normal dashboard loading
      } catch { /* ignore */ }
    }
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.error.set(null);

    this.bookingFacade.getCurrentCustomerProfile().pipe(
      takeUntilDestroyed(this.destroyRef),
      switchMap((profile) => {
        this.firstName.set(profile.firstName);
        if (!profile.email) {
          return forkJoin({
            active: of(null),
            upcoming: of({ data: [] as Booking[] })
          });
        }
        return forkJoin({
          active: this.bookingFacade.getActiveBooking(),
          upcoming: this.bookingApi.getAll({
            guestQuery: profile.email,
            status: 'Booked',
            pageNumber: 1,
            pageSize: 1,
            sortBy: 'checkInDate',
            sortDescending: false
          })
        });
      }),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: ({ active, upcoming }) => {
        this.currentBooking.set(active);
        this.upcomingBooking.set(upcoming.data.length > 0 ? upcoming.data[0] : null);
        if (active) {
          this.loadRoomServiceStatus();
        } else {
          this.pendingHousekeeping.set([]);
          this.pendingMaintenance.set([]);
          this.pendingFoodOrders.set([]);
        }
        if (upcoming.data.length > 0) {
          this.loadUpcomingRoomTypes(upcoming.data[0]);
        } else {
          this.upcomingRoomTypes.set([]);
        }
      },
      error: (err: unknown) => {
        this.error.set(this.extractErrorMessage(err));
      }
    });
  }

  private loadUpcomingRoomTypes(booking: Booking): void {
    if (!booking.rooms || booking.rooms.length === 0) {
      this.upcomingRoomTypes.set([]);
      return;
    }
    const ids = [...new Set(booking.rooms.map(r => r.roomTypeId))];
    const requests = ids.map(id =>
      this.roomTypeApi.getById(id).pipe(
        catchError(() => of(null)),
        map(rt => rt?.name ?? `Room Type ${id}`)
      )
    );
    forkJoin(requests).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(names => {
      this.upcomingRoomTypes.set(names);
    });
  }

  private loadRoomServiceStatus(): void {
    const booking = this.currentBooking();
    if (!booking) return;
    const roomIds = booking.rooms.map(r => r.roomId).filter(id => id != null) as number[];
    if (roomIds.length === 0) return;

    // Helper to fetch housekeeping/maintenance for a single status
    const fetchHousekeeping = (status: string) =>
      forkJoin(roomIds.map(roomId =>
        this.housekeepingApi.getAll({ roomId, status, pageSize: 20 }).pipe(
          map(res => res.data.map(hk => ({
            ...hk,
            type: 'Housekeeping' as const,
            roomNumber: hk.location ?? `Room ${hk.roomId}`,
            description: hk.description ?? ''
          }))),
          catchError(() => of([]))
        )
      )).pipe(map(results => results.flat()));

    const fetchMaintenance = (status: string) =>
      forkJoin(roomIds.map(roomId =>
        this.maintenanceApi.getAll({ roomId, status, pageSize: 20 }).pipe(
          map(res => res.data.map(mt => ({
            ...mt,
            type: 'Maintenance' as const,
            roomNumber: mt.location ?? `Room ${mt.roomId}`,
            description: mt.description ?? ''
          }))),
          catchError(() => of([]))
        )
      )).pipe(map(results => results.flat()));

    // Fetch both statuses for housekeeping and maintenance
    forkJoin({
      hkPending: fetchHousekeeping('Pending'),
      hkInProgress: fetchHousekeeping('InProgress'),
      mtPending: fetchMaintenance('Pending'),
      mtInProgress: fetchMaintenance('InProgress'),
      food: this.orderApi.getAll({ status: 'Pending', pageSize: 50 }).pipe(
        switchMap((res: any) => {
          return this.orderApi.getAll({ status: 'Preparing', pageSize: 50 }).pipe(
            map((res2: any) => [...res.data, ...res2.data].filter(o => o.bookingId === booking.id))
          );
        }),
        catchError(() => of([]))
      )
    }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: ({ hkPending, hkInProgress, mtPending, mtInProgress, food }) => {
        this.pendingHousekeeping.set([...hkPending, ...hkInProgress]);
        this.pendingMaintenance.set([...mtPending, ...mtInProgress]);
        // Normalize status field (API may return 'status' or 'orderStatus')
        this.pendingFoodOrders.set(
          (food as any[]).map((o: any) => ({
            ...o,
            orderStatus: o.orderStatus ?? o.status ?? 'Pending'
          }))
        );
      },
      error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  }

  openServiceRequest(type: 'housekeeping' | 'maintenance'): void {
    const booking = this.currentBooking();
    if (!booking || !booking.rooms.length || booking.rooms[0].roomId === null) {
      return;
    }

    const roomId = booking.rooms[0].roomId as number;
    const roomNumber = booking.rooms[0].roomNumber ?? roomId.toString();

    const data: RequestServiceDialogData = { roomNumber, roomId, type };
    const dialogRef = this.dialog.open<RequestServiceDialogComponent, RequestServiceDialogData, RequestServiceDialogResult>(
      RequestServiceDialogComponent,
      { data, width: '420px' }
    );

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((result: RequestServiceDialogResult | undefined) => {
      if (!result) return;

      const api$ = type === 'housekeeping'
        ? this.housekeepingApi.trigger(roomId, { description: result.description })
        : this.maintenanceApi.trigger(roomId, { description: result.description });

      api$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.snackBar.open(
            type === 'housekeeping' ? 'Housekeeping request submitted.' : 'Maintenance request submitted.',
            'Close',
            { duration: 4000 }
          );
        },
        error: (err: unknown) => {
          this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 });
        }
      });
    });
  }

  getRoomNumbers(booking: Booking): string {
    return booking.rooms
      .filter(r => r.roomNumber !== null)
      .map(r => r.roomNumber as string)
      .join(', ') || '—';
  }

  private extractErrorMessage(err: unknown): string {
    if (typeof err === 'string') return err;
    const e = err as { error?: { message?: string }; message?: string };
    if (e?.error?.message) return e.error.message;
    if (e?.message) return e.message;
    return 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/user/pages/room-service.component.html

<div class="room-service">
  @if (loadingActiveBooking()) {
    <div style="display: flex; justify-content: center; padding: 32px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (activeBookingError()) {
    <app-alert
      type="error"
      [message]="activeBookingError()!"
      (closed)="activeBookingError.set(null)"
    >
      <button
        mat-button
        (click)="loadActiveBooking()"
      >
        Retry
      </button>
    </app-alert>
  } @else if (activeBooking(); as booking) {
    <mat-tab-group>
      <mat-tab label="Food Order">
        <app-food-order
          [activeBookingId]="booking.id"
          [rooms]="booking.rooms"
          (orderPlaced)="onOrderPlaced()"
        />
      </mat-tab>
      <mat-tab label="Request Service">
        <app-request-service
          [activeBooking]="booking"
          (requestCreated)="onRequestCreated()"
        />
      </mat-tab>
      <mat-tab label="My Requests">
        <app-my-requests
          [roomIds]="roomIds()"
          [bookingId]="booking.id"
          [refresh]="refreshRequests()"
        />
      </mat-tab>
    </mat-tab-group>
  } @else {
    <mat-card class="no-booking-card">
      <mat-card-content class="no-booking-content">
        <mat-icon class="info-icon">info</mat-icon>
        <p>You need an active stay (Checked In) to use room service.</p>
        <p>Please visit <a routerLink="/user/bookings">My Bookings</a>.</p>
      </mat-card-content>
    </mat-card>
  }
</div>


# /Frontend/src/app/features/user/pages/room-service.component.scss

.room-service {
  padding: 24px;

  .no-booking-card {
    max-width: 480px;
    margin: 40px auto;
    text-align: center;
    padding: 24px;
    border-radius: 8px;
    border: 1px solid rgba(0, 0, 0, 0.12);
  }

  .no-booking-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 12px;

    .info-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: #3f51b5;
    }

    p {
      margin: 0;
      font-size: 1.1rem;
      color: rgba(0, 0, 0, 0.87);

      a {
        color: #3f51b5;
        text-decoration: none;
        font-weight: 500;
        &:hover {
          text-decoration: underline;
        }
      }
    }
  }
}


# /Frontend/src/app/features/user/pages/room-service.component.ts

import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { CustomerBookingFacade } from '../facades/customer-booking.facade';
import { Booking } from '../../admin/models/booking.model';
import { AlertComponent } from '../../../features/auth/components/alert.component';
import { FoodOrderComponent } from '../components/food-order/food-order.component';
import { RequestServiceComponent } from '../components/request-service/request-service.component';
import { MyRequestsComponent } from '../components/my-requests/my-requests.component';

@Component({
  selector: 'app-customer-room-service',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTabsModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    AlertComponent,
    FoodOrderComponent,
    RequestServiceComponent,
    MyRequestsComponent
  ],
  templateUrl: './room-service.component.html',
  styleUrls: ['./room-service.component.scss']
})
export class RoomServiceComponent implements OnInit {
  private readonly facade = inject(CustomerBookingFacade);
  private readonly destroyRef = inject(DestroyRef);

  activeBooking = signal<Booking | null>(null);
  loadingActiveBooking = signal(false);
  activeBookingError = signal<string | null>(null);
  refreshRequests = signal(0);

  roomIds = computed(() => {
    const booking = this.activeBooking();
    return booking ? booking.rooms.map(r => r.roomId).filter(id => id != null) as number[] : [];
  });

  ngOnInit(): void {
    this.loadActiveBooking();
  }

  loadActiveBooking(): void {
    this.loadingActiveBooking.set(true);
    this.activeBookingError.set(null);

    this.facade.getActiveBooking().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loadingActiveBooking.set(false))
    ).subscribe({
      next: booking => this.activeBooking.set(booking),
      error: (err: any) => this.activeBookingError.set(this.extractErrorMessage(err))
    });
  }

  onOrderPlaced(): void {
    // Only show a snackbar or log – no need to refresh My Requests tab.
  }

  onRequestCreated(): void {
    this.refreshRequests.update(n => n + 1);
  }

  private extractErrorMessage(err: any): string {
    return err.error?.message || err.message || 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/features/user/services/amenity-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Amenity } from '../../../features/admin/models/amenity.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class AmenityApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/amenities`;

  getAll(params: {
    pageNumber: number;
    pageSize: number;
    isAvailable?: boolean;
  }): Observable<PaginatedResponse<Amenity>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.isAvailable !== undefined) {
      httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
    }

    return this.http.get<PaginatedResponse<Amenity>>(this.baseUrl, { params: httpParams });
  }
}


# /Frontend/src/app/features/user/services/auth-api.service.ts

export { AuthApiService } from '../../../core/services/auth-api.service';


# /Frontend/src/app/features/user/services/billing-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { BillingFolio } from '../models/billing-folio.model';

@Injectable({ providedIn: 'root' })
export class BillingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/billing`;

  getByBookingId(bookingId: number): Observable<BillingFolio> {
    return this.http.get<BillingFolio>(`${this.baseUrl}/${bookingId}`);
  }

  pay(bookingId: number, dto: { amount: number; paymentMethod: string; transactionId: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${bookingId}/pay`, dto);
  }
}


# /Frontend/src/app/features/user/services/booking-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Booking } from '../../../features/admin/models/booking.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class BookingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/bookings`;

  getAll(params: {
    status?: string;
    bookingStatus?: string;
    guestQuery?: string;
    movementStatus?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<PaginatedResponse<Booking>> {
    let httpParams = new HttpParams()
      .set('pageNumber', (params.pageNumber ?? 1).toString())
      .set('pageSize', (params.pageSize ?? 10).toString())
      .set('sortBy', params.sortBy ?? 'id')
      .set('sortDescending', (params.sortDescending ?? false).toString());

    if (params.status) {
      httpParams = httpParams.set('bookingStatus', params.status);
    }
    if (params.bookingStatus) {
      httpParams = httpParams.set('bookingStatus', params.bookingStatus);
    }
    if (params.guestQuery) {
      httpParams = httpParams.set('guestQuery', params.guestQuery);
    }
    if (params.movementStatus) {
      httpParams = httpParams.set('movementStatus', params.movementStatus);
    }

    return this.http.get<PaginatedResponse<Booking>>(this.baseUrl, { params: httpParams });
  }

  create(booking: {
    roomTypeIds: number[];
    guestCount: number;
    checkInDate: string;
    checkOutDate: string;
    guestName?: string;
    guestEmail?: string;
    amenityIds?: number[];
  }): Observable<Booking> {
    return this.http.post<Booking>(this.baseUrl, booking);
  }

  cancel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/cancel`);
  }

  checkIn(id: number): Observable<Booking> {
    return this.http.post<Booking>(`${this.baseUrl}/${id}/checkin`, {});
  }

  extendStay(id: number, dto: { checkOutDate: string }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/extend-stay`, dto);
  }

  checkOut(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/checkout`, {});
  }
}


# /Frontend/src/app/features/user/services/feedback-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Feedback, CreateFeedbackDTO } from '../models/feedback.model';

@Injectable({ providedIn: 'root' })
export class FeedbackApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/feedback`;

  getByBookingId(bookingId: number): Observable<Feedback | null> {
    return this.http.get<Feedback | null>(`${this.baseUrl}/booking/${bookingId}`);
  }

  submit(dto: CreateFeedbackDTO): Observable<Feedback> {
    return this.http.post<Feedback>(this.baseUrl, dto);
  }
}


# /Frontend/src/app/features/user/services/housekeeping-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { HousekeepingTask } from '../../admin/models/housekeeping-task.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class HousekeepingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/housekeeping`;

  trigger(roomId: number, body: { description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/trigger/${roomId}`, body);
  }

  getAll(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    roomId?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<PaginatedResponse<HousekeepingTask>> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.roomId) httpParams = httpParams.set('roomId', params.roomId.toString());
      if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
      if (params.sortDescending !== undefined) httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    }
    return this.http.get<PaginatedResponse<HousekeepingTask>>(this.baseUrl, { params: httpParams });
  }

  createInternal(body: { location: string; description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/internal`, body);
  }

  updateStatus(id: number, dto: { status: string }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, dto);
  }
}


# /Frontend/src/app/features/user/services/maintenance-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { MaintenanceTask } from '../../admin/models/maintenance-task.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class MaintenanceApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/maintenance`;

  trigger(roomId: number, body: { description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/trigger/${roomId}`, body);
  }

  getAll(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    roomId?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<PaginatedResponse<MaintenanceTask>> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.roomId) httpParams = httpParams.set('roomId', params.roomId.toString());
      if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
      if (params.sortDescending !== undefined) httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    }
    return this.http.get<PaginatedResponse<MaintenanceTask>>(this.baseUrl, { params: httpParams });
  }

  createInternal(body: { location: string; description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/internal`, body);
  }

  updateStatus(id: number, dto: { status: string }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, dto);
  }
}


# /Frontend/src/app/features/user/services/menu-item-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { MenuItem } from '../../../features/admin/models/menu-item.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class MenuItemApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/menu-items`;

  getAll(params?: {
    isAvailable?: boolean;
    pageSize?: number;
    pageNumber?: number;
  }): Observable<PaginatedResponse<MenuItem>> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.isAvailable !== undefined) {
        httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
      }
      if (params.pageSize) {
        httpParams = httpParams.set('pageSize', params.pageSize.toString());
      }
      if (params.pageNumber) {
        httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      }
    }
    return this.http.get<PaginatedResponse<MenuItem>>(this.baseUrl, { params: httpParams });
  }
}


# /Frontend/src/app/features/user/services/order-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface CreateFoodOrderDTO {
  bookingId: number;
  roomId: number;
  items: { menuItemId: number; quantity: number }[];
}

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/orders`;

  create(dto: CreateFoodOrderDTO): Observable<any> {
    return this.http.post<any>(this.baseUrl, dto);
  }

  getAll(params?: any): Observable<any> {
    return this.http.get<any>(this.baseUrl, { params });
  }

  updateStatus(id: number, dto: { status: string }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}`, dto);
  }
}


# /Frontend/src/app/features/user/services/room-type-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AvailableRoomType } from '../models/available-room-type.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class RoomTypeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/room-types`;

  getAvailable(
    checkIn: string,
    checkOut: string,
    pageNumber: number = 1,
    pageSize: number = 100
  ): Observable<PaginatedResponse<AvailableRoomType>> {
    const params = new HttpParams()
      .set('checkIn', checkIn)
      .set('checkOut', checkOut)
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<AvailableRoomType>>(`${this.baseUrl}/availability`, { params });
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }
}


# /Frontend/src/app/features/user/user-shell.component.html

<mat-sidenav-container>
  <!-- SIDEBAR -->
  <mat-sidenav
    #sidenav
    [mode]="isMobile() ? 'over' : 'side'"
    [opened]="isMobile() ? sidebarOpen() : true"
    aria-label="Customer navigation">
    <mat-toolbar color="primary">Hotel</mat-toolbar>
    <mat-nav-list>
      <a mat-list-item routerLink="/user/dashboard" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon aria-hidden="true">dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
      <a mat-list-item routerLink="/user/bookings" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon aria-hidden="true">book_online</mat-icon>
        <span matListItemTitle>My Bookings</span>
      </a>
      <a mat-list-item routerLink="/user/room-service" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon aria-hidden="true">room_service</mat-icon>
        <span matListItemTitle>Room Service</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <!-- MAIN CONTENT -->
  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
        <button mat-icon-button (click)="sidebarOpen.set(!sidebarOpen())">
          <mat-icon aria-hidden="true">menu</mat-icon>
        </button>
      }
      <span>Hotel</span>
      <span class="spacer"></span>
      <button mat-icon-button [matMenuTriggerFor]="userMenu" aria-label="Open user menu">
        <mat-icon aria-hidden="true">account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button mat-menu-item routerLink="/user/profile">
          <mat-icon aria-hidden="true">manage_accounts</mat-icon> Profile
        </button>
        <button mat-menu-item (click)="logout()">
          <mat-icon aria-hidden="true">logout</mat-icon> Logout
        </button>
      </mat-menu>
    </mat-toolbar>

    <!-- ROUTER OUTLET -->
    <div class="content">
      <router-outlet></router-outlet>
      
      <footer class="site-footer">
        <div class="footer-links">
          <a href="#">Privacy Policy</a>
          <a href="#">Terms of Service</a>
          <a href="#">Press</a>
          <a href="#">Careers</a>
          <a href="#">Contact</a>
        </div>
        <div class="footer-logo">AETHERIS</div>
        <div class="footer-info">
          <span>1 AETHERIS PEAK, THE SILENT VALLEY</span>
          <span class="separator"></span>
          <span>&copy; 2024 AETHERIS. ALL RIGHTS RESERVED.</span>
        </div>
      </footer>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>


# /Frontend/src/app/features/user/user-shell.component.scss

@import '../../../styles/theme/index';

mat-sidenav-container {
  height: 100vh;
  width: 100%;
}

mat-sidenav {
  width: 250px;
  border-right: 1px solid rgba(0, 0, 0, 0.12);

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

mat-sidenav-content {
  display: flex;
  flex-direction: column;
  height: 100%;

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

.spacer {
  flex: 1 1 auto;
}

.content {
  padding: 24px;
  flex-grow: 1;
  overflow-y: auto;
  box-sizing: border-box;
}

.active {
  background-color: rgba(63, 81, 181, 0.08);
  color: #3f51b5 !important;
  font-weight: 500;

  mat-icon {
    color: #3f51b5;
  }
}

@media (max-width: 1024px) {
  .content {
    padding: 16px;
  }
}

.site-footer {
  background: var(--color-surface-container-lowest);
  padding: 6rem 1rem 3rem;
  text-align: center;
  border-top: 1px solid var(--glass-border);
  margin-top: 4rem; // Add spacing above the footer inside content container

  .footer-links {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 2rem;
    margin-bottom: 3rem;
    a {
      @include font-body-md;
      color: var(--color-on-tertiary-container);
      text-decoration: none;
      transition: color 0.3s;
      &:hover { color: var(--color-secondary); }
    }
    @media (max-width: 768px) {
      gap: 1.2rem;
      a { font-size: 0.85rem; }
    }
  }

  .footer-logo {
    font-family: var(--font-headline);
    font-size: clamp(3rem, 10vw, 7.5rem);
    letter-spacing: 0.3em;
    color: var(--color-on-surface);
    margin-bottom: 1.5rem;
    text-transform: uppercase;
  }

  .footer-info {
    font-family: var(--font-body);
    font-size: 0.625rem;
    font-weight: 500;
    letter-spacing: 0.3em;
    text-transform: uppercase;
    color: rgba(228, 226, 221, 0.4);
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    align-items: center;
    gap: 1.5rem;
    .separator {
      display: inline-block;
      width: 4px;
      height: 4px;
      border-radius: 50%;
      background: rgba(228, 226, 221, 0.2);
    }
  }
}


# /Frontend/src/app/features/user/user-shell.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-user-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './user-shell.component.html',
  styleUrls: ['./user-shell.component.scss'],
})
export class UserShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  sidebarOpen = signal(false);

  onNavClick(): void {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }
}


# /Frontend/src/app/shared/components/alert/alert.component.ts

import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div [class]="'alert-box ' + type" role="alert" aria-live="polite">
      <span class="alert-message">{{ message }}</span>
      <button mat-icon-button type="button" aria-label="Close alert" (click)="closed.emit()">
        <mat-icon>close</mat-icon>
      </button>
    </div>
  `,
  styles: [`
    .alert-box {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      margin: 16px 0;
      border-radius: 4px;
      font-size: 14px;
    }
    .alert-box.success {
      background-color: #e8f5e9;
      color: #2e7d32;
      border: 1px solid #c8e6c9;
    }
    .alert-box.error {
      background-color: #ffebee;
      color: #c62828;
      border: 1px solid #ffcdd2;
    }
    .alert-message {
      flex-grow: 1;
    }
    button {
      color: inherit;
    }
  `]
})
export class AlertComponent {
  @Input() type: 'success' | 'error' = 'success';
  @Input() message = '';
  @Output() closed = new EventEmitter<void>();
}


# /Frontend/src/app/shared/components/confirm-dialog/confirm-dialog.component.html

<h2 mat-dialog-title>{{ data.title }}</h2>
<mat-dialog-content>
  <p>{{ data.message }}</p>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button (click)="cancel()">Cancel</button>
  <button mat-raised-button color="warn" (click)="confirm()">Confirm</button>
</mat-dialog-actions>


# /Frontend/src/app/shared/components/confirm-dialog/confirm-dialog.component.scss

mat-dialog-content {
  p {
    margin: 0;
    font-size: 0.95rem;
    color: rgba(0, 0, 0, 0.8);
  }
}

mat-dialog-actions {
  gap: 8px;
}


# /Frontend/src/app/shared/components/confirm-dialog/confirm-dialog.component.ts

import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface ConfirmDialogData {
  title: string;
  message: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss'],
})
export class ConfirmDialogComponent {
  readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}


# /Frontend/src/app/shared/components/custom-cursor/custom-cursor.component.html

<div class="custom-cursor" #cursor></div>


# /Frontend/src/app/shared/components/custom-cursor/custom-cursor.component.scss

.custom-cursor {
  position: fixed;
  pointer-events: none;
  z-index: 9999;
  width: 12px;
  height: 12px;
  border: 1px solid #e4c285;
  border-radius: 50%;
  background: transparent;
  transform: translate(-50%, -50%);
  transition: width 0.2s, height 0.2s, background-color 0.2s, border-radius 0.2s;
  mix-blend-mode: difference;

  &.enlarged {
    width: 24px;
    height: 24px;
    background: rgba(228, 194, 133, 0.2);
    border-radius: 50%;
  }

  &.oval {
    width: 4px;
    height: 20px;
    border-radius: 2px;
    border-color: #e4c285;
    background: transparent;
  }
}

// Hide cursor on touch devices
@media (any-pointer: coarse) {
  .custom-cursor { display: none; }
}


# /Frontend/src/app/shared/components/custom-cursor/custom-cursor.component.ts

import { Component, AfterViewInit, OnDestroy, ElementRef, Renderer2, inject, PLATFORM_ID, ViewChild } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { DOCUMENT } from '@angular/common';

@Component({
  selector: 'app-custom-cursor',
  standalone: true,
  imports: [],
  templateUrl: './custom-cursor.component.html',
  styleUrls: ['./custom-cursor.component.scss'],
})
export class CustomCursorComponent implements AfterViewInit, OnDestroy {
  @ViewChild('cursor', { static: true }) cursorRef!: ElementRef<HTMLElement>;
  private cursorEl!: HTMLElement;
  private renderer = inject(Renderer2);
  private document = inject(DOCUMENT);
  private platformId = inject(PLATFORM_ID);

  private readonly INTERACTIVE_SELECTOR = 'a, button, .cursor-hover, mat-slide-toggle, mat-icon-button, [role="button"]';
  private readonly INPUT_SELECTOR = 'input, textarea, select, mat-select, .mat-mdc-input-element';

  private rafId: number | null = null;
  private mouseX = 0;
  private mouseY = 0;

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.cursorEl = this.cursorRef.nativeElement;
    this.document.addEventListener('mousemove', this.onMouseMove);
    this.document.addEventListener('mouseover', this.onMouseOver);
    this.document.addEventListener('mouseout', this.onMouseOut);
    this.updatePosition();
  }

  ngOnDestroy(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.document.removeEventListener('mousemove', this.onMouseMove);
    this.document.removeEventListener('mouseover', this.onMouseOver);
    this.document.removeEventListener('mouseout', this.onMouseOut);
    if (this.rafId) cancelAnimationFrame(this.rafId);
  }

  private onMouseMove = (e: MouseEvent): void => {
    this.mouseX = e.clientX;
    this.mouseY = e.clientY;
  };

  private onMouseOver = (e: MouseEvent): void => {
    const target = e.target as HTMLElement;
    if (!target) return;
    if (target.matches(this.INTERACTIVE_SELECTOR) || target.closest(this.INTERACTIVE_SELECTOR)) {
      this.renderer.addClass(this.cursorEl, 'enlarged');
    } else if (target.matches(this.INPUT_SELECTOR) || target.closest(this.INPUT_SELECTOR)) {
      this.renderer.addClass(this.cursorEl, 'oval');
    }
  };

  private onMouseOut = (e: MouseEvent): void => {
    const target = e.target as HTMLElement;
    if (!target) return;
    if (target.matches(this.INTERACTIVE_SELECTOR) || target.closest(this.INTERACTIVE_SELECTOR)) {
      this.renderer.removeClass(this.cursorEl, 'enlarged');
    }
    if (target.matches(this.INPUT_SELECTOR) || target.closest(this.INPUT_SELECTOR)) {
      this.renderer.removeClass(this.cursorEl, 'oval');
    }
  };

  private updatePosition(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.rafId = requestAnimationFrame(() => {
        this.renderer.setStyle(this.cursorEl, 'left', `${this.mouseX}px`);
        this.renderer.setStyle(this.cursorEl, 'top', `${this.mouseY}px`);
        this.updatePosition();
      });
    }
  }
}


# /Frontend/src/app/shared/components/generic-crud/cards-view/cards-view.component.html

@for (row of data(); track $index) {
  <mat-card class="entity-card">
    <mat-card-content>
      @for (col of columns(); track col.field) {
        <div class="card-row">
          <span class="card-label">{{ col.header }}:</span>
          <span class="card-value">
            @if (col.cellTemplate) {
              <ng-container *ngTemplateOutlet="col.cellTemplate; context: { $implicit: row }"></ng-container>
            } @else {
              {{ col.getValue(row) }}
            }
          </span>
        </div>
      }
    </mat-card-content>
    <mat-card-actions align="end">
      <button mat-icon-button (click)="edit.emit(row)" aria-label="Edit">
        <mat-icon>edit</mat-icon>
      </button>
    </mat-card-actions>
  </mat-card>
}


# /Frontend/src/app/shared/components/generic-crud/cards-view/cards-view.component.scss

.entity-card {
  margin-bottom: 12px;

  .card-row {
    display: flex;
    gap: 8px;
    padding: 4px 0;
    font-size: 0.875rem;
    border-bottom: 1px solid #f5f5f5;

    &:last-child {
      border-bottom: none;
    }
  }

  .card-label {
    font-weight: 600;
    color: rgba(0, 0, 0, 0.6);
    min-width: 120px;
  }

  .card-value {
    color: rgba(0, 0, 0, 0.87);
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
    word-break: break-word;
  }
}

.card-item {
  overflow: hidden;
  .card-content {
    p,
    span {
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
      text-overflow: ellipsis;
      word-break: break-word;
    }
  }
}



# /Frontend/src/app/shared/components/generic-crud/cards-view/cards-view.component.ts

import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ColumnDef } from '../../../models/crud-config.model';

@Component({
  selector: 'app-cards-view',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './cards-view.component.html',
  styleUrls: ['./cards-view.component.scss'],
})
export class CardsViewComponent {
  data = input.required<any[]>();
  columns = input.required<ColumnDef[]>();
  edit = output<any>();
}


# /Frontend/src/app/shared/components/generic-crud/crud-modal/crud-modal.component.html

<h2 mat-dialog-title>
  {{ data.editMode ? 'Edit' : 'Add' }} {{ data.formFields.length > 0 ? '' : 'Item' }}
  <button mat-icon-button mat-dialog-close class="close-btn" aria-label="Close dialog">
    <mat-icon>close</mat-icon>
  </button>
</h2>

<mat-dialog-content>
  @if (modalForm) {
    <form [formGroup]="modalForm" (ngSubmit)="submit()">
      @for (field of getFieldDefs(); track field.key) {
        @if (field.type === 'select') {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ field.label }}</mat-label>
            <mat-select [formControl]="getControl(field.key)">
              @for (option of field.options; track option.value) {
                <mat-option [value]="option.value">{{ option.label }}</mat-option>
              }
            </mat-select>
            @if (getControl(field.key).invalid && getControl(field.key).touched) {
              <mat-error>{{ getErrorMessage(field, getControl(field.key)) }}</mat-error>
            }
          </mat-form-field>
        } @else if (field.type === 'textarea') {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ field.label }}</mat-label>
            <textarea matInput rows="3" [formControl]="getControl(field.key)"></textarea>
            @if (getControl(field.key).invalid && getControl(field.key).touched) {
              <mat-error>{{ getErrorMessage(field, getControl(field.key)) }}</mat-error>
            }
          </mat-form-field>
        } @else if (field.type === 'toggle') {
          <div class="toggle-row">
            <label class="toggle-label">{{ field.label }}</label>
            <mat-slide-toggle [formControl]="getControl(field.key)"></mat-slide-toggle>
          </div>
        } @else if (field.type === 'keyValueList') {
          <div class="key-value-list" [formGroupName]="field.key">
            <label class="list-label">{{ field.label }}</label>
            <div formArrayName="pairs">
              @for (pair of getKeyValueArray(field.key).controls; let i = $index; track i) {
                <div class="pair-row" [formGroupName]="i">
                  <mat-form-field appearance="outline">
                    <mat-label>Bed Type</mat-label>
                    <input matInput formControlName="key" placeholder="e.g., King" />
                    <mat-error *ngIf="pair.get('key')?.invalid && pair.get('key')?.touched">
                      {{ getErrorMessage(field, pair.get('key')!) }}
                    </mat-error>
                  </mat-form-field>
                  <mat-form-field appearance="outline">
                    <mat-label>Quantity</mat-label>
                    <input matInput type="number" formControlName="value" min="1" />
                    <mat-error *ngIf="pair.get('value')?.invalid && pair.get('value')?.touched">
                      Quantity is required (min 1)
                    </mat-error>
                  </mat-form-field>
                  <button mat-icon-button type="button" (click)="removeKeyValuePair(field.key, i)" aria-label="Remove bed type">
                    <mat-icon>close</mat-icon>
                  </button>
                </div>
              }
            </div>
            <button mat-button type="button" class="add-btn" (click)="addKeyValuePair(field.key)">
              <mat-icon>add</mat-icon> Add bed type
            </button>
          </div>
        } @else if (field.type === 'imageUrlList') {
          <div class="image-url-list" [formGroupName]="field.key">
            <label class="list-label">{{ field.label }}</label>
            <div formArrayName="urls">
              @for (urlCtrl of getImageUrlArray(field.key).controls; let i = $index; track i) {
                <div class="url-row">
                  <mat-form-field appearance="outline" class="full-width-input">
                    <mat-label>Image URL</mat-label>
                    <input matInput [formControl]="$any(urlCtrl)" placeholder="https://..." />
                    <mat-error *ngIf="urlCtrl.invalid && urlCtrl.touched">
                      Enter a valid URL
                    </mat-error>
                  </mat-form-field>
                  <button mat-icon-button type="button" (click)="removeImageUrl(field.key, i)" aria-label="Remove URL">
                    <mat-icon>close</mat-icon>
                  </button>
                </div>
              }
            </div>
            <button mat-button type="button" class="add-btn" (click)="addImageUrl(field.key)">
              <mat-icon>add</mat-icon> Add image URL
            </button>
          </div>
        } @else {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>{{ field.label }}</mat-label>
            <input matInput [type]="field.type" [formControl]="getControl(field.key)" />
            @if (getControl(field.key).invalid && getControl(field.key).touched) {
              <mat-error>{{ getErrorMessage(field, getControl(field.key)) }}</mat-error>
            }
          </mat-form-field>
        }
      }

      @if (data.supportsToggle && data.editMode && !hasToggleField()) {
        <div class="toggle-row">
          <label class="toggle-label">Active</label>
          <mat-slide-toggle [formControl]="isActiveControl"></mat-slide-toggle>
        </div>
      }
    </form>
  }
</mat-dialog-content>

<mat-dialog-actions align="end">
  <button mat-button (click)="cancel()">Cancel</button>
  <button
    mat-raised-button
    color="primary"
    [disabled]="modalForm?.invalid"
    (click)="submit()"
  >
    Save
  </button>
</mat-dialog-actions>


# /Frontend/src/app/shared/components/generic-crud/crud-modal/crud-modal.component.scss

h2[mat-dialog-title] {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin: 0;
  padding: 16px 16px 16px 24px;
  font-size: 1.125rem;
  font-weight: 600;
  border-bottom: 1px solid #e0e0e0;

  .close-btn {
    margin-left: auto;
  }
}

mat-dialog-content {
  padding: 16px 24px;
  min-width: 360px;
  max-width: 560px;

  form {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  @media (max-width: 768px) {
    min-width: unset;
    width: 100%;
  }
}

.full-width {
  width: 100%;
}

.toggle-row {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 8px 0;

  .toggle-label {
    font-size: 0.875rem;
    font-weight: 500;
    color: rgba(0, 0, 0, 0.6);
  }
}

mat-dialog-actions {
  padding: 12px 24px 16px;
  border-top: 1px solid #e0e0e0;
  gap: 8px;
}

.key-value-list, .image-url-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin: 8px 0;

  .list-label {
    font-size: 0.875rem;
    font-weight: 500;
    color: rgba(0, 0, 0, 0.6);
  }

  .pair-row {
    display: flex;
    align-items: center;
    gap: 8px;
    width: 100%;

    mat-form-field {
      flex: 1;
      margin-bottom: -1.25em;
    }
    
    button {
      align-self: center;
    }
  }

  .url-row {
    display: flex;
    align-items: center;
    gap: 8px;
    width: 100%;

    mat-form-field {
      flex: 1;
      margin-bottom: -1.25em;
    }

    button {
      align-self: center;
    }
  }

  .add-btn {
    align-self: flex-start;
    margin-top: 8px;
  }
}


# /Frontend/src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts

import { Component, inject, OnInit, signal, DestroyRef, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, AbstractControl, FormArray, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { CrudModalData, CrudModalResult, FormFieldDef } from '../../../models/crud-config.model';
import { ConfirmDialogComponent } from '../../confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-crud-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatIconModule,
  ],
  templateUrl: './crud-modal.component.html',
  styleUrls: ['./crud-modal.component.scss'],
})
export class CrudModalComponent implements OnInit {
  readonly data = inject<CrudModalData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CrudModalComponent>);
  private readonly dialog = inject(MatDialog);

  modalForm!: FormGroup;
  isActiveControl = new FormControl<any>(true);
  activeFields: FormFieldDef[] = [];
  hasToggleField = computed(() => this.activeFields.some(f => f.type === 'toggle'));

  ngOnInit(): void {
    this.activeFields = this.data.formFields.filter((f) => {
      if (this.data.editMode) {
        return f.showInEdit !== false;
      } else {
        return f.showInAdd !== false;
      }
    });

    const controls: Record<string, AbstractControl> = {};
    for (const field of this.activeFields) {
      if (field.type === 'keyValueList') {
        const pairsArray = new FormArray<FormGroup>([]);
        const bedConfig = this.data.entity ? this.data.entity[field.key] : null;
        if (bedConfig && typeof bedConfig === 'object') {
          Object.entries(bedConfig).forEach(([key, value]) => {
            pairsArray.push(
              new FormGroup({
                key: new FormControl(key, [
                  Validators.required,
                  Validators.pattern(/^[a-zA-Z0-9\s\-']+$/),
                ]),
                value: new FormControl(value, [Validators.required, Validators.min(1)]),
              }),
            );
          });
        }
        controls[field.key] = new FormGroup({ pairs: pairsArray });
      } else if (field.type === 'imageUrlList') {
        const urlsArray = new FormArray<FormControl>([]);
        const urls = this.data.entity ? this.data.entity[field.key] : null;
        if (urls && Array.isArray(urls)) {
          urls.forEach((url) => {
            urlsArray.push(new FormControl(url, [Validators.pattern(/^https?:\/\/.+/)]));
          });
        }
        controls[field.key] = new FormGroup({ urls: urlsArray });
      } else {
        const value = this.data.entity ? (this.data.entity[field.key] ?? null) : null;
        controls[field.key] = new FormControl(value, field.validators ?? []);
      }
    }
    this.modalForm = new FormGroup(controls);

    const toggleField = this.activeFields.find((f) => f.type === 'toggle');
    if (toggleField) {
      this.isActiveControl = this.getControl(toggleField.key);
    } else {
      if (this.data.supportsToggle && this.data.entity) {
        this.isActiveControl.setValue(
          this.data.entity.isActive ?? this.data.entity.isAvailable ?? true,
        );
      }
    }
  }

  getFieldDefs(): FormFieldDef[] {
    return this.activeFields;
  }

  getControl(key: string): FormControl {
    return this.modalForm.get(key) as FormControl;
  }

  submit(): void {
    this.modalForm.markAllAsTouched();
    if (this.modalForm.invalid) return;

    const rawValue = this.modalForm.getRawValue();
    for (const field of this.activeFields) {
      if (field.type === 'keyValueList') {
        const pairs: { key: string; value: number }[] = rawValue[field.key]?.pairs ?? [];
        const obj: Record<string, number> = {};
        pairs.forEach((p) => {
          if (p.key) obj[p.key] = p.value;
        });
        rawValue[field.key] = Object.keys(obj).length > 0 ? obj : null;
      } else if (field.type === 'imageUrlList') {
        rawValue[field.key] = (rawValue[field.key]?.urls ?? []).filter(Boolean);
      }
    }

    const result: CrudModalResult = {
      formValue: rawValue,
      isActive: this.data.supportsToggle ? (this.isActiveControl.value ?? true) : true,
      previousIsActive: this.data.supportsToggle && this.data.entity
        ? (this.data.entity.isActive ?? true)
        : true,
      entityId: this.data.entity?.id,
    };

    if (this.data.editMode && result.previousIsActive && !result.isActive) {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Confirm Deactivation',
          message: `Are you sure you want to disable this ${this.data.entityName ?? 'item'}?`,
        },
      });
      dialogRef.afterClosed().subscribe((confirmed) => {
        if (confirmed) {
          this.dialogRef.close(result);
        }
      });
    } else {
      this.dialogRef.close(result);
    }
  }

  getKeyValueArray(fieldName: string): FormArray {
    return this.modalForm.get(fieldName + '.pairs') as FormArray;
  }

  getImageUrlArray(fieldName: string): FormArray {
    return this.modalForm.get(fieldName + '.urls') as FormArray;
  }

  addKeyValuePair(fieldName: string): void {
    const pair = new FormGroup({
      key: new FormControl('', [
        Validators.required,
        Validators.pattern(/^[a-zA-Z0-9\s\-']+$/),
      ]),
      value: new FormControl(1, [Validators.required, Validators.min(1)]),
    });
    this.getKeyValueArray(fieldName).push(pair);
  }

  removeKeyValuePair(fieldName: string, index: number): void {
    this.getKeyValueArray(fieldName).removeAt(index);
  }

  addImageUrl(fieldName: string): void {
    this.getImageUrlArray(fieldName).push(
      new FormControl('', [Validators.pattern(/^https?:\/\/.+/)]),
    );
  }

  removeImageUrl(fieldName: string, index: number): void {
    this.getImageUrlArray(fieldName).removeAt(index);
  }

  cancel(): void {
    this.dialogRef.close(null);
  }

  getErrorMessage(field: FormFieldDef, control: AbstractControl): string | null {
    if (!control.errors || !control.touched) return null;
    const errors = control.errors;
    if (errors['required']) {
      return `${field.label} is required.`;
    }
    if (errors['email'] || (field.type === 'email' && errors['pattern'])) {
      return 'Please enter a valid email address.';
    }
    if (errors['pattern']) {
      return `${field.label} contains invalid characters or format.`;
    }
    if (errors['min']) {
      return `${field.label} must be at least ${errors['min'].min}.`;
    }
    if (errors['max']) {
      return `${field.label} must be at most ${errors['max'].max}.`;
    }
    if (errors['minlength']) {
      return `${field.label} must be at least ${errors['minlength'].requiredLength} characters.`;
    }
    if (errors['maxlength']) {
      return `${field.label} must be at most ${errors['maxlength'].requiredLength} characters.`;
    }
    return `${field.label} is invalid.`;
  }
}


# /Frontend/src/app/shared/components/generic-crud/generic-crud.component.html

<div class="crud-container">
  <!-- Show full-page spinner ONLY when loading AND no data yet -->
  @if (isInitialLoad()) {
    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
  } @else {
    <!-- Always show the content area, even when loading (if data exists) -->
    <div class="crud-content">
      <!-- Top bar -->
      <div class="top-bar">
        <h2>{{ config().entityNamePlural }}</h2>
        <button
          mat-raised-button
          color="primary"
          (click)="openAddModal()"
        >
          <mat-icon>add</mat-icon> Add {{ config().entityName }}
        </button>
      </div>

      <!-- Search & Filter Bar -->
      <div class="search-filter-bar">
        <mat-form-field
          appearance="outline"
          class="search-field"
        >
          <mat-label>Search {{ config().entityNamePlural }}</mat-label>
          <input
            matInput
            [formControl]="searchControl"
            (keyup)="onSearchDebounced()"
          />
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
        @for (filter of config().filters; track filter.key) {
        <mat-form-field appearance="outline">
          <mat-label>{{ filter.label }}</mat-label>
          <mat-select
            [formControl]="getFilterControl(filter.key)"
            (selectionChange)="onFilterChange(filter.key)"
          >
            @for (option of filter.options; track option.value) {
            <mat-option [value]="option.value">{{ option.label }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        } @if (hasActiveFilters()) {
        <button
          mat-button
          (click)="clearFilters()"
        >
          Clear Filters
        </button>
        }
      </div>

      <!-- Loading indicator for data refresh -->
      @if (config().loading()) {
        <mat-progress-bar mode="indeterminate" color="primary"></mat-progress-bar>
      }

      <!-- Error state -->
      @if (config().error()) {
        <app-alert
          type="error"
          [message]="config().error()!"
          (closed)="config().error.set(null)"
        ></app-alert>
      }

      <!-- Empty state -->
      @if (!config().loading() && (!config().data() || config().data().length === 0)) {
        <div class="empty-state">
          <img
            src="assets/empty-state.svg"
            alt=""
          />
          <p>No {{ config().entityNamePlural }} found.</p>
          @if (hasActiveFilters()) {
          <p>Try adjusting your filters.</p>
          <button
            mat-button
            (click)="clearFilters()"
          >
            Clear Filters
          </button>
          } @else {
          <button
            mat-raised-button
            (click)="openAddModal()"
          >
            Add your first {{ config().entityName }}
          </button>
          }
        </div>
      }

      <!-- Table or Card View (always mounted if data exists) -->
      @if (config().data() && config().data().length > 0) {
        <!-- Desktop Table -->
        <div class="desktop-view">
          <table
            mat-table
            [dataSource]="config().data()"
            matSort
            matSortDisableClear
            (matSortChange)="onSortChange($event)"
          >
            @for (col of config().columns; track col.field; let i = $index) {
            <ng-container [matColumnDef]="col.field">
              <th
                mat-header-cell
                *matHeaderCellDef
                mat-sort-header="{{ col.sortable ? col.field : '' }}"
                [style.width]="columnWidths()[i]"
              >
                {{ col.header }}
              </th>
              <td
                mat-cell
                *matCellDef="let row"
                [style.width]="columnWidths()[i]"
              >
                @if (col.cellTemplate) {
                <ng-container
                  *ngTemplateOutlet="col.cellTemplate; context: { $implicit: row }"
                ></ng-container>
                } @else { {{ col.getValue(row) }} }
              </td>
            </ng-container>
            }
            <ng-container matColumnDef="actions">
              <th
                mat-header-cell
                *matHeaderCellDef
                [style.width]="columnWidths()[config().columns.length]"
              >
                Actions
              </th>
              <td
                mat-cell
                *matCellDef="let row"
                [style.width]="columnWidths()[config().columns.length]"
              >
                <button
                  mat-icon-button
                  (click)="openEditModal(row)"
                  aria-label="Edit"
                >
                  <mat-icon>edit</mat-icon>
                </button>
              </td>
            </ng-container>
            <tr
              mat-header-row
              *matHeaderRowDef="displayedColumns"
            ></tr>
            <tr
              mat-row
              *matRowDef="let row; columns: displayedColumns"
              [attr.data-row-id]="row.id"
            ></tr>
          </table>
        </div>

        <!-- Mobile Card View -->
        <div class="mobile-view">
          <app-cards-view
            [data]="config().data()"
            [columns]="config().columns"
            (edit)="openEditModal($event)"
          ></app-cards-view>
        </div>

        <!-- Paginator -->
        <mat-paginator
          [length]="config().totalCount()"
          [pageIndex]="config().pageIndex()"
          [pageSize]="config().pageSize()"
          [pageSizeOptions]="[10, 25, 50, 100]"
          (page)="onPageChange($event)"
        ></mat-paginator>
      }
    </div>
  }
</div>


# /Frontend/src/app/shared/components/generic-crud/generic-crud.component.scss

.crud-container {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;

  .crud-content {
    display: flex;
    flex-direction: column;
    gap: 16px;
    width: 100%;
  }

  // Top bar
  .top-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 12px;

    h2 {
      margin: 0;
      font-size: 1.25rem;
      font-weight: 600;
    }
  }

  // Search & filter bar
  .search-filter-bar {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;

    mat-form-field {
      flex: 1 1 200px;
      min-width: 150px;
    }

    .search-field {
      min-width: 150px;
      flex: 1 1 200px;
    }
  }

  // Loading spinner
  .loading {
    display: flex;
    justify-content: center;
    padding: 48px 0;
  }

  // Empty state
  .empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 12px;
    padding: 48px 24px;
    text-align: center;
    color: rgba(0, 0, 0, 0.5);

    img {
      width: 120px;
      opacity: 0.5;
    }

    p {
      margin: 0;
      font-size: 0.95rem;
    }
  }

  // Desktop table — hide on mobile
  .desktop-view {
    display: block;
    overflow-x: auto;

    table {
      width: 100%;
      table-layout: fixed;
    }

    th,
    td {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    @media (max-width: 768px) {
      display: none;
    }
  }

  // Mobile card view — hide on desktop
  .mobile-view {
    display: none;

    @media (max-width: 768px) {
      display: block;
    }
  }
}

@keyframes highlight-fade {
  0% {
    background-color: #fff176;
  }
  100% {
    background-color: transparent;
  }
}

:host ::ng-deep .highlight-row {
  animation: highlight-fade 2s ease-out;
}


# /Frontend/src/app/shared/components/generic-crud/generic-crud.component.ts

import {
  Component,
  inject,
  signal,
  computed,
  DestroyRef,
  input,
  output,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialogModule, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CrudConfig, CrudModalData, CrudModalResult } from '../../models/crud-config.model';
import { CardsViewComponent } from './cards-view/cards-view.component';
import { CrudModalComponent } from './crud-modal/crud-modal.component';
import { AlertComponent } from '../../../features/auth/components/alert.component';

@Component({
  selector: 'app-generic-crud',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatDialogModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatSelectModule,
    CardsViewComponent,
    AlertComponent,
  ],
  templateUrl: './generic-crud.component.html',
  styleUrls: ['./generic-crud.component.scss'],
})
export class GenericCrudComponent {
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  isInitialLoad = computed(() => this.config().loading() && (!this.config().data() || this.config().data().length === 0));

  constructor() {
    effect(() => {
      const query = this.searchQuery();
      if (query !== this.searchControl.value) {
        this.searchControl.setValue(query, { emitEvent: false });
      }
    });
  }

  // Inputs & Outputs per spec §4
  config = input.required<CrudConfig<any>>();

  searchChange = output<string>();
  filterChange = output<Record<string, any>>();
  sortChange = output<{ active: string; direction: 'asc' | 'desc' }>();
  pageChange = output<{ pageIndex: number; pageSize: number }>();
  save = output<{ formValue: any; isActive: boolean; entityId?: number }>();
  edit = output<any>();
  searchQuery = input<string, any>('', {
    transform: (value: any) => value ?? '',
  });

  // Internal signals per spec §4
  isModalOpen = signal(false);
  editMode = signal(false);
  selectedEntity = signal<any | null>(null);
  modalLoading = signal(false);
  modalError = signal<string | null>(null);

  // Internal form controls
  searchControl = new FormControl<string>('', { nonNullable: true });
  filterControls = new Map<string, FormControl>();

  private dialogRef: MatDialogRef<CrudModalComponent> | undefined;

  private searchDebounceTimer: ReturnType<typeof setTimeout> | null = null;

  get displayedColumns(): string[] {
    return [...this.config().columns.map((c) => c.field), 'actions'];
  }

  columnWidths = computed(() => {
    const cols = this.config().columns;
    const defaultWidth = `${100 / (cols.length + 1)}%`; // +1 for actions column
    return [...cols.map((col) => col.width ?? defaultWidth), defaultWidth];
  });

  getFilterControl(key: string): FormControl {
    if (!this.filterControls.has(key)) {
      this.filterControls.set(key, new FormControl(null));
    }
    return this.filterControls.get(key)!;
  }

  hasActiveFilters(): boolean {
    if (this.searchControl.value) return true;
    for (const ctrl of this.filterControls.values()) {
      if (ctrl.value !== null && ctrl.value !== '') return true;
    }
    return false;
  }

  onSearchDebounced(): void {
    if (this.searchDebounceTimer) clearTimeout(this.searchDebounceTimer);
    this.searchDebounceTimer = setTimeout(() => {
      this.searchChange.emit(this.searchControl.value);
    }, 300);
  }

  onFilterChange(key: string): void {
    const filters: Record<string, any> = {};
    this.filterControls.forEach((ctrl, k) => {
      if (ctrl.value !== null && ctrl.value !== '') {
        filters[k] = ctrl.value;
      }
    });
    this.filterChange.emit(filters);
  }

  clearFilters(): void {
    this.searchControl.reset('');
    this.filterControls.forEach((ctrl) => ctrl.reset(null));
    this.searchChange.emit('');
    this.filterChange.emit({});
  }

  onSortChange(sort: Sort): void {
    if (sort.active && sort.direction) {
      this.sortChange.emit({ active: sort.active, direction: sort.direction as 'asc' | 'desc' });
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageChange.emit({ pageIndex: event.pageIndex, pageSize: event.pageSize });
  }

  openAddModal(): void {
    this.editMode.set(false);
    this.selectedEntity.set(null);
    const data: CrudModalData = {
      editMode: false,
      entity: null,
      formFields: this.config().formFields,
      supportsToggle: this.config().supportsToggle,
      entityName: this.config().entityName,
    };
    this.dialogRef = this.dialog.open(CrudModalComponent, { data });
    this.handleModalClose();
  }

  openEditModal(row: any): void {
    this.edit.emit(row);
    this.editMode.set(true);
    this.selectedEntity.set(row);
    const data: CrudModalData = {
      editMode: true,
      entity: row,
      formFields: this.config().formFields,
      supportsToggle: this.config().supportsToggle,
      entityName: this.config().entityName,
    };
    this.dialogRef = this.dialog.open(CrudModalComponent, { data });
    this.handleModalClose();
  }

  private handleModalClose(): void {
    this.dialogRef!.afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result: CrudModalResult | null) => {
        if (result) {
          this.save.emit({ formValue: result.formValue, isActive: result.isActive, entityId: result.entityId });
        }
        // Reset modal state
        this.modalError.set(null);
        this.selectedEntity.set(null);
      });
  }
}


# /Frontend/src/app/shared/components/notification-snackbar/notification-snackbar.component.html

<div class="notification-container">
  <div class="icon-container">
    <mat-icon>notifications_active</mat-icon>
  </div>
  <div class="content">
    <strong>{{ data.title }}</strong>
    <p>{{ data.message }}</p>
  </div>
  <button mat-icon-button class="close-btn" (click)="snackBarRef.dismiss()">
    <mat-icon>close</mat-icon>
  </button>
</div>


# /Frontend/src/app/shared/components/notification-snackbar/notification-snackbar.component.scss

.notification-container {
  display: flex;
  align-items: center;
  gap: 12px;
  background-color: #2e7d32; // green
  color: white;
  padding: 8px 16px;
  border-radius: 8px;
  max-width: 400px;
  .icon-container {
    mat-icon { font-size: 24px; }
  }
  .content {
    flex: 1;
    strong { font-size: 0.95rem; }
    p { font-size: 0.85rem; margin: 4px 0 0; }
  }
  .close-btn {
    color: white;
    margin-left: auto;
  }
}


# /Frontend/src/app/shared/components/notification-snackbar/notification-snackbar.component.ts

import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';

@Component({
  selector: 'app-notification-snackbar',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './notification-snackbar.component.html',
  styleUrls: ['./notification-snackbar.component.scss']
})
export class NotificationSnackbarComponent {
  data: { title: string; message: string } = inject(MAT_SNACK_BAR_DATA);
  snackBarRef = inject(MatSnackBarRef);
}


# /Frontend/src/app/shared/components/profile/profile.component.html

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


# /Frontend/src/app/shared/components/profile/profile.component.scss

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


# /Frontend/src/app/shared/components/profile/profile.component.ts

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


# /Frontend/src/app/shared/components/task-dashboard/task-dashboard.component.html

<div class="task-dashboard">
  <!-- Summary Cards Row -->
  <div class="summary-row">
    @for (card of summaryCards(); track card.status) {
    <mat-card
      class="summary-card"
      [class.active]="statusFilter() === card.status"
      (click)="setStatusFilter(card.status)"
    >
      <mat-card-header>
        <mat-card-title>{{ card.label }}</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <span class="count">{{ card.count }}</span>
      </mat-card-content>
    </mat-card>
    }
  </div>

  <!-- Status Filter Dropdown -->
  <div class="filter-bar">
    <mat-form-field appearance="outline">
      <mat-label>Status</mat-label>
      <mat-select
        [formControl]="statusFilterControl"
        (selectionChange)="onStatusFilterChange($event.value)"
      >
        @for (opt of config().statusOptions; track opt.value) {
        <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
        }
      </mat-select>
    </mat-form-field>
  </div>

  <!-- Loading / Error / Table or future Kanban -->
  @if (loading() && data().length === 0) {
  <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchData()"
    >
      Retry
    </button>
  </app-alert>
  } @if (viewMode() === 'table') { @if (data().length > 0 || loading()) { @if
  (loading()) {
  <mat-progress-bar mode="indeterminate"></mat-progress-bar>
  }
  <table
    mat-table
    [dataSource]="data()"
    matSort
    matSortDisableClear
    (matSortChange)="onSortChange($event)"
    aria-label="Tasks"
  >
    <ng-container matColumnDef="id">
      <th
        mat-header-cell
        *matHeaderCellDef
        mat-sort-header="id"
      >
        ID
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ t.id }}
      </td>
    </ng-container>
    <ng-container matColumnDef="location">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Location
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ config().getLocation(t) }}
      </td>
    </ng-container>
    <ng-container matColumnDef="description">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Description
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        {{ config().getDescription(t) }}
      </td>
    </ng-container>
    <ng-container matColumnDef="status">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Status
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        <span
          class="status-chip"
          [class]="t.status"
          >{{ t.status }}</span
        >
      </td>
    </ng-container>
    <ng-container matColumnDef="actions">
      <th
        mat-header-cell
        *matHeaderCellDef
      >
        Actions
      </th>
      <td
        mat-cell
        *matCellDef="let t"
      >
        <button
          mat-icon-button
          (click)="openDetail(t); $event.stopPropagation();"
          aria-label="View details"
        >
          <mat-icon>visibility</mat-icon>
        </button>
      </td>
    </ng-container>
    <tr
      mat-header-row
      *matHeaderRowDef="displayedColumns"
    ></tr>
    <tr
      mat-row
      *matRowDef="let row; columns: displayedColumns"
      (click)="openDetail(row)"
      class="clickable-row"
    ></tr>
  </table>
  <mat-paginator
    [length]="totalCount()"
    [pageIndex]="pageIndex()"
    [pageSize]="pageSize()"
    [pageSizeOptions]="[10, 25, 50]"
    (page)="onPageChange($event)"
  >
  </mat-paginator>
  } @else {
  <div class="empty-state">
    <p>No {{ config().entityName }}s found.</p>
  </div>
  } } @else {
  <!-- Future Kanban placeholder -->
  <p>Kanban view coming soon.</p>
  }
</div>


# /Frontend/src/app/shared/components/task-dashboard/task-dashboard.component.scss

.task-dashboard {
  display: flex;
  flex-direction: column;
  gap: 20px;

  .summary-row {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;

    .summary-card {
      flex: 1 1 200px;
      cursor: pointer;
      transition: all 0.2s ease-in-out;
      border: 2px solid transparent;

      &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
      }

      &.active {
        border-color: var(--mdc-theme-primary, #3f51b5);
        background-color: rgba(63, 81, 181, 0.04);
      }

      mat-card-title {
        font-size: 1rem;
        font-weight: 500;
        color: rgba(0, 0, 0, 0.6);
      }

      .count {
        font-size: 2rem;
        font-weight: 600;
        display: block;
        margin-top: 8px;
        color: rgba(0, 0, 0, 0.87);
      }
    }
  }

  .filter-bar {
    display: flex;
    justify-content: flex-start;
    margin-top: 8px;

    mat-form-field {
      width: 200px;
    }
  }

  table {
    width: 100%;
    margin-top: 8px;
    border: 1px solid rgba(0, 0, 0, 0.08);
    border-radius: 4px;
    overflow: hidden;

    .clickable-row {
      cursor: pointer;

      &:hover {
        background-color: rgba(0, 0, 0, 0.03);
      }
    }
  }

  .status-chip {
    display: inline-block;
    padding: 2px 8px;
    border-radius: 12px;
    font-size: 0.85rem;
    font-weight: 500;
    text-transform: capitalize;

    // Default fallbacks
    background-color: #e0e0e0;
    color: #424242;

    &.Pending {
      background-color: #fff3e0;
      color: #e65100;
    }

    &.InProgress, &.Preparing {
      background-color: #e3f2fd;
      color: #1565c0;
    }

    &.Completed, &.Delivered {
      background-color: #e8f5e9;
      color: #2e7d32;
    }
  }

  .empty-state {
    text-align: center;
    padding: 48px;
    color: rgba(0, 0, 0, 0.54);
    background-color: #fafafa;
    border-radius: 8px;
    border: 1px dashed rgba(0, 0, 0, 0.12);
  }
}

// Mobile responsive layouts
@media (max-width: 599px) {
  .task-dashboard {
    .summary-row {
      .summary-card {
        flex: 1 1 100%;
      }
    }

    table {
      display: block;
      overflow-x: auto;
      white-space: nowrap;
    }
  }
}


# /Frontend/src/app/shared/components/task-dashboard/task-dashboard.component.ts

import { Component, inject, signal, computed, DestroyRef, input, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { finalize, forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { Task, TaskDashboardConfig } from '../../models/task.model';
import { AlertComponent } from '../../../features/auth/components/alert.component';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { TaskDetailDialogComponent } from './task-detail-dialog.component';

@Component({
  selector: 'app-task-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDialogModule,
    MatSnackBarModule,
    AlertComponent,
  ],
  templateUrl: './task-dashboard.component.html',
  styleUrls: ['./task-dashboard.component.scss'],
})
export class TaskDashboardComponent {
  config = input.required<TaskDashboardConfig<any>>();
  viewMode = input<'table' | 'kanban'>('table');
  refresh = input(0);

  // Data
  data = signal<Task[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Pagination & sorting
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('id');
  sortDescending = signal(false);

  // Status filter
  statusFilter = signal('All');
  statusFilterControl = new FormControl('All', { nonNullable: true });

  // Summary cards
  summaryCards = signal<{ status: string; label: string; count: number }[]>([]);

  // Table columns
  displayedColumns = ['id', 'location', 'description', 'status', 'actions'];

  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.setupRefreshEffect();
  }

  /**
   * Data fetching is driven by the `refresh` input via an effect.
   * Increment the signal/input in the parent component to trigger a reload.
   */
  private setupRefreshEffect(): void {
    effect(() => {
      this.refresh(); // read to track
      this.pageIndex.set(0); // reset to first page when refreshed
      this.fetchData();
      this.refreshSummaryCounts();
    });
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    const params: any = {
      pageNumber: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      sortDescending: this.sortDescending(),
    };
    if (this.statusFilter() !== 'All') {
      params.status = this.statusFilter();
    }
    this.config()
      .fetchTasks(params)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.data);
          this.totalCount.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
          }
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  refreshSummaryCounts(): void {
    const statuses = this.config().statusOptions.filter((s) => s.value !== 'All');
    const requests = statuses.map((s) =>
      this.config()
        .fetchTasks({ pageNumber: 1, pageSize: 1, status: s.value })
        .pipe(
          map((res) => ({ status: s.value, label: s.label, count: res.totalCount })),
          catchError(() => of({ status: s.value, label: s.label, count: 0 }))
        )
    );
    forkJoin(requests)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((cards) => {
        this.summaryCards.set(cards);
      });
  }

  setStatusFilter(status: string): void {
    const newStatus = this.statusFilter() === status ? 'All' : status;
    this.statusFilter.set(newStatus);
    this.statusFilterControl.setValue(newStatus);
    this.pageIndex.set(0);
    this.fetchData();
  }

  onStatusFilterChange(value: string): void {
    this.statusFilter.set(value);
    this.pageIndex.set(0);
    this.fetchData();
  }

  onSortChange(event: Sort): void {
    this.sortField.set(event.active);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.fetchData();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.fetchData();
  }

  openDetail(task: Task): void {
    const config = this.config();
    const activeStatuses = config.statusOptions.filter((s) => s.value !== 'All').map((s) => s.value);
    const pendingVal = activeStatuses[0] ?? 'Pending';
    const inProgressVal = activeStatuses[1] ?? 'InProgress';
    const completedVal = activeStatuses[2] ?? 'Completed';

    const dialogRef = this.dialog.open(TaskDetailDialogComponent, {
      data: {
        task,
        detailSections: config.getDetailSections(task),
        canStart: task.status === pendingVal,
        canComplete: task.status === inProgressVal,
        inProgressStatus: inProgressVal,
        completedStatus: completedVal,
      },
      width: '90vw',
      maxWidth: '500px',
    });

    dialogRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result: { newStatus: string } | null) => {
        if (result) {
          this.updateStatus(task.id, result.newStatus);
        }
      });
  }

  updateStatus(id: number, newStatus: string): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Status Change',
        message: `Are you sure you want to transition this task status to ${newStatus}?`,
      },
    });

    dialogRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (confirmed) {
          this.loading.set(true);
          this.config()
            .updateTaskStatus(id, newStatus)
            .pipe(
              finalize(() => this.loading.set(false)),
              takeUntilDestroyed(this.destroyRef)
            )
            .subscribe({
              next: () => {
                this.snackBar.open('Task status updated successfully.', 'Close', { duration: 3000 });
                this.fetchData();
                this.refreshSummaryCounts();
              },
              error: (err) => {
                this.snackBar.open(
                  'Failed to update task status: ' + (err.error?.message || err.message),
                  'Close',
                  { duration: 5000 }
                );
              },
            });
        }
      });
  }

  private extractErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'An unexpected error occurred.';
  }
}


# /Frontend/src/app/shared/components/task-dashboard/task-detail-dialog.component.html

<h2 mat-dialog-title>
  {{ data.task.type ? data.task.type + ' #' : '' }}{{ data.task.id }}
</h2>
<mat-dialog-content>
  @for (section of data.detailSections; track section.title) {
  <div class="detail-section" style="margin-bottom: 12px; margin-top: 12px;">
    <h3 style="margin-bottom: 8px; font-weight: 500; font-size: 1.05rem;">{{ section.title }}</h3>
    @for (field of section.fields; track field.label) {
    <p style="margin: 4px 0;"><strong>{{ field.label }}:</strong> {{ field.value }}</p>
    }
  </div>
  <mat-divider *ngIf="!$last"></mat-divider>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  @if (data.canStart) {
  <button
    mat-raised-button
    color="primary"
    (click)="start()"
    style="margin-right: 8px;"
  >
    Start
  </button>
  } @if (data.canComplete) {
  <button
    mat-raised-button
    color="accent"
    (click)="complete()"
    style="margin-right: 8px;"
  >
    Complete
  </button>
  }
  <button
    mat-button
    mat-dialog-close
  >
    Close
  </button>
</mat-dialog-actions>


# /Frontend/src/app/shared/components/task-dashboard/task-detail-dialog.component.ts

import { Component, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';

import { Task, DetailSection } from '../../models/task.model';

@Component({
  selector: 'app-task-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatDividerModule,
  ],
  templateUrl: './task-detail-dialog.component.html',
})
export class TaskDetailDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<TaskDetailDialogComponent>);

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: {
      task: any;
      detailSections: DetailSection[];
      canStart: boolean;
      canComplete: boolean;
      inProgressStatus: string;
      completedStatus: string;
    }
  ) {}

  start(): void {
    this.dialogRef.close({ newStatus: this.data.inProgressStatus });
  }

  complete(): void {
    this.dialogRef.close({ newStatus: this.data.completedStatus });
  }
}


# /Frontend/src/app/shared/models/crud-config.model.ts

import { Signal, TemplateRef, WritableSignal } from '@angular/core';

export interface FilterOption {
  label: string;
  value: any;
}

export interface FilterDef {
  key: string;
  label: string;
  options: FilterOption[];
}

export interface ColumnDef {
  field: string;
  header: string;
  sortable: boolean;
  getValue: (row: any) => string;
  cellTemplate?: TemplateRef<any>;
  width?: string;
}

export interface FormFieldDef {
  key: string;
  label: string;
  type:
    | 'text'
    | 'number'
    | 'email'
    | 'password'
    | 'textarea'
    | 'date'
    | 'url'
    | 'select'
    | 'toggle'
    | 'keyValueList'
    | 'imageUrlList';
  options?: FilterOption[];
  validators?: any[];
  showInAdd?: boolean;
  showInEdit?: boolean;
}

export interface CrudConfig<T> {
  entityName: string;
  entityNamePlural: string;
  columns: ColumnDef[];
  formFields: FormFieldDef[];
  filters: FilterDef[];
  supportsToggle: boolean;
  data: Signal<T[]>;
  loading: Signal<boolean>;
  error: WritableSignal<string | null>;
  totalCount: Signal<number>;
  pageIndex: Signal<number>;
  pageSize: Signal<number>;
}

export interface CrudModalData {
  editMode: boolean;
  entity: any | null;
  formFields: FormFieldDef[];
  supportsToggle: boolean;
  entityName?: string;
}

export interface CrudModalResult {
  formValue: any;
  isActive: boolean;
  previousIsActive: boolean;
  entityId?: number;
}



# /Frontend/src/app/shared/models/task.model.ts

import { Observable } from 'rxjs';

export interface Task {
  id: number;
  status: string; // raw status from API (e.g., 'Pending', 'InProgress', 'Completed')
  location: string; // e.g., 'Room 201', 'Lobby', 'N/A'
  description: string; // e.g., 'AC not working', 'Order #123'
  createdAt: string; // ISO date
  raw: any; // original DTO for detail modal
}

export interface DetailSection {
  title: string; // e.g., 'Basic Information'
  fields: { label: string; value: string }[];
}

export interface TaskDashboardConfig<T extends Task = Task> {
  entityName: string; // 'Food Order', 'Housekeeping Task', etc.
  fetchTasks: (params: {
    pageNumber: number;
    pageSize: number;
    status?: string;
    sortBy?: string;
    sortDescending?: boolean;
  }) => Observable<{ totalCount: number; data: T[] }>;

  updateTaskStatus: (id: number, newStatus: string) => Observable<void>;

  statusOptions: { value: string; label: string }[]; // includes 'All' option

  getLocation: (task: T) => string;
  getDescription: (task: T) => string;
  getDetailSections: (task: T) => DetailSection[];
}


# /Frontend/src/environments/environment.development.ts

export const environment = {
  baseUrl: 'http://localhost:5264/api/v1'
};



# /Frontend/src/environments/environment.ts

export const environment = {
  baseUrl: 'http://localhost:5264/api/v1'
};



# /Frontend/src/index.html

<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>Frontend</title>
    <base href="/" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="icon" type="image/x-icon" href="favicon.ico" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link
      href="https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500&display=swap"
      rel="stylesheet"
    />
    <link href="https://fonts.googleapis.com/icon?family=Material+Icons" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Manrope:wght@300;500&family=Playfair+Display:ital,wght@0,400;1,400&display=swap" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&display=swap" rel="stylesheet" />
  </head>
  <body>
    <app-root></app-root>
  </body>
</html>


# /Frontend/src/main.ts

import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));


# /Frontend/src/styles.scss

// Include theming for Angular Material with `mat.theme()`.
@use '@angular/material' as mat;

@import 'styles/theme/index';

html {
  height: 100%;
  -webkit-text-size-adjust: 100%;
  @include mat.theme(
    (
      color: (
        primary: mat.$azure-palette,
        tertiary: mat.$blue-palette,
      ),
      typography: Roboto,
      density: 0,
    )
  );
}

*, *::before, *::after {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  background-color: var(--color-background);
  color: var(--color-on-background);
  @include font-body-md;
  overflow-x: hidden;
  -webkit-font-smoothing: antialiased;
  height: 100%;
}

// Hide scrollbar
::-webkit-scrollbar { display: none; }
* { -ms-overflow-style: none; scrollbar-width: none; }

// Selection
::-selection {
  background: rgba(228, 194, 133, 0.3);
  color: var(--color-secondary);
}

.table-section {
  max-width: 100%;
  overflow-x: auto;
}

// Prevent text inflation and improve touch usability (spec §5.1)
html,
body {
  touch-action: manipulation;
}

// Prevent media elements from causing overflow (spec §5.1)
img,
video,
canvas,
svg {
  max-width: 100%;
  height: auto;
}

// Utility
.sr-only { // for screen reader only
  position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0,0,0,0); border: 0;
}

@media (max-width: 500px) {
  mat-form-field,
  mat-button-toggle-group,
  .mat-button-toggle-group {
    width: 100%;
  }

  .table-section,
  .mat-table-container {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .mat-card,
  .kpi-card,
  .health-cards .mat-card {
    margin: 8px 0;
    padding: 12px;
  }
}

@media (max-width: 360px) {
  body {
    font-size: 14px;
  }
  mat-card {
    margin: 4px;
    padding: 8px;
  }
}

.notification-snackbar {
  background: transparent !important;
  box-shadow: none !important;
  .mat-mdc-snackbar-surface { background: transparent; box-shadow: none; }
}

// ── Material Datepicker theme overrides ────────────────
.mat-datepicker-content {
  background-color: var(--color-surface-container) !important;
  border: 1px solid rgba(228, 194, 133, 0.3) !important;
  color: var(--color-on-surface) !important;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5) !important;
  border-radius: 12px !important;
  backdrop-filter: blur(15px) !important;
  -webkit-backdrop-filter: blur(15px) !important;
  
  .mat-calendar {
    font-family: var(--font-body) !important;
    background-color: transparent !important;
  }
  
  .mat-calendar-header {
    color: var(--color-secondary) !important;
    .mat-calendar-arrow {
      fill: var(--color-secondary) !important;
    }
    .mat-calendar-previous-button, 
    .mat-calendar-next-button {
      color: var(--color-secondary) !important;
    }
  }

  .mat-calendar-table-header {
    color: var(--color-outline) !important;
  }

  .mat-calendar-body-label {
    color: var(--color-secondary) !important;
  }

  .mat-calendar-body-cell-content {
    color: var(--color-on-surface) !important;
    border-radius: 50% !important;
    border: 1px solid transparent !important;
    transition: all 0.2s ease-in-out !important;
    &:hover {
      background-color: rgba(228, 194, 133, 0.1) !important;
      color: var(--color-secondary) !important;
      border-color: rgba(228, 194, 133, 0.3) !important;
    }
  }

  .mat-calendar-body-selected {
    background-color: var(--color-secondary) !important;
    color: var(--color-on-secondary) !important;
    font-weight: 600 !important;
  }

  .mat-calendar-body-today {
    border-color: var(--color-secondary) !important;
    background-color: rgba(228, 194, 133, 0.05) !important;
  }

  // Active hover/focus visual states
  .mat-calendar-body-active {
    .mat-calendar-body-cell-content {
      border-color: var(--color-secondary) !important;
    }
  }
}


# /Frontend/src/styles/theme/_colors.scss

:root {
  --color-surface: #131411;
  --color-surface-dim: #131411;
  --color-surface-bright: #393936;
  --color-surface-container-lowest: #0e0e0c;
  --color-surface-container-low: #1b1c19;
  --color-surface-container: #1f201d;
  --color-surface-container-high: #2a2a27;
  --color-surface-container-highest: #353532;
  --color-on-surface: #e4e2dd;
  --color-on-surface-variant: #c4c7c7;
  --color-outline: #8e9192;
  --color-outline-variant: #444748;
  --color-primary: #c9c6c5;
  --color-on-primary: #313030;
  --color-primary-container: #0a0a0a;
  --color-on-primary-container: #7b7979;
  --color-secondary: #e4c285;
  --color-on-secondary: #412d00;
  --color-secondary-container: #5d4514;
  --color-on-secondary-container: #d5b478;
  --color-background: #131411;
  --color-on-background: #e4e2dd;
  --color-tertiary: #c8c6c5;
  --color-on-tertiary: #313030;
  --color-tertiary-container: #0a0a0a;
  --color-on-tertiary-container: #7a7979;
  --color-error: #ffb4ab;
  --color-on-error: #690005;
  --color-error-container: #93000a;
  --color-on-error-container: #ffdad6;
  // Glassmorphism
  --glass-bg: rgba(26, 26, 26, 0.7);
  --glass-border: rgba(228, 194, 133, 0.2);
}


# /Frontend/src/styles/theme/_glassmorphism.scss

@mixin glass-panel {
  background: var(--glass-bg);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid var(--glass-border);
}


# /Frontend/src/styles/theme/_index.scss

@forward 'colors';
@forward 'typography';
@forward 'spacing';
@forward 'glassmorphism';
@forward 'mixins';


# /Frontend/src/styles/theme/_mixins.scss

@mixin gold-underline {
  position: relative;
  padding-bottom: 4px;
  &::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 1px;
    background-color: var(--color-secondary);
    transition: width 0.5s ease;
  }
  &:hover::after { width: 0%; }
}
@mixin hover-scale-img {
  img {
    transition: transform 1.2s cubic-bezier(0.2, 0, 0.2, 1);
  }
  &:hover img { transform: scale(1.05); }
}
@mixin underline-reveal {
  position: relative;
  overflow: hidden;
  &::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 1px;
    background: var(--color-secondary);
    transform: scaleX(0);
    transform-origin: right;
    transition: transform 0.6s cubic-bezier(0.19, 1, 0.22, 1);
  }
  &:hover::after {
    transform: scaleX(1);
    transform-origin: left;
  }
}


# /Frontend/src/styles/theme/_spacing.scss

:root {
  --space-unit: 8px;
  --gutter: 32px;
  --margin-desktop: 80px;
  --margin-mobile: 24px;
  --section-gap: 160px;
  --container-max: 1440px;
}


# /Frontend/src/styles/theme/_typography.scss

:root {
  --font-headline: 'Playfair Display', serif;
  --font-body: 'Manrope', sans-serif;

  --fs-display-lg: 72px;
  --fs-display-lg-mobile: 40px;
  --fs-headline-md: 32px;
  --fs-headline-sm: 24px;
  --fs-body-lg: 18px;
  --fs-body-md: 16px;
  --fs-label-caps: 12px;

  --lh-display-lg: 1.1;
  --lh-display-lg-mobile: 1.2;
  --lh-headline-md: 1.3;
  --lh-headline-sm: 1.4;
  --lh-body-lg: 1.6;
  --lh-body-md: 1.6;
  --lh-label-caps: 1.0;

  --ls-display-lg: -0.02em;
  --ls-display-lg-mobile: -0.01em;
  --ls-body-lg: 0.02em;
  --ls-body-md: 0.01em;
  --ls-label-caps: 0.2em;
}

// Mixins
@mixin font-display-lg {
  font-family: var(--font-headline);
  font-size: var(--fs-display-lg);
  font-weight: 400;
  line-height: var(--lh-display-lg);
  letter-spacing: var(--ls-display-lg);
}
@mixin font-display-lg-mobile {
  font-family: var(--font-headline);
  font-size: var(--fs-display-lg-mobile);
  font-weight: 400;
  line-height: var(--lh-display-lg-mobile);
  letter-spacing: var(--ls-display-lg-mobile);
}
@mixin font-headline-md {
  font-family: var(--font-headline);
  font-size: var(--fs-headline-md);
  font-weight: 400;
  line-height: var(--lh-headline-md);
}
@mixin font-headline-sm {
  font-family: var(--font-headline);
  font-size: var(--fs-headline-sm);
  font-weight: 400;
  line-height: var(--lh-headline-sm);
}
@mixin font-body-lg {
  font-family: var(--font-body);
  font-size: var(--fs-body-lg);
  font-weight: 300;
  line-height: var(--lh-body-lg);
  letter-spacing: var(--ls-body-lg);
}
@mixin font-body-md {
  font-family: var(--font-body);
  font-size: var(--fs-body-md);
  font-weight: 300;
  line-height: var(--lh-body-md);
  letter-spacing: var(--ls-body-md);
}
@mixin font-label-caps {
  font-family: var(--font-body);
  font-size: var(--fs-label-caps);
  font-weight: 500;
  line-height: var(--lh-label-caps);
  letter-spacing: var(--ls-label-caps);
  text-transform: uppercase;
}
