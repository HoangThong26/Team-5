import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WeeklyReportRequest, WeeklyReportDto, WeeklyReportHistoryDto, CouncilEligibilityDto } from '../models/weekly-report.model';

@Injectable({
  providedIn: 'root'
})
export class WeeklyReportService {
  private apiUrl = 'https://localhost:7084/api/WeeklyReport';

  constructor(private http: HttpClient) { }

submitReport(request: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/submit`, request);
  }

  getReportsByGroupId(groupId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/group/${groupId}`);
  }

  getMentorInbox(): Observable<WeeklyReportDto[]> {
    return this.http.get<WeeklyReportDto[]>(`${this.apiUrl}/mentor-inbox`);
  }
  // Thêm vào WeeklyReportService
  getHistoryByGroupId(groupId: number): Observable<WeeklyReportHistoryDto[]> {
    return this.http.get<WeeklyReportHistoryDto[]>(`${this.apiUrl}/history/${groupId}`);
  }

  getCouncilEligibility(groupId: number): Observable<CouncilEligibilityDto> {
    return this.http.get<CouncilEligibilityDto>(`${this.apiUrl}/group/${groupId}/council-eligibility`);
  }

  getMyCouncilEligibility(): Observable<CouncilEligibilityDto> {
    return this.http.get<CouncilEligibilityDto>(`${this.apiUrl}/my-council-eligibility`);
  }

  updateReport(reportId: number, request: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/update/${reportId}`, request);
  }

  downloadFile(fileName: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/download/${fileName}`, { responseType: 'blob' });
  }
}
