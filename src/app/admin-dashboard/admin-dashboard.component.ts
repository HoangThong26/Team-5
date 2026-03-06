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

  // Search
  searchKeyword = '';
  isSearching = false;

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
        this.errorMessage = 'Unable to load user list.';
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
      this.errorMessage = 'Please fill in all required fields.';
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
        this.successMessage = res?.message || 'User created successfully!';
        this.resetForm();
        this.loadUsers();
      },
      error: (err) => {
        this.isCreating = false;
        this.errorMessage = this.extractError(err, 'Failed to create user!');
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

  searchUsers() {
    const keyword = this.searchKeyword.trim();
    if (!keyword) {
      this.loadUsers();
      return;
    }

    this.isSearching = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.adminService.searchUsers(keyword).subscribe({
      next: (res) => {
        this.users = res;
        this.isSearching = false;
        this.successMessage = `Found ${res.length} results for "${keyword}".`;
      },
      error: (err) => {
        this.isSearching = false;
        this.errorMessage = this.extractError(err, 'Search failed!');
      }
    });
  }

  clearSearch() {
    this.searchKeyword = '';
    this.successMessage = '';
    this.errorMessage = '';
    this.loadUsers();
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
