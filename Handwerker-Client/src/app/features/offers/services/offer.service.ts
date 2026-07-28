import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Offer,
  OfferDetail,
  CreateOfferRequest,
  UpdateOfferRequest
} from '../../../core/entities';

@Injectable({
  providedIn: 'root'
})
export class OfferService {
  private http = inject(HttpClient);
  private apiUrl = '/api/offers';

  getOffers(): Observable<Offer[]> {
    return this.http.get<Offer[]>(this.apiUrl);
  }

  getOfferById(id: number): Observable<OfferDetail> {
    return this.http.get<OfferDetail>(`${this.apiUrl}/${id}`);
  }

  getSentOffers(): Observable<Offer[]> {
    return this.http.get<Offer[]>(`${this.apiUrl}/sent`);
  }

  getReceivedOffers(): Observable<Offer[]> {
    return this.http.get<Offer[]>(`${this.apiUrl}/received`);
  }

  createOffer(request: CreateOfferRequest): Observable<OfferDetail> {
    return this.http.post<OfferDetail>(this.apiUrl, request);
  }

  updateOffer(id: number, request: UpdateOfferRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  deleteOffer(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  convertToOrder(id: number): Observable<{ orderId: number | null; message: string }> {
    return this.http.post<{ orderId: number | null; message: string }>(
      `${this.apiUrl}/${id}/convert-to-order`,
      {}
    );
  }
}
