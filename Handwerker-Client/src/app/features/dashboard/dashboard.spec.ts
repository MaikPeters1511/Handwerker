import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { Router } from '@angular/router';
import { Dashboard } from './dashboard';
import { DashboardService } from './services/dashboard.service';
import { AuthService } from '../../core/services';

describe('Dashboard', () => {
  let component: Dashboard;
  let dashboardService: any;
  let router: any;

  const emptyInvoiceStats = {
    totalInvoices: 0,
    paidInvoices: 0,
    unpaidInvoices: 0,
    totalAmount: 0,
    paidAmount: 0,
    unpaidAmount: 0
  };

  beforeEach(() => {
    dashboardService = {
      getStats: vi.fn().mockReturnValue(of({ offers: 0, invoices: 0, products: 0, recipients: 0 })),
      getMonthlyStats: vi.fn().mockReturnValue(of([])),
      getMonthlyAmounts: vi.fn().mockReturnValue(of([])),
      getInvoiceStats: vi.fn().mockReturnValue(of(emptyInvoiceStats))
    };

    router = {
      navigate: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        Dashboard,
        { provide: DashboardService, useValue: dashboardService },
        { provide: AuthService, useValue: { userName: signal('Test User') } },
        { provide: Router, useValue: router }
      ]
    });

    component = TestBed.inject(Dashboard);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load dashboard stats on init', () => {
    const mockStats = { offers: 5, invoices: 10, products: 3, recipients: 7 };
    dashboardService.getStats.mockReturnValue(of(mockStats));

    component.ngOnInit();

    expect(dashboardService.getStats).toHaveBeenCalled();
    expect(component.stats()).toEqual(mockStats);
  });

  it('should load monthly stats on init', () => {
    const mockMonthlyData = [
      { month: 'Februar', year: 2026, invoices: 1, offers: 2 },
      { month: 'Januar', year: 2026, invoices: 0, offers: 1 }
    ];
    dashboardService.getMonthlyStats.mockReturnValue(of(mockMonthlyData));

    component.ngOnInit();

    expect(dashboardService.getMonthlyStats).toHaveBeenCalled();
    expect(component.monthlyData()).toEqual(mockMonthlyData);
  });

  it('should handle error when loading stats', () => {
    const error = { status: 500, statusText: 'Internal Server Error' };
    dashboardService.getStats.mockReturnValue(throwError(() => error));

    component.ngOnInit();

    expect(component.stats()).toEqual({ offers: 0, invoices: 0, products: 0, recipients: 0 });
  });

  it('should handle error when loading monthly stats', () => {
    const error = { status: 500, statusText: 'Internal Server Error' };
    dashboardService.getMonthlyStats.mockReturnValue(throwError(() => error));

    component.ngOnInit();

    expect(component.monthlyData()).toEqual([]);
  });

  it('should navigate to create offer', () => {
    component.createOffer();
    expect(router.navigate).toHaveBeenCalledWith(['/offers/new']);
  });

  it('should navigate to create invoice', () => {
    component.createInvoice();
    expect(router.navigate).toHaveBeenCalledWith(['/invoices/new']);
  });

  it('should calculate max value correctly', () => {
    component.monthlyData.set([
      { month: 'Februar', year: 2026, invoices: 5, offers: 3 },
      { month: 'Januar', year: 2026, invoices: 2, offers: 8 }
    ]);

    expect(component.getMaxValue()).toBe(8);
  });

  it('should return 1 as minimum max value when no data', () => {
    component.monthlyData.set([]);
    expect(component.getMaxValue()).toBe(1);
  });

  it('should calculate bar height correctly', () => {
    component.monthlyData.set([
      { month: 'Februar', year: 2026, invoices: 5, offers: 10 }
    ]);

    expect(component.getBarHeight(5)).toBe(50);
    expect(component.getBarHeight(10)).toBe(100);
  });
});

