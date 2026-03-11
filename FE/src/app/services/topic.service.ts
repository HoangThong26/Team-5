import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TopicService {
  private apiUrl = 'https://localhost:7084/api/Topics'; 

  constructor(private http: HttpClient) {}

  submitTopic(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/submit`, data);
  }

  editTopic(topicId: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/edit/${topicId}`, data);
  }
  getTopicByGroupId(groupId: number): Observable<any> {
  return this.http.get(`${this.apiUrl}/group/${groupId}`);
}
}