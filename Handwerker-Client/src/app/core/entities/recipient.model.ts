export interface Recipient {
  id: number;
  customerNumber: string;
  salutation: string;
  name: string;
  contactPerson: string;
  street: string;
  addressLine2: string;
  zipCode: string;
  postalCode?: string; // Alias for zipCode
  city: string;
  country: string;
  email: string;
  phone: string;
}
