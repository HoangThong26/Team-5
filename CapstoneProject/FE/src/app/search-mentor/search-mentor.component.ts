import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

interface TopicDto {
  topicId: number;
  groupId: number;
  title: string;
  description: string;
  status: string;
  groupName: string;
  submittedAt: string;
}

@Component({
  selector: 'app-search-mentor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="search-container">
      <h2>Mentor Search Topics</h2>
      <div class="search-form">
        <input type="text" [(ngModel)]="keyword" placeholder="Enter keyword (title, description)" />
        <select [(ngModel)]="status">
          <option value="">All Status</option>
          <option value="Pending">Pending</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
        </select>
        <button (click)="search()">Search</button>
      </div>
      <div class="results">
        <div *ngIf="results.length === 0 && hasSearched" class="no-results">
          No results found
        </div>
        <ul *ngIf="results.length > 0">
          <li *ngFor="let item of results" class="result-item">
            <strong>{{ item.title }}</strong> - Group: {{ item.groupName }} - Status: {{ item.status }}
            <p>{{ item.description }}</p>
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
export class SearchMentorComponent {
  keyword = '';
  status = '';
  results: TopicDto[] = [];
  hasSearched = false;

  constructor(private http: HttpClient) {}

  search() {
    this.hasSearched = true;
    const params = {
      keyword: this.keyword || undefined,
      status: this.status || undefined,
      supervisorId: 1 // Assume current mentor id, in real app get from auth
    };

    this.http.get<TopicDto[]>('/api/topics/search', { params }).subscribe(data => {
      this.results = data;
    });
  }
}