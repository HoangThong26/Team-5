import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { ProfileComponent } from './profile/profile.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { UserDashboardComponent } from './user-dashboard/user-dashboard.component';
import { MentorDashboardComponent } from './mentor-dashboard/mentor-dashboard.component';
// Import các functional guards bạn đã fix ở bước trước
import { authGuard, adminGuard, guestGuard, mentorGuard, roleGuard, councilGuard } from './guards/auth.guard'; 
import { CouncilDashboardComponent } from './council-dashboard/council-dashboard.component';

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

  {
    path: 'admin/defense-schedule',
    component: AdminDashboardComponent,
    canActivate: [adminGuard],
    data: { viewMode: 'defense' }
  },

  { 
    path: 'council-dashboard', 
    component: CouncilDashboardComponent, 
    canActivate: [councilGuard] // Sử dụng guard riêng biệt cho an toàn
  },

  // Mặc định
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' } // Route xử lý lỗi 404
];