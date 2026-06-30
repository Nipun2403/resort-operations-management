import { AfterViewInit, Component, ElementRef, OnInit, QueryList, ViewChildren, inject, signal, computed, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatRadioModule } from '@angular/material/radio';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { NgxEchartsDirective } from 'ngx-echarts';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AlertComponent } from '../../auth/components/alert.component';
import { AnalyticsApiService } from '../services/analytics-api.service';
import { HousekeepingApiService } from '../services/housekeeping-api.service';
import { MaintenanceApiService } from '../services/maintenance-api.service';
import { AuditLogApiService } from '../services/audit-log-api.service';
import { AnalyticsDashboardDTO } from '../models/analytics-dashboard.dto';
import { AuditLogEntry } from '../models/audit-log-entry.model';
import { CreateInternalTicketDialogComponent } from '../components/create-internal-ticket-dialog.component';
import { ActiveTicketsDialogComponent } from '../../front-desk/components/active-tickets-dialog/active-tickets-dialog.component';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTableModule,
    MatDialogModule,
    MatRadioModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatSnackBarModule,
    NgxEchartsDirective,
    AlertComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit, AfterViewInit {
  @ViewChildren('chartRef') charts!: QueryList<ElementRef>;
  private readonly analyticsApi = inject(AnalyticsApiService);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly auditLogApi = inject(AuditLogApiService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  // Form controls
  startDateCtrl = new FormControl<Date | null>(null);
  endDateCtrl = new FormControl<Date | null>(null);

  // Analytics signals
  analytics = signal<AnalyticsDashboardDTO | null>(null);
  analyticsLoading = signal(false);
  analyticsError = signal<string | null>(null);

  // Pending counts signals
  housekeepingPendingCount = signal(0);
  maintenancePendingCount = signal(0);
  pendingLoading = signal(false);
  pendingError = signal<string | null>(null);

  // Audit log signals
  auditEntries = signal<AuditLogEntry[]>([]);
  auditLoading = signal(false);
  auditError = signal<string | null>(null);

  // Ticket creation feedback
  ticketCreatedMessage = signal<string | null>(null);

  // Table column definition
  displayedColumns = ['timestamp', 'entity', 'action', 'changedBy', 'summary'];

  // KPI cards computed
  kpiCards = computed(() => {
    const a = this.analytics();
    if (!a) {
      return [
        { label: 'Occupancy Rate', value: '—' },
        { label: 'Avg Daily Rate', value: '—' },
        { label: 'RevPAR', value: '—' },
        { label: 'Guest Satisfaction', value: '—' },
        { label: 'Cancellation Rate', value: '—' },
        { label: 'Avg Length of Stay', value: '—' },
      ];
    }
    return [
      { label: 'Occupancy Rate', value: `${a.occupancyRate}%` },
      { label: 'Avg Daily Rate', value: `$${a.averageDailyRate}` },
      { label: 'RevPAR', value: `$${a.revPAR}` },
      { label: 'Guest Satisfaction', value: `${a.guestSatisfactionScore}%` },
      { label: 'Cancellation Rate', value: `${a.cancellationRate}%` },
      { label: 'Avg Length of Stay', value: `${a.averageLengthOfStay} days` },
    ];
  });

  // Revenue chart options computed
  revenueChartOptions = computed(() => {
    const a = this.analytics();
    if (!a)
      return {
        xAxis: { type: 'category', data: [] },
        yAxis: { type: 'value' },
        series: [],
      };
    return {
      backgroundColor: 'transparent',
      textStyle: { color: '#c4c7c7', fontFamily: 'Outfit, sans-serif' },
      title: { text: 'Revenue Overview', textStyle: { color: '#e4e2dd', fontFamily: 'Outfit, sans-serif' } },
      tooltip: { trigger: 'axis' },
      xAxis: {
        type: 'category',
        data: ['Total Revenue', 'Gross Turnover'],
        axisLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.3)' } }
      },
      yAxis: {
        type: 'value',
        axisLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.3)' } },
        splitLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.1)' } }
      },
      series: [
        {
          type: 'bar',
          data: [a.totalRevenue, a.grossTurnover],
          color: '#e4c285',
        },
      ],
    };
  });

  // Expenditure chart options computed
  expenditureChartOptions = computed(() => {
    const a = this.analytics();
    if (!a)
      return {
        xAxis: { type: 'category', data: [] },
        yAxis: { type: 'value' },
        series: [],
      };
    return {
      backgroundColor: 'transparent',
      textStyle: { color: '#c4c7c7', fontFamily: 'Outfit, sans-serif' },
      title: { text: 'Non‑Room Expenditure', textStyle: { color: '#e4e2dd', fontFamily: 'Outfit, sans-serif' } },
      tooltip: { trigger: 'item' },
      series: [
        {
          type: 'pie',
          data: [
            { name: 'Food', value: a.nonRoomExpenditure.totalFoodSpend, itemStyle: { color: '#e4c285' } },
            { name: 'Amenities', value: a.nonRoomExpenditure.totalAmenitySpend, itemStyle: { color: '#8e9192' } },
          ],
          label: { formatter: '{b}: {c} ({d}%)', textStyle: { color: '#c4c7c7' } },
        },
      ],
    };
  });

  ngOnInit(): void {
    this.loadAnalytics();
    this.loadPendingCounts();
    this.loadAuditLogs();
  }

  ngAfterViewInit(): void {
    // Force ECharts to recalculate dimensions after view initialisation
    setTimeout(() => {
      window.dispatchEvent(new Event('resize'));
    });
  }

  loadAnalytics(params?: { startDate?: string; endDate?: string }): void {
    this.analyticsLoading.set(true);
    this.analyticsError.set(null);
    this.analyticsApi
      .getAnalytics(params)
      .pipe(finalize(() => this.analyticsLoading.set(false)))
      .subscribe({
        next: (data) => this.analytics.set(data),
        error: (err) => this.analyticsError.set(err.error?.message || 'Failed to load analytics'),
      });
  }

  loadPendingCounts(): void {
    this.pendingLoading.set(true);
    this.pendingError.set(null);
    forkJoin({
      hk: this.housekeepingApi.getAll({ status: 'Pending', pageNumber: 1, pageSize: 10 }),
      mt: this.maintenanceApi.getAll({ status: 'Pending', pageNumber: 1, pageSize: 10 }),
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.pendingLoading.set(false)),
      )
      .subscribe({
        next: ({ hk, mt }) => {
          this.housekeepingPendingCount.set(hk.totalCount);
          this.maintenancePendingCount.set(mt.totalCount);
        },
        error: (err: Error) => this.pendingError.set(err.message),
      });
  }

  loadAuditLogs(): void {
    this.auditLoading.set(true);
    this.auditError.set(null);
    this.auditLogApi
      .getAll({ sortBy: 'timestamp', sortDescending: true, pageSize: 5, pageNumber: 1 })
      .pipe(finalize(() => this.auditLoading.set(false)))
      .subscribe({
        next: (res) => this.auditEntries.set(res && Array.isArray(res.data) ? res.data : []),
        error: (err) => this.auditError.set(err.error?.message || 'Failed to load audit logs'),
      });
  }

  applyDateFilter(): void {
    const start = this.startDateCtrl.value;
    const end = this.endDateCtrl.value;
    if (!start || !end) return;
    const startISO = `${start.toISOString().split('T')[0]}T00:00:00Z`;
    const endISO = `${end.toISOString().split('T')[0]}T23:59:59Z`;
    this.loadAnalytics({ startDate: startISO, endDate: endISO });
  }

  clearDateFilter(): void {
    this.startDateCtrl.reset();
    this.endDateCtrl.reset();
    this.loadAnalytics();
  }

  openCreateTicketDialog(): void {
    const dialogRef = this.dialog.open(CreateInternalTicketDialogComponent);
    dialogRef.afterClosed().subscribe((result) => {
      if (result === true) {
        this.ticketCreatedMessage.set('Ticket created successfully');
        this.loadPendingCounts();
        setTimeout(() => this.ticketCreatedMessage.set(null), 3000);
      }
    });
  }

  getAuditSummary(entry: AuditLogEntry): string {
    const newKeys = Object.keys(entry.newValues ?? {});
    if (newKeys.length > 0) {
      return `${entry.action} on ${entry.entityName}: ${newKeys.slice(0, 2).join(', ')}`;
    }
    return `${entry.action} on ${entry.entityName}`;
  }

  openActiveTickets(): void {
    this.dialog.open(ActiveTicketsDialogComponent, {
      data: {
        housekeepingCount: this.housekeepingPendingCount(),
        maintenanceCount: this.maintenancePendingCount(),
        foodOrdersCount: 0,
      },
      width: '90vw',
      maxWidth: '1200px',
    });
  }
}
