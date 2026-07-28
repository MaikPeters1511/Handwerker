import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {Provider} from '../../../core/entities';


@Injectable({
  providedIn: 'root'
})
export class ProviderService {
  private http = inject(HttpClient);
  private readonly apiUrl = '/api/providers';

  getProviders(): Observable<Provider[]> {
    return this.http.get<Provider[]>(this.apiUrl);
  }

  getProvider(id: number): Observable<Provider> {
    return this.http.get<Provider>(`${this.apiUrl}/${id}`);
  }

  createProvider(provider: Omit<Provider, 'id'>): Observable<Provider> {
    return this.http.post<Provider>(this.apiUrl, provider);
  }

  updateProvider(id: number, provider: Provider): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, provider);
  }

  deleteProvider(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
