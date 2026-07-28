import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Recipient } from '../../../core/entities';

export type { Recipient };

@Injectable({
  providedIn: 'root'
})
export class RecipientService {
  private http = inject(HttpClient);
  private readonly apiUrl = '/api/recipients';

  getRecipients(): Observable<Recipient[]> {
    return this.http.get<Recipient[]>(this.apiUrl);
  }

  getRecipient(id: number): Observable<Recipient> {
    return this.http.get<Recipient>(`${this.apiUrl}/${id}`);
  }

  createRecipient(recipient: Omit<Recipient, 'id'>): Observable<Recipient> {
    return this.http.post<Recipient>(this.apiUrl, recipient);
  }

  updateRecipient(id: number, recipient: Recipient): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, recipient);
  }

  deleteRecipient(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
