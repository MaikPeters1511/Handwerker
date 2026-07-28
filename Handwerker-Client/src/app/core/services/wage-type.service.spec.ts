import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withXhr } from '@angular/common/http';

import { WageTypeService } from './wage-type.service';
import { WageType } from '../entities';

describe('WageTypeService', () => {
  let service: WageTypeService;
  let httpMock: HttpTestingController;

  const mockWageType: WageType = {
    id: 1,
    wageNumber: 'LN-0001',
    name: 'Facharbeiter',
    description: 'Ausgelernter Facharbeiter',
    hourlyRate: 45,
    taxRate: 19,
    isActive: true
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [WageTypeService, provideHttpClient(withXhr()), provideHttpClientTesting()]
    });

    service = TestBed.inject(WageTypeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch all wage types', () => {
    let result: WageType[] | undefined;
    service.getWageTypes().subscribe(wageTypes => (result = wageTypes));

    const req = httpMock.expectOne('/api/wagetypes');
    expect(req.request.method).toBe('GET');
    req.flush([mockWageType]);

    expect(result).toEqual([mockWageType]);
  });

  it('should search wage types by term', () => {
    let result: WageType[] | undefined;
    service.searchWageTypes('Facharbeiter').subscribe(wageTypes => (result = wageTypes));

    const req = httpMock.expectOne(r => r.url === '/api/wagetypes/search' && r.params.get('term') === 'Facharbeiter');
    expect(req.request.method).toBe('GET');
    req.flush([mockWageType]);

    expect(result).toEqual([mockWageType]);
  });

  it('should create a wage type', () => {
    const request = { name: 'Facharbeiter', hourlyRate: 45, taxRate: 19 };
    let result: WageType | undefined;
    service.createWageType(request).subscribe(created => (result = created));

    const req = httpMock.expectOne('/api/wagetypes');
    expect(req.request.method).toBe('POST');
    req.flush(mockWageType);

    expect(result).toEqual(mockWageType);
  });
});
