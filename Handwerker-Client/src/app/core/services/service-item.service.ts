import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ServiceItem, CreateServiceItemRequest, UpdateServiceItemRequest } from '../entities';

@Injectable({
  providedIn: 'root'
})
export class ServiceItemService {
  private http = inject(HttpClient);
  private apiUrl = '/api/services';

  getServices(): Observable<ServiceItem[]> {
    return this.http.get<ServiceItem[]>(this.apiUrl);
  }

  getActiveServices(): Observable<ServiceItem[]> {
    return this.http.get<ServiceItem[]>(`${this.apiUrl}/active`);
  }

  searchServices(term: string): Observable<ServiceItem[]> {
    return this.http.get<ServiceItem[]>(`${this.apiUrl}/search`, {
      params: { term }
    });
  }

  getService(id: number): Observable<ServiceItem> {
    return this.http.get<ServiceItem>(`${this.apiUrl}/${id}`);
  }

  createService(request: CreateServiceItemRequest): Observable<ServiceItem> {
    return this.http.post<ServiceItem>(this.apiUrl, request);
  }

  updateService(id: number, request: UpdateServiceItemRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  deleteService(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
