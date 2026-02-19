import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { IdeaService } from '../../services/idea.service';
import { HackathonService } from '../../services/hackathon.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-idea-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="form-container">
      <h1>{{ isEditMode ? 'Edit Idea' : 'Submit Your Idea' }}</h1>

      <form (ngSubmit)="submitForm()" class="idea-form">
        <!-- Basic Info Section -->
        <section class="form-section">
          <h2>Basic Information</h2>

          <div class="form-group">
            <label for="title" class="required">Idea Title</label>
            <input 
              id="title"
              type="text" 
              [(ngModel)]="form.title"
              name="title"
              placeholder="Give your idea a catchy title"
              class="form-input"
              required>
            <p class="help-text">Max 100 characters</p>
          </div>

          <div class="form-group">
            <label for="description" class="required">Description</label>
            <textarea 
              id="description"
              [(ngModel)]="form.description"
              name="description"
              placeholder="Describe your idea in detail..."
              class="form-textarea"
              rows="5"
              required></textarea>
            <p class="help-text">{{ form.description?.length || 0 }}/500 characters</p>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label for="track" class="required">Track</label>
              <select 
                id="track"
                [(ngModel)]="form.trackId"
                name="trackId"
                class="form-select"
                required>
                <option value="">Select a track...</option>
                <option *ngFor="let track of tracks" [value]="track.id">
                  {{ track.name }}
                </option>
              </select>
            </div>

            <div class="form-group">
              <label for="team">Team</label>
              <select 
                id="team"
                [(ngModel)]="form.teamId"
                name="teamId"
                class="form-select">
                <option value="">Individual submission</option>
                <option *ngFor="let team of userTeams" [value]="team.id">
                  {{ team.name }}
                </option>
              </select>
            </div>
          </div>
        </section>

        <!-- Problem & Solution Section -->
        <section class="form-section">
          <h2>Problem & Solution</h2>

          <div class="form-group">
            <label for="problem" class="required">What problem does this solve?</label>
            <textarea 
              id="problem"
              [(ngModel)]="form.problemStatement"
              name="problemStatement"
              placeholder="Describe the problem your idea addresses..."
              class="form-textarea"
              rows="4"
              required></textarea>
          </div>

          <div class="form-group">
            <label for="solution" class="required">How does your solution work?</label>
            <textarea 
              id="solution"
              [(ngModel)]="form.proposedSolution"
              name="proposedSolution"
              placeholder="Explain your proposed solution..."
              class="form-textarea"
              rows="4"
              required></textarea>
          </div>

          <div class="form-group">
            <label for="metrics" class="required">How will you measure success?</label>
            <textarea 
              id="metrics"
              [(ngModel)]="form.successMetrics"
              name="successMetrics"
              placeholder="Define key success metrics and KPIs..."
              class="form-textarea"
              rows="4"
              required></textarea>
          </div>
        </section>

        <!-- Technical Details Section -->
        <section class="form-section">
          <h2>Technical Details</h2>

          <div class="form-row">
            <div class="form-group">
              <label for="tech">Technologies/Tools</label>
              <input 
                id="tech"
                type="text"
                placeholder="E.g., React, Node.js, Python"
                class="form-input">
            </div>

            <div class="form-group">
              <label for="resources">Resources Needed</label>
              <input 
                id="resources"
                type="text"
                placeholder="E.g., Database, API access, Computing power"
                class="form-input">
            </div>
          </div>

          <div class="form-group">
            <label for="timeline">Timeline</label>
            <input 
              id="timeline"
              type="text"
              placeholder="E.g., 24 hours, 2-3 days"
              class="form-input">
          </div>
        </section>

        <!-- Form Actions -->
        <section class="form-actions">
          <button type="submit" class="btn-primary" [disabled]="!isFormValid || isSubmitting">
            {{ isSubmitting ? 'Saving...' : (isEditMode ? 'Update Idea' : 'Save as Draft') }}
          </button>

          <button 
            type="button" 
            class="btn-secondary"
            [disabled]="!isEditMode"
            (click)="submitForJudging()">
            {{ isEditMode ? 'Update & Submit' : 'Submit for Judging' }}
          </button>

          <button type="button" class="btn-cancel" (click)="cancel()">Cancel</button>
        </section>

        <!-- Validation Messages -->
        <div *ngIf="validationErrors.length > 0" class="error-messages">
          <div *ngFor="let error of validationErrors" class="error-message">
            {{ error }}
          </div>
        </div>

        <!-- Success Message -->
        <div *ngIf="successMessage" class="success-message">
          {{ successMessage }}
        </div>
      </form>
    </div>
  `,
  styles: [`
    .form-container {
      max-width: 800px;
      margin: 2rem auto;
      padding: 2rem;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .form-container h1 {
      margin-top: 0;
      margin-bottom: 2rem;
      font-size: 2rem;
    }

    .idea-form {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }

    .form-section {
      border-bottom: 1px solid #eee;
      padding-bottom: 2rem;
    }

    .form-section:last-of-type {
      border-bottom: none;
    }

    .form-section h2 {
      font-size: 1.25rem;
      margin-top: 0;
      margin-bottom: 1.5rem;
      color: #333;
    }

    .form-group {
      margin-bottom: 1.5rem;
      display: flex;
      flex-direction: column;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }

    label {
      font-weight: 600;
      margin-bottom: 0.5rem;
      color: #333;
    }

    .required::after {
      content: ' *';
      color: #f44336;
    }

    .form-input, .form-textarea, .form-select {
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-family: inherit;
      font-size: 1rem;
      transition: border-color 0.3s;
    }

    .form-input:focus, .form-textarea:focus, .form-select:focus {
      outline: none;
      border-color: #0066cc;
      box-shadow: 0 0 0 3px rgba(0, 102, 204, 0.1);
    }

    .form-textarea {
      resize: vertical;
      font-family: inherit;
    }

    .help-text {
      font-size: 0.875rem;
      color: #999;
      margin-top: 0.25rem;
      margin-bottom: 0;
    }

    .form-actions {
      display: flex;
      gap: 1rem;
      margin-top: 2rem;
      padding-top: 2rem;
      border-top: 1px solid #eee;
    }

    .btn-primary, .btn-secondary, .btn-cancel {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 4px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s;
      font-size: 1rem;
    }

    .btn-primary {
      background: #0066cc;
      color: white;
      flex: 1;
    }

    .btn-primary:hover:not(:disabled) {
      background: #0052a3;
    }

    .btn-secondary {
      background: white;
      color: #0066cc;
      border: 1px solid #0066cc;
      flex: 1;
    }

    .btn-secondary:hover:not(:disabled) {
      background: #f0f0f0;
    }

    .btn-cancel {
      background: white;
      color: #666;
      border: 1px solid #ddd;
    }

    .btn-cancel:hover {
      background: #f5f5f5;
    }

    .btn-primary:disabled, .btn-secondary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .error-messages {
      background: #ffebee;
      border-left: 4px solid #f44336;
      padding: 1rem;
      border-radius: 4px;
      margin-top: 1rem;
    }

    .error-message {
      color: #c62828;
      margin-bottom: 0.5rem;
      font-size: 0.875rem;
    }

    .error-message:last-child {
      margin-bottom: 0;
    }

    .success-message {
      background: #e8f5e9;
      border-left: 4px solid #4caf50;
      padding: 1rem;
      border-radius: 4px;
      color: #2e7d32;
      margin-top: 1rem;
    }

    @media (max-width: 600px) {
      .form-container {
        margin: 0;
        padding: 1rem;
        border-radius: 0;
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .form-actions {
        flex-direction: column;
      }

      .btn-primary, .btn-secondary {
        flex: initial;
      }
    }
  `]
})
export class IdeaFormComponent implements OnInit, OnDestroy {
  isEditMode = false;
  isSubmitting = false;
  isFormValid = false;
  successMessage = '';
  validationErrors: string[] = [];
  tracks: any[] = [];
  userTeams: any[] = [];

  form = {
    title: '',
    description: '',
    trackId: '',
    teamId: '',
    problemStatement: '',
    proposedSolution: '',
    successMetrics: '',
    technologies: '',
    resourcesNeeded: '',
    timeline: ''
  };

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ideaService: IdeaService,
    private hackathonService: HackathonService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        if (params['id']) {
          this.isEditMode = true;
          this.loadIdea(parseInt(params['id']));
        } else {
          this.loadInitialData();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadInitialData(): void {
    this.hackathonService.getCurrentHackathon()
      .pipe(takeUntil(this.destroy$))
      .subscribe(hackathon => {
        this.tracks = hackathon.tracks || [];
      });
  }

  loadIdea(id: number): void {
    this.ideaService.getIdea(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(idea => {
        this.form = {
          title: idea.title,
          description: idea.description,
          trackId: String(idea.trackId ?? ''),
          teamId: String(idea.submission?.teamId ?? ''),
          problemStatement: idea.problemStatement || '',
          proposedSolution: idea.proposedSolution || '',
          successMetrics: idea.successMetrics || '',
          technologies: '',
          resourcesNeeded: '',
          timeline: ''
        };
      });

    this.loadInitialData();
  }

  submitForm(): void {
    this.validationErrors = [];

    if (!this.validateForm()) {
      return;
    }

    this.isSubmitting = true;

    const ideaData = {
      title: this.form.title,
      description: this.form.description,
      trackId: parseInt(this.form.trackId),
      problemStatement: this.form.problemStatement,
      proposedSolution: this.form.proposedSolution,
      successMetrics: this.form.successMetrics,
      status: 'draft' as const
    };

    if (this.isEditMode) {
      this.ideaService.updateIdea(0, ideaData)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.isSubmitting = false;
            this.successMessage = 'Idea updated successfully!';
            setTimeout(() => {
              this.router.navigate(['/ideas']);
            }, 1500);
          },
          error: (error: unknown) => {
            this.isSubmitting = false;
            this.validationErrors = ['Error saving idea. Please try again.'];
            console.error('Error:', error);
          }
        });
      return;
    }

    this.ideaService.createIdea(ideaData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSubmitting = false;
          this.successMessage = 'Idea saved as draft!';
          setTimeout(() => {
            this.router.navigate(['/ideas']);
          }, 1500);
        },
        error: (error: unknown) => {
          this.isSubmitting = false;
          this.validationErrors = ['Error saving idea. Please try again.'];
          console.error('Error:', error);
        }
      });
  }

  validateForm(): boolean {
    if (!this.form.title?.trim()) {
      this.validationErrors.push('Idea title is required');
    } else if (this.form.title.length > 100) {
      this.validationErrors.push('Idea title must be less than 100 characters');
    }

    if (!this.form.description?.trim()) {
      this.validationErrors.push('Description is required');
    } else if (this.form.description.length > 500) {
      this.validationErrors.push('Description must be less than 500 characters');
    }

    if (!this.form.trackId) {
      this.validationErrors.push('Track selection is required');
    }

    if (!this.form.problemStatement?.trim()) {
      this.validationErrors.push('Problem statement is required');
    }

    if (!this.form.proposedSolution?.trim()) {
      this.validationErrors.push('Proposed solution is required');
    }

    if (!this.form.successMetrics?.trim()) {
      this.validationErrors.push('Success metrics are required');
    }

    return this.validationErrors.length === 0;
  }

  submitForJudging(): void {
    this.submitForm();
    // Additional logic to change status to 'submitted'
  }

  cancel(): void {
    if (confirm('Discard changes?')) {
      this.router.navigate(['/ideas']);
    }
  }
}
