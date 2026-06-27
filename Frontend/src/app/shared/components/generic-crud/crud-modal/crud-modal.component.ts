import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { CrudModalData, CrudModalResult, FormFieldDef } from '../../../models/crud-config.model';

@Component({
  selector: 'app-crud-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatIconModule,
  ],
  templateUrl: './crud-modal.component.html',
  styleUrls: ['./crud-modal.component.scss'],
})
export class CrudModalComponent implements OnInit {
  readonly data = inject<CrudModalData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CrudModalComponent>);

  modalForm!: FormGroup;
  isActiveControl = new FormControl<boolean>(true);
  activeFields: FormFieldDef[] = [];

  ngOnInit(): void {
    this.activeFields = this.data.formFields.filter((f) => {
      if (this.data.editMode) {
        return f.showInEdit !== false;
      } else {
        return f.showInAdd !== false;
      }
    });

    const controls: Record<string, FormControl> = {};
    for (const field of this.activeFields) {
      const value = this.data.entity ? (this.data.entity[field.key] ?? null) : null;
      controls[field.key] = new FormControl(value, field.validators ?? []);
    }
    this.modalForm = new FormGroup(controls);

    if (this.data.supportsToggle && this.data.entity) {
      this.isActiveControl.setValue(this.data.entity.isActive ?? true);
    }
  }

  getFieldDefs(): FormFieldDef[] {
    return this.activeFields;
  }

  getControl(key: string): FormControl {
    return this.modalForm.get(key) as FormControl;
  }

  submit(): void {
    this.modalForm.markAllAsTouched();
    if (this.modalForm.invalid) return;

    const result: CrudModalResult = {
      formValue: this.modalForm.value,
      isActive: this.data.supportsToggle ? (this.isActiveControl.value ?? true) : true,
      previousIsActive: this.data.supportsToggle && this.data.entity
        ? (this.data.entity.isActive ?? true)
        : true,
      entityId: this.data.entity?.id,
    };

    this.dialogRef.close(result);
  }

  cancel(): void {
    this.dialogRef.close(null);
  }
}
