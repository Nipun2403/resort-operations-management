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
  presetControl = new FormControl<'last7' | 'last30' | 'quarterly' | 'custom'>(
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

  barChartOptions = computed(() => {
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
      tooltip: {
        trigger: 'axis',
        backgroundColor: '#1f201d',
        borderColor: 'rgba(228, 194, 133, 0.2)',
        borderWidth: 1,
        textStyle: { color: '#e4e2dd', fontFamily: 'Manrope' }
      },
      xAxis: {
        type: 'category',
        data: xData,
        axisLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.15)' } },
        axisLabel: { color: '#c4c7c7', fontFamily: 'Manrope', fontSize: 10 }
      },
      yAxis: {
        type: 'value',
        splitLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.05)' } },
        axisLabel: { color: '#c4c7c7', fontFamily: 'Manrope', fontSize: 10 }
      },
      series: [{
        type: 'bar',
        data: yData,
        color: '#e4c285',
        barWidth: '40%',
        itemStyle: {
          color: 'rgba(228, 194, 133, 0.1)',
          borderColor: 'rgba(228, 194, 133, 0.4)',
          borderWidth: 1
        }
      }],
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
      tooltip: {
        trigger: 'axis',
        backgroundColor: '#1f201d',
        borderColor: 'rgba(228, 194, 133, 0.2)',
        borderWidth: 1,
        textStyle: { color: '#e4e2dd', fontFamily: 'Manrope' }
      },
      xAxis: {
        type: 'category',
        data: xData,
        axisLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.15)' } },
        axisLabel: { color: '#c4c7c7', fontFamily: 'Manrope', fontSize: 10 }
      },
      yAxis: {
        type: 'value',
        splitLine: { lineStyle: { color: 'rgba(228, 194, 133, 0.05)' } },
        axisLabel: { color: '#c4c7c7', fontFamily: 'Manrope', fontSize: 10 }
      },
      series: [{
        type: 'line',
        data: yData,
        color: '#e4c285',
        smooth: true,
        symbol: 'circle',
        symbolSize: 6,
        lineStyle: {
          width: 2,
          color: '#e4c285'
        },
        areaStyle: {
          color: {
            type: 'linear',
            x: 0,
            y: 0,
            x2: 0,
            y2: 1,
            colorStops: [{
              offset: 0, color: 'rgba(228, 194, 133, 0.2)'
            }, {
              offset: 1, color: 'rgba(228, 194, 133, 0)'
            }]
          }
        }
      }],
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
      tooltip: {
        trigger: 'item',
        backgroundColor: '#1f201d',
        borderColor: 'rgba(228, 194, 133, 0.2)',
        borderWidth: 1,
        textStyle: { color: '#e4e2dd', fontFamily: 'Manrope' }
      },
      radar: {
        indicator,
        shape: 'polygon',
        splitNumber: 3,
        axisName: {
          color: '#c4c7c7',
          fontFamily: 'Manrope',
          fontSize: 9
        },
        splitLine: {
          lineStyle: {
            color: 'rgba(228, 194, 133, 0.1)'
          }
        },
        splitArea: {
          show: false
        },
        axisLine: {
          lineStyle: {
            color: 'rgba(228, 194, 133, 0.1)'
          }
        }
      },
      series: [{
        type: 'radar',
        data: [{
          value,
          name: 'Current',
          itemStyle: { color: '#e4c285' },
          lineStyle: { color: '#e4c285', width: 1.5 },
          areaStyle: { color: 'rgba(228, 194, 133, 0.05)' }
        }]
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
      tooltip: {
        trigger: 'item',
        backgroundColor: '#1f201d',
        borderColor: 'rgba(228, 194, 133, 0.2)',
        borderWidth: 1,
        textStyle: { color: '#e4e2dd', fontFamily: 'Manrope' }
      },
      legend: {
        orient: 'horizontal',
        bottom: '0',
        textStyle: { color: '#c4c7c7', fontFamily: 'Manrope', fontSize: 10 },
        icon: 'circle'
      },
      series: [
        {
          type: 'pie',
          radius: ['45%', '70%'],
          center: ['50%', '45%'],
          avoidLabelOverlap: false,
          itemStyle: {
            borderRadius: 0,
            borderColor: '#131411',
            borderWidth: 2
          },
          label: {
            show: false,
            position: 'center'
          },
          emphasis: {
            label: {
              show: true,
              fontSize: 14,
              fontWeight: 'bold',
              color: '#e4c285',
              fontFamily: 'Manrope'
            }
          },
          labelLine: {
            show: false
          },
          data: [
            { name: 'Food', value: d.nonRoomExpenditure.totalFoodSpend, itemStyle: { color: '#e4c285' } },
            { name: 'Amenities', value: d.nonRoomExpenditure.totalAmenitySpend, itemStyle: { color: '#d5b478' } },
          ],
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
      this.fetchData(this.toLocalISOString(startDate), this.toLocalISOString(endDate));
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
        start = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 6);
        break;
      case 'last30':
        start = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 29);
        break;
      case 'quarterly':
        const currentQuarterMonth = Math.floor(now.getMonth() / 3) * 3;
        start = new Date(now.getFullYear(), currentQuarterMonth, 1);
        break;
      default:
        return null;
    }
    start.setHours(0, 0, 0, 0);
    end.setHours(23, 59, 59, 999);
    return { start: this.toLocalISOString(start), end: this.toLocalISOString(end) };
  }

  private toLocalISOString(date: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');
    const offset = -date.getTimezoneOffset();
    const sign = offset >= 0 ? '+' : '-';
    const hh = pad(Math.floor(Math.abs(offset) / 60));
    const mm = pad(Math.abs(offset) % 60);
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}.${date.getMilliseconds().toString().padStart(3, '0')}${sign}${hh}:${mm}`;
  }
}
