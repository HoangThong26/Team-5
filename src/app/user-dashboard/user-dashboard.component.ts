import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserService } from '../services/user.service';
import { UserProfile } from '../models/user.model';

@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './user-dashboard.component.html',
  styleUrls: ['./user-dashboard.component.css']
})
export class UserDashboardComponent implements OnInit {

  private isBrowser: boolean;

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

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit() {
    if (this.isBrowser) {
      this.loadProfile();
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

    // Resize image to fit DB base64 limit (500 chars)
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

  logout() {
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => {
        this.authService.clearToken();
        this.router.navigate(['/login']);
      }
    });
  }
}
