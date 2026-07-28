export interface Company {
  id: number;
  name: string;
  taxId?: string;
  taxNumber?: string;
  street?: string;
  zipCode?: string;
  city?: string;
  country?: string;
  email?: string;
  phone?: string;
  bankName?: string;
  iban?: string;
  bic?: string;
  commercialRegister?: string;
  registerCourt?: string;
  vatExemption?: boolean;
  logoUrl?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateCompanyRequest {
  name: string;
  taxId?: string;
  taxNumber?: string;
  street?: string;
  zipCode?: string;
  city?: string;
  country?: string;
  email?: string;
  phone?: string;
  bankName?: string;
  iban?: string;
  bic?: string;
  commercialRegister?: string;
  registerCourt?: string;
  vatExemption?: boolean;
}
