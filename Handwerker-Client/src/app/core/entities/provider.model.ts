import {Bank} from './bank.model';

export interface Provider {
  id: number;
  name: string;
  company: string;
  street: string;
  zipCode: string;
  city: string;
  email: string;
  phone: string;
  website?: string;
  taxId: string;
  taxNumber: string;
  commercialRegister: string;
  registerCourt: string;
  bank: Bank;
}
