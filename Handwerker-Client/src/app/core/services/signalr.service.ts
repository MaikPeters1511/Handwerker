import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';

export interface RealtimeNotification {
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: Date;
}

export interface LowStockAlert {
  articleId: number;
  articleName: string;
  warehouseName: string;
  currentStock: number;
  minStockLevel: number;
  timestamp: Date;
}

export interface OrderStatusChange {
  orderId: number;
  orderNumber: string;
  newStatus: string;
  message: string;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  
  // Signals für eingehende Nachrichten
  notifications = signal<RealtimeNotification[]>([]);
  lowStockAlerts = signal<LowStockAlert[]>([]);
  orderStatusChanges = signal<OrderStatusChange[]>([]);
  
  isConnected = signal(false);
  connectionError = signal<string | null>(null);

  startConnection(): void {
    const hubUrl = `/hubs/notifications`;
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        // Für Authentication mit JWT Token
        accessTokenFactory: () => localStorage.getItem('access_token') || ''
      })
      .withAutomaticReconnect()
      .build();

    this.setupEventHandlers();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR Connected');
        this.isConnected.set(true);
        this.connectionError.set(null);
      })
      .catch((err) => {
        console.error('SignalR Connection Error:', err);
        this.isConnected.set(false);
        this.connectionError.set(err.message);
      });

    // Reconnection events
    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR Reconnecting...', error);
      this.isConnected.set(false);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR Reconnected:', connectionId);
      this.isConnected.set(true);
      this.connectionError.set(null);
    });

    this.hubConnection.onclose((error) => {
      console.log('SignalR Connection Closed:', error);
      this.isConnected.set(false);
    });
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Allgemeine Benachrichtigungen
    this.hubConnection.on('ReceiveNotification', (data: any) => {
      const notification: RealtimeNotification = {
        title: data.title,
        message: data.message,
        type: data.type,
        timestamp: new Date(data.timestamp)
      };
      this.notifications.update(n => [notification, ...n].slice(0, 50)); // Max 50 speichern
      this.showBrowserNotification(notification.title, notification.message);
    });

    // Low-Stock Warnungen
    this.hubConnection.on('LowStockAlert', (data: any) => {
      const alert: LowStockAlert = {
        articleId: data.articleId,
        articleName: data.articleName,
        warehouseName: data.warehouseName,
        currentStock: data.currentStock,
        minStockLevel: data.minStockLevel,
        timestamp: new Date(data.timestamp)
      };
      this.lowStockAlerts.update(a => [alert, ...a].slice(0, 20));
      this.showBrowserNotification(
        '⚠️ Niedriger Lagerbestand',
        `${alert.articleName} in ${alert.warehouseName}: Nur noch ${alert.currentStock} verfügbar!`
      );
    });

    // Order Status Änderungen
    this.hubConnection.on('OrderStatusChanged', (data: any) => {
      const change: OrderStatusChange = {
        orderId: data.orderId,
        orderNumber: data.orderNumber,
        newStatus: data.newStatus,
        message: data.message,
        timestamp: new Date(data.timestamp)
      };
      this.orderStatusChanges.update(o => [change, ...o].slice(0, 20));
      this.showBrowserNotification(
        '📋 Auftragsstatus geändert',
        `Auftrag ${change.orderNumber}: ${change.message}`
      );
    });

    // Persönliche Benachrichtigungen
    this.hubConnection.on('PersonalNotification', (data: any) => {
      const notification: RealtimeNotification = {
        title: data.title,
        message: data.message,
        type: data.type,
        timestamp: new Date(data.timestamp)
      };
      this.notifications.update(n => [notification, ...n].slice(0, 50));
      this.showBrowserNotification(notification.title, notification.message);
    });
  }

  // Browser Notifications
  requestNotificationPermission(): void {
    if ('Notification' in window) {
      Notification.requestPermission();
    }
  }

  private showBrowserNotification(title: string, body: string): void {
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification(title, {
        body,
        icon: '/favicon.ico'
      });
    }
  }

  // Methoden zum Senden (wenn Benutzer Interaktion hat)
  async broadcastNotification(title: string, message: string, type: string = 'info'): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('BroadcastNotification', title, message, type);
    }
  }

  async joinGroup(groupName: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinGroup', groupName);
    }
  }

  async leaveGroup(groupName: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveGroup', groupName);
    }
  }

  // Hilfsmethoden
  clearNotifications(): void {
    this.notifications.set([]);
  }

  clearLowStockAlerts(): void {
    this.lowStockAlerts.set([]);
  }

  getUnreadCount(): number {
    return this.notifications().length;
  }
}
