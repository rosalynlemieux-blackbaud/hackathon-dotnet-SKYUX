import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="callback-container">
      <div class="callback-content">
        <div *ngIf="loading" class="loading-state">
          <div class="spinner"></div>
          <h2>Authenticating...</h2>
          <p>Please wait while we complete your login.</p>
        </div>

        <div *ngIf="error" class="error-state">
          <div class="error-icon">⚠️</div>
          <h2>Authentication Failed</h2>
          <p>{{ error }}</p>
          <button class="btn-retry" (click)="retry()">Try Again</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .callback-container {
      min-height: calc(100vh - 200px);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 40px 20px;
    }

    .callback-content {
      text-align: center;
      max-width: 500px;
    }

    .loading-state,
    .error-state {
      background: white;
      padding: 60px 40px;
      border-radius: 16px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }

    .spinner {
      width: 48px;
      height: 48px;
      border: 4px solid #e9ecef;
      border-top-color: #00b4d8;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 24px;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    h2 {
      font-size: 24px;
      font-weight: 600;
      margin: 0 0 12px 0;
      color: #212529;
    }

    p {
      color: #6c757d;
      margin: 0;
      font-size: 16px;
    }

    .error-icon {
      font-size: 48px;
      margin-bottom: 20px;
    }

    .btn-retry {
      margin-top: 24px;
      padding: 12px 32px;
      background: #00b4d8;
      color: white;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }

    .btn-retry:hover {
      background: #0096c7;
    }
  `]
})
export class AuthCallbackComponent implements OnInit {
  loading = true;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const code = params['code'];
      const state = params['state'];
      const error = params['error'];

      if (error) {
        this.loading = false;
        this.error = `Authentication error: ${error}`;
        return;
      }

      if (!code) {
        this.loading = false;
        this.error = 'No authorization code received';
        return;
      }

      // Handle authentication callback
      this.authService.handleCallback(code, state).subscribe({
        next: (response) => {
          // Navigate to return URL or home
          const returnUrl = state && state !== 'null' ? state : '/';
          this.router.navigate([returnUrl]);
        },
        error: (err) => {
          this.loading = false;
          this.error = err.error?.error || 'Authentication failed. Please try again.';
        }
      });
    });
  }

  retry(): void {
    this.router.navigate(['/login']);
  }
}
