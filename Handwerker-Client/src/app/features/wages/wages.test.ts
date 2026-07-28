import '@angular/compiler';
import { Injector, runInInjectionContext } from '@angular/core';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { Wages } from './wages';
import { WageTypeService } from '../../core/services';
import { TranslationService } from '../../core/services';
import { WageType } from '../../core/entities';

describe('Wages', () => {
  const mockWageType: WageType = {
    id: 1,
    wageNumber: 'LN-0001',
    name: 'Facharbeiter',
    description: '',
    hourlyRate: 45,
    taxRate: 19,
    isActive: true
  };

  const wageTypeServiceMock = {
    getWageTypes: vi.fn(() => of([mockWageType])),
    getActiveWageTypes: vi.fn(),
    searchWageTypes: vi.fn(() => of([mockWageType])),
    getWageType: vi.fn(),
    createWageType: vi.fn(() => of(mockWageType)),
    updateWageType: vi.fn(() => of(undefined)),
    deleteWageType: vi.fn(() => of(undefined))
  };

  const translationServiceMock = {
    translate: vi.fn((key: string) => key)
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  function createInstance(): Wages {
    const injector = Injector.create({
      providers: [
        { provide: WageTypeService, useValue: wageTypeServiceMock },
        { provide: TranslationService, useValue: translationServiceMock }
      ]
    });
    return runInInjectionContext(injector, () => new Wages());
  }

  it('initializes with an empty, non-editing form', () => {
    const component = createInstance();
    expect(component.isEditing()).toBe(false);
    expect(component.wageForm.value.name).toBe('');
  });

  it('populates the form when selecting a wage type', () => {
    const component = createInstance();
    component.selectWageType(mockWageType);

    expect(component.isEditing()).toBe(true);
    expect(component.wageForm.value.name).toBe('Facharbeiter');
    expect(component.wageForm.value.wageNumber).toBe('LN-0001');
  });

  it('calls createWageType when saving a new wage type', () => {
    const component = createInstance();
    component.wageForm.patchValue({ name: 'Meister', hourlyRate: 60, taxRate: 19 });

    component.saveWageType();

    expect(wageTypeServiceMock.createWageType).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'Meister', hourlyRate: 60, taxRate: 19 })
    );
  });

  it('calls updateWageType when saving an existing wage type', () => {
    const component = createInstance();
    component.selectWageType(mockWageType);
    component.wageForm.patchValue({ name: 'Facharbeiter (angepasst)' });

    component.saveWageType();

    expect(wageTypeServiceMock.updateWageType).toHaveBeenCalledWith(
      1,
      expect.objectContaining({ id: 1, name: 'Facharbeiter (angepasst)' })
    );
  });

  it('loads the wage type list on init', () => {
    const component = createInstance();
    component.ngOnInit();

    expect(wageTypeServiceMock.getWageTypes).toHaveBeenCalled();
    expect(component.wageTypes()).toEqual([mockWageType]);
    expect(component.isLoadingList()).toBe(false);
  });

  it('opens the delete confirmation for the selected wage type', () => {
    const component = createInstance();
    component.selectWageType(mockWageType);
    component.deleteModal = { open: vi.fn() } as any;

    component.deleteSelectedWageType();

    expect(component.deleteModal.open).toHaveBeenCalledWith('1');
  });

  it('does not open the delete confirmation without a selected wage type', () => {
    const component = createInstance();
    component.deleteModal = { open: vi.fn() } as any;

    component.deleteSelectedWageType();

    expect(component.deleteModal.open).not.toHaveBeenCalled();
  });

  it('deletes the wage type and refreshes the list on confirmation', () => {
    const component = createInstance();
    component.selectWageType(mockWageType);

    component.onDeleteConfirmed('1');

    expect(wageTypeServiceMock.deleteWageType).toHaveBeenCalledWith(1);
    expect(component.isEditing()).toBe(false);
    expect(wageTypeServiceMock.getWageTypes).toHaveBeenCalled();
  });

  it('does nothing when the delete confirmation is cancelled', () => {
    const component = createInstance();
    component.onDeleteConfirmed(null);

    expect(wageTypeServiceMock.deleteWageType).not.toHaveBeenCalled();
  });
});
