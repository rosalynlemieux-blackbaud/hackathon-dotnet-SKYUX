import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { TeamService, Team } from '../../services/team.service';

@Component({
  selector: 'app-team-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="team-detail-container" *ngIf="!loading && team">
      <button class="back-btn" [routerLink]="['/teams']">← Back to Teams</button>

      <div class="team-detail">
        <div class="team-main">
          <h1>{{ team.name }}</h1>
          <p *ngIf="team.description" class="description">{{ team.description }}</p>

          <section class="team-section">
            <h2>Team Leader</h2>
            <div class="leader-card">
              <strong>{{ team.leader?.firstName }} {{ team.leader?.lastName }}</strong>
              <p>{{ team.leader?.email }}</p>
            </div>
          </section>

          <section class="team-section">
            <h2>Team Members ({{ team.teamMembers?.length || 0 }})</h2>
            <div class="members-list">
              <div *ngFor="let member of team.teamMembers" class="member-card">
                <div class="member-name">{{ member.user?.firstName }} {{ member.user?.lastName }}</div>
                <div class="member-email">{{ member.user?.email }}</div>
                <div class="joined-date">Joined: {{ formatDate(member.joinedAt) }}</div>
              </div>
            </div>
          </section>

          <section class="team-section" *ngIf="team.ideas && team.ideas.length > 0">
            <h2>Ideas Submitted</h2>
            <div class="ideas-list">
              <a *ngFor="let idea of team.ideas" [routerLink]="['/ideas', idea.id]" class="idea-link">
                {{ idea.title }}
              </a>
            </div>
          </section>
        </div>

        <aside class="team-sidebar">
          <div class="info-card">
            <h3>Team Information</h3>
            <p><strong>Created:</strong> {{ formatDate(team.createdAt) }}</p>
            <p><strong>Members:</strong> {{ team.teamMembers?.length || 0 }}</p>
            <p><strong>Ideas:</strong> {{ team.ideas?.length || 0 }}</p>
          </div>
        </aside>
      </div>
    </div>

    <div *ngIf="loading" class="loading">Loading team details...</div>
  `,
  styles: [`
    .team-detail-container {
      padding: 2rem;
      max-width: 1000px;
      margin: 0 auto;
    }

    .back-btn {
      background: white;
      border: 1px solid #ddd;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
      color: #0066cc;
      margin-bottom: 1.5rem;
    }

    .team-detail {
      display: grid;
      grid-template-columns: 1fr 250px;
      gap: 2rem;
    }

    .team-main h1 {
      font-size: 2rem;
      margin: 0 0 0.5rem 0;
    }

    .description {
      color: #666;
      font-size: 1.05rem;
      margin: 0 0 2rem 0;
    }

    .team-section {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 1.5rem;
      margin-bottom: 2rem;
    }

    .team-section h2 {
      margin-top: 0;
      margin-bottom: 1.5rem;
      font-size: 1.25rem;
    }

    .leader-card {
      background: #f5f5f5;
      padding: 1rem;
      border-radius: 4px;
    }

    .members-list {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 1rem;
    }

    .member-card {
      background: #f5f5f5;
      padding: 1rem;
      border-radius: 4px;
      border-left: 3px solid #0066cc;
    }

    .member-name {
      font-weight: 600;
      margin-bottom: 0.25rem;
    }

    .member-email {
      color: #666;
      font-size: 0.9rem;
      margin-bottom: 0.5rem;
    }

    .joined-date {
      color: #999;
      font-size: 0.85rem;
    }

    .ideas-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .idea-link {
      display: block;
      padding: 0.75rem;
      background: #e3f2fd;
      color: #0066cc;
      text-decoration: none;
      border-radius: 4px;
      transition: all 0.3s;
    }

    .idea-link:hover {
      background: #bbdefb;
    }

    .team-sidebar {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .info-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 1.5rem;
    }

    .info-card h3 {
      margin-top: 0;
      margin-bottom: 1rem;
    }

    .info-card p {
      margin: 0.5rem 0;
      font-size: 0.9rem;
    }

    .loading {
      text-align: center;
      padding: 3rem;
      color: #666;
    }

    @media (max-width: 768px) {
      .team-detail {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class TeamDetailComponent implements OnInit, OnDestroy {
  team: Team | null = null;
  loading = true;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private teamService: TeamService
  ) {}

  ngOnInit(): void {
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.loadTeam(parseInt(params['id']));
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadTeam(id: number): void {
    this.teamService.getTeam(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        team => {
          this.team = team;
          this.loading = false;
        },
        error => {
          console.error('Error loading team:', error);
          this.loading = false;
        }
      );
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }
}
