import { Component, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { NotificationService } from './services/notification.service';
import { NotificationsComponent } from './components/notifications/notifications.component';
import { User } from './models/models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, NotificationsComponent],
  template: `
    <div class="app-container">
      <!-- Header -->
      <header class="app-header">
        <div class="header-content">
          <div class="header-left">
            <h1 class="app-title" (click)="navigateHome()">
              <span class="logo-icon">⚡</span>
              Off the Grid
            </h1>
            <nav class="main-nav" *ngIf="currentUser">
              <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">Home</a>
              <a routerLink="/ideas" routerLinkActive="active">Ideas</a>
              <a routerLink="/teams" routerLinkActive="active">Teams</a>
              <a routerLink="/judging" routerLinkActive="active" *ngIf="isJudge">Judging</a>
              <a routerLink="/admin" routerLinkActive="active" *ngIf="isAdmin">Admin</a>
            </nav>
          </div>
          
          <div class="header-right">
            <div class="user-menu" *ngIf="currentUser; else loginButton">
              <div class="user-info">
                <img *ngIf="currentUser.avatarUrl" [src]="currentUser.avatarUrl" alt="Avatar" class="user-avatar">
                <div *ngIf="!currentUser.avatarUrl" class="user-avatar-placeholder">
                  {{ currentUser.firstName[0] }}{{ currentUser.lastName[0] }}
                </div>
                <span class="user-name">{{ currentUser.firstName }} {{ currentUser.lastName }}</span>
              </div>
              <button class="btn-logout" (click)="logout()">Logout</button>
            </div>
            <ng-template #loginButton>
              <button class="btn-login" (click)="login()">Login with Blackbaud</button>
            </ng-template>
          </div>
        </div>
      </header>

      <!-- Main Content -->
      <main class="app-main">
        <router-outlet></router-outlet>
      </main>

      <!-- Notifications -->
      <app-notifications></app-notifications>

      <!-- Footer -->
      <footer class="app-footer">
        <div class="footer-content">
          <p>&copy; 2025 Blackbaud. All rights reserved.</p>
          <div class="footer-links">
            <a href="/faq">FAQ</a>
            <a href="/rules">Rules</a>
            <a href="https://developer.blackbaud.com" target="_blank">Developer Portal</a>
          </div>
        </div>
      </footer>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
    }

    .app-header {
      background: #fff;
      border-bottom: 1px solid #dee2e6;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .header-content {
      max-width: 1400px;
      margin: 0 auto;
      padding: 0 20px;
      height: 64px;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .header-left {
      display: flex;
      align-items: center;
      gap: 40px;
    }

    .app-title {
      margin: 0;
      font-size: 24px;
      font-weight: 700;
      color: #00b4d8;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .logo-icon {
      font-size: 28px;
    }

    .main-nav {
      display: flex;
      gap: 24px;
    }

    .main-nav a {
      color: #495057;
      text-decoration: none;
      font-weight: 500;
      padding: 8px 12px;
      border-radius: 4px;
      transition: all 0.2s;
    }

    .main-nav a:hover {
      background: #f8f9fa;
      color: #00b4d8;
    }

    .main-nav a.active {
      color: #00b4d8;
      background: #e7f6f8;
    }

    .header-right {
      display: flex;
      align-items: center;
    }

    .user-menu {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .user-avatar,
    .user-avatar-placeholder {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      object-fit: cover;
    }

    .user-avatar-placeholder {
      background: #00b4d8;
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 14px;
    }

    .user-name {
      font-weight: 500;
      color: #212529;
    }

    .btn-login,
    .btn-logout {
      padding: 8px 20px;
      border-radius: 6px;
      border: none;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }

    .btn-login {
      background: #00b4d8;
      color: white;
    }

    .btn-login:hover {
      background: #0096c7;
    }

    .btn-logout {
      background: transparent;
      color: #6c757d;
      border: 1px solid #dee2e6;
    }

    .btn-logout:hover {
      background: #f8f9fa;
      color: #495057;
    }

    .app-main {
      flex: 1;
      max-width: 1400px;
      width: 100%;
      margin: 0 auto;
      padding: 40px 20px;
    }

    .app-footer {
      background: #212529;
      color: #adb5bd;
      padding: 30px 20px;
      margin-top: 60px;
    }

    .footer-content {
      max-width: 1400px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .footer-links {
      display: flex;
      gap: 24px;
    }

    .footer-links a {
      color: #adb5bd;
      text-decoration: none;
      transition: color 0.2s;
    }

    .footer-links a:hover {
      color: #fff;
    }

    @media (max-width: 768px) {
      .header-content {
        flex-direction: column;
        height: auto;
        padding: 16px 20px;
      }

      .header-left {
        flex-direction: column;
        gap: 16px;
        align-items: flex-start;
        width: 100%;
      }

      .main-nav {
        gap: 12px;
      }

      .footer-content {
        flex-direction: column;
        gap: 16px;
        text-align: center;
      }

      .user-name {
        display: none;
      }
    }
  `]
})
export class AppComponent implements OnInit {
  currentUser: User | null = null;
  isAdmin = false;
  isJudge = false;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Initialize notifications
    this.notificationService.start().catch(err => {
      console.error('Failed to start notifications:', err);
    });

    // Subscribe to auth changes
    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
      this.isAdmin = this.authService.isAdmin();
      this.isJudge = this.authService.isJudge();
    });
  }

  login(): void {
    this.authService.login();
  }

  logout(): void {
    this.authService.logout();
  }

  navigateHome(): void {
    this.router.navigate(['/']);
  }
}
