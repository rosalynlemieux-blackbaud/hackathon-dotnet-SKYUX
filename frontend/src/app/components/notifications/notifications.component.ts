import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Notification } from '../services/notification.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="notifications-container">
      <div 
        *ngFor="let notification of notifications$ | async" 
        class="notification-toast"
        [class]="'notification-' + notification.type"
        [@slideIn]
      >
        <div class="notification-content">
          <div class="notification-header">
            <span class="notification-icon">{{ getIcon(notification.type) }}</span>
            <h4 class="notification-title">{{ notification.title }}</h4>
            <button 
              class="notification-close" 
              (click)="markAsRead(notification.id)">
              ✕
            </button>
          </div>
          <p class="notification-message">{{ notification.message }}</p>
          <span class="notification-time">{{ formatTime(notification.timestamp) }}</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .notifications-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      max-width: 400px;
      pointer-events: none;
    }

    .notification-toast {
      background: white;
      border-radius: 8px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
      margin-bottom: 12px;
      overflow: hidden;
      animation: slideIn 0.3s ease-out;
      pointer-events: all;
      border-left: 4px solid #007bff;
    }

    .notification-toast.notification-comment {
      border-left-color: #17a2b8;
    }

    .notification-toast.notification-rating {
      border-left-color: #ffc107;
    }

    .notification-toast.notification-idea {
      border-left-color: #28a745;
    }

    .notification-toast.notification-team {
      border-left-color: #6f42c1;
    }

    .notification-toast.notification-judge {
      border-left-color: #fd7e14;
    }

    .notification-toast.notification-winner {
      border-left-color: #dc3545;
    }

    .notification-toast.notification-deadline {
      border-left-color: #e83e8c;
    }

    .notification-toast.notification-status {
      border-left-color: #20c997;
    }

    .notification-content {
      padding: 14px 16px;
    }

    .notification-header {
      display: flex;
      align-items: center;
      margin-bottom: 6px;
      gap: 8px;
    }

    .notification-icon {
      font-size: 18px;
      min-width: 24px;
    }

    .notification-title {
      margin: 0;
      flex: 1;
      font-size: 14px;
      font-weight: 600;
      color: #212529;
    }

    .notification-close {
      background: none;
      border: none;
      color: #999;
      font-size: 16px;
      cursor: pointer;
      padding: 0;
      min-width: 24px;
      min-height: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .notification-close:hover {
      color: #333;
    }

    .notification-message {
      margin: 4px 0 6px 0;
      font-size: 13px;
      color: #666;
      line-height: 1.4;
    }

    .notification-time {
      font-size: 11px;
      color: #999;
    }

    @keyframes slideIn {
      from {
        transform: translateX(400px);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    @media (max-width: 640px) {
      .notifications-container {
        left: 10px;
        right: 10px;
        max-width: none;
        top: 10px;
      }

      .notification-toast {
        margin-bottom: 8px;
      }
    }
  `]
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications$;
  private destroy$ = new Subject<void>();

  constructor(private notificationService: NotificationService) {
    this.notifications$ = this.notificationService.notifications$;
  }

  ngOnInit(): void {
    // Component lifecycle managed by parent
  }

  markAsRead(id: string | undefined): void {
    if (id) {
      this.notificationService.markAsRead(id);
    }
  }

  getIcon(type: string): string {
    const icons: { [key: string]: string } = {
      comment: '💬',
      rating: '⭐',
      idea: '💡',
      team: '👥',
      judge: '🔍',
      winner: '🏆',
      deadline: '⏰',
      status: '✓'
    };
    return icons[type] || '📢';
  }

  formatTime(timestamp: Date): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
