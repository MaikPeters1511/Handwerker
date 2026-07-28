import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { WageType, CreateWageTypeRequest, UpdateWageTypeRequest } from '../entities';

@Injectable({
  providedIn: 'root'
})
export class WageTypeService {
  private http = inject(HttpClient);
  private apiUrl = '/api/wagetypes';

  getWageTypes(): Observable<WageType[]> {
    return this.http.get<WageType[]>(this.apiUrl);
  }

  getActiveWageTypes(): Observable<WageType[]> {
    return this.http.get<WageType[]>(`${this.apiUrl}/active`);
  }

  searchWageTypes(term: string): Observable<WageType[]> {
    return this.http.get<WageType[]>(`${this.apiUrl}/search`, {
      params: { term }
    });
  }

  getWageType(id: number): Observable<WageType> {
    return this.http.get<WageType>(`${this.apiUrl}/${id}`);
  }

  createWageType(request: CreateWageTypeRequest): Observable<WageType> {
    return this.http.post<WageType>(this.apiUrl, request);
  }

  updateWageType(id: number, request: UpdateWageTypeRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  deleteWageType(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
