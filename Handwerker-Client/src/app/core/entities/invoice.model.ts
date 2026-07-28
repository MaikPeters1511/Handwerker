import { Product } from './product.model';
import { Provider } from './provider.model';
import { Recipient } from './recipient.model';

export interface Invoice {
  id: number;
  invoiceNumber: string;
  invoiceDate: string;
  servicePeriod: string;
  customerNumber: string;
  recipient: Recipient;
  provider: Provider;
  products: Product[];
  totalNet: number;
  totalTaxAmount: number;
  totalGross: number;
  dueDate: string;
  paymentTerms: string;
  isPaid: boolean;
  introText: string;
  outroText: string;
}
