import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withXhr } from '@angular/common/http';

import { ServiceItemService } from './service-item.service';
import { ServiceItem } from '../entities';

describe('ServiceItemService', () => {
  let service: ServiceItemService;
  let httpMock: HttpTestingController;

  const mockService: ServiceItem = {
    id: 1,
    serviceNumber: 'L-0001',
    name: 'Montage',
    description: 'Montage vor Ort',
    unit: 'Std.',
    unitPrice: 65,
    taxRate: 19,
    isActive: true
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ServiceItemService, provideHttpClient(withXhr()), provideHttpClientTesting()]
    });

    service = TestBed.inject(ServiceItemService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch all services', () => {
    let result: ServiceItem[] | undefined;
    service.getServices().subscribe(services => (result = services));

    const req = httpMock.expectOne('/api/services');
    expect(req.request.method).toBe('GET');
    req.flush([mockService]);

    expect(result).toEqual([mockService]);
  });

  it('should search services by term', () => {
    let result: ServiceItem[] | undefined;
    service.searchServices('Montage').subscribe(services => (result = services));

    const req = httpMock.expectOne(r => r.url === '/api/services/search' && r.params.get('term') === 'Montage');
    expect(req.request.method).toBe('GET');
    req.flush([mockService]);

    expect(result).toEqual([mockService]);
  });

  it('should create a service', () => {
    const request = { name: 'Montage', unit: 'Std.', unitPrice: 65, taxRate: 19 };
    let result: ServiceItem | undefined;
    service.createService(request).subscribe(created => (result = created));

    const req = httpMock.expectOne('/api/services');
    expect(req.request.method).toBe('POST');
    req.flush(mockService);

    expect(result).toEqual(mockService);
  });
});
