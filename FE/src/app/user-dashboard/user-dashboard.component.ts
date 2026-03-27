
import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { AuthService } from '../services/auth.service';
import { UserService } from '../services/user.service';
import { UserProfile } from '../models/user.model';
import { GroupService, GroupDetailResponse } from '../services/group.service';
import { WebsocketService } from '../services/websocket.service';
import { TopicService } from '../services/topic.service';
import { WeeklyReportService } from '../services/weekly-report.service';
import {WeeklyReportHistoryDto} from '../models/weekly-report.model'
import { WeeklyEvaluationService } from '../services/weekly-evaluation.service';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [FormsModule, CommonModule],
  providers: [DatePipe],
  templateUrl: './user-dashboard.component.html',
  styleUrls: ['./user-dashboard.component.css']
})
export class UserDashboardComponent implements OnInit, OnDestroy {

  activeModal: 'none' | 'details' | 'create' | 'invite' | 'members' | 'topic' | 'report' = 'none';
  weeklyReports: any[] = [];
  isReportAdding: boolean = false;
  isHistoryLoading: boolean = false;
  showGroupDetails: boolean = false;
  private isBrowser: boolean;
  myGroup: GroupDetailResponse | null = null;
  newGroupName = '';
  inviteEmail = '';
  isGroupLoading = false;
  submissionHistory: any[] = [];
  selectedReport: any = null;

  // Sidebar
  activeTab: 'overview' | 'profile' | 'password' = 'overview';
  sidebarCollapsed = false;

  // Profile
  profile: UserProfile | null = null;
  editMode = false;
  isLoading = false;

  // Messages
  successMessage = '';
  errorMessage = '';

  // Edit fields
  fullName = '';
  phone = '';
  avatarUrl = '';
  avatarPreview = '';
  selectedAvatarFile: File | null = null;

  // Password change fields
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  otp = '';
  isChangingPassword = false;
  isSendingOtp = false;
  otpSent = false;
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  // Topic fields
  topicTitle = '';
  topicDescription = '';
  isTopicEditing = false;
  isTopicLoading = false;

  // Weekly Report fields
  reportWeek: number = 1;
  reportContent: string = '';
  reportGithubLink: string = '';
  reportFileUrl: string = '';
  isReportLoading: boolean = false;
  isReportEditing: boolean = false;
  editingReportId: number | null = null;
  selectedReportFile: File | null = null;
  isFileReading: boolean = false;


