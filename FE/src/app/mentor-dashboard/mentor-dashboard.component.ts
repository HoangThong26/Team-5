import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TopicService } from '../services/topic.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { WebsocketService } from '../services/websocket.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-mentor-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './mentor-dashboard.component.html',
  styleUrls: ['./mentor-dashboard.component.css']
})
export class MentorDashboardComponent implements OnInit, OnDestroy {
  // KHAI BÁO LẠI TÊN BIẾN ĐỂ KHỚP VỚI LOGIC (Fix lỗi TS2339)
  allTopics: any[] = [];
  selectedTopic: any = null;
  reviewComment: string = '';
  
  successMessage: string = '';
  private wsSubscription?: Subscription;
  private isBrowser: boolean;

  constructor(
    private topicService: TopicService,
    private authService: AuthService,
    private wsService: WebsocketService,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: Object
  ) { 
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit() {
    this.loadTopics();
    this.setupWebsocket();
  }

  ngOnDestroy() {
    if (this.wsSubscription) {
      this.wsSubscription.unsubscribe();
    }
    // Không disconnect ở đây vì có thể user vẫn đang dùng app nhưng ở page khác 
    // (tuy nhiên Mentor Dashboard thường là component chính cho mentor)
  }

  setupWebsocket() {
    if (this.isBrowser) {
      const currentUser = this.authService.getCurrentUser();
      const userId = currentUser?.email;

      if (userId) {
        this.wsService.connect();

        this.wsSubscription = this.wsService.getMessages().subscribe({
          next: (message) => {
            console.log('Mentor Dashboard received message:', message);
            const type = (message.type || message.Type || '').toUpperCase();
            
            if (type === 'TOPIC_SUBMITTED') {
              this.loadTopics();
              this.successMessage = message.message || 'A new topic has been submitted!';
              setTimeout(() => this.successMessage = '', 5000);
            }
          },
          error: (err) => console.error('WebSocket Error:', err)
        });
      }
    }
  }

  loadTopics() {
    this.topicService.getPendingTopics().subscribe({
      next: (res: any) => {
        this.allTopics = res;
      },
      error: (err) => console.error('Lỗi API:', err)
    });
  }

  selectTopic(topic: any) {
    this.selectedTopic = { ...topic };
    this.reviewComment = topic.ReviewComment || topic.reviewComment || '';
  }

  handleApproval(status: string) {
    if (!this.selectedTopic) return;

    const request = {
      versionId: this.selectedTopic.VersionId || this.selectedTopic.versionId,
      topicId: this.selectedTopic.TopicId || this.selectedTopic.topicId,
      status: status,
      reviewComment: this.reviewComment
    };

    this.topicService.approveTopic(request).subscribe({
      next: (res) => {
        alert('Successfull!');

        const newStatus = (status === 'Approved') ? 'Active' : 'Rejected';
        this.allTopics = this.allTopics.map((t: any) => {
          if ((t.TopicId || t.topicId) === request.topicId) {
            return {
              ...t,
              Status: newStatus,
              status: newStatus,
              ReviewComment: this.reviewComment
            };
          }
          return t;
        });
        if (this.selectedTopic) {
          this.selectedTopic.Status = newStatus;
          this.selectedTopic.status = newStatus;
        }

        // Nếu bạn muốn bấm xong đóng luôn modal thì thêm dòng này:
        // this.selectedTopic = null;
      },
      error: (err) => alert('Error: ' + err.error?.message)
    });
  }

  logout() {
    if (confirm('Do you want to logou?')) {
      this.authService.logout().subscribe({
        next: () => this.router.navigate(['/login']),
        error: () => {
          this.authService.clearToken();
          this.router.navigate(['/login']);
        }
      });
    }
  }

  countByStatus(status: string): number {
    return this.allTopics.filter(t =>
      (t.Status || t.status) === status ||
      // 'Active' is the API's term for Approved
      (status === 'Approved' && ((t.Status || t.status) === 'Active'))
    ).length;
  }

  getStatusClass(status: string): string {
    const s = (status || '').toLowerCase();
    if (s === 'approved' || s === 'active') return 'approved';
    if (s === 'rejected') return 'rejected';
    return 'pending';
  }

  /** Percentage of topics that have been reviewed (not Pending) */
  getReviewedPct(): number {
    if (!this.allTopics.length) return 0;
    const reviewed = this.allTopics.filter(t => {
      const s = (t.Status || t.status || '').toLowerCase();
      return s !== 'pending' && s !== 'submitted';
    }).length;
    return Math.round((reviewed / this.allTopics.length) * 100);
  }

  /** Percentage of a given status out of total */
  getPct(status: string): number {
    if (!this.allTopics.length) return 0;
    return Math.round((this.countByStatus(status) / this.allTopics.length) * 100);
  }
}

