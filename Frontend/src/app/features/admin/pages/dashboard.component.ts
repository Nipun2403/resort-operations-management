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
import { BreakpointObserver } from '@angular/cdk/layout';
import { forkJoin } from 'rxjs';
import { finalize, map } from 'rxjs/operators';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';

import { AlertComponent } from '../../auth/components/alert.component';
import { AnalyticsApiService } from '../services/analytics-api.service';
import { HousekeepingApiService } from '../services/housekeeping-api.service';
import { MaintenanceApiService } from '../services/maintenance-api.service';
import { AuditLogApiService } from '../services/audit-log-api.service';
import { AnalyticsDashboardDTO } from '../models/analytics-dashboard.dto';
import { AuditLogEntry } from '../models/audit-log-entry.model';
import { CreateInternalTicketDialogComponent } from '../components/create-internal-ticket-dialog.component';

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
  private readonly breakpointObserver = inject(BreakpointObserver);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 768px)').pipe(
      map(result => result.matches)
    ),
    { initialValue: false }
  );

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
    const mobile = this.isMobile();
    if (!a)
      return {
        xAxis: { type: 'category', data: [] },
        yAxis: { type: 'value' },
        series: [],
      };
    return {
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'axis',
        backgroundColor: '#1b1c19',
        borderColor: 'rgba(228, 194, 133, 0.2)',
        textStyle: { color: '#e4e2dd' }
      },
      grid: {
        left: mobile ? '5%' : '4%',
        right: mobile ? '5%' : '4%',
        bottom: mobile ? '15%' : '10%',
        top: mobile ? '15%' : '10%',
        containLabel: true
      },
      xAxis: {
        type: 'category',
        data: ['Total Revenue', 'Gross Turnover'],
        axisLine: { lineStyle: { color: 'rgba(228, 226, 221, 0.2)' } },
        axisLabel: { color: '#c8c6c5', fontFamily: 'Manrope', fontSize: mobile ? 10 : 11 }
      },
      yAxis: {
        type: 'value',
        splitLine: { lineStyle: { color: 'rgba(228, 226, 221, 0.05)' } },
        axisLabel: { color: '#c8c6c5', fontFamily: 'Manrope', fontSize: mobile ? 10 : 11 }
      },
      series: [
        {
          type: 'bar',
          data: [a.totalRevenue, a.grossTurnover],
          barWidth: mobile ? '55%' : '40%',
          itemStyle: {
            color: '#e4c285',
            borderRadius: [4, 4, 0, 0]
          }
        },
      ],
    };
  });

  // Expenditure chart options computed
  expenditureChartOptions = computed(() => {
    const a = this.analytics();
    const mobile = this.isMobile();
    if (!a)
      return {
        xAxis: { type: 'category', data: [] },
        yAxis: { type: 'value' },
        series: [],
      };
    return {
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'item',
        backgroundColor: '#1b1c19',
        borderColor: 'rgba(228, 194, 133, 0.2)',
        textStyle: { color: '#e4e2dd' }
      },
      legend: {
        orient: mobile ? 'horizontal' : 'vertical',
        left: mobile ? 'center' : 'left',
        bottom: mobile ? 0 : 'auto',
        textStyle: { color: '#e4e2dd', fontFamily: 'Manrope', fontSize: 10 }
      },
      series: [
        {
          type: 'pie',
          radius: mobile ? ['35%', '60%'] : ['45%', '75%'],
          center: mobile ? ['50%', '40%'] : ['50%', '50%'],
          avoidLabelOverlap: true,
          itemStyle: {
            borderRadius: 4,
            borderColor: '#131411',
            borderWidth: 2
          },
          label: {
            show: !mobile,
            color: '#e4e2dd',
            fontFamily: 'Manrope',
            fontSize: 10,
            formatter: '{b}: ${c}'
          },
          emphasis: {
            label: {
              show: true,
              fontSize: 11,
              fontWeight: 'bold'
            }
          },
          data: [
            {
              name: 'Food',
              value: a.nonRoomExpenditure.totalFoodSpend,
              itemStyle: { color: '#e4c285' }
            },
            {
              name: 'Amenities',
              value: a.nonRoomExpenditure.totalAmenitySpend,
              itemStyle: { color: 'rgba(228, 194, 133, 0.5)' }
            },
          ],
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

  splitKpiValue(value: string): { prefix: string; number: string; unit: string } {
    if (!value || value === '—') return { prefix: '', number: '—', unit: '' };
    
    let prefix = '';
    let unit = '';
    let number = value;

    if (value.startsWith('$')) {
      prefix = '$';
      number = value.substring(1);
    }

    if (number.endsWith('%')) {
      unit = '%';
      number = number.slice(0, -1);
    } else if (number.endsWith(' days')) {
      unit = 'D';
      number = number.slice(0, -5);
    } else if (number.endsWith('/5')) {
      unit = '/5';
      number = number.slice(0, -2);
    }

    return { prefix, number, unit };
  }

  getKpiSubtext(label: string): { text: string; icon: string; isDecrease?: boolean } {
    switch (label) {
      case 'Occupancy Rate':
        return { text: '+2.4% vs LAST MO.', icon: 'trending_up' };
      case 'Avg Daily Rate':
        return { text: '+12% ANNUAL', icon: 'trending_up' };
      case 'RevPAR':
        return { text: 'STABLE', icon: 'horizontal_rule' };
      case 'Guest Satisfaction':
        return { text: '98th PERCENTILE', icon: 'star' };
      case 'Cancellation Rate':
        return { text: '-0.4% DECREASE', icon: 'trending_down', isDecrease: true };
      case 'Avg Length of Stay':
        return { text: '+1.5 DAYS', icon: 'trending_up' };
      default:
        return { text: '', icon: '' };
    }
  }

  getAuditSummary(entry: AuditLogEntry): string {
    const newKeys = Object.keys(entry.newValues ?? {});
    if (newKeys.length > 0) {
      return `${entry.action} on ${entry.entityName}: ${newKeys.slice(0, 2).join(', ')}`;
    }
    return `${entry.action} on ${entry.entityName}`;
  }
}
