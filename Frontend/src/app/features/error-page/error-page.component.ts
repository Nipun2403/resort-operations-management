import { CommonModule } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

type ErrorStatus = 403 | 404 | 500;

interface ErrorPageCopy {
  code: ErrorStatus;
  eyebrow: string;
  title: string;
  message: string;
  actionLabel: string;
}

@Component({
  selector: 'app-error-page',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule],
  templateUrl: './error-page.component.html',
  styleUrls: ['./error-page.component.scss'],
})
export class ErrorPageComponent {
  private readonly route = inject(ActivatedRoute);

  readonly copyMap: Record<ErrorStatus, ErrorPageCopy> = {
    403: {
      code: 403,
      eyebrow: 'Private Vault',
      title: 'Access Restricted',
      message: 'This area is reserved for guests and staff. If you believe you should have access, please contact the concierge or return to a permitted area.',
      actionLabel: 'Return to Home',
    },
    404: {
      code: 404,
      eyebrow: 'Hidden Corridor',
      title: 'Page Not Found',
      message: 'The corridor you are seeking does not exist. It may have been moved, removed, or never existed at all.',
      actionLabel: 'Return to Home',
    },
    500: {
      code: 500,
      eyebrow: 'System Fault',
      title: 'Unexpected Error',
      message: 'The command centre encountered an unexpected fault. Please try again in a moment or return to a safe section of the app.',
      actionLabel: 'Return to Home',
    },
  };

  readonly current = this.resolveCopy();
  readonly accentCode = this.current.code.toString();

  @HostListener('document:mousemove', ['$event'])
  onMouseMove(event: MouseEvent): void {
    const x = (event.clientX / window.innerWidth) * 100;
    const y = (event.clientY / window.innerHeight) * 100;
    document.documentElement.style.setProperty('--error-glow-x', `${x}%`);
    document.documentElement.style.setProperty('--error-glow-y', `${y}%`);
  }

  private resolveCopy(): ErrorPageCopy {
    const raw = Number(this.route.snapshot.paramMap.get('status'));
    if (raw === 403 || raw === 404 || raw === 500) {
      return this.copyMap[raw];
    }
    return this.copyMap[500];
  }
}
