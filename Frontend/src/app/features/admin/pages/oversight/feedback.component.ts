import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { finalize, map } from 'rxjs';

import { FeedbackApiService } from '../../services/feedback-api.service';
import { Feedback } from '../../models/feedback.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';

type FeedbackSortField = 'id' | 'rating' | 'createdAt';

@Component({
  selector: 'app-feedback',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDialogModule,
    MatCardModule,
    AlertComponent,
  ],
  templateUrl: './feedback.component.html',
  styleUrls: ['./feedback.component.scss'],
})
export class FeedbackComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly feedbackApi = inject(FeedbackApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly breakpointObserver = inject(BreakpointObserver);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 767px)').pipe(map((r) => r.matches)),
    { initialValue: false },
  );

  private readonly STORAGE_KEY = 'feedbackState';

  // Table columns
  displayedColumns = ['id', 'bookingId', 'rating', 'comments', 'createdAt', 'actions'];

  // Data (canonical signals)
  entries = signal<Feedback[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query state (canonical signals)
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal<FeedbackSortField>('createdAt');
  sortDescending = signal(true);

  // UI input (form control)
  includeHiddenControl = new FormControl(false, { nonNullable: true });

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.feedbackApi
      .getAll({
        includeHidden: this.includeHiddenControl.value,
        pageNumber: this.pageIndex() + 1,
        pageSize: this.pageSize(),
        sortBy: this.sortField(),
        sortDescending: this.sortDescending(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.entries.set(res.data);
          this.totalCount.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.pageSize()) - 1);
          if (this.pageIndex() > maxPage) {
            this.pageIndex.set(maxPage);
            this.saveState();
          }
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  onIncludeHiddenToggle(value: boolean): void {
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: Sort): void {
    if (!event.active || !event.direction) return;
    const field = event.active as FeedbackSortField;
    if (!['id', 'rating', 'createdAt'].includes(field)) return;
    this.sortField.set(field);
    this.sortDescending.set(event.direction === 'desc');
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.saveState();
    this.fetchData();
  }

  onToggleHidden(feedback: Feedback, isHidden: boolean): void {
    if (!feedback.isHidden && isHidden) {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Hide Feedback',
          message: 'Are you sure you want to hide this feedback?',
        },
      });
      dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
        if (confirmed) {
          this.performToggle(feedback, isHidden);
        } else {
          // Revert the optimistic UI toggle
          this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden: false } : f));
        }
      });
    } else {
      this.performToggle(feedback, isHidden);
    }
  }

  private performToggle(feedback: Feedback, isHidden: boolean): void {
    // Optimistic update
    this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden } : f));

    this.feedbackApi.moderate(feedback.id, { isHidden }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open(
          isHidden ? 'Feedback hidden' : 'Feedback visible',
          'Close',
          { duration: 2000 }
        );
      },
      error: (err: any) => {
        // Revert on failure
        this.entries.update(arr => arr.map(f => f.id === feedback.id ? { ...f, isHidden: !isHidden } : f));
        this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 });
      }
    });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }

  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;

      if (typeof parsed.includeHidden === 'boolean')
        this.includeHiddenControl.setValue(parsed.includeHidden);
      if (parsed.sortField === 'id' || parsed.sortField === 'rating' || parsed.sortField === 'createdAt') this.sortField.set(parsed.sortField);
      if (typeof parsed.sortDescending === 'boolean')
        this.sortDescending.set(parsed.sortDescending);
      if (Number.isInteger(parsed.pageIndex) && parsed.pageIndex >= 0)
        this.pageIndex.set(parsed.pageIndex);
      if (Number.isInteger(parsed.pageSize) && parsed.pageSize > 0)
        this.pageSize.set(parsed.pageSize);
    } catch {
      /* fallback silently */
    }
  }

  private saveState(): void {
    sessionStorage.setItem(
      this.STORAGE_KEY,
      JSON.stringify({
        includeHidden: this.includeHiddenControl.value,
        sortField: this.sortField(),
        sortDescending: this.sortDescending(),
        pageIndex: this.pageIndex(),
        pageSize: this.pageSize(),
      }),
    );
  }
}
