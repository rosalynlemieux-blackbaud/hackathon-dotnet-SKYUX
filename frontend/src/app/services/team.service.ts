import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Team {
  id: number;
  hackathonId: number;
  name: string;
  description?: string;
  leaderId: number;
  leader?: any;
  teamMembers: TeamMember[];
  ideas?: any[];
  createdAt: Date;
  updatedAt?: Date;
}

export interface TeamMember {
  id: number;
  teamId: number;
  userId: number;
  user?: any;
  joinedAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class TeamService {
  private apiUrl = `${environment.apiUrl}/teams`;

  constructor(private http: HttpClient) {}

  getTeams(hackathonId?: number): Observable<Team[]> {
    let url = this.apiUrl;
    if (hackathonId) {
      url += `?hackathonId=${hackathonId}`;
    }
    return this.http.get<Team[]>(url);
  }

  getTeam(id: number): Observable<Team> {
    return this.http.get<Team>(`${this.apiUrl}/${id}`);
  }

  createTeam(team: Partial<Team>): Observable<Team> {
    return this.http.post<Team>(this.apiUrl, team);
  }

  updateTeam(id: number, team: Partial<Team>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, team);
  }

  deleteTeam(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  addMember(teamId: number, userId: number): Observable<TeamMember> {
    return this.http.post<TeamMember>(
      `${this.apiUrl}/${teamId}/members`,
      { userId }
    );
  }

  removeMember(teamId: number, userId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${teamId}/members/${userId}`
    );
  }
}
