import { Component, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { HousekeepingApiService } from '../../../../user/services/housekeeping-api.service';
import { MaintenanceApiService } from '../../../../user/services/maintenance-api.service';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-internal-ticket-panel',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonToggleModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
  ],
  templateUrl: './internal-ticket-panel.component.html',
})
export class InternalTicketPanelComponent {
  ticketType = new FormControl<'housekeeping' | 'maintenance'>('housekeeping', {
    nonNullable: true,
  });
  location = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(200)],
  });
  description = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(5)],
  });
  submitting = signal(false);

  private hkApi = inject(HousekeepingApiService);
  private mtApi = inject(MaintenanceApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  submit(): void {
    if (this.submitting() || this.location.invalid || this.description.invalid) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Confirm Ticket', message: `Create an internal ${this.ticketType.value} ticket?` },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.submitting.set(true);

        const body = { location: this.location.value, description: this.description.value };
        const request$ =
          this.ticketType.value === 'housekeeping'
            ? this.hkApi.createInternal(body)
            : this.mtApi.createInternal(body);

        request$
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.submitting.set(false))
          )
          .subscribe({
            next: () => {
              this.snackBar.open('Internal ticket created', 'Close', { duration: 3000 });
              this.location.reset();
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
