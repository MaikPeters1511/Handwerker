export enum NotificationType {
  Info = 'Info',
  Success = 'Success',
  Warning = 'Warning',
  Error = 'Error'
}

export interface Notification {
  id: number;
  userId: string;
  type: NotificationType;
  message: string;
  entityType: string;
  entityId?: number;
  isRead: boolean;
  createdAt: Date;
}
