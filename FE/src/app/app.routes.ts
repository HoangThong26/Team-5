import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { ProfileComponent } from './profile/profile.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { UserDashboardComponent } from './user-dashboard/user-dashboard.component';
import { MentorDashboardComponent } from './mentor-dashboard/mentor-dashboard.component';
// Import các functional guards bạn đã fix ở bước trước
import { authGuard, adminGuard, guestGuard, mentorGuard, roleGuard } from './guards/auth.guard'; 

export const routes: Routes = [
  // Khách (chưa login) mới vào được trang này
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },

  { path: 'profile', component: ProfileComponent, canActivate: [authGuard] },

  { 
    path: 'dashboard', 
    component: UserDashboardComponent, 
    canActivate: [roleGuard], 
    data: { role: 'student' } 
  },

  // Phân quyền cho Mentor
  { 
    path: 'mentor-dashboard', 
    component: MentorDashboardComponent, 
    canActivate: [mentorGuard] 
  },

  // Phân quyền cho Admin
  { 
    path: 'admin', 
    component: AdminDashboardComponent, 
    canActivate: [adminGuard] 
  },

  // Mặc định
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' } // Route xử lý lỗi 404
];