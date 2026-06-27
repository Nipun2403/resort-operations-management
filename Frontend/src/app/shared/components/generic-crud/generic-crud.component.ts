import {
  Component,
  inject,
  signal,
  computed,
  DestroyRef,
  input,
  output,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialogModule, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CrudConfig, CrudModalData, CrudModalResult } from '../../models/crud-config.model';
import { CardsViewComponent } from './cards-view/cards-view.component';
import { CrudModalComponent } from './crud-modal/crud-modal.component';
import { AlertComponent } from '../../../features/auth/components/alert.component';

@Component({
  selector: 'app-generic-crud',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatDialogModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatSelectModule,
    CardsViewComponent,
    AlertComponent,
  ],
  templateUrl: './generic-crud.component.html',
  styleUrls: ['./generic-crud.component.scss'],
})
export class GenericCrudComponent {
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  isInitialLoad = computed(() => this.config().loading() && (!this.config().data() || this.config().data().length === 0));

  constructor() {
    effect(() => {
      const query = this.searchQuery();
      if (query !== this.searchControl.value) {
        this.searchControl.setValue(query, { emitEvent: false });
      }
    });
  }

  // Inputs & Outputs per spec §4
  config = input.required<CrudConfig<any>>();

  searchChange = output<string>();
  filterChange = output<Record<string, any>>();
  sortChange = output<{ active: string; direction: 'asc' | 'desc' }>();
  pageChange = output<{ pageIndex: number; pageSize: number }>();
  save = output<{ formValue: any; isActive: boolean; entityId?: number }>();
  edit = output<any>();
  searchQuery = input<string, any>('', {
    transform: (value: any) => value ?? '',
  });

  // Internal signals per spec §4
  isModalOpen = signal(false);
  editMode = signal(false);
  selectedEntity = signal<any | null>(null);
  modalLoading = signal(false);
  modalError = signal<string | null>(null);

  // Internal form controls
  searchControl = new FormControl<string>('', { nonNullable: true });
  filterControls = new Map<string, FormControl>();

  private dialogRef: MatDialogRef<CrudModalComponent> | undefined;

  private searchDebounceTimer: ReturnType<typeof setTimeout> | null = null;

  get displayedColumns(): string[] {
    return [...this.config().columns.map((c) => c.field), 'actions'];
  }

  columnWidths = computed(() => {
    const cols = this.config().columns;
    const defaultWidth = `${100 / (cols.length + 1)}%`; // +1 for actions column
    return [...cols.map((col) => col.width ?? defaultWidth), defaultWidth];
  });

  getFilterControl(key: string): FormControl {
    if (!this.filterControls.has(key)) {
      this.filterControls.set(key, new FormControl(null));
    }
    return this.filterControls.get(key)!;
  }

  hasActiveFilters(): boolean {
    if (this.searchControl.value) return true;
    for (const ctrl of this.filterControls.values()) {
      if (ctrl.value !== null && ctrl.value !== '') return true;
    }
    return false;
  }

  onSearchDebounced(): void {
    if (this.searchDebounceTimer) clearTimeout(this.searchDebounceTimer);
    this.searchDebounceTimer = setTimeout(() => {
      this.searchChange.emit(this.searchControl.value);
    }, 300);
  }

  onFilterChange(key: string): void {
    const filters: Record<string, any> = {};
    this.filterControls.forEach((ctrl, k) => {
      if (ctrl.value !== null && ctrl.value !== '') {
        filters[k] = ctrl.value;
      }
    });
    this.filterChange.emit(filters);
  }

  clearFilters(): void {
    this.searchControl.reset('');
    this.filterControls.forEach((ctrl) => ctrl.reset(null));
    this.searchChange.emit('');
    this.filterChange.emit({});
  }

  onSortChange(sort: Sort): void {
    if (sort.active && sort.direction) {
      this.sortChange.emit({ active: sort.active, direction: sort.direction as 'asc' | 'desc' });
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageChange.emit({ pageIndex: event.pageIndex, pageSize: event.pageSize });
  }

  openAddModal(): void {
    this.editMode.set(false);
    this.selectedEntity.set(null);
    const data: CrudModalData = {
      editMode: false,
      entity: null,
      formFields: this.config().formFields,
      supportsToggle: this.config().supportsToggle,
    };
    this.dialogRef = this.dialog.open(CrudModalComponent, { data });
    this.handleModalClose();
  }

  openEditModal(row: any): void {
    this.edit.emit(row);
    this.editMode.set(true);
    this.selectedEntity.set(row);
    const data: CrudModalData = {
      editMode: true,
      entity: row,
      formFields: this.config().formFields,
      supportsToggle: this.config().supportsToggle,
    };
    this.dialogRef = this.dialog.open(CrudModalComponent, { data });
    this.handleModalClose();
  }

  private handleModalClose(): void {
    this.dialogRef!.afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result: CrudModalResult | null) => {
        if (result) {
          this.save.emit({ formValue: result.formValue, isActive: result.isActive, entityId: result.entityId });
        }
        // Reset modal state
        this.modalError.set(null);
        this.selectedEntity.set(null);
      });
  }
}
