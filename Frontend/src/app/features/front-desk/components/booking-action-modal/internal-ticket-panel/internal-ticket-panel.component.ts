import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs/operators';
import { HousekeepingApiService } from '../../../../admin/services/housekeeping-api.service';
import { MaintenanceApiService } from '../../../../admin/services/maintenance-api.service';
// import { MaintenanceApiService } from '../services/maintenance-api.service';
// Frontend / src / app / features / admin / services / housekeeping - api.service.ts;
@Component({
  selector: 'app-create-internal-ticket-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatRadioModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './internal-ticket-panel.component.html',
  styleUrls: ['./internal-ticket-panel.component.scss'],
})
export class InternalTicketPanelComponent {
  private readonly dialogRef = inject(MatDialogRef<InternalTicketPanelComponent>);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);

  form = new FormGroup({
    type: new FormControl<'housekeeping' | 'maintenance'>('housekeeping', Validators.required),
    location: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    description: new FormControl('', [Validators.required, Validators.minLength(5)]),
  });

  loading = signal(false);
  errorMessage = signal<string | null>(null);

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const { type, location, description } = this.form.value;
    const body = { location: location!, description: description! };

    this.loading.set(true);
    this.errorMessage.set(null);

    const request$ =
      type === 'maintenance'
        ? this.maintenanceApi.createInternal(body)
        : this.housekeepingApi.createInternal(body);

    request$.pipe(finalize(() => this.loading.set(false))).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) =>
        this.errorMessage.set(err.error?.message || 'Failed to create ticket. Please try again.'),
    });
  }
}
