import { Component, inject, signal, computed, input, output, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatButtonModule } from '@angular/material/button';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';

import { BookingApiService } from '../../../user/services/booking-api.service';
import { Booking } from '../../../admin/models/booking.model';
import { AlertComponent } from '../../../auth/components/alert.component';

@Component({
  selector: 'app-movement-table',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonToggleModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatButtonModule,
    AlertComponent,
  ],
  templateUrl: './movement-table.component.html',
  styleUrls: ['./movement-table.component.scss'],
})
export class MovementTableComponent {
  private bookingApi = inject(BookingApiService);
  private destroyRef = inject(DestroyRef);

  refresh = input(0);
  bookingSelected = output<Booking>();

  searchControl = new FormControl('', { nonNullable: true });
  isSearching = signal(false);
  activeFilter = new FormControl<'arrivals' | 'departures'>('arrivals', { nonNullable: true });

  data = signal<Booking[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('bookedAt');
  sortDescending = signal(true);

  tableTitle = computed(() => (this.isSearching() ? 'Search Results' : 'Today’s Movement'));
  displayedColumns = ['guestName', 'status', 'roomNumber', 'actions'];

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(val => {
        const trimmed = val.trim();
        this.isSearching.set(trimmed.length > 0);
        this.pageIndex.set(0);
        this.fetchData();
      });

    effect(() => {
      this.refresh();
      this.pageIndex.set(0);
      this.fetchData();
    });
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);

    let sortBy = this.sortField();
    if (sortBy === 'status') {
      sortBy = 'bookingStatus';
    }

    const params: any = {
      pageNumber: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: sortBy,
      sortDescending: this.sortDescending(),
    };

    if (this.isSearching()) {
      params.guestQuery = this.searchControl.value.trim();
    } else {
      params.movementStatus = this.activeFilter.value === 'arrivals' ? 'incoming' : 'outgoing';
    }

    this.bookingApi
      .getAll(params)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: res => {
          this.data.set(res.data);
          this.totalCount.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
          }
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  onToggleChange(): void {
    this.pageIndex.set(0);
    this.fetchData();
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.isSearching.set(false);
    this.pageIndex.set(0);
    this.fetchData();
  }

  onSortChange(event: Sort): void {
    if (!event.active || !event.direction) return;
    this.sortField.set(event.active);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.fetchData();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.fetchData();
  }

  onRowClick(booking: Booking): void {
    this.bookingSelected.emit(booking);
  }

  getRoomNumbers(booking: Booking): string {
    return booking.rooms?.filter(r => r.roomNumber).map(r => r.roomNumber).join(', ') || '';
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
