import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {

  fullName = '';
  email = '';
  password = '';
  phone = '';
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  constructor(private authService: AuthService) {}

  private extractError(err: any, fallback: string): string {
    let error = err?.error;
    if (typeof error === 'string') {
      try {
        error = JSON.parse(error);
      } catch {
        return error;
      }
    }
    if (error?.errors) {
      const messages = Object.values(error.errors).flat();
      return (messages as string[]).join(' ');
    }
    if (error?.message) return error.message;
    if (error?.title) return error.title;
    return fallback;
  }

  onSubmit() {
    this.successMessage = '';
    this.errorMessage = '';

    if (!this.fullName.trim()) {
      this.errorMessage = 'Vui lòng nhập họ và tên.';
      return;
    }
    if (this.fullName.trim().length < 2) {
      this.errorMessage = 'Họ tên phải có 2 ký tự trở lên.';
      return;
    }
    if (!this.email.trim()) {
      this.errorMessage = 'Vui lòng nhập email.';
      return;
    }
    if (!this.password || this.password.length < 6) {
      this.errorMessage = 'Mật khẩu phải có ít nhất 6 ký tự.';
      return;
    }

    this.isLoading = true;

    const request = {
      fullName: this.fullName,
      email: this.email,
      password: this.password,
      phone: this.phone
    };

    this.authService.register(request).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        this.successMessage = res?.message || res || 'Đăng ký thành công! Vui lòng kiểm tra email để xác minh.';
        this.fullName = '';
        this.email = '';
        this.password = '';
        this.phone = '';
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = this.extractError(err, 'Đăng ký thất bại! Vui lòng thử lại.');
      }
    });
  }
}