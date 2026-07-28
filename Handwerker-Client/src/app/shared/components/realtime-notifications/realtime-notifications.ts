import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { SignalRService } from '../../../core/services';

@Component({
  selector: 'app-realtime-notifications',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './realtime-notifications.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RealtimeNotifications implements OnInit {
  signalrService = inject(SignalRService);

  ngOnInit() {
    // SignalR Verbindung starten
    this.signalrService.startConnection();
    this.signalrService.requestNotificationPermission();
  }

  clearAll() {
    this.signalrService.clearNotifications();
    this.signalrService.clearLowStockAlerts();
  }

  getTypeColor(type: string): string {
    const colors: Record<string, string> = {
      'info': 'blue',
      'success': 'green',
      'warning': 'yellow',
      'error': 'red'
    };
    return colors[type] || 'blue';
  }
}
