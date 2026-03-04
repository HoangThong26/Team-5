import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {

  email = '';
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  constructor(private authService: AuthService) {}

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

  onSubmit() {
    this.successMessage = '';
    this.errorMessage = '';

    if (!this.email.trim()) {
      this.errorMessage = 'Vui lòng nhập email.';
      return;
    }

    this.isLoading = true;

    this.authService.forgotPassword({ email: this.email }).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        this.successMessage = res?.message || 'OTP đã được gửi đến email của bạn!';
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = this.extractError(err, 'Gửi OTP thất bại! Vui lòng thử lại.');
      }
    });
  }
}
