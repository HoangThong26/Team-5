import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router,ActivatedRouteSnapshot } from '@angular/router';
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
    } else if (role === 'mentor') {
      router.navigate(['/mentor-dashboard']);
    } else if (role === 'council') {
      router.navigate(['/council-dashboard']);
    } else {
      router.navigate(['/dashboard']);
    }
  }
  return true; 
};
export const mentorGuard: CanActivateFn = () => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) return true;

  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.getCurrentUser();
  const role = user?.role?.toLowerCase();

  if (authService.isLoggedIn() && role === 'mentor') {
    return true;
  }

  // Nếu không phải mentor, đẩy về trang login hoặc trang phù hợp
  router.navigate(['/login']);
  return false;
};

// Thêm mới Council Guard
export const councilGuard: CanActivateFn = () => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) return true;

  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.getCurrentUser();
  const role = user?.role?.toLowerCase();

  // Kiểm tra đăng nhập và đúng role council
  if (authService.isLoggedIn() && role === 'council') {
    return true;
  }

  router.navigate(['/login']);
  return false;
};

/**
 * Fix hàm cuối cùng: Chuyển thành Functional Guard để dùng trong App Routing
 * Hàm này dùng cho trường hợp bạn muốn check động role từ data của Route
 */
export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) return true;

  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  const user = authService.getCurrentUser();
  const userRole = user?.role?.toLowerCase();
  const expectedRole = route.data['role']?.toLowerCase();

  if (userRole === expectedRole) {
    return true;
  }

  // Nếu không đúng role, đừng vội đẩy về login gây loop. 
  // Kiểm tra xem họ có thể vào dashboard nào khác không.
  if (userRole === 'admin') {
    router.navigate(['/admin']);
  } else if (userRole === 'mentor') {
    router.navigate(['/mentor-dashboard']);
  } else if (userRole === 'council') {
    router.navigate(['/council-dashboard']);
  } else {
    router.navigate(['/dashboard']);
  }
  return false;
};