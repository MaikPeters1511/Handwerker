import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {DashboardStats, InvoiceStats, MonthlyAmounts, MonthlyStats} from '../../../core/entities';

export type { DashboardStats, MonthlyStats, MonthlyAmounts, InvoiceStats };

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private apiUrl = '/api/Dashboard';

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/stats`);
  }

  getMonthlyStats(): Observable<MonthlyStats[]> {
    return this.http.get<MonthlyStats[]>(`${this.apiUrl}/monthly`);
  }

  getMonthlyAmounts(): Observable<MonthlyAmounts[]> {
    return this.http.get<MonthlyAmounts[]>(`${this.apiUrl}/monthly-amounts`);
  }

  getInvoiceStats(): Observable<InvoiceStats> {
    return this.http.get<InvoiceStats>(`${this.apiUrl}/invoice-stats`);
  }
}
