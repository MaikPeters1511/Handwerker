import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withXhr } from '@angular/common/http';
import { signal } from '@angular/core';

import { ProfileService, KeycloakUserProfile, ProfileFormData } from './profile.service';
import { AuthService } from './auth.service';

describe('ProfileService', () => {
  let service: ProfileService;
  let httpMock: HttpTestingController;
  let authServiceMock: Partial<AuthService>;

  const mockProfile: KeycloakUserProfile = {
    id: '123',
    username: 'testuser',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    emailVerified: true,
    attributes: {
      phoneNumber: ['+49 123 456789'],
      gender: ['male'],
      country: ['DE'],
      address: ['Test Street 1, 12345 Test City']
    }
  };

  beforeEach(() => {
    authServiceMock = {
      userProfile: signal({
        info: {
          given_name: 'John',
          family_name: 'Doe',
          email: 'john.doe@example.com',
          preferred_username: 'testuser',
          email_verified: true
        }
      })
    } as Partial<AuthService>;

    TestBed.configureTestingModule({
      providers: [
        ProfileService,
        { provide: AuthService, useValue: authServiceMock },
        provideHttpClient(withXhr()),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ProfileService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getUserProfile', () => {
    it('should fetch user profile from the backend API', () => {
      let result: KeycloakUserProfile | undefined;
      service.getUserProfile().subscribe(profile => (result = profile));

      const req = httpMock.expectOne('/api/profile');
      expect(req.request.method).toBe('GET');
      req.flush(mockProfile);

      expect(result).toEqual(mockProfile);
    });

    it('should fall back to token data when the backend request fails', () => {
      let result: KeycloakUserProfile | undefined;
      service.getUserProfile().subscribe(profile => (result = profile));

      const req = httpMock.expectOne('/api/profile');
      req.error(new ProgressEvent('error'), { status: 500, statusText: 'Internal Server Error' });

      expect(result).toEqual({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com',
        username: 'testuser',
        emailVerified: true,
        attributes: {
          phoneNumber: [''],
          gender: [''],
          country: [''],
          address: ['']
        }
      });
    });

    it('should throw an error when no token fallback data is available', () => {
      authServiceMock.userProfile = signal(null);

      let error: Error | undefined;
      service.getUserProfile().subscribe({
        next: () => fail('Should have thrown an error'),
        error: (err) => (error = err)
      });

      const req = httpMock.expectOne('/api/profile');
      req.error(new ProgressEvent('error'), { status: 500, statusText: 'Internal Server Error' });

      expect(error?.message).toBe('No user profile data available');
    });
  });

  describe('updateUserProfile', () => {
    it('should update the user profile via the backend API', () => {
      const updateData: Partial<KeycloakUserProfile> = {
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane.smith@example.com'
      };

      const expectedResponse: KeycloakUserProfile = {
        ...mockProfile,
        ...updateData
      };

      let result: KeycloakUserProfile | undefined;
      service.updateUserProfile(updateData).subscribe(profile => (result = profile));

      const req = httpMock.expectOne('/api/profile');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateData);
      req.flush(expectedResponse);

      expect(result?.firstName).toBe('Jane');
      expect(result?.lastName).toBe('Smith');
      expect(result?.email).toBe('jane.smith@example.com');
    });
  });

  describe('profileToFormData', () => {
    it('should convert a minimal profile to form data', () => {
      const minimalProfile: KeycloakUserProfile = {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com'
      };

      const formData = service.profileToFormData(minimalProfile);

      expect(formData).toEqual({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        phoneNumber: '',
        gender: '',
        country: '',
        address: ''
      });
    });

    it('should handle profile with empty strings in fields', () => {
      const emptyProfile: KeycloakUserProfile = {
        firstName: '',
        lastName: '',
        email: ''
      };

      const formData = service.profileToFormData(emptyProfile);

      expect(formData.firstName).toBe('');
      expect(formData.lastName).toBe('');
      expect(formData.email).toBe('');
    });
  });

  describe('formDataToProfile', () => {
    it('should convert form data to Keycloak profile format', () => {
      const formData: ProfileFormData = {
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane.smith@example.com',
        phoneNumber: '+49 987 654321',
        gender: 'female',
        country: 'AT',
        address: 'Main Street 5, 54321 Vienna'
      };

      const profile = service.formDataToProfile(formData);

      expect(profile).toEqual({
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane.smith@example.com',
        attributes: {
          phoneNumber: ['+49 987 654321'],
          gender: ['female'],
          country: ['AT'],
          address: ['Main Street 5, 54321 Vienna']
        }
      });
    });

    it('should handle form data with only required fields', () => {
      const formData: ProfileFormData = {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com'
      };

      const profile = service.formDataToProfile(formData);

      expect(profile).toEqual({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        attributes: {}
      });
    });

    it('should exclude empty optional fields from attributes', () => {
      const formData: ProfileFormData = {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        phoneNumber: '+49 123 456789',
        gender: '',
        country: '',
        address: ''
      };

      const profile = service.formDataToProfile(formData);

      expect(profile.attributes).toEqual({
        phoneNumber: ['+49 123 456789']
      });
      expect(profile.attributes?.gender).toBeUndefined();
      expect(profile.attributes?.country).toBeUndefined();
      expect(profile.attributes?.address).toBeUndefined();
    });
  });

  describe('round-trip conversion', () => {
    it('should maintain data integrity through profileToFormData and formDataToProfile', () => {
      const formData = service.profileToFormData(mockProfile);
      const profile = service.formDataToProfile(formData);

      expect(profile.firstName).toBe(mockProfile.firstName);
      expect(profile.lastName).toBe(mockProfile.lastName);
      expect(profile.email).toBe(mockProfile.email);
      expect(profile.attributes?.phoneNumber?.[0]).toBe(mockProfile.attributes?.phoneNumber?.[0]);
      expect(profile.attributes?.gender?.[0]).toBe(mockProfile.attributes?.gender?.[0]);
      expect(profile.attributes?.country?.[0]).toBe(mockProfile.attributes?.country?.[0]);
      expect(profile.attributes?.address?.[0]).toBe(mockProfile.attributes?.address?.[0]);
    });
  });
});
