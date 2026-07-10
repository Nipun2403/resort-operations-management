import { Component, inject, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, AbstractControl, FormArray, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { CrudModalData, CrudModalResult, FormFieldDef } from '../../../models/crud-config.model';
import { ConfirmDialogComponent } from '../../confirm-dialog/confirm-dialog.component';
import { ImageUploadOrUrlComponent } from '../image-upload-or-url/image-upload-or-url.component';

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
    ImageUploadOrUrlComponent,
  ],
  templateUrl: './crud-modal.component.html',
  styleUrls: ['./crud-modal.component.scss'],
})
export class CrudModalComponent implements OnInit {
  readonly data = inject<CrudModalData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CrudModalComponent>);
  private readonly dialog = inject(MatDialog);

  modalForm!: FormGroup;
  isActiveControl = new FormControl<any>(true);
  activeFields: FormFieldDef[] = [];
  hasToggleField = computed(() => this.activeFields.some(f => f.type === 'toggle'));
  isSaving = signal(false);

  ngOnInit(): void {
    this.activeFields = this.data.formFields.filter((f) => {
      if (this.data.editMode) {
        return f.showInEdit !== false;
      } else {
        return f.showInAdd !== false;
      }
    });

    const controls: Record<string, AbstractControl> = {};
    for (const field of this.activeFields) {
      if (field.type === 'keyValueList') {
        const pairsArray = new FormArray<FormGroup>([]);
        const bedConfig = this.data.entity ? this.data.entity[field.key] : null;
        if (bedConfig && typeof bedConfig === 'object') {
          Object.entries(bedConfig).forEach(([key, value]) => {
            pairsArray.push(
              new FormGroup({
                key: new FormControl(key, [
                  Validators.required,
                  Validators.pattern(/^[a-zA-Z0-9\s\-']+$/),
                ]),
                value: new FormControl(value, [Validators.required, Validators.min(1)]),
              }),
            );
          });
        }
        controls[field.key] = new FormGroup({ pairs: pairsArray });
      } else if (field.type === 'imageUrlList') {
        const urlsArray = new FormArray<FormControl>([]);
        const urls = this.data.entity ? this.data.entity[field.key] : null;
        if (urls && Array.isArray(urls)) {
          urls.forEach((url) => {
            urlsArray.push(new FormControl(url, [Validators.pattern(/^https?:\/\/.+/)]));
          });
        }
        controls[field.key] = new FormGroup({ urls: urlsArray });
      } else if (field.type === 'image') {
        const value = this.data.entity ? (this.data.entity[field.key] ?? null) : null;
        controls[field.key] = new FormControl(value, field.validators ?? []);
      } else {
        const value = this.data.entity ? (this.data.entity[field.key] ?? null) : null;
        controls[field.key] = new FormControl(value, field.validators ?? []);
      }
    }
    this.modalForm = new FormGroup(controls);

    const toggleField = this.activeFields.find((f) => f.type === 'toggle');
    if (toggleField) {
      this.isActiveControl = this.getControl(toggleField.key);
    } else {
      if (this.data.supportsToggle && this.data.entity) {
        this.isActiveControl.setValue(
          this.data.entity.isActive ?? this.data.entity.isAvailable ?? true,
        );
      }
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

    const rawValue = this.modalForm.getRawValue();
    for (const field of this.activeFields) {
      if (field.type === 'keyValueList') {
        const pairs: { key: string; value: number }[] = rawValue[field.key]?.pairs ?? [];
        const obj: Record<string, number> = {};
        pairs.forEach((p) => {
          if (p.key) obj[p.key] = p.value;
        });
        rawValue[field.key] = Object.keys(obj).length > 0 ? obj : null;
      } else if (field.type === 'imageUrlList') {
        rawValue[field.key] = (rawValue[field.key]?.urls ?? []).filter(Boolean);
      }
    }

    const result: CrudModalResult = {
      formValue: rawValue,
      isActive: this.data.supportsToggle ? (this.isActiveControl.value ?? true) : true,
      previousIsActive: this.data.supportsToggle && this.data.entity
        ? (this.data.entity.isActive ?? true)
        : true,
      entityId: this.data.entity?.id,
    };

    if (this.data.editMode && result.previousIsActive && !result.isActive) {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Confirm Deactivation',
          message: `Are you sure you want to disable this ${this.data.entityName ?? 'item'}?`,
        },
      });
      dialogRef.afterClosed().subscribe((confirmed) => {
        if (confirmed) {
          this.dialogRef.close(result);
        }
      });
    } else {
      this.dialogRef.close(result);
    }
  }

  getKeyValueArray(fieldName: string): FormArray {
    return this.modalForm.get(fieldName + '.pairs') as FormArray;
  }

  getImageUrlArray(fieldName: string): FormArray {
    return this.modalForm.get(fieldName + '.urls') as FormArray;
  }

  addKeyValuePair(fieldName: string): void {
    const pair = new FormGroup({
      key: new FormControl('', [
        Validators.required,
        Validators.pattern(/^[a-zA-Z0-9\s\-']+$/),
      ]),
      value: new FormControl(1, [Validators.required, Validators.min(1)]),
    });
    this.getKeyValueArray(fieldName).push(pair);
  }

  removeKeyValuePair(fieldName: string, index: number): void {
    this.getKeyValueArray(fieldName).removeAt(index);
  }

  addImageUrl(fieldName: string): void {
    this.getImageUrlArray(fieldName).push(
      new FormControl('', [Validators.pattern(/^https?:\/\/.+/)]),
    );
  }

  removeImageUrl(fieldName: string, index: number): void {
    this.getImageUrlArray(fieldName).removeAt(index);
  }

  cancel(): void {
    this.dialogRef.close(null);
  }

  getErrorMessage(field: FormFieldDef, control: AbstractControl): string | null {
    if (!control.errors || !control.touched) return null;
    const errors = control.errors;
    if (errors['required']) {
      return `${field.label} is required.`;
    }
    if (errors['email'] || (field.type === 'email' && errors['pattern'])) {
      return 'Please enter a valid email address.';
    }
    if (errors['pattern']) {
      return `${field.label} contains invalid characters or format.`;
    }
    if (errors['min']) {
      return `${field.label} must be at least ${errors['min'].min}.`;
    }
    if (errors['max']) {
      return `${field.label} must be at most ${errors['max'].max}.`;
    }
    if (errors['minlength']) {
      return `${field.label} must be at least ${errors['minlength'].requiredLength} characters.`;
    }
    if (errors['maxlength']) {
      return `${field.label} must be at most ${errors['maxlength'].requiredLength} characters.`;
    }
    return `${field.label} is invalid.`;
  }
}