  // ==========================================
  // BIẾN QUẢN LÝ WEBSOCKET
  // ==========================================
  private wsSubscription?: Subscription;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private groupService: GroupService,
    private wsService: WebsocketService,
    private topicService: TopicService,
    private weeklyReportService: WeeklyReportService,
    private weeklyEvaluationService: WeeklyEvaluationService,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }
  loadMyGroup() {
    this.isGroupLoading = true;
    this.groupService.getMyGroup().subscribe({
      next: (res) => {
        this.myGroup = res;
        if (res.topic) {
          this.topicTitle = res.topic.title;
          this.topicDescription = res.topic.description;
        }
        this.loadWeeklyReports();
        this.isGroupLoading = false;
      },
      error: (err) => {
        this.isGroupLoading = false;
        this.myGroup = null;
      }
    });
  }

  loadMyGroupSilently() {
    this.groupService.getMyGroup().subscribe({
      next: (res) => {
        this.myGroup = res;

        if (this.myGroup?.groupId) {
          this.topicService.getTopicByGroupId(this.myGroup.groupId).subscribe({
            next: (topicRes) => {
              if (this.myGroup) {
                // Gán toàn bộ object topic mới từ API vào myGroup
                // Đảm bảo topicRes này chứa trường reviewComment
                this.myGroup.topic = topicRes;

                // Log ra để kiểm tra chắc chắn giá trị nhận được
                console.log('--- Topic Info Loaded ---');
                console.log('Status:', topicRes.status);
                console.log('Mentor Feedback:', topicRes.reviewComment);

                // Nếu trạng thái là Rejected, chuẩn bị sẵn dữ liệu trong form
                if (topicRes.status === 'Rejected') {
                  // Chỉ tự động điền nếu SV chưa chủ động nhấn vào nút Edit/Resubmit
                  if (!this.isTopicEditing) {
                    this.topicTitle = topicRes.title;
                    this.topicDescription = topicRes.description;
                  }
                }
              }
            },
            error: (err) => {
              console.warn('Nhóm chưa có topic hoặc lỗi khi lấy topic:', err);
              if (this.myGroup) this.myGroup.topic = undefined;
            }
          });
        }
      },
      error: (err) => console.error('Silently load group failed', err)
    });
  }


  // ===== Topic Actions =====

  // Hàm này dùng để mở form edit và gán giá trị hiện tại của topic vào input
  enableTopicEdit() {
    if (this.myGroup?.topic) {
      this.topicTitle = this.myGroup.topic.title;
      this.topicDescription = this.myGroup.topic.description;
      this.isTopicEditing = true;
    }
  }

  cancelTopicEdit() {
    this.isTopicEditing = false;
    this.topicTitle = '';
    this.topicDescription = '';
  }


  getStatusTextColor(status?: string): string {
    if (!status) return '#475569';
    switch (status) {
      case 'Approved':
      case 'Reviewed': return '#166534';
      case 'Pending':
      case 'Submitted': return '#1e40af';
      case 'Rejected': return '#991b1b';
      case 'Draft': return '#475569';
      default: return '#475569';
    }
  }

  private alertTimer: any;
  handleTopicSubmit() {
    if (this.alertTimer) clearTimeout(this.alertTimer);

    // 1. Validate dữ liệu đầu vào
    if (!this.topicTitle.trim() || !this.topicDescription.trim() || !this.myGroup) {
      this.errorMessage = 'Please fill in both title and description.';
      this.alertTimer = setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    this.isTopicLoading = true;
    const groupId = this.myGroup.groupId;
    const currentStatus = this.myGroup.topic?.status;
    if (!this.myGroup.topic || currentStatus === 'Rejected') {
      const request = {
        groupId: groupId,
        title: this.topicTitle,
        description: this.topicDescription
      };

      this.topicService.submitTopic(request).subscribe({
        next: (res) => {
          this.handleTopicSuccess(res.message || 'Topic submitted successfully!');
        },
        error: (err) => this.handleTopicError(err)
      });
    }
    else if (this.isTopicEditing && currentStatus === 'Pending') {
      this.topicService.editTopic(this.myGroup.topic.topicId, {
        title: this.topicTitle,
        description: this.topicDescription
      }).subscribe({
        next: (res) => {
          this.handleTopicSuccess(res.message || 'Topic updated successfully!');
        },
        error: (err) => this.handleTopicError(err)
      });
    }
  }

  private handleTopicSuccess(message: string) {
    this.isTopicLoading = false;
    this.isTopicEditing = false;
    this.successMessage = message;
    this.loadMyGroupSilently(); // Load lại để cập nhật trạng thái Topic mới
    setTimeout(() => {
      this.successMessage = ''; // Xóa thông báo
    }, 3000);
  }

  private handleTopicError(err: any) {
    this.isTopicLoading = false;
    this.errorMessage = this.extractError(err, 'Failed to process topic.');
    setTimeout(() => {
      this.errorMessage = ''; // Xóa thông báo (Sửa lỗi: gán nhầm successMessage ở file gốc)
    }, 3000);
  }

  // ===== Weekly Report Actions =====
  handleReportSubmit() {
    if (!this.myGroup) return;

    const content = this.reportContent.trim();
    const githubLink = this.reportGithubLink.trim();
    
    // Validation for all fields
    if (!content) {
      alert('Please enter report content.');
      return;
    }
    if (!githubLink) {
      alert('Please enter GitHub repository or PR link.');
      return;
    }
    if (!this.selectedReportFile && !this.isReportEditing) {
      alert('Please attach a supporting document (doc/docx).');
      return;
    }

    this.isReportLoading = true;
    
    const formData = new FormData();
    formData.append('GroupId', this.myGroup.groupId.toString());
    formData.append('Content', this.reportContent);
    formData.append('WeekId', this.reportWeek.toString());
    
    if (this.reportGithubLink) {
      formData.append('GithubLink', this.reportGithubLink);
    }
    
    if (this.selectedReportFile) {
      formData.append('ReportFile', this.selectedReportFile, this.selectedReportFile.name);
    }

    if (this.isReportEditing && this.editingReportId) {
      this.weeklyReportService.updateReport(this.editingReportId, formData).subscribe({
        next: (res: any) => {
          const isSuccess = res.success !== undefined ? res.success : res.Success;
          const msg = res.message || res.Message || 'Weekly report updated successfully!';
          
          if (isSuccess === false) {
            this.isReportLoading = false;
            alert(msg);
          } else {
            this.handleReportSuccess(msg);
          }
        },
        error: (err) => {
          this.isReportLoading = false;
          alert(this.extractError(err, 'Update failed!'));
        }
      });
    } else {
      this.weeklyReportService.submitReport(formData).subscribe({
        next: (res: any) => {
          const isSuccess = res.success !== undefined ? res.success : res.Success;
          const msg = res.message || res.Message || 'Weekly report submitted successfully!';
          
          if (isSuccess === false) {
            this.isReportLoading = false;
            alert(msg);
          } else {
            this.handleReportSuccess(msg);
          }
        },
        error: (err) => {
          this.isReportLoading = false;
          alert(this.extractError(err, 'Submit failed!'));
        }
      });
    }
  }

  private handleReportSuccess(message: string) {
    this.isReportLoading = false;
    this.isReportAdding = false;
    this.isReportEditing = false;
    this.editingReportId = null;
    this.successMessage = message;
    this.loadHistory();
    this.loadWeeklyReports();
    this.reportContent = '';
    this.reportGithubLink = '';
    this.reportFileUrl = '';
    this.selectedReportFile = null;
    setTimeout(() => this.successMessage = '', 3000);
  }

  onReportFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      
      // Validate file extension
      const allowedExtensions = ['.doc', '.docx'];
      const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
      
      if (!allowedExtensions.includes(fileExtension)) {
        alert('Only Word documents (.doc or .docx) are allowed.');
        input.value = ''; // Reset input
        return;
      }

      if (file.size > 5 * 1024 * 1024) { // 5MB limit
        alert('File size must not exceed 5MB.');
        input.value = ''; // Reset input
        return;
      }
      this.selectedReportFile = file;
    }
  }

  enableReportEdit() {
    if (!this.selectedReport) return;
    this.isReportAdding = true;
    this.isReportEditing = true;
    this.editingReportId = this.selectedReport.reportId;
    this.reportWeek = this.selectedReport.weekId;
    this.reportContent = this.selectedReport.content;
    this.reportGithubLink = this.selectedReport.githubLink;
    this.reportFileUrl = this.selectedReport.fileUrl;
  }
  loadHistory() {
    const groupId = this.myGroup?.groupId;
    if (!groupId) return;

    this.isHistoryLoading = true;
    this.weeklyReportService.getHistoryByGroupId(groupId).subscribe({
      next: (data: WeeklyReportHistoryDto[]) => {
        this.submissionHistory = data.sort((a, b) => (b.weekId || 0) - (a.weekId || 0));
        if (this.submissionHistory.length > 0) {
          this.selectedReport = this.submissionHistory[0];
        }
        this.isHistoryLoading = false;
      },
      error: (err) => {
        console.error('Lỗi load lịch sử:', err);
        this.isHistoryLoading = false;
      }
    });
  }
