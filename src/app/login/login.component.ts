import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  email = '';
  password = '';
  errorMessage = '';
  isLoading = false;

  constructor(private authService: AuthService, private router: Router) {}

  private extractError(err: any): string {
    const error = err?.error;
    // Backend trả về BadRequest(string) → plain text, HttpClient parse JSON thất bại
    // Trong trường hợp đó err.status vẫn có, dùng status để trả message phù hợp
    if (err?.status === 400 || err?.status === 401) {
      if (typeof error === 'string') return error;
      if (error?.errors) {
        const messages = Object.values(error.errors).flat();
        return (messages as string[]).join(' ');
      }
      if (error?.message && !(error instanceof Error)) return error.message;
      if (error?.title) return error.title;
      return 'Sai email hoặc mật khẩu!';
    }
    return 'Sai email hoặc mật khẩu!';
  }

  onSubmit() {
    this.errorMessage = '';
    this.isLoading = true;

    const request = {
      email: this.email,
      password: this.password
    };

    this.authService.login(request).subscribe({
      next: (res) => {
        this.isLoading = false;
        console.log('[Login] Success. Token saved. User:', res.user);

        // Verify token was saved
        const savedUser = this.authService.getCurrentUser();
        if (!savedUser) {
          this.errorMessage = 'Lỗi lưu phiên đăng nhập. Vui lòng thử lại.';
          return;
        }

        // Redirect based on role (case-insensitive)
        const role = savedUser.role?.toLowerCase();
        if (role === 'admin') {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = this.extractError(err);
      }
    });
  }
}