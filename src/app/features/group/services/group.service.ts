import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { CreateGroupRequest, CreateGroupResponse } from '../models/group.model';
import { GroupDetail } from '../models/group-detail.model';

@Injectable({
  providedIn: 'root'
})
export class GroupService {

  private apiUrl = 'https://localhost:7084/api/Groups';

  constructor(private http: HttpClient) {}

  createGroup(data: CreateGroupRequest): Observable<CreateGroupResponse> {
    return this.http.post<CreateGroupResponse>(
      `${this.apiUrl}/create`,
      data
    );
  }

  getGroupDetail(id: string): Observable<GroupDetail> {
    return this.http.get<GroupDetail>(
      `${this.apiUrl}/${id}`
    );
  }
  getUserGroup(userId: number) {
  return this.http.get<any>(`${this.apiUrl}/user/${userId}`);
}
  inviteMember(data: { groupId: number, inviteeEmail: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/invite`, data);
  }
}