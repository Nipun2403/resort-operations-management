import { Component, HostListener, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-public-shell',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './public-shell.component.html',
  styleUrls: ['./public-shell.component.scss']
})
export class PublicShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  readonly authService = inject(AuthService);

  readonly dashboardRoute = computed(() => {
    const roleRoutes: Record<string, string> = {
      Admin: '/operations/admin',
      FrontDesk: '/operations/front-desk',
      Kitchen: '/operations/kitchen',
      Housekeeping: '/operations/housekeeping',
      Maintenance: '/operations/maintenance',
      RegisteredUser: '/user/dashboard',
    };
    return roleRoutes[this.authService.role() ?? ''] ?? '/user/dashboard';
  });

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
