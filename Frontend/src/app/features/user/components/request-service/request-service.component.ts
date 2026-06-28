import { Component, OnInit, inject, signal, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HousekeepingApiService } from '../../services/housekeeping-api.service';
import { MaintenanceApiService } from '../../services/maintenance-api.service';
import { Booking } from '../../../../features/admin/models/booking.model';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-request-service',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonToggleModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './request-service.component.html',
  styleUrls: ['./request-service.component.scss']
})
export class RequestServiceComponent implements OnInit {
  activeBooking = input.required<Booking>();
  requestCreated = output<void>();

  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly snackBar = inject(MatSnackBar);

  requestType = new FormControl<'housekeeping' | 'maintenance'>('housekeeping', { nonNullable: true });
  selectedRoomId = new FormControl<number>(0, { nonNullable: true, validators: [Validators.required] });
  description = new FormControl<string>('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(5)]
  });

  submitting = signal(false);

  ngOnInit(): void {
    const rooms = this.activeBooking().rooms || [];
    if (rooms.length > 0 && rooms[0].roomId != null) {
      this.selectedRoomId.setValue(rooms[0].roomId);
    }
  }

  submitRequest(): void {
    if (this.description.invalid || this.submitting()) {
      this.description.markAsTouched();
      return;
    }

    const roomId = this.selectedRoomId.value;
    if (!roomId) return;

    this.submitting.set(true);
    const type = this.requestType.value;
    const desc = this.description.value;

    const request$ = type === 'housekeeping'
      ? this.housekeepingApi.trigger(roomId, { description: desc })
      : this.maintenanceApi.trigger(roomId, { description: desc });

    request$.pipe(
      finalize(() => this.submitting.set(false))
    ).subscribe({
      next: () => {
        this.snackBar.open(`${type === 'housekeeping' ? 'Housekeeping' : 'Maintenance'} request submitted successfully.`, 'Close', {
          duration: 4000
        });
        this.description.reset('');
        this.requestCreated.emit();
      },
      error: (err) => {
        const msg = err.error?.message || err.message || 'Failed to submit request.';
        this.snackBar.open(msg, 'Close', { duration: 5000 });
      }
    });
  }
}
