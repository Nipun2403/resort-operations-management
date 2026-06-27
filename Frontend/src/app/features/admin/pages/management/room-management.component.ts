import { Component, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BreakpointObserver } from '@angular/cdk/layout';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { map } from 'rxjs/operators';

import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
import { CrudConfig } from '../../../../shared/models/crud-config.model';
import { RoomStatusGridComponent } from '../../components/room-status-grid/room-status-grid.component';
import { RoomApiService } from '../../services/room-api.service';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { Room, CreateRoomDTO, UpdateRoomDTO, RoomStatus } from '../../models/room.model';

interface RoomsState {
  roomTypeId: number | null;
  includeRetired: boolean;
  searchQuery: string;
  sortField: string;
  sortDescending: boolean;
  pageIndex: number;
  pageSize: number;
}

@Component({
  selector: 'app-room-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonToggleModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    GenericCrudComponent,
    RoomStatusGridComponent,
  ],
  templateUrl: './room-management.component.html',
  styleUrls: ['./room-management.component.scss'],
})
export class RoomManagementComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly roomApi = inject(RoomApiService);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly breakpointObserver = inject(BreakpointObserver);

  private readonly STORAGE_KEY = 'roomsState';

  // Data signals
  data = signal<Room[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query param signals
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('id');
  sortDescending = signal(false);
  searchQuery = signal('');
  roomTypeFilter = signal<number | null>(null);
  includeRetired = signal(false);
  editingEntity = signal<Room | null>(null);

  // Mobile
  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 768px)').pipe(map((r) => r.matches)),
    { initialValue: false },
  );
  viewMode = new FormControl<'table' | 'grid'>('table', { nonNullable: true });

  // CrudConfig
  crudConfig: CrudConfig<Room> = {
    entityName: 'Room',
    entityNamePlural: 'Rooms',
    columns: [
      {
        header: 'Room #',
        field: 'roomNumber',
        sortable: false,
        getValue: (r: Room) => r.roomNumber,
      },
      {
        header: 'Type',
        field: 'roomTypeName',
        sortable: false,
        getValue: (r: Room) => r.roomTypeName,
      },
      {
        header: 'Base Price',
        field: 'basePrice',
        sortable: true,
        getValue: (r: Room) => `$${r.basePrice}`,
      },
      {
        header: 'Max Occ.',
        field: 'maxOccupancy',
        sortable: true,
        getValue: (r: Room) => String(r.maxOccupancy),
      },
      // {
      //   header: 'Active',
      //   field: 'isActive',
      //   sortable: false,
      //   getValue: (r: Room) => (r.isActive ? 'Yes' : 'No'),
      // },
      {
        header: 'Available',
        field: 'isAvailable',
        sortable: false,
        getValue: (r: Room) => (r.isAvailable ? 'Yes' : 'No'),
      },
    ],
    filters: [
      {
        key: 'roomTypeId',
        label: 'Room Type',
        options: [], // populated dynamically
      },
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
        key: 'roomNumber',
        label: 'Room Number',
        type: 'text',
        validators: [Validators.required, Validators.maxLength(100)],
      },
      {
        key: 'roomTypeId',
        label: 'Room Type',
        type: 'select',
        validators: [Validators.required],
        options: [], // populated dynamically
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
    // Load room types for dropdowns
    this.roomTypeApi
      .getAll({
        includeRetired: false,
        pageNumber: 1,
        pageSize: 100,
        sortBy: 'name',
        sortDescending: false,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((res) => {
        const options = res.data.map((rt) => ({ value: rt.id, label: rt.name }));
        this.crudConfig.filters[0].options = options;
        this.crudConfig.formFields[1].options = options;
      });
  }

  private fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.roomApi
      .getAll({
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        roomTypeId: this.roomTypeFilter() ?? undefined,
        includeRetired: this.includeRetired(),
        searchQuery: this.searchQuery() || undefined,
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.data);
          this.totalCount.set(res.totalCount);
          // Normalize page if out of bounds
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
            this.fetchData();
          }
        },
        error: (err: Error) => this.error.set(err.message),
      });
  }

  private saveState(): void {
    const state: RoomsState = {
      roomTypeId: this.roomTypeFilter(),
      includeRetired: this.includeRetired(),
      searchQuery: this.searchQuery(),
      sortField: this.sortField(),
      sortDescending: this.sortDescending(),
      pageIndex: this.pageIndex(),
      pageSize: this.pageSize(),
    };
    sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
  }

  private restoreState(): void {
    const raw = sessionStorage.getItem(this.STORAGE_KEY);
    if (!raw) return;
    try {
      const state: RoomsState = JSON.parse(raw);
      this.roomTypeFilter.set(state.roomTypeId ?? null);
      this.includeRetired.set(state.includeRetired ?? false);
      this.searchQuery.set(state.searchQuery ?? '');
      this.sortField.set(state.sortField ?? 'id');
      this.sortDescending.set(state.sortDescending ?? false);
      this.pageIndex.set(state.pageIndex ?? 0);
      this.pageSize.set(state.pageSize ?? 10);
    } catch {
      // Ignore corrupt state
    }
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query.trim());
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onFilterChange(filters: Record<string, any>): void {
    this.roomTypeFilter.set(filters['roomTypeId'] ?? null);
    this.includeRetired.set(filters['includeRetired'] ?? false);
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' }): void {
    this.sortField.set(event.active || 'id');
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

  onEdit(entity: Room): void {
    this.editingEntity.set(entity);
  }

  onSave(event: { formValue: any; isActive: boolean; entityId?: number }): void {
    const { formValue, isActive } = event;
    if (this.editingEntity()) {
      const dto: UpdateRoomDTO = { ...formValue, isActive };
      this.roomApi
        .update(this.editingEntity()!.id, dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Room updated', 'Close', { duration: 3000 });
            this.editingEntity.set(null);
            this.fetchData();
          },
          error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 }),
        });
    } else {
      const dto: CreateRoomDTO = formValue;
      this.roomApi
        .create(dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Room created', 'Close', { duration: 3000 });
            this.fetchData();
          },
          error: (err: Error) => this.snackBar.open(err.message, 'Close', { duration: 5000 }),
        });
    }
  }

  onGridRoomClicked(room: RoomStatus): void {
    this.searchQuery.set(room.roomNumber);
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }
}
