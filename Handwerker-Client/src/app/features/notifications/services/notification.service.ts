import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, interval } from 'rxjs';
import { tap, catchError, switchMap, startWith } from 'rxjs/operators';
import { Notification } from '../../../core/entities';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private readonly apiUrl = '/api/notifications';

  // Signals für reaktives State Management
  unreadCount = signal<number>(0);
  notifications = signal<Notification[]>([]);

  constructor() {
    // Polling alle 30 Sekunden für neue Benachrichtigungen
    interval(30000)
      .pipe(
        startWith(0), // Sofort beim Start laden
        switchMap(() => this.getUnreadCount()),
        catchError(() => [0])
      )
      .subscribe(count => this.unreadCount.set(count));
  }

  getNotifications(skip = 0, take = 50, isRead?: boolean): Observable<Notification[]> {
    let url = `${this.apiUrl}?skip=${skip}&take=${take}`;
    if (isRead !== undefined) {
      url += `&isRead=${isRead}`;
    }

    return this.http.get<Notification[]>(url).pipe(
      tap(notifications => {
        // Timestamps in Date-Objekte konvertieren
        notifications.forEach(n => {
          n.createdAt = new Date(n.createdAt);
        });
        this.notifications.set(notifications);
      })
    );
  }

  getUnreadCount(): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/unread-count`).pipe(
      tap(count => this.unreadCount.set(count))
    );
  }

  markAsRead(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/mark-read`, {}).pipe(
      tap(() => {
        // State aktualisieren
        this.unreadCount.update(count => Math.max(0, count - 1));
        this.notifications.update(notifications =>
          notifications.map(n => n.id === id ? { ...n, isRead: true } : n)
        );
      })
    );
  }

  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/mark-all-read`, {}).pipe(
      tap(() => {
        this.unreadCount.set(0);
        this.notifications.update(notifications =>
          notifications.map(n => ({ ...n, isRead: true }))
        );
      })
    );
  }

  deleteNotification(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        const notification = this.notifications().find(n => n.id === id);
        if (notification && !notification.isRead) {
          this.unreadCount.update(count => Math.max(0, count - 1));
        }
        this.notifications.update(notifications =>
          notifications.filter(n => n.id !== id)
        );
      })
    );
  }

  clearAll(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/clear-all`).pipe(
      tap(() => {
        this.unreadCount.set(0);
        this.notifications.set([]);
      })
    );
  }
}
