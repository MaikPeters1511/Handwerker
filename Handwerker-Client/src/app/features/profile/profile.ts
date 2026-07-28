import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { take } from 'rxjs';
import { TranslatePipe } from '../../shared';
import { AuthService, ProfileService, TranslationService } from '../../core/services';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Profile implements OnInit {
  authService = inject(AuthService);
  profileService = inject(ProfileService);
  translationService = inject(TranslationService);
  fb = inject(FormBuilder);

  // Form
  profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.maxLength(20)]],
    gender: [''],
    country: ['', [Validators.maxLength(100)]],
    address: ['', [Validators.maxLength(200)]]
  });

  // State
  isLoading = signal(false);
  isSaving = signal(false);
  saveSuccess = signal(false);
  saveError = signal<string | null>(null);
  profileImageUrl = signal<string | null>(null);
  profileImagePreviewUrl = signal<string | null>(null);
  selectedImageFile = signal<File | null>(null);

  // Computed
  readonly isFormDirty = computed(() => this.profileForm.dirty && this.profileForm.valid);

  // Options
  genderOptions = [
    { value: '', label: 'profile.form.genderNotSpecified' },
    { value: 'male', label: 'profile.form.genderMale' },
    { value: 'female', label: 'profile.form.genderFemale' },
    { value: 'other', label: 'profile.form.genderOther' }
  ];

  countryOptions = [
    { value: 'Germany', label: 'Germany' },
    { value: 'Austria', label: 'Austria' },
    { value: 'Switzerland', label: 'Switzerland' },
    { value: 'United States', label: 'United States' },
    { value: 'United Kingdom', label: 'United Kingdom' },
    { value: 'France', label: 'France' }
  ];

  ngOnInit() {
    this.loadProfile();
    this.loadProfileImage();
  }

  loadProfile() {
    this.isLoading.set(true);
    this.saveError.set(null); // Reset error before loading

    this.profileService
      .getUserProfile()
      .pipe(take(1))
      .subscribe({
        next: profile => {
          console.log('Loaded profile from Backend:', profile);
          console.log('Profile attributes:', profile.attributes);
          const formData = this.profileService.profileToFormData(profile);
          console.log('Converted to form data:', formData);
          this.profileForm.patchValue(formData);
          this.profileForm.markAsPristine();
          this.isLoading.set(false);
          this.saveError.set(null); // Ensure error is cleared on success
        },
        error: err => {
          console.error('Failed to load profile', err);

          // Nur Fehler anzeigen wenn der Fallback komplett fehlschlägt
          // (sollte nicht passieren, da der Service einen Fallback hat)
          this.saveError.set(
            this.translationService.translate('profile.form.loadError')
          );

          this.isLoading.set(false);
        }
      });
  }

  loadProfileImage() {
    this.profileService
      .getProfileImageUrl()
      .pipe(take(1))
      .subscribe({
        next: response => {
          if (response.imageUrl) {
            // Füge Cache-Buster hinzu, damit das Bild neu geladen wird
            const imageUrlWithCache = `${response.imageUrl}?t=${Date.now()}`;
            this.profileImageUrl.set(imageUrlWithCache);
            console.log('Loaded profile image from backend:', response.imageUrl);
          }
        },
        error: err => {
          console.warn('Failed to load profile image', err);
          // Kein Fehler anzeigen, da Profilbild optional ist
        }
      });
  }

  saveProfile() {
    if (!this.profileForm.valid || this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.saveSuccess.set(false);
    this.saveError.set(null);

    const formData = this.profileForm.getRawValue();
    const profileUpdate = this.profileService.formDataToProfile(formData);

    console.log('Form data:', formData);
    console.log('Saving profile:', profileUpdate);
    console.log('Profile attributes:', profileUpdate.attributes);

    this.profileService
      .updateUserProfile(profileUpdate)
      .pipe(take(1))
      .subscribe({
        next: (updatedProfile) => {
          console.log('Profile saved successfully:', updatedProfile);
          this.profileForm.markAsPristine();
          this.saveSuccess.set(true);

          // Toast nach 5 Sekunden ausblenden
          setTimeout(() => {
            this.saveSuccess.set(false);
          }, 5000);

          this.isSaving.set(false);
        },
        error: err => {
          console.error('Failed to save profile', err);

          // Zeige detaillierte Fehlermeldung für Debugging
          let errorMessage = this.translationService.translate('profile.form.saveError');
          if (err.error?.error_description) {
            errorMessage = err.error.error_description;
          } else if (err.error?.message) {
            errorMessage = err.error.message;
          } else if (err.message) {
            errorMessage = err.message;
          }

          this.saveError.set(errorMessage);

          // Fehler nach 5 Sekunden ausblenden
          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);

          this.isSaving.set(false);
        }
      });
  }

  onProfileImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];

    // Validate file type
    if (!file.type.startsWith('image/')) {
      this.saveError.set(
        this.translationService.translate('profile.form.invalidImageType')
      );
      return;
    }

    // Validate file size (max 5MB)
    const maxSize = 5 * 1024 * 1024;
    if (file.size > maxSize) {
      this.saveError.set(
        this.translationService.translate('profile.form.imageTooLarge')
      );
      return;
    }

    // Speichere das File-Objekt für Upload
    this.selectedImageFile.set(file);

    // Create preview URL (separate von der Server-URL)
    const reader = new FileReader();
    reader.onload = (e) => {
      this.profileImagePreviewUrl.set(e.target?.result as string);
    };
    reader.readAsDataURL(file);

    console.log('Profile image selected:', file.name, file.size, 'bytes');
  }

  updateProfileImage() {
    const imageFile = this.selectedImageFile();
    if (!imageFile) {
      this.saveError.set('Bitte wählen Sie zuerst ein Bild aus');
      return;
    }

    console.log('Uploading profile image to backend...');
    this.isSaving.set(true);
    this.saveError.set(null);

    this.profileService
      .uploadProfileImage(imageFile)
      .pipe(take(1))
      .subscribe({
        next: (response) => {
          console.log('Profile image uploaded successfully:', response.imageUrl);

          // Verwende die Server-URL statt der base64-Preview
          // Füge einen Cache-Buster hinzu, damit das Bild neu geladen wird
          const imageUrlWithCache = `${response.imageUrl}?t=${Date.now()}`;
          this.profileImageUrl.set(imageUrlWithCache);
          this.profileImagePreviewUrl.set(null); // Lösche Preview nach Upload
          this.selectedImageFile.set(null); // Reset nach erfolgreichem Upload
          this.saveSuccess.set(true);

          setTimeout(() => {
            this.saveSuccess.set(false);
          }, 3000);

          this.isSaving.set(false);
        },
        error: err => {
          console.error('Failed to upload profile image', err);

          let errorMessage = 'Fehler beim Hochladen des Profilbilds';
          if (err.error?.message) {
            errorMessage = err.error.message;
          } else if (err.message) {
            errorMessage = err.message;
          }

          this.saveError.set(errorMessage);

          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);

          this.isSaving.set(false);
        }
      });
  }
}
