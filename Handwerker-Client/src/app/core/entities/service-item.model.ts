export interface ServiceItem {
  id: number;
  serviceNumber: string;
  name: string;
  description?: string;
  unit: string;
  unitPrice: number;
  taxRate: number;
  isActive: boolean;
}

export interface CreateServiceItemRequest {
  name: string;
  description?: string;
  unit: string;
  unitPrice: number;
  taxRate: number;
}

export interface UpdateServiceItemRequest {
  id: number;
  name: string;
  description?: string;
  unit: string;
  unitPrice: number;
  taxRate: number;
  isActive: boolean;
}
