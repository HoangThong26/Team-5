import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserProfile, UpdateProfileRequest, UpdatePasswordProfileRequest } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private apiUrl = 'https://localhost:7084/api/User';

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/get-profile`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-profile`, request);
  }

  updatePasswordProfile(request: UpdatePasswordProfileRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-password-profile`, request);
  }

  sendOtpChangePassword(): Observable<any> {
    return this.http.post(`${this.apiUrl}/send-otp-change-password`, {}, { responseType: 'text' });
  }
}
