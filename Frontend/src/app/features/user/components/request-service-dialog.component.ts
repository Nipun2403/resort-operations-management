import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface RequestServiceDialogData {
  roomNumber: string;
  roomId: number;
  type: 'housekeeping' | 'maintenance';
}

export interface RequestServiceDialogResult {
  description: string;
  isEmergency: boolean;
}

@Component({
  selector: 'app-request-service-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './request-service-dialog.component.html',
})
export class RequestServiceDialogComponent {
  readonly data: RequestServiceDialogData = inject(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<RequestServiceDialogComponent>);

  descriptionControl = new FormControl<string>('', { validators: [Validators.required], nonNullable: true });

  isEmergencyControl = new FormControl(false, { nonNullable: true });

  submit(): void {
    this.descriptionControl.markAsTouched();
    if (this.descriptionControl.invalid) {
      return;
    }
    const result: RequestServiceDialogResult = { description: this.descriptionControl.value, isEmergency: this.isEmergencyControl.value };
    this.dialogRef.close(result);
  }
}
