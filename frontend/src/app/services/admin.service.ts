import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface HackathonForAdmin {
  id: number;
  name: string;
  description?: string;
  startDate: Date;
  endDate: Date;
  submissionDeadline: Date;
  judgingDeadline: Date;
  status: string;
}

export interface JudgingCriterion {
  id: number;
  hackathonId: number;
  name: string;
  description?: string;
  weight: number;
  maxScore: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  // Hackathon Management
  getHackathons(): Observable<HackathonForAdmin[]> {
    return this.http.get<HackathonForAdmin[]>(`${this.apiUrl}/hackathons`);
  }

  getHackathon(id: number): Observable<HackathonForAdmin> {
    return this.http.get<HackathonForAdmin>(`${this.apiUrl}/hackathons/${id}`);
  }

  updateHackathon(id: number, data: Partial<HackathonForAdmin>): Observable<HackathonForAdmin> {
    return this.http.put<HackathonForAdmin>(
      `${this.apiUrl}/hackathons/${id}`,
      data
    );
  }

  // User Management
  getHackathonUsers(hackathonId: number): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/hackathons/${hackathonId}/users`
    );
  }

  updateUserRole(userId: number, role: string): Observable<any> {
    return this.http.put<any>(
      `${this.apiUrl}/users/${userId}/role`,
      { role }
    );
  }

  // Judging Criteria Management
  getJudgingCriteria(hackathonId: number): Observable<JudgingCriterion[]> {
    return this.http.get<JudgingCriterion[]>(
      `${this.apiUrl}/hackathons/${hackathonId}/criteria`
    );
  }

  addJudgingCriterion(hackathonId: number, criterion: Partial<JudgingCriterion>): Observable<JudgingCriterion> {
    return this.http.post<JudgingCriterion>(
      `${this.apiUrl}/hackathons/${hackathonId}/criteria`,
      criterion
    );
  }

  updateJudgingCriterion(id: number, criterion: Partial<JudgingCriterion>): Observable<JudgingCriterion> {
    return this.http.put<JudgingCriterion>(
      `${this.apiUrl}/criteria/${id}`,
      criterion
    );
  }

  deleteJudgingCriterion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/criteria/${id}`);
  }

  // Awards Management
  getAwards(hackathonId: number): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/hackathons/${hackathonId}/awards`
    );
  }

  // Export & Announcements
  announceWinners(hackathonId: number, awardIds: number[]): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/hackathons/${hackathonId}/announce-winners`,
      awardIds
    );
  }

  exportHackathonData(hackathonId: number): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/hackathons/${hackathonId}/export`
    );
  }
}
