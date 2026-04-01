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

  constructor(private authService: AuthService, private router: Router) { }

  private extractError(err: any): string {
    console.log('[Login Error]', err?.status, err?.error, err);

    if (!err?.status || err.status === 0) {
      return 'Unable to connect to server.';
    }

    const error = err?.error;

    let msg: string | undefined;
    if (typeof error === 'string') {
      msg = error;
    } else if (error?.detail) {
      msg = error.detail;
    } else if (error?.message && !(error instanceof Error)) {
      msg = error.message;
    } else if (error?.title) {
      msg = error.title;
    }

    if (error?.errors) {
      const messages = Object.values(error.errors).flat();
      return (messages as string[]).join(' ');
    }

    if (msg) {
      return msg;
    }

    return 'Invalid email or password.';
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
        console.log('[Login] Success. User context:', savedUser);
        if (!savedUser) {
          this.errorMessage = 'Failed to save session. Please try again.';
          return;
        }
        const role = savedUser.role?.toLowerCase();

        if (role === 'admin') {
          this.router.navigate(['/admin']);
        } else if (role === 'mentor') {
          this.router.navigate(['/mentor-dashboard']);
        } else if (role === 'council') {
          this.router.navigate(['/council-dashboard']);
        }
        else {
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