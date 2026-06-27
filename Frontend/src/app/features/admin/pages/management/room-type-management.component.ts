import { Component, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
import { CrudConfig } from '../../../../shared/models/crud-config.model';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { RoomType, CreateRoomTypeDTO, UpdateRoomTypeDTO } from '../../models/room-type.model';

const STATE_KEY = 'roomTypesState';

interface RoomTypesState {
  includeRetired: boolean;
  sortField: string;
  sortDescending: boolean;
  pageIndex: number;
  pageSize: number;
  searchQuery: string;
}

@Component({
  selector: 'app-room-type-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatSnackBarModule, GenericCrudComponent],
  templateUrl: './room-type-management.component.html',
  styleUrls: ['./room-type-management.component.scss'],
})
export class RoomTypeManagementComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly snackBar = inject(MatSnackBar);

  // Data signals
  data = signal<RoomType[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query param signals
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('name');
  sortDescending = signal(false);
  includeRetired = signal(false);
  searchQuery = signal('');

  // CrudConfig
  crudConfig: CrudConfig<RoomType> = {
    entityName: 'Room Type',
    entityNamePlural: 'Room Types',
    columns: [
      { header: 'Name', field: 'name', sortable: true, getValue: (r: RoomType) => r.name },
      {
        header: 'Base Price',
        field: 'basePrice',
        sortable: true,
        getValue: (r: RoomType) => `$${r.basePrice}`,
      },
      {
        header: 'Max Occupancy',
        field: 'maxOccupancy',
        sortable: true,
        getValue: (r: RoomType) => String(r.maxOccupancy),
      },
      {
        header: 'Active',
        field: 'isActive',
        sortable: false,
        getValue: (r: RoomType) => (r.isActive ? 'Yes' : 'No'),
      },
    ],
    filters: [
      {
        key: 'includeRetired',
        label: 'Status',
        options: [
          { value: false, label: 'Active Only' },
          { value: true, label: 'All' },
        ],
      },
    ],
    formFields: [
      {
        key: 'name',
        label: 'Name',
        type: 'text',
        validators: [Validators.required, Validators.maxLength(100)],
      },
      {
        key: 'description',
        label: 'Description',
        type: 'textarea',
        validators: [Validators.maxLength(500)],
      },
      {
        key: 'basePrice',
        label: 'Base Price',
        type: 'number',
        validators: [Validators.required, Validators.min(0)],
      },
      {
        key: 'maxOccupancy',
        label: 'Max Occupancy',
        type: 'number',
        validators: [Validators.required, Validators.min(1)],
      },
      {
        key: 'squareFootage',
        label: 'Square Footage',
        type: 'number',
        validators: [],
      },
      {
        key: 'bedConfiguration',
        label: 'Bed Configuration',
        type: 'keyValueList',
        validators: [],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'imageUrls',
        label: 'Images',
        type: 'imageUrlList',
        validators: [],
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

  private fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.roomTypeApi
      .getAll({
        includeRetired: this.includeRetired(),
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
        searchQuery: this.searchQuery() || undefined,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.data);
          this.totalCount.set(res.totalCount);
        },
        error: (err: Error) => this.error.set(err.message),
      });
  }

  private saveState(): void {
    const state: RoomTypesState = {
      includeRetired: this.includeRetired(),
      sortField: this.sortField(),
      sortDescending: this.sortDescending(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
      searchQuery: this.searchQuery(),
    };
    sessionStorage.setItem(STATE_KEY, JSON.stringify(state));
  }

  private restoreState(): void {
    const raw = sessionStorage.getItem(STATE_KEY);
    if (!raw) return;
    try {
      const state: RoomTypesState = JSON.parse(raw);
      this.includeRetired.set(state.includeRetired ?? false);
      this.sortField.set(state.sortField ?? 'name');
      this.sortDescending.set(state.sortDescending ?? false);
      this.pageIndex.set(state.pageIndex ?? 0);
      this.pageSize.set(state.pageSize ?? 10);
      this.searchQuery.set(state.searchQuery ?? '');
    } catch {
      // Ignore corrupt state
    }
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query.trim() || '');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onFilterChange(filters: Record<string, any>): void {
    if ('includeRetired' in filters) {
      this.includeRetired.set(filters['includeRetired']);
    }
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    this.sortField.set(event.active || 'name');
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

  onSave(event: { formValue: any; isActive: boolean; entityId?: number }): void {
    const { formValue, isActive, entityId } = event;

    const imageUrls = formValue.imageUrls ?? [];
    const bedConfig = formValue.bedConfiguration || null;

    if (entityId != null) {
      // Edit mode
      const dto: UpdateRoomTypeDTO = {
        name: formValue.name,
        description: formValue.description,
        basePrice: formValue.basePrice,
        maxOccupancy: formValue.maxOccupancy,
        imageUrls: imageUrls,
        squareFootage: formValue.squareFootage,
        bedConfiguration: bedConfig,
        isActive: isActive,
      };
      this.roomTypeApi
        .update(entityId, dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Room type updated', 'Close', { duration: 3000 });
            this.fetchData();
          },
          error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 }),
        });
    } else {
      // Create mode
      const dto: CreateRoomTypeDTO = {
        name: formValue.name,
        description: formValue.description,
        basePrice: formValue.basePrice,
        maxOccupancy: formValue.maxOccupancy,
        imageUrls: imageUrls,
        squareFootage: formValue.squareFootage,
        bedConfiguration: bedConfig,
      };
      this.roomTypeApi
        .create(dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Room type created', 'Close', { duration: 3000 });
            this.fetchData();
          },
          error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 }),
        });
    }
  }
}
