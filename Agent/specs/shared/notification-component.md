# Specsheet A: SignalR Notification Service & Snackbar Component

## 1. Purpose
- Create a shared `NotificationService` that maintains a SignalR WebSocket connection to `ws://localhost:5264/notifications`.
- The service exposes observables for three event types: `NewFoodOrder`, `NewHousekeepingTask`, `NewMaintenanceTask`.
- A beautiful custom snackbar component (`NotificationSnackbarComponent`) displays a coloured alert with icon, title, and message.
- The service provides a `showNotification(title, message, icon?)` method that opens the custom snackbar.

## 2. Files to Create
| File | Action |
|------|--------|
| `src/app/core/services/notification.service.ts` | New service |
| `src/app/shared/components/notification-snackbar/notification-snackbar.component.ts` | New component |
| `src/app/shared/components/notification-snackbar/notification-snackbar.component.html` | New template |
| `src/app/shared/components/notification-snackbar/notification-snackbar.component.scss` | New styles |

## 3. NotificationService

### 3.1 Dependencies
- `@microsoft/signalr` (must be installed; `npm install @microsoft/signalr` if not present)
- `AuthService` for JWT token
- `MatSnackBar` for snackbar
- `environment.baseUrl` for the WebSocket URL

### 3.2 Implementation (exact)

```typescript
// src/app/core/services/notification.service.ts
import { Injectable, inject, signal, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuthService } from './auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NotificationSnackbarComponent } from '../../../shared/components/notification-snackbar/notification-snackbar.component';

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
  readonly onNewFoodOrder = new Subject<NewTaskNotification>();
  readonly onNewHousekeepingTask = new Subject<NewTaskNotification>();
  readonly onNewMaintenanceTask = new Subject<NewTaskNotification>();

  constructor() {
    // Do not connect automatically; wait for login. We'll call startConnection() from app init or after login.
    // For simplicity, we'll connect in startConnection() which should be called after login.
  }

  startConnection(): void {
    if (this.hubConnection) return;
    const token = this.authService.token();
    if (!token) return;

    const wsUrl = environment.baseUrl.replace('http', 'ws') + '/notifications'; // build ws URL
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(wsUrl, { accessTokenFactory: () => token })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('NewFoodOrder', (data: any) => {
      this.onNewFoodOrder.next({
        id: data.id,
        type: 'FoodOrder',
        description: `Order #${data.id}`,
        roomNumber: data.roomNumber
      });
    });

    this.hubConnection.on('NewHousekeepingTask', (data: any) => {
      this.onNewHousekeepingTask.next({
        id: data.id,
        type: 'Housekeeping',
        description: data.description || `Housekeeping task #${data.id}`,
        roomNumber: data.roomNumber
      });
    });

    this.hubConnection.on('NewMaintenanceTask', (data: any) => {
      this.onNewMaintenanceTask.next({
        id: data.id,
        type: 'Maintenance',
        description: data.description || `Maintenance task #${data.id}`,
        roomNumber: data.roomNumber
      });
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

  // Show custom snackbar
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
```

**Important:** The `startConnection()` method should be called after the user logs in and the token is available. We can do this from a component that runs after login, e.g., the role dashboard, or from an APP_INITIALIZER that checks auth state. To keep it simple, the role dashboard pages will call `startConnection()` on init if authenticated. Since the service is singleton, it will connect only once. Alternatively, we can connect in the service constructor if token exists? But constructor runs at injection time, token may not be set yet. We'll call `startConnection()` from the role dashboard component on init after ensuring token exists.

## 4. NotificationSnackbarComponent

**Selector:** `app-notification-snackbar`  
**Standalone:** `true`  
**Imports:** `MatIconModule`, `MatButtonModule`, `CommonModule`.  
**Injected data:** via `MAT_SNACK_BAR_DATA` (title, message).

**Template (exact):**
```html
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
```

**Logic:**
```typescript
import { Component, Inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';

@Component({
  selector: 'app-notification-snackbar',
  standalone: true,
  imports: [MatIconModule, MatButtonModule, CommonModule],
  templateUrl: './notification-snackbar.component.html',
  styleUrls: ['./notification-snackbar.component.scss']
})
export class NotificationSnackbarComponent {
  data: { title: string; message: string } = inject(MAT_SNACK_BAR_DATA);
  snackBarRef = inject(MatSnackBarRef);
}
```

**SCSS:**
```scss
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
```

**Note:** The snackbar panel class can be styled globally to remove default background and shadow. Add to `styles.scss`:
```scss
.notification-snackbar {
  background: transparent !important;
  box-shadow: none !important;
  .mat-mdc-snackbar-surface { background: transparent; box-shadow: none; }
}
```

## 5. Self‑Review Checklist
- [ ] NotificationService compiles and exposes the three observables.
- [ ] `startConnection` connects to SignalR hub with JWT token.
- [ ] Incoming events push to corresponding Subject.
- [ ] Custom snackbar component renders with green background, icon, title, message, and close button.
- [ ] `showNotification` opens the snackbar at top‑right for 5 seconds.
- [ ] Connection stops on logout (via `stopConnection`).

## 6. Integration Notes
- The `environment.baseUrl` is something like `http://localhost:5264/api/v1`. We need to strip `/api/v1` and append `/notifications`. We'll construct: `baseUrl.replace(/\/api\/v1$/, '') + '/notifications'`.
- The SignalR events object structure is assumed; we'll use `data` fields as per backend. If the actual messages differ, the mapping in the service can be adjusted later.
- The service is provided in root; only one instance exists.

