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
  readonly onNewFoodOrder = new Subject<NewTaskNotification>();
  readonly onNewHousekeepingTask = new Subject<NewTaskNotification>();
  readonly onNewMaintenanceTask = new Subject<NewTaskNotification>();

  constructor() {
    effect(() => {
      const token = this.authService.token();
      if (token) {
        this.startConnection();
      } else {
        this.stopConnection();
      }
    });

    this.onNewFoodOrder.pipe(takeUntilDestroyed()).subscribe(n => {
      this.showNotification('New Food Order', `Order #${n.id} received for room ${n.roomNumber || 'N/A'}`);
    });
    this.onNewHousekeepingTask.pipe(takeUntilDestroyed()).subscribe(n => {
      this.showNotification('New Housekeeping Task', `${n.description} for room ${n.roomNumber || 'N/A'}`);
    });
    this.onNewMaintenanceTask.pipe(takeUntilDestroyed()).subscribe(n => {
      this.showNotification('New Maintenance Task', `${n.description} for room ${n.roomNumber || 'N/A'}`);
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
