import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import {
  ArcElement,
  Chart,
  ChartData,
  ChartOptions,
  DoughnutController,
  Legend,
  Tooltip
} from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

import { PassFailChartData } from '../models/pass-fail-chart-data.model';

Chart.register(DoughnutController, ArcElement, Tooltip, Legend);

interface DonutSegment {
  label: 'Pass' | 'Fail';
  value: number;
  color: string;
  percentage: number;
}

@Component({
  selector: 'app-pass-fail-donut',
  standalone: true,
  imports: [BaseChartDirective],
  templateUrl: './pass-fail-donut.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PassFailDonutComponent {
  readonly data = input.required<PassFailChartData>();

  readonly total = computed(() => {
    const { pass, fail } = this.data();
    return this.normalizeValue(pass) + this.normalizeValue(fail);
  });

  readonly segments = computed<DonutSegment[]>(() => {
    const { pass, fail } = this.data();
    const safePass = this.normalizeValue(pass);
    const safeFail = this.normalizeValue(fail);
    const total = safePass + safeFail;

    return [
      {
        label: 'Pass',
        value: safePass,
        color: '#22c55e',
        percentage: total > 0 ? Math.round((safePass / total) * 100) : 0
      },
      {
        label: 'Fail',
        value: safeFail,
        color: '#ef4444',
        percentage: total > 0 ? Math.round((safeFail / total) * 100) : 0
      }
    ];
  });

  readonly hasData = computed(() => this.total() > 0);

  readonly doughnutChartData = computed<ChartData<'doughnut'>>(() => ({
    labels: this.segments().map((segment) => segment.label),
    datasets: [
      {
        data: this.segments().map((segment) => segment.value),
        backgroundColor: this.segments().map((segment) => segment.color),
        hoverBackgroundColor: this.segments().map((segment) => segment.color),
        borderColor: ['#ffffff', '#ffffff'],
        borderWidth: 6,
        hoverOffset: 6
      }
    ]
  }));

  readonly doughnutChartOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '72%',
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            const label = context.label ?? '';
            const value = Number(context.raw ?? 0);
            const total = context.dataset.data.reduce((sum, item) => sum + Number(item), 0);
            const percentage = total > 0 ? Math.round((value / total) * 100) : 0;

            return `${label}: ${value} (${percentage}%)`;
          }
        }
      }
    }
  };

  private normalizeValue(value: number): number {
    return Number.isFinite(value) && value > 0 ? value : 0;
  }
}
