import '@angular/compiler';
import { Injector, Provider, runInInjectionContext } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { of } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { describe, expect, it, vi } from 'vitest';

import { InvoiceDetail } from './invoice-detail';
import { InvoiceService } from '../../../core/services';
import { I18nService } from '../../../core/services';
import { TranslationService } from '../../../core/services';
import { RecipientService } from '../../recipients/services/recipient.service';
import { CompanyService } from '../../company/services/company.service';
import { ProductService } from '../../products/services/product.service';

describe('InvoiceDetail (injection context)', () => {
  const invoiceServiceMock = {
    getNextInvoiceNumber: vi.fn(() => of('RE-2026-1001')),
    getInvoice: vi.fn(),
    createInvoice: vi.fn(),
    updateInvoice: vi.fn(),
    deleteInvoice: vi.fn(),
    getInvoices: vi.fn(),
    convertFromOffer: vi.fn()
  };

  const recipientServiceMock = {
    getRecipients: vi.fn(() =>
      of([
        {
          id: 1,
          customerNumber: 'K-100',
          salutation: 'Herr',
          name: 'Max Mustermann',
          contactPerson: '',
          street: 'Musterweg 1',
          addressLine2: '',
          zipCode: '12345',
          city: 'Berlin',
          country: 'DE',
          email: 'max@example.com',
          phone: '0123'
        }
      ])
    )
  };

  const companyServiceMock = {
    getCompanies: vi.fn(() =>
      of([
        {
          id: 7,
          name: 'Meine Firma',
          taxId: 'DE123',
          taxNumber: '123/456/789',
          street: 'Firmenweg 2',
          zipCode: '54321',
          city: 'Hamburg',
          country: 'DE',
          email: 'firma@example.com',
          phone: '0987',
          bankName: 'Musterbank',
          iban: 'DE44500105175407324931',
          bic: 'COBADEFFXXX',
          commercialRegister: 'HRB 123',
          registerCourt: 'AG Hamburg',
          vatExemption: false
        }
      ])
    ),
    getCompany: vi.fn(() =>
      of({
        id: 7,
        name: 'Meine Firma',
        taxId: 'DE123',
        taxNumber: '123/456/789',
        street: 'Firmenweg 2',
        zipCode: '54321',
        city: 'Hamburg',
        country: 'DE',
        email: 'firma@example.com',
        phone: '0987',
        bankName: 'Musterbank',
        iban: 'DE44500105175407324931',
        bic: 'COBADEFFXXX',
        commercialRegister: 'HRB 123',
        registerCourt: 'AG Hamburg',
        vatExemption: false
      })
    )
  };

  const productServiceMock = {
    getProducts: vi.fn(() => of([])),
    searchProducts: vi.fn(() => of([]))
  };

  const routerMock = {
    navigate: vi.fn()
  };

  const i18nServiceMock = {
    currentLanguage: vi.fn(() => ({ code: 'de', name: 'Deutsch', locale: 'de-DE' }))
  };

  const translationServiceMock = {
    translate: vi.fn((key: string) => key)
  };

  const activatedRouteMock = {
    snapshot: {
      paramMap: {
        get: (key: string) => (key === 'id' ? 'new' : null)
      },
      queryParamMap: {
        get: () => null
      }
    }
  };

  function createComponent(): InvoiceDetail {
    const providers: Provider[] = [
      FormBuilder,
      { provide: InvoiceService, useValue: invoiceServiceMock },
      { provide: RecipientService, useValue: recipientServiceMock },
      { provide: CompanyService, useValue: companyServiceMock },
      { provide: ProductService, useValue: productServiceMock },
      { provide: Router, useValue: routerMock },
      { provide: ActivatedRoute, useValue: activatedRouteMock },
      { provide: I18nService, useValue: i18nServiceMock },
      { provide: TranslationService, useValue: translationServiceMock }
    ];

    const injector = Injector.create({ providers });
    return runInInjectionContext(injector, () => new InvoiceDetail());
  }

  it('initialisiert neue Rechnung mit automatisch geladener Rechnungsnummer', () => {
    const component = createComponent();

    component.ngOnInit();

    expect(component.mode()).toBe('new');
    expect(component.form.get('invoiceNumber')?.value).toBe('RE-2026-1001');
  });

  it('setzt Recipient-Auswahl und Kundennummer bei Autocomplete-Auswahl', () => {
    const component = createComponent();
    component.ngOnInit();

    const recipient = component.recipients()[0];
    component.selectRecipientFromAutocomplete(recipient);

    expect(component.form.get('recipientId')?.value).toBe(1);
    expect(component.form.get('customerNumber')?.value).toBe('K-100');
    expect(component.recipientSearchTerm()).toBe('Max Mustermann');
  });

  it('übernimmt vorgeschlagenes Produkt in eine Position (edge: leere Position)', () => {
    const component = createComponent();
    component.ngOnInit();

    component.addProduct();
    component.selectSuggestedProduct(0, {
      id: 10,
      articleNumber: 'A-10',
      name: 'Produkt A',
      position: 1,
      quantity: 1,
      unit: 'Stk',
      description: 'Testprodukt',
      taxRate: 19,
      taxAmount: 0,
      unitPrice: 25,
      discountPercent: 0,
      discountAmount: 0,
      totalNet: 25,
      totalGross: 29.75
    });

    const row = component.productsArray.at(0);
    expect(row.get('name')?.value).toBe('Produkt A');
    expect(row.get('unitPrice')?.value).toBe(25);
    expect(component.totalNet()).toBeGreaterThan(0);
  });
});
