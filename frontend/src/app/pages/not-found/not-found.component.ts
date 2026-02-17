import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="not-found-container">
      <div class="not-found-content">
        <h1 class="error-code">404</h1>
        <h2>Page Not Found</h2>
        <p>The page you're looking for doesn't exist or has been moved.</p>
        <a routerLink="/" class="btn-home">Go Home</a>
      </div>
    </div>
  `,
  styles: [`
    .not-found-container {
      min-height: calc(100vh - 200px);
      display: flex;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: 40px 20px;
    }

    .error-code {
      font-size: 120px;
      font-weight: 700;
      color: #00b4d8;
      margin: 0;
      line-height: 1;
    }

    h2 {
      font-size: 32px;
      font-weight: 600;
      color: #212529;
      margin: 20px 0 16px;
    }

    p {
      font-size: 18px;
      color: #6c757d;
      margin-bottom: 32px;
    }

    .btn-home {
      display: inline-block;
      padding: 12px 32px;
      background: #00b4d8;
      color: white;
      text-decoration: none;
      border-radius: 8px;
      font-weight: 600;
      transition: all 0.2s;
    }

    .btn-home:hover {
      background: #0096c7;
      transform: translateY(-2px);
    }
  `]
})
export class NotFoundComponent {}
