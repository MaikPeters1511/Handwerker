export interface WageType {
  id: number;
  wageNumber: string;
  name: string;
  description?: string;
  hourlyRate: number;
  taxRate: number;
  isActive: boolean;
}

export interface CreateWageTypeRequest {
  name: string;
  description?: string;
  hourlyRate: number;
  taxRate: number;
}

export interface UpdateWageTypeRequest {
  id: number;
  name: string;
  description?: string;
  hourlyRate: number;
  taxRate: number;
  isActive: boolean;
}
