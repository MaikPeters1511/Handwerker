import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

import { Profile } from './profile';
import { AuthService, ProfileService, TranslationService } from '../../core/services';

describe('Profile', () => {
  let component: Profile;

  let profileServiceMock: {
    getUserProfile: ReturnType<typeof vi.fn>;
    updateUserProfile: ReturnType<typeof vi.fn>;
    profileToFormData: ReturnType<typeof vi.fn>;
    formDataToProfile: ReturnType<typeof vi.fn>;
  };
  let authServiceMock: Partial<AuthService>;
  let translationServiceMock: { translate: ReturnType<typeof vi.fn> };

  const mockProfile = {
    id: '123',
    username: 'testuser',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    emailVerified: true,
    attributes: {
      phoneNumber: ['+49 123 456789'],
      gender: ['male'],
      country: ['Germany'],
      address: ['Test Street 1, 12345 Test City']
    }
  };

  beforeEach(() => {
    profileServiceMock = {
      getUserProfile: vi.fn(),
      updateUserProfile: vi.fn(),
      profileToFormData: vi.fn(),
      formDataToProfile: vi.fn()
    };

    authServiceMock = {
      userProfile: signal({
        info: { given_name: 'John', family_name: 'Doe', email: 'john.doe@example.com' }
      }),
      isLoggedIn: signal(true),
      userName: signal('John Doe'),
      roles: signal([])
    } as Partial<AuthService>;

    translationServiceMock = {
      translate: vi.fn().mockReturnValue('Translated Text')
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: ProfileService, useValue: profileServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: TranslationService, useValue: translationServiceMock }
      ]
    });

    component = TestBed.runInInjectionContext(() => new Profile());
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('loadProfile', () => {
    it('should load user profile successfully', () => {
      const formData = {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com',
        phoneNumber: '+49 123 456789',
        gender: 'male',
        country: 'Germany',
        address: 'Test Street 1, 12345 Test City'
      };

      profileServiceMock.getUserProfile.mockReturnValue(of(mockProfile));
      profileServiceMock.profileToFormData.mockReturnValue(formData);

      component.loadProfile();

      expect(component.isLoading()).toBe(false);
      expect(component.profileForm.value).toEqual(formData);
      expect(component.profileForm.pristine).toBe(true);
      expect(component.saveError()).toBeNull();
    });

    it('should handle profile load error by surfacing an error message', () => {
      profileServiceMock.getUserProfile.mockReturnValue(
        throwError(() => new Error('Failed to load'))
      );

      component.loadProfile();

      // ProfileService fällt bereits intern auf Token-Daten zurück;
      // schlägt das Observable dennoch fehl, zeigt die Komponente nur einen Fehler an
      // und lässt das (leere) Formular unverändert.
      expect(component.isLoading()).toBe(false);
      expect(component.saveError()).toBeTruthy();
      expect(component.profileForm.value.firstName).toBe('');
      expect(component.profileForm.value.lastName).toBe('');
      expect(component.profileForm.value.email).toBe('');
    });
  });

  describe('saveProfile', () => {
    beforeEach(() => {
      component.profileForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com',
        phoneNumber: '+49 123 456789',
        gender: 'male',
        country: 'Germany',
        address: 'Test Street 1'
      });
      component.profileForm.markAsDirty();
    });

    it('should save profile successfully', () => {
      const profileUpdate = {
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com',
        attributes: {
          phoneNumber: ['+49 123 456789'],
          gender: ['male'],
          country: ['Germany'],
          address: ['Test Street 1']
        }
      };

      profileServiceMock.formDataToProfile.mockReturnValue(profileUpdate);
      profileServiceMock.updateUserProfile.mockReturnValue(of(mockProfile));

      component.saveProfile();

      expect(component.isSaving()).toBe(false);
      expect(component.saveSuccess()).toBe(true);
      expect(component.profileForm.pristine).toBe(true);
    });

    it('should handle save error', () => {
      const error = { error: { message: 'Save failed' } };
      profileServiceMock.formDataToProfile.mockReturnValue({});
      profileServiceMock.updateUserProfile.mockReturnValue(throwError(() => error));

      component.saveProfile();

      expect(component.isSaving()).toBe(false);
      expect(component.saveError()).toBe('Save failed');
    });

    it('should not save if form is invalid', () => {
      component.profileForm.controls['email'].setValue('invalid-email');
      component.profileForm.markAsDirty();

      component.saveProfile();

      expect(profileServiceMock.updateUserProfile).not.toHaveBeenCalled();
    });

    it('should not save if already saving', () => {
      component['isSaving'].set(true);

      component.saveProfile();

      expect(profileServiceMock.updateUserProfile).not.toHaveBeenCalled();
    });
  });

  describe('Form Validation', () => {
    it('should require firstName', () => {
      const control = component.profileForm.controls['firstName'];
      control.setValue('');
      expect(control.errors?.['required']).toBeTruthy();
    });

    it('should require minimum length for firstName', () => {
      const control = component.profileForm.controls['firstName'];
      control.setValue('J');
      expect(control.errors?.['minlength']).toBeTruthy();
    });

    it('should require lastName', () => {
      const control = component.profileForm.controls['lastName'];
      control.setValue('');
      expect(control.errors?.['required']).toBeTruthy();
    });

    it('should require valid email', () => {
      const control = component.profileForm.controls['email'];
      control.setValue('invalid');
      expect(control.errors?.['email']).toBeTruthy();
    });

    it('should enforce maxLength for phoneNumber', () => {
      const control = component.profileForm.controls['phoneNumber'];
      control.setValue('a'.repeat(21));
      expect(control.errors?.['maxlength']).toBeTruthy();
    });

    it('should enforce maxLength for country', () => {
      const control = component.profileForm.controls['country'];
      control.setValue('a'.repeat(101));
      expect(control.errors?.['maxlength']).toBeTruthy();
    });
  });

  describe('isFormDirty computed', () => {
    it('should be false when form is pristine', () => {
      expect(component.isFormDirty()).toBe(false);
    });

    it('should be false when form is dirty but invalid', () => {
      component.profileForm.controls['email'].setValue('invalid');
      component.profileForm.markAsDirty();
      expect(component.isFormDirty()).toBe(false);
    });

    it('should be true when form is dirty and valid', () => {
      component.profileForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com'
      });
      component.profileForm.markAsDirty();
      expect(component.isFormDirty()).toBe(true);
    });
  });

  describe('onProfileImageSelected', () => {
    it('should reject non-image files', () => {
      const file = new File(['content'], 'test.txt', { type: 'text/plain' });
      const event = {
        target: { files: [file] }
      } as unknown as Event;

      component.onProfileImageSelected(event);

      expect(component.saveError()).toBeTruthy();
      expect(component.profileImageUrl()).toBeNull();
    });

    it('should reject files larger than 5MB', () => {
      const largeFile = new File([new ArrayBuffer(6 * 1024 * 1024)], 'large.jpg', {
        type: 'image/jpeg'
      });
      const event = {
        target: { files: [largeFile] }
      } as unknown as Event;

      component.onProfileImageSelected(event);

      expect(component.saveError()).toBeTruthy();
    });

    it('should create preview for valid image', (done) => {
      const validFile = new File(['content'], 'test.jpg', { type: 'image/jpeg' });
      const event = {
        target: { files: [validFile] }
      } as unknown as Event;

      component.onProfileImageSelected(event);

      // Wait for FileReader to complete
      setTimeout(() => {
        expect(component.profileImageUrl()).toBeTruthy();
        done();
      }, 100);
    });

    it('should handle no files selected', () => {
      const event = {
        target: { files: [] }
      } as unknown as Event;

      component.onProfileImageSelected(event);

      expect(component.profileImageUrl()).toBeNull();
    });
  });
});

