import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs/operators';

import { GenericCrudComponent } from '../../../../shared/components/generic-crud/generic-crud.component';
import {
  CrudConfig,
  ColumnDef,
  FilterDef,
  FormFieldDef,
} from '../../../../shared/models/crud-config.model';
import { MenuItemApiService } from '../../services/menu-item-api.service';
import { MenuItem, CreateMenuItemDTO, UpdateMenuItemDTO } from '../../models/menu-item.model';

/** Validator that requires at least one letter if a value is present */
function optionalLetterPattern(
  control: AbstractControl,
): ValidationErrors | null {
  const value = control.value as string;
  if (!value || value.trim().length === 0) {
    return null; // empty is valid
  }
  const regex = /^(?=.*[a-zA-Z])[a-zA-Z0-9\s\-']+$/;
  return regex.test(value) ? null : { pattern: true };
}

@Component({
  selector: 'app-menu-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    GenericCrudComponent,
  ],
  templateUrl: './menu-management.component.html',
  styleUrls: ['./menu-management.component.scss'],
})
export class MenuManagementComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly snackBar = inject(MatSnackBar);
  private readonly menuItemApi = inject(MenuItemApiService);

  private readonly STORAGE_KEY = 'menuState';

  data = signal<MenuItem[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('name');
  sortDescending = signal(false);
  searchQuery = signal('');
  availabilityFilter = signal<boolean | null>(null); // null = all, true = available, false = unavailable
  editingEntity = signal<MenuItem | null>(null);

  crudConfig: CrudConfig<MenuItem> = {
    entityName: 'Menu Item',
    entityNamePlural: 'Menu Items',
    columns: [
      { header: 'Name', field: 'name', sortable: true, getValue: (r) => r.name },
      {
        header: 'Category',
        field: 'category',
        sortable: false,
        getValue: (r) => r.category || '—',
      },
      { header: 'Price', field: 'price', sortable: true, getValue: (r) => `$${r.price}` },
      {
        header: 'Available',
        field: 'isAvailable',
        sortable: true,
        getValue: (r) => (r.isAvailable ? 'Yes' : 'No'),
      },
    ],
    filters: [
      {
        key: 'isAvailable',
        label: 'Availability',
        options: [
          { value: null, label: 'All' },
          { value: true, label: 'Available' },
          { value: false, label: 'Unavailable' },
        ],
      },
    ],
    formFields: [
      {
        key: 'name',
        label: 'Name',
        type: 'text',
        validators: [
          Validators.required,
          Validators.maxLength(100),
          Validators.minLength(1),
          Validators.pattern(/^(?=.*[a-zA-Z])[a-zA-Z0-9\s\-']+$/),
        ],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'category',
        label: 'Category',
        type: 'text',
        validators: [Validators.maxLength(100), optionalLetterPattern],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'price',
        label: 'Price',
        type: 'number',
        validators: [Validators.required, Validators.min(0)],
        showInAdd: true,
        showInEdit: true,
      },
      {
        key: 'isAvailable',
        label: 'Available',
        type: 'toggle',
        validators: [],
        showInAdd: false, // not shown on creation (defaults to true)
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
    this.menuItemApi
      .getAll({
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        searchQuery: this.searchQuery() || undefined,
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
        isAvailable: this.availabilityFilter() ?? undefined,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.data.set(res.data);
          this.totalCount.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
            this.saveState();
          }
        },
        error: (err: any) =>
          this.error.set(err instanceof Error ? err.message : 'Unexpected error'),
      });
  }

  onEdit(entity: MenuItem): void {
    this.editingEntity.set(entity);
  }

  onSave(event: { formValue: any; isActive: boolean }): void {
    const { formValue, isActive } = event;
    if (this.editingEntity()) {
      const dto: UpdateMenuItemDTO = {
        name: formValue.name,
        price: formValue.price,
        category: formValue.category ?? '',
        isAvailable: isActive,
      };
      this.menuItemApi
        .update(this.editingEntity()!.id, dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Menu item updated', 'Close', { duration: 3000 });
            this.editingEntity.set(null);
            this.fetchData();
          },
          error: (err: any) =>
            this.snackBar.open(
              err instanceof Error ? err.message : 'Unexpected error',
              'Close',
              { duration: 5000 },
            ),
        });
    } else {
      const dto: CreateMenuItemDTO = {
        name: formValue.name,
        price: formValue.price,
        category: formValue.category ?? '',
        isAvailable: true, // new items are always available by default
      };
      this.menuItemApi
        .create(dto)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.snackBar.open('Menu item created', 'Close', { duration: 3000 });
            this.fetchData();
          },
          error: (err: any) =>
            this.snackBar.open(
              err instanceof Error ? err.message : 'Unexpected error',
              'Close',
              { duration: 5000 },
            ),
        });
    }
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query.trim() || '');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onFilterChange(filters: Record<string, any>): void {
    const val = filters['isAvailable'];
    this.availabilityFilter.set(val === '' || val === undefined ? null : val);
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
      if (typeof parsed.searchQuery === 'string') this.searchQuery.set(parsed.searchQuery);
      if (['name', 'price', 'isAvailable'].includes(parsed.sortField))
        this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean')
        this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0)
        this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0)
        this.pageSize.set(parsed.pageSize);
      if (parsed.availabilityFilter === null || typeof parsed.availabilityFilter === 'boolean') {
        this.availabilityFilter.set(parsed.availabilityFilter);
      }
    } catch {
      /* fallback silently */
    }
  }

  private saveState(): void {
    sessionStorage.setItem(
      this.STORAGE_KEY,
      JSON.stringify({
        searchQuery: this.searchQuery(),
        sortField: this.sortField(),
        sortDescending: this.sortDescending(),
        pageIndex: this.pageIndex(),
        pageSize: this.pageSize(),
        availabilityFilter: this.availabilityFilter(),
      }),
    );
  }
}
