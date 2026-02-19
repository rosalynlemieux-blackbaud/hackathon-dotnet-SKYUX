import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface Notification {
  id?: string;
  type: 'comment' | 'rating' | 'idea' | 'team' | 'judge' | 'winner' | 'deadline' | 'status';
  title: string;
  message: string;
  data?: any;
  timestamp: Date;
  read?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService implements OnDestroy {
  private hubConnection: HubConnection;
  private destroy$ = new Subject<void>();

  // Notification streams
  notifications$ = new BehaviorSubject<Notification[]>([]);
  commentAdded$ = new Subject<any>();
  ratingSubmitted$ = new Subject<any>();
  ideaSubmitted$ = new Subject<any>();
  ideaDeleted$ = new Subject<any>();
  ideaStatusChanged$ = new Subject<any>();
  judgeOnline$ = new Subject<any>();
  judgeOffline$ = new Subject<any>();
  teamMemberJoined$ = new Subject<any>();
  teamMemberLeft$ = new Subject<any>();
  winnerAnnounced$ = new Subject<any>();
  judgingDeadline$ = new Subject<any>();

  // Connection state
  isConnected$ = new BehaviorSubject<boolean>(false);
  unreadCount$ = new BehaviorSubject<number>(0);
  onlineJudges$ = new BehaviorSubject<any[]>([]);

  constructor() {
    const hubUrl = `${environment.apiUrl.replace('api/', '')}hubs/notifications`;
    
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.getToken(),
        withCredentials: true
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .withServerTimeout(1000 * 60 * 10) // 10 minutes timeout
      .build();

    this.setupHubEventListeners();
  }

  /**
   * Start the SignalR connection
   */
  start(): Promise<void> {
    if (this.hubConnection.state === HubConnectionState.Connected) {
      return Promise.resolve();
    }

    return this.hubConnection.start()
      .then(() => {
        console.log('SignalR connected');
        this.isConnected$.next(true);
      })
      .catch((err: unknown) => {
        console.error('SignalR connection error:', err);
        this.isConnected$.next(false);
        return Promise.reject(err);
      });
  }

  /**
   * Stop the SignalR connection
   */
  stop(): Promise<void> {
    return this.hubConnection.stop()
      .then(() => {
        this.isConnected$.next(false);
      })
      .catch((err: unknown) => {
        console.error('SignalR stop error:', err);
        return Promise.reject(err);
      });
  }

  /**
   * Join a hackathon group for notifications
   */
  joinHackathon(hackathonId: number): Promise<void> {
    return this.hubConnection.invoke('JoinHackathon', hackathonId)
      .catch((err: unknown) => {
        console.error('Failed to join hackathon group:', err);
        return Promise.reject(err);
      });
  }

  /**
   * Leave a hackathon group
   */
  leaveHackathon(hackathonId: number): Promise<void> {
    return this.hubConnection.invoke('LeaveHackathon', hackathonId)
      .catch((err: unknown) => {
        console.error('Failed to leave hackathon group:', err);
        return Promise.reject(err);
      });
  }

  /**
   * Join judging session
   */
  joinJudging(hackathonId: number): Promise<void> {
    return this.hubConnection.invoke('JoinJudging', hackathonId)
      .catch((err: unknown) => {
        console.error('Failed to join judging:', err);
        return Promise.reject(err);
      });
  }

  /**
   * Leave judging session
   */
  leaveJudging(hackathonId: number): Promise<void> {
    return this.hubConnection.invoke('LeaveJudging', hackathonId)
      .catch((err: unknown) => {
        console.error('Failed to leave judging:', err);
        return Promise.reject(err);
      });
  }

  /**
   * Watch specific idea for updates
   */
  watchIdea(ideaId: number): Promise<void> {
    return this.hubConnection.invoke('WatchIdea', ideaId)
      .catch((err: unknown) => {
        console.error('Failed to watch idea:', err);
        return Promise.reject(err);
      });
  }

  /**
   * Stop watching specific idea
   */
  unwatchIdea(ideaId: number): Promise<void> {
    return this.hubConnection.invoke('UnwatchIdea', ideaId)
      .catch((err: unknown) => {
        console.error('Failed to unwatch idea:', err);
        return Promise.reject(err);
      });
  }

  /**
   * Get list of online judges
   */
  getOnlineJudges(hackathonId: number): Promise<any> {
    return this.hubConnection.invoke('GetOnlineJudges', hackathonId)
      .catch((err: unknown) => {
        console.error('Failed to get online judges:', err);
        return Promise.reject(err);
      });
  }

  /**
   * Add notification to list
   */
  private addNotification(notification: Notification): void {
    const newNotification: Notification = {
      ...notification,
      id: `${notification.type}-${Date.now()}`,
      read: false
    };

    const current = this.notifications$.value;
    this.notifications$.next([newNotification, ...current]);
    this.unreadCount$.next(current.length + 1);

    // Auto-remove notification after 10 seconds
    setTimeout(() => {
      const updated = this.notifications$.value.filter(n => n.id !== newNotification.id);
      this.notifications$.next(updated);
    }, 10000);
  }

  /**
   * Mark notification as read
   */
  markAsRead(notificationId: string): void {
    const updated = this.notifications$.value.map(n => 
      n.id === notificationId ? { ...n, read: true } : n
    );
    this.notifications$.next(updated);
    const unread = updated.filter(n => !n.read).length;
    this.unreadCount$.next(unread);
  }

  /**
   * Clear all notifications
   */
  clearAll(): void {
    this.notifications$.next([]);
    this.unreadCount$.next(0);
  }

  /**
   * Setup all hub event listeners
   */
  private setupHubEventListeners(): void {
    // Comment added
    this.hubConnection.on('CommentAdded', (data: any) => {
      this.addNotification({
        type: 'comment',
        title: 'New Comment',
        message: `${data.comment.authorName} commented on an idea`,
        data,
        timestamp: new Date(data.timestamp)
      });
      this.commentAdded$.next(data);
    });

    // Rating submitted
    this.hubConnection.on('RatingSubmitted', (data: any) => {
      this.addNotification({
        type: 'rating',
        title: 'New Rating',
        message: `${data.rating.judgeEmail} rated an idea`,
        data,
        timestamp: new Date(data.timestamp)
      });
      this.ratingSubmitted$.next(data);
    });

    // Rating updated
    this.hubConnection.on('RatingUpdated', (data: any) => {
      this.ratingSubmitted$.next(data);
    });

    // Idea submitted
    this.hubConnection.on('IdeaSubmitted', (data: any) => {
      this.addNotification({
        type: 'idea',
        title: 'New Idea',
        message: `${data.idea.authorName} submitted: "${data.idea.title}"`,
        data,
        timestamp: new Date(data.timestamp)
      });
      this.ideaSubmitted$.next(data);
    });

    // Idea deleted
    this.hubConnection.on('IdeaDeleted', (data: any) => {
      this.addNotification({
        type: 'idea',
        title: 'Idea Deleted',
        message: `An idea has been deleted`,
        data,
        timestamp: new Date(data.timestamp)
      });
      this.ideaDeleted$.next(data);
    });

    // Idea status changed
    this.hubConnection.on('IdeaStatusChanged', (data: any) => {
      this.addNotification({
        type: 'status',
        title: 'Status Changed',
        message: `An idea status changed to ${data.status}`,
        data,
        timestamp: new Date(data.timestamp)
      });
      this.ideaStatusChanged$.next(data);
    });

    // Judge came online
    this.hubConnection.on('JudgeOnline', (data: any) => {
      this.judgeOnline$.next(data);
    });

    // Judge went offline
    this.hubConnection.on('JudgeOffline', (data: any) => {
      this.judgeOffline$.next(data);
    });

    // Judge status changed
    this.hubConnection.on('JudgeStatusChanged', (data: any) => {
      if (data.isOnline) {
        this.judgeOnline$.next(data);
      } else {
        this.judgeOffline$.next(data);
      }
    });

    // Team member joined
    this.hubConnection.on('TeamMemberJoined', (data: any) => {
      this.addNotification({
        type: 'team',
        title: 'Team Member Joined',
        message: `${data.member.email} joined a team`,
        data,
        timestamp: new Date(data.timestamp)
      });
      this.teamMemberJoined$.next(data);
    });

    // Team member left
    this.hubConnection.on('TeamMemberLeft', (data: any) => {
      this.addNotification({
        type: 'team',
        title: 'Team Member Left',
        message: `A team member left`,
        data,
        timestamp: new Date(data.timestamp)
      });
      this.teamMemberLeft$.next(data);
    });

    // Winner announced
    this.hubConnection.on('WinnerAnnounced', (data: any) => {
      this.addNotification({
        type: 'winner',
        title: 'Winner Announced!',
        message: `Congratulations to "${data.winner.title}"!`,
        data,
        timestamp: new Date(data.timestamp)
      });
      this.winnerAnnounced$.next(data);
    });

    // Judging deadline approaching
    this.hubConnection.on('JudgingDeadlineApproaching', (data: any) => {
      const hours = Math.round(data.hoursRemaining);
      this.addNotification({
        type: 'deadline',
        title: 'Deadline Approaching',
        message: `Judging deadline in ${hours} hours!`,
        data,
        timestamp: new Date(data.timestamp)
      });
      this.judgingDeadline$.next(data);
    });

    // Online judges list
    this.hubConnection.on('OnlineJudgesList', (data: any) => {
      this.onlineJudges$.next(data.judges || []);
    });

    // Connection events
    this.hubConnection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      this.isConnected$.next(false);
    });

    this.hubConnection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.isConnected$.next(true);
    });

    this.hubConnection.onclose(() => {
      console.log('SignalR disconnected');
      this.isConnected$.next(false);
    });
  }

  /**
   * Get JWT token from localStorage
   */
  private getToken(): string {
    return localStorage.getItem('access_token') || '';
  }

  /**
   * Cleanup on service destroy
   */
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.stop().catch(() => {});
  }
}
