import { CommonModule } from '@angular/common';
import { Component, inject, signal, DestroyRef } from '@angular/core';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { BreakpointObserver, LayoutModule } from '@angular/cdk/layout';
import { map, filter } from 'rxjs/operators';
import { toSignal, takeUntilDestroyed } from '@angular/core/rxjs-interop';
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
    LayoutModule,
  ],
  templateUrl: './housekeeping-shell.component.html',
  styleUrls: ['./housekeeping-shell.component.scss'],
})
export class HousekeepingShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  sidebarOpen = signal(false);
  roleTitle = 'Housekeeping';
  role = 'housekeeping';
  userDisplayName = this.authService.fullName;
  title = signal('Dashboard');

  constructor() {
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
    const url = this.router.url;
    if (url.includes('/profile')) {
      this.title.set('Profile');
    } else {
      this.title.set('Dashboard');
    }
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
