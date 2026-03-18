import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {TopicApprovalRequest,TopicDto} from '../models/topic.model'

@Injectable({ providedIn: 'root' })
export class TopicService {
  private apiUrl = 'https://localhost:7084/api/Topics'; 
  private mentorUrl = 'https://localhost:7084/api/Mentor';

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

getPendingTopics(): Observable<TopicDto[]> {
  // Bỏ chữ /Topic/ đi, chỉ để đường dẫn tương đối từ [Route("api/[controller]")]
  return this.http.get<TopicDto[]>(`${this.apiUrl}/mentor/pending-topics`); 
}

approveTopic(request: TopicApprovalRequest): Observable<any> {
    // URL sẽ là: https://localhost:7084/api/Mentor/approve
    return this.http.post(`${this.mentorUrl}/topics/approve`, request);
  }
}