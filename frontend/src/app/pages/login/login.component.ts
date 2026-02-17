import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="login-container">
      <div class="login-card">
        <div class="login-icon">⚡</div>
        <h1>Off the Grid</h1>
        <p class="subtitle">Hackathon Platform</p>
        <p class="description">
          Sign in with your Blackbaud account to participate in hackathons,
          submit ideas, join teams, and more.
        </p>
        <button class="btn-login" (click)="login()">
          <span class="blackbaud-logo">B</span>
          Sign in with Blackbaud
        </button>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: calc(100vh - 200px);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 40px 20px;
    }

    .login-card {
      background: white;
      padding: 60px 40px;
      border-radius: 16px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
      max-width: 450px;
      width: 100%;
      text-align: center;
    }

    .login-icon {
      font-size: 64px;
      margin-bottom: 20px;
    }

    h1 {
      font-size: 32px;
      font-weight: 700;
      color: #212529;
      margin: 0 0 8px 0;
    }

    .subtitle {
      font-size: 18px;
      color: #6c757d;
      margin: 0 0 24px 0;
    }

    .description {
      font-size: 16px;
      color: #6c757d;
      line-height: 1.6;
      margin-bottom: 32px;
    }

    .btn-login {
      width: 100%;
      padding: 16px;
      background: #00b4d8;
      color: white;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 12px;
      transition: all 0.2s;
    }

    .btn-login:hover {
      background: #0096c7;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 180, 216, 0.3);
    }

    .blackbaud-logo {
      width: 32px;
      height: 32px;
      background: white;
      color: #00b4d8;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 18px;
    }
  `]
})
export class LoginComponent {
  constructor(private authService: AuthService) {}

  login(): void {
    this.authService.login();
  }
}
