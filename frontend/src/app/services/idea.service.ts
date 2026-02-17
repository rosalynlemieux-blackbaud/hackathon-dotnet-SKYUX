import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Idea } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class IdeaService {
  private apiUrl = `${environment.apiUrl}/ideas`;

  constructor(private http: HttpClient) {}

  getIdeas(hackathonId?: number, status?: string): Observable<Idea[]> {
    let params: any = {};
    if (hackathonId) params.hackathonId = hackathonId;
    if (status) params.status = status;

    return this.http.get<Idea[]>(this.apiUrl, { params }).pipe(
      map(ideas => ideas.map(i => this.formatDates(i)))
    );
  }

  getIdea(id: number): Observable<Idea> {
    return this.http.get<Idea>(`${this.apiUrl}/${id}`).pipe(
      map(idea => this.formatDates(idea))
    );
  }

  createIdea(idea: Partial<Idea>): Observable<Idea> {
    return this.http.post<Idea>(this.apiUrl, idea);
  }

  updateIdea(id: number, idea: Partial<Idea>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, idea);
  }

  submitIdea(id: number): Observable<Idea> {
    return this.http.post<Idea>(`${this.apiUrl}/${id}/submit`, {});
  }

  deleteIdea(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  private formatDates(idea: Idea): Idea {
    return {
      ...idea,
      createdAt: new Date(idea.createdAt),
      submittedAt: idea.submittedAt ? new Date(idea.submittedAt) : undefined
    };
  }
}
