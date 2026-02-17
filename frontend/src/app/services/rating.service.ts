import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Rating {
  id: number;
  ideaId: number;
  judgeId: number;
  judge?: any;
  criterionId: number;
  criterion?: JudgingCriterion;
  score: number;
  feedback?: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface JudgingCriterion {
  id: number;
  hackathonId: number;
  name: string;
  description?: string;
  weight: number;
  maxScore: number;
}

export interface RatingAverage {
  averageScore: number;
  ratingCount: number;
  byJudge?: { judgeId: number; count: number }[];
}

@Injectable({
  providedIn: 'root'
})
export class RatingService {
  private apiUrl = `${environment.apiUrl}/ratings`;

  constructor(private http: HttpClient) {}

  getRatings(ideaId?: number, judgeId?: number): Observable<Rating[]> {
    let url = this.apiUrl;
    const params: string[] = [];
    
    if (ideaId) params.push(`ideaId=${ideaId}`);
    if (judgeId) params.push(`judgeId=${judgeId}`);
    
    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }
    
    return this.http.get<Rating[]>(url);
  }

  getRating(id: number): Observable<Rating> {
    return this.http.get<Rating>(`${this.apiUrl}/${id}`);
  }

  submitRating(ideaId: number, criterionId: number, score: number, feedback?: string): Observable<Rating> {
    return this.http.post<Rating>(this.apiUrl, {
      ideaId,
      criterionId,
      score,
      feedback
    });
  }

  getAverageRating(ideaId: number): Observable<RatingAverage> {
    return this.http.get<RatingAverage>(
      `${this.apiUrl}/idea/${ideaId}/average`
    );
  }

  deleteRating(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
