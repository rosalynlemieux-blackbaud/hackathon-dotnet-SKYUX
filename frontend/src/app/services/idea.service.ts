import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Idea } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class IdeaService {
  private apiUrl = `${environment.apiUrl}/ideas`;
  private ideasSubject = new BehaviorSubject<Idea[]>([]);
  public ideas$ = this.ideasSubject.asObservable();

  constructor(private http: HttpClient) {}

  getIdeas(hackathonId?: number, status?: string, trackId?: number): Observable<Idea[]> {
    let params: any = {};
    if (hackathonId) params.hackathonId = hackathonId;
    if (status) params.status = status;
    if (trackId) params.trackId = trackId;

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
    return this.http.post<Idea>(this.apiUrl, idea).pipe(
      map(idea => this.formatDates(idea))
    );
  }

  updateIdea(id: number, idea: Partial<Idea>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, idea);
  }

  submitIdea(id: number): Observable<Idea> {
    return this.http.post<Idea>(`${this.apiUrl}/${id}/submit`, {}).pipe(
      map(idea => this.formatDates(idea))
    );
  }

  deleteIdea(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Get ideas by team
  getIdeasByTeam(teamId: number): Observable<Idea[]> {
    return this.http.get<Idea[]>(`${this.apiUrl}/team/${teamId}`).pipe(
      map(ideas => ideas.map(i => this.formatDates(i)))
    );
  }

  // Search ideas
  searchIdeas(query: string, hackathonId?: number): Observable<Idea[]> {
    let params: any = { q: query };
    if (hackathonId) params.hackathonId = hackathonId;

    return this.http.get<Idea[]>(`${this.apiUrl}/search`, { params }).pipe(
      map(ideas => ideas.map(i => this.formatDates(i)))
    );
  }

  // Update ideas cache
  updateIdeasCache(ideas: Idea[]): void {
    this.ideasSubject.next(ideas);
  }

  // Get cached ideas
  getCachedIdeas(): Idea[] {
    return this.ideasSubject.value;
  }

  private formatDates(idea: Idea): Idea {
    const ideaAwards = (idea as any).ideaAwards || [];
    const submittedByUser = (idea as any).submittedByUser;

    return {
      ...idea,
      author: idea.author || submittedByUser,
      awards: idea.awards || ideaAwards.map((ia: any) => ia.award).filter((award: any) => !!award),
      submission: idea.submission || { teamId: idea.teamId },
      createdAt: new Date(idea.createdAt),
      submittedAt: idea.submittedAt ? new Date(idea.submittedAt) : undefined
    };
  }
}
