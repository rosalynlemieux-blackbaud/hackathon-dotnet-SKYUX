import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { AnalyticsService } from '../../services/analytics.service';
import { AdminService } from '../../services/admin.service';
import { HackathonService } from '../../services/hackathon.service';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="admin-container">
      <h1>Admin Dashboard</h1>

      <!-- Tab Navigation -->
      <div class="tab-navigation">
        <button 
          *ngFor="let tab of tabs"
          [class.active]="activeTab === tab"
          (click)="activeTab = tab"
          class="tab-button">
          {{ tab }}
        </button>
      </div>

      <!-- Overview Tab -->
      <div *ngIf="activeTab === 'Overview'" class="tab-content">
        <div *ngIf="loading" class="loading">Loading analytics...</div>
        <div *ngIf="!loading && dashboardSummary" class="overview">
          <!-- Key Metrics -->
          <div class="metrics-grid">
            <div class="metric-card">
              <div class="metric-value">{{ dashboardSummary.hackathon.totalParticipants }}</div>
              <div class="metric-label">Total Participants</div>
            </div>
            <div class="metric-card">
              <div class="metric-value">{{ dashboardSummary.hackathon.totalIdeas }}</div>
              <div class="metric-label">Total Ideas</div>
            </div>
            <div class="metric-card">
              <div class="metric-value">{{ dashboardSummary.submissions.submissionRate | number:'0.0' }}%</div>
              <div class="metric-label">Submission Rate</div>
            </div>
            <div class="metric-card">
              <div class="metric-value">{{ dashboardSummary.judging.totalJudges }}</div>
              <div class="metric-label">Judges Active</div>
            </div>
            <div class="metric-card">
              <div class="metric-value">{{ dashboardSummary.teams.totalTeams }}</div>
              <div class="metric-label">Teams Formed</div>
            </div>
            <div class="metric-card">
              <div class="metric-value">{{ dashboardSummary.judging.averageScoreAcrossAll | number:'0.0' }}/10</div>
              <div class="metric-label">Average Score</div>
            </div>
          </div>

          <!-- Top 5 Ideas -->
          <section class="top-ideas">
            <h2>Top 5 Ideas by Score</h2>
            <table class="ranking-table">
              <thead>
                <tr>
                  <th>Rank</th>
                  <th>Title</th>
                  <th>Author</th>
                  <th>Score</th>
                  <th>Ratings</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let idea of dashboardSummary.topIdeas">
                  <td class="rank">{{ idea.rank }}</td>
                  <td>{{ idea.title }}</td>
                  <td>{{ idea.author }}</td>
                  <td class="score">{{ idea.averageScore | number:'0.0' }}/10</td>
                  <td>{{ idea.ratingCount }}</td>
                </tr>
              </tbody>
            </table>
          </section>

          <!-- Detailed Stats Sections -->
          <div class="stats-grid">
            <!-- Submissions -->
            <section class="stats-card">
              <h3>Submission Statistics</h3>
              <dl>
                <dt>Total Submissions</dt>
                <dd>{{ dashboardSummary.submissions.totalSubmissions }}</dd>
                <dt>Individual Submissions</dt>
                <dd>{{ dashboardSummary.submissions.individualSubmissions }}</dd>
                <dt>Team Submissions</dt>
                <dd>{{ dashboardSummary.submissions.teamSubmissions }}</dd>
                <dt>Deadline Met</dt>
                <dd>{{ dashboardSummary.submissions.submissionDeadlineMetCount }}</dd>
              </dl>
            </section>

            <!-- Judging -->
            <section class="stats-card">
              <h3>Judging Statistics</h3>
              <dl>
                <dt>Active Judges</dt>
                <dd>{{ dashboardSummary.judging.totalJudges }}</dd>
                <dt>Ideas Being Judged</dt>
                <dd>{{ dashboardSummary.judging.ideasBeingJudged }}</dd>
                <dt>Total Ratings Submitted</dt>
                <dd>{{ dashboardSummary.judging.totalRatingsSubmitted }}</dd>
                <dt>Avg Ratings/Judge</dt>
                <dd>{{ dashboardSummary.judging.ratingsPerJudge | number:'0.0' }}</dd>
              </dl>
            </section>

            <!-- Teams -->
            <section class="stats-card">
              <h3>Team Statistics</h3>
              <dl>
                <dt>Total Teams</dt>
                <dd>{{ dashboardSummary.teams.totalTeams }}</dd>
                <dt>Total Members</dt>
                <dd>{{ dashboardSummary.teams.totalTeamMembers }}</dd>
                <dt>Teams with Ideas</dt>
                <dd>{{ dashboardSummary.teams.teamsWithIdeas }}</dd>
                <dt>Avg Team Size</dt>
                <dd>{{ dashboardSummary.teams.averageMembersPerTeam | number:'0.0' }}</dd>
              </dl>
            </section>
          </div>
        </div>
      </div>

      <!-- Management Tab -->
      <div *ngIf="activeTab === 'Management'" class="tab-content">
        <div class="management-section">
          <h2>Hackathon Management</h2>
          <div *ngIf="!editingHackathon" class="view-mode">
            <div class="hackathon-details">
              <p><strong>Name:</strong> {{ currentHackathon?.name }}</p>
              <p><strong>Start:</strong> {{ formatDate(currentHackathon?.startDate) }}</p>
              <p><strong>End:</strong> {{ formatDate(currentHackathon?.endDate) }}</p>
              <p><strong>Submission Deadline:</strong> {{ formatDate(currentHackathon?.submissionDeadline) }}</p>
            </div>
            <button (click)="editingHackathon = true" class="btn-edit">Edit Hackathon</button>
          </div>

          <div *ngIf="editingHackathon" class="edit-mode">
            <form (ngSubmit)="saveHackathon()">
              <div class="form-group">
                <label>Name</label>
                <input [(ngModel)]="currentHackathon.name" name="name" class="form-input">
              </div>
              <div class="form-row">
                <div class="form-group">
                  <label>Start Date</label>
                  <input type="datetime-local" [(ngModel)]="currentHackathon.startDate" name="startDate" class="form-input">
                </div>
                <div class="form-group">
                  <label>End Date</label>
                  <input type="datetime-local" [(ngModel)]="currentHackathon.endDate" name="endDate" class="form-input">
                </div>
              </div>
              <div class="form-actions">
                <button type="submit" class="btn-save">Save</button>
                <button type="button" (click)="editingHackathon = false" class="btn-cancel">Cancel</button>
              </div>
            </form>
          </div>
        </div>

        <!-- Judging Criteria -->
        <div class="management-section">
          <h2>Judging Criteria</h2>
          <table class="management-table" *ngIf="judgingCriteria.length > 0">
            <thead>
              <tr>
                <th>Name</th>
                <th>Weight</th>
                <th>Max Score</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let criterion of judgingCriteria">
                <td>{{ criterion.name }}</td>
                <td>{{ (criterion.weight * 100) | number:'0.0' }}%</td>
                <td>{{ criterion.maxScore }}</td>
                <td>
                  <button (click)="deleteCriterion(criterion.id)" class="btn-delete-small">Delete</button>
                </td>
              </tr>
            </tbody>
          </table>

          <div class="add-criterion">
            <h3>Add New Criterion</h3>
            <form (ngSubmit)="addCriterion()">
              <div class="form-row">
                <input 
                  [(ngModel)]="newCriterion.name" 
                  name="critName"
                  placeholder="Criterion name"
                  class="form-input">
                <input 
                  type="number"
                  [(ngModel)]="newCriterion.weight" 
                  name="critWeight"
                  placeholder="Weight (0-1)"
                  step="0.05"
                  class="form-input">
              </div>
              <button type="submit" [disabled]="!newCriterion.name" class="btn-add">Add Criterion</button>
            </form>
          </div>
        </div>
      </div>

      <!-- Users Tab -->
      <div *ngIf="activeTab === 'Users'" class="tab-content">
        <h2>Users & Roles</h2>
        <table class="users-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Current Role</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let user of hackathonUsers">
              <td>{{ user.firstName }} {{ user.lastName }}</td>
              <td>{{ user.email }}</td>
              <td>
                <select [(ngModel)]="user.role" (change)="updateUserRole(user)" [ngModelOptions]="{updateOn: 'blur'}" class="role-select">
                  <option value="participant">Participant</option>
                  <option value="judge">Judge</option>
                  <option value="admin">Admin</option>
                </select>
              </td>
              <td>{{ user.role }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Export Tab -->
      <div *ngIf="activeTab === 'Export'" class="tab-content">
        <h2>Data Export</h2>
        <div class="export-options">
          <div class="export-card">
            <h3>Export All Ideas</h3>
            <p>Download a CSV file containing all submitted ideas with ratings and metadata.</p>
            <button (click)="exportData()" class="btn-primary">Download CSV</button>
          </div>

          <div class="export-card">
            <h3>Announce Winners</h3>
            <p>Mark this hackathon as concluded and announce the winners.</p>
            <button (click)="announceWinners()" class="btn-primary">Announce Winners</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .admin-container {
      padding: 2rem;
      max-width: 1400px;
      margin: 0 auto;
    }

    .admin-container h1 {
      margin-top: 0;
      margin-bottom: 2rem;
      font-size: 2rem;
    }

    .tab-navigation {
      display: flex;
      gap: 1rem;
      margin-bottom: 2rem;
      border-bottom: 2px solid #eee;
    }

    .tab-button {
      padding: 1rem 1.5rem;
      background: none;
      border: none;
      border-bottom: 3px solid transparent;
      cursor: pointer;
      font-weight: 600;
      font-size: 1rem;
      color: #666;
      transition: all 0.3s;
    }

    .tab-button.active {
      color: #0066cc;
      border-bottom-color: #0066cc;
    }

    .tab-button:hover {
      color: #0066cc;
    }

    .tab-content {
      animation: fadeIn 0.3s ease-in;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    .loading {
      text-align: center;
      padding: 2rem;
      color: #666;
    }

    /* Metrics Grid */
    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1.5rem;
      margin-bottom: 3rem;
    }

    .metric-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 1.5rem;
      text-align: center;
    }

    .metric-value {
      font-size: 2.5rem;
      font-weight: bold;
      color: #0066cc;
      margin-bottom: 0.5rem;
    }

    .metric-label {
      color: #666;
      font-size: 0.95rem;
    }

    /* Ranking Table */
    .ranking-table, .management-table, .users-table {
      width: 100%;
      border-collapse: collapse;
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      margin-bottom: 2rem;
    }

    .ranking-table thead, .management-table thead, .users-table thead {
      background: #f5f5f5;
      font-weight: 600;
    }

    .ranking-table th, .ranking-table td {
      padding: 1rem;
      text-align: left;
      border-bottom: 1px solid #eee;
    }

    .ranking-table tbody tr:hover {
      background: #f9f9f9;
    }

    .rank {
      font-weight: 600;
      font-size: 1.1rem;
    }

    .score {
      font-weight: 600;
      color: #0066cc;
    }

    /* Stats Grid */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 2rem;
      margin-top: 3rem;
    }

    .stats-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 1.5rem;
    }

    .stats-card h3 {
      margin-top: 0;
      margin-bottom: 1rem;
      border-bottom: 2px solid #0066cc;
      padding-bottom: 0.5rem;
    }

    .stats-card dl {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    .stats-card dt {
      font-weight: 600;
      color: #666;
      font-size: 0.85rem;
    }

    .stats-card dd {
      font-size: 1.5rem;
      font-weight: bold;
      color: #0066cc;
      margin: 0;
    }

    /* Management Section */
    .management-section {
      margin-bottom: 3rem;
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 2rem;
    }

    .management-section h2 {
      margin-top: 0;
      margin-bottom: 1.5rem;
      font-size: 1.5rem;
    }

    .view-mode {
      margin-bottom: 1.5rem;
    }

    .hackathon-details p {
      margin: 0.5rem 0;
      color: #666;
    }

    .edit-mode form {
      max-width: 500px;
    }

    .form-group {
      margin-bottom: 1rem;
    }

    .form-group label {
      display: block;
      font-weight: 600;
      margin-bottom: 0.5rem;
    }

    .form-input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-family: inherit;
      box-sizing: border-box;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    .form-actions {
      display: flex;
      gap: 1rem;
      margin-top: 1rem;
    }

    .btn-save, .btn-cancel, .btn-edit, .btn-add, .btn-primary, .btn-delete-small {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      transition: all 0.3s;
    }

    .btn-save {
      background: #4caf50;
      color: white;
    }

    .btn-save:hover {
      background: #388e3c;
    }

    .btn-cancel {
      background: #f5f5f5;
      color: #666;
      border: 1px solid #ddd;
    }

    .btn-edit {
      background: #0066cc;
      color: white;
    }

    .btn-edit:hover {
      background: #0052a3;
    }

    .btn-primary {
      background: #0066cc;
      color: white;
    }

    .btn-primary:hover {
      background: #0052a3;
    }

    .btn-delete-small {
      background: #f44336;
      color: white;
      padding: 0.5rem 1rem;
      font-size: 0.85rem;
    }

    .btn-delete-small:hover {
      background: #da190b;
    }

    .btn-add {
      background: #4caf50;
      color: white;
    }

    .btn-add:hover:not(:disabled) {
      background: #388e3c;
    }

    .btn-add:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .add-criterion {
      background: #f5f5f5;
      padding: 1.5rem;
      border-radius: 4px;
      max-width: 500px;
    }

    .add-criterion h3 {
      margin-top: 0;
      margin-bottom: 1rem;
    }

    .add-criterion .form-row {
      grid-template-columns: 2fr 1fr;
      margin-bottom: 1rem;
    }

    .role-select {
      padding: 0.5rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-family: inherit;
    }

    .export-options {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 2rem;
    }

    .export-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 2rem;
      text-align: center;
    }

    .export-card h3 {
      margin-top: 0;
      margin-bottom: 1rem;
    }

    .export-card p {
      color: #666;
      margin-bottom: 1.5rem;
    }

    .top-ideas {
      margin-bottom: 3rem;
    }

    .top-ideas h2 {
      margin-bottom: 1.5rem;
      font-size: 1.5rem;
    }

    @media (max-width: 768px) {
      .metrics-grid {
        grid-template-columns: repeat(2, 1fr);
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .stats-grid {
        grid-template-columns: 1fr;
      }

      .tab-navigation {
        flex-wrap: wrap;
      }
    }
  `]
})
export class AdminComponent implements OnInit, OnDestroy {
  activeTab = 'Overview';
  tabs = ['Overview', 'Management', 'Users', 'Export'];
  loading = true;
  
  dashboardSummary: any;
  currentHackathon: any;
  hackathonUsers: any[] = [];
  judgingCriteria: any[] = [];

  editingHackathon = false;
  newCriterion = { name: '', weight: 0.25 };

  private destroy$ = new Subject<void>();

  constructor(
    private analyticsService: AnalyticsService,
    private adminService: AdminService,
    private hackathonService: HackathonService
  ) {}

  ngOnInit(): void {
    this.hackathonService.getCurrentHackathon()
      .pipe(takeUntil(this.destroy$))
      .subscribe(hackathon => {
        this.currentHackathon = { ...hackathon };
        this.loadAnalytics(hackathon.id);
        this.loadUsers(hackathon.id);
        this.loadJudgingCriteria(hackathon.id);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadAnalytics(hackathonId: number): void {
    this.analyticsService.getDashboardSummary(hackathonId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        summary => {
          this.dashboardSummary = summary;
          this.loading = false;
        },
        error => {
          console.error('Error loading analytics:', error);
          this.loading = false;
        }
      );
  }

  loadUsers(hackathonId: number): void {
    this.adminService.getHackathonUsers(hackathonId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        users => {
          this.hackathonUsers = users;
        }
      );
  }

  loadJudgingCriteria(hackathonId: number): void {
    this.adminService.getJudgingCriteria(hackathonId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        criteria => {
          this.judgingCriteria = criteria;
        }
      );
  }

  saveHackathon(): void {
    this.adminService.updateHackathon(this.currentHackathon.id, this.currentHackathon)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.editingHackathon = false;
        alert('Hackathon updated successfully');
      });
  }

  addCriterion(): void {
    if (!this.newCriterion.name) return;

    this.adminService.addJudgingCriterion(this.currentHackathon.id, this.newCriterion)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.newCriterion = { name: '', weight: 0.25 };
        this.loadJudgingCriteria(this.currentHackathon.id);
      });
  }

  deleteCriterion(id: number): void {
    if (confirm('Delete this criterion?')) {
      this.adminService.deleteJudgingCriterion(id)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadJudgingCriteria(this.currentHackathon.id);
        });
    }
  }

  updateUserRole(user: any): void {
    this.adminService.updateUserRole(user.id, user.role)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        () => {
          alert('User role updated');
        }
      );
  }

  exportData(): void {
    this.adminService.exportHackathonData(this.currentHackathon.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {
        this.downloadCSV(data);
      });
  }

  announceWinners(): void {
    if (confirm('Announce winners for this hackathon?')) {
      this.adminService.announceWinners(this.currentHackathon.id, [])
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          alert('Winners announced!');
        });
    }
  }

  downloadCSV(data: any[]): void {
    const csv = this.convertToCSV(data);
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `hackathon-ideas-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
  }

  convertToCSV(data: any[]): string {
    const headers = Object.keys(data[0] || {});
    const csvContent = [
      headers.join(','),
      ...data.map(row =>
        headers.map(header =>
          JSON.stringify(row[header] || '')
        ).join(',')
      )
    ].join('\n');

    return csvContent;
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
