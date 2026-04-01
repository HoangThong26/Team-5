import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaders } from '@angular/common/http';

export interface EvaluationRequest {
  reportId: number;
  score: number;
  comment: string;
}

@Injectable({
  providedIn: 'root'
})
export class WeeklyEvaluationService {
  private apiUrl = 'https://localhost:7084/api/WeeklyEvaluation';

  constructor(private http: HttpClient) { }

  submitEvaluation(request: EvaluationRequest): Observable<any> {
    const token = localStorage.getItem('token'); 
    
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    return this.http.post(`${this.apiUrl}/submit-grade`, request, { headers });
  }

  /** GET evaluation for a specific report (to retrieve saved score & comment) */
  getEvaluation(reportId: number): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({ 'Authorization': `Bearer ${token}` });
    return this.http.get(`${this.apiUrl}/report/${reportId}`, { headers });
  }
}
