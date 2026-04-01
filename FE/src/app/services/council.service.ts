import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Staff {
  userId: number;
  fullName: string;
  email: string;
}

export interface CouncilCreateRequest {
  name: string;
  memberIds: number[];
}

export interface ServiceResponse<T> {
  data: T;
  success: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class CouncilService {
  private apiUrl = 'https://localhost:7084/api/Council';

  constructor(private http: HttpClient) { }

  getAvailableStaffs(): Observable<ServiceResponse<Staff[]>> {
    return this.http.get<ServiceResponse<Staff[]>>(`${this.apiUrl}/available-staffs`);
  }

  searchStaff(query: string): Observable<ServiceResponse<Staff[]>> {
    return this.http.get<ServiceResponse<Staff[]>>(`${this.apiUrl}/search-staff?query=${query}`);
  }

  createFullCouncil(data: CouncilCreateRequest): Observable<ServiceResponse<number>> {
    return this.http.post<ServiceResponse<number>>(`${this.apiUrl}/create-full`, data);
  }
}
