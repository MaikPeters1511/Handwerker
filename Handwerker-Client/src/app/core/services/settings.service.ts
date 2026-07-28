import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserSettings {
  id: number;
  userId: string;
  theme: string;
  languageCode: string;
  emailNotifications: boolean;
  pushNotifications: boolean;
  smsNotifications: boolean;
  testEmail?: string | null;
  testEmailSubject?: string | null;
  testEmailBody?: string | null;
  invoicePrefix: string;
  nextInvoiceNumber: number;
  taxRate: number;
  currency: string;
  createdAt: string;
  updatedAt: string;
}

export type UserSettingsSave = Omit<UserSettings, 'id' | 'userId' | 'createdAt' | 'updatedAt'>;

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private http = inject(HttpClient);

  // Relative URL, damit der Angular Dev-Server Proxy genutzt werden kann (verhindert CORS-Probleme)
  private apiUrl = '/api/Settings';

  getSettings(): Observable<UserSettings> {
    return this.http.get<UserSettings>(this.apiUrl);
  }

  saveSettings(settings: UserSettingsSave): Observable<UserSettings> {
    return this.http.put<UserSettings>(this.apiUrl, settings);
  }

  sendTestEmail(to: string, subject: string, body: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/send-test-email`, {
      to,
      subject,
      body
    });
  }
}
