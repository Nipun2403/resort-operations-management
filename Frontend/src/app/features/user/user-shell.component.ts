import { CommonModule } from '@angular/common';
import { Component, inject, signal, computed } from '@angular/core';
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
import { ConciergeApiService, GuestContext } from './services/concierge-api.service';
import { ConciergeChatComponent } from './components/concierge-chat/concierge-chat.component';

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
    ConciergeChatComponent,
  ],
  templateUrl: './user-shell.component.html',
  styleUrls: ['./user-shell.component.scss'],
})
export class UserShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);
  private api = inject(ConciergeApiService);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  sidebarOpen = signal(false);
  showConcierge = signal(false);
  context = signal<GuestContext | null>(null);

  constructor() {
    // Load context on init
    this.api.getContext().subscribe({
      next: (ctx: GuestContext) => this.context.set(ctx)
    });
  }

  isMobileView = computed(() => this.isMobile());

  onNavClick(): void {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  toggleConcierge(): void {
    this.showConcierge.update(v => !v);
    if (this.showConcierge()) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  closeConcierge(): void {
    this.showConcierge.set(false);
    document.body.style.overflow = '';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }
}
