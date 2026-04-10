import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminCreateUserRequest } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  private apiUrl = 'https://localhost:7084/api/Admin';
  private apiUrlGroup = 'https://localhost:7084/api/Groups';
  private apiUrlAdminTimeline = 'https://localhost:7084/api/Admin/setup-timeline';

  constructor(private http: HttpClient) { }

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

  importUsers(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post(`${this.apiUrl}/import-users`, formData);
  }

  exportStudents(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export-students`, {
      responseType: 'blob'
    });
  }

  getAllGroups(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrlGroup}/admin/all-groups`);
  }

  deleteGroup(groupId: number): Observable<any> {
    return this.http.delete(`${this.apiUrlGroup}/admin/${groupId}`);
  }

  kickMentor(groupId: number): Observable<any> {
    return this.http.delete(`${this.apiUrlGroup}/admin/${groupId}/kick-mentor`);
  }

  assignMentor(groupId: number, mentorId: number): Observable<any> {
    return this.http.post(`${this.apiUrlGroup}/admin/assign-mentor`, {
      groupId: groupId,
      mentorId: mentorId
    });
  }

  getAllMentors(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/admin/all-mentors`);
  }

  setupTimeline(startDate: string): Observable<any> {
    const body = { startDate: startDate };
    return this.http.post(this.apiUrlAdminTimeline, body);
  }
}
