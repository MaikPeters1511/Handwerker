export interface Article {
  id: number;
  articleNumber: string;
  name: string;
  description?: string;
  unit: string;
  unitPrice: number;
  taxRate: number;
  category?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  totalStock: number;
  articleWarehouses?: ArticleWarehouse[];
}

export interface ArticleWarehouse {
  warehouseId: number;
  warehouseName: string;
  stockQuantity: number;
  minStockLevel: number;
  maxStockLevel?: number;
  storageLocation?: string;
  isLowStock: boolean;
}

export interface CreateArticleRequest {
  articleNumber: string;
  name: string;
  description?: string;
  unit: string;
  unitPrice: number;
  taxRate: number;
  category?: string;
}

export interface UpdateArticleRequest {
  id: number;
  articleNumber: string;
  name: string;
  description?: string;
  unit: string;
  unitPrice: number;
  taxRate: number;
  category?: string;
  isActive: boolean;
}

export interface Warehouse {
  id: number;
  name: string;
  description?: string;
  address?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  articleCount?: number;
}

export interface CreateWarehouseRequest {
  name: string;
  description?: string;
  address?: string;
}

export interface UpdateWarehouseRequest {
  id: number;
  name: string;
  description?: string;
  address?: string;
  isActive: boolean;
}

export interface InventoryMovement {
  id: number;
  articleId: number;
  warehouseId: number;
  type: MovementType;
  quantity: number;
  stockBefore: number;
  stockAfter: number;
  referenceType: string;
  referenceId?: number;
  reason?: string;
  createdBy: string;
  createdAt: string;
}

export type MovementType = 'In' | 'Out' | 'Adjustment' | 'Reservation' | 'ReservationCancelled' | 'ReservationConfirmed';

export interface StockMovementRequest {
  articleId: number;
  warehouseId: number;
  quantity: number;
  reason: string;
}

export interface ReserveStockRequest {
  articleId: number;
  warehouseId: number;
  quantity: number;
  reason: string;
  orderId: number;
}

export interface StockInfo {
  articleId: number;
  warehouseId: number;
  totalStock: number;
  availableStock: number;
  reservedStock: number;
}

export interface AvailabilityCheck {
  available: boolean;
  requestedQuantity: number;
  currentStock: number;
  shortage: number;
}
