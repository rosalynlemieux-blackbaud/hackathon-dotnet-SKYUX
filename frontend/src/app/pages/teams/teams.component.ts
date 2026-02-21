import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { TeamService, Team, TeamMember } from '../../services/team.service';
import { HackathonService } from '../../services/hackathon.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-teams',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="teams-container">
      <div class="teams-header">
        <h1>Hackathon Teams</h1>
        <button class="btn-primary" (click)="showCreateForm = !showCreateForm">
          {{ showCreateForm ? '× Close' : '+ Create Team' }}
        </button>
      </div>

      <!-- Create Team Form -->
      <div *ngIf="showCreateForm" class="create-form">
        <h2>Create a New Team</h2>
        <form (ngSubmit)="createTeam()">
          <div class="form-group">
            <label for="teamName">Team Name *</label>
            <input 
              id="teamName"
              type="text"
              [(ngModel)]="newTeam.name"
              name="teamName"
              placeholder="Enter team name"
              class="form-input"
              required>
          </div>

          <div class="form-group">
            <label for="teamDesc">Description</label>
            <textarea
              id="teamDesc"
              [(ngModel)]="newTeam.description"
              name="teamDesc"
              placeholder="What's your team about?"
              class="form-textarea"
              rows="3"></textarea>
          </div>

          <div class="form-actions">
            <button type="submit" class="btn-submit" [disabled]="!newTeam.name || isCreating">
              {{ isCreating ? 'Creating...' : 'Create Team' }}
            </button>
            <button type="button" class="btn-cancel" (click)="showCreateForm = false">Cancel</button>
          </div>

          <div *ngIf="createError" class="error-message">{{ createError }}</div>
        </form>
      </div>

      <!-- Teams Grid -->
      <div *ngIf="!loading && teams.length > 0" class="teams-grid">
        <div *ngFor="let team of teams" class="team-card">
          <div class="team-header">
            <h3>{{ team.name }}</h3>
            <span class="member-count">👥 {{ team.teamMembers?.length || 0 }} members</span>
          </div>

          <p *ngIf="team.description" class="team-description">
            {{ team.description }}
          </p>

          <div class="team-leader">
            <span class="label">Leader:</span>
            <strong>{{ team.leader?.firstName }} {{ team.leader?.lastName }}</strong>
          </div>

          <div class="team-members">
            <span class="label">Members:</span>
            <div class="members-list">
              <div *ngFor="let member of team.teamMembers" class="member-item">
                {{ member.user?.firstName }} {{ member.user?.lastName }}
              </div>
            </div>
          </div>

          <div class="team-ideas">
            <span class="label">Ideas Submitted:</span>
            <strong>{{ team.ideas?.length || 0 }}</strong>
          </div>

          <div class="team-actions">
            <button 
              *ngIf="isTeamLeader(team)"
              class="btn-action"
              (click)="editTeam(team)">
              Edit Team
            </button>
            <button 
              *ngIf="isTeamLeader(team)"
              class="btn-action btn-danger"
              (click)="deleteTeam(team.id)">
              Delete
            </button>
            <button 
              *ngIf="!isTeamLeader(team) && isTeamMember(team)"
              class="btn-action btn-secondary"
              (click)="leaveTeam(team.id)">
              Leave Team
            </button>
            <button 
              *ngIf="!isTeamMember(team)"
              class="btn-action"
              (click)="joinTeam(team.id)">
              Join Team
            </button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div *ngIf="!loading && teams.length === 0" class="empty-state">
        <p>No teams yet. Be the first to create one!</p>
      </div>

      <!-- Loading State -->
      <div *ngIf="loading" class="loading">
        <p>Loading teams...</p>
      </div>
    </div>
  `,
  styles: [`
    .teams-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .teams-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    .teams-header h1 {
      font-size: 2rem;
      margin: 0;
    }

    .btn-primary {
      background: #0066cc;
      color: white;
      border: none;
      padding: 0.75rem 1.5rem;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
    }

    .btn-primary:hover {
      background: #0052a3;
    }

    .create-form {
      background: #f5f5f5;
      padding: 2rem;
      border-radius: 8px;
      margin-bottom: 2rem;
      max-width: 500px;
    }

    .create-form h2 {
      margin-top: 0;
      font-size: 1.5rem;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    .form-group label {
      display: block;
      font-weight: 600;
      margin-bottom: 0.5rem;
    }

    .form-input, .form-textarea {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-family: inherit;
      font-size: 1rem;
      box-sizing: border-box;
    }

    .form-input:focus, .form-textarea:focus {
      outline: none;
      border-color: #0066cc;
      box-shadow: 0 0 0 3px rgba(0, 102, 204, 0.1);
    }

    .form-actions {
      display: flex;
      gap: 1rem;
      margin-top: 1rem;
    }

    .btn-submit {
      background: #4caf50;
      color: white;
      border: none;
      padding: 0.75rem 1.5rem;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      flex: 1;
    }

    .btn-submit:hover:not(:disabled) {
      background: #388e3c;
    }

    .btn-submit:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-cancel {
      background: white;
      color: #666;
      border: 1px solid #ddd;
      padding: 0.75rem 1.5rem;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      flex: 1;
    }

    .error-message {
      background: #ffebee;
      color: #c62828;
      padding: 0.75rem;
      border-radius: 4px;
      margin-top: 1rem;
      font-size: 0.875rem;
    }

    .teams-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 1.5rem;
    }

    .team-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 1.5rem;
      transition: all 0.3s ease;
    }

    .team-card:hover {
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
      transform: translateY(-2px);
    }

    .team-header {
      display: flex;
      justify-content: space-between;
      align-items: start;
      margin-bottom: 1rem;
    }

    .team-header h3 {
      margin: 0;
      font-size: 1.25rem;
      flex: 1;
    }

    .member-count {
      color: #666;
      font-size: 0.875rem;
      background: #f5f5f5;
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
    }

    .team-description {
      color: #666;
      margin: 0 0 1rem 0;
      line-height: 1.5;
      font-size: 0.95rem;
    }

    .label {
      display: block;
      font-weight: 600;
      color: #333;
      font-size: 0.875rem;
      margin-bottom: 0.25rem;
    }

    .team-leader, .team-members, .team-ideas {
      margin-bottom: 1rem;
    }

    .members-list {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .member-item {
      background: #e3f2fd;
      color: #0066cc;
      padding: 0.25rem 0.75rem;
      border-radius: 15px;
      font-size: 0.85rem;
    }

    .team-actions {
      display: flex;
      gap: 0.5rem;
      margin-top: 1.5rem;
      flex-wrap: wrap;
    }

    .btn-action {
      flex: 1;
      min-width: 100px;
      padding: 0.5rem 1rem;
      background: white;
      color: #0066cc;
      border: 1px solid #0066cc;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      font-size: 0.875rem;
      transition: all 0.3s;
    }

    .btn-action:hover {
      background: #f0f0f0;
    }

    .btn-action.btn-danger {
      color: #f44336;
      border-color: #f44336;
    }

    .btn-action.btn-danger:hover {
      background: #ffebee;
    }

    .btn-action.btn-secondary {
      color: #666;
      border-color: #ddd;
    }

    .btn-action.btn-secondary:hover {
      background: #f5f5f5;
    }

    .empty-state {
      text-align: center;
      padding: 3rem;
      color: #999;
      background: #f5f5f5;
      border-radius: 4px;
    }

    .loading {
      text-align: center;
      padding: 2rem;
      color: #666;
    }

    @media (max-width: 768px) {
      .teams-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class TeamsComponent implements OnInit, OnDestroy {
  teams: Team[] = [];
  loading = true;
  showCreateForm = false;
  isCreating = false;
  createError = '';
  newTeam = { name: '', description: '' };
  currentUserId = 0;
  currentHackathonId = 0;

  private destroy$ = new Subject<void>();

  constructor(
    private teamService: TeamService,
    private hackathonService: HackathonService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.getCurrentUserId() || 0;
    this.loadTeams();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadTeams(): void {
    this.loading = true;
    this.hackathonService.getCurrentHackathon()
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        hackathon => {
          this.currentHackathonId = hackathon.id;
          this.teamService.getTeams(hackathon.id)
            .pipe(takeUntil(this.destroy$))
            .subscribe(
              teams => {
                this.teams = teams;
                this.loading = false;
              },
              error => {
                console.error('Error loading teams:', error);
                this.loading = false;
              }
            );
        }
      );
  }

  createTeam(): void {
    if (!this.newTeam.name.trim()) return;

    this.isCreating = true;
    this.createError = '';

    const hackathonId = this.currentHackathonId;
    if (!hackathonId) {
      this.createError = 'Unable to determine current hackathon.';
      this.isCreating = false;
      return;
    }

    const team: Partial<Team> = {
      name: this.newTeam.name,
      description: this.newTeam.description,
      hackathonId
    };

    this.teamService.createTeam(team)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        () => {
          this.newTeam = { name: '', description: '' };
          this.showCreateForm = false;
          this.isCreating = false;
          this.loadTeams();
        },
        error => {
          this.createError = 'Error creating team. Please try again.';
          this.isCreating = false;
        }
      );
  }

  isTeamLeader(team: Team): boolean {
    return team.leaderId === this.currentUserId;
  }

  isTeamMember(team: Team): boolean {
    return team.teamMembers?.some(m => m.userId === this.currentUserId) || false;
  }

  joinTeam(teamId: number): void {
    this.teamService.addMember(teamId, this.currentUserId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        () => this.loadTeams()
      );
  }

  leaveTeam(teamId: number): void {
    if (confirm('Leave this team?')) {
      this.teamService.removeMember(teamId, this.currentUserId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(
          () => this.loadTeams()
        );
    }
  }

  editTeam(team: Team): void {
    // Navigate to edit page
    console.log('Edit team:', team);
  }

  deleteTeam(teamId: number): void {
    if (confirm('Delete this team permanently?')) {
      this.teamService.deleteTeam(teamId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(
          () => this.loadTeams()
        );
    }
  }
}
