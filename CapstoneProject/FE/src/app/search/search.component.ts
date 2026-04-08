import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

interface SearchResult {
  type: string;
  data: any[];
}

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="search-container">
      <h2>Search / Filter</h2>
      <div class="search-form">
        <input type="text" [(ngModel)]="keyword" placeholder="Enter keyword" />
        <select [(ngModel)]="filterType">
          <option value="">All</option>
          <option value="groups">Groups</option>
          <option value="topics">Topics</option>
          <option value="users">Users</option>
        </select>
        <select [(ngModel)]="status">
          <option value="">All Status</option>
          <option value="Active">Active</option>
          <option value="Forming">Forming</option>
          <option value="Pending">Pending</option>
          <option value="Approved">Approved</option>
        </select>
        <button (click)="search()">Search</button>
      </div>
      <div class="results">
        <div *ngIf="results.length === 0 && hasSearched" class="no-results">
          No results found
        </div>
        <div *ngFor="let result of results" class="result-item">
          <h3>{{ result.type }}</h3>
          <ul>
            <li *ngFor="let item of result.data">
              <span *ngIf="result.type === 'groups'">{{ item.groupName }} - {{ item.status }}</span>
              <span *ngIf="result.type === 'topics'">{{ item.title }} - {{ item.status }}</span>
              <span *ngIf="result.type === 'users'">{{ item.fullName }} - {{ item.role }}</span>
            </li>
          </ul>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .search-container { max-width: 800px; margin: 0 auto; padding: 20px; }
    .search-form { display: flex; gap: 10px; margin-bottom: 20px; }
    .search-form input, .search-form select { padding: 8px; flex: 1; }
    .results { margin-top: 20px; }
    .result-item { margin-bottom: 20px; }
    .no-results { text-align: center; color: gray; }
  `]
})
export class SearchComponent {
  keyword = '';
  filterType = '';
  status = '';
  results: SearchResult[] = [];
  hasSearched = false;

  constructor(private http: HttpClient) {}

  search() {
    this.hasSearched = true;
    this.results = [];

    const params = {
      keyword: this.keyword || undefined,
      status: this.status || undefined
    };

    if (this.filterType === '' || this.filterType === 'groups') {
      this.http.get<any[]>('/api/groups/search', { params }).subscribe(data => {
        if (data.length > 0) {
          this.results.push({ type: 'Groups', data });
        }
      });
    }

    if (this.filterType === '' || this.filterType === 'topics') {
      this.http.get<any[]>('/api/topics/search', { params }).subscribe(data => {
        if (data.length > 0) {
          this.results.push({ type: 'Topics', data });
        }
      });
    }

    if (this.filterType === '' || this.filterType === 'users') {
      this.http.get<any[]>('/api/users/search', { params }).subscribe(data => {
        if (data.length > 0) {
          this.results.push({ type: 'Users', data });
        }
      });
    }
  }
}