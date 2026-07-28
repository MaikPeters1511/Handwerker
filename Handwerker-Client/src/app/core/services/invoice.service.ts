import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import {Invoice} from '../entities';


@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private http = inject(HttpClient);
  private apiUrl = '/api/invoices';

  // Signal für State Management
  invoices = signal<Invoice[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  getInvoices(): Observable<Invoice[]> {
    this.loading.set(true);
    this.error.set(null);
    return this.http.get<Invoice[]>(this.apiUrl).pipe(
      tap({
        next: (data) => {
          this.invoices.set(data);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err.message);
          this.loading.set(false);
        }
      })
    );
  }

  getInvoice(id: number): Observable<Invoice> {
    this.loading.set(true);
    this.error.set(null);
    return this.http.get<Invoice>(`${this.apiUrl}/${id}`).pipe(
      tap({
        next: () => this.loading.set(false),
        error: (err) => {
          this.error.set(err.message);
          this.loading.set(false);
        }
      })
    );
  }

  createInvoice(invoice: Omit<Invoice, 'id'>): Observable<Invoice> {
    this.loading.set(true);
    this.error.set(null);
    return this.http.post<Invoice>(this.apiUrl, invoice).pipe(
      tap({
        next: (newInvoice) => {
          this.invoices.update(current => [...current, newInvoice]);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err.message);
          this.loading.set(false);
        }
      })
    );
  }

  updateInvoice(id: number, invoice: Invoice): Observable<void> {
    this.loading.set(true);
    this.error.set(null);
    return this.http.put<void>(`${this.apiUrl}/${id}`, invoice).pipe(
      tap({
        next: () => {
          this.invoices.update(current =>
            current.map(inv => inv.id === id ? invoice : inv)
          );
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err.message);
          this.loading.set(false);
        }
      })
    );
  }

  deleteInvoice(id: number): Observable<void> {
    this.loading.set(true);
    this.error.set(null);
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap({
        next: () => {
          this.invoices.update(current => current.filter(inv => inv.id !== id));
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err.message);
          this.loading.set(false);
        }
      })
    );
  }

  getNextInvoiceNumber(): Observable<string> {
    return this.http.get(`${this.apiUrl}/next-invoice-number`, { responseType: 'text' });
  }

  convertFromOffer(offerId: number, includeOfferLines = true): Observable<Invoice> {
    return this.http.post<Invoice>(
      `${this.apiUrl}/convert-from-offer/${offerId}?includeOfferLines=${includeOfferLines}`,
      {}
    );
  }
}
