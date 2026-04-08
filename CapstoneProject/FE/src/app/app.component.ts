import { Component } from '@angular/core';
import { SearchComponent } from './search/search.component';
import { SearchCouncilComponent } from './search-council/search-council.component';
import { SearchMentorComponent } from './search-mentor/search-mentor.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, SearchComponent, SearchCouncilComponent, SearchMentorComponent],
  template: `
    <div class="app-container">
      <nav>
        <button (click)="activeTab = 'admin'" [class.active]="activeTab === 'admin'">Admin Search</button>
        <button (click)="activeTab = 'council'" [class.active]="activeTab === 'council'">Council Search</button>
        <button (click)="activeTab = 'mentor'" [class.active]="activeTab === 'mentor'">Mentor Search</button>
      </nav>
      <div class="content">
        <app-search *ngIf="activeTab === 'admin'"></app-search>
        <app-search-council *ngIf="activeTab === 'council'"></app-search-council>
        <app-search-mentor *ngIf="activeTab === 'mentor'"></app-search-mentor>
      </div>
    </div>
  `,
  styles: [`
    .app-container { font-family: Arial, sans-serif; }
    nav { display: flex; gap: 10px; margin-bottom: 20px; }
    button { padding: 10px 20px; cursor: pointer; }
    button.active { background-color: #007bff; color: white; }
    .content { padding: 20px; }
  `]
})
export class AppComponent {
  activeTab = 'admin';
}