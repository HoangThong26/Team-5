import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TopicDto } from '../models/topic.model';

// Interface khớp với CreateGroupRequest trong Backend
export interface CreateGroupRequest {
  groupName: string;
  targetMembers: number;
}

// Quan trọng: Đổi từ 'email' thành 'inviteeEmail' để khớp với Backend
export interface InviteMemberRequest {
  groupId: number; 
  inviteeEmail: string;
}

// Interface mô tả dữ liệu trả về từ GetGroupDetails / GetMyGroup
export interface GroupMemberDto {
  userId: number;
  fullName?: string;
  roleInGroup: string;
  joinedAt: Date;
}

export interface TopicResponse {
  topicId: number;
  title: string;
  description: string;
  status: string;
}

export interface GroupDetailResponse {
  groupId: number;
  groupName: string;
  status: string;
  createdAt: string;
  mentorName: string | null;
  members: GroupMemberDto[];
  topic?: TopicDto;
}

@Injectable({
  providedIn: 'root'
})
export class GroupService {
  // Đảm bảo URL này khớp với cấu hình launchSettings.json của Backend
  private apiUrl = 'https://localhost:7084/api/Groups'; 

  constructor(private http: HttpClient) { }

  // POST: api/Groups/create
  createGroup(request: CreateGroupRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, request);
  }

  // GET: api/Groups
  getMyGroup(): Observable<GroupDetailResponse> {
    return this.http.get<GroupDetailResponse>(`${this.apiUrl}`);
  }

  // GET: api/Groups/{id}
  getGroupById(id: number): Observable<GroupDetailResponse> {
    return this.http.get<GroupDetailResponse>(`${this.apiUrl}/${id}`);
  }

  // POST: api/Groups/invite
  inviteMember(request: InviteMemberRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/invite`, request);
  }

  /**
   * GET: api/Groups/accept-invite?invitationId=...
   * Vì Backend trả về Content(html, "text/html") nên bắt buộc phải có { responseType: 'text' }
   */
  acceptInvite(invitationId: number): Observable<string> {
    const params = new HttpParams().set('invitationId', invitationId.toString());
    return this.http.get(`${this.apiUrl}/accept-invite`, { 
      params, 
      responseType: 'text' 
    });
  }

  kickMember(groupId: number, targetUserId: number): Observable<any> {
    const url = `${this.apiUrl}/${groupId}/members/${targetUserId}`;
    
    return this.http.delete(url);
  }

  // Trong group.service.ts
deleteGroup(groupId: number): Observable<any> {
  // Vì Leader cũng dùng chung API này (đã phân quyền Roles="Admin,Student")
  return this.http.delete(`https://localhost:7084/api/Groups/admin/${groupId}`);
}

assignMentor(groupId: number, mentorId: number): Observable<any> {
  return this.http.post(`${this.apiUrl}/admin/assign-mentor`, { 
    groupId: groupId, 
    mentorId: mentorId 
  });
}
}