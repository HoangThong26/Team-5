import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TopicService } from '../services/topic.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-mentor-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './mentor-dashboard.component.html',
  styleUrls: ['./mentor-dashboard.component.css']
})
export class MentorDashboardComponent implements OnInit {
  // KHAI BÁO LẠI TÊN BIẾN ĐỂ KHỚP VỚI LOGIC (Fix lỗi TS2339)
  allTopics: any[] = [];
  selectedTopic: any = null;
  reviewComment: string = '';

  constructor(
    private topicService: TopicService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit() {
    this.loadTopics();
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
}