selectHistoryItem(report: any) {
  this.selectedReport = report;
  this.isReportAdding = false; // Tắt form nộp bài nếu đang mở để xem chi tiết
  
  // Focus or scroll to detail view if needed
  const status = (report.status || report.Status || '').toLowerCase();
  if (status === 'reviewed') {
    const reportId = report.reportId || report.ReportId;
    if (reportId) {
      this.weeklyEvaluationService.getEvaluation(reportId).subscribe({
        next: (evRes: any) => {
          this.selectedReport = {
            ...this.selectedReport,
            score: evRes.score || evRes.Score,
            isPass: evRes.isPass !== undefined ? evRes.isPass : evRes.IsPass,
            mentorComment: evRes.comment || evRes.Comment,
            mentorName: evRes.mentorName || evRes.MentorName,
            reviewedAt: evRes.reviewedAt || evRes.ReviewedAt
          };
        },
        error: (err) => console.error('Failed to load evaluation details', err)
      });
    }
  }
}
  getStatusColor(status: string): string {
    const s = (status || '').toLowerCase();
    if (s === 'reviewed') return '#10b981'; // Xanh lá
    if (s === 'submitted') return '#3b82f6'; // Xanh dương
    return '#94a3b8'; // Xám
  }

  ensureExternalLink(url: string | undefined | null): string {
    if (!url) return '';
    const trimmedUrl = url.trim();
    if (trimmedUrl.startsWith('http://') || trimmedUrl.startsWith('https://')) {
      return trimmedUrl;
    }
    return 'https://' + trimmedUrl;
  }

  loadWeeklyReports() {
    if (!this.myGroup) return;

    this.weeklyReportService.getReportsByGroupId(this.myGroup.groupId).subscribe({
      next: (res: any) => {
        // Đảm bảo res là mảng, sắp xếp tuần mới nhất lên đầu
        const data = Array.isArray(res) ? res : (res.data || []);
        this.weeklyReports = data.sort((a: any, b: any) => (b.weekId || 0) - (a.weekId || 0));

        // Lấy số tuần của bài mới nhất để hiển thị
        if (this.weeklyReports.length > 0) {
          this.reportWeek = this.weeklyReports[0].weekId;
        }
      },
      error: (err) => console.error('Lỗi load báo cáo:', err)
    });
  }

  isReportReviewed(report: any): boolean {
    if (!report) return false;
    const status = (report.status || report.Status || '').toLowerCase();
    return status === 'reviewed';
  }

  canEditReport(report: any): boolean {
    if (!report) return false;
    const status = (report.status || report.Status || '').toLowerCase();
    return status !== 'reviewed';
  }

  openReportModal() {
    this.isReportAdding = false;
    this.activeModal = 'report';
    this.loadHistory();
    this.loadWeeklyReports();
  }

  startNewReport() {
    this.isReportAdding = true;
    this.isReportEditing = false;
    this.editingReportId = null;
    if (this.weeklyReports.length > 0) {
      const latest = this.weeklyReports[0];
      this.reportWeek = (latest.weekId || latest.weekNumber || 0) + 1;
    } else {
      this.reportWeek = 1;
    }
    this.reportContent = '';
    this.reportGithubLink = '';
    this.reportFileUrl = '';
  }



  ngOnInit() {
    if (this.isBrowser) {
      this.loadProfile();
      this.loadMyGroup();
      this.loadMyGroupSilently();

      const currentUser = this.getCurrentUser();
      const userId = currentUser?.email;

      if (userId) {
        this.wsService.connect(userId);

        this.wsSubscription = this.wsService.getMessages().subscribe({
          next: (message) => {
            console.log('Received from Backend:', message);
            const msgType = message.type || message.Type;

            if (
              msgType === 'GROUP_UPDATED' ||
              msgType === 'MEMBER_ACCEPTED' ||
              msgType === 'TOPIC_STATUS_UPDATED' ||
              msgType === 'TOPIC_SUBMITTED'
            ) {
              this.loadMyGroupSilently();
              this.successMessage = message.message || 'Data has been updated automatically!';
              setTimeout(() => this.successMessage = '', 4000);
            }
          },
          error: (err) => console.error('WebSocket Error:', err)
        });
      }
    }
  }

  ngOnDestroy() {
    if (this.wsSubscription) {
      this.wsSubscription.unsubscribe();
    }
  }

  switchTab(tab: 'overview' | 'profile' | 'password') {
    this.activeTab = tab;
    this.successMessage = '';
    this.errorMessage = '';
    this.editMode = false;
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }



  handleCreateGroup() {
    if (!this.newGroupName.trim()) {
      this.errorMessage = 'Please enter a group name.';
      return;
    }

    this.isLoading = true;
    this.groupService.createGroup({
      groupName: this.newGroupName,
      targetMembers: 4
    }).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.successMessage = res.message;
        this.newGroupName = '';
        this.loadMyGroup();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = this.extractError(err, 'Failed to create group.');
      }
    });
  }

  handleInviteMember() {
    if (!this.inviteEmail.trim() || !this.myGroup) return;

    this.isLoading = true;
    this.groupService.inviteMember({
      groupId: this.myGroup.groupId,
      inviteeEmail: this.inviteEmail
    }).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.successMessage = res.message;
        this.inviteEmail = '';
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = this.extractError(err, 'Failed to send invitation.');
      }
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
        a.download = fileName; // Important: tells browser to act as download
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
      },
      error: (err) => {
        this.errorMessage = this.extractError(err, 'Failed to download file.');
      }
    });
  }

  // ===== Profile =====
  private extractError(err: any, fallback: string): string {
    const error = err?.error;
    if (typeof error === 'string') return error;
    if (error?.errors) {
      const messages = Object.values(error.errors).flat();
      return (messages as string[]).join(' ');
    }
    if (error?.message) return error.message;
    if (error?.title) return error.title;
    return fallback;
  }

  loadProfile() {
    this.isLoading = true;
    this.userService.getProfile().subscribe({
      next: (res) => {
        this.profile = res;
        this.fullName = res.fullName;
        this.phone = res.phone || '';
        this.avatarUrl = res.avatarUrl || '';
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Unable to load profile.';
      }
    });
  }

  toggleEdit() {
    this.editMode = !this.editMode;
    this.successMessage = '';
    this.errorMessage = '';
    this.selectedAvatarFile = null;
    this.avatarPreview = '';
    if (this.editMode && this.profile) {
      this.fullName = this.profile.fullName;
      this.phone = this.profile.phone || '';
      this.avatarUrl = this.profile.avatarUrl || '';
      this.avatarPreview = this.profile.avatarUrl || '';
    }
  }

  onAvatarSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || !input.files[0]) return;

    const file = input.files[0];

    if (!file.type.startsWith('image/')) {
      this.errorMessage = 'Please select an image file (jpg, png, gif, ...).';
      return;
    }

    if (file.size > 2 * 1024 * 1024) {
      this.errorMessage = 'Image must not exceed 2MB.';
      return;
    }

    this.selectedAvatarFile = file;
    this.errorMessage = '';

    this.resizeImage(file, 64, 64).then((dataUrl) => {
      this.avatarPreview = dataUrl;
      this.avatarUrl = dataUrl;
    });
  }

  private resizeImage(file: File, maxWidth: number, maxHeight: number): Promise<string> {
    return new Promise((resolve) => {
      const reader = new FileReader();
      reader.onload = (e) => {
        const img = new Image();
        img.onload = () => {
          const canvas = document.createElement('canvas');
          let width = img.width;
          let height = img.height;

          if (width > height) {
            if (width > maxWidth) {
              height = Math.round(height * maxWidth / width);
              width = maxWidth;
            }
          } else {
            if (height > maxHeight) {
              width = Math.round(width * maxHeight / height);
              height = maxHeight;
            }
          }

          canvas.width = width;
          canvas.height = height;
          const ctx = canvas.getContext('2d')!;
          ctx.drawImage(img, 0, 0, width, height);

          resolve(canvas.toDataURL('image/jpeg', 0.7));
        };
        img.src = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    });
  }

  removeAvatar() {
    this.selectedAvatarFile = null;
    this.avatarPreview = '';
    this.avatarUrl = '';
  }

  saveProfile() {
    this.successMessage = '';
    this.errorMessage = '';

    if (!this.fullName.trim()) {
      this.errorMessage = 'Please enter your full name.';
      return;
    }

    this.isLoading = true;

    this.userService.updateProfile({
      fullName: this.fullName,
      phone: this.phone || undefined,
      avatarUrl: this.avatarUrl || undefined
    }).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        this.successMessage = res?.message || 'Profile updated successfully!';
        this.editMode = false;
        this.loadProfile();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = this.extractError(err, 'Update failed!');
      }
    });
  }

  // ===== Password =====
  sendOtp() {
    this.successMessage = '';
    this.errorMessage = '';
    this.isSendingOtp = true;

    this.userService.sendOtpChangePassword().subscribe({
      next: (res: any) => {
        this.isSendingOtp = false;
        this.otpSent = true;
        this.successMessage = res || 'OTP has been sent to your email.';
      },
      error: (err) => {
        this.isSendingOtp = false;
        this.errorMessage = this.extractError(err, 'Failed to send OTP!');
      }
    });
  }

  changePassword() {
    this.successMessage = '';
    this.errorMessage = '';

    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }

    if (this.newPassword.length < 6) {
      this.errorMessage = 'New password must be at least 6 characters.';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    if (!this.otp.trim()) {
      this.errorMessage = 'Please enter the OTP code.';
      return;
    }

    this.isChangingPassword = true;

    this.userService.updatePasswordProfile({
      oldPassword: this.currentPassword,
      newPassword: this.newPassword,
      otp: this.otp
    }).subscribe({
      next: (res: any) => {
        this.isChangingPassword = false;
        this.successMessage = res?.message || 'Password changed successfully!';
        this.currentPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
        this.otp = '';
        this.otpSent = false;
      },
      error: (err) => {
        this.isChangingPassword = false;
        this.errorMessage = this.extractError(err, 'Failed to change password!');
      }
    });
  }

  togglePasswordVisibility(field: 'current' | 'new' | 'confirm') {
    if (field === 'current') this.showCurrentPassword = !this.showCurrentPassword;
    if (field === 'new') this.showNewPassword = !this.showNewPassword;
    if (field === 'confirm') this.showConfirmPassword = !this.showConfirmPassword;
  }

  // ===== Misc =====
  getCurrentUser() {
    return this.authService.getCurrentUser();
  }

  getInitials(): string {
    const name = this.profile?.fullName || this.getCurrentUser()?.fullName || '';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  handleKickMember(targetUserId: number) {
    // Kiểm tra xem đã có thông tin nhóm chưa
    if (!this.myGroup) return;

    // Hiển thị hộp thoại xác nhận trước khi xóa
    const confirmKick = confirm("Are you sure you want to remove this member from the group?");
    if (!confirmKick) return;

    this.isGroupLoading = true; // Bật loading
    this.successMessage = '';
    this.errorMessage = '';

    this.groupService.kickMember(this.myGroup.groupId, targetUserId).subscribe({
      next: (res: any) => {
        this.isGroupLoading = false;
        this.successMessage = res?.message || 'Member successfully removed.';

        // Gọi lại hàm này để load lại danh sách thành viên mới nhất mà không làm chớp màn hình
        this.loadMyGroupSilently();
      },
      error: (err) => {
        this.isGroupLoading = false;
        this.errorMessage = this.extractError(err, 'Failed to remove member.');
      }
    });
  }

  isCurrentUserLeader(): boolean {
    const currentUser = this.getCurrentUser();
    if (!currentUser || !this.myGroup) return false;

    // Tìm user đang đăng nhập trong nhóm dựa vào fullName thay vì userId
    const myRecord = this.myGroup.members.find(m => m.fullName === currentUser.fullName);

    return myRecord?.roleInGroup === 'Leader';
  }

  logout() {
    // ==========================================
    // ĐÓNG WEBSOCKET KHI ĐĂNG XUẤT
    // ==========================================
    this.wsService.disconnect();

    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => {
        this.authService.clearToken();
        this.router.navigate(['/login']);
      }
    });
  }

  // Thêm vào trong class UserDashboardComponent

  handleDeleteGroup() {
    if (!this.myGroup) return;

    const confirmDelete = confirm(
      "WARNING: This will permanently delete the group and all its data (members, topics, reports). This action cannot be undone. Are you sure?"
    );

    if (!confirmDelete) return;

    this.isGroupLoading = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.groupService.deleteGroup(this.myGroup.groupId).subscribe({
      next: (res: any) => {
        this.isGroupLoading = false;
        this.successMessage = res?.message || 'Group has been deleted successfully.';

        // Quan trọng: Reset lại biến group về null để UI hiển thị nút "Create Group"
        this.myGroup = null;

        // Nếu có dùng WebSocket, có thể gửi thông báo disband tại đây nếu cần
      },
      error: (err) => {
        this.isGroupLoading = false;
        this.errorMessage = this.extractError(err, 'Failed to delete group.');
      }
    });
  }





}

