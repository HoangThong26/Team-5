import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DefenseEvaluationRequest, DefenseScoreDto } from '../models/defense.model';
import { AssignedDefense, EvaluationDetails } from '../models/evaluation.model';


@Injectable({
  providedIn: 'root'
})
export class DefenseService {
  private apiUrl = 'https://localhost:7084/api/Defense';

  constructor(private http: HttpClient) {}

  getAssignedDefenses(): Observable<AssignedDefense[]> {
    return this.http.get<AssignedDefense[]>(`${this.apiUrl}/assigned-defenses`);
  }

  submitEvaluation(request: DefenseEvaluationRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/submit-evaluation`, request);
  }

 getEvaluationDetails(defenseId: number, userId: number): Observable<EvaluationDetails> {
    return this.http.get<EvaluationDetails>(`${this.apiUrl}/my-evaluation/${defenseId}/${userId}`);
  }
}