import { Component, input, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { MaintenanceApiService } from '../../../../user/services/maintenance-api.service';
import { BookingRoom } from '../../../../admin/models/booking.model';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-maintenance-request-panel',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatCheckboxModule,
  ],
  templateUrl: './maintenance-request-panel.component.html',
})
export class MaintenanceRequestPanelComponent implements OnInit {
  rooms = input.required<BookingRoom[]>();

  selectedRoomId = new FormControl<number | null>(null, {
    validators: Validators.required,
  });
  description = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(5)],
  });
  isEmergency = new FormControl(false, { nonNullable: true });
  submitting = signal(false);

  private mtApi = inject(MaintenanceApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    const validRooms = this.rooms().filter(r => r.roomId !== null);
    if (validRooms.length > 0) {
      this.selectedRoomId.setValue(validRooms[0].roomId);
    }
  }

  submit(): void {
    if (this.submitting() || this.selectedRoomId.invalid || this.description.invalid) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Confirm Request', message: 'Send a maintenance request for the selected room?' },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.submitting.set(true);

        const roomId = this.selectedRoomId.value!;
        this.mtApi
          .trigger(roomId, { description: this.description.value, isEmergency: this.isEmergency.value })
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.submitting.set(false))
          )
          .subscribe({
            next: () => {
              this.snackBar.open('Maintenance request sent', 'Close', { duration: 3000 });
              this.description.reset();
            },
            error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 }),
          });
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
