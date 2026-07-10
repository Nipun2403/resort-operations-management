import { Injectable, inject, OnDestroy, effect } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HttpTransportType, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
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

    const hubUrl = environment.signalrHubUrl;

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
