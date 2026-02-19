import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { User, AuthResponse } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;
  private tokenKey = 'hackathon_access_token';
  private userKey = 'hackathon_user';

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    const storedUser = localStorage.getItem(this.userKey);
    this.currentUserSubject = new BehaviorSubject<User | null>(
      storedUser ? JSON.parse(storedUser) : null
    );
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  public isAuthenticated(): boolean {
    return !!this.token && !!this.currentUserValue;
  }

  public hasRole(role: string): boolean {
    return this.currentUserValue?.roles.includes(role) ?? false;
  }

  public isAdmin(): boolean {
    return this.hasRole('admin');
  }

  public isJudge(): boolean {
    return this.hasRole('judge') || this.isAdmin();
  }

  public isParticipant(): boolean {
    return this.hasRole('participant') || this.isJudge();
  }

  /**
   * Initiates BBID OAuth flow
   */
  login(returnUrl?: string): void {
    this.http.get<{ authUrl: string }>(`${environment.apiUrl}/auth/login`, {
      params: returnUrl ? { returnUrl } : {}
    }).subscribe({
      next: (response) => {
        // Redirect to Blackbaud OAuth
        window.location.href = response.authUrl;
      },
      error: (error) => {
        console.error('Error initiating login:', error);
      }
    });
  }

  /**
   * Handles OAuth callback with authorization code
   */
  handleCallback(code: string, state?: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/callback`, {
      code,
      state
    }).pipe(
      tap(response => {
        // Store token and user info
        localStorage.setItem(this.tokenKey, response.accessToken);
        localStorage.setItem(this.userKey, JSON.stringify(response.user));
        this.currentUserSubject.next(response.user);
      }),
      catchError(error => {
        console.error('Authentication failed:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Gets current user from API
   */
  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${environment.apiUrl}/auth/me`).pipe(
      tap(user => {
        localStorage.setItem(this.userKey, JSON.stringify(user));
        this.currentUserSubject.next(user);
      })
    );
  }

  getCurrentUserId(): number | null {
    return this.currentUserValue?.id ?? null;
  }

  /**
   * Logs out the current user
   */
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUserSubject.next(null);
    this.router.navigate(['/']);
  }

  /**
   * Gets authorization headers for API requests
   */
  getAuthHeaders(): HttpHeaders {
    const token = this.token;
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }
}
