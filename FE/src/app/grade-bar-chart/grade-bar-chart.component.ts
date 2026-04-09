import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import {
  BarController,
  BarElement,
  CategoryScale,
  Chart,
  ChartData,
  ChartOptions,
  Legend,
  LinearScale,
  Tooltip
} from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

import { GradeDistributionItem } from '../models/grade-distribution-item.model';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend);

@Component({
  selector: 'app-grade-bar-chart',
  standalone: true,
  imports: [BaseChartDirective],
  templateUrl: './grade-bar-chart.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GradeBarChartComponent {
  readonly data = input.required<GradeDistributionItem[]>();

  private readonly gradeOrder: Array<GradeDistributionItem['grade']> = ['A', 'B', 'C', 'D', 'F'];

  private readonly colorMap: Record<GradeDistributionItem['grade'], string> = {
    A: '#22c55e',
    B: '#3b82f6',
    C: '#f59e0b',
    D: '#f97316',
    F: '#ef4444'
  };

  readonly normalizedData = computed(() =>
    this.gradeOrder.map((grade) => {
      const match = this.data().find((item) => item.grade === grade);
      return {
        grade,
        count: this.normalizeCount(match?.count ?? 0)
      };
    })
  );

  readonly hasData = computed(() => this.normalizedData().some((item) => item.count > 0));

  readonly barChartData = computed<ChartData<'bar'>>(() => ({
    labels: this.normalizedData().map((item) => item.grade),
    datasets: [
      {
        label: 'Số lượng',
        data: this.normalizedData().map((item) => item.count),
        backgroundColor: this.normalizedData().map((item) => this.colorMap[item.grade]),
        borderRadius: 10,
        borderSkipped: false,
        maxBarThickness: 56
      }
    ]
  }));

  readonly barChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        grid: { display: false },
        ticks: {
          color: '#475569',
          font: { size: 13, weight: 600 }
        }
      },
      y: {
        beginAtZero: true,
        ticks: {
          precision: 0,
          color: '#64748b'
        },
        grid: {
          color: 'rgba(148, 163, 184, 0.18)'
        }
      }
    },
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (context) => `Số lượng: ${Number(context.raw ?? 0)}`
        }
      }
    }
  };

  private normalizeCount(value: number): number {
    return Number.isFinite(value) && value > 0 ? Math.round(value) : 0;
  }
}
