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
import { GradeDistributionItem } from '../models/grade-distribution-item.model';
import { GradeBarChartComponent } from '../grade-bar-chart/grade-bar-chart.component';
import { FinalGrade, GradeService } from '../services/grade.service';

@Component({
  selector: 'app-mentor-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, GradeBarChartComponent],
  templateUrl: './mentor-dashboard.component.html',
  styleUrls: ['./mentor-dashboard.component.css']
})
export class MentorDashboardComponent implements OnInit, OnDestroy {
  // KHAI BÁO BIẾN
  allTopics: any[] = [];
  selectedTopic: any = null;
  reviewComment: string = '';
  sidebarCollapsed: boolean = false;
  selectedTab: string = 'All';
  pendingTopics: any[] = [];
  approvedTopics: any[] = [];
  rejectedTopics: any[] = [];

  get topicBoardData() {
    return {
      Pending: this.pendingTopics,
      Approved: this.approvedTopics,
      Rejected: this.rejectedTopics
    };
  }

  successMessage: string = '';
  viewMode: 'topics' | 'reports' | 'grades' = 'topics';
  mentorName: string = 'Mentor';

  // Weekly Reports
  weeklyReports: any[] = [];
  gradeDistribution: GradeDistributionItem[] = [
    { grade: 'A', count: 0 },
    { grade: 'B', count: 0 },
    { grade: 'C', count: 0 },
    { grade: 'D', count: 0 },
    { grade: 'F', count: 0 }
  ];
  isLoadingGradeDistribution = false;
  gradeDistributionError = '';
  isLoadingReports = false;
  selectedReport: any = null;
  evaluationScore: number = 0;
  evaluationComment: string = '';
  isEvaluating = false;
  isReadOnly = false;
  private wsSubscription?: Subscription;
  private isBrowser: boolean;
  errorMessage: string = '';

