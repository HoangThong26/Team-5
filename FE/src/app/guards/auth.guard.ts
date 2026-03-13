import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Guard kiểm tra user đã đăng nhập chưa (dùng cho các trang cần auth chung) */
export const authGuard: CanActivateFn = () => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) return true;

  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};

/** Guard CHỈ cho phép Admin truy cập */
export const adminGuard: CanActivateFn = () => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) return true;

  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.getCurrentUser();
  
  console.log('User from LocalStorage in adminGuard:', user);

  if (user && user.role && user.role.toLowerCase() === 'admin') {
      return true; 
  }

  router.navigate(['/dashboard']); 
  return false;
};

export const guestGuard: CanActivateFn = () => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) return true;

  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    const user = authService.getCurrentUser();
    const role = user?.role?.toLowerCase();
    

    if (role === 'admin') {
      router.navigate(['/admin']);
    } else {
      router.navigate(['/dashboard']); 
    }
    return false;
  }

  return true; 
};