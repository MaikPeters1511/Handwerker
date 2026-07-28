import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Order,
  OrderStatus,
  CreateOrderRequest,
  CreateOrderFromOffersRequest,
  UpdateOrderRequest,
  UpdateStatusRequest,
  WorkTimeEntry,
  WorkTimeEntryRequest,
  OrderMaterial,
  AddMaterialRequest,
  ConfirmMaterialRequest
} from '../entities';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private http = inject(HttpClient);
  private apiUrl = '/api/orders';

  // Orders
  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl);
  }

  getOrdersByStatus(status: OrderStatus): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/by-status/${status}`);
  }

  searchOrders(term: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/search`, {
      params: { term }
    });
  }

  getOrder(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.apiUrl}/${id}`);
  }

  createOrder(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.apiUrl, request);
  }

  createOrderFromOffers(request: CreateOrderFromOffersRequest): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/from-offers`, request);
  }

  updateOrder(id: number, request: UpdateOrderRequest): Observable<Order> {
    return this.http.put<Order>(`${this.apiUrl}/${id}`, request);
  }

  deleteOrder(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  updateStatus(id: number, request: UpdateStatusRequest): Observable<Order> {
    return this.http.patch<Order>(`${this.apiUrl}/${id}/status`, request);
  }

  convertToInvoice(id: number): Observable<{ invoiceId: number; invoiceNumber: string }> {
    return this.http.post<{ invoiceId: number; invoiceNumber: string }>(
      `${this.apiUrl}/${id}/convert-to-invoice`,
      {}
    );
  }

  // Work Time
  getWorkTimeEntries(orderId: number): Observable<WorkTimeEntry[]> {
    return this.http.get<WorkTimeEntry[]>(`${this.apiUrl}/${orderId}/worktime`);
  }

  addWorkTimeEntry(orderId: number, request: WorkTimeEntryRequest): Observable<WorkTimeEntry> {
    return this.http.post<WorkTimeEntry>(`${this.apiUrl}/${orderId}/worktime`, request);
  }

  deleteWorkTimeEntry(entryId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/worktime/${entryId}`);
  }

  // Materials
  getMaterials(orderId: number): Observable<OrderMaterial[]> {
    return this.http.get<OrderMaterial[]>(`${this.apiUrl}/${orderId}/materials`);
  }

  addMaterial(orderId: number, request: AddMaterialRequest): Observable<OrderMaterial> {
    return this.http.post<OrderMaterial>(`${this.apiUrl}/${orderId}/materials`, request);
  }

  confirmMaterial(materialId: number, request: ConfirmMaterialRequest): Observable<OrderMaterial> {
    return this.http.post<OrderMaterial>(
      `${this.apiUrl}/materials/${materialId}/confirm`,
      request
    );
  }

  // Helper methods
  getStatusLabel(status: OrderStatus): string {
    const labels: Record<OrderStatus, string> = {
      'Draft': 'Entwurf',
      'Planned': 'Geplant',
      'InProgress': 'In Bearbeitung',
      'Completed': 'Abgeschlossen',
      'Invoiced': 'Abgerechnet',
      'Cancelled': 'Storniert'
    };
    return labels[status] || status;
  }

  getStatusColor(status: OrderStatus): string {
    const colors: Record<OrderStatus, string> = {
      'Draft': 'gray',
      'Planned': 'yellow',
      'InProgress': 'blue',
      'Completed': 'green',
      'Invoiced': 'purple',
      'Cancelled': 'red'
    };
    return colors[status] || 'gray';
  }

  getPriorityLabel(priority: string): string {
    const labels: Record<string, string> = {
      'Low': 'Niedrig',
      'Normal': 'Normal',
      'High': 'Hoch',
      'Urgent': 'Eilig'
    };
    return labels[priority] || priority;
  }

  getPriorityColor(priority: string): string {
    const colors: Record<string, string> = {
      'Low': 'green',
      'Normal': 'blue',
      'High': 'orange',
      'Urgent': 'red'
    };
    return colors[priority] || 'blue';
  }
}
