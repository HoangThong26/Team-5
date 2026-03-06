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
      this.errorMessage = 'Please enter your full name.';
      return;
    }
    if (this.fullName.trim().length < 2) {
      this.errorMessage = 'Full name must be at least 2 characters.';
      return;
    }
    if (!this.email.trim()) {
      this.errorMessage = 'Please enter your email.';
      return;
    }
    const passwordRegex = /^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;
    if (!this.password || !passwordRegex.test(this.password)) {
      this.errorMessage = 'Password must be at least 8 characters, including 1 uppercase letter, 1 number, and 1 special character.';
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
        this.successMessage = res?.message || res || 'Registration successful! Please check your email to verify.';
        this.fullName = '';
        this.email = '';
        this.password = '';
        this.phone = '';
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = this.extractError(err, 'Registration failed! Please try again.');
      }
    });
  }
}