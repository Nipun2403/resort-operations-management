import { Component, inject, signal, computed, DestroyRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { NgxEchartsDirective } from 'ngx-echarts';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { AnalyticsApiService } from '../../services/analytics-api.service';
import { AnalyticsDashboardDTO } from '../../models/analytics-dashboard.dto';
import { AlertComponent } from '../../../auth/components/alert.component';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatButtonToggleModule,
    NgxEchartsDirective,
    AlertComponent,
  ],
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss'],
})
export class AnalyticsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly analyticsApi = inject(AnalyticsApiService);

  private readonly STORAGE_KEY = 'analyticsState';

  // Data
  analytics = signal<AnalyticsDashboardDTO | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  // Date range
  presetControl = new FormControl<'last7' | 'last30' | 'thisMonth' | 'custom'>(
    'last7',
    { nonNullable: true },
  );
  startDateCtrl = new FormControl<Date | null>(null);
  endDateCtrl = new FormControl<Date | null>(null);

  // Comparison
  comparisonMode = signal(false);
  comparisonAnalytics = signal<AnalyticsDashboardDTO | null>(null);
  comparisonStartDateCtrl = new FormControl<Date | null>(null);
  comparisonEndDateCtrl = new FormControl<Date | null>(null);
  comparisonPresetControl = new FormControl<
    'last7' | 'last30' | 'thisMonth' | 'custom'
  >('last7', { nonNullable: true });

  ngOnInit(): void {
    this.fetchData();
  }

  fetchData(startDate?: string, endDate?: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.analyticsApi
      .getAnalytics({ startDate, endDate })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => this.analytics.set(data),
        error: (err: any) =>
          this.error.set(err instanceof Error ? err.message : 'Unexpected error'),
      });
  }

  fetchComparisonData(startDate?: string, endDate?: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.analyticsApi
      .getAnalytics({ startDate, endDate })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => this.comparisonAnalytics.set(data),
        error: (err: any) =>
          this.error.set(err instanceof Error ? err.message : 'Unexpected error'),
      });
  }

  onPresetChange(): void {
    const preset = this.presetControl.value;
    if (preset === 'custom') {
      return;
    }
    const dates = this.getPresetDates(preset);
    if (dates) {
      this.fetchData(dates.start, dates.end);
    }
  }

  onComparisonPresetChange(): void {
    const preset = this.comparisonPresetControl.value;
    if (preset === 'custom') {
      return;
    }
    const dates = this.getPresetDates(preset);
    if (dates) {
      this.fetchComparisonData(dates.start, dates.end);
    }
  }

  applyCustomRange(): void {
    const start = this.startDateCtrl.value;
    const end = this.endDateCtrl.value;
    if (start && end) {
      const s = new Date(start);
      s.setHours(0, 0, 0, 0);
      const e = new Date(end);
      e.setHours(23, 59, 59, 999);
      this.fetchData(s.toISOString(), e.toISOString());
    }
  }

  applyComparisonCustomRange(): void {
    const start = this.comparisonStartDateCtrl.value;
    const end = this.comparisonEndDateCtrl.value;
    if (start && end) {
      const s = new Date(start);
      s.setHours(0, 0, 0, 0);
      const e = new Date(end);
      e.setHours(23, 59, 59, 999);
      this.fetchComparisonData(s.toISOString(), e.toISOString());
    }
  }

  onTabChange(event: any): void {
    // Empty tab change handler to satisfy output binding
  }

  // Preset date math helper
  private getPresetDates(preset: string): { start: string; end: string } | null {
    const now = new Date();
    let start: Date;
    let end: Date = now;
    switch (preset) {
      case 'last7':
        start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
        break;
      case 'last30':
        start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
        break;
      case 'thisMonth':
        start = new Date(now.getFullYear(), now.getMonth(), 1);
        break;
      default:
        return null;
    }
    start.setHours(0, 0, 0, 0);
    end.setHours(23, 59, 59, 999);
    return { start: start.toISOString(), end: end.toISOString() };
  }

  // Computed ECharts options
  revenueBarOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      title: { text: 'Revenue & Turnover' },
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: ['Total Revenue', 'Gross Turnover'] },
      yAxis: { type: 'value' },
      series: [
        {
          type: 'bar',
          data: [d.totalRevenue, d.grossTurnover],
          color: '#1976d2',
        },
      ],
    };
  });

  revParGaugeOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      series: [
        {
          type: 'gauge',
          min: 0,
          max: Math.max(d.revPAR * 1.5, 1000),
          detail: { formatter: '${value}' },
          data: [{ value: d.revPAR, name: 'RevPAR' }],
        },
      ],
    };
  });

  occupancyGaugeOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      series: [
        {
          type: 'gauge',
          min: 0,
          max: 100,
          detail: { formatter: '{value}%' },
          data: [{ value: d.occupancyRate, name: 'Occupancy' }],
        },
      ],
    };
  });

  radarOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      title: { text: 'Operational Overview' },
      radar: {
        indicator: [
          { name: 'Occupancy', max: 100 },
          { name: 'Cancellation', max: 50 },
          { name: 'Length of Stay', max: 30 },
          { name: 'Satisfaction', max: 100 },
        ],
      },
      series: [
        {
          type: 'radar',
          data: [
            {
              value: [
                d.occupancyRate,
                d.cancellationRate,
                d.averageLengthOfStay,
                d.guestSatisfactionScore,
              ],
              name: 'Current',
            },
          ],
        },
      ],
    };
  });

  satisfactionGaugeOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      series: [
        {
          type: 'gauge',
          min: 0,
          max: 100,
          detail: { formatter: '{value}%' },
          data: [{ value: d.guestSatisfactionScore, name: 'Satisfaction' }],
        },
      ],
    };
  });

  expenditurePieOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    return {
      title: { text: 'Non-Room Expenditure' },
      tooltip: { trigger: 'item' },
      series: [
        {
          type: 'pie',
          data: [
            { name: 'Food', value: d.nonRoomExpenditure.totalFoodSpend },
            {
              name: 'Amenities',
              value: d.nonRoomExpenditure.totalAmenitySpend,
            },
          ],
          label: { formatter: '{b}: {c} ({d}%)' },
        },
      ],
    };
  });

  comparisonBarOptions = computed(() => {
    const a = this.analytics();
    const b = this.comparisonAnalytics();
    if (!a || !b) return {};
    return {
      title: { text: 'Comparison' },
      tooltip: { trigger: 'axis' },
      legend: { data: ['Period 1', 'Period 2'] },
      xAxis: {
        type: 'category',
        data: ['Revenue', 'Occupancy', 'Satisfaction'],
      },
      yAxis: { type: 'value' },
      series: [
        {
          name: 'Period 1',
          type: 'bar',
          data: [a.totalRevenue, a.occupancyRate, a.guestSatisfactionScore],
        },
        {
          name: 'Period 2',
          type: 'bar',
          data: [b.totalRevenue, b.occupancyRate, b.guestSatisfactionScore],
        },
      ],
    };
  });
}
