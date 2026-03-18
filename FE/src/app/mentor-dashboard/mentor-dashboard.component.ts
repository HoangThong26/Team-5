import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; // Thêm dòng này để dùng *ngIf, *ngFor
import { FormsModule } from '@angular/forms'; // THÊM DÒNG NÀY ĐỂ HẾT LỖI ngModel
import { TopicService } from '../services/topic.service';
import {TopicApprovalRequest} from '../models/topic.model'
import { AuthService } from '../services/auth.service'; // <--- THÊM DÒNG NÀY
import { Router } from '@angular/router';

@Component({
  selector: 'app-mentor-dashboard',
  standalone: true, 
  imports: [CommonModule, FormsModule], 
  templateUrl: './mentor-dashboard.component.html',
  styleUrls: ['./mentor-dashboard.component.css']
})
export class MentorDashboardComponent implements OnInit {
  pendingTopics: any[] = [];
  selectedTopic: any = null;
  reviewComment: string = '';

 constructor(
    private topicService: TopicService,
    private authService: AuthService, // <--- THÊM DÒNG NÀY
    private router: Router            // <--- THÊM DÒNG NÀY
  ) {}

  ngOnInit() {
    this.loadPendingTopics();
  }
  logout() {
    if (confirm('Bạn có chắc muốn đăng xuất?')) {
      this.authService.logout().subscribe({
        next: () => {
          this.router.navigate(['/login']); // Về trang login sau khi xóa token
        },
        error: (err) => {
          console.error('Logout error:', err);
          // Kể cả lỗi API thì vẫn nên ép xóa token ở client và về login
          this.authService.clearToken();
          this.router.navigate(['/login']);
        }
      });
    }
  }

loadPendingTopics() {
  this.topicService.getPendingTopics().subscribe({
    next: (res: any) => {
      console.log('Dữ liệu Mentor nhận được:', res); 
      this.pendingTopics = res;
    },
    error: (err) => {
      console.error('Lỗi gọi API:', err);
    }
  });
}

// Trong mentor-dashboard.component.ts

selectTopic(topic: any) {
  // Console log để bạn tự kiểm tra: nếu thấy Title (viết hoa) thì phải dùng viết hoa
  console.log('Selected Topic Data:', topic); 
  this.selectedTopic = topic;
}

handleApproval(status: string) {
  if (!this.selectedTopic) return;

  const request = {
    // Sửa ở đây: Sử dụng tên thuộc tính viết hoa khớp với API trả về
    versionId: this.selectedTopic.VersionId || this.selectedTopic.versionId,
    topicId: this.selectedTopic.TopicId || this.selectedTopic.topicId,
    status: status,
    reviewComment: this.reviewComment
  };

  console.log('Request sending to API:', request); // Kiểm tra xem versionId có > 0 không

  this.topicService.approveTopic(request).subscribe({
    next: (res) => {
      alert('Topic status updated successfully!');
      this.selectedTopic = null;
      this.loadPendingTopics(); // Load lại bảng để mất dòng vừa duyệt
    },
    error: (err) => {
      // Hiện thông báo lỗi từ Backend (ví dụ: "Topic version not found")
      alert(err.error?.message || 'Error occurred');
    }
  });
}
}