import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DefenseEvaluationRequest, DefenseScoreDto } from '../models/defense.model';
import { AssignedDefense, EvaluationDetails } from '../models/evaluation.model';
import {
  CouncilUserDto,
  CreateDefenseScheduleRequest,
  DefenseCommitteeDto,
  DefenseRegistrationItemDto,
  DefenseRegistrationStatusDto,
  UpdateDefenseScheduleRequest
} from '../models/defense-registration.model';


@Injectable({
  providedIn: 'root'
})
export class DefenseService {
  private apiUrl = 'https://localhost:7084/api/Defense';

  constructor(private http: HttpClient) {}

  getMyRegistrationStatus(): Observable<DefenseRegistrationStatusDto> {
    return this.http.get<DefenseRegistrationStatusDto>(`${this.apiUrl}/my-registration-status`);
  }

  registerForDefense(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/register`, {});
  }

  getRegistrations(): Observable<DefenseRegistrationItemDto[]> {
    return this.http.get<DefenseRegistrationItemDto[]>(`${this.apiUrl}/registrations`);
  }

  getCommittees(): Observable<DefenseCommitteeDto[]> {
    return this.http.get<DefenseCommitteeDto[]>(`${this.apiUrl}/admin/committees`);
  }

  getCouncilUsers(): Observable<CouncilUserDto[]> {
    return this.http.get<CouncilUserDto[]>(`${this.apiUrl}/admin/council-users`);
  }

  createSchedule(payload: CreateDefenseScheduleRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/admin/schedule`, payload);
  }

  updateSchedule(defenseId: number, payload: UpdateDefenseScheduleRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/admin/schedule/${defenseId}`, payload);
  }

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