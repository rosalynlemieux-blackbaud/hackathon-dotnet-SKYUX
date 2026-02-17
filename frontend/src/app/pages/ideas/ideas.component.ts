import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { IdeaService } from '../../services/idea.service';
import { AuthService } from '../../services/auth.service';
import { HackathonService } from '../../services/hackathon.service';
import { Idea } from '../../models/models';

@Component({
  selector: 'app-ideas',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="ideas-container">
      <div class="ideas-header">
        <h1>Hackathon Ideas</h1>
        <button 
          *ngIf="isAuthenticated && !isJudge"
          class="btn-primary"
          [routerLink]="['/ideas/new']">
          + Submit New Idea
        </button>
      </div>

      <div class="ideas-filters">
        <div class="search-box">
          <input
            type="text"
            placeholder="Search ideas..."
            [(ngModel)]="searchQuery"
            (input)="onSearch()"
            class="search-input">
        </div>

        <div class="filter-group">
          <select [(ngModel)]="statusFilter" (change)="applyFilters()" class="filter-select">
            <option value="">All Statuses</option>
            <option value="draft">Draft</option>
            <option value="submitted">Submitted</option>
            <option value="judging">In Judging</option>
            <option value="complete">Complete</option>
          </select>

          <select [(ngModel)]="trackFilter" (change)="applyFilters()" class="filter-select">
            <option value="">All Tracks</option>
            <option *ngFor="let track of tracks" [value]="track.id">
              {{ track.name }}
            </option>
          </select>
        </div>
      </div>

      <div *ngIf="loading" class="loading">Loading ideas...</div>

      <div *ngIf="!loading && filteredIdeas.length === 0" class="empty-state">
        <p>No ideas found. {{ !isAuthenticated ? 'Sign in to submit an idea!' : 'Be the first to submit!' }}</p>
      </div>

      <div *ngIf="!loading && filteredIdeas.length > 0" class="ideas-grid">
        <div *ngFor="let idea of paginatedIdeas" class="idea-card" [routerLink]="['/ideas', idea.id]">
          <div class="idea-header">
            <h3>{{ idea.title }}</h3>
            <span [class]="'status-badge status-' + idea.status">
              {{ idea.status | uppercase }}
            </span>
          </div>

          <p class="idea-description">{{ idea.description | slice:0:100 }}...</p>

          <div class="idea-meta">
            <span class="track-badge" [style.background-color]="getTrackColor(idea.trackId)">
              {{ getTrackName(idea.trackId) }}
            </span>
            <span class="author">{{ idea.author?.firstName }} {{ idea.author?.lastName }}</span>
          </div>

          <div class="idea-stats">
            <span *ngIf="idea.submission?.teamId" class="stat">
              👥 Team
            </span>
            <span *ngIf="idea.awards && idea.awards.length > 0" class="stat">
              🏆 {{ idea.awards.length }} Award(s)
            </span>
            <span class="date">{{ formatDate(idea.submittedAt || idea.createdAt) }}</span>
          </div>
        </div>
      </div>

      <div *ngIf="!loading && filteredIdeas.length > 0" class="pagination">
        <button 
          [disabled]="currentPage === 1"
          (click)="previousPage()"
          class="btn-secondary">
          ← Previous
        </button>
        <span>Page {{ currentPage }} of {{ totalPages }}</span>
        <button 
          [disabled]="currentPage === totalPages"
          (click)="nextPage()"
          class="btn-secondary">
          Next →
        </button>
      </div>
    </div>
  `,
  styles: [`
    .ideas-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .ideas-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    .ideas-header h1 {
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

    .ideas-filters {
      display: grid;
      grid-template-columns: 1fr auto;
      gap: 1rem;
      margin-bottom: 2rem;
    }

    .search-box {
      position: relative;
    }

    .search-input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
    }

    .filter-group {
      display: flex;
      gap: 1rem;
    }

    .filter-select {
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
    }

    .loading {
      text-align: center;
      padding: 2rem;
      color: #666;
    }

    .empty-state {
      text-align: center;
      padding: 3rem;
      color: #999;
      background: #f5f5f5;
      border-radius: 4px;
    }

    .ideas-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1.5rem;
      margin-bottom: 2rem;
    }

    .idea-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 1.5rem;
      cursor: pointer;
      transition: all 0.3s ease;
      text-decoration: none;
      color: inherit;
    }

    .idea-card:hover {
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
      transform: translateY(-2px);
    }

    .idea-header {
      display: flex;
      justify-content: space-between;
      align-items: start;
      margin-bottom: 1rem;
    }

    .idea-header h3 {
      margin: 0;
      font-size: 1.25rem;
      flex: 1;
    }

    .status-badge {
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.75rem;
      font-weight: 600;
      white-space: nowrap;
      margin-left: 1rem;
    }

    .status-draft { background: #f0f0f0; color: #666; }
    .status-submitted { background: #e3f2fd; color: #0066cc; }
    .status-judging { background: #fff3e0; color: #f57c00; }
    .status-complete { background: #e8f5e9; color: #2e7d32; }
    .status-awarded { background: #fce4ec; color: #c2185b; }

    .idea-description {
      color: #666;
      margin: 0 0 1rem 0;
      line-height: 1.5;
    }

    .idea-meta {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 1rem;
      flex-wrap: wrap;
    }

    .track-badge {
      padding: 0.25rem 0.75rem;
      border-radius: 4px;
      color: white;
      font-size: 0.875rem;
      font-weight: 600;
    }

    .author {
      color: #999;
      font-size: 0.875rem;
    }

    .idea-stats {
      display: flex;
      align-items: center;
      gap: 1rem;
      font-size: 0.875rem;
      color: #999;
    }

    .stat {
      background: #f5f5f5;
      padding: 0.25rem 0.5rem;
      border-radius: 3px;
    }

    .date {
      margin-left: auto;
      white-space: nowrap;
    }

    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 1rem;
      margin-top: 2rem;
    }

    .btn-secondary {
      background: white;
      color: #0066cc;
      border: 1px solid #0066cc;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
    }

    .btn-secondary:hover:not(:disabled) {
      background: #f0f0f0;
    }

    .btn-secondary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    @media (max-width: 768px) {
      .ideas-filters {
        grid-template-columns: 1fr;
      }

      .filter-group {
        flex-direction: column;
      }

      .ideas-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class IdeasComponent implements OnInit, OnDestroy {
  ideas: Idea[] = [];
  filteredIdeas: Idea[] = [];
  loading = true;
  searchQuery = '';
  statusFilter = '';
  trackFilter = '';
  isAuthenticated = false;
  isJudge = false;
  tracks: any[] = [];
  currentPage = 1;
  pageSize = 12;
  totalPages = 1;

  private destroy$ = new Subject<void>();

  constructor(
    private ideaService: IdeaService,
    private authService: AuthService,
    private hackathonService: HackathonService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    this.isJudge = this.authService.isJudge();
    
    this.hackathonService.getCurrentHackathon()
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        hackathon => {
          this.tracks = hackathon.tracks || [];
          this.loadIdeas(hackathon.id);
        },
        error => {
          console.error('Error loading hackathon:', error);
          this.loading = false;
        }
      );
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadIdeas(hackathonId: number): void {
    this.loading = true;
    this.ideaService.getIdeas(hackathonId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        ideas => {
          this.ideas = ideas;
          this.ideaService.updateIdeasCache(ideas);
          this.applyFilters();
          this.loading = false;
        },
        error => {
          console.error('Error loading ideas:', error);
          this.loading = false;
        }
      );
  }

  onSearch(): void {
    if (this.searchQuery) {
      this.hackathonService.getCurrentHackathon()
        .pipe(takeUntil(this.destroy$))
        .subscribe(hackathon => {
          this.ideaService.searchIdeas(this.searchQuery, hackathon.id)
            .pipe(takeUntil(this.destroy$))
            .subscribe(
              ideas => {
                this.ideas = ideas;
                this.applyFilters();
              }
            );
        });
    } else {
      this.hackathonService.getCurrentHackathon()
        .pipe(takeUntil(this.destroy$))
        .subscribe(hackathon => {
          this.loadIdeas(hackathon.id);
        });
    }
  }

  applyFilters(): void {
    this.filteredIdeas = this.ideas.filter(idea => {
      const statusMatch = !this.statusFilter || idea.status === this.statusFilter;
      const trackMatch = !this.trackFilter || idea.trackId === parseInt(this.trackFilter);
      return statusMatch && trackMatch;
    });

    this.totalPages = Math.ceil(this.filteredIdeas.length / this.pageSize);
    this.currentPage = 1;
  }

  get paginatedIdeas(): Idea[] {
    const startIdx = (this.currentPage - 1) * this.pageSize;
    return this.filteredIdeas.slice(startIdx, startIdx + this.pageSize);
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      window.scrollTo(0, 0);
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      window.scrollTo(0, 0);
    }
  }

  getTrackName(trackId: number | undefined): string {
    if (!trackId) return 'General';
    return this.tracks.find(t => t.id === trackId)?.name || 'Unknown';
  }

  getTrackColor(trackId: number | undefined): string {
    const colors: { [key: number]: string } = {
      1: '#FF6B6B',
      2: '#4ECDC4',
      3: '#95E1D3',
      4: '#F38181',
      5: '#AA96DA'
    };
    return colors[trackId || 0] || '#999';
  }

  formatDate(date: Date | undefined): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }
}
