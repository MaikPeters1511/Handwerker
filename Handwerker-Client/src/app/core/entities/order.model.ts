import type { Recipient } from './recipient.model';
import type { Provider } from './provider.model';
import type { Product } from './product.model';
import type { Article, Warehouse } from './article.model';

export type OrderStatus = 'Draft' | 'Planned' | 'InProgress' | 'Completed' | 'Invoiced' | 'Cancelled';
export type Priority = 'Low' | 'Normal' | 'High' | 'Urgent';

export interface Order {
  id: number;
  orderNumber: string;
  orderDate: string;
  customerNumber: string;
  recipient: Recipient;
  provider: Provider;
  status: OrderStatus;
  priority: Priority;
  totalNet: number;
  totalTaxAmount: number;
  totalGross: number;
  plannedStartDate?: string;
  plannedEndDate?: string;
  actualStartDate?: string;
  actualEndDate?: string;
  estimatedHours: number;
  actualHours: number;
  description: string;
  internalNotes: string;
  products: Product[];
  sourceOffers?: OrderSourceOffer[];
  materials?: OrderMaterial[];
  workTimeEntries?: WorkTimeEntry[];
  invoiceId?: number;
  isPaid: boolean;
  createdAt: string;
  updatedAt?: string;
  createdBy: string;
}

export interface OrderSourceOffer {
  offerId: number;
  offerNumber: string;
  portionPercentage: number;
}

export interface OrderMaterial {
  id: number;
  articleId: number;
  article?: Article;
  warehouseId: number;
  warehouse?: Warehouse;
  plannedQuantity: number;
  actualQuantity: number;
  isReserved: boolean;
  isConfirmed: boolean;
  reservedAt?: string;
  confirmedAt?: string;
  notes?: string;
  createdAt: string;
}

export interface WorkTimeEntry {
  id: number;
  orderId: number;
  date: string;
  startTime: string;
  endTime: string;
  breakDuration: string;
  totalHours: string;
  description: string;
  isBillable: boolean;
  hourlyRate?: number;
  userId: string;
  userName: string;
  approvedBy?: string;
  approvedAt?: string;
  createdAt: string;
}

export interface CreateOrderRequest {
  orderDate: string;
  customerNumber: string;
  recipient: Recipient;
  provider: Provider;
  products: Product[];
  priority: Priority;
  plannedStartDate?: string;
  plannedEndDate?: string;
  estimatedHours: number;
  description: string;
  internalNotes: string;
}

export interface CreateOrderFromOffersRequest {
  offerIds: number[];
  orderDate: string;
  priority: Priority;
  plannedStartDate?: string;
  plannedEndDate?: string;
  estimatedHours: number;
  description: string;
  internalNotes: string;
}

export interface UpdateOrderRequest {
  id: number;
  orderDate: string;
  customerNumber: string;
  recipient: Recipient;
  provider: Provider;
  products: Product[];
  priority: Priority;
  plannedStartDate?: string;
  plannedEndDate?: string;
  estimatedHours: number;
  description: string;
  internalNotes: string;
}

export interface UpdateStatusRequest {
  status: OrderStatus;
}

export interface WorkTimeEntryRequest {
  date: string;
  startTime: string;
  endTime: string;
  breakDuration: string;
  description: string;
  isBillable: boolean;
  hourlyRate?: number;
}

export interface AddMaterialRequest {
  articleId: number;
  warehouseId: number;
  plannedQuantity: number;
}

export interface ConfirmMaterialRequest {
  actualQuantity: number;
}
