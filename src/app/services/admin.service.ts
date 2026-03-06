import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminCreateUserRequest } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  private apiUrl = 'https://localhost:7084/api/Admin';

  constructor(private http: HttpClient) {}

  createUser(request: AdminCreateUserRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/create-user`, request);
  }

  getAllUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/all-users`);
  }

  searchUsers(keyword: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/search-users`, {
      params: { keyword }
    });
  }
}
