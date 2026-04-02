import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface FinalGrade {
  groupId: number;
  averageScore: number;
  gradeLetter: string;
  isPublished: boolean;
  publishedAt?: Date;
  group?: { groupName: string;
    topic?: { topicName: string };
   }; // Để hiển thị tên nhóm
}

@Injectable({ providedIn: 'root' })
export class GradeService {
  private apiUrl = 'https://localhost:7084/api/Grades';

  constructor(private http: HttpClient) {}

  // Lấy toàn bộ điểm (Admin)
  getAllGrades(): Observable<FinalGrade[]> {
    return this.http.get<FinalGrade[]>(`${this.apiUrl}/all`);
  }

  // Tính điểm cho nhóm (Admin)
  calculateGrade(groupId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/calculate/${groupId}`, {});
  }

  // Công bố điểm (Admin)
  publishGrade(groupId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/publish/${groupId}`, {});
  }

  // Xem điểm cá nhân (Student)
  getMyGrade(groupId: number): Observable<FinalGrade> {
    return this.http.get<FinalGrade>(`${this.apiUrl}/my-grade/${groupId}`);
  }
}