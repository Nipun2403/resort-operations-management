import { Component, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
import {
  CrudConfig,
  ColumnDef,
  FilterDef,
  FormFieldDef,
} from '../../../../shared/models/crud-config.model';
import { StaffApiService } from '../../services/staff-api.service';
import {
  Staff,
  CreateStaffDTO,
  UpdateStaffDTO,
  StaffRole,
} from '../../models/staff.model';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-staff-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    MatDialogModule,
    GenericCrudComponent,
  ],
  templateUrl: './staff-management.component.html',
  styleUrls: ['./staff-management.component.scss'],
})
export class StaffManagementComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly staffApi = inject(StaffApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  private readonly STORAGE_KEY = 'staffState';

  data = signal<Staff[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query params
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('isActive');
  sortDescending = signal(false);
  searchQuery = signal('');          // parent's own search state, updated via searchChange
  includeFired = signal(false);      // false = active only, true = all
  editingEntity = signal<Staff | null>(null);

  crudConfig: CrudConfig<Staff> = {
    entityName: 'Staff',
    entityNamePlural: 'Staff',
    columns: [
      {
        header: 'First Name',
        field: 'firstName',
        sortable: true,
        getValue: (r: Staff) => r.firstName,
      },
      {
        header: 'Last Name',
        field: 'lastName',
        sortable: true,
        getValue: (r: Staff) => r.lastName,
      },
      {
        header: 'Email',
        field: 'email',
        sortable: true,
        getValue: (r: Staff) => r.email,
      },
      { header: 'Role', field: 'role', sortable: true, getValue: (r: Staff) => r.role },
      {
        header: 'Active',
        field: 'isActive',
        sortable: true,
        getValue: (r: Staff) => (r.isActive ? 'Yes' : 'No'),
      },
    ],
    filters: [
      {
        key: 'includeFired',
        label: 'Status',
        options: [
          { value: false, label: 'Active Only' },
          { value: true, label: 'All' },
        ],
      },
    ],
    formFields: [
      {
        key: 'email',
        label: 'Email',
        type: 'email',
        validators: [
          Validators.required,
          Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/),
        ],
        showInAdd: true,
        showInEdit: false,
      },
      {
        key: 'password',
        label: 'Password',
        type: 'password',
        validators: [
          Validators.required,
          Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/),
        ],
        showInAdd: true,
        showInEdit: false,
      },
      {
        key: 'firstName',
        label: 'First Name',
        type: 'text',
        validators: [
          Validators.required,
          Validators.pattern(/^[a-zA-ZÀ-ž\s\-']{2,50}$/),
        ],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'lastName',
        label: 'Last Name',
        type: 'text',
        validators: [
          Validators.required,
          Validators.pattern(/^[a-zA-ZÀ-ž\s\-']{2,50}$/),
        ],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'role',
        label: 'Role',
        type: 'select',
        options: [
          { value: 'Admin', label: 'Admin' },
          { value: 'FrontDesk', label: 'Front Desk' },
          { value: 'Kitchen', label: 'Kitchen' },
          { value: 'Housekeeping', label: 'Housekeeping' },
          { value: 'Maintenance', label: 'Maintenance' },
        ],
        validators: [Validators.required],
        showInAdd: true,
        showInEdit: true,
      },
    ],
    supportsToggle: true,
    data: this.data,
    totalCount: this.totalCount,
    loading: this.loading,
    error: this.error,
    pageIndex: this.pageIndex,
    pageSize: this.pageSize,
  };

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.staffApi.getAll({
      includeFired: this.includeFired(),
      pageNumber: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      sortDescending: this.sortDescending(),
      searchQuery: this.searchQuery() || undefined,
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => {
        this.data.set(res.data);
        this.totalCount.set(res.totalCount);
        // Page normalization – only after successful data update
        const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
        if (this.pageIndex() > maxPage) {
          this.pageIndex.set(maxPage);
          this.saveState();
        }
      },
      error: (err: Error) => this.error.set(err.message)
    });
  }

  onEdit(entity: Staff): void {
    this.editingEntity.set(entity);
  }

  onSave(event: { formValue: any; isActive: boolean }): void {
    const { formValue, isActive } = event;
    if (this.editingEntity()) {
      // Deactivation confirmation: compare original state vs submitted isActive
      if (this.editingEntity()!.isActive && !isActive) {
        this.showDisableConfirmation(formValue, isActive);
        return;
      }
      this.performUpdate(formValue, isActive);
    } else {
      this.performCreate(formValue);
    }
  }

  private showDisableConfirmation(formValue: any, isActive: boolean): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Deactivate Staff Member',
        message: `Are you sure you want to deactivate ${this.editingEntity()!.firstName} ${this.editingEntity()!.lastName}?`,
      },
    });
    dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
      if (confirmed) {
        this.performUpdate(formValue, isActive);
      }
    });
  }

  private performUpdate(formValue: any, isActive: boolean): void {
    const dto: UpdateStaffDTO = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      role: formValue.role as StaffRole,
      isActive: isActive,
    };
    this.staffApi.update(this.editingEntity()!.id, dto).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Staff updated', 'Close', { duration: 3000 });
        this.editingEntity.set(null);
        this.fetchData();
      },
      error: (err: any) => {
        const message = err instanceof Error ? err.message : 'Unexpected error';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  private performCreate(formValue: any): void {
    const dto: CreateStaffDTO = {
      email: formValue.email,
      password: formValue.password,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      role: formValue.role as StaffRole,
    };
    this.staffApi.create(dto).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Staff created', 'Close', { duration: 3000 });
        this.fetchData();
      },
      error: (err: any) => {
        const message = err instanceof Error ? err.message : 'Unexpected error';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query.trim() || '');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onFilterChange(filters: Record<string, any>): void {
    if ('includeFired' in filters) {
      this.includeFired.set(filters['includeFired'] ?? false);
    }
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    if (!event.active || !event.direction) return;
    this.sortField.set(event.active);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.saveState();
    this.fetchData();
  }

  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;
      // Validate types
      if (typeof parsed.includeFired === 'boolean') this.includeFired.set(parsed.includeFired);
      if (typeof parsed.searchQuery === 'string') this.searchQuery.set(parsed.searchQuery);
      if (['firstName', 'lastName', 'email', 'role', 'isActive'].includes(parsed.sortField)) this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean') this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0) this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0) this.pageSize.set(parsed.pageSize);
    } catch {
      // fallback silently to defaults
    }
  }

  private saveState(): void {
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify({
      includeFired: this.includeFired(),
      searchQuery: this.searchQuery(),
      sortField: this.sortField(),
      sortDescending: this.sortDescending(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
    }));
  }
}
