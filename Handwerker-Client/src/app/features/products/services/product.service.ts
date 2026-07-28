import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../../../core/entities';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private http = inject(HttpClient);

  // Relative URL, damit der Angular Dev-Server Proxy genutzt werden kann (verhindert CORS-Probleme)
  private apiUrl = '/api/Products';

  getProducts(page: number = 1, pageSize: number = 50): Observable<Product[]> {
    let params = new HttpParams()
        .set('page', page)
        .set('pageSize', pageSize);
    return this.http.get<Product[]>(this.apiUrl, { params });
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  searchProducts(term: string): Observable<Product[]> {
    let params = new HttpParams().set('term', term);
    return this.http.get<Product[]>(`${this.apiUrl}/search`, { params });
  }

  createProduct(product: Omit<Product, 'id'>): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }

  updateProduct(id: number, product: Product): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, product);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
