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
  isExporting: boolean = false;

  // --- CÁC BIẾN VÀ HÀM PHÂN TRANG ---
  currentPage = 1;
  pageSize = 10;

  get pagedUsers() {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    return this.users.slice(startIndex, startIndex + this.pageSize);
  }

  get totalPages() {
    return Math.ceil(this.users.length / this.pageSize) || 1;
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    for (let i = 1; i <= this.totalPages; i++) {
      pages.push(i);
    }
    return pages;
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }
  // ------------------------------------

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

  // Import Excel form
  showImportForm = false;
  selectedFile: File | null = null;
  isImporting = false;

  // Messages
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
        this.currentPage = 1; // Reset về trang 1 khi load data mới
      },
      error: (err) => {
        this.isLoadingUsers = false;
        this.errorMessage = 'Unable to load user list.';
      }
    });
  }

  // --- QUẢN LÝ CREATE FORM ---
  toggleCreateForm() {
    this.showCreateForm = !this.showCreateForm;
    this.successMessage = '';
    this.errorMessage = '';
    // Đóng form Import nếu đang mở
    if (this.showCreateForm) {
      this.showImportForm = false; 
    }
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

  // --- QUẢN LÝ IMPORT EXCEL FORM ---
  toggleImportForm() {
    this.showImportForm = !this.showImportForm;
    this.successMessage = '';
    this.errorMessage = '';
    // Đóng form Create nếu đang mở
    if (this.showImportForm) {
      this.showCreateForm = false;
    }
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onUpload(): void {
    if (!this.selectedFile) {
      this.errorMessage = 'Please select an Excel file first.';
      return;
    }

    this.isImporting = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.adminService.importUsers(this.selectedFile).subscribe({
      next: (res: any) => {
        this.isImporting = false;
        this.successMessage = res?.message || 'Users imported successfully!';
        this.selectedFile = null;
        this.showImportForm = false; // Đóng form sau khi thành công
        this.loadUsers(); // Tự động làm mới danh sách
      },
      error: (err) => {
        this.isImporting = false;
        this.errorMessage = this.extractError(err, 'Failed to import users!');
      }
    });
  }

  // --- QUẢN LÝ TÌM KIẾM & LOGOUT ---
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
        this.currentPage = 1; // Reset về trang 1 khi có kết quả tìm kiếm
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
  downloadStudentList() {
    this.isExporting = true;
    this.errorMessage = ''; // Xóa thông báo lỗi cũ (nếu có) trước khi chạy
    this.successMessage = ''; 

    this.adminService.exportStudents().subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        
        const a = document.createElement('a');
        a.href = url;
      
        const today = new Date();
        const dd = String(today.getDate()).padStart(2, '0');
        const mm = String(today.getMonth() + 1).padStart(2, '0'); 
        const yyyy = today.getFullYear();
        
        // Đổi tên file sang tiếng Anh
        a.download = `StudentList_${dd}${mm}${yyyy}.xlsx`; 
      
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url); 
        
        this.isExporting = false;
        
        // (Tùy chọn) Bạn có thể bật dòng dưới nếu muốn hiện thông báo thành công màu xanh
        // this.successMessage = 'Student list exported successfully!'; 
      },
      error: (error) => {
        console.error('Error exporting Excel file:', error);
        
        // Sử dụng biến errorMessage thay cho alert() để đồng bộ UI
        this.errorMessage = 'An error occurred while exporting the student list!';
        this.isExporting = false;
      }
    });
  }
}