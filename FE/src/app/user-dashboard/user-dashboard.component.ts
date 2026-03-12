
import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs'; 

import { AuthService } from '../services/auth.service';
import { UserService } from '../services/user.service';
import { UserProfile } from '../models/user.model';
import { GroupService, GroupDetailResponse } from '../services/group.service';
import { WebsocketService } from '../services/websocket.service'; 
import { TopicService } from '../services/topic.service';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './user-dashboard.component.html',
  styleUrls: ['./user-dashboard.component.css']
})
export class UserDashboardComponent implements OnInit, OnDestroy { 
  
  activeModal: 'none' | 'details' | 'create' | 'invite' | 'members' = 'none';
  showGroupDetails: boolean = false;
  private isBrowser: boolean;
  myGroup: GroupDetailResponse | null = null;
  newGroupName = '';
  inviteEmail = '';
  isGroupLoading = false;
  
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
        this.isGroupLoading = false;
      },
      error: (err) => {
        this.isGroupLoading = false;
        this.myGroup = null;
      }
    });
  }

  // ==========================================
  // HÀM LOAD DỮ LIỆU NGẦM (KHÔNG BẬT LOADING)
  // ==========================================
  loadMyGroupSilently() {
  this.groupService.getMyGroup().subscribe({
    next: (res) => {
      this.myGroup = res; 

      // BỔ SUNG: Sau khi có group, phải đi lấy topic thì UI mới hiện View Mode được
      if (this.myGroup?.groupId) {
        this.topicService.getTopicByGroupId(this.myGroup.groupId).subscribe({
          next: (topicRes) => {
            if (this.myGroup) {
              this.myGroup.topic = topicRes; // Gán topic vào để HTML nhận diện
              console.log('Topic đã được nạp:', topicRes);
            }
          },
          error: (err) => {
            console.log('Nhóm này chưa có topic hoặc lỗi API', err);
            if (this.myGroup) this.myGroup.topic = undefined;
          }
        });
      }
    },
    error: (err) => console.log('Silently load group failed', err)
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
  getStatusColor(status: string): string {
  switch (status) {
    case 'Approved': return '#dcfce7'; // Xanh lá
    case 'Pending':  return '#fef9c3'; // Vàng nhạt
    case 'Rejected': return '#fee2e2'; // Đỏ nhạt
    default:         return '#e2e8f0'; // Xám
  }
  }

  getStatusTextColor(status: string): string {
    switch (status) {
      case 'Approved': return '#166534';
      case 'Pending':  return '#854d0e';
      case 'Rejected': return '#991b1b';
      default:         return '#475569';
    }
  }

  private alertTimer: any;
  handleTopicSubmit() {
    if (this.alertTimer) clearTimeout(this.alertTimer);
    if (!this.topicTitle.trim() || !this.topicDescription.trim() || !this.myGroup) {
      this.errorMessage = 'Please fill in both title and description.';
      this.alertTimer = setTimeout(() => {
            this.errorMessage = '';
        }, 3000);
      return;
    }

    this.isTopicLoading = true;
    const request = {
      groupId: this.myGroup.groupId,
      title: this.topicTitle,
      description: this.topicDescription
    };

    // Nếu chưa có topic thì gọi Submit, nếu có rồi (đang ở mode Edit) thì gọi Edit
    if (this.myGroup.topic && this.isTopicEditing) {
      this.topicService.editTopic(this.myGroup.topic.topicId, {
        title: this.topicTitle,
        description: this.topicDescription
      }).subscribe({
        next: (res) => this.handleTopicSuccess(res.message),
        error: (err) => this.handleTopicError(err)
      });
    } else {
      this.topicService.submitTopic(request).subscribe({
        next: (res) => this.handleTopicSuccess(res.message),
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
    this.successMessage = ''; // Xóa thông báo
  }, 3000);
  }


    
    ngOnInit() {
      if (this.isBrowser) {
        this.loadProfile();
        this.loadMyGroup();

        // ==========================================
        // THIẾT LẬP KẾT NỐI WEBSOCKET (SIGNALR)
        // ==========================================
        const currentUser = this.getCurrentUser();

        // SỬA Ở ĐÂY: Lấy email làm định danh thay vì id để không bị lỗi gạch đỏ
        const userId = currentUser?.email;

        if (userId) {
          // 1. Mở kết nối
          this.wsService.connect(userId);

          // 2. Lắng nghe tin nhắn từ C# Backend gửi về
          this.wsSubscription = this.wsService.getMessages().subscribe({
            next: (message) => {
              console.log('Received from Backend:', message);

              // SỬA Ở ĐÂY: Bắt cả chữ 'type' (thường) và 'Type' (hoa) do C# trả về
              if (
                message.type === 'GROUP_UPDATED' || message.Type === 'GROUP_UPDATED' ||
                message.type === 'MEMBER_ACCEPTED' || message.Type === 'MEMBER_ACCEPTED'
              ) {
                this.loadMyGroupSilently();
                this.successMessage = 'Group data has been updated automatically!';
              }
            },
            error: (err) => console.error('WebSocket Error:', err)
          });
        }
      }
    }

    // ==========================================
    // HỦY KẾT NỐI KHI RỜI KHỎI TRANG
    // ==========================================
    ngOnDestroy() {
      if (this.wsSubscription) {
        this.wsSubscription.unsubscribe();
      }
    }

    // ===== Sidebar =====
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

