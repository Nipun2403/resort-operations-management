import { ApplicationConfig, ErrorHandler, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorPageInterceptor } from './core/interceptors/error-page.interceptor';
import { idempotencyInterceptor } from './core/interceptors/idempotency.interceptor';
import { routes } from './app.routes';
import { provideEchartsCore } from 'ngx-echarts';
import * as echarts from 'echarts';
import { GlobalErrorHandler } from './core/services/global-error-handler.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorPageInterceptor, idempotencyInterceptor])),
    provideAnimationsAsync(),
    provideEchartsCore({ echarts }),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
  ],
};
