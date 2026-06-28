import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { CustomerBookingFacade } from '../facades/customer-booking.facade';
import { Booking } from '../../admin/models/booking.model';
import { AlertComponent } from '../../../features/auth/components/alert.component';
import { FoodOrderComponent } from '../components/food-order/food-order.component';
import { RequestServiceComponent } from '../components/request-service/request-service.component';
import { MyRequestsComponent } from '../components/my-requests/my-requests.component';

@Component({
  selector: 'app-customer-room-service',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTabsModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    AlertComponent,
    FoodOrderComponent,
    RequestServiceComponent,
    MyRequestsComponent
  ],
  templateUrl: './room-service.component.html',
  styleUrls: ['./room-service.component.scss']
})
export class RoomServiceComponent implements OnInit {
  private readonly facade = inject(CustomerBookingFacade);
  private readonly destroyRef = inject(DestroyRef);

  activeBooking = signal<Booking | null>(null);
  loadingActiveBooking = signal(false);
  activeBookingError = signal<string | null>(null);
  refreshRequests = signal(0);

  roomIds = computed(() => {
    const booking = this.activeBooking();
    return booking ? booking.rooms.map(r => r.roomId).filter(id => id != null) as number[] : [];
  });

  ngOnInit(): void {
    this.loadActiveBooking();
  }

  loadActiveBooking(): void {
    this.loadingActiveBooking.set(true);
    this.activeBookingError.set(null);

    this.facade.getActiveBooking().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loadingActiveBooking.set(false))
    ).subscribe({
      next: booking => this.activeBooking.set(booking),
      error: (err: any) => this.activeBookingError.set(this.extractErrorMessage(err))
    });
  }

  onOrderPlaced(): void {
    // Only show a snackbar or log – no need to refresh My Requests tab.
  }

  onRequestCreated(): void {
    this.refreshRequests.update(n => n + 1);
  }

  private extractErrorMessage(err: any): string {
    return err.error?.message || err.message || 'An unexpected error occurred.';
  }
}
