import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent {

  email = '';
  token = '';
  newPassword = '';
  confirmPassword = '';
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

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
      this.errorMessage = 'Please enter your email.';
      return;
    }
    if (!this.token.trim()) {
      this.errorMessage = 'Please enter the OTP code.';
      return;
    }
    if (!this.newPassword || this.newPassword.length < 6) {
      this.errorMessage = 'New password must be at least 6 characters.';
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isLoading = true;

    this.authService.resetPassword({
      email: this.email,
      token: this.token,
      newPassword: this.newPassword
    }).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        this.successMessage = res?.message || 'Password changed successfully!';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = this.extractError(err, 'Failed to change password! Please try again.');
      }
    });
  }
}
