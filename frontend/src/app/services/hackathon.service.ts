import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Hackathon } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class HackathonService {
  private apiUrl = `${environment.apiUrl}/hackathons`;

  constructor(private http: HttpClient) {}

  getHackathons(): Observable<Hackathon[]> {
    return this.http.get<Hackathon[]>(this.apiUrl).pipe(
      map(hackathons => hackathons.map(h => this.formatDates(h)))
    );
  }

  getHackathon(id: number): Observable<Hackathon> {
    return this.http.get<Hackathon>(`${this.apiUrl}/${id}`).pipe(
      map(hackathon => this.formatDates(hackathon))
    );
  }

  getCurrentHackathon(): Observable<Hackathon> {
    return this.http.get<Hackathon>(`${this.apiUrl}/current`).pipe(
      map(hackathon => this.formatDates(hackathon))
    );
  }

  createHackathon(hackathon: Hackathon): Observable<Hackathon> {
    return this.http.post<Hackathon>(this.apiUrl, hackathon);
  }

  updateHackathon(id: number, hackathon: Hackathon): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, hackathon);
  }

  deleteHackathon(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private formatDates(hackathon: Hackathon): Hackathon {
    return {
      ...hackathon,
      registrationStart: new Date(hackathon.registrationStart),
      registrationEnd: new Date(hackathon.registrationEnd),
      startDate: new Date(hackathon.startDate),
      endDate: new Date(hackathon.endDate),
      judgingStart: new Date(hackathon.judgingStart),
      judgingEnd: new Date(hackathon.judgingEnd),
      winnersAnnouncement: new Date(hackathon.winnersAnnouncement),
      milestones: hackathon.milestones.map(m => ({
        ...m,
        dueDate: new Date(m.dueDate)
      }))
    };
  }
}
