import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Comment {
  id: number;
  ideaId: number;
  userId: number;
  user?: any;
  content: string;
  parentCommentId?: number;
  replies?: Comment[];
  isDeleted: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private apiUrl = `${environment.apiUrl}/comments`;

  constructor(private http: HttpClient) {}

  getComments(ideaId: number): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.apiUrl}?ideaId=${ideaId}`);
  }

  getComment(id: number): Observable<Comment> {
    return this.http.get<Comment>(`${this.apiUrl}/${id}`);
  }

  createComment(ideaId: number, content: string, parentCommentId?: number): Observable<Comment> {
    return this.http.post<Comment>(this.apiUrl, {
      ideaId,
      content,
      parentCommentId
    });
  }

  updateComment(id: number, content: string): Observable<Comment> {
    return this.http.put<Comment>(`${this.apiUrl}/${id}`, { content });
  }

  deleteComment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
