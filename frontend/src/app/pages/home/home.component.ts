import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HackathonService } from '../../services/hackathon.service';
import { Hackathon } from '../../models/models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="home-container">
      <div class="hero-section" *ngIf="currentHackathon">
        <div class="hero-content">
          <h1 class="hero-title">{{ currentHackathon.name }}</h1>
          <p class="hero-description">{{ currentHackathon.description }}</p>
          <div class="hero-status">
            <span class="status-badge" [class]="'status-' + currentHackathon.status">
              {{ getStatusLabel(currentHackathon.status) }}
            </span>
          </div>
          <div class="hero-actions">
            <button class="btn btn-primary" (click)="submitIdea()">Submit Your Idea</button>
            <button class="btn btn-secondary" (click)="viewIdeas()">View All Ideas</button>
          </div>
        </div>
      </div>

      <div class="info-grid" *ngIf="currentHackathon">
        <!-- Tracks -->
        <div class="info-card">
          <h2 class="card-title">Tracks</h2>
          <div class="track-list">
            <div class="track-item" *ngFor="let track of currentHackathon.tracks">
              <div class="track-color" [style.background-color]="track.color || '#00b4d8'"></div>
              <div class="track-info">
                <h3>{{ track.name }}</h3>
                <p *ngIf="track.description">{{ track.description }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Awards -->
        <div class="info-card">
          <h2 class="card-title">Awards</h2>
          <div class="award-list">
            <div class="award-item" *ngFor="let award of currentHackathon.awards">
              <div class="award-icon">{{ award.icon || '🏆' }}</div>
              <div class="award-info">
                <h3>{{ award.name }}</h3>
                <p *ngIf="award.description">{{ award.description }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Timeline -->
        <div class="info-card timeline-card">
          <h2 class="card-title">Timeline</h2>
          <div class="timeline">
            <div class="timeline-item" *ngFor="let milestone of currentHackathon.milestones" 
                 [class.completed]="milestone.isComplete">
              <div class="timeline-marker"></div>
              <div class="timeline-content">
                <h3>{{ milestone.name }}</h3>
                <p class="timeline-date">{{ milestone.dueDate | date:'MMM d, yyyy' }}</p>
                <p *ngIf="milestone.description">{{ milestone.description }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Judging Criteria -->
        <div class="info-card">
          <h2 class="card-title">Judging Criteria</h2>
          <div class="criteria-list">
            <div class="criteria-item" *ngFor="let criterion of currentHackathon.judgingCriteria">
              <div class="criteria-header">
                <h3>{{ criterion.name }}</h3>
                <span class="criteria-weight" *ngIf="criterion.weight !== 1">
                  {{ criterion.weight }}x weight
                </span>
              </div>
              <p *ngIf="criterion.description">{{ criterion.description }}</p>
              <div class="criteria-score">Max Score: {{ criterion.maxScore }}</div>
            </div>
          </div>
        </div>
      </div>

      <div class="loading-container" *ngIf="loading">
        <p>Loading hackathon information...</p>
      </div>

      <div class="error-container" *ngIf="error">
        <h3 class="error-title">Error Loading Hackathon</h3>
        <p>{{ error }}</p>
      </div>
    </div>
  `,
  styles: [`
    .home-container {
      min-height: calc(100vh - 200px);
    }

    .hero-section {
      background: linear-gradient(135deg, #00b4d8 0%, #0077b6 100%);
      color: white;
      padding: 80px 40px;
      border-radius: 12px;
      margin-bottom: 40px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }

    .hero-content {
      max-width: 800px;
      margin: 0 auto;
      text-align: center;
    }

    .hero-title {
      font-size: 48px;
      font-weight: 700;
      margin-bottom: 20px;
    }

    .hero-description {
      font-size: 20px;
      margin-bottom: 30px;
      opacity: 0.95;
    }

    .hero-status {
      margin-bottom: 30px;
    }

    .hero-actions {
      display: flex;
      gap: 16px;
      justify-content: center;
    }

    .btn {
      padding: 12px 32px;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
    }

    .btn-primary {
      background: white;
      color: #00b4d8;
    }

    .btn-primary:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(255, 255, 255, 0.3);
    }

    .btn-secondary {
      background: transparent;
      color: white;
      border: 2px solid white;
    }

    .btn-secondary:hover {
      background: rgba(255, 255, 255, 0.1);
    }

    .info-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(500px, 1fr));
      gap: 30px;
    }

    .info-card {
      background: white;
      padding: 30px;
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .card-title {
      font-size: 24px;
      font-weight: 700;
      margin-bottom: 20px;
      color: #212529;
    }

    .track-list,
    .award-list,
    .criteria-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .track-item,
    .award-item {
      display: flex;
      gap: 16px;
      padding: 16px;
      background: #f8f9fa;
      border-radius: 8px;
    }

    .track-color {
      width: 4px;
      border-radius: 2px;
      flex-shrink: 0;
    }

    .track-info h3,
    .award-info h3,
    .criteria-header h3 {
      font-size: 18px;
      font-weight: 600;
      margin: 0 0 8px 0;
      color: #212529;
    }

    .track-info p,
    .award-info p {
      margin: 0;
      color: #6c757d;
      font-size: 14px;
    }

    .award-icon {
      font-size: 32px;
      flex-shrink: 0;
    }

    .timeline {
      position: relative;
      padding-left: 30px;
    }

    .timeline::before {
      content: '';
      position: absolute;
      left: 8px;
      top: 0;
      bottom: 0;
      width: 2px;
      background: #dee2e6;
    }

    .timeline-item {
      position: relative;
      padding-bottom: 30px;
    }

    .timeline-marker {
      position: absolute;
      left: -26px;
      top: 4px;
      width: 16px;
      height: 16px;
      border-radius: 50%;
      background: white;
      border: 3px solid #00b4d8;
    }

    .timeline-item.completed .timeline-marker {
      background: #06d6a0;
      border-color: #06d6a0;
    }

    .timeline-content h3 {
      font-size: 16px;
      font-weight: 600;
      margin: 0 0 4px 0;
    }

    .timeline-date {
      font-size: 14px;
      color: #6c757d;
      margin: 0 0 8px 0;
    }

    .criteria-item {
      padding: 16px;
      background: #f8f9fa;
      border-radius: 8px;
    }

    .criteria-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
    }

    .criteria-weight {
      background: #00b4d8;
      color: white;
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;
    }

    .criteria-score {
      margin-top: 8px;
      font-size: 14px;
      color: #6c757d;
      font-weight: 500;
    }

    @media (max-width: 768px) {
      .hero-title {
        font-size: 32px;
      }

      .hero-description {
        font-size: 16px;
      }

      .hero-actions {
        flex-direction: column;
      }

      .info-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class HomeComponent implements OnInit {
  currentHackathon: Hackathon | null = null;
  loading = true;
  error: string | null = null;

  constructor(
    private hackathonService: HackathonService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCurrentHackathon();
  }

  loadCurrentHackathon(): void {
    this.hackathonService.getCurrentHackathon().subscribe({
      next: (hackathon) => {
        this.currentHackathon = hackathon;
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load hackathon information';
        this.loading = false;
      }
    });
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      upcoming: 'Coming Soon',
      active: 'Active Now',
      judging: 'Judging Phase',
      completed: 'Completed'
    };
    return labels[status] || status;
  }

  submitIdea(): void {
    this.router.navigate(['/ideas/new']);
  }

  viewIdeas(): void {
    this.router.navigate(['/ideas']);
  }
}
