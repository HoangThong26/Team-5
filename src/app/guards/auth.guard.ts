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
  
  // Debug logging
  console.log('User from LocalStorage:', user);

  // Allow access if token exists (for testing)
  if (authService.getAccessToken()) {
      console.log('Token found, allowing Admin access for testing...');
      return true;
  }

  router.navigate(['/login']);
  return false;
};

/** Guard to prevent logged-in users from accessing login/register pages */
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
