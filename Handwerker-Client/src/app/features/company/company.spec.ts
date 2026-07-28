import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { CompanyPage } from './company';
import { CompanyService } from './services/company.service';
import { Company } from '../../core/entities';

describe('CompanyPage', () => {
  let component: CompanyPage;
  let companyServiceMock: {
    getCompanies: ReturnType<typeof vi.fn>;
    getFullLogoUrl: ReturnType<typeof vi.fn>;
    createCompany: ReturnType<typeof vi.fn>;
    updateCompany: ReturnType<typeof vi.fn>;
    deleteCompany: ReturnType<typeof vi.fn>;
    uploadLogo: ReturnType<typeof vi.fn>;
  };

  const mockCompany: Company = {
    id: 1,
    name: 'Muster GmbH',
    taxId: 'DE123456789',
    logoUrl: 'logos/1.png'
  };

  beforeEach(() => {
    companyServiceMock = {
      getCompanies: vi.fn().mockReturnValue(of([])),
      getFullLogoUrl: vi.fn().mockReturnValue(null),
      createCompany: vi.fn(),
      updateCompany: vi.fn(),
      deleteCompany: vi.fn(),
      uploadLogo: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [CompanyPage, { provide: CompanyService, useValue: companyServiceMock }]
    });

    component = TestBed.inject(CompanyPage);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load companies on init', () => {
    expect(companyServiceMock.getCompanies).toHaveBeenCalled();
    expect(component.isLoading()).toBe(false);
    expect(component.companies()).toEqual([]);
  });

  it('should load the company list via loadAll', () => {
    companyServiceMock.getCompanies.mockReturnValue(of([mockCompany]));

    component.loadAll();

    expect(component.companies()).toEqual([mockCompany]);
    expect(component.isLoading()).toBe(false);
  });

  it('should select a company and patch the form', () => {
    companyServiceMock.getFullLogoUrl.mockReturnValue('http://localhost:7001/logos/1.png');

    component.select(mockCompany);

    expect(component.selected()).toEqual(mockCompany);
    expect(component.companyForm.value.name).toBe('Muster GmbH');
    expect(component.previewUrl()).toBe('http://localhost:7001/logos/1.png');
    expect(component.selectedFile()).toBeNull();
  });

  it('should reset the form for a new company', () => {
    component.select(mockCompany);

    component.newCompany();

    expect(component.selected()).toBeNull();
    expect(component.companyForm.value.id).toBeNull();
    expect(component.companyForm.value.name).toBe('');
    expect(component.previewUrl()).toBeNull();
    expect(component.selectedFile()).toBeNull();
  });

  it('should not save when the form is invalid', () => {
    component.companyForm.patchValue({ name: '' });

    component.save();

    expect(companyServiceMock.createCompany).not.toHaveBeenCalled();
    expect(companyServiceMock.updateCompany).not.toHaveBeenCalled();
  });

  it('should create a new company when the form has no id', () => {
    companyServiceMock.createCompany.mockReturnValue(of(mockCompany));
    component.companyForm.patchValue({ name: 'Muster GmbH' });

    component.save();

    expect(companyServiceMock.createCompany).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'Muster GmbH' })
    );
    expect(component.selected()).toEqual(mockCompany);
  });

  it('should update an existing company when the form has an id', () => {
    companyServiceMock.updateCompany.mockReturnValue(of(undefined));
    companyServiceMock.getCompanies.mockReturnValue(of([mockCompany]));
    component.companyForm.patchValue({ id: 1, name: 'Muster GmbH' });

    component.save();

    expect(companyServiceMock.updateCompany).toHaveBeenCalledWith(
      1,
      expect.objectContaining({ name: 'Muster GmbH' })
    );
  });

  it('should surface an error message when saving fails', () => {
    companyServiceMock.createCompany.mockReturnValue(
      throwError(() => ({ error: { message: 'Fehler beim Erstellen' } }))
    );
    component.companyForm.patchValue({ name: 'Muster GmbH' });

    component.save();

    expect(component.uploadError()).toBe('Fehler beim Erstellen');
  });

  it('should delete the selected company', () => {
    companyServiceMock.deleteCompany.mockReturnValue(of(undefined));
    component.select(mockCompany);

    component.deleteSelected();

    expect(companyServiceMock.deleteCompany).toHaveBeenCalledWith(1);
  });

  it('should do nothing when deleting without a selected company', () => {
    component.deleteSelected();

    expect(companyServiceMock.deleteCompany).not.toHaveBeenCalled();
  });
});
