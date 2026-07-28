import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Article,
  CreateArticleRequest,
  UpdateArticleRequest,
  Warehouse,
  CreateWarehouseRequest,
  UpdateWarehouseRequest,
  InventoryMovement,
  StockMovementRequest,
  ReserveStockRequest,
  StockInfo,
  AvailabilityCheck
} from '../entities';

@Injectable({
  providedIn: 'root'
})
export class ArticleService {
  private http = inject(HttpClient);
  private apiUrl = '/api/articles';
  private warehouseApiUrl = '/api/warehouses';
  private inventoryApiUrl = '/api/inventory';

  // Articles
  getArticles(): Observable<Article[]> {
    return this.http.get<Article[]>(this.apiUrl);
  }

  getActiveArticles(): Observable<Article[]> {
    return this.http.get<Article[]>(`${this.apiUrl}/active`);
  }

  getLowStockArticles(): Observable<Article[]> {
    return this.http.get<Article[]>(`${this.apiUrl}/low-stock`);
  }

  searchArticles(term: string): Observable<Article[]> {
    return this.http.get<Article[]>(`${this.apiUrl}/search`, {
      params: { term }
    });
  }

  getArticle(id: number): Observable<Article> {
    return this.http.get<Article>(`${this.apiUrl}/${id}`);
  }

  createArticle(request: CreateArticleRequest): Observable<Article> {
    return this.http.post<Article>(this.apiUrl, request);
  }

  updateArticle(id: number, request: UpdateArticleRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  deleteArticle(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Warehouses
  getWarehouses(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(this.warehouseApiUrl);
  }

  getActiveWarehouses(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(`${this.warehouseApiUrl}/active`);
  }

  getWarehouse(id: number): Observable<Warehouse> {
    return this.http.get<Warehouse>(`${this.warehouseApiUrl}/${id}`);
  }

  createWarehouse(request: CreateWarehouseRequest): Observable<Warehouse> {
    return this.http.post<Warehouse>(this.warehouseApiUrl, request);
  }

  updateWarehouse(id: number, request: UpdateWarehouseRequest): Observable<void> {
    return this.http.put<void>(`${this.warehouseApiUrl}/${id}`, request);
  }

  deleteWarehouse(id: number): Observable<void> {
    return this.http.delete<void>(`${this.warehouseApiUrl}/${id}`);
  }

  getStock(warehouseId: number, articleId: number): Observable<{
    stockQuantity: number;
    minStockLevel: number;
    maxStockLevel?: number;
    storageLocation?: string;
    isLowStock: boolean;
  }> {
    return this.http.get<any>(`${this.warehouseApiUrl}/${warehouseId}/articles/${articleId}/stock`);
  }

  // Inventory
  addStock(request: StockMovementRequest): Observable<InventoryMovement> {
    return this.http.post<InventoryMovement>(`${this.inventoryApiUrl}/in`, request);
  }

  removeStock(request: StockMovementRequest): Observable<InventoryMovement> {
    return this.http.post<InventoryMovement>(`${this.inventoryApiUrl}/out`, request);
  }

  reserveStock(request: ReserveStockRequest): Observable<InventoryMovement> {
    return this.http.post<InventoryMovement>(`${this.inventoryApiUrl}/reserve`, request);
  }

  confirmReservation(movementId: number): Observable<InventoryMovement> {
    return this.http.post<InventoryMovement>(`${this.inventoryApiUrl}/confirm-reservation/${movementId}`, {});
  }

  cancelReservation(movementId: number, reason: string): Observable<InventoryMovement> {
    return this.http.post<InventoryMovement>(`${this.inventoryApiUrl}/cancel-reservation/${movementId}`, { reason });
  }

  adjustStock(request: StockMovementRequest & { newQuantity: number }): Observable<InventoryMovement> {
    return this.http.post<InventoryMovement>(`${this.inventoryApiUrl}/adjust`, request);
  }

  getStockInfo(articleId: number, warehouseId: number): Observable<StockInfo> {
    return this.http.get<StockInfo>(`${this.inventoryApiUrl}/stock/${articleId}/${warehouseId}`);
  }

  checkAvailability(articleId: number, warehouseId: number, quantity: number): Observable<AvailabilityCheck> {
    return this.http.get<AvailabilityCheck>(`${this.inventoryApiUrl}/check/${articleId}/${warehouseId}`, {
      params: { quantity: quantity.toString() }
    });
  }

  getMovements(articleId: number): Observable<InventoryMovement[]> {
    return this.http.get<InventoryMovement[]>(`${this.inventoryApiUrl}/movements/${articleId}`);
  }
}
