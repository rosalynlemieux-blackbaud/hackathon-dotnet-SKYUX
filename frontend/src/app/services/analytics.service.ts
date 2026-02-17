import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface HackathonAnalytics {
  hackathonId: number;
  hackathonName: string;
  totalParticipants: number;
  totalIdeas: number;
  submittedIdeas: number;
  draftIdeas: number;
  tracks: number;
  awards: number;
  averageIdeasPerParticipant: number;
  startDate: Date;
  endDate: Date;
}

export interface SubmissionAnalytics {
  totalSubmissions: number;
  individualSubmissions: number;
  teamSubmissions: number;
  submissionRate: number;
  submissionDeadlineMetCount: number;
  earliestSubmissionTime?: Date;
  latestSubmissionTime?: Date;
}

export interface JudgingAnalytics {
  totalJudges: number;
  ideasBeingJudged: number;
  averageScoreAcrossAll: number;
  highestScore: number;
  lowestScore: number;
  totalRatingsSubmitted: number;
  ratingsPerJudge: number;
}

export interface TeamAnalytics {
  totalTeams: number;
  totalTeamMembers: number;
  teamsWithIdeas: number;
  averageMembersPerTeam: number;
  largestTeamSize: number;
  ideasSubmittedByTeams: number;
}

export interface IdeaRanking {
  rank: number;
  ideaId: number;
  title: string;
  author: string;
  averageScore: number;
  ratingCount: number;
  status: string;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private apiUrl = `${environment.apiUrl}/analytics`;

  constructor(private http: HttpClient) {}

  getHackathonAnalytics(hackathonId: number): Observable<HackathonAnalytics> {
    return this.http.get<HackathonAnalytics>(
      `${this.apiUrl}/hackathon/${hackathonId}`
    );
  }

  getSubmissionAnalytics(hackathonId: number): Observable<SubmissionAnalytics> {
    return this.http.get<SubmissionAnalytics>(
      `${this.apiUrl}/submissions/${hackathonId}`
    );
  }

  getJudgingAnalytics(hackathonId: number): Observable<JudgingAnalytics> {
    return this.http.get<JudgingAnalytics>(
      `${this.apiUrl}/judging/${hackathonId}`
    );
  }

  getTeamAnalytics(hackathonId: number): Observable<TeamAnalytics> {
    return this.http.get<TeamAnalytics>(
      `${this.apiUrl}/teams/${hackathonId}`
    );
  }

  getTopIdeas(hackathonId: number, limit: number = 10): Observable<IdeaRanking[]> {
    return this.http.get<IdeaRanking[]>(
      `${this.apiUrl}/top-ideas/${hackathonId}?limit=${limit}`
    );
  }

  getSubmissionsByTrack(hackathonId: number): Observable<{ [key: string]: number }> {
    return this.http.get<{ [key: string]: number }>(
      `${this.apiUrl}/submissions-by-track/${hackathonId}`
    );
  }

  getAverageScoresByTrack(hackathonId: number): Observable<{ [key: string]: number }> {
    return this.http.get<{ [key: string]: number }>(
      `${this.apiUrl}/average-scores-by-track/${hackathonId}`
    );
  }

  getDashboardSummary(hackathonId: number): Observable<any> {
    return this.http.get<any>(
      `${this.apiUrl}/dashboard-summary/${hackathonId}`
    );
  }
}
