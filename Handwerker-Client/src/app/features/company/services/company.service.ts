import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {Company, CreateCompanyRequest} from "../../../core/entities";

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private http = inject(HttpClient);
  private readonly apiUrl = '/api/companies';
  private readonly baseUrl = 'http://localhost:7001';

  getFullLogoUrl(relativePath: string | undefined | null): string | null {
    if (!relativePath) return null;
    if (relativePath.startsWith('data:')) return relativePath;
    return `${this.baseUrl}/${relativePath.replace(/^\/+/, '')}`;
  }

  getCompanies(): Observable<Company[]> {
    return this.http.get<Company[]>(this.apiUrl);
  }

  getCompany(id: number): Observable<Company> {
    return this.http.get<Company>(`${this.apiUrl}/${id}`);
  }

  createCompany(company: CreateCompanyRequest): Observable<Company> {
    return this.http.post<Company>(this.apiUrl, company);
  }

  updateCompany(id: number, company: Company): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, company);
  }

  deleteCompany(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  uploadLogo(id: number, file: File): Observable<{ logoUrl: string }> {
    const fd = new FormData();
    fd.append('file', file, file.name);
    return this.http.post<{ logoUrl: string }>(`${this.apiUrl}/${id}/logo`, fd);
  }

  deleteLogo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}/logo`);
  }
}
