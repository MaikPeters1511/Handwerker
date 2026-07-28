import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { Sidenav } from './sidenav';
import { AuthService, RoleDashboardService } from '../../../core/services';

describe('Sidenav', () => {
  let component: Sidenav;
  let authServiceMock: Partial<AuthService>;
  let roleDashboardMock: Partial<RoleDashboardService>;

  beforeEach(() => {
    authServiceMock = {
      isLoggedIn: signal(true),
      userProfile: signal(null),
      isAdmin: signal(false) as any
    };
    roleDashboardMock = {
      visibleSections: signal(new Set())
    };

    TestBed.configureTestingModule({
      providers: [
        Sidenav,
        { provide: AuthService, useValue: authServiceMock },
        { provide: RoleDashboardService, useValue: roleDashboardMock }
      ]
    });

    component = TestBed.inject(Sidenav);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have toggleSubmenu method', () => {
    expect(component.toggleSubmenu).toBeDefined();
    expect(typeof component.toggleSubmenu).toBe('function');
  });
});
