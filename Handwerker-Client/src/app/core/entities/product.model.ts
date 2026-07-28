export interface Product {
  id: number;
  articleNumber: string;
  name: string;
  position: number;
  quantity: number;
  unit: string;
  description: string;
  taxRate: number;
  taxAmount: number;
  unitPrice: number;
  discountPercent: number;
  discountAmount: number;
  totalNet: number;
  totalGross: number;
}
