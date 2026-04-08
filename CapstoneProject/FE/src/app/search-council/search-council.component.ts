import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

interface DefenseScheduleDto {
  defenseId: number;
  groupId: number;
  groupName: string;
  councilId: number;
  room: string;
  startTime: string;
  endTime: string;
  status: string;
}

@Component({
  selector: 'app-search-council',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="search-container">
      <h2>Council Search</h2>
      <div class="search-form">
        <input type="text" [(ngModel)]="keyword" placeholder="Enter keyword (group name, room)" />
        <select [(ngModel)]="status">
          <option value="">All Status</option>
          <option value="Scheduled">Scheduled</option>
          <option value="Completed">Completed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
        <button (click)="search()">Search</button>
      </div>
      <div class="results">
        <div *ngIf="results.length === 0 && hasSearched" class="no-results">
          No results found
        </div>
        <ul *ngIf="results.length > 0">
          <li *ngFor="let item of results" class="result-item">
            <strong>{{ item.groupName }}</strong> - Room: {{ item.room }} - Status: {{ item.status }} - Time: {{ item.startTime | date:'short' }}
          </li>
        </ul>
      </div>
    </div>
  `,
  styles: [`
    .search-container { max-width: 800px; margin: 0 auto; padding: 20px; }
    .search-form { display: flex; gap: 10px; margin-bottom: 20px; }
    .search-form input, .search-form select { padding: 8px; flex: 1; }
    .results { margin-top: 20px; }
    .result-item { margin-bottom: 10px; padding: 10px; border: 1px solid #ccc; }
    .no-results { text-align: center; color: gray; }
  `]
})
export class SearchCouncilComponent {
  keyword = '';
  status = '';
  results: DefenseScheduleDto[] = [];
  hasSearched = false;

  constructor(private http: HttpClient) {}

  search() {
    this.hasSearched = true;
    const params = {
      keyword: this.keyword || undefined,
      status: this.status || undefined
    };

    this.http.get<DefenseScheduleDto[]>('/api/defense/search', { params }).subscribe(data => {
      this.results = data;
    });
  }
}