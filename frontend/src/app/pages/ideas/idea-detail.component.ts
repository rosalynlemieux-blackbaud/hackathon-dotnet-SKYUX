import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { IdeaService } from '../../services/idea.service';
import { CommentService } from '../../services/comment.service';
import { RatingService } from '../../services/rating.service';
import { NotificationService } from '../../services/notification.service';
import { AuthService } from '../../services/auth.service';
import { Idea } from '../../models/models';

interface CommentThread {
  comment: any;
  replies: any[];
}

@Component({
  selector: 'app-idea-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="idea-detail-container" *ngIf="!loading">
      <div class="idea-detail-header">
        <button class="back-btn" (click)="goBack()">← Back to Ideas</button>
      </div>

      <div class="idea-content">
        <!-- Main Idea Section -->
        <div class="idea-main">
          <h1>{{ idea.title }}</h1>
          
          <div class="idea-metadata">
            <span [class]="'status-badge status-' + idea.status">{{ idea.status | uppercase }}</span>
            <span class="track-badge" [style.background-color]="'#4ECDC4'">
              AI Acceleration
            </span>
            <span class="date">{{ formatDate(idea.createdAt) }}</span>
          </div>

          <div class="idea-author">
            <div class="author-info">
              <strong>{{ idea.author?.firstName }} {{ idea.author?.lastName }}</strong>
              <p class="author-email">{{ idea.author?.email }}</p>
            </div>
          </div>

          <!-- Description -->
          <section class="idea-section">
            <h2>Description</h2>
            <p>{{ idea.description }}</p>
          </section>

          <!-- Problem & Solution -->
          <div class="section-grid">
            <section class="idea-section">
              <h3>Problem Statement</h3>
              <p>{{ idea.problemStatement || 'Not provided' }}</p>
            </section>
            
            <section class="idea-section">
              <h3>Proposed Solution</h3>
              <p>{{ idea.proposedSolution || 'Not provided' }}</p>
            </section>
          </div>

          <!-- Success Metrics -->
          <section class="idea-section">
            <h3>Success Metrics</h3>
            <p>{{ idea.successMetrics || 'Not provided' }}</p>
          </section>

          <!-- Team & Collaboration -->
          <section class="idea-section" *ngIf="idea.submission?.teamId">
            <h3>Team</h3>
            <div class="team-info">
              <p>Team ID: {{ idea.submission.teamId }}</p>
              <p>Collaborating to bring this idea to life!</p>
            </div>
          </section>

          <!-- Awards -->
          <section class="idea-section" *ngIf="idea.awards && idea.awards.length > 0">
            <h3>Awards</h3>
            <div class="awards-list">
              <div *ngFor="let award of idea.awards" class="award-item">
                <strong>🏆 {{ award.name }}</strong>
                <p>{{ award.description }}</p>
              </div>
            </div>
          </section>

          <!-- Judging Section -->
          <section class="idea-section" *ngIf="isJudge">
            <h2>Judging</h2>
            <div class="judging-criteria">
              <div *ngFor="let criterion of judgingCriteria" class="criterion-item">
                <label>{{ criterion.name }} (Weight: {{ criterion.weight * 100 }}%)</label>
                <div class="rating-input">
                  <input 
                    type="range" 
                    min="1" 
                    max="10" 
                    [(ngModel)]="ratings[criterion.id]"
                    class="slider">
                  <span class="rating-value">{{ ratings[criterion.id] || 5 }}/10</span>
                </div>
                <textarea 
                  [(ngModel)]="feedback[criterion.id]"
                  placeholder="Optional feedback..."
                  class="feedback-box"></textarea>
              </div>
            </div>
            <button class="btn-submit" (click)="submitRatings()">Submit Ratings</button>
          </section>

          <!-- Comments Section -->
          <section class="comments-section">
            <h2>Comments ({{ comments.length }})</h2>
            
            <div *ngIf="isAuthenticated" class="comment-form">
              <textarea 
                [(ngModel)]="newCommentText"
                placeholder="Add a comment..."
                class="comment-input"></textarea>
              <button 
                (click)="addComment()"
                [disabled]="!newCommentText.trim()"
                class="btn-submit">
                Post Comment
              </button>
            </div>

            <div *ngIf="comments.length === 0 && !isAuthenticated" class="no-comments">
              <p>Sign in to participate in the discussion</p>
            </div>

            <div *ngIf="comments.length === 0 && isAuthenticated" class="no-comments">
              <p>No comments yet. Be the first to share your thoughts!</p>
            </div>

            <div *ngFor="let thread of commentThreads" class="comment-thread">
              <div class="comment">
                <div class="comment-header">
                  <strong>{{ thread.comment.user?.firstName }} {{ thread.comment.user?.lastName }}</strong>
                  <span class="comment-date">{{ formatDate(thread.comment.createdAt) }}</span>
                </div>
                <p class="comment-text">{{ thread.comment.content }}</p>
                <div class="comment-actions" *ngIf="isAuthorOf(thread.comment)">
                  <button (click)="deleteComment(thread.comment.id)" class="btn-delete">Delete</button>
                </div>
              </div>

              <div *ngFor="let reply of thread.replies" class="reply">
                <div class="comment-header">
                  <strong>{{ reply.user?.firstName }} {{ reply.user?.lastName }}</strong>
                  <span class="comment-date">{{ formatDate(reply.createdAt) }}</span>
                </div>
                <p class="comment-text">{{ reply.content }}</p>
              </div>
            </div>
          </section>
        </div>

        <!-- Sidebar -->
        <aside class="idea-sidebar">
          <div class="sidebar-card">
            <h3>Impact Score</h3>
            <div class="score-display">
              <div class="score-number">{{ averageRating.toFixed(1) }}</div>
              <div class="score-bar">
                <div class="score-fill" [style.width.%]="(averageRating / 10) * 100"></div>
              </div>
              <p class="score-meta">{{ averageRating.toFixed(1) }}/10 • {{ ratingCount }} judges</p>
            </div>
          </div>

          <div class="sidebar-card" *ngIf="idea.submission?.teamId">
            <h3>Team Details</h3>
            <p>Team ID: {{ idea.submission.teamId }}</p>
            <button class="btn-secondary" [routerLink]="['/teams', idea.submission.teamId]">
              View Team
            </button>
          </div>

          <div class="sidebar-card" *ngIf="isAuthenticated">
            <h3>Actions</h3>
            <button *ngIf="isAuthor" class="btn-secondary" [routerLink]="['/ideas', idea.id, 'edit']">
              Edit Idea
            </button>
            <button *ngIf="isAuthor && idea.status === 'draft'" 
              (click)="submitIdea()"
              class="btn-primary">
              Submit for Judging
            </button>
            <button *ngIf="isAuthor" (click)="deleteIdea()" class="btn-delete">
              Delete Idea
            </button>
          </div>

          <div class="sidebar-card">
            <h3>Idea Info</h3>
            <p><strong>Created:</strong> {{ formatDate(idea.createdAt) }}</p>
            <p *ngIf="idea.submittedAt"><strong>Submitted:</strong> {{ formatDate(idea.submittedAt) }}</p>
            <p><strong>Status:</strong> {{ idea.status }}</p>
          </div>
        </aside>
      </div>
    </div>

    <div *ngIf="loading" class="loading">
      <p>Loading idea details...</p>
    </div>
  `,
  styles: [`
    .idea-detail-container {
      padding: 2rem;
      max-width: 1400px;
      margin: 0 auto;
    }

    .idea-detail-header {
      margin-bottom: 2rem;
    }

    .back-btn {
      background: white;
      border: 1px solid #ddd;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
      color: #0066cc;
    }

    .back-btn:hover {
      background: #f5f5f5;
    }

    .idea-content {
      display: grid;
      grid-template-columns: 1fr 300px;
      gap: 2rem;
    }

    .idea-main {
      background: white;
      border-radius: 8px;
      padding: 2rem;
    }

    .idea-main h1 {
      font-size: 2rem;
      margin: 0 0 1rem 0;
    }

    .idea-metadata {
      display: flex;
      gap: 1rem;
      margin-bottom: 2rem;
      flex-wrap: wrap;
    }

    .status-badge, .track-badge {
      padding: 0.5rem 1rem;
      border-radius: 20px;
      font-size: 0.875rem;
      font-weight: 600;
      background: #e3f2fd;
      color: #0066cc;
    }

    .date {
      color: #999;
      font-size: 0.875rem;
    }

    .idea-author {
      background: #f5f5f5;
      padding: 1rem;
      border-radius: 4px;
      margin-bottom: 2rem;
    }

    .author-info strong {
      display: block;
      margin-bottom: 0.25rem;
    }

    .author-email {
      margin: 0;
      color: #666;
      font-size: 0.875rem;
    }

    .idea-section {
      margin-bottom: 2rem;
      padding-bottom: 2rem;
      border-bottom: 1px solid #eee;
    }

    .idea-section h2 {
      font-size: 1.5rem;
      margin-top: 0;
    }

    .idea-section h3 {
      font-size: 1.1rem;
      margin-bottom: 1rem;
    }

    .section-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 2rem;
    }

    .judging-criteria {
      background: #f5f5f5;
      padding: 1.5rem;
      border-radius: 4px;
      margin-bottom: 1rem;
    }

    .criterion-item {
      margin-bottom: 2rem;
      padding-bottom: 1.5rem;
      border-bottom: 1px solid #ddd;
    }

    .criterion-item:last-child {
      border-bottom: none;
    }

    .rating-input {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin: 0.5rem 0;
    }

    .slider {
      flex: 1;
      height: 6px;
      border-radius: 3px;
      background: #ddd;
      outline: none;
    }

    .rating-value {
      min-width: 50px;
      text-align: right;
      font-weight: 600;
      color: #0066cc;
    }

    .feedback-box {
      width: 100%;
      padding: 0.5rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 0.875rem;
      resize: vertical;
      min-height: 60px;
      font-family: inherit;
    }

    .comments-section {
      margin-top: 2rem;
    }

    .comment-form {
      background: #f5f5f5;
      padding: 1.5rem;
      border-radius: 4px;
      margin-bottom: 2rem;
    }

    .comment-input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-family: inherit;
      font-size: 1rem;
      margin-bottom: 0.5rem;
      resize: vertical;
      min-height: 80px;
    }

    .comment-thread {
      margin-bottom: 2rem;
      padding-bottom: 1.5rem;
      border-bottom: 1px solid #eee;
    }

    .comment {
      background: white;
      border: 1px solid #eee;
      border-radius: 4px;
      padding: 1rem;
      margin-bottom: 0.5rem;
    }

    .reply {
      background: white;
      border: 1px solid #eee;
      border-left: 3px solid #0066cc;
      border-radius: 4px;
      padding: 1rem;
      margin-left: 2rem;
      margin-bottom: 0.5rem;
    }

    .comment-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.5rem;
    }

    .comment-date {
      color: #999;
      font-size: 0.875rem;
    }

    .comment-text {
      margin: 0.5rem 0 0 0;
      line-height: 1.5;
    }

    .comment-actions {
      margin-top: 0.5rem;
    }

    .no-comments {
      text-align: center;
      padding: 2rem;
      color: #999;
      background: #f5f5f5;
      border-radius: 4px;
    }

    .idea-sidebar {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .sidebar-card {
      background: white;
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 1.5rem;
    }

    .sidebar-card h3 {
      margin-top: 0;
      margin-bottom: 1rem;
    }

    .score-display {
      text-align: center;
    }

    .score-number {
      font-size: 2.5rem;
      font-weight: bold;
      color: #0066cc;
      margin-bottom: 0.5rem;
    }

    .score-bar {
      height: 8px;
      background: #eee;
      border-radius: 4px;
      overflow: hidden;
      margin-bottom: 0.5rem;
    }

    .score-fill {
      height: 100%;
      background: linear-gradient(to right, #ff6b6b, #ffd93d, #6bcf7f);
      transition: width 0.3s ease;
    }

    .score-meta {
      margin: 0.5rem 0 0 0;
      color: #999;
      font-size: 0.875rem;
    }

    .btn-primary, .btn-secondary, .btn-delete, .btn-submit {
      width: 100%;
      padding: 0.75rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      margin-bottom: 0.5rem;
    }

    .btn-primary {
      background: #0066cc;
      color: white;
    }

    .btn-primary:hover {
      background: #0052a3;
    }

    .btn-secondary {
      background: white;
      color: #0066cc;
      border: 1px solid #0066cc;
    }

    .btn-secondary:hover {
      background: #f0f0f0;
    }

    .btn-delete {
      background: #f44336;
      color: white;
    }

    .btn-delete:hover {
      background: #da190b;
    }

    .btn-submit {
      background: #4caf50;
      color: white;
      width: auto;
    }

    .btn-submit:hover:not(:disabled) {
      background: #388e3c;
    }

    .btn-submit:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .loading {
      text-align: center;
      padding: 4rem 2rem;
      color: #666;
    }

    @media (max-width: 900px) {
      .idea-content {
        grid-template-columns: 1fr;
      }

      .section-grid {
        grid-template-columns: 1fr;
      }

      .reply {
        margin-left: 1rem;
      }
    }
  `]
})
export class IdeaDetailComponent implements OnInit, OnDestroy {
  idea: any;
  comments: any[] = [];
  commentThreads: CommentThread[] = [];
  loading = true;
  isAuthenticated = false;
  isJudge = false;
  isAuthor = false;
  newCommentText = '';
  judgingCriteria: any[] = [];
  ratings: { [key: number]: number } = {};
  feedback: { [key: number]: string } = {};
  averageRating = 0;
  ratingCount = 0;
  currentUserId = 0;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ideaService: IdeaService,
    private commentService: CommentService,
    private ratingService: RatingService,
    private notificationService: NotificationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    this.isJudge = this.authService.isJudge();
    this.currentUserId = this.authService.getCurrentUserId() || 0;

    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const ideaId = parseInt(params['id']);
        this.loadIdea(ideaId);
        this.setupRealtimeNotifications(ideaId);
      });
  }

  setupRealtimeNotifications(ideaId: number): void {
    // Watch this specific idea for updates
    this.notificationService.watchIdea(ideaId).catch(err => {
      console.error('Failed to watch idea:', err);
    });

    // Subscribe to new comments
    this.notificationService.commentAdded$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {
        if (data.ideaId === ideaId) {
          // Add new comment to the list
          const newComment = {
            id: data.comment.id,
            content: data.comment.content,
            userId: data.comment.authorId,
            userName: data.comment.authorName,
            createdAt: data.comment.createdAt,
            parentCommentId: data.comment.parentCommentId,
            isDeleted: false
          };
          
          this.comments.push(newComment);
          this.buildCommentThreads(this.comments);
        }
      });

    // Subscribe to rating updates
    this.notificationService.ratingSubmitted$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {
        if (data.ideaId === ideaId) {
          // Refresh ratings
          this.loadRatings(ideaId);
        }
      });
  }

  ngOnDestroy(): void {
    // Stop watching idea when leaving
    if (this.idea?.id) {
      this.notificationService.unwatchIdea(this.idea.id).catch(err => {
        console.error('Failed to unwatch idea:', err);
      });
    }
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadIdea(id: number): void {
    this.ideaService.getIdea(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        idea => {
          this.idea = idea;
          this.isAuthor = idea.author?.id === this.currentUserId;
          this.loadComments(id);
          this.loadRatings(id);
          this.loading = false;
        },
        error => {
          console.error('Error loading idea:', error);
          this.loading = false;
        }
      );
  }

  loadComments(ideaId: number): void {
    this.commentService.getComments(ideaId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        comments => {
          this.comments = comments;
          this.buildCommentThreads(comments);
        }
      );
  }

  loadRatings(ideaId: number): void {
    this.ratingService.getAverageRating(ideaId)
      .pipe(takeUntil(this.destroy$))
      .subscribe((ratings: any) => {
        this.averageRating = ratings.averageScore;
        this.ratingCount = ratings.ratingCount;
      });
  }

  buildCommentThreads(comments: any[]): void {
    this.commentThreads = comments
      .filter(c => !c.parentCommentId)
      .map(comment => ({
        comment,
        replies: comments.filter(c => c.parentCommentId === comment.id)
      }));
  }

  addComment(): void {
    if (!this.newCommentText.trim()) return;

    this.commentService.createComment(this.idea.id, this.newCommentText)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        () => {
          this.newCommentText = '';
          this.loadComments(this.idea.id);
        }
      );
  }

  deleteComment(commentId: number): void {
    if (confirm('Delete this comment?')) {
      this.commentService.deleteComment(commentId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadComments(this.idea.id);
        });
    }
  }

  submitRatings(): void {
    // Submit ratings for each criterion
    console.log('Submitting ratings:', this.ratings, this.feedback);
    // Implementation would submit each rating via RatingService
  }

  submitIdea(): void {
    this.ideaService.submitIdea(this.idea.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.idea.status = 'submitted';
        this.idea.submittedAt = new Date();
      });
  }

  deleteIdea(): void {
    if (confirm('Delete this idea permanently?')) {
      this.ideaService.deleteIdea(this.idea.id)
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.router.navigate(['/ideas']);
        });
    }
  }

  isAuthorOf(comment: any): boolean {
    return comment.userId === this.currentUserId;
  }

  goBack(): void {
    this.router.navigate(['/ideas']);
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
