import '@angular/compiler';
import { Injector, runInInjectionContext } from '@angular/core';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { Services } from './services';
import { ServiceItemService } from '../../core/services';
import { TranslationService } from '../../core/services';
import { ServiceItem } from '../../core/entities';

describe('Services', () => {
  const mockService: ServiceItem = {
    id: 1,
    serviceNumber: 'L-0001',
    name: 'Montage',
    description: '',
    unit: 'Std.',
    unitPrice: 65,
    taxRate: 19,
    isActive: true
  };

  const serviceItemServiceMock = {
    getServices: vi.fn(() => of([mockService])),
    getActiveServices: vi.fn(),
    searchServices: vi.fn(() => of([mockService])),
    getService: vi.fn(),
    createService: vi.fn(() => of(mockService)),
    updateService: vi.fn(() => of(undefined)),
    deleteService: vi.fn(() => of(undefined))
  };

  const translationServiceMock = {
    translate: vi.fn((key: string) => key)
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  function createInstance(): Services {
    const injector = Injector.create({
      providers: [
        { provide: ServiceItemService, useValue: serviceItemServiceMock },
        { provide: TranslationService, useValue: translationServiceMock }
      ]
    });
    return runInInjectionContext(injector, () => new Services());
  }

  it('initializes with an empty, non-editing form', () => {
    const component = createInstance();
    expect(component.isEditing()).toBe(false);
    expect(component.serviceForm.value.name).toBe('');
  });

  it('populates the form when selecting a service', () => {
    const component = createInstance();
    component.selectService(mockService);

    expect(component.isEditing()).toBe(true);
    expect(component.serviceForm.value.name).toBe('Montage');
    expect(component.serviceForm.value.serviceNumber).toBe('L-0001');
  });

  it('calls createService when saving a new service', () => {
    const component = createInstance();
    component.serviceForm.patchValue({ name: 'Beratung', unit: 'Std.', unitPrice: 90, taxRate: 19 });

    component.saveService();

    expect(serviceItemServiceMock.createService).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'Beratung', unit: 'Std.', unitPrice: 90, taxRate: 19 })
    );
  });

  it('calls updateService when saving an existing service', () => {
    const component = createInstance();
    component.selectService(mockService);
    component.serviceForm.patchValue({ name: 'Montage (angepasst)' });

    component.saveService();

    expect(serviceItemServiceMock.updateService).toHaveBeenCalledWith(
      1,
      expect.objectContaining({ id: 1, name: 'Montage (angepasst)' })
    );
  });

  it('loads the service list on init', () => {
    const component = createInstance();
    component.ngOnInit();

    expect(serviceItemServiceMock.getServices).toHaveBeenCalled();
    expect(component.services()).toEqual([mockService]);
    expect(component.isLoadingList()).toBe(false);
  });

  it('opens the delete confirmation for the selected service', () => {
    const component = createInstance();
    component.selectService(mockService);
    component.deleteModal = { open: vi.fn() } as any;

    component.deleteSelectedService();

    expect(component.deleteModal.open).toHaveBeenCalledWith('1');
  });

  it('does not open the delete confirmation without a selected service', () => {
    const component = createInstance();
    component.deleteModal = { open: vi.fn() } as any;

    component.deleteSelectedService();

    expect(component.deleteModal.open).not.toHaveBeenCalled();
  });

  it('deletes the service and refreshes the list on confirmation', () => {
    const component = createInstance();
    component.selectService(mockService);

    component.onDeleteConfirmed('1');

    expect(serviceItemServiceMock.deleteService).toHaveBeenCalledWith(1);
    expect(component.isEditing()).toBe(false);
    expect(serviceItemServiceMock.getServices).toHaveBeenCalled();
  });

  it('does nothing when the delete confirmation is cancelled', () => {
    const component = createInstance();
    component.onDeleteConfirmed(null);

    expect(serviceItemServiceMock.deleteService).not.toHaveBeenCalled();
  });
});
