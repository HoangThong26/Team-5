import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { AdminService } from '../services/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {

  // User list
  users: any[] = [];
  isLoadingUsers = false;

  // Create user form
  showCreateForm = false;
  newEmail = '';
  newFullName = '';
  newPhone = '';
  newRole = 'Student';
  newPassword = '';
  isCreating = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadUsers();
  }

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

  loadUsers() {
    this.isLoadingUsers = true;
    this.adminService.getAllUsers().subscribe({
      next: (res) => {
        this.users = res;
        this.isLoadingUsers = false;
      },
      error: (err) => {
        this.isLoadingUsers = false;
        this.errorMessage = 'Không thể tải danh sách người dùng.';
      }
    });
  }

  toggleCreateForm() {
    this.showCreateForm = !this.showCreateForm;
    this.successMessage = '';
    this.errorMessage = '';
  }

  createUser() {
    this.successMessage = '';
    this.errorMessage = '';

    if (!this.newEmail.trim() || !this.newFullName.trim() || !this.newPassword.trim()) {
      this.errorMessage = 'Vui lòng điền đầy đủ thông tin.';
      return;
    }

    this.isCreating = true;

    this.adminService.createUser({
      email: this.newEmail,
      fullName: this.newFullName,
      phone: this.newPhone,
      role: this.newRole,
      password: this.newPassword
    }).subscribe({
      next: (res: any) => {
        this.isCreating = false;
        this.successMessage = res?.message || 'Tạo người dùng thành công!';
        this.resetForm();
        this.loadUsers();
      },
      error: (err) => {
        this.isCreating = false;
        this.errorMessage = this.extractError(err, 'Tạo người dùng thất bại!');
      }
    });
  }

  resetForm() {
    this.newEmail = '';
    this.newFullName = '';
    this.newPhone = '';
    this.newRole = 'Student';
    this.newPassword = '';
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
