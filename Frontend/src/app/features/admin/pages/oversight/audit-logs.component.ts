import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { debounceTime, distinctUntilChanged, finalize, map } from 'rxjs';

import { AuditLogApiService } from '../../services/audit-log-api.service';
import { AuditLogEntry } from '../../models/audit-log-entry.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { AuditLogDetailDialogComponent } from './audit-log-detail-dialog.component';

type AuditSortField = 'id' | 'timestamp';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDialogModule,
    MatCardModule,
    AlertComponent,
  ],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.scss'],
})
export class AuditLogsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly auditLogApi = inject(AuditLogApiService);
  private readonly dialog = inject(MatDialog);
  private readonly breakpointObserver = inject(BreakpointObserver);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 767px)').pipe(map((r) => r.matches)),
    { initialValue: false },
  );

  private readonly STORAGE_KEY = 'auditLogsState';

  // Table columns
  displayedColumns = ['id', 'entityName', 'action', 'changedBy', 'timestamp', 'actions'];

  // Data (canonical signals)
  entries = signal<AuditLogEntry[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);

  // Query state (canonical signals)
  pageIndex = signal(0);
  pageSize = signal(10);
  sortField = signal<AuditSortField>('timestamp');
  sortDescending = signal(false);

  // UI input (form control)
  searchControl = new FormControl('', { nonNullable: true });

  ngOnInit(): void {
    this.restoreState();
    this.fetchData();
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.pageIndex.set(0);
        this.saveState();
        this.fetchData();
      });
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);
    this.auditLogApi
      .getAll({
        guestQuery: this.searchControl.value?.trim() || undefined,
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

  onSearchDebounced(): void {
    // debounce is handled by a dedicated subscription in ngOnInit
  }

  clearSearch(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.pageIndex.set(0);
    this.saveState();
    this.fetchData();
  }

  onSortChange(event: Sort): void {
    if (!event.active || !event.direction) return;
    const field = event.active as AuditSortField;
    if (!['id', 'timestamp'].includes(field)) return;
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

  openDetail(entry: AuditLogEntry): void {
    this.dialog.open(AuditLogDetailDialogComponent, {
      data: entry,
      maxWidth: '700px',
      width: '90%',
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

      if (typeof parsed.searchQuery === 'string') this.searchControl.setValue(parsed.searchQuery);
      if (parsed.sortField === 'id' || parsed.sortField === 'timestamp') this.sortField.set(parsed.sortField);
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
        searchQuery: this.searchControl.value,
        sortField: this.sortField(),
        sortDescending: this.sortDescending(),
        pageIndex: this.pageIndex(),
        pageSize: this.pageSize(),
      }),
    );
  }
}