  constructor(
    private topicService: TopicService,
    private authService: AuthService,
    private wsService: WebsocketService,
    private weeklyReportService: WeeklyReportService,
    private evaluationService: WeeklyEvaluationService,
    private gradeService: GradeService,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit() {
    this.loadTopics();
    this.setupWebsocket();

    const user = this.authService.getCurrentUser();
    if (user && user.fullName) {
      this.mentorName = user.fullName;
    }

    this.loadMentorInbox();
    this.loadGradeDistribution();
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
          this.allTopics = data.All || data.all || [];
          this.pendingTopics = data.Pending || data.pending || [];
          this.approvedTopics = data.Approved || data.approved || [];
          this.rejectedTopics = data.Rejected || data.rejected || [];
        } else if (Array.isArray(res)) {
          this.allTopics = res;
          this.pendingTopics = res.filter((t: any) => {
            const s = (t.Status || t.status || '').toLowerCase();
            return s === 'pending' || s === 'submitted';
          });
          this.approvedTopics = res.filter((t: any) => {
            const s = (t.Status || t.status || '').toLowerCase();
            return s === 'approved' || s === 'active';
          });
          this.rejectedTopics = res.filter((t: any) => (t.Status || t.status || '').toLowerCase() === 'rejected');
        }
      },
      error: (err) => console.error('Lỗi khi gọi API Mentor Board:', err)
    });
  }

  selectTab(tab: string) {
    this.selectedTab = tab;
  }

  get displayedTopics(): any[] {
    if (this.selectedTab === 'Pending') return this.pendingTopics;
    if (this.selectedTab === 'Approved') return this.approvedTopics;
    if (this.selectedTab === 'Rejected') return this.rejectedTopics;
    return this.allTopics;
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  ensureExternalLink(url: string): string {
    if (!url) return '#';
    return url.startsWith('http') ? url : `https://${url}`;
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
        this.loadTopics();
      },
      error: (err) => alert('Error: ' + err.error?.message)
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
      (status === 'Approved' && ((t.Status || t.status) === 'Active'))
    ).length;
  }

  getStatusClass(status: string): string {
    const s = (status || '').toLowerCase();
    if (s === 'approved' || s === 'active') return 'approved';
    if (s === 'rejected') return 'rejected';
    return 'pending';
  }

  getReviewedPct(): number {
    if (!this.allTopics.length) return 0;
    const reviewed = this.allTopics.filter(t => {
      const s = (t.Status || t.status || '').toLowerCase();
      return s !== 'pending' && s !== 'submitted';
    }).length;
    return Math.round((reviewed / this.allTopics.length) * 100);
  }

  getPct(status: string): number {
    if (!this.allTopics.length) return 0;
    return Math.round((this.countByStatus(status) / this.allTopics.length) * 100);
  }

  switchView(mode: 'topics' | 'reports' | 'grades') {
    this.viewMode = mode;
    if (mode === 'reports') {
      this.loadMentorInbox();
    } else if (mode === 'grades') {
      this.loadGradeDistribution();
    }
  }

  loadMentorInbox() {
    this.isLoadingReports = true;
    this.weeklyReportService.getMentorInbox().subscribe({
      next: (res) => {
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
    this.isReadOnly = this.isEvaluationCompletedStatus(status);

    if (this.isReadOnly) {
      this.evaluationService.getEvaluation(report.reportId).subscribe({
        next: (res: any) => {
          this.evaluationScore = res.score ?? res.Score ?? 0;
          this.evaluationComment = res.comment ?? res.Comment ?? '';
        },
        error: () => {
          this.evaluationScore = report.score ?? report.Score ?? 0;
          this.evaluationComment = report.comment ?? report.Comment ?? '';
        }
      });
    } else {
      this.evaluationScore = report.score ?? report.Score ?? 0;
      this.evaluationComment = report.comment ?? report.Comment ?? '';
    }
  }

  private isEvaluationCompletedStatus(status: string): boolean {
    return status === 'reviewed' || status === 'pass' || status === 'fail';
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
        this.loadGradeDistribution();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (err) => {
        this.isEvaluating = false;
        alert('Evaluation failed: ' + (err.error?.message || 'Unknown error'));
      }
    });
  }

  loadGradeDistribution() {
    this.isLoadingGradeDistribution = true;
    this.gradeDistributionError = '';

    this.gradeService.getAllGrades().subscribe({
      next: (grades) => {
        this.updateGradeDistribution(grades || []);
        this.isLoadingGradeDistribution = false;
      },
      error: () => {
        this.isLoadingGradeDistribution = false;
        this.gradeDistributionError = 'Unable to load final grade distribution for this account.';
        this.gradeDistribution = [
          { grade: 'A', count: 0 },
          { grade: 'B', count: 0 },
          { grade: 'C', count: 0 },
          { grade: 'D', count: 0 },
          { grade: 'F', count: 0 }
        ];
      }
    });
  }

  private updateGradeDistribution(grades: FinalGrade[]) {
    const distribution: Record<GradeDistributionItem['grade'], number> = {
      A: 0,
      B: 0,
      C: 0,
      D: 0,
      F: 0
    };

    for (const grade of grades || []) {
      const bucket = this.resolveGradeBucket(grade);
      if (bucket) {
        distribution[bucket] += 1;
      }
    }

    this.gradeDistribution = (['A', 'B', 'C', 'D', 'F'] as const).map((grade) => ({
      grade,
      count: distribution[grade]
    }));
  }

  private resolveGradeBucket(grade: FinalGrade): GradeDistributionItem['grade'] | null {
    const normalizedLetter = `${grade?.gradeLetter || ''}`.trim().toUpperCase();

    if (normalizedLetter === 'A' || normalizedLetter === 'B' || normalizedLetter === 'C' || normalizedLetter === 'D' || normalizedLetter === 'F') {
      return normalizedLetter;
    }

    const score = Number(grade?.averageScore);
    if (!Number.isFinite(score)) return null;

    if (score >= 8.5) return 'A';
    if (score >= 7) return 'B';
    if (score >= 5.5) return 'C';
    if (score >= 4) return 'D';
    return 'F';
  }

  get totalDistributedGrades(): number {
    return this.gradeDistribution.reduce((sum, item) => sum + item.count, 0);
  }

  downloadReportFile(report: any) {
    const fileUrl = report.fileUrl || report.FileUrl;
    
    if (!fileUrl) {
      this.errorMessage = "File URL not found!";
      return;
    }

    // Extract filename from URL if it contains one, otherwise use a default
    let fileName = 'report_file';
    if (fileUrl && typeof fileUrl === 'string') {
      const urlParts = fileUrl.split('/');
      const lastPart = urlParts[urlParts.length - 1];
      if (lastPart && !lastPart.includes('?')) {
        fileName = lastPart;
      }
    }

    this.weeklyReportService.downloadFile(fileUrl).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        window.URL.revokeObjectURL(url);
        this.successMessage = 'File downloaded successfully!';
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (err) => {
        console.error('Download error:', err);
        this.errorMessage = "Could not download file. It might have been deleted.";
        setTimeout(() => this.errorMessage = '', 5000);
      }
    });
  }
}
