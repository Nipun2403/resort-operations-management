import { Component, inject, signal, computed, DestroyRef, input, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { finalize, forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { Task, TaskDashboardConfig } from '../../models/task.model';
import { AlertComponent } from '../../../features/auth/components/alert.component';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { TaskDetailDialogComponent } from './task-detail-dialog.component';

@Component({
  selector: 'app-task-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDialogModule,
    MatSnackBarModule,
    AlertComponent,
  ],
  templateUrl: './task-dashboard.component.html',
  styleUrls: ['./task-dashboard.component.scss'],
})
export class TaskDashboardComponent {
  config = input.required<TaskDashboardConfig<any>>();
  viewMode = input<'table' | 'kanban'>('table');
  refresh = input(0);

  // Data
  data = signal<Task[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Pagination & sorting
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal('id');
  sortDescending = signal(false);

  // Status filter
  statusFilter = signal('All');
  statusFilterControl = new FormControl('All', { nonNullable: true });

  // Summary cards
  summaryCards = signal<{ status: string; label: string; count: number }[]>([]);

  // Table columns
  displayedColumns = ['id', 'location', 'description', 'status', 'actions'];

  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    this.setupRefreshEffect();
  }

  /**
   * Data fetching is driven by the `refresh` input via an effect.
   * Increment the signal/input in the parent component to trigger a reload.
   */
  private setupRefreshEffect(): void {
    effect(() => {
      const config = this.config();
      this.sortField.set(config.defaultSortBy ?? 'id');
      this.sortDescending.set(config.defaultSortDescending ?? false);
      this.refresh(); // read to track
      this.pageIndex.set(0); // reset to first page when refreshed
      this.fetchData();
      this.refreshSummaryCounts();
    });
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    const params: any = {
      pageNumber: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      sortDescending: this.sortDescending(),
    };
    if (this.statusFilter() !== 'All') {
      params.status = this.statusFilter();
    }
    this.config()
      .fetchTasks(params)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (res) => {
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

  refreshSummaryCounts(): void {
    const statuses = this.config().statusOptions.filter((s) => s.value !== 'All');
    const requests = statuses.map((s) =>
      this.config()
        .fetchTasks({ pageNumber: 1, pageSize: 1, status: s.value })
        .pipe(
          map((res) => ({ status: s.value, label: s.label, count: res.totalCount })),
          catchError(() => of({ status: s.value, label: s.label, count: 0 }))
        )
    );
    forkJoin(requests)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((cards) => {
        this.summaryCards.set(cards);
      });
  }

  setStatusFilter(status: string): void {
    const newStatus = this.statusFilter() === status ? 'All' : status;
    this.statusFilter.set(newStatus);
    this.statusFilterControl.setValue(newStatus);
    this.pageIndex.set(0);
    this.fetchData();
  }

  onStatusFilterChange(value: string): void {
    this.statusFilter.set(value);
    this.pageIndex.set(0);
    this.fetchData();
  }

  onSortChange(event: Sort): void {
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

  openDetail(task: Task): void {
    const config = this.config();
    const activeStatuses = config.statusOptions.filter((s) => s.value !== 'All').map((s) => s.value);
    const pendingVal = activeStatuses[0] ?? 'Pending';
    const inProgressVal = activeStatuses[1] ?? 'InProgress';
    const completedVal = activeStatuses[2] ?? 'Completed';

    const dialogRef = this.dialog.open(TaskDetailDialogComponent, {
      data: {
        task,
        detailSections: config.getDetailSections(task),
        canStart: task.status === pendingVal,
        canComplete: task.status === inProgressVal,
        inProgressStatus: inProgressVal,
        completedStatus: completedVal,
      },
      width: '90vw',
      maxWidth: '500px',
    });

    dialogRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result: { newStatus: string } | null) => {
        if (result) {
          this.updateStatus(task.id, result.newStatus);
        }
      });
  }

  updateStatus(id: number, newStatus: string): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Status Change',
        message: `Are you sure you want to transition this task status to ${newStatus}?`,
      },
    });

    dialogRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (confirmed) {
          this.loading.set(true);
          this.config()
            .updateTaskStatus(id, newStatus)
            .pipe(
              finalize(() => this.loading.set(false)),
              takeUntilDestroyed(this.destroyRef)
            )
            .subscribe({
              next: () => {
                this.snackBar.open('Task status updated successfully.', 'Close', { duration: 3000 });
                this.fetchData();
                this.refreshSummaryCounts();
              },
              error: (err) => {
                this.snackBar.open(
                  'Failed to update task status: ' + (err.error?.message || err.message),
                  'Close',
                  { duration: 5000 }
                );
              },
            });
        }
      });
  }

  private extractErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'An unexpected error occurred.';
  }
}
