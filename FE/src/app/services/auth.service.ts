import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import {
  LoginRequest,
  RegisterRequest,
  TokenResponse,
  ForgotPasswordRequest,
  ResetPasswordRequest
} from '../models/auth.model';

export interface UserInfo {
  id: number;
  email: string;
  fullName: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = 'https://localhost:7084/api/auth';
  private isBrowser: boolean;

  constructor(private http: HttpClient, @Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  // ===== Auth endpoints =====

  login(request: LoginRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, request).pipe(
      tap(res => {
        if (res?.tokens) {
          this.saveToken(res.tokens);
        }
      })
    );
  }

  register(request: RegisterRequest): Observable<string> {
    return this.http.post(`${this.apiUrl}/register`, request, { responseType: 'text' });
  }

  verify(token: string): Observable<string> {
    return this.http.get(`${this.apiUrl}/verify`, {
      params: { token },
      responseType: 'text'
    });
  }

  refreshToken(): Observable<TokenResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<TokenResponse>(`${this.apiUrl}/refresh-token`, { refreshToken }).pipe(
      tap(res => this.saveToken(res))
    );
  }

  logout(): Observable<any> {
    const refreshToken = this.getRefreshToken();
    return this.http.post(`${this.apiUrl}/logout`, { refreshToken }).pipe(
      tap(() => this.clearToken())
    );
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, request);
  }

  // ===== Token management =====

  saveToken(response: any): void {
  if (!this.isBrowser) return;

  localStorage.setItem('accessToken', response.accessToken);
  localStorage.setItem('refreshToken', response.refreshToken);

  // Giải mã token để lấy ID và các thông tin khác
  try {
    const payload = JSON.parse(atob(response.accessToken.split('.')[1]));
    
    // Tạo object user hoàn chỉnh từ thông tin trong Token và thông tin từ API
    const fullUser = {
      id: payload.id || payload.nameid, // Lấy ID từ token (3010)
      email: response.user?.email || payload.email,
      fullName: response.user?.fullName || "",
      role: response.user?.role || payload.role
    };

    localStorage.setItem('user', JSON.stringify(fullUser));

    const exp = payload.exp * 1000;
    localStorage.setItem('accessTokenExpiry', exp.toString());
  } catch (e) {
    console.error("Error decoding token:", e);
    // Backup nếu decode lỗi thì lưu user mặc định
    if (response.user) {
      localStorage.setItem('user', JSON.stringify(response.user));
    }
  }
}

  getAccessToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem('refreshToken');
  }

  getCurrentUser(): UserInfo | null {
    if (!this.isBrowser) return null;
    const user = localStorage.getItem('user');
    if (!user || user === 'undefined' || user === 'null') return null;

    try {
      return JSON.parse(user);
    } catch {
      localStorage.removeItem('user');
      return null;
    }
  }

  // Thêm hàm này để sửa lỗi ts(2339) trong Council Dashboard
  getUserId(): number | null {
    const user = this.getCurrentUser();
    return user ? Number(user.id) : null;
  }

  isLoggedIn(): boolean {
    if (!this.isBrowser) return false;
    const token = this.getAccessToken();
    const user = this.getCurrentUser();
    // Check if both token and user are present to avoid "half-logged-in" states
    return !!token && !!user;
  }

  clearToken(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('expiryDate');
    localStorage.removeItem('user');
  }

  // Thêm vào trong class AuthService trong file auth.service.ts
  getRole(): string | null {
    const user = this.getCurrentUser();
    return user ? user.role : null;
  }

}