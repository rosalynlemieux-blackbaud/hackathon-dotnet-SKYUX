import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { IdeaService } from '../../services/idea.service';
import { RatingService } from '../../services/rating.service';
import { HackathonService } from '../../services/hackathon.service';

interface IdeaWithRating {
  idea: any;
  ratings: { [key: number]: number };
  feedback: { [key: number]: string };
  averageScore: number;
  isSubmitted: boolean;
}

@Component({
  selector: 'app-judging',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="judging-container">
      <h1>Judging Dashboard</h1>

      <div *ngIf="loading" class="loading">Loading ideas for judging...</div>

      <div *ngIf="!loading && ideesForJudging.length === 0" class="empty-state">
        <p>No ideas awaiting your judgment at this time.</p>
      </div>

      <div *ngIf="!loading && ideesForJudging.length > 0" class="judging-interface">
        <!-- Filter Section -->
        <div class="judging-filters">
          <select [(ngModel)]="statusFilter" (change)="applyFilters()" class="filter-select">
            <option value="">All Statuses</option>
            <option value="not-started">Not Started</option>
            <option value="in-progress">In Progress</option>
            <option value="completed">Completed</option>
          </select>

          <select [(ngModel)]="sortBy" (change)="applySort()" class="filter-select">
            <option value="score-desc">Highest Score</option>
            <option value="score-asc">Lowest Score</option>
            <option value="recent">Most Recent</option>
            <option value="name">Alphabetical</option>
          </select>
        </div>

        <!-- Judging Grid -->
        <div class="judging-grid">
          <div *ngFor="let item of filteredIdeas; trackBy: trackByIdeaId" class="judging-card">
            <div class="idea-header">
              <h3>{{ item.idea.title }}</h3>
              <span [class]="'status-' + getJudgingStatus(item)" class="status-badge">
                {{ getJudgingStatus(item) | uppercase }}
              </span>
            </div>

            <p class="idea-author">
              By <strong>{{ item.idea.author?.firstName }} {{ item.idea.author?.lastName }}</strong>
            </p>

            <p class="idea-summary">{{ item.idea.description | slice:0:80 }}...</p>

            <!-- Criteria Ratings -->
            <div class="rating-section">
              <h4>Rate by Criteria</h4>
              <div *ngFor="let criterion of judgingCriteria" class="criterion-rating">
                <label>
                  <span class="criterion-name">{{ criterion.name }}</span>
                  <span class="criterion-weight">({{ (criterion.weight * 100) | number:'0.0' }}%)</span>
                </label>
                <div class="rating-controls">
                  <div class="rating-buttons">
                    <button 
                      *ngFor="let score of [1,2,3,4,5,6,7,8,9,10]"
                      [class.selected]="item.ratings[criterion.id] === score"
                      (click)="setRating(item, criterion.id, score)"
                      class="score-btn">
                      {{ score }}
                    </button>
                  </div>
                  <span class="current-score">
                    {{ item.ratings[criterion.id] || '-' }}/10
                  </span>
                </div>
                <textarea 
                  [(ngModel)]="item.feedback[criterion.id]"
                  placeholder="Optional feedback..."
                  class="feedback-input"></textarea>
              </div>
            </div>

            <!-- Average Score Display -->
            <div class="average-score">
              <strong>Current Average:</strong>
              <span class="score-display">{{ calculateAverageScore(item).toFixed(1) }}/10</span>
            </div>

            <!-- Actions -->
            <div class="card-actions">
              <button 
                [routerLink]="['/ideas', item.idea.id]"
                class="btn-view">
                View Full Idea
              </button>
              <button 
                (click)="submitRatings(item)"
                [disabled]="!canSubmitRatings(item)"
                class="btn-submit">
                Save Ratings
              </button>
            </div>
          </div>
        </div>

        <!-- Summary Section -->
        <div class="judging-summary">
          <h2>Your Judging Progress</h2>
          <div class="summary-stats">
            <div class="stat-card">
              <div class="stat-number">{{ totalIdeasForJudging }}</div>
              <div class="stat-label">Total Ideas</div>
            </div>
            <div class="stat-card">
              <div class="stat-number">{{ completedCount }}</div>
              <div class="stat-label">Completed</div>
            </div>
            <div class="stat-card">
              <div class="stat-number">{{ inProgressCount }}</div>
              <div class="stat-label">In Progress</div>
            </div>
            <div class="stat-card">
              <div class="stat-number">{{ notStartedCount }}</div>
              <div class="stat-label">Not Started</div>
            </div>
          </div>
          <div class="progress-bar">
            <div class="progress-fill" [style.width.%]="progressPercentage"></div>
          </div>
          <p class="progress-text">{{ progressPercentage | number:'0.0' }}% of judging complete</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .judging-container {
      padding: 2rem;
      max-width: 1400px;
      margin: 0 auto;
    }

    .judging-container h1 {
      margin-top: 0;
      font-size: 2rem;
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

    .judging-filters {
      display: flex;
      gap: 1rem;
      margin-bottom: 2rem;
    }

    .filter-select {
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
    }

    .judging-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(450px, 1fr));
      gap: 1.5rem;
      margin-bottom: 3rem;
    }

    .judging-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 1.5rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.05);
    }

    .idea-header {
      display: flex;
      justify-content: space-between;
      align-items: start;
      margin-bottom: 1rem;
    }

    .idea-header h3 {
      margin: 0;
      font-size: 1.1rem;
      flex: 1;
    }

    .status-badge {
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.75rem;
      font-weight: 600;
      white-space: nowrap;
      margin-left: 0.5rem;
    }

    .status-not-started {
      background: #f5f5f5;
      color: #999;
    }

    .status-in-progress {
      background: #fff3e0;
      color: #f57c00;
    }

    .status-completed {
      background: #e8f5e9;
      color: #2e7d32;
    }

    .idea-author {
      margin: 0.5rem 0 1rem 0;
      font-size: 0.9rem;
      color: #666;
    }

    .idea-summary {
      margin: 0.5rem 0 1.5rem 0;
      color: #666;
      line-height: 1.5;
      font-size: 0.95rem;
    }

    .rating-section {
      background: #f5f5f5;
      padding: 1.5rem;
      border-radius: 4px;
      margin-bottom: 1rem;
    }

    .rating-section h4 {
      margin-top: 0;
      margin-bottom: 1rem;
      font-size: 0.95rem;
    }

    .criterion-rating {
      margin-bottom: 1.5rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid #ddd;
    }

    .criterion-rating:last-child {
      border-bottom: none;
    }

    .criterion-rating label {
      display: block;
      font-weight: 600;
      margin-bottom: 0.5rem;
      font-size: 0.85rem;
    }

    .criterion-name {
      display: block;
    }

    .criterion-weight {
      font-weight: 400;
      color: #999;
      font-size: 0.80rem;
    }

    .rating-controls {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 0.5rem;
    }

    .rating-buttons {
      display: flex;
      gap: 0.25rem;
      flex: 1;
    }

    .score-btn {
      width: 28px;
      height: 28px;
      padding: 0;
      border: 1px solid #ddd;
      background: white;
      border-radius: 3px;
      cursor: pointer;
      font-size: 0.75rem;
      font-weight: 600;
      transition: all 0.2s;
    }

    .score-btn:hover {
      border-color: #0066cc;
      background: #f0f0f0;
    }

    .score-btn.selected {
      background: #0066cc;
      color: white;
      border-color: #0066cc;
    }

    .current-score {
      font-weight: 600;
      color: #0066cc;
      min-width: 40px;
      text-align: right;
    }

    .feedback-input {
      width: 100%;
      padding: 0.5rem;
      border: 1px solid #ddd;
      border-radius: 3px;
      font-family: inherit;
      font-size: 0.85rem;
      resize: vertical;
      min-height: 50px;
    }

    .average-score {
      background: white;
      padding: 0.75rem;
      border-radius: 4px;
      margin-bottom: 1rem;
      text-align: center;
      border: 1px solid #e3f2fd;
    }

    .score-display {
      display: block;
      font-size: 1.5rem;
      color: #0066cc;
      font-weight: bold;
    }

    .card-actions {
      display: flex;
      gap: 0.5rem;
    }

    .btn-view, .btn-submit {
      flex: 1;
      padding: 0.75rem;
      border: none;
      border-radius: 4px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s;
    }

    .btn-view {
      background: white;
      color: #0066cc;
      border: 1px solid #0066cc;
    }

    .btn-view:hover {
      background: #f0f0f0;
    }

    .btn-submit {
      background: #4caf50;
      color: white;
    }

    .btn-submit:hover:not(:disabled) {
      background: #388e3c;
    }

    .btn-submit:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .judging-summary {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 2rem;
      margin-top: 3rem;
    }

    .judging-summary h2 {
      margin-top: 0;
      margin-bottom: 1.5rem;
    }

    .summary-stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 1.5rem;
      margin-bottom: 2rem;
    }

    .stat-card {
      text-align: center;
      padding: 1rem;
      background: #f5f5f5;
      border-radius: 4px;
    }

    .stat-number {
      font-size: 2rem;
      font-weight: bold;
      color: #0066cc;
      margin-bottom: 0.5rem;
    }

    .stat-label {
      color: #666;
      font-size: 0.9rem;
    }

    .progress-bar {
      height: 8px;
      background: #eee;
      border-radius: 4px;
      overflow: hidden;
      margin-bottom: 0.5rem;
    }

    .progress-fill {
      height: 100%;
      background: linear-gradient(to right, #ff6b6b, #ffd93d, #6bcf7f);
      transition: width 0.3s ease;
    }

    .progress-text {
      text-align: center;
      color: #666;
      font-size: 0.9rem;
    }

    @media (max-width: 900px) {
      .judging-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class JudgingComponent implements OnInit, OnDestroy {
  ideesForJudging: IdeaWithRating[] = [];
  filteredIdeas: IdeaWithRating[] = [];
  loading = true;
  judgingCriteria: any[] = [];
  statusFilter = '';
  sortBy = 'score-desc';

  totalIdeasForJudging = 0;
  completedCount = 0;
  inProgressCount = 0;
  notStartedCount = 0;
  progressPercentage = 0;

  private destroy$ = new Subject<void>();

  constructor(
    private ideaService: IdeaService,
    private ratingService: RatingService,
    private hackathonService: HackathonService
  ) {}

  ngOnInit(): void {
    this.loadJudgingData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadJudgingData(): void {
    this.hackathonService.getCurrentHackathon()
      .pipe(takeUntil(this.destroy$))
      .subscribe(hackathon => {
        // Set default judging criteria
        this.judgingCriteria = hackathon.judgingCriteria || [
          { id: 1, name: 'Innovation', weight: 0.3 },
          { id: 2, name: 'Feasibility', weight: 0.25 },
          { id: 3, name: 'Impact', weight: 0.25 },
          { id: 4, name: 'Presentation', weight: 0.2 }
        ];

        this.loadIdeasForJudging(hackathon.id);
      });
  }

  loadIdeasForJudging(hackathonId: number): void {
    this.ideaService.getIdeas(hackathonId, 'submitted')
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        ideas => {
          this.ideesForJudging = ideas.map(idea => ({
            idea,
            ratings: {},
            feedback: {},
            averageScore: 0,
            isSubmitted: false
          }));

          this.totalIdeasForJudging = ideas.length;
          this.updateProgressStats();
          this.applyFilters();
          this.loading = false;
        },
        error => {
          console.error('Error loading ideas:', error);
          this.loading = false;
        }
      );
  }

  setRating(item: IdeaWithRating, criterionId: number, score: number): void {
    item.ratings[criterionId] = item.ratings[criterionId] === score ? undefined : score;
  }

  calculateAverageScore(item: IdeaWithRating): number {
    const ratings = Object.entries(item.ratings).filter(([, score]) => score !== undefined);
    if (ratings.length === 0) return 0;

    let totalWeightedScore = 0;
    let totalWeight = 0;

    ratings.forEach(([criterionId, score]) => {
      const criterion = this.judgingCriteria.find(c => c.id === parseInt(criterionId));
      if (criterion && score) {
        totalWeightedScore += score * criterion.weight;
        totalWeight += criterion.weight;
      }
    });

    return totalWeight > 0 ? totalWeightedScore / totalWeight : 0;
  }

  canSubmitRatings(item: IdeaWithRating): boolean {
    return Object.keys(item.ratings).length > 0 && !item.isSubmitted;
  }

  submitRatings(item: IdeaWithRating): void {
    // Submit each rating
    const ratings = Object.entries(item.ratings)
      .filter(([, score]) => score !== undefined)
      .map(([criterionId, score]) => ({
        ideaId: item.idea.id,
        criterionId: parseInt(criterionId),
        score,
        feedback: item.feedback[parseInt(criterionId)]
      }));

    Promise.all(
      ratings.map(rating =>
        this.ratingService.submitRating(
          rating.ideaId,
          rating.criterionId,
          rating.score,
          rating.feedback
        ).toPromise()
      )
    ).then(
      () => {
        item.isSubmitted = true;
        this.completedCount++;
        this.notStartedCount = Math.max(0, this.notStartedCount - 1);
        this.updateProgressStats();
        this.applyFilters();
      }
    ).catch(error => {
      console.error('Error submitting ratings:', error);
      alert('Error saving ratings. Please try again.');
    });
  }

  getJudgingStatus(item: IdeaWithRating): string {
    if (item.isSubmitted) return 'completed';
    if (Object.keys(item.ratings).length > 0) return 'in-progress';
    return 'not-started';
  }

  applyFilters(): void {
    this.filteredIdeas = this.ideesForJudging.filter(item => {
      if (this.statusFilter && this.getJudgingStatus(item) !== this.statusFilter) {
        return false;
      }
      return true;
    });

    this.applySort();
  }

  applySort(): void {
    switch (this.sortBy) {
      case 'score-desc':
        this.filteredIdeas.sort((a, b) =>
          this.calculateAverageScore(b) - this.calculateAverageScore(a)
        );
        break;
      case 'score-asc':
        this.filteredIdeas.sort((a, b) =>
          this.calculateAverageScore(a) - this.calculateAverageScore(b)
        );
        break;
      case 'recent':
        this.filteredIdeas.sort((a, b) =>
          new Date(b.idea.createdAt).getTime() - new Date(a.idea.createdAt).getTime()
        );
        break;
      case 'name':
        this.filteredIdeas.sort((a, b) =>
          a.idea.title.localeCompare(b.idea.title)
        );
        break;
    }
  }

  updateProgressStats(): void {
    this.completedCount = this.ideesForJudging.filter(i => i.isSubmitted).length;
    this.inProgressCount = this.ideesForJudging.filter(i =>
      !i.isSubmitted && Object.keys(i.ratings).length > 0
    ).length;
    this.notStartedCount = this.totalIdeasForJudging - this.completedCount - this.inProgressCount;
    this.progressPercentage = this.totalIdeasForJudging > 0
      ? (this.completedCount / this.totalIdeasForJudging) * 100
      : 0;
  }

  trackByIdeaId(index: number, item: IdeaWithRating): number {
    return item.idea.id;
  }
}
