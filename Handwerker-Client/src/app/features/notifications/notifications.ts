import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Notification, NotificationType } from '../../core/entities';
import {TranslatePipe} from '../../shared';
import {NotificationService} from './services/notification.service';


@Component({
  selector: 'app-notifications-page',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './notifications.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './notifications.scss'
})
export class NotificationsPage {
  notificationService = inject(NotificationService);

  isLoading = signal(false);
  filterRead = signal<boolean | undefined>(undefined);

  constructor() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.isLoading.set(true);
    this.notificationService.getNotifications(0, 100, this.filterRead())
      .subscribe({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      });
  }

  setFilter(filter: boolean | undefined) {
    this.filterRead.set(filter);
    this.loadNotifications();
  }

  markAsRead(notification: Notification) {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }
  }

  markAllAsRead() {
    this.notificationService.markAllAsRead().subscribe(() => {
      this.loadNotifications();
    });
  }

  deleteNotification(id: number) {
    this.notificationService.deleteNotification(id).subscribe();
  }

  clearAll() {
    if (confirm('Wirklich alle Benachrichtigungen löschen?')) {
      this.notificationService.clearAll().subscribe(() => {
        this.loadNotifications();
      });
    }
  }

  getIcon(type: NotificationType): string {
    switch (type) {
      case NotificationType.Success: return 'fa-circle-check';
      case NotificationType.Error: return 'fa-circle-xmark';
      case NotificationType.Warning: return 'fa-triangle-exclamation';
      case NotificationType.Info:
      default: return 'fa-circle-info';
    }
  }

  getIconColor(type: NotificationType): string {
    switch (type) {
      case NotificationType.Success: return 'text-success';
      case NotificationType.Error: return 'text-error';
      case NotificationType.Warning: return 'text-warning';
      case NotificationType.Info:
      default: return 'text-info';
    }
  }

  formatTime(date: Date): string {
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return 'Gerade eben';
    if (diffMins < 60) return `vor ${diffMins} Min.`;
    if (diffHours < 24) return `vor ${diffHours} Std.`;
    if (diffDays < 7) return `vor ${diffDays} Tag${diffDays > 1 ? 'en' : ''}`;

    return date.toLocaleDateString('de-DE', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
