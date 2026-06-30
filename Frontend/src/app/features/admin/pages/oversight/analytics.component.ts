import { CommonModule } from '@angular/common';
import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSelectModule } from '@angular/material/select';
import { NgxEchartsDirective } from 'ngx-echarts';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs/operators';

import { AnalyticsApiService } from '../../services/analytics-api.service';
import { AnalyticsDashboardDTO } from '../../models/analytics-dashboard.dto';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

function optionalLetterPattern() {
  return null;
}

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatButtonToggleModule,
    MatSelectModule,
    NgxEchartsDirective,
    AlertComponent,
  ],
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss'],
})
export class AnalyticsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly analyticsApi = inject(AnalyticsApiService);

  // Data Signals
  analytics = signal<AnalyticsDashboardDTO | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  // Date controls
  presetControl = new FormControl<'last7' | 'last30' | 'thisMonth' | 'custom'>(
    'last7',
    { nonNullable: true },
  );
  startDateCtrl = new FormControl<Date | null>(null);
  endDateCtrl = new FormControl<Date | null>(null);

  // Category dropdown and reactive signal
  categoryControl = new FormControl<'all' | 'revenue' | 'operations' | 'guests'>(
    'all',
    { nonNullable: true },
  );
  categorySignal = signal<'all' | 'revenue' | 'operations' | 'guests'>('all');

  kpiCards = computed(() => {
    const d = this.analytics();
    if (!d) return [];
    return [
      { label: 'Total Revenue', value: '$' + (d.totalRevenue?.toLocaleString() || '0') },
      { label: 'Occupancy Rate', value: (d.occupancyRate || 0) + '%' },
      { label: 'Guest Satisfaction', value: (d.guestSatisfactionScore || 0) + '%' },
      { label: 'Avg Daily Rate', value: '$' + (d.averageDailyRate || 0) },
    ];
  });

  revenueChartOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    const cat = this.categorySignal();
    let xData: string[] = [];
    let yData: number[] = [];
    switch (cat) {
      case 'all':
      case 'revenue':
        xData = ['Total Revenue', 'Gross Turnover', 'RevPAR', 'Avg Daily Rate'];
        yData = [d.totalRevenue, d.grossTurnover, d.revPAR, d.averageDailyRate];
        break;
      case 'operations':
        xData = ['Occupancy', 'Cancellation', 'Length of Stay', 'HK Turnaround'];
        yData = [
          d.occupancyRate,
          d.cancellationRate,
          d.averageLengthOfStay,
          d.averageHousekeepingTurnaroundMinutes,
        ];
        break;
      case 'guests':
        xData = ['Satisfaction', 'Food Spend', 'Amenity Spend'];
        yData = [
          d.guestSatisfactionScore,
          d.nonRoomExpenditure.totalFoodSpend,
          d.nonRoomExpenditure.totalAmenitySpend,
        ];
        break;
    }
    return {
      backgroundColor: 'transparent',
      textStyle: { color: '#c4c7c7', fontFamily: 'Outfit, sans-serif' },
      tooltip: { trigger: 'axis' },
      grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
      xAxis: { 
        type: 'category', 
        data: xData,
        axisLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.3)' } }
      },
      yAxis: { 
        type: 'value',
        splitLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.1)' } }
      },
      series: [{ type: 'bar', data: yData, color: '#e4c285' }],
    };
  });

  lineChartOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    const cat = this.categorySignal();
    let xData: string[] = [];
    let yData: number[] = [];
    switch (cat) {
      case 'all':
      case 'revenue':
        xData = ['Total Revenue', 'Gross Turnover', 'RevPAR', 'Avg Daily Rate'];
        yData = [d.totalRevenue, d.grossTurnover, d.revPAR, d.averageDailyRate];
        break;
      case 'operations':
        xData = ['Occupancy', 'Cancellation', 'Length of Stay', 'HK Turnaround'];
        yData = [
          d.occupancyRate,
          d.cancellationRate,
          d.averageLengthOfStay,
          d.averageHousekeepingTurnaroundMinutes,
        ];
        break;
      case 'guests':
        xData = ['Satisfaction', 'Food Spend', 'Amenity Spend'];
        yData = [
          d.guestSatisfactionScore,
          d.nonRoomExpenditure.totalFoodSpend,
          d.nonRoomExpenditure.totalAmenitySpend,
        ];
        break;
    }
    return {
      backgroundColor: 'transparent',
      textStyle: { color: '#c4c7c7', fontFamily: 'Outfit, sans-serif' },
      tooltip: { trigger: 'axis' },
      grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
      xAxis: { 
        type: 'category', 
        data: xData,
        axisLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.3)' } }
      },
      yAxis: { 
        type: 'value',
        splitLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.1)' } }
      },
      series: [{ type: 'line', data: yData, color: '#e4c285', smooth: true }],
    };
  });

  radarChartOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    const cat = this.categorySignal();
    let indicator: any[] = [];
    let value: number[] = [];
    switch (cat) {
      case 'all':
        indicator = [
          { name: 'Occupancy', max: 100 },
          { name: 'Cancellation', max: 50 },
          { name: 'Length of Stay', max: 30 },
          { name: 'Satisfaction', max: 100 },
        ];
        value = [
          d.occupancyRate,
          d.cancellationRate,
          d.averageLengthOfStay,
          d.guestSatisfactionScore,
        ];
        break;
      case 'revenue':
        indicator = [
          { name: 'Occupancy', max: 100 },
          { name: 'RevPAR', max: 2000 },
          { name: 'Avg Daily Rate', max: 500 },
          { name: 'Turnover', max: 20000 },
        ];
        value = [d.occupancyRate, d.revPAR, d.averageDailyRate, d.grossTurnover];
        break;
      case 'operations':
        indicator = [
          { name: 'Occupancy', max: 100 },
          { name: 'Cancellation', max: 50 },
          { name: 'Length of Stay', max: 30 },
          { name: 'Satisfaction', max: 100 },
        ];
        value = [
          d.occupancyRate,
          d.cancellationRate,
          d.averageLengthOfStay,
          d.guestSatisfactionScore,
        ];
        break;
      case 'guests':
        indicator = [
          { name: 'Satisfaction', max: 100 },
          { name: 'Occupancy', max: 100 },
          { name: 'Length of Stay', max: 30 },
        ];
        value = [
          d.guestSatisfactionScore,
          d.occupancyRate,
          d.averageLengthOfStay,
        ];
        break;
    }
    return {
      backgroundColor: 'transparent',
      textStyle: { color: '#c4c7c7', fontFamily: 'Outfit, sans-serif' },
      radar: {
        indicator,
        axisLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.2)' } },
        splitLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.05)' } },
        splitArea: { show: false }
      },
      series: [{
        type: 'radar',
        data: [{ value, name: 'Current' }],
        lineStyle: { color: '#e4c285' },
        areaStyle: { color: 'rgba(228, 194, 133, 0.05)' }
      }],
    };
  });

  pieChartOptions = computed(() => {
    const d = this.analytics();
    if (!d) return {};
    const cat = this.categorySignal();
    if (cat === 'revenue' || cat === 'operations') {
      return {}; // hidden (no data)
    }
    return {
      backgroundColor: 'transparent',
      textStyle: { color: '#c4c7c7', fontFamily: 'Outfit, sans-serif' },
      tooltip: { trigger: 'item' },
      color: ['#e4c285', '#d5b478', '#5d4514'],
      series: [
        {
          type: 'pie',
          radius: '55%',
          data: [
            { name: 'Food', value: d.nonRoomExpenditure.totalFoodSpend },
            { name: 'Amenities', value: d.nonRoomExpenditure.totalAmenitySpend },
          ],
          label: { 
            formatter: '{b}: {c} ({d}%)',
            color: '#c4c7c7'
          },
        },
      ],
    };
  });

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

  onPresetChange(): void {
    const preset = this.presetControl.value;
    if (preset === 'custom') {
      return;
    }
    const dates = this.getPresetDates(preset);
    if (dates) {
      this.fetchData(dates.start, dates.end);
    } else {
      this.fetchData();
    }
  }

  applyCustomRange(): void {
    const start = this.startDateCtrl.value;
    const end = this.endDateCtrl.value;
    if (start && end) {
      const startDate = new Date(start);
      startDate.setHours(0, 0, 0, 0);
      const endDate = new Date(end);
      endDate.setHours(23, 59, 59, 999);
      this.fetchData(startDate.toISOString(), endDate.toISOString());
    }
  }

  onCategoryChange(): void {
    this.categorySignal.set(this.categoryControl.value);
  }

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
}
