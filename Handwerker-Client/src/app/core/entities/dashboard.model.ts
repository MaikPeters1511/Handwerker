export interface DashboardStats {
  offers: number;
  invoices: number;
  products: number;
  recipients: number;
}

export interface MonthlyStats {
  month: string;
  year: number;
  invoices: number;
  offers: number;
}

export interface MonthlyAmounts {
  month: string;
  year: number;
  totalGross: number;
  totalNet: number;
  totalTax: number;
}

export interface InvoiceStats {
  totalInvoices: number;
  paidInvoices: number;
  unpaidInvoices: number;
  totalAmount: number;
  paidAmount: number;
  unpaidAmount: number;
}

