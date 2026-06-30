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
