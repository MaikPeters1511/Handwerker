import { Component, inject, signal, ElementRef, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '../../pipes/translate.pipe';
import {NotificationService} from '../../../features/notifications/services/notification.service';
import {Notification, NotificationType} from '../../../core/entities';

@Component({
  selector: 'app-notification-dropdown',
  imports: [CommonModule, RouterModule, TranslatePipe],
  templateUrl: './notification-dropdown.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './notification-dropdown.scss'
})
export class NotificationDropdown implements OnDestroy {
  notificationService = inject(NotificationService);
  private elementRef = inject(ElementRef);

  isOpen = signal(false);
  isLoading = signal(false);
  private isToggling = false;
  private clickHandler: (event: MouseEvent) => void;

  constructor() {
    this.loadNotifications();

    this.clickHandler = (event: MouseEvent) => {
      if (this.isToggling) {
        this.isToggling = false;
        return;
      }

      if (!this.elementRef.nativeElement.contains(event.target)) {
        this.isOpen.set(false);
      }
    };

    document.addEventListener('click', this.clickHandler);
  }

  ngOnDestroy() {
    document.removeEventListener('click', this.clickHandler);
  }

  toggleDropdown(event: Event) {
    console.log('🔔 Notification Dropdown Toggle clicked!', { currentState: this.isOpen() });
    event.stopPropagation();
    this.isToggling = true;

    this.isOpen.update(v => {
      const newState = !v;
      console.log('🔔 State changed:', v, '->', newState);
      return newState;
    });

    if (this.isOpen()) {
      console.log('🔔 Loading notifications...');
      this.loadNotifications();
    }
  }

  closeDropdown() {
    this.isOpen.set(false);
  }

  loadNotifications() {
    this.isLoading.set(true);
    this.notificationService.getNotifications(0, 10, undefined)
      .subscribe({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      });
  }

  markAsRead(notification: Notification, event: Event) {
    event.stopPropagation();
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }
  }

  markAllAsRead() {
    this.notificationService.markAllAsRead().subscribe();
  }

  deleteNotification(id: number, event: Event) {
    event.stopPropagation();
    this.notificationService.deleteNotification(id).subscribe();
  }

  getIcon(type: NotificationType): string {
    switch (type) {
      case NotificationType.Success:
        return 'fa-circle-check';
      case NotificationType.Error:
        return 'fa-circle-xmark';
      case NotificationType.Warning:
        return 'fa-triangle-exclamation';
      case NotificationType.Info:
      default:
        return 'fa-circle-info';
    }
  }

  getIconColor(type: NotificationType): string {
    switch (type) {
      case NotificationType.Success:
        return 'text-success';
      case NotificationType.Error:
        return 'text-error';
      case NotificationType.Warning:
        return 'text-warning';
      case NotificationType.Info:
      default:
        return 'text-info';
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
      year: 'numeric'
    });
  }
}
