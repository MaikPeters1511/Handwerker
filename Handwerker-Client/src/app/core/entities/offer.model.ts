import { Recipient } from './recipient.model';
import { Provider } from './provider.model';
import { Product } from './product.model';

export type OfferStatus = 'Draft' | 'Sent' | 'Accepted' | 'Declined' | 'Converted';

export interface Offer {
  id: number;
  offerNumber: string;
  offerDate: Date | string;
  validUntil: Date | string;
  customerNumber: string;
  customerName: string;
  totalNet: number;
  totalGross: number;
  status: OfferStatus;
  isReceived: boolean;
  convertedToOrderId?: number | null;
}

export interface OfferDetail {
  id: number;
  offerNumber: string;
  offerDate: Date | string;
  validUntil: Date | string;
  customerNumber: string;
  recipient: Recipient;
  provider: Provider;
  products: Product[];
  totalNet: number;
  totalTaxAmount: number;
  totalGross: number;
  status: OfferStatus;
  introText: string;
  outroText: string;
  notes: string;
  isReceived: boolean;
  convertedToOrderId?: number | null;
  deliveryDate?: Date | string | null;
  shippingMethod?: string | null;
}

export interface CreateOfferRequest {
  offerDate: Date | string;
  validUntil: Date | string;
  customerNumber: string;
  recipient: Recipient;
  provider: Provider;
  products: Product[];
  totalNet: number;
  totalTaxAmount: number;
  totalGross: number;
  status: OfferStatus;
  introText: string;
  outroText: string;
  notes: string;
  isReceived: boolean;
  deliveryDate?: Date | string | null;
  shippingMethod?: string | null;
}

export interface UpdateOfferRequest {
  id: number;
  offerNumber: string;
  offerDate: Date | string;
  validUntil: Date | string;
  customerNumber: string;
  recipient: Recipient;
  provider: Provider;
  products: Product[];
  totalNet: number;
  totalTaxAmount: number;
  totalGross: number;
  status: OfferStatus;
  introText: string;
  outroText: string;
  notes: string;
  isReceived: boolean;
  convertedToOrderId?: number | null;
  deliveryDate?: Date | string | null;
  shippingMethod?: string | null;
}
