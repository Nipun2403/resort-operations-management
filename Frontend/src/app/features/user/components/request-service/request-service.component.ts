import { Component, OnInit, inject, signal, input, output, DestroyRef } from '@angular/core';
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
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { HousekeepingApiService } from '../../services/housekeeping-api.service';
import { MaintenanceApiService } from '../../services/maintenance-api.service';
import { Booking } from '../../../../features/admin/models/booking.model';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

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
    MatSnackBarModule,
    MatDialogModule
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
  private readonly dialog = inject(MatDialog);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 599px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

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

    const room = this.activeBooking().rooms.find(r => r.roomId === roomId);
    const roomLabel = room?.roomNumber ?? 'selected room';
    const typeLabel = this.requestType.value === 'housekeeping' ? 'Housekeeping' : 'Maintenance';

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Service Request',
        message: `Send a ${typeLabel} request for ${roomLabel}?`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.performSubmit(roomId);
      }
    });
  }

  applyPreset(text: string): void {
    this.description.setValue(text);
    this.description.markAsDirty();
    this.description.markAsTouched();
  }

  private performSubmit(roomId: number): void {
    this.submitting.set(true);
    const type = this.requestType.value;
    const desc = this.description.value;

    const request$ = type === 'housekeeping'
      ? this.housekeepingApi.trigger(roomId, { description: desc })
      : this.maintenanceApi.trigger(roomId, { description: desc });

    request$.pipe(
      finalize(() => this.submitting.set(false)),
      takeUntilDestroyed(this.destroyRef)
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
