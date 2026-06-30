import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationService } from './core/services/notification.service';
import { CustomCursorComponent } from './shared/components/custom-cursor/custom-cursor.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CustomCursorComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('Frontend');
  private readonly notificationService = inject(NotificationService);
}
