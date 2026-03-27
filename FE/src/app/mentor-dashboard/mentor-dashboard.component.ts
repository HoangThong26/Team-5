import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TopicService } from '../services/topic.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { WebsocketService } from '../services/websocket.service';
import { Subscription } from 'rxjs';
import { WeeklyReportService } from '../services/weekly-report.service';
import { WeeklyEvaluationService } from '../services/weekly-evaluation.service';

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
  
  // Tab filtering
  selectedTab: string = 'All';
  topicBoardData: {
    All: any[];
    Pending: any[];
    Approved: any[];
    Rejected: any[];
    [key: string]: any[];
  } = {
    All: [],
    Pending: [],
    Approved: [],
    Rejected: []
  };
  
  successMessage: string = '';
  viewMode: 'topics' | 'reports' = 'topics';
  mentorName: string = 'Mentor';
  sidebarCollapsed: boolean = false;

  // Weekly Reports
  weeklyReports: any[] = [];
  isLoadingReports = false;
  selectedReport: any = null;
  evaluationScore: number = 0;
  evaluationComment: string = '';
  isEvaluating = false;
  isReadOnly = false;
  private wsSubscription?: Subscription;
  private isBrowser: boolean;

  constructor(
    private topicService: TopicService,
    private authService: AuthService,
    private wsService: WebsocketService,
    private weeklyReportService: WeeklyReportService,
    private evaluationService: WeeklyEvaluationService,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: Object
  ) { 
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit() {
    this.loadTopics();
    this.setupWebsocket();
    
    // Fetch user profile info
    const user = this.authService.getCurrentUser();
    if (user && user.fullName) {
      this.mentorName = user.fullName;
    }

    // Load reports initially for badge counts
    this.loadMentorInbox();
  }

  ngOnDestroy() {
    if (this.wsSubscription) {
      this.wsSubscription.unsubscribe();
    }
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
            } else if (type === 'WEEKLY_REPORT_SUBMITTED') {
              this.loadMentorInbox();
              this.successMessage = message.message || 'A new weekly report has been submitted!';
              setTimeout(() => this.successMessage = '', 5000);
            }
          },
          error: (err) => console.error('WebSocket Error:', err)
        });
      }
    }
  }

  loadTopics() {
    this.topicService.getMentorBoardTopicVersions().subscribe({
      next: (res: any) => {
        const success = res.success || res.Success;
        const data = res.data || res.Data;

        if (success && data) {
          this.topicBoardData = {
            All: data.All || data.all || [],
            Pending: data.Pending || data.pending || [],
            Approved: data.Approved || data.approved || [],
            Rejected: data.Rejected || data.rejected || []
          };
          this.allTopics = this.topicBoardData.All;
        } else if (Array.isArray(res)) {
          this.allTopics = res;
          this.topicBoardData = {
            All: res,
            Pending: res.filter((t: any) => {
              const s = (t.Status || t.status || '').toLowerCase();
              return s === 'pending' || s === 'submitted';
            }),
            Approved: res.filter((t: any) => {
              const s = (t.Status || t.status || '').toLowerCase();
              return s === 'approved' || s === 'active';
            }),
            Rejected: res.filter((t: any) => (t.Status || t.status || '').toLowerCase() === 'rejected')
          };
        }
      },
      error: (err) => console.error('Lỗi API:', err)
    });
  }

  selectTab(tab: string) {
    this.selectedTab = tab;
  }

  get displayedTopics(): any[] {
    return this.topicBoardData[this.selectedTab] || [];
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
        alert('Successful!');
        this.loadTopics(); // Refresh everything from backend
        
        const newStatus = (status === 'Approved') ? 'Approved' : 'Rejected';
        if (this.selectedTopic) {
          this.selectedTopic.Status = newStatus;
          this.selectedTopic.status = newStatus;
        }
 
        // Optional: Close modal after decision
        // this.selectedTopic = null;
      },
      error: (err) => alert('Error: ' + (err.error?.message || 'Action failed'))
    });
  }

  downloadReportFile(fileUrl: string | undefined): void {
    if (!fileUrl) return;
    const fileName = fileUrl.split('/').pop();
    if (!fileName) return;

    this.weeklyReportService.downloadFile(fileName).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName; // Quan trọng: bật tính năng tải file
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
      },
      error: (err) => {
        alert('Failed to download file.');
      }
    });
  }

  logout() {
    if (confirm('Do you want to logout?')) {
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

  /** Helper to ensure absolute URL for external links */
  ensureExternalLink(url: string | undefined | null): string {
    if (!url) return '';
    const trimmedUrl = url.trim();
    if (trimmedUrl.startsWith('http://') || trimmedUrl.startsWith('https://')) {
      return trimmedUrl;
    }
    return 'https://' + trimmedUrl;
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

  // ===== Weekly Report Methods =====
  switchView(mode: 'topics' | 'reports') {
    this.viewMode = mode;
    if (mode === 'reports') {
      this.loadMentorInbox();
    }
  }

  loadMentorInbox() {
    this.isLoadingReports = true;
    console.log('Fetching mentor inbox...');
    this.weeklyReportService.getMentorInbox().subscribe({
      next: (res) => {
        console.log('Mentor inbox received:', res);
        this.weeklyReports = res;
        this.isLoadingReports = false;
      },
      error: (err) => {
        this.isLoadingReports = false;
        console.error('Lỗi load inbox:', err);
      }
    });
  }

  selectReport(report: any) {
    this.selectedReport = { ...report };
    this.evaluationScore = 0;
    this.evaluationComment = '';

    const status = (report.status || report.Status || '').toLowerCase();
    this.isReadOnly = (status === 'reviewed');

    if (this.isReadOnly) {
      // Fetch saved evaluation from API so the score/comment are shown correctly
      this.evaluationService.getEvaluation(report.reportId).subscribe({
        next: (res: any) => {
          this.evaluationScore = res.score ?? res.Score ?? 0;
          this.evaluationComment = res.comment ?? res.Comment ?? '';
        },
        error: () => {
          // Fallback to whatever is on the report object
          this.evaluationScore = report.score ?? report.Score ?? 0;
          this.evaluationComment = report.comment ?? report.Comment ?? '';
        }
      });
    } else {
      // Not yet reviewed — pre-fill if already set
      this.evaluationScore = report.score ?? report.Score ?? 0;
      this.evaluationComment = report.comment ?? report.Comment ?? '';
    }
  }

  submitEvaluation() {
    if (!this.selectedReport || this.isEvaluating) return;

    this.isEvaluating = true;
    const request = {
      reportId: this.selectedReport.reportId,
      score: this.evaluationScore,
      comment: this.evaluationComment
    };

    this.evaluationService.submitEvaluation(request).subscribe({
      next: (res) => {
        this.isEvaluating = false;
        this.successMessage = res.message || 'Evaluation submitted successfully!';
        this.selectedReport = null;
        this.loadMentorInbox();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (err) => {
        this.isEvaluating = false;
        alert('Evaluation failed: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }
}

