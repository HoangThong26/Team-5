import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

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

export const adminGuard: CanActivateFn = () => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) return true;

  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.getCurrentUser();
  
  // LOG THỰC TẾ ĐỂ SOI LỖI
  console.log('Dữ liệu User lấy từ LocalStorage:', user);

  // Thử cho qua mọi trường hợp nếu có Token để test
  if (authService.getAccessToken()) {
      console.log('Có Token, tạm thời cho phép vào Admin để test...');
      return true;
  }

  router.navigate(['/login']);
  return false;
};

/** Guard ngăn user đã đăng nhập truy cập lại trang login/register */
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
