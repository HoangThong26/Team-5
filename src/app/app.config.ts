import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http'; // Thêm withInterceptors
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth-interceptor'; // Import interceptor vào đây

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])) 
  ]
};