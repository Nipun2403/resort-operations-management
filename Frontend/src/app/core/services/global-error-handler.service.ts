import { ErrorHandler, Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly router = inject(Router);

  handleError(error: unknown): void {
    const currentUrl = this.router.url || '';
    if (currentUrl.startsWith('/error')) {
      return;
    }

    queueMicrotask(() => {
      this.router.navigate(['/error', 500], { replaceUrl: true });
    });
  }
}